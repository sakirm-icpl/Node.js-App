using Courses.API.APIModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface IRewardsPointRepository
    {
        Task<int> RewardPointSave(string functionCode, string category, int referenceId, int userId, int? point = null, string description = null);
        Task<int> AddReviewSubmitReward(int userId, int referenceId, int courseId, string OrgCode, string Category, string Condition);
        Task<int> AddCourseFeedbackReward(int userId, int referenceId, int courseId, string OrgCode);
        Task<int> AddModuleFeedbackReward(int userId, int referenceId, int courseId, int moduleId, string OrgCode);
       
        Task<int> AddRatingSubmitReward(int userId, int referenceId, int courseId, string OrgCode, string Category, string Condition);
        Task<int> AddCourseReward(Model.Course course, int userId, string OrgCode);
        Task<int> AddRewardDiscussionReply(int UserId, int CourseId, string OrgCode, string Category, string Condition,string coursTitle);
        Task<int> AddRewardCertificate(int CourseId, int Userid, string OrgCode,string coursTitle);
        Task<int> AddPreAssessment(int CourseID ,int userId, string OrgCode, int ModuleId);
        Task<int> AddAssignmentDetailsRewardReward(int userId, int referenceId, int courseId,string OrgCode);
        Task<int> AddBookReadRewardPoint(int userId, int referenceId, string OrgCode);
        Task<int> AddCourseCompletionBaseOnDate(Model.Course course,  int courseId, int dateTime, string OrgCode);
       Task<int> AddPostAssessment(int CourseID, int userId, string OrgCode, int ModuleID);
        Task<string> GetRewardPointDescription(string Condition,string Category);
        Task<List<APIRewardPoints>> checkcache(string cacheKeyConfig);
        Task<int> AddCourseCreditPoints(Model.Course course, int userId, string OrgCode);


    }
}
