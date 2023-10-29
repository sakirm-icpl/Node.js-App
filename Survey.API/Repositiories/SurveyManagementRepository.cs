// ======================================
// <copyright file="SurveyManagementRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AutoMapper;
using Survey.API.APIModel;
using Survey.API.Common;
using Survey.API.Data;
using Survey.API.Helper;
using Survey.API.Metadata;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using Survey.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using log4net;
using System.Threading.Tasks;
using static Survey.API.Helper.ApiHelper;

namespace Survey.API.Repositories
{
    public class SurveyManagementRepository : Repository<SurveyManagement>, ISurveyManagementRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SurveyManagementRepository));
        private GadgetDbContext db;
        private readonly IConfiguration _configuration;
        private INotification _notification;
        private readonly IIdentityService _identitySvc;
        private ILcmsRepository _lcmsRepository;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private ISurveyResultRepository surveyResultRepository;
        private ISurveyResultDetailRepository surveyResultDetailRepository;
        public SurveyManagementRepository(GadgetDbContext context,
            INotification notification,
            IConfiguration configuration,
            ILcmsRepository lcmsRepository,
            IIdentityService identitySvc,
            ISurveyResultRepository surveyResultRepository,
            ISurveyResultDetailRepository surveyResultDetailRepository,
            ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.db = context;
            this._notification = notification;
            this._configuration = configuration;
            this._identitySvc = identitySvc;
            this._lcmsRepository = lcmsRepository;
            this.surveyResultRepository = surveyResultRepository;
            this.surveyResultDetailRepository = surveyResultDetailRepository;
            this._customerConnectionString = customerConnectionString;
        }

        public async Task<IEnumerable<SurveyManagement>> GetAllSurveyManagement(int UserId, string UserRole, int page, int pageSize, string search = null)
        {
            try
            {
                if (UserRole == Record.LoginUserRole)
                {
                    IQueryable<Models.SurveyManagement> Query = this.db.SurveyManagement;

                    if (!string.IsNullOrEmpty(search))
                    {
                        Query = Query.Where(v => v.SurveySubject.Contains(search) || Convert.ToString(v.SurveyPurpose).Contains(search) && v.IsDeleted == Record.NotDeleted);
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
                else
                {
                    IQueryable<Models.SurveyManagement> Query = this.db.SurveyManagement;

                    if (!string.IsNullOrEmpty(search))
                    {
                        Query = Query.Where(v => v.SurveySubject.Contains(search) || Convert.ToString(v.SurveyPurpose).Contains(search) && v.IsDeleted == Record.NotDeleted);
                        Query = Query.OrderByDescending(v => v.Id);
                    }
                    else
                    {
                        Query = Query.Where(v => v.IsDeleted == Record.NotDeleted);
                    }
                    Query = Query.Where(v => v.CreatedBy == UserId);
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
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
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
                return await this.db.SurveyManagement.Where(r => (r.IsDeleted == Record.NotDeleted && r.Id == pubId && (DateTime.Compare(r.StartDate.Date, parsedDate.Date) == 0))).CountAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }

        public async Task<int> FirstResponse(int surveyId)
        {

            try
            {
                return await this.db.SurveyResult.Where(r => (r.IsDeleted == Record.NotDeleted && r.SurveyId == surveyId)).CountAsync();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }
        public async Task<int> Count(int UserId, string UserRole, string search = null)
        {

            if (UserRole == Record.LoginUserRole)
            {
                if (!string.IsNullOrWhiteSpace(search))
                    return await this.db.SurveyManagement.Where(r => ((r.SurveySubject.Contains(search) || Convert.ToString(r.SurveyPurpose).Contains(search) && r.IsDeleted == Record.NotDeleted))).CountAsync();
                return await this.db.SurveyManagement.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(search))
                    return await this.db.SurveyManagement.Where(r => ((r.SurveySubject.Contains(search) || Convert.ToString(r.SurveyPurpose).Contains(search)) && (r.IsDeleted == Record.NotDeleted && r.CreatedBy == UserId))).CountAsync();
                return await this.db.SurveyManagement.Where(r => r.IsDeleted == Record.NotDeleted && r.CreatedBy == UserId).CountAsync();
            }
        }


        public async Task<bool> Exist(string search)
        {
            int count = await this.db.SurveyManagement.Where(p => p.SurveySubject.ToLower() == search.ToLower() && p.IsDeleted == false).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<bool> ExistsInResult(int surveyId)
        {
            int count = await this.db.SurveyResult.Where(p => (p.SurveyId == surveyId && p.IsDeleted == Record.NotDeleted)).CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<bool> existSurvey(string surveySubject, int? surveyID)
        {
            int count = await this.db.SurveyManagement.Where(p => (p.SurveySubject.ToLower() == surveySubject.ToLower()) && p.Id != surveyID && p.IsDeleted == Record.NotDeleted).CountAsync();

            if (count > 0)
                return true;
            return false;
        }
        public async Task<IEnumerable<SurveyManagement>> Search(string query)
        {
            Task<List<SurveyManagement>> surveyManagementList = (from surveyManagement in this.db.SurveyManagement
                                                                 where
                                                            (surveyManagement.SurveySubject.Contains(query) ||
                                                           Convert.ToString(surveyManagement.SurveyPurpose).Contains(query)
                                                           )
                                                            && surveyManagement.IsDeleted == false
                                                                 select surveyManagement).ToListAsync();
            return await surveyManagementList;
        }

        public async Task<IEnumerable<SurveyManagement>> GetAllSurveyManagement(int userId)
        {

            try
            {
                SurveyManagement album = new SurveyManagement();
                List<SurveyManagement> albums = new List<SurveyManagement>();
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetSurvey";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                album = new SurveyManagement
                                {
                                    Id = Convert.ToInt32(row["Id"].ToString()),
                                    SurveySubject = row["SurveySubject"].ToString(),
                                    SurveyPurpose = row["SurveyPurpose"].ToString(),
                                    LcmsId = Convert.ToInt32(row["LcmsId"].ToString())
                                };
                                albums.Add(album);
                            }
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
                return albums;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<Action> RewardPointSave(string functionCode, string category, int referenceId, int point, int userId)
        {
            try
            {

                using (DbConnection connection = this.db.Database.GetDbConnection())
                {
                    connection.Open();
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "RewardPointsDaily_Upsert";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FunctionCode", SqlDbType.NVarChar) { Value = functionCode });
                        cmd.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar) { Value = category });
                        cmd.Parameters.Add(new SqlParameter("@ReferenceId", SqlDbType.Int) { Value = referenceId });
                        cmd.Parameters.Add(new SqlParameter("@Point", SqlDbType.Int) { Value = point });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        reader.Dispose();
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<int> GetAllSurveyManagementCount(int userId)
        {
            int totalCount = 0;
            DateTime datecurrent = DateTime.UtcNow;
            string date = datecurrent.ToString("dd-MM-yyyy");
            string format = "dd-MM-yyyy";
            DateTime parsedDate = DateTime.ParseExact(date, format, null);
            try
            {
                SurveyManagement album = new SurveyManagement();
                List<SurveyManagement> albums = new List<SurveyManagement>();
                using (DbConnection connection = this.db.Database.GetDbConnection())
                {
                    connection.Open();
                    using (DbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetSurvey";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.Int) { Value = userId });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        totalCount = dt.Rows.Count;
                        reader.Dispose();
                    }
                }
                return totalCount;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return 0;
        }
        public async Task<bool> ExistsSurvey(int surveyId, int userid)
        {
            int count = await this.db.SurveyResult
                .Where(p => (p.SurveyId == surveyId && p.UserId == userid && p.IsDeleted == Record.NotDeleted))
                .CountAsync();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<List<SurveyConfiguration>> GetDetailsFromSuveryID(int LcmsId)
        {
            List<SurveyConfiguration> obj = new List<SurveyConfiguration>();
            obj = await this.db.SurveyConfiguration.Where(a => a.LcmsId == LcmsId).ToListAsync();
            return obj;
        }

        public async Task<IEnumerable<SurveyManagement>> SearchSurvey(string Survey)
        {
            try
            {
                Survey = Survey.Trim();
                IQueryable<SurveyManagement> result = (from surveyManagement in this.db.SurveyManagement
                                                       where (surveyManagement.SurveySubject.StartsWith(Survey) && surveyManagement.IsDeleted == Record.NotDeleted)
                                                       select new SurveyManagement
                                                       {
                                                           SurveySubject = surveyManagement.SurveySubject,
                                                           Id = surveyManagement.Id

                                                       });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async Task<IEnumerable<SurveyManagement>> SearchSurveyTODO(string Survey)
        {
            try
            {
                DateTime datecurrent = DateTime.UtcNow;
                //string date = datecurrent.ToString("dd-MM-yyyy");
                //string format = "dd-MM-yyyy";
                string date = datecurrent.ToString("yyyy-mm-dd");
                string format = "yyyy-mm-dd";
                DateTime parsedDate = DateTime.ParseExact(date, format, null);


                Survey = string.IsNullOrEmpty(Survey) ? "null" : Survey.Trim();
                //Survey = Survey.Trim();
                IQueryable<SurveyManagement> result = (from surveyManagement in this.db.SurveyManagement
                                                       where (surveyManagement.SurveySubject.StartsWith(Survey.ToLower()) && (DateTime.Compare(surveyManagement.ValidityDate.Date, parsedDate.Date) >= 0) && surveyManagement.IsDeleted == Record.NotDeleted && surveyManagement.Status == true)
                                                       select new SurveyManagement
                                                       {
                                                           SurveySubject = surveyManagement.SurveySubject,
                                                           Id = surveyManagement.Id,
                                                           ValidityDate = surveyManagement.ValidityDate
                                                       });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int> SendNotification(string surveySubject, string token, int surveyID)
        {
            ApiNotification Notification = new ApiNotification
            {
                Title = Record.SurveyNotification,
                Type = Record.Survey
            };


            string Url = this._configuration[Configuration.NotificationApi];
            Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.SurveyNotification;
            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
            string result = await response.Content.ReadAsStringAsync();
            Notification.Message = JsonConvert.DeserializeObject(result).ToString();

            Notification.Message = Notification.Message.Replace("[surveySubject]", surveySubject);
            Notification.Url = "social/";
            Notification.SurveyId = surveyID;
            //  Notification.value1 = surveySubject;           
            await this._notification.SendNotification(Notification, token);
            return 1;
        }
        public async Task<int> AddSurvey(ApiSurveyLcms apiSurveyLcms, int userId)
        {
            List<SurveyConfiguration> SurveyConfigurationList = new List<SurveyConfiguration>();
            ApiLcms ApiLcms = new ApiLcms
            {
                Name = apiSurveyLcms.Name.Trim(),
                MetaData = apiSurveyLcms.MetaData,
                IsNested = apiSurveyLcms.IsNested,
                ContentType = "survey"
            };
            int LcmsId = await this._lcmsRepository.PostLcms(ApiLcms);
            if (LcmsId == 0)
                return 0;
            if (LcmsId == -1)
                return -1;
            int SequenceNumber = 1;
            foreach (SurveyLcmsQuestion surveyQuestion in apiSurveyLcms.SurveyQuestion)
            {
                SurveyConfiguration SurveyConfiguration = new SurveyConfiguration
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    QuestionId = surveyQuestion.Id,
                    LcmsId = LcmsId,
                    SequenceNumber = SequenceNumber
                };
                SurveyConfigurationList.Add(SurveyConfiguration);
                SequenceNumber++;
            }
            await this.SurveyConfigurationAddRange(SurveyConfigurationList);
            return 1;
        }
        public async Task<int> UpdateSurvey(ApiSurveyLcms apiSurveyLcms, int userId)
        {
            List<SurveyConfiguration> SurveyConfigurationList = new List<SurveyConfiguration>();
            ApiLcms ApiLcms = new ApiLcms
            {
                Name = apiSurveyLcms.Name,
                MetaData = apiSurveyLcms.MetaData,
                IsNested = apiSurveyLcms.IsNested,
                ContentType = "survey",
                Id = apiSurveyLcms.LcmsId
            };
            int Result = await this._lcmsRepository.UpdateLcms(ApiLcms);
            if (Result == 0)
                return 0;

            SurveyConfigurationList = await this.db.SurveyConfiguration.Where(sc => sc.LcmsId == apiSurveyLcms.LcmsId).ToListAsync();
            await RemoveSurveyConfigurations(SurveyConfigurationList);
            SurveyConfigurationList.Clear();
            int SequenceNumber = 1;
            foreach (SurveyLcmsQuestion surveyQuestion in apiSurveyLcms.SurveyQuestion)
            {
                SurveyConfiguration SurveyConfiguration = new SurveyConfiguration
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    QuestionId = surveyQuestion.Id,
                    LcmsId = apiSurveyLcms.LcmsId,
                    SequenceNumber = SequenceNumber
                };
                SurveyConfigurationList.Add(SurveyConfiguration);
                SequenceNumber++;
            }
            await this.SurveyConfigurationAddRange(SurveyConfigurationList);
            return 1;
        }

        public async Task<GetApiSurveyLcms> GetLcmsSurvey(int lcmsId)
        {
            ApiLcms ApiLcms = await this._lcmsRepository.GetLcms(lcmsId);
            if (ApiLcms == null)
                return null;

            GetApiSurveyLcms apiSurveyLcms = new GetApiSurveyLcms
            {
                SurveyQuestion = new List<SurveyLcmsQuestion>()
            };

            var result = (from surveyConfig in db.SurveyConfiguration
                          join surveyQuestion in db.SurveyQuestion on surveyConfig.QuestionId equals surveyQuestion.Id
                          where surveyQuestion.IsDeleted == false && surveyConfig.IsDeleted == false
                                && surveyConfig.LcmsId == lcmsId
                          orderby surveyConfig.SequenceNumber ascending
                          select new SurveyLcmsQuestion
                          {
                              Id = surveyQuestion.Id,
                              Question = surveyQuestion.Question
                          });
            apiSurveyLcms.SurveyQuestion = result.ToList();
            apiSurveyLcms.Name = ApiLcms.Name;
            apiSurveyLcms.MetaData = ApiLcms.MetaData;
            apiSurveyLcms.LcmsId = lcmsId;
            apiSurveyLcms.IsNested = ApiLcms.IsNested;

            apiSurveyLcms.Ismodulecreate = await db.LCMS.Where(a => a.Id == lcmsId).Select(a => a.Ismodulecreate).FirstOrDefaultAsync();

            return apiSurveyLcms;
        }
        public async Task<int> SurveyConfigurationAddRange(List<SurveyConfiguration> SurveyConfigurationList)
        {
            await this.db.SurveyConfiguration.AddRangeAsync(SurveyConfigurationList);
            await this.db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> RemoveSurveyConfigurations(List<SurveyConfiguration> SurveyConfigurationList)
        {
            this.db.SurveyConfiguration.RemoveRange(SurveyConfigurationList);
            await this.db.SaveChangesAsync();
            return 1;
        }
        public async Task<APISurveyManagement> GetSurveyMangementLcms(int surveyId)
        {
            var SurveyManagementResult = await (from surveyManagement in this.db.SurveyManagement
                                                join surveyAccessibilityRule in this.db.SurveyManagementAccessibilityRule on surveyManagement.Id equals surveyAccessibilityRule.SurveyManagementId
                                                into accessibilityTemp
                                                from surveyAccessibilityRule in accessibilityTemp.DefaultIfEmpty()
                                                where surveyManagement.Id == surveyId
                                                select new { surveyManagement, surveyAccessibilityRule }).FirstOrDefaultAsync();


            APISurveyManagement ApiSurvey = Mapper.Map<APISurveyManagement>(SurveyManagementResult.surveyManagement);
            if (SurveyManagementResult.surveyAccessibilityRule != null)
            {
                //Get All coumns of PollAccessibilityRule
                PropertyInfo[] columns = SurveyManagementResult.surveyAccessibilityRule.GetType().GetProperties();
                foreach (PropertyInfo column in columns)
                {

                    string columnName = column.Name.ToLower();
                    if (columnName == "emailid")
                    {
                        string value = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null :
                            column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        if (value != null)
                        {
                            ApiSurvey.ApplicabilityParameter = column.Name;
                            ApiSurvey.ApplicabilityParameterValue = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null :
                                column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString().Decrypt();
                        }
                    }
                    else
                    if (columnName == "mobilenumber")
                    {
                        string value = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null :
                            column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        if (value != null)
                        {
                            ApiSurvey.ApplicabilityParameter = column.Name;
                            ApiSurvey.ApplicabilityParameterValue = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null :
                                column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString().Decrypt();
                        }
                    }

                    //else

                    //    string columnName = column.Name.ToLower();
                    //if(columnName.Equals("EmailId",StringComparison.OrdinalIgnoreCase))
                    //{
                    //    string value = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null :
                    //        column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                    //    if (value != null)
                    //    {
                    //        ApiSurvey.ApplicabilityParameter = column.Name;
                    //        ApiSurvey.ApplicabilityParameterValue = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null :
                    //            column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                    //    }
                    //}
                    else if (columnName.Equals("conditionforrules"))
                    {
                        //Get value of column
                        string value = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null
                            : column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        ApiSurvey.ApplicabilityParameterValue = value;
                    }
                    else if (columnName.Equals("createddate"))
                    {
                        //Get value of column
                        string value = Convert.ToDateTime(column.GetValue(SurveyManagementResult.surveyAccessibilityRule)) == null ? null
                            : column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        ApiSurvey.ApplicabilityParameterValue = value;
                    }
                    else if (columnName.Equals("modifieddate"))
                    {
                        string value = Convert.ToDateTime(column.GetValue(SurveyManagementResult.surveyAccessibilityRule)) == null ? null
                            : column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        ApiSurvey.ApplicabilityParameterValue = value;
                    }
                    else if (columnName.Equals("isdeleted"))
                    {
                        string value = Convert.ToBoolean(column.GetValue(SurveyManagementResult.surveyAccessibilityRule)) == null ? null
                            : column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        ApiSurvey.ApplicabilityParameterValue = value;
                    }
                    else if (columnName.Equals("rowguid"))
                    {
                        string value = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null
                            : column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        ApiSurvey.ApplicabilityParameterValue = value;
                    }
                    else if (columnName != "id" && columnName != "surveymanagementid" && columnName != "value")
                    {
                        //Get value of column
                        int? value = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? (int?)null
                            : Int32.Parse(column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString());

                        if (value != null)
                        {
                            ApiSurvey.ApplicabilityParameter = column.Name;
                            ApiSurvey.ApplicabilityParameterValueId = value;
                        }
                    }
                    else if (columnName == "value")
                    {
                        //Get value of column
                        string value = column.GetValue(SurveyManagementResult.surveyAccessibilityRule) == null ? null
                            : column.GetValue(SurveyManagementResult.surveyAccessibilityRule).ToString();
                        ApiSurvey.ApplicabilityParameterValue = value;
                    }
                }
            }

            ApiLcms Lcms = await this._lcmsRepository.GetLcms(SurveyManagementResult.surveyManagement.LcmsId);
            if (Lcms != null)
                ApiSurvey.LcmsName = Lcms.Name;
            return ApiSurvey;
        }
        public async Task<int> AddSurveyApplicability(int surveyId, string accessibilityParameter, string parameterValue, int parameterValueId)
        {
            SurveyManagementAccessibilityRule SurveyAccessibilityRule = new SurveyManagementAccessibilityRule
            {
                SurveyManagementId = surveyId,
                Value = parameterValue
            };
            switch (accessibilityParameter.ToLower())
            {
                case "area":
                    SurveyAccessibilityRule.Area = parameterValueId;
                    break;
                case "group":
                    SurveyAccessibilityRule.Group = parameterValueId;
                    break;
                case "location":
                    SurveyAccessibilityRule.Location = parameterValueId;
                    break;
                case "buisness":
                case "business":
                    SurveyAccessibilityRule.Business = parameterValueId;
                    break;
                case "configurationcolumn1":
                    SurveyAccessibilityRule.ConfigurationColumn1 = parameterValueId;
                    break;
                case "configurationcolumn2":
                    SurveyAccessibilityRule.ConfigurationColumn2 = parameterValueId;
                    break;
                case "configurationcolumn3":
                    SurveyAccessibilityRule.ConfigurationColumn3 = parameterValueId;
                    break;
                case "configurationcolumn4":
                    SurveyAccessibilityRule.ConfigurationColumn4 = parameterValueId;
                    break;
                case "configurationcolumn5":
                    SurveyAccessibilityRule.ConfigurationColumn5 = parameterValueId;
                    break;
                case "configurationcolumn6":
                    SurveyAccessibilityRule.ConfigurationColumn6 = parameterValueId;
                    break;
                case "configurationcolumn7":
                    SurveyAccessibilityRule.ConfigurationColumn7 = parameterValueId;
                    break;
                case "configurationcolumn8":
                    SurveyAccessibilityRule.ConfigurationColumn8 = parameterValueId;
                    break;
                case "configurationcolumn9":
                    SurveyAccessibilityRule.ConfigurationColumn9 = parameterValueId;
                    break;
                case "configurationcolumn10":
                    SurveyAccessibilityRule.ConfigurationColumn10 = parameterValueId;
                    break;
                case "configurationcolumn11":
                    SurveyAccessibilityRule.ConfigurationColumn11 = parameterValueId;
                    break;
                case "emailid":
                    SurveyAccessibilityRule.EmailId = parameterValue.Encrypt();
                    break;
                case "userid":
                    SurveyAccessibilityRule.UserId = parameterValueId;
                    break;
                case "mobilenumber":
                    SurveyAccessibilityRule.MobileNumber = parameterValue.Encrypt();
                    break;
                default:
                    return 0;
            }
            await this.db.SurveyManagementAccessibilityRule.AddAsync(SurveyAccessibilityRule);
            await this.db.SaveChangesAsync();
            return 1;
        }

        public async Task<SurveyManagement> CheckForDuplicate(string SurveySubject)
        {
            SurveyManagement obj = new SurveyManagement();
            obj = await this.db.SurveyManagement.Where(a => a.SurveySubject == SurveySubject && a.IsDeleted == false).FirstOrDefaultAsync();
            return obj;
        }

        public async Task<int> UpdateSurveyApplicability(int surveyId, string accessibilityParameter, string parameterValue, int? parameterValueId)
        {
            SurveyManagementAccessibilityRule OldSurveyAccessibilityRule = await this.db.SurveyManagementAccessibilityRule.Where(p => p.SurveyManagementId == surveyId).FirstOrDefaultAsync();

            if (OldSurveyAccessibilityRule != null)
            {
                this.db.SurveyManagementAccessibilityRule.Remove(OldSurveyAccessibilityRule);
                await this.db.SaveChangesAsync();
            }

            SurveyManagementAccessibilityRule SurveyAccessibilityRule = new SurveyManagementAccessibilityRule();

            if (parameterValueId == null || accessibilityParameter == null)
                return 0;

            SurveyAccessibilityRule.SurveyManagementId = surveyId;
            SurveyAccessibilityRule.Value = parameterValue;
            switch (accessibilityParameter.ToLower())
            {
                case "area":
                    SurveyAccessibilityRule.Area = parameterValueId;
                    break;
                case "group":
                    SurveyAccessibilityRule.Group = parameterValueId;
                    break;
                case "location":
                    SurveyAccessibilityRule.Location = parameterValueId;
                    break;
                case "buisness":
                case "business":
                    SurveyAccessibilityRule.Business = parameterValueId;
                    break;
                case "configurationcolumn1":
                    SurveyAccessibilityRule.ConfigurationColumn1 = parameterValueId;
                    break;
                case "configurationcolumn2":
                    SurveyAccessibilityRule.ConfigurationColumn2 = parameterValueId;
                    break;
                case "configurationcolumn3":
                    SurveyAccessibilityRule.ConfigurationColumn3 = parameterValueId;
                    break;
                case "configurationcolumn4":
                    SurveyAccessibilityRule.ConfigurationColumn4 = parameterValueId;
                    break;
                case "configurationcolumn5":
                    SurveyAccessibilityRule.ConfigurationColumn5 = parameterValueId;
                    break;
                case "configurationcolumn6":
                    SurveyAccessibilityRule.ConfigurationColumn6 = parameterValueId;
                    break;
                case "configurationcolumn7":
                    SurveyAccessibilityRule.ConfigurationColumn7 = parameterValueId;
                    break;
                case "configurationcolumn8":
                    SurveyAccessibilityRule.ConfigurationColumn8 = parameterValueId;
                    break;
                case "configurationcolumn9":
                    SurveyAccessibilityRule.ConfigurationColumn9 = parameterValueId;
                    break;
                case "configurationcolumn10":
                    SurveyAccessibilityRule.ConfigurationColumn10 = parameterValueId;
                    break;
                case "configurationcolumn11":
                    SurveyAccessibilityRule.ConfigurationColumn11 = parameterValueId;
                    break;
                case "configurationcolumn12":
                    SurveyAccessibilityRule.ConfigurationColumn12 = parameterValueId;
                    break;
                case "emailid":
                    SurveyAccessibilityRule.EmailId = parameterValue.Encrypt();
                    break;
                case "userid":
                    SurveyAccessibilityRule.UserId = parameterValueId;
                    break;
                case "mobilenumber":
                    SurveyAccessibilityRule.MobileNumber = parameterValue.Encrypt();
                    break;
                default:
                    return 0;
            }
            this.db.SurveyManagementAccessibilityRule.Update(SurveyAccessibilityRule);
            await this.db.SaveChangesAsync();
            return 1;
        }

        public async Task<APIResponse> GetSurveyQuestionTypeAhead(int surveyid)
        {

            APIResponse Response = new APIResponse();
            var SurveyQuestion = (from surveyManagement in db.SurveyManagement
                                  join surveyConfiguration in db.SurveyConfiguration on surveyManagement.LcmsId equals surveyConfiguration.LcmsId
                                  join surveyQuestion in this.db.SurveyQuestion on surveyConfiguration.QuestionId equals surveyQuestion.Id
                                  where (surveyManagement.Id == surveyid && surveyQuestion.Section.Trim() == "objective")
                                  select new
                                  {
                                      Id = surveyQuestion.Id,
                                      Question = surveyQuestion.Question
                                  });

            Response.StatusCode = 200;
            Response.ResponseObject = await SurveyQuestion.ToListAsync();

            return Response;

        }


        public async Task<IEnumerable<SurveyManagement>> SurveyReportTypeHead(string Survey)
        {
            try
            {
                IQueryable<SurveyManagement> result = (from surveyManagement in this.db.SurveyManagement
                                                       join surveyConfiguration in db.SurveyConfiguration on surveyManagement.LcmsId equals surveyConfiguration.LcmsId
                                                       join surveyQuestion in this.db.SurveyQuestion on surveyConfiguration.QuestionId equals surveyQuestion.Id
                                                       where (surveyManagement.SurveySubject.StartsWith(Survey) && surveyManagement.IsDeleted == Record.NotDeleted)
                                                       select new SurveyManagement
                                                       {
                                                           SurveySubject = surveyManagement.SurveySubject,
                                                           Id = surveyManagement.Id

                                                       });
                return await result.AsNoTracking().Distinct().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<IEnumerable<SurveyManagement>> SurveyNotApplicableTypeAhead(string Survey)
        {
            try
            {
                IQueryable<SurveyManagement> result = (from surveyManagement in this.db.SurveyManagement
                                                       join surveyConfiguration in db.SurveyConfiguration on surveyManagement.LcmsId equals surveyConfiguration.LcmsId
                                                       join surveyQuestion in this.db.SurveyQuestion on surveyConfiguration.QuestionId equals surveyQuestion.Id
                                                       where (surveyManagement.SurveySubject.StartsWith(Survey) && surveyManagement.IsDeleted == Record.NotDeleted && surveyManagement.IsApplicableToAll == false)
                                                       select new SurveyManagement
                                                       {
                                                           SurveySubject = surveyManagement.SurveySubject,
                                                           Id = surveyManagement.Id

                                                       });
                return await result.AsNoTracking().Distinct().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<bool> IsQuestionUsed(int questionId)
        {
            int lcmsID = await (from e in db.SurveyConfiguration where e.QuestionId == questionId select e.LcmsId).FirstOrDefaultAsync();
            if (lcmsID > 0)
            {
                int count = await this.db.SurveyManagement.Where(p => (p.LcmsId == lcmsID && p.IsDeleted == Record.NotDeleted)).CountAsync();
                if (count > 0)
                    return true;
                else
                    return false;
            }

            return false;
        }
        public async Task<string> ProcessImportFile(FileInfo file, ISurveyManagementRepository surveyManagementRepository, ISurveyQuestionRepository surveyQuestionRepository, ISurveyOptionRepository surveyOptionRepository, ISurveyQuestionRejectedRepository _surveyQuestionRejectedRepository, int UserId)
        {
            string result;
            try
            {

                SurveyQuestionImport.ProcessFile.Reset();
                int resultMessage = await SurveyQuestionImport.ProcessFile.InitilizeAsync(file);
                if (resultMessage == 1)
                {
                    result = await SurveyQuestionImport.ProcessFile.ProcessRecordsAsync(file, surveyManagementRepository, surveyQuestionRepository, surveyOptionRepository, _surveyQuestionRejectedRepository, UserId);
                    SurveyQuestionImport.ProcessFile.Reset();
                    return result;
                }
                else if (resultMessage == 2)
                {
                    result = Record.CannotContainNewLineCharacters;
                    SurveyQuestionImport.ProcessFile.Reset();
                    return result;
                }
                else
                {
                    result = Record.FileInvalid;
                    SurveyQuestionImport.ProcessFile.Reset();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Record.FileInvalid;
        }
        public async Task<List<SurveyQuestionRejected>> GetAllSurvey()
        {
            IQueryable<SurveyQuestionRejected> Query = (from survey in this.db.SurveyManagement
                                                            //join surveyquestion in this.db.SurveyQuestion on survey.Id equals surveyquestion.Id
                                                            //join surveyoption in this.db.SurveyOption on survey.Id equals surveyoption.Id
                                                        where survey.IsDeleted == Record.NotDeleted
                                                        select new SurveyQuestionRejected
                                                        {
                                                            Id = survey.Id,
                                                            // Question = surveyquestion.Question


                                                        });
            return await Query.ToListAsync();
        }
        //public async Task<int> Count(string search = null)
        //{
        //    if (!string.IsNullOrWhiteSpace(search))
        //        return await this._db.FeedbackQuestionRejected.Where(r => r.QuestionText.Contains(search) || Convert.ToString(r.Section).Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
        //    return await this._db.FeedbackQuestionRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        //}
        public async Task<bool> IsSurveyActive(int SurveyId)
        {
            bool IsActive = this.db.SurveyManagement.Where(a => a.Id == SurveyId).Select(a => a.Status).SingleOrDefault();
            return IsActive;
        }
        public async Task<List<AccessibilitySurveyRules>> Post(APISurveyAccessibility apiAccessibility, int userId, string orgnizationCode = null, string token = null)
        {
            string GuidNo = null;
            string url = "";
            string urlSMS = "";
            List<AccessibilitySurveyRules> Duplicates = new List<AccessibilitySurveyRules>();
            AccessibilitySurveyRules[] AndAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower() == "and").ToArray();
            AccessibilitySurveyRules[] OrAccessibilityRules = apiAccessibility.AccessibilityRule.Where(a => a.Condition.ToLower() == "or" || a.Condition.ToLower() == "null").ToArray();
            if (AndAccessibilityRules.Count() > 0)
            {
                //AccessibilityRule ExistingRule = GetRuleByCourIdAndCondition("and", apiAccessibility.CourseId, userId);
                //AccessibilityRule accessibilityRules = ExistingRule != null ? ExistingRule : new AccessibilityRule();
                //SurveyManagementAccessibilityRule surveyaccessibilityRules = new SurveyManagementAccessibilityRule
                //{
                //    SurveyManagementId = apiAccessibility.SurveyManagementId,
                //    ConditionForRules = "and",
                //    CreatedDate = DateTime.UtcNow,
                //    ModifiedDate = DateTime.UtcNow,
                //    CreatedBy = userId,
                //    RowGuid = GuidNo

                //};

                foreach (AccessibilitySurveyRules accessibility in AndAccessibilityRules)
                {
                    SurveyManagementAccessibilityRule surveyaccessibilityRules = new SurveyManagementAccessibilityRule()
                    {
                        SurveyManagementId = apiAccessibility.SurveyManagementId,
                        ConditionForRules = "and",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = userId,
                        RowGuid = Convert.ToString(Guid.NewGuid())
                    };

                    if (!string.IsNullOrEmpty(apiAccessibility.GroupTemplateId))
                    {
                        surveyaccessibilityRules.GroupTemplateId = Convert.ToInt32(apiAccessibility.GroupTemplateId);
                    }
                    else
                    {
                        surveyaccessibilityRules.GroupTemplateId = null;
                    }


                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn1")
                        surveyaccessibilityRules.ConfigurationColumn1 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn2")
                        surveyaccessibilityRules.ConfigurationColumn2 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn3")
                        surveyaccessibilityRules.ConfigurationColumn3 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn4")
                        surveyaccessibilityRules.ConfigurationColumn4 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn5")
                        surveyaccessibilityRules.ConfigurationColumn5 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn6")
                        surveyaccessibilityRules.ConfigurationColumn6 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn7")
                        surveyaccessibilityRules.ConfigurationColumn7 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn8")
                        surveyaccessibilityRules.ConfigurationColumn8 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn9")
                        surveyaccessibilityRules.ConfigurationColumn9 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn10")
                        surveyaccessibilityRules.ConfigurationColumn10 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn11")
                        surveyaccessibilityRules.ConfigurationColumn11 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "configurationcolumn12")
                        surveyaccessibilityRules.ConfigurationColumn12 = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "area")
                        surveyaccessibilityRules.Area = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "business")
                        surveyaccessibilityRules.Business = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "emailid")
                        surveyaccessibilityRules.EmailId = accessibility.ParameterValue;
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "location")
                        surveyaccessibilityRules.Location = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "group")
                        surveyaccessibilityRules.Group = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "userid")
                        surveyaccessibilityRules.UserId = Convert.ToInt32(accessibility.ParameterValue);
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;
                    if (accessibility.AccessibilityRule.ToLower() == "mobilenumber")
                        surveyaccessibilityRules.MobileNumber = accessibility.ParameterValue;
                    surveyaccessibilityRules.Value = accessibility.ParameterValue;


                    if (await RuleExist(surveyaccessibilityRules))
                    {
                        Duplicates.Add(AndAccessibilityRules[0]);
                    }
                    else
                    {
                        //await this.AddAccessibilityRule(surveyaccessibilityRules);
                        this.db.SurveyManagementAccessibilityRule.Add(surveyaccessibilityRules);
                        await this.db.SaveChangesAsync();

                        bool IsActive = this.db.SurveyManagement.Where(a => a.Id == surveyaccessibilityRules.SurveyManagementId).Select(a => a.Status).SingleOrDefault();

                        if(IsActive == true)
                        {
                            //if (String.Equals(orgnizationCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                            ////if (orgnizationCode.ToLower().Equals("sbil"))
                            //{
                            //    urlSMS = _configuration[Configuration.NotificationApi];

                            //    urlSMS += "/CourseApplicabilitySMS";
                            //    JObject oJsonObjectSMS = new JObject();
                            //    oJsonObjectSMS.Add("CourseId", accessibilityRules.CourseId);
                            //    oJsonObjectSMS.Add("organizationCode", orgnizationCode);
                            //    HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                            //}
                            try
                            {
                                var Title1 = this.db.SurveyManagement.Where(a => a.Id == surveyaccessibilityRules.SurveyManagementId).Select(a => a.SurveySubject).SingleOrDefault();
                                APINotifications Notification = new APINotifications();
                                Notification.Title = Record.SurveyNotification;
                                Notification.Type = Record.Survey;
                                string Url1 = this._configuration[Configuration.NotificationApi];
                                Url1 = Url1 + "/tlsNotification/GetNotificationMessage/" + Record.SurveyNotification;
                                HttpResponseMessage response = await ApiHelper.CallGetAPI(Url1, token);
                                string result = await response.Content.ReadAsStringAsync();
                                Notification.Message = JsonConvert.DeserializeObject(result).ToString();
                                Notification.Message = Notification.Message.Replace("[surveySubject]", Title1);

                                Notification.Url = "social/";
                                Notification.UserId = Convert.ToInt32(userId);
                                Notification.IsRead = false;
                                Notification.SurveyId = surveyaccessibilityRules.SurveyManagementId;
                                bool IsApplicableToAll = this.db.SurveyManagement.Where(a => a.Id == surveyaccessibilityRules.SurveyManagementId).Select(a => a.IsApplicableToAll).SingleOrDefault();
                                if (IsApplicableToAll == false)
                                {
                                    int notificationID = await this.SendNotificationForSurvey(Notification, IsApplicableToAll);
                                    int NotificationId = notificationID;
                                    GuidNo = surveyaccessibilityRules.RowGuid;
                                    // List<APIApplicableNotifications> aPINotification2 = await this.SendDataForNotifications(NotificationId, Notification.UserId);
                                    await this.SendNotificationCustomizeSurvey(Notification, IsApplicableToAll, GuidNo, NotificationId, userId);

                                }
                            }
                            catch{ }
                        }
                    }
                }
            }
            if (OrAccessibilityRules.Count() > 0)
            {
                foreach (AccessibilitySurveyRules accessibility in OrAccessibilityRules)
                {
                    SurveyManagementAccessibilityRule surveymanagementaccessibilityRule = new SurveyManagementAccessibilityRule
                    {
                        // SurveyManagementId = apiAccessibility.GroupTemplateId
                    };

                    if (!string.IsNullOrEmpty(apiAccessibility.GroupTemplateId))
                    {
                        surveymanagementaccessibilityRule.GroupTemplateId = Convert.ToInt32(apiAccessibility.GroupTemplateId);
                    }
                    else
                    {
                        surveymanagementaccessibilityRule.GroupTemplateId = null;
                    }

                    if (accessibility.Condition.ToLower() != "null")
                        surveymanagementaccessibilityRule.ConditionForRules = "or";
                    surveymanagementaccessibilityRule.CreatedDate = DateTime.UtcNow;
                    bool RecordExist = false;
                    string columnName = accessibility.AccessibilityRule.ToLower();
                    var Query = db.SurveyManagementAccessibilityRule.Where(a => a.SurveyManagementId == apiAccessibility.SurveyManagementId); //&& a.IsDeleted == false);
                    switch (columnName)
                    {
                        case "configurationcolumn1":
                            surveymanagementaccessibilityRule.ConfigurationColumn1 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn1 == surveymanagementaccessibilityRule.ConfigurationColumn1);
                            break;
                        case "configurationcolumn2":

                            surveymanagementaccessibilityRule.ConfigurationColumn2 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn2 == surveymanagementaccessibilityRule.ConfigurationColumn2);
                            break;
                        case "configurationcolumn3":
                            surveymanagementaccessibilityRule.ConfigurationColumn3 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn3 == surveymanagementaccessibilityRule.ConfigurationColumn3);
                            break;
                        case "configurationcolumn4":
                            surveymanagementaccessibilityRule.ConfigurationColumn4 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn4 == surveymanagementaccessibilityRule.ConfigurationColumn4);
                            break;
                        case "configurationcolumn5":
                            surveymanagementaccessibilityRule.ConfigurationColumn5 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn5 == surveymanagementaccessibilityRule.ConfigurationColumn5);
                            break;
                        case "configurationcolumn6":
                            surveymanagementaccessibilityRule.ConfigurationColumn6 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn6 == surveymanagementaccessibilityRule.ConfigurationColumn6);
                            break;
                        case "configurationcolumn7":
                            surveymanagementaccessibilityRule.ConfigurationColumn7 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn7 == surveymanagementaccessibilityRule.ConfigurationColumn7);
                            break;
                        case "configurationcolumn8":
                            surveymanagementaccessibilityRule.ConfigurationColumn8 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn8 == surveymanagementaccessibilityRule.ConfigurationColumn8);
                            break;
                        case "configurationcolumn9":
                            surveymanagementaccessibilityRule.ConfigurationColumn9 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn9 == surveymanagementaccessibilityRule.ConfigurationColumn9);
                            break;
                        case "configurationcolumn10":
                            surveymanagementaccessibilityRule.ConfigurationColumn10 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn10 == surveymanagementaccessibilityRule.ConfigurationColumn10);
                            break;
                        case "configurationcolumn11":
                            surveymanagementaccessibilityRule.ConfigurationColumn11 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn11 == surveymanagementaccessibilityRule.ConfigurationColumn11);
                            break;
                        case "configurationcolumn12":
                            surveymanagementaccessibilityRule.ConfigurationColumn12 = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn12 == surveymanagementaccessibilityRule.ConfigurationColumn12);
                            break;
                        case "area":
                            surveymanagementaccessibilityRule.Area = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.Area == surveymanagementaccessibilityRule.Area);
                            break;
                        case "business":
                            surveymanagementaccessibilityRule.Business = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.Business == surveymanagementaccessibilityRule.Business);
                            break;
                        case "emailid":
                            surveymanagementaccessibilityRule.EmailId = accessibility.ParameterValue;
                            Query = Query.Where(x => x.EmailId == surveymanagementaccessibilityRule.EmailId);
                            break;
                        case "location":
                            surveymanagementaccessibilityRule.Location = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.Location == surveymanagementaccessibilityRule.Location);
                            break;
                        case "group":
                            surveymanagementaccessibilityRule.Group = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.Group == surveymanagementaccessibilityRule.Group && x.IsDeleted == Record.NotDeleted);
                            break;
                        case "userid":
                            surveymanagementaccessibilityRule.UserId = Convert.ToInt32(accessibility.ParameterValue);
                            Query = Query.Where(x => x.UserId == surveymanagementaccessibilityRule.UserId && x.IsDeleted == Record.NotDeleted);
                            break;
                        case "mobilenumber":
                            surveymanagementaccessibilityRule.MobileNumber = accessibility.ParameterValue;
                            Query = Query.Where(x => x.MobileNumber == surveymanagementaccessibilityRule.MobileNumber && x.IsDeleted == Record.NotDeleted);
                            break;
                    }
                    RecordExist = Query.Count() > 0 ? true : false;
                    if (!RecordExist)
                    {
                        await this.AddAccessibilityRule(surveymanagementaccessibilityRule);

                        //url = _configuration[Configuration.NotificationApi];
                        //url += "/CourseApplicability";
                        //JObject oJsonObject = new JObject();
                        //oJsonObject.Add("CourseId", accessibilityRule.CourseId);
                        //oJsonObject.Add("organizationCode", orgnizationCode);
                        //HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

                        bool IsActive = this.db.SurveyManagement.Where(a => a.Id == surveymanagementaccessibilityRule.SurveyManagementId).Select(a => a.Status).SingleOrDefault();

                        if (IsActive == true)
                        {
                            //if (String.Equals(orgnizationCode, "sbil", StringComparison.CurrentCultureIgnoreCase))
                            ////if (orgnizationCode.ToLower().Equals("sbil"))
                            //{
                            //    urlSMS = _configuration[Configuration.NotificationApi];

                            //    urlSMS += "/CourseApplicabilitySMS";
                            //    JObject oJsonObjectSMS = new JObject();
                            //    oJsonObjectSMS.Add("CourseId", accessibilityRule.CourseId);
                            //    oJsonObjectSMS.Add("organizationCode", orgnizationCode);
                            //    HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                            //}
                            try
                            {
                                int UserId = Convert.ToInt32(surveymanagementaccessibilityRule.UserId);
                                var Title1 = this.db.SurveyManagement.Where(a => a.Id == surveymanagementaccessibilityRule.SurveyManagementId).Select(a => a.SurveySubject).SingleOrDefault();
                                APINotifications Notification = new APINotifications();
                                Notification.Title = Record.QuizNotification;
                                Notification.Type = Record.Quiz;
                                string Url = this._configuration[Configuration.NotificationApi];
                                Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.QuizNotification;
                                HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, null);
                                string result = await response.Content.ReadAsStringAsync();
                                Notification.Message = JsonConvert.DeserializeObject(result).ToString();
                                Notification.Message = Notification.Message.Replace("[surveySubject]", Title1);

                                Notification.Url = "social/";
                                Notification.UserId = Convert.ToInt32(surveymanagementaccessibilityRule.UserId);
                                Notification.IsRead = false;
                                Notification.SurveyId = surveymanagementaccessibilityRule.SurveyManagementId;
                                List<ApiNotification> aPINotification = await this.GetCountByUserId(Convert.ToInt32(surveymanagementaccessibilityRule.SurveyManagementId), userId, Notification.Message);
                                if (aPINotification != null)
                                {
                                    //var Title = this.db.SurveyManagement.Where(a => a.Id == surveymanagementaccessibilityRule.SurveyManagementId).Select(a => a.SurveySubject).SingleOrDefault();
                                    bool IsApplicableToAll = this.db.SurveyManagement.Where(a => a.Id == surveymanagementaccessibilityRule.SurveyManagementId).Select(a => a.IsApplicableToAll).SingleOrDefault();
                                    int notificationID = await this.SendNotificationForSurvey(Notification, IsApplicableToAll);
                                    int NotificationId = notificationID;
                                    GuidNo = surveymanagementaccessibilityRule.RowGuid;
                                    // int UserId = aPINotification1.Select(c => c.UserId).FirstOrDefault();
                                    //List<APIApplicableNotifications> aPINotification2 = await this.SendDataForNotifications(NotificationId, Notification.UserId);
                                    await this.SendNotificationCustomizeSurvey(Notification, IsApplicableToAll, GuidNo, NotificationId, UserId);
                                }
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        Duplicates.Add(accessibility);
                    }
                }
            }
            
            var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_SURVEY_APPLICABILITY");
            if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
            {
                bool IsActive = this.db.SurveyManagement.Where(a => a.Id == apiAccessibility.SurveyManagementId).Select(a => a.Status).SingleOrDefault();

                if(IsActive == true) 
                { 
                    if (string.Equals(orgnizationCode, "bandhan", StringComparison.CurrentCultureIgnoreCase))
                    {
                        urlSMS = _configuration[Configuration.NotificationApi];
                        urlSMS += "/SurveyApplicabilitySMS";

                        JObject jObject = new JObject();
                        jObject.Add("SurveyManagementId", apiAccessibility.SurveyManagementId);
                        jObject.Add("OrganizationCode", orgnizationCode);
                        HttpResponseMessage responsesSMS = CallAPI(urlSMS, jObject).Result;
                    }
                }
            }

            if (Duplicates.Count > 0)
                return Duplicates;
            return null;
        }
        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configurationCode });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = dt.Rows[0]["Value"].ToString();
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return value;
        }
        public async Task<int> SendNotificationForSurvey(APINotifications apiNotification, bool IsApplicabletoall)
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
                    cmd.Parameters.Add(new SqlParameter("@SurveyId", SqlDbType.Int) { Value = apiNotification.SurveyId });
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
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return Id;
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
                    cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = apiNotification.Type });


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
        public async Task<List<APIApplicableNotifications>> SendDataForNotifications(int NotificationId, int UserId)
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
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return listUserApplicability;
        }

        public async Task<SurveyManagementAccessibilityRule> AddAccessibilityRule(SurveyManagementAccessibilityRule surveymanagementaccessibilityrule)
        {
            SurveyManagementAccessibilityRule surveyManagementAccessibility = new SurveyManagementAccessibilityRule();
            //surveyManagementAccessibility.Area = surveymanagementaccessibilityrule.Area;
            //surveyManagementAccessibility.Business = surveymanagementaccessibilityrule.Business;
            //surveyManagementAccessibility.Group = surveymanagementaccessibilityrule.Group;
            //surveyManagementAccessibility.Location = surveymanagementaccessibilityrule.Location;
            //surveyManagementAccessibility.EmailId = surveymanagementaccessibilityrule.EmailId;
            //surveyManagementAccessibility.ConfigurationColumn1 = surveymanagementaccessibilityrule.ConfigurationColumn1;
            //surveyManagementAccessibility.ConfigurationColumn2 = surveymanagementaccessibilityrule.ConfigurationColumn2;
            //surveyManagementAccessibility.ConfigurationColumn3 = surveymanagementaccessibilityrule.ConfigurationColumn3;
            //surveyManagementAccessibility.ConfigurationColumn4 = surveymanagementaccessibilityrule.ConfigurationColumn4;
            //surveyManagementAccessibility.ConfigurationColumn5 = surveymanagementaccessibilityrule.ConfigurationColumn5;
            //surveyManagementAccessibility.ConfigurationColumn6 = surveymanagementaccessibilityrule.ConfigurationColumn6;
            //surveyManagementAccessibility.ConfigurationColumn7 = surveymanagementaccessibilityrule.ConfigurationColumn7;
            //surveyManagementAccessibility.ConfigurationColumn8 = surveymanagementaccessibilityrule.ConfigurationColumn8;
            //surveyManagementAccessibility.ConfigurationColumn9 = surveymanagementaccessibilityrule.ConfigurationColumn9;
            //surveyManagementAccessibility.ConfigurationColumn10 = surveymanagementaccessibilityrule.ConfigurationColumn10;
            //surveyManagementAccessibility.ConfigurationColumn11 = surveymanagementaccessibilityrule.ConfigurationColumn11;
            //surveyManagementAccessibility.ConfigurationColumn12 = surveymanagementaccessibilityrule.ConfigurationColumn12;
            //surveyManagementAccessibility.UserId = surveymanagementaccessibilityrule.UserId;
            //surveyManagementAccessibility.MobileNumber = surveymanagementaccessibilityrule.MobileNumber;
            //surveyManagementAccessibility.SurveyManagementId = surveymanagementaccessibilityrule.SurveyManagementId;
            this.db.SurveyManagementAccessibilityRule.Add(surveymanagementaccessibilityrule);
            await this.db.SaveChangesAsync();
            return (surveymanagementaccessibilityrule);

        }
        public async Task<List<ApiNotification>> GetCountByUserId(int SurveyManagementId, int UserId, string Message)
        {
            List<ApiNotification> listUserApplicability = new List<ApiNotification>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "InsertQuizAndSurveyNotifications";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SurveyManagementId", SqlDbType.Int) { Value = SurveyManagementId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar) { Value = Message });
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            ApiNotification rule = new ApiNotification();
                            rule.Title = Security.Decrypt(row["Title"].ToString());
                            // rule.UserName = row["UserName"].ToString();
                            listUserApplicability.Add(rule);
                        }
                    }
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return listUserApplicability;
        }

        public async Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int SurveyManagementId)
        {
            bool isvalid = true;

            if (db.SurveyManagement.Where(y => y.Id == SurveyManagementId && y.IsDeleted == false).Count() <= 0)
            {
                isvalid = false;
                return isvalid;
            }

            try
            {
                using (var dbContext = _customerConnectionString.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "CheckValidDataForUserSetting";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter1", SqlDbType.VarChar) { Value = AccessibilityParameter1 });
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityValue1", SqlDbType.VarChar) { Value = AccessibilityValue1 });
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityParameter2", SqlDbType.VarChar) { Value = AccessibilityParameter2 });
                            cmd.Parameters.Add(new SqlParameter("@AccessibilityValue2", SqlDbType.VarChar) { Value = AccessibilityValue2 });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count > 0)
                            {
                                isvalid = Boolean.Parse(dt.Rows[0]["IsValid"].ToString());
                            }
                            reader.Dispose();
                            connection.Close();
                        }
                    }
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return isvalid;
        }
        public async Task<bool> RuleExist(SurveyManagementAccessibilityRule accessibilityRule)
        {
            IQueryable<SurveyManagementAccessibilityRule> Query = this.db.SurveyManagementAccessibilityRule.Where(a => a.SurveyManagementId == accessibilityRule.SurveyManagementId);

            if (accessibilityRule.Area != null)
                Query = Query.Where(a => a.Area == accessibilityRule.Area);
            if (accessibilityRule.Business != null)
                Query = Query.Where(a => a.Business == accessibilityRule.Business);
            if (accessibilityRule.ConfigurationColumn1 != null)
                Query = Query.Where(a => a.ConfigurationColumn1 == accessibilityRule.ConfigurationColumn1);
            if (accessibilityRule.ConfigurationColumn2 != null)
                Query = Query.Where(a => a.ConfigurationColumn2 == accessibilityRule.ConfigurationColumn2);
            if (accessibilityRule.ConfigurationColumn3 != null)
                Query = Query.Where(a => a.ConfigurationColumn3 == accessibilityRule.ConfigurationColumn3);
            if (accessibilityRule.ConfigurationColumn4 != null)
                Query = Query.Where(a => a.ConfigurationColumn4 == accessibilityRule.ConfigurationColumn4);
            if (accessibilityRule.ConfigurationColumn5 != null)
                Query = Query.Where(a => a.ConfigurationColumn5 == accessibilityRule.ConfigurationColumn5);
            if (accessibilityRule.ConfigurationColumn6 != null)
                Query = Query.Where(a => a.ConfigurationColumn6 == accessibilityRule.ConfigurationColumn6);
            if (accessibilityRule.ConfigurationColumn7 != null)
                Query = Query.Where(a => a.ConfigurationColumn7 == accessibilityRule.ConfigurationColumn7);
            if (accessibilityRule.ConfigurationColumn8 != null)
                Query = Query.Where(a => a.ConfigurationColumn8 == accessibilityRule.ConfigurationColumn8);
            if (accessibilityRule.ConfigurationColumn9 != null)
                Query = Query.Where(a => a.ConfigurationColumn9 == accessibilityRule.ConfigurationColumn9);
            if (accessibilityRule.ConfigurationColumn10 != null)
                Query = Query.Where(a => a.ConfigurationColumn10 == accessibilityRule.ConfigurationColumn10);
            if (accessibilityRule.ConfigurationColumn11 != null)
                Query = Query.Where(a => a.ConfigurationColumn11 == accessibilityRule.ConfigurationColumn11);
            if (accessibilityRule.ConfigurationColumn12 != null)
                Query = Query.Where(a => a.ConfigurationColumn12 == accessibilityRule.ConfigurationColumn12);
            if (accessibilityRule.MobileNumber != null)
                Query = Query.Where(a => a.MobileNumber == accessibilityRule.MobileNumber);
            if (accessibilityRule.EmailId != null)
                Query = Query.Where(a => a.EmailId == accessibilityRule.EmailId);
            if (accessibilityRule.Location != null)
                Query = Query.Where(a => a.Location == accessibilityRule.Location);
            if (accessibilityRule.Group != null)
                Query = Query.Where(a => a.Group == accessibilityRule.Group);
            if (accessibilityRule.UserId != null)
                Query = Query.Where(a => a.UserId == accessibilityRule.UserId);

            //  Query = Query.Where(a => a.CourseId == accessibilityRule.CourseId && a.IsDeleted == Record.NotDeleted);
            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }
        public async Task<List<APISurveyAccessibilityRules>> GetAccessibilityRules(int courseId, string orgnizationCode, string token, int Page, int PageSize)
        {
            var Result = await (from accessibiltyRule in db.SurveyManagementAccessibilityRule
                                join surveymanagement in db.SurveyManagement on accessibiltyRule.SurveyManagementId equals surveymanagement.Id
                                into c
                                from course in c.DefaultIfEmpty()
                                where accessibiltyRule.SurveyManagementId == courseId && accessibiltyRule.IsDeleted == false //&& (accessibiltyRule.Area == null || accessibiltyRule.Area != null)
                                select new
                                {
                                    accessibiltyRule.ConfigurationColumn1,
                                    accessibiltyRule.ConfigurationColumn2,
                                    accessibiltyRule.ConfigurationColumn3,
                                    accessibiltyRule.ConfigurationColumn4,
                                    accessibiltyRule.ConfigurationColumn5,
                                    accessibiltyRule.ConfigurationColumn6,
                                    accessibiltyRule.ConfigurationColumn7,
                                    accessibiltyRule.ConfigurationColumn8,
                                    accessibiltyRule.ConfigurationColumn9,
                                    accessibiltyRule.ConfigurationColumn10,
                                    accessibiltyRule.ConfigurationColumn11,
                                    accessibiltyRule.ConfigurationColumn12,
                                    accessibiltyRule.Area,
                                    accessibiltyRule.Business,
                                    accessibiltyRule.EmailId,
                                    accessibiltyRule.MobileNumber,
                                    accessibiltyRule.Location,
                                    accessibiltyRule.Group,
                                    accessibiltyRule.UserId,
                                    accessibiltyRule.ConditionForRules,
                                    accessibiltyRule.SurveyManagementId,
                                    accessibiltyRule.Id,
                                    course.SurveySubject
                                }).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            //var ResultForGroupApplicability = await (from accessibiltyRule in db.SurveyManagementAccessibilityRule
            //                                         join course in db.SurveyManagement on accessibiltyRule.SurveyManagementId equals course.Id

            //                                         where (accessibiltyRule.GroupTemplateId != null && accessibiltyRule.GroupTemplateId != 0)  && accessibiltyRule.IsDeleted == false
            //                                         select new
            //                                         {
            //                                             accessibiltyRule.SurveyManagementId,
            //                                             accessibiltyRule.Id,
            //                                             course.SurveySubject,
            //                                             accessibiltyRule.GroupTemplateId,
            //                                             //applicabilityGroupTemplate.ApplicabilityGroupName
            //                                         }).ToListAsync();
            List<APISurveyAccessibilityRules> AccessibilityRules = new List<APISurveyAccessibilityRules>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int SurveyManagementId = 0;
                int Id = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower() == "surveymanagementid")
                        SurveyManagementId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower() == "id")
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        rule.Name != "ConditionForRules" &&
                        rule.Name != "SurveySubject" &&
                        rule.Name != "SurveyManagementId" &&
                        rule.Name != "Id")
                    {
                        Rules Rule = new Rules
                        {
                            AccessibilityParameter = rule.Name,
                            AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                            Condition = Condition
                        };
                        Rules.Add(Rule);
                    }
                }
                if (Rules.Count == 2)
                {
                    APISurveyAccessibilityRules ApiRule = new APISurveyAccessibilityRules
                    {
                        SurveyManagementId = SurveyManagementId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                        // ApiRule.AccessibilityValue1 = (Rules.ElementAt(0).AccessibilityValue);
                        Condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue)
                    };
                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    APISurveyAccessibilityRules ApiRule = new APISurveyAccessibilityRules
                    {
                        SurveyManagementId = SurveyManagementId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue)
                    };

                    AccessibilityRules.Add(ApiRule);
                }
            }
            string UserUrls = _configuration[APIHelper.UserAPI];
            string settings = "setting/1/20/";
            UserUrls += settings;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrls, token);
            List<ConfiguredColumns> ConfiguredColumns = new List<ConfiguredColumns>();
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ConfiguredColumns = JsonConvert.DeserializeObject<List<ConfiguredColumns>>(result);
            }
            foreach (APISurveyAccessibilityRules AccessRule in AccessibilityRules)
            {
                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = AccessRule.AccessibilityParameter1;
                int Value = AccessRule.AccessibilityValueId1;
                string Apiurl = UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value;
                response = await APIHelper.CallGetAPI(Apiurl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Title _Title = JsonConvert.DeserializeObject<Title>(result);
                    AccessRule.AccessibilityParameter1 = AccessRule.AccessibilityParameter1;
                    AccessRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                }

                if (AccessRule.AccessibilityValueId2 != 0)
                {
                    ColumnName = AccessRule.AccessibilityParameter2;
                    Value = AccessRule.AccessibilityValueId2;
                    response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                }
                //if (AccessRule.AccessibilityParameter1 == "UserId")
                //{
                //    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => AccessRule.AccessibilityParameter1.ToLower() == c.ConfiguredColumnName.ToLower()).Select(c => c.ChangedColumnName).FirstOrDefault();
                //    AccessRule.AccessibilityParameter1 = "UserId";


                //}
                //else if(AccessRule.AccessibilityParameter1 != "UserId")
                //{
                //    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => AccessRule.AccessibilityParameter1.ToLower() == c.ConfiguredColumnName).Select(c => c.ChangedColumnName).FirstOrDefault();

                //}
                //else if (AccessRule.AccessibilityParameter2 == "UserId")
                //{
                //    AccessRule.AccessibilityParameter2 = "UserId";
                //    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => AccessRule.AccessibilityParameter1.ToLower() == c.ConfiguredColumnName).Select(c => c.ChangedColumnName).FirstOrDefault();
                //    //AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();

                //}
                if (ConfiguredColumns.Count > 0)
                {
                    if (AccessRule.AccessibilityParameter1 == "UserId")
                    {
                        AccessRule.AccessibilityParameter1 = "UserId";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "MobileNumber")
                    {
                        AccessRule.AccessibilityParameter1 = "MobileNumber";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "EmailId")
                    {
                        AccessRule.AccessibilityParameter1 = "EmailId";
                    }
                    else
                    {
                        AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter1, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                    if (AccessRule.AccessibilityParameter2 == "UserId")
                    {
                        AccessRule.AccessibilityParameter2 = "UserId";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "MobileNumber")
                    {
                        AccessRule.AccessibilityParameter1 = "MobileNumber";
                    }
                    else if (AccessRule.AccessibilityParameter1 == "EmailId")
                    {
                        AccessRule.AccessibilityParameter1 = "EmailId";
                    }
                    else
                    {
                        AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                }
            }
            return AccessibilityRules;
        }






        //if (ResultForGroupApplicability != null)
        //{
        //    foreach (var item in ResultForGroupApplicability)
        //    {
        //        int SurveyManagementId = 0;

        //        SurveyManagementId = Int32.Parse(item.SurveyManagementId.ToString());

        //        APISurveyAccessibilityRules accessRule = new APISurveyAccessibilityRules
        //        {
        //            Id = item.Id,
        //           // AccessibilityParameter1 = "Group Template Name",
        //           // AccessibilityValue1 = item.ApplicabilityGroupName,
        //           // AccessibilityValueId1 = Int32.Parse(item.GroupTemplateId.ToString()),
        //            SurveyManagementId = SurveyManagementId
        //        };
        //        AccessibilityRules.Add(accessRule);
        //    }
        //}



        public async Task<int> GetAccessibilityRulesCount(int SurveyManagementId)
        {
            int Count = 0;
            Count = await (from surveyaccessibility in db.SurveyManagementAccessibilityRule
                           join course in db.SurveyManagement on surveyaccessibility.SurveyManagementId equals course.Id
                           into c
                           from course in c.DefaultIfEmpty()
                           where surveyaccessibility.SurveyManagementId == SurveyManagementId && surveyaccessibility.IsDeleted == false

                           select new
                           {
                               surveyaccessibility.ConfigurationColumn1,
                               surveyaccessibility.ConfigurationColumn2,
                               surveyaccessibility.ConfigurationColumn3,
                               surveyaccessibility.ConfigurationColumn4,
                               surveyaccessibility.ConfigurationColumn5,
                               surveyaccessibility.ConfigurationColumn6,
                               surveyaccessibility.ConfigurationColumn7,
                               surveyaccessibility.ConfigurationColumn8,
                               surveyaccessibility.ConfigurationColumn9,
                               surveyaccessibility.ConfigurationColumn10,
                               surveyaccessibility.ConfigurationColumn11,
                               surveyaccessibility.ConfigurationColumn12,
                               surveyaccessibility.Area,
                               surveyaccessibility.Business,
                               surveyaccessibility.EmailId,
                               surveyaccessibility.MobileNumber,
                               surveyaccessibility.Location,
                               surveyaccessibility.Group,
                               surveyaccessibility.UserId,
                               surveyaccessibility.ConditionForRules,
                               surveyaccessibility.SurveyManagementId,
                               surveyaccessibility.Id,
                               course.SurveySubject
                           }).CountAsync();
            return Count;
        }

        public async Task<List<SurveyApplicableUser>> GetSurveyApplicableUserList(int SurveyManagementId)
        {
            List<SurveyApplicableUser> listUserApplicability = new List<SurveyApplicableUser>();

            var connection = this.db.Database.GetDbConnection();
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "GetSurveyApplicableUserList_Export";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SurveyManagementId", SqlDbType.BigInt) { Value = SurveyManagementId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            SurveyApplicableUser rule = new SurveyApplicableUser();
                            rule.UserID = Security.Decrypt(row["UserID"].ToString());
                            rule.UserName = row["UserName"].ToString();
                            listUserApplicability.Add(rule);
                        }
                    }
                    reader.Dispose();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return listUserApplicability;
        }
        public FileInfo GetSurveyApplicableExcel(List<APISurveyAccessibilityRules> surveyApplicableUsers, List<SurveyApplicableUser> applicableUsers, string surveysubject, string OrgCode)
        {

            //var surveysubject = this.db.SurveyManagement.Where(a => a.Id == Id).Select(a => a.SurveySubject).FirstOrDefaultAsync();

            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            string sFileName = @"SurveyApplicability.xlsx";
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SurveyApplicability");
                //First add the headers
                int row = 1, column = 1;
                worksheet.Cells[row, column].Value = "SurveyName";
                row++;
                worksheet.Cells[row, column].Value = surveysubject;
                row++;
                column = 1;
                row++;
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value1";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Additional Criteria";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Parameter2";
                worksheet.Cells[row, column].Style.Font.Bold = true;
                worksheet.Cells[row, column++].Value = "Accessibility Value2";
                worksheet.Cells[row, column].Style.Font.Bold = true;

                foreach (APISurveyAccessibilityRules course in surveyApplicableUsers)
                {
                    column = 1; row++;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter1 == null ? "-" : course.AccessibilityParameter1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue1 == null ? "-" : course.AccessibilityValue1;
                    worksheet.Cells[row, column++].Value = course.Condition1 == null ? "-" : course.Condition1;
                    worksheet.Cells[row, column++].Value = course.AccessibilityParameter2 == null ? "-" : course.AccessibilityParameter2;
                    worksheet.Cells[row, column++].Value = course.AccessibilityValue2 == null ? "-" : course.AccessibilityValue2;



                }
                row++;
                row++;

                worksheet.Cells[row, 1].Value = "Applicable Users";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                row++;
                worksheet.Cells[row, 1].Value = "UserId";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Value = "UserName";
                worksheet.Cells[row, 2].Style.Font.Bold = true;

                foreach (SurveyApplicableUser surveyapplicableusers in applicableUsers)
                {
                    row++; column = 1;
                    worksheet.Cells[row, column++].Value = surveyapplicableusers.UserID == null ? "-" : surveyapplicableusers.UserID;
                    worksheet.Cells[row, column++].Value = surveyapplicableusers.UserName == null ? "-" : surveyapplicableusers.UserName;

                }

                using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                {
                    rngitems.Style.Font.Bold = true;

                }

                package.Save(); //Save the workbook.

            }
            return file;

        }
        public async Task<string> GetSurveySubject(int surveymanagementId)
        {
            var SurveyName = await (from c in db.SurveyManagement
                                    where c.IsDeleted == false && c.Id == surveymanagementId
                                    select c.SurveySubject).SingleOrDefaultAsync();
            return SurveyName;
        }
        public async Task<List<object>> GetSurveyAccessibility(int page, int pageSize, string search = null, string filter = null)
        {
            var Query = db.SurveyManagementAccessibilityRule.Join(db.SurveyManagement, r => r.SurveyManagementId, (p => p.Id), (r, p) => new { r, p })

                   .Where(c => (c.r.IsDeleted == false))
                    .GroupBy(od => new
                    {
                        od.p.Id,
                        od.r.SurveyManagementId,
                        od.p.SurveySubject
                    })
                    .OrderByDescending(a => a.Key.Id)
                   .Select(m => new APISurveys
                   {
                       Id = m.Key.Id,
                       SurveyManagementId = m.Key.SurveyManagementId,
                       SurveySubject = m.Key.SurveySubject,
                       // IsDeleted=m.Key.
                   });

            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(r => r.SurveySubject.ToLower().Contains(search.ToLower()));
                }
            }

            if (page != -1)
                Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
            if (pageSize != -1)
                Query = Query.Take(Convert.ToInt32(pageSize));

            return await Query.ToListAsync<object>();
        }

        public async Task<int> count(string search = null, string filter = null)
        {
            var Query = db.SurveyManagementAccessibilityRule.Join(db.SurveyManagement, r => r.SurveyManagementId, (p => p.Id), (r, p) => new { r, p })

                   .Where(c => (c.r.IsDeleted == false))
                    .GroupBy(od => new
                    {
                        od.p.Id,
                        od.r.SurveyManagementId,
                        od.p.SurveySubject
                    })
                    .OrderByDescending(a => a.Key.Id)
                   .Select(m => new APISurveys
                   {
                       Id = m.Key.Id,
                       SurveyManagementId = m.Key.SurveyManagementId,
                       SurveySubject = m.Key.SurveySubject,
                       // IsDeleted=m.Key.
                   });
            if (!string.IsNullOrEmpty(search))
            {

                Query = Query.Where(v => v.SurveySubject.StartsWith(search) || search == null);
                Query = Query.OrderByDescending(v => v.Id);
            }

            Query = Query.OrderByDescending(v => v.Id);

            return await Query.CountAsync();

        }

        public async Task<int> DeleteRule(int roleId)
        {
            try
            {
                IQueryable<SurveyManagementAccessibilityRule> accessibilityRules = await this.GetRules(roleId);
                foreach (var result in accessibilityRules)
                {
                    using (var dbContext = this._customerConnectionString.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                dbContext.Database.ExecuteSqlCommand("Update Gadget.SurveyManagementAccessibilityRule set IsDeleted = 1 where Id = " + result.Id);
                            }
                        }
                    }
                }


                //return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

            }
            return 1;
        }

        public async Task<IQueryable<SurveyManagementAccessibilityRule>> GetRules(int roleId)
        {
            var query = (from accessibiltyRule in db.SurveyManagementAccessibilityRule
                         where accessibiltyRule.Id == roleId
                         select new SurveyManagementAccessibilityRule
                         {
                             Area = accessibiltyRule.Area,
                             Business = accessibiltyRule.Business,
                             ConfigurationColumn1 = accessibiltyRule.ConfigurationColumn1,
                             ConfigurationColumn3 = accessibiltyRule.ConfigurationColumn3,
                             ConfigurationColumn2 = accessibiltyRule.ConfigurationColumn2,
                             ConfigurationColumn4 = accessibiltyRule.ConfigurationColumn4,
                             ConfigurationColumn5 = accessibiltyRule.ConfigurationColumn5,
                             ConfigurationColumn6 = accessibiltyRule.ConfigurationColumn6,
                             ConfigurationColumn7 = accessibiltyRule.ConfigurationColumn7,
                             ConfigurationColumn8 = accessibiltyRule.ConfigurationColumn8,
                             ConfigurationColumn9 = accessibiltyRule.ConfigurationColumn9,
                             ConfigurationColumn10 = accessibiltyRule.ConfigurationColumn10,
                             ConfigurationColumn11 = accessibiltyRule.ConfigurationColumn11,
                             ConfigurationColumn12 = accessibiltyRule.ConfigurationColumn12,
                             SurveyManagementId = accessibiltyRule.SurveyManagementId,
                             EmailId = accessibiltyRule.EmailId,
                             UserId = accessibiltyRule.UserId,
                             Location = accessibiltyRule.Location,
                             MobileNumber = accessibiltyRule.MobileNumber,
                             Value = accessibiltyRule.Value,
                             Id = accessibiltyRule.Id


                         });
            return query;
        }

        public async Task<int> SendSurveyApplicabilityPushNotification(int SurveyManagementId, string orgCode)
        {
            string Url = this._configuration[Configuration.NotificationApi];

            Url += "/SurveyApplicabilityPushNotification";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("SurveyManagementId", SurveyManagementId);
            oJsonObject.Add("OrganizationCode", orgCode);
            HttpResponseMessage responses = CallAPI(Url, oJsonObject).Result;
            return 1;
        }

        public async Task<List<APISurveyAccessibilityRules>> GetAccessibilityRulesForExport(int courseId, string orgnizationCode, string token)
        {
            var Result = await (from accessibiltyRule in db.SurveyManagementAccessibilityRule
                                join surveymanagement in db.SurveyManagement on accessibiltyRule.SurveyManagementId equals surveymanagement.Id
                                into c
                                from course in c.DefaultIfEmpty()
                                where accessibiltyRule.SurveyManagementId == courseId && accessibiltyRule.IsDeleted == false
                                select new
                                {
                                    accessibiltyRule.ConfigurationColumn1,
                                    accessibiltyRule.ConfigurationColumn2,
                                    accessibiltyRule.ConfigurationColumn3,
                                    accessibiltyRule.ConfigurationColumn4,
                                    accessibiltyRule.ConfigurationColumn5,
                                    accessibiltyRule.ConfigurationColumn6,
                                    accessibiltyRule.ConfigurationColumn7,
                                    accessibiltyRule.ConfigurationColumn8,
                                    accessibiltyRule.ConfigurationColumn9,
                                    accessibiltyRule.ConfigurationColumn10,
                                    accessibiltyRule.ConfigurationColumn11,
                                    accessibiltyRule.ConfigurationColumn12,
                                    accessibiltyRule.Area,
                                    accessibiltyRule.Business,
                                    accessibiltyRule.EmailId,
                                    accessibiltyRule.MobileNumber,
                                    accessibiltyRule.Location,
                                    accessibiltyRule.Group,
                                    accessibiltyRule.UserId,
                                    accessibiltyRule.ConditionForRules,
                                    accessibiltyRule.SurveyManagementId,
                                    accessibiltyRule.Id,
                                    course.SurveySubject
                                }).ToListAsync();

            //var ResultForGroupApplicability = await (from accessibiltyRule in db.SurveyManagementAccessibilityRule
            //                                         join course in db.SurveyManagement on accessibiltyRule.SurveyManagementId equals course.Id

            //                                         where (accessibiltyRule.GroupTemplateId != null && accessibiltyRule.GroupTemplateId != 0)  && accessibiltyRule.IsDeleted == false
            //                                         select new
            //                                         {
            //                                             accessibiltyRule.SurveyManagementId,
            //                                             accessibiltyRule.Id,
            //                                             course.SurveySubject,
            //                                             accessibiltyRule.GroupTemplateId,
            //                                             //applicabilityGroupTemplate.ApplicabilityGroupName
            //                                         }).ToListAsync();
            List<APISurveyAccessibilityRules> AccessibilityRules = new List<APISurveyAccessibilityRules>();
            foreach (var AccessRule in Result)
            {
                string Condition = AccessRule.ConditionForRules;
                PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                List<Rules> Rules = new List<Rules>();
                int SurveyManagementId = 0;
                int Id = 0;
                foreach (PropertyInfo rule in properties)
                {
                    if (rule.Name.ToLower() == ("surveymanagementid"))
                        SurveyManagementId = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.Name.ToLower() == "id")
                        Id = Int32.Parse(rule.GetValue(AccessRule).ToString());
                    if (rule.GetValue(AccessRule, null) != null &&
                        (rule.Name != "ConditionForRules") &&
                        (rule.Name != "SurveySubject") &&
                        (rule.Name != "SurveyManagementId") &&
                        (rule.Name != "Id"))
                    {
                        Rules Rule = new Rules
                        {
                            AccessibilityParameter = rule.Name,
                            AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                            Condition = Condition
                        };
                        Rules.Add(Rule);
                    }
                }
                if (Rules.Count == 2)
                {
                    APISurveyAccessibilityRules ApiRule = new APISurveyAccessibilityRules
                    {
                        SurveyManagementId = SurveyManagementId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue),
                        // ApiRule.AccessibilityValue1 = (Rules.ElementAt(0).AccessibilityValue);
                        Condition1 = "and",
                        AccessibilityParameter2 = Rules.ElementAt(1).AccessibilityParameter,
                        AccessibilityValueId2 = Int32.Parse(Rules.ElementAt(1).AccessibilityValue)
                    };
                    AccessibilityRules.Add(ApiRule);
                }
                else if (Rules.Count == 1)
                {
                    APISurveyAccessibilityRules ApiRule = new APISurveyAccessibilityRules
                    {
                        SurveyManagementId = SurveyManagementId,
                        Id = Id,
                        AccessibilityParameter1 = Rules.ElementAt(0).AccessibilityParameter,
                        AccessibilityValueId1 = Int32.Parse(Rules.ElementAt(0).AccessibilityValue)
                    };

                    AccessibilityRules.Add(ApiRule);
                }
            }
            string UserUrls = _configuration[APIHelper.UserAPI];
            string settings = "setting/1/20/";
            UserUrls += settings;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrls, token);
            List<ConfiguredColumns> ConfiguredColumns = new List<ConfiguredColumns>();
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ConfiguredColumns = JsonConvert.DeserializeObject<List<ConfiguredColumns>>(result);
            }
            foreach (APISurveyAccessibilityRules AccessRule in AccessibilityRules)
            {
                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = AccessRule.AccessibilityParameter1;
                int Value = AccessRule.AccessibilityValueId1;
                string Apiurl = UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value;
                response = await APIHelper.CallGetAPI(Apiurl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Title _Title = JsonConvert.DeserializeObject<Title>(result);
                    AccessRule.AccessibilityParameter1 = AccessRule.AccessibilityParameter1;
                    AccessRule.AccessibilityValue1 = _Title == null ? null : _Title.Name;
                }

                if (AccessRule.AccessibilityValueId2 != 0)
                {
                    ColumnName = AccessRule.AccessibilityParameter2;
                    Value = AccessRule.AccessibilityValueId2;
                    response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        AccessRule.AccessibilityValue2 = _Title == null ? null : _Title.Name;
                    }
                }
                if (AccessRule.AccessibilityParameter1 == "UserId")
                {
                    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => AccessRule.AccessibilityParameter1.ToLower() == c.ConfiguredColumnName).Select(c => c.ChangedColumnName).FirstOrDefault();
                    AccessRule.AccessibilityParameter1 = "UserId";


                }
                else if (AccessRule.AccessibilityParameter2 == "UserId")
                {
                    AccessRule.AccessibilityParameter2 = "UserId";
                    AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => AccessRule.AccessibilityParameter1.ToLower() == c.ConfiguredColumnName).Select(c => c.ChangedColumnName).FirstOrDefault();
                    //AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => String.Equals(AccessRule.AccessibilityParameter2, c.ConfiguredColumnName, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.ChangedColumnName).FirstOrDefault();

                }

                else
                {
                    if (AccessRule.AccessibilityParameter1 != null)
                    {
                        AccessRule.AccessibilityParameter1 = ConfiguredColumns.Where(c => AccessRule.AccessibilityParameter1.ToLower() == c.ConfiguredColumnName).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                    if (AccessRule.AccessibilityParameter2 != null)
                    {
                        AccessRule.AccessibilityParameter2 = ConfiguredColumns.Where(c => AccessRule.AccessibilityParameter2.ToLower() == c.ConfiguredColumnName).Select(c => c.ChangedColumnName).FirstOrDefault();
                    }
                }

            }

            //if (ResultForGroupApplicability != null)
            //{
            //    foreach (var item in ResultForGroupApplicability)
            //    {
            //        int SurveyManagementId = 0;

            //        SurveyManagementId = Int32.Parse(item.SurveyManagementId.ToString());

            //        APISurveyAccessibilityRules accessRule = new APISurveyAccessibilityRules
            //        {
            //            Id = item.Id,
            //           // AccessibilityParameter1 = "Group Template Name",
            //           // AccessibilityValue1 = item.ApplicabilityGroupName,
            //           // AccessibilityValueId1 = Int32.Parse(item.GroupTemplateId.ToString()),
            //            SurveyManagementId = SurveyManagementId
            //        };
            //        AccessibilityRules.Add(accessRule);
            //    }
            //}
            return AccessibilityRules;
        }





        // ------------------------------------- Nested Survey --------------------------------------

        public async Task<List<APINestedSurveyQuestions>> GetSurveyQuestionsByLcmsId(int LcmsId)
        {
            try
            {
                List<SurveyConfiguration> nestedSurveyQuestions = new List<SurveyConfiguration>();
                List<APINestedSurveyQuestions> apiNestedSurveyQuestions = new List<APINestedSurveyQuestions>();
                APINestedSurveyQuestions surveyLcmsQuestion = new APINestedSurveyQuestions();
                nestedSurveyQuestions = await this.db.SurveyConfiguration.Where(x => x.LcmsId == LcmsId).Select(x => new SurveyConfiguration { Id = x.Id, QuestionId = x.QuestionId, IsRoot = x.IsRoot, LcmsId = x.LcmsId, IsDeleted = x.IsDeleted, CreatedBy = x.CreatedBy, CreatedDate = x.CreatedDate, ModifiedBy = x.ModifiedBy, ModifiedDate = x.ModifiedDate }).ToListAsync();
                for (int i = 0; i < nestedSurveyQuestions.Count; i++)
                {
                    APINestedSurveyQuestions apiNestedSurveyQuestion = new APINestedSurveyQuestions();
                    surveyLcmsQuestion = await GetQuestionByQuestionId(nestedSurveyQuestions[i].QuestionId);
                    apiNestedSurveyQuestion.QuestionId = nestedSurveyQuestions[i].QuestionId;
                    apiNestedSurveyQuestion.Question = surveyLcmsQuestion.Question;
                    apiNestedSurveyQuestion.LcmsId = nestedSurveyQuestions[i].LcmsId;
                    apiNestedSurveyQuestion.IsRoot = nestedSurveyQuestions[i].IsRoot;
                    apiNestedSurveyQuestions.Add(apiNestedSurveyQuestion);
                }
                return apiNestedSurveyQuestions;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<APINestedSurveyQuestions> GetQuestionByQuestionId(int Questionid)
        {
            try
            {
                List<SurveyQuestion> surveyQuestion = new List<SurveyQuestion>();
                APINestedSurveyQuestions surveyLcmsQuestion = new APINestedSurveyQuestions();
                surveyQuestion = await this.db.SurveyQuestion.Where(x => x.Id == Questionid).Select(x => new SurveyQuestion { Id = x.Id, Question = x.Question }).ToListAsync();
                surveyLcmsQuestion.QuestionId = surveyQuestion[0].Id;
                surveyLcmsQuestion.Question = surveyQuestion[0].Question;
                return surveyLcmsQuestion;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APINestedSurveyOptions>> GetOptionsByLcmsQuestion(int LcmsId, int QuestionId)
        {
            try
            {
                List<SurveyOption> nestedSurveyOptions = new List<SurveyOption>();
                List<APINestedSurveyOptions> apiNestedSurveyOptions = new List<APINestedSurveyOptions>();
                nestedSurveyOptions = await this.db.SurveyOption.Where(x => x.QuestionId == QuestionId).Select(x => new SurveyOption { Id = x.Id, OptionText = x.OptionText, QuestionId = x.QuestionId, IsDeleted = x.IsDeleted, CreatedBy = x.CreatedBy, CreatedDate = x.CreatedDate, ModifiedBy = x.ModifiedBy, ModifiedDate = x.ModifiedDate }).ToListAsync();
                for (int i = 0; i < nestedSurveyOptions.Count; i++)
                {
                    APINestedSurveyOptions apiNestedSurveyOption = new APINestedSurveyOptions();
                    apiNestedSurveyOption.Id = nestedSurveyOptions[i].Id;
                    apiNestedSurveyOption.LcmsId = LcmsId;
                    apiNestedSurveyOption.OptionText = nestedSurveyOptions[i].OptionText;
                    apiNestedSurveyOption.QuestionId = nestedSurveyOptions[i].QuestionId;
                    apiNestedSurveyOption.NextQuestionId = await this.GetNextQuestionId(LcmsId, nestedSurveyOptions[i].Id);
                    if (apiNestedSurveyOption.NextQuestionId == -1)
                    {
                        apiNestedSurveyOption.NextQuestionId = null;
                    }
                    apiNestedSurveyOptions.Add(apiNestedSurveyOption);
                }
                return apiNestedSurveyOptions;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APINestedSurveyOptions>> GetOptionByLcmsOption(int LcmsId, int OptionId)
        {
            try
            {
                List<SurveyOption> nestedSurveyOptions = new List<SurveyOption>();
                List<APINestedSurveyOptions> apiNestedSurveyOptions = new List<APINestedSurveyOptions>();
                APINestedSurveyOptions apiNestedSurveyOption = new APINestedSurveyOptions();
                nestedSurveyOptions = await this.db.SurveyOption.Where(x => x.Id == OptionId).Select(x => new SurveyOption { Id = x.Id, OptionText = x.OptionText, QuestionId = x.QuestionId, IsDeleted = x.IsDeleted, CreatedBy = x.CreatedBy, CreatedDate = x.CreatedDate, ModifiedBy = x.ModifiedBy, ModifiedDate = x.ModifiedDate }).ToListAsync();
                for (int i = 0; i < nestedSurveyOptions.Count; i++)
                {
                    apiNestedSurveyOption.Id = nestedSurveyOptions[i].Id;
                    apiNestedSurveyOption.LcmsId = LcmsId;
                    apiNestedSurveyOption.OptionText = nestedSurveyOptions[i].OptionText;
                    apiNestedSurveyOption.QuestionId = nestedSurveyOptions[i].QuestionId;
                    apiNestedSurveyOption.NextQuestionId = await this.GetNextQuestionId(LcmsId, nestedSurveyOptions[i].Id);
                    if (apiNestedSurveyOption.NextQuestionId == -1)
                    {
                        apiNestedSurveyOption.NextQuestionId = null;
                    }
                    apiNestedSurveyOptions.Add(apiNestedSurveyOption);
                }
                return apiNestedSurveyOptions;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int?> GetNextQuestionId(int LcmsId, int OptionId)
        {
            try
            {
                List<SurveyOptionNested> surveyOptionNested = new List<SurveyOptionNested>();
                surveyOptionNested = await this.db.SurveyOptionNested.Where(x => x.LcmsId == LcmsId && x.OptionId == OptionId).Select(x => new SurveyOptionNested { OptionId = x.OptionId, LcmsId = x.LcmsId, NextQuestionId = x.NextQuestionId }).ToListAsync();

                if (surveyOptionNested.Count == 0)
                {
                    return null;
                }
                return surveyOptionNested[0].NextQuestionId;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APINestedSurveyQuestions>> GetRootQuestion(int LcmsId)
        {
            try
            {
                List<SurveyConfiguration> nestedSurveyQuestions = new List<SurveyConfiguration>();
                List<APINestedSurveyQuestions> rootQuestions = new List<APINestedSurveyQuestions>();
                APINestedSurveyQuestions rootQuestion = new APINestedSurveyQuestions();
                APINestedSurveyQuestions surveyLcmsQuestion = new APINestedSurveyQuestions();
                nestedSurveyQuestions = await this.db.SurveyConfiguration.Where(x => x.LcmsId == LcmsId && x.IsRoot == 1).Select(x => new SurveyConfiguration { Id = x.Id, QuestionId = x.QuestionId, IsRoot = x.IsRoot, LcmsId = x.LcmsId, IsDeleted = x.IsDeleted, CreatedBy = x.CreatedBy, CreatedDate = x.CreatedDate, ModifiedBy = x.ModifiedBy, ModifiedDate = x.ModifiedDate }).ToListAsync();
                for (int i = 0; i < nestedSurveyQuestions.Count; i++)
                {
                    surveyLcmsQuestion = await GetQuestionByQuestionId(nestedSurveyQuestions[i].QuestionId);
                    rootQuestion.QuestionId = nestedSurveyQuestions[i].QuestionId;
                    rootQuestion.Question = surveyLcmsQuestion.Question;
                    rootQuestion.LcmsId = nestedSurveyQuestions[i].LcmsId;
                    rootQuestion.IsRoot = nestedSurveyQuestions[i].IsRoot;
                    rootQuestions.Add(rootQuestion);
                }
                return rootQuestions;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task SubmitNestedSurvey(APINestedSurveyResult apiNestedSurveyResult, int UserId)
        {
            try
            {
                SurveyResult surveyResult = new SurveyResult
                {
                    SurveyId = apiNestedSurveyResult.SurveyId,
                    SurveyResultStatus = "Completed",
                    UserId = UserId,
                    IsDeleted = false,
                    ModifiedBy = UserId,
                    ModifiedDate = DateTime.UtcNow,
                    CreatedBy = UserId,
                    CreatedDate = DateTime.UtcNow
                };
                await surveyResultRepository.Add(surveyResult);

                List<SurveyResultDetail> surveyResultDetails = new List<SurveyResultDetail>();
                for (int i = 0; i < apiNestedSurveyResult.apiNestedSurveyResultDetail.Length; i++)
                {
                    APINestedSurveyResultDetail opt = apiNestedSurveyResult.apiNestedSurveyResultDetail[i];
                    SurveyResultDetail surveyResultDetail = new SurveyResultDetail
                    {
                        SurveyResultId = surveyResult.Id,
                        Section = "Objective",
                        ServeyQuestionId = opt.SurveyQuestionId,
                        ServeyOptionId = opt.SurveyOptionId,
                        SubjectiveAnswer = null,
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow
                    };
                    surveyResultDetails.Add(surveyResultDetail);
                }
                await surveyResultDetailRepository.AddRange(surveyResultDetails);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<bool?> IsSurveyNested(int LcmsId)
        {
            try
            {
                ApiLcms apiLcms = await this._lcmsRepository.GetLcms(LcmsId);
                return apiLcms.IsNested;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
    }

    public class SurveyConfigurationRepository : Repository<SurveyConfiguration>, ISurveyConfigurationRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SurveyConfigurationRepository));
        private GadgetDbContext db;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identitySvc;
        private ILcmsRepository _lcmsRepository;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public SurveyConfigurationRepository(GadgetDbContext context,
            IConfiguration configuration,
            ILcmsRepository lcmsRepository,
            IIdentityService identitySvc,
            ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.db = context;
            this._configuration = configuration;
            this._identitySvc = identitySvc;
            this._lcmsRepository = lcmsRepository;
            this._customerConnectionString = customerConnectionString;
        }
    }

    public class SurveyOptionNestedRepository : Repository<SurveyOptionNested>, ISurveyOptionNestedRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SurveyOptionNestedRepository));
        private GadgetDbContext db;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identitySvc;
        private ILcmsRepository _lcmsRepository;
        private ICustomerConnectionStringRepository _customerConnectionString;
        public SurveyOptionNestedRepository(GadgetDbContext context,
            IConfiguration configuration,
            ILcmsRepository lcmsRepository,
            IIdentityService identitySvc,
            ICustomerConnectionStringRepository customerConnectionString) : base(context)
        {
            this.db = context;
            this._configuration = configuration;
            this._identitySvc = identitySvc;
            this._lcmsRepository = lcmsRepository;
            this._customerConnectionString = customerConnectionString;
        }
    }
}