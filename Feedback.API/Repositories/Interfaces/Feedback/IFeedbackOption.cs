using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IFeedbackOption : IRepository<FeedbackOption>
    {
        bool Exists(string name);
        List<FeedbackOption> Get(int page, int pageSize, string? search = null, string? filter = null);
        int Count(string? search = null, string? filter = null);
    }
}
