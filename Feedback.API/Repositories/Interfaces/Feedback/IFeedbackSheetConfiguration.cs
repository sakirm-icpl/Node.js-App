using Feedback.API.APIModel;
using Feedback.API.Model;

namespace Feedback.API.Repositories.Interfaces
{
    public interface IFeedbackSheetConfiguration : IRepository<FeedbackSheetConfiguration>
    {
        Task<IEnumerable<APIFeedbackConfiguration>> GetEditFeedbackConfigurationID(int ConfigureId);
    }
}
