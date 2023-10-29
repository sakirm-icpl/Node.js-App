using TNA.API.Helper;
using TNA.API.Model;
//using TNA.API.Models;
using TNA.API.Repositories.Interfaces;
//using TNA.API.Repositories.Interfaces.TNA;
using TNA.API.Services;
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
using System.Threading.Tasks;
using log4net;
using TNA.API.APIModel;

namespace TNA.API.Repositories
{
    public class CourseRequestRepository : Repository<CourseRequest>, ICourseRequest
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRequestRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        IAccessibilityRule _accessibilityRule;
        private readonly ITLSHelper _tlsHelper;
        private IConfiguration _configuration;
        INotification _notification;
        IIdentityService _identitySv;
        IEmail _emailRepository;
        ICourseRepository _courseRepository;
        public CourseRequestRepository(IHttpContextAccessor httpContextAccessor, ICustomerConnectionStringRepository customerConnection,
                                       ICustomerConnectionStringRepository customerConnectionStringRepository, IIdentityService identitySv,
                                       IAccessibilityRule AccessibilityRule, INotification notification, IConfiguration configuration, ITLSHelper tlsHelper, IEmail emailRepository, CourseContext context,
                                       ICourseRepository courseRepository) : base(context)
        {
            _db = context;
            this._customerConnection = customerConnection;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            _accessibilityRule = AccessibilityRule;
            this._tlsHelper = tlsHelper;
            this._configuration = configuration;
            this._notification = notification;
            this._identitySv = identitySv;
            this._emailRepository = emailRepository;
            this._courseRepository = courseRepository;
        }

