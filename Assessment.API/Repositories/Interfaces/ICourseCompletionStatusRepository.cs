using Assessment.API.APIModel;
using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface ICourseCompletionStatusRepository : IRepository<CourseCompletionStatus>
    {
        Task<int> Post(CourseCompletionStatus courseCompletionStatus, string? OrgCode = null);
        Task<string> GetCourseCompletionStatus(int userId, int courseId);
        Task<CourseCompletionStatus> Get(int userId, int courseId);
        Task<string> GetBoolConfigurablevalue(string configurableparameter);
        Task<DateTime> GetCourseAssignedDate(int userId, int courseId);
        Task<int> ScheduleRequestNotificationTo_CommonBulk(List<ApiNotification> Notification, string token);
        Task<int> PostCompletion(CourseCompletionStatus courseCompletionStatus, string? OrgCode = null);

    }
}
