using Feedback.API.Model;
using Feedback.API.Models;
using Feedback.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Feedback.API.Repositories
{
    public class FeedbackSheetConfigurationDetailsRepository : Repository<FeedbackSheetConfigurationDetails>, IFeedbackSheetConfigurationDetails
    {
        private FeedbackContext _db;

        public FeedbackSheetConfigurationDetailsRepository(FeedbackContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<int> DeleteConfiguredQuestions(int[] QuestionsId, int configId)
        {
            List<FeedbackSheetConfigurationDetails> Questions = new List<FeedbackSheetConfigurationDetails>();
            foreach (int QuestionId in QuestionsId)
            {
                FeedbackSheetConfigurationDetails Question = await this._db.FeedbackSheetConfigurationDetails.Where(c => c.FeedbackId == QuestionId && c.ConfigurationSheetId == configId).FirstOrDefaultAsync();
                if (Question != null)
                {
                    Question.IsDeleted = true;
                    Questions.Add(Question);
                }
            }
            if (Questions.Count > 0)
                this.RemoveRange(Questions);
            return 1;
        }

        public async Task<int> DeleteQuestionsByConfigId(int configId)
        {
            List<FeedbackSheetConfigurationDetails> Questions = await this._db.FeedbackSheetConfigurationDetails.Where(c => c.ConfigurationSheetId == configId).ToListAsync();
            this._db.FeedbackSheetConfigurationDetails.RemoveRange(Questions);
            return 1;
        }
    }
}
