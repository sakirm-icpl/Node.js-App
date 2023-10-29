using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Payment.API.APIModel;
using Payment.API.Controllers;
using Payment.API.Helper;
using Payment.API.Helper.Log_API_Count;
using Payment.API.Models;
using Payment.API.Repositories.Interfaces;
using Payment.API.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Payment.API.Helper.EnumHelper;

namespace Payment.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    public class PaymentController : IdentityController
    {
        private ICustomerConnectionStringRepository _customerConnectionString;
        private IConfiguration _configuration;
        private IUserSignUpRepository _userSignUpRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSignUpController));
        public PaymentController(ICustomerConnectionStringRepository customerConnectionString,
                IConfiguration configuration, IUserSignUpRepository userSignUpRepository,
                IIdentityService identityService) : base(identityService)
        {
            _customerConnectionString = customerConnectionString;
            _configuration = configuration;
            _userSignUpRepository = userSignUpRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string q, string c, string u = null)
        {
            try
            {
                if (string.IsNullOrEmpty(q))
                    new ContentResult() { Content = JsonConvert.SerializeObject(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) }), ContentType = "application/json", StatusCode = 400 };

                string RequestIds = Security.Decrypt(q);
                string OrgCode = Convert.ToString(_configuration["IAAOrgCode"]).ToLower();

                string UserIds = Security.Decrypt(u);

                string ConnectionString = await this._customerConnectionString.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("error")), HttpUtility.UrlEncode(Security.EncryptForUI("Unexpected error occured. Please try later"))));

                int RequestId;
                _logger.Debug("RequestIds - " + RequestIds);
                if (!int.TryParse(RequestIds, out RequestId))
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("invalid")), HttpUtility.UrlEncode(Security.EncryptForUI(EnumHelper.GetEnumDescription(MessageType.InvalidRequest)))));

                _logger.Debug("RequestId - " + RequestId);
                int response = await _userSignUpRepository.CheckRequest(RequestId, UserIds, ConnectionString);
                if (response == 0)
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("invalid")), HttpUtility.UrlEncode(Security.EncryptForUI(EnumHelper.GetEnumDescription(MessageType.InvalidRequest)))));
                else if (response == -1)
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("duplicate")), HttpUtility.UrlEncode(Security.EncryptForUI(EnumHelper.GetEnumDescription(MessageType.RequestPaymentDone)))));

                var result = await _userSignUpRepository.MakePayment(RequestId, UserIds, ConnectionString, OrgCode);

                return Redirect(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("error")), HttpUtility.UrlEncode(Security.EncryptForUI("Unexpected error occured. Please try later"))));
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post()
        {
            try
            {
                string Response = HttpContext.Request.Form["respData"].ToString();
                string merchantId = HttpContext.Request.Form["merchantId"].ToString();
                string OrgCode = Convert.ToString(_configuration["IAAOrgCode"]).ToLower();
                string ConnectionString = await this._customerConnectionString.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("error")), HttpUtility.UrlEncode(Security.EncryptForUI("Unexpected error occured. Please try later"))));

                var result = await _userSignUpRepository.ProcessResponse(Response, ConnectionString, OrgCode);
                _logger.Debug("Payment response - " + result);
                if (result.Message == "Transaction Failed")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("failed"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("failed")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else if (result.Message == "Transaction Timeout")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("timeout"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("timeout")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else if (result.Message == "Customer Cancelled")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("cancelled"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("cancelled")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else if (result.Message == "Transaction Successful")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("success"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("success")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("failed"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("failed")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
            }
            catch (Exception ex)
            {
                _logger.Debug("Exception Occured.");
                _logger.Error(Utilities.GetDetailedException(ex));
                return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("error")), HttpUtility.UrlEncode(Security.EncryptForUI("Unexpected error occured. Please try later"))));
            }
        }
        [AllowAnonymous]
        [HttpPost("GetStatus")]
        public async Task<IActionResult> GetStatus([FromBody] PaymentStatusRequest paymentStatusRequest)
        {
            try
            {
                string OrgCode = Convert.ToString(_configuration["IAAOrgCode"]).ToLower();
                string ConnectionString = await this._customerConnectionString.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                var result = await _userSignUpRepository.GetStatus(paymentStatusRequest, ConnectionString);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetPaymentResponse")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentResponse()
        {
            try
            { // invoice_id // OrgCode
                string Response = HttpContext.Request.Form["respData"].ToString();
                string merchantId = HttpContext.Request.Form["merchantId"].ToString();
                string OrgCode = Convert.ToString(_configuration["IAAOrgCode"]).ToLower();
                string ConnectionString = await this._customerConnectionString.GetConnectionStringByOrgnizationCode(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("error")), HttpUtility.UrlEncode(Security.EncryptForUI("Unexpected error occured. Please try later"))));

                var result = await _userSignUpRepository.ProcessResponse(Response, ConnectionString, OrgCode);
                _logger.Debug("Payment response - " + result);
                if (result.Message == "Transaction Failed")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("failed"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("failed")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else if (result.Message == "Transaction Timeout")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("timeout"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("timeout")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else if (result.Message == "Customer Cancelled")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("cancelled"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("cancelled")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else if (result.Message == "Transaction Successful")
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("success"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("success")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
                else
                {
                    _logger.Debug(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode("failed"), HttpUtility.UrlEncode(result.Description), HttpUtility.UrlEncode(result.TransactionId)));
                    return Redirect(string.Format("{0}{1}?s={2}&m={3}&id={4}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("failed")), HttpUtility.UrlEncode(Security.EncryptForUI(result.Description)), HttpUtility.UrlEncode(Security.EncryptForUI(result.TransactionId))));
                }
            }
            catch (Exception ex)
            {
                _logger.Debug("Exception Occured.");
                _logger.Error(Utilities.GetDetailedException(ex));
                return Redirect(string.Format("{0}{1}?s={2}&m={3}", _configuration["ReturnUrl"], "PaymentStatus", HttpUtility.UrlEncode(Security.EncryptForUI("error")), HttpUtility.UrlEncode(Security.EncryptForUI("Unexpected error occured. Please try later"))));
            }
        }
    }
}
