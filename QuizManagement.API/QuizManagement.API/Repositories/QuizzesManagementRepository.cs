// ======================================
// <copyright file="QuizzesManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================


using AutoMapper;
using QuizManagement.API.APIModel;
using QuizManagement.API.Data;
using QuizManagement.API.Helper;
using QuizManagement.API.Models;
using QuizManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using log4net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QuizManagement.API.Repositories
{
    public class QuizzesManagementRepository : Repository<QuizzesManagement>, IQuizzesManagementRepository
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QuizzesManagementRepository));
        private GadgetDbContext db;
        private INotification _notification;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private readonly IConfiguration _configuration;
        public QuizzesManagementRepository(GadgetDbContext context,
            INotification notification,
            IConfiguration configuration,
        ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.db = context;
            this._notification = notification;
            this._customerConnectionString = customerConnectionString;
            this._configuration = configuration;
        }
        public async Task<IEnumerable<QuizzesManagement>> GetAllQuizzesManagement(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<Models.QuizzesManagement> Query = this.db.QuizzesManagement;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.QuizTitle.Contains(search) && v.IsDeleted == Record.NotDeleted);
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

        public async Task<IEnumerable<QuizzesManagement>> GetAllQuizzesManagementForEndUser(int userId)
        {
            List<QuizzesManagement> QuizList = new List<QuizzesManagement>();
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "dbo.GetQuiz";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            QuizzesManagement Quiz = new QuizzesManagement
                            {
                                Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                QuizTitle = string.IsNullOrEmpty(row["QuizTitle"].ToString()) ? null : row["QuizTitle"].ToString()
                            };
                            QuizList.Add(Quiz);
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                    return QuizList;
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> GetNotificationId(string title)
        {
            var connection = this.db.Database.GetDbConnection();//Dont use Using statement for Connection variable
            int id = 0;
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetIdNotifications";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = title });
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : Convert.ToInt32(row["Id"].ToString());

                        }
                    }
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return id;
        }
        public async Task<int> GetAllQuizzesManagementForEndUserCount(int userId)
        {
            DbConnection connection = this.db.Database.GetDbConnection();//Dont use using statment for connection
            int Count = 0;
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "[dbo].[GetQuizCount]";
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
                        Count = string.IsNullOrEmpty(row["QuizCount"].ToString()) ? 0 : int.Parse(row["QuizCount"].ToString());
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
        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.QuizzesManagement.Where(r => r.QuizTitle.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.QuizzesManagement.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }
        public async Task<bool> Exist(string search)
        {
            int count = await this.db.QuizzesManagement.Where(p => (p.QuizTitle.ToLower() == search.ToLower() && p.IsDeleted == Record.NotDeleted)).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<IEnumerable<QuizzesManagement>> Search(string query)
        {
            Task<List<QuizzesManagement>> quizzesManagementList = (from quizzesManagement in this.db.QuizzesManagement
                                                                   where
                                                              (quizzesManagement.QuizTitle.StartsWith(query)

                                                             )
                                                              && quizzesManagement.IsDeleted == false
                                                                   select quizzesManagement).ToListAsync();
            return await quizzesManagementList;
        }
        public async Task<bool> ExistsQuiz(int quizId, int userid)
        {
            int count = await this.db.QuizResult.Where(p => p.QuizId == quizId && p.UserId == userid).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<bool> ExistsQuizInResult(int? quizId)
        {
            int count = await this.db.QuizResult.Where(p => (p.QuizId == quizId && p.IsDeleted == Record.NotDeleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> isQuestionExist(int? quizId, int? questionId, string question)
        {
            int count = await this.db.QuizQuestionMaster.Where(p => (p.QuizId == quizId && p.Id != questionId && p.Question.ToLower() == question.ToLower() && p.IsDeleted == Record.NotDeleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> ExistsQuizQuestionInResult(int? questionId)
        {
            int count = await this.db.QuizResultDetail.Where(p => (p.QuizQuestionId == questionId && p.IsDeleted == Record.NotDeleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> isQuestionDeleted(int? questionId)
        {
            int count = await this.db.QuizQuestionMaster.Where(p => (p.Id == questionId && p.IsDeleted == Record.Deleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<IEnumerable<QuizzesManagement>> SearchQuizz(string qizz)
        {
            try
            {
                IQueryable<QuizzesManagement> result = (from quizzesManagement in this.db.QuizzesManagement
                                                        where ((quizzesManagement.QuizTitle.StartsWith(qizz) && quizzesManagement.IsDeleted == Record.NotDeleted))
                                                        select new QuizzesManagement
                                                        {
                                                            QuizTitle = quizzesManagement.QuizTitle,
                                                            Id = quizzesManagement.Id

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
        public async Task<APIQuizzesManagement> GetQuiz(int quizId)
        {
          
                var QuizMangementResult = await (from quizManagement in this.db.QuizzesManagement
                                                 join quizAccessibilityRule in this.db.QuizManagementAccessibilityRule on quizManagement.Id equals quizAccessibilityRule.QuizManagementId
                                                 into accessibilityTemp
                                                 from quizAccessibilityRule in accessibilityTemp.DefaultIfEmpty()
                                                 where quizManagement.Id == quizId
                                                 select new { quizManagement, quizAccessibilityRule }).FirstOrDefaultAsync();


                APIQuizzesManagement ApiQuiz = Mapper.Map<APIQuizzesManagement>(QuizMangementResult.quizManagement);
                if (QuizMangementResult.quizAccessibilityRule != null)
                {
                    //Get All coumns of PollAccessibilityRule
                    PropertyInfo[] columns = QuizMangementResult.quizAccessibilityRule.GetType().GetProperties();
                    foreach (PropertyInfo column in columns)
                    {


                        string columnName = column.Name.ToLower();
                        if (columnName == "emailid")
                        {
                            string value = column.GetValue(QuizMangementResult.quizAccessibilityRule) == null ? null :
                                column.GetValue(QuizMangementResult.quizAccessibilityRule).ToString();
                            if (value != null)
                            {
                                ApiQuiz.ApplicabilityParameter = column.Name;
                                ApiQuiz.ApplicabilityParameterValue = column.GetValue(QuizMangementResult.quizAccessibilityRule) == null ? null :
                                    column.GetValue(QuizMangementResult.quizAccessibilityRule).ToString().Decrypt();
                            }
                        }
                        else
                        if(columnName == "createddate" || columnName == "isdeleted" || columnName == "rowguid")
                        {
                            string value = column.GetValue(QuizMangementResult.quizAccessibilityRule) == null ? null :
                                column.GetValue(QuizMangementResult.quizAccessibilityRule).ToString();
                            if (value != null)
                            {
                                
                            }

                        }
                        else
                        if (columnName == "mobilenumber")
                        {
                            string value = column.GetValue(QuizMangementResult.quizAccessibilityRule) == null ? null :
                                column.GetValue(QuizMangementResult.quizAccessibilityRule).ToString();
                            if (value != null)
                            {
                                ApiQuiz.ApplicabilityParameter = column.Name;
                                ApiQuiz.ApplicabilityParameterValue = column.GetValue(QuizMangementResult.quizAccessibilityRule) == null ? null :
                                    column.GetValue(QuizMangementResult.quizAccessibilityRule).ToString().Decrypt();
                            }
                        }
                        else
                        if (columnName != "id" && columnName != "quizmanagementid" && columnName != "value" )
                        {
                            //Get value of column
                            int? value = column.GetValue(QuizMangementResult.quizAccessibilityRule) == null ? (int?)null
                                : Int32.Parse(column.GetValue(QuizMangementResult.quizAccessibilityRule).ToString());
                            if (value != null)
                            {
                                ApiQuiz.ApplicabilityParameter = column.Name;
                                ApiQuiz.ApplicabilityParameterValueId = value;
                            }
                        }
                        if (columnName == "value")
                        {
                            //Get value of column
                            string value = column.GetValue(QuizMangementResult.quizAccessibilityRule) == null ? null
                                : column.GetValue(QuizMangementResult.quizAccessibilityRule).ToString();
                            ApiQuiz.ApplicabilityParameterValue = value;
                        }
                    }
                }
                return ApiQuiz; 
        }
        public async Task<int> AddQuizApplicabilityParameter(int quizId, string accessibilityParameter, string parameterValue, int parameterValueId, string GuidNo)
        {
            QuizManagementAccessibilityRule QuizAccessibilityRule = new QuizManagementAccessibilityRule
            {
                QuizManagementId = quizId,
                Value = parameterValue,
                RowGuid = GuidNo,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false


            };
            switch (accessibilityParameter.ToLower())
            {
                case "area":
                    QuizAccessibilityRule.Area = parameterValueId;
                    break;
                case "group":
                    QuizAccessibilityRule.Group = parameterValueId;
                    break;
                case "location":
                    QuizAccessibilityRule.Location = parameterValueId;
                    break;
                case "buisness":
                case "business":
                    QuizAccessibilityRule.Business = parameterValueId;
                    break;
                case "configurationcolumn1":
                    QuizAccessibilityRule.ConfigurationColumn1 = parameterValueId;
                    break;
                case "configurationcolumn2":
                    QuizAccessibilityRule.ConfigurationColumn2 = parameterValueId;
                    break;
                case "configurationcolumn3":
                    QuizAccessibilityRule.ConfigurationColumn3 = parameterValueId;
                    break;
                case "configurationcolumn4":
                    QuizAccessibilityRule.ConfigurationColumn4 = parameterValueId;
                    break;
                case "configurationcolumn5":
                    QuizAccessibilityRule.ConfigurationColumn5 = parameterValueId;
                    break;
                case "configurationcolumn6":
                    QuizAccessibilityRule.ConfigurationColumn6 = parameterValueId;
                    break;
                case "configurationcolumn7":
                    QuizAccessibilityRule.ConfigurationColumn7 = parameterValueId;
                    break;
                case "configurationcolumn8":
                    QuizAccessibilityRule.ConfigurationColumn8 = parameterValueId;
                    break;
                case "configurationcolumn9":
                    QuizAccessibilityRule.ConfigurationColumn9 = parameterValueId;
                    break;
                case "configurationcolumn10":
                    QuizAccessibilityRule.ConfigurationColumn10 = parameterValueId;
                    break;
                case "configurationcolumn11":
                    QuizAccessibilityRule.ConfigurationColumn11 = parameterValueId;
                    break;
                case "configurationcolumn12":
                    QuizAccessibilityRule.ConfigurationColumn12 = parameterValueId;
                    break;
                case "emailid":
                    QuizAccessibilityRule.EmailId = Convert.ToString(parameterValueId);
                    break;
                case "userid":
                    QuizAccessibilityRule.UserId = parameterValueId;
                    break;
                case "mobilenumber":
                    QuizAccessibilityRule.MobileNumber = Convert.ToString(parameterValueId);
                    break;
                default:
                    return 0;
            }
            await this.db.QuizManagementAccessibilityRule.AddAsync(QuizAccessibilityRule);
            await this.db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> UpdateQuizApplicability(int quizId, string accessibilityParameter, string parameterValue, int? parameterValueId, string rowGuid)
        {
            QuizManagementAccessibilityRule OldQuizAccessiblityRule = await this.db.QuizManagementAccessibilityRule.Where(p => p.QuizManagementId == quizId).FirstOrDefaultAsync();
            if (OldQuizAccessiblityRule != null)
            {
                this.db.QuizManagementAccessibilityRule.Remove(OldQuizAccessiblityRule);
                await this.db.SaveChangesAsync();
            }
            QuizManagementAccessibilityRule QuizAccessiblityRule = new QuizManagementAccessibilityRule();

            if (parameterValueId == null || accessibilityParameter == null)
                return 0;
            QuizAccessiblityRule.QuizManagementId = quizId;
            QuizAccessiblityRule.Value = parameterValue;
            QuizAccessiblityRule.RowGuid = rowGuid;
            switch (accessibilityParameter.ToLower())
            {
                case "area":
                    QuizAccessiblityRule.Area = parameterValueId;
                    break;
                case "group":
                    QuizAccessiblityRule.Group = parameterValueId;
                    break;
                case "location":
                    QuizAccessiblityRule.Location = parameterValueId;
                    break;
                case "buisness":
                case "business":
                    QuizAccessiblityRule.Business = parameterValueId;
                    break;
                case "configurationcolumn1":
                    QuizAccessiblityRule.ConfigurationColumn1 = parameterValueId;
                    break;
                case "configurationcolumn2":
                    QuizAccessiblityRule.ConfigurationColumn2 = parameterValueId;
                    break;
                case "configurationcolumn3":
                    QuizAccessiblityRule.ConfigurationColumn3 = parameterValueId;
                    break;
                case "configurationcolumn4":
                    QuizAccessiblityRule.ConfigurationColumn4 = parameterValueId;
                    break;
                case "configurationcolumn5":
                    QuizAccessiblityRule.ConfigurationColumn5 = parameterValueId;
                    break;
                case "configurationcolumn6":
                    QuizAccessiblityRule.ConfigurationColumn6 = parameterValueId;
                    break;
                case "configurationcolumn7":
                    QuizAccessiblityRule.ConfigurationColumn7 = parameterValueId;
                    break;
                case "configurationcolumn8":
                    QuizAccessiblityRule.ConfigurationColumn8 = parameterValueId;
                    break;
                case "configurationcolumn9":
                    QuizAccessiblityRule.ConfigurationColumn9 = parameterValueId;
                    break;
                case "configurationcolumn10":
                    QuizAccessiblityRule.ConfigurationColumn10 = parameterValueId;
                    break;
                case "configurationcolumn11":
                    QuizAccessiblityRule.ConfigurationColumn11 = parameterValueId;
                    break;
                case "configurationcolumn12":
                    QuizAccessiblityRule.ConfigurationColumn12 = parameterValueId;
                    break;
                case "emailid":
                    QuizAccessiblityRule.EmailId = Convert.ToString(parameterValueId);
                    break;
                case "userid":
                    QuizAccessiblityRule.UserId = parameterValueId;
                    break;
                case "mobilenumber":
                    QuizAccessiblityRule.MobileNumber = Convert.ToString(parameterValueId);
                    break;
                default:
                    return 0;
            }
            this.db.QuizManagementAccessibilityRule.Update(QuizAccessiblityRule);
            await this.db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> SendNotification(string quizTitle, string token,int QuizID)
        {
            try
            {
                ApiNotification Notification = new ApiNotification
                {
                    Title = Record.QuizNotification,
                    Type = Record.Quiz
                };

                string Url = this._configuration[Configuration.NotificationApi];
                Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.QuizNotification;
                HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
                string result = await response.Content.ReadAsStringAsync();
                Notification.Message = JsonConvert.DeserializeObject(result).ToString();
                Notification.Message = Notification.Message.Replace("[quizTitle]", quizTitle);
                Notification.Url = "social/";
                Notification.QuizId = QuizID;
                await this._notification.SendNotification(Notification, token);
                return 1;
            }
            catch (Exception ex)
            { 
                 _logger.Error( Utilities.GetDetailedException(ex));
                
            }
            return 1;
        }

        public async Task<bool> existQuiz(string quizTitle, int? quizId)
        {
            int count = await this.db.QuizzesManagement.Where(p => p.QuizTitle.ToLower() == quizTitle.ToLower() && p.Id != quizId && p.IsDeleted == Record.NotDeleted).CountAsync();

            if (count > 0)
                return true;
            return false;
        }


        public async Task<List<TypeAhead>> GetTypeAHead(string search = null)
        {

            IQueryable<TypeAhead> Quizzes = (from c in this.db.QuizzesManagement
                                             where (search == null || c.QuizTitle.StartsWith(search)) && c.IsDeleted == false
                                             orderby c.QuizTitle
                                             select new TypeAhead
                                             {
                                                 Id = c.Id,
                                                 Title = c.QuizTitle
                                             });

            return await Quizzes.ToListAsync();

        }

        public async Task<APIResponse> GetQuizQuestion(int quizid)
        {

            APIResponse Response = new APIResponse();
            var QuizQuestion = (from c in this.db.QuizQuestionMaster
                                where (c.QuizId == quizid && c.IsDeleted == false)
                                orderby c.Id
                                select new
                                {
                                    Id = c.Id,
                                    Question = c.Question
                                });

            Response.StatusCode = 200;
            Response.ResponseObject = await QuizQuestion.ToListAsync();

            return Response;

        }


        public async Task<List<TypeAhead>> GetTypeAHeadQuizReport(string search = null)
        {
            IQueryable<TypeAhead> Quizzes = (from c in this.db.QuizzesManagement
                                             join question in this.db.QuizQuestionMaster on c.Id equals question.QuizId
                                             where (search == null || c.QuizTitle.StartsWith(search)) && c.IsDeleted == false && question.IsDeleted == false
                                             orderby c.QuizTitle
                                             select new TypeAhead
                                             {
                                                 Id = c.Id,
                                                 Title = c.QuizTitle
                                             });

            return await Quizzes.Distinct().ToListAsync();

        }

        public async Task<List<APIQuizQuestionOptionDetails>> GetQuizQuestionOptionDetails(int QuizID, int UserId)
        {
            List<APIQuizQuestionOptionDetails> aPIQuizQuestionOptionDetailsList = new List<APIQuizQuestionOptionDetails>();
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "dbo.GetQuizQuestionOptionDetails";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@QuizID", SqlDbType.Int) { Value = QuizID });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            APIQuizQuestionOptionDetails obj = new APIQuizQuestionOptionDetails
                            {
                                QuizID = string.IsNullOrEmpty(row["QuizID"].ToString()) ? 0 : int.Parse(row["QuizID"].ToString()),
                                QuizQuestionID = string.IsNullOrEmpty(row["QuizQuestionID"].ToString()) ? 0 : int.Parse(row["QuizQuestionID"].ToString()),
                                QuizQuestionText = row["QuizQuestionText"].ToString(),
                                QuizOptionID = string.IsNullOrEmpty(row["QuizOptionID"].ToString()) ? 0 : int.Parse(row["QuizOptionID"].ToString()),
                                QuizOptionText = row["QuizOptionText"].ToString(),
                                IsCorrectAnswer = string.IsNullOrEmpty(row["IsCorrectAnswer"].ToString()) ? false : bool.Parse(row["IsCorrectAnswer"].ToString()),
                                SelectedAnswer = row["SelectedAnswer"].ToString()
                            };

                            aPIQuizQuestionOptionDetailsList.Add(obj);
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                    return aPIQuizQuestionOptionDetailsList;
                }
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<bool> isSubmittedQuiz(int? quizId, int? userID)
        {
            int count = await this.db.QuizResult.Where(p => (p.QuizId == quizId && p.UserId == userID)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<List<QuizQuestionMaster>> GetQuestionByQuizId(int QuizId)
        {
            List<QuizQuestionMaster> objQuestion = new List<QuizQuestionMaster>();
            objQuestion = await this.db.QuizQuestionMaster.Where(a => a.QuizId == QuizId && a.IsDeleted == Record.NotDeleted).ToListAsync();
            return objQuestion;
        }
        public async Task<int> SendNotificationForQuizAndSurvey(APINotifications apiNotification, bool IsApplicabletoall)
        {
            int Id = 0;
            List<APINotifications> listUserApplicability = new List<APINotifications>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "InsertNotifications";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@IsRead", SqlDbType.Int) { Value = apiNotification.IsRead });
                    cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar) { Value = apiNotification.Message });
                    cmd.Parameters.Add(new SqlParameter("@Url", SqlDbType.NVarChar) { Value = apiNotification.Url });
                    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar) { Value = apiNotification.Title });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = apiNotification.UserId });
                    cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = apiNotification.Type });
                    cmd.Parameters.Add(new SqlParameter("@QuizId", SqlDbType.Int) { Value = apiNotification.QuizId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count <= 0)
                    {
                        reader.Dispose();
                        connection.Close();
                        //return 0;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        Id = Convert.ToInt32(row["Id"].ToString());
                    }
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

            }
            return Id;
        }

        public async Task<int> SendNotificationForQuizSurvey(APINotifications apiNotification, bool IsApplicabletoall, string GuidNo, int notificationID,int UserId)
        {
            int Id = 0;
            List<APINotifications> listUserApplicability = new List<APINotifications>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetQuizSurveyCustomizationNotification";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@RowGuid", SqlDbType.NVarChar) { Value = GuidNo });
                    cmd.Parameters.Add(new SqlParameter("@NotificationId", SqlDbType.Int) { Value = notificationID });
                    cmd.Parameters.Add(new SqlParameter("@IsApplicableToAll", SqlDbType.Bit) { Value = IsApplicabletoall });
                    cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@Message ", SqlDbType.NVarChar) { Value = apiNotification.Message });
                    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar) { Value = apiNotification.Title });
                    cmd.Parameters.Add(new SqlParameter("@URL", SqlDbType.NVarChar) { Value = apiNotification.Url });
                    cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = apiNotification.Type });


                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                      reader.Dispose();
                        connection.Close();
                 }
                connection.Close();
            }
            catch (Exception ex)
            {
              _logger.Error( Utilities.GetDetailedException(ex));
                
            }
            return Id;
        }
        public async Task<int> SendQuizApplicabilityPushNotification(int QuizId, string orgCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/QuizApplicabilityPushNotification";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("QuizId", QuizId);
            oJsonObject.Add("OrganizationCode", orgCode);
           
            HttpResponseMessage responses = ApiHelper.CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> SendPollApplicabilityPushNotification(int PollId, string orgCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/PollApplicabilityPushNotification";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("PollId", PollId);
            oJsonObject.Add("OrganizationCode", orgCode);
           
            HttpResponseMessage responses = ApiHelper.CallAPI(Url, oJsonObject).Result;
            return 1;
        }
        public async Task<int> SendNotificationCustomizeSurvey(APINotifications apiNotification, bool IsApplicabletoall, string GuidNo, int notificationID, int UserId)
        {
            int Id = 0;
            List<APINotifications> listUserApplicability = new List<APINotifications>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetSurveyCustomizeNotification";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@RowGuid", SqlDbType.NVarChar) { Value = GuidNo });
                    cmd.Parameters.Add(new SqlParameter("@NotificationId", SqlDbType.Int) { Value = notificationID });
                    cmd.Parameters.Add(new SqlParameter("@IsApplicableToAll", SqlDbType.Bit) { Value = IsApplicabletoall });
                    cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@Message ", SqlDbType.NVarChar) { Value = apiNotification.Message });
                    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar) { Value = apiNotification.Title });
                    cmd.Parameters.Add(new SqlParameter("@URL", SqlDbType.NVarChar) { Value = apiNotification.Url });
                    cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = "Opinion" });


                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    reader.Dispose();
                    connection.Close();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Id;
        }

        public async Task<List<APIApplicableNotifications>> SendDataForApplicableNotifications(int NotificationId, int UserId)
        {
            List<APIApplicableNotifications> listUserApplicability = new List<APIApplicableNotifications>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "ApplicableInsertNotifications";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@NotificationId", SqlDbType.NVarChar) { Value = NotificationId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });


                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            APIApplicableNotifications rule = new APIApplicableNotifications();
                            rule.IsRead = false;
                            rule.IsReadCount = false;
                            rule.NotificationId = NotificationId;
                            rule.UserId = UserId;
                            //rule.Url = row["Url"].ToString();
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
    }
}
