namespace Feedback.API.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Feedback.API.Model.Course>
    {
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
    }
}
