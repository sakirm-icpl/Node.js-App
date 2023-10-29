using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Assessment.API.Helper;
using Assessment.API.Models;
using Assessment.API.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Assessment.API.Helper;

namespace Assessment.API.Repositories
{
    public class AssessmentConfigurationRepositories : Repository<AssessmentConfiguration>, IAssessmentConfiguration
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentConfigurationRepositories));
        private AssessmentContext _db;
        public AssessmentConfigurationRepositories(AssessmentContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<IEnumerable<AssessmentConfiguration>> GetAllAssessmentConfiguration(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.AssessmentConfiguration> Query = this._db.AssessmentConfiguration;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
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

    }
}
