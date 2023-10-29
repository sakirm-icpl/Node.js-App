using Assessment.API.Models;
using Assessment.API.Repositories.Interfaces;

namespace Assessment.API.Repositories.Interface
{
    public interface IAssessmentSheetConfigurationDetails : IRepository<AssessmentSheetConfigurationDetails>
    {
        AssessmentSheetConfigurationDetails GetByConfigid(int? assessmentSheetConfigId);
        AssessmentSheetConfigurationDetails GetConfigurationDetails(int QuestionID, int? assessmentDetailsId);
        int GetTotalQuestion(int AssessmentSheetConfigID, string orgCode);
        Task<int> DeleteQuestion(int ConfigurationID, int[] QuestionsId, int UserId);
        List<AssessmentSheetConfigurationDetails> GetConfigurations(int? assessmentDetailsId);
        int RemoveQuestions(List<AssessmentSheetConfigurationDetails> listquestion);
    }
}
