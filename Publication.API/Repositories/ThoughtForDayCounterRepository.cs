// ======================================
// <copyright file="ThoughtForDayCounterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Data;
using Publication.API.Helper;
using Publication.API.Models;
using Publication.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace Publication.API.Repositories
{
    public class ThoughtForDayCounterRepository : Repository<ThoughtForDayCounter>, IThoughtForDayCounterRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ThoughtForDayCounterRepository));
        private GadgetDbContext db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public ThoughtForDayCounterRepository(GadgetDbContext context,
            ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this._customerConnectionString = customerConnectionString;
            this.db = context;
        }
        public async Task<IEnumerable<ThoughtForDayCounter>> GetAllThoughtForDayCounter(int page, int pageSize, string search = null)
        {
            try
            {

                using (GadgetDbContext context = this.db)
                {
                    IQueryable<ThoughtForDayCounter> result = (from thoughtForDayCounter in context.ThoughtForDayCounter
                                                               where thoughtForDayCounter.IsDeleted == Record.NotDeleted
                                                               select new ThoughtForDayCounter
                                                               {
                                                                   Id = thoughtForDayCounter.Id,
                                                                   Date = thoughtForDayCounter.Date,
                                                                   UserAction = thoughtForDayCounter.UserAction,
                                                                   UserId = thoughtForDayCounter.UserId


                                                               });
                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where(a => Convert.ToString(a.UserId).StartsWith(search));
                    }
                    if (page != -1)
                        result = result.Skip((page - 1) * pageSize);

                    if (pageSize != -1)
                        result = result.Take(pageSize);

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;


        }

        public async Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId)
        {
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "RewardPointsDaily_Upsert";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FunctionCode", SqlDbType.NVarChar) { Value = functionCode });
                        cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar) { Value = category });
                        cmd.Parameters.Add(new SqlParameter("@ReferenceId", SqlDbType.Int) { Value = referenceId });
                        cmd.Parameters.Add(new SqlParameter("@Point", SqlDbType.Int) { Value = point });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        reader.Dispose();
                    }
                        await dbContext.Database.CloseConnectionAsync();
                }
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
                return await this.db.ThoughtForDayCounter.Where(r => Convert.ToString(r.UserId).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.ThoughtForDayCounter.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

    }
}
