using Assessment.API.APIModel;
using Assessment.API.Helper;
using Assessment.API.Repositories;
using Assessment.API.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Courses.API.Repositories
{
    public class NotificationRepository : INotification
    {
        private readonly IConfiguration _configuration;
        IIdentityService _identityService;
        //    INotification _notification;

        public NotificationRepository(IConfiguration configuration, IIdentityService identityService)
        {
            this._configuration = configuration;
            this._identityService = identityService;
        }

        public async Task<int> ScheduleRequestNotificationTo_Common(List<ApiNotification> notifications, string? token = null)
        {
            string Body = JsonConvert.SerializeObject(notifications);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/ScheduleRequestNotificationTo_Common";
            HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            return 1;
        }

        public async Task<int> ScheduleRequestNotificationTo_Common(ApiNotification notification, string? token = null)
        {
            if (token == null)
                token = this._identityService.GetToken();
            JObject oJsonObject = new JObject();
            oJsonObject.Add("title", notification.Title);
            oJsonObject.Add("message", notification.Message);
            oJsonObject.Add("url", notification.Url);
            oJsonObject.Add("type", notification.Type);
            oJsonObject.Add("userId", notification.UserId);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/ScheduleRequestNotificationTo_Common";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
            return 1;
        }
    }
}