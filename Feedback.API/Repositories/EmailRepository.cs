using Feedback.API.APIModel;
using Courses.API.Repositories.Interfaces;
using Newtonsoft.Json.Linq;
using log4net;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Services;
//using System.Configuration;
using Feedback.API.Helper;
using System.Text;

namespace Feedback.API.Repositories
{
    public class EmailRepository : IEmail
    {
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailRepository));

        public EmailRepository(IConfiguration configuration, IIdentityService identityService,  ICustomerConnectionStringRepository customerConnection)
        {
            this._customerConnection = customerConnection;
            _configuration = configuration;
            _identityService = identityService;
           
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
        public async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));

                return response;
            }

        }
    }
}

