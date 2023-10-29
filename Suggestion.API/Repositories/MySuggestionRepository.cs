// ======================================
// <copyright file="MySuggestionRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Suggestion.API.APIModel;
using Suggestion.API.Data;
using Suggestion.API.Helper;
using Suggestion.API.Metadata;
using Suggestion.API.Models;
using Suggestion.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories
{
    public class MySuggestionRepository : Repository<MySuggestion>, IMySuggestionRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MySuggestionRepository));
        private GadgetDbContext db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        public MySuggestionRepository(GadgetDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(context)
        {
            this.db = context;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
        }
        public async Task<IEnumerable<APIMySuggestion>> GetAllSuggestions(int userId, int page, int pageSize, string search = null)
        {
            try
            {
                using (GadgetDbContext context = this.db)
                {
                    IQueryable<APIMySuggestion> result = (from mySuggestion in context.MySuggestion
                                                          join mySuggestionDetail in context.MySuggestionDetail on mySuggestion.Id equals mySuggestionDetail.SuggestionId
                                                          orderby mySuggestion.Id descending
                                                          where mySuggestion.IsDeleted == Record.NotDeleted && mySuggestion.CreatedBy == userId
                                                          select new APIMySuggestion
                                                          {
                                                              Id = mySuggestion.Id,
                                                              Date = mySuggestion.Date,
                                                              SuggestionBrief = mySuggestion.SuggestionBrief,
                                                              ContextualAreaofBusiness = mySuggestion.ContextualAreaofBusiness,
                                                              DetailedDescription = mySuggestion.DetailedDescription,
                                                              Status = mySuggestion.Status,
                                                              ContentDescription = mySuggestionDetail.ContentDescription

                                                          });
                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where(a => a.ContextualAreaofBusiness.StartsWith(search) || a.SuggestionBrief.StartsWith(search) || Convert.ToString(a.ContentDescription).StartsWith(search));
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
        //public async Task<int> Count(int userId, string search = null)
        //{
        //    if (!string.IsNullOrWhiteSpace(search))
        //        return await this.db.MySuggestion.Where(r => ((r.ContextualAreaofBusiness.Contains(search) || Convert.ToString(r.SuggestionBrief).Contains(search)) && (r.IsDeleted == Record.NotDeleted && r.CreatedBy == userId))).CountAsync();
        //    return await this.db.MySuggestion.Where(r => r.IsDeleted == Record.NotDeleted && r.CreatedBy == userId).CountAsync();
        //}


        public async Task<int> Count(int userId, string search = null)
        {
            int Count = 0;
            try
            {
                
                using (GadgetDbContext context = this.db)
                {
                    IQueryable<APIMySuggestion> result = (from mySuggestion in context.MySuggestion
                                                          join mySuggestionDetail in context.MySuggestionDetail on mySuggestion.Id equals mySuggestionDetail.SuggestionId
                                                          orderby mySuggestion.Id descending
                                                          where mySuggestion.IsDeleted == Record.NotDeleted && mySuggestion.CreatedBy == userId 
                                                          select new APIMySuggestion
                                                          {
                                                              Id = mySuggestion.Id,
                                                              Date = mySuggestion.Date,
                                                              SuggestionBrief = mySuggestion.SuggestionBrief,
                                                              ContextualAreaofBusiness = mySuggestion.ContextualAreaofBusiness,
                                                              DetailedDescription = mySuggestion.DetailedDescription,
                                                              Status = mySuggestion.Status,
                                                              ContentDescription = mySuggestionDetail.ContentDescription

                                                          });
                   
                    return await result.CountAsync();
                }
            }
            catch (Exception ex)
            {
                 _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return Count;
        }
        public async Task<bool> Exists(int UserId, DateTime Create)
        {

            var count = await this.db.MySuggestion.Where((p => (p.CreatedBy == UserId) && (p.CreatedDate.Date == Create.Date) && (p.IsDeleted == Record.NotDeleted))).CountAsync();

            if (count == 1)
                return true;
            return false;
        }
        public async Task<IEnumerable<MySuggestion>> Search(string query)
        {
            Task<List<MySuggestion>> suggestionsManagementList = (from mySuggestion in this.db.MySuggestion
                                                                  where
                                                                  (mySuggestion.SuggestionBrief.StartsWith(query) ||
                                                                 Convert.ToString(mySuggestion.ContextualAreaofBusiness).StartsWith(query)
                                                                 )
                                                                  && mySuggestion.IsDeleted == false
                                                                  select mySuggestion).ToListAsync();
            return await suggestionsManagementList;
        }
        public async Task<List<APIUpdateSuggestions>> GetAllSuggestionsData(APIUpdateSuggestions aPIMySuggestion)
        {
            List<APIUpdateSuggestions> listUserApplicability = new List<APIUpdateSuggestions>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetAllSuggestionsManagement";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = aPIMySuggestion.Id });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            APIUpdateSuggestions rule = new APIUpdateSuggestions();
                            rule.Id = Convert.ToInt32(row["Id"].ToString());
                            rule.UserName = Security.Decrypt(row["UserName"].ToString());
                            rule.Date = Convert.ToDateTime(row["Date"].ToString());
                            rule.ContextualAreaofBusiness = (row["ContextualAreaofBusiness"].ToString());
                            rule.Status = Convert.ToBoolean(row["Status"].ToString());
                            rule.ApprovalStatus = row["ApprovalStatus"].ToString();
                            rule.BriefDescription = row["DetailedDescription"].ToString();


                            listUserApplicability.Add(rule);
                        }
                    }
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                 _logger.Error( Utilities.GetDetailedException(ex));

            }
            return listUserApplicability;
        }

        public async Task<MySuggestion> GetSuggestionDetails(int id)
        {
            MySuggestion assignmentDetail = await this.db.MySuggestion.Where(Suggestion => Suggestion.Id == id).FirstOrDefaultAsync();
            return assignmentDetail;
        }

        public async Task<MySuggestionDetail> GetSuggestion(int id)
        {
            MySuggestionDetail assignmentDetail = await this.db.MySuggestionDetail.Where(Suggestion => Suggestion.SuggestionId == id).FirstOrDefaultAsync();
            return assignmentDetail;
        }

        public async Task<IEnumerable<APIMySuggestion>> GetAllMySuggestions(int page, int pageSize, string search = null)
        {
            try
            {
                using (GadgetDbContext context = this.db)
                {
                    IQueryable<APIMySuggestion> result = (from mySuggestion in context.MySuggestion
                                                          join mySuggestionDetail in context.MySuggestionDetail on mySuggestion.Id equals mySuggestionDetail.SuggestionId
                                                          orderby mySuggestion.Id descending
                                                          where mySuggestion.IsDeleted == Record.NotDeleted //&& mySuggestion.CreatedBy == userId
                                                          select new APIMySuggestion
                                                          {
                                                              Id = mySuggestion.Id,
                                                              Date = mySuggestion.Date,
                                                              SuggestionBrief = mySuggestion.SuggestionBrief,
                                                              ContextualAreaofBusiness = mySuggestion.ContextualAreaofBusiness,
                                                              DetailedDescription = mySuggestion.DetailedDescription,
                                                              Status = mySuggestion.Status,
                                                              ContentDescription = mySuggestionDetail.ContentDescription

                                                          });
                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where(a => a.ContextualAreaofBusiness.StartsWith(search) || a.SuggestionBrief.StartsWith(search) || Convert.ToString(a.ContentDescription).StartsWith(search));
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

        public async Task<string> SaveFile(IFormFile uploadedFile, string fileType, string OrganizationCode)
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string FilePath = string.Empty;
                string ReturnFilePath = string.Empty;
                string FileName = string.Empty;
                string coursesPath = this._configuration["ApiGatewayWwwroot"];

                coursesPath = Path.Combine(coursesPath, OrganizationCode, fileType);
                ReturnFilePath = Path.Combine(OrganizationCode, fileType);

                if (!Directory.Exists(coursesPath))
                {
                    Directory.CreateDirectory(coursesPath);
                }
                FileName = DateTime.Now.Ticks + uploadedFile.FileName.Trim();
                FilePath = Path.Combine(coursesPath, FileName);
                ReturnFilePath = Path.Combine(ReturnFilePath, FileName);

                FilePath = string.Concat(FilePath.Split(' '));
                ReturnFilePath = string.Concat(ReturnFilePath.Split(' '));

                using (var fs = new FileStream(Path.Combine(FilePath), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fs);
                }
                return ReturnFilePath;
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                 _logger.Error( Utilities.GetDetailedException(ex));
                return "";
            }
        }
    }
}
