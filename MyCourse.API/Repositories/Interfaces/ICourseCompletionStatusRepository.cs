using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface ICourseCompletionStatusRepository : IRepository<CourseCompletionStatus>
    {
        Task<CourseCompletionStatus> Get(int userId, int courseId);
        Task<IEnumerable<object>> GetStatus(int courseId);
        Task<DateTime> GetCourseCompletionDate(int courseId, int userId);
        Task<bool> Exist(int courseId, int userId);
        Task<int> Post(CourseCompletionStatus courseCompletionStatus, string OrgCode = null);
        Task<int> PostCompletion(CourseCompletionStatus courseCompletionStatus, string OrgCode = null);
        Task<string> GetCourseCompletionStatus(int userId, int courseId);
        Task<DateTime> GetCourseAssignedDate(int courseId, int userId);
        Task<int> ScheduleRequestNotificationTo_CommonBulk(List<ApiNotification> Notification, string token);
    }
}
