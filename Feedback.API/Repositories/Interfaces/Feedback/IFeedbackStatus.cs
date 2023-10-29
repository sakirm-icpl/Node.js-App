using Courses.API.Repositories.Interfaces;
using Feedback.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IFeedbackStatus : IRepository<FeedbackStatus>
    {
        bool Exists(int courseId, int moduleId, int userId, int? dpId = null, bool IsOJT = false);
        Task<FeedbackStatus> FeedbackStatusExists(int courseId, int moduleId, int userId);
        Task<List<FeedbackStatus>> Get(int page, int pageSize, string? search = null, string? filter = null);
        Task<int> Count(string? search = null, string? filter = null);
        Task<int> AddModuleCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null);
        Task<bool> IsFeedbackSubmitted(int courseId, int moduleId, int userId, bool IsOJT);
        Task<bool> IsIdpFeedbackSubmited(int DpId, int userId);
        Task<int> AddCourseCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null);

        Task<bool> IsContentCompletedforFeeback(int userId, int courseId, int? moduleId, bool IsOJT = false);
        Task<int> AddModuleFeedbackCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null);
        Task<bool> IsDevPlanCompletedforFeeback(int userId, int? dpId);

    }
}
