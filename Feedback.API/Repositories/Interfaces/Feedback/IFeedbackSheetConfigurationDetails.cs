using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IFeedbackSheetConfigurationDetails : IRepository<FeedbackSheetConfigurationDetails>
    {
        Task<int> DeleteConfiguredQuestions(int[] QuestionsId, int UserId);
        Task<int> DeleteQuestionsByConfigId(int configId);
    }
}
