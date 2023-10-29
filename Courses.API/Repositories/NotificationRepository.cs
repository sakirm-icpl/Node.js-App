using Courses.API.APIModel;
using Courses.API.Helper;
using Courses.API.Helper.Metadata;
using Courses.API.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Courses.API.Repositories
{
    public class NotificationRepository : INotification
    {
        private readonly IConfiguration _configuration;
        IIdentityService _identityService;
    //    INotification _notification;

        public NotificationRepository(IConfiguration configuration,   IIdentityService identityService)
        {
            this._configuration = configuration;
            this._identityService = identityService;
        }
        public async Task<int> SendNotification(ApiNotification notification, string token)
        {
            JObject oJsonObject = new JObject();
            oJsonObject.Add("title", notification.Title);
            oJsonObject.Add("message", notification.Message);
            oJsonObject.Add("url", notification.Url);
            oJsonObject.Add("type", notification.Type);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
            return 1;
        }

        public async Task<int> SendNotificationForCourseApplicability(APIApplicableNotifications notification, string token)
        {
            JObject oJsonObject = new JObject();
            oJsonObject.Add("UserId", notification.UserId);
            oJsonObject.Add("NotificationId", notification.NotificationId);
            oJsonObject.Add("IsRead", notification.IsRead);
            oJsonObject.Add("IsReadCount", notification.IsReadCount);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/CourseApplicabilityAddNotification";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
            return 1;
        }
        public async Task<int> SendScheduleCreationNotification(ApiNotification notification, string token, string organizationcode)
        {
            JObject oJsonObject = new JObject();
            oJsonObject.Add("title", notification.Title);
            oJsonObject.Add("message", notification.Message);
            oJsonObject.Add("url", notification.Url);
            oJsonObject.Add("type", notification.Type);
            oJsonObject.Add("Organizationcode", organizationcode);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/AddNotificationForEnrolledCourseSchedule";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
            return 1;
        }

        public async Task<int> UpdateNotification(int Id, ApiNotification notification, string token)
        {
            JObject oJsonObject = new JObject();
            oJsonObject.Add("Id", Id);
            oJsonObject.Add("title", notification.Title);
            oJsonObject.Add("message", notification.Message);
            oJsonObject.Add("url", notification.Url);
            oJsonObject.Add("type", notification.Type);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/" + Id;
            HttpResponseMessage response = await ApiHelper.CallPutAPI(Url, oJsonObject, token);
            return 1;
        }

        public async Task<int> DeleteNotification(int Id, string token)
        {
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/" + Id;
            _ = ApiHelper.CallDeleteAPI(Url, token);
            return 1;
        }

        public async Task<int> SendLmHrBuIndivudualNotification(ApiNotification notification, string token = null)
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
            Url = Url + "/tlsNotification/AddNotificationForTNALmHrBu";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
            return 1;
        }

        public async Task<int> ScheduleRequestNotificationTo_Common(ApiNotification notification, string token = null)
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

        public async Task<int> SendIndivudualNotification(ApiNotification notification, string token = null)
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
            Url = Url + "/tlsNotification/AddForParticularUser";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
            return 1;
        }

        public async Task<int> SendIndivudualNotificationBulk(ApiNotification notification, string token = null)
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
            Url = Url + "/tlsNotification/AddNotificationForTNA";
            HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
            return 1;
        }
        public async Task<int> SendGroupNotification(List<ApiNotification> notifications, string token = null)
        {
            if (token == null)
                token = this._identityService.GetToken();
            string Body = JsonConvert.SerializeObject(notifications);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/AddGroupNotification";
            HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            return 1;
        }
        public async Task<int> SendCourseInProgressNotification(Model.Course course)
        {
            ApiNotification Notification = new ApiNotification();
            string Message = this._configuration[Configuration.CourseCompletionNotification].ToString();
            Message = Message.Replace("{course}", course.Title);
            Notification.Message = Message;
            Notification.Title = NotificationMessage.CourseInprogess;
            Notification.Type = Record.Course;
            Notification.Url = TlsUrl.NotificationAPost + course.Id;
            Notification.UserId = Convert.ToInt32(Security.Decrypt(_identityService.GetUserIdentity()));
            await this.SendIndivudualNotification(Notification);
            return 1;
        }

        public async Task<int> ScheduleRequestNotificationTo_Common(List<ApiNotification> notifications, string token = null)
        {
               string Body = JsonConvert.SerializeObject(notifications);
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/ScheduleRequestNotificationTo_Common";
            HttpResponseMessage response = await ApiHelper.CallPostAPI(Url, Body, token);
            return 1;
        }
    }
}
