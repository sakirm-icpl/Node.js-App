// ======================================
// <copyright file="PollsManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using PollManagement.API.APIModel;
using PollManagement.API.Data;
using PollManagement.API.Helper;
using PollManagement.API.Models;
using PollManagement.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using log4net;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace PollManagement.API.Repositories
{
    public class PollsManagementRepository : Repository<PollsManagement>, IPollsManagementRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PollsManagementRepository));
        private GadgetDbContext db;
        private INotification _notification;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private readonly IConfiguration _configuration;
        public PollsManagementRepository(GadgetDbContext context,
            INotification notification,
            ICustomerConnectionStringRepository customerConnectionString, IConfiguration configuration) : base(context)
        {
            this.db = context;
            this._configuration = configuration;
            this._notification = notification;
            this._customerConnectionString = customerConnectionString;
        }
        public async Task<IEnumerable<PollsManagement>> GetAllPollsManagement(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.PollsManagement> Query = this.db.PollsManagement;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.Question.Contains(search) && v.IsDeleted == Record.NotDeleted);
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
                return await this.db.PollsManagement.Where(r => r.Question.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.PollsManagement.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<bool> Exist(string search)
        {
            int count = await this.db.PollsManagement.Where(p => p.Question.ToLower() == search.ToLower()).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> Existquestion(string question, int? pollId)
        {
            int count = await this.db.PollsManagement.Where(p => p.Question.ToLower() ==  question.ToLower() && p.Id != pollId && p.IsDeleted == Record.NotDeleted).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> ExistsInResult(int pollId)
        {
            int count = await this.db.PollsResult.Where(p => (p.PollsId == pollId && p.IsDeleted == Record.NotDeleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<IEnumerable<PollsManagement>> Search(string query)
        {
            Task<List<PollsManagement>> pollsManagementList = (from pollsManagement in this.db.PollsManagement
                                                               where
                                                               (pollsManagement.Question.StartsWith(query))
                                                               && pollsManagement.IsDeleted == false
                                                               select pollsManagement).ToListAsync();
            return await pollsManagementList;
        }

        public async Task<IEnumerable<APIPollsManagement>> GetAllPollsManagement(int userId)
        {
            DbConnection connection = this.db.Database.GetDbConnection();//Dont use using statment for connection
            List<APIPollsManagement> pollList = new List<APIPollsManagement>();
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetOpinionPolls";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                APIPollsManagement poll = new APIPollsManagement
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["StartDate"].ToString()),
                                    ValidityDate = string.IsNullOrEmpty(row["ValidityDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["ValidityDate"].ToString()),
                                    TargetResponseCount = Convert.ToInt32(row["TargetResponseCount"].ToString()),
                                    Question = row["Question"].ToString(),
                                    Option1 = row["Option1"].ToString(),
                                    Option2 = row["Option2"].ToString(),
                                    Option3 = row["Option3"].ToString(),
                                    Option4 = row["Option4"].ToString(),
                                    Option5 = row["Option5"].ToString(),
                                    Status = Convert.ToBoolean(row["Status"].ToString()),
                                    CompletionStatus = row["CompletionStatus"].ToString(),
                                    ValidTargetResponseCount = Convert.ToBoolean(row["ValidTargetResponseCount"].ToString())
                                };
                                pollList.Add(poll);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                    return pollList;
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> GetAllPollsManagementCount(int userId)
        {
            DbConnection connection = this.db.Database.GetDbConnection();//Dont use using statment for connection
            int Count = 0;
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetOpinionPollsCount";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
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
                        Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                    }
                    reader.Dispose();

                }
                connection.Close();
                return Count;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                throw ex;
            }
        }

        public async Task<bool> ExistPoll(int pollid, int userid)
        {
            int count = await this.db.PollsResult.Where(p => (p.PollsId == pollid) && (p.UserId == userid)).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<int> GetCount()
        {
            return await this.db.PollsManagement.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<JsonResult> GetTotal(int id, int userid)
        {
            try
            {

                int totalCount = await this.db.PollsResult.Where(p => p.PollsId == id).CountAsync();

                int option1Count = await this.db.PollsResult.Where(p => (p.PollsId == id) && (p.Option1 != null)).CountAsync();
                int option2Count = await this.db.PollsResult.Where(p => (p.PollsId == id) && (p.Option2 != null)).CountAsync();
                int option3Count = await this.db.PollsResult.Where(p => (p.PollsId == id) && (p.Option3 != null)).CountAsync();
                int option4Count = await this.db.PollsResult.Where(p => (p.PollsId == id) && (p.Option4 != null)).CountAsync();
                int option5Count = await this.db.PollsResult.Where(p => (p.PollsId == id) && (p.Option5 != null)).CountAsync();

                double count1 = Math.Round(((option1Count * 100.00) / totalCount), 2);
                double count2 = Math.Round(((option2Count * 100.00) / totalCount), 2);
                double count3 = Math.Round(((option3Count * 100.00) / totalCount), 2);
                double count4 = Math.Round(((option4Count * 100.00) / totalCount), 2);
                double count5 = Math.Round(((option5Count * 100.00) / totalCount), 2);

                PollsManagement pollsManagementList = (from pollsManagement in this.db.PollsManagement
                                                       where
                                                      pollsManagement.Id == id
                                                       && pollsManagement.IsDeleted == false
                                                       select pollsManagement).FirstOrDefault();

                string optionText1 = pollsManagementList.Option1;
                string optionText2 = pollsManagementList.Option2;
                string optionText3 = pollsManagementList.Option3;
                string optionText4 = pollsManagementList.Option4;
                string optionText5 = pollsManagementList.Option5;


                return new JsonResult(new List<object>()
               {
                 new {count1, optionText1},
                 new {count2, optionText2},
                 new {count3, optionText3},
                 new {count4, optionText4},
                 new {count5, optionText5}
                });

            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> AddPollsApplicability(int pollId, string accessibilityParameter, string parameterValue, int parameterValueId)
        {
            PollsManagementAccessibilityRule PollsAccessibilityRule = new PollsManagementAccessibilityRule
            {
                Value = parameterValue,
                PollManagementId = pollId
            };
            switch (accessibilityParameter.ToLower())
            {
                case "area":
                    PollsAccessibilityRule.Area = parameterValueId;
                    break;
                case "group":
                    PollsAccessibilityRule.Group = parameterValueId;
                    break;
                case "location":
                    PollsAccessibilityRule.Location = parameterValueId;
                    break;
                case "buisness":
                case "business":
                    PollsAccessibilityRule.Business = parameterValueId;
                    break;
                case "configurationcolumn1":
                    PollsAccessibilityRule.ConfigurationColumn1 = parameterValueId;
                    break;
                case "configurationcolumn2":
                    PollsAccessibilityRule.ConfigurationColumn2 = parameterValueId;
                    break;
                case "configurationcolumn3":
                    PollsAccessibilityRule.ConfigurationColumn3 = parameterValueId;
                    break;
                case "configurationcolumn4":
                    PollsAccessibilityRule.ConfigurationColumn4 = parameterValueId;
                    break;
                case "configurationcolumn5":
                    PollsAccessibilityRule.ConfigurationColumn5 = parameterValueId;
                    break;
                case "configurationcolumn6":
                    PollsAccessibilityRule.ConfigurationColumn6 = parameterValueId;
                    break;
                case "configurationcolumn7":
                    PollsAccessibilityRule.ConfigurationColumn7 = parameterValueId;
                    break;
                case "configurationcolumn8":
                    PollsAccessibilityRule.ConfigurationColumn8 = parameterValueId;
                    break;
                case "configurationcolumn9":
                    PollsAccessibilityRule.ConfigurationColumn9 = parameterValueId;
                    break;
                case "configurationcolumn10":
                    PollsAccessibilityRule.ConfigurationColumn10 = parameterValueId;
                    break;
                case "configurationcolumn11":
                    PollsAccessibilityRule.ConfigurationColumn11 = parameterValueId;
                    break;
                case "configurationcolumn12":
                    PollsAccessibilityRule.ConfigurationColumn12 = parameterValueId;
                    break;
                case "emailid":
                    
                    PollsAccessibilityRule.EmailId = Convert.ToString(parameterValueId);
                    break;
                case "userid":
                    PollsAccessibilityRule.UserId = parameterValueId;
                    break;
                case "mobilenumber":
                    PollsAccessibilityRule.MobileNumber = Convert.ToString(parameterValueId);
                    break;
                default:
                    return 0;
            }
            await this.db.PollsManagementAccessibilityRule.AddAsync(PollsAccessibilityRule);
            await this.db.SaveChangesAsync();
            return PollsAccessibilityRule.Id;
        }
        public async Task<int> UpdatePollsApplicability(int pollId, string accessibilityParameter, string parameterValue, int? parameterValueId)
        {
            PollsManagementAccessibilityRule OldPollsAccessibilityRule = await this.db.PollsManagementAccessibilityRule.AsNoTracking().Where(p => p.PollManagementId == pollId).FirstOrDefaultAsync();
            if (OldPollsAccessibilityRule != null)
            {
                this.db.PollsManagementAccessibilityRule.Remove(OldPollsAccessibilityRule);
                await this.db.SaveChangesAsync();
            }

            PollsManagementAccessibilityRule PollsAccessibilityRule = new PollsManagementAccessibilityRule();

            if (parameterValueId == null || accessibilityParameter == null)
                return 0;
            PollsAccessibilityRule.PollManagementId = pollId;
            PollsAccessibilityRule.Value = parameterValue;
            switch (accessibilityParameter.ToLower())
            {
                case "area":
                    PollsAccessibilityRule.Area = parameterValueId;
                    break;
                case "group":
                    PollsAccessibilityRule.Group = parameterValueId;
                    break;
                case "location":
                    PollsAccessibilityRule.Location = parameterValueId;
                    break;
                case "buisness":
                case "business":
                    PollsAccessibilityRule.Business = parameterValueId;
                    break;
                case "configurationcolumn1":
                    PollsAccessibilityRule.ConfigurationColumn1 = parameterValueId;
                    break;
                case "configurationcolumn2":
                    PollsAccessibilityRule.ConfigurationColumn2 = parameterValueId;
                    break;
                case "configurationcolumn3":
                    PollsAccessibilityRule.ConfigurationColumn3 = parameterValueId;
                    break;
                case "configurationcolumn4":
                    PollsAccessibilityRule.ConfigurationColumn4 = parameterValueId;
                    break;
                case "configurationcolumn5":
                    PollsAccessibilityRule.ConfigurationColumn5 = parameterValueId;
                    break;
                case "configurationcolumn6":
                    PollsAccessibilityRule.ConfigurationColumn6 = parameterValueId;
                    break;
                case "configurationcolumn7":
                    PollsAccessibilityRule.ConfigurationColumn7 = parameterValueId;
                    break;
                case "configurationcolumn8":
                    PollsAccessibilityRule.ConfigurationColumn8 = parameterValueId;
                    break;
                case "configurationcolumn9":
                    PollsAccessibilityRule.ConfigurationColumn9 = parameterValueId;
                    break;
                case "configurationcolumn10":
                    PollsAccessibilityRule.ConfigurationColumn10 = parameterValueId;
                    break;
                case "configurationcolumn11":
                    PollsAccessibilityRule.ConfigurationColumn11 = parameterValueId;
                    break;
                case "configurationcolumn12":
                    PollsAccessibilityRule.ConfigurationColumn12 = parameterValueId;
                    break;
                case "emailid":
                    PollsAccessibilityRule.EmailId = parameterValue.Encrypt();
                    break;
                case "userid":
                    PollsAccessibilityRule.UserId = parameterValueId;
                    break;
                case "mobilenumber":
                    PollsAccessibilityRule.MobileNumber = parameterValue.Encrypt();
                    break;
            }
            this.db.PollsManagementAccessibilityRule.Update(PollsAccessibilityRule);
            await this.db.SaveChangesAsync();
            return PollsAccessibilityRule.Id;
        }
        public async Task<APIPollsManagement> GetPollManagement(int pollId)
        {
            var Result = await (from pollsManagement in this.db.PollsManagement
                                join pollAccessibilityRule in this.db.PollsManagementAccessibilityRule on pollsManagement.Id equals pollAccessibilityRule.PollManagementId
                                into pollAccessibilityRuleTemp
                                from pollAccessibilityRule in pollAccessibilityRuleTemp.DefaultIfEmpty()
                                where pollsManagement.Id == pollId
                                select new { pollsManagement, pollAccessibilityRule }).FirstOrDefaultAsync();
            if (Result == null)
                return null;

            APIPollsManagement APIPollsManagementObj = new APIPollsManagement
            {
                Id = Result.pollsManagement.Id,
                StartDate = Result.pollsManagement.StartDate,
                ValidityDate = Result.pollsManagement.ValidityDate,
                TargetResponseCount = Result.pollsManagement.TargetResponseCount,
                Question = Result.pollsManagement.Question,
                Option1 = Result.pollsManagement.Option1,
                Option2 = Result.pollsManagement.Option2,
                Option3 = Result.pollsManagement.Option3,
                Option4 = Result.pollsManagement.Option4,
                Option5 = Result.pollsManagement.Option5,
                Status = Result.pollsManagement.Status
            };

            if (Result.pollAccessibilityRule != null)
            {
                //Get All coumns of PollAccessibilityRule
                PropertyInfo[] columns = Result.pollAccessibilityRule.GetType().GetProperties();
                foreach (PropertyInfo column in columns)
                {

                    string columnName = column.Name.ToLower();
                    if (columnName == "emailid")
                    {
                        string value = column.GetValue(Result.pollAccessibilityRule) == null ? null :
                            column.GetValue(Result.pollAccessibilityRule).ToString();
                        if (value != null)
                        {
                            APIPollsManagementObj.ApplicabilityParameter = column.Name;
                            APIPollsManagementObj.ParameterValue = column.GetValue(Result.pollAccessibilityRule) == null ? null :
                                column.GetValue(Result.pollAccessibilityRule).ToString().Decrypt();
                        }
                    }
                    else
                    if (columnName == "mobilenumber")
                    {
                        string value = column.GetValue(Result.pollAccessibilityRule) == null ? null :
                            column.GetValue(Result.pollAccessibilityRule).ToString();
                        if (value != null)
                        {
                            APIPollsManagementObj.ApplicabilityParameter = column.Name;
                            APIPollsManagementObj.ParameterValue = column.GetValue(Result.pollAccessibilityRule) == null ? null :
                                column.GetValue(Result.pollAccessibilityRule).ToString().Decrypt();
                        }
                    }
                    else

                    if ((columnName != "id")  &&  (columnName != "pollmanagementid")  && (columnName !="value"))
                    {
                        //Get value of column
                        int? value = column.GetValue(Result.pollAccessibilityRule) == null ? (int?)null
                        : Int32.Parse(column.GetValue(Result.pollAccessibilityRule).ToString());
                        if (value != null)
                        {
                            APIPollsManagementObj.ApplicabilityParameter = column.Name;
                            APIPollsManagementObj.ParameterValueId = value;
                        }
                    }
                    if (columnName == "value")
                    {
                        //Get value of column
                        string value = column.GetValue(Result.pollAccessibilityRule) == null ? null
                        : column.GetValue(Result.pollAccessibilityRule).ToString();
                        APIPollsManagementObj.ParameterValue = value;
                    }
                }
            }
            return APIPollsManagementObj;
        }
        public async Task<int> SendNotification(string pollQuestion, string token)
        {
            ApiNotification Notification = new ApiNotification
            {
                Title = Record.PollNotification,
                // Notification.Message = "You have a new poll available"; 
                Type = Record.Poll,
                //Notification.value1 = pollQuestion;
                //Url = "social/"
            };
            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.PollNotification;
            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
            string result = await response.Content.ReadAsStringAsync();
            Notification.Message = JsonConvert.DeserializeObject(result).ToString();

           // Notification.Message = Notification.Message.Replace("[pollQuestion]", pollQuestion);

            Notification.Url = "social/";

            await this._notification.SendNotification(Notification, token);
            return 1;
        }
        public async Task<IEnumerable<APIOpinionPollQuestion>> OpinionPollsReportTypeHead(string question)
        {
            try
            {
                IQueryable<APIOpinionPollQuestion> result = (from pollmanagement in this.db.PollsManagement
                                                             where (pollmanagement.Question.StartsWith(question) && pollmanagement.IsDeleted == false)
                                                             select new APIOpinionPollQuestion
                                                             {
                                                                 Id = pollmanagement.Id,
                                                                 Title = pollmanagement.Question

                                                             });
                return await result.AsNoTracking().Distinct().ToListAsync();
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
