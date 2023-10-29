// ======================================
// <copyright file="ArticleCategoryRepository.cs" company="Enthralltech Pvt. Ltd.">
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
    public class ArticleCategoryRepository : Repository<InterestingArticleCategory>, IArticleCategoryRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ArticleCategoryRepository));
        private GadgetDbContext db;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public ArticleCategoryRepository(GadgetDbContext context, ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this._customerConnectionString = customerConnectionString;
            this.db = context;
        }
        public async Task<IEnumerable<InterestingArticleCategory>> Search(string Category)
        {
            try
            {
                IQueryable<InterestingArticleCategory> result = (from articleCategory in this.db.InterestingArticleCategory
                                                                 where (articleCategory.Category.StartsWith(Category) && articleCategory.IsDeleted == Record.NotDeleted)
                                                                 select new InterestingArticleCategory
                                                                 {
                                                                     Category = articleCategory.Category,
                                                                     Id = articleCategory.Id

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
        public async Task<IEnumerable<InterestingArticleCategory>> GetCategoryCreatedBy(string Category, int userid)
        {
            try
            {
                IQueryable<InterestingArticleCategory> result = (from articleCategory in this.db.InterestingArticleCategory
                                                                 where (articleCategory.Category.StartsWith(Category) && articleCategory.IsDeleted == Record.NotDeleted)
                                                                 select new InterestingArticleCategory
                                                                 {
                                                                     Category = articleCategory.Category,
                                                                     Id = articleCategory.Id

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
        public async Task<int> GetIdIfExist(string category)
        {
            IQueryable<int> result = (from articleCategory in this.db.InterestingArticleCategory
                                      where (articleCategory.IsDeleted == Record.NotDeleted && (articleCategory.Category.ToLower() == category.ToLower()))
                                      select articleCategory.Id);
            if (result.Count() != 0)
                return await result.FirstAsync();

            return 0;
        }

        public async Task<int> GetLastInsertedId()
        {
            IQueryable<int> result = (from articleCategory in this.db.InterestingArticleCategory
                                      orderby articleCategory.Id descending
                                      select articleCategory.Id);
            if (result != null)
                return await result.CountAsync();
            return 0;
        }

        public async Task<IEnumerable<APIInterestingArticleCategory>> GetArticles(int userid)
        {
            try
            {
                APIInterestingArticleCategory articleCategory = new APIInterestingArticleCategory();
                List<APIInterestingArticleCategory> articleCategories = new List<APIInterestingArticleCategory>();
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetInterestingArticleCategory";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.Int) { Value = userid });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                articleCategory = new APIInterestingArticleCategory
                                {
                                    Id = Convert.ToInt32(row["ID"].ToString()),
                                    Category = row["Category"].ToString(),
                                };
                                articleCategories.Add(articleCategory);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
                return articleCategories;
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
