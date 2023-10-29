using Courses.API.Helper;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Courses.API.Repositories
{
    public class SMSRepository : ISMS
    {
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;
        public SMSRepository(IConfiguration configuration, IIdentityService identityService)
        {
            _configuration = configuration;
            _identityService = identityService;
        }


        //public async Task<int> SendCourseApplicabilitySMS(int CourseId, string orgCode)

        //{
        //    string Url = this._configuration[Configuration.NotificationApi];

        //    Url += "/CourseApplicabilitySMS";
        //    JObject oJsonObject = new JObject();
        //    oJsonObject.Add("CourseId", CourseId);
        //    oJsonObject.Add("organizationCode", orgCode);
        //    HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
        //    return 1;
        //}

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
