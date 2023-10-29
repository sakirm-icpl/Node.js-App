using System.Collections.Generic;
using System.Threading.Tasks;
using TNA.API.APIModel;

namespace TNA.API.Repositories
{
    public interface INotification
    {
        Task<int> SendNotification(ApiNotification notification, string token);
        Task<int> SendIndivudualNotification(ApiNotification notification, string token = null);
        Task<int> SendCourseInProgressNotification(Model.Course course);
        Task<int> SendGroupNotification(List<ApiNotification> notifications, string token = null);
        Task<int> SendLmHrBuIndivudualNotification(ApiNotification notification, string token = null);
        Task<int> UpdateNotification(int id, ApiNotification notification, string token);
        Task<int> DeleteNotification(int id, string token);
        Task<int> SendIndivudualNotificationBulk(ApiNotification notification, string token = null);
        Task<int> SendScheduleCreationNotification(ApiNotification notification, string token, string organizationcode);
        Task<int> ScheduleRequestNotificationTo_Common(ApiNotification notification, string token = null);
        Task<int> SendNotificationForCourseApplicability(APIApplicableNotifications notification, string token);
        Task<int> ScheduleRequestNotificationTo_Common(List<ApiNotification> notification, string token = null);

    }
}
