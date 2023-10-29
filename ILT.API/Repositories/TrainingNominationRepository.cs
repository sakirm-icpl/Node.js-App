using ILT.API.APIModel;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Model.ILT;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using ILT.API.Services;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using ILT.API.Common;
using AutoMapper;
using ILT.API.Helper.Metadata;
using Microsoft.Identity.Client;
using System.Security;
using Microsoft.Office.Core;
using Google.Apis.Calendar.v3.Data;

namespace ILT.API.Repositories
{
    public class TrainingNominationRepository : Repository<TrainingNomination>, ITrainingNomination
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TrainingNominationRepository));
        private CourseContext _db;
        IEmail _email;
        ICustomerConnectionStringRepository _customerConnection;
        IIdentityService _identitySvc;
        private IConfiguration _configuration;
        private ITLSHelper _tlsHelper;
        ICourseRepository _courseRepository;
        IILTSchedule _IILTSchedule;
        IOnlineWebinarRepository _onlineWebinarRepository;
        public TrainingNominationRepository(CourseContext context, IEmail email,
            ICustomerConnectionStringRepository customerConnection, IConfiguration Configuration,
           IIdentityService identitySvc, ITLSHelper tLSHelper, ICourseRepository courseRepository, IILTSchedule IILTSchedule, IOnlineWebinarRepository onlineWebinarRepository) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
            this._email = email;
            this._identitySvc = identitySvc;
            _configuration = Configuration;
            _tlsHelper = tLSHelper;
            _courseRepository = courseRepository;
            _IILTSchedule = IILTSchedule;
            _onlineWebinarRepository = onlineWebinarRepository;
        }
        public async Task<APIILTSchedular> GetByID(int Id)
        {
            var Query = (from ILTSchedule in this._db.ILTSchedule
                         join TrainingPlace in this._db.TrainingPlace on ILTSchedule.PlaceID equals TrainingPlace.Id into tempPlace
                         from TrainingPlace in tempPlace.DefaultIfEmpty()
                         join Module in this._db.Module on ILTSchedule.ModuleId equals Module.Id into tempModule
                         from Module in tempModule.DefaultIfEmpty()
                         where ILTSchedule.IsActive == true && ILTSchedule.IsDeleted == Record.NotDeleted && ILTSchedule.ID == Id
                         select new APIILTSchedular
                         {
                             ID = ILTSchedule.ID,
                             ModuleId = ILTSchedule.ModuleId,
                             ModuleName = Module == null ? null : Module.Name,
                             StartDate = ILTSchedule.StartDate,
                             EndDate = ILTSchedule.EndDate,
                             StartTime = ILTSchedule.StartTime.ToString(@"hh\:mm"),
                             EndTime = ILTSchedule.EndTime.ToString(@"hh\:mm"),
                             RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                             ScheduleCode = ILTSchedule.ScheduleCode,
                             PlaceID = ILTSchedule.PlaceID,
                             PlaceName = TrainingPlace == null ? null : TrainingPlace.PlaceName,
                             TrainerType = ILTSchedule.TrainerType
                         });

            var result = await Query.FirstOrDefaultAsync();

            DateTime startTime = DateTime.Today.Add(TimeSpan.Parse(result.StartTime));
            string displayStartTime = startTime.ToString("hh:mm tt"); // It will give "03:00 AM"

            DateTime endTime = DateTime.Today.Add(TimeSpan.Parse(result.EndTime));
            string displayEndTime = endTime.ToString("hh:mm tt"); // It will give "03:00 AM"

            result.StartTimeString = displayStartTime;
            result.EndTimeString = displayEndTime;

            return result;
        }

        public void Delete()
        {
            try
            {
                //Truncate Table to delete all old records.
                _db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Course].[TrainingNominationRejected]");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }

        }
        public async Task<List<SchedularTypeahead>> GetByModuleId(int ModuleId, int? CourseId, int UserId, string OrganisationCode)
        {
            List<SchedularTypeahead> ScheduleList = new List<SchedularTypeahead>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetSchdule";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = ModuleId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrganisationCode", SqlDbType.NVarChar) { Value = OrganisationCode });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return null;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                SchedularTypeahead obj = new SchedularTypeahead();

                                obj.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();

                                ScheduleList.Add(obj);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ScheduleList;
        }

        public async Task<List<SchedularTypeahead>> GetScheduleByModuleId(int Id)
        {
            var Query = (from ILTSchedule in this._db.ILTSchedule
                         where ILTSchedule.IsActive == true && ILTSchedule.IsDeleted == Record.NotDeleted && ILTSchedule.ModuleId == Id && ILTSchedule.StartDate.Date >= DateTime.Now.Date.ToLocalTime()
                         select new SchedularTypeahead
                         {
                             ID = ILTSchedule.ID,
                             ScheduleCode = ILTSchedule.ScheduleCode
                         });
            Query = Query.Distinct();
            return await Query.ToListAsync();
        }
        public async Task<List<APIILTSchedular>> GetAllActiveSchedules(int page, int pageSize, string OrganisationCode, int UserId, string searchParameter = null, string searchText = null)
        {
            List<APIILTSchedular> ScheduleList = new List<APIILTSchedular>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllActiveSchedules";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrganisationCode", SqlDbType.NVarChar) { Value = OrganisationCode });
                            cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = false });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APIILTSchedular obj = new APIILTSchedular();

                                obj.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                obj.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                obj.ModuleName = row["ModuleName"].ToString();
                                obj.StartDate = Convert.ToDateTime(row["StartDate"].ToString());
                                obj.EndDate = Convert.ToDateTime(row["EndDate"].ToString());
                                obj.StartTime = row["StartTime"].ToString();
                                obj.EndTime = row["EndTime"].ToString();
                                obj.RegistrationEndDate = Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();
                                obj.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                obj.PlaceName = row["PlaceName"].ToString();
                                obj.TrainerType = row["TrainerType"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                obj.CourseName = row["CourseName"].ToString();
                                obj.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : int.Parse(row["BatchId"].ToString());
                                obj.BatchCode = row["BatchCode"].ToString();
                                obj.BatchName = row["BatchName"].ToString();
                                obj.UserName = row["UserName"].ToString();
                                obj.UserCreated = Convert.ToBoolean(row["UserCreated"]);
                                ScheduleList.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ScheduleList;
        }

        public async Task<int> GetAllActiveSchedulesCount(string OrganisationCode, int UserId, string searchParameter = null, string searchText = null, bool showAllData = false)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllActiveSchedulesCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrganisationCode", SqlDbType.NVarChar) { Value = OrganisationCode });
                            cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = showAllData });

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
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }

        public async Task<ApiResponse> checkValidData(int ScheduleID, int ModuleID, int CourseID)
        {
            ApiResponse objApiResponse = new ApiResponse();
            string batchwiseNomination = await GetMasterConfigurableParameterValue("ENABLE_BATCHWISE_NOMINATION");
            if (string.Equals(batchwiseNomination, "no", StringComparison.CurrentCultureIgnoreCase))
            {
                int ScheduleModuleID = await this._db.ILTSchedule.Where(a => a.ID == ScheduleID).Select(a => a.ModuleId).FirstOrDefaultAsync();
                if (ScheduleModuleID != ModuleID)
                {
                    objApiResponse.StatusCode = 410;
                    objApiResponse.Description = "ModuleID is not valid";
                    return objApiResponse;
                }

                int flag = 0;
                List<CourseModuleAssociation> ModuleCourseID = await this._db.CourseModuleAssociation.Where(a => a.ModuleId == ModuleID).ToListAsync();
                foreach (CourseModuleAssociation obj in ModuleCourseID)
                {
                    if (obj.CourseId == CourseID)
                    {
                        flag = flag + 1;
                    }
                }
                if (flag == 0)
                {
                    objApiResponse.StatusCode = 411;
                    objApiResponse.Description = "CourseID is not valid";
                    return objApiResponse;
                }
            }
            return objApiResponse;
        }

        public async Task<ApiResponseILT> PostNominateUser(int id, int moduleId, int courseId, List<APIUserData> userID, int userId, string OrganisationCode, int BatchId)
        {
            string batchwiseNomination = await GetMasterConfigurableParameterValue("ENABLE_BATCHWISE_NOMINATION");
            ApiResponseILT objApiResponse = new ApiResponseILT();
            try
            {
                foreach (var item in userID)
                    item.OTP = GenerateRandomPassword();
                SqlParameter ResponseParam = null;
                DataTable UserDataTable = new DataTable();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase) && id == 0 && moduleId == 0)
                            {
                                cmd.CommandText = "[dbo].[ILTScheduleBatchwiseNomination_Insert]";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = userId });
                                cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrganisationCode });
                                cmd.Parameters.Add(new SqlParameter("@BatchId", SqlDbType.Int) { Value = BatchId });
                                cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                                cmd.Parameters.Add(new SqlParameter("@ILTNominateUserList", SqlDbType.Structured) { Value = userID.ToList().ToDataTable() });
                                ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 500);
                                ResponseParam.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(ResponseParam);
                            }
                            else
                            {
                                cmd.CommandText = "[dbo].[ILTScheduleNomination_Insert]";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = userId });
                                cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrganisationCode });
                                cmd.Parameters.Add(new SqlParameter("@ScheduleId", SqlDbType.Int) { Value = id });
                                cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = moduleId });
                                cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                                cmd.Parameters.Add(new SqlParameter("@ILTNominateUserList", SqlDbType.Structured) { Value = userID.ToList().ToDataTable() });
                                ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 500);
                                ResponseParam.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(ResponseParam);
                            }
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            UserDataTable.Load(reader);
                            connection.Close();
                        }
                    }
                }
                string ResponseValue = Convert.ToString(ResponseParam.Value);
                if (ResponseValue == "Success")
                {
                    List<APINominationResponse> aPINominationResponseList = UserDataTable.ConvertToList<APINominationResponse>();
                    List<APINominationUserResponse> aPINominationUserResponses = Mapper.Map<List<APINominationUserResponse>>(aPINominationResponseList);
                    int TotalNominated = aPINominationUserResponses.Where(x => x.Status == "Nominated").Count();
                    int TotalRejected = aPINominationUserResponses.Where(x => x.Status == "Rejected").Count();
                    aPINominationResponseList = aPINominationResponseList.Where(x => x.Status == "Nominated").ToList();
                    await this.PostILTNomination(id, aPINominationResponseList, OrganisationCode);
                    await SendCourseApplicabilityNominationNotifications(aPINominationResponseList, OrganisationCode, userId);
                    if (TotalRejected > 0)
                        objApiResponse.StatusCode = 400;
                    else
                        objApiResponse.StatusCode = 200;
                    objApiResponse.Description = "Total Users Nominated: " + TotalNominated + " and Rejected: " + TotalRejected + ".";
                    objApiResponse.aPINominationResponses = aPINominationUserResponses;
                    ApiConfigurableParameters ATPTLWCS = _db.configurableParameters.Where(a => a.Code == "ATPTLWCS").FirstOrDefault();
                    if (ATPTLWCS != null)
                    {
                        if (ATPTLWCS.Value.ToLower() == "no")
                        {
                            ApiConfigurableParameters conferenceParameters = _db.configurableParameters.Where(a => a.Code == "ONLINE_LOGIN").FirstOrDefault();
                            if (conferenceParameters.Value.ToLower() == "yes")
                            {
                                ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(a => a.ID == id && a.WebinarType == "TEAMS").FirstOrDefaultAsync();
                                ApiConfigurableParameters withoutPassword = _db.configurableParameters.Where(a => a.Code == "WUP").FirstOrDefault();
                                if (withoutPassword.Value.ToLower() == "yes")
                                {

                                    List<TeamsScheduleDetails> teamsScheduleDetailss = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID).ToList();
                                    AuthenticationResult authenticationResult = await _IILTSchedule.GetTeamsToken();
                                    foreach (TeamsScheduleDetails teamsScheduleDetails in teamsScheduleDetailss)
                                    {
                                        UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();
                                        Model.Course course = _db.Course.Where(a => a.Id == courseId).FirstOrDefault();
                                        Teams teams1 = new Teams();
                                        teams1.CourseID = courseId;
                                        teams1.CourseName = course.Title;
                                        teams1.EndDate = Convert.ToDateTime(teamsScheduleDetails.EndTime);
                                        teams1.EndTime = iLTSchedule.EndTime.ToString();
                                        teams1.ScheduleID = teamsScheduleDetails.ScheduleID;
                                        teams1.StartDate = Convert.ToDateTime(teamsScheduleDetails.StartTime);
                                        teams1.StartTime = iLTSchedule.StartTime.ToString();
                                        teams1.Username = webinarMaster.TeamsEmail;

                                        TeamsScheduleDetails teamsScheduleDetails2 = await _onlineWebinarRepository.UpdateTeamsMeeting(userId, teams1, webinarMaster, teamsScheduleDetails.ID, authenticationResult);

                                    }
                                }
                                else
                                {
                                    if (iLTSchedule != null && userID.Count != 0)
                                    {
                                        List<TeamsScheduleDetails> teamsScheduleDetailss = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID).ToList();
                                        foreach (TeamsScheduleDetails teamsScheduleDetails in teamsScheduleDetailss)
                                        {
                                            IPublicClientApplication app = _IILTSchedule.GetTeamsAuthentication();
                                            UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();
                                            AuthenticationResult results = await _IILTSchedule.GetTeamsToken(app, webinarMaster.TeamsEmail, 0);

                                            TeamsScheduleDetails teamsScheduleDetails1 = await CallTeamsEventCalendars(results.AccessToken, userId, teamsScheduleDetails, iLTSchedule, userID, webinarMaster);
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        ApiConfigurableParameters conferenceParameters = _db.configurableParameters.Where(a => a.Code == "ONLINE_LOGIN").FirstOrDefault();
                        if (conferenceParameters.Value.ToLower() == "yes")
                        {
                            ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(a => a.ID == id && a.WebinarType == "TEAMS").FirstOrDefaultAsync();
                            ApiConfigurableParameters withoutPassword = _db.configurableParameters.Where(a => a.Code == "WUP").FirstOrDefault();
                            if (withoutPassword.Value.ToLower() == "yes")
                            {

                                List<TeamsScheduleDetails> teamsScheduleDetailss = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID).ToList();
                                AuthenticationResult authenticationResult = await _IILTSchedule.GetTeamsToken();
                                foreach (TeamsScheduleDetails teamsScheduleDetails in teamsScheduleDetailss)
                                {
                                    UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();
                                    Model.Course course = _db.Course.Where(a => a.Id == courseId).FirstOrDefault();
                                    Teams teams1 = new Teams();
                                    teams1.CourseID = courseId;
                                    teams1.CourseName = course.Title;
                                    teams1.EndDate = Convert.ToDateTime(teamsScheduleDetails.EndTime);
                                    teams1.EndTime = iLTSchedule.EndTime.ToString();
                                    teams1.ScheduleID = teamsScheduleDetails.ScheduleID;
                                    teams1.StartDate = Convert.ToDateTime(teamsScheduleDetails.StartTime);
                                    teams1.StartTime = iLTSchedule.StartTime.ToString();
                                    teams1.Username = webinarMaster.TeamsEmail;

                                    TeamsScheduleDetails teamsScheduleDetails2 = await _onlineWebinarRepository.UpdateTeamsMeeting(userId, teams1, webinarMaster, teamsScheduleDetails.ID, authenticationResult);

                                }
                            }
                            else
                            {
                                if (iLTSchedule != null && userID.Count != 0)
                                {
                                    List<TeamsScheduleDetails> teamsScheduleDetailss = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID).ToList();
                                    foreach (TeamsScheduleDetails teamsScheduleDetails in teamsScheduleDetailss)
                                    {
                                        IPublicClientApplication app = _IILTSchedule.GetTeamsAuthentication();
                                        UserWebinarMaster webinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();
                                        AuthenticationResult results = await _IILTSchedule.GetTeamsToken(app, webinarMaster.TeamsEmail, 0);

                                        TeamsScheduleDetails teamsScheduleDetails1 = await CallTeamsEventCalendars(results.AccessToken, userId, teamsScheduleDetails, iLTSchedule, userID, webinarMaster);
                                    }
                                }
                            }

                        }
                    }

                    return objApiResponse;
                }
                else if (ResponseValue == "Cannot Nominate as no schedules found in batch.")
                    return new ApiResponseILT { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ResponseValue, StatusCode = 400 };
                else
                    return new ApiResponseILT { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ResponseValue, StatusCode = 400 };
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return objApiResponse;
            }
        }
        private async Task SendCourseApplicabilityNominationNotifications(List<APINominationResponse> aPINominationResponses, string OrgCode, int CreatedBy)
        {
            List<int> CourseIds = aPINominationResponses.Select(c => c.CourseId).Distinct().ToList();
            string SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");
            List<APITrainingNominationNotification> aPITrainingNominationNotifications = new List<APITrainingNominationNotification>();
            foreach (int CourseId in CourseIds)
            {
                string baseurl = _configuration[Configuration.NotificationApi];
                string appurl = baseurl + "/CourseApplicability";
                JObject oJsonObject = new JObject();
                oJsonObject.Add("CourseId", CourseId);
                oJsonObject.Add("OrganizationCode", OrgCode);
                CallAPI(appurl, oJsonObject);

                string pushurl = baseurl + "/CourseApplicabilityPushNotification";
                JObject Pushnotification = new JObject();
                Pushnotification.Add("CourseId", CourseId);
                Pushnotification.Add("OrganizationCode", OrgCode);
                CallAPI(pushurl, Pushnotification);

                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                {
                    string urlSMS = _configuration[Configuration.NotificationApi];

                    urlSMS += "/CourseApplicabilitySMS";
                    JObject oJsonObjectSMS = new JObject();
                    oJsonObjectSMS.Add("CourseId", CourseId);
                    oJsonObjectSMS.Add("OrganizationCode", OrgCode);
                    CallAPI(urlSMS, oJsonObjectSMS);
                }

                APITrainingNominationNotification aPITrainingNominationNotification = new APITrainingNominationNotification();
                List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(CourseId);

                aPITrainingNominationNotification.CourseId = CourseId;
                aPITrainingNominationNotification.aPINotification = aPINotification;
                aPITrainingNominationNotifications.Add(aPITrainingNominationNotification);
            }

            foreach (APINominationResponse item in aPINominationResponses)
            {
                List<ApiNotification> aPINotification = aPITrainingNominationNotifications.Where(x => x.CourseId == item.CourseId).Select(x => x.aPINotification).FirstOrDefault();
                if (aPINotification != null)
                {
                    ApiNotification Notification = new ApiNotification();
                    Notification.Title = item.Title;
                    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                    Notification.Message = Notification.Message.Replace("{course}", item.Title);
                    Notification.Url = TlsUrl.NotificationAPost + item.CourseId;
                    Notification.Type = Record.Course;
                    Notification.CourseId = item.CourseId;
                    int NotificationId = await this.SendNotificationCourseApplicability(Notification, item.IsApplicableToAll);

                    DataTable dtUserIds = new DataTable();
                    dtUserIds.Columns.Add("UserIds");
                    dtUserIds.Rows.Add(item.UserId);
                    await this.SendDataForApplicableNotifications(NotificationId, dtUserIds, CreatedBy);
                }
            }
        }
        private async Task<int> PostILTNomination(int ScheduleId, List<APINominationResponse> aPINominationResponseList, string OrgCode)
        {
            int i = 0;
            List<APIILTNominationEmail> oEmailList = new List<APIILTNominationEmail>();
            List<APINominateUserSMS> SMSList = new List<APINominateUserSMS>();
            List<APIattendees> attendees = new List<APIattendees>();
            List<EventAttendee> gsuitattendees = new List<EventAttendee>();
            var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_ILT");
            bool IsSendSMSToUser = false;
            string urlSMS = string.Empty;
            if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
            {
                IsSendSMSToUser = true;
                urlSMS = _configuration[Configuration.NotificationApi];
            }
            string GoToMeetingDetails = string.Empty;
            string ZoomMeetingDetails = string.Empty;
            string TeamsMeetingDetails = string.Empty;
            string GoogleMeetDetails = string.Empty;
            if (aPINominationResponseList.Where(x=>x.ModuleType == "vilt").Count()>0)
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    GoToMeetingDetails = dbContext.GoToMeetingDetails.Where(a => a.ScheduleID == ScheduleId).Select(c => c.JoinURL).FirstOrDefault();
                    ZoomMeetingDetails = dbContext.ZoomMeetingDetails.Where(a => a.ScheduleID == ScheduleId).Select(c => c.Join_url).FirstOrDefault();
                    TeamsMeetingDetails = dbContext.TeamsScheduleDetails.Where(a => a.ScheduleID == ScheduleId).Select(c => c.JoinUrl).FirstOrDefault();
                    GoogleMeetDetails = dbContext.GoogleMeetDetails.Where(a => a.ScheduleID == ScheduleId).Select(c => c.HangoutLink).FirstOrDefault();
                }
            }
            foreach (APINominationResponse item in aPINominationResponseList)
            {
                EventAttendee gsuitattendee = new EventAttendee();
                APIILTNominationEmail objIltEmail = new APIILTNominationEmail();
                //----------------- Email ---------------------------//
                objIltEmail.LearnerName = item.UserName;
                objIltEmail.SupervisorName = item.TrainerName;
                objIltEmail.CourseTitle = item.Title;
                objIltEmail.StartDate = Convert.ToString(item.StartDate.Date.ToString("MMM dd, yyyy"));
                objIltEmail.VenueAddress = item.PostalAddress;
                objIltEmail.ScheduleCode = item.ScheduleCode;

                objIltEmail.URL = this._configuration["EmpoweredLmsPath"] + "/myCourseModule/" + item.CourseId;

                objIltEmail.toEmail = item.EmailId;
                objIltEmail.StartTime = Convert.ToString(item.StartTime);
                objIltEmail.ContactNumber = item.ContactNumber;
                objIltEmail.ContactPerson = item.ContactPerson;
                objIltEmail.EndDate = Convert.ToString(item.EndDate);
                objIltEmail.EndTime = Convert.ToString(item.EndTime);
                objIltEmail.PlaceName = item.PlaceName;

                if (item.ModuleType == "vilt")
                {
                    if (item.WebinarType.ToLower() == "gotomeeting")
                    {
                        objIltEmail.GoToMeetingUrl = GoToMeetingDetails;
                    }
                    else if (item.WebinarType.ToLower() == "zoom")
                    {
                        objIltEmail.GoToMeetingUrl = ZoomMeetingDetails;
                    }
                    else if (item.WebinarType.ToLower() == "teams")
                    {
                        objIltEmail.GoToMeetingUrl = TeamsMeetingDetails;
                        //APIattendees userattendee = new APIattendees();
                        //APIemailAddress emailAddress = new APIemailAddress();
                        //APIstatus status = new APIstatus();
                        //emailAddress.address = item.EmailId;
                        //emailAddress.name = item.UserName;

                        //status.response = "none";
                        //status.time = "0001-01-01T00:00:00Z";

                        //userattendee.type = "required";
                        //userattendee.emailAddress = emailAddress;

                        //attendees.Add(userattendee);
                    }
                    else if (item.WebinarType.ToLower() == "googlemeet")
                    {
                        objIltEmail.GoToMeetingUrl = GoogleMeetDetails;
                        gsuitattendee.Email = item.EmailId;
                        gsuitattendees.Add(gsuitattendee);
                    }
                }

                if (attendees.Count > 0)
                {
                    APIattendees[] allattendees = await InviteForTeams(item.CourseId, item.ScheduleId, attendees.ToArray());
                }
               
                string SupervisorEmailId = null, FunctionalAdminEmailId = null;
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetUsersForNotification_HR1HR2";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = item.UserId });
                            cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 5 });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                SupervisorEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());
                                FunctionalAdminEmailId = string.IsNullOrEmpty(row["FunctionalAdminEmailId"].ToString()) ? null : Security.Decrypt(row["FunctionalAdminEmailId"].ToString());
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }

                if (String.Equals(OrgCode, "ivp", StringComparison.CurrentCultureIgnoreCase))
                {
                    objIltEmail.SupervisorEmail = FunctionalAdminEmailId;   //function admin mail id
                }
                else
                {
                    objIltEmail.SupervisorEmail = SupervisorEmailId;   // reporting to mail id
                }

                objIltEmail.orgCode = Security.Decrypt(this._identitySvc.GetOrgCode());
                oEmailList.Add(objIltEmail);

                //Prepare SMS List                
                if (IsSendSMSToUser)
                {
                    APINominateUserSMS objSMS = new APINominateUserSMS();
                    objSMS.OTP = item.OTP;
                    objSMS.CourseTitle = item.Title;
                    objSMS.UserName = item.UserName;
                    objSMS.StartDate = item.StartDate;
                    objSMS.StartTime = item.StartTime;
                    objSMS.MobileNumber = item.MobileNumber;
                    objSMS.ScheduleCode = item.ScheduleCode;
                    objSMS.organizationCode = OrgCode;
                    objSMS.EndDate = item.EndDate;
                    objSMS.EndTime = item.EndTime;
                    objSMS.UserID = item.UserId;
                    SMSList.Add(objSMS);
                }
            }

            if (gsuitattendees.Count > 0)
            {
                GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    googleMeetDetails = dbContext.GoogleMeetDetails.Where(a => a.ScheduleID == ScheduleId).FirstOrDefault();
                }
                UpdateGsuit updateGsuit = new UpdateGsuit();
                updateGsuit.eventId = googleMeetDetails.MeetingId;
                updateGsuit.Username = googleMeetDetails.OrganizerEmail;
                EventAttendee[] allattendees = await _onlineWebinarRepository.CallGSuitUpdateEventCalendars(updateGsuit, gsuitattendees);
            }

            await this._email.SendILTNominationEmail(oEmailList);
            if (IsSendSMSToUser)
                await this._email.SendILTNotificationSMS(SMSList);
            return 1;
        }

        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
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
        public async Task<APIattendees[]> InviteForTeams(int courseID, int scheduleID, APIattendees[] attendeesnew)
        {
            try
            {
                TeamsAccessToken existingvalue = _db.TeamsAccessToken.FirstOrDefault();
                if (existingvalue != null)
                {

                    TeamsScheduleDetails teams = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == scheduleID).FirstOrDefault();
                    if (teams != null)
                    {
                        APIattendees[] attendees = await GetExistingAttedees(existingvalue.TeamsToken, teams.MeetingId);

                        int existingattendeeslength = 0;
                        if (attendees != null)
                            existingattendeeslength = attendees.Length;

                        Array.Resize<APIattendees>(ref attendees, existingattendeeslength + attendeesnew.Length);
                        Array.Copy(attendeesnew, 0, attendees, existingattendeeslength, attendeesnew.Length);

                        string requestUri = "https://graph.microsoft.com/v1.0/me/calendar/events/" + teams.MeetingId;


                        APIPatchattendees patchattendees = new APIPatchattendees();
                        patchattendees.attendees = attendees;

                        HttpResponseMessage Response = await ApiHelper.CallPatchAPI(requestUri, JsonConvert.SerializeObject(patchattendees), existingvalue.TeamsToken);

                        if (Response.IsSuccessStatusCode)
                        {
                            return attendees;
                        }

                        return attendees;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public static async Task<APIattendees[]> GetExistingAttedees(string access_token, string MeetingID)
        {
            try
            {
                string URL = "https://graph.microsoft.com/v1.0/me/calendar/events/" + MeetingID;

                HttpResponseMessage Response = await ApiHelper.CallGetAPI(URL, access_token);

                if (Response.IsSuccessStatusCode)
                {
                    var result = Response.Content.ReadAsStringAsync().Result;
                    APITeamsCreateResponce teamresponce = JsonConvert.DeserializeObject<APITeamsCreateResponce>(result);
                    APIattendees[] attendees = teamresponce.attendees;

                    return attendees;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
            }
        }

        public async Task<List<APITrainingNomination>> GetNominateUserDetails(int id, int courseId, int page, int pageSize, string search = null, string searchText = null)
        {
            List<APITrainingNomination> TrainingNominationList = new List<APITrainingNomination>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllNominateUserDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return null;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                APITrainingNomination TrainingNomination = new APITrainingNomination();
                                TrainingNomination.ID = string.IsNullOrEmpty(row["AutoGenerateUserID"].ToString()) ? 0 : int.Parse(row["AutoGenerateUserID"].ToString());
                                TrainingNomination.ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString());
                                TrainingNomination.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? null : Security.Decrypt(row["UserId"].ToString());
                                TrainingNomination.UserName = row["UserName"].ToString();
                                TrainingNomination.EmailId = string.IsNullOrEmpty(row["EmailId"].ToString()) ? null : Security.Decrypt(row["EmailId"].ToString());
                                TrainingNomination.MobileNumber = string.IsNullOrEmpty(row["MobileNumber"].ToString()) ? null : Security.Decrypt(row["MobileNumber"].ToString());
                                TrainingNomination.Status = row["Status"].ToString();
                                TrainingNomination.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                TrainingNomination.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());

                                TrainingNominationList.Add(TrainingNomination);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return TrainingNominationList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<ApiResponse> GetRoleCourseName(int userId, string userRole, string search = null)
        {
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
            ApiResponse obj = new ApiResponse();

            IQueryable<ApiRoleCourseTypeAhead> Query = (from Course in this._db.Course
                                                        join umd in this._db.UserMasterDetails on Course.CreatedBy equals umd.UserMasterId
                                                        join CourseModuleAssociation in this._db.CourseModuleAssociation on Course.Id equals CourseModuleAssociation.CourseId into tempCourseModuleAssociation
                                                        from CourseModuleAssociation in tempCourseModuleAssociation.DefaultIfEmpty()
                                                        where Course.IsDeleted == Record.NotDeleted && Course.IsActive == Record.Active &&
                                                        (Course.CourseType == "Classroom" || Course.CourseType == "Blended" || Course.CourseType == "Webinar" || Course.CourseType.ToLower() == "vilt")
                                                        select new ApiRoleCourseTypeAhead
                                                        {
                                                            Id = Course.Id,
                                                            Title = Course.Title,
                                                            CreatedBy = Course.CreatedBy,
                                                            AreaId = umd.AreaId,
                                                            LocationId = umd.LocationId,
                                                            GroupId = umd.GroupId,
                                                            BusinessId = umd.BusinessId,
                                                        }).AsNoTracking();

            if (userRole == UserRoles.BA)
            {
                Query = Query.Where(r => r.BusinessId == userdetails.BusinessId);
            }
            if (userRole == UserRoles.GA)
            {
                Query = Query.Where(r => r.GroupId == userdetails.GroupId);
            }
            if (userRole == UserRoles.LA)
            {
                Query = Query.Where(r => r.LocationId == userdetails.LocationId);
            }
            if (userRole == UserRoles.AA)
            {
                Query = Query.Where(r => r.AreaId == userdetails.AreaId);
            }

            List<ApiRoleCourseTypeAhead> list = await Query.Distinct().ToListAsync();

            obj.ResponseObject = (from l in list select new { l.Id, l.Title }).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                obj.ResponseObject = list.Where(a => a.Title.ToLower().StartsWith(search.ToLower()));
            }

            return obj;
        }

        public async Task<ApiResponse> GetRoleAllCourseName(int userId, string userRole, string search = null)
        {
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
            ApiResponse obj = new ApiResponse();

            IQueryable<ApiRoleCourseTypeAhead> Query = (from Course in this._db.Course
                                                        join umd in this._db.UserMasterDetails on Course.CreatedBy equals umd.UserMasterId
                                                        join CourseModuleAssociation in this._db.CourseModuleAssociation on Course.Id equals CourseModuleAssociation.CourseId into tempCourseModuleAssociation
                                                        from CourseModuleAssociation in tempCourseModuleAssociation.DefaultIfEmpty()
                                                        where Course.IsDeleted == Record.NotDeleted
                                                        select new ApiRoleCourseTypeAhead
                                                        {
                                                            Id = Course.Id,
                                                            Title = Course.Title,
                                                            CreatedBy = Course.CreatedBy,
                                                            AreaId = umd.AreaId,
                                                            LocationId = umd.LocationId,
                                                            GroupId = umd.GroupId,
                                                            BusinessId = umd.BusinessId,
                                                        }).AsNoTracking();

            if (userRole == UserRoles.BA)
            {
                Query = Query.Where(r => r.BusinessId == userdetails.BusinessId);
            }
            if (userRole == UserRoles.GA)
            {
                Query = Query.Where(r => r.GroupId == userdetails.GroupId);
            }
            if (userRole == UserRoles.LA)
            {
                Query = Query.Where(r => r.LocationId == userdetails.LocationId);
            }
            if (userRole == UserRoles.AA)
            {
                Query = Query.Where(r => r.AreaId == userdetails.AreaId);
            }

            List<ApiRoleCourseTypeAhead> list = await Query.Distinct().ToListAsync();

            obj.ResponseObject = (from l in list select new { l.Id, l.Title }).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                obj.ResponseObject = list.Where(a => a.Title.ToLower().Contains(search.ToLower()));
            }

            return obj;
        }

        public async Task<ApiResponse> GetScheduleTrainerCourses(int userId, string userRole, string search = null)
        {
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
            ApiResponse obj = new ApiResponse();

            IQueryable<ApiRoleCourseTypeAhead> Query = (from Course in this._db.Course
                                                        join umd in this._db.UserMasterDetails on Course.CreatedBy equals umd.UserMasterId
                                                        join schedule in this._db.ILTSchedule on Course.Id equals schedule.CourseId

                                                        where Course.IsDeleted == Record.NotDeleted && Course.IsActive == true &&
                                                        (Course.CourseType == "Classroom" || Course.CourseType == "Blended" || Course.CourseType == "Webinar" || Course.CourseType.ToLower() == "vilt")
                                                        select new ApiRoleCourseTypeAhead
                                                        {
                                                            Id = Course.Id,
                                                            Title = Course.Title,
                                                            CreatedBy = Course.CreatedBy,
                                                            AreaId = umd.AreaId,
                                                            LocationId = umd.LocationId,
                                                            GroupId = umd.GroupId,
                                                            BusinessId = umd.BusinessId,
                                                        }).AsNoTracking().Distinct();

            if (userRole == UserRoles.BA)
            {
                Query = Query.Where(r => r.BusinessId == userdetails.BusinessId);
            }
            if (userRole == UserRoles.GA)
            {
                Query = Query.Where(r => r.GroupId == userdetails.GroupId);
            }
            if (userRole == UserRoles.LA)
            {
                Query = Query.Where(r => r.LocationId == userdetails.LocationId);
            }
            if (userRole == UserRoles.AA)
            {
                Query = Query.Where(r => r.AreaId == userdetails.AreaId);
            }


            IQueryable<ApiRoleCourseTypeAhead> trainerQuery = (from Course in this._db.Course
                                                               join schedule in this._db.ILTSchedule on Course.Id equals schedule.CourseId
                                                               join trainer in _db.ILTScheduleTrainerBindings on schedule.ID equals trainer.ScheduleID into user
                                                               from trainer in user.DefaultIfEmpty()

                                                               where Course.IsDeleted == Record.NotDeleted && trainer.TrainerID == userId &&
                                                               (Course.CourseType == "Classroom" || Course.CourseType == "Blended" || Course.CourseType == "Webinar" || Course.CourseType.ToLower() == "vilt")
                                                               select new ApiRoleCourseTypeAhead
                                                               {
                                                                   Id = Course.Id,
                                                                   Title = Course.Title,

                                                               }).AsNoTracking();

            List<ApiRoleCourseTypeAhead> trainerlist = await trainerQuery.Distinct().ToListAsync();
            List<ApiRoleCourseTypeAhead> list = await Query.Distinct().ToListAsync();

            List<TrainerCourseTypeAhead> l1 = (from l in trainerlist select new TrainerCourseTypeAhead { Id = l.Id, Title = l.Title }).ToList();
            List<TrainerCourseTypeAhead> l2 = (from l in list select new TrainerCourseTypeAhead { Id = l.Id, Title = l.Title }).ToList();
            List<TrainerCourseTypeAhead> combinelist = l1.Concat(l2).ToList();
            combinelist = combinelist.GroupBy(p => p.Title).Select(g => g.First()).ToList();



            obj.ResponseObject = (from l in combinelist select new { l.Id, l.Title }).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                obj.ResponseObject = combinelist.Where(a => a.Title.ToLower().StartsWith(search.ToLower()));
            }

            return obj;
        }

        public async Task<ApiResponse> GetCourseName(string search = null)
        {
            ApiResponse obj = new ApiResponse();

            var Query = await (from Course in this._db.Course
                               join CourseModuleAssociation in this._db.CourseModuleAssociation on Course.Id equals CourseModuleAssociation.CourseId into tempCourseModuleAssociation
                               from CourseModuleAssociation in tempCourseModuleAssociation.DefaultIfEmpty()
                               where Course.IsDeleted == Record.NotDeleted &&
                               (Course.CourseType == "Classroom" || Course.CourseType == "Blended" || Course.CourseType == "vilt")
                               select new { Course.Id, Course.Title }).Distinct().ToListAsync();

            obj.ResponseObject = Query;
            if (!string.IsNullOrEmpty(search))
            {
                obj.ResponseObject = Query.Where(a => a.Title.ToLower().StartsWith(search.ToLower()));
            }

            return obj;
        }

        public async Task<List<APITrainingNomination>> getAllUsersForSectionalAdmin(int scheduleID, int courseId, int moduleId, int UserId, int page, int pageSize, string OrganisationCode, string search = null, string searchText = null)
        {
            List<APITrainingNomination> objAPITrainingNominationList = new List<APITrainingNomination>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            if (!string.IsNullOrEmpty(search) && (search.ToLower().Equals("emailid") || search.ToLower().Equals("userid") || search.ToLower().Equals("mobilenumber")))
                            {
                                if (!string.IsNullOrEmpty(searchText))
                                {
                                    searchText = Security.Encrypt(searchText.ToLower());
                                }
                            }

                            cmd.CommandText = "GetAllUsersForSectionalAdmin";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.NVarChar) { Value = OrganisationCode });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                APITrainingNomination TrainingNomination = new APITrainingNomination();

                                TrainingNomination.ID = string.IsNullOrEmpty(row["AutoGenerateUserID"].ToString()) ? 0 : int.Parse(row["AutoGenerateUserID"].ToString());
                                TrainingNomination.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? null : Security.Decrypt(row["UserId"].ToString());
                                TrainingNomination.UserName = row["UserName"].ToString();
                                TrainingNomination.EmailId = string.IsNullOrEmpty(row["EmailId"].ToString()) ? null : Security.Decrypt(row["EmailId"].ToString());
                                TrainingNomination.MobileNumber = string.IsNullOrEmpty(row["MobileNumber"].ToString()) ? null : Security.Decrypt(row["MobileNumber"].ToString());
                                TrainingNomination.Status = row["Status"].ToString();
                                TrainingNomination.NoticePeriod = Convert.ToBoolean(row["NoticePeriod"].ToString());
                                objAPITrainingNominationList.Add(TrainingNomination);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return objAPITrainingNominationList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APITrainingNomination>> GetUsersForNomination(int scheduleID, int courseId, int moduleId, int UserId, int page, int pageSize, string search = null, string searchText = null, string Type = null)
        {
            List<APITrainingNomination> objAPITrainingNominationList = new List<APITrainingNomination>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            if (!string.IsNullOrEmpty(search) && (search.ToLower().Equals("emailid") || search.ToLower().Equals("userid") || search.ToLower().Equals("mobilenumber")))
                            {
                                if (!string.IsNullOrEmpty(searchText))
                                {
                                    searchText = Security.Encrypt(searchText.ToLower());
                                }
                            }

                            cmd.CommandText = "GetAllUsersForNomination";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = Type });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return null;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                APITrainingNomination TrainingNomination = new APITrainingNomination();

                                TrainingNomination.ID = string.IsNullOrEmpty(row["AutoGenerateUserID"].ToString()) ? 0 : int.Parse(row["AutoGenerateUserID"].ToString());
                                TrainingNomination.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? null : Security.Decrypt(row["UserId"].ToString());
                                TrainingNomination.UserName = row["UserName"].ToString();
                                TrainingNomination.EmailId = string.IsNullOrEmpty(row["EmailId"].ToString()) ? null : Security.Decrypt(row["EmailId"].ToString());
                                TrainingNomination.MobileNumber = string.IsNullOrEmpty(row["MobileNumber"].ToString()) ? null : Security.Decrypt(row["MobileNumber"].ToString());
                                TrainingNomination.Status = row["Status"].ToString();

                                objAPITrainingNominationList.Add(TrainingNomination);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return objAPITrainingNominationList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetUsersCountForSectionalAdmin(int scheduleID, int courseId, int moduleId, int UserId, string OrganisationCode, string search = null, string searchText = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            if (!string.IsNullOrEmpty(search) && (search.ToLower().Equals("emailid") || search.ToLower().Equals("userid") || search.ToLower().Equals("mobilenumber")))
                            {
                                if (!string.IsNullOrEmpty(searchText))
                                {
                                    searchText = Security.Encrypt(searchText.ToLower());
                                }
                            }

                            cmd.CommandText = "GetUsersCountForSectionalAdmin";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.NVarChar) { Value = OrganisationCode });
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
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }

        public async Task<int> GetUsersCountForNomination(int scheduleID, int courseId, int moduleId, int UserId, string search = null, string searchText = null, string Type = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            if (!string.IsNullOrEmpty(search) && (search.ToLower().Equals("emailid") || search.ToLower().Equals("userid") || search.ToLower().Equals("mobilenumber")))
                            {
                                if (!string.IsNullOrEmpty(searchText))
                                {
                                    searchText = Security.Encrypt(searchText.ToLower());
                                }
                            }

                            cmd.CommandText = "GetUsersCountForNomination";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar) { Value = Type });

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
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }

        public async Task<int> GetNominateUserCount(int id, int courseId, string search = null, string searchText = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllNominateUserCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ScheduleID", SqlDbType.Int) { Value = id });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
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
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Count;
        }

        public async Task<ApiResponse> DeleteNominateUsers(int moduleId, int courseId, int scheduleId, int userId, string orgCode)
        {
            ApiResponse objApiResponse = new ApiResponse();

            ILTTrainingAttendance objILTTrainingAttendance = await this._db.ILTTrainingAttendance.Where(a => a.ModuleID == moduleId && a.CourseID == courseId && a.ScheduleID == scheduleId && a.UserID == userId && a.IsActive == Record.Active).FirstOrDefaultAsync();
            if (objILTTrainingAttendance != null)
            {
                objApiResponse.StatusCode = 505;
                return objApiResponse;
            }

            List<ILTRequestResponse> iLTRequestResponses = await _db.ILTRequestResponse.Where(x => x.ModuleID == moduleId && x.CourseID == courseId && x.ScheduleID == scheduleId && x.UserID == userId && x.IsDeleted == false).ToListAsync();
            foreach (ILTRequestResponse item in iLTRequestResponses)
            {
                item.IsActive = false;
                item.IsDeleted = true;
            }
            _db.ILTRequestResponse.UpdateRange(iLTRequestResponses);
            await _db.SaveChangesAsync();

            List<TrainingNomination> objTrainingNominationList = await this._db.TrainingNomination.Where(a => a.ModuleID == moduleId && a.ScheduleID == scheduleId && a.UserID == userId && a.IsActive == Record.Active).ToListAsync();
            foreach (TrainingNomination obj in objTrainingNominationList)
            {
                obj.IsDeleted = true;
                obj.IsActive = false;
                obj.TrainingRequestStatus = "";
            }

            await this.UpdateRange(objTrainingNominationList);

            ILTRequestResponse objILTWaitingData = await this._db.ILTRequestResponse.Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                        && a.ModuleID == moduleId
                                                                                                        && a.CourseID == courseId
                                                                                                        && a.ScheduleID == scheduleId
                                                                                                        && a.TrainingRequesStatus == "Waiting")
                                                                                                .FirstOrDefaultAsync();
            if (objILTWaitingData != null)
            {
                ILTRequestResponse firstILTRequestResponseForWaiting = await this._db.ILTRequestResponse.AsNoTracking().Where(a => a.IsActive == true && a.IsDeleted == false
                                                                                                     && a.ModuleID == moduleId
                                                                                                     && a.CourseID == courseId
                                                                                                     && a.ScheduleID == scheduleId
                                                                                                     && a.TrainingRequesStatus == "Waiting").OrderBy(a => a.ID)
                                                                                             .FirstOrDefaultAsync();

                firstILTRequestResponseForWaiting.IsActive = false;
                firstILTRequestResponseForWaiting.IsDeleted = true;
                this._db.ILTRequestResponse.Update(objILTWaitingData);
                await _db.SaveChangesAsync();

                ILTRequestResponse objILTResponseRequest = new ILTRequestResponse();
                objILTResponseRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                objILTResponseRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                objILTResponseRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                objILTResponseRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                objILTResponseRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                objILTResponseRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                objILTResponseRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                objILTResponseRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                objILTResponseRequest.TrainingRequesStatus = "Requested";
                objILTResponseRequest.IsActive = true;
                objILTResponseRequest.IsDeleted = false;
                objILTResponseRequest.ID = 0;
                this._db.ILTRequestResponse.Add(objILTResponseRequest);
                await _db.SaveChangesAsync();

                if (orgCode.Contains("wns"))
                {
                    ILTRequestResponse objILTApprovedRequest = new ILTRequestResponse();
                    objILTApprovedRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    objILTApprovedRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    objILTApprovedRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    objILTApprovedRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                    objILTApprovedRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    objILTApprovedRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                    objILTApprovedRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                    objILTApprovedRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                    objILTApprovedRequest.TrainingRequesStatus = "Approved";
                    objILTApprovedRequest.IsActive = true;
                    objILTApprovedRequest.IsDeleted = false;
                    objILTApprovedRequest.ID = 0;
                    this._db.ILTRequestResponse.Add(objILTApprovedRequest);
                    await _db.SaveChangesAsync();

                    ILTRequestResponse objILTAvailabilityRequest = new ILTRequestResponse();
                    objILTAvailabilityRequest.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    objILTAvailabilityRequest.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    objILTAvailabilityRequest.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    objILTAvailabilityRequest.UserID = firstILTRequestResponseForWaiting.UserID;
                    objILTAvailabilityRequest.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    objILTAvailabilityRequest.ModifiedBy = firstILTRequestResponseForWaiting.ModifiedBy;
                    objILTAvailabilityRequest.CreatedDate = firstILTRequestResponseForWaiting.CreatedDate;
                    objILTAvailabilityRequest.ModifiedDate = firstILTRequestResponseForWaiting.ModifiedDate;
                    objILTAvailabilityRequest.TrainingRequesStatus = "Availability";
                    objILTAvailabilityRequest.IsActive = true;
                    objILTAvailabilityRequest.IsDeleted = false;
                    objILTAvailabilityRequest.ID = 0;
                    this._db.ILTRequestResponse.Add(objILTApprovedRequest);
                    await _db.SaveChangesAsync();


                    string Otp = null;
                    bool flag = false;

                    while (flag == false)
                    {
                        Otp = GenerateRandomPassword();
                        List<TrainingNomination> existingOTP = this._db.TrainingNomination.Where(a => a.ScheduleID == firstILTRequestResponseForWaiting.ScheduleID
                                                                                                  && a.ModuleID == firstILTRequestResponseForWaiting.ModuleID
                                                                                                  && a.CourseID == firstILTRequestResponseForWaiting.CourseID
                                                                                                  && a.IsActive == true
                                                                                                  && a.IsDeleted == Record.NotDeleted
                                                                                                  && a.IsActiveNomination == true).ToList();
                        if (existingOTP != null)
                        {
                            flag = true;
                        }
                    }
                    TrainingNomination obj = new TrainingNomination();
                    TrainingNomination lastRequestCode = await _db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false && a.IsActiveNomination == true).OrderByDescending(a => a.ID).FirstOrDefaultAsync();
                    if (lastRequestCode == null)
                    {
                        obj.RequestCode = "RQ1";
                    }
                    else
                    {
                        obj.RequestCode = "RQ" + (lastRequestCode.ID + 1);
                    }
                    obj.ScheduleID = firstILTRequestResponseForWaiting.ScheduleID;
                    obj.UserID = firstILTRequestResponseForWaiting.UserID;
                    obj.TrainingRequestStatus = "Registered";
                    obj.ModuleID = firstILTRequestResponseForWaiting.ModuleID;
                    obj.CourseID = firstILTRequestResponseForWaiting.CourseID;
                    obj.OTP = Otp;
                    obj.CreatedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.ModifiedBy = firstILTRequestResponseForWaiting.CreatedBy;
                    obj.ModifiedDate = DateTime.UtcNow;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    obj.IsActiveNomination = true;
                    _db.TrainingNomination.Add(obj);
                    await _db.SaveChangesAsync();

                }


            }
            ////  ---------------------Email-----------------------------------------//
            List<APIILTNominationEmail> oEmailList = new List<APIILTNominationEmail>();

            ILTSchedule objILTSchedule = await this._db.ILTSchedule.Where(a => a.IsActive == true && a.IsDeleted == false && a.ID == scheduleId).FirstOrDefaultAsync();
            string CourseTitle = await this._db.Course.Where(a => a.IsActive == true && a.IsDeleted == false && a.Id == courseId).Select(a => a.Title).FirstOrDefaultAsync();

            // int TotalNominations = await this._db.TrainingNomination.Where(tn => tn.IsActive == true && tn.IsDeleted == Record.NotDeleted && tn.ScheduleID == scheduleId && tn.TrainingRequestStatus == "Registered").CountAsync();

            int PlaceID = await this._db.ILTSchedule.Where(a => a.IsActive == true && a.IsDeleted == false && a.ID == scheduleId)
                                                                    .Select(ilt => ilt.PlaceID).FirstOrDefaultAsync();

            TrainingPlace PlaceInfo = await this._db.TrainingPlace.Where(a => a.IsDeleted == false && a.Id == PlaceID).FirstOrDefaultAsync();

            string Username = null, UserEmail = null;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetUsersForNotification_HR1HR2";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 3 });

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            connection.Close();
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Username = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                            UserEmail = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }

            APIILTNominationEmail objIltEmail = new APIILTNominationEmail();

            objIltEmail.LearnerName = Username;
            objIltEmail.SupervisorName = objILTSchedule.AcademyTrainerName == null ? objILTSchedule.AgencyTrainerName : objILTSchedule.AcademyTrainerName;
            objIltEmail.CourseTitle = CourseTitle;
            objIltEmail.StartDate = Convert.ToString(objILTSchedule.StartDate.Date.ToString("MMM dd, yyyy"));
            objIltEmail.VenueAddress = PlaceInfo.PostalAddress;
            objIltEmail.ScheduleCode = objILTSchedule.ScheduleCode;
            objIltEmail.URL = this._configuration["EmpoweredLmsPath"];
            objIltEmail.toEmail = UserEmail;
            objIltEmail.StartTime = Convert.ToString(objILTSchedule.StartTime);
            objIltEmail.ContactNumber = PlaceInfo.ContactNumber;
            objIltEmail.ContactPerson = PlaceInfo.ContactPerson;
            objIltEmail.EndDate = Convert.ToString(objILTSchedule.EndDate);
            objIltEmail.EndTime = Convert.ToString(objILTSchedule.EndTime);
            objIltEmail.PlaceName = PlaceInfo.PlaceName;
            var ModuleType = _db.Module.Where(a => a.Id == moduleId).Select(c => c.ModuleType).FirstOrDefault();


            string SupervisorEmailId = null, FunctionalAdminEmailId = null;

            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetUsersForNotification_HR1HR2";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 5 });

                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            connection.Close();
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            SupervisorEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());
                            FunctionalAdminEmailId = string.IsNullOrEmpty(row["FunctionalAdminEmailId"].ToString()) ? null : Security.Decrypt(row["FunctionalAdminEmailId"].ToString());
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }

            objIltEmail.SupervisorEmail = SupervisorEmailId;   // reporting to mail id

            objIltEmail.orgCode = Security.Decrypt(this._identitySvc.GetOrgCode());

            oEmailList.Add(objIltEmail);
            await this._email.SendILTNominationCancellationEmail(oEmailList);
            //  ---------------------Email-----------------------------------------//
            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> ProcessImportFile(APITrainingNominationPath aPITrainingNominationPath, int UserId, string OrganisationCode)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string filepath = sWebRootFolder + aPITrainingNominationPath.Path;

                DataTable nominationImportdt = ReadFile(filepath);
                if (nominationImportdt == null || nominationImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                bool resultMessage = await ValidateFileColumnHeaders(nominationImportdt, OrganisationCode);
                if (resultMessage)
                {
                    return await ProcessRecordsAsync(nominationImportdt, UserId, OrganisationCode);
                }
                else
                {
                    Response.StatusCode = 200;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }
        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filepath))
                    pck.Load(stream);
                var ws = pck.Workbook.Worksheets.First();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    if (!dt.Columns.Contains(firstRowCell.Text))
                        dt.Columns.Add(firstRowCell.Text);
                }
                var startRow = 2;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    if (!string.IsNullOrEmpty(Convert.ToString(wsRow.ElementAtOrDefault(0))))
                    {
                        DataRow row = dt.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            if (!string.IsNullOrEmpty(cell.Text))
                                row[cell.Start.Column - 1] = cell.Text;
                        }

                    }
                    else
                        break;
                }
            }
            //check for empty rows
            DataTable validDt = new DataTable();
            validDt = dt.Clone();
            foreach (DataRow dataRow in dt.Rows)
            {
                bool IsEmpty = true;
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dataRow[dataColumn])))
                    {
                        IsEmpty = false;
                        break;
                    }
                }
                if (!IsEmpty)
                    validDt.ImportRow(dataRow);
            }

            return validDt;
        }

        public async Task<ApiResponse> ProcessRecordsAsync(DataTable nominationImportDt, int UserId, string OrgCode)
        {
            int totalInserted = 0, totalRejected = 0;
            int columnIndex = 0;
            List<string> schedule = new List<string>();
            DataColumnCollection columns = nominationImportDt.Columns;
            string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrgCode);

            if (string.Equals(batchwiseNomination, "no", StringComparison.CurrentCultureIgnoreCase))
                nominationImportDt.Columns.Add("BatchCode", typeof(string));

            List<string> importcolumns = GetImportColumns(batchwiseNomination, true);
            foreach (string column in importcolumns)
            {
                nominationImportDt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            nominationImportDt.Columns.Add("UserIdEncrypted", typeof(string));
            nominationImportDt.Columns.Add("OTP", typeof(string));
            List<KeyValuePair<string, int>> columnlengths = GetImportColumnsLength();

            DataTable dtschedules = new DataTable();
            if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase))
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[ILTNomination_GetSchedulesByBatch]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ILTBulkNominateType", SqlDbType.Structured) { Value = nominationImportDt });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        dtschedules.Load(reader);
                    }
                    connection.Close();
                }
                nominationImportDt.Rows.Clear();
                nominationImportDt = dtschedules;
            }

            DataTable finalDt = nominationImportDt.Clone();
            List<TrainingNominationRejected> trainingNominationRejectedList = new List<TrainingNominationRejected>();
            foreach (DataRow dataRow in nominationImportDt.Rows)
            {
                bool IsError = false;
                string errorMsg = "";
                foreach (string column in columnlengths.Select(c => c.Key).ToList())
                {
                    if (string.Compare(column, "UserId", true) == 0)
                        dataRow[column] = !string.IsNullOrEmpty(Convert.ToString(dataRow[column])) ? Convert.ToString(dataRow[column]).ToLower() : null;
                    else if (string.Compare(column, "UserIdEncrypted", true) == 0)
                        dataRow[column] = !string.IsNullOrEmpty(Convert.ToString(dataRow["UserId"])) ? Security.Encrypt(Convert.ToString(dataRow["UserId"]).ToLower()) : null;
                    else if (string.Compare(column, "OTP", true) == 0)
                        dataRow[column] = GenerateRandomPassword();

                    if (string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase) && (string.Compare(column, "ScheduleCode", true) == 0))
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                        {
                            IsError = true;
                            errorMsg = "Cannot Nominate as no schedules found in batch.";
                            break;
                        }
                    }

                    if (!DBNull.Value.Equals(dataRow[column]))
                    {
                        int columnlength = columnlengths.Where(c => c.Key == column).Select(len => len.Value).FirstOrDefault();
                        if (columnlength < Convert.ToString(dataRow[column]).Length)
                        {
                            IsError = true;
                            errorMsg = "Invalid data in " + column + ". Must be less than equal to " + Convert.ToString(columnlength) + " characters.";
                            break;
                        }
                    }
                }
                if (IsError)
                {
                    TrainingNominationRejected trainingNominationRejected = new TrainingNominationRejected();
                    trainingNominationRejected.CourseName = dataRow["CourseCode"] != null ? Convert.ToString(dataRow["CourseCode"]) : null;
                    trainingNominationRejected.ModuleName = dataRow["ModuleName"] != null ? Convert.ToString(dataRow["ModuleName"]) : null;
                    trainingNominationRejected.ScheduleCode = dataRow["ScheduleCode"] != null ? Convert.ToString(dataRow["ScheduleCode"]) : null;
                    trainingNominationRejected.UserId = dataRow["UserId"] != null ? Convert.ToString(dataRow["UserId"]) : null;
                    trainingNominationRejected.CreatedBy = UserId;
                    trainingNominationRejected.CreatedDate = DateTime.UtcNow;
                    trainingNominationRejected.ModifiedBy = UserId;
                    trainingNominationRejected.ModifiedDate = DateTime.UtcNow;
                    trainingNominationRejected.IsDeleted = false;
                    trainingNominationRejected.ErrMessage = errorMsg;
                    trainingNominationRejectedList.Add(trainingNominationRejected);
                }
                else
                    finalDt.ImportRow(dataRow);
            }

            List<APITrainingNominationImportResult> aPITrainingNominationImportResultsRejected = Mapper.Map<List<APITrainingNominationImportResult>>(trainingNominationRejectedList);
            int rejectedForLength = aPITrainingNominationImportResultsRejected.Count;
            DataTable dtResult = new DataTable();
            using (var dbContext = _customerConnection.GetDbContext())
            {
                var connection = dbContext.Database.GetDbConnection();

                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "[dbo].[ILTNomination_BulkImport]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                    cmd.Parameters.Add(new SqlParameter("@ILTBulkNominateType", SqlDbType.Structured) { Value = finalDt });

                    cmd.CommandTimeout = 0;
                    SqlParameter totalInsertedParam = new SqlParameter("@TotalInserted", SqlDbType.Int);
                    totalInsertedParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(totalInsertedParam);
                    SqlParameter totalRejectedParam = new SqlParameter("@TotalRejected", SqlDbType.Int);
                    totalRejectedParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(totalRejectedParam);
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    dtResult.Load(reader);
                    totalInserted = Convert.ToInt32(totalInsertedParam.Value);
                    totalRejected = Convert.ToInt32(totalRejectedParam.Value);
                    foreach (DataRow dataRow in finalDt.Rows)
                    {
                        if (!schedule.Contains(dataRow["ScheduleCode"].ToString()))
                        {
                            schedule.Add(dataRow["ScheduleCode"].ToString());
                        }
                    }
                }
                connection.Close();
            }
            trainingNominationRejectedList.ForEach(x => x.UserId = x.UserId != null ? Security.Encrypt(x.UserId) : null);
            await _db.TrainingNominationRejected.AddRangeAsync(trainingNominationRejectedList);
            await _db.SaveChangesAsync();

            totalRejected = totalRejected + rejectedForLength;
            List<APITrainingNominationImportResult> aPITrainingNominationImportResults = dtResult.ConvertToList<APITrainingNominationImportResult>();
            List<APITrainingNominationImportResult> aPITrainingNominationImportResultsInserted = aPITrainingNominationImportResults.Where(x => x.IsValid == true && x.ErrorMessage == null).ToList();
            aPITrainingNominationImportResultsRejected.AddRange(aPITrainingNominationImportResults.Where(x => x.IsValid == false && x.ErrorMessage != null).ToList());

            await SendCourseApplicabilityNotifications(aPITrainingNominationImportResultsInserted, OrgCode, UserId);
            string SendMailToUser = await GetMasterConfigurableParameterValue("NOMINATE_COURSE_CC_MANAGER");
            string SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_ILT");
            string Url = this._configuration[Configuration.NotificationApi];
            List<APINominateUserSMS> SMSList = new List<APINominateUserSMS>();
            List<APIILTNominationEmail> oEmailList = new List<APIILTNominationEmail>();            
              List<UserListGsuitNomination> gsuitAttendeeList = new List<UserListGsuitNomination>();
            foreach (APITrainingNominationImportResult item in aPITrainingNominationImportResultsInserted)
            {
                //Nomination notifications
                JObject oJsonObject = new JObject();

                oJsonObject.Add("UserId", item.UserMasterId);
                oJsonObject.Add("OrgCode", OrgCode);
                oJsonObject.Add("CourseTitle", item.CourseTitle);
                oJsonObject.Add("ModuleName", item.ModuleName);
                oJsonObject.Add("ScheduleCode", item.ScheduleCode);
                oJsonObject.Add("PlaceName", item.PlaceName);
                oJsonObject.Add("VenueAddress", item.PostalAddress);
                oJsonObject.Add("StartDate", item.StartDate);
                oJsonObject.Add("EndDate", item.EndDate);
                oJsonObject.Add("StartTime", item.StartTime);
                oJsonObject.Add("EndTime", item.EndTime);
                oJsonObject.Add("ContactPerson", item.ContactPerson);
                oJsonObject.Add("ContactNumber", item.ContactNumber);
                string SupervisorEmailId = null;
                try
                {
                    using (var courseContext = this._customerConnection.GetDbContext())
                    {
                        using (var dbConnection = courseContext.Database.GetDbConnection())
                        {
                            if (dbConnection.State == ConnectionState.Broken || dbConnection.State == ConnectionState.Closed)
                                dbConnection.Open();
                            using (var cmd = dbConnection.CreateCommand())
                            {
                                cmd.CommandText = "GetUsersForNotification_HR1HR2";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = item.UserMasterId });
                                cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 5 });

                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count <= 0)
                                {
                                    reader.Dispose();
                                    dbConnection.Close();
                                }
                                foreach (DataRow row in dt.Rows)
                                {
                                    SupervisorEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());
                                }
                                reader.Dispose();
                            }
                            dbConnection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                if (Convert.ToString(SendMailToUser).ToLower() == "yes")
                    oJsonObject.Add("SupervisorEmail", SupervisorEmailId);
                else
                    oJsonObject.Add("SupervisorEmail", null);

                CallAPI(Url, oJsonObject);

                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                {
                    APINominateUserSMS objSMS = new APINominateUserSMS();
                    objSMS.OTP = item.OTP;
                    objSMS.CourseTitle = item.CourseTitle;
                    objSMS.UserName = item.UserName;
                    objSMS.StartDate = item.StartDate;
                    objSMS.StartTime = item.StartTime;
                    objSMS.MobileNumber = Security.Decrypt(item.MobileNumber);
                    objSMS.ScheduleCode = item.ScheduleCode;
                    objSMS.organizationCode = OrgCode;
                    objSMS.EndDate = item.EndDate;
                    objSMS.EndTime = item.EndTime;
                    objSMS.UserID = item.UserMasterId;
                    SMSList.Add(objSMS);
                }


                APIILTNominationEmail objIltEmail = new APIILTNominationEmail();
                //----------------- Email ---------------------------//
                objIltEmail.LearnerName = item.UserName;
                objIltEmail.SupervisorName = null;
                objIltEmail.CourseTitle = item.CourseTitle;
                objIltEmail.StartDate = Convert.ToString(item.StartDate.Date.ToString("MMM dd, yyyy"));
                objIltEmail.VenueAddress = item.PostalAddress;
                objIltEmail.ScheduleCode = item.ScheduleCode;
                objIltEmail.orgCode = OrgCode;
                objIltEmail.URL = this._configuration["EmpoweredLmsPath"] + "/myCourseModule/" + item.CourseId;
                UserMaster userMaster = _db.UserMaster.Where(a => a.Id == item.UserMasterId).FirstOrDefault();
                string email = Security.Decrypt(userMaster.EmailId);
                objIltEmail.toEmail = email;
                objIltEmail.StartTime = Convert.ToString(item.StartTime);
                objIltEmail.ContactNumber = item.ContactNumber;
                objIltEmail.ContactPerson = item.ContactPerson;
                objIltEmail.EndDate = Convert.ToString(item.EndDate);
                objIltEmail.EndTime = Convert.ToString(item.EndTime);
                objIltEmail.PlaceName = item.PlaceName;
                oEmailList.Add(objIltEmail);

                if (item.WebinarType.ToLower() == "googlemeet")
                {
                    UserListGsuitNomination nominateGsuit = new UserListGsuitNomination();
                    nominateGsuit.ScheduleId = item.ScheduleId;
                    nominateGsuit.UserName = item.UserName;
                    nominateGsuit.EmailId = email;
                    gsuitAttendeeList.Add(nominateGsuit);
                }

            }
            await this._email.SendILTNominationEmail(oEmailList);

            if (Convert.ToString(SendSMSToUser).ToLower() == "yes" && SMSList.Count > 0)
                await this._email.SendILTNotificationSMS(SMSList);

            if (gsuitAttendeeList.Count > 0)
            {
                _logger.Info("gsuitAttendeeList count = "+ gsuitAttendeeList.Count.ToString());
                List<int> uniqueSchedule = gsuitAttendeeList.GroupBy(x => x.ScheduleId).Select(x => x.Key).ToList();

                foreach (int item in uniqueSchedule)
                {
                    List<EventAttendee> gsuitattendees = new List<EventAttendee>();
                    List<UserListGsuitNomination> scheduleNomination = gsuitAttendeeList.Where(a => a.ScheduleId == item).ToList();

                    foreach (UserListGsuitNomination data in scheduleNomination)
                    {
                        if (!string.IsNullOrEmpty(data.EmailId))
                        {
                            EventAttendee att = new EventAttendee();
                            att.Email = data.EmailId;
                            att.DisplayName = data.UserName;
                            gsuitattendees.Add(att);
                        }
                    }
                    if (gsuitattendees.Count > 0)
                    {
                        GoogleMeetDetails googleMeetDetails = new GoogleMeetDetails();
                        using (var dbContext = this._customerConnection.GetDbContext())
                        {
                            googleMeetDetails = dbContext.GoogleMeetDetails.Where(a => a.ScheduleID == item).FirstOrDefault();
                        }
                        UpdateGsuit updateGsuit = new UpdateGsuit();
                        updateGsuit.eventId = googleMeetDetails.MeetingId;
                        updateGsuit.Username = googleMeetDetails.OrganizerEmail;
                        EventAttendee[] allattendees = await _onlineWebinarRepository.CallGSuitUpdateEventCalendars(updateGsuit, gsuitattendees);
                    }
                }
            }

            foreach (string schedules in schedule)
            {
                List<APIUserData> mailSendUser = new List<APIUserData>();
                ILTSchedule iLTSchedule = await _db.ILTSchedule.Where(a => a.ScheduleCode == schedules && a.WebinarType == "TEAMS").FirstOrDefaultAsync();
                if (iLTSchedule != null)
                {
                    Model.Course course = _db.Course.Where(a => a.Id == iLTSchedule.CourseId).FirstOrDefault();
                    if (course != null)
                    {
                        List<APITrainingNomination> aPITrainingNomination = await GetNominateUserDetails(iLTSchedule.ID, course.Id, 1, 10000, "userName", null);
                        if (aPITrainingNomination != null)
                        {
                            for (int i = 0; i < aPITrainingNomination.Count; i++)
                            {
                                APIUserData aPIUserData = new APIUserData();
                                aPIUserData.emailId = aPITrainingNomination[i].EmailId;
                                aPIUserData.userName = aPITrainingNomination[i].UserName;
                                mailSendUser.Add(aPIUserData);
                            }
                        }
                        TeamsScheduleDetails teamsScheduleDetails = _db.TeamsScheduleDetails.Where(a => a.ScheduleID == iLTSchedule.ID).FirstOrDefault();
                        if (teamsScheduleDetails != null)
                        {
                            UserWebinarMaster userWebinarMaster = _db.UserWebinarMasters.Where(a => a.Id == teamsScheduleDetails.UserWebinarId).FirstOrDefault();
                            if (userWebinarMaster != null)
                            {

                                EventMeeting eventMeeting = new EventMeeting();
                                Start start = new Start();
                                End end = new End();

                                try
                                {
                                    eventMeeting.start = new Start();
                                    eventMeeting.end = new End();

                                    string startdate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", iLTSchedule.StartDate);
                                    string enddate1 = string.Format("{0:yyyy-MM-ddThh:mm:ss}", iLTSchedule.EndDate);

                                    var sdate = startdate1.Substring(0, 11);
                                    var sdate1 = iLTSchedule.StartTime;
                                    var sdate2 = startdate1.Substring(17, 2);

                                    var edate = enddate1.Substring(0, 11);
                                    var edate1 = iLTSchedule.EndTime;
                                    var edate2 = enddate1.Substring(17, 2);

                                    eventMeeting.subject = course.Title;
                                    eventMeeting.start.dateTime = sdate + sdate1 + ":" + sdate2;
                                    eventMeeting.start.timeZone = "Asia/Kolkata";
                                    eventMeeting.end.dateTime = edate + edate1 + ":" + edate2;
                                    eventMeeting.end.timeZone = "Asia/Kolkata";
                                    eventMeeting.IsOnlineMeeting = true;
                                    eventMeeting.OnlineMeetingProvider = "teamsForBusiness";
                                    eventMeeting.attendees = new Attendance[mailSendUser.Count];
                                    for (int i = 0; i < mailSendUser.Count; i++)
                                    {
                                        eventMeeting.attendees[i] = new Attendance();
                                        eventMeeting.attendees[i].emailAddress = new EmailAddress();
                                        eventMeeting.attendees[i].emailAddress.address = mailSendUser[i].emailId;
                                        eventMeeting.attendees[i].emailAddress.name = mailSendUser[i].userName;
                                        eventMeeting.attendees[i].status = new Status1();
                                        eventMeeting.attendees[i].type = "required";
                                    }

                                    JObject oJsonObject1 = new JObject();
                                    oJsonObject1 = JObject.Parse(JsonConvert.SerializeObject(eventMeeting));
                                    string baseUrl = "https://graph.microsoft.com/v1.0/users/" + userWebinarMaster.TeamsEmail + "/calendar/events/" + teamsScheduleDetails.MeetingId;
                                    AuthenticationResult results = await _onlineWebinarRepository.GetTeamsToken();
                                    if (results != null)
                                    {
                                        HttpResponseMessage Response = await ApiHelper.CallPatchAPIForTeams(baseUrl, oJsonObject1, results.AccessToken);
                                        TeamsEventResponse TeamsResponce = null;
                                        if (Response.IsSuccessStatusCode)
                                        {
                                            var result = Response.Content.ReadAsStringAsync().Result;

                                            TeamsResponce = JsonConvert.DeserializeObject<TeamsEventResponse>(result);
                                        }
                                        if (TeamsResponce != null)
                                        {

                                            teamsScheduleDetails.CourseID = iLTSchedule.CourseId;
                                            teamsScheduleDetails.ScheduleID = Convert.ToInt32(teamsScheduleDetails.ScheduleID);
                                            teamsScheduleDetails.CreatedBy = Convert.ToInt32(UserId);
                                            teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                                            teamsScheduleDetails.ModifiedBy = Convert.ToInt32(UserId);
                                            teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                                            teamsScheduleDetails.IsActive = Record.Active;
                                            teamsScheduleDetails.IsDeleted = Record.NotDeleted;

                                            teamsScheduleDetails.iCalUId = TeamsResponce.iCalUId;
                                            teamsScheduleDetails.MeetingId = TeamsResponce.id;
                                            teamsScheduleDetails.StartTime = TeamsResponce.start.dateTime.ToString();
                                            teamsScheduleDetails.EndTime = TeamsResponce.start.dateTime.ToString();
                                            if (TeamsResponce.onlineMeeting == null)
                                            {
                                                teamsScheduleDetails.JoinUrl = TeamsResponce.webLink;
                                            }
                                            else
                                            {
                                                teamsScheduleDetails.JoinUrl = TeamsResponce.onlineMeeting.joinUrl;
                                            }

                                            teamsScheduleDetails.UserWebinarId = userWebinarMaster.Id;
                                        }
                                    }

                                }

                                catch (Exception ex)

                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                        }

                    }

                }
            }

            string resultstring = "Total number of record inserted :" + totalInserted + ",  Total number of record rejected : " + totalRejected;
            int TotalCount = totalInserted + totalRejected;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, TotalCount, aPITrainingNominationImportResultsRejected };
            return response;
        }

        private async Task SendCourseApplicabilityNotifications(List<APITrainingNominationImportResult> aPITrainingNominationImportResultsInserted, string OrgCode, int CreatedBy)
        {
            List<int> CourseIds = aPITrainingNominationImportResultsInserted.Select(c => c.CourseId).Distinct().ToList();
            string SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");
            List<APITrainingNominationNotification> aPITrainingNominationNotifications = new List<APITrainingNominationNotification>();
            foreach (int CourseId in CourseIds)
            {
                string baseurl = _configuration[Configuration.NotificationApi];
                string appurl = baseurl + "/CourseApplicability";
                JObject oJsonObject = new JObject();
                oJsonObject.Add("CourseId", CourseId);
                oJsonObject.Add("OrganizationCode", OrgCode);
                CallAPI(appurl, oJsonObject);

                string pushurl = baseurl + "/CourseApplicabilityPushNotification";
                JObject Pushnotification = new JObject();
                Pushnotification.Add("CourseId", CourseId);
                Pushnotification.Add("OrganizationCode", OrgCode);
                CallAPI(pushurl, Pushnotification);

                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                {
                    string urlSMS = _configuration[Configuration.NotificationApi];

                    urlSMS += "/CourseApplicabilitySMS";
                    JObject oJsonObjectSMS = new JObject();
                    oJsonObjectSMS.Add("CourseId", CourseId);
                    oJsonObjectSMS.Add("OrganizationCode", OrgCode);
                    CallAPI(urlSMS, oJsonObjectSMS);
                }
                APITrainingNominationNotification aPITrainingNominationNotification = new APITrainingNominationNotification();
                List<ApiNotification> aPINotification = await this.GetCountByCourseIdAndUserId(CourseId);

                aPITrainingNominationNotification.CourseId = CourseId;
                aPITrainingNominationNotification.aPINotification = aPINotification;
                aPITrainingNominationNotifications.Add(aPITrainingNominationNotification);
            }

            foreach (APITrainingNominationImportResult item in aPITrainingNominationImportResultsInserted)
            {
                List<ApiNotification> aPINotification = aPITrainingNominationNotifications.Where(x => x.CourseId == item.CourseId).Select(x => x.aPINotification).FirstOrDefault();
                if (aPINotification != null)
                {
                    ApiNotification Notification = new ApiNotification();
                    Notification.Title = item.CourseTitle;
                    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
                    Notification.Message = Notification.Message.Replace("{course}", item.CourseTitle);
                    Notification.Url = TlsUrl.NotificationAPost + item.CourseId;
                    Notification.Type = Record.Course;
                    Notification.CourseId = item.CourseId;
                    int NotificationId = await this.SendNotificationCourseApplicability(Notification, item.IsApplicableToAll);

                    DataTable dtUserIds = new DataTable();
                    dtUserIds.Columns.Add("UserIds");
                    dtUserIds.Rows.Add(item.UserMasterId);
                    await this.SendDataForApplicableNotifications(NotificationId, dtUserIds, CreatedBy);
                }
            }
        }
        public async Task<List<ApiNotification>> GetCountByCourseIdAndUserId(int Url)
        {
            List<ApiNotification> listUserApplicability = new List<ApiNotification>();

            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetCountByCourseIdAndUserId";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.NVarChar) { Value = Url });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    listUserApplicability.Add(new ApiNotification() { Title = row["Title"].ToString(), Id = Convert.ToInt32(row["Id"]) });
                                }
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return listUserApplicability;
        }

        public async Task<int> SendNotificationCourseApplicability(ApiNotification apiNotification, bool IsApplicabletoall)
        {
            int Id = 0;

            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
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
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.BigInt) { Value = apiNotification.CourseId });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                Id = Convert.ToInt32(row["Id"].ToString());
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Id;
        }

        public async Task SendDataForApplicableNotifications(int notificationId, DataTable dtUserIds, int createdBy)
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (SqlCommand cmd = (SqlCommand)connection.CreateCommand())
                        {
                            cmd.CommandText = "ApplicableInsertNotifications";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@NotificationId", SqlDbType.NVarChar) { Value = notificationId });
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = createdBy });
                            cmd.Parameters.AddWithValue("@TVP_UserIDs", dtUserIds);

                            DbDataReader reader = await cmd.ExecuteReaderAsync();

                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
        }

        public async Task<bool> ValidateFileColumnHeaders(DataTable userImportdt, string OrgCode)
        {
            string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrgCode);
            List<string> importColumns = GetImportColumns(batchwiseNomination);
            if (userImportdt.Columns.Count != importColumns.Count)
                return false;

            for (int i = 0; i < userImportdt.Columns.Count; i++)
            {
                string col = userImportdt.Columns[i].ColumnName.Replace("*", "").Replace(" ", "");
                userImportdt.Columns[i].ColumnName = col;

                if (!importColumns.Contains(userImportdt.Columns[i].ColumnName))
                    return false;
            }
            return true;
        }
        private List<string> GetImportColumns(string batchwiseNomination, bool allcolumns = false)
        {
            List<string> columns = new List<string>();
            columns.Add(APITrainingNominationImportColumns.CourseCode);
            if (allcolumns || string.Equals(batchwiseNomination, "yes", StringComparison.CurrentCultureIgnoreCase))
                columns.Add(APITrainingNominationImportColumns.BatchCode);
            columns.Add(APITrainingNominationImportColumns.ModuleName);
            columns.Add(APITrainingNominationImportColumns.ScheduleCode);
            columns.Add(APITrainingNominationImportColumns.UserId);
            return columns;
        }
        private List<KeyValuePair<string, int>> GetImportColumnsLength()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APITrainingNominationImportColumns.CourseCode, 60));
            columns.Add(new KeyValuePair<string, int>(APITrainingNominationImportColumns.ModuleName, 600));
            columns.Add(new KeyValuePair<string, int>(APITrainingNominationImportColumns.ScheduleCode, 100));
            columns.Add(new KeyValuePair<string, int>(APITrainingNominationImportColumns.UserId, 400));
            columns.Add(new KeyValuePair<string, int>(APITrainingNominationImportColumns.UserIdEncrypted, 2000));
            columns.Add(new KeyValuePair<string, int>(APITrainingNominationImportColumns.OTP, 100));
            return columns;
        }

        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            Random generator = new Random();
            return generator.Next(100000, 999999).ToString("D6");

        }
        public async Task<List<SchedularTypeahead>> GetScheduleByModuleId_AttendanceReport(int ModuleId, int? CourseId)
        {
            List<SchedularTypeahead> schedularTypeaheadList = new List<SchedularTypeahead>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetScheduleByModuleIdForAttendance";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = ModuleId });
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return null;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                SchedularTypeahead schedularTypeahead = new SchedularTypeahead();

                                schedularTypeahead.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                schedularTypeahead.ScheduleCode = row["ScheduleCode"].ToString();

                                schedularTypeaheadList.Add(schedularTypeahead);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return schedularTypeaheadList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<IEnumerable<APINominatedUsersForExport>> GetNominatedWiseReport(APINominatedUsersForExport trainingnominated)
        {
            List<APINominatedUsersForExport> Obj = new List<APINominatedUsersForExport>();

            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@CourseID", trainingnominated.CourseID);
                        parameters.Add("@SceduleID", trainingnominated.ScheduleID);


                        var Result = await SqlMapper.QueryAsync<APINominatedUsersForExport>((SqlConnection)connection, "[dbo].[GetAllNominationDetailsForExport]", parameters, null, null, CommandType.StoredProcedure);
                        Obj = Result.ToList();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Obj;
        }

        public async Task<IEnumerable<APITrainingNominationRejected>> GetAllTrainingNominationReject(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<TrainingNominationRejected> Query = this._db.TrainingNominationRejected;

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => v.CourseName.StartsWith(search) && v.IsDeleted == Record.NotDeleted);
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
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                }
                IEnumerable<TrainingNominationRejected> aPITrainingNominationRejecteds = await Query.ToListAsync();
                IEnumerable<APITrainingNominationRejected> aPITrainingNominationRejectedRecords = Mapper.Map<IEnumerable<APITrainingNominationRejected>>(aPITrainingNominationRejecteds);

                return aPITrainingNominationRejectedRecords;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<FileInfo> ExportTrainingNominationReject(string search)
        {
            IEnumerable<APITrainingNominationRejected> aPITrainingNominationRejecteds = await GetAllTrainingNominationReject(-1, -1, search);
            FileInfo fileInfo = GetTrainingNominationReject(aPITrainingNominationRejecteds);
            return fileInfo;
        }

        private FileInfo GetTrainingNominationReject(IEnumerable<APITrainingNominationRejected> aPITrainingNominationRejecteds)
        {
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();

            List<string> ExportHeader = GetTrainingNominationRejectHeader();
            ExportData.Add(RowNumber, ExportHeader);

            foreach (APITrainingNominationRejected aPITrainingNominationRejected in aPITrainingNominationRejecteds)
            {
                List<string> DataRow = new List<string>();
                DataRow = GetTrainingNominationRejectRowData(aPITrainingNominationRejected);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }

            FileInfo fileInfo = this._tlsHelper.GenerateExcelFile(FileName.TrainingNominationRejected, ExportData);
            return fileInfo;
        }

        private List<string> GetTrainingNominationRejectHeader()
        {
            List<string> ExportHeader = new List<string>()
            {
                HeaderName.CourseName,
                HeaderName.ModuleName,
                HeaderName.ScheduleCode,
                HeaderName.UserId,
                HeaderName.ErrMessage
            };
            return ExportHeader;
        }

        private List<string> GetTrainingNominationRejectRowData(APITrainingNominationRejected aPITrainingNominationRejected)
        {
            List<string> ExportData = new List<string>()
            {
                aPITrainingNominationRejected.CourseName,
                aPITrainingNominationRejected.ModuleName,
                aPITrainingNominationRejected.ScheduleCode,
                aPITrainingNominationRejected.UserId,
                aPITrainingNominationRejected.ErrMessage
            };
            return ExportData;
        }

        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this._db.TrainingNominationRejected.Where(r => r.CourseName.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this._db.TrainingNominationRejected.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<int> GetUserDetailsByUserID(string userId)
        {
            int UserId = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetUserDetailsByUserID";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            UserId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return UserId;
        }

        private async Task<TeamsScheduleDetails> CallTeamsEventCalendars(string TeamsAccessToken, int userId, TeamsScheduleDetails teamsScheduleDetail, ILTSchedule iLTSchedule, List<APIUserData> userID, UserWebinarMaster userWebinarMaster)
        {

            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();

            Model.Course course = _db.Course.Where(a => a.Id == iLTSchedule.CourseId).FirstOrDefault();
            APIILTSchedular aPIILTSchedular1 = await _IILTSchedule.GetByID(iLTSchedule.ID);
            List<APIUserData> mailSendUser = new List<APIUserData>();

            for (int i = 0; i < aPIILTSchedular1.TrainerList.Length; i++)
            {
                UserMaster userMaster = _db.UserMaster.Where(a => a.Id == aPIILTSchedular1.TrainerList[i].AcademyTrainerID).FirstOrDefault();
                APIUserData aPIUserData = new APIUserData();
                aPIUserData.emailId = Security.Decrypt(userMaster.EmailId);
                aPIUserData.userName = userMaster.UserName;
                mailSendUser.Add(aPIUserData);
            }
            List<APITrainingNomination> aPITrainingNomination = await GetNominateUserDetails(iLTSchedule.ID, course.Id, 1, 1000, "userName", null);

            if (aPITrainingNomination != null)
            {
                for (int i = 0; i < aPITrainingNomination.Count; i++)
                {
                    APIUserData aPIUserData = new APIUserData();
                    aPIUserData.emailId = aPITrainingNomination[i].EmailId;
                    aPIUserData.userName = aPITrainingNomination[i].UserName;
                    mailSendUser.Add(aPIUserData);
                }
            }

            if (userWebinarMaster != null)
            {

                EventMeeting eventMeeting = new EventMeeting();
                Start start = new Start();
                End end = new End();

                try
                {
                    eventMeeting.start = new Start();
                    eventMeeting.end = new End();

                    eventMeeting.subject = course.Title;
                    eventMeeting.start.dateTime = teamsScheduleDetail.StartTime;
                    eventMeeting.start.timeZone = "Asia/Kolkata";
                    eventMeeting.end.dateTime = teamsScheduleDetail.EndTime;
                    eventMeeting.end.timeZone = "Asia/Kolkata";
                    eventMeeting.IsOnlineMeeting = true;
                    eventMeeting.OnlineMeetingProvider = "teamsForBusiness";
                    eventMeeting.attendees = new Attendance[mailSendUser.Count];
                    for (int i = 0; i < mailSendUser.Count; i++)
                    {
                        eventMeeting.attendees[i] = new Attendance();
                        eventMeeting.attendees[i].emailAddress = new EmailAddress();
                        eventMeeting.attendees[i].emailAddress.address = mailSendUser[i].emailId;
                        eventMeeting.attendees[i].emailAddress.name = mailSendUser[i].userName;
                        eventMeeting.attendees[i].status = new Status1();
                        eventMeeting.attendees[i].type = "required";
                    }

                    JObject oJsonObject1 = new JObject();
                    oJsonObject1 = JObject.Parse(JsonConvert.SerializeObject(eventMeeting));

                    ConfigurableValues configurableValues = _db.ConfigurableValues.Where(a => a.ValueCode == "TEAMS").FirstOrDefault();

                    var baseUrl = "https://graph.microsoft.com/v1.0/me/events/" + teamsScheduleDetail.MeetingId;
                    HttpResponseMessage Response = await ApiHelper.CallPatchAPIForTeams(baseUrl, oJsonObject1, TeamsAccessToken);
                    TeamsEventResponse TeamsResponce = null;
                    if (Response.IsSuccessStatusCode)
                    {
                        var result = Response.Content.ReadAsStringAsync().Result;

                        TeamsResponce = JsonConvert.DeserializeObject<TeamsEventResponse>(result);
                    }
                    if (TeamsResponce != null)
                    {

                        teamsScheduleDetails.CourseID = iLTSchedule.CourseId;
                        teamsScheduleDetails.ScheduleID = Convert.ToInt32(teamsScheduleDetail.ScheduleID);
                        teamsScheduleDetails.CreatedBy = Convert.ToInt32(userId);
                        teamsScheduleDetails.CreatedDate = DateTime.UtcNow;
                        teamsScheduleDetails.ModifiedBy = Convert.ToInt32(userId);
                        teamsScheduleDetails.ModifiedDate = DateTime.UtcNow;

                        teamsScheduleDetails.IsActive = Record.Active;
                        teamsScheduleDetails.IsDeleted = Record.NotDeleted;

                        teamsScheduleDetails.iCalUId = TeamsResponce.iCalUId;
                        teamsScheduleDetails.MeetingId = TeamsResponce.id;
                        teamsScheduleDetails.StartTime = TeamsResponce.start.dateTime.ToString();
                        teamsScheduleDetails.EndTime = TeamsResponce.start.dateTime.ToString();
                        if (TeamsResponce.onlineMeeting == null)
                        {
                            teamsScheduleDetails.JoinUrl = TeamsResponce.webLink;
                        }
                        else
                        {
                            teamsScheduleDetails.JoinUrl = TeamsResponce.onlineMeeting.joinUrl;
                        }

                        teamsScheduleDetails.UserWebinarId = userWebinarMaster.Id;
                    }
                }

                catch (Exception ex)

                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return teamsScheduleDetails;
        }

        public async Task<List<APIILTSchedular>> GetAllActiveSchedulesV2(ApiNominationGet apiNominationGet, string OrganisationCode, int UserId)
        {
            List<APIILTSchedular> ScheduleList = new List<APIILTSchedular>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllActiveSchedules";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = apiNominationGet.page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = apiNominationGet.pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = apiNominationGet.searchParameter });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = apiNominationGet.searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@OrganisationCode", SqlDbType.NVarChar) { Value = OrganisationCode });
                            cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = apiNominationGet.showAllData });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APIILTSchedular obj = new APIILTSchedular();

                                obj.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                obj.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                obj.ModuleName = row["ModuleName"].ToString();
                                obj.StartDate = Convert.ToDateTime(row["StartDate"].ToString());
                                obj.EndDate = Convert.ToDateTime(row["EndDate"].ToString());
                                obj.StartTime = row["StartTime"].ToString();
                                obj.EndTime = row["EndTime"].ToString();
                                obj.RegistrationEndDate = Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();
                                obj.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                obj.PlaceName = row["PlaceName"].ToString();
                                obj.TrainerType = row["TrainerType"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                obj.CourseName = row["CourseName"].ToString();
                                obj.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : int.Parse(row["BatchId"].ToString());
                                obj.BatchCode = row["BatchCode"].ToString();
                                obj.BatchName = row["BatchName"].ToString();
                                obj.UserName = row["UserName"].ToString();
                                obj.UserCreated = Convert.ToBoolean(row["UserCreated"]);
                                ScheduleList.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return ScheduleList;
        }
    }
}


