using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using log4net;
using Assessment.API.Helper;

namespace Assessment.API.Repositories
{
    public class AssessmentQuestionRejectedRepository : Repository<AssessmentQuestionRejected>, IAssessmentQuestionRejectedRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentQuestionRejectedRepository));
        private AssessmentContext _db;

        public AssessmentQuestionRejectedRepository(AssessmentContext context) : base(context)
        {
            this._db = context;
        }
        public void Delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                _db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Course].[AssessmentQuestionRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }

        }
        public async Task<IEnumerable<AssessmentQuestionRejected>> GetAllAssessmentsQuestionReject(int page, int pageSize, string? search = null)
        {
            try
            {
                IQueryable<Models.AssessmentQuestionRejected> Query = this._db.AssessmentQuestionRejected;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.QuestionText.Contains(search) && v.IsDeleted == Record.NotDeleted);
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
                    Query = Query.OrderByDescending(v => v.Id);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                    Query = Query.OrderByDescending(v => v.Id);
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

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.AssessmentQuestionRejected.Where(r => r.QuestionText.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.AssessmentQuestionRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
    }
}
