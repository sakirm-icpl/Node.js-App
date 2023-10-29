// ======================================
// <copyright file="AnnouncementsRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class PublicationsRepository : Repository<Publications>, IPublicationsRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PublicationsRepository));
        private GadgetDbContext db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private readonly ITokensRepository _tokensRepository;
        public PublicationsRepository(GadgetDbContext context,
            ICustomerConnectionStringRepository customerConnectionString,  ITokensRepository tokensRepository) : base(context)
        {
            this.db = context;
            this._customerConnectionString = customerConnectionString;
            this._tokensRepository = tokensRepository;
        }
        public async Task<IEnumerable<Publications>> GetAllPublications(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.Publications> Query = this.db.Publications;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => (v.Publication.Contains(search) || Convert.ToString(v.AverageRating).Contains(search)) && (v.IsDeleted == Record.NotDeleted));
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
                return await this.db.Publications.Where(r => (r.Publication.Contains(search) || Convert.ToString(r.AverageRating).Contains(search)) && (r.IsDeleted == Record.NotDeleted)).CountAsync();
            return await this.db.Publications.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public bool Exist(string search)
        {
            if (this.db.Publications.Count(x => x.Publication == search) > 0)
                return true;
            return false;
        }
        //public async Task<IEnumerable<Publications>> Search(string query)
        //{
        //    var publicationsList = (from publications in this.db.Publications
        //                             where
        //                             (publications.Publication.StartsWith(query) ||
        //                            Convert.ToString(publications.AverageRating).StartsWith(query)
        //                            )
        //                             && publications.IsDeleted == false
        //                             select publications).ToListAsync();
        //    return await publicationsList;
        //}

        public async Task<int> GetTotalPublicationCount()
        {
            return await this.db.Publications.CountAsync();
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
        public async Task<IEnumerable<Publications>> SearchTitle(string Category)
        {
            try
            {
                IQueryable<Publications> result = (from publications in this.db.Publications
                                                   where (publications.Publication.StartsWith(Category) && publications.IsDeleted == Record.NotDeleted)
                                                   select new Publications
                                                   {
                                                       Publication = publications.Publication,
                                                       Id = publications.Id,
                                                       VolumeNumber = publications.VolumeNumber

                                                   } into t1
                                                   group t1 by t1.Publication into pub
                                                   select pub.FirstOrDefault());
                return await result.AsNoTracking().Distinct().ToListAsync();
            }
            catch (Exception ex)
            {
                 _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<int> ReadToady(int pubId)
        {
            DateTime datecurrent = DateTime.UtcNow;
            string date = datecurrent.ToString("dd-MM-yyyy");
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(date, format, null);
            try
            {
                return await this.db.Publications.Where(r => (r.IsDeleted == Record.NotDeleted && r.Id == pubId && (DateTime.Compare(r.PublishedDate.Date, parsedDate.Date) == 0))).CountAsync();

            }
            catch (Exception ex)
            {
                 _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return 0;
        }
        public async Task<IEnumerable<Publications>> GetAllPublications(int userId)
        {
            try
            {
                var BusinessFilter_OnSocial = await _tokensRepository.GetMasterConfigurableParameterValue("BusinessFilter_OnSocial");
                _logger.Info("BusinessFilter_OnSocial : " + BusinessFilter_OnSocial);

                if (Convert.ToString(string.IsNullOrEmpty(BusinessFilter_OnSocial) ? "no" : BusinessFilter_OnSocial).ToLower() == "no")
                {
                    IQueryable<Models.Publications> Query = this.db.Publications;
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted && v.PublishedDate.Date <= DateTime.Today);
                    Query = Query.OrderByDescending(v => v.ModifiedDate);                   
                    return await Query.AsNoTracking().ToListAsync();
                }
                else
                {
                    UserMasterDetails userdetails = await db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
                    IQueryable<Models.Publications> Query =  (from publication in this.db.Publications
                                                              join businessdetails in this.db.UserMasterDetails on publication.CreatedBy equals businessdetails.UserMasterId

                                                            where (publication.IsDeleted == Record.NotDeleted)
                                                            && (publication.PublishedDate.Date <= DateTime.Today)
                                                                && (userdetails.BusinessId == businessdetails.BusinessId)
                                                            select publication);

                  
                    Query = Query.OrderByDescending(v => v.ModifiedDate);


                    return await Query.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                 _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<IEnumerable<Publications>> Search(string search = null)
        {
            try
            {
                IQueryable<Models.Publications> Query = this.db.Publications;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => (v.Publication.Contains(search) || Convert.ToString(v.AverageRating).Contains(search)) && (v.IsDeleted == Record.NotDeleted));
                    Query = Query.OrderByDescending(v => v.ModifiedDate);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                Query = Query.OrderByDescending(v => v.ModifiedDate);

                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                 _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

    }
}
