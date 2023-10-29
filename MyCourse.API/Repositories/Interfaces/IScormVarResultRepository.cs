using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IScormVarResultRepository : IRepository<ScormVarResult>
    {
        Task<ScormVarResult> Get(int userId, int courseId, int moduleId);
        //Task<int> Count(string search = null);
        Task<int> Count(int userId, int courseId, int moduleId);
        //Task<int> GetNoAttempt(int userId, int courseId, int moduleId);
        Task<string> GetScore(int userId, int courseId, int moduleId);
        Task<string> GetResult(int userId, int courseId, int moduleId);
        //Task<APIScormCompletionDetails> GetScormDetails(int userId, int courseId, int moduleId);
        //Task<APIAssessmentCompletionDetails> GetAssessmentDetails(int userId, int courseId, int moduleId);
        Task<bool> IsContentCompleted(int UserId, int CourseId, int ModuleId);
    }
}
