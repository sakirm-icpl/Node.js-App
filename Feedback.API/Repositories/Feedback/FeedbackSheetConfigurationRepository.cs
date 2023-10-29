using Feedback.API.APIModel;
using Feedback.API.Model;
using Feedback.API.Models;
using Feedback.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Feedback.API.Repositories
{
    public class FeedbackSheetConfigurationRepository : Repository<FeedbackSheetConfiguration>, IFeedbackSheetConfiguration
    {
        private FeedbackContext _db;

        public FeedbackSheetConfigurationRepository(FeedbackContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<APIFeedbackConfiguration>> GetEditFeedbackConfigurationID(int ConfigureId)
        {
            List<APIFeedbackConfiguration> feedbackconfigAPI = new List<APIFeedbackConfiguration>();
            var result = (from feedbackConfigDetails in _db.FeedbackSheetConfigurationDetails
                          join feedbackQuestions in _db.FeedbackQuestion on feedbackConfigDetails.FeedbackId equals feedbackQuestions.Id
                          where feedbackQuestions.IsDeleted == false && feedbackConfigDetails.IsDeleted == false
                                && feedbackConfigDetails.ConfigurationSheetId == ConfigureId
                          orderby feedbackConfigDetails.SequenceNumber ascending
                          select new APIFeedbackConfiguration
                          {
                              QuestionText = feedbackQuestions.QuestionText,
                              QuestionType = feedbackQuestions.QuestionType,
                              FeedbackId = feedbackQuestions.Id
                          });
            feedbackconfigAPI = await result.ToListAsync();
            if (feedbackconfigAPI.Count > 0)
                return feedbackconfigAPI;
            return null;
        }
    }
}
