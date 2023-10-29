using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks; 

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IModuleCompletionStatusRepository : IRepository<ModuleCompletionStatus>
    {
        Task<ModuleCompletionStatus> Get(int userId, int courseId, int moduleId);
        Task<int> Post(ModuleCompletionStatus moduleCompletionStatus, string CourseType = null, string Token = null, string Orgcode = null);
        Task<int> PostCompletion(ModuleCompletionStatus moduleCompletionStatus, string CourseType = null, string Token = null, string OrgCode = null, string CourseStatusFromSP = null);

        Task<bool> Exist(int userId, int courseId, int moduleId);
        Task<string> GetStatus(int courseId, int moduleId, int userId);
        Task<List<APIModuleStatus>> GetModuleStatus(int userId, int courseId, int page, int pageSize, string status = null);
        Task<int> GetModuleCount(int userId, int courseId, string status = null);
        Task<int> PostFeedbackCompletion(ModuleCompletionStatus moduleCompletionStatus, string CourseType = "noclassroom", string Token = null, string Orgcode = null);
    }
}
