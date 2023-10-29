using Assessment.API.APIModel;
using Assessment.API.Common;
using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;


namespace Assessment.API.Repositories.Interface
{
    public interface IAssessmentConfigurationSheets : IRepository<AssessmentSheetConfiguration>
    {
        int GetPassingPercentage(int assessmentSheetConfigID, string OrgCode);
        Task<int> ConfigureAssessment(APIAssessmentConfiguration aPIAssessmentsQuestion, int UserId);
        Task<Message> UpdateConfiguration(int id, APIAssessmentConfiguration aPIAssessmentQuestion, int UserId, string OrgCode);
        Task<bool> IsAssessmentConfigurationIdExist(APIStartAssessment aPIStartAssessment);
    }
}