        public async Task<string> GetMultipleRoleForUser(int UserId, string UserName)
        {
            try
            {
                string RoleList = string.Empty;
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetImplicitRoleList";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@decryptuserid", SqlDbType.NVarChar) { Value = UserName });

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
                                RoleList = row["RoleCodes"].ToString();
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return RoleList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseRequest>> GetAllRequestDetails(int TNAYearId, int UserId)
        {
            var tna_workflow = await _courseRepository.GetMasterConfigurableParameterValue("TNA_WORK_FLOW");
            IEnumerable<APICourseRequest> Query;
            try
            {
                if (tna_workflow == "No")
                {
                    Query = (from CourseRequest in this._db.CourseRequest
                             join Course in this._db.Course on CourseRequest.CourseID equals Course.Id into tempCourse
                             from Course in tempCourse.DefaultIfEmpty()
                             join CourseRating in this._db.CourseRating on Course.Id equals CourseRating.CourseId into tempCourseRating
                             from CourseRating in tempCourseRating.DefaultIfEmpty()
                             where CourseRequest.IsActive == true && CourseRequest.IsDeleted == Record.NotDeleted && CourseRequest.UserID == UserId && CourseRequest.TNAYear == TNAYearId
                             select new APICourseRequest
                             {
                                 Id = CourseRequest.Id,
                                 CourseID = CourseRequest.CourseID,
                                 UserID = CourseRequest.UserID,
                                 CourseName = CourseRequest.CourseID == 0 ? CourseRequest.OtherCourseName : Course.Title,
                                 Status = CourseRequest.Status,
                                 NewStatus = CourseRequest.Status == "Rejected" ? "Rejected" : CourseRequest.NewStatus,
                                 Date = CourseRequest.Date,
                                 IsRequestSendToLM = CourseRequest.IsRequestSendToLM,
                                 CourseDescription = Course.Description,
                                 Rating = CourseRating.Average,
                                 Image = Course.ThumbnailPath,
                                 IsNominate = (from coursesRequestDetails in this._db.CoursesRequestDetails
                                               where CourseRequest.Id == coursesRequestDetails.CourseRequestId
                                               orderby coursesRequestDetails.IsNominate descending
                                               select (coursesRequestDetails.IsNominate == true) ? true : false).FirstOrDefault()
                             }).AsEnumerable();
                }
                else
                {
                    Query = (from CourseRequest in this._db.CourseRequest
                             join Course in this._db.Course on CourseRequest.CourseID equals Course.Id into tempCourse
                             from Course in tempCourse.DefaultIfEmpty()
                             join CourseRating in this._db.CourseRating on Course.Id equals CourseRating.CourseId into tempCourseRating
                             from CourseRating in tempCourseRating.DefaultIfEmpty()
                             where CourseRequest.IsActive == true && CourseRequest.IsDeleted == Record.NotDeleted && CourseRequest.UserID == UserId && CourseRequest.TNAYear == TNAYearId
                             select new APICourseRequest
                             {
                                 Id = CourseRequest.Id,
                                 CourseID = CourseRequest.CourseID,
                                 UserID = CourseRequest.UserID,
                                 CourseName = CourseRequest.CourseID == 0 ? CourseRequest.OtherCourseName : Course.Title,
                                 Status = CourseRequest.Status,
                                 NewStatus = CourseRequest.Status == "Rejected" ? "Rejected" : CourseRequest.NewStatus,
                                 Date = CourseRequest.Date,
                                 IsRequestSendToLM = CourseRequest.IsRequestSendToLM,
                                 CourseDescription = Course.Description,
                                 Rating = CourseRating.Average,
                                 Image = Course.ThumbnailPath,
                                 IsNominate = (from coursesRequestDetails in this._db.CoursesRequestDetails
                                               where CourseRequest.Id == coursesRequestDetails.CourseRequestId
                                               orderby coursesRequestDetails.IsNominate descending
                                               select (coursesRequestDetails.IsNominate == true) ? true : false).FirstOrDefault()
                             })
                             .AsEnumerable()
                             .Select(x => new APICourseRequest
                             {
                                 Id = x.Id,
                                 CourseID = x.CourseID,
                                 UserID = x.UserID,
                                 CourseName = x.CourseName,
                                 Status = x.Status,
                                 NewStatus = CheckTNALevel(x.NewStatus),
                                 Date = x.Date,
                                 IsRequestSendToLM = x.IsRequestSendToLM,
                                 CourseDescription = x.CourseDescription,
                                 Rating = x.Rating,
                                 Image = x.Image,
                                 IsNominate = x.IsNominate
                             });
                }




                Query = Query.Distinct().OrderByDescending(courseRequest => courseRequest.Id);

                //return await Query.ToListAsync();

                return Query.ToList();
            }
            catch (Exception ex)
            {

                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

        }

        public static string CheckTNALevel(string newStatus)
        {
            if (newStatus == "Pending LM")
            {
                return "Pending from L1 manager";
            }
            else if (newStatus == "Pending HR")
            {
                return "Pending from L2 Manager";
            }
            else if (newStatus == "Pending BU")
            {
                return "Pending from L3 Manager";
            }
            else if (newStatus == "Completed")
            {
                return "Enrolled";
            }
            else
            { return "Rejected"; }
        }

        public async Task<int> GetAllRequestCount(int TNAYearId, int UserId)
        {
            var Query = (from CourseRequest in this._db.CourseRequest
                         join Course in this._db.Course on CourseRequest.CourseID equals Course.Id into tempCourse
                         from Course in tempCourse.DefaultIfEmpty()
                         where CourseRequest.IsActive == true && CourseRequest.IsDeleted == Record.NotDeleted && CourseRequest.UserID == UserId && CourseRequest.TNAYear == TNAYearId
                         select new APICourseRequest
                         {
                             Id = CourseRequest.Id
                         });

            return await Query.Distinct().CountAsync();
        }

        public async Task<List<APICourseRequest>> GetAllRequest(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null, string LoginId = null, string RoleName = null)
        {
            List<APICourseRequest> CourseRequestList = new List<APICourseRequest>();
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
                            cmd.CommandText = "GetAllCourseRequestForApproval";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });
                            cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar) { Value = RoleName });

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
                                APICourseRequest CourseRequest = new APICourseRequest();
                                CourseRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                CourseRequest.UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString());
                                CourseRequest.UserName = row["UserName"].ToString();
                                CourseRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                CourseRequest.CourseName = row["Title"].ToString();
                                CourseRequest.Date = DateTime.Parse(row["RequestedOn"].ToString());
                                CourseRequest.Department = row["Department"].ToString();
                                CourseRequest.Status = row["Status1"].ToString();

                                CourseRequestList.Add(CourseRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetAllCourseRequestCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null, string LoginId = null, string RoleName = null)
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
                            cmd.CommandText = "GetAllCourseRequestCountForApproval";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });
                            cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar) { Value = RoleName });

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
        public async Task<List<APICourseRequest>> GetAllRequestForTA(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APICourseRequest> CourseRequestList = new List<APICourseRequest>();
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
                            cmd.CommandText = "GetAllCourseRequestForTA";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.NVarChar) { Value = status });

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
                                APICourseRequest CourseRequest = new APICourseRequest();
                                CourseRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                CourseRequest.UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString());
                                CourseRequest.UserName = row["UserName"].ToString();
                                CourseRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                CourseRequest.CourseName = row["Title"].ToString();
                                CourseRequest.Date = DateTime.Parse(row["RequestedOn"].ToString());
                                CourseRequest.Department = row["Department"].ToString();
                                CourseRequest.Status = row["Status1"].ToString();

                                CourseRequestList.Add(CourseRequest);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return CourseRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> GetAllCourseRequestForTACount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetAllCourseRequestForTACount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.NVarChar) { Value = status });

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
        public async Task<string> GetCourseStatus(int courseID, int? UserId)
        {
            string CourseStatus;
            CourseStatus = await this._db.CourseRequest.Where(a => a.CourseID == courseID && a.UserID == UserId && a.IsActive == true && a.IsDeleted == false).Select(a => a.Status).FirstOrDefaultAsync();
            return CourseStatus;
        }

        public async Task<List<TypeAhead>> GetAllDepartmentUnderBU(string UserId)
        {
            List<TypeAhead> objList = new List<TypeAhead>();
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
                            cmd.CommandText = "GetDynamicColumnApplicableValues";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.NVarChar) { Value = UserId });

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
                                TypeAhead obj = new TypeAhead();
                                obj.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                obj.Title = row["NAME"].ToString();

                                objList.Add(obj);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return objList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseRequest>> GetAllDetailsForEndUser(int courseRequestId, int UserId)
        {
            List<APICourseRequest> CourseRequestList = new List<APICourseRequest>();
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
                            cmd.CommandText = "GetAllDetailsForEndUserBuyCourseRequestId";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@courseRequestId", SqlDbType.NVarChar) { Value = courseRequestId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });

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
                                APICourseRequest CourseRequest = new APICourseRequest();
                                CourseRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                CourseRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                CourseRequest.CourseName = row["Title"].ToString();
                                CourseRequest.StatusUpdatedBy = string.IsNullOrEmpty(row["StatusUpdatedBy"].ToString()) ? 0 : int.Parse(row["StatusUpdatedBy"].ToString());
                                CourseRequest.StatusApprovedBy = row["StatusApprovedBy"].ToString();
                                CourseRequest.RoleName = row["RoleName"].ToString();
                                CourseRequest.ReasonForRejection = row["ReasonForRejection"].ToString();
                                CourseRequest.Date = DateTime.Parse(row["RequestedOn"].ToString());
                                CourseRequest.Status = (row["Status"].ToString());

                                CourseRequestList.Add(CourseRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseRequest>> GetAllCourseDetailsByUserID(int UserID, int UserId, int Role)
        {
            List<APICourseRequest> CourseRequestList = new List<APICourseRequest>();
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
                            cmd.CommandText = "GetAllCourseDetailsByUserID";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserIdLogin", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserIDDetails", SqlDbType.NVarChar) { Value = UserID });
                            cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.NVarChar) { Value = Role });

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
                                APICourseRequest CourseRequest = new APICourseRequest();
                                CourseRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                CourseRequest.UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString());
                                CourseRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                CourseRequest.CourseName = row["Title"].ToString();
                                CourseRequest.StatusUpdatedBy = string.IsNullOrEmpty(row["StatusUpdatedBy"].ToString()) ? 0 : int.Parse(row["StatusUpdatedBy"].ToString());
                                CourseRequest.StatusApprovedBy = row["StatusApprovedBy"].ToString();
                                CourseRequest.RoleName = row["RoleName"].ToString();
                                CourseRequest.ReasonForRejection = row["ReasonForRejection"].ToString();
                                CourseRequest.Date = DateTime.Parse(row["RequestedOn"].ToString());
                                CourseRequest.Status = (row["Status"].ToString());
                                CourseRequest.IsApproved = string.IsNullOrEmpty(row["IsApproved"].ToString()) ? (int?)null : int.Parse(row["IsApproved"].ToString());
                                CourseRequest.IsRequestSend = string.IsNullOrEmpty(row["IsRequestSend"].ToString()) ? false : bool.Parse(row["IsRequestSend"].ToString());
                                int isNominate = string.IsNullOrEmpty(row["IsNominate"].ToString()) ? 0 : int.Parse(row["IsNominate"].ToString());
                                CourseRequest.IsNominate = isNominate == 0 ? false : true;

                                CourseRequestList.Add(CourseRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return CourseRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<ApiResponse> RequestForCourse(int CourseID, int UserId)
        {
            ApiResponse objApiResponse = new ApiResponse();

            int PreviousRequestCount = 0;
            PreviousRequestCount = await this._db.CourseRequest.Where(a => a.UserID == UserId).CountAsync();

            //--------- Get Configurable Count --------------//
            int ConfigurableCount = 0;
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
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "MAX_ENROLL_REQUEST" });
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
                                ConfigurableCount = string.IsNullOrEmpty(row["Value"].ToString()) ? 0 : int.Parse(row["Value"].ToString());
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
            //--------- Get Configurable Count --------------//

            if (ConfigurableCount >= (PreviousRequestCount + 1))
            {
                CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();
                CourseRequest objCourseRequest = new CourseRequest();

                objCourseRequest.UserID = UserId;
                objCourseRequest.CourseID = CourseID;
                objCourseRequest.Status = "Requested";
                objCourseRequest.Date = DateTime.UtcNow;
                objCourseRequest.IsActive = true;
                objCourseRequest.IsDeleted = false;
                objCourseRequest.CreatedBy = UserId;
                objCourseRequest.ModifiedBy = UserId;
                objCourseRequest.CreatedDate = DateTime.UtcNow;
                objCourseRequest.ModifiedDate = DateTime.UtcNow;

                await this.Add(objCourseRequest);

                int CourseRequestID = objCourseRequest.Id;

                objCoursesRequestDetails.CourseRequestId = CourseRequestID;
                objCoursesRequestDetails.Status = "Requested";
                objCoursesRequestDetails.StatusUpdatedBy = UserId;
                objCoursesRequestDetails.CourseID = CourseID;
                objCoursesRequestDetails.Date = DateTime.UtcNow;
                objCoursesRequestDetails.CreatedBy = UserId;
                objCoursesRequestDetails.CreatedDate = DateTime.UtcNow;
                objCoursesRequestDetails.ModifiedBy = UserId;
                objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;
                objCoursesRequestDetails.IsActive = true;
                objCoursesRequestDetails.IsDeleted = false;

                this._db.CoursesRequestDetails.Add(objCoursesRequestDetails);
                await this._db.SaveChangesAsync();

                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "SUCCESS";
            }
            else
            {
                objApiResponse.StatusCode = 400;
                objApiResponse.Description = "The maximum number of courses is exceeded";
            }

            return objApiResponse;
        }

        public async Task<List<APICourseRequest>> GetHistoryData(int userID)
        {
            List<APICourseRequest> CourseRequestList = new List<APICourseRequest>();
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
                            cmd.CommandText = "GetAllHistoryDetailsByCourseRequest";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@userID", SqlDbType.Int) { Value = userID });

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
                                APICourseRequest CourseRequest = new APICourseRequest();

                                CourseRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                CourseRequest.CourseRequestID = string.IsNullOrEmpty(row["CourseRequestID"].ToString()) ? 0 : int.Parse(row["CourseRequestID"].ToString());
                                CourseRequest.UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString());
                                CourseRequest.UserName = row["UserName"].ToString();
                                CourseRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                CourseRequest.CourseName = row["Title"].ToString();
                                CourseRequest.Date = DateTime.Parse(row["RequestedOn"].ToString());
                                CourseRequest.Status = row["Status"].ToString();
                                CourseRequest.StatusUpdatedBy = string.IsNullOrEmpty(row["StatusUpdatedBy"].ToString()) ? 0 : int.Parse(row["StatusUpdatedBy"].ToString());
                                CourseRequest.StatusApprovedBy = row["StatusApprovedBy"].ToString();
                                CourseRequest.RoleName = row["RoleName"].ToString();
                                CourseRequest.ReasonForRejection = row["ReasonForRejection"].ToString();
                        
                                CourseRequestList.Add(CourseRequest);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return CourseRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<FileInfo> GetDataMigrationReport(int UserId, string LoginId, int Role, string userName = null, string search = null, string searchText = null)
        {
            IEnumerable<APICourseRequest> aPICourseRequest = await this.GetDataFromHRToBUHeadExport(UserId, LoginId, Role, userName, search, searchText);
            FileInfo File = GetDataMigrationReportExcel(aPICourseRequest);
            return File;
        }

        public FileInfo GetDataMigrationReportExcel(IEnumerable<APICourseRequest> obj)
        {
            String ExcelName = "XYZ.xlsx";
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            //Adding Headers for excel file
            List<string> DataMigrationReportHeaders = GetDataMigrationReportHeaders();
            ExcelData.Add(RowNumber, DataMigrationReportHeaders);

            //Adding data row wise for excel file
            foreach (var row in obj)
            {
                List<string> DataMigrationReportRow = GetDataMigrationReportRow(row);
                RowNumber++;
                ExcelData.Add(RowNumber, DataMigrationReportRow);
            }

            FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
            return ExcelFile;
        }

        private List<string> GetDataMigrationReportRow(APICourseRequest data)
        {

            List<string> DataMigrationReportRow = new List<string>();

            DataMigrationReportRow.Add(data.UserName);
            DataMigrationReportRow.Add(data.CourseName);
            DataMigrationReportRow.Add(data.Department);
            DataMigrationReportRow.Add((data.Date).ToString());
            DataMigrationReportRow.Add(data.Status);
            DataMigrationReportRow.Add(data.StatusApprovedBy);
            DataMigrationReportRow.Add(data.ReasonForRejection);
            DataMigrationReportRow.Add((data.IsCatalogue).ToString());
             return DataMigrationReportRow;
        }

        private List<string> GetDataMigrationReportHeaders()
        {
            List<string> DataMigrationReportHeader = new List<string>();

            DataMigrationReportHeader.Add("UserName");
            DataMigrationReportHeader.Add("CourseTitle");
            DataMigrationReportHeader.Add("Department");
            DataMigrationReportHeader.Add("RequestedOn");
            DataMigrationReportHeader.Add("Status");
            DataMigrationReportHeader.Add("Actioned by");
            DataMigrationReportHeader.Add("Comment");
            DataMigrationReportHeader.Add("IsCatalogue");
            return DataMigrationReportHeader;
        }

        public async Task<List<TypeAhead>> GetSubordinateUsers(int UserId, string search)
        {
            List<TypeAhead> objList = new List<TypeAhead>();
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
                            cmd.CommandText = "[dbo].[GetSubordinateUser]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = search });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return objList;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                TypeAhead obj = new TypeAhead();

                                obj.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                obj.Title = row["UserName"].ToString();

                                objList.Add(obj);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }
                return objList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseRequest>> GetDataFromHRToBUHead(int TNAYearId, string userName = null, string search = null, string searchText = null)
        {
            List<APICourseRequest> CourseRequestList = new List<APICourseRequest>();
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
                            cmd.CommandText = "GetAllCourseRequestDataFromHRToBUHead";
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });

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
                                APICourseRequest CourseRequest = new APICourseRequest();

                                CourseRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                CourseRequest.UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString());
                                CourseRequest.UserName = row["UserName"].ToString();
                                CourseRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                CourseRequest.CourseName = row["Title"].ToString();
                                CourseRequest.Date = DateTime.Parse(row["RequestedOn"].ToString());
                                CourseRequest.Status = row["Status"].ToString();
                                CourseRequest.StatusUpdatedBy = string.IsNullOrEmpty(row["StatusUpdatedBy"].ToString()) ? 0 : int.Parse(row["StatusUpdatedBy"].ToString());
                                CourseRequest.StatusApprovedBy = row["StatusApprovedBy"].ToString();
                                CourseRequest.RoleName = row["RoleName"].ToString();
                                CourseRequest.Department = row["Department"].ToString();
                                CourseRequest.ReasonForRejection = row["ReasonForRejection"].ToString();

                                CourseRequestList.Add(CourseRequest);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }

                return CourseRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APICourseRequest>> GetDataFromHRToBUHeadExport(int UserId, string LoginId, int Role, string userName = null, string search = null, string searchText = null)
        {
            List<APICourseRequest> CourseRequestList = new List<APICourseRequest>();
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
                            cmd.CommandText = "GetAllCourseRequestDataFromHRToBUHeadExport";
                            cmd.CommandType = CommandType.StoredProcedure;

                           cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.NVarChar) { Value = Role });
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
                                APICourseRequest CourseRequest = new APICourseRequest();

                                CourseRequest.Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                                CourseRequest.UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString());
                                CourseRequest.UserName = row["UserName"].ToString();
                                CourseRequest.CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString());
                                CourseRequest.CourseName = row["Title"].ToString();
                                CourseRequest.Date = DateTime.Parse(row["RequestedOn"].ToString());
                                CourseRequest.Status = row["Status"].ToString();
                                CourseRequest.StatusUpdatedBy = string.IsNullOrEmpty(row["StatusUpdatedBy"].ToString()) ? 0 : int.Parse(row["StatusUpdatedBy"].ToString());
                                CourseRequest.StatusApprovedBy = row["StatusApprovedBy"].ToString();
                                CourseRequest.RoleName = row["RoleName"].ToString();
                                CourseRequest.Department = row["Department"].ToString();
                                CourseRequest.ReasonForRejection = row["ReasonForRejection"].ToString();
                                int IsCatalogue = string.IsNullOrEmpty(row["IsCatalogue"].ToString()) ? 0 : int.Parse(row["IsCatalogue"].ToString());
                                CourseRequest.IsCatalogue = IsCatalogue == 0 ? false : true;
                            
                                CourseRequestList.Add(CourseRequest);
                            }
                            reader.Dispose();

                        }
                        connection.Close();
                    }
                }

                return CourseRequestList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> isCourseRequestSendByUser(int UserId)
        {
            int result = 0;
            CourseRequest objCourseRequest = new CourseRequest();

            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();
             objCourseRequest = await this._db.CourseRequest.Where(a => a.UserID == UserId && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == false && a.IsRequestSendToLM == true).FirstOrDefaultAsync();

            if (objCourseRequest != null)
            {
                result = result + 1;
            }

            return result;
        }

        public async Task<int> CheckForTNAYearExpiry()
        {
            DateTime ExpiryDate = new DateTime();
            ExpiryDate= await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.ExpiryDate).FirstOrDefaultAsync();
            if (ExpiryDate.ToString("dd-MM-yyyy") != "01-01-0001")
            {
                if (ExpiryDate.Date < DateTime.UtcNow.Date)
                    return 0;
                else
                    return 1;
            }
            return 1;
        }

        public async Task<int> isCourseRequestSend(APICourseRequest obj, int UserId, string UserName)
        {
            int result = 0;
            CourseRequest objCourseRequest = new CourseRequest();


            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();

            if (obj.Role == 4)
            {
                objCourseRequest = await this._db.CourseRequest.Where(a => a.UserID == obj.UserID && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == false && a.IsRequestSendFromHRTOBU == true).FirstOrDefaultAsync();

                if (objCourseRequest != null)
                {
                    result = result + 1;
                }
            }
            else if (obj.Role == 3)
            {
                objCourseRequest = await this._db.CourseRequest.Where(a => a.UserID == obj.UserID && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == false && a.IsRequestSendToHR == true).FirstOrDefaultAsync();

                if (objCourseRequest != null)
                {
                    result = result + 1;
                }
            }
            else
            {
                objCourseRequest = await this._db.CourseRequest.Where(a => a.UserID == obj.UserID && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == false && a.IsRequestSendToHR == true).FirstOrDefaultAsync();

                if (objCourseRequest != null)
                {
                    result = result + 1;
                }
            }

            return result;
        }

        public async Task<int> CheckForSystemCourse(string CourseName)
        {
            int result = 0;
            Model.Course course = new Model.Course();

            course = await this._db.Course.Where(a => a.Title == CourseName && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
            if (course != null)
            {
                result = result + 1;
            }
            return result;
        }

        public async Task<int> DuplicateOtherCourseRequestCheck(string CourseName, int UserId)
        {
            int result = 0;
            CourseRequest objCourseRequest = new CourseRequest();

            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();

            objCourseRequest = await this._db.CourseRequest.Where(a => a.OtherCourseName == CourseName && a.TNAYear == TNAYearID && a.UserID == UserId && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();

            if (objCourseRequest != null)
            {
                result = result + 1;
            }

            return result;
        }

        public async Task<int> DuplicateCourseRequestCheck(int[] CourseID, int UserId)
        {
            int result = 0;
            CourseRequest objCourseRequest = new CourseRequest();

            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();

            foreach (var m in CourseID)
            {
                objCourseRequest = await this._db.CourseRequest.Where(a => a.CourseID == m && a.UserID == UserId && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();

                if (objCourseRequest != null)
                {
                    result = result + 1;
                }
            }

            return result;
        }

        public async Task<ApiResponse> PostRequest(int[] CourseID, int UserId, string UserName)
        {
            ApiResponse objApiResponse = new ApiResponse();

            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();

            int PreviousRequestCount = 0;
            PreviousRequestCount = await this._db.CourseRequest.Where(a => a.UserID == UserId && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == Record.NotDeleted).CountAsync();

            int Count = 0;
            Count = CourseID.Count();

            //--------- Get Configurable Count --------------//

            int ConfigurableCount = 0;
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
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "MAX_ENROLL_REQUEST" });
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
                                ConfigurableCount = string.IsNullOrEmpty(row["Value"].ToString()) ? 0 : int.Parse(row["Value"].ToString());
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
            //--------- Get Configurable Count --------------//

            if (ConfigurableCount >= (PreviousRequestCount + Count))
            {
                foreach (var m in CourseID)
                {
                    CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();
                    CourseRequest objCourseRequest = new CourseRequest();

                    objCourseRequest.UserID = UserId;
                    objCourseRequest.CourseID = m;
                    objCourseRequest.TNAYear = TNAYearID;
                    objCourseRequest.Status = "Requested";
                    objCourseRequest.Date = DateTime.UtcNow;
                    objCourseRequest.IsActive = true;
                    objCourseRequest.IsDeleted = false;
                    objCourseRequest.CreatedBy = UserId;
                    objCourseRequest.ModifiedBy = UserId;
                    objCourseRequest.CreatedDate = DateTime.UtcNow;
                    objCourseRequest.ModifiedDate = DateTime.UtcNow;

                    await this.Add(objCourseRequest);

                    int CourseRequestID = objCourseRequest.Id;

                    objCoursesRequestDetails.CourseRequestId = CourseRequestID;
                    objCoursesRequestDetails.Status = "Requested";
                    objCoursesRequestDetails.StatusUpdatedBy = UserId;
                    objCoursesRequestDetails.CourseID = m;
                    objCoursesRequestDetails.Date = DateTime.UtcNow;
                    objCoursesRequestDetails.CreatedBy = UserId;
                    objCoursesRequestDetails.CreatedDate = DateTime.UtcNow;
                    objCoursesRequestDetails.ModifiedBy = UserId;
                    objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;
                    objCoursesRequestDetails.IsActive = true;
                    objCoursesRequestDetails.IsDeleted = false;

                    await this._db.CoursesRequestDetails.AddAsync(objCoursesRequestDetails);
                    await this._db.SaveChangesAsync();
                }
                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "SUCCESS";
            }
            else
            {
                objApiResponse.StatusCode = 400;
                objApiResponse.Description = "The maximum number of courses is exceeded";
            }

            return objApiResponse;
        }
        public async Task<int> SendLmHrBuNotification(int courseId, string title, string token, int ReportsToID, string Message, string type, int? CourseId = null)
        {
            ApiNotification Notification = new ApiNotification();
            Notification.Title = title;
            Notification.Message = Message;
            if (CourseId != null)
                Notification.Url = TlsUrl.NotificationAPost + CourseId;
            Notification.Type = type;
            Notification.UserId = ReportsToID;
            await this._notification.SendLmHrBuIndivudualNotification(Notification, token);
            return 1;
        }
        public async Task<int> SendCourseAddedNotification(int courseId, string title, string token, int ReportsToID, string Message, string type, int? CourseId = null)
        {
            ApiNotification Notification = new ApiNotification();
            Notification.Title = title;
            Notification.Message = Message;
            if (CourseId != null)
            {
                Model.Course course = _db.Course.Where(a => a.Id == CourseId).FirstOrDefault();
                if (course.Code.Contains("urn") || course.Code.Contains("SkillSoft") || course.Code.Contains("ZOBBLE"))
                {
                    Notification.Url = course.CourseURL;
                }
                else
                {
                    Notification.Url = TlsUrl.NotificationAPost + CourseId;
                }
            }
            Notification.Type = type;
            Notification.UserId = ReportsToID;
            await this._notification.SendIndivudualNotification(Notification, token);
            return 1;
        }

  
        public async Task<ApiResponse> PostCourseRequest(int[] data, int UserId, string UserName, string orgCode)
        {
            ApiResponse objApiResponse = new ApiResponse();

            foreach (int m in data)
            {
                CourseRequest objCourseRequest = new CourseRequest();

                objCourseRequest = await this._db.CourseRequest.Where(a => a.Id == m && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
                if (objCourseRequest != null)
                {
                    objCourseRequest.IsRequestSendToLM = true;
                    objCourseRequest.NewStatus = "Pending LM";
                    objCourseRequest.ModifiedBy = UserId;
                    objCourseRequest.ModifiedDate = DateTime.UtcNow;

                    this._db.CourseRequest.Update(objCourseRequest);
                    await this._db.SaveChangesAsync();
                }
            }

            // ------------ Find Role to send Notification ------------- //
            string EmailID = null;
            string EndUserEmailID = null;
            string EndUserName = null;
            int ReportsToID = 0;
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
                            cmd.Parameters.Add(new SqlParameter("@IsEndUser", SqlDbType.Int) { Value = 1 });
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
                                ReportsToID = string.IsNullOrEmpty(row["LMUserId"].ToString()) ? 0 : int.Parse(row["LMUserId"].ToString());
                                EmailID = string.IsNullOrEmpty(row["LMEmailID"].ToString()) ? null : Security.Decrypt(row["LMEmailID"].ToString());
                                EndUserEmailID = string.IsNullOrEmpty(row["EndUserEmail"].ToString()) ? null : Security.Decrypt(row["EndUserEmail"].ToString());
                                EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : row["EndUserName"].ToString();
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

            //----  Send Notification ----//

            string title = "Send Request";
            string token = _identitySv.GetToken();
            int UserIDToSend = ReportsToID;
            string Type = Record.TNA2;
            string Message = "The TNA request of " + UserName + " has been submitted for your approval.";
            await SendCourseAddedNotification(0, title, token, UserIDToSend, Message, Type);
            await this._emailRepository.SendTNASubmissionByEndUserEmail(EndUserName, EndUserEmailID, orgCode, EmailID);
            //----  Send Notification ----//

            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> PostCourseRequestFromHRToBU(int[] data, int UserId, string UserName)
        {
            ApiResponse objApiResponse = new ApiResponse();
            List<int> RejectedUserId = new List<int>();
            bool IsStatusApproved = false;

            // -------- Code For Actioned LM -------- //

            int UserID = 0;
            int TotalNoOfUserRequest = 0;
            int TotalNoOfRejectedUserRequest = 0;

            UserID = await this._db.CourseRequest.Where(a => a.Id == data[0] && a.IsActive == true && a.IsDeleted == Record.NotDeleted)
                                   .Select(a => a.UserID).FirstOrDefaultAsync();

            TotalNoOfUserRequest = await this._db.CourseRequest.Where(a => a.UserID == UserID && a.IsActive == true && a.IsDeleted == Record.NotDeleted).CountAsync();

            TotalNoOfRejectedUserRequest = await this._db.CourseRequest.Where(a => a.UserID == UserID && a.Status == "Rejected" && a.IsActive == true && a.IsDeleted == Record.NotDeleted).CountAsync();

            // -------- Code For Actioned LM -------- //

            foreach (int m in data)
            {
                CourseRequest objCourseRequest = new CourseRequest();

                objCourseRequest = await this._db.CourseRequest.Where(a => a.Id == m && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
                if (objCourseRequest != null)
                {
                    objCourseRequest.IsRequestSendFromHRTOBU = true;
                    if (TotalNoOfUserRequest == TotalNoOfRejectedUserRequest)
                    {
                        objCourseRequest.NewStatus = "Actioned HR";
                    }
                   objCourseRequest.ModifiedBy = UserId;
                    objCourseRequest.ModifiedDate = DateTime.UtcNow;
                    this._db.CourseRequest.Update(objCourseRequest);
                    await this._db.SaveChangesAsync();
                    if (string.Equals(objCourseRequest.Status, "rejected", StringComparison.OrdinalIgnoreCase))
                    {
                        RejectedUserId.Add(objCourseRequest.UserID);
                    }
                }
            }
            RejectedUserId = RejectedUserId.Distinct().ToList();
            foreach (int rejecteduser in RejectedUserId)
            {
                string title = "Send Request";
                string token = _identitySv.GetToken();
                int UserIDToSend = rejecteduser;
                string Type = Record.TNA1;
                string Message = "Your TNA request have been rejected, please refer to My Learning Needs for details ";
                await SendCourseAddedNotification(0, title, token, UserIDToSend, Message, Type);
            }

            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> PostCourseRequestFromLMToHR(int[] data, int UserId, string UserName, string orgCode)
        {
            ApiResponse objApiResponse = new ApiResponse();
            List<int> RejectedUserId = new List<int>();
            bool IsStatusApproved = false;
            List<Tuple<int, int, bool>> UserIds = new List<Tuple<int, int, bool>>();

            // -------- Code For Actioned LM -------- //

            int UserID = 0;
            int TotalNoOfUserRequest = 0;
            int TotalNoOfRejectedUserRequest = 0;
            string TNAYear = null;

            UserID = await this._db.CourseRequest.Where(a => a.Id == data[0] && a.IsActive == true && a.IsDeleted == Record.NotDeleted)
                                   .Select(a => a.UserID).FirstOrDefaultAsync();

            TotalNoOfUserRequest = await this._db.CourseRequest.Where(a => a.UserID == UserID && a.IsActive == true && a.IsDeleted == Record.NotDeleted).CountAsync();

            TotalNoOfRejectedUserRequest = await this._db.CourseRequest.Where(a => a.UserID == UserID && a.Status == "Rejected" && a.IsActive == true && a.IsDeleted == Record.NotDeleted).CountAsync();

            // -------- Code For Actioned LM -------- //


            foreach (int m in data)
            {
                CourseRequest objCourseRequest = new CourseRequest();

                objCourseRequest = await this._db.CourseRequest.Where(a => a.Id == m && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
                if (objCourseRequest != null)
                {
                    objCourseRequest.IsRequestSendToHR = true;
                    if (TotalNoOfUserRequest == TotalNoOfRejectedUserRequest)
                    {
                        objCourseRequest.NewStatus = "Actioned LM";
                    }
                    else
                    {
                        objCourseRequest.NewStatus = "Pending HR";
                    }
                    objCourseRequest.ModifiedBy = UserId;
                    objCourseRequest.ModifiedDate = DateTime.UtcNow;
                    this._db.CourseRequest.Update(objCourseRequest);
                    await this._db.SaveChangesAsync();

                    TNAYear = await this._db.TNAYear.Where(a => a.Id == objCourseRequest.TNAYear).Select(a => a.Year).FirstOrDefaultAsync();

                    if (string.Equals(objCourseRequest.Status, "rejected", StringComparison.OrdinalIgnoreCase))
                    {
                        string title = "Send Request";
                        string token = _identitySv.GetToken();
                        int UserIDToSend = objCourseRequest.UserID;
                        string Type = Record.TNA1;
                        string Message = "Your TNA request have been rejected, please refer to My Learning Needs for details";
                        await SendCourseAddedNotification(0, title, token, UserIDToSend, Message, Type);
                    }
                    if (string.Equals(objCourseRequest.Status, "approved", StringComparison.OrdinalIgnoreCase))
                    {
                        var obj = new Tuple<int, int, bool>(objCourseRequest.UserID, objCourseRequest.CourseID, objCourseRequest.IsRequestSendFromTA);
                        int count = UserIds.Where(u => u.Item1 == objCourseRequest.UserID).Count();
                        if (count == 0)
                            UserIds.Add(obj);
                        IsStatusApproved = true;
                    }
                }
            }
            if (IsStatusApproved)
            {
                foreach (Tuple<int, int, bool> uid in UserIds)
                {
                    // ------------ Find Role to send Notification ------------- //
                    Dictionary<string, string> Users = new Dictionary<string, string>();
                    string EmailID = null;
                    string LmEmail = null;
                    string EndUserEmailID = null;
                    string EndUserName = null;
                    int TaUserId = 0;
                    string TaEmailId = null;
                    string CC = string.Empty;
                    int ReportsToID = 0;
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
                                    cmd.Parameters.Add(new SqlParameter("@endUserId", SqlDbType.NVarChar) { Value = uid.Item1 });
                                    cmd.Parameters.Add(new SqlParameter("@IsLMTA", SqlDbType.NVarChar) { Value = 1 });
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
                                        ReportsToID = string.IsNullOrEmpty(row["LMUserId"].ToString()) ? 0 : int.Parse(row["LMUserId"].ToString());
                                        EmailID = string.IsNullOrEmpty(row["LMEmailID"].ToString()) ? null : Security.Decrypt(row["LMEmailID"].ToString());
                                        EndUserEmailID = string.IsNullOrEmpty(row["EndUserEmail"].ToString()) ? null : Security.Decrypt(row["EndUserEmail"].ToString());
                                        EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : row["EndUserName"].ToString();
                                        TaUserId = string.IsNullOrEmpty(row["TaUserId"].ToString()) ? 0 : int.Parse(row["TaUserId"].ToString());
                                        TaEmailId = string.IsNullOrEmpty(row["TaEmailId"].ToString()) ? null : Security.Decrypt(row["TaEmailId"].ToString());
                                        LmEmail = string.IsNullOrEmpty(row["EmailId"].ToString()) ? null : Security.Decrypt(row["EmailId"].ToString());
                                        //----  Send Notification ----//                                                  
                                        if (uid.Item3)
                                        {
                                            //Ta notification
                                            string title = "Send Request";
                                            string token = _identitySv.GetToken();
                                            int UserIDToSend = ReportsToID;
                                            string Type = Record.TNA4;
                                            string Message = "You have pending requests awaiting your action.";
                                            await SendLmHrBuNotification(0, title, token, UserIDToSend, Message, Type);
                                        }
                                        if (!uid.Item3)
                                        {
                                            //Lm notification
                                            string title = "Send Request";
                                            string token = _identitySv.GetToken();
                                            int UserIDToSend = ReportsToID;
                                            string Type = Record.TNA4;
                                            string Message = "You have pending requests awaiting your action.";
                                            await SendLmHrBuNotification(0, title, token, UserIDToSend, Message, Type);
                                        }
                                        if (uid.Item3)
                                        {
                                   
                                            if (!CC.Contains(LmEmail))
                                            {
                                                CC = LmEmail;
                                                if (!String.IsNullOrEmpty(CC))
                                                    CC = String.Concat(CC, ", ", TaEmailId);
                                            }
                                            if (!Users.ContainsKey(EndUserName))
                                            {
                                                Users.Add(EndUserName, EndUserEmailID);
                                            }
                                        }
                                        else
                                        {
                                           CC = LmEmail;

                                            if (!Users.ContainsKey(EndUserName))
                                            {
                                                Users.Add(EndUserName, EndUserEmailID);
                                            }
                                         }
                                        //----  Send Email----//
                                  }
                                    reader.Dispose();
                                }
                                connection.Close();
                            }
                            foreach (KeyValuePair<string, string> u in Users)
                            {
                                await _emailRepository.SendTNASubmissionByLmEmail(u.Key, u.Value, orgCode, false, TNAYear, CC);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw ex;
                    }
                }
            }
            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> PostOtherCourses(string CourseName, string CourseDescription, int UserId)
        {
            ApiResponse objApiResponse = new ApiResponse();

            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();

            int PreviousRequestCount = 0;
            PreviousRequestCount = await this._db.CourseRequest.Where(a => a.UserID == UserId && a.TNAYear == TNAYearID && a.IsDeleted == Record.NotDeleted && a.IsActive == true).CountAsync();

            //--------- Get Configurable Count --------------//
            int ConfigurableCount = 0;
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
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "MAX_ENROLL_REQUEST" });
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
                                ConfigurableCount = string.IsNullOrEmpty(row["Value"].ToString()) ? 0 : int.Parse(row["Value"].ToString());
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
            //--------- Get Configurable Count --------------//

            if (ConfigurableCount >= (PreviousRequestCount + 1))
            {
                CourseRequest objCourseRequest = new CourseRequest();
                CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();

                objCourseRequest.UserID = UserId;
                objCourseRequest.TNAYear = TNAYearID;
                objCourseRequest.OtherCourseName = CourseName;
                objCourseRequest.OtherCourseDescription = CourseDescription;
                objCourseRequest.Status = "Requested";
                objCourseRequest.Date = DateTime.UtcNow;
                objCourseRequest.IsActive = true;
                objCourseRequest.IsDeleted = false;
                objCourseRequest.CreatedBy = UserId;
                objCourseRequest.ModifiedBy = UserId;
                objCourseRequest.CreatedDate = DateTime.UtcNow;
                objCourseRequest.ModifiedDate = DateTime.UtcNow;

                await this.Add(objCourseRequest);

                int CourseRequestID = objCourseRequest.Id;

                objCoursesRequestDetails.CourseRequestId = CourseRequestID;
                objCoursesRequestDetails.Status = "Requested";
                objCoursesRequestDetails.StatusUpdatedBy = UserId;
                objCoursesRequestDetails.CourseID = 0;
                objCoursesRequestDetails.Date = DateTime.UtcNow;
                objCoursesRequestDetails.CreatedBy = UserId;
                objCoursesRequestDetails.CreatedDate = DateTime.UtcNow;
                objCoursesRequestDetails.ModifiedBy = UserId;
                objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;
                objCoursesRequestDetails.IsActive = true;
                objCoursesRequestDetails.IsDeleted = false;

                await this._db.CoursesRequestDetails.AddAsync(objCoursesRequestDetails);
                await this._db.SaveChangesAsync();

                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "SUCCESS";
            }
            else
            {
                objApiResponse.StatusCode = 400;
                objApiResponse.Description = "The maximum number of courses is exceeded";
            }
            return objApiResponse;
        }

        public async Task<ApiResponse> PostFromHRToBUHead(int[] CourseRequestId, int UserId, string UserName, string search = null, string searchText = null)
        {
            ApiResponse objApiResponse = new ApiResponse();
            List<int> UserIds = new List<int>();
            foreach (int ID in CourseRequestId)
            {
                CourseRequest obj = new CourseRequest();
                obj = await this._db.CourseRequest.Where(a => a.Id == ID).FirstOrDefaultAsync();
                obj.NewStatus = "Pending BU";
                obj.IsRequestSendToBUHead = true;
                this._db.CourseRequest.Update(obj);
                await this._db.SaveChangesAsync();
              if (string.Equals(obj.Status, "approved", StringComparison.OrdinalIgnoreCase))
                {
                    UserIds.Add(obj.UserID);
                }
            }

            // ------------ Find Role to send Notification ------------- //
            string EmailID = null;
            int ReportsToID = 0;
            string DepartmentName = string.Empty;
            UserIds = UserIds.Distinct().ToList();
            try
            {
                foreach (int endUserId in UserIds)
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
                                cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                                cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                                cmd.Parameters.Add(new SqlParameter("@endUserId", SqlDbType.NVarChar) { Value = endUserId });
                                cmd.Parameters.Add(new SqlParameter("@IsHR", SqlDbType.NVarChar) { Value = 1 });

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
                                    ReportsToID = string.IsNullOrEmpty(row["LMUserId"].ToString()) ? 0 : int.Parse(row["LMUserId"].ToString());
                                    EmailID = string.IsNullOrEmpty(row["LMEmailID"].ToString()) ? null : Security.Decrypt(row["LMEmailID"].ToString());
                                    DepartmentName = string.IsNullOrEmpty(row["LmUserDepartment"].ToString()) ? null : Security.Decrypt(row["LmUserDepartment"].ToString());
                                    //----  Send Notification ----//

                                    string title = "Send Request";
                                    string token = _identitySv.GetToken();
                                    int UserIDToSend = ReportsToID;
                                    string type = Record.TNA5;
                                    string Message = "The TNA request for " + DepartmentName + " has been submitted for your approval.";
                                    await SendLmHrBuNotification(0, title, token, UserIDToSend, Message, type);
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


            //----  Send Notification ----//

            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        public async Task<ApiResponse> Put(int id, APICoursesRequestDetails obj, int UserId, string UserName, int Role)
        {
            ApiResponse objApiResponse = new ApiResponse();

            CoursesRequestDetails PreviousCoursesRequestStatus = new CoursesRequestDetails();
            PreviousCoursesRequestStatus = await this._db.CoursesRequestDetails.Where(a => a.CourseRequestId == id && a.StatusUpdatedBy == UserId && a.Role == Role).OrderByDescending(a => a.Id).FirstOrDefaultAsync();

            if (PreviousCoursesRequestStatus != null && PreviousCoursesRequestStatus.Status != "Requested")
            {
                objApiResponse.StatusCode = 400;
                objApiResponse.Description = "Response already registered";
                return objApiResponse;
            }

           
            // ------------- Find RoleCode From UserId -----------------//

            if (Role == 4)
            {
                if (obj.CourseID == 0 && obj.Status == "Approved")
                {
                    objApiResponse.StatusCode = 400;
                    objApiResponse.Description = "You will have to create this Course in the Catalogue.";
                    return objApiResponse;
                }
            }

            CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();
            objCoursesRequestDetails.CourseRequestId = id;
            objCoursesRequestDetails.Status = obj.Status;
            objCoursesRequestDetails.Role = Role;
            objCoursesRequestDetails.StatusUpdatedBy = UserId;
            objCoursesRequestDetails.CourseID = obj.CourseID;
            objCoursesRequestDetails.Date = DateTime.UtcNow;
            objCoursesRequestDetails.ReasonForRejection = obj.ReasonForRejection;
            objCoursesRequestDetails.IsActive = true;
            objCoursesRequestDetails.IsDeleted = false;
            objCoursesRequestDetails.CreatedDate = DateTime.UtcNow;
            objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;
            objCoursesRequestDetails.CreatedBy = UserId;
            objCoursesRequestDetails.ModifiedBy = UserId;

            await this._db.CoursesRequestDetails.AddAsync(objCoursesRequestDetails);
            await this._db.SaveChangesAsync();

            CourseRequest objCourseRequest = new CourseRequest();
            objCourseRequest = await this._db.CourseRequest.Where(a => a.Id == id).FirstOrDefaultAsync();

            objCourseRequest.Status = obj.Status;
            objCourseRequest.ModifiedDate = DateTime.UtcNow;
            objCourseRequest.ModifiedBy = UserId;

            await this.Update(objCourseRequest);
            await this._db.SaveChangesAsync();

            if (obj.Status == "Enrolled")
            {
                APIAccessibility apiAccessibility = new APIAccessibility();
                AccessibilityRules objsad = new AccessibilityRules
                {
                    AccessibilityRule = "UserId",
                    Condition = "AND",
                    ParameterValue = objCourseRequest.UserID.ToString(),

                };
                apiAccessibility.CourseId = objCourseRequest.CourseID;
                apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
                apiAccessibility.AccessibilityRule[0] = objsad;
                await _accessibilityRule.Post(apiAccessibility, UserId);
            }

       
            // ------------ Find Role to send Notification ------------- //            

            objApiResponse.StatusCode = 200;
            objApiResponse.Description = "SUCCESS";
            return objApiResponse;
        }

        private async Task<int> SaveResponse(int id, APICoursesRequestDetails obj, int UserId)
        {
            CoursesRequestDetails PreviousCoursesRequestDetails = new CoursesRequestDetails();
            PreviousCoursesRequestDetails = await this._db.CoursesRequestDetails.Where(a => a.CourseRequestId == id).FirstOrDefaultAsync();

            if (PreviousCoursesRequestDetails != null)
            {
                return 0;
            }

            CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();
            objCoursesRequestDetails.CourseRequestId = id;
            objCoursesRequestDetails.Status = obj.Status;
            objCoursesRequestDetails.StatusUpdatedBy = UserId;
            objCoursesRequestDetails.Date = DateTime.UtcNow;
            objCoursesRequestDetails.ReasonForRejection = obj.ReasonForRejection;
            objCoursesRequestDetails.IsActive = true;
            objCoursesRequestDetails.IsDeleted = false;
            objCoursesRequestDetails.CreatedDate = DateTime.UtcNow;
            objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;
            objCoursesRequestDetails.CreatedBy = UserId;
            objCoursesRequestDetails.ModifiedBy = UserId;

            await this._db.CoursesRequestDetails.AddAsync(objCoursesRequestDetails);
            await this._db.SaveChangesAsync();

            CourseRequest objCourseRequest = new CourseRequest();
            objCourseRequest = await this._db.CourseRequest.Where(a => a.Id == id).FirstOrDefaultAsync();

            objCourseRequest.Status = obj.Status;
            objCourseRequest.ModifiedDate = DateTime.UtcNow;

            await this.Update(objCourseRequest);
            await this._db.SaveChangesAsync();

            if (obj.Status == "Enroll")
            {
                APIAccessibility apiAccessibility = new APIAccessibility();
                AccessibilityRules objsad = new AccessibilityRules
                {
                    AccessibilityRule = "UserId",
                    Condition = "AND",
                    ParameterValue = objCourseRequest.UserID.ToString(),

                };
                apiAccessibility.CourseId = objCourseRequest.CourseID;
                apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
                apiAccessibility.AccessibilityRule[0] = objsad;
                await _accessibilityRule.Post(apiAccessibility, UserId);
            }
            return 1;
        }

        public async Task<ApiResponse> PostForAmendment(APIAmendment obj, int UserId, string UserName, int Role)
        {
            ApiResponse objApiResponse = new ApiResponse();

         
           
            // ------------- Find RoleCode From UserId -----------------//

            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();

            if (Role == 4)
            {
                CoursesRequestDetails PreviousRequestForHR = new CoursesRequestDetails();
                PreviousRequestForHR = await this._db.CoursesRequestDetails.Where(a => a.CourseRequestId == obj.CourseRequestID).OrderByDescending(a => a.Id).FirstOrDefaultAsync();

                if (PreviousRequestForHR.Status == "Requested")
                {
                    objApiResponse.StatusCode = 400;
                    objApiResponse.Description = "You will be allowed to mark response after Line Manager has marked his response.";
                    return objApiResponse;
                }
            }

            CoursesRequestDetails objSameCourseForAmend = new CoursesRequestDetails();
            objSameCourseForAmend = await this._db.CoursesRequestDetails.Where(a => a.CourseRequestId == obj.CourseRequestID && a.CourseID == obj.CourseID && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();

            if (objSameCourseForAmend == null)
            {
                CourseRequest objSameRequestForAmend = new CourseRequest();
                objSameRequestForAmend = await this._db.CourseRequest.Where(a => a.UserID == obj.UserID && a.CourseID == obj.CourseID && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();

                if (objSameRequestForAmend == null)
                {
                    CourseRequest objCourseRequest = new CourseRequest();
                    CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();

                    objCourseRequest = await this._db.CourseRequest.Where(a => a.Id == obj.CourseRequestID && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();

                    objCourseRequest.CourseID = obj.CourseID;
                    objCourseRequest.Status = "Approved";
                    objCourseRequest.Date = DateTime.UtcNow;
                    objCourseRequest.ModifiedBy = UserId;
                    objCourseRequest.ModifiedDate = DateTime.UtcNow;

                    this._db.Update(objCourseRequest);
                    await this._db.SaveChangesAsync();

                    objCoursesRequestDetails = await this._db.CoursesRequestDetails.Where(a => a.CourseRequestId == obj.CourseRequestID && a.IsActive == true && a.IsDeleted == false).OrderByDescending(a => a.Id).FirstOrDefaultAsync();

                    objCoursesRequestDetails.CourseRequestId = obj.CourseRequestID;
                    objCoursesRequestDetails.Status = "Amended";
                    objCoursesRequestDetails.Role = Role;
                    objCoursesRequestDetails.ReasonForRejection = obj.Comment;
                    objCoursesRequestDetails.StatusUpdatedBy = UserId;
                    objCoursesRequestDetails.Id = 0;
                    objCoursesRequestDetails.Date = DateTime.UtcNow;
                    objCoursesRequestDetails.IsActive = true;
                    objCoursesRequestDetails.IsDeleted = false;
                    objCoursesRequestDetails.CreatedDate = DateTime.UtcNow;
                    objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;
                    objCoursesRequestDetails.CreatedBy = UserId;
                    objCoursesRequestDetails.ModifiedBy = UserId;

                    await this._db.CoursesRequestDetails.AddAsync(objCoursesRequestDetails);
                    await this._db.SaveChangesAsync();

                    CoursesRequestDetails objCoursesRequestDetailsAmend = new CoursesRequestDetails();
                    objCoursesRequestDetailsAmend.CourseRequestId = obj.CourseRequestID;
                    objCoursesRequestDetailsAmend.Status = "Approved";
                    objCoursesRequestDetailsAmend.Role = Role;
                    objCoursesRequestDetailsAmend.StatusUpdatedBy = UserId;
                    objCoursesRequestDetailsAmend.Date = DateTime.UtcNow;
                    objCoursesRequestDetailsAmend.CourseID = obj.CourseID;
                    objCoursesRequestDetailsAmend.IsActive = true;
                    objCoursesRequestDetailsAmend.IsDeleted = false;
                    objCoursesRequestDetailsAmend.CreatedDate = DateTime.UtcNow;
                    objCoursesRequestDetailsAmend.ModifiedDate = DateTime.UtcNow;
                    objCoursesRequestDetailsAmend.CreatedBy = UserId;
                    objCoursesRequestDetailsAmend.ModifiedBy = UserId;

                    await this._db.CoursesRequestDetails.AddAsync(objCoursesRequestDetailsAmend);
                    await this._db.SaveChangesAsync();

                    objApiResponse.StatusCode = 200;
                    objApiResponse.Description = "SUCCESS";
                    return objApiResponse;
                }
            }
            objApiResponse.StatusCode = 400;
            objApiResponse.Description = "Same course cannot be ammended";
            return objApiResponse;
        }

        public async Task<ApiResponse> PostForNominate(APICourseRequest obj, int UserId, string UserName)
        {
            ApiResponse objApiResponse = new ApiResponse();

            int TNAYearID = 0;
            TNAYearID = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).Select(a => a.Id).FirstOrDefaultAsync();

            int PreviousRequestCount = 0;
            PreviousRequestCount = await this._db.CourseRequest.Where(a => a.UserID == obj.UserID && a.TNAYear == TNAYearID && a.IsActive == true && a.IsDeleted == Record.NotDeleted).CountAsync();

            //--------- Get Configurable Count --------------//
            int ConfigurableCount = 0;
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
                            cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.NVarChar) { Value = "MAX_ENROLL_REQUEST" });
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
                                ConfigurableCount = string.IsNullOrEmpty(row["Value"].ToString()) ? 0 : int.Parse(row["Value"].ToString());
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
            //--------- Get Configurable Count --------------//

            if ((PreviousRequestCount + 1) > ConfigurableCount)
            {
                objApiResponse.StatusCode = 400;
                objApiResponse.Description = "The maximum number of courses is exceeded";
                return objApiResponse;
            }

            CourseRequest objSameRequestForAmend = new CourseRequest();

            if (obj.CourseID == 0)
                objSameRequestForAmend = await this._db.CourseRequest.Where(a => a.UserID == obj.UserID && a.TNAYear == TNAYearID && a.OtherCourseName == obj.OtherCourseName && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();
            else
                objSameRequestForAmend = await this._db.CourseRequest.Where(a => a.UserID == obj.UserID && a.TNAYear == TNAYearID && a.CourseID == obj.CourseID && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();

            if (objSameRequestForAmend == null)
            {
                CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();
                CourseRequest objCourseRequest = new CourseRequest();

                objCourseRequest.UserID = obj.UserID;
                objCourseRequest.CourseID = obj.CourseID;
                objCourseRequest.TNAYear = TNAYearID;
                objCourseRequest.Status = "Approved";
                objCourseRequest.OtherCourseName = obj.OtherCourseName;
                objCourseRequest.OtherCourseDescription = obj.OtherCourseDescription;
                if (obj.Role == 2)
                {
                    objCourseRequest.IsRequestSendToLM = true;
                    objCourseRequest.NewStatus = "Pending LM";
                }
                else if (obj.Role == 3)
                {
                    objCourseRequest.IsRequestSendFromTA = true;
                    objCourseRequest.NewStatus = "Pending TA";
                }
                else if (obj.Role == 4)
                {
                    objCourseRequest.IsRequestSendToLM = true;
                    objCourseRequest.IsRequestSendToHR = true;
                    objCourseRequest.NewStatus = "Pending HR";
                }
                objCourseRequest.Date = DateTime.UtcNow;
                objCourseRequest.CreatedBy = UserId;
                objCourseRequest.CreatedDate = DateTime.UtcNow;
                objCourseRequest.ModifiedBy = UserId;
                objCourseRequest.ModifiedDate = DateTime.UtcNow;
                objCourseRequest.IsActive = true;
                objCourseRequest.IsDeleted = false;

                await this.Add(objCourseRequest);

                int CourseRequestID = objCourseRequest.Id;

                objCoursesRequestDetails.CourseRequestId = CourseRequestID;
                objCoursesRequestDetails.Status = "Approved";
                objCoursesRequestDetails.Role = obj.Role;
                objCoursesRequestDetails.CourseID = obj.CourseID;
                objCoursesRequestDetails.StatusUpdatedBy = UserId;
                objCoursesRequestDetails.ReasonForRejection = obj.ReasonForRejection;
                objCoursesRequestDetails.IsNominate = true;
                objCoursesRequestDetails.Date = DateTime.UtcNow;
                objCoursesRequestDetails.CreatedBy = UserId;
                objCoursesRequestDetails.CreatedDate = DateTime.UtcNow;
                objCoursesRequestDetails.ModifiedBy = UserId;
                objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;
                objCoursesRequestDetails.IsActive = true;
                objCoursesRequestDetails.IsDeleted = false;

                await this._db.CoursesRequestDetails.AddAsync(objCoursesRequestDetails);
                await this._db.SaveChangesAsync();

                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "SUCCESS";
                return objApiResponse;
            }
            objApiResponse.StatusCode = 400;
            objApiResponse.Description = "Same course  cannot be nominated";
            return objApiResponse;
        }
        public async Task<ApiResponse> PostCourseRequestIds(int[] courseRequestId, int UserId, string UserName, string OrgCode, int isAccept)
        {
            ApiResponse Response = new ApiResponse();
            List<CourseRequest> UpdateCourseRequest = new List<CourseRequest>();
            List<CoursesRequestDetails> coursesRequestDetails = new List<CoursesRequestDetails>();
            List<Tuple<int, int, bool>> UserIds = new List<Tuple<int, int, bool>>();
            foreach (int uid in courseRequestId)
            {
                CourseRequest CourseRequest = await this._db.CourseRequest.Where(c => c.Id == uid && (c.Status.ToLower()== "approved") && c.IsRequestSendToBUHead == true).FirstOrDefaultAsync();
                if (CourseRequest == null)
                {
                    continue;
                }
                bool IsTaInitiated = false;
                if (CourseRequest.IsRequestSendFromTA)
                    IsTaInitiated = true;
                Tuple<int, int, bool> obj = new Tuple<int, int, bool>(CourseRequest.UserID, CourseRequest.CourseID, IsTaInitiated);
                int count = UserIds.Where(u => u.Item1 == CourseRequest.UserID).Count();
                if (count == 0)
                    UserIds.Add(obj);
                if (isAccept == 1)
                {
                    CourseRequest.Status = "Enrolled";
                }
                else if (isAccept == 0)
                {
                    CourseRequest.Status = "Rejected";
                    CourseRequest.IsDeleted = true;
                }
                 
                CourseRequest.NewStatus = "Completed";
                CourseRequest.ModifiedBy = UserId;
                CourseRequest.ModifiedDate = DateTime.UtcNow;
                this._db.CourseRequest.Update(CourseRequest);

                CoursesRequestDetails RequestDetail = new CoursesRequestDetails();
                RequestDetail.Id = 0;
                RequestDetail.IsActive = true;
                RequestDetail.ModifiedBy = UserId;
                RequestDetail.ModifiedDate = DateTime.UtcNow;
                if (isAccept == 1)
                {
                    RequestDetail.Status = "Enrolled";
                }
                else if (isAccept == 0)
                {
                    RequestDetail.Status = "Rejected";
                }
                //RequestDetail.Status = "Enrolled";
                RequestDetail.Role = 5;
                RequestDetail.CourseID = CourseRequest.CourseID;
                RequestDetail.CourseRequestId = uid;
                RequestDetail.StatusUpdatedBy = UserId;
                RequestDetail.CreatedBy = UserId;
                RequestDetail.CreatedDate = DateTime.UtcNow;
                RequestDetail.Date = DateTime.UtcNow;
                coursesRequestDetails.Add(RequestDetail);

                if (isAccept == 1)
                {
                    APIAccessibility apiAccessibility = new APIAccessibility();
                    AccessibilityRules objsad = new AccessibilityRules
                    {
                        AccessibilityRule = "UserId",
                        Condition = "OR",
                        ParameterValue = CourseRequest.UserID.ToString(),

                    };
                    apiAccessibility.CourseId = CourseRequest.CourseID;
                    apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
                    apiAccessibility.AccessibilityRule[0] = objsad;
                    await _accessibilityRule.Post(apiAccessibility, UserId);
                }
                

                //----  Send Notification ----//
            }
            UserIds = UserIds.Distinct().ToList();
            if (isAccept == 1)
            {
                foreach (Tuple<int, int, bool> ids in UserIds)
                {
                    //----  Send Notification ----//
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
                                    cmd.CommandText = "GetUsersLmHr";
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = ids.Item1 });
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
                                        string CC = string.Empty;
                                        string LmEmailId = string.IsNullOrEmpty(row["LmEmailId"].ToString()) ? null : Security.Decrypt(row["LmEmailId"].ToString());
                                        string HrEmailId = string.IsNullOrEmpty(row["HrEmailId"].ToString()) ? null : Security.Decrypt(row["HrEmailId"].ToString());
                                        string TaEmailId = string.IsNullOrEmpty(row["TaEmailId"].ToString()) ? null : Security.Decrypt(row["TaEmailId"].ToString());
                                        string EndUserEmailId = string.IsNullOrEmpty(row["EndUserEmailId"].ToString()) ? null : Security.Decrypt(row["EndUserEmailId"].ToString());
                                        string EndUserName = row["EndUserName"].ToString();

                                        int LmUserId = string.IsNullOrEmpty(row["LmUserId"].ToString()) ? 0 : int.Parse(row["LmUserId"].ToString());
                                        int HrUserId = string.IsNullOrEmpty(row["HrUserId"].ToString()) ? 0 : int.Parse(row["HrUserId"].ToString());
                                        int TaUserId = string.IsNullOrEmpty(row["TaUserId"].ToString()) ? 0 : int.Parse(row["TaUserId"].ToString());
                                        if (ids.Item1 != 0)
                                        {
                                            //send notification for to end user
                                            string title = "Request Enrolled";
                                            string token = _identitySv.GetToken();
                                            int UserIDToSend = ids.Item1;
                                            string type = Record.Course;
                                            int CourseId = ids.Item2;
                                            string Message = "The TNA request for " + EndUserName + " has been successfully reviewed and completed.";
                                            await SendLmHrBuNotification(0, title, token, UserIDToSend, Message, type, CourseId);
                                            CC = LmEmailId;
                                            if (ids.Item3)
                                                CC = string.Concat(CC, ",", TaEmailId);
                                            await _emailRepository.SendTNASubmissionByBUEmail(EndUserName, EndUserEmailId, OrgCode, CC);
                                        }
                                        if (ids.Item3 == false)
                                        {
                                            //send notification for to lm
                                            string title = "Request Enrolled";
                                            string token = _identitySv.GetToken();
                                            int UserIDToSend = LmUserId;
                                            string type = Record.TNA2;
                                            int CourseId = ids.Item2;
                                            string Message = "The TNA request for " + EndUserName + " has been successfully reviewed and completed.";
                                            await SendLmHrBuNotification(0, title, token, UserIDToSend, Message, type, CourseId);

                                        }
                                        if (ids.Item3 == true)
                                        {
                                            //send notification for to Ta
                                            string title = "Request Enrolled";
                                            string token = _identitySv.GetToken();
                                            int UserIDToSend = TaUserId;
                                            string type = Record.TNA3;
                                            int CourseId = ids.Item2;
                                            string Message = "The TNA request for " + EndUserName + " has been successfully reviewed and completed.";
                                            await SendLmHrBuNotification(0, title, token, UserIDToSend, Message, type, CourseId);
                                        }
                                        if (HrUserId != 0)
                                        {
                                            //send notification for to hr
                                            string title = "Request Enrolled";
                                            string token = _identitySv.GetToken();
                                            int UserIDToSend = HrUserId;
                                            string type = Record.TNA4;
                                            int CourseId = ids.Item2;
                                            string Message = "The TNA request for " + EndUserName + " has been successfully reviewed and completed.";
                                            await SendLmHrBuNotification(0, title, token, UserIDToSend, Message, type, CourseId);
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
                        throw ex;
                    }

                }
            }
            
            this._db.CourseRequest.UpdateRange(UpdateCourseRequest);
            await this._db.CoursesRequestDetails.AddRangeAsync(coursesRequestDetails);
            await this._db.SaveChangesAsync();

            Response.Description = "success";
            Response.StatusCode = 200;
            return Response;
        }
        public async Task<ApiResponse> EnrollAll(int UserId, string UserName)
        {
            ApiResponse Response = new ApiResponse();
            List<CourseRequest> UpdateCourseRequest = await this._db.CourseRequest.Where(c => c.Status== "approved" && c.IsRequestSendToBUHead == true).ToListAsync();
            List<CoursesRequestDetails> coursesRequestDetails = new List<CoursesRequestDetails>();
            foreach (CourseRequest request in UpdateCourseRequest)
            {
                request.Status = "Enrolled";
                CoursesRequestDetails RequestDetail = new CoursesRequestDetails();
                RequestDetail.IsActive = true;
                RequestDetail.ModifiedBy = UserId;
                RequestDetail.ModifiedDate = DateTime.UtcNow;
                RequestDetail.Status = "Enrolled";
                RequestDetail.CourseID = request.CourseID;
                RequestDetail.CourseRequestId = request.Id;
                RequestDetail.StatusUpdatedBy = UserId;
                RequestDetail.CreatedBy = UserId;
                RequestDetail.CreatedDate = DateTime.UtcNow;
                RequestDetail.Date = DateTime.UtcNow;
                coursesRequestDetails.Add(RequestDetail);
                APIAccessibility apiAccessibility = new APIAccessibility();
                AccessibilityRules objsad = new AccessibilityRules
                {
                    AccessibilityRule = "UserId",
                    Condition = "OR",
                    ParameterValue = request.UserID.ToString(),

                };
                apiAccessibility.CourseId = request.CourseID;
                apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
                apiAccessibility.AccessibilityRule[0] = objsad;
                await _accessibilityRule.Post(apiAccessibility, UserId);

                //----  Send Notification ----//

                string title = "Request Enrolled";
                string token = _identitySv.GetToken();
                int UserIDToSend = request.UserID;
                string type = Record.Course;
                int CourseID = request.CourseID;
                string Message = UserName + ", Your Training Needs request is Enrolled.";
                await SendCourseAddedNotification(0, title, token, UserIDToSend, Message, type, CourseID);

                //----  Send Notification ----//
            }
            this._db.CourseRequest.UpdateRange(UpdateCourseRequest);
            await this._db.CoursesRequestDetails.AddRangeAsync(coursesRequestDetails);
            await this._db.SaveChangesAsync();

            Response.Description = "success";
            Response.StatusCode = 200;
            return Response;
        }

        public async Task<ApiResponse> DeleteCourseRequest(int courseRequestId, int UserId)
        {
            ApiResponse obj = new ApiResponse();

            CourseRequest objCourseRequest = new CourseRequest();
            CoursesRequestDetails objCoursesRequestDetails = new CoursesRequestDetails();

            objCourseRequest = await this._db.CourseRequest.Where(a => a.Id == courseRequestId && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
            if (objCourseRequest != null)
            {
                objCourseRequest.IsDeleted = true;
                objCourseRequest.IsActive = false;
                objCourseRequest.ModifiedBy = UserId;
                objCourseRequest.ModifiedDate = DateTime.UtcNow;

                this._db.CourseRequest.Update(objCourseRequest);
                await this._db.SaveChangesAsync();
            }

            objCoursesRequestDetails = await this._db.CoursesRequestDetails.Where(a => a.CourseRequestId == courseRequestId && a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();
            if (objCoursesRequestDetails != null)
            {
                objCoursesRequestDetails.IsDeleted = true;
                objCoursesRequestDetails.IsActive = false;
                objCoursesRequestDetails.ModifiedBy = UserId;
                objCoursesRequestDetails.ModifiedDate = DateTime.UtcNow;

                this._db.CoursesRequestDetails.Update(objCoursesRequestDetails);
                await this._db.SaveChangesAsync();
            }

            obj.Description = "success";
            obj.StatusCode = 200;
            return obj;
        }
    }
}
