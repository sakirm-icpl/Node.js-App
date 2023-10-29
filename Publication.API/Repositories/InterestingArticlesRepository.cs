// ======================================
// <copyright file="InterestingArticlesRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.APIModel;
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
using System.Threading.Tasks;
using log4net;

namespace Publication.API.Repositories
{
    public class InterestingArticlesRepository : Repository<InterestingArticles>, IInterestingArticlesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InterestingArticlesRepository));
        private GadgetDbContext db;
        private IArticleCategoryRepository _articleCategoryRepository;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private readonly ITokensRepository _tokensRepository;
        public InterestingArticlesRepository(GadgetDbContext context,
            IArticleCategoryRepository articleCategoryRepository,
             ITokensRepository tokensRepository,
            ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.db = context;
            this._customerConnectionString = customerConnectionString;
            this._articleCategoryRepository = articleCategoryRepository;
            this._tokensRepository = tokensRepository;
        }
        public async Task<IEnumerable<InterestingArticles>> GetAllInterestingArticles(int UserId, string UserRole, int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.InterestingArticles> Query = this.db.InterestingArticles;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => (v.Article.StartsWith(search) || v.Category.StartsWith(search)) && v.IsDeleted == Record.NotDeleted);
                }
                else
                {
                    Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                }
                //Query = Query.Where(v => v.CreatedBy == UserId);
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
        public async Task<int> Count(int UserId, string UserRole, string search = null)
        {

            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.InterestingArticles.Where(r => (r.Article.StartsWith(search) || r.Category.StartsWith(search)) && r.IsDeleted == Record.NotDeleted).CountAsync();//&& r.CreatedBy == UserId
            return await this.db.InterestingArticles.Where(r => r.IsDeleted == Record.NotDeleted ).CountAsync();//&& r.CreatedBy == UserId
        }
        public async Task<bool> Exist(string search)
        {
            int count = await this.db.InterestingArticles.Where(p => (p.Article.ToLower() == search.ToLower()) && p.IsDeleted == false).CountAsync();

            if (count > 0)
                return true;
            return false;
        }


        public async Task<InterestingArticles> GetInterestingArticlesObject(InterestingArticles interestingArticles, APIInterestingArticles aPIInterestingArticles, int UserId)
        {
            int categoryId = await this._articleCategoryRepository.GetIdIfExist(aPIInterestingArticles.Category);
            if (categoryId != 0)
                interestingArticles.CategoryId = categoryId;
            else if (!string.IsNullOrEmpty(aPIInterestingArticles.Category))
            {
                InterestingArticleCategory articleCategory = new InterestingArticleCategory
                {
                    Category = aPIInterestingArticles.Category,
                    ShowToAll = aPIInterestingArticles.ShowToAll,
                    CreatedBy = UserId,
                    CreatedDate = DateTime.Now,
                    ModifiedBy = UserId,
                    ModifiedDate = DateTime.Now
                };
                //articleCategory.CreatedBy = aPIInterestingArticles.CreatedBy;
                //articleCategory.ModifiedBy = aPIInterestingArticles.CreatedBy;
                await this._articleCategoryRepository.Add(articleCategory);
                int cid = await this._articleCategoryRepository.GetLastInsertedId();
                interestingArticles.CategoryId = cid;
            }
            return interestingArticles;
        }
        public async Task<IEnumerable<InterestingArticles>> Search(string query)
        {
            Task<List<InterestingArticles>> interestingArticlesList = (from interestingArticles in this.db.InterestingArticles
                                                                       where
                                                                       (interestingArticles.Article.StartsWith(query) ||
                                                                      Convert.ToString(interestingArticles.ArticleDescription).StartsWith(query) ||
                                                                      Convert.ToString(interestingArticles.Category).StartsWith(query)
                                                                      )
                                                                       && interestingArticles.IsDeleted == false
                                                                       select interestingArticles).ToListAsync();
            return await interestingArticlesList;
        }
        public async Task<IEnumerable<InterestingArticles>> GetAllInterestingArticlesByCategoryId(int id, int userId)
        {
            
            DateTime datecurrent = DateTime.UtcNow;
            string date = datecurrent.ToString("dd-MM-yyyy");
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(date, format, null);

            try
            {
               
                var BusinessFilter_OnSocial = await _tokensRepository.GetMasterConfigurableParameterValue("BusinessFilter_OnSocial");
                _logger.Info("BusinessFilter_OnSocial : " + BusinessFilter_OnSocial);

                if (Convert.ToString(string.IsNullOrEmpty(BusinessFilter_OnSocial) ? "no" : BusinessFilter_OnSocial).ToLower() == "no")
                {
                    IQueryable<InterestingArticles> result = (from interestingArticles in this.db.InterestingArticles
                                                              where (interestingArticles.CategoryId == id && interestingArticles.IsDeleted == Record.NotDeleted && interestingArticles.Status == true && ((DateTime.Compare(interestingArticles.ValidityDate.Date, parsedDate.Date) >= 0)))
                                                              select new InterestingArticles
                                                              {
                                                                  Date = interestingArticles.Date,
                                                                  Id = interestingArticles.Id,
                                                                  CategoryId = interestingArticles.CategoryId,
                                                                  Category = interestingArticles.Category,
                                                                  Article = interestingArticles.Article,
                                                                  ShowToAll = interestingArticles.ShowToAll,
                                                                  ArticleDescription = interestingArticles.ArticleDescription,
                                                                  ValidityDate = interestingArticles.ValidityDate,
                                                                  Status = interestingArticles.Status
                                                              });
                    return await result.AsNoTracking().ToListAsync();
                }
                else
                {
                    UserMasterDetails userdetails = await db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
                    IQueryable<InterestingArticles> result = (from interestingArticles in this.db.InterestingArticles
                                                                 join businessdetails in this.db.UserMasterDetails on interestingArticles.CreatedBy equals businessdetails.UserMasterId 

                                                              where (interestingArticles.CategoryId == id && interestingArticles.IsDeleted == Record.NotDeleted && interestingArticles.Status == true && ((DateTime.Compare(interestingArticles.ValidityDate.Date, parsedDate.Date) >= 0)) 
                                                              && (userdetails.BusinessId== businessdetails.BusinessId) || interestingArticles.ShowToAll==true)
                                                              
                                                              select new InterestingArticles
                                                              {                                                                  
                                                                  Date = interestingArticles.Date,
                                                                  Id = interestingArticles.Id,
                                                                  CategoryId = interestingArticles.CategoryId,
                                                                  Category = interestingArticles.Category,
                                                                  Article = interestingArticles.Article,
                                                                  ShowToAll = interestingArticles.ShowToAll,
                                                                  ArticleDescription = interestingArticles.ArticleDescription,
                                                                  ValidityDate = interestingArticles.ValidityDate,
                                                                  Status = interestingArticles.Status
                                                              });
                    return await result.AsNoTracking().ToListAsync();

                }
               
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<IEnumerable<InterestingArticles>> GetAllInterestingArticlesBySearch(int id, string search)
        {
            DateTime datecurrent = DateTime.UtcNow;
            string date = datecurrent.ToString("dd-MM-yyyy");
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(date, format, null);

            try
            {
                IQueryable<InterestingArticles> result = (from interestingArticles in this.db.InterestingArticles
                                                          where (interestingArticles.Article.StartsWith(search) && interestingArticles.CategoryId == id && interestingArticles.IsDeleted == Record.NotDeleted && interestingArticles.Status == true && ((DateTime.Compare(interestingArticles.ValidityDate.Date, parsedDate.Date) >= 0)))
                                                          select new InterestingArticles
                                                          {
                                                              Date = interestingArticles.Date,
                                                              Id = interestingArticles.Id,
                                                              CategoryId = interestingArticles.CategoryId,
                                                              Category = interestingArticles.Category,
                                                              Article = interestingArticles.Article,
                                                              ShowToAll = interestingArticles.ShowToAll,
                                                              ArticleDescription = interestingArticles.ArticleDescription,
                                                              ValidityDate = interestingArticles.ValidityDate,
                                                              Status = interestingArticles.Status
                                                          });
                return await result.AsNoTracking().ToListAsync();
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
