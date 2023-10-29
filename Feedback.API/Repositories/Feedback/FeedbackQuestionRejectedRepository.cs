using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using log4net;
using Feedback.API.Models;
using Feedback.API.Helper;

namespace Feedback.API.Repositories
{
    public class FeedbackQuestionRejectedRepository : Repository<FeedbackQuestionRejected>, IFeedbackQuestionRejectedRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedbackQuestionRejectedRepository));
        private FeedbackContext _db;
        public FeedbackQuestionRejectedRepository(FeedbackContext context) : base(context)
        {
            _db = context;
        }
        public void Delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                _db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Course].[FeedbackQuestionRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }

        }

        public async Task<IEnumerable<FeedbackQuestionRejected>> GetAllFeedbackQuestionReject(int page, int pageSize, string? search = null)
        {
            try
            {
                IQueryable<FeedbackQuestionRejected> Query = this._db.FeedbackQuestionRejected;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.QuestionText.StartsWith(search) || Convert.ToString(v.Section).StartsWith(search) && v.IsDeleted == Record.NotDeleted);
                    Query = Query.OrderByDescending(v => v.Id);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                }
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> Count(string? search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.FeedbackQuestionRejected.Where(r => r.QuestionText.Contains(search) || Convert.ToString(r.Section).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.FeedbackQuestionRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
    }

}
