using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AspNet.Security.OAuth.Introspection;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using User.API.APIModel;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Helper.EnumHelper;
using static User.API.Models.UserMaster;
using log4net;
using static User.API.Common.AuthorizePermissions;
using User.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using User.API.Helper.Log_API_Count;
using System.Collections.Specialized;

namespace User.API.Controllers
{
   // [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    public class UserSignUpController : IdentityController
    {   
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSignUpController));
        private ISignUpRepository _signUpRepository;
        private IUserRepository _userRepository;
        private IConfiguration _configuration;
        private string url;
        private string supportMail;
        private string aPPurl;
        private IEmail _email;
        private string reagardName;
        private readonly IIdentityService _identitySvc;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private IUserSignUpRepository _userSignUpRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IAzureStorage _azurestorage;

        public UserSignUpController(IUserSignUpRepository userSignUpRepository, ISignUpRepository signUpRepository, ICustomerConnectionStringRepository customerConnectionStringRepository, IUserRepository userRepository, IIdentityService identityService, IConfiguration configuration, IEmail email,
         IAzureStorage azurestorage,
           IHttpContextAccessor httpContextAccessor) :base(identityService)
        {
            this._signUpRepository = signUpRepository;
            this._userRepository = userRepository;
            this._configuration = configuration;
            this._identitySvc = identityService;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            this._userSignUpRepository = userSignUpRepository;
            this._email = email;
            this._httpContextAccessor = httpContextAccessor;
            this._azurestorage = azurestorage;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] APIUserSignUp apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    Exists exist = apiUser.ValidationsForUserSignUp(this._userSignUpRepository, apiUser);
                    if (!exist.Equals(Exists.No))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(exist) });
                    else
                    {
                        string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiUser.OrganizationCode);
                        if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                        {
                            return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                        }

                        if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                        {
                            return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                        }
                        apiUser.Password = apiUser.Password;
                        APIUserSignUp user = apiUser.MapUserSignupToAPIUser(apiUser);
                        user.CustomerCode = apiUser.OrganizationCode;
                        user.Id = 0;
                        user.IsActive = true;
                        int Result = await this._userRepository.AddUserSignUp(user, "EU", OrgnizationConnectionString);
                        if (Result == 0)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                        }
                  
                        return Ok();
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }

        [HttpPost("CheckUserCourseAccess")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserCourseAccess([FromBody] APIUserSignUp apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    Exists exist = apiUser.ValidationsForUserSignUp(this._userSignUpRepository, apiUser);
                    if (!exist.Equals(Exists.No))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(exist) });
                    else
                    {
                        string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiUser.OrganizationCode);
                        if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                        {
                            return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                        }

                        if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                        {
                            return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                        }
                        apiUser.Password = apiUser.Password;
                        APIUserSignUp user = apiUser.MapUserSignupToAPIUser(apiUser);
                        user.CustomerCode = apiUser.OrganizationCode;
                        user.Id = 0;
                        user.IsActive = true;
                        int Result = await this._userRepository.AddUserSignUp(user, "EU", OrgnizationConnectionString);
                        if (Result == 0)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                        }
     
                        return Ok();
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }


        [HttpPost("CheckUserSignUp")]
        [AllowAnonymous]
        public async Task<IActionResult> PostSignupcsl([FromBody] APIUserSignUpCsl apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {

    
                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiUser.CustomerCode);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                    }

                    if (await this._userRepository.IsEmployeeCodeExists(apiUser.UserId, OrgnizationConnectionString))
                        return this.BadRequest(new ResponseMessage { Message = "Employee code " + "'"  +apiUser.UserId +"'" + " already exists in the system", Description = EnumHelper.GetEnumName(MessageType.Duplicate) });


                    var bearertoken = await this.GetBearerToken();
                    if (!string.IsNullOrEmpty(bearertoken))
                    {
                        string Otp = RandomPassword.GenerateRandomPassword();
                        SignUpOTP userotp = new SignUpOTP();
                        userotp.EmpCode = apiUser.UserId;
                        userotp.OTP = Otp;
                        userotp.CreatedDate = DateTime.UtcNow;
                        int Result = await this._userRepository.AddUserSignUpOTP(userotp, OrgnizationConnectionString);


                        string otpurl = "https://apps.cochinshipyard.in:444/novexauth/services/userService/users/validateUser";
                        APIEmpDetailsForOTP validateuser = new APIEmpDetailsForOTP();
                        validateuser.action = "validateEmpCode";
                        validateuser.empCode = apiUser.UserId;

                        JObject oJsonObjectvalidate = new JObject();
                        oJsonObjectvalidate = JObject.Parse(JsonConvert.SerializeObject(validateuser));

                        HttpResponseMessage validateuserResponse = await Api.CallPostAPICsl(otpurl, oJsonObjectvalidate, bearertoken);

                        if (validateuserResponse.IsSuccessStatusCode)
                        {
                            var userresult = validateuserResponse.Content.ReadAsStringAsync().Result;
                            APIEmpDetailsForOTPResp validateuserresp = JsonConvert.DeserializeObject<APIEmpDetailsForOTPResp>(userresult);
                            if (validateuserresp.status != "SUCCESS")
                                return this.BadRequest(new ResponseMessage { Message = validateuserresp.message, Description = validateuserresp.status });


                            APIEmpDetailsForOTP otpdetails = new APIEmpDetailsForOTP();
                            otpdetails.action = "sendOTP";
                            otpdetails.empCode = apiUser.UserId;
                            otpdetails.otp = Otp;
                            JObject oJsonObjectotp = new JObject();
                            oJsonObjectotp = JObject.Parse(JsonConvert.SerializeObject(otpdetails));

                            HttpResponseMessage otpResponse = await Api.CallPostAPICsl(otpurl, oJsonObjectotp, bearertoken);

                            if (otpResponse.IsSuccessStatusCode)
                            {
                                var result = otpResponse.Content.ReadAsStringAsync().Result;
                                APIEmpDetailsForOTPResp otpresp = JsonConvert.DeserializeObject<APIEmpDetailsForOTPResp>(result);
                                if (otpresp.status == "SUCCESS")
                                {
                                    return Ok(new ResponseMessage { Message = otpresp.message, Description = otpresp.status });
                                }
                                return this.BadRequest(new ResponseMessage { Message = otpresp.message, Description = otpresp.status });
                            }

                        }

                    }

                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumName(MessageType.DataNotValid) });
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }
        private async Task<string> GetBearerToken()
        {
            string bearertoken = null;

            try
            {
                APICslTokenDetails getcsltokendetails = new APICslTokenDetails();
                getcsltokendetails.username = "csluser";
                getcsltokendetails.password = "b513d609cada6dcadc0c195e9db39779";
                JObject oJsonObject = new JObject();
                oJsonObject = JObject.Parse(JsonConvert.SerializeObject(getcsltokendetails));


                string url = "https://apps.cochinshipyard.in:444/novexauth/services/auth/authentication";

                HttpResponseMessage Response = await Api.CallAPI(url, oJsonObject);

                if (Response.IsSuccessStatusCode)
                {
                    bearertoken = Response.Content.ReadAsStringAsync().Result;
                    return bearertoken;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
            return bearertoken;
        }

        [AllowAnonymous]
        [HttpPost("UserExist")]
        public async Task<IActionResult> Exist([FromBody] UserExists apiUserExists)
        {
            try

            {
                if (ModelState.IsValid)
                {
                 string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiUserExists.OrganizationCode);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                    }

                    if (await this._userRepository.IsEmployeeCodeExists(apiUserExists.UserId, OrgnizationConnectionString))
                        return this.Ok(true);
                    return this.Ok(false);
                }
                return this.BadRequest(this.ModelState);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

            }
        }


        [HttpPost("UserRegister")]
        [AllowAnonymous]
        public async Task<IActionResult> UserRegister([FromBody] APIUserSignUp apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
  
                    Exists exist = apiUser.ValidationsForUserSignUp(this._userSignUpRepository, apiUser);
                    if (!exist.Equals(Exists.No))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(exist) });
                    else
                    {
                        string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiUser.OrganizationCode);
                        if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                        {
                            return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                        }

                        if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                        {
                            return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                        }
                        apiUser.loginId = apiUser.loginId;
                        apiUser.RandomUserPassword = RandomPassword.GenerateUserPassword(8, 1);
                         apiUser.Password = apiUser.RandomUserPassword;
                        APIUserSignUp user = apiUser.MapUserSignupToAPIUser(apiUser);
                        user.CustomerCode = apiUser.OrganizationCode;
                        user.Id = 0;
                        user.IsActive = true;
                        user.loginId = user.loginId;
                        user.employeeGroup = apiUser.employeeGroup;
                        user.retirementDate = apiUser.retirementDate;
                        int Result = await this._userRepository.AddUserSignUp(user, "EU", OrgnizationConnectionString);
                        if (Result == 0)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                        }
                        
                        return Ok(new ResponseMessageForUserRegister { UserId = apiUser.loginId, Password = apiUser.RandomUserPassword });

                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }


        [HttpPost("CheckOTPExists")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckOTPExists([FromBody] APIUserSignUpOTP aPIUserSignUp)
        {
            try
            {
      
                string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(aPIUserSignUp.CustomerCode);
                if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                {
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                }

                if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                {
                    return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                }
                SignUpOTP signUpOTP = new SignUpOTP();
                int result = await this._userSignUpRepository.ExistsOTP(aPIUserSignUp.UserId, aPIUserSignUp.OTP, aPIUserSignUp.CustomerCode, OrgnizationConnectionString);
                if (result==1)
                {
                   
                    var bearertoken = await this.GetBearerToken();

                    if (!string.IsNullOrEmpty(bearertoken))
                    {

                        string urlapprovalurl = "https://apps.cochinshipyard.in:444/novexauth/services/userService/users/userApproval";
                        APIEmpDetailsForOTP approvaluser = new APIEmpDetailsForOTP();
                        approvaluser.action = "userApproval";
                        approvaluser.empCode = aPIUserSignUp.UserId;

                        JObject oJsonObjectuser = new JObject();
                        oJsonObjectuser = JObject.Parse(JsonConvert.SerializeObject(approvaluser));

                        HttpResponseMessage UserApprovalResponse = await Api.CallPostAPICsl(urlapprovalurl, oJsonObjectuser, bearertoken);

                        if (UserApprovalResponse.IsSuccessStatusCode)
                        {
                            var userApprresult = UserApprovalResponse.Content.ReadAsStringAsync().Result;
                            APIEmpDetailsForOTPResp userApprresp = JsonConvert.DeserializeObject<APIEmpDetailsForOTPResp>(userApprresult);

                            if (userApprresp.status == "SUCCESS")
                            {
                                return Ok(new ResponseMessage { Message = userApprresp.message, Description = userApprresp.status });
                            }
                            return BadRequest(new ResponseMessage { Message = userApprresp.message, Description = userApprresp.status });

                        }
                    }
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumName(MessageType.InvalidData) });


                }
                else if(result==0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidOTP), Description = EnumHelper.GetEnumName(MessageType.InvalidOTP) });

                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.OTPExpired), Description = EnumHelper.GetEnumName(MessageType.OTPExpired) });
                }


            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }
        [HttpPost("PostVFSSignUp")]
        [AllowAnonymous]
        public async Task<IActionResult> PostVFSSignUp([FromBody] APIVFSUserSignUp apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    Exists exist = apiUser.ValidationsForUserSignUp(this._userSignUpRepository, apiUser);
                    if (!exist.Equals(Exists.No))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(exist) });
                    else
                    {
                        string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(apiUser.OrganizationCode);
                        if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                        {
                            return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                        }

                        if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                        {
                            return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                        }
                       var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");
     
                        var allowUserNameValidation = await _userRepository.GetMasterConfigurableParameterValueByConnectionString("ALLOW_SPECIALCHAR_USERNAME", OrgnizationConnectionString);
                        if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                        {
                            if (!UserNameRegExpression.IsMatch(apiUser.UserName))
                            {
                                return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                            }
                        }

               APIVFSUserSignUp user = apiUser.MapUserSignupToAPIUser(apiUser);
                        user.CustomerCode = apiUser.OrganizationCode;
                        user.Id = 0;
                        user.IsActive = true;
                        user.CreatedBy = 1;
                        user.ModifiedBy = 1;
                        user.UserType = "Internal";
                        user.UserId = apiUser.EmailId.Substring(0, 2) + Convert.ToString(RandomPassword.GenerateRandomPassword());
                        user.UserId = Security.Encrypt(user.UserId);
                        int Result = await this._userRepository.AddVFSUserSignUp
                            (user, user.UserRole, OrgnizationConnectionString);
                        if (Result == 0)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                        }

                        return Ok(apiUser);
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }

        [HttpPost("GetVFSTypeAhead")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVFSTypeAhead([FromBody] APISignUpTypeAhead aPISignUpTypeAhead)
        {
            try
            {
                string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(aPISignUpTypeAhead.OrgCode);
                if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                {
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                }

                if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                {
                    return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                }

                if (aPISignUpTypeAhead.Search != null)
                    aPISignUpTypeAhead.Search = aPISignUpTypeAhead.Search.ToLower().Equals("null") ? null : aPISignUpTypeAhead.Search;

                return this.Ok(await this._userSignUpRepository.GetVFSSignUp(aPISignUpTypeAhead, OrgnizationConnectionString));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("GetUserConfigurationSetting")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserConfigurationSetting([FromBody] APIVFSSetting aPIVFSSetting)
        {
            try
            {
                string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(aPIVFSSetting.OrgCode);
                if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                {
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                }

                if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                {
                    return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                }

                var userSettings = await this._userSignUpRepository.GetVFSSettings(OrgnizationConnectionString);
                return this.Ok(userSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SendOTPToUser")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOTPToUser([FromBody] APIUserSignUp aPIVFSSignUp)
        {
            try
            {


                string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(aPIVFSSignUp.OrganizationCode);
                if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                {
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                }

                if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                {
                    return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                }
                aPIVFSSignUp.Otp = RandomPassword.GenerateRandomPassword();
                aPIVFSSignUp.EmailId = Security.Encrypt(aPIVFSSignUp.EmailId);
                APIUserSignUp aPIUserSignUp = await this._userRepository.GetUserIdByEmailId(aPIVFSSignUp.EmailId, OrgnizationConnectionString);
                if (aPIUserSignUp == null)
                {
                    aPIVFSSignUp.Otp = RandomPassword.GenerateRandomPassword();
                    aPIVFSSignUp.EmailId = Security.Decrypt(aPIVFSSignUp.EmailId);
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
               await _userSignUpRepository.AddUserSignUp(aPIVFSSignUp, OrgnizationConnectionString);
                await _signUpRepository.SendOtpEmailToUser(aPIVFSSignUp.EmailId, aPIVFSSignUp.OrganizationCode, aPIVFSSignUp.Otp);
                return Ok();


            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("VFSUserOTPExist")]
        public async Task<IActionResult> VFSUserOTPExist([FromBody] APIVFSSignUpOTP aPIVFSSignUpOTP)
        {
            try

            {
                if (ModelState.IsValid)
                {

                    string OrgnizationConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(aPIVFSSignUpOTP.organizationCode);
                    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                    {
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    }

                    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    {
                        return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                    }

                    if (await this._userSignUpRepository.ExistsOTPForVFS(aPIVFSSignUpOTP.EmailId, aPIVFSSignUpOTP.OTP, aPIVFSSignUpOTP.organizationCode, OrgnizationConnectionString))

                    {
                        await this._userSignUpRepository.getdataforVFS(aPIVFSSignUpOTP.EmailId, aPIVFSSignUpOTP.OTP, aPIVFSSignUpOTP.organizationCode, OrgnizationConnectionString);
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidOTP), Description = EnumHelper.GetEnumName(MessageType.InvalidOTP) });
                    }

                }

                return this.BadRequest(this.ModelState);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

            }
        }
        [AllowAnonymous]
        [HttpPost("GetAirports")]
        public async Task<IActionResult> GetAirports([FromBody] APINodalUserOrgCode aPINodalUserOrgCode)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPINodalUserOrgCode.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                List<APIAirportInfo> airports = await _userSignUpRepository.GetAirports(ConnectionString);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("NodalUserSignUp")]
        public async Task<IActionResult> NodalUserSignUp([FromForm] APINodalUserSignUp aPINodalUserSignUp)
        {
            try
            {
                if (aPINodalUserSignUp.ConfigurationColumn7Id == null)
                {
                    ModelState["ConfigurationColumn7Id"].Errors.Clear();
                    ModelState["ConfigurationColumn7Id"].ValidationState = ModelValidationState.Valid;
                }
                if (aPINodalUserSignUp.ConfigurationColumn10Id == null)
                {
                    ModelState["ConfigurationColumn10Id"].Errors.Clear();
                    ModelState["ConfigurationColumn10Id"].ValidationState = ModelValidationState.Valid;
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPINodalUserSignUp.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                if (aPINodalUserSignUp.EmailId == "" || aPINodalUserSignUp.MobileNumber == "")
                    return this.BadRequest(new ResponseMessage { Message = "Email/Mobile No is required", Description = "Please check Email/Mobile. It is required field" });
                var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");

                var allowUserNameValidation = await _userSignUpRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode, ConnectionString);
                if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                {
                    if (!UserNameRegExpression.IsMatch(aPINodalUserSignUp.UserName))
                        return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                }

                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    IFormFile uploadedFile = request.Form.Files.First();
                    if (uploadedFile.Length < 0 || uploadedFile == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });

                    if (FileValidation.IsValidPdf(uploadedFile) || FileValidation.IsValidImage(uploadedFile))
                    {
                        string filename = uploadedFile.FileName;
                        string fileDir = this._configuration["ApiGatewayWwwroot"];
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        fileDir = Path.Combine(fileDir, OrgCode, Record.AadharImages);
                        if (!Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        string filePrefix = DateTime.Now.Ticks.ToString();
                        string file = Path.Combine(fileDir, filePrefix + filename);
                        string filePath = string.Concat(DomainName, OrgCode, "/", Record.AadharImages, "/", filePrefix + filename);
                        using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(fs);
                            aPINodalUserSignUp.AadhaarPath = filePath;
                        }
                        if (String.IsNullOrEmpty(file))
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                    }
                    else
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                }
                var Result = await _userSignUpRepository.NodalUserSignUp(aPINodalUserSignUp, ConnectionString);
                if (Result == -1)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAccount), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAccount) });
                else if (Result == -5)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateEmailId), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                else if (Result == -6)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateEmailId) });
                else if (Result == -7)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.BirthdateNotValid), Description = EnumHelper.GetEnumDescription(MessageType.BirthdateNotValid) });
                else if (Result == -2)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });
                else if (Result == -8)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAadharNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAadharNumber) });
                else if (Result == -9)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.ApprovedRequestsExists), Description = EnumHelper.GetEnumDescription(MessageType.ApprovedRequestsExists) });
                else if (Result == -10)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.PendingRequestsExists), Description = EnumHelper.GetEnumDescription(MessageType.PendingRequestsExists) });

                return Ok(aPINodalUserSignUp);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("CreateNodalUser")]
        [PermissionRequired(Permissions.course_registrations)]
        public async Task<IActionResult> CreateNodalUser([FromForm] APINodalUserSignUp aPINodalUserSignUp)
        {
            try
            {
                if (aPINodalUserSignUp.ConfigurationColumn7Id == null)
                {
                    ModelState["ConfigurationColumn7Id"].Errors.Clear();
                    ModelState["ConfigurationColumn7Id"].ValidationState = ModelValidationState.Valid;
                }
                if (aPINodalUserSignUp.ConfigurationColumn10Id == null)
                {
                    ModelState["ConfigurationColumn10Id"].Errors.Clear();
                    ModelState["ConfigurationColumn10Id"].ValidationState = ModelValidationState.Valid;
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (aPINodalUserSignUp.EmailId == "" || aPINodalUserSignUp.MobileNumber == "")
                    return this.BadRequest(new ResponseMessage { Message = "Email/Mobile No is required", Description = "Please check Email/Mobile. It is required field" });
                var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");

                var allowUserNameValidation = await _userSignUpRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode, ConnectionString);
                if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                {
                    if (!UserNameRegExpression.IsMatch(aPINodalUserSignUp.UserName))
                        return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                }
                aPINodalUserSignUp.OrgCode = Security.EncryptForUI(OrgCode);

                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    IFormFile uploadedFile = request.Form.Files.First();
                    if (uploadedFile.Length < 0 || uploadedFile == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });

                    var EnableBlobStorage = await _userRepository.GetConfigurationValueAsync("Enable_BlobStorage",OrgCode);
                    if (FileValidation.IsValidPdf(uploadedFile) || FileValidation.IsValidImage(uploadedFile))
                    {
                        string fileName = uploadedFile.FileName;
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        string filePrefix = DateTime.Now.Ticks.ToString();
                        string filePath = string.Concat(DomainName, OrgCode, "/", Record.AadharImages, "/", filePrefix + fileName);
                        if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                        {
                            
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
      
                            fileDir = Path.Combine(fileDir, OrgCode, Record.AadharImages);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                          
                            string file = Path.Combine(fileDir, filePrefix + fileName);
                           
                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await uploadedFile.CopyToAsync(fs);
                                aPINodalUserSignUp.AadhaarPath = filePath;
                            }
                            if (String.IsNullOrEmpty(file))
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                        }
                        else
                        {
                            try
                            {
                                BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, OrgCode, Record.AadharImages);
                                if (res != null)
                                {
                                    if (res.Error == false)
                                    {
                                        filePath = res.Blob.Name.ToString();
                                        filePath = filePath.Replace(@"\", "/");
                                        aPINodalUserSignUp.AadhaarPath = filePath;
                                    }
                                    else
                                    {
                                        _logger.Error(res.ToString());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(Utilities.GetDetailedException(ex));
                            }
                        }
                     }
                    else
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                }

                var Result = await _userSignUpRepository.CreateNodalUser(UserId, aPINodalUserSignUp, ConnectionString);
                if (Result == -1)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.AccountExists), Description = EnumHelper.GetEnumDescription(MessageType.AccountExists) });
                else if (Result == -5)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateEmailId), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                else if (Result == -6)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateEmailId) });
                else if (Result == -2)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });
                else if (Result == -7)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.BirthdateNotValid), Description = EnumHelper.GetEnumDescription(MessageType.BirthdateNotValid) });
                else if (Result == -8)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAadharNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAadharNumber) });
                else if (Result == -3)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidInfo), Description = EnumHelper.GetEnumDescription(MessageType.InvalidInfo) });
                else if (Result == -9)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.AlreadyRegistered), Description = EnumHelper.GetEnumDescription(MessageType.AlreadyRegistered) });
                else if (Result == -10)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.AlreadyRequested), Description = EnumHelper.GetEnumDescription(MessageType.AlreadyRequested) });

                return Ok(aPINodalUserSignUp);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("GetOrganizationDetailsTypeahead")]
        public async Task<IActionResult> GetOrganizationDetailsTypeahead([FromBody] APINodalUserTypeAhead aPINodalUserTypeAhead)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPINodalUserTypeAhead.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                
                ApiResponse result = new ApiResponse();
                result = await _userSignUpRepository.GetOrganizationDetailsTypeahead(aPINodalUserTypeAhead, ConnectionString);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("GetOrganizationIDTypeahead")]
        public async Task<IActionResult> GetOrganizationIDTypeahead([FromBody] APINodalUserTypeAhead aPINodalUserTypeAhead)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPINodalUserTypeAhead.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                ApiResponse result = new ApiResponse();
                result = await _userSignUpRepository.GetOrganizationIDTypeahead(aPINodalUserTypeAhead, ConnectionString);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("GetBillingAddressTypeahead")]
        public async Task<IActionResult> GetBillingAddressTypeahead([FromBody] APINodalUserTypeAhead aPINodalUserTypeAhead)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPINodalUserTypeAhead.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                ApiResponse result = new ApiResponse();
                result = await _userSignUpRepository.GetBillingAddressTypeahead(aPINodalUserTypeAhead, ConnectionString);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("GroupAdminSignUp")]
        public async Task<IActionResult> GroupAdminSignUp([FromBody] APIGroupAdminSignUp aPIGroupAdminSignUp)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPIGroupAdminSignUp.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                if (aPIGroupAdminSignUp.EmailId == "" || aPIGroupAdminSignUp.MobileNumber == "")
                    return this.BadRequest(new ResponseMessage { Message = "Email/Mobile No is required", Description = "Please check Email/Mobile. It is required field" });
                var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");

                var allowUserNameValidation = await _userSignUpRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode, ConnectionString);
                if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                {
                    if (!UserNameRegExpression.IsMatch(aPIGroupAdminSignUp.UserName))
                        return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                }

                var Result = await _userSignUpRepository.GroupAdminSignUp(aPIGroupAdminSignUp, ConnectionString);
                if (Result == -1)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAccount), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAccount) });
                else if (Result == -5)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateEmailId), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                else if (Result == -6)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateEmailId) });
                else if (Result == -7)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.BirthdateNotValid), Description = EnumHelper.GetEnumDescription(MessageType.BirthdateNotValid) });
                else if (Result == -2)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });
                else if (Result == -8)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAadharNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAadharNumber) });

                return Ok(aPIGroupAdminSignUp);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("DhangyanSignUp")]
        public async Task<IActionResult> DhangyanSignUp([FromBody] APIDhangyanUserSignUp aPIDhangyanUserSignUp)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPIDhangyanUserSignUp.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                
                var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");

                var allowUserNameValidation = await _userSignUpRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode, ConnectionString);
                if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                {
                    string UserName = !string.IsNullOrEmpty(aPIDhangyanUserSignUp.MiddleName) ? aPIDhangyanUserSignUp.FirstName + " " + aPIDhangyanUserSignUp.MiddleName + " " + aPIDhangyanUserSignUp.LastName : aPIDhangyanUserSignUp.FirstName + " " + aPIDhangyanUserSignUp.LastName;
                    if (!UserNameRegExpression.IsMatch(UserName))
                        return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                }

                var Result = await _userSignUpRepository.DhangyanSignUp(aPIDhangyanUserSignUp, ConnectionString);
                if (Result.Response == -1)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAccount), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAccount) });
                else if (Result.Response == -3)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.FirstNameLength) });
                else if (Result.Response == -4)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.LastNameLength) });
                else if (Result.Response == -5)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateEmailId), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                else if (Result.Response == -6)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateEmailId) });
                else if (Result.Response == -7)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.BirthdateNotValid), Description = EnumHelper.GetEnumDescription(MessageType.BirthdateNotValid) });
                else if (Result.Response == -2)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });

                return Ok(Result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpPost("DhangyanSchoolSignUp")]
        public async Task<IActionResult> DhangyanSchoolSignUp([FromBody] APISchoolDhangyanSignUp aPIDhangyanUserSignUp)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                //string OrgCode = "ent";
                //aPIDhangyanUserSignUp.OrgCode = Security.EncryptForUI("ent"); ;
               
                string OrgCode = Security.DecryptForUI(aPIDhangyanUserSignUp.OrgCode);

                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");

                var allowUserNameValidation = await _userSignUpRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode, ConnectionString);
                if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                {
                    string UserName = !string.IsNullOrEmpty(aPIDhangyanUserSignUp.MiddleName) ? aPIDhangyanUserSignUp.FirstName + " " + aPIDhangyanUserSignUp.MiddleName + " " + aPIDhangyanUserSignUp.LastName : aPIDhangyanUserSignUp.FirstName + " " + aPIDhangyanUserSignUp.LastName;
                    if (!UserNameRegExpression.IsMatch(UserName))
                        return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                }

                var Result = await _userSignUpRepository.DhangyanSchoolSignUp(aPIDhangyanUserSignUp, ConnectionString);
                if (Result.Response == -1)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAccount), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAccount) });
                else if (Result.Response == -3)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.FirstNameLength) });
                else if (Result.Response == -4)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.LastNameLength) });
                else if (Result.Response == -5)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateEmailId), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                else if (Result.Response == -6)
                    return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateEmailId) });
                else if (Result.Response == -7)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.BirthdateNotValid), Description = EnumHelper.GetEnumDescription(MessageType.BirthdateNotValid) });
                else if (Result.Response == -2)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });

                return Ok(Result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpPost("GetStates")]
        public async Task<IActionResult> GetStates([FromBody] APINodalUserOrgCode aPINodalUserOrgCode)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPINodalUserOrgCode.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                List<APIAirportInfo> airports = await _userSignUpRepository.GetStates(ConnectionString);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("GetOrganizations")]
        public async Task<IActionResult> GetOrganizations([FromBody] APIDhangyanUserTypeAhead aPIDhangyanUserTypeAhead)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPIDhangyanUserTypeAhead.OrgCode);
                string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                List<APIAirportInfo> airports = await _userSignUpRepository.GetOrganizations(aPIDhangyanUserTypeAhead, ConnectionString);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostSignupTTGroup")]
        [AllowAnonymous]
        public async Task<IActionResult> PostSignupTTGroup([FromForm] APITTUserSignUp apiUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string OrgCode = Security.DecryptForUI(apiUser.OrgCode);
                    string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                    if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                        || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                        return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");
                    if (apiUser.EmailId == "" || apiUser.MobileNumber == "")
                        return this.BadRequest(new ResponseMessage { Message = "Email/Mobile No is required", Description = "Please check Email/Mobile. It is required field" });
                    var UserNameRegExpression = new Regex(@"^[a-zA-Z .-]+$");

                    //var allowUserNameValidation = await _userSignUpRepository.GetConfigurationValueAsync("ALLOW_SPECIALCHAR_USERNAME", OrgCode, ConnectionString);
                    //if (Convert.ToString(allowUserNameValidation).ToLower() != "yes")
                    //{
                    //    if (!UserNameRegExpression.IsMatch(apiUser.UserName))
                    //        return this.BadRequest(new ResponseMessage { Message = "Invalid Username", Description = "Invalid Username" });
                    //}

                    var request = _httpContextAccessor.HttpContext.Request;
                    if (request.Form.Files.Count > 0)
                    {
                        IFormFile uploadedFile = request.Form.Files.First();
                        if (uploadedFile.Length < 0 || uploadedFile == null)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });

                        if (FileValidation.IsValidPdf(uploadedFile) || FileValidation.IsValidImage(uploadedFile))
                        {
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrgCode);
                            fileDir = Path.Combine(fileDir, Record.AadharImages);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + uploadedFile.FileName);
                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await uploadedFile.CopyToAsync(fs);
                                apiUser.AadhaarPath = file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/");
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }

                            }
                        else
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                    var Result = await _userSignUpRepository.TTUserSignUp(apiUser, ConnectionString);
                    if (Result == -1)
                        return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAccount), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAccount) });
                    else if (Result == -5)
                        return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateEmailId), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateMobileNumber) });
                    else if (Result == -6)
                        return this.Conflict(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateMobileNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateEmailId) });
                    else if (Result == -7)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.BirthdateNotValid), Description = EnumHelper.GetEnumDescription(MessageType.BirthdateNotValid) });
                    else if (Result == -2)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.MobileNumberNotExists), Description = EnumHelper.GetEnumDescription(MessageType.MobileNumberNotExists) });
                    else if (Result == -8)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateAadharNumber), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateAadharNumber) });
                    else if (Result == -9)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.ApprovedRequestsExists), Description = EnumHelper.GetEnumDescription(MessageType.ApprovedRequestsExists) });
                    else if (Result == -10)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.PendingRequestsExists), Description = EnumHelper.GetEnumDescription(MessageType.PendingRequestsExists) });

                    return Ok(apiUser);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ex.Message });
            }
        }



        [HttpPost("TransactionCallback")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentResponse()
        {
            try
            {
                string encRespBody = string.Empty;
               
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    encRespBody = await reader.ReadToEndAsync();                 
                    
                }
                _logger.Info("encResp-" + encRespBody);
                string[] segments = encRespBody.Split('&');
                _logger.Info("segments[0]-" + segments[0]);
                _logger.Info("segments[1]-" + segments[1]);

                if (segments.Length == 0)
                {
                    return StatusCode(400, "please check encResp, getting blank.");
                }
                else
                {
                    NameValueCollection Params = new NameValueCollection();
                    foreach (string seg in segments)
                    {
                        string[] parts = seg.Split('=');
                        if (parts.Length > 0)
                        {
                            string Key = parts[0].Trim();
                            string Value = parts[1].Trim();
                            Params.Add(Key, Value);
                        }
                    }


                    TransactionRequest transactionResponse = _userSignUpRepository.DecryptTransactionRespone(Params["encResp"]);
                    _logger.Info("decrypted encResp-" + JsonConvert.SerializeObject(transactionResponse));

                    if (transactionResponse != null)
                    {
                        string OrgCode = Convert.ToString(transactionResponse.merchant_param5.ToLower());
                        // string OrgCode = "ttgroupglobal";
                        _logger.Info("OrgCode" + OrgCode);
                         transactionResponse.orderNo = Params["order_id"];
                        string ConnectionString = await this._customerConnectionStringRepository.GetConnectionStringByOrgnizationCode(OrgCode);
                        if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                            || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                            return StatusCode(400, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");


                        return Ok(await _userSignUpRepository.ProcessPaymentResponse(transactionResponse, ConnectionString, OrgCode));

                    }
                    else
                    {
                        return StatusCode(400, "transactionResponse value getting null, Please contact System Administrator !");

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug("Exception Occured.");
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

            }
        }




    }
}