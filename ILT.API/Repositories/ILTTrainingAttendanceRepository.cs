using AutoMapper;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Model.ILT;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static ILT.API.APIModel.APIGoToMeeting;
using log4net;
using Microsoft.Extensions.Configuration;

namespace ILT.API.Repositories
{
    public class ILTTrainingAttendanceRepository : Repository<ILTTrainingAttendance>, IILTTrainingAttendance
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTTrainingAttendanceRepository));
        StringBuilder sb = new StringBuilder();
        string[] header = { };
        string[] headerStar = { };
        string[] headerWithoutStar = { };
        List<string> ILTAttendance = new List<string>();
        APIAttendanceImport attendanceImport = new APIAttendanceImport();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;

        private CourseContext _db;
        IEmail _email;
        private readonly ICourseModuleAssociationRepository _courseModuleAssociationRepository;
        private readonly IModuleCompletionStatusRepository _moduleCompletionStatus;
        ICustomerConnectionStringRepository _customerConnection;
     //   IContentCompletionStatus _contentCompletionStatus;
     //   IMyCoursesRepository _myCoursesRepository;
        private IAccessibilityRule _accessibilityRule;
        private readonly IConfiguration _configuration;
        private readonly ICourseRepository _courseRepository;
        // IILTTrainingAttendance _iILTTrainingAttendance;
        public ILTTrainingAttendanceRepository(CourseContext context, IEmail email,
                                               ICourseModuleAssociationRepository courseModuleAssociationRepository,
                                               IModuleCompletionStatusRepository moduleCompletionStatus, IAccessibilityRule accessibilityRule,
                                               ICustomerConnectionStringRepository customerConnection, /*IMyCoursesRepository myCoursesRepository,*/
                                               //IContentCompletionStatus contentCompletionStatus,
                                               IConfiguration configuration,
                                               ICourseRepository courseRepository) : base(context)
        // IILTTrainingAttendance iILTTrainingAttendance
        {
            _db = context;
            this._email = email;
            _courseModuleAssociationRepository = courseModuleAssociationRepository;
            _moduleCompletionStatus = moduleCompletionStatus;
            this._customerConnection = customerConnection;
            //_contentCompletionStatus = contentCompletionStatus;
            this._accessibilityRule = accessibilityRule;
           // this._myCoursesRepository = myCoursesRepository;
            _configuration = configuration;
            _courseRepository = courseRepository;
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
                            cmd.CommandText = "GetSchduleForAttendance";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = CourseId });
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

        public async Task<List<APIILTScheduleDetails>> GetCourseAndSchedule(int page, int pageSize, int userid)
        {
            List<APITrainingAttendanceForAll> ScheduleList = new List<APITrainingAttendanceForAll>();
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
                            cmd.CommandText = "GetScheduleList";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userid });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

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
                                APITrainingAttendanceForAll obj = new APITrainingAttendanceForAll();

                                obj.ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();
                                obj.StartDate = DateTime.Parse(row["StartDate"].ToString());
                                obj.EndDate = DateTime.Parse(row["EndDate"].ToString());
                                obj.StartTime = TimeSpan.Parse(row["StartTime"].ToString());
                                obj.EndTime = TimeSpan.Parse(row["EndTime"].ToString());
                                obj.PlaceName = row["PlaceName"].ToString();
                                obj.Venue = row["Venue"].ToString();
                                obj.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                obj.ModuleName = row["ModuleName"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                obj.CourseName = row["CourseTitle"].ToString();

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

            List<APIILTScheduleDetails> aPIILTScheduleDetails = new List<APIILTScheduleDetails>();

            foreach (var data in ScheduleList)
            {
                APIILTScheduleDetails apiIltScheduleDetail = new APIILTScheduleDetails();

                apiIltScheduleDetail.CourseID = data.CourseID;
                apiIltScheduleDetail.CourseTitle = data.CourseName;
                apiIltScheduleDetail.ModuleID = data.ModuleID;
                apiIltScheduleDetail.ModuleTitle = data.ModuleName;

                apiIltScheduleDetail.ScheduleDetails = ScheduleList.Where(c => c.ModuleID == data.ModuleID).Select(c => new ScheduleList
                {
                    ScheduleID = c.ScheduleID,
                    ScheduleCode = c.ScheduleCode,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    startTime = c.StartTime,
                    endTime = c.EndTime,
                    Place = c.PlaceName,
                    Venue = c.Venue,
                    StartTimeNew = Convert.ToString(c.StartTime),
                    EndTimeNew = Convert.ToString(c.EndTime)

                }).ToList();

                aPIILTScheduleDetails.Add(apiIltScheduleDetail);
            }
            var total = aPIILTScheduleDetails.Select(p => p.CourseID).Count();

            var skip = pageSize * (page - 1);

            var canPage = skip < total;


            var a = aPIILTScheduleDetails.Select(p => new APIILTScheduleDetails
            {
                CourseID = p.CourseID,
                CourseTitle = p.CourseTitle,
                ModuleID = p.ModuleID,
                ModuleTitle = p.ModuleTitle,
                ScheduleDetails = p.ScheduleDetails,

            })
                        .Skip(skip)
                        .Take(pageSize).ToList();

            return a;
        }

        public async Task<List<APIILTCourseDetails>> GetILTCourses(int page, int pageSize, int userid)
        {
            List<APIILTCourseDetails> aPIILTCourseDetailsList = new List<APIILTCourseDetails>();
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
                            cmd.CommandText = "GetScheduleList";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userid });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

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
                                APIILTCourseDetails obj = new APIILTCourseDetails();
                                obj.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                obj.ModuleTitle = row["ModuleName"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                obj.CourseTitle = row["CourseTitle"].ToString();
                                obj.CourseType = row["CourseType"].ToString();
                                aPIILTCourseDetailsList.Add(obj);
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
            aPIILTCourseDetailsList = aPIILTCourseDetailsList.GroupBy(g=>new { g.CourseID, g.CourseTitle, g.ModuleID, g.ModuleTitle}).Select(g=>g.FirstOrDefault()).ToList();

            var total = aPIILTCourseDetailsList.Select(p => p.CourseID).Count();
            
            var skip = pageSize * (page - 1);
            
            var canPage = skip < total;
            
            return aPIILTCourseDetailsList.Skip(skip).Take(pageSize).ToList();
        }
        public async Task<List<ScheduleList>> GetILTScheduleDetails(int page, int pageSize, int userid, int ModuleId, int CourseId)
        {
            List<APITrainingAttendanceForAll> ScheduleList = new List<APITrainingAttendanceForAll>();
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
                            cmd.CommandText = "GetScheduleList";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userid });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

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
                                APITrainingAttendanceForAll obj = new APITrainingAttendanceForAll();

                                obj.ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();
                                obj.StartDate = DateTime.Parse(row["StartDate"].ToString());
                                obj.EndDate = DateTime.Parse(row["EndDate"].ToString());
                                obj.StartTime = TimeSpan.Parse(row["StartTime"].ToString());
                                obj.EndTime = TimeSpan.Parse(row["EndTime"].ToString());
                                obj.PlaceName = row["PlaceName"].ToString();
                                obj.Venue = row["Venue"].ToString();
                                obj.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                obj.ModuleName = row["ModuleName"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                obj.CourseName = row["CourseTitle"].ToString();

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
            List<ScheduleList> schedulesList = new List<ScheduleList>();
            schedulesList = ScheduleList.Where(c => c.ModuleID == ModuleId && c.CourseID == CourseId).Select(c => new ScheduleList
            {
                ScheduleID = c.ScheduleID,
                ScheduleCode = c.ScheduleCode,
                startDate = c.StartDate,
                endDate = c.EndDate,
                startTime = c.StartTime,
                endTime = c.EndTime,
                Place = c.PlaceName,
                Venue = c.Venue,
                StartTimeNew = Convert.ToString(c.StartTime),
                EndTimeNew = Convert.ToString(c.EndTime)

            }).ToList();

                
            var total = schedulesList.Select(p => p.ScheduleID).Count();

            var skip = pageSize * (page - 1);

            var canPage = skip < total;

            var result = schedulesList.Skip(skip).Take(pageSize).ToList();

            return result;
        }
        public async Task<List<APITrainingAttendanceForAll>> GetAllDetails(int page, int pageSize, int userId, string search = null, string searchText = null, bool showAllData = false)
        {
            List<APITrainingAttendanceForAll> TrainingAttendanceForAll = new List<APITrainingAttendanceForAll>();
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
                            cmd.CommandText = "GetAllDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = showAllData });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                APITrainingAttendanceForAll obj = new APITrainingAttendanceForAll();

                                obj.ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString());
                                obj.ScheduleCode = row["ScheduleCode"].ToString();
                                obj.StartDate = DateTime.Parse(row["StartDate"].ToString());
                                obj.EndDate = DateTime.Parse(row["EndDate"].ToString());
                                obj.PlaceName = row["PlaceName"].ToString();
                                obj.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                obj.ModuleName = row["ModuleName"].ToString();
                                obj.CourseID = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : int.Parse(row["CourseId"].ToString());
                                obj.CourseName = row["CourseTitle"].ToString();
                                obj.StartTime = TimeSpan.Parse(row["StartTime"].ToString());
                                obj.EndTime = TimeSpan.Parse(row["EndTime"].ToString());

                                obj.Venue = row["Venue"].ToString();
                                obj.ModifiedDate = DateTime.Parse(row["ModifiedDate"].ToString());
                                obj.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : int.Parse(row["BatchId"].ToString());
                                obj.BatchCode = row["BatchCode"].ToString();
                                obj.BatchName = row["BatchName"].ToString();
                                obj.UserName = row["UserName"].ToString();
                                TrainingAttendanceForAll.Add(obj);
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
            return TrainingAttendanceForAll;
        }

        public async Task<int> GetAllDetailsCount(int userId ,string search = null, string searchText = null, bool showAllData = false)
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
                            cmd.CommandText = "GetAllDetailsCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search ", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
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
                                APITrainingAttendanceForAll obj = new APITrainingAttendanceForAll();

                                Count = int.Parse(row["Count"].ToString());

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

        public async Task<List<APITrainingNomination>> GetAllUsersForAttendance(int scheduleID, int courseID, int page, int pageSize, string search, string searchText)
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
                            cmd.CommandText = "GetAllUsersForAttendance";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ScheduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseID });
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
                                TrainingNomination.TrainingRequestStatus = row["TrainingRequestStatus"].ToString();
                                TrainingNomination.IsPresent = string.IsNullOrEmpty(row["IsPresent"].ToString()) ? false : Convert.ToBoolean(row["IsPresent"].ToString());
                                TrainingNomination.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                TrainingNomination.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                TrainingNomination.OverAllStatus = row["OverAllStatus"].ToString();

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

        public async Task<List<APITrainingNomination>> GetWebinarUsersForAttendance(int scheduleID, int courseID, int page, int pageSize, string search, string searchText)
        {
            List<APITrainingNomination> TrainingNominationList = new List<APITrainingNomination>();

            //--------- Get ENABLE_GOTOMEETING congiguration --------------//
            string ENABLE_GOTOMEETING = await this.GetBoolConfigurablevalue("ENABLE_GOTOMEETING");

            //--------- Get ENABLE_GOTOMEETING configuration --------------//

            try
            {
                List<int> selectIndexList = null;
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetAllUsersForAttendance";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ScheduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseID });
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

                            //--------- Get ENABLE_GOTOMEETING configuration --------------//
                            if (ENABLE_GOTOMEETING == "Yes")
                            {
                                string uniqueMeetingId = _db.GoToMeetingDetails.Where(a => a.ScheduleID == scheduleID).Select(a => a.UniqueMeetingId).FirstOrDefault().ToString();

                                if ((!string.IsNullOrEmpty(uniqueMeetingId)) && (uniqueMeetingId != "0"))
                                {
                                    ILTOnlineSetting iltonlineSetting = _db.ILTOnlineSetting.Where(a => a.Type == "GOTOMEETING").FirstOrDefault();
                                    string responce = await ApiHelper.DirectLogin(iltonlineSetting);
                                    string accessToken = responce.ToString();
                                    string StartMeetingURL = "https://api.getgo.com/G2M/rest/meetings/" + uniqueMeetingId + "/attendees";
                                    MeetingAttendees[] MeetingAttendees = await ApiHelper.GetAttendeesByMeeting(accessToken, StartMeetingURL);
                                    List<int> typeIDList = new List<int>();
                                    try
                                    {
                                        StringBuilder strMailID = new StringBuilder();
                                        int i = 0;
                                        if (MeetingAttendees != null)
                                        {
                                            foreach (MeetingAttendees MeetingAttendees1 in MeetingAttendees)
                                            {

                                                if (i == 0)
                                                {
                                                    strMailID = strMailID.Append(Security.Encrypt(MeetingAttendees1.attendeeEmail.ToLower()));
                                                }
                                                else
                                                {
                                                    strMailID = strMailID.Append("," + Security.Encrypt(MeetingAttendees1.attendeeEmail.ToLower()));
                                                }
                                                i++;
                                            }
                                        }
                                        string InsertedUSerIDList = null;
                                        try
                                        {
                                            using (var dbContext1 = this._customerConnection.GetDbContext())
                                            {
                                                using (var connection1 = dbContext.Database.GetDbConnection())
                                                {
                                                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                                        connection.Open();
                                                    using (var cmd1 = connection.CreateCommand())
                                                    {
                                                        cmd1.CommandText = "GetUsersId_ForAttendance";
                                                        cmd1.CommandType = CommandType.StoredProcedure;
                                                        cmd1.Parameters.Add(new SqlParameter("@UserEmailIDsList", SqlDbType.NVarChar) { Value = strMailID.ToString() });
                                                        DbDataReader reader1 = await cmd1.ExecuteReaderAsync();
                                                        DataTable dt1 = new DataTable();
                                                        dt1.Load(reader1);
                                                        if (dt1.Rows.Count <= 0)
                                                        {
                                                            reader1.Dispose();
                                                            connection.Close();
                                                        }
                                                        if (dt1.Rows.Count > 0)
                                                            InsertedUSerIDList = string.IsNullOrEmpty(dt1.Rows[0]["InsertedUSerIDList"].ToString()) ? null : dt1.Rows[0]["InsertedUSerIDList"].ToString();

                                                        reader1.Dispose();
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
                                        if (InsertedUSerIDList != null)
                                        {
                                            selectIndexList = InsertedUSerIDList.ToString().Split(',').Select(int.Parse).ToList();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
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
                                    TrainingNomination.TrainingRequestStatus = row["TrainingRequestStatus"].ToString();

                                    if (selectIndexList.Contains(Convert.ToInt32(row["AutoGenerateUserID"].ToString())))
                                    {
                                        TrainingNomination.IsPresent = true;
                                    }
                                    else
                                    {
                                        TrainingNomination.IsPresent = false;
                                    }

                                    TrainingNomination.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                    TrainingNomination.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                    TrainingNomination.AttendanceStatus = row["AttendanceStatus"].ToString();

                                    TrainingNominationList.Add(TrainingNomination);
                                }
                                reader.Dispose();
                            }
                            else
                            {
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
                                    TrainingNomination.TrainingRequestStatus = row["TrainingRequestStatus"].ToString();
                                    TrainingNomination.IsPresent = string.IsNullOrEmpty(row["IsPresent"].ToString()) ? false : Convert.ToBoolean(row["IsPresent"].ToString());
                                    TrainingNomination.ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString());
                                    TrainingNomination.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                    TrainingNomination.AttendanceStatus = row["AttendanceStatus"].ToString();

                                    TrainingNominationList.Add(TrainingNomination);
                                }
                                reader.Dispose();
                            }
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

        public async Task<int> GetUsersCountForAttendance(int scheduleID, int courseID, string search, string searchText)
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
                            cmd.CommandText = "GetAllUsersCountForAttendance";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ScheduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseID });
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

        public async Task<bool> GetRegeneratedOTP(APIILTTrainingAttendance aPIILTTrainingAttendance, string OrganisationCode)
        {
            bool flag = false;
            // -------------- Check for Valid User ------------ //

            int UserId = 0;
            string UserName = null;
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
                            cmd.CommandText = "CheckForValidMobileNumber";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@MobileNumber", SqlDbType.VarChar) { Value = Security.Encrypt(aPIILTTrainingAttendance.MobileNumber) });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return false;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                UserId = int.Parse(row["Id"].ToString());
                                UserName = row["UserName"].ToString();
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

            // -------------- Check for Valid User ------------ //

            if (UserId != null)
            {
                ILTSchedule objSchedule = await this._db.ILTSchedule.Where(a => a.ID == aPIILTTrainingAttendance.ScheduleID).FirstOrDefaultAsync();
                string Otp = GenerateRandomPassword();

                // ------------ Add to UserOTPBindings ----------- //
                UserOTPBindings objUserOTPBindings = new UserOTPBindings();
                UserOTPBindings prevOTPBindings = new UserOTPBindings();

                prevOTPBindings = await this._db.UserOTPBindings.Where(a => a.ScheduleID == aPIILTTrainingAttendance.ScheduleID && a.ModuleID == aPIILTTrainingAttendance.ModuleID
                                                                       && a.CourseID == aPIILTTrainingAttendance.CourseID && a.UserID == UserId
                                                                       && a.AttendanceDate.Date == aPIILTTrainingAttendance.AttendanceDate.Date).FirstOrDefaultAsync();

                if (prevOTPBindings == null)
                {
                    objUserOTPBindings.ID = 0;
                    objUserOTPBindings.ScheduleID = aPIILTTrainingAttendance.ScheduleID;
                    objUserOTPBindings.ModuleID = aPIILTTrainingAttendance.ModuleID;
                    objUserOTPBindings.CourseID = aPIILTTrainingAttendance.CourseID;
                    objUserOTPBindings.UserID = UserId;
                    objUserOTPBindings.OTP = Otp;
                    objUserOTPBindings.IsAddedInNomination = false;
                    objUserOTPBindings.AttendanceDate = aPIILTTrainingAttendance.AttendanceDate;


                    this._db.UserOTPBindings.Add(objUserOTPBindings);
                    this._db.SaveChanges();
                }
                else
                {
                    prevOTPBindings.OTP = Otp;

                    this._db.UserOTPBindings.Update(prevOTPBindings);
                    this._db.SaveChanges();
                }

                // ------------ Add to UserOTPBindings ----------- //
                var SendSMSToUser = await GetMasterConfigurableParameterValue("SMS_FOR_ILT");
                if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                {
                    List<APINominateUserSMS> SMSList = new List<APINominateUserSMS>();
                    APINominateUserSMS objSMS = new APINominateUserSMS();
                    if (OrganisationCode.ToLower().Contains("sbil"))
                    {
                        objSMS.OTP = Otp;
                        objSMS.CourseTitle = aPIILTTrainingAttendance.CourseTitle;
                        objSMS.UserName = UserName;
                        objSMS.StartDate = aPIILTTrainingAttendance.AttendanceDate;
                        objSMS.StartTime = objSchedule.StartTime;
                        objSMS.EndDate = objSchedule.EndDate;
                        objSMS.EndTime = objSchedule.EndTime;
                        objSMS.ScheduleCode = objSchedule.ScheduleCode;
                        objSMS.UserID = UserId;
                        objSMS.MobileNumber = "91" + aPIILTTrainingAttendance.MobileNumber;
                        objSMS.organizationCode = OrganisationCode;
                    }
                    else
                    {
                        objSMS.OTP = Otp;
                        objSMS.CourseTitle = aPIILTTrainingAttendance.CourseTitle;
                        objSMS.UserName = UserName;
                        objSMS.StartDate = objSchedule.StartDate;
                        objSMS.StartTime = objSchedule.StartTime;
                        objSMS.ScheduleCode = objSchedule.ScheduleCode;
                        objSMS.UserID = UserId;
                        objSMS.MobileNumber = "91" + aPIILTTrainingAttendance.MobileNumber;
                        objSMS.organizationCode = OrganisationCode;
                    }
                    SMSList.Add(objSMS);

                    await this._email.SendILTNotificationSMS(SMSList);
                }
                return true;
            }
            return false;
        }

        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            Random generator = new Random();
            return generator.Next(100000, 999999).ToString("D6");
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
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return value;
        }
        public async Task<List<APIILTTrainingAttendance>> GOTOMeetingAttendance(int scheduleID, List<APIILTTrainingAttendance> aPIILTTrainingAttendance)
        {
            string uniqueMeetingId = _db.GoToMeetingDetails.Where(a => a.ScheduleID == scheduleID).Select(a => a.UniqueMeetingId).FirstOrDefault().ToString();

            if ((!string.IsNullOrEmpty(uniqueMeetingId)) && (uniqueMeetingId != "0"))
            {
                ILTOnlineSetting iltonlineSetting = _db.ILTOnlineSetting.Where(a => a.Type == "GOTOMEETING").FirstOrDefault();
                string responce = await ApiHelper.DirectLogin(iltonlineSetting);
                string accessToken = responce.ToString();
                string StartMeetingURL = "https://api.getgo.com/G2M/rest/meetings/" + uniqueMeetingId + "/attendees";
                MeetingAttendees[] MeetingAttendees = await ApiHelper.GetAttendeesByMeeting(accessToken, StartMeetingURL);
                List<int> typeIDList = new List<int>();
                try
                {
                    StringBuilder strMailID = new StringBuilder();
                    int i = 0;
                    if (MeetingAttendees != null)
                    {
                        foreach (MeetingAttendees MeetingAttendees1 in MeetingAttendees)
                        {

                            if (i == 0)
                            {
                                strMailID = strMailID.Append(Security.Encrypt(MeetingAttendees1.attendeeEmail.ToLower()));
                            }
                            else
                            {
                                strMailID = strMailID.Append("," + Security.Encrypt(MeetingAttendees1.attendeeEmail.ToLower()));
                            }
                            i++;
                        }
                    }

                    List<int> selectIndexList = null;

                    string InsertedUSerIDList = null;
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
                                    cmd.CommandText = "GetUsersId_ForAttendance";
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@UserEmailIDsList", SqlDbType.NVarChar) { Value = strMailID.ToString() });
                                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                                    DataTable dt = new DataTable();
                                    dt.Load(reader);
                                    if (dt.Rows.Count <= 0)
                                    {
                                        reader.Dispose();
                                        connection.Close();
                                    }

                                    InsertedUSerIDList = string.IsNullOrEmpty(dt.Rows[0]["InsertedUSerIDList"].ToString()) ? null : dt.Rows[0]["InsertedUSerIDList"].ToString();

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
                    if (InsertedUSerIDList != null)
                    {
                        selectIndexList = InsertedUSerIDList.ToString().Split(',').Select(int.Parse).ToList();

                        foreach (APIILTTrainingAttendance row in aPIILTTrainingAttendance)
                        {
                            if (selectIndexList.Contains(row.UserID))
                            {
                                row.IsPresent = true;
                            }
                            else
                            {
                                row.IsPresent = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            return aPIILTTrainingAttendance;
        }

        public async Task<string> GetBoolConfigurablevalue(string configurableparameter)
        {
            string ConfigurablevalueYN = "No";
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
                            cmd.CommandText = "GetConfigurableParameterValue";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = configurableparameter });
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
                                ConfigurablevalueYN = string.IsNullOrEmpty(row["Value"].ToString()) ? null : row["Value"].ToString();
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
            return ConfigurablevalueYN;
        }

        public async Task<List<APIUserAttendanceDetails>> GetAttendanceUserDetails(int ModuleId, int ScheduleId, int CourseId, int UserId, DateTime AttendanceDate)
        {
            List<APIUserAttendanceDetails> aPIUserAttendanceDetails = new List<APIUserAttendanceDetails>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@ModuleId", ModuleId);
                        parameters.Add("@ScheduleId", ScheduleId);
                        parameters.Add("@CourseId", CourseId);
                        parameters.Add("@UserId", UserId);
                        parameters.Add("@Date", AttendanceDate);
                        var Result = await SqlMapper.QueryAsync<APIUserAttendanceDetails>((SqlConnection)connection, "[dbo].[GetAttendanceUserDetails]", parameters, null, null, CommandType.StoredProcedure);
                        aPIUserAttendanceDetails = Result.ToList();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return aPIUserAttendanceDetails;
        }

        public async Task<int> CheckForValidDate(APIILTTrainingAttendance obj)
        {
            ILTSchedule objILTSchedule = await this._db.ILTSchedule.Where(a => a.IsActive == true && a.IsDeleted == false && a.ID == obj.ScheduleID).FirstOrDefaultAsync();
            if (objILTSchedule.StartDate.Date <= obj.AttendanceDate.Date && objILTSchedule.EndDate.Date >= obj.AttendanceDate.Date)
                return 1;
            return 0;
        }
        public async Task<ApiResponseILT> PostUserAttendance(List<APIILTTrainingAttendance> aPIILTTrainingAttendance, int userId, string OrganisationCode, string LoginId, string UserName, string UserRole)
        {
            try
            {
                int ScheduleId = aPIILTTrainingAttendance.Select(x => x.ScheduleID).FirstOrDefault();
                string ENABLE_GOTOMEETING = await this.GetBoolConfigurablevalue("ENABLE_GOTOMEETING");
                //--------- Get ENABLE_GOTOMEETING configuration --------------//
                if (ENABLE_GOTOMEETING == "Yes")
                {
                    aPIILTTrainingAttendance = await GOTOMeetingAttendance(ScheduleId, aPIILTTrainingAttendance);
                }
                //foreach (APIILTTrainingAttendance item in aPIILTTrainingAttendance)
                  //  item.isWeb = true;
                ApiResponseILT objApiResponse = new ApiResponseILT();
                List<APIILTTrainingAttendanceUsers> aPIILTTrainingAttendanceUsersList = Mapper.Map<List<APIILTTrainingAttendance>, List<APIILTTrainingAttendanceUsers>>(aPIILTTrainingAttendance);
                DataTable attendanceDataTable = new DataTable();
                SqlParameter ResponseParam;
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "[dbo].[ILTScheduleAttendance_Insert]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = UserRole });
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrganisationCode });
                            cmd.Parameters.Add(new SqlParameter("@ScheduleId", SqlDbType.Int) { Value = ScheduleId });
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPIILTTrainingAttendance.Select(x => x.CourseID).FirstOrDefault() });
                            cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = aPIILTTrainingAttendance.Select(x => x.ModuleID).FirstOrDefault() });
                            cmd.Parameters.Add(new SqlParameter("@AttendanceDate", SqlDbType.DateTime) { Value = aPIILTTrainingAttendance.Select(x => x.AttendanceDate).FirstOrDefault() });
                            cmd.Parameters.Add(new SqlParameter("@ILTAttendanceUserList", SqlDbType.Structured) { Value = aPIILTTrainingAttendanceUsersList.ToList().ToDataTable() });
                            ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 100);
                            ResponseParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(ResponseParam);
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            attendanceDataTable.Load(reader);
                            connection.Close();
                        }
                    }
                }
                string ResponseValue = Convert.ToString(ResponseParam.Value);
                if (ResponseValue == "Success")
                {
                    List<APIILTAttendanceResponse> aPIILTAttendanceResponseList = attendanceDataTable.ConvertToList<APIILTAttendanceResponse>();
                    List<APINominationUserResponse> aPINominationUserResponses = Mapper.Map<List<APINominationUserResponse>>(aPIILTAttendanceResponseList);
                    int TotalInserted = aPINominationUserResponses.Where(x => x.Status == "Inserted").Count();
                    int TotalUpdated = aPINominationUserResponses.Where(x => x.Status == "Updated").Count();
                    int TotalRejected = aPINominationUserResponses.Where(x => (x.Status == "Rejected" || x.Status == "Duplicate")).Count();
                    aPIILTAttendanceResponseList = aPIILTAttendanceResponseList.Where(x => (x.RecordStatus == "Inserted" || (x.RecordStatus == "Updated" && x.IsDuplicate==false))).ToList();
                    await SendCourseApplicabilityNominationNotifications(aPIILTAttendanceResponseList.Where(x => x.IsNominated == true).ToList(), OrganisationCode, userId);
                    await PostAttendance(aPIILTAttendanceResponseList, userId, OrganisationCode, LoginId, UserName);
                    if(TotalRejected>0)
                        objApiResponse.StatusCode = 400;
                    else
                        objApiResponse.StatusCode = 200;
                    objApiResponse.aPINominationResponses = aPINominationUserResponses;
                    objApiResponse.Description = "Total Inserted: "+TotalInserted+", Updated: "+TotalUpdated+" and Rejected: "+TotalRejected+".";
                    return objApiResponse;
                }
                else
                {
                    return new ApiResponseILT { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ResponseValue, StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        private async Task PostAttendance(List<APIILTAttendanceResponse> aPIILTAttendanceResponseList, int UserId, string OrganisationCode, string LoginId, string UserName)
        {
            string PUSH_ATTENDANCE_DATA_TO_CLIENT = await this.GetBoolConfigurablevalue("PUSH_ATTENDANCE_DATA_TO_CLIENT");
            string SCHEDULE_ABSENT_EMAIL = await this.GetBoolConfigurablevalue("SCHEDULE_ABSENT_EMAIL");
            string COURSE_COMPLETION_EMAIL = await this.GetBoolConfigurablevalue("COURSE_COMPLETION_EMAIL");

            foreach (APIILTAttendanceResponse item in aPIILTAttendanceResponseList)
            {
                if (OrganisationCode.ToLower().Contains("canh"))
                {
                    APIAttendancePushData aPIAttendancePushData = new APIAttendancePushData();

                    if (PUSH_ATTENDANCE_DATA_TO_CLIENT.ToLower() == "yes")
                    {
                        if (!string.IsNullOrEmpty(item.UserId))
                        {
                            aPIAttendancePushData.brCode = item.UserId;
                            aPIAttendancePushData.planCode = item.ScheduleCode;
                            aPIAttendancePushData.prdTrainComplDate = item.AttendanceDate.ToString("MM/dd/yyyy");
                            aPIAttendancePushData.prdTrainComplDate = aPIAttendancePushData.prdTrainComplDate.Replace("-", "/");
                            aPIAttendancePushData.nameOfTrainer = UserName;
                            aPIAttendancePushData.trainerEmpCode = UserId.ToString();
                            aPIAttendancePushData.isAmlCompleted = Convert.ToString(item.OverallStatus).ToLower() == "completed" ? "yes" : "no";
                            aPIAttendancePushData.sharePointId = item.AttendanceId;

                            await SaveAttendanceData(aPIAttendancePushData, UserId);
                        }
                    }
                }

                if (item.AttendanceStatus == AttendaceStatus.Attended || item.AttendanceStatus == AttendaceStatus.Waiver)
                {
                    if (Convert.ToString(item.OverallStatus).ToLower() == "completed" && Convert.ToString(item.CourseStatus).ToLower() == "completed")
                    {
                        if (COURSE_COMPLETION_EMAIL.ToLower().ToString() == "yes")
                        {
                            APIUserDetails aPIUserDetails = new APIUserDetails()
                            {
                                CustomerCode = OrganisationCode,
                                EmailId = item.EmailId,
                                UserName = item.UserName
                            };
                            if (aPIUserDetails != null)
                                Task.Run(() => _email.SendCourseCompletionStatusMail(item.CourseTitle, item.UserMasterId, aPIUserDetails, item.CourseId));
                        }
                    }
                    if (!item.IsAttedanceEmailSent)
                    {
                        Task.Run(() => _email.SendMailForAttendanceUsers(item, OrganisationCode));
                    }
                }
                // mail trigger for absent users
                else if (item.AttendanceStatus == AttendaceStatus.Absent)
                {
                    if (SCHEDULE_ABSENT_EMAIL == "Yes")
                    {
                        Task.Run(() => _email.SendMailForAbsentUsers(item, OrganisationCode));
                    }
                }
            }
        }
        private async Task SendCourseApplicabilityNominationNotifications(List<APIILTAttendanceResponse> aPIILTAttendanceResponse, string OrgCode, int CreatedBy)
        {
            List<int> CourseIds = aPIILTAttendanceResponse.Select(c => c.CourseId).Distinct().ToList();
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

            foreach (APIILTAttendanceResponse item in aPIILTAttendanceResponse)
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
        public async Task CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
            }
        }
        public async Task<ApiResponse> PostUserAttendance(APIILTTrainingAttendance aPIILTTrainingAttendance, int userId)
        {
            ApiResponse objApiResponse = new ApiResponse();

            int scheduleID = aPIILTTrainingAttendance.ScheduleID;
            DateTime getScheduleEndDate = await this._db.ILTSchedule.Where(a => a.IsActive == true && a.IsDeleted == false && a.ID == scheduleID)
                                                                    .Select(ilt => ilt.EndDate).FirstOrDefaultAsync();

            bool IsNominated = await this._db.TrainingNomination.Where(r => r.IsActive == true && r.IsDeleted == false && r.ID == scheduleID && r.UserID == aPIILTTrainingAttendance.UserID && r.TrainingRequestStatus == "Nominated" && r.IsActiveNomination == true)
                                                                    .Select(reg => reg.IsActive).FirstOrDefaultAsync();

            bool IsRegistered = await this._db.TrainingNomination.Where(n => n.IsActive == true && n.IsDeleted == false && n.ID == scheduleID && n.UserID == aPIILTTrainingAttendance.UserID && n.TrainingRequestStatus == "Registered" && n.IsActiveNomination == true)
                                                             .Select(nom => nom.IsActive).FirstOrDefaultAsync();


            if (IsNominated == false)
            {
                objApiResponse.StatusCode = 102;
                objApiResponse.Description = "User Not Nominated";
                return objApiResponse;
            }
            if (IsRegistered == true)
            {
                objApiResponse.StatusCode = 103;
                objApiResponse.Description = "User Not Nominated";
                return objApiResponse;
            }

            foreach (UserDetails row in aPIILTTrainingAttendance.UserDetails)
            {
                ILTTrainingAttendance objILTTrainingAttendance = Mapper.Map<ILTTrainingAttendance>(row);
                objILTTrainingAttendance.IsActive = true;
                objILTTrainingAttendance.IsDeleted = false;
                objILTTrainingAttendance.UserID = row.UserID;
                objILTTrainingAttendance.CreatedBy = userId;
                objILTTrainingAttendance.CreatedDate = DateTime.UtcNow;
                objILTTrainingAttendance.ModifiedBy = userId;
                objILTTrainingAttendance.ModifiedDate = DateTime.UtcNow;

                await this.Add(objILTTrainingAttendance);

                //ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();

                //ContentCompletionStatus.Id = 0;
                //ContentCompletionStatus.CourseId = aPIILTTrainingAttendance.CourseID;
                //ContentCompletionStatus.ModuleId = aPIILTTrainingAttendance.ModuleID;
                //ContentCompletionStatus.Status = Status.Completed;
                //ContentCompletionStatus.UserId = userId;
                //ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                //ContentCompletionStatus.IsDeleted = false;
                //ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                //ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                //ContentCompletionStatus.UserId = userId;
                //ContentCompletionStatus.ScheduleId = aPIILTTrainingAttendance.ScheduleID;

                //await _contentCompletionStatus.Post(ContentCompletionStatus);
            }

            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponseILT> UpdateILTAttendance(List<APIILTTrainingAttendance> aPIILTTrainingAttendance, int UserId, string LoginId, string OrganisationCode, string UserName, string UserRole)
        {
            try
            {
                int ScheduleId = aPIILTTrainingAttendance.Select(x => x.ScheduleID).FirstOrDefault();

                ApiResponseILT objApiResponse = new ApiResponseILT();
                List<APIILTTrainingAttendanceUsers> aPIILTTrainingAttendanceUsersList = Mapper.Map<List<APIILTTrainingAttendance>, List<APIILTTrainingAttendanceUsers>>(aPIILTTrainingAttendance);
                DataTable attendanceDataTable = new DataTable();
                SqlParameter ResponseParam;
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "[dbo].[ILTScheduleAttendance_Insert]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@RoleCode", SqlDbType.VarChar) { Value = UserRole });
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrganisationCode });
                            cmd.Parameters.Add(new SqlParameter("@ScheduleId", SqlDbType.Int) { Value = ScheduleId });
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPIILTTrainingAttendance.Select(x => x.CourseID).FirstOrDefault() });
                            cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = aPIILTTrainingAttendance.Select(x => x.ModuleID).FirstOrDefault() });
                            cmd.Parameters.Add(new SqlParameter("@AttendanceDate", SqlDbType.DateTime) { Value = aPIILTTrainingAttendance.Select(x => x.AttendanceDate).FirstOrDefault() });
                            cmd.Parameters.Add(new SqlParameter("@ILTAttendanceUserList", SqlDbType.Structured) { Value = aPIILTTrainingAttendanceUsersList.ToList().ToDataTable() });
                            ResponseParam = new SqlParameter("@Response", SqlDbType.VarChar, 100);
                            ResponseParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(ResponseParam);
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            attendanceDataTable.Load(reader);
                            connection.Close();
                        }
                    }
                }

                string ResponseValue = Convert.ToString(ResponseParam.Value);
                if (ResponseValue == "Success")
                {
                    List<APIILTAttendanceResponse> aPIILTAttendanceResponseList = attendanceDataTable.ConvertToList<APIILTAttendanceResponse>();
                    List<APINominationUserResponse> aPINominationUserResponses = Mapper.Map<List<APINominationUserResponse>>(aPIILTAttendanceResponseList);
                    int TotalInserted = aPINominationUserResponses.Where(x => x.Status == "Inserted").Count();
                    int TotalUpdated = aPINominationUserResponses.Where(x => x.Status == "Updated").Count();
                    int TotalRejected = aPINominationUserResponses.Where(x => (x.Status == "Rejected" || x.Status == "Duplicate")).Count();
                    aPIILTAttendanceResponseList = aPIILTAttendanceResponseList.Where(x => (x.RecordStatus == "Inserted" || (x.RecordStatus == "Updated" && x.IsDuplicate==false))).ToList();
                    await SendCourseApplicabilityNominationNotifications(aPIILTAttendanceResponseList.Where(x => x.IsNominated == true).ToList(), OrganisationCode, UserId);
                    await PostAttendance(aPIILTAttendanceResponseList, UserId, OrganisationCode, LoginId, UserName);
                    if (TotalRejected > 0)
                        objApiResponse.StatusCode = 400;
                    else
                        objApiResponse.StatusCode = 200;
                    objApiResponse.aPINominationResponses = aPINominationUserResponses;
                    objApiResponse.Description = "Total Inserted: " + TotalInserted + ", Updated: " + TotalUpdated + " and Rejected: " + TotalRejected + ".";
                    return objApiResponse;
                }
                else
                {
                    return new ApiResponseILT { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ResponseValue, StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;//return 0;
            }
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
                        dt.Columns.Add(firstRowCell.Text.Trim());
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
        public async Task<bool> ValidateFileColumnHeaders(DataTable userImportdt, List<string> importColumns)
        {
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
        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APITrainingAttendanceImportColumns.CourseCode, 250));
            columns.Add(new KeyValuePair<string, int>(APITrainingAttendanceImportColumns.ModuleName, 250));
            columns.Add(new KeyValuePair<string, int>(APITrainingAttendanceImportColumns.ScheduleCode, 250));
            columns.Add(new KeyValuePair<string, int>(APITrainingAttendanceImportColumns.UserId, 2000));
            columns.Add(new KeyValuePair<string, int>(APITrainingAttendanceImportColumns.IsPresent, 50));
            columns.Add(new KeyValuePair<string, int>(APITrainingAttendanceImportColumns.IsWaiver, 50));
            columns.Add(new KeyValuePair<string, int>(APITrainingAttendanceImportColumns.AttendanceDate, 30));
            return columns;
        }
        public async Task<ApiResponse> ProcessImportFile(APIDataMigrationFilePath aPIDataMigration, int UserId, string OrgCode, string LoginId, string UserName)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                string filepath = sWebRootFolder + aPIDataMigration.Path;

                DataTable attendanceImportdt = ReadFile(filepath);
                if (attendanceImportdt == null || attendanceImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(attendanceImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(attendanceImportdt, UserId, OrgCode, LoginId, UserName, importcolumns);
                    Reset();
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
                    Reset();
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }

        public void Reset()
        {
            sb.Clear();
            header = new string[0];
            headerStar = new string[0];
            headerWithoutStar = new string[0];
            ILTAttendance.Clear();

            sbError.Clear();
            totalRecordInsert = 0;
            totalRecordRejected = 0;
        }

        public async Task<bool> InitilizeAsync(FileInfo file)
        {
            bool result = false;
            try
            {
                using (ExcelPackage package = new ExcelPackage(file))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    int rowCount = worksheet.Dimension.Rows;
                    int ColCount = worksheet.Dimension.Columns;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= ColCount; col++)
                        {
                            string append = "";
                            if (worksheet.Cells[row, col].Value == null)
                            {

                            }
                            else
                            {
                                append = Convert.ToString(worksheet.Cells[row, col].Value.ToString());
                            }
                            string finalAppend = append + "\t";
                            sb.Append(finalAppend);

                        }
                        sb.Append(Environment.NewLine);
                    }

                    string fileInfo = sb.ToString();
                    ILTAttendance = new List<string>(fileInfo.Split('\n'));
                    foreach (string record in ILTAttendance)
                    {
                        string[] mainsp = record.Split('\r');
                        string[] mainsp2 = mainsp[0].Split('\"');
                        header = mainsp2[0].Split('\t');
                        headerStar = mainsp2[0].Split('\t');
                        break;
                    }
                    ILTAttendance.RemoveAt(0);
                    result = true;

                }
                /////Remove Star from Header
                for (int i = 0; i < header.Count(); i++)
                {
                    header[i] = header[i].Replace("*", "");

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                result = false;
            }

            return result;
        }

        public async Task<ApiResponse> ProcessRecordsAsync(DataTable attendanceImportdt, int userId, string OrgCode, string LoginId, string UserName, List<string> importcolumns)
        {
            ApiResponse response = new ApiResponse();
            List<APIAttendanceImport> apiAttendanceImportRejected = new List<APIAttendanceImport>();
            var applicationDateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);

            attendanceImportdt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = attendanceImportdt.Columns;
            foreach (string column in importcolumns)
            {
                attendanceImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = attendanceImportdt.Clone();
            if (attendanceImportdt != null && attendanceImportdt.Rows.Count > 0)
            {
                List<APIAttendanceImport> apiAttendanceImportList = new List<APIAttendanceImport>();
                foreach (DataRow dataRow in attendanceImportdt.Rows)
                {
                    bool IsError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column, "AttendanceDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string AttendanceDate = Convert.ToString(dataRow[column]);
                                string attDate = ValidateDateOfAttendance(AttendanceDate, applicationDateFormat);
                                if (attDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "Attendance date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                {
                                    dataRow[column] = attDate;
                                }
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Attendance date required.";
                            }
                        }
                        else if (string.Compare(column, "UserId", true) == 0)
                        {
                            dataRow[column] = !string.IsNullOrEmpty(Convert.ToString(dataRow[column])) ? Security.Encrypt(Convert.ToString(dataRow[column]).ToLower()) : null;
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
                        attendanceImport.UserId = dataRow["UserId"] != null ? Convert.ToString(dataRow["UserId"]) : null;
                        attendanceImport.CourseCode = dataRow["CourseCode"] != null ? Convert.ToString(dataRow["CourseCode"]) : null;
                        attendanceImport.ModuleName = dataRow["ModuleName"] != null ? Convert.ToString(dataRow["ModuleName"]) : null;
                        attendanceImport.ScheduleCode = dataRow["ScheduleCode"] != null ? Convert.ToString(dataRow["ScheduleCode"]) : null;
                        attendanceImport.IsPresent = dataRow["IsPresent"] != null ? Convert.ToString(dataRow["IsPresent"]) : null;
                        attendanceImport.IsWaiver = dataRow["IsWaiver"] != null ? Convert.ToString(dataRow["IsWaiver"]) : null;
                        try
                        {
                            if (dataRow["AttendanceDate"] != null)
                                attendanceImport.AttendanceDate = Convert.ToDateTime(dataRow["AttendanceDate"]);
                        }
                        catch { }
                        attendanceImport.ErrMessage = errorMsg;
                        attendanceImport.IsInserted = "false";
                        attendanceImport.IsUpdated = "false";
                        attendanceImport.InsertedID = null;
                        attendanceImport.InsertedCode = "";
                        attendanceImport.notInsertedCode = "";
                        dataRow["ErrorMessage"] = attendanceImport.ErrMessage;
                        apiAttendanceImportList.Add(attendanceImport);
                    }
                    else
                    {
                        finalDt.ImportRow(dataRow);
                    }

                    attendanceImport = new APIAttendanceImport();
                    sbError.Clear();
                }

                try
                {
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DataTable dtResult = new DataTable();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "dbo.ILT_ScheduleAttendance_BulkUpload";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                            cmd.Parameters.Add(new SqlParameter("@ILTScheduleAttendanceBulkUpload_TVP", SqlDbType.Structured) { Value = finalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }
                        apiAttendanceImportList.AddRange(dtResult.ConvertToList<APIAttendanceImport>());
                        connection.Close();
                        if (apiAttendanceImportList != null)
                        {
                            if (apiAttendanceImportList.Where(x => x.ErrMessage == null).Count() > 0)
                            {
                                List<APIILTAttendanceResponse> aPIILTAttendanceResponseList = dtResult.ConvertToList<APIILTAttendanceResponse>();
                                await SendCourseApplicabilityNominationNotifications(aPIILTAttendanceResponseList.Where(x => x.IsNominated == true).ToList(), OrgCode, userId);
                                await PostAttendance(aPIILTAttendanceResponseList, userId, OrgCode, LoginId, UserName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiAttendanceImportList)
                {
                    if (!string.IsNullOrEmpty(data.CourseCode))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            data.UserId = data.UserId.Decrypt();
                            apiAttendanceImportRejected.Add(data);
                        }
                        else
                        {
                            totalRecordInsert++;
                        }
                    }
                }
            }
            string resultstring = "Total number of record inserted :" + totalRecordInsert + ",  Total number of record record rejected : " + totalRecordRejected;

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiAttendanceImportRejected };
            return response;
        }

        public string ValidateDateOfAttendance(string dateOfAtt, string validDateFormat)
        {
            string dateOfAttendance = null;
            try
            {
                DateTime result;
                result = DateTime.ParseExact(dateOfAtt, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                string inputstring = result.ToString("dd/MM/yyyy");
                inputstring = inputstring.Replace("-", "/");
                inputstring = inputstring.Replace(".", "/");
                string[] dateParts = inputstring.Split('/');
                string day = dateParts[0];
                string month = dateParts[1];
                string year = dateParts[2];
                dateOfAttendance = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return dateOfAttendance;
        }

        public async Task<IEnumerable<APIAttendance>> GetAttendanceWiseReport(APIAttendance aPIAttendance)
        {
            List<APIAttendance> Obj = new List<APIAttendance>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@CourseID", aPIAttendance.CourseID);
                        parameters.Add("@ScheduleID", aPIAttendance.ScheduleID);


                        var Result = await SqlMapper.QueryAsync<APIAttendance>((SqlConnection)connection, "[dbo].[GetAllUsersForAttendanceExport]", parameters, null, null, CommandType.StoredProcedure);
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

        public async Task<List<APIDetailsOfUserAfterAttendance>> GetDetailsForUserAttendance(APIGetDetailsForUserAttendance objUserDetails)
        {
            List<APIDetailsOfUserAfterAttendance> UserDetails = new List<APIDetailsOfUserAfterAttendance>();
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
                            cmd.CommandText = "GetDetailsForUserAttendance";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ScheduleID", SqlDbType.Int) { Value = objUserDetails.scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = objUserDetails.courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = objUserDetails.moduleId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = objUserDetails.UserId });

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
                                APIDetailsOfUserAfterAttendance obj = new APIDetailsOfUserAfterAttendance();

                                obj.AttendanceStatus = row["AttendanceStatus"].ToString();
                                obj.AttendanceDate = row["AttendanceDate"].ToString();

                                UserDetails.Add(obj);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return UserDetails;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public bool CheckForHolidayAttendance(APIILTTrainingAttendance aPIILTTrainingAttendance)
        {
            try
            {
                ScheduleHolidayDetails scheduleHolidayDetails = this._db.ScheduleHolidayDetails.Where(a => a.ReferenceID == aPIILTTrainingAttendance.ScheduleID && a.Date.Date == aPIILTTrainingAttendance.AttendanceDate.Date && a.IsHoliday == true).FirstOrDefault();
                if (scheduleHolidayDetails != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return false;
            }
        }

        public async Task<bool> SaveAttendanceData(APIAttendancePushData data, int UserId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    JObject oJsonObject = new JObject();

                    oJsonObject.Add("brCode", data.brCode);
                    oJsonObject.Add("planCode", data.planCode);
                    oJsonObject.Add("prdTrainComplDate", data.prdTrainComplDate);
                    oJsonObject.Add("nameOfTrainer", data.nameOfTrainer);
                    oJsonObject.Add("trainerEmpCode", data.trainerEmpCode);
                    oJsonObject.Add("isAmlCompleted", data.isAmlCompleted);
                    oJsonObject.Add("sharePointId", data.sharePointId);

                    HttpResponseMessage response = await client.PostAsJsonAsync("https://touchstone.canarahsbc.org/LBSService/saveLbsTrainingData", oJsonObject);

                    if (response.IsSuccessStatusCode)
                    {
                        // Get the URI of the created resource.
                        Uri gizmoUrl = response.Headers.Location;
                    }

                    AttendanceDataForCANH attendanceDataForCANH = new AttendanceDataForCANH();

                    attendanceDataForCANH.AttendanceId = data.sharePointId;
                    attendanceDataForCANH.Message = response.ReasonPhrase;
                    attendanceDataForCANH.InsertedJSON = oJsonObject.ToString();
                    attendanceDataForCANH.IsActive = true;
                    attendanceDataForCANH.IsDeleted = false;
                    attendanceDataForCANH.CreatedDate = DateTime.UtcNow;
                    attendanceDataForCANH.ModifiedDate = DateTime.UtcNow;
                    attendanceDataForCANH.CreatedBy = UserId;
                    attendanceDataForCANH.ModifiedBy = UserId;

                    this._db.AttendanceDataForCANH.Add(attendanceDataForCANH);
                    this._db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return true;
        }

        public class APIAttendancePushData
        {
            public string brCode { get; set; } // SP code EmployeeCode
            public string planCode { get; set; } // ILTScheduleCode
            public string prdTrainComplDate { get; set; } // TrainingCompletionDate
            public string nameOfTrainer { get; set; } // TrainerName
            public string trainerEmpCode { get; set; } // TRainerEmployeeCode
            public string isAmlCompleted { get; set; } // Training completed or Not (Y/N) Flag 
            public int sharePointId { get; set; } // Unique transaction ID
        }
        public async Task<int> CheckMultipleOTP(APIILTTrainingAttendance aPIILTTrainingAttendance)
        {
            ILTTrainingAttendanceDetails iLTTrainingAttendanceDetails = new ILTTrainingAttendanceDetails();

            try
            {
                UserOTPBindings userOTPBindings = new UserOTPBindings();
                iLTTrainingAttendanceDetails = await this._db.ILTTrainingAttendanceDetails.Where(tu => /*tu.AttendanceId == IDforOTP &&*/ tu.OTP == aPIILTTrainingAttendance.OTP).FirstOrDefaultAsync();
                if (iLTTrainingAttendanceDetails != null)
                {
                    return 0;
                }
                else
                    userOTPBindings = await this._db.UserOTPBindings.OrderByDescending(a => a.ID).Where(a => a.ScheduleID == aPIILTTrainingAttendance.ScheduleID && a.ModuleID == aPIILTTrainingAttendance.ModuleID
                                      && a.CourseID == aPIILTTrainingAttendance.CourseID && a.AttendanceDate.Date == aPIILTTrainingAttendance.AttendanceDate.Date && a.OTP == aPIILTTrainingAttendance.OTP
                                      /*a.UserID == aPIILTTrainingAttendance.UserID*/).FirstOrDefaultAsync();
                DateTime? userOTPDate = userOTPBindings.AttendanceDate.Date;
                DateTime userAPIDate = aPIILTTrainingAttendance.AttendanceDate.Date;
                if (userOTPDate == userAPIDate)
                    return 2;
                else
                    return 1;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 1;
            }
        }
        public async Task<ILTSchedule> ReadSchedulestartdaate(APIILTTrainingAttendance aPIILTTrainingAttendance)
        {
            try
            {
                ILTSchedule aPIILTSchedule = new ILTSchedule();
                aPIILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == aPIILTTrainingAttendance.ScheduleID).FirstOrDefaultAsync();
                return aPIILTSchedule;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }
        public async Task<bool> CheckForValidTrainer(APIILTTrainingAttendance aPIILTTrainingAttendance, int UserId, string RoleCode)
        {
            try
            {
                ILTScheduleTrainerBindings iLTScheduleTrainerBindings = new ILTScheduleTrainerBindings();

                iLTScheduleTrainerBindings = await this._db.ILTScheduleTrainerBindings.Where(a => a.IsActive == true && a.IsDeleted == false && a.ScheduleID == aPIILTTrainingAttendance.ScheduleID && a.TrainerID == UserId).FirstOrDefaultAsync();

                if (iLTScheduleTrainerBindings == null)
                {
                    return true;
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return true;
            }
        }
        public async Task<string> GetApplicationDateFormat(string OrgCode)
        {
            string dateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
            return dateFormat;
        }
    }
}
