//using Courses.API.APIModel.TNA;
using TNA.API.Helper;
using TNA.API.Model;
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
using log4net;
using TNA.API.Common;
using TNA.API.APIModel;

namespace TNA.API.Repositories
{
    public class TNACourseRequestRepository : Repository<CourseRequest>, ITNACourseRequest
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TNACourseRequestRepository));
        private CourseContext _db;
        private readonly ICustomerConnectionStringRepository _customerConnection;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly IAccessibilityRule _accessibilityRule;
        private readonly ITLSHelper _tlsHelper;
        private readonly IConfiguration _configuration;
        private readonly INotification _notification;
        private readonly IIdentityService _identitySv;
        private readonly IEmail _emailRepository;
        public TNACourseRequestRepository(IHttpContextAccessor httpContextAccessor, ICustomerConnectionStringRepository customerConnection,
                                       ICustomerConnectionStringRepository customerConnectionStringRepository, IIdentityService identitySv,
                                       IAccessibilityRule AccessibilityRule, INotification notification, IConfiguration configuration, ITLSHelper tlsHelper, IEmail emailRepository, CourseContext context) : base(context)
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
        }

        public async Task<List<TNAYear>> GetTNAYear()
        {
            List<TNAYear> obj = new List<TNAYear>();

            obj = await this._db.TNAYear.Where(a => a.IsDeleted == false).ToListAsync();
            return obj;
        }

        public async Task<List<APICourseRequest>> GetAllCourseRequestLevelOne(int page, int pageSize, int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetAllCourseRequestForLMApproval";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });

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
                                APICourseRequest CourseRequest = new APICourseRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    Date = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Department = row["Department"].ToString(),
                                    Status = row["Status1"].ToString(),
                                    NewStatus = row["NewStatus"].ToString()
                                };

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

        public async Task<int> GetAllCourseRequestLevelOneCount(int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetCourseRequestCountForLMApproval";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });

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

        public async Task<List<APICourseRequest>> GetAllCourseRequestLevelTwo(int page, int pageSize, int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });

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
                                APICourseRequest CourseRequest = new APICourseRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    Date = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Department = row["Department"].ToString(),
                                    Status = row["Status1"].ToString(),
                                    NewStatus = row["NewStatus"].ToString()
                                };

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

        public async Task<int> GetAllCourseRequestLevelTwoCount(int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });

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

        public async Task<List<APICourseRequest>> GetAllCourseRequestLevelThree(int page, int pageSize, int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetAllCourseRequestForHRApproval";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });

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
                                APICourseRequest CourseRequest = new APICourseRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    Date = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Department = row["Department"].ToString(),
                                    Status = row["Status1"].ToString(),
                                    NewStatus = row["NewStatus"].ToString()
                                };

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

        public async Task<int> GetAllCourseRequestLevelThreeCount(int TNAYearId, int UserId, string userName = null, string search = null, string searchText = null, string status = null)
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
                            cmd.CommandText = "GetAllCourseRequestForHRCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });

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
        public async Task<List<APICourseRequest>> GetAllCourseRequestLevelFour(int page, int pageSize, int TNAYearId, int UserId, string userName, string LoginId, string search, string searchText, string status)
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
                            cmd.CommandText = "GetAllCourseRequestForBUApproval";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });

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
                                APICourseRequest CourseRequest = new APICourseRequest
                                {
                                    Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                    UserID = string.IsNullOrEmpty(row["UserID"].ToString()) ? 0 : int.Parse(row["UserID"].ToString()),
                                    UserName = row["UserName"].ToString(),
                                    Date = DateTime.Parse(row["RequestedOn"].ToString()),
                                    Department = row["Department"].ToString(),
                                    Status = row["Status1"].ToString(),
                                    NewStatus = row["NewStatus"].ToString()
                                };

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

        public async Task<int> GetAllCourseRequestLevelFourCount(int TNAYearId, int UserId, string userName, string LoginId, string search, string searchText, string status)
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
                            cmd.CommandText = "GetAllCourseRequestForBUCount";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = search });
                            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchText });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName });
                            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = status });
                            cmd.Parameters.Add(new SqlParameter("@LoginID", SqlDbType.NVarChar) { Value = LoginId });
                            cmd.Parameters.Add(new SqlParameter("@TNAYearId", SqlDbType.Int) { Value = TNAYearId });


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


        public async Task<List<APITNAYear>> GetAllTNAYear()

        {
            var obj1 = (from TNAYear in this._db.TNAYear
                        select new APITNAYear
                        {
                            Id = TNAYear.Id,
                            Year = TNAYear.Year,
                            Status = TNAYear.IsActive,
                            ExpiryDate = TNAYear.ExpiryDate, //== null ? '0001-01-01 00:00:00.0000000' : TNAYear.ExpiryDate //TNAYear.ExpiryDate,
                            CreatedBy = TNAYear.CreatedBy,
                            CreatedDate = TNAYear.CreatedDate
                        });
            obj1 = obj1.OrderByDescending(a => a.Id);
            return obj1.ToList();
        }

        public async Task<int> PostTNAYear(string Year, int UserId)
        {

            TNAYear obj = new TNAYear();
            obj = await this._db.TNAYear.Where(a => a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
            if (obj != null)
            {
                obj.IsActive = false;

                this._db.TNAYear.Update(obj);
                await this._db.SaveChangesAsync();
            }
            TNAYear objPost = new TNAYear();
            TNAYear objTNAYear = new TNAYear();
            objPost = await this._db.TNAYear.Where(a => a.Year == Year).FirstOrDefaultAsync();

            if (objPost != null)
            {
                return 0;
            }
            objTNAYear.Year = Year;
            objTNAYear.IsActive = true;
            objTNAYear.IsDeleted = false;
            objTNAYear.CreatedBy = UserId;
            objTNAYear.CreatedDate = DateTime.UtcNow;
            objTNAYear.ModifiedBy = UserId;
            objTNAYear.ModifiedDate = DateTime.UtcNow;

            this._db.TNAYear.Add(objTNAYear);
            await this._db.SaveChangesAsync();

            return 1;
        }

        public async Task<ApiResponse> DeleteCourseRequest(int courseRequestId, int UserId, bool IsNominate)
        {
            ApiResponse obj = new ApiResponse();

            try
            {
                var table = await (from courseRequest in _db.CourseRequest
                                   join courseRequestDetails in _db.CoursesRequestDetails on courseRequest.Id equals courseRequestDetails.CourseRequestId
                                   where courseRequest.Id == courseRequestId && courseRequest.IsActive == true && courseRequest.IsDeleted == Record.NotDeleted
                                          && courseRequestDetails.IsNominate == IsNominate && courseRequestDetails.IsActive == true && courseRequestDetails.IsDeleted == Record.NotDeleted
                                          && courseRequest.ModifiedBy == courseRequestDetails.StatusUpdatedBy && courseRequestDetails.StatusUpdatedBy == UserId
                                   select new
                                   {
                                       courseRequest,
                                       courseRequestDetails
                                   }).SingleOrDefaultAsync();

                if (table.courseRequest != null && table.courseRequestDetails != null)
                {
                    table.courseRequest.IsDeleted = true;
                    table.courseRequest.IsActive = false;
                    table.courseRequest.ModifiedBy = UserId;
                    table.courseRequest.ModifiedDate = DateTime.UtcNow;

                    this._db.CourseRequest.Update(table.courseRequest);

                    table.courseRequestDetails.IsDeleted = true;
                    table.courseRequestDetails.IsActive = false;
                    table.courseRequestDetails.ModifiedBy = UserId;
                    table.courseRequestDetails.ModifiedDate = DateTime.UtcNow;

                    this._db.CoursesRequestDetails.Update(table.courseRequestDetails);

                    await this._db.SaveChangesAsync();
                    obj.Description = "success";
                    obj.StatusCode = 200;
                    return obj;
                }
                obj.Description = "dependancy exists";
                obj.StatusCode = 400;
                return obj;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<TnaRequestData>> GetAllRequestedBatches(int UserId, int page, int pageSize, string searchUser = null)
        {
            List<TnaRequestData> aPIILTRequestedBatchesList = new List<TnaRequestData>();
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
                            cmd.CommandText = "[dbo].[GetAllTnaRequests]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@searchUser", SqlDbType.NVarChar) { Value = searchUser });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                TnaRequestData aPIILTRequestedBatches = new TnaRequestData();
                                aPIILTRequestedBatches.UserId = row["UserId"].ToString();
                                aPIILTRequestedBatches.UserName = row["UserName"].ToString();
                                aPIILTRequestedBatches.RequestDate = row["RequestDate"].ToString();
                                aPIILTRequestedBatchesList.Add(aPIILTRequestedBatches);
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
            return aPIILTRequestedBatchesList;
        }

        public async Task<int> GetAllRequestedBatchesCount(int UserId, string searchUser = null)
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
                            cmd.CommandText = "[dbo].[GetAllTnaRequests]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = 1 });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = 0 });
                            cmd.Parameters.Add(new SqlParameter("@searchUser", SqlDbType.NVarChar) { Value = searchUser });

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
                                Count = string.IsNullOrEmpty(row["TotalRecords"].ToString()) ? 0 : int.Parse(row["TotalRecords"].ToString());
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

        public async Task<List<TnaEmployeeNominateRequestPayload>> GetTnaRequestsByUser(int UserId)
        {
            try
            {
                var obj = (from tnrd in _db.TnaNominateRequestData
                           join u in _db.UserMaster on tnrd.UserId equals u.Id
                           join ted in _db.TnaEmployeeData on tnrd.TnaEmployeeDataId equals ted.Id
                           where u.IsDeleted == false && u.Id == UserId && (tnrd.RequestStatus == "Requested" || tnrd.RequestStatus == "Approved" || tnrd.RequestStatus == "Assigned" || tnrd.RequestStatus == "Reassigned")
                           select new TnaEmployeeNominateRequestPayload
                           {
                               Id = ted.Id,
                               CourseDetails = ted.CourseDetails
                           });

                return obj.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<Message> PostTnaTnaSupervisedRequest(int SelectedUserId, int UserId, List<TnaEmployeeNominateRequestPayload> TnaSupervisedRequest)
        {
            try
            {
                var obj = (from tnrd in _db.TnaNominateRequestData
                           join u in _db.UserMaster on tnrd.UserId equals u.Id
                           join ted in _db.TnaEmployeeData on tnrd.TnaEmployeeDataId equals ted.Id
                           where u.IsDeleted == false && u.Id == SelectedUserId
                           select new TnaEmployeeNominateRequestPayload
                           {
                               Id = ted.Id,
                               CourseDetails = ted.CourseDetails
                           });

                List<TnaEmployeeNominateRequestPayload> TnaEmployeeRequests = obj.ToListAsync().Result;

                foreach (TnaEmployeeNominateRequestPayload SupervisedRequest in TnaSupervisedRequest)
                {
                    var reqd = (from tnrd in _db.TnaNominateRequestData
                                join ted in _db.TnaEmployeeData on tnrd.TnaEmployeeDataId equals ted.Id
                                where ted.Id == SupervisedRequest.Id && tnrd.UserId == SelectedUserId
                                select tnrd);

                    TnaNominateRequestData TnaRequestData = reqd.FirstOrDefaultAsync().Result;

                    if (TnaRequestData == null)
                    {
                        TnaNominateRequestData TnaEmployeeNominateRequestData = new TnaNominateRequestData();
                        TnaEmployeeNominateRequestData.UserId = SelectedUserId;
                        TnaEmployeeNominateRequestData.TnaEmployeeDataId = SupervisedRequest.Id;
                        TnaEmployeeNominateRequestData.RequestStatus = "Assigned";
                        TnaEmployeeNominateRequestData.IsDeleted = false;
                        TnaEmployeeNominateRequestData.IsActive = true;
                        TnaEmployeeNominateRequestData.CreatedBy = UserId;
                        TnaEmployeeNominateRequestData.CreatedDate = DateTime.UtcNow;
                        _db.TnaNominateRequestData.Add(TnaEmployeeNominateRequestData);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        foreach (TnaEmployeeNominateRequestPayload TnaEmployeeRequest in TnaEmployeeRequests)
                        {
                            if (TnaEmployeeRequest.Id == SupervisedRequest.Id)
                            {
                                if (TnaRequestData.RequestStatus == "Requested" || TnaRequestData.RequestStatus == "Rejected")
                                {
                                    TnaRequestData.RequestStatus = "Approved";
                                    TnaRequestData.ModifiedBy = UserId;
                                    TnaRequestData.ModifiedDate = DateTime.UtcNow;
                                    this._db.TnaNominateRequestData.Update(TnaRequestData);
                                    await this._db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }

                foreach (TnaEmployeeNominateRequestPayload leftRequestData in TnaEmployeeRequests)
                {
                    TnaEmployeeNominateRequestPayload data = TnaSupervisedRequest.Find(x => x.CourseDetails.Contains(leftRequestData.CourseDetails));

                    if(data == null)
                    {
                        var reqd = (from tnrd in _db.TnaNominateRequestData
                                    join ted in _db.TnaEmployeeData on tnrd.TnaEmployeeDataId equals ted.Id
                                    where ted.Id == leftRequestData.Id && tnrd.UserId == SelectedUserId
                                    select tnrd);

                        TnaNominateRequestData leftRequest = reqd.FirstOrDefaultAsync().Result;

                        leftRequest.RequestStatus = "Rejected";
                        leftRequest.ModifiedBy = UserId;
                        leftRequest.ModifiedDate = DateTime.UtcNow;
                        this._db.TnaNominateRequestData.Update(leftRequest);
                        await this._db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return Message.Ok;
        }

        public async Task<List<TnaRequestsDetails>> GetTnaRequestsDetails(int UserId, int page, int pageSize, string searchData = null)
        {
            List<TnaRequestsDetails> tnaRequestsDetailsList = new List<TnaRequestsDetails>();
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
                            cmd.CommandText = "[dbo].[GetTnaRequestsDetails]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = page });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                            cmd.Parameters.Add(new SqlParameter("@searchData", SqlDbType.NVarChar) { Value = searchData });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                            connection.Close();

                            foreach (DataRow row in dt.Rows)
                            {
                                TnaRequestsDetails tnaRequestsDetails = new TnaRequestsDetails();
                                tnaRequestsDetails.Id = row["Id"].ToString();
                                tnaRequestsDetails.CourseDetails = row["CourseDetails"].ToString();
                                tnaRequestsDetails.CategoryType = row["CategoryType"].ToString();
                                tnaRequestsDetails.RequestStatus = row["RequestStatus"].ToString();
                                tnaRequestsDetails.RequestDate = row["RequestDate"].ToString();
                                tnaRequestsDetails.ActionDate = row["ActionDate"].ToString();
                                tnaRequestsDetailsList.Add(tnaRequestsDetails);
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
            return tnaRequestsDetailsList;
        }

        public async Task<int> GetTnaRequestsDetailsCount(int UserId, string searchData = null)
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
                            cmd.CommandText = "[dbo].[GetTnaRequestsDetails]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = 1 });
                            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = 0 });
                            cmd.Parameters.Add(new SqlParameter("@searchData", SqlDbType.NVarChar) { Value = searchData });

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
                                Count = string.IsNullOrEmpty(row["TotalRecords"].ToString()) ? 0 : int.Parse(row["TotalRecords"].ToString());
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
    }
}
