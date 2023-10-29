using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface IModuleCompletionStatusRepository : IRepository<ModuleCompletionStatus>
    {
        Task<int> Post(ModuleCompletionStatus moduleCompletionStatus, string CourseType = "noclassroom", string? Token = null, string? Orgcode = null);
        Task<int> PostCompletion(ModuleCompletionStatus moduleCompletionStatus, string? CourseType = null, string? Token = null, string? OrgCode = null, string? CourseStatusFromSP = null);

    }
}
