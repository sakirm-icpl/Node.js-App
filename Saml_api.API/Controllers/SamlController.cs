using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Saml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Saml.API.APIModel;
using Saml.API.Helper;
using Saml.API.Models;
using Saml.API.Repositories.Interfaces;
using Saml.API.Services;
using static Saml.API.Helper.EnumHelper;
using log4net;
using System.Web;
using NETCore.Encrypt;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Linq;
using System.Net.Http.Headers;
using Saml.API.Helper.Log_API_Count;

namespace Saml.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    public class SamlController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SamlController));
        private IUserRepository _userRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cacheOrgCode;
        public SamlController
            (IUserRepository userReposirotry,
            IWebHostEnvironment environment,
            IConfiguration configure,
            IIdentityService identityService,
            IHttpContextAccessor httpContextAccessor,
            ICustomerConnectionStringRepository customerConnectionStringRepository,
            IMemoryCache memoryCacheOrgCode
            ) : base(identityService)
        {
            this._userRepository = userReposirotry;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            this._configuration = configure;
            this.hostingEnvironment = environment;
            this._identitySvc = identityService;
            this._httpContextAccessor = httpContextAccessor;
            this._cacheOrgCode = memoryCacheOrgCode;
        }
        //Added for SBIG user validation 
        [AllowAnonymous]
        [HttpPost("validateuser")]
        public async Task<IActionResult> validateuser([FromForm] ApiInfo apiInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var OrgnizationConnectionString = await this.OrgnizationConnectionString(apiInfo.OrgCode); //todo move to cache
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString);
                    if (await this._userRepository.IsExists(apiInfo.EmployeeCode))
                    {
                        string email = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(apiInfo.EmployeeCode));
                        string courseId = "-1";
                        string dateTimeUtcNow = null;

                        dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                        var url = "https://gogetempowered.com/saml/SAML?EmailID=" + email + "/" + apiInfo.OrgCode + "/" + dateTimeUtcNow + "/" + courseId;
                        return Redirect(url);

                    }
                    else
                    {
                        return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

        }

        //Added for Degreed assertion 
        [AllowAnonymous]
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromForm] ApiSAMLInfo apiSAMLInfo)
        {
            string samlCertificate = @"-----BEGIN CERTIFICATE-----
            MIIDHDCCAgSgAwIBAgIQ3IEHXZ5lLYRGThtI++NvljANBgkqhkiG9w0BAQsFADAfMR0wGwYDVQQDExRiZXRhdGVzdC5kZWdyZWVkLmNvbTAeFw0xNTA4MTAxODUyMjVaFw0zOTEyMzEyMzU5NTlaMB8xHTAbBgNVBAMTFGJldGF0ZXN0LmRlZ3JlZWQuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzD94rZhTbnOuA4dKGHrF0bVcLcvRaC0ZQNXi1tedNYPhWlYf8BIf2grGX7Cvqs50ORrStyBtu6l2GqqtHTRLr689e+B5UThrAPpvdOuTtQgi6G2f8iOhULdi2c6phVRdsFTgnhi/aZu3gB9bqFx2qiyvAgB3pLGeP/BT9thxD1Frxjhwr1YyHP2d415i4y5t42k28ISkekOevFjer50HYMgFf0gL7CEfDtLZpxW03FV+FVHBZ52vR/zOgGcl8qln6Mp8WA5vJtT3R8lFLn/Uk/R8DKhipmEGt5sxB7/BJBadgwMaQTXXGXptjNcxkVdehG8t4co7fY9DIpp408+QiQIDAQABo1QwUjBQBgNVHQEESTBHgBAkwvL+FxcK73cHIW6Nclc6oSEwHzEdMBsGA1UEAxMUYmV0YXRlc3QuZGVncmVlZC5jb22CENyBB12eZS2ERk4bSPvjb5YwDQYJKoZIhvcNAQELBQADggEBABm+1zcxrII8jTnTflD+jQokhEfzSvYNEFcixmYY0s9s2IDp9VICg8jSgcx7Oex6gApqxajmqNQrIfphJ+kQM1K+zzD+3To7e/aFHmmC4fCxupxXV4cX35cy3JImRPJDP4qetQ/oeR/hQW5SLKBgWZLIl8dyf0jeU1zOgFCAOZdxL4rle34LKBbt+dLNy2jF5kGU7M1BFjZsPTBZsrUxMh7W3LefQhxj2BYvqwPscrKXRb+8h7kbrpN8wiBpOFM55FjlD9TCdA92lhn28+TKSpxUx2+7c1A9Wulysa3H9HqWF6p4gS+goY2NtZ/MwaX0hC31GNSrvMNUxM+p36JF+Og=
            -----END CERTIFICATE-----";


            Saml.Response samlResponse = new Response(samlCertificate);
            samlResponse.LoadXmlFromBase64(apiSAMLInfo.SAMLResponse);

            if (true)
            {
                string username, samlEmail;
                try
                {
                    username = samlResponse.GetNameID();
                    samlEmail = samlResponse.GetEmail();

                    var OrgnizationConnectionString = await CacheTryGetValueSetDegreed(apiSAMLInfo.RelayState);
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);

                    if (await this._userRepository.EmailExists(samlEmail))
                    {
                        string email = Security.EncryptForUI(samlEmail);
                        string courseId = null;
                        string dateTimeUtcNow = null;

                        //get Course Id from relay state
                        courseId = GetCourseId(apiSAMLInfo.RelayState);
                        dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                        var url = "https://gogetempowered.com/saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow + "/" + courseId;
                        return Redirect(url);
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
        }

        private string GetCourseId(string relayState)
        {
            int index = relayState.LastIndexOf('/');
            string remainingString = null, courseId = null;

            if (index != -1)
                remainingString = relayState.Substring(0, index);

            if (String.IsNullOrEmpty(remainingString))
                return null;

            index = remainingString.LastIndexOf('/');

            if (index != -1)
                courseId = remainingString.Substring(index + 1);

            return courseId;
        }

        private async Task<string> OrgnizationConnectionString(string organizationCode)
        {
            string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(organizationCode);
            if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                return string.Empty;

            if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                return string.Empty;

            return OrgnizationConnectionString;
        }

        private async Task<(string, string)> CacheTryGetValueSetSBIL(string organizationCode)
        {

            if (String.IsNullOrEmpty(organizationCode))
                return (null, null);


            // Look for cache key.
            if (!_cacheOrgCode.TryGetValue(CacheKeys.EntryOrgnizationConnectionDegreed, out string cacheEntryOrgnizationConnectionStringDegreed))
            {
                // Key not in cache, so get data.
                cacheEntryOrgnizationConnectionStringDegreed = await this.OrgnizationConnectionString(organizationCode);

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromDays(31));

                // Save data in cache.
                _cacheOrgCode.Set(CacheKeys.EntryOrgnizationConnectionDegreed, cacheEntryOrgnizationConnectionStringDegreed, cacheEntryOptions);
            }
            return (organizationCode, cacheEntryOrgnizationConnectionStringDegreed);
        }
        private async Task<(string, string)> CacheTryGetValueSetDegreed(string relayState)
        {
            int index = relayState.LastIndexOf('/');
            string organizationCode = null;
            if (index != -1)
                organizationCode = relayState.Substring(index + 1);

            if (String.IsNullOrEmpty(organizationCode))
                return (null, null);


            // Look for cache key.
            if (!_cacheOrgCode.TryGetValue(CacheKeys.EntryOrgnizationConnectionDegreed, out string cacheEntryOrgnizationConnectionStringDegreed))
            {
                // Key not in cache, so get data.
                cacheEntryOrgnizationConnectionStringDegreed = await this.OrgnizationConnectionString(organizationCode);

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromDays(31));

                // Save data in cache.
                _cacheOrgCode.Set(CacheKeys.EntryOrgnizationConnectionDegreed, cacheEntryOrgnizationConnectionStringDegreed, cacheEntryOptions);
            }
            return (organizationCode, cacheEntryOrgnizationConnectionStringDegreed);
        }

        //Added for G-Suite assertion 
        [AllowAnonymous]
        [HttpPost("acs")]
        public async Task<IActionResult> ACS([FromForm] ApiSAMLInfo apiSAMLInfo)
        {
            // specify the certificate that your SAML provider has given to you
            string samlCertificate = @"-----BEGIN CERTIFICATE-----
                MIIDdDCCAlygAwIBAgIGAWtBHJNlMA0GCSqGSIb3DQEBCwUAMHsxFDASBgNVBAoTC0dvb2dsZSBJ
                bmMuMRYwFAYDVQQHEw1Nb3VudGFpbiBWaWV3MQ8wDQYDVQQDEwZHb29nbGUxGDAWBgNVBAsTD0dv
                b2dsZSBGb3IgV29yazELMAkGA1UEBhMCVVMxEzARBgNVBAgTCkNhbGlmb3JuaWEwHhcNMTkwNjEw
                MTExODM5WhcNMjQwNjA4MTExODM5WjB7MRQwEgYDVQQKEwtHb29nbGUgSW5jLjEWMBQGA1UEBxMN
                TW91bnRhaW4gVmlldzEPMA0GA1UEAxMGR29vZ2xlMRgwFgYDVQQLEw9Hb29nbGUgRm9yIFdvcmsx
                CzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpDYWxpZm9ybmlhMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
                MIIBCgKCAQEAmyFTIgje4oteaP4pwOWRlEEHfJOwO2hdcl/AGdzZn1gqW2kdi2kRNpY3QAFY9/3Z
                6fatns6XDv/lme83hIehVndrJ5s8IhwldwEIK8oBDi/CEIRYgp1Ixf1d9Y51uX2Ri/dP6W6zJoUB
                9knFOrlHhZxmY00VdV7xLLzNaaIe+am7gE01wEbSlB/wfaULPcJ5/Bn7ORCqNigYDSWKbEtdlPUt
                ZzIBil8Y50vxp3AfjmZ6aXlP613QxWf9zkMQn6ZymHW8p7Qc3XPYT8rYxl/a49otJ4hBqhBibV6H
                2tOu0r8Pe7vbQncI+wubXfoX6UMK51++l8iGEMS/wPcqE8lR5QIDAQABMA0GCSqGSIb3DQEBCwUA
                A4IBAQCND6nqKkWU+L9hAP5SqEuX9Ogww+tPuaummjTKIWVrF7e9F2htgT/+6f6xiACPBEEJBMBh
                ggNvpBFgeu6rsgfXRXuaN/JhGxMJTYHuYQiRizcRj6w55j9wpvw9lVgZrr0ED5ZPOk3qgbc+45IW
                DCbTkjsc6XCfCygqAArcqDY+2EHr7WJITeAF/cGvAQS+RFonpfQpbXiFmRypjO5tbYuPtBmorUGx
                fVLvFul3kZOYsNcnYqaEzFkvjbSqgHXXBvLLpdf+vgolTGEtjfCSAAWsxxMAegGb+mp+JIPSisPj
                T9XBhXubU4p+I+GNwSTQEk5B7wJ+hK5P0YYj6MzDYnGh
                -----END CERTIFICATE-----";

            Saml.Response samlResponse = new Response(samlCertificate);
            samlResponse.LoadXmlFromBase64(apiSAMLInfo.SAMLResponse);

            string username, samlEmail;
            try
            {
                username = samlResponse.GetNameID();

                samlEmail = samlResponse.GetEmail();

                var OrgnizationConnectionString = await CacheTryGetValueSetGSuite(apiSAMLInfo.RelayState);
                this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);

                if (await this._userRepository.EmailExists(username))
                {
                    string email = Security.EncryptForUI(username);
                    string dateTimeUtcNow = null;
                    string courseId = null;

                    //get Course Id from relay state
                    courseId = GetCourseId(apiSAMLInfo.RelayState);

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    var url = "https://gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
        }

        private bool isDebugMode = false;
        private void debugLog(string msg)
        {
            if (isDebugMode)
            {
                if (!System.IO.Directory.Exists("c:\\samldebug"))
                {
                    System.IO.Directory.CreateDirectory("c:\\samldebug");
                }
                System.IO.File.AppendAllText("c:\\samldebug\\" + DateTime.Now.ToString("ddMMyyyy HHmmss") + ".txt", msg + Environment.NewLine + "---------------------------------------------------------------------" + Environment.NewLine);
            }
        }

        [AllowAnonymous]
        [HttpPost("connect/O365/{orgcode?}")]
        public async Task<IActionResult> samlO365([FromForm] ApiSAMLInfo apiSAMLInfo, string orgcode = null)
        {
            string samlCertificate = null;

            try
            {
                if (apiSAMLInfo.RelayState == null)
                {
                    if (orgcode == "icicis")
                    {
                        isDebugMode = true;
                        debugLog("In function O365");
                        debugLog(Newtonsoft.Json.JsonConvert.SerializeObject(apiSAMLInfo));
                        orgcode = "icicis";
                    }
                    else
                    {

                        isDebugMode = true;
                        debugLog("In function O365");
                        debugLog(Newtonsoft.Json.JsonConvert.SerializeObject(apiSAMLInfo));
                        orgcode = "firstamerican";
                    }
                }
                else
                {
                    int index = apiSAMLInfo.RelayState.LastIndexOf('/');

                    if (index != -1)
                        orgcode = apiSAMLInfo.RelayState.Substring(index + 1);
                }
            }
            catch (Exception ex)
            {
                debugLog("issue in apiSAMLInfo object");
                debugLog(Utilities.GetDetailedException(ex));
            }

            debugLog(string.Format("orgcode:{0}", orgcode));

            // specify the certificate that your SAML provider has given to you
            if (orgcode == "ghfl")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQeFlOcMBKlpFFV2yAhltqPDANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQD
EylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMDA3MjkxMjUy
MDlaFw0yMzA3MjkxMjUyMDlaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQg
U1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxktISos/Iaq0
QD8mKz96d7XWzyAtr3g0PcfcPzHEtTUjNsKeR2iRjLwsIHoTVjB07Kcg8FwB/S7p9rFV/9tBx9Nz
aTi9RlegP4mfN7TbJll2wzJSiDBkeg1+/nkKhroERQ/MJW+vMkR+/v2J+QdWYFstdaOmj6PX/1tE
vAQYSWtI8Az6SkjrUhTdy/YWQx4URYKDgTEJ+BURsNc+JyP6CkUPXeXl9+wIVzcuEAZ063jkyCvk
ThKJ8nsnFuvF8nbpZsPbnuklmYYQTDFkwFDDDdhj/rAps64GKZWtjJ89XNEuQjCxR4Sy3CVOpGWr
PBH2GuVwDvvx0Xl3hgifqa0I+QIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQA5/2Q9POV7WbIZlTiw
pV+OsQ7cG2k0i0HgkCSScEMybkhjpRsFpf5vH4Mw9mkKQBI2CIq8YSlXvy599IVE7eWOt9+10DoF
l7rr5SnAnS4Coprr7Szi+qmITrk9Ld3ymVdHScazUqkHvPntW0iylWTfC7w0UUPKKaTCOwIIUg1m
oSZ24NhMFzm0erTVASZntV11ep0I45BFf9UECTM/re+u7USxYi1vGP6lTuk7exWV5xLk+W5MXAz6
WcQ6uPwnP+fE7WbfZhpHencmqHtMcv2RYJcQfR6M/FMix2ragD1BinfrZYRdKxH8fRvgXqq51ujS
z6yKcucIhJy9J4bQyVj1
-----END CERTIFICATE-----";
            }
            else if (orgcode == "nsdc")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQGF2HZK+cEotGQAupKXdl6TANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQD
EylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMDAyMTQwNzU3
MzBaFw0yMzAyMTQwNzU3MzBaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQg
U1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvYBdoxGKnTOk
HHL2+v8CS/+6D+bB/5cGdMkDq6mNBBhboAGuPJ2KixCOsNUd0qyu0RsFI92XoQv39LIawveeEz98
Wb7U0gmJ3k5brRm9sD0nx3gRo9/WZDw/t2CRPc3ebWGlsCu1db669OLCEYCYRU0rvvhJPdldbno6
L+GT555wI0GrCqKkDAMIUzg0q5YbXeQFtdCygI/Rjg76noab0nNikJDej8e13Ovg8LuVupfYNTdT
a35OZUe9aFVfErvB9h3QdOhecCNO1DnaHb13csm/QNCqaBg3TkFR5zHho+A00dXwrS+9oVSBpPkV
CF3x/FMRUYHnUUgt6LynfGOcCQIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQCaGVskn6CzW5JOSDeq
8E7t9v6oVVxfI9aRlrlSxOtFmexvWio6IvPZkpxcPzzlW2I+mJM9D8n28E66eoaa8l/1FZ9dIEJg
dNgaLomtngSncN/ILw8lN87PrUZq5AQaBc3Nr/jZiV4sFbwMuqFI4iDzf7ybzGEqI/RhA+xg5FOh
7fV/AWyKE5P2/5H1LdorHi9Alw1Vd2HCARSwGfzqYfZToKV6FwXHuhpmZVWpuI1XeL8Y/jtdW4kJ
RuIWk8Vjx9hs1AiawP/f5ZMivLsrWWNBu7MWuA4/LuhSWwAGj1kYm5NmH9hfzfGtorbSkrrbV5yT
EqJ7OJxvBRB43QtkoJQn
-----END CERTIFICATE-----";

            }
            else if (orgcode == "ltfs")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIGcTCCBVmgAwIBAgIQCyr8z/2WdXHC1DvsY10WzTANBgkqhkiG9w0BAQsFADBc
MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
d3cuZGlnaWNlcnQuY29tMRswGQYDVQQDExJUaGF3dGUgUlNBIENBIDIwMTgwHhcN
MTkwMzExMDAwMDAwWhcNMjEwNjA5MTIwMDAwWjBwMQswCQYDVQQGEwJJTjEUMBIG
A1UECBMLTWFoYXJhc2h0cmExDzANBgNVBAcTBk11bWJhaTEYMBYGA1UECgwPTCZU
IEZJTkFOQ0UgTFREMQswCQYDVQQLEwJJVDETMBEGA1UEAwwKKi5sdGZzLmNvbTCC
ASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKLexY6Y2QTjtJ5SvW6GOgsn
61KGf4tM5q/5Z067YECN7SWBRKlpmv2SQgPbxxyiIdcaCthqxVzL8GGlvjkM9Hn9
4VsuRZs6PvUvGB64yIiqiYW6Kq30iJBJBxjqgrLgm2dWf/Xk/FUnxDwTjJ+7Ju5+
o1tix3hFg7UstPCL+G3gLrEjKG6wZwdlaO5NT/P4SaYcNjOOCSdAvtWS/XcsNmX6
eQ2HqVMBJ0Tpxzb9SbaMUGDpmDMbXwI8LqKRHhhdoyTLT1F7i3TMhKNi2t9+Uo3H
PoMgZM/xZF9UBGW3PE9BPjr2OIaQwdiY8wVh22obf+mxMMnn5xOeG6F1bA/tNE0C
AwEAAaOCAxkwggMVMB8GA1UdIwQYMBaAFKPIXmVU5TB4wQXqBwpqWcy5/t5aMB0G
A1UdDgQWBBTqQ48ZvWCzZ45JsDCYN0zQehucnTAfBgNVHREEGDAWggoqLmx0ZnMu
Y29tgghsdGZzLmNvbTAOBgNVHQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUH
AwEGCCsGAQUFBwMCMDoGA1UdHwQzMDEwL6AtoCuGKWh0dHA6Ly9jZHAudGhhd3Rl
LmNvbS9UaGF3dGVSU0FDQTIwMTguY3JsMEwGA1UdIARFMEMwNwYJYIZIAYb9bAEB
MCowKAYIKwYBBQUHAgEWHGh0dHBzOi8vd3d3LmRpZ2ljZXJ0LmNvbS9DUFMwCAYG
Z4EMAQICMG8GCCsGAQUFBwEBBGMwYTAkBggrBgEFBQcwAYYYaHR0cDovL3N0YXR1
cy50aGF3dGUuY29tMDkGCCsGAQUFBzAChi1odHRwOi8vY2FjZXJ0cy50aGF3dGUu
Y29tL1RoYXd0ZVJTQUNBMjAxOC5jcnQwCQYDVR0TBAIwADCCAXsGCisGAQQB1nkC
BAIEggFrBIIBZwFlAHUA7ku9t3XOYLrhQmkfq+GeZqMPfl+wctiDAMR7iXqo/csA
AAFpbHtPUAAABAMARjBEAiBvb1W3FD+vuVF6jztObo0SLdbeq/P9r9KjahJ9AmpO
lAIgVO6vhDQiytYrQDCxnGH/Q/0ILNXfg4k1ymp2m6946ccAdQCHdb/nWXz4jEOZ
X73zbv9WjUdWNv9KtWDBtOr/XqCDDwAAAWlse1CDAAAEAwBGMEQCIFlPXyhevlo/
qBDaGL9GR75ez45vg+vT31Cx79OALrsIAiBmysqW6kDTsWwsh+kIgU9jCVIrUkrf
o7s/HhbRT9+MlgB1AESUZS6w7s6vxEAH2Kj+KMDa5oK+2MsxtT/TM5a1toGoAAAB
aWx7T2UAAAQDAEYwRAIgdRA02QDNb9x3WvzhQeujb/EXvXm76rN0EAM9NT8GjVIC
IBH2yPKXmSmJTsNQBlMiwvZ/5TG4koJ4SAvy7YPCOndLMA0GCSqGSIb3DQEBCwUA
A4IBAQAKcpd4saY7y42squ4f9FpsB4ErIWci1xRbFay6MhmuRKwCPrPYy3OneTkB
1jmEluSa7uulxKec8/kjIE1wRhglnq8V84ixHVtGBq5CVQwK8yUWBrWLCx/zsBwn
B6GgrpqgpY0Kr1f3l4k7ItYttBLdL6YgTLQOxqIqDxoZjNDoziRKdY5NUPWn1omq
uHgRCzlLpaeyPt9TBnIQ0b4fX+4ngecdy1AYmJHYkDHNuMmdfaj1L1+mbPkEmYZE
7UHR42IB/hM5NRfa29DmvRcB7pmqyT5UYcbyoptsSQ192+e8vExbjOBONoA2R+vR
ibtiiVXhSpmrvXbSAdOx5n4uG8ix
-----END CERTIFICATE-----";

            }
            else if (orgcode == "altum")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQamqs+1XutbRIU95Kj8thhTANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQD
EylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMTAyMDYwNzA0
NTJaFw0yNDAyMDYwNzA0NTJaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQg
U1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA8po/hVGN+WIn
D4HqSfZfgBBodNL/GCBrZC4h5T+RSREKejGDSO4Xvk+n97FEhHJd/9oQZqEtEGWUVglPA4++fyhh
Ck61bZdMlYfmcgW460rNsyJZAC97dllemKOsZaGREXGl55+oYcKiQyOyUG3KvD0yYYpN63TUmsHj
40IaN9Zzn2bSFRiZOCy+lhYLlNBP5z1YaPAL9CyX1otwlZDwjuy5FMDuclhX9NCSRWWtPXj7hHgY
zAcIg+BiTSsLp+HTAgOJNMpU92DzLugm4sNgDiuLi4wqgqVXn4saZ8roe+VqXMULVzq2QG5r6vEW
pna579ptkSQ0sTG6JfBGJn6VZQIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQBEvwMtNRe1IYkFfxHE
MaKkhkHDPVZSL8nLOGONt8RnfXChAFgtYtxvNCnDIJDO5bz0RT+ErfyPmbvH2IBsHw97nAW89cJP
YKpgpZxSo1YKQnMpkmOz4JXwFE4g4O3oe7GWdL3hsbIUDij6xMwv0qDLo1lIzJxB6qC3+Szj6Yg9
7gVg4vSsyj4VjdyN8cvVSWti029XTmpCfWUXTI4V8qBq+m+SddMFm6FZMtfzOmc4lqLxewixSlvp
iM2FgO8uNiejZb6fImIfFWmmG5ORcDiZg1aNgxnB8mfP6dl+RqEBsFa2KZd8lx8XhSNwfpYWQK9D
grRLUdkxjtSP0W39ZJr4
-----END CERTIFICATE-----";

            }
            else if (orgcode == "firstamerican")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQWeNpDL7blIJDtv3GWyfdvjANBgkqhkiG9w0BAQsFADA0
MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZp
Y2F0ZTAeFw0yMjA3MjExMzI5MDBaFw0yNTA3MjExMzI5MDJaMDQxMjAwBgNVBAMT
KU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjAN
BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwwKEnJAwP+TCDwNwtSTm7azqB/d9
HHIeik8GllA70t1MpsNaeGKeBscsRpg3uQvyFfBk4Xp+AAfJAAWI4VxUiplxtoYo
DXLMjIFNJoQNaCbi/IoabUVC3tB2mA54IT7CTXMpTadZHEfkg4TJMvH21Oz66BR+
6AF81YeDCloBI9lCBR2gj/sMj6YCcCG52oq8Jiha1rsfjxYZnTzSsESFtxE3klq6
lO8u/cl/TAEcs+yQ4PH8irFlg1yvqPkms5lQ2Nd5G1d8BWEY6A2zKME9ND760DW8
WW1TtShT6QNek6+8HA5o3E6Rr7pdGwCg7g+kfccjF8jeidnmm0gcEUxuyQIDAQAB
MA0GCSqGSIb3DQEBCwUAA4IBAQA3afOgwK94qMz/OZ5RL00tcIfMg4ROiYiZpCI2
XjRIAYxV4dgAe8RmkALNM6qR9V2gLXNARuM1qnSPb6GiGq2+ZMx4/PgUljVC7dmR
SK9U1pKbCdPi3sahxFXH0JqzTe+QGA1PABfWY7uZcMUWwumli7PBKe4336HxWyDt
Fs6V0Fbbwiwo+5EGvnGI+LMdrhIo5UopVSV/Hwvu0gF+8alu/vFSax5meLQ9IkiP
Cq0zmUXInaunZPMC/+aBU7iilYW4WtnugJt35HwWJyxf0RfG4cFJWZPOGQXHZoba
8LT+YRD2sIK6bAnsLIYnYTtYt6IXs7vnxTfrA6LptOuo6ZM/
-----END CERTIFICATE-----";
            }
            else if (orgcode == "khaitancouat")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQVxSq+ecakIlJWhRN5RneETANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQD
EylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMjA0MDgwODEw
NDVaFw0yNTA0MDgwODEwMjNaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQg
U1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxCrsqriuphmx
yN6VFjtgkEF5baeAdlCjlMBWTw/s5dvYoICbmJl/8qwEKiYDmCbe153qTBPOZ8Gis0LDVoh2jBW4
jInKqWsnCDV1SobSgcBwrgamSkOOMFCguR/jYK3gxc9uTytWFwfa8tlcb/urHO68rcvrzxzGa1wy
8nhRN9DEkBu4quh5uGz4avM8eambbXmtC5hm400px59Fw23QLMXqCcOTIDNvIHon6jc+mBqVRSFt
slxH3NRmlKaAs9/LkRZz802f58hlTAhH/BJMzfqY3Z9XhQwpGjXaAPGhHkqmduRnf3a+vgQpkF/o
3f10jKYJrrNJYfk+DoHvck6yMQIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQC+Y5QoiegVA3mTURav
A0WRmyTLxl8516b8w8V7p7W0FdmDQy5+bgL+cNZNzsv5614bUjhgsOqyX90lace3QHr4LncBjwUt
ozwyAb7f41wR5XUY0Q0dpEdWK4jD6o5505sBLwkyjlIBsPmfIXF/7zpBgjRTjiKg+CzwbSooj3dV
wvMKyNGdAohf37JxmQXcWmywGUkxTlO74w8ZhQbcWI2WOmX5DYDZrwf7xyex1qChaXmkUb8EnZcx
25PqT63ojubT7SnV/Yl5mXX1Y7dfFjIIRxZ1ylTtAfEGQSAJdakbAGaIFtmEOfVvolqVv853yX92
dXGXNaV/lVC4gBTMnS4y
-----END CERTIFICATE-----";
            }
            else if (orgcode == "khaitanco")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQOEqkRcZHo5VPhei8DAG+XTANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQD
EylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMjA1MTEwNjU3
NDZaFw0yNTA1MTEwNjU3NDVaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQg
U1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA6nIFJIb89hLJ
80A2AMbwzMj1OCDsbTl6NYg9S+rhcDjMPAXayI8lz9yIXRzmcL5wypXC2QsX8rtYSJJQl1vp+5uv
H1WlFdlCcJmQ4hUR+zLKzXPWuLjqn4KM6bGH1Bp5BBiMTUty/yt4ScwZGl79wZnOrIAhbtt3CdxF
W9POqXnsqvTC2ndBonaN+W3dandzSvdTQ9lM4ZMSeHoM7MK4z8G797qVzg8ME/veyL95w8NWmKig
d7ZCyvXa78tiyzu96hju95PY4Zm77q0Bx/MC7pZkwyqoYsXkdXuOR97/SwXTq+54lnGUP2gh5ZsX
7p1GIVzdRg7A4LxK8S1+cE0R8QIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQAClw4iKL9zEhZxpixA
R5HcBWvxUD3yw/nUMGyb0JhyDUMRxOQCDiZBgRIDy/hsjIxzZ8Kt0Q/9roO02onFWAApFf/loU5w
tzVTkAxg4sEFEjb9XilrtrcXhJx+aYWhuf5cLMirlGB+swhrLpcaoy3iPuUrOil0C0CGCryb8r35
vIbTNk79cPjZEeZ7rnl8gciBqF/hFJwb2XtJ0Gr+45p5Ea11WfffddjYT0cFh2IxCWorgDvZc4Y/
U686AsStkB+rtfy0+6u3YY6F7B0WPOQb50jm/J8Q5wwOOtaLBKsgWUa9Ib187bXJQD3c+Iy5mfPF
/eWu3wpTN2nOkW0C7Ysa
-----END CERTIFICATE-----";
            }
            else if (orgcode == "singlife")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQcyw10HmacJJGKIpwzr0NkTANBgkqhkiG9w0BAQsFADA0
MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZp
Y2F0ZTAeFw0yMjAxMDUwMjE1MzdaFw0yNTAxMDUwMjE1MzdaMDQxMjAwBgNVBAMT
KU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjAN
BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwOhVTmpFASRI3C44TYDWTwr7fgm/
67a2nDd7kFJ5dvVK71SST4iRFW1F9AIAlCKFUL6mCSQk8cwL5S2sunaWaNij/kYw
zGOh8ZhnK/zOt50944eZxn37qqBG1/dftQyn7ugqTaJGYQvk2mgKXGN9SHRD9s32
BgCfqM1AOagUPag6g2dJNtt/gkg379COVv7xCBYtJ8DFuHVQ475TfuWRfMxJfBz9
2LhD8Bgbm8msK8yMgkZdprGhH29dMnwh9BfvGSSpDnogGF9NaIfvf8rjRXxJXKPB
KHmEShNcjDwl94Zd/AfY5/6Ti2xmMbDN58RpNQXhMoQu28DRSv4pIVPm6QIDAQAB
MA0GCSqGSIb3DQEBCwUAA4IBAQAzuu8grCZ33+rltRxHvdtM5rPziBLy2R27UGV5
Du3K49xoDSawrRA08QiY3ydAFg/FV8GTlgGxqitYG5fiRMxSGsh41ELuQSVv7gXK
1U+vaigtbWi5ufYSQM2v7yvEAEhZuB24pDzJZuFAWKJdmHry8K6Fe1KWiuHH2f1R
k+sWN4voL9L3vynNhIMw3IQdVx+CRCxeQy679p6uyfvaz3BK6HS7cjyN00RbS9er
eI5x+arecHVUxUGNoDqDCN8TK4I2YkFHw3CurOr7nn/jTRuwKf8RyRLYn89t/F9B
KCXk28QlRLfP+w7Xxh+vpQ6xusMd2H46WJOPIYKDItICuHhy
-----END CERTIFICATE-----";
            }
            else if (orgcode == "icicis")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQYWKkzjsz65JOL8gAzoyKRjANBgkqhkiG9w0BAQsFADA0
MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZp
Y2F0ZTAeFw0yMjA3MDQwOTQ4MzJaFw0yNTA3MDQwOTQ4MzJaMDQxMjAwBgNVBAMT
KU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjAN
BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAttk99PfChaCYEErfFuVpQFXFHKXf
nWnuFDu8Sm6wzr9sWt7oIHgAU2tzMdfhZLsLsz+h7YUdloBnZnXUw58jtfRjKbSb
9FgY3W+WlgysP4e7cOby2Nvbx+o87uXqDfDxadmCC/Tc6FuqVMWVnZifsRRylFnX
DvUhAOTj/bGCbJ7wMPHH93UOWOhSC6OTko82FaqEg+WKR7WHSP8EjSwp+NhVKwPs
v9VbvVB60WjKfBlzFih0bmikFj/KfUAFxqoru9mV9oHya4kp8uxGqG/a291v4nxf
wEfXpwOL/J3QB3D/Rmz8cp6+MzzujrSSqzUv05cECPzJvWew3jWLTflZgQIDAQAB
MA0GCSqGSIb3DQEBCwUAA4IBAQCBPaKyfxcs1u6WFAm3SOp56RolqCl/OADM0hEs
MKCRlLDtDilWZzEzo9b9OU0U9tiZZ5EO1Q2xbrjk4QIh5PcAk3nYDk3I82FZHVU6
UJKIhsVDghOMxS+zkXXhgxBvpEB5KFS5UZNrWJpH+WMuX4DGME7unPJvUK2HaXTr
d5nwjps7YnK1niukZdxpffjkkjgnw7dp5Ivst0eXLwIlDiZimXEMXFn53rM6hELH
luEHjvP2u7vgrfWMBLo8isMyTXwYriVq21PO/H5gCRvqREePXo7Vk239l4n6+ILg
ULHhtuyN7n3CEMxeAz1sBmpHe1Y/hx8iew10tc8DcjijvD78
-----END CERTIFICATE-----";
            }
            else if (orgcode == "wnsuat")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIEEzCCAvugAwIBAgIJAMw/PuJtmldOMA0GCSqGSIb3DQEBCwUAMHAxFjAUBgNV
BAMTDWluc3Rhc2FmZS5jb20xCzAJBgNVBAYTAklOMRIwEAYDVQQIEwlLYXJuYXRh
a2ExEjAQBgNVBAcTCUJhbmdhbG9yZTESMBAGA1UEChMJSW5zdGFzYWZlMQ0wCwYD
VQQLEwRUZXN0MB4XDTIxMTEyNjA5NTQ1NVoXDTIzMTEyNjA5NTQ1NVowcDEWMBQG
A1UEAxMNaW5zdGFzYWZlLmNvbTELMAkGA1UEBhMCSU4xEjAQBgNVBAgTCUthcm5h
dGFrYTESMBAGA1UEBxMJQmFuZ2Fsb3JlMRIwEAYDVQQKEwlJbnN0YXNhZmUxDTAL
BgNVBAsTBFRlc3QwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCJu8uR
9eSp79ecGPq+iF/I2aZDZwQpGjrI2R6HYd8vcClTtp8jEzUErU88Q2MGzH1HcJa6
q/cV2ZUmNxwaIpeIBjMTQDhU364+SLbyyFHXGWv9aYw9Uqw0Zror8pIfrmfGwkww
U/+AcuLgRXPZgh5pMqR8yAFPPE0J/hyEtNjcuYGJECcWb8MxMn3spGFMX4xRM27P
Y3gaR/h6+VQae5Ak4cJqeOknhTanJiGiD0cac6SW/HTUZf6asmmv1GmCN5Z0o2e3
0fcV34hPiZHPzC2yR/nwe/51YD4KpazGrbHohQckmjcpAY8V4rl/IuAvepul76KI
lLgRT1dYPAf56CKxAgMBAAGjga8wgawwDAYDVR0TBAUwAwEB/zALBgNVHQ8EBAMC
AvQwMQYDVR0lBCowKAYIKwYBBQUHAwEGCCsGAQUFBwMCBggrBgEFBQcDAwYIKwYB
BQUHAwQwEQYJYIZIAYb4QgEBBAQDAgD3MCoGA1UdEQQjMCGGGWludGVybmFsLmFw
cC5pbnN0YXNhZmUuaW+HBH8AAAEwHQYDVR0OBBYEFHXO6Hn8/Ysh6Ug91kCZz+R4
9fTKMA0GCSqGSIb3DQEBCwUAA4IBAQCFcdd2GeCfMIf1wF1VrTAuDQHHo/lLl7H2
1wN2K5Yr/QJZLKX67GM8KHmDxIUHk6FL+3Ly8Qhr3ohvXAsK7ZIV+Y5dni84ZrHy
9PcIYVSLDNEx4ultfXgbQ7EYuFpCGZOaIYN4whXBW5KUYO0cst1O0stiHl781Olj
/gr/E63IyOtM6dfc8V1mfjBBDVLjCWegYPncRZCy9m1xm83z5oVIbScmb8vf+94Y
oLA0qGAIOHO8fStxG8mrjJ5rthLavwKYl62NUzSntqrGYz0CRx5RZEQBpukE1xfs
M0dGCUZ5VCsTaoF62I2Jgv8hc79rragLqQjbpWS+z4nSju+rRJ66
-----END CERTIFICATE-----";
            }
            else if (orgcode == "wnsuatplus")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIDnDCCAoSgAwIBAgIGAYRmU8PiMA0GCSqGSIb3DQEBCwUAMIGOMQswCQYDVQQG
EwJVUzETMBEGA1UECAwKQ2FsaWZvcm5pYTEWMBQGA1UEBwwNU2FuIEZyYW5jaXNj
bzENMAsGA1UECgwET2t0YTEUMBIGA1UECwwLU1NPUHJvdmlkZXIxDzANBgNVBAMM
BmVkY2FzdDEcMBoGCSqGSIb3DQEJARYNaW5mb0Bva3RhLmNvbTAeFw0yMjExMTEx
MDUzMzZaFw0zMjExMTExMDU0MzZaMIGOMQswCQYDVQQGEwJVUzETMBEGA1UECAwK
Q2FsaWZvcm5pYTEWMBQGA1UEBwwNU2FuIEZyYW5jaXNjbzENMAsGA1UECgwET2t0
YTEUMBIGA1UECwwLU1NPUHJvdmlkZXIxDzANBgNVBAMMBmVkY2FzdDEcMBoGCSqG
SIb3DQEJARYNaW5mb0Bva3RhLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBALeIHN0ZBYUkwKtKsuurrhzOul1pRhRreDHAjIR0+gKqCZaiNT3yiStZ
UMm2966+hzLRYJz+C+r1PZVnGWPENOcbzJxFYhRcm1WDv09EFKHsS60FOBHUKxkP
c5hNaCJp1NVGBTFfcBkRStVNciL2J4Fa4QCSQPQdeE8zt6Mj8DmEmT6pGeS9kHwm
9jkrny/a/nMiW3ZkPh6acUt0n0+wO9dpyCgCixzd5VPH7obCx089DwHdHxsdLvIr
hCaH9MRTNmpUbwzpAlHfqquTJUAQaW7tNIGUS+OSr+2l/BNKrhvyixYejeSF2T8e
T0s2XvoO1/p1ewflWZoIip4JzaWo8Z0CAwEAATANBgkqhkiG9w0BAQsFAAOCAQEA
K3uIXdXupDs3ex2o5Cl98GLeoOlm+di1b7tEq0A+COvF6YjCjXheJWEf8YnzrqDm
pEnx+Q46PShJIFiweSbxhuDiUGZ2XYfuyTDVxTz5Vs8kdRwgPbTAUjgWzq2EypTN
rFqQ7+mebALZ6Ew5BH5j+GASRwe/R9LJyLFBo8wbWCJxW4B3X6bm/eXs++NEpQyK
IQcGNS51giDA8v3zx+3nEPt71T+00CY7utak4bYRDYDXF42mMax/sXP1BrRLDndx
ELdf7MkH7IfhMNGoWnC6sTFiueutXqaFn6F9swZqu0AF0WdPjewL7BVxGxCUOVxR
018vO5WCBTyKob5TXHWKKw==
-----END CERTIFICATE-----";
            }
            else if (orgcode == "wnsplus")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIDnDCCAoSgAwIBAgIGAYYNIXUSMA0GCSqGSIb3DQEBCwUAMIGOMQswCQYDVQQG
