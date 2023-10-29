//using Courses.API.APIModel.ILT;
//using Courses.API.APIModel.TNA;
using TNA.API.Helper;
using TNA.API.Model;
//using Courses.API.Model.ILT;
//using Courses.API.Model.TNA;
//using Courses.API.Models;
using TNA.API.Repositories.Interfaces;
//using Courses.API.Repositories.Interfaces.TNA;
using TNA.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TNA.API.Common;
using log4net;
using TNA.API.APIModel;

namespace TNA.API.Repositories
{
    // Schedule Enrollment Request Levels (Without BeSpoke)
    // EU-1, LM-2, TA-3, BU-4, HR1-5, HR2-6
    public class CourseScheduleEnrollmentRequestRepository : Repository<CourseScheduleEnrollmentRequest>, ICourseScheduleEnrollmentRequest
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseScheduleEnrollmentRequestRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        IAccessibilityRule _accessibilityRule;
        private readonly ITLSHelper _tlsHelper;
        private readonly IConfiguration _configuration;
        INotification _notification;
        IEmail _email;
        IIdentityService _identitySv;
        IEmail _emailRepository;
        public CourseScheduleEnrollmentRequestRepository(IHttpContextAccessor httpContextAccessor, ICustomerConnectionStringRepository customerConnection,
                                       ICustomerConnectionStringRepository customerConnectionStringRepository, IIdentityService identitySv,
                                       IAccessibilityRule AccessibilityRule, INotification notification, IConfiguration configuration, IEmail email, ITLSHelper tlsHelper, IEmail emailRepository, CourseContext context) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            _accessibilityRule = AccessibilityRule;
            this._tlsHelper = tlsHelper;
            this._configuration = configuration;
            this._notification = notification;
            this._email = email;
            this._identitySv = identitySv;
            this._emailRepository = emailRepository;
        }


        public async Task<APIModuleCompletionDetails> GetModuleCompletionStatus(int ModuleID, int UserId)

        {
            APIModuleCompletionDetails obj = new APIModuleCompletionDetails();
            int scheduleId = _db.CourseScheduleEnrollmentRequest.Where(a => a.ModuleID == ModuleID && a.UserID == UserId).Select(a => a.ScheduleID).FirstOrDefault();
            if (scheduleId != 0)
            {
                obj = (from ILTSchedule in this._db.ILTSchedule
                       join TrainingNomination in this._db.TrainingNomination on ILTSchedule.ID equals TrainingNomination.ScheduleID
                       join ModuleCompletionStatus in this._db.ModuleCompletionStatus on ILTSchedule.ModuleId equals ModuleCompletionStatus.ModuleId into tempModuleCompletionStatus
                       from ModuleCompletionStatus in tempModuleCompletionStatus.DefaultIfEmpty()
                       join TrainingPlace in this._db.TrainingPlace on ILTSchedule.PlaceID equals TrainingPlace.Id into tempPlace
                       from TrainingPlace in tempPlace.DefaultIfEmpty()
                       join AcademyAgency in this._db.AcademyAgencyMaster on ILTSchedule.AcademyAgencyID equals AcademyAgency.Id into tempAcademyAgency
                       from AcademyAgency in tempAcademyAgency.DefaultIfEmpty()
                       join module in this._db.Module on ModuleCompletionStatus.ModuleId equals module.Id into tempmodule
                       from module in tempmodule.DefaultIfEmpty()
                       join ModuleTopicAssociation in this._db.ModuleTopicAssociation on module.Id equals ModuleTopicAssociation.ModuleId
                       where ModuleCompletionStatus.ModuleId == ModuleID && TrainingNomination.UserID == UserId && ILTSchedule.ID == scheduleId && ModuleCompletionStatus.IsDeleted == Record.NotDeleted
                       select new APIModuleCompletionDetails
                       {
                           ModuleCompletionStatus = ModuleCompletionStatus.Status,
                           ScheduleID = ILTSchedule.ID,
                           StartDate = ILTSchedule.StartDate,
                           EndDate = ILTSchedule.EndDate,
                           StartTime = ILTSchedule.StartTime.ToString("hh\\:mm"),
                           EndTime = ILTSchedule.EndTime.ToString("hh\\:mm"),
                           Venue = TrainingPlace.PostalAddress,
                           AcademyName = AcademyAgency.AcademyAgencyName,
                           ScheduleCode = ILTSchedule.ScheduleCode,
                           ModuleName = module.Name,
                       }).FirstOrDefault();
            }


            obj = (from ILTSchedule in this._db.ILTSchedule
                   join TrainingNomination in this._db.TrainingNomination on ILTSchedule.ID equals TrainingNomination.ScheduleID
                   join ModuleCompletionStatus in this._db.ModuleCompletionStatus on ILTSchedule.ModuleId equals ModuleCompletionStatus.ModuleId into tempModuleCompletionStatus
                   from ModuleCompletionStatus in tempModuleCompletionStatus.DefaultIfEmpty()
                   join TrainingPlace in this._db.TrainingPlace on ILTSchedule.PlaceID equals TrainingPlace.Id into tempPlace
                   from TrainingPlace in tempPlace.DefaultIfEmpty()
                   join AcademyAgency in this._db.AcademyAgencyMaster on ILTSchedule.AcademyAgencyID equals AcademyAgency.Id into tempAcademyAgency
                   from AcademyAgency in tempAcademyAgency.DefaultIfEmpty()
                   join module in this._db.Module on ModuleCompletionStatus.ModuleId equals module.Id into tempmodule
                   from module in tempmodule.DefaultIfEmpty()
                   join ModuleTopicAssociation in this._db.ModuleTopicAssociation on module.Id equals ModuleTopicAssociation.ModuleId
                   where ModuleCompletionStatus.ModuleId == ModuleID && TrainingNomination.UserID == UserId && ModuleCompletionStatus.IsDeleted == Record.NotDeleted
                   select new APIModuleCompletionDetails
                   {
                       ModuleCompletionStatus = ModuleCompletionStatus.Status,
                       ScheduleID = ILTSchedule.ID,
                       StartDate = ILTSchedule.StartDate,
                       EndDate = ILTSchedule.EndDate,
                       StartTime = ILTSchedule.StartTime.ToString("hh\\:mm"),
                       EndTime = ILTSchedule.EndTime.ToString("hh\\:mm"),
                       Venue = TrainingPlace.PostalAddress,
                       AcademyName = AcademyAgency.AcademyAgencyName,
                       ScheduleCode = ILTSchedule.ScheduleCode,
                       ModuleName = module.Name,
                   }).FirstOrDefault();

            List<APIModuleCompletionDetails> list = new List<APIModuleCompletionDetails>();
            list.Add(obj);
            if (obj != null)
            {
                foreach (APIModuleCompletionDetails details in list)
                {
                    obj.TopicList = (from TopicMaster in this._db.TopicMaster
                                     join ModuleTopicAssociation in this._db.ModuleTopicAssociation on TopicMaster.ID equals ModuleTopicAssociation.TopicId
                                     where ModuleTopicAssociation.ModuleId == ModuleID && ModuleTopicAssociation.IsActive == true && ModuleTopicAssociation.IsDeleted == Record.NotDeleted
                                     select new APIModel.TopicList
                                     {
                                         TopicId = ModuleTopicAssociation.TopicId,
                                         TopicName = TopicMaster.TopicName
                                     }).ToArray();
                }
            }
            return obj;
        }

        public async Task<APIRequestedFrom> GetCourseRequestedFrom(int CourseID, int UserId)
        {
            APIRequestedFrom requestedFrom = new APIRequestedFrom();

            CourseRequest courseRequest = new CourseRequest();
            courseRequest = await this._db.CourseRequest.Where(a => a.UserID == UserId && a.CourseID == CourseID && a.IsActive == true && a.IsDeleted == Record.NotDeleted && a.Status == "Enrolled").FirstOrDefaultAsync();


            if (courseRequest != null)
            {
                requestedFrom.RequestedFrom = "TNA";
                requestedFrom.RequestedFromLevel = 1;
            }
            else
            {
                requestedFrom.RequestedFrom = "Catalogue";
                requestedFrom.RequestedFromLevel = 4;
            }
            return requestedFrom;
        }

        public async Task<ApiResponse> GetCourseName(string type, string search = null)
        {
            ApiResponse obj = new ApiResponse();

            var Query = await (from Course in this._db.Course
                               join CourseModuleAssociation in this._db.CourseModuleAssociation on Course.Id equals CourseModuleAssociation.CourseId
                               where Course.IsActive == true && Course.IsDeleted == Record.NotDeleted &&
                               (Course.CourseType == type)
                               select new { Course.Id, Course.Title }).Distinct().ToListAsync();

            obj.ResponseObject = Query;
            if (!string.IsNullOrEmpty(search))
            {
                obj.ResponseObject = Query.Where(a => a.Title.StartsWith(search.ToLower()));
            }

            return obj;
        }

        public async Task<List<APITrainingNomination>> GetUsersForNomination(int Level, int scheduleID, int courseId, int moduleId, int UserId, string LoginId, int page, int pageSize, string search = null, string searchText = null)
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

                            cmd.CommandText = "GetAllUsersForScheduleNomination";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Level", SqlDbType.Int) { Value = Level });
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });

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

        public async Task<int> GetUsersCountForNomination(int Level, int scheduleID, int courseId, int moduleId, int UserId, string LoginId, string search = null, string searchText = null)
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

                            cmd.CommandText = "GetUsersCountForScheduleNomination";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Level", SqlDbType.Int) { Value = Level });
                            cmd.Parameters.Add(new SqlParameter("@SceduleID", SqlDbType.Int) { Value = scheduleID });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });

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


        public async Task<List<APICourseScheduleEnrollmentRequest>> GetScheduleEnrollRequest(APIBeSpokeSearch beSpokeSearch, int UserId)

        {

            var Query = (from CourseScheduleEnrollmentRequest in this._db.CourseScheduleEnrollmentRequest
                         join Course in this._db.Course on CourseScheduleEnrollmentRequest.CourseID equals Course.Id into tempCourse
                         from Course in tempCourse.DefaultIfEmpty()
                         join Module in this._db.Module on CourseScheduleEnrollmentRequest.ModuleID equals Module.Id into tempModule
                         from Module in tempModule.DefaultIfEmpty()
                         join ILTSchedule in this._db.ILTSchedule on CourseScheduleEnrollmentRequest.ScheduleID equals ILTSchedule.ID into tempschedule
                         from ILTSchedule in tempschedule.DefaultIfEmpty()
                         join bespokerequest in this._db.BespokeRequest on CourseScheduleEnrollmentRequest.BeSpokeId equals bespokerequest.Id into temprequest
                         from bespokerequest in temprequest.DefaultIfEmpty()
                         join bespokeparticipants in this._db.BespokeParticipants on CourseScheduleEnrollmentRequest.BeSpokeId equals bespokeparticipants.Id into tempparticipants
                         from bespokeparticipants in tempparticipants.DefaultIfEmpty()
                         where CourseScheduleEnrollmentRequest.IsActive == true && CourseScheduleEnrollmentRequest.IsDeleted == Record.NotDeleted
                         && CourseScheduleEnrollmentRequest.UserID == UserId
                         select new APICourseScheduleEnrollmentRequest
                         {
                             Id = CourseScheduleEnrollmentRequest.Id,
                             CourseID = CourseScheduleEnrollmentRequest.CourseID,
                             RequestedFromLevel = CourseScheduleEnrollmentRequest.RequestedFromLevel,
                             ScheduleID = CourseScheduleEnrollmentRequest.ScheduleID,
                             ScheduleCode = CourseScheduleEnrollmentRequest.BeSpokeId == null ? ILTSchedule.ScheduleCode : "--",
                             ModuleID = CourseScheduleEnrollmentRequest.ModuleID,
                             ModuleName = CourseScheduleEnrollmentRequest.ModuleID == 0 ? bespokerequest.TrainingName : Module.Name,
                             UserID = CourseScheduleEnrollmentRequest.UserID,
                             RequestStatus = CourseScheduleEnrollmentRequest.RequestStatus,
                             IsRequestSendToLevel1 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel1,
                             IsRequestSendToLevel2 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel2,
                             IsRequestSendToLevel3 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel3,
                             IsRequestSendToLevel4 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel4,
                             IsRequestSendToLevel5 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel5,
                             IsRequestSendToLevel6 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel6,
                             UserStatusInfo = ILTSchedule.ScheduleType == "Cancelled" ? ILTSchedule.ScheduleType : CourseScheduleEnrollmentRequest.UserStatusInfo,
                             RequestedFrom = CourseScheduleEnrollmentRequest.BeSpokeId == null ? CourseScheduleEnrollmentRequest.RequestedFrom : "Bespoke",
                             SentBy = CourseScheduleEnrollmentRequest.SentBy,
                             BeSpokeId = CourseScheduleEnrollmentRequest.BeSpokeId,
                             RequestedOn = CourseScheduleEnrollmentRequest.CreatedDate,

                         }).Distinct();


            if (beSpokeSearch.RequestType == "bespoke")
            {

                Query = Query.Where(a => a.BeSpokeId != null);
            }
            else
            {
                Query = Query.Where(a => a.BeSpokeId == null);
            }
            if (beSpokeSearch.Page != -1)
                Query = Query.Skip((Convert.ToInt32(beSpokeSearch.Page) - 1) * Convert.ToInt32(beSpokeSearch.PageSize));
            if (beSpokeSearch.PageSize != -1)
                Query = Query.Take(Convert.ToInt32(beSpokeSearch.PageSize));

            return Query.ToList();
        }


        public async Task<int> GetScheduleEnrollRequestCount(int UserId, APIBeSpokeSearch searchbespoke)

        {
            var Query = (from CourseScheduleEnrollmentRequest in this._db.CourseScheduleEnrollmentRequest
                             //join CourseScheduleEnrollmentRequestDetails in this._db.CourseScheduleEnrollmentRequestDetails on CourseScheduleEnrollmentRequest.Id equals CourseScheduleEnrollmentRequestDetails.CourseScheduleEnrollmentRequestID
                         join Course in this._db.Course on CourseScheduleEnrollmentRequest.CourseID equals Course.Id into tempCourse
                         from Course in tempCourse.DefaultIfEmpty()
                         join Module in this._db.Module on CourseScheduleEnrollmentRequest.ModuleID equals Module.Id into tempModule
                         from Module in tempModule.DefaultIfEmpty()
                         join ILTSchedule in this._db.ILTSchedule on CourseScheduleEnrollmentRequest.ScheduleID equals ILTSchedule.ID into tempschedule
                         from ILTSchedule in tempschedule.DefaultIfEmpty()
                         join bespokerequest in this._db.BespokeRequest on CourseScheduleEnrollmentRequest.BeSpokeId equals bespokerequest.Id into temprequest
                         from bespokerequest in temprequest.DefaultIfEmpty()
                         join bespokeparticipants in this._db.BespokeParticipants on CourseScheduleEnrollmentRequest.BeSpokeId equals bespokeparticipants.Id into tempparticipants
                         from bespokeparticipants in tempparticipants.DefaultIfEmpty()
                         where CourseScheduleEnrollmentRequest.IsActive == true && CourseScheduleEnrollmentRequest.IsDeleted == Record.NotDeleted
                          && CourseScheduleEnrollmentRequest.UserID == UserId
                         select new APICourseScheduleEnrollmentRequest
                         {
                             Id = CourseScheduleEnrollmentRequest.Id,
                             CourseID = CourseScheduleEnrollmentRequest.CourseID,
                             CourseName = Course.Title,
                             ScheduleID = CourseScheduleEnrollmentRequest.ScheduleID,
                             ScheduleCode = ILTSchedule.ScheduleCode,
                             ModuleID = CourseScheduleEnrollmentRequest.ModuleID,
                             ModuleName = Module.Name,
                             UserID = CourseScheduleEnrollmentRequest.UserID,
                             RequestStatus = CourseScheduleEnrollmentRequest.RequestStatus,
                             IsRequestSendToLevel1 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel1,
                             IsRequestSendToLevel2 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel2,
                             IsRequestSendToLevel3 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel3,
                             IsRequestSendToLevel4 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel4,
                             IsRequestSendToLevel5 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel5,
                             IsRequestSendToLevel6 = CourseScheduleEnrollmentRequest.IsRequestSendToLevel6,
                             UserStatusInfo = ILTSchedule.ScheduleType == "Cancelled" ? ILTSchedule.ScheduleType : CourseScheduleEnrollmentRequest.UserStatusInfo,
                             RequestedFrom = CourseScheduleEnrollmentRequest.BeSpokeId == null ? CourseScheduleEnrollmentRequest.RequestedFrom : "Bespoke",
                             SentBy = CourseScheduleEnrollmentRequest.SentBy,
                             BeSpokeId = CourseScheduleEnrollmentRequest.BeSpokeId,

                         });
            if (searchbespoke.RequestType == "bespoke")
            {
                Query = Query.Where(a => a.BeSpokeId != null);

            }
            else
            {
                Query = Query.Where(a => a.BeSpokeId == null);
            }

            return Query.Count();
        }

        public async Task<bool> CheckDuplicate(APICourseScheduleEnrollmentRequest obj, int UserId)
        {
            CourseScheduleEnrollmentRequest objCourseEnrollmentRequest = new CourseScheduleEnrollmentRequest();
            objCourseEnrollmentRequest = await this._db.CourseScheduleEnrollmentRequest.Where(a => a.CourseID == obj.CourseID && a.ScheduleID == obj.ScheduleID
                                                                                                 && a.ModuleID == obj.ModuleID && a.UserID == UserId
                                                                                                 && a.RequestStatus == "Requested").FirstOrDefaultAsync();
            if (objCourseEnrollmentRequest != null)
                return true;
            else
                return false;
        }

        public async Task<ApiResponse> PostRequestSchedule(APICourseScheduleEnrollmentRequest obj, int UserId, string UserName, string orgcode)
        {
            // main table
            ApiResponse objApiResponse = new ApiResponse();
            CourseScheduleEnrollmentRequest objCourseEnrollmentRequest = new CourseScheduleEnrollmentRequest();
            CourseScheduleEnrollmentRequestDetails objDetails = new CourseScheduleEnrollmentRequestDetails();

            objCourseEnrollmentRequest.Id = 0;
            objCourseEnrollmentRequest.CourseID = obj.CourseID;
            objCourseEnrollmentRequest.ScheduleID = obj.ScheduleID;
            objCourseEnrollmentRequest.ModuleID = obj.ModuleID;
            objCourseEnrollmentRequest.UserID = UserId;
            objCourseEnrollmentRequest.RequestStatus = "Requested";
            objCourseEnrollmentRequest.IsRequestSendToLevel1 = true;
            objCourseEnrollmentRequest.UserStatusInfo = "Pending LM";
            objCourseEnrollmentRequest.RequestedFrom = obj.RequestedFrom;
            objCourseEnrollmentRequest.RequestedFromLevel = obj.RequestedFromLevel;
            objCourseEnrollmentRequest.SentBy = "EU";
            objCourseEnrollmentRequest.IsActive = true;
            objCourseEnrollmentRequest.IsDeleted = false;
            objCourseEnrollmentRequest.CreatedBy = UserId;
            objCourseEnrollmentRequest.CreatedDate = DateTime.UtcNow;
            objCourseEnrollmentRequest.ModifiedBy = UserId;
            objCourseEnrollmentRequest.ModifiedDate = DateTime.UtcNow;

            this._db.Add(objCourseEnrollmentRequest);
            await this._db.SaveChangesAsync();

            int CourseScheduleEnrollmentRequestID = objCourseEnrollmentRequest.Id;

            objDetails.Id = 0;
            objDetails.CourseScheduleEnrollmentRequestID = CourseScheduleEnrollmentRequestID;
            objDetails.Status = "Requested";
            objDetails.StatusUpdatedBy = UserId;
            objDetails.Comment = obj.Comment;
            objDetails.ApprovedLevel = 0;
            objDetails.IsNominated = false;
            objDetails.IsActive = true;
            objDetails.IsDeleted = false;
            objDetails.CreatedBy = UserId;
            objDetails.CreatedDate = DateTime.UtcNow;
            objDetails.ModifiedBy = UserId;
            objDetails.ModifiedDate = DateTime.UtcNow;

            await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objDetails);
            await this._db.SaveChangesAsync();

            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";

            // ------------ schedule details ------------ //
            ILTSchedule objILTSchedule = new ILTSchedule();
            objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == obj.ScheduleID).FirstOrDefaultAsync();
            string venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
            string trainingName = this._db.Module.Where(a => a.Id == obj.ModuleID).Select(a => a.Name).FirstOrDefault();

            // ------------ Find Role to send Notification EU,LM ------------- //

            int LmUserId = 0;
            int TaUserId = 0;
            string LmEmailId = null, TaEmailId = null, EndUserEmailId = null, LmUserName = null;
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
                            cmd.CommandText = "GetUsersForNotification_HR1HR2";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = UserId });
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
                                string[] TaMailID = row["TaEmailId"].ToString().Split(",");
                                int length = TaMailID.Length;
                                for (int i = 0; i < length; i++)
                                {
                                    if (string.IsNullOrEmpty(TaEmailId))
                                    {
                                        TaEmailId = Security.Decrypt(TaMailID[i]);
                                    }
                                    else
                                    {
                                        TaEmailId = TaEmailId + ',' + Security.Decrypt(TaMailID[i]);
                                    }

                                }


                                LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());
                                EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());
                                LmUserName = string.IsNullOrEmpty(row["LmUserName"].ToString()) ? null : Security.Decrypt(row["LmUserName"].ToString());
                                LmUserId = string.IsNullOrEmpty(row["LmUserId"].ToString()) ? 0 : int.Parse(row["LmUserId"].ToString());
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

            // ------------ Find Role to send Notification ------------- //

            // notification to user while requested schedule
            string title = "Send Enrollment Request";
            string token = _identitySv.GetToken();
            int UserIDToSend = UserId;
            string Type = Record.Enrollment1;
            string Message = "Your training request for  '{scheduleCode}'  has been successfully submitted for approval.";
            Message = Message.Replace("{scheduleCode}", trainingName);
            await ScheduleRequestNotificationTo_Common(obj.CourseID, obj.ScheduleID, title, token, UserIDToSend, Message, Type);


            // notification to LM while requested schedule           
            UserIDToSend = LmUserId;
            Type = Record.Enrollment2;
            string Message_LM = "You have a pending training request for '{scheduleCode}' awaiting your approval.";
            Message_LM = Message_LM.Replace("{scheduleCode}", trainingName);
            await ScheduleRequestNotificationTo_Common(obj.CourseID, obj.ScheduleID, title, token, UserIDToSend, Message_LM, Type);

            await _email.CourseRequestMailToUser(orgcode, EndUserEmailId, null, null, UserName, trainingName);

            await _email.ScheduleEnrollmentMailToLM(orgcode, EndUserEmailId, LmEmailId, TaEmailId, LmUserName, trainingName, UserName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);


            // ------------ Find Role to send Notification Multiple TA ------------- //
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
                            cmd.CommandText = "GetUsersForNotification_HR1HR2";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 4 });


                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                            }
                            int count = 0;
                            List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                            foreach (DataRow row in dt.Rows)
                            {
                                // notification to TA while requested schedule
                                TaUserId = string.IsNullOrEmpty(row["TaUserId"].ToString()) ? 0 : int.Parse(row["TaUserId"].ToString());
                                UserIDToSend = TaUserId;
                                Type = Record.Enrollment3;
                                string Message_TA = "You have a pending training request for '{scheduleCode}' awaiting your approval.";
                                Message_TA = Message_TA.Replace("{scheduleCode}", trainingName);

                                ApiNotification Notification = new ApiNotification();
                                Notification.Title = title;
                                Notification.Message = Message_LM;
                                Notification.Url = TlsUrl.NotificationAPost + obj.CourseID + '/' + obj.ScheduleID;
                                Notification.Type = Type;
                                Notification.UserId = UserIDToSend;
                                lstApiNotification.Add(Notification);
                                count++;
                                if (count % Constants.BATCH_SIZE == 0)
                                {
                                    await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                    lstApiNotification.Clear();
                                }
 
                            }
                            if (lstApiNotification.Count > 0)
                            {
                                await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                lstApiNotification.Clear();
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
            // ------------ Find Role to send Notification Multiple TA ------------- //
            return objApiResponse;
        }

        public async Task<APIILTRequestRsponse> GetAllRequestDetails(int moduleId, int courseId, int userId)
        {
            List<APIILTRequest> ILTRequestResponseList = new List<APIILTRequest>();
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
                            cmd.CommandText = "GetAllScheduleEnrollmentRequestDetails";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = moduleId });
                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = courseId });
                            cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });

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
                                APIILTRequest ILTRequestResponse = new APIILTRequest();

                                ILTRequestResponse.ID = string.IsNullOrEmpty(row["ID"].ToString()) ? 0 : int.Parse(row["ID"].ToString());
                                ILTRequestResponse.ModuleID = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : int.Parse(row["ModuleId"].ToString());
                                ILTRequestResponse.ModuleName = row["Name"].ToString();
                                ILTRequestResponse.ModuleDescription = row["Description"].ToString();
                                ILTRequestResponse.StartDate = string.IsNullOrEmpty(row["StartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["StartDate"].ToString());
                                ILTRequestResponse.EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["EndDate"].ToString());
                                ILTRequestResponse.StartTime = string.IsNullOrEmpty(row["StartTime"].ToString()) ? null : row["StartTime"].ToString();
                                ILTRequestResponse.EndTime = string.IsNullOrEmpty(row["EndTime"].ToString()) ? null : row["EndTime"].ToString();
                                ILTRequestResponse.RegistrationEndDate = string.IsNullOrEmpty(row["RegistrationEndDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["RegistrationEndDate"].ToString());
                                ILTRequestResponse.ScheduleCode = row["ScheduleCode"].ToString();
                                ILTRequestResponse.PlaceID = string.IsNullOrEmpty(row["PlaceID"].ToString()) ? 0 : int.Parse(row["PlaceID"].ToString());
                                ILTRequestResponse.PlaceName = row["PlaceName"].ToString();
                                ILTRequestResponse.TrainerType = row["TrainerType"].ToString();
                                ILTRequestResponse.TrainingRequesStatus = row["TrainingRequestStatus"].ToString() == "Rejected" ? "" : row["TrainingRequestStatus"].ToString();
                                ILTRequestResponse.AcademyAgencyID = string.IsNullOrEmpty(row["AcademyAgencyID"].ToString()) ? 0 : int.Parse(row["AcademyAgencyID"].ToString());
                                ILTRequestResponse.AcademyAgencyName = row["AcademyAgencyName"].ToString();
                                ILTRequestResponse.AcademyTrainerID = string.IsNullOrEmpty(row["AcademyTrainerID"].ToString()) ? 0 : int.Parse(row["AcademyTrainerID"].ToString());
                                ILTRequestResponse.AcademyTrainerName = row["AcademyTrainerName"].ToString();
                                ILTRequestResponse.TrainerDescription = row["TrainerDescription"].ToString();
                                ILTRequestResponse.ScheduleType = row["ScheduleType"].ToString();
                                ILTRequestResponse.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                ILTRequestResponse.CourseName = row["Title"].ToString();
                                ILTRequestResponse.AgencyTrainerName = row["AgencyTrainerName"].ToString();
                                ILTRequestResponse.City = row["Cityname"].ToString();
                                ILTRequestResponse.SeatCapacity = row["SeatCapacity"].ToString();
                                ILTRequestResponse.ContactNumber = row["ContactNumber"].ToString();
                                ILTRequestResponse.postalAddress = row["PostalAddress"].ToString();
                                ILTRequestResponse.ContactPersonName = row["ContactPerson"].ToString();
                                ILTRequestResponse.PlaceType = row["PlaceType"].ToString();
                                ILTRequestResponse.EventLogo = row["EventLogo"].ToString();

                                ILTRequestResponse.Cost = row["Cost"].ToString();
                                ILTRequestResponse.Currency = row["Currency"].ToString();

                                ILTRequestResponseList.Add(ILTRequestResponse);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }

                APIILTRequestRsponse aPIILTRequestsList = new APIILTRequestRsponse();
                aPIILTRequestsList = ILTRequestResponseList.GroupBy(a => a.ModuleID).Select(a => a.FirstOrDefault()).Select(r => new APIILTRequestRsponse
                {
                    ModuleID = r.ModuleID,
                    ModuleName = r.ModuleName,
                    ModuleDescription = r.ModuleDescription
                }).FirstOrDefault();

                aPIILTRequestsList.APIRequestScheduleDetails =
                    ILTRequestResponseList.Where(a => a.ModuleID == aPIILTRequestsList.ModuleID && (
                    (a.TrainingRequesStatus.ToLower() == "rejected")
                    || (a.TrainingRequesStatus.ToLower() == "requested")
                    || (a.TrainingRequesStatus.ToLower() == "waiting")
                    || (a.TrainingRequesStatus == "")))
                    .Select(ILTSchedule => new APIRequestScheduleDetails
                    {
                        ID = ILTSchedule.ID,
                        StartDate = ILTSchedule.StartDate,
                        EndDate = ILTSchedule.EndDate,
                        StartTime = ILTSchedule.StartTime,
                        EndTime = ILTSchedule.EndTime,
                        RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                        ScheduleCode = ILTSchedule.ScheduleCode,
                        PlaceID = ILTSchedule.PlaceID,
                        PlaceName = ILTSchedule.PlaceName,
                        TrainerType = ILTSchedule.TrainerType,
                        TrainingRequesStatus = ILTSchedule.TrainingRequesStatus,
                        AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                        AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                        AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                        AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                        TrainerDescription = ILTSchedule.TrainerDescription,
                        ScheduleType = ILTSchedule.ScheduleType,
                        CourseID = ILTSchedule.CourseID,
                        CourseName = ILTSchedule.CourseName,
                        AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                        City = ILTSchedule.City,
                        SeatCapacity = ILTSchedule.SeatCapacity,
                        ContactNumber = ILTSchedule.ContactNumber,
                        postalAddress = ILTSchedule.postalAddress,
                        ContactPersonName = ILTSchedule.ContactPersonName,
                        PlaceType = ILTSchedule.PlaceType,
                        EventLogo = ILTSchedule.EventLogo,
                        Cost = ILTSchedule.Cost,
                        Currency = ILTSchedule.Currency,

                    }).ToList();

                aPIILTRequestsList.APIRequestScheduleDetailsForRegistered =
               ILTRequestResponseList.Where(i => i.TrainingRequesStatus.ToLower().Equals("registered")
                                            || i.TrainingRequesStatus.ToLower().Equals("availability")
                                            || i.TrainingRequesStatus.ToLower().Equals("unavailability")
                                            || i.TrainingRequesStatus.ToLower().Equals("approved"))
                                            .Select(ILTSchedule => new APIRequestScheduleDetails
                                            {
                                                ID = ILTSchedule.ID,
                                                StartDate = ILTSchedule.StartDate,
                                                EndDate = ILTSchedule.EndDate,
                                                StartTime = ILTSchedule.StartTime,
                                                EndTime = ILTSchedule.EndTime,
                                                RegistrationEndDate = ILTSchedule.RegistrationEndDate,
                                                ScheduleCode = ILTSchedule.ScheduleCode,
                                                PlaceID = ILTSchedule.PlaceID,
                                                PlaceName = ILTSchedule.PlaceName,
                                                TrainerType = ILTSchedule.TrainerType,
                                                TrainingRequesStatus = ILTSchedule.TrainingRequesStatus,
                                                AcademyAgencyID = ILTSchedule.AcademyAgencyID,
                                                AcademyAgencyName = ILTSchedule.AcademyAgencyName,
                                                AcademyTrainerID = ILTSchedule.AcademyTrainerID,
                                                AcademyTrainerName = ILTSchedule.AcademyTrainerName,
                                                TrainerDescription = ILTSchedule.TrainerDescription,
                                                ScheduleType = ILTSchedule.ScheduleType,
                                                CourseID = ILTSchedule.CourseID,
                                                CourseName = ILTSchedule.CourseName,
                                                AgencyTrainerName = ILTSchedule.AgencyTrainerName,
                                                City = ILTSchedule.City,
                                                SeatCapacity = ILTSchedule.SeatCapacity,
                                                ContactNumber = ILTSchedule.ContactNumber,
                                                postalAddress = ILTSchedule.postalAddress,
                                                ContactPersonName = ILTSchedule.ContactPersonName,
                                                PlaceType = ILTSchedule.PlaceType,
                                                EventLogo = ILTSchedule.EventLogo,


                                            }).FirstOrDefault();

                var IsNominated = aPIILTRequestsList.APIRequestScheduleDetails.Where(a => (a.TrainingRequesStatus.ToLower() == "registered") || (a.TrainingRequesStatus.ToLower() == "rejected")).FirstOrDefault();
                if (IsNominated != null)
                    aPIILTRequestsList.IsNominated = true;

                return aPIILTRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelTwo(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APICourseScheduleEnrollmentRequest> CourseEnrollmentRequestList = new List<APICourseScheduleEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllScheduleEnrollmentRequestLevelTwo";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                                APICourseScheduleEnrollmentRequest CourseEnrollmentRequest = new APICourseScheduleEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ScheduleCode = row["ScheduleCode"].ToString(),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    Status = row["RequestStatus"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Comment = row["Comment"].ToString(),
                                    CourseFee = row["CourseFee"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    StartDate = row["StartDate"].ToString(),
                                    StartTime = row["StartTime"].ToString(),
                                    EndDate = row["EndDate"].ToString(),
                                    EndTime = row["EndTime"].ToString(),
                                    Currency = row["Currency"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())

                                };

                                CourseEnrollmentRequestList.Add(CourseEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseEnrollmentRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetScheduleEnrollmentRequestLevelTwoCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetScheduleEnrollmentRequestLevelTwoCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                return Count;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelThree(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APICourseScheduleEnrollmentRequest> CourseEnrollmentRequestList = new List<APICourseScheduleEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllScheduleEnrollmentRequestLevelThree";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                                APICourseScheduleEnrollmentRequest CourseEnrollmentRequest = new APICourseScheduleEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ScheduleCode = row["ScheduleCode"].ToString(),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    Status = row["RequestStatus"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Comment = row["Comment"].ToString(),
                                    CourseFee = row["CourseFee"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    StartDate = row["StartDate"].ToString(),
                                    StartTime = row["StartTime"].ToString(),
                                    EndDate = row["EndDate"].ToString(),
                                    EndTime = row["EndTime"].ToString(),
                                    Currency = row["Currency"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                CourseEnrollmentRequestList.Add(CourseEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseEnrollmentRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetScheduleEnrollmentRequestLevelThreeCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetScheduleEnrollmentRequestLevelThreeCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                return Count;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelFour(int page, int pageSize, int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APICourseScheduleEnrollmentRequest> CourseEnrollmentRequestList = new List<APICourseScheduleEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllScheduleEnrollmentRequestLevelFour";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });

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
                                APICourseScheduleEnrollmentRequest CourseEnrollmentRequest = new APICourseScheduleEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ScheduleCode = row["ScheduleCode"].ToString(),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    Status = row["RequestStatus"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Comment = row["Comment"].ToString(),
                                    CourseFee = row["CourseFee"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    StartDate = row["StartDate"].ToString(),
                                    StartTime = row["StartTime"].ToString(),
                                    EndDate = row["EndDate"].ToString(),
                                    EndTime = row["EndTime"].ToString(),
                                    Currency = row["Currency"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                CourseEnrollmentRequestList.Add(CourseEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseEnrollmentRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetScheduleEnrollmentRequestLevelFourCount(int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetScheduleEnrollmentRequestLevelFourCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });

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
                return Count;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelFive(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APICourseScheduleEnrollmentRequest> CourseEnrollmentRequestList = new List<APICourseScheduleEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllScheduleEnrollmentRequestLevelFive";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                                APICourseScheduleEnrollmentRequest CourseEnrollmentRequest = new APICourseScheduleEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ScheduleCode = row["ScheduleCode"].ToString(),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    Status = row["RequestStatus"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Comment = row["Comment"].ToString(),
                                    CourseFee = row["CourseFee"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    StartDate = row["StartDate"].ToString(),
                                    StartTime = row["StartTime"].ToString(),
                                    EndDate = row["EndDate"].ToString(),
                                    EndTime = row["EndTime"].ToString(),
                                    Currency = row["Currency"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                CourseEnrollmentRequestList.Add(CourseEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseEnrollmentRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetScheduleEnrollmentRequestLevelFiveCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetScheduleEnrollmentRequestLevelFiveCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                return Count;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseScheduleEnrollmentRequest>> GetAllScheduleEnrollmentRequestLevelSix(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APICourseScheduleEnrollmentRequest> CourseEnrollmentRequestList = new List<APICourseScheduleEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllScheduleEnrollmentRequestLevelSix";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                                APICourseScheduleEnrollmentRequest CourseEnrollmentRequest = new APICourseScheduleEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ScheduleCode = row["ScheduleCode"].ToString(),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    Status = row["RequestStatus"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Comment = row["Comment"].ToString(),
                                    CourseFee = row["CourseFee"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    StartDate = row["StartDate"].ToString(),
                                    StartTime = row["StartTime"].ToString(),
                                    EndDate = row["EndDate"].ToString(),
                                    EndTime = row["EndTime"].ToString(),
                                    Currency = row["Currency"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                CourseEnrollmentRequestList.Add(CourseEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseEnrollmentRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetScheduleEnrollmentRequestLevelSixCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetScheduleEnrollmentRequestLevelSixCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });

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
                return Count;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<ApiResponse> ActionsByAdmins(int level, APIActionsByAdmins obj, int UserId, string UserName, string orgcode)
        {
            ApiResponse objApiResponse = new ApiResponse();
            string trainingName = null;
            string venue = null;
            if (level == 2)
            {
                CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                objRequest = await this._db.CourseScheduleEnrollmentRequest.Where(a => a.Id == obj.CourseScheduleRequestID && a.IsDeleted == Record.NotDeleted && a.IsActive == true).FirstOrDefaultAsync();
                objRequest.RequestStatus = obj.RequestStatus;
                objRequest.Id = obj.CourseScheduleRequestID;
                objRequest.ModifiedBy = UserId;
                objRequest.ModifiedDate = DateTime.UtcNow;

                ILTSchedule objILTSchedule = new ILTSchedule();
                if (objRequest.RequestedFrom != "BeSpoke")
                {
                    objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == objRequest.ScheduleID).FirstOrDefaultAsync();
                    venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
                    trainingName = this._db.Module.Where(a => a.Id == objRequest.ModuleID).Select(a => a.Name).FirstOrDefault();
                }

                if (obj.RequestStatus == "Approved")
                {
                    objRequest.IsRequestSendToLevel2 = true;
                    objRequest.UserStatusInfo = "Pending BU";

                    // ------------ Find Role to send Notification ------------- //

                    string BUEmailID = null, BUName = null, EUMailID = null, EUName = null;
                    int ReportsToID = 0;
                    string DepartmentName = string.Empty;

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
                                    cmd.CommandText = "GetUsersForNotification";
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = UserId });
                                    cmd.Parameters.Add(new SqlParameter("@userName", SqlDbType.NVarChar) { Value = UserName });
                                    cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = null });
                                    cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = null });
                                    cmd.Parameters.Add(new SqlParameter("@endUserId", SqlDbType.NVarChar) { Value = objRequest.UserID });
                                    cmd.Parameters.Add(new SqlParameter("@IsHR", SqlDbType.NVarChar) { Value = 1 });

                                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                                    DataTable dt = new DataTable();
                                    dt.Load(reader);
                                    if (dt.Rows.Count <= 0)
                                    {
                                        reader.Dispose();
                                        connection.Close();
                                    }
                                    int count = 0;
                                    List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                                    string token = _identitySv.GetToken();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        ReportsToID = string.IsNullOrEmpty(row["LMUserId"].ToString()) ? 0 : int.Parse(row["LMUserId"].ToString());
                                        BUEmailID = string.IsNullOrEmpty(row["LMEmailID"].ToString()) ? null : Security.Decrypt(row["LMEmailID"].ToString());
                                        BUName = string.IsNullOrEmpty(row["LMUserName"].ToString()) ? null : Security.Decrypt(row["LMUserName"].ToString());
                                        EUMailID = string.IsNullOrEmpty(row["EUEmailID"].ToString()) ? null : Security.Decrypt(row["EUEmailID"].ToString());
                                        EUName = string.IsNullOrEmpty(row["EUName"].ToString()) ? null : Security.Decrypt(row["EUName"].ToString());

                                        DepartmentName = string.IsNullOrEmpty(row["LmUserDepartment"].ToString()) ? null : Security.Decrypt(row["LmUserDepartment"].ToString());

                                        //----  Send Notification ----//

                                        string title = "Send Enrollment Request";

                                        int UserIDToSend = ReportsToID;
                                        string type = Record.Enrollment4;
                                        string Message = "You have a pending training request for {ScheduleCode} awaiting your approval.";
                                        if (objRequest.RequestedFrom != "BeSpoke")
                                        {
                                            type = Record.Enrollment4;
                                            Message = "You have a pending training request for {ScheduleCode} awaiting your approval.";
                                            Message = Message.Replace("{ScheduleCode}", trainingName);
                                        }
                                        else
                                        {
                                            type = Record.Bespoke4;
                                            Message = "You have a pending bespoke training request , awaiting your approval.";
                                        }

                                        ApiNotification Notification = new ApiNotification();
                                        Notification.Title = title;
                                        Notification.Message = Message;
                                        Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                                        Notification.Type = type;
                                        Notification.UserId = UserIDToSend;
                                        lstApiNotification.Add(Notification);
                                        count++;
                                        if (count % Constants.BATCH_SIZE == 0)
                                        {
                                            await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                            lstApiNotification.Clear();
                                        }

                                        await _email.TrainingRequestApprovalByLM(orgcode, BUEmailID, null, null, BUName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue, EUName);
                                    }
                                    if (lstApiNotification.Count > 0)
                                    {
                                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                        lstApiNotification.Clear();
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
                    // ------------ Find Role to send Notification ------------- //
                }
                else
                {
                    objRequest.UserStatusInfo = "Actioned LM";

                    // ------------ Find Role to send Notification ------------- //

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
                                cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = objRequest.UserID });
                                cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 3 });
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count <= 0)
                                {
                                    reader.Dispose();
                                    connection.Close();
                                }
                                int count = 0;
                                List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                                string token = _identitySv.GetToken();
                                foreach (DataRow row in dt.Rows)
                                {

                                    string TaEmailId = string.IsNullOrEmpty(row["TaEmailId"].ToString()) ? null : Security.Decrypt(row["TaEmailId"].ToString());
                                    string LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());

                                    string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                    string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());

                                    if (obj.RequestStatus != "Approved")
                                    {
                                        //----  Send Notification ----//

                                        string title = "Send Enrollment Rejection";

                                        int UserIDToSend = objRequest.UserID;
                                        string type = Record.Enrollment1;

                                        string Message = "Your training request for {ScheduleCode} has been rejected. Please visit the “My Enrollment Requests” section for the details.";

                                        if (objRequest.RequestedFrom != "BeSpoke")
                                        {
                                            type = Record.Enrollment1;
                                            Message = "Your training request for {ScheduleCode} has been rejected. Please visit the “My Enrollment Requests” section for the details.";
                                            Message = string.IsNullOrEmpty(Message.ToString()) ? null : Message.Replace("{ScheduleCode}", trainingName);
                                        }
                                        else
                                        {
                                            type = Record.Bespoke1;
                                            Message = "You have a pending bespoke training request , awaiting your approval.";
                                        }

                                        ApiNotification Notification = new ApiNotification();
                                        Notification.Title = title;
                                        Notification.Message = Message;
                                        Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                                        Notification.Type = type;
                                        Notification.UserId = UserIDToSend;
                                        lstApiNotification.Add(Notification);
                                        count++;
                                        if (count % Constants.BATCH_SIZE == 0)
                                        {
                                            await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                            lstApiNotification.Clear();
                                        }

                                        await _email.TrainingRequestRejectedToUser(orgcode, EndUserEmailId, LmEmailId, TaEmailId, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);

                                    }
                                }
                                if (lstApiNotification.Count > 0)
                                {
                                    await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                    lstApiNotification.Clear();
                                }
                                reader.Dispose();
                            }
                            connection.Close();
                        }
                    }

                    // ------------ Find Role to send Notification ------------- //
                }

                this._db.CourseScheduleEnrollmentRequest.Update(objRequest);

                objRequestDetails.Id = 0;
                objRequestDetails.CourseScheduleEnrollmentRequestID = obj.CourseScheduleRequestID;
                objRequestDetails.Comment = (obj.Comment != null) ? obj.Comment.Trim() : obj.Comment;
                objRequestDetails.Status = obj.RequestStatus;
                objRequestDetails.StatusUpdatedBy = UserId;
                objRequestDetails.ApprovedLevel = 2;
                objRequestDetails.CreatedBy = UserId;
                objRequestDetails.CreatedDate = DateTime.UtcNow;
                objRequestDetails.ModifiedBy = UserId;
                objRequestDetails.ModifiedDate = DateTime.UtcNow;
                objRequestDetails.IsActive = true;
                objRequestDetails.IsDeleted = false;

                await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                await this._db.SaveChangesAsync();
            }
            else if (level == 4)
            {
                CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                objRequest = await this._db.CourseScheduleEnrollmentRequest.Where(a => a.Id == obj.CourseScheduleRequestID && a.IsDeleted == Record.NotDeleted && a.IsActive == true).FirstOrDefaultAsync();
                objRequest.RequestStatus = obj.RequestStatus;
                objRequest.Id = obj.CourseScheduleRequestID;
                objRequest.ModifiedBy = UserId;
                objRequest.ModifiedDate = DateTime.UtcNow;
                if (obj.RequestStatus == "Approved")
                {
                    objRequest.IsRequestSendToLevel4 = true;
                    objRequest.UserStatusInfo = "Pending HR1";

                }
                else
                {
                    objRequest.UserStatusInfo = "Actioned BU";
                }

                this._db.CourseScheduleEnrollmentRequest.Update(objRequest);

                objRequestDetails.Id = 0;
                objRequestDetails.CourseScheduleEnrollmentRequestID = obj.CourseScheduleRequestID;
                objRequestDetails.Comment = (obj.Comment != null) ? obj.Comment.Trim() : obj.Comment;
                objRequestDetails.Status = obj.RequestStatus;
                objRequestDetails.StatusUpdatedBy = UserId;
                objRequestDetails.ApprovedLevel = 4;
                objRequestDetails.CreatedBy = UserId;
                objRequestDetails.CreatedDate = DateTime.UtcNow;
                objRequestDetails.ModifiedBy = UserId;
                objRequestDetails.ModifiedDate = DateTime.UtcNow;
                objRequestDetails.IsActive = true;
                objRequestDetails.IsDeleted = false;

                await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                await this._db.SaveChangesAsync();


                // ------------ Find Role to send Notification ------------- //
                ILTSchedule objILTSchedule = new ILTSchedule();
                if (objRequest.RequestedFrom != "BeSpoke")
                {
                    objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == objRequest.ScheduleID).FirstOrDefaultAsync();

                    venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
                    trainingName = this._db.Module.Where(a => a.Id == objRequest.ModuleID).Select(a => a.Name).FirstOrDefault();
                }

                try
                {
                    if (obj.RequestStatus == "Approved")
                    {
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
                                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = objRequest.UserID });
                                    cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 1 });

                                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                                    DataTable dt = new DataTable();
                                    dt.Load(reader);
                                    if (dt.Rows.Count <= 0)
                                    {
                                        reader.Dispose();
                                        connection.Close();
                                    }
                                    int count = 0;
                                    List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                                    string token = _identitySv.GetToken();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        string Hr1Name = string.IsNullOrEmpty(row["HR1Name"].ToString()) ? null : Security.Decrypt(row["HR1Name"].ToString());
                                        string Hr1EmailId = string.IsNullOrEmpty(row["Hr1EmailId"].ToString()) ? null : Security.Decrypt(row["Hr1EmailId"].ToString());
                                        int Hr1UserId = string.IsNullOrEmpty(row["Hr1UserId"].ToString()) ? 0 : int.Parse(row["Hr1UserId"].ToString());
                                        string LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());

                                        string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                        string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());

                                        if (obj.RequestStatus == "Approved")
                                        {
                                            //----  Send Notification ----//

                                            string title = "Send Enrollment Request";

                                            int UserIDToSend = Hr1UserId;
                                            string type = Record.Enrollment5;

                                            string Message = "You have a pending training request for {ScheduleCode} awaiting your approval.";
                                            if (objRequest.RequestedFrom != "BeSpoke")
                                            {
                                                type = Record.Enrollment5;
                                                Message = "You have a pending training request for {ScheduleCode} awaiting your approval.";
                                                Message = string.IsNullOrEmpty(Message.ToString()) ? null : Message.Replace("{ScheduleCode}", trainingName);
                                            }
                                            else
                                            {
                                                type = Record.Bespoke5;
                                                Message = "You have a pending bespoke training request , awaiting your approval.";
                                            }

                                            ApiNotification Notification = new ApiNotification();
                                            Notification.Title = title;
                                            Notification.Message = Message;
                                            Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                                            Notification.Type = type;
                                            Notification.UserId = UserIDToSend;

                                            lstApiNotification.Add(Notification);
                                            count++;
                                            if (count % Constants.BATCH_SIZE == 0)
                                            {
                                                await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                                lstApiNotification.Clear();
                                            }

                                            await _email.EnrollmentNotificationToHR(orgcode, Hr1EmailId, LmEmailId, null, Hr1Name, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue, EndUserName);

                                        }
                                    }
                                    if (lstApiNotification.Count > 0)
                                    {
                                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                        lstApiNotification.Clear();
                                    }
                                    reader.Dispose();
                                }
                                connection.Close();
                            }
                        }
                    }
                    else
                    {
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
                                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = objRequest.UserID });
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
                                        string TaEmailId = null;
                                        string[] TaMailID = row["TaEmailId"].ToString().Split(",");
                                        int length = TaMailID.Length;
                                        for (int i = 0; i < length; i++)
                                        {
                                            if (string.IsNullOrEmpty(TaEmailId))
                                            {
                                                TaEmailId = Security.Decrypt(TaMailID[i]);
                                            }
                                            else
                                            {
                                                TaEmailId = TaEmailId + ',' + Security.Decrypt(TaMailID[i]);
                                            }

                                        }
                                        string LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());

                                        string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                        string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());

                                        if (obj.RequestStatus != "Approved")
                                        {
                                            //----  Send Notification ----//

                                            string title = "Send Enrollment Rejection";
                                            string token = _identitySv.GetToken();
                                            int UserIDToSend = objRequest.UserID;
                                            string type = Record.Enrollment1;



                                            string Message = "Your training request for {ScheduleCode} has been rejected. Please visit the “My Enrollment Requests” section for the details.";

                                            if (objRequest.RequestedFrom != "BeSpoke")
                                            {
                                                type = Record.Enrollment1;
                                                Message = "Your training request for {ScheduleCode} has been rejected. Please visit the “My Enrollment Requests” section for the details.";
                                                Message = string.IsNullOrEmpty(Message.ToString()) ? null : Message.Replace("{ScheduleCode}", trainingName);
                                            }
                                            else
                                            {
                                                type = Record.Bespoke1;
                                                Message = "You have a pending bespoke training request , awaiting your approval.";
                                            }


                                            await ScheduleRequestNotificationTo_Common(objRequest.CourseID, objRequest.ScheduleID, title, token, UserIDToSend, Message, type);

                                            await _email.TrainingRequestRejectedToUser(orgcode, EndUserEmailId, LmEmailId, TaEmailId, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);

                                        }
                                    }
                                    reader.Dispose();
                                }
                                connection.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    throw ex;
                }
                // ------------ Find Role to send Notification ------------- //
            }
            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> ActionsByAdminsHR(int level, APIActionsByAdmins obj, int UserId, string orgcode)
        {
            ApiResponse objApiResponse = new ApiResponse();

            if (level == 5)
            {
                CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                objRequest = await this._db.CourseScheduleEnrollmentRequest.Where(a => a.Id == obj.CourseScheduleRequestID && a.IsDeleted == Record.NotDeleted && a.IsActive == true).FirstOrDefaultAsync();
                objRequest.RequestStatus = obj.RequestStatus;
                objRequest.Id = obj.CourseScheduleRequestID;
                objRequest.ModifiedBy = UserId;
                objRequest.ModifiedDate = DateTime.UtcNow;
                if (obj.RequestStatus == "Approved")
                {
                    objRequest.IsRequestSendToLevel5 = true;
                    objRequest.UserStatusInfo = "Pending HR2";

                }
                else
                {
                    objRequest.UserStatusInfo = "Actioned HR1";

                }

                this._db.CourseScheduleEnrollmentRequest.Update(objRequest);

                objRequestDetails.Id = 0;
                objRequestDetails.CourseScheduleEnrollmentRequestID = obj.CourseScheduleRequestID;
                objRequestDetails.Comment = (obj.Comment != null) ? obj.Comment.Trim() : obj.Comment;
                objRequestDetails.Status = obj.RequestStatus;
                objRequestDetails.StatusUpdatedBy = UserId;
                objRequestDetails.ApprovedLevel = 5;
                objRequestDetails.CreatedBy = UserId;
                objRequestDetails.CreatedDate = DateTime.UtcNow;
                objRequestDetails.ModifiedBy = UserId;
                objRequestDetails.ModifiedDate = DateTime.UtcNow;
                objRequestDetails.IsActive = true;
                objRequestDetails.IsDeleted = false;

                await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                await this._db.SaveChangesAsync();



                // ------------ Find Role to send Notification ------------- //
                ILTSchedule objILTSchedule = new ILTSchedule();
                string venue = null;
                string trainingName = null;
                if (objRequest.RequestedFrom != "BeSpoke")
                {
                    objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == objRequest.ScheduleID).FirstOrDefaultAsync();

                    venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
                    trainingName = this._db.Module.Where(a => a.Id == objRequest.ModuleID).Select(a => a.Name).FirstOrDefault();
                }
                else
                {

                    trainingName = this._db.BespokeRequest.Where(a => a.Id == objRequest.ModuleID).Select(a => a.TrainingName).FirstOrDefault();
                }
                int count = 0;
                List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                string token = _identitySv.GetToken();
                try
                {
                    if (obj.RequestStatus == "Approved")
                    {
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
                                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = objRequest.UserID });
                                    cmd.Parameters.Add(new SqlParameter("@IsHR1", SqlDbType.Int) { Value = 2 });


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
                                        string Hr2Name = string.IsNullOrEmpty(row["HR2Name"].ToString()) ? null : Security.Decrypt(row["HR2Name"].ToString());
                                        string Hr2EmailId = string.IsNullOrEmpty(row["Hr2EmailId"].ToString()) ? null : Security.Decrypt(row["Hr2EmailId"].ToString());
                                        int Hr2UserId = string.IsNullOrEmpty(row["Hr2UserId"].ToString()) ? 0 : int.Parse(row["Hr2UserId"].ToString());
                                        string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());


                                        //----  Send Notification ----//

                                        string title = "Send Enrollment Request";
                                        int UserIDToSend = Hr2UserId;
                                        string type = Record.Enrollment6;
                                        string Message = "You have a pending training request for {ScheduleCode} awaiting your approval.";

                                        if (objRequest.RequestedFrom != "BeSpoke")
                                        {
                                            type = Record.Enrollment6;
                                            Message = "You have a pending training request for {ScheduleCode} awaiting your approval.";
                                            Message = Message.Replace("{ScheduleCode}", trainingName);
                                        }
                                        else
                                        {
                                            type = Record.Bespoke6;
                                            Message = "You have a pending bespoke training request , awaiting your approval.";
                                        }

                                        ApiNotification Notification = new ApiNotification();
                                        Notification.Title = title;
                                        Notification.Message = Message;
                                        Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                                        Notification.Type = type;
                                        Notification.UserId = UserIDToSend;

                                        lstApiNotification.Add(Notification);
                                        count++;
                                        if (count % Constants.BATCH_SIZE == 0)
                                        {
                                            await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                            lstApiNotification.Clear();
                                        }

                                        await _email.EnrollmentNotificationToHR(orgcode, Hr2EmailId, null, null, Hr2Name, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue, EndUserName);

                                    }
                                    if (lstApiNotification.Count > 0)
                                    {
                                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                        lstApiNotification.Clear();
                                    }
                                    reader.Dispose();
                                }
                                connection.Close();
                            }
                        }
                    }
                    else
                    {
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
                                    cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = objRequest.UserID });
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
                                        string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                        string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());
                                        string LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());

                                        string TaEmailId = null;
                                        string[] TaMailID = row["TaEmailId"].ToString().Split(",");
                                        int length = TaMailID.Length;
                                        for (int i = 0; i < length; i++)
                                        {
                                            if (string.IsNullOrEmpty(TaEmailId))
                                            {
                                                TaEmailId = Security.Decrypt(TaMailID[i]);
                                            }
                                            else
                                            {
                                                TaEmailId = TaEmailId + ',' + Security.Decrypt(TaMailID[i]);
                                            }

                                        }

                                        if (obj.RequestStatus != "Approved")
                                        {

                                            //----  Send Notification ----//

                                            string title = "Send Enrollment Rejection";
                                            int UserIDToSend = objRequest.UserID;
                                            string type = Record.Enrollment1;

                                            string Message = null;
                                            if (objRequest.RequestedFrom != "BeSpoke")
                                            {
                                                Message = "Your training request for {ScheduleCode} has been rejected. Please visit the “My Enrollment Requests” section for the details.";
                                                Message = Message.Replace("{ScheduleCode}", trainingName);
                                                type = Record.Enrollment1;
                                            }
                                            else
                                            {
                                                Message = "Your bespoke request has been rejected.";
                                                type = Record.Bespoke1;
                                            }

                                            ApiNotification Notification = new ApiNotification();
                                            Notification.Title = title;
                                            Notification.Message = Message;
                                            Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                                            Notification.Type = type;
                                            Notification.UserId = UserIDToSend;
                                            lstApiNotification.Add(Notification);
                                            count++;
                                            if (count % Constants.BATCH_SIZE == 0)
                                            {
                                                await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                                lstApiNotification.Clear();
                                            }


                                            await _email.TrainingRequestRejectedToUser(orgcode, EndUserEmailId, LmEmailId, TaEmailId, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);

                                        }

                                    }
                                    if (lstApiNotification.Count > 0)
                                    {
                                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                                        lstApiNotification.Clear();
                                    }
                                    reader.Dispose();
                                }
                                connection.Close();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    throw ex;
                }
                // ------------ Find Role to send Notification ------------- //

            }
            else if (level == 6)
            {


                CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                objRequest = await this._db.CourseScheduleEnrollmentRequest.Where(a => a.Id == obj.CourseScheduleRequestID && a.IsDeleted == Record.NotDeleted && a.IsActive == true).FirstOrDefaultAsync();
                objRequest.RequestStatus = obj.RequestStatus;
                objRequest.Id = obj.CourseScheduleRequestID;
                objRequest.ModifiedBy = UserId;
                objRequest.ModifiedDate = DateTime.UtcNow;




                // ------------ Find Role to send Notification ------------- //
                string EndUserName = null, EndUserEmailId = null, TaEmailId = null;
                string LmEmailId = null;
                ILTSchedule objILTSchedule = new ILTSchedule();
                string venue = null;
                string trainingName = null;
                if (objRequest.RequestedFrom != "BeSpoke")
                {
                    trainingName = this._db.Module.Where(a => a.Id == objRequest.ModuleID).Select(a => a.Name).FirstOrDefault();
                    objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == objRequest.ScheduleID).FirstOrDefaultAsync();
                    venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
                }
                else
                {
                    trainingName = this._db.BespokeRequest.Where(a => a.Id == objRequest.ModuleID).Select(a => a.TrainingName).FirstOrDefault();
                }

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
                                cmd.CommandText = "GetUsersForNotification_HR1HR2";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = objRequest.UserID });
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

                                    string[] TaMailID = row["TaEmailId"].ToString().Split(",");
                                    int length = TaMailID.Length;
                                    for (int i = 0; i < length; i++)
                                    {
                                        if (string.IsNullOrEmpty(TaEmailId))
                                        {
                                            TaEmailId = Security.Decrypt(TaMailID[i]);
                                        }
                                        else
                                        {
                                            TaEmailId = TaEmailId + ',' + Security.Decrypt(TaMailID[i]);
                                        }

                                    }
                                    LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());
                                    EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                    EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());


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

                // ------ end notification ------//



                if (obj.RequestStatus == "Approved")
                {
                    objRequest.IsRequestSendToLevel6 = true;
                    objRequest.UserStatusInfo = "Completed";


                    //----  Send Notification ----//

                    string title = "Send Enrollment Approval";
                    string token = _identitySv.GetToken();
                    int UserIDToSend = objRequest.UserID;
                    string type = Record.Enrollment1;
                    string Message = null;
                    if (objRequest.RequestedFrom != "BeSpoke")
                    {
                        Message = "Your training request for {ScheduleCode} has been approved. You are now enrolled in this course.";
                        Message = Message.Replace("{ScheduleCode}", trainingName);
                        type = Record.Enrollment1;
                    }
                    else
                    {
                        Message = "Your bespoke request has been approved.";
                        type = Record.Bespoke1;
                    }

                    await ScheduleRequestNotificationTo_Common(objRequest.CourseID, objRequest.ScheduleID, title, token, UserIDToSend, Message, type);

                    await _email.TrainingRequestFullyApprovedToUser(orgcode, EndUserEmailId, LmEmailId, TaEmailId, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);

                }
                else
                {
                    objRequest.UserStatusInfo = "Actioned HR2";


                    string title = "Send Enrollment Rejection";
                    string token = _identitySv.GetToken();
                    int UserIDToSend = objRequest.UserID;
                    string type = Record.Enrollment1;

                    string Message = null;
                    if (objRequest.RequestedFrom != "BeSpoke")
                    {
                        Message = "Your training request for {ScheduleCode} has been rejected. Please visit the “My Enrollment Requests” section for the details.";
                        Message = Message.Replace("{ScheduleCode}", trainingName);
                        type = Record.Enrollment1;
                    }
                    else
                    {
                        Message = "Your bespoke request has been rejected.";
                        type = Record.Bespoke1;
                    }

                    await ScheduleRequestNotificationTo_Common(objRequest.CourseID, objRequest.ScheduleID, title, token, UserIDToSend, Message, type);

                    await _email.TrainingRequestRejectedToUser(orgcode, EndUserEmailId, LmEmailId, TaEmailId, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);


                }

                this._db.CourseScheduleEnrollmentRequest.Update(objRequest);

                objRequestDetails.Id = 0;
                objRequestDetails.CourseScheduleEnrollmentRequestID = obj.CourseScheduleRequestID;
                objRequestDetails.Comment = (obj.Comment != null) ? obj.Comment.Trim() : obj.Comment;
                objRequestDetails.Status = obj.RequestStatus;
                objRequestDetails.StatusUpdatedBy = UserId;
                objRequestDetails.ApprovedLevel = 6;
                objRequestDetails.CreatedBy = UserId;
                objRequestDetails.CreatedDate = DateTime.UtcNow;
                objRequestDetails.ModifiedBy = UserId;
                objRequestDetails.ModifiedDate = DateTime.UtcNow;
                objRequestDetails.IsActive = true;
                objRequestDetails.IsDeleted = false;

                await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                await this._db.SaveChangesAsync();



                // ---------- Training Nomination ------------- //
                if (obj.RequestStatus == "Approved")
                {
                    TrainingNomination objTrainingNomination = new TrainingNomination();

                    TrainingNomination lastRequestCode = await this._db.TrainingNomination.Where(a => a.IsActive == true && a.IsDeleted == false).OrderByDescending(a => a.ID).FirstOrDefaultAsync();

                    if (lastRequestCode == null)
                    {
                        objTrainingNomination.RequestCode = "RQ1";
                    }
                    else
                    {
                        objTrainingNomination.RequestCode = "RQ" + (lastRequestCode.ID + 1);
                    }

                    objTrainingNomination.UserID = objRequest.UserID;
                    objTrainingNomination.ScheduleID = objRequest.ScheduleID;
                    objTrainingNomination.ModuleID = objRequest.ModuleID;
                    objTrainingNomination.CourseID = objRequest.CourseID;
                    objTrainingNomination.CreatedBy = UserId;
                    objTrainingNomination.CreatedDate = DateTime.Now;
                    objTrainingNomination.ModifiedBy = UserId;
                    objTrainingNomination.ModifiedDate = DateTime.Now;
                    objTrainingNomination.IsDeleted = false;
                    objTrainingNomination.IsActive = true;
                    objTrainingNomination.TrainingRequestStatus = "Registered";
                    objTrainingNomination.ID = 0;
                    objTrainingNomination.IsActiveNomination = true;

                    this._db.TrainingNomination.Add(objTrainingNomination);
                    await this._db.SaveChangesAsync();
                }

                // ---------- Training Nomination ------------- //

                // ---------- AccessibilityRule --------------//

                if (obj.RequestStatus == "Approved")
                {
                    AccessibilityRule objAccessibilityRule = new AccessibilityRule();
                    objAccessibilityRule = await this._db.AccessibilityRule.Where(a => a.CourseId == objRequest.CourseID && a.UserID == objRequest.UserID && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
                    if (objAccessibilityRule == null)
                    {
                        APIAccessibility apiAccessibility = new APIAccessibility();
                        AccessibilityRules objsad = new AccessibilityRules
                        {
                            AccessibilityRule = "UserId",
                            Condition = "OR",
                            ParameterValue = objRequest.UserID.ToString(),

                        };
                        apiAccessibility.CourseId = objRequest.CourseID;
                        apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
                        apiAccessibility.AccessibilityRule[0] = objsad;
                        await _accessibilityRule.Post(apiAccessibility, UserId);
                    }
                }

                // ---------- AccessibilityRule --------------//
            }
            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> NominateUsersForAdmins(int level, NominateUsersForAdmins obj, int UserId, string orgcode)
        {
            ApiResponse objApiResponse = new ApiResponse();
            int count = 0;
            List<ApiNotification> lstApiNotification = new List<ApiNotification>();
            string token = _identitySv.GetToken();

            if (level == 2)
            {
                foreach (var m in obj.APIUserData)
                {
                    CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                    CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                    objRequest.CourseID = obj.CourseId;
                    objRequest.ScheduleID = obj.SchduleId;
                    objRequest.ModuleID = obj.ModuleId;
                    objRequest.UserID = m.userId;
                    objRequest.RequestStatus = obj.Status;
                    objRequest.IsRequestSendToLevel1 = true;
                    objRequest.IsRequestSendToLevel2 = true;
                    objRequest.UserStatusInfo = "Pending BU";
                    objRequest.RequestedFrom = obj.RequestFrom;
                    objRequest.SentBy = "Level Two";
                    objRequest.RequestedFromLevel = obj.RequestFromLevel;
                    objRequest.CreatedBy = UserId;
                    objRequest.CreatedDate = DateTime.Now;
                    objRequest.ModifiedBy = UserId;
                    objRequest.ModifiedDate = DateTime.Now;
                    objRequest.IsActive = true;
                    objRequest.IsDeleted = false;
                    objRequest.Id = 0;

                    await this.Add(objRequest);

                    int CourseScheduleEnrollmentRequestID = objRequest.Id;

                    objRequestDetails.Id = 0;
                    objRequestDetails.CourseScheduleEnrollmentRequestID = CourseScheduleEnrollmentRequestID;
                    objRequestDetails.Status = obj.Status;
                    objRequestDetails.StatusUpdatedBy = UserId;
                    objRequestDetails.ApprovedLevel = 2;
                    objRequestDetails.IsNominated = true;
                    objRequestDetails.Comment = obj.Commnet.Trim();
                    objRequestDetails.CreatedBy = UserId;
                    objRequestDetails.CreatedDate = DateTime.Now;
                    objRequestDetails.ModifiedBy = UserId;
                    objRequestDetails.ModifiedDate = DateTime.Now;
                    objRequestDetails.IsActive = true;
                    objRequestDetails.IsDeleted = false;

                    await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                    await this._db.SaveChangesAsync();

                    string trainingName = this._db.Module.Where(a => a.Id == obj.ModuleId).Select(a => a.Name).FirstOrDefault();

                    string Message = "You have been enrolled in the training course {ScheduleCode}. Please visit the “My Enrollment Requests” section for the details.";
                    Message = Message.Replace("{ScheduleCode}", trainingName);

                    ApiNotification Notification = new ApiNotification();
                    Notification.Title = "Send Nomination Alert";
                    Notification.Message = Message;
                    Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                    Notification.Type = Record.Enrollment1;
                    Notification.UserId = objRequest.UserID;
                    lstApiNotification.Add(Notification);
                    count++;
                    if (count % Constants.BATCH_SIZE == 0)
                    {
                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                        lstApiNotification.Clear();
                    }
                    await NominationMailToEmployee(objRequest.ScheduleID, objRequest.ModuleID, objRequest.CourseID, objRequest.UserID, orgcode);
                }
            }

            else if (level == 3)
            {
                foreach (var m in obj.APIUserData)
                {
                    CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                    CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                    objRequest.CourseID = obj.CourseId;
                    objRequest.ScheduleID = obj.SchduleId;
                    objRequest.ModuleID = obj.ModuleId;
                    objRequest.UserID = m.userId;
                    objRequest.RequestStatus = obj.Status;
                    objRequest.IsRequestSendToLevel1 = true;
                    objRequest.IsRequestSendToLevel3 = true;
                    objRequest.UserStatusInfo = "Pending BU";
                    objRequest.RequestedFrom = obj.RequestFrom;
                    objRequest.SentBy = "Level Three";
                    objRequest.RequestedFromLevel = obj.RequestFromLevel;
                    objRequest.CreatedBy = UserId;
                    objRequest.CreatedDate = DateTime.Now;
                    objRequest.ModifiedBy = UserId;
                    objRequest.ModifiedDate = DateTime.Now;
                    objRequest.IsActive = true;
                    objRequest.IsDeleted = false;
                    objRequest.Id = 0;

                    await this.Add(objRequest);

                    int CourseScheduleEnrollmentRequestID = objRequest.Id;

                    objRequestDetails.Id = 0;
                    objRequestDetails.CourseScheduleEnrollmentRequestID = CourseScheduleEnrollmentRequestID;
                    objRequestDetails.Status = obj.Status;
                    objRequestDetails.StatusUpdatedBy = UserId;
                    objRequestDetails.ApprovedLevel = 3;
                    objRequestDetails.IsNominated = true;
                    objRequestDetails.Comment = obj.Commnet.Trim();
                    objRequestDetails.CreatedBy = UserId;
                    objRequestDetails.CreatedDate = DateTime.Now;
                    objRequestDetails.ModifiedBy = UserId;
                    objRequestDetails.ModifiedDate = DateTime.Now;
                    objRequestDetails.IsActive = true;
                    objRequestDetails.IsDeleted = false;

                    await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                    await this._db.SaveChangesAsync();

                    string trainingName = this._db.Module.Where(a => a.Id == obj.ModuleId).Select(a => a.Name).FirstOrDefault();
                    string Message = "You have been enrolled in the training course {ScheduleCode}. Please visit the “My Enrollment Requests” section for the details.";
                    Message = Message.Replace("{ScheduleCode}", trainingName);

                    ApiNotification Notification = new ApiNotification();
                    Notification.Title = "Send Nomination Alert";
                    Notification.Message = Message;
                    Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                    Notification.Type = Record.Enrollment1;
                    Notification.UserId = objRequest.UserID;
                    lstApiNotification.Add(Notification);

                    if (count % Constants.BATCH_SIZE == 0)
                    {
                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                        lstApiNotification.Clear();
                    }
                    await NominationMailToEmployee(objRequest.ScheduleID, objRequest.ModuleID, objRequest.CourseID, objRequest.UserID, orgcode);
                }
            }

            if (lstApiNotification.Count > 0)
            {
                await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                lstApiNotification.Clear();
            }
            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> NominateUsersForAdminsBUHR(int level, NominateUsersForAdmins obj, int UserId, string orgcode)
        {
            ApiResponse objApiResponse = new ApiResponse();
            int count = 0;
            List<ApiNotification> lstApiNotification = new List<ApiNotification>();
            string token = _identitySv.GetToken();
            if (level == 4)
            {
                foreach (var m in obj.APIUserData)
                {
                    CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                    CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                    objRequest.CourseID = obj.CourseId;
                    objRequest.ScheduleID = obj.SchduleId;
                    objRequest.ModuleID = obj.ModuleId;
                    objRequest.UserID = m.userId;
                    objRequest.RequestStatus = obj.Status;
                    objRequest.IsRequestSendToLevel1 = true;
                    objRequest.IsRequestSendToLevel2 = true;
                    objRequest.IsRequestSendToLevel4 = true;
                    objRequest.UserStatusInfo = "Pending HR1";
                    objRequest.RequestedFrom = obj.RequestFrom;
                    objRequest.SentBy = "Level Four";
                    objRequest.RequestedFromLevel = obj.RequestFromLevel;
                    objRequest.CreatedBy = UserId;
                    objRequest.CreatedDate = DateTime.Now;
                    objRequest.ModifiedBy = UserId;
                    objRequest.ModifiedDate = DateTime.Now;
                    objRequest.IsActive = true;
                    objRequest.IsDeleted = false;
                    objRequest.Id = 0;

                    await this.Add(objRequest);

                    int CourseScheduleEnrollmentRequestID = objRequest.Id;

                    objRequestDetails.Id = 0;
                    objRequestDetails.CourseScheduleEnrollmentRequestID = CourseScheduleEnrollmentRequestID;
                    objRequestDetails.Status = obj.Status;
                    objRequestDetails.StatusUpdatedBy = UserId;
                    objRequestDetails.ApprovedLevel = 4;
                    objRequestDetails.IsNominated = true;
                    objRequestDetails.Comment = obj.Commnet.Trim();
                    objRequestDetails.CreatedBy = UserId;
                    objRequestDetails.CreatedDate = DateTime.Now;
                    objRequestDetails.ModifiedBy = UserId;
                    objRequestDetails.ModifiedDate = DateTime.Now;
                    objRequestDetails.IsActive = true;
                    objRequestDetails.IsDeleted = false;

                    await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                    await this._db.SaveChangesAsync();
                }
            }

            else if (level == 5)
            {
                foreach (var m in obj.APIUserData)
                {
                    CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                    CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                    objRequest.CourseID = obj.CourseId;
                    objRequest.ScheduleID = obj.SchduleId;
                    objRequest.ModuleID = obj.ModuleId;
                    objRequest.UserID = m.userId;
                    objRequest.RequestStatus = obj.Status;
                    objRequest.IsRequestSendToLevel1 = true;
                    objRequest.IsRequestSendToLevel2 = true;
                    objRequest.IsRequestSendToLevel4 = true;
                    objRequest.IsRequestSendToLevel5 = true;
                    objRequest.UserStatusInfo = "Pending HR2";
                    objRequest.RequestedFrom = obj.RequestFrom;
                    objRequest.SentBy = "Level Five";
                    objRequest.RequestedFromLevel = obj.RequestFromLevel;
                    objRequest.CreatedBy = UserId;
                    objRequest.CreatedDate = DateTime.Now;
                    objRequest.ModifiedBy = UserId;
                    objRequest.ModifiedDate = DateTime.Now;
                    objRequest.IsActive = true;
                    objRequest.IsDeleted = false;
                    objRequest.Id = 0;

                    await this.Add(objRequest);

                    int CourseScheduleEnrollmentRequestID = objRequest.Id;

                    objRequestDetails.Id = 0;
                    objRequestDetails.CourseScheduleEnrollmentRequestID = CourseScheduleEnrollmentRequestID;
                    objRequestDetails.Status = obj.Status;
                    objRequestDetails.StatusUpdatedBy = UserId;
                    objRequestDetails.ApprovedLevel = 5;
                    objRequestDetails.IsNominated = true;
                    objRequestDetails.Comment = obj.Commnet.Trim();
                    objRequestDetails.CreatedBy = UserId;
                    objRequestDetails.CreatedDate = DateTime.Now;
                    objRequestDetails.ModifiedBy = UserId;
                    objRequestDetails.ModifiedDate = DateTime.Now;
                    objRequestDetails.IsActive = true;
                    objRequestDetails.IsDeleted = false;

                    await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                    await this._db.SaveChangesAsync();

                    string trainingName = this._db.Module.Where(a => a.Id == obj.ModuleId).Select(a => a.Name).FirstOrDefault();

                    string title = "Send Nomination Alert";

                    int UserIDToSend = objRequest.UserID;
                    string type = Record.Enrollment1;
                    string Message = "You have been enrolled in the training course {ScheduleCode}. Please visit the “My Enrollment Requests” section for the details.";
                    Message = Message.Replace("{ScheduleCode}", trainingName);

                    ApiNotification Notification = new ApiNotification();
                    Notification.Title = title;
                    Notification.Message = Message;
                    Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                    Notification.Type = type;
                    Notification.UserId = UserIDToSend;

                    lstApiNotification.Add(Notification);
                    count++;
                    if (count % Constants.BATCH_SIZE == 0)
                    {
                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                        lstApiNotification.Clear();
                    }

                    await NominationMailToEmployee(objRequest.ScheduleID, objRequest.ModuleID, objRequest.CourseID, objRequest.UserID, orgcode);
                }
            }

            else if (level == 6)
            {

                foreach (var m in obj.APIUserData)
                {
                    CourseScheduleEnrollmentRequest objRequest = new CourseScheduleEnrollmentRequest();
                    CourseScheduleEnrollmentRequestDetails objRequestDetails = new CourseScheduleEnrollmentRequestDetails();

                    objRequest.CourseID = obj.CourseId;
                    objRequest.ScheduleID = obj.SchduleId;
                    objRequest.ModuleID = obj.ModuleId;
                    objRequest.UserID = m.userId;
                    objRequest.RequestStatus = obj.Status;
                    objRequest.IsRequestSendToLevel1 = true;
                    objRequest.IsRequestSendToLevel2 = true;
                    objRequest.IsRequestSendToLevel4 = true;
                    objRequest.IsRequestSendToLevel5 = true;
                    objRequest.IsRequestSendToLevel6 = true;
                    objRequest.UserStatusInfo = "Completed";
                    objRequest.RequestedFrom = obj.RequestFrom;
                    objRequest.SentBy = "Level Six";
                    objRequest.RequestedFromLevel = obj.RequestFromLevel;
                    objRequest.CreatedBy = UserId;
                    objRequest.CreatedDate = DateTime.Now;
                    objRequest.ModifiedBy = UserId;
                    objRequest.ModifiedDate = DateTime.Now;
                    objRequest.IsActive = true;
                    objRequest.IsDeleted = false;
                    objRequest.Id = 0;

                    await this.Add(objRequest);

                    int CourseScheduleEnrollmentRequestID = objRequest.Id;

                    objRequestDetails.Id = 0;
                    objRequestDetails.CourseScheduleEnrollmentRequestID = CourseScheduleEnrollmentRequestID;
                    objRequestDetails.Status = obj.Status;
                    objRequestDetails.StatusUpdatedBy = UserId;
                    objRequestDetails.ApprovedLevel = 6;
                    objRequestDetails.IsNominated = true;
                    objRequestDetails.Comment = obj.Commnet.Trim();
                    objRequestDetails.CreatedBy = UserId;
                    objRequestDetails.CreatedDate = DateTime.Now;
                    objRequestDetails.ModifiedBy = UserId;
                    objRequestDetails.ModifiedDate = DateTime.Now;
                    objRequestDetails.IsActive = true;
                    objRequestDetails.IsDeleted = false;

                    await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                    await this._db.SaveChangesAsync();

                    string trainingName = this._db.Module.Where(a => a.Id == obj.ModuleId).Select(a => a.Name).FirstOrDefault();

                    string Message = "You have been enrolled in the training course {ScheduleCode}. Please visit the “My Enrollment Requests” section for the details.";
                    Message = Message.Replace("{ScheduleCode}", trainingName);

                    ApiNotification Notification = new ApiNotification();
                    Notification.Title = "Send Nomination Alert";
                    Notification.Message = Message;
                    Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                    Notification.Type = Record.Enrollment1;
                    Notification.UserId = objRequest.UserID;
                    lstApiNotification.Add(Notification);
                    count++;
                    if (count % Constants.BATCH_SIZE == 0)
                    {
                        await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                        lstApiNotification.Clear();
                    }

                    await NominationMailToEmployee(objRequest.ScheduleID, objRequest.ModuleID, objRequest.CourseID, objRequest.UserID, orgcode);
                }
            }
            if (lstApiNotification.Count > 0)
            {
                await ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                lstApiNotification.Clear();
            }
            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<List<APIScheduleEnrollmentRequest>> GetAllDetailsForEndUser(int scheduleRequestId, int UserId)
        {
            List<APIScheduleEnrollmentRequest> scheduleEnrollmentRequestList = new List<APIScheduleEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllDetailsForEndUserByCourseScheduleEnrollmentRequest";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseScheduleEnrollmentRequestID", SqlDbType.Int) { Value = scheduleRequestId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

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
                                APIScheduleEnrollmentRequest scheduleEnrollmentRequest = new APIScheduleEnrollmentRequest();
                                scheduleEnrollmentRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                scheduleEnrollmentRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                scheduleEnrollmentRequest.CourseName = row["Title"].ToString();
                                scheduleEnrollmentRequest.StatusUpdatedBy = string.IsNullOrEmpty(row["StatusUpdatedBy"].ToString()) ? 0 : int.Parse(row["StatusUpdatedBy"].ToString());
                                scheduleEnrollmentRequest.StatusApprovedBy = row["StatusApprovedBy"].ToString();
                                scheduleEnrollmentRequest.Comment = string.IsNullOrEmpty(row["Comment"].ToString()) ? "--" : row["Comment"].ToString();
                                scheduleEnrollmentRequest.RequestedOn = DateTime.Parse(row["RequestedOn"].ToString());
                                scheduleEnrollmentRequest.Status = (row["Status"].ToString());

                                scheduleEnrollmentRequestList.Add(scheduleEnrollmentRequest);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return scheduleEnrollmentRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


        public async Task<int> ScheduleRequestNotificationTo_Common(int courseId, int ScheduleId, string title, string token, int ReportsToID, string Message, string type, int? CourseId = null)
        {
            ApiNotification Notification = new ApiNotification();
            Notification.Title = title;
            Notification.Message = Message;
            Notification.Url = TlsUrl.NotificationAPost + courseId + '/' + ScheduleId;
            Notification.Type = type;
            Notification.UserId = ReportsToID;
            await this._notification.ScheduleRequestNotificationTo_Common(Notification, token);
            return 1;

        }

        public async Task<int> ScheduleRequestNotificationTo_CommonBulk(List<ApiNotification> apiNotification, string token)
        {
            await this._notification.ScheduleRequestNotificationTo_Common(apiNotification, token);
            return 1;
        }


        public async Task<int> NominationMailToEmployee(int ScheduleID, int ModuleID, int courseId, int UserID, string orgcode)
        {
            // ------------ Find Role to send Notification ------------- //
            ILTSchedule objILTSchedule = new ILTSchedule();
            objILTSchedule = await this._db.ILTSchedule.Where(a => a.ID == ScheduleID).FirstOrDefaultAsync();

            string venue = this._db.TrainingPlace.Where(a => a.Id == objILTSchedule.PlaceID).Select(a => a.PostalAddress).FirstOrDefault();
            string trainingName = this._db.Module.Where(a => a.Id == ModuleID).Select(a => a.Name).FirstOrDefault();


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
                            cmd.CommandText = "GetUsersForNotification_HR1HR2";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = UserID });
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


                                string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());
                                string LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());
                                string BUEmailId = string.IsNullOrEmpty(row["BUEmailId"].ToString()) ? null : Security.Decrypt(row["BUEmailId"].ToString());

                                string TaEmailId = null;
                                string[] TaMailID = row["TaEmailId"].ToString().Split(",");
                                int length = TaMailID.Length;
                                for (int i = 0; i < length; i++)
                                {
                                    if (string.IsNullOrEmpty(TaEmailId))
                                    {
                                        TaEmailId = Security.Decrypt(TaMailID[i]);
                                    }
                                    else
                                    {
                                        TaEmailId = TaEmailId + ',' + Security.Decrypt(TaMailID[i]);
                                    }

                                }

                                await _email.NominationToEmpInEnrollment(orgcode, EndUserEmailId, LmEmailId, TaEmailId, BUEmailId, EndUserName, trainingName, Convert.ToString(objILTSchedule.StartTime), Convert.ToString(objILTSchedule.EndTime), Convert.ToString(objILTSchedule.StartDate), Convert.ToString(objILTSchedule.EndDate), venue);

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
            // ------------ Find Role to send Notification ------------- //
            return 1;
        }


    }
}
