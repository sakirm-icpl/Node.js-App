using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IContentCompletionStatus : IRepository<ContentCompletionStatus>
    {
        Task<ContentCompletionStatus> Get(int userId, int courseId, int moduleId);
        Task<int> Post(ContentCompletionStatus contentCompletionStatus, string CourseType = null, string Token = null, string OrgCode = null);
        Task<int> PostCompletion(ContentCompletionStatus contentCompletionStatus, string CourseType = null, string Token = null);
        Task<bool> IsContentCompleted(int userId, int courseId, int moduleId);
        Task<string> GetStatus(int courseId, int moduleId, int userId);
        Task<bool> IsContentStarted(int userId, int courseId, int moduleId);
        Task<KpointStatus> SaveKpointStatus(APIContentCompletionStatusForKpoint aPIContentCompletionStatusForKpoint, int Userid);
        Task<List<KPointReportV2>> getKpointReport(APIForKpointReport aPIForKpointReport, int Userid);
    }
}
