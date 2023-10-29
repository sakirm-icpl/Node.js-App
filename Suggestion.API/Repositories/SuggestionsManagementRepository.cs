// ======================================
// <copyright file="SuggestionsManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Suggestion.API.APIModel;
using Suggestion.API.Data;
using Suggestion.API.Helper;
using Suggestion.API.Models;
using Suggestion.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using log4net;
using System.Threading.Tasks;


namespace Suggestion.API.Repositories
{
    public class SuggestionsManagementRepository : Repository<SuggestionsManagement>, ISuggestionsManagementRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SuggestionsManagementRepository));
        private GadgetDbContext db;
        public SuggestionsManagementRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
        public async Task<List<APISuggestionsManagement>> GetAllSuggestionsManagement(int page, int pageSize,string CreatedBy, string search = null)
        {
            try
            {
                var suggestionsManagementList = (from suggestionsManagement in this.db.SuggestionsManagement
                                                 where suggestionsManagement.IsDeleted==false
                                                 select new APISuggestionsManagement
                                                 {
                                                     UserName=CreatedBy,
                                                     ContextualAreaofBusiness=suggestionsManagement.ContextualAreaofBusiness,
                                                     BriefResponse=suggestionsManagement.BriefResponse,
                                                     Date=suggestionsManagement.Date,
                                                     Suggestion=suggestionsManagement.Suggestion,
                                                     SuggestionDate=suggestionsManagement.SuggestionDate,

                                                 }).ToListAsync();

                return await suggestionsManagementList;
                //if (page != -1)
                //{
                //    listUserApplicability = listUserApplicability.Skip((page - 1) * pageSize);
                //    listUserApplicability = listUserApplicability.OrderByDescending(v => v.Id);
                //}
                //if (pageSize != -1)
                //{
                //    Query = Query.Take(pageSize);
                //    Query = Query.OrderByDescending(v => v.Id);
                //}
                //return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<List<APISuggestionsManagement>> GetUserNameById(int Page, int PageSize, string search = null)
        {
            List<APISuggestionsManagement> listUserApplicability = new List<APISuggestionsManagement>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetUserNameByID";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.BigInt) { Value = Page });
                    cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.BigInt) { Value = PageSize });
                    cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            APISuggestionsManagement rule = new APISuggestionsManagement();
                            rule.Id = Convert.ToInt32(row["Id"].ToString());
                            rule.UserName = row["UserName"].ToString();
                            rule.ContextualAreaofBusiness = row["ContextualAreaofBusiness"].ToString();
                            rule.BriefResponse = row["SuggestionBrief"].ToString();
                            rule.Date = Convert.ToDateTime(row["Date"].ToString());
                            rule.Status = Convert.ToBoolean(row["Status"].ToString()) == false ? true : Convert.ToBoolean(row["Status"].ToString());
                            rule.FilePath = row["FilePath"].ToString();
                            rule.ApprovalStatus = row["ApprovalStatus"].ToString();
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


        public async Task<int> Count(string search = null)
        {
            int Count = 0;

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetUserNameByIDCount";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                   
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count <= 0)
                    {
                        reader.Dispose();
                        connection.Close();
                        return 0;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        Count = int.Parse(row["COUNT"].ToString());
                    }
                    reader.Dispose();

                }
                connection.Close();


            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }
        public async Task<List<APISuggestionsManagement>> GetAllSuggestions(int Id)
        {
            List<APISuggestionsManagement> listUserApplicability = new List<APISuggestionsManagement>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetALLSuggestions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = Id });
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            APISuggestionsManagement rule = new APISuggestionsManagement();
                            rule.Id = Convert.ToInt32(row["Id"].ToString());
                            rule.UserName = Security.Decrypt(row["UserName"].ToString());
                            rule.ContextualAreaofBusiness = Security.Decrypt(row["ContextualAreaofBusiness"].ToString());
                            rule.BriefResponse = string.IsNullOrEmpty(row["SuggestionBrief"].ToString())?"-": row["SuggestionBrief"].ToString();
                            //string.IsNullOrEmpty(row["location"].ToString()) ? null : row["location"].ToString()
                            rule.Date = Convert.ToDateTime(row["Date"].ToString());
                            rule.FilePath = row["FilePath"].ToString();
                            rule.FileType = row["FileType"].ToString();
                            rule.Status = Convert.ToBoolean(row["Status"].ToString()) == false ? true : Convert.ToBoolean(row["Status"].ToString());
                            rule.ApprovalStatus = row["ApprovalStatus"].ToString();
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

        public async Task<int> GetAllSuggestionsCount(int Id,string search=null,string searchText=null)
        {
            int Count = 0;
            
                var connection = this.db.Database.GetDbConnection();
                try
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetALLSuggestionsCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                           cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = Id });
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return 0;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                Count = int.Parse(row["COUNT"].ToString());
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    
                
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }
        //public async Task<int> Count(string search = null)
        //{
        //    if (!string.IsNullOrWhiteSpace(search))
        //        return await this.db.SuggestionsManagement.Where(r => r.Suggestion.Contains(search) || Convert.ToString(r.BriefResponse).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
        //    return await this.db.SuggestionsManagement.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        //}
        public async Task<IEnumerable<SuggestionsManagement>> Search(string query)
        {
            Task<List<SuggestionsManagement>> suggestionsManagementList = (from suggestionsManagement in this.db.SuggestionsManagement
                                                                           where
                                                                           (suggestionsManagement.Suggestion.StartsWith(query) ||
                                                                          Convert.ToString(suggestionsManagement.BriefResponse).StartsWith(query)
                                                                          )
                                                                           && suggestionsManagement.IsDeleted == false
                                                                           select suggestionsManagement).ToListAsync();
            return await suggestionsManagementList;
        }
        public async Task<SuggestionsManagement> GetSuggestionDetail(int id)
        {
            SuggestionsManagement assignmentDetail = await this.db.SuggestionsManagement.Where(Suggestion => Suggestion.Id == id).FirstOrDefaultAsync();
            return assignmentDetail;
        }
    }
}
