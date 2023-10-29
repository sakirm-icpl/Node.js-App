
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using User.API.APIModel;
using log4net;
using User.API.Helper;
using User.API.Repositories.Interfaces;
using static User.API.Helper.UserMasterImport;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace User.API.Repositories
{
    public class EmailRepository : IEmail
    {
        private readonly IConfiguration _configuration;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailRepository));

        public EmailRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int SendEmail(string toEmail, string subject, string message, string orgCode, string customerCode = null)
        {
            JObject oJsonObject = new JObject
            {
                { "toEmail", toEmail },
                { "subject", subject },
                { "message", message },
                { "customerCode", customerCode },
                { "organizationCode", orgCode }
            };
            string Url = this._configuration[Configuration.NotificationApi];
            HttpResponseMessage response = Api.CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public int SendEmailtoUser1(string toEmail, string orgCode, string UserName, string MobileNumber, string UserId, string Password)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/NewUserSignUp";
            JObject oJsonObject = new JObject
            {
                { "EmailId", toEmail },
                { "OrgCode", orgCode },
                { "UserName", UserName },
                { "MobileNumber", MobileNumber },
                { "UserId", UserId },
                { "Password", Password }
            };


            HttpResponseMessage response = Api.CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        //public int NewUserAdded(string toEmail, string orgCode, string userName, string mobileNumber, string UserID, string password, string SendSMSToUser, int Id)
        //{
        //    string Url = _configuration[Configuration.NotificationApi];
        //    Url += "/NewUserAdded";
        //    JObject oJsonObject = new JObject
        //    {
        //        { "toEmail", toEmail },
        //        { "organizationCode", orgCode },
        //        { "userName", userName },
        //        { "UserID", UserID },
        //        { "mobileNumber", mobileNumber },
        //        { "Password" , password},
        //        { "Id" , Id}
        //    };
        //    HttpResponseMessage response = Api.CallAPI(Url, oJsonObject).Result;
        //    //try
        //    //{
        //    //    string urlSMS = "";
        //    //    System.Collections.Generic.List<APISendSMS> SMSList = new System.Collections.Generic.List<APISendSMS>();
        //    //    APISendSMS objSMS = new APISendSMS();
        //    //    if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
        //    //    {
        //    //        urlSMS = _configuration[Configuration.NotificationApi];
        //    //        urlSMS += "/UserActivationSMS";

        //    //        JObject oJsonObjectSMS = new JObject();

        //    //        oJsonObjectSMS.Add("UserName", userName);
        //    //        oJsonObjectSMS.Add("MobileNumber", mobileNumber);
        //    //        oJsonObjectSMS.Add("orgCode", orgCode);
        //    //        oJsonObjectSMS.Add("UserID", UserID);
        //    //        _logger.Debug("urlSMS : " + urlSMS.ToString());
        //    //        try
        //    //        {
        //    //            Task.Run(() => CallAPI(urlSMS, oJsonObjectSMS).Result);
        //    //        }
        //    //        catch (Exception ex)
        //    //        {
        //    //            _logger.Debug(ex.ToString());
        //    //            _logger.Error(Utilities.GetDetailedException(ex));
        //    //        }
        //    //    }
                
        //    //}

        //    //catch (Exception ex)
        //    //{
        //    //    _logger.Debug("Exception in NewUserAdded " + ex.ToString());
        //    //    _logger.Error(Utilities.GetDetailedException(ex));
        //    //    throw ex;
        //    //}

        //    return 1;
        //}
        private async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }
        }
        public int NewUserAdded_Import(string toEmail, string orgCode, string userName, string mobileNumber, string UserID, string DeafultPassword)
        {
            string url = _configuration[Configuration.NotificationApi];
            url += "/NewUserAdded";
            JObject oJsonObject = new JObject
            {
                { "ToEmail", toEmail },
                { "OrganizationCode", orgCode },
                { "UserId", UserID },
                { "UserName", userName },
                { "MobileNumber", mobileNumber },
                {"Password",DeafultPassword }
            };
            HttpResponseMessage responses = Api.CallAPI(url, oJsonObject).Result;
            return 1;
        }
        public int SendApplicableCoursesEmail(string toEmail, string orgCode, string userName, string mobileNumber, string UserID, int Id, bool IsActive)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/SendApplicableCoursesEmail";

            JObject oJsonObject = new JObject
            {
                { "toEmail", toEmail },
                { "organizationCode", orgCode },
                { "userName", userName },
                { "UserID", UserID },
                { "mobileNumber", mobileNumber },
                 { "Id", Id },
                  { "IsActive", IsActive }
            };
            HttpResponseMessage response = Api.CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public int UserSignUpMailManager(string orgCode, string userName, string UserId, string toEmail, string EmailId, string mobileNumber)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/UserSignUpMailToManager";
            JObject oJsonObject = new JObject
            {
                { "ToEmail", toEmail },
                { "organizationCode", orgCode },
                { "userName", userName },
                { "UserID", UserId },
                { "mobileNumber", mobileNumber },
                { "EmailId", EmailId }
            };

            HttpResponseMessage response = Api.CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public int UserSignUpMailToNodalOfficers(List<APINodalUserDetails> aPINodalUserDetailsList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/UserSignUpMailToNodalOfficers";
            string Body = JsonConvert.SerializeObject(aPINodalUserDetailsList);

            HttpResponseMessage response = Api.CallPostAPI(Url, Body).Result;
            return 1;
        }
        public int UserCreationMailByNodalOfficerToUser(List<APINodalUserDetails> aPINodalUserDetailsList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/UserCreationMailByNodalOfficerToUser";
            string Body = JsonConvert.SerializeObject(aPINodalUserDetailsList);

            HttpResponseMessage response = Api.CallPostAPI(Url, Body).Result;
            return 1;
        }
        public async Task GroupAdminSelfRegistrationMail(APINodalUser aPINodalUser)
        {
            APIGroupAdminDetails aPIGroupAdminDetails = new APIGroupAdminDetails()
            {
                UserMasterId = aPINodalUser.Id,
                UserId = Security.Decrypt(aPINodalUser.UserId),
                UserName = aPINodalUser.UserName,
                EmailId = aPINodalUser.EmailId,
                MobileNumber = aPINodalUser.MobileNumber,
                OrgCode = aPINodalUser.CustomerCode,
                Password = this._configuration["DeafultPassword"]
            };

            string Url = _configuration[Configuration.NotificationApi];
            Url += "/GroupAdminSelfRegistrationMail";
            string Body = JsonConvert.SerializeObject(aPIGroupAdminDetails);

            HttpResponseMessage response = Api.CallPostAPI(Url, Body).Result;
            
        }
        public int GroupRequestMailToNodalOfficers(APIGroupEmails aPIGroupEmails)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/GroupRequestMailToNodalOfficers";
            string Body = JsonConvert.SerializeObject(aPIGroupEmails);

            HttpResponseMessage response = Api.CallPostAPI(Url, Body).Result;
            return 1;
        }
        public int PaymentSuccessfulMailToUser(APIPaymentMailDetails aPIPaymentMailDetails)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/PaymentSuccessfulMailToUser";
            string Body = JsonConvert.SerializeObject(aPIPaymentMailDetails);

            HttpResponseMessage response = Api.CallPostAPI(Url, Body).Result;
            return 1;
        }
        public int SendOtpEmailToUser(string toEmail, string orgCode, string Otp)
        {
            try
            {
                string Url = _configuration[Configuration.NotificationApi];
                Url += "/NewUserSignUp";
                JObject oJsonObject = new JObject
            {
                { "EmailId", toEmail },
                { "UserName", toEmail },
                { "OrgCode", orgCode },
                { "Password", Otp },

            };


                HttpResponseMessage response = Api.CallAPI(Url, oJsonObject).Result;
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 1;
        }

        public int NewUserAddedForVFS(string toEmail, string orgCode, string userName, string mobileNumber, string UserID, string password, string SendSMSToUser, int Id)
        {

            string Url = _configuration[Configuration.NotificationApi];
            Url += "/NewUserAdded";
            JObject oJsonObject = new JObject
            {
                { "toEmail", toEmail },
                { "organizationCode", orgCode },
                { "userName", userName },
                { "UserID", UserID },
                { "mobileNumber", mobileNumber },
                { "Password" , password},
                { "Id" , Id}
            };
            HttpResponseMessage response = Api.CallAPI(Url, oJsonObject).Result;

            try
            {
                string urlSMS = "";
                System.Collections.Generic.List<APISendSMS> SMSList = new System.Collections.Generic.List<APISendSMS>();
                APISendSMS objSMS = new APISendSMS();
                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                {
                    urlSMS = _configuration[Configuration.NotificationApi];
                    urlSMS += "/UserActivationSMS";

                    JObject oJsonObjectSMS = new JObject();

                    oJsonObjectSMS.Add("UserName", userName);
                    oJsonObjectSMS.Add("MobileNumber", mobileNumber);
                    oJsonObjectSMS.Add("orgCode", orgCode);
                    oJsonObjectSMS.Add("UserID", UserID);
                    HttpResponseMessage responsesSMS = Api.CallAPI(urlSMS, oJsonObjectSMS).Result;
                }
               
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

            return 1;
        }
    }
}
