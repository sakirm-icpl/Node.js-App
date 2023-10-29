using TNA.API.Helper;
using TNA.API.Model;
using TNA.API.Repositories.Interfaces;
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
    // BeSpoke Enrollment Request Levels 
    // EU-1, HR1-2, LM-3, BU-4, HR2-5
    public class BespokeEnrollmentRequestRepository : Repository<CourseScheduleEnrollmentRequest>, IBespokeEnrollmentRequest
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BespokeEnrollmentRequestRepository));
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
        public BespokeEnrollmentRequestRepository(IHttpContextAccessor httpContextAccessor, ICustomerConnectionStringRepository customerConnection,
                                       ICustomerConnectionStringRepository customerConnectionStringRepository, IIdentityService identitySv,
                                      IAccessibilityRule AccessibilityRule,INotification notification, IConfiguration configuration, IEmail email, ITLSHelper tlsHelper, IEmail emailRepository, CourseContext context) : base(context)
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

        public async Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelTwo(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APIBeSpokeEnrollmentRequest> beSpokeEnrollmentRequestsList = new List<APIBeSpokeEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllBeSpokeEnrollmentRequestLevelTwo";
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
                                APIBeSpokeEnrollmentRequest beSpokeEnrollmentRequest = new APIBeSpokeEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Status = row["RequestStatus"].ToString(),
                                    Comment = row["Comment"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                beSpokeEnrollmentRequestsList.Add(beSpokeEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return beSpokeEnrollmentRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetAllBeSpokeEnrollmentRequestLevelTwoCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetBeSpokeEnrollmentRequestLevelTwoCount";
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
        public async Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelThree(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APIBeSpokeEnrollmentRequest> beSpokeEnrollmentRequestsList = new List<APIBeSpokeEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllBeSpokeEnrollmentRequestLevelThree";
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
                                APIBeSpokeEnrollmentRequest beSpokeEnrollmentRequest = new APIBeSpokeEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Status = row["RequestStatus"].ToString(),
                                    Comment = row["Comment"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                beSpokeEnrollmentRequestsList.Add(beSpokeEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return beSpokeEnrollmentRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetAllBeSpokeEnrollmentRequestLevelThreeCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetAllBeSpokeEnrollmentRequestLevelThreeCount";
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

        public async Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelFour(int page, int pageSize, int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APIBeSpokeEnrollmentRequest> beSpokeEnrollmentRequestsList = new List<APIBeSpokeEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllBeSpokeEnrollmentRequestLevelFour";
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
                                APIBeSpokeEnrollmentRequest beSpokeEnrollmentRequest = new APIBeSpokeEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    Status = row["RequestStatus"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Comment = row["Comment"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                beSpokeEnrollmentRequestsList.Add(beSpokeEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return beSpokeEnrollmentRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetAllBeSpokeEnrollmentRequestLevelFourCount(int UserId, string LoginId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetAllBeSpokeEnrollmentRequestLevelFourCount";
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

        public async Task<List<APIBeSpokeEnrollmentRequest>> GetAllBeSpokeEnrollmentRequestLevelFive(int page, int pageSize, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
        {
            List<APIBeSpokeEnrollmentRequest> beSpokeEnrollmentRequestsList = new List<APIBeSpokeEnrollmentRequest>();
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
                            cmd.CommandText = "GetAllBeSpokeEnrollmentRequestLevelFive";
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
                                APIBeSpokeEnrollmentRequest beSpokeEnrollmentRequest = new APIBeSpokeEnrollmentRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    CourseID = string.IsNullOrEmpty(row["CourseID"].ToString()) ? 0 : int.Parse(row["CourseID"].ToString()),
                                    CourseName = row["CourseName"].ToString(),
                                    ScheduleID = string.IsNullOrEmpty(row["ScheduleID"].ToString()) ? 0 : int.Parse(row["ScheduleID"].ToString()),
                                    ModuleID = string.IsNullOrEmpty(row["ModuleID"].ToString()) ? 0 : int.Parse(row["ModuleID"].ToString()),
                                    ModuleName = row["ModuleName"].ToString(),
                                    RequestStatus = row["FinalStatus"].ToString(),
                                    RequestedFrom = row["RequestedFrom"].ToString(),
                                    RequestedOn = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Status = row["RequestStatus"].ToString(),
                                    Comment = row["Comment"].ToString(),
                                    BeSpokeId = string.IsNullOrEmpty(row["BeSpokeId"].ToString()) ? 0 : int.Parse(row["BeSpokeId"].ToString())
                                };

                                beSpokeEnrollmentRequestsList.Add(beSpokeEnrollmentRequest);
                            }
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
                return beSpokeEnrollmentRequestsList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetAllBeSpokeEnrollmentRequestLevelFiveCount(int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetAllBeSpokeEnrollmentRequestLevelFiveCount";
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

        public async Task<ApiResponse> ActionsByAdminsHRLM(int level, APIActionsByAdmins obj, int UserId, string UserName)
        {
            try
            {
                ApiResponse objApiResponse = new ApiResponse();
                if (level == 2)
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
                        objRequest.IsRequestSendToLevel2 = true;
                        objRequest.UserStatusInfo = "Pending LM";
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
                    objRequestDetails.ApprovedLevel = 2;
                    objRequestDetails.CreatedBy = UserId;
                    objRequestDetails.CreatedDate = DateTime.UtcNow;
                    objRequestDetails.ModifiedBy = UserId;
                    objRequestDetails.ModifiedDate = DateTime.UtcNow;
                    objRequestDetails.IsActive = true;
                    objRequestDetails.IsDeleted = false;

                    await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                    await this._db.SaveChangesAsync();

                    int LmUserId = 0;
                    string LmEmailId = null, EndUserEmailId = null, LmUserName = null;
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
                            string title = "Send Bespoke Request";
                            string token = _identitySv.GetToken();

                            string Type = Record.Bespoke1;

                            string Message = "You have a pending bespoke training request , awaiting your approval.";
                            await ScheduleRequestNotificationTo_Common(0, 0, title, token, LmUserId, Message, Type);
                        }
                        else
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw ex;
                    }
                }
                else if (level == 3)
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
                        objRequest.IsRequestSendToLevel3 = true;
                        objRequest.UserStatusInfo = "Pending BU";
                    }
                    else
                    {
                        objRequest.UserStatusInfo = "Actioned LM";
                    }
                    this._db.CourseScheduleEnrollmentRequest.Update(objRequest);

                    objRequestDetails.Id = 0;
                    objRequestDetails.CourseScheduleEnrollmentRequestID = obj.CourseScheduleRequestID;
                    objRequestDetails.Comment = (obj.Comment != null) ? obj.Comment.Trim() : obj.Comment;
                    objRequestDetails.Status = obj.RequestStatus;
                    objRequestDetails.StatusUpdatedBy = UserId;
                    objRequestDetails.ApprovedLevel = 3;
                    objRequestDetails.CreatedBy = UserId;
                    objRequestDetails.CreatedDate = DateTime.UtcNow;
                    objRequestDetails.ModifiedBy = UserId;
                    objRequestDetails.ModifiedDate = DateTime.UtcNow;
                    objRequestDetails.IsActive = true;
                    objRequestDetails.IsDeleted = false;

                    await this._db.CourseScheduleEnrollmentRequestDetails.AddAsync(objRequestDetails);
                    await this._db.SaveChangesAsync();


                    string BUEmailID = null, BUName = null, EUMailID = null, EUName = null;
                    int ReportsToID = 0;
                    string DepartmentName = string.Empty;

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

                                            string Message = "You have a pending bespoke training request , awaiting your approval.";

                                            ApiNotification Notification = new ApiNotification();
                                            Notification.Title = "Send Enrollment Request";
                                            Notification.Message = Message;
                                            Notification.Url = TlsUrl.NotificationAPost + objRequest.CourseID + '/' + objRequest.ScheduleID;
                                            Notification.Type = Record.Bespoke4;
                                            Notification.UserId = ReportsToID;
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
                        else
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw ex;
                    }
                }
                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "SUCCESS";
                return objApiResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<ApiResponse> ActionsByAdminsBUHRFinal(int level, APIActionsByAdmins obj, int UserId)
        {
            try
            {
                ApiResponse objApiResponse = new ApiResponse();
                if (level == 4)
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
                        objRequest.UserStatusInfo = "Pending HR Final";
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

                    string trainingName = this._db.BespokeRequest.Where(a => a.Id == objRequest.ModuleID).Select(a => a.TrainingName).FirstOrDefault();

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
                                        int count = 0;
                                        List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                                        string token = _identitySv.GetToken();
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            string Hr2Name = string.IsNullOrEmpty(row["HR2Name"].ToString()) ? null : Security.Decrypt(row["HR2Name"].ToString());
                                            string Hr2EmailId = string.IsNullOrEmpty(row["Hr2EmailId"].ToString()) ? null : Security.Decrypt(row["Hr2EmailId"].ToString());
                                            int Hr2UserId = string.IsNullOrEmpty(row["Hr2UserId"].ToString()) ? 0 : int.Parse(row["Hr2UserId"].ToString());
                                            string EndUserName = string.IsNullOrEmpty(row["EndUserName"].ToString()) ? null : Security.Decrypt(row["EndUserName"].ToString());
                                            //----  Send Notification ----//

                                            string title = "Send Enrollment Request";
                                            int UserIDToSend = Hr2UserId;
                                            string type = Record.Bespoke6;
                                            string Message = "You have a pending bespoke training request , awaiting your approval.";

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
                }
                else if (level == 5)
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
                        objRequest.UserStatusInfo = "Completed";
                    }
                    else
                    {
                        objRequest.UserStatusInfo = "Actioned HR Final";
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

                    string EndUserName = null, EndUserEmailId = null, TaEmailId = null;
                    string LmEmailId = null;

                    string trainingName = this._db.BespokeRequest.Where(a => a.Id == objRequest.ModuleID).Select(a => a.TrainingName).FirstOrDefault();

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

                        Message = "Your bespoke request has been approved.";
                        type = Record.Bespoke1;

                        await ScheduleRequestNotificationTo_Common(objRequest.CourseID, objRequest.ScheduleID, title, token, UserIDToSend, Message, type);
                    }
                    else
                    {
                        string title = "Send Enrollment Rejection";
                        string token = _identitySv.GetToken();
                        int UserIDToSend = objRequest.UserID;
                        string type = Record.Enrollment1;

                        string Message = null;

                        Message = "Your bespoke request has been rejected.";
                        type = Record.Bespoke1;

                        await ScheduleRequestNotificationTo_Common(objRequest.CourseID, objRequest.ScheduleID, title, token, UserIDToSend, Message, type);
                    }
                }
                objApiResponse.StatusCode = 200;
                objApiResponse.Description = "SUCCESS";
                return objApiResponse;
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

    }
}
