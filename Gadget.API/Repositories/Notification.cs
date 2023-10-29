using Gadget.API.APIModel;
using Gadget.API.Helper;
using Gadget.API.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gadget.API.Repositories
{
    public class Notification : INotification
    {
        private readonly IConfiguration _configuration;
        public Notification(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        public async Task<int> SendNotification(ApiNotification notification, string token = null)
        {
       
        JObject JsonObject = new JObject
            {
                { "title", notification.Title },
                { "message", notification.Message },
                { "url", notification.Url },
                { "type", notification.Type },
                 { "QuizId", notification.QuizId },
                  { "SurveyId", notification.SurveyId }
            };
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, JsonObject, token);


            return 1;
        }
        public async Task<int> SendEmail(string toEmail, string subject, string message, string orgCode, string customerCode = null)
        {
            JObject JsonObject = new JObject
            {
                { "toEmail", toEmail },
                { "subject", subject },
                { "message", message },
                { "customerCode", customerCode },
                { "organizationCode", orgCode }
            };
            string Url = this._configuration[Configuration.NotificationApi];
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, JsonObject); ;
            return 1;
        }
    }
}
