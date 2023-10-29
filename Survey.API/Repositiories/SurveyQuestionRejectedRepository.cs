using Survey.API.Data;
using Survey.API.Helper;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace Survey.API.Repositories
{
    public class SurveyQuestionRejectedRepository : Repository<SurveyQuestionRejected>, ISurveyQuestionRejectedRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SurveyQuestionRejectedRepository));
        private GadgetDbContext db;
        public SurveyQuestionRejectedRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {

            this.db = context;
        }
        public void Delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Gadget].[SurveyQuestionRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }

        }
        public async Task<IEnumerable<SurveyQuestionRejected>> GetAllSurveyQuestionReject(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<SurveyQuestionRejected> Query = this.db.SurveyQuestionRejected;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.Question.StartsWith(search) && v.IsDeleted == Record.NotDeleted);
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
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.SurveyQuestionRejected.Where(r => r.Question.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.SurveyQuestionRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public bool QuestionExists(string question)
        {
            if (this.db.SurveyQuestionRejected.Count(x => x.Question == question && x.IsDeleted == false) > 0)
                return true;
            return false;
        }

    }
}
