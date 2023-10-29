// ======================================
// <copyright file="NewsUpdatesRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class NewsUpdatesRepository : Repository<NewsUpdates>, INewsUpdatesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NewsUpdatesRepository));
        private GadgetDbContext db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        ITokensRepository _tokensRepository;
        public NewsUpdatesRepository(GadgetDbContext context,
            ICustomerConnectionStringRepository customerConnectionString, ITokensRepository tokensRepository) : base(context)
        {
            this.db = context;
            this._customerConnectionString = customerConnectionString;
            this._tokensRepository = tokensRepository;
        }
        public async Task<IEnumerable<NewsUpdates>> GetAllApplicableNews(int userId)
        {
            try
            {

                var BusinessFilter_OnSocial = await _tokensRepository.GetMasterConfigurableParameterValue("BusinessFilter_OnSocial");
                _logger.Info("BusinessFilter_OnSocial : " + BusinessFilter_OnSocial);

                if (Convert.ToString(string.IsNullOrEmpty(BusinessFilter_OnSocial) ? "no" : BusinessFilter_OnSocial).ToLower() == "no")
                {
                    IQueryable<Models.NewsUpdates> Query = this.db.NewsUpdates.Where(v => v.IsDeleted == Record.NotDeleted);
                    DateTime TodaysDate = DateTime.UtcNow;
                    Query = Query.Where(n => n.PublishDate.Date <= TodaysDate.Date && n.ValidityDate.Date >= TodaysDate.Date);
                    Query = Query.OrderByDescending(v => v.Id);
                    return await Query.AsNoTracking().ToListAsync();
                }
                else
                {
                    UserMasterDetails userdetails = await db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
                    IQueryable<NewsUpdates> Query = (from news in this.db.NewsUpdates
                                                     join businessdetails in this.db.UserMasterDetails on news.CreatedBy equals businessdetails.UserMasterId

                                                     where (news.IsDeleted == Record.NotDeleted)
                                                      && (userdetails.BusinessId == businessdetails.BusinessId)
                                                     select news);


                   DateTime TodaysDate = DateTime.UtcNow;
                    Query = Query.Where(n => n.PublishDate.Date <= TodaysDate.Date && n.ValidityDate.Date >= TodaysDate.Date);
                    Query = Query.OrderByDescending(v => v.Id);
                    return await Query.AsNoTracking().ToListAsync();

                }
            }


            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<IEnumerable<NewsUpdates>> GetAllNewsUpdates(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.NewsUpdates> Query = this.db.NewsUpdates;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => (v.Headline.Contains(search) || Convert.ToString(v.SubHead).Contains(search)) && (v.IsDeleted == Record.NotDeleted));
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
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.NewsUpdates.Where(r => (r.Headline.Contains(search) || Convert.ToString(r.SubHead).Contains(search)) && (r.IsDeleted == Record.NotDeleted)).CountAsync();
            return await this.db.NewsUpdates.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
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
        public async Task<int> GetCount()
        {
            return await this.db.NewsUpdates.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<IEnumerable<NewsUpdates>> Search(string query)
        {
            DateTime TodaysDate = DateTime.UtcNow;
            Task<List<NewsUpdates>> newsList = (from newsUpdates in this.db.NewsUpdates
                                                where
                                                (newsUpdates.Headline.Contains(query) || Convert.ToString(newsUpdates.SubHead).Contains(query))
                                                && (newsUpdates.PublishDate.Date <= TodaysDate.Date && newsUpdates.ValidityDate.Date >= TodaysDate.Date)
                                                  && (newsUpdates.IsDeleted == false)
                                                select newsUpdates).ToListAsync();
            return await newsList;
        }


    }
}
