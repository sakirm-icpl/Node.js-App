using Assessment.API.APIModel;

namespace Assessment.API.Repositories
{
    public interface INotification
    {
       
        Task<int> ScheduleRequestNotificationTo_Common(List<ApiNotification> notification, string? token = null);
        Task<int> ScheduleRequestNotificationTo_Common(ApiNotification notification, string? token = null);


    }
}
