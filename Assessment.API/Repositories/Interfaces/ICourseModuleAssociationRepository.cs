using Assessment.API.Model;

namespace Assessment.API.Repositories.Interfaces
{
    public interface ICourseModuleAssociationRepository : IRepository<CourseModuleAssociation>
    {
        Task<bool> IsFeedbackExist(int CourseID, int ModuleId);
        Task<bool> IsAssementExist(int CourseID, int ModuleId);
    }
}