EwJVUzETMBEGA1UECAwKQ2FsaWZvcm5pYTEWMBQGA1UEBwwNU2FuIEZyYW5jaXNj
bzENMAsGA1UECgwET2t0YTEUMBIGA1UECwwLU1NPUHJvdmlkZXIxDzANBgNVBAMM
BmVkY2FzdDEcMBoGCSqGSIb3DQEJARYNaW5mb0Bva3RhLmNvbTAeFw0yMzAyMDEx
MzE4MDFaFw0zMzAyMDExMzE5MDFaMIGOMQswCQYDVQQGEwJVUzETMBEGA1UECAwK
Q2FsaWZvcm5pYTEWMBQGA1UEBwwNU2FuIEZyYW5jaXNjbzENMAsGA1UECgwET2t0
YTEUMBIGA1UECwwLU1NPUHJvdmlkZXIxDzANBgNVBAMMBmVkY2FzdDEcMBoGCSqG
SIb3DQEJARYNaW5mb0Bva3RhLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBAMP0o0XprIxjwhE32Y7o7V2fTnk8OboPyNOB62JOTc830I408nebG8My
E85AtNuwKwiyYPMx7zfdw3WGTVQKYp9WxmTI29yYtgLJvFFduEhNjTnCBpSntMAn
8bnfCFeq0rALQG63ua2fGevp0PHcZYl3wHAuVpOcTVAfSy9lZiMZhjUmqpzKszse
JMcI2OTBWjkOEV0MhabB5UDXGDXbq1xv1AY6tiYb7QChfBUGx4lS9pyf01Ixw2VE
+H1UINBNmTiYfWNHue4TryipS1DMMXQbniESg0EEkz4nM760819vY8GSewqfAL+i
Wk9ziQ5p6wU7I2VGhHy+jvj/eBJLvFsCAwEAATANBgkqhkiG9w0BAQsFAAOCAQEA
vvcGH/eVu1P8xoKgr+Ckhx7fxSA6ZHfy813WcFYH+Nyh0ij1DfEeAS9SGA3UMvke
45aipwB2RogwCASeICG6aU3QwAGBzHDBkuoJjv/E8/XCE5KMb9/p6mtLHeofbzbM
5yRvAhKwJqTkyMcT+/MliCB2yoE3/l8ZmnnmfxQgrPh6eGG4M+/OQ1A1zbUZQdBX
0uCp0ZyCHYNaZECQAD8aotWA43g80f5/VF3yQKZqwUzGEImnuJy5Lwn/LseWy7Sh
HNxaU5EjLqxxvowtxM87j2JeFEiw74MlfORERHJfPVgLnXoZ9nS5J+u1K+4/shxb
K8R4voBLP7W7rdFn6mTdWg==
-----END CERTIFICATE-----";
            }
            else if (orgcode == "wns")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIEEzCCAvugAwIBAgIJAMw/PuJtmldOMA0GCSqGSIb3DQEBBQUAMHAxFjAUBgNV
BAMTDWluc3Rhc2FmZS5jb20xCzAJBgNVBAYTAklOMRIwEAYDVQQIEwlLYXJuYXRh
a2ExEjAQBgNVBAcTCUJhbmdhbG9yZTESMBAGA1UEChMJSW5zdGFzYWZlMQ0wCwYD
VQQLEwRUZXN0MB4XDTIyMDUxMzA4MDEwOVoXDTI0MDUxMzA4MDEwOVowcDEWMBQG
A1UEAxMNaW5zdGFzYWZlLmNvbTELMAkGA1UEBhMCSU4xEjAQBgNVBAgTCUthcm5h
dGFrYTESMBAGA1UEBxMJQmFuZ2Fsb3JlMRIwEAYDVQQKEwlJbnN0YXNhZmUxDTAL
BgNVBAsTBFRlc3QwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCKze3v
9uqXz96nzmDEwKXK6jAlduMaMBnJDSFyh91jRu8ZvOAG0V58fGBJnnxVN4ZZ9Pm3
GjdCIeyCbTPay90GyjBogNEObFjdLUt9Dr48gkj67ftQVx4cBCN1/LrwqATq/Wkx
gyTeDqg2uRNqgntvrFExaW8bbfkAldP4aNUq4qL4SETlqks3/MV/hhsu+nKsQ1Ak
aDLEdE3Bb8yfj3v8rb++r6dVY7+G2gnckyS+aggNkmplDX0NTAyPFQ8w9K6FCQnP
epxP3eYYlGiZOsWgHBVmaIrbUIwSyoaVtL9+wXimRJw4nWyYtjxqg9Gkk7Cb4gdB
G9ypqfL/nLxzIuvLAgMBAAGjga8wgawwDAYDVR0TBAUwAwEB/zALBgNVHQ8EBAMC
AvQwMQYDVR0lBCowKAYIKwYBBQUHAwEGCCsGAQUFBwMCBggrBgEFBQcDAwYIKwYB
BQUHAwQwEQYJYIZIAYb4QgEBBAQDAgD3MCoGA1UdEQQjMCGGGWludGVybmFsLmFw
cC5pbnN0YXNhZmUuaW+HBH8AAAEwHQYDVR0OBBYEFG/uGmXe4/pupjzNdZl/SnxL
2sigMA0GCSqGSIb3DQEBBQUAA4IBAQBr5UV02j1tcEpaQOf/E9VIsBPu498kvrpX
tZ0K0ASfYubZfuHjxJDnDhah2xPC+jqb0ufEjx4zgXp6CcQEBjIXZ5BcAcqP0C5z
YVg73amCPk7s8kxm52mwvoVkCnzjbQIGP85HOeMTHptPxEOgzwsFVjdzz+xzlveK
SD6p3kjSjiBJWwa03XCWvLXXSGWSNboW5ekKAypMXamYrfYoTf90JT+zXALhs7WL
Mt/azPoFbMko53a2MYBn/s+MpPgpIDZLBKPfcHzEJ9ZgLq276fOCahl5GK4tLjM0
R2QDkO/hko23jRAJnye/nSZ6d7oiT2vGOPAlo3r5zqD10f3ICF9I
-----END CERTIFICATE-----";
            }
            else if (orgcode == "page")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQdumo5z9624NHIGbKq8l78TANBgkqhkiG9w0BAQsFADA0
MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZp
Y2F0ZTAeFw0yMjA4MjIwOTMwMjRaFw0yNTA4MjIwOTMwMjRaMDQxMjAwBgNVBAMT
KU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjAN
BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAulS6izL6Uz4+hYz2fT0wV0el6L3E
weZO9SsNjQPrPn1HDQilonbINgiwfsUYhRuAjjilPRSYaukiFiKlcXCh1LYY+BwK
QvDvPOFiUn4gq6uCXzBbWMqGHZCmFa9w0tW5rnwdaTknA/OqBhfbU7aF5TlgfFuG
KT6dHHsHbw/DLlR7d2Xx5wCAo18G1+IU+adyc0mGuuqpOm7mUq0nV8M/PCmcN+yN
8qNmtSDDlO5LXwQ66dJ3nZppogXtBdT571Y6IfYdFUCAT14qSRawLGZ2H/Wv9hMa
3UOGZ3vnZvozVTgtSREQ4AEJVXLFXOqGB2qrg7ys/bLm/TadoDQY8JA9gQIDAQAB
MA0GCSqGSIb3DQEBCwUAA4IBAQACcxDBSDsLnBykyuXb9O6/1h2m+K80S0+F46CN
UejZLApVp6eBv+NLeQEMvZ/1mqcfn3vcDwWuc1BTzkcL7T7bOQapZSJ2j0kboWdJ
omAj6CD4jlTvt678l+/QfOjKnkplDfNzILOdIDiRPcDRciRu8lqBqc5mAr22/1W0
eH0l51XTpTTw8KN/9ACmJZwyTip/+JGbGE5x8+hEOX9z1MnnrF/GIo8YerVz3mBu
GXfoeV9NTPImOSjpg3y5C+htY+Z1EVcbuv76aBBU832oQ1W2fAXfs6Zm9by1/S84
jchlkhcbw8MEgHSR2bxmkr4JJycVxPfb4X498qUDLwRLC5de
-----END CERTIFICATE-----";
            }
            else if (orgcode == "darwin")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQfsFvgwXrVaRB5OWpM5SKpzANBgkqhkiG9w0BAQsFADA0
MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZp
Y2F0ZTAeFw0yMzAzMzAwNjA5NTNaFw0yNjAzMzAwNjA5NTNaMDQxMjAwBgNVBAMT
KU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjAN
BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA2yl5bg4wkcFI5wyK1cnsxQSheitS
GG/Y/WhEt3Oq15VjkHgY2rIHZgno+HDqVxJF9bwV7OQxeh5ojQoGifK0amq7UNlr
CxZkI5kuC5PqpfPFFdjHLCdmul5TzZS21XDbYQEgAC1IN2uk0p1wNBGoBVaStxr/
1ob2yKt3dHopS2pvqOEC2nwyzPcu9jRupQDG/gjzSmP342NRvtNOqpPvZXyNc7Ks
yWUaH6N+7+NJZnbwEtDttikBGys4m3pLupbd/Sz2eveBsfX2SU5dTVpNwcvhNqRk
GhGfb276K3xF3eqjGl+lv9bVZWd4KwhLuW6nt3cMhxRRzMXPVvhLTTMiiQIDAQAB
MA0GCSqGSIb3DQEBCwUAA4IBAQA9pHCjvtr44kyIUynLYe5q+JV65JQsQnmvr0/j
5ZYBj/LiCS8fce+7k4mxMom4usafWoKSr8tdyIeLEAkJ9gr/9YzOaIeQRLp/6AvX
8LWiTH0lA1kWYz9/T8EIXLfHFIGLjso+mE7pWGQZ44vUKufS+LJ0xq+lVR3EA26b
X8aHRLnS+S+n0ViidgSZUPCPsbIcj/nkvbLR6e9BajeOMt3sXxSgjuGVPfPL8GQb
jMWQuX5/eGtP+iYldFVomlfs+nTfh9B6uhMPJ5dY8RqYHscOyz0NF5XHugoXHpB1
bBDMy4m9scwW+SLy1mt/4mxA3TRBkbCiSwnRyEztjQ064i/0
-----END CERTIFICATE-----";
            }
            else if (orgcode == "thermax")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQEWHXcVbbJI9FgQet84iiMzANBgkqhkiG9w0BAQsFADA0
MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZp
Y2F0ZTAeFw0yMzA0MTMxMzA3NDRaFw0yNjA0MTMxMzA3NDRaMDQxMjAwBgNVBAMT
KU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjAN
BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyzq5nkgoAGRYDR9xNd5SQ5bTXPYT
rjxOMpa1mu7p5HfNicolAnBztbVXUSCT/o/kOaRcGD8lxA4uLrPS4bjfyWhozhHi
/ZIa/pIcJk5+KvOXuyCOf5+km66x1CYGz2YG1jUWAHKX+bfBHRHhS60+5lGQAOjw
Mi0V/tpbt78ftdY/16Zcxrm8kJaT0rK1m/DfE0Enqwz3ArlcYQ3maurrs83yZCZR
ZN/zMWJaxEPqHRUY3U8s/9cRRZ2VSsn1mCxoIk9GRYfp0gmOyai/qaoi/bFxAb54
7Is1Nqu+61sz3kh+2Kha0WFhu+zfjlFTI58113RgT34MK+Yr98b9pAaIJQIDAQAB
MA0GCSqGSIb3DQEBCwUAA4IBAQAZ4REOQFXNhmlmyM430K/qbgSTDdTBMd+sRh33
6YfjopPzyU2Q8A3g99Psqgl70sujUGbztWcKjmqSzr+hgcbSSIomQnqRksbPXph1
JtiEJE0rdc1MTLfaZfXd+KNtLv2qeFmSqRAYk4f0RmsGkgzB1sLX/i5gjKg+KmiO
TViDAmIKVBVnoJjyeLTloKAvG6x+IpXJZYYLNF3eh1z3mLSfPdIVrW1cZRYGxqzS
yrnBfqjpsaMbZKZdFm2sM9+1hfLldO57yb1R26muQySspISuEOOU6Heamt4NDp8Y
IwTt/KCjLiEPy8YJSUZePs5ODLlNxDqDO/iAgPIHDtJZwh51
-----END CERTIFICATE-----";
            }
            else
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQeFlOcMBKlpFFV2yAhltqPDANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQD
EylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMDA3MjkxMjUy
MDlaFw0yMzA3MjkxMjUyMDlaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQg
U1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxktISos/Iaq0
QD8mKz96d7XWzyAtr3g0PcfcPzHEtTUjNsKeR2iRjLwsIHoTVjB07Kcg8FwB/S7p9rFV/9tBx9Nz
aTi9RlegP4mfN7TbJll2wzJSiDBkeg1+/nkKhroERQ/MJW+vMkR+/v2J+QdWYFstdaOmj6PX/1tE
vAQYSWtI8Az6SkjrUhTdy/YWQx4URYKDgTEJ+BURsNc+JyP6CkUPXeXl9+wIVzcuEAZ063jkyCvk
ThKJ8nsnFuvF8nbpZsPbnuklmYYQTDFkwFDDDdhj/rAps64GKZWtjJ89XNEuQjCxR4Sy3CVOpGWr
PBH2GuVwDvvx0Xl3hgifqa0I+QIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQA5/2Q9POV7WbIZlTiw
pV+OsQ7cG2k0i0HgkCSScEMybkhjpRsFpf5vH4Mw9mkKQBI2CIq8YSlXvy599IVE7eWOt9+10DoF
l7rr5SnAnS4Coprr7Szi+qmITrk9Ld3ymVdHScazUqkHvPntW0iylWTfC7w0UUPKKaTCOwIIUg1m
oSZ24NhMFzm0erTVASZntV11ep0I45BFf9UECTM/re+u7USxYi1vGP6lTuk7exWV5xLk+W5MXAz6
WcQ6uPwnP+fE7WbfZhpHencmqHtMcv2RYJcQfR6M/FMix2ragD1BinfrZYRdKxH8fRvgXqq51ujS
z6yKcucIhJy9J4bQyVj1
-----END CERTIFICATE-----";
            }
            debugLog(samlCertificate);
            Saml.Response samlResponse = new Response(samlCertificate);
            samlResponse.LoadXmlFromBase64(apiSAMLInfo.SAMLResponse);
            debugLog(samlResponse.Xml);
            string username, samlEmail;
            try
            {
                username = samlResponse.GetNameID();

                samlEmail = samlResponse.GetEmail();
                debugLog(string.Format("username:{0} email:{1}", username, samlEmail));
                //debugLog(string.Format("RelayState:{0}", apiSAMLInfo.RelayState));
                //debugLog(string.Format("RelayId:{0}", Convert.ToInt32(Request.Query["Relay"])));
                string cacheParam = apiSAMLInfo.RelayState;


                if (orgcode == "firstamerican" || orgcode == "khaitancouat" || orgcode == "khaitanco" || orgcode == "icicis" || orgcode == "wnsuat" || orgcode == "wns" || orgcode == "page" || orgcode == "wnsuatplus" || orgcode == "wnsplus" || orgcode == "darwin" || orgcode == "thermax")
                {
                    if (samlResponse.Xml.Contains("khaitancouat"))
                    {
                        orgcode = "khaitancouat";
                    }
                    else if (samlResponse.Xml.Contains("khaitanco"))
                    {
                        orgcode = "khaitanco";
                    }
                    else if (samlResponse.Xml.Contains("firstamerican"))
                    {
                        orgcode = "firstamerican";
                    }
                    else if (samlResponse.Xml.Contains("icicis"))
                    {
                        //OrgCode Mentioned
                        orgcode = "icicis";
                    }
                    else if (samlResponse.Xml.Contains("wnsuat"))
                    {
                        orgcode = "wnsuat";
                    }
                    else if (samlResponse.Xml.Contains("wns"))
                    {
                        orgcode = "wns";
                    }
                    else if (samlResponse.Xml.Contains("wnsuatplus"))
                    {
                        orgcode = "wnsuatplus";
                    }
                    else if (samlResponse.Xml.Contains("wnsplus"))
                    {
                        orgcode = "wnsplus";
                    }
                    else if (samlResponse.Xml.Contains("page"))
                    {
                        orgcode = "page";
                    }
                    else if (samlResponse.Xml.Contains("darwin"))
                    {
                        orgcode = "darwin";
                    }
                    else if (samlResponse.Xml.Contains("thermax"))
                    {
                        orgcode = "thermax";
                    }
                    cacheParam = orgcode;

                }
                debugLog("cacheParam:" + cacheParam);
                var OrgnizationConnectionString = await CacheTryGetValueSetO365(cacheParam);
                debugLog("Connection String:" + OrgnizationConnectionString);
                this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);

                var userExists = cacheParam == "icicis" ? await this._userRepository.EmailExists(samlEmail) : await this._userRepository.EmailExists(username);
                if (userExists)
                {
                    debugLog("User exists");
                    string email = cacheParam == "icicis" ? Security.EncryptForUI(samlEmail) : Security.EncryptForUI(username);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));
                    if (orgcode == "ghfl")
                    {
                        var url = "https://ghfl.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else if (orgcode == "nsdc")
                    {
                        var url = "https://nsdc.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else if (orgcode == "ltfs")
                    {
                        var url = "https://ltfs.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else if (orgcode == "altum")
                    {
                        var url = "https://altum.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else if (orgcode == "firstamerican")
                    {
                        var url = "https://firstam.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else if (orgcode == "khaitancouat")
                    {
                        if (Request.QueryString.HasValue)
                        {
                            var guid = Request.QueryString.Value.Replace("?Relay=", "");
                            CacheKeys.CacheSamlEmailId.Add(guid, username);
                        }
                        var url = "https://khaitancouat.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        debugLog(string.Format("redirecting to :{0} ", url));
                        return Redirect(url);
                    }
                    else if (orgcode == "khaitanco")
                    {
                        if (Request.QueryString.HasValue)
                        {
                            var guid = Request.QueryString.Value.Replace("?Relay=", "");
                            CacheKeys.CacheSamlEmailId.Add(guid, username);
                        }
                        var url = "https://khaitanco.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        debugLog(string.Format("redirecting to :{0} ", url));
                        return Redirect(url);
                    }
                    else if (orgcode == "page")
                    {
                        if (Request.QueryString.HasValue)
                        {
                            var guid = Request.QueryString.Value.Replace("?Relay=", "");
                            CacheKeys.CacheSamlEmailId.Add(guid, username);
                        }
                        var url = "https://turn2learn.pageind.com//ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        debugLog(string.Format("redirecting to :{0} ", url));
                        return Redirect(url);
                    }
                    else if (orgcode == "icicis")
                    {
                        if (Request.QueryString.HasValue)
                        {
                            var guid = Request.QueryString.Value.Replace("?Relay=", "");
                            CacheKeys.CacheSamlEmailId.Add(guid, samlEmail);
                        }
                        var url = "https://mpower.lxp.enthral.ai/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        debugLog(string.Format("redirecting to :{0} ", url));
                        return Redirect(url);
                    }
                    else if (orgcode == "wnsuat")
                    {
                        int courseId = Convert.ToInt32(Request.Query["Relay"]);
                        //if (Request.QueryString.HasValue)
                        //{
                        //    var guid = Request.QueryString.Value.Replace("?Relay=", "");
                        //    CacheKeys.CacheSamlEmailId.Add(guid, username);
                        //}
                        if(courseId == 0)
                        {
                            var url = "https://wnsuat.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }
                        else
                        {
                            var url = "https://wnsuat.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow + "/" + courseId;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }       

                    }
                    else if (orgcode == "wnsuatplus")
                    {
                        int courseId = Convert.ToInt32(Request.Query["Relay"]);
                        //if (Request.QueryString.HasValue)
                        //{
                        //    var guid = Request.QueryString.Value.Replace("?Relay=", "");
                        //    CacheKeys.CacheSamlEmailId.Add(guid, username);
                        //}
                        if (courseId == 0)
                        {
                            var url = "https://wnsuat.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }
                        else
                        {
                            var url = "https://wnsuat.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow + "/" + courseId;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }

                    }
                    else if (orgcode == "wns")
                    {
                        int courseId = Convert.ToInt32(Request.Query["Relay"]);
                        //if (Request.QueryString.HasValue)
                        //{
                        //    var guid = Request.QueryString.Value.Replace("?Relay=", "");
                        //    CacheKeys.CacheSamlEmailId.Add(guid, username);
                        //}
                        if (courseId == 0)
                        {
                            var url = "https://wns.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }
                        else
                        {
                            var url = "https://wns.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow + "/" + courseId;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }

                    }
                    else if (orgcode == "wns")
                    {
                        int courseId = Convert.ToInt32(Request.Query["Relay"]);
                        //if (Request.QueryString.HasValue)
                        //{
                        //    var guid = Request.QueryString.Value.Replace("?Relay=", "");
                        //    CacheKeys.CacheSamlEmailId.Add(guid, username);
                        //}
                        if (courseId == 0)
                        {
                            var url = "https://wns.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }
                        else
                        {
                            var url = "https://wns.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow + "/" + courseId;
                            debugLog(string.Format("redirecting to :{0} ", url));
                            return Redirect(url);
                        }

                    }
                    else if (orgcode == "singlife")
                    {
                        var url = "https://singlife.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else if (orgcode == "darwin")
                    {
                        if (Request.QueryString.HasValue)
                        {
                            var guid = Request.QueryString.Value.Replace("?Relay=", "");
                            CacheKeys.CacheSamlEmailId.Add(guid, username);
                        }
                        var url = "https://darwin.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        debugLog(string.Format("redirecting to :{0} ", url));
                        return Redirect(url);
                    }
                    else if (orgcode == "thermax")
                    {
                        if (Request.QueryString.HasValue)
                        {
                            var guid = Request.QueryString.Value.Replace("?Relay=", "");
                            CacheKeys.CacheSamlEmailId.Add(guid, username);
                        }
                        var url = "https://thermax.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        debugLog(string.Format("redirecting to :{0} ", url));
                        return Redirect(url);
                    }
                    else
                    {
                        var url = "https://ghfl.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                }
            }
            catch (Exception ex)
            {
                debugLog(Utilities.GetDetailedException(ex));
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
            debugLog("End of function");
            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
        }



        [AllowAnonymous]
        [HttpPost("connect/ADFS/{orgcode?}")]
        public async Task<IActionResult> samlADFS([FromForm] ApiSAMLADFSInfo apiSAMLInfo, string orgcode = null)
        {
            var val = Request.QueryString.Value;
            string samlCertificate = null;
            StringBuilder str = new StringBuilder("test ");
            // specify the certificate that your SAML provider has given to you
            if (orgcode == "ltfs")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
                MIIGcTCCBVmgAwIBAgIQCyr8z/2WdXHC1DvsY10WzTANBgkqhkiG9w0BAQsFADBc
                MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
                d3cuZGlnaWNlcnQuY29tMRswGQYDVQQDExJUaGF3dGUgUlNBIENBIDIwMTgwHhcN
                MTkwMzExMDAwMDAwWhcNMjEwNjA5MTIwMDAwWjBwMQswCQYDVQQGEwJJTjEUMBIG
                A1UECBMLTWFoYXJhc2h0cmExDzANBgNVBAcTBk11bWJhaTEYMBYGA1UECgwPTCZU
                IEZJTkFOQ0UgTFREMQswCQYDVQQLEwJJVDETMBEGA1UEAwwKKi5sdGZzLmNvbTCC
                ASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKLexY6Y2QTjtJ5SvW6GOgsn
                61KGf4tM5q/5Z067YECN7SWBRKlpmv2SQgPbxxyiIdcaCthqxVzL8GGlvjkM9Hn9
                4VsuRZs6PvUvGB64yIiqiYW6Kq30iJBJBxjqgrLgm2dWf/Xk/FUnxDwTjJ+7Ju5+
                o1tix3hFg7UstPCL+G3gLrEjKG6wZwdlaO5NT/P4SaYcNjOOCSdAvtWS/XcsNmX6
                eQ2HqVMBJ0Tpxzb9SbaMUGDpmDMbXwI8LqKRHhhdoyTLT1F7i3TMhKNi2t9+Uo3H
                PoMgZM/xZF9UBGW3PE9BPjr2OIaQwdiY8wVh22obf+mxMMnn5xOeG6F1bA/tNE0C
                AwEAAaOCAxkwggMVMB8GA1UdIwQYMBaAFKPIXmVU5TB4wQXqBwpqWcy5/t5aMB0G
                A1UdDgQWBBTqQ48ZvWCzZ45JsDCYN0zQehucnTAfBgNVHREEGDAWggoqLmx0ZnMu
                Y29tgghsdGZzLmNvbTAOBgNVHQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUH
                AwEGCCsGAQUFBwMCMDoGA1UdHwQzMDEwL6AtoCuGKWh0dHA6Ly9jZHAudGhhd3Rl
                LmNvbS9UaGF3dGVSU0FDQTIwMTguY3JsMEwGA1UdIARFMEMwNwYJYIZIAYb9bAEB
                MCowKAYIKwYBBQUHAgEWHGh0dHBzOi8vd3d3LmRpZ2ljZXJ0LmNvbS9DUFMwCAYG
                Z4EMAQICMG8GCCsGAQUFBwEBBGMwYTAkBggrBgEFBQcwAYYYaHR0cDovL3N0YXR1
                cy50aGF3dGUuY29tMDkGCCsGAQUFBzAChi1odHRwOi8vY2FjZXJ0cy50aGF3dGUu
                Y29tL1RoYXd0ZVJTQUNBMjAxOC5jcnQwCQYDVR0TBAIwADCCAXsGCisGAQQB1nkC
                BAIEggFrBIIBZwFlAHUA7ku9t3XOYLrhQmkfq+GeZqMPfl+wctiDAMR7iXqo/csA
                AAFpbHtPUAAABAMARjBEAiBvb1W3FD+vuVF6jztObo0SLdbeq/P9r9KjahJ9AmpO
                lAIgVO6vhDQiytYrQDCxnGH/Q/0ILNXfg4k1ymp2m6946ccAdQCHdb/nWXz4jEOZ
                X73zbv9WjUdWNv9KtWDBtOr/XqCDDwAAAWlse1CDAAAEAwBGMEQCIFlPXyhevlo/
                qBDaGL9GR75ez45vg+vT31Cx79OALrsIAiBmysqW6kDTsWwsh+kIgU9jCVIrUkrf
                o7s/HhbRT9+MlgB1AESUZS6w7s6vxEAH2Kj+KMDa5oK+2MsxtT/TM5a1toGoAAAB
                aWx7T2UAAAQDAEYwRAIgdRA02QDNb9x3WvzhQeujb/EXvXm76rN0EAM9NT8GjVIC
                IBH2yPKXmSmJTsNQBlMiwvZ/5TG4koJ4SAvy7YPCOndLMA0GCSqGSIb3DQEBCwUA
                A4IBAQAKcpd4saY7y42squ4f9FpsB4ErIWci1xRbFay6MhmuRKwCPrPYy3OneTkB
                1jmEluSa7uulxKec8/kjIE1wRhglnq8V84ixHVtGBq5CVQwK8yUWBrWLCx/zsBwn
                B6GgrpqgpY0Kr1f3l4k7ItYttBLdL6YgTLQOxqIqDxoZjNDoziRKdY5NUPWn1omq
                uHgRCzlLpaeyPt9TBnIQ0b4fX+4ngecdy1AYmJHYkDHNuMmdfaj1L1+mbPkEmYZE
                7UHR42IB/hM5NRfa29DmvRcB7pmqyT5UYcbyoptsSQ192+e8vExbjOBONoA2R+vR
                ibtiiVXhSpmrvXbSAdOx5n4uG8ix
                -----END CERTIFICATE-----";
            }
            else if (orgcode == "ujjivan")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC3DCCAcSgAwIBAgIQRe+DnQ9w2blI9WlzCkqvgTANBgkqhkiG9w0BAQsFADAq
MSgwJgYDVQQDEx9BREZTIFNpZ25pbmcgLSBhZGZzLnVqaml2YW4uY29tMB4XDTIw
MTEwNjAzMDcxNloXDTIxMTEwNjAzMDcxNlowKjEoMCYGA1UEAxMfQURGUyBTaWdu
aW5nIC0gYWRmcy51amppdmFuLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBAMH46mi5K0ny9YZ7DNFuS+FlFaguVSu3eOh3X+v/mIr6epa4LXJKA56u
nV8NkQsDeLpai9BP0ZqBkczmMrDN4dI/VptQWYoDr9ftQ4CKX5glED3AnWMbf03u
rRp/TV1Iekxzkoom6jI/S2VL/VoacTa7kxWDBbvlJ7NOs6x3MFqdr0fOjd9YIDnu
t76Ojc36M2hBhnYZlygDK5GN349s+1bruDgY9D9tDLI5nXZg93t4SdKU1ztPZfdX
MCiI5B6ATL3NRmCIyiOV6iwtNFIk6E8K3wEPBDPEZqdqPNlBP+KOlcv1yqUl/EHa
fvMVXfyu7730mkW6wGaT4UEtOsPInPkCAwEAATANBgkqhkiG9w0BAQsFAAOCAQEA
GWXEO+9+A/Zpe0mYWCPoblAHedbx+05iL3wZ3cj18+5soYqNlTe4NOvKiYfzrbzF
0ttcbmz518NfWCZaCO2lILkutZtrJHtTSVTmV2SC5pUNcGbWXB7wNL4AH7KpkkzK
nHRIkuPP7rAOubDFwIUYeKAtPkh0lCj2YEOAyGkGKtnH9mt6EiPZYdjlVNnHFe2X
WPA0Tn/OY9dx7faCx81A2VaY8+cq5y4LaSo5YMYDqBkq8ln17kzTljuWWHr/mthy
b4AIwv6MSotSJQqfNS8eeY5lRc5pE7N86uLuCfVWObuTl9H+rtZki6dApivLCgQJ
YxBXBTxNZPWbvW/JLkqtNQ==
-----END CERTIFICATE-----";
            }
            else if (orgcode == "firstamerican")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQWeNpDL7blIJDtv3GWyfdvjANBgkqhkiG9w0BAQsFADA0
MTIwMAYDVQQDEylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZp
Y2F0ZTAeFw0yMjA3MjExMzI5MDBaFw0yNTA3MjExMzI5MDJaMDQxMjAwBgNVBAMT
KU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQgU1NPIENlcnRpZmljYXRlMIIBIjAN
BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwwKEnJAwP+TCDwNwtSTm7azqB/d9
HHIeik8GllA70t1MpsNaeGKeBscsRpg3uQvyFfBk4Xp+AAfJAAWI4VxUiplxtoYo
DXLMjIFNJoQNaCbi/IoabUVC3tB2mA54IT7CTXMpTadZHEfkg4TJMvH21Oz66BR+
6AF81YeDCloBI9lCBR2gj/sMj6YCcCG52oq8Jiha1rsfjxYZnTzSsESFtxE3klq6
lO8u/cl/TAEcs+yQ4PH8irFlg1yvqPkms5lQ2Nd5G1d8BWEY6A2zKME9ND760DW8
WW1TtShT6QNek6+8HA5o3E6Rr7pdGwCg7g+kfccjF8jeidnmm0gcEUxuyQIDAQAB
MA0GCSqGSIb3DQEBCwUAA4IBAQA3afOgwK94qMz/OZ5RL00tcIfMg4ROiYiZpCI2
XjRIAYxV4dgAe8RmkALNM6qR9V2gLXNARuM1qnSPb6GiGq2+ZMx4/PgUljVC7dmR
SK9U1pKbCdPi3sahxFXH0JqzTe+QGA1PABfWY7uZcMUWwumli7PBKe4336HxWyDt
Fs6V0Fbbwiwo+5EGvnGI+LMdrhIo5UopVSV/Hwvu0gF+8alu/vFSax5meLQ9IkiP
Cq0zmUXInaunZPMC/+aBU7iilYW4WtnugJt35HwWJyxf0RfG4cFJWZPOGQXHZoba
8LT+YRD2sIK6bAnsLIYnYTtYt6IXs7vnxTfrA6LptOuo6ZM/
-----END CERTIFICATE-----";
            }
            else if (orgcode == "khaitancouat")
            {
                samlCertificate = @"-----BEGIN CERTIFICATE-----
MIIC8DCCAdigAwIBAgIQVxSq+ecakIlJWhRN5RneETANBgkqhkiG9w0BAQsFADA0MTIwMAYDVQQD
EylNaWNyb3NvZnQgQXp1cmUgRmVkZXJhdGVkIFNTTyBDZXJ0aWZpY2F0ZTAeFw0yMjA0MDgwODEw
NDVaFw0yNTA0MDgwODEwMjNaMDQxMjAwBgNVBAMTKU1pY3Jvc29mdCBBenVyZSBGZWRlcmF0ZWQg
U1NPIENlcnRpZmljYXRlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxCrsqriuphmx
yN6VFjtgkEF5baeAdlCjlMBWTw/s5dvYoICbmJl/8qwEKiYDmCbe153qTBPOZ8Gis0LDVoh2jBW4
jInKqWsnCDV1SobSgcBwrgamSkOOMFCguR/jYK3gxc9uTytWFwfa8tlcb/urHO68rcvrzxzGa1wy
8nhRN9DEkBu4quh5uGz4avM8eambbXmtC5hm400px59Fw23QLMXqCcOTIDNvIHon6jc+mBqVRSFt
slxH3NRmlKaAs9/LkRZz802f58hlTAhH/BJMzfqY3Z9XhQwpGjXaAPGhHkqmduRnf3a+vgQpkF/o
3f10jKYJrrNJYfk+DoHvck6yMQIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQC+Y5QoiegVA3mTURav
A0WRmyTLxl8516b8w8V7p7W0FdmDQy5+bgL+cNZNzsv5614bUjhgsOqyX90lace3QHr4LncBjwUt
ozwyAb7f41wR5XUY0Q0dpEdWK4jD6o5505sBLwkyjlIBsPmfIXF/7zpBgjRTjiKg+CzwbSooj3dV
wvMKyNGdAohf37JxmQXcWmywGUkxTlO74w8ZhQbcWI2WOmX5DYDZrwf7xyex1qChaXmkUb8EnZcx
25PqT63ojubT7SnV/Yl5mXX1Y7dfFjIIRxZ1ylTtAfEGQSAJdakbAGaIFtmEOfVvolqVv853yX92
dXGXNaV/lVC4gBTMnS4y
-----END CERTIFICATE-----";
            }
            Saml.Response samlResponse = new Response(samlCertificate);
            try
            {
                samlResponse.LoadXmlFromBase64(apiSAMLInfo.SAMLResponse);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = str.ToString(), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

            }

            string username, samlEmail;
            try
            {
                username = samlResponse.GetNameID();
                samlEmail = samlResponse.GetEmail();

                var OrgnizationConnectionString = await CacheTryGetValueSetADFS(orgcode);
                this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);

                if (await this._userRepository.UserIdExists(username))
                {

                    string email = Security.EncryptForUI(username);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));
                    if (orgcode == "ltfs")
                    {
                        var url = "https://ltfs.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else if (orgcode == "ujjivan")
                    {
                        if (Request.QueryString.HasValue)
                        {
                            var guid = Request.QueryString.Value.Replace("?Relay=", "");
                            CacheKeys.CacheSamlEmailId.Add(guid, username);
                        }

                        var url = "https://ujjivan.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else
                    {
                        var url = "https://gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidUser), Description = "Name ID,Email" + samlResponse.GetNameID() + "," + samlResponse.GetEmail() });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }


        [AllowAnonymous]
        [HttpPost("GetUserName")]
        public IActionResult GetUserName([FromBody] ApiGetUserNameADFS ADFSData)
        {

            try
            {
                if (CacheKeys.CacheSamlEmailId.ContainsKey(ADFSData.Id))
                {
                    var username = CacheKeys.CacheSamlEmailId.GetValueOrDefault(ADFSData.Id);
                    CacheKeys.CacheSamlEmailId.Remove(ADFSData.Id);
                    return Ok(new { Username = username });
                }
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }

        }

        [AllowAnonymous]
        [HttpGet("GetUserName/{Id?}")]
        public IActionResult GetUserName_Get(string Id)
        {

            try
            {
                if (CacheKeys.CacheSamlEmailId.ContainsKey(Id))
                {
                    var username = CacheKeys.CacheSamlEmailId.GetValueOrDefault(Id);
                    CacheKeys.CacheSamlEmailId.Remove(Id);
                    return Ok(new { Username = username });
                }
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }

        }


        /*[AllowAnonymous]
        [HttpGet("SAMLRequestGenerate/{Id?}")]
        public IActionResult SAMLRequestGenerate(string Id)
        {
            try
            {

                string samlEndpoint = "https://adfs.ujjivan.com/adfs/ls";
                string returnurl = "https://gogetempowered.com:10000/api/v1/saml/connect/ADFS/ujjivan";
                if (!string.IsNullOrEmpty(Id))
                {
                    returnurl = returnurl + "?Relay=" + Id;
                }
                var request = new ADFSSAML.AuthRequest("https://ujjivan.gogetempowered.com", returnurl);
                var url = request.GetRedirectUrl(samlEndpoint);

                return Redirect(url);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }*/
        [AllowAnonymous]
        [HttpGet("SAMLRequestGenerate/{Id?}/{orgcode?}")]
        public IActionResult SAMLRequestGenerate(string Id, string orgcode = null)
        {
            try
            {

                if (orgcode == "firstamerican")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://login.microsoftonline.com/4cc65fd6-9c76-4871-a542-eb12a5a7800c/saml2";
                    string returnUrl = "https://firstam.gogetempowered.com:10000/api/v1/saml/connect/O365/firstamerican";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("5ebe49fb-9dba-40a0-b1f7-0d17a8b7579f", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "khaitancouat")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://login.microsoftonline.com/4e30ff55-ee30-46c6-a827-03e4c3eac70c/saml2";
                    string returnUrl = "https://khaitancouat.gogetempowered.com:10000/api/v1/saml/connect/O365/khaitancouat";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("049f0b27-bb9a-44ee-a23c-cd1c6c438083", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "khaitanco")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://login.microsoftonline.com/4e30ff55-ee30-46c6-a827-03e4c3eac70c/saml2";
                    string returnUrl = "https://khaitanco.gogetempowered.com/api/v1/saml/connect/O365/khaitanco";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("149d424f-7e69-4386-a8ef-4e4c1554b374", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "icicis")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://login.microsoftonline.com/61275075-9376-485d-820e-102b887c823f/saml2";
                    string returnUrl = "https://mpower.lxp.enthral.ai/api/v1/saml/connect/O365/icicis";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://mpower.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "wnsuat")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://wns.app.instasafe.io/console/idpproxy/validate/idp/62cbf033b320b32b00a4cf7d";
                    string returnUrl = "https://wnsuat.gogetempowered.com:10000/api/v1/saml/connect/O365/wnsuat";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://wnsuat.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "wnsuatplus")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://edcast.oktapreview.com/app/edcast_empoweredcontentwnsglintplusuat_1/exk1eygj9j4WKYyWf0h8/sso/saml";
                    string returnUrl = "https://wnsuat.gogetempowered.com:10000/api/v1/saml/connect/O365/wnsuatplus";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://wnsuat.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);

                    debugLog(string.Format("returnurl:{0}", returnUrl));
                    debugLog(string.Format("relayid:{0}", Id));
                    return Redirect(url);
                }
                else if (orgcode == "wnsplus")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://edcast.okta.com/app/edcast_glintpluscontententhralllms_1/exkkobm1m3drp946I2p7/sso/saml";
                    string returnUrl = "https://wns.gogetempowered.com:10000/api/v1/saml/connect/O365/wnsplus";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://wns.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "wns")
                {
                    isDebugMode = true;
                    var samlEndpoint = "https://wns.app.instasafe.io/console/idpproxy/validate/idp/62e39462b6ef056125467ca4";
                    string returnUrl = "https://wns.gogetempowered.com:10000/api/v1/saml/connect/O365/wns";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://wns.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "singlife")
                {
                    var samlEndpoint = "https://login.microsoftonline.com/ff2a83c7-ec1d-4cc7-8bbe-6c529a23f41a/saml2";
                    string returnUrl = "https://singlife.gogetempowered.com/login:10000/api/v1/saml/connect/O365/singlife";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://singlife.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "page")
                {
                    var samlEndpoint = "https://login.microsoftonline.com/1fb9d3d1-9b9a-40a2-a8e9-d1a9cf5c0c8d/saml2";
                    string returnUrl = "https://turn2learn.pageind.com/api/v1/saml/connect/O365/page";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("cf860b86-c9c7-4a4d-bf04-9be5ebc3ee6a", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "darwin")
                {
                    var samlEndpoint = "https://login.microsoftonline.com/87270ae3-5581-4945-8537-a6f16728a776/saml2";
                    string returnUrl = "https://darwin.gogetempowered.com/api/v1/saml/connect/O365/darwin";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://darwin.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else if (orgcode == "thermax")
                {
                    var samlEndpoint = "https://login.microsoftonline.com/36229fec-607e-44a0-87c8-cd1fe9df82bb/saml2";
                    string returnUrl = "https://thermax.gogetempowered.com/api/v1/saml/connect/O365/thermax";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnUrl = returnUrl + "?Relay=" + Id;
                    }
                    var request = new Saml.AuthRequest("https://thermax.gogetempowered.com", returnUrl);
                    var url = request.GetRedirectUrl(samlEndpoint);
                    return Redirect(url);
                }
                else
                {
                    string samlEndpoint = "https://adfs.ujjivan.com/adfs/ls";
                    string returnurl = "https://gogetempowered.com:10000/api/v1/saml/connect/ADFS/ujjivan";
                    if (!string.IsNullOrEmpty(Id))
                    {
                        returnurl = returnurl + "?Relay=" + Id;
                    }
                    var request = new ADFSSAML.AuthRequest("https://ujjivan.gogetempowered.com", returnurl);
                    var url = request.GetRedirectUrl(samlEndpoint);

                    return Redirect(url);
                }



            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        private async Task<(string, string)> CacheTryGetValueSetGSuite(string relayState)
        {
            int index = relayState.LastIndexOf('/');
            string organizationCode = null;
            if (index != -1)
                organizationCode = relayState.Substring(index + 1);

            if (String.IsNullOrEmpty(organizationCode))
                return (null, null);

            string cacheEntryOrgnizationConnectionStringGSuite = null;
            string cacheEntryOrgnizationConnectionStringGSuiteOYOUAT = null;

            if (organizationCode.ToLower() == "ivp")
            {
                // Look for cache key.
                if (!_cacheOrgCode.TryGetValue(CacheKeys.EntryOrgnizationConnectionGSuite, out cacheEntryOrgnizationConnectionStringGSuite))
                {
                    // Key not in cache, so get data.
                    cacheEntryOrgnizationConnectionStringGSuite = await this.OrgnizationConnectionString(organizationCode);

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromDays(31));

                    // Save data in cache.
                    _cacheOrgCode.Set(CacheKeys.EntryOrgnizationConnectionGSuite, cacheEntryOrgnizationConnectionStringGSuite, cacheEntryOptions);

                }
                return (organizationCode, cacheEntryOrgnizationConnectionStringGSuite);

            }
            else
            {
                // Look for cache key.
                if (!_cacheOrgCode.TryGetValue(CacheKeys.EntryOrgnizationConnectionGSuiteOYOUAT, out cacheEntryOrgnizationConnectionStringGSuiteOYOUAT))
                {
                    // Key not in cache, so get data.
                    cacheEntryOrgnizationConnectionStringGSuiteOYOUAT = await this.OrgnizationConnectionString(organizationCode);

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromDays(31));

                    // Save data in cache.
                    _cacheOrgCode.Set(CacheKeys.EntryOrgnizationConnectionGSuiteOYOUAT, cacheEntryOrgnizationConnectionStringGSuiteOYOUAT, cacheEntryOptions);

                }
                return (organizationCode, cacheEntryOrgnizationConnectionStringGSuiteOYOUAT);
            }

        }

        private async Task<(string, string)> CacheTryGetValueSetO365(string relayState)
        {
            string organizationCode = relayState;
            debugLog("CacheTryGetValueSetO365-->" + relayState);
            if (relayState == "firstamerican")
            {
                organizationCode = "firstamerican";
            }
            else if (relayState == "khaitancouat")
            {
                organizationCode = "khaitancouat";
            }
            else if (relayState == "khaitanco")
            {
                organizationCode = "khaitanco";
            }
            else if (relayState == "icicis")
            {
                organizationCode = "icicis";
            }
            else if (relayState == "wnsuat")
            {
                organizationCode = "wnsuat";
            }
            else
            {
                int index = relayState.LastIndexOf('/');
                if (index != -1)
                    organizationCode = relayState.Substring(index + 1);
            }

            if (String.IsNullOrEmpty(organizationCode))
                return (null, null);

            string cacheEntryOrgnizationConnectionStringo365 = null;
            string cacheEntryOrgnizationConnectionStringGSuiteo365UAT = null;

            if (organizationCode.ToLower() == "nsdc")
            {
                // Look for cache key.
                if (!_cacheOrgCode.TryGetValue(CacheKeys.EntryOrgnizationConnectionO365, out cacheEntryOrgnizationConnectionStringo365))
                {
                    // Key not in cache, so get data.
                    cacheEntryOrgnizationConnectionStringo365 = await this.OrgnizationConnectionString(organizationCode);

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromDays(31));

                    // Save data in cache.
                    _cacheOrgCode.Set(CacheKeys.EntryOrgnizationConnectionO365, cacheEntryOrgnizationConnectionStringo365, cacheEntryOptions);

                }
                return (organizationCode, cacheEntryOrgnizationConnectionStringo365);

            }
            else
            {
                // Look for cache key.
                if (!_cacheOrgCode.TryGetValue(organizationCode, out cacheEntryOrgnizationConnectionStringGSuiteo365UAT))
                {
                    // Key not in cache, so get data.
                    cacheEntryOrgnizationConnectionStringGSuiteo365UAT = await this.OrgnizationConnectionString(organizationCode);

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromMinutes(15));

                    // Save data in cache.
                    //_cacheOrgCode.Set(CacheKeys.EntryOrgnizationConnectiono365UAT, cacheEntryOrgnizationConnectionStringGSuiteo365UAT, cacheEntryOptions);
                    _cacheOrgCode.Set(organizationCode, cacheEntryOrgnizationConnectionStringGSuiteo365UAT, cacheEntryOptions);

                }
                return (organizationCode, cacheEntryOrgnizationConnectionStringGSuiteo365UAT);
            }

        }

        private async Task<(string, string)> CacheTryGetValueSetADFS(string orgcode)
        {
            if (String.IsNullOrEmpty(orgcode))
                return (null, null);

            string Connstring = await this.OrgnizationConnectionString(orgcode);
            return (orgcode, Connstring);


        }



        [AllowAnonymous]
        [HttpPost("SSOSpectra")]
        public IActionResult SSOSpectra([FromForm] APISamlInformation apiSAMLInfo1)
        {
            try
            {
                string ssourl = this._configuration["BaseUrl"] + "/clientSSO/sso?accesstoken=";
                string accesstoken = string.Empty;
                var url = ssourl + apiSAMLInfo1.AccessToken;
                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [AllowAnonymous]
        [HttpGet("ssodarwinlogin/{data?}")]
        public async Task<IActionResult> SSODarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("darwin");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "harveyspecter.dbox@gmail.com"; //TODO: Make it configurable
                string secretKey = "d35888a0ffd85a9487a0ac35bfe23641";  //TODO: Make it configurable
   

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();//unixTimeStamp

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = "https://darwin.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);


                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssokotaklogin/{data?}")]
        public async Task<IActionResult> SSOkotakDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("kotak");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox.admin@kotak.com"; //TODO: Make it configurable
                string secretKey = "6392c3e9d63280a28405913411328237";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssozeemedialogin/{data?}")]
        public async Task<IActionResult> SSOZeeMediaDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("zeemedia");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "hr@zeemedia.esselgroup.com"; //TODO: Make it configurable
                string secretKey = "2b57070b9a02611b809b1de253d11f58";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);


                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssofirstmeridianlogin/{data?}")]
        public async Task<IActionResult> SSOFirstMeridianDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("firstmeridian");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

              //  string adminemail = "HRISAdmin@firstmeridian.com"; //TODO: Make it configurable
                string secretKey = "062653d2f4d3777d6c78ff811b8627d6";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssorockmanlogin/{data?}")]
        public async Task<IActionResult> SSORockmanDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("rockman");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "rockman@admin.darwinbox.io"; //TODO: Make it configurable
                string secretKey = "1ea272bc9d0528f31e386265a07ddf4b";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssorebitlogin/{data?}")]
        public async Task<IActionResult> SSOReBITDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("rebit");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "hr@rebit.org.in"; //TODO: Make it configurable
                string secretKey = "c02915a574356bffee5746c6573c2ea9";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssotataskylogin/{data?}")]
        public async Task<IActionResult> SSOTataskyLogin(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("tpf");

                byte[] cipherText = Convert.FromBase64String(data);
                byte[] encryption_key = Encoding.ASCII.GetBytes("92541d5aa5dcef4d137a109abb0586e8"); //Key form Zepal
                byte[] iv = Encoding.ASCII.GetBytes("84455dcea96a936b"); //Key from Zepal

                string phonenumber = DecryptStringFromBytes_Aes(cipherText, encryption_key, iv);

                this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);

                string emailId = await _userRepository.GetEmailByMobileNumber(phonenumber);

                if (!string.IsNullOrEmpty(emailId))
                {
                    emailId = Security.Decrypt(emailId);
                    emailId = Security.EncryptForUI(emailId);
                    string dateTimeUtcNow = null;

                    string email = System.Web.HttpUtility.UrlEncode(emailId);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "User phone number doesnot exists.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoanalytixlogin/{data?}")]
        public async Task<IActionResult> SSOAnalytixDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("analytix");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "sgmudaliar@analytix.com"; //TODO: Make it configurable
                string secretKey = "2c774c5a8b2011f4d4cfe906842ace84";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssopadminilogin/{data?}")]
        public async Task<IActionResult> SSOPadminiEnggDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("padmini");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "adminpconnect@darwinbox.in"; //TODO: Make it configurable
                string secretKey = "a33e71a0b1734baf0bbf9eb3bdb56193";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoakgrouplogin/{data?}")]
        public async Task<IActionResult> SSOAkGroupDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("akgroup");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox.admin@akgroup.co.in"; //TODO: Make it configurable
                string secretKey = "30f11c584e63b1f7b2b75c3640aec346";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssomaplogin/{data?}")]
        public async Task<IActionResult> SSOMapDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("map");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssocorporatelogin/{data?}")]
        public async Task<IActionResult> SSOCorporateDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("corporate");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoactivelogin/{data?}")]
        public async Task<IActionResult> SSOActiveDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("active");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssombalogin/{data?}")]
        public async Task<IActionResult> SSOMBADarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("mba");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssodominoslogin/{data?}")]
        public async Task<IActionResult> SSODominosDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("dominos");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoburgerkinglogin/{data?}")]
        public async Task<IActionResult> SSOBurgerKingDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("burgerking");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssosamsonitelogin/{data?}")]
        public async Task<IActionResult> SSOSamsoniteDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("samsonite");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssodigimaplogin/{data?}")]
        public async Task<IActionResult> SSODigimapDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("digimap");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssodepartmentstorelogin/{data?}")]
        public async Task<IActionResult> SSOPliDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("pli");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssosupermarketlogin/{data?}")]
        public async Task<IActionResult> SSOFoodhallDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("foodhall");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssofashionlogin/{data?}")]
        public async Task<IActionResult> SSOFashionDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("fashion");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoinditexlogin/{data?}")]
        public async Task<IActionResult> SSOInditexDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("inditex");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssostarbuckslogin/{data?}")]
        public async Task<IActionResult> SSOStarbucksDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("starbucks");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@map.co.id"; //TODO: Make it configurable
                string secretKey = "df0721e297148f3d5ffb7e0148dd8e51";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoquickheallogin/{data?}")]
        public async Task<IActionResult> SSOQuickHealDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("quickheal");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "dboxadqhtl@gmail.com"; //TODO: Make it configurable
                string secretKey = "44392fd2f4839d8363e7ec91d7d614e7";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoqapitollogin/{data?}")]
        public async Task<IActionResult> SSOQuapitolDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("qapitol");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "admin.darwinbox@qapitol.com"; //TODO: Make it configurable
                string secretKey = "40f33d3c2c9c96437557df438338036f";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoloconavlogin/{data?}")]
        public async Task<IActionResult> SSOLoconavDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("loconav");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox.admin@loconav.com"; //TODO: Make it configurable
                string secretKey = "b3c7f3b3012177c10a9d1c0cbda48efc";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoavivalogin/{data?}")]
        public async Task<IActionResult> SSOAvivaDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("singlife");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;


                string adminemail = "singlife@admin.darwinbox.io"; //TODO: Make it configurable
                string secretKey = "385be8f6f593ec2a7fd944c2017d12b9";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string GetEmail = await _userRepository.GetEmailByUserId((darwinBoxPayload.employee_no).ToLower());
                    GetEmail = Security.Decrypt(GetEmail);
                    string email = Security.EncryptForUI(GetEmail);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssocaplogin/{data?}")]
        public async Task<IActionResult> SSOCapDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("cap");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "admin.darwinbox@capcx.com"; //TODO: Make it configurable
                string secretKey = "7f7030374362ba7d01198770fa2d0b90";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoioaglogin/{data?}")]
        public async Task<IActionResult> SSOIOAGDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("ioag");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "admindbox@ioagpl.com"; //TODO: Make it configurable
                string secretKey = "a3a03ed8fc5990813a3ace8e33d50ccd";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoletstransportlogin/{data?}")]
        public async Task<IActionResult> SSOLetsTransportDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("letstransport");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@letstransport.team"; //TODO: Make it configurable
                string secretKey = "026d149ca0237811acd9d4990ef034b5";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoelgilogin/{data?}")]
        public async Task<IActionResult> SSOElgiDarwinBox(string data)
        {
            try
            {


                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("elgi");
                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinboxadmin@elgi.com"; //TODO: Make it configurable
                string secretKey = "c4782d843b74c43336bff3512c45f775";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssozinnovlogin/{data?}")]
        public async Task<IActionResult> SSOZinnovDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("zinnov");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@zinnov.com"; //TODO: Make it configurable
                string secretKey = "79e833a13216ee2220176ee61cbec90c";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoeverestlogin/{data?}")]
        public async Task<IActionResult> SSOEverestDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("everest");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

               // string adminemail = "darwinbox@zinnov.com"; //TODO: Make it configurable
                string secretKey = "c1e3378ffa8ef451031b6fb625f15dc8";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssomandologin/{data?}")]
        public async Task<IActionResult> SSOMandoDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("mando");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                // string adminemail = "darwinbox@zinnov.com"; //TODO: Make it configurable
                string secretKey = "dcbf28893113278c173bc3d58a918f40";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        private string XOREncryptDecrypt(string inputString, int encryptionKey)
        {
            StringBuilder inputStringBuild = new StringBuilder(inputString);
            StringBuilder outStringBuild = new StringBuilder(inputString.Length);

            string key = encryptionKey.ToString();
            char text;
            char outText;

            for (int i = 0; i < inputString.Length; i++)
            {
                for (int j = 0; j < key.Length; j++)
                {
                    text = inputString[i];
                    outText = (char)(text ^ Convert.ToInt32(key[j]));
                    outStringBuild.Append(outText);
                }
            }
            return outStringBuild.ToString();
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        [AllowAnonymous]
        [HttpGet("ssompokketlogin/{data?}")]
        public async Task<IActionResult> SSOMpokketLogin(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("mpokket");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "admindbox@mpokket.com"; //TODO: Make it configurable
                string secretKey = "1c4b7178aac1cced95d740061ace9caa";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();
                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoabplogin/{data?}")]
        public async Task<IActionResult> SSOAbpLogin(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("abp");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox.stage@abpnetwork.in"; //TODO: Make it configurable
                string secretKey = "147b7c14676e8f38596a8342d1d06fa7";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssothermaxlogin/{data?}")]
        public async Task<IActionResult> SSOThermaxLogin(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("thermax");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "hrmsadmin@thermaxglobal.com"; //TODO: Make it configurable
                string secretKey = "7415eb68d92b23287fd70f412f627ec2";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }


        }
        [AllowAnonymous]
        [HttpGet("ssodanamonlogin/{data?}")]
        public async Task<IActionResult> SSODanamonLogin(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("danamon");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "eazy.admin@danamon.co.id"; //TODO: Make it configurable
                string secretKey = "19182ce652b3b5073699d753a6fb5d82";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpGet("ssoshardamotorslogin/{data?}")]
        public async Task<IActionResult> SSOShardaMotorsDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("shardamotors");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "admin.dbox@shardamotor.com"; //TODO: Make it configurable
                string secretKey = "d9418c9b949e011ad40db36112bb694d";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpGet("ssohdfccredilalogin/{data?}")]
        public async Task<IActionResult> SSOCredilaDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("credila");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox.admin@hdfccredila.com"; //TODO: Make it configurable
                string secretKey = "68b1108f30cec455b93251e333dc6e50";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI(darwinBoxPayload.email);
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpGet("ssosanjeevgrouplogin/{data?}")]
        public async Task<IActionResult> SSOSanjeevGroupDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("sapmpl");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinboxadmin@sanjeevgroup.com"; //TODO: Make it configurable
                string secretKey = "66a7e0c3937408afb8890e7ce076cffb";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoswondercementlogin/{data?}")]
        public async Task<IActionResult> SSOWonderCementDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("wondercement");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "darwinbox@wondercement.com"; //TODO: Make it configurable
                string secretKey = "566586661b94c61e42903618e062cfd1";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                   // string loginURL = this._configuration["SsoLoginUrl"];

                    var url = "https://wondercement.gogetempowered.com/ivp-saml/SAML?EmailID=" + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssosbigenlogin/{data?}")]
        public async Task<IActionResult> SSOSbigDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("sbigen");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "hrnxt@sbigeneral.in"; //TODO: Make it configurable
                string secretKey = "f8457c1e8f41a66e1d9e578f59a9a7af";  //TODO: Make it configurable


                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssomenalogin/{data?}")]
        public async Task<IActionResult> SSOMenaDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("ent");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "farazjavedfateh@gmail.com"; //TODO: Make it configurable
                string secretKey = "952da879a5738f5790d70cd94240a831";  //TODO: Make it configurable

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssodouatlogin/{courseid}/{data?}")]
        public async Task<IActionResult> SSODbUatDarwinBox(int courseid, string data)
        {
            try
            {
               

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                string adminemail = "farazjavedfateh@gmail.com"; //TODO: Make it configurable
                string secretKey = "ffebaa24e2791540db4f613ef628112f";  //TODO: Make it configurable

                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("dbuat");

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];
                    if (courseid == 0)
                    {
                        var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                        return Redirect(url);
                    }
                    else
                    {
                        var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow + "/" + courseid;
                        return Redirect(url);
                    }
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }


        }
        [AllowAnonymous]
        [HttpGet("ssoaillogin/{data?}")]
        public async Task<IActionResult> SSOAILDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("ail");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

               // string adminemail = "farazjavedfateh@gmail.com"; //TODO: Make it configurable
                string secretKey = "ce495da8a35fa2c656d09218bd847086";  //TODO: Make it configurable

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpGet("ssoevalueservelogin/{data?}")]
        public async Task<IActionResult> SSOEvalueserveDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("evalueserve");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                // string adminemail = "farazjavedfateh@gmail.com"; //TODO: Make it configurable
                string secretKey = "f748e99f8bd2010fa14ac5bec02b857e";  //TODO: Make it configurable

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ssovyomlabslogin/{data?}")]
        public async Task<IActionResult> SSOVyomLabsDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("vyomlabs");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                // string adminemail = "farazjavedfateh@gmail.com"; //TODO: Make it configurable
                string secretKey = "46de435687e8c6ae82f68531002ae5a6";  //TODO: Make it configurable

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssoinsurancedekhologin/{data?}")]
        public async Task<IActionResult> SSOiDekhoDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("insurancedekho");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                // string adminemail = "farazjavedfateh@gmail.com"; //TODO: Make it configurable
                string secretKey = "bfcccff063415b1bf350a1559c59c4f7";  //TODO: Make it configurable

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssodbiplogin/{data?}")]
        public async Task<IActionResult> SSODbipDarwinBox(string data)
        {
            try
            {
                var OrgnizationConnectionString = await CacheTryGetValueSetADFS("dbip");

                byte[] dataIteration1 = Convert.FromBase64String(data);
                string decodedStringI1 = Encoding.UTF8.GetString(dataIteration1);

                int iXORKey = 6; // 666666 //TODO: Make it configurable
                string dataXORDecrypt = string.Empty;
                dataXORDecrypt = XOREncryptDecrypt(decodedStringI1, iXORKey);

                byte[] dataIteration2 = Convert.FromBase64String(dataXORDecrypt);
                string decodedStringI2 = Encoding.UTF8.GetString(dataIteration2);

                DarwinBoxPayload darwinBoxPayload = new DarwinBoxPayload();
                darwinBoxPayload = JsonConvert.DeserializeObject<DarwinBoxPayload>(decodedStringI2);

                string inputHashValue = darwinBoxPayload.hash;

                // string adminemail = "farazjavedfateh@gmail.com"; //TODO: Make it configurable
                string secretKey = "3116c8aace030f7c9baf95ee725c520b";  //TODO: Make it configurable

                string verificationHashValue = EncryptProvider.Sha512(darwinBoxPayload.email + secretKey + darwinBoxPayload.timestamp).ToLower();

                if (inputHashValue == verificationHashValue)
                {
                    this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                    string email = Security.EncryptForUI((darwinBoxPayload.email).ToLower());
                    string dateTimeUtcNow = null;

                    email = System.Web.HttpUtility.UrlEncode(email);
                    dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                    string loginURL = this._configuration["SsoLoginUrl"];

                    var url = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                    return Redirect(url);
                }
                else
                {
                    return StatusCode(401, "Invalid Credentials! Please retry.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("ssotvslogin/{sessiontoken?}")]
        public async Task<IActionResult> SSOTvsLogin(string sessiontoken)
        {
            try
            {
                string url = "https://tvscsbandhan.workline.hr/api/WL/WS_GETSSOToken";
                string body = "";
                List<UserLoginWorkline> UserloginWorking = new List<UserLoginWorkline>();
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                      {"APPID", "TVSCSSSO"},
                      {"SID", sessiontoken}
                    };
                    var content = new FormUrlEncodedContent(values);
                    string authorization = "TVSCS:TvScS@123";
                    string base64String = EncodeTo64(authorization);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64String);
                    client.DefaultRequestHeaders
                          .Accept
                          .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("APPID", "TVSCSSSO");
                    client.DefaultRequestHeaders.Add("SID", sessiontoken);
                    string apiUrl = url;
                    var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        UserloginWorking = JsonConvert.DeserializeObject<List<UserLoginWorkline>>(result);
                        string TvSemail = "";
                        foreach (UserLoginWorkline userLoginWorkline in UserloginWorking)
                        {
                            TvSemail = userLoginWorkline.emaiL_ID;
                            var OrgnizationConnectionString = await CacheTryGetValueSetADFS("tvs");
                            this._userRepository.ChangeDbContext(OrgnizationConnectionString.Item2);
                            string email = Security.EncryptForUI((TvSemail).ToLower());
                            string dateTimeUtcNow = null;

                            email = System.Web.HttpUtility.UrlEncode(email);
                            dateTimeUtcNow = System.Web.HttpUtility.UrlEncode(Security.EncryptForUI(DateTime.UtcNow.AddMinutes(2).ToString()));

                            string loginURL = this._configuration["SsoLoginUrl"];

                            var LoginURL = loginURL + email + "/" + OrgnizationConnectionString.Item1 + "/" + dateTimeUtcNow;
                            return Redirect(LoginURL);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        static public string EncodeTo64(string toEncode)
        {

            byte[] toEncodeAsBytes

                  = Encoding.ASCII.GetBytes(toEncode);

            string returnValue

                  = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;

        }
    }
}