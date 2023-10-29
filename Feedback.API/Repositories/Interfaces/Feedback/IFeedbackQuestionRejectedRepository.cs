using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IFeedbackQuestionRejectedRepository : IRepository<FeedbackQuestionRejected>
    {
        void Delete();
        Task<IEnumerable<FeedbackQuestionRejected>> GetAllFeedbackQuestionReject(int page, int pageSize, string? search = null);
        Task<int> Count(string? search = null);
    }

}
