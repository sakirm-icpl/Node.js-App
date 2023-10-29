using Assessment.API.APIModel;

namespace Assessment.API.Repositories.Interfaces
{
    public interface IMyCoursesRepository
    {
         Task<int> GetUserDetailsByUserID(string userId);
        Task<APIMyCoursesModule> GetModule(int userId, int courseId, string? organizationcode = null, int? groupId = null);

        Task<ApiCourseInfo> GetModuleInfo(int userId, int courseId, int? moduleId);

    }
}
