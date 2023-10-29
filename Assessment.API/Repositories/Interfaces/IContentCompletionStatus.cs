using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface IContentCompletionStatus : IRepository<ContentCompletionStatus>
    {
        Task<int> Post(ContentCompletionStatus contentCompletionStatus, string? CourseType = null, string? Token = null, string? OrgCode = null);
        Task<int> PostCompletion(ContentCompletionStatus contentCompletionStatus, string? CourseType = null, string? Token = null);
        Task<ContentCompletionStatus> Get(int userId, int courseId, int moduleId);


    }
}
