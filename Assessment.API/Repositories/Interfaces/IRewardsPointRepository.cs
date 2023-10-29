using Assessment.API.APIModel;
using Assessment.API.Models;


namespace Assessment.API.Repositories.Interfaces

{
    public interface IRewardsPointRepository
    {

        Task<int> AddCourseReward(Model.Course course, int userId, string OrgCode);
        Task<int> AddCourseCompletionBaseOnDate(Model.Course course, int userId, int dateTime, string OrgCode);

        Task<int> AddCourseCreditPoints(Model.Course course, int userId, string OrgCode);
        Task<List<APIRewardPoints>> checkcache(string cacheKeyConfig);
        Task<int> RewardPointSave(string functionCode, string category, int referenceId, int userId, int? point = null, string? description = null);
        Task<List<APIRewardPoints>> GetAllRewardPoints();
        Task<int> AddPreAssessment(int CourseID, int userId, string OrgCode, int ModuleId);
        Task<int> AddPostAssessment(int CourseID, int userId, string OrgCode, int ModuleID);

    }
}
