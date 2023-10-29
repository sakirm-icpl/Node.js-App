using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
namespace ILT.API.Repositories
{
    public class AccessibilityRuleRejectedRepository : Repository<AccessibilityRuleRejected>, IAccessibilityRuleRejectedRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibilityRuleRejectedRepository));
        private CourseContext _db;

        public AccessibilityRuleRejectedRepository(CourseContext context) : base(context)
        {
            this._db = context;
        }
        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.AccessibilityRuleRejected.Where(r => r.CourseCode.Contains(search) || Convert.ToString(r.UserName).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.AccessibilityRuleRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public void Delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                _db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Course].[AccessibilityRuleRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
        }
        public async Task<IEnumerable<AccessibilityRuleRejected>> GetAllAccessibilityRuleReject(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Model.AccessibilityRuleRejected> Query = this._db.AccessibilityRuleRejected;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.CourseCode.StartsWith(search) || Convert.ToString(v.UserName).StartsWith(search) && v.IsDeleted == Record.NotDeleted);
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
