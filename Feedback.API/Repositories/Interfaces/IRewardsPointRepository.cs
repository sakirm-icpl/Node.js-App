using Courses.API.APIModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IRewardsPointRepository
    {
        Task<int> AddCourseCompletionBaseOnDate(Model.Course course, int courseId, int dateTime, string OrgCode);
        Task<int> AddCourseReward(Model.Course course, int userId, string OrgCode);

        Task<int> AddCourseFeedbackReward(int userId, int referenceId, int courseId, string OrgCode);

        Task<int> AddModuleFeedbackReward(int userId, int referenceId, int courseId, int moduleId, string OrgCode);

    }
}
