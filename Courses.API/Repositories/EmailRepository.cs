using Courses.API.APIModel;
using Courses.API.APIModel.ILT;
using Courses.API.Helper;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Courses.API.Repositories
{
    public class EmailRepository : IEmail
    {
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;
        private readonly INotification _notification;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailRepository));

        public EmailRepository(IConfiguration configuration, IIdentityService identityService, INotification notification, ICustomerConnectionStringRepository customerConnection)
        {
            this._customerConnection = customerConnection;
            _configuration = configuration;
            _identityService = identityService;
            this._notification = notification;
        }

        public int SendEmail(string toEmail, string subject, string message, string orgCode, string customerCode = null)
        {
            JObject oJsonObject = new JObject();
            oJsonObject.Add("toEmail", toEmail);
            oJsonObject.Add("subject", subject);
            oJsonObject.Add("message", message);
            oJsonObject.Add("customerCode", customerCode);
            oJsonObject.Add("organizationCode", orgCode);
            string Url = this._configuration[Configuration.NotificationApi];
            HttpResponseMessage response = ApiHelper.CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> SendCourseAddedNotification(int courseId, string title, string courseCode, string token)
        {
            ApiNotification Notification = new ApiNotification();
            Notification.Title = title;
            Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
            Notification.Message = Notification.Message.Replace("{course}", title);
            Notification.Url = TlsUrl.NotificationAPost + courseId;
            Notification.Type = Record.Course;
            await this._notification.SendNotification(Notification, token);
            return 1;
        }

        public async Task<int> SendILTNominationEmail(List<APIILTNominationEmail> objAPIILTNominationEmail)
        {
            List<JObject> objJObject = new List<JObject>();
            var SendMailToUser = await GetMasterConfigurableParameterValue("NOMINATE_COURSE_CC_MANAGER");
            foreach (APIILTNominationEmail obj in objAPIILTNominationEmail)
            {
                JObject oJsonObject = new JObject();
                oJsonObject.Add("LearnerName", obj.LearnerName);
                oJsonObject.Add("SupervisorName", obj.SupervisorName);
                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("VenueAddress", obj.VenueAddress);
                oJsonObject.Add("ScheduleCode", obj.ScheduleCode);
                if (string.IsNullOrEmpty(obj.toEmail))
                {
                    continue;
                }
                else
                {
                    oJsonObject.Add("toEmail", obj.toEmail);
                }

                if (string.IsNullOrEmpty(obj.orgCode))
                {
                    continue;
                }
                else
                {
                    oJsonObject.Add("orgCode", obj.orgCode);
                }
                
                oJsonObject.Add("URL", obj.URL);
                if(string.IsNullOrEmpty(SendMailToUser))
                {
                    oJsonObject.Add("SupervisorEmail", null);
                }
                else
                {
                    if (Convert.ToString(SendMailToUser).ToLower() == "yes")
                        oJsonObject.Add("SupervisorEmail", obj.SupervisorEmail);
                    else
                        oJsonObject.Add("SupervisorEmail", null);
                }
               
                oJsonObject.Add("GoToMeetingUrl", obj.GoToMeetingUrl);
                oJsonObject.Add("StartTime", obj.StartTime);
                oJsonObject.Add("ContactPerson", obj.ContactPerson);
                oJsonObject.Add("ContactNumber", obj.ContactNumber);
                oJsonObject.Add("EndDate", obj.EndDate);
                oJsonObject.Add("EndTime", obj.EndTime);
                oJsonObject.Add("PlaceName", obj.PlaceName);
                objJObject.Add(oJsonObject);
            }
            string Url = this._configuration[Configuration.NotificationApi] + "/NominateUsersForSchedule";
            string Body = JsonConvert.SerializeObject(objJObject);
            string token = _identityService.GetToken();
            HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            return 1;
        }

        public async Task<int> SendILTNominationCancellationEmail(List<APIILTNominationEmail> objAPIILTNominationEmail)
        {
            List<JObject> objJObject = new List<JObject>();
            var SendMailToUser = await GetMasterConfigurableParameterValue("NOMINATE_COURSE_CC_MANAGER");
            foreach (APIILTNominationEmail obj in objAPIILTNominationEmail)
            {
                JObject oJsonObject = new JObject();
                oJsonObject.Add("LearnerName", obj.LearnerName);
                oJsonObject.Add("SupervisorName", obj.SupervisorName);
                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("VenueAddress", obj.VenueAddress);
                oJsonObject.Add("ScheduleCode", obj.ScheduleCode);
                oJsonObject.Add("toEmail", obj.toEmail);
                oJsonObject.Add("orgCode", obj.orgCode);
                oJsonObject.Add("URL", obj.URL);

                if (Convert.ToString(SendMailToUser).ToLower() == "yes")
                    oJsonObject.Add("SupervisorEmail", obj.SupervisorEmail);
                else
                    oJsonObject.Add("SupervisorEmail", null);
                oJsonObject.Add("GoToMeetingUrl", obj.GoToMeetingUrl);
                oJsonObject.Add("StartTime", obj.StartTime);
                oJsonObject.Add("ContactPerson", obj.ContactPerson);
                oJsonObject.Add("ContactNumber", obj.ContactNumber);
                oJsonObject.Add("EndDate", obj.EndDate);
                oJsonObject.Add("EndTime", obj.EndTime);
                oJsonObject.Add("PlaceName", obj.PlaceName);
                objJObject.Add(oJsonObject);
            }
            string Url = this._configuration[Configuration.NotificationApi] + "/CancelNominateUsersForSchedule";
            string Body = JsonConvert.SerializeObject(objJObject);
            string token = _identityService.GetToken();
            HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            return 1;
        }

        public async Task<int> SendTrainerMailForSchedule(List<APIILTNominationEmail> objAPIILTNominationEmail)
        {
            List<JObject> objJObject = new List<JObject>();
           
            foreach (APIILTNominationEmail obj in objAPIILTNominationEmail)
            {
                JObject oJsonObject = new JObject();
                oJsonObject.Add("UserName", obj.LearnerName);            
                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("VenueAddress", obj.VenueAddress);
                oJsonObject.Add("ScheduleCode", obj.ScheduleCode);
                oJsonObject.Add("toEmail", obj.toEmail);
                oJsonObject.Add("orgCode", obj.orgCode);
                oJsonObject.Add("URL", obj.URL);               
                oJsonObject.Add("StartTime", obj.StartTime);
                oJsonObject.Add("GoToMeetingUrl", obj.GoToMeetingUrl);

                objJObject.Add(oJsonObject);
            }
            string Url = this._configuration[Configuration.NotificationApi] + "/TrainerMailForSchedule";
            string Body = JsonConvert.SerializeObject(objJObject);
            string token = _identityService.GetToken();
            HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            return 1;
        }

        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configurationCode });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = dt.Rows[0]["Value"].ToString();
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return value;
        }

        public async Task<int> SendNominationMail(string UserID, string OrganisationCode, string lblCourseName, string lblModuleName, string lblScheduleCode, string PlaceName, string Venue, string StartDate, string EndDate, string StartTime, string EndTime, string ContactPersonName,
            string ContactPersonNumber)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/SendNominationEmails";
            JObject oJsonObject = new JObject();

            oJsonObject.Add("UserId", UserID);
            oJsonObject.Add("OrgCode", OrganisationCode);
            oJsonObject.Add("CourseTitle", lblCourseName);
            oJsonObject.Add("ModuleName", lblModuleName);
            oJsonObject.Add("ScheduleCode", lblScheduleCode);
            oJsonObject.Add("PlaceName", PlaceName);
            oJsonObject.Add("VenueAddress", Venue);
            oJsonObject.Add("StartDate", StartDate);
            oJsonObject.Add("EndDate", EndDate);
            oJsonObject.Add("StartTime", StartTime);
            oJsonObject.Add("EndTime", EndTime);
            oJsonObject.Add("ContactPerson", ContactPersonName);
            oJsonObject.Add("ContactNumber", ContactPersonNumber);

            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> SendILTNotificationSMS(List<APINominateUserSMS> objSMS)
        {
            List<JObject> objJObject = new List<JObject>();

            foreach (APINominateUserSMS obj in objSMS)
            {
                JObject oJsonObject = new JObject();

                oJsonObject.Add("OTP", obj.OTP);
                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("UserName", obj.UserName);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("StartTime", obj.StartTime);
                oJsonObject.Add("EndDate", obj.EndDate);
                oJsonObject.Add("EndTime", obj.EndTime);
                oJsonObject.Add("MobileNumber", obj.MobileNumber);
                oJsonObject.Add("orgCode", obj.organizationCode);
                oJsonObject.Add("ScheduleCode", obj.ScheduleCode);
                oJsonObject.Add("UserId", obj.UserID);
                objJObject.Add(oJsonObject);

                string Url = this._configuration[Configuration.NotificationApi] + "/NominateUserSMS";
                string Body = JsonConvert.SerializeObject(objJObject);
                string token = _identityService.GetToken();
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            }

            return 1;
        }

        public async Task<int> SendScheduleCancellationNotificationSMS(List<APINominateUserSMS> objSMS)
        {
            List<JObject> objJObject = new List<JObject>();

            foreach (APINominateUserSMS obj in objSMS)
            {
                JObject oJsonObject = new JObject();

                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("UserName", obj.UserName);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("MobileNumber", obj.MobileNumber);
                oJsonObject.Add("orgCode", obj.organizationCode);
                oJsonObject.Add("UserId", obj.UserID);
                objJObject.Add(oJsonObject);

                string Url = this._configuration[Configuration.NotificationApi] + "/ScheduleCancelSMS";
                string Body = JsonConvert.SerializeObject(objJObject);
                string token = _identityService.GetToken();
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            }

            return 1;
        }
        public async Task<int> SendScheduleRequestApproveNotificationSMS(List<APINominateUserSMS> objSMS)
        {
            List<JObject> objJObject = new List<JObject>();

            foreach (APINominateUserSMS obj in objSMS)
            {
                JObject oJsonObject = new JObject();

                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("UserName", obj.UserName);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("MobileNumber", obj.MobileNumber);
                oJsonObject.Add("orgCode", obj.organizationCode);
                oJsonObject.Add("UserId", obj.UserID);
                objJObject.Add(oJsonObject);

                string Url = this._configuration[Configuration.NotificationApi] + "/ScheduleRequestApproveSMS";
                string Body = JsonConvert.SerializeObject(objJObject);
                string token = _identityService.GetToken();
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            }

            return 1;
        }
        public async Task<int> SendScheduleRequestNotificationSMSToManager(List<APINominateUserSMS> objSMS)
        {
            List<JObject> objJObject = new List<JObject>();

            foreach (APINominateUserSMS obj in objSMS)
            {
                JObject oJsonObject = new JObject();

                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("UserName", obj.UserName);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("MobileNumber", obj.MobileNumber);
                oJsonObject.Add("orgCode", obj.organizationCode);
                oJsonObject.Add("UserId", obj.UserID);
                objJObject.Add(oJsonObject);

                string Url = this._configuration[Configuration.NotificationApi] + "/ScheduleRequestToManagerSMS";
                string Body = JsonConvert.SerializeObject(objJObject);
                string token = _identityService.GetToken();
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            }

            return 1;
        }
        public async Task<int> SendScheduleRequestRejectNotificationSMS(List<APINominateUserSMS> objSMS)
        {
            List<JObject> objJObject = new List<JObject>();

            foreach (APINominateUserSMS obj in objSMS)
            {
                JObject oJsonObject = new JObject();

                oJsonObject.Add("CourseTitle", obj.CourseTitle);
                oJsonObject.Add("UserName", obj.UserName);
                oJsonObject.Add("StartDate", obj.StartDate);
                oJsonObject.Add("MobileNumber", obj.MobileNumber);
                oJsonObject.Add("orgCode", obj.organizationCode);
                oJsonObject.Add("UserId", obj.UserID);
                objJObject.Add(oJsonObject);

                string Url = this._configuration[Configuration.NotificationApi] + "/ScheduleRequestRejectSMS";
                string Body = JsonConvert.SerializeObject(objJObject);
                string token = _identityService.GetToken();
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            }

            return 1;
        }
        public async Task<int> SendTNASubmissionByEndUserEmail(string userName, string ToEmail, string orgCode, string CC = null)
        {
            try
            {
                List<List<string>> Tokens = new List<List<string>>();
                List<string> Token = new List<string>();
                Token.Add("[UserId]");
                Token.Add("[Year]");
                Token.Add("[ToEmail]");
                Token.Add("[URLforHowToSubmitTNA]");
                Token.Add("[CC]");
                Token.Add("[Admin Mail]");
                Tokens.Add(Token);
                List<List<string>> TokenValues = new List<List<string>>();
                List<string> TokenValue = new List<string>();
                TokenValue.Add(userName);
                TokenValue.Add(DateTime.UtcNow.Year.ToString());
                TokenValue.Add(ToEmail);
                TokenValue.Add(this._configuration["EmpoweredLmsPath"]);
                TokenValue.Add(CC);
                TokenValue.Add("admin@dwtc.com");
                TokenValues.Add(TokenValue);

                object json = new
                {
                    TemplateTitle = "TNA submission by Employee",
                    Tokens = Tokens,
                    TokenValues = TokenValues,
                    orgCode = orgCode
                };
                string Url = this._configuration[Configuration.NotificationApi];
                Url += "/GetTemplateAndSendMail";
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, JsonConvert.SerializeObject(json));
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }

        public async Task<int> SendCourseApplicabilityEmail(int CourseId, string orgCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/CourseApplicability";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", CourseId);
            oJsonObject.Add("organizationCode", orgCode);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public int SendCourseApplicabilityPushNotification(int CourseId, string orgCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/CourseApplicabilityPushNotification";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", CourseId);
            oJsonObject.Add("OrganizationCode", orgCode);
           
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> SendScheduleCreationEmailNotification(int? CourseId, string orgCode, int scheduleId)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/SendScheduleCreationEmail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", CourseId);
            oJsonObject.Add("ScheduleId", scheduleId);
            oJsonObject.Add("organizationCode", orgCode);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }


        public async Task<int> SendScheduleCancellationEmail(string orgCode, int scheduleId)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/MailScheduleCancellataion";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("ScheduleId", scheduleId);
            oJsonObject.Add("organizationCode", orgCode);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> SendMailForAbsentUsers(APIILTAttendanceResponse aPIILTAttendanceResponse, string OrgCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/SendMailForAbsentUsers";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseTitle", aPIILTAttendanceResponse.CourseTitle);
            oJsonObject.Add("ScheduleCode", aPIILTAttendanceResponse.ScheduleCode);
            oJsonObject.Add("organizationCode", OrgCode);
            oJsonObject.Add("UserName", aPIILTAttendanceResponse.UserName);
            oJsonObject.Add("ToMailId", aPIILTAttendanceResponse.EmailId);
            oJsonObject.Add("AdminEmailId", aPIILTAttendanceResponse.ManagerEmailId);
            oJsonObject.Add("SLMEmailID", aPIILTAttendanceResponse.skipLevelManagerEmailId);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> SendMailForAttendanceUsers(APIILTAttendanceResponse aPIILTAttendanceResponse, string OrgCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/SendMailForAttendanceUsers";
            JObject oJsonObject = new JObject();

            oJsonObject.Add("ScheduleCode", aPIILTAttendanceResponse.ScheduleCode);
            oJsonObject.Add("organizationCode", OrgCode);
            oJsonObject.Add("UserName", aPIILTAttendanceResponse.UserName);
            oJsonObject.Add("ToMailId", aPIILTAttendanceResponse.EmailId);
            oJsonObject.Add("CourseName", aPIILTAttendanceResponse.CourseTitle);
            oJsonObject.Add("ModuleName", aPIILTAttendanceResponse.ModuleName);
            oJsonObject.Add("IsFeedback", aPIILTAttendanceResponse.IsFeedback);
            oJsonObject.Add("CourseId", aPIILTAttendanceResponse.CourseId);
            oJsonObject.Add("Id", aPIILTAttendanceResponse.UserMasterId);
            oJsonObject.Add("AttendanceDate", aPIILTAttendanceResponse.AttendanceDate);


            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }


        public async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));

                return response;
            }

        }

        public async Task<int> SendTNASubmissionByLmEmail(string userName, string ToEmail, string orgCode, bool isApprovedByTa, string TNAYear, string CC = null)
        {
            try
            {
                List<List<string>> Tokens = new List<List<string>>();
                List<string> Token = new List<string>();
                Token.Add("[UserId]");
                Token.Add("[Year]");
                Token.Add("[ToEmail]");
                Token.Add("[URLforHowToSubmitTNA]");
                Token.Add("[CC]");
                Token.Add("[Admin Mail]");
                Tokens.Add(Token);
                List<List<string>> TokenValues = new List<List<string>>();
                List<string> TokenValue = new List<string>();
                TokenValue.Add(userName);
                TokenValue.Add(TNAYear);
                TokenValue.Add(ToEmail);
                TokenValue.Add(this._configuration["EmpoweredLmsPath"]);
                TokenValue.Add(CC);
                TokenValue.Add("admin@dwtc.com");
                TokenValues.Add(TokenValue);

                string TemplateName = null;
                if (isApprovedByTa)
                    TemplateName = "TNA submission by Learning Admin";
                else
                    TemplateName = "Line Manager’s Response to TNA";
                object json = new
                {
                    TemplateTitle = TemplateName,
                    Tokens = Tokens,
                    TokenValues = TokenValues,
                    orgCode = orgCode
                };
                string Url = this._configuration[Configuration.NotificationApi];
                Url += "/GetTemplateAndSendMail";
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, JsonConvert.SerializeObject(json));
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }
        public async Task<int> SendTNASubmissionByBUEmail(string userName, string ToEmail, string orgCode, string CC = null)
        {
            try
            {
                List<List<string>> Tokens = new List<List<string>>();
                List<string> Token = new List<string>();
                Token.Add("[UserId]");
                Token.Add("[Year]");
                Token.Add("[ToEmail]");
                Token.Add("[URLforHowToSubmitTNA]");
                Token.Add("[CC]");
                Token.Add("[Admin Mail]");
                Tokens.Add(Token);
                List<List<string>> TokenValues = new List<List<string>>();
                List<string> TokenValue = new List<string>();
                TokenValue.Add(userName);
                TokenValue.Add(DateTime.UtcNow.Year.ToString());
                TokenValue.Add(ToEmail);
                TokenValue.Add(this._configuration["EmpoweredLmsPath"]);
                TokenValue.Add(CC);
                TokenValue.Add("admin@dwtc.com");
                TokenValues.Add(TokenValue);

                object json = new
                {
                    TemplateTitle = "BU HEAD’s Response to TNA",
                    Tokens = Tokens,
                    TokenValues = TokenValues,
                    orgCode = orgCode
                };
                string Url = this._configuration[Configuration.NotificationApi];
                Url += "/GetTemplateAndSendMail";
                HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, JsonConvert.SerializeObject(json));
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }

        // Enrollment email notification //


        //Email - User
        //  LM_EmailID - LM
        //  LA_EmailID - LA
        //  [User Name] - User Name
        //  [Schedule Code]
        public async Task<int> CourseRequestMailToUser(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/CourseRequestMailToUser";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> ScheduleEnrollmentMailToLM(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string EmployeeName, string startTime, string endtime, string sdate, string edate, string venue)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/ScheduleEnrollmentMailToLM";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("EmployeeName", EmployeeName);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> ScheduleRequestedMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string EmployeeName, string startTime, string endtime, string sdate, string edate, string venue)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/ScheduleRequestedMail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("EmployeeName", EmployeeName);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> ScheduleCreationMailToSupervisor(string orgCode, string Email, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/ScheduleCreationMailToSupervisor";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }



        public async Task<int> TrainingRequestApprovalByLM(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string EndUserName)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/TrainingRequestApprovalByLM";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            oJsonObject.Add("EmployeeName", EndUserName);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> TrainingRequestRejectedToUser(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/TrainingRequestRejectedToUser";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);

            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> TrainingRequestRejectedMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string reason)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/TrainingRequestRejectedMail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            oJsonObject.Add("Reason", reason);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> TrainingBatchRequestRejectedMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string reason)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/TrainingBatchRequestRejectedMail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            oJsonObject.Add("Reason", reason);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }



        public async Task<int> EnrollmentNotificationToHR(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string EndUserName)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/EnrollmentNotificationToHR";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            oJsonObject.Add("EmployeeName", EndUserName);

            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }


        public async Task<int> NominationToEmpInEnrollment(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string BU_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/NominationToEmpInEnrollment";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("LA_EmailID", LA_EmailID);
            oJsonObject.Add("BU_EmailID", BU_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);


            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }


        public async Task<int> TrainingRequestFullyApprovedToUser(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/TrainingRequestFullyApprovedToUser";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> TrainingRequestApprovalMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue, string courseName, int courseId, string EmpoweredHost)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/TrainingRequestApprovalMail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            oJsonObject.Add("CourseTitle", courseName);
            oJsonObject.Add("CourseId", courseId);
            oJsonObject.Add("EmpoweredHost", EmpoweredHost);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> TrainingBatchRequestApprovalMail(string orgCode, string Email, string LM_EmailID, string LA_EmailID, string UserName, string ScheduleCode, string startTime, string endtime, string sdate, string edate, string venue)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/TrainingBatchRequestApprovalMail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Email", Email);
            oJsonObject.Add("LM_EmailID", LM_EmailID);
            oJsonObject.Add("UserName", UserName);
            oJsonObject.Add("ScheduleCode", ScheduleCode);
            oJsonObject.Add("organizationCode", orgCode);
            oJsonObject.Add("StartDate", sdate);
            oJsonObject.Add("EndDate", edate);
            oJsonObject.Add("StartTime", startTime);
            oJsonObject.Add("EndTime", endtime);
            oJsonObject.Add("TrainingPlace", venue);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> SendRemainderMailForCourse(int CourseID, string organizationCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/RemainderMailToUsersForCourse";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", CourseID);
            oJsonObject.Add("organizationCode", organizationCode);
           
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> SendRemainderSMSForCourse(int CourseID, string organizationCode)
        {
            try
            {
                string Url = this._configuration[Configuration.NotificationApi];
                Url += "/RemainderSMSToUsersForCourse";
                JObject oJsonObject = new JObject();
                oJsonObject.Add("CourseId", CourseID);
                oJsonObject.Add("organizationCode", organizationCode);
                HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
                return 1;
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 1;
            }
        }

        public async Task<int> SendRemainderMailForUserWiseCourse(string UserId, int CourseID, string organizationCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/RemainderMailToUserWiseForCourse";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("UserId", UserId);
            oJsonObject.Add("CourseId", CourseID);
            oJsonObject.Add("organizationCode", organizationCode);            
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> SendNominationSMS(string lblUserId, string Coursename, string StartDate, string StartTime, string OTP, string OrgCode, string EndDate, string EndTime)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/NominateUserImportSMS";
            JObject oJsonObject = new JObject();

            oJsonObject.Add("OTP", OTP);
            oJsonObject.Add("CourseTitle", Coursename);
            oJsonObject.Add("UserId", lblUserId);
            oJsonObject.Add("StartDate", StartDate);
            oJsonObject.Add("StartTime", StartTime);
            oJsonObject.Add("EndDate", EndDate);
            oJsonObject.Add("EndTime", EndTime);

            oJsonObject.Add("OrgCode", OrgCode);


            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<int> SendCourseCompletionStatusMail(string CourseTitle, int UserId, APIUserDetails aPIUserDetails, int CourseId)
        {


            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/SendCourseCompletionStatusMail";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseTitle", CourseTitle);
            oJsonObject.Add("username", aPIUserDetails.UserName);
            oJsonObject.Add("UserId", UserId);
            oJsonObject.Add("EmailId", aPIUserDetails.EmailId);
            oJsonObject.Add("OrgCode", aPIUserDetails.CustomerCode);
            oJsonObject.Add("CourseId", CourseId);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> SendCourseRequestApprovalMailToUsers(List<APINodalRequestList> aPINodalRequestList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/CourseRequestApprovalMailToUsers";
            string Body = JsonConvert.SerializeObject(aPINodalRequestList);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }
        public async Task<int> SendCourseRequestRejectedMailToUsers(List<APINodalRequestList> aPINodalRequestList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/CourseRequestRejectionMailToUsers";
            string Body = JsonConvert.SerializeObject(aPINodalRequestList);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }
        public async Task<int> SendSelfCourseRequestMail(APISelfCourseRequestEmail aPISelfCourseRequestEmail)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/SelfCourseRequestMailToUser";
            string Body = JsonConvert.SerializeObject(aPISelfCourseRequestEmail);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }
        public async Task<int> UserSignUpMailToNodalOfficers(List<APINodalUserDetailsEmail> aPINodalUserDetailsList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/UserSignUpMailToNodalOfficers";
            string Body = JsonConvert.SerializeObject(aPINodalUserDetailsList);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }
    }
}
