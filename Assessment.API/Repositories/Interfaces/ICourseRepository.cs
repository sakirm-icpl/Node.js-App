using Assessment.API.Model.EdCastAPI;
using Assessment.API.Repositories.Interfaces;

namespace Assessment.API.Repositories.Interfaces
{
    public interface ICourseRepository : IRepository<Assessment.API.Model.Course>
    {
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
        Task<string> GetAssessmentConfigurationID(int? id, int? CId, string orgCode, bool isPreAssessment = false, bool isContentAssessment = false);
        Task<string> GetManagerAssessmentConfigurationID(int? courseId, int? moduleId);
        Task<string> GetCourseNam(int? id);
        Task<string> GetModuleName(int? id);

        Task<DarwinboxTransactionDetails> PostCourseStatusToDarwinbox(int courseID, int userId, string status, string orgcode, string? connectionstring = null);


    }
}