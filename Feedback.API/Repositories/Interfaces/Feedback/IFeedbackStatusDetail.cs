using Feedback.API.APIModel;
using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IFeedbackStatusDetail : IRepository<FeedbackStatusDetail>
    {
        Task<bool> Exists(string name);
        Task<List<FeedbackStatusDetail>> Get(int page, int pageSize, string? search = null, string? filter = null);
        Task<int> Count(string? search = null, string? filter = null);
        Task<List<CourseFeedbackAPI>> GetFeedback(int configurationId);
        Task<bool> IsDependacyExist(int id);
        Task<ApiResponse> AddForFeedbackAggregationReport(SubmitFeedback submitFeedback, string UserName);

    }
}
