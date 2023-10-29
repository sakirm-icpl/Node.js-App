using Courses.API.APIModel;
using Courses.API.APIModel.Competency;
using Courses.API.Common;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Model.Competency;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Courses.API.Common.EnumHelper;
using log4net;
using Courses.API.Model.TNA;
using TimeZoneConverter;
using Microsoft.AspNetCore.Mvc.Rendering;
using Courses.API.Migrations;
using static Courses.API.Model.ResponseModels;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using System.IO;

namespace Courses.API.Repositories
{
    public class MyCoursesRepository : IMyCoursesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MyCoursesRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;
        private readonly List<TimeZoneList> _tzList;

            
        ICustomerConnectionStringRepository _customerConnection;
        IAccessibilityRule _accessibilityRule;
        INodalCourseRequestsRepository _nodalCourseRequests;
        IAzureStorage _azurestorage;
        public MyCoursesRepository(CourseContext context,
            IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnection, IAccessibilityRule AccessibilityRule, IAzureStorage  azurestorage,
        INodalCourseRequestsRepository nodalCourseRequests)
        {
            this._db = context;
            this._configuration = configuration;
            this._customerConnection = customerConnection;
            _accessibilityRule = AccessibilityRule;
            _nodalCourseRequests = nodalCourseRequests;
            _azurestorage = azurestorage;
            // get system time zone
            var tzs = TimeZoneInfo.GetSystemTimeZones();
            _tzList = tzs.Select(tz => new TimeZoneList
            {
                Text = tz.DisplayName,
                Value = tz.Id
            }).ToList();
        }
        public int GetIdFromUserId(string userid)
        {
            UserMaster userMaster = _db.UserMaster.Where(a => a.UserId == userid && a.IsDeleted == false).FirstOrDefault();
            return userMaster.Id;
        }
        public async Task<List<APIMyCourses>> Get(int userId, int page, int pageSize, int? categoryId = null, string CourseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null,string IsActive=null)
        {

            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        if(provider == null)
                        {
                            cmd.CommandText = "GetApplicableCourses";
                        }
                        else
                        {
                            cmd.CommandText = "GetApplicableCoursesExternal";
                        }
                      
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = CourseStatus });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@subSubCategoryId", SqlDbType.Int) { Value = subSubCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyFilter", SqlDbType.VarChar) { Value = CompetencyFilter });
                        cmd.Parameters.Add(new SqlParameter("@JobRoleId", SqlDbType.Int) { Value = JobRoleId });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyId", SqlDbType.Int) { Value = CompetencyId });
                        cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = provider });
                        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.VarChar) { Value = IsActive });

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
                            APIMyCourses Course = new APIMyCourses();
                            Course.Title = row["Title"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CourseCreationdDays = string.IsNullOrEmpty(row["CourseCreationdDays"].ToString()) ? 0 : int.Parse(row["CourseCreationdDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.Status = string.IsNullOrEmpty(row["CourseStatus"].ToString()) ? "NotStarted" : row["CourseStatus"].ToString();
                            Course.CourseType = row["CourseType"].ToString();
                            Course.CourseStartDate = string.IsNullOrEmpty(row["CourseStartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseStartDate"].ToString());
                            Course.CourseCompleteDate = string.IsNullOrEmpty(row["CourseCompletedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseCompletedDate"].ToString());
                            Course.CourseStartDate = Course.CourseStartDate == DateTime.MinValue ? null : Course.CourseStartDate;
                            Course.CourseCompleteDate = Course.CourseCompleteDate == DateTime.MinValue ? null : Course.CourseCompleteDate;
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            Course.IsCourseApplicable = true;
                            Course.CourseApprovalStatus = "Enrolled";
                            Course.AssessmentPercentage = string.IsNullOrEmpty(row["AssessmentPercentage"].ToString()) ? "--" : row["AssessmentPercentage"].ToString();
                            Course.ScheduleRequestStatus = row["ScheduleRequestStatus"].ToString();
                            Course.NumberofModules = string.IsNullOrEmpty(row["NumberofModules"].ToString()) ? 0 : int.Parse(row["NumberofModules"].ToString());
                            Course.AssessmentResult = string.IsNullOrEmpty(row["AssessmentResult"].ToString()) ? "--" : row["AssessmentResult"].ToString();
                            Course.IsPreRequisiteCourse = string.IsNullOrEmpty(row["PreRequisiteStatus"].ToString()) ? false : bool.Parse(row["PreRequisiteStatus"].ToString());
                            Course.DurationInMinutes = string.IsNullOrEmpty(row["DurationInMinutes"].ToString()) ? 0 : int.Parse(row["DurationInMinutes"].ToString());
                            Course.IsExternalProvider = string.IsNullOrEmpty(row["IsExternalProvider"].ToString()) ? false : bool.Parse(row["IsExternalProvider"].ToString());
                            Course.ExternalProvider = string.IsNullOrEmpty(row["ExternalProvider"].ToString()) ? null : row["ExternalProvider"].ToString();
                            Course.CourseURL = string.IsNullOrEmpty(row["CourseURL"].ToString()) ? null : row["CourseURL"].ToString();
                            Course.IsRetraining = string.IsNullOrEmpty(row["IsRetraining"].ToString()) ? false : bool.Parse(row["IsRetraining"].ToString());
                            Course.NodalApprovalStatus = row["NodalApprovalStatus"].ToString();
                            Course.IsSCORM = Convert.ToBoolean(row["IsSCORM"].ToString());
                            MyCourses.Add(Course);
                        }
                        reader.Dispose();
                        return MyCourses;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }



        public async Task<List<APIMyCourses>> GetAllCourseDetails(APIUserCourseDetails obj)
        {

            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllApplicableCoursesDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@EncryptedUserID", SqlDbType.VarChar) { Value = obj.UserId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = obj.categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = obj.search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = obj.status });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = obj.courseType });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = obj.page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = obj.pageSize });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = obj.subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = obj.sortBy });
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
                            APIMyCourses Course = new APIMyCourses();
                            Course.Title = row["Title"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CourseCreationdDays = string.IsNullOrEmpty(row["CourseCreationdDays"].ToString()) ? 0 : int.Parse(row["CourseCreationdDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.Status = string.IsNullOrEmpty(row["CourseStatus"].ToString()) ? "NotStarted" : row["CourseStatus"].ToString();
                            Course.CourseType = row["CourseType"].ToString();
                            Course.CourseStartDate = string.IsNullOrEmpty(row["CourseStartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseStartDate"].ToString());
                            Course.CourseCompleteDate = string.IsNullOrEmpty(row["CourseCompletedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseCompletedDate"].ToString());
                            Course.CourseStartDate = Course.CourseStartDate == DateTime.MinValue ? null : Course.CourseStartDate;
                            Course.CourseCompleteDate = Course.CourseCompleteDate == DateTime.MinValue ? null : Course.CourseCompleteDate;
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsAssignment = string.IsNullOrEmpty(row["IsAssignment"].ToString()) ? false : bool.Parse(row["IsAssignment"].ToString());
                            Course.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : bool.Parse(row["IsAssessment"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            Course.IsCourseApplicable = true;
                            Course.CourseApprovalStatus = "Enrolled";
                            Course.AssessmentPercentage = string.IsNullOrEmpty(row["AssessmentPercentage"].ToString()) ? "--" : row["AssessmentPercentage"].ToString();
                            Course.ScheduleRequestStatus = row["ScheduleRequestStatus"].ToString();
                            Course.FirstAccessDate = row["FirstAccessDate"].ToString();
                            Course.LastAccessDate = row["LastAccessDate"].ToString();
                            Course.TimeSpent = row["TimeSpent"].ToString();
                            Course.views = Convert.ToInt32(row["views"]);
                            Course.Progressinpercentage = Convert.ToInt32(row["Progressinpercentage"]);
                            Course.score = Convert.ToInt32(row["score"]);
                            MyCourses.Add(Course);
                        }
                        reader.Dispose();
                        return MyCourses;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetProgressStatusDuration(int userId, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetApplicableCoursesDuration";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = courseStatus });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Duration"].ToString()) ? 0 : int.Parse(row["Duration"].ToString());
                        }
                        reader.Dispose();
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

        public async Task<int> GetNotStartedDuration(int userId, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetNotStartedCoursesDuration";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Duration"].ToString()) ? 0 : int.Parse(row["Duration"].ToString());
                        }
                        reader.Dispose();
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

        public async Task<int> Count(int userId, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null, string IsActive=null)
        {
            int Count = 0;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "GetApplicableCoursesCount";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                    cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = search });
                    cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = courseStatus });
                    cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                    cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                    cmd.Parameters.Add(new SqlParameter("@subSubCategoryId", SqlDbType.Int) { Value = subSubCategoryId });
                    cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                    cmd.Parameters.Add(new SqlParameter("@CompetencyFilter", SqlDbType.VarChar) { Value = CompetencyFilter });
                    cmd.Parameters.Add(new SqlParameter("@JobRoleId", SqlDbType.Int) { Value = JobRoleId });
                    cmd.Parameters.Add(new SqlParameter("@CompetencyId", SqlDbType.Int) { Value = CompetencyId });
                    cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = provider });
                    cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.VarChar) { Value = IsActive });

                    await dbContext.Database.OpenConnectionAsync();
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count <= 0)
                    {
                        reader.Dispose();
                        return 0;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                    }
                    reader.Dispose();
                }
            }

            return Count;
        }

        public async Task<int> AnonymousCount(string orgcode, int userId, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetApplicableCoursesCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = courseStatus });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyFilter", SqlDbType.VarChar) { Value = CompetencyFilter });
                        cmd.Parameters.Add(new SqlParameter("@JobRoleId", SqlDbType.Int) { Value = JobRoleId });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyId", SqlDbType.Int) { Value = CompetencyId });
                        cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = provider });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                        }
                        reader.Dispose();
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return Count;
        }
        public async Task<List<APIMyCourses>> GetAllCourse(int userId, int page, int pageSize, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string status = null, string provider = null, string IsActive=null)
        {
            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllCourse";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@subSubCategoryId", SqlDbType.Int) { Value = subSubCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyCategoryID", SqlDbType.Int) { Value = CompetencyCategoryID });
                        cmd.Parameters.Add(new SqlParameter("@isShowCatalogue", SqlDbType.Bit) { Value = isShowCatalogue });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = status });
                        cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = provider });
                        cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.VarChar) { Value = IsActive });


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
                            APIMyCourses Course = new APIMyCourses();
                            Course.Title = row["Title"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.SubCategoryName = row["SubCategoryName"].ToString();
                            Course.SubSubCategoryName = row["SubSubCategoryName"].ToString();

                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.CourseType = row["CourseType"].ToString();
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            //Course.SubSubCategoryId = string.IsNullOrEmpty(row["SubSubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubSubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : bool.Parse(row["IsAssessment"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            Course.IsCourseApplicable = string.IsNullOrEmpty(row["IsApplicableStatus"].ToString()) ? false : bool.Parse(row["IsApplicableStatus"].ToString());
                            Course.CourseApprovalStatus = row["CourseApprovalStatus"].ToString();
                            Course.CompetencyCategoryID = string.IsNullOrEmpty(row["CompetencyCategoryID"].ToString()) ? 0 : int.Parse(row["CompetencyCategoryID"].ToString());
                            Course.CompetencyCategory = row["CompetencyCategory"].ToString();
                            if (Course.IsCourseApplicable == true && Course.CourseApprovalStatus == null)
                            {
                                Course.CourseApprovalStatus = "Assigned";
                            }
                            Course.ScheduleRequestStatus = row["ScheduleRequestStatus"].ToString();
                            Course.NumberofModules = string.IsNullOrEmpty(row["NumberofModules"].ToString()) ? 0 : int.Parse(row["NumberofModules"].ToString());
                            Course.NodalApprovalStatus = row["NodalApprovalStatus"].ToString();
                            Course.IsSCORM = Convert.ToBoolean(row["IsSCORM"].ToString());
                            // Below code is added for UDEMY integration
                            Course.CourseURL = Convert.ToString(row["CourseURL"]);
                            MyCourses.Add(Course);
                        }
                        reader.Dispose();

                    }
                }
                return MyCourses;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        private async Task<string> OrgnizationConnectionString(string organizationCode)
        {
            string OrgnizationConnectionString = await this._customerConnection.GetConnectionStringByOrgnizationCode(organizationCode);
            if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
                return string.Empty; //return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

            if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                return string.Empty;

            return OrgnizationConnectionString;
        }
        public async Task<List<APIMyCourses>> GetAllCatalogCourse(string orgcode, int page, int pageSize, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string status = null, string provider = null)
        {
            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {

                var OrgnizationConnectionString = await this.OrgnizationConnectionString(orgcode); //todo move to cache

                using (var dbContext = this._customerConnection.GetDbContext(OrgnizationConnectionString))
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllCourseForCatalog";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyCategoryID", SqlDbType.Int) { Value = CompetencyCategoryID });
                        cmd.Parameters.Add(new SqlParameter("@isShowCatalogue", SqlDbType.Bit) { Value = isShowCatalogue });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = status });

                        // Below code is added for UDEMY integration
                        cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = provider });


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
                            APIMyCourses Course = new APIMyCourses();
                            Course.Title = row["Title"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.CourseType = row["CourseType"].ToString();
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : bool.Parse(row["IsAssessment"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            Course.IsCourseApplicable = string.IsNullOrEmpty(row["IsApplicableStatus"].ToString()) ? false : bool.Parse(row["IsApplicableStatus"].ToString());
                            Course.CourseApprovalStatus = row["CourseApprovalStatus"].ToString();
                            Course.CompetencyCategoryID = string.IsNullOrEmpty(row["CompetencyCategoryID"].ToString()) ? 0 : int.Parse(row["CompetencyCategoryID"].ToString());
                            Course.CompetencyCategory = row["CompetencyCategory"].ToString();
                            Course.Status = row["Status"].ToString();
                            if (Course.IsCourseApplicable == true && Course.CourseApprovalStatus == null)
                            {
                                Course.CourseApprovalStatus = "Assigned";
                            }
                            Course.ScheduleRequestStatus = row["ScheduleRequestStatus"].ToString();
                            Course.NumberofModules = string.IsNullOrEmpty(row["NumberofModules"].ToString()) ? 0 : int.Parse(row["NumberofModules"].ToString());

                            // Below code is added for UDEMY integration
                            Course.CourseURL = Convert.ToString(row["CourseURL"]);
                            MyCourses.Add(Course);
                        }
                        reader.Dispose();

                    }
                }
                return MyCourses;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


        public async Task<List<APIMyCourses>> GetAllUserCourseData(APIUserCourseDetails obj)
        {
            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllUserCourseData";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@EncryptedUserID", SqlDbType.VarChar) { Value = obj.UserId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = obj.categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = obj.search });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = obj.courseType });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = obj.page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = obj.pageSize });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = obj.subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = obj.sortBy });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyCategoryID", SqlDbType.Int) { Value = obj.CompetencyCategoryID });
                        cmd.Parameters.Add(new SqlParameter("@isShowCatalogue", SqlDbType.Bit) { Value = obj.isShowCatalogue });

                        // Below code is added for UDEMY integration
                        cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = obj.provider });

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
                            APIMyCourses Course = new APIMyCourses();
                            Course.Title = row["Title"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.CourseType = row["CourseType"].ToString();
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsAssignment = string.IsNullOrEmpty(row["IsAssignment"].ToString()) ? false : bool.Parse(row["IsAssignment"].ToString());
                            Course.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : bool.Parse(row["IsAssessment"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            Course.IsCourseApplicable = string.IsNullOrEmpty(row["IsApplicableStatus"].ToString()) ? false : bool.Parse(row["IsApplicableStatus"].ToString());
                            Course.CourseApprovalStatus = row["CourseApprovalStatus"].ToString();
                            Course.CompetencyCategoryID = string.IsNullOrEmpty(row["CompetencyCategoryID"].ToString()) ? 0 : int.Parse(row["CompetencyCategoryID"].ToString());
                            Course.CompetencyCategory = row["CompetencyCategory"].ToString();
                            Course.Status = row["Status"].ToString();
                            if (Course.IsCourseApplicable == true && Course.CourseApprovalStatus == null)
                            {
                                Course.CourseApprovalStatus = "Assigned";
                            }
                            Course.ScheduleRequestStatus = row["ScheduleRequestStatus"].ToString();

                            // Below code is added for UDEMY integration
                            Course.CourseURL = Convert.ToString(row["CourseURL"]);
                            Course.FirstAccessDate = row["FirstAccessDate"].ToString();
                            Course.LastAccessDate = row["LastAccessDate"].ToString();
                            Course.TimeSpent = row["TimeSpent"].ToString();
                            Course.views = Convert.ToInt32(row["views"]);
                            Course.Progressinpercentage = Convert.ToInt32(row["Progressinpercentage"]);
                            Course.score = Convert.ToInt32(row["score"]);

                            MyCourses.Add(Course);
                        }
                        reader.Dispose();

                    }
                }
                return MyCourses;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetAllCatalogCourseCount(string orgcode, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string provider = null)
        {
            int Count = 0;
            try
            {
                var OrgnizationConnectionString = await this.OrgnizationConnectionString(orgcode); //todo move to cache

                using (var dbContext = this._customerConnection.GetDbContext(OrgnizationConnectionString))
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetAllCatalogCourseCount";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                        cmd.Parameters.Add(new SqlParameter("@CompetencyCategoryID", SqlDbType.Int) { Value = CompetencyCategoryID });
                        cmd.Parameters.Add(new SqlParameter("@isShowCatalogue", SqlDbType.Bit) { Value = isShowCatalogue });
                        cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = provider });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                        }
                        reader.Dispose();
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

        public async Task<int> GetAllCourseCount(int userId, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null, bool? isShowCatalogue = null, string provider = null, string IsActive=null)
        {
            int Count = 0;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "GetAllCourseCount";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                    cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = search });
                    cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                    cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                    cmd.Parameters.Add(new SqlParameter("@subSubCategoryId", SqlDbType.Int) { Value = subSubCategoryId });
                    cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                    cmd.Parameters.Add(new SqlParameter("@CompetencyCategoryID", SqlDbType.Int) { Value = CompetencyCategoryID });
                    cmd.Parameters.Add(new SqlParameter("@isShowCatalogue", SqlDbType.Bit) { Value = isShowCatalogue });
                    cmd.Parameters.Add(new SqlParameter("@provider", SqlDbType.VarChar) { Value = provider });
                    cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.VarChar) { Value = IsActive });

                    await dbContext.Database.OpenConnectionAsync();
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count <= 0)
                    {
                        reader.Dispose();
                        return 0;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                    }
                    reader.Dispose();
                }
            }
            return Count;
        }
        public async Task<List<APIMyCourses>> GetCourseProgress(int userId, int page, int pageSize, string courseStatus = null, string courseType = null, int? subCategoryId = null, int? categoryId = null, string search = null)
        {
            var Query = (from c in _db.Course
                         join ccs in this._db.CourseCompletionStatus on c.Id equals ccs.CourseId
                         join cat in this._db.Category on c.CategoryId equals cat.Id into r
                         from category in r.DefaultIfEmpty()
                         join subCat in this._db.SubCategory on c.SubCategoryId equals subCat.Id into subCatTemp
                         from subCat in subCatTemp.DefaultIfEmpty()
                         join CourseRating in this._db.CourseRating on c.Id equals CourseRating.CourseId into ratingTemp
                         from CourseRating in ratingTemp.DefaultIfEmpty()
                         where (c.IsDeleted == false && c.IsActive == true && ccs.Status.Equals(Status.InProgress) && ccs.UserId == userId)
                         select new
                         {
                             Course = c,
                             CourseStatus = ccs,
                             Category = category,
                             SubCategory = subCat,
                             CourseRating = CourseRating
                         });

            if (categoryId != null)
                Query = Query.Where(c => c.Course.CategoryId == categoryId);
            if (!string.IsNullOrEmpty(search) && !search.ToLower().Equals("null"))
                Query = Query.Where(c => c.Course.Title.Contains(search) || c.Course.Metadata.Contains(search));
            if (subCategoryId != null)
                Query = Query.Where(c => c.Course.SubCategoryId == subCategoryId);
            if (courseStatus != null)
                Query = Query.Where(c => (c.CourseStatus.Status.ToLower() == courseStatus.ToLower()));
            if (courseType != null)
                Query = Query.Where(c => (c.Course.CourseType.ToLower() == courseType.ToLower()));
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);

            Query = Query.OrderByDescending(c => c.Course.Id);
            return await Query.Select(c => new APIMyCourses
            {
                Title = c.Course.Title,
                Code = c.Course.Code,
                CourseFee = c.Course.CourseFee,
                CourseId = c.Course.Id,
                ThumbnailPath = c.Course.ThumbnailPath,
                Currency = c.Course.Currency,
                Description = c.Course.Description,
                CompletionPeriodDays = c.Course.CompletionPeriodDays,
                CourseType = c.Course.CourseType,
                Status = c.CourseStatus.Status,
                CategoryName = (c.Category == null) ? null : c.Category.Name,
                CategoryId = (c.Category == null) ? 0 : c.Category.Id,
                CourseRating = (c.CourseRating == null) ? (float?)null : float.Parse(c.CourseRating.Average.ToString()),
                SubCategoryId = (c.SubCategory == null) ? (int?)null : c.SubCategory.Id,
                CourseStartDate = (c.CourseStatus == null) ? (DateTime?)null : c.CourseStatus.CreatedDate,
                CourseCompleteDate = (c.CourseStatus == null) ? (DateTime?)null : c.CourseStatus.ModifiedDate
            }).Distinct().ToListAsync();
        }
        public async Task<int> GetCourseProgressCount(int userId, string courseStatus = null, string courseType = null, int? subCategoryId = null, int? categoryId = null, string search = null)
        {
            var Query = (from c in _db.Course
                         join ccs in this._db.CourseCompletionStatus on c.Id equals ccs.CourseId
                         join cat in this._db.Category on c.CategoryId equals cat.Id into r
                         from category in r.DefaultIfEmpty()
                         join subCat in this._db.SubCategory on c.SubCategoryId equals subCat.Id into subCatTemp
                         from subCat in subCatTemp.DefaultIfEmpty()
                         join CourseRating in this._db.CourseRating on c.Id equals CourseRating.CourseId into ratingTemp
                         from CourseRating in ratingTemp.DefaultIfEmpty()
                         where (c.IsDeleted == false && c.IsActive == true && ccs.Status.Equals(Status.InProgress) && ccs.UserId == userId)
                         select new
                         {
                             Course = c,
                             CourseStatus = ccs,
                             Category = category,
                             SubCategory = subCat,
                             CourseRating = CourseRating
                         });
            if (categoryId != null)
                Query = Query.Where(c => c.Course.CategoryId == categoryId);
            if (!string.IsNullOrEmpty(search) && !search.ToLower().Equals("null"))
                Query = Query.Where(c => c.Course.Title.Contains(search) || c.Course.Metadata.Contains(search));
            if (subCategoryId != null)
                Query = Query.Where(c => c.Course.SubCategoryId == subCategoryId);
            if (courseStatus != null)
                Query = Query.Where(c => (c.CourseStatus.Status.ToLower() == courseStatus.ToLower()));
            if (courseType != null)
                Query = Query.Where(c => (c.Course.CourseType.ToLower() == courseType.ToLower()));
            return await Query.CountAsync();
        }
        public async Task<List<APIMyCourses>> GetNotStarted(int userId, int page, int pageSize, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null)
        {
            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetNotStartedCourses";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
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
                            APIMyCourses Course = new APIMyCourses();
                            Course.Title = row["Title"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.Status = row["CourseStatus"].ToString();
                            Course.CourseType = row["CourseType"].ToString();
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsAssignment = string.IsNullOrEmpty(row["IsAssignment"].ToString()) ? false : bool.Parse(row["IsAssignment"].ToString());
                            Course.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : bool.Parse(row["IsAssessment"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            Course.IsCourseApplicable = true;
                            Course.CourseApprovalStatus = "Enrolled";
                            Course.ScheduleRequestStatus = row["ScheduleRequestStatus"].ToString();

                            MyCourses.Add(Course);
                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return MyCourses;
        }
        public async Task<int> NotStartedCount(int userId, int? categoryId = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetNotStartedCoursesCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());


                        }
                        reader.Dispose();

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

        public async Task<APIMyCoursesModule> GetModule(int userId, int courseId, string orgcode, int? groupId = null)
        {
            string Status = string.Empty;
            if (groupId != null && groupId != 0)
            {
                APIScormGroup aPIScormGroup = await _nodalCourseRequests.GetUserforCompletion((int)groupId);
                if (aPIScormGroup == null)
                    return null;
                userId = aPIScormGroup.UserId;
                Status = aPIScormGroup.Status;
            }

            APIMyCoursesModule CourseInfo = await GetModulesFromDb(userId, courseId, orgcode);
            List<int> ModuleId = CourseInfo.Modules.Select(a => a.ModuleId).ToList();
            #region "Code added to add information related to BigBlueButton Meeting details"
            try
            {
                if (CourseInfo.ModuleSequence != null)
                {
                    #region "Code to handle multiple schedules"
                    try
                    {
                        SqlParameter[] spParameters = new SqlParameter[2];
                        spParameters[0] = new SqlParameter("@USERID", SqlDbType.Int) { Value = userId };
                        spParameters[1] = new SqlParameter("@COURSEID", SqlDbType.Int) { Value = courseId };


                        DataTable dataTable = await ExecuteStoredProcedure("[dbo].[uspGetBigBlueMeetingSchedule]", spParameters);
                        int index = 0;
                        if (dataTable != null)
                        {
                            foreach (var item in CourseInfo.ModuleSequence)
                            {
                                DataRow[] dataRow = dataTable.Select("ModuleId=" + item.ModuleId);
                                if (dataRow != null)
                                {
                                    CourseInfo.ModuleSequence[index].BBBmeetingDetails = new List<BBMeetingDetails>();
                                    for (int i = 0; i < dataRow.Length; i++)
                                    {

                                        int elapsedMinutesInDay = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalMinutes);
                                        int meetingDuration = Convert.ToInt32(DateTime.Parse((string)dataRow[i]["MeetingTime"]).TimeOfDay.TotalMinutes + (int)dataRow[i]["Duration"]);
                                        bool isMeetingExpired = true;
                                        if (Convert.ToDateTime(dataRow[i]["MeetingDate"]).Date == DateTime.Now.Date)
                                        {
                                            if (meetingDuration >= elapsedMinutesInDay)
                                                isMeetingExpired = false;
                                        }
                                        else if (Convert.ToDateTime(dataRow[i]["MeetingDate"]) > DateTime.Now.Date)
                                            isMeetingExpired = false;
                                        CourseInfo.ModuleSequence[index].BBBmeetingDetails.Add(new BBMeetingDetails { MeetingID = Convert.ToString(dataRow[i]["MeetingID"]), MeetingName = Convert.ToString(dataRow[i]["MeetingName"]), MeetingTime = Convert.ToString(dataRow[i]["MeetingTime"]), MeetingDate = Convert.ToDateTime(dataRow[i]["MeetingDate"]), CourseId = Convert.ToInt32(dataRow[i]["CourseId"]), MeetingExpired = isMeetingExpired, Duration = Convert.ToInt32(dataRow[i]["Duration"]) });

                                    }
                                }

                                index++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            #endregion
            if (CourseInfo.CourseType == "vilt")
            {
                try
                {
                    if (CourseInfo.ModuleSequence != null)
                    {
                        #region "Code to handle multiple schedules"
                        try
                        {
                            int index = 0;
                            for (int j = 0; j < ModuleId.Count; j++)
                            {

                                SqlParameter[] spParameters = new SqlParameter[3];
                                spParameters[0] = new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId };
                                spParameters[1] = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
                                spParameters[2] = new SqlParameter("@ModuleId", SqlDbType.Int) { Value = ModuleId[j] };

                                DataTable dataTable = await ExecuteStoredProcedure("[dbo].[GetZoomMeetingSchedule]", spParameters);

                                if (dataTable != null)
                                {
                                    CourseInfo.ModuleSequence[index].apizooms = new List<APIZoom>();
                                    for (int i = 0; i < dataTable.Rows.Count; i++)
                                    {
                                        APIZoom zoom = new APIZoom();
                                        CourseInfo.ModuleSequence[index].apizooms.Add(new APIZoom
                                        {
                                            is_zoom_created = Convert.ToString(dataTable.Rows[i]["is_zoom_created"]),
                                            zoom_link = Convert.ToString(dataTable.Rows[i]["zoom_link"]),
                                            zoom_name = Convert.ToString(dataTable.Rows[i]["zoom_name"]),
                                            ID = Convert.ToInt32(dataTable.Rows[i]["ID"]),
                                            ScheduleCode = Convert.ToString(dataTable.Rows[i]["ScheduleCode"]),
                                            StartDate = Convert.ToString(dataTable.Rows[i]["StartDate"]),
                                            StartTime = Convert.ToString(dataTable.Rows[i]["StartTime"]),
                                            EndDate = Convert.ToString(dataTable.Rows[i]["EndDate"]),
                                            EndTime = Convert.ToString(dataTable.Rows[i]["EndTime"]),
                                            ScheduleTime = Convert.ToString(dataTable.Rows[i]["ScheduleTime"]),
                                            EnableSchedule = Convert.ToBoolean(dataTable.Rows[i]["EnableSchedule"])

                                        });// index++;
                                    }
                                }
                                index++;
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                #endregion
                try
                {
                    if (CourseInfo.ModuleSequence != null)
                    {
                        try
                        {
                            int index = 0;
                            for (int j = 0; j < ModuleId.Count; j++)
                            {

                                SqlParameter[] spParameters = new SqlParameter[3];
                                spParameters[0] = new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId };
                                spParameters[1] = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
                                spParameters[2] = new SqlParameter("@ModuleId", SqlDbType.Int) { Value = ModuleId[j] };

                                DataTable dataTable = await ExecuteStoredProcedure("[dbo].[GetTeamsMeetingSchedule]", spParameters);

                                if (dataTable != null)
                                {
                                    CourseInfo.ModuleSequence[index].apiteams = new List<APITeams>();
                                    for (int i = 0; i < dataTable.Rows.Count; i++)
                                    {
                                        APITeams zoom = new APITeams();
                                        CourseInfo.ModuleSequence[index].apiteams.Add(new APITeams
                                        {
                                            is_teams_created = Convert.ToString(dataTable.Rows[i]["is_teams_created"]),
                                            teams_link = Convert.ToString(dataTable.Rows[i]["teams_link"]),
                                            teams_name = Convert.ToString(dataTable.Rows[i]["teams_name"]),
                                            ID = Convert.ToInt32(dataTable.Rows[i]["ID"]),
                                            ScheduleCode = Convert.ToString(dataTable.Rows[i]["ScheduleCode"]),
                                            StartDate = Convert.ToString(dataTable.Rows[i]["StartDate"]),
                                            StartTime = Convert.ToString(dataTable.Rows[i]["StartTime"]),
                                            EndDate = Convert.ToString(dataTable.Rows[i]["EndDate"]),
                                            EndTime = Convert.ToString(dataTable.Rows[i]["EndTime"]),
                                            ScheduleTime = Convert.ToString(dataTable.Rows[i]["ScheduleTime"]),
                                            EnableSchedule = Convert.ToBoolean(dataTable.Rows[i]["EnableSchedule"])

                                        });// index++;
                                    }
                                }
                                index++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                try
                {
                    if (CourseInfo.ModuleSequence != null)
                    {
                        try
                        {
                            int index = 0;
                            for (int j = 0; j < ModuleId.Count; j++)
                            {

                                SqlParameter[] spParameters = new SqlParameter[3];
                                spParameters[0] = new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId };
                                spParameters[1] = new SqlParameter("@UserId", SqlDbType.Int) { Value = userId };
                                spParameters[2] = new SqlParameter("@ModuleId", SqlDbType.Int) { Value = ModuleId[j] };

                                DataTable dataTable = await ExecuteStoredProcedure("[dbo].[GetGsuitMeetingSchedule]", spParameters);

                                if (dataTable != null)
                                {
                                    CourseInfo.ModuleSequence[index].apigsuit = new List<APIGoogleMeet>();
                                    for (int i = 0; i < dataTable.Rows.Count; i++)
                                    {
                                        APIGoogleMeet gsuit = new APIGoogleMeet();
                                        CourseInfo.ModuleSequence[index].apigsuit.Add(new APIGoogleMeet
                                        {
                                            is_gsuit_created = Convert.ToString(dataTable.Rows[i]["is_gsuit_created"]),
                                            gsuit_link = Convert.ToString(dataTable.Rows[i]["gsuit_link"]),
                                            gsuit_name = Convert.ToString(dataTable.Rows[i]["gsuit_name"]),
                                            ID = Convert.ToInt32(dataTable.Rows[i]["ID"]),
                                            ScheduleCode = Convert.ToString(dataTable.Rows[i]["ScheduleCode"]),
                                            StartDate = Convert.ToString(dataTable.Rows[i]["StartDate"]),
                                            StartTime = Convert.ToString(dataTable.Rows[i]["StartTime"]),
                                            EndDate = Convert.ToString(dataTable.Rows[i]["EndDate"]),
                                            EndTime = Convert.ToString(dataTable.Rows[i]["EndTime"]),
                                            ScheduleTime = Convert.ToString(dataTable.Rows[i]["ScheduleTime"]),
                                            EnableSchedule = Convert.ToBoolean(dataTable.Rows[i]["EnableSchedule"])

                                        });// index++;
                                    }
                                }
                                index++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
           
            if (CourseInfo == null)
                return null;

            if (groupId != null && groupId != 0)
            {
                if (string.Equals(orgcode, _configuration["IAAOrgCode"], StringComparison.CurrentCultureIgnoreCase) && string.Equals(Status, "inprogress", StringComparison.CurrentCultureIgnoreCase))
                    CourseInfo.Status = Status;
                CourseInfo.LearningApproach = true;
            }
            return CourseInfo;
        }


        public async Task<DataTable> ExecuteStoredProcedure(string spName, SqlParameter[] sqlParameters)
        {
            DataTable dt = null;
            using (var dbContext = this._customerConnection.GetDbContext())
            {
                using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;

                    foreach (var parameter in sqlParameters)
                    {
                        cmd.Parameters.Add(new SqlParameter(parameter.ParameterName, parameter.SqlDbType) { Value = parameter.Value });
                    }

                    await dbContext.Database.OpenConnectionAsync();
                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    dt = new DataTable();
                    dt.Load(reader);
                    reader.Dispose();
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
            return dt;
        }


        public async Task<APIMyCoursesModule> GetModulesFromDb(int userId, int courseId, string orgcode)
        {
            int IsCourseApplicable = 1;
            APIMyCoursesModule CourseInfo = new APIMyCoursesModule();
            List<object> ModuleList = new List<object>();
            try
            {
                
                string ToTimeZone = _db.UserMasterDetails.Where(u => u.UserMasterId == userId).Select(u => u.TimeZone).FirstOrDefault();
               

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCourseModuleInfo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        List<APIModulesofCourses> CourseModules = new List<APIModulesofCourses>();
                        List<CourseSection> Sections = new List<CourseSection>();
                        if (dt.Rows.Count == 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return null;
                        }
                        if (dt.Rows.Count > 0)
                        {
                            string ModuleStatus = "Defaultcompleted";
                            foreach (DataRow row in dt.Rows)
                            {
                                APIModulesofCourses Modules = new APIModulesofCourses();
                                CourseInfo.CourseTitle = row["CourseTitle"].ToString();
                                CourseInfo.CourseType = row["CourseType"].ToString();
                                CourseInfo.CategoryName = row["CourseCategory"].ToString();
                                CourseInfo.Description = row["CourseDescription"].ToString();
                                CourseInfo.CourseCreditPoints = string.IsNullOrEmpty(row["CourseCreditPoints"].ToString()) ? 0 : float.Parse(row["CourseCreditPoints"].ToString());
                                CourseInfo.language = row["Courselanguage"].ToString();
                                CourseInfo.LearningApproach = string.IsNullOrEmpty(row["CourseLearningApproach"].ToString()) ? false : bool.Parse(row["CourseLearningApproach"].ToString());
                                CourseInfo.CourseCode = row["CourseCode"].ToString();
                                CourseInfo.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : Convert.ToInt32(row["CourseId"].ToString());
                                CourseInfo.CompletionPeriodDays = string.IsNullOrEmpty(row["CourseCompletionPeriodDays"].ToString()) ? 0 : Convert.ToInt32(row["CourseCompletionPeriodDays"].ToString());
                                CourseInfo.CourseFee = string.IsNullOrEmpty(row["CourseFees"].ToString()) ? 0 : float.Parse(row["CourseFees"].ToString());
                                CourseInfo.Currency = row["CourseCurrency"].ToString();
                                CourseInfo.ThumbnailPath = row["CourseThumbnailPath"].ToString();
                                CourseInfo.IsAssessment = string.IsNullOrEmpty(row["CourseIsAssessment"].ToString()) ? false : bool.Parse(row["CourseIsAssessment"].ToString());
                                CourseInfo.IsFeedback = string.IsNullOrEmpty(row["CourseIsFeedback"].ToString()) ? false : bool.Parse(row["CourseIsFeedback"].ToString());
                                CourseInfo.IsFeedbackOptional = string.IsNullOrEmpty(row["IsFeedbackOptional"].ToString()) ? false : bool.Parse(row["IsFeedbackOptional"].ToString());
                                CourseInfo.IsAssignment = string.IsNullOrEmpty(row["IsAssignment"].ToString()) ? false : bool.Parse(row["IsAssignment"].ToString());
                                CourseInfo.IsPreAssessment = string.IsNullOrEmpty(row["CourseIsPreAssessment"].ToString()) ? false : bool.Parse(row["CourseIsPreAssessment"].ToString());
                                CourseInfo.IsCertificateIssued = string.IsNullOrEmpty(row["CourseIsCertificateIssued"].ToString()) ? false : bool.Parse(row["CourseIsCertificateIssued"].ToString());
                                CourseInfo.AssessmentId = string.IsNullOrEmpty(row["CourseAssessmentId"].ToString().ToString()) ? 0 : Convert.ToInt32(row["CourseAssessmentId"].ToString().ToString());

                                CourseInfo.PreAssessmentId = string.IsNullOrEmpty(row["CoursePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["CoursePreAssessmentId"].ToString());
                                CourseInfo.FeedbackId = string.IsNullOrEmpty(row["CourseFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["CourseFeedbackId"].ToString());
                                CourseInfo.AssignmentId = string.IsNullOrEmpty(row["AssignmentId"].ToString()) ? 0 : Convert.ToInt32(row["AssignmentId"].ToString());
                                CourseInfo.AssessmentStatus = string.IsNullOrEmpty(row["CourseAssessmentStatus"].ToString()) ? null : row["CourseAssessmentStatus"].ToString();
                                CourseInfo.FeedbackStatus = string.IsNullOrEmpty(row["CourseFeedbackStatus"].ToString()) ? null : row["CourseFeedbackStatus"].ToString();
                                CourseInfo.Status = row["CourseStatus"].ToString();
                                CourseInfo.AdminName = row["AdminName"].ToString();
                                CourseInfo.AssignmentStatus = row["AssignmentStatus"].ToString();
                                CourseInfo.PreAssessmentStatus = row["PreAssessmentStatus"].ToString();
                                CourseInfo.Duration = string.IsNullOrEmpty(row["DurationInMinutes"].ToString()) ? 0 : Convert.ToInt32(row["DurationInMinutes"].ToString());
                                CourseInfo.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                                CourseInfo.CourseRatingCount = string.IsNullOrEmpty(row["CourseRatingCount"].ToString()) ? 0 : Convert.ToInt32(row["CourseRatingCount"].ToString());
                                CourseInfo.IsAdaptiveLearning = string.IsNullOrEmpty(row["IsAdaptiveLearning"].ToString()) ? false : bool.Parse(row["IsAdaptiveLearning"].ToString());
                                CourseInfo.CourseAssignedDate = Convert.ToString(row["CourseAssignedDate"].ToString());
                                CourseInfo.LastActivityDate = Convert.ToString(row["LastActivityDate"].ToString());
                                CourseInfo.IsDilinkingILT = string.IsNullOrEmpty(row["IsDilinkingILT"].ToString()) ? false : bool.Parse(row["IsDilinkingILT"].ToString());
                                CourseInfo.IsModuleHasAssFeed = string.IsNullOrEmpty(row["IsModuleHasAssFeed"].ToString()) ? false : bool.Parse(row["IsModuleHasAssFeed"].ToString());
                                CourseInfo.IsManagerEvaluation = string.IsNullOrEmpty(row["IsManagerEvaluation"].ToString()) ? false : bool.Parse(row["IsManagerEvaluation"].ToString());
                                CourseInfo.ManagerEvaluationStatus = Convert.ToString(row["ManagerEvaluationStatus"].ToString());
                                CourseInfo.IsPreRequisiteCourse = string.IsNullOrEmpty(row["PreRequisiteStatus"].ToString()) ? false : bool.Parse(row["PreRequisiteStatus"].ToString());
                                CourseInfo.ExpiryMessage = string.IsNullOrEmpty(row["ExpiryMessage"].ToString()) ? null : row["ExpiryMessage"].ToString();
                                CourseInfo.IsOJT = string.IsNullOrEmpty(row["IsOJT"].ToString()) ? false : bool.Parse(row["IsOJT"].ToString());
                                CourseInfo.OJTStatus = Convert.ToString(row["OJTStatus"].ToString());
                                CourseInfo.OJTId = string.IsNullOrEmpty(row["OJTId"].ToString()) ? 0 : Convert.ToInt32(row["OJTId"].ToString());
                                CourseInfo.isVisibleAssessmentDetails = string.IsNullOrEmpty(row["isVisibleAssessmentDetails"].ToString()) ? true : bool.Parse(row["isVisibleAssessmentDetails"].ToString());
                                CourseInfo.StartDate = Convert.ToString(row["CourseStartDate"].ToString());
                                CourseInfo.EndDate = Convert.ToString(row["CourseEndDate"].ToString());
                                if (dt.Columns.Contains("ExternalProvider"))
                                {
                                    if (!String.IsNullOrEmpty(row["ExternalProvider"].ToString()))
                                    {
                                        CourseInfo.ExternalProvider = row["ExternalProvider"].ToString();
                                        LCMS lCMS = (from Course in _db.Course
                                                     join
                          CourseModuleAssociation in _db.CourseModuleAssociation on Course.Id equals CourseModuleAssociation.CourseId
                                                     join M in _db.Module on CourseModuleAssociation.ModuleId equals M.Id
                                                     join L in _db.LCMS on M.LCMSId equals L.Id
                                                     where Course.Id == CourseInfo.CourseId && CourseModuleAssociation.ModuleId == M.Id && M.LCMSId == L.Id
                                                     select new LCMS
                                                     {
                                                         InternalName = L.InternalName
                                                     }).FirstOrDefault();



                                        CourseInfo.ExternalProviderCategory = lCMS.InternalName;
                                    }
                                }
                                if (!DBNull.Value.Equals(row["IsCourseExpired"]))
                                    CourseInfo.IsCourseExpired = Convert.ToBoolean(row["IsCourseExpired"]);
                                CourseInfo.RetrainingDate = Convert.ToString(row["RetrainingDate"].ToString());

                                APIModulesofCourses Module = new APIModulesofCourses();
                                Module.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleId"].ToString());
                                Module.IsAssessment = string.IsNullOrEmpty(row["ModuleIsAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsAssessment"].ToString());
                                Module.IsPreAssessment = string.IsNullOrEmpty(row["ModuleIsPreAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsPreAssessment"].ToString());
                                Module.IsFeedback = string.IsNullOrEmpty(row["ModuleIsFeedback"].ToString()) ? false : bool.Parse(row["ModuleIsFeedback"].ToString());
                                Module.AssessmentId = string.IsNullOrEmpty(row["ModuleAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleAssessmentId"].ToString());
                                Module.PreAssessmentId = string.IsNullOrEmpty(row["ModulePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModulePreAssessmentId"].ToString());
                                Module.FeedbackId = string.IsNullOrEmpty(row["ModuleFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleFeedbackId"].ToString());
                                Module.ModuleName = row["ModuleModuleName"].ToString();
                                Module.ModuleType = row["ModuleModuleType"].ToString();

                                if (!String.IsNullOrEmpty(row["ModulePath"].ToString()))
                                    Module.Path = Security.EncryptForUI(row["ModulePath"].ToString());
                                else
                                    Module.Path = row["ModulePath"].ToString();
                                Module.Thumbnail = row["ModuleThumbnail"].ToString();
                                Module.Description = row["ModuleDescription"].ToString();
                                Module.IsMobileCompatible = string.IsNullOrEmpty(row["ModuleIsMobileCompatible"].ToString()) ? false : bool.Parse(row["ModuleIsMobileCompatible"].ToString());
                                Module.CreditPoints = string.IsNullOrEmpty(row["ModuleCreditPoints"].ToString()) ? 0 : Convert.ToInt32(row["ModuleCreditPoints"].ToString());
                                Module.LCMSId = string.IsNullOrEmpty(row["ModuleLCMSId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleLCMSId"].ToString());
                                Module.YoutubeVideoId = row["ModuleYoutubeVideoId"].ToString();
                                Module.AssessmentStatus = string.IsNullOrEmpty(row["ModuleAssessmentStatus"].ToString()) ? null : row["ModuleAssessmentStatus"].ToString();
                                Module.PreAssessmentStatus = string.IsNullOrEmpty(row["ModulePreAssessmentStatus"].ToString()) ? null : row["ModulePreAssessmentStatus"].ToString();
                                Module.FeedbackStatus = string.IsNullOrEmpty(row["ModuleFeedbackStatus"].ToString()) ? null : row["ModuleFeedbackStatus"].ToString();
                                Module.ContentStatus = string.IsNullOrEmpty(row["ModuleContentStatus"].ToString()) ? Status.Incompleted : row["ModuleContentStatus"].ToString();
                                Module.Status = string.IsNullOrEmpty(row["ModuleStatus"].ToString()) ? Status.Incompleted : row["ModuleStatus"].ToString();
                                Module.Duration = string.IsNullOrEmpty(row["ModuleDuration"].ToString()) ? (float?)null : float.Parse(row["ModuleDuration"].ToString());
                                Module.ZipPath = Convert.ToString(row["ZipPath"]);
                                Module.MimeType = row["MimeType"].ToString();

                                Module.ActualModuleType = (from am in _db.AuthoringMaster
                                                           where am.LCMSId == Module.LCMSId
                                                           select am.ModuleType).FirstOrDefault();
                                if (dt.Columns.Contains("ExternalLCMSId"))
                                {
                                    if (!String.IsNullOrEmpty(row["ExternalLCMSId"].ToString()))
                                    {
                                        Module.ExternalLCMSId = Convert.ToString(row["ExternalLCMSId"]);
                                    }
                                }
                                        //ILT Classroom

                                        if (!DBNull.Value.Equals(row["StartDate"]))
                                    Module.StartDate = Convert.ToDateTime(Convert.ToString(row["StartDate"]));

                                if (!DBNull.Value.Equals(row["EndDate"]))
                                    Module.EndDate = Convert.ToDateTime(Convert.ToString(row["EndDate"]));

                                if (!DBNull.Value.Equals(row["RegistrationEndDate"]))
                                    Module.RegistrationEndDate = Convert.ToDateTime(Convert.ToString(row["RegistrationEndDate"]));

                                if (!DBNull.Value.Equals(row["StartTime"]) && !string.IsNullOrEmpty(Convert.ToString(row["StartTime"])))
                                    Module.StartTime = TimeSpan.Parse(Convert.ToString(row["StartTime"])).ToString(@"hh\:mm");

                                if (!DBNull.Value.Equals(row["EndTime"]) && !string.IsNullOrEmpty(Convert.ToString(row["EndTime"])))
                                    Module.EndTime = TimeSpan.Parse(Convert.ToString(row["EndTime"])).ToString(@"hh\:mm");

                              

                                Module.BatchId = string.IsNullOrEmpty(row["BatchId"].ToString()) ? 0 : Convert.ToInt32(row["BatchId"].ToString());
                                Module.BatchCode = Convert.ToString(row["BatchCode"]);
                                Module.BatchName = Convert.ToString(row["BatchName"]);
                                Module.ScheduleCode = Convert.ToString(row["ScheduleCode"]);
                                Module.ScheduleID = string.IsNullOrEmpty(row["ScheduleId"].ToString()) ? 0 : Convert.ToInt32(row["ScheduleId"].ToString());
                                Module.TrainingRequestStatus = Convert.ToString(row["TrainingRequestStatus"]);
                                Module.PlaceName = Convert.ToString(row["PlaceName"]);
                                Module.Address = Convert.ToString(row["PostalAddress"]);


                                Module.City = Convert.ToString(row["City"]);
                                Module.SectionId = string.IsNullOrEmpty(row["SectionId"].ToString()) ? (int?)null : Convert.ToInt32(row["SectionId"].ToString());
                                Module.SequenceNo = string.IsNullOrEmpty(row["SequenceNo"].ToString()) ? 0 : Convert.ToInt32(row["SequenceNo"].ToString());
                                Module.Location = row["Location"].ToString();
                                Module.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : Convert.ToInt32(row["CompletionPeriodDays"].ToString());
                                Module.FinalDate = Convert.ToString(row["FinalDate"].ToString());
                                Module.IsEnableModule = true;
                                Module.ActivityID = string.IsNullOrEmpty(row["ActivityID"].ToString()) ? null : Convert.ToString(row["ActivityID"].ToString());
                                Module.IsMultilingual = string.IsNullOrEmpty(row["IsMultilingual"].ToString()) ? false : bool.Parse(row["IsMultilingual"].ToString());
                                Module.selectedLanguageCode = string.IsNullOrEmpty(row["selectedLanguageCode"].ToString()) ? null : Convert.ToString(row["selectedLanguageCode"].ToString());
                                Module.IsEmbed = string.IsNullOrEmpty(row["IsEmbed"].ToString()) ? false : bool.Parse(row["IsEmbed"].ToString());
                                Module.AttendanceStatus = row["AttendanceStatus"].ToString();
                                Module.WAIVERStatus = row["WAIVERStatus"].ToString();                                                   
                                Module.ScheduleCreatedBy = Convert.ToInt32(row["ScheduleCreatedBy"]);

                                if (!string.IsNullOrEmpty(Module.FinalDate))
                                {
                                    DateTime finaldt = Convert.ToDateTime(row["FinalDate"].ToString());
                                    if (CourseInfo.LearningApproach == true)
                                    {
                                        if (ModuleStatus == "Defaultcompleted")
                                            Module.IsEnableModule = true;
                                        else
                                        {
                                            if (finaldt.Date <= DateTime.UtcNow.Date)
                                            {
                                                if (ModuleStatus == "completed")
                                                    Module.IsEnableModule = true;
                                                else
                                                    Module.IsEnableModule = false;
                                            }
                                            else
                                                Module.IsEnableModule = false;
                                        }
                                    }
                                    else
                                        Module.IsEnableModule = true;
                                }

                                ModuleStatus = Convert.ToString(row["ModuleStatus"]);

                                string ContentType = row["ContentType"].ToString();
                                if (!string.IsNullOrEmpty(ContentType))
                                {
                                    if (ContentType.ToLower().Contains("scorm"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        Module.ZipPath = string.Concat(DomainName, Module.ZipPath);
                                        Module.ZipPath = Security.EncryptForUI(Module.ZipPath);
                                    }
                                    else if (ContentType.ToLower().Contains("xapi"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        string endpoint = this._configuration[APIHelper.xAPIEndPoint];
                                        string basic = this._configuration[APIHelper.xAPIBasic];

                                        string UserUrl = _configuration[APIHelper.UserAPI];
                                        string NameById = "GetNameById";
                                        string ColumnName = "username";
                                        int Value = userId;
                                        HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgcode + "/" + ColumnName + "/" + Value);
                                        xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                                        if (response.IsSuccessStatusCode)
                                        {
                                            var username = await response.Content.ReadAsStringAsync();
                                            _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                                        }
                                        dynamic xAPIPatah = new JObject();
                                        dynamic account = new JObject();
                                        account.accountServiceHomePage = DomainName;
                                        account.accountName = _xAPIUserDetails.EmailId;
                                        xAPIPatah.name = new JArray(_xAPIUserDetails.Name);
                                        xAPIPatah.account = new JArray(account);

                                        Module.Path = row["ModulePath"].ToString() + "?actor=" + JsonConvert.SerializeObject(xAPIPatah) + "&endpoint=" + endpoint + "&auth=Basic " + basic + "&activity_id=" + Module.ActivityID + "&content_token=" + courseId + "-" + Module.ModuleId;

                                        Module.Path = Security.EncryptForUI(Module.Path);


                                    }
                                    else if (ContentType.ToLower().Contains("cmi5"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        string endpoint = this._configuration[APICMI5Helper.cmi5EndPoint];
                                        string fetch = this._configuration[APICMI5Helper.fetch];
                                        string token = this._configuration[APICMI5Helper.token];

                                        string UserUrl = _configuration[APIHelper.UserAPI];
                                        string NameById = "GetNameById";
                                        string ColumnName = "username";
                                        int Value = userId;
                                        HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgcode + "/" + ColumnName + "/" + Value);
                                        xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                                        if (response.IsSuccessStatusCode)
                                        {
                                            var username = await response.Content.ReadAsStringAsync();
                                            _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                                        }
                                        dynamic xAPIPatah = new JObject();
                                        dynamic account = new JObject();
                                        account.accountServiceHomePage = DomainName;
                                        account.accountName = _xAPIUserDetails.EmailId;
                                        xAPIPatah.name = new JArray(_xAPIUserDetails.Name);
                                        xAPIPatah.account = new JArray(account);

                                        Module.Path = row["ModulePath"].ToString() + "?actor=" + JsonConvert.SerializeObject(xAPIPatah) + "&endpoint=" + endpoint + "&fetch=" + fetch + token + "&activity_id=" + Module.ActivityID + "&registration=" + courseId + "-" + Module.ModuleId;

                                        Module.Path = Security.EncryptForUI(Module.Path);


                                    }

                                    try  // block added to downlod video at local server issue bookmarking
                                    {
                                        var EnableBlobStorage = await _accessibilityRule.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                                        if (Module.ModuleType == "Video" && !string.IsNullOrEmpty(orgcode) && !(Security.DecryptForUI(Module.Path)).Contains("https://vimeo.com"))
                                        {
                                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "yes")
                                            {
                                                if (!string.IsNullOrEmpty(Module.Path))
                                                {
                                                    string coursesPathExists = this._configuration["CoursePath"];
                                                    string filePathExists = string.Empty;
                                                    coursesPathExists = Path.Combine(coursesPathExists, orgcode, Record.Courses);
                                                    filePathExists = Path.Combine(coursesPathExists, FileType.Video, Path.GetFileName(Security.DecryptForUI(Module.Path)));
                                                    filePathExists = string.Concat(filePathExists.Split(' '));
                                                    if (!System.IO.File.Exists(filePathExists))
                                                    {
                                                        string filename = Path.Combine(orgcode, Record.Courses, FileType.Video, Path.GetFileName(Security.DecryptForUI(Module.Path)));
                                                        BlobDto imgres = await _azurestorage.DownloadAsync(filename);
                                                        if (imgres != null)
                                                        {
                                                            if (!string.IsNullOrEmpty(imgres.Name))
                                                            {
                                                                string filePath = string.Empty;
                                                                string coursesPath = this._configuration["CoursePath"];
                                                                coursesPath = Path.Combine(coursesPath, orgcode, Record.Courses);
                                                                if (!Directory.Exists(coursesPath))
                                                                {
                                                                    Directory.CreateDirectory(coursesPath);
                                                                }
                                                                filePath = Path.Combine(coursesPath, FileType.Video);
                                                                if (!Directory.Exists(filePath))
                                                                {
                                                                    Directory.CreateDirectory(filePath);
                                                                }
                                                                filePath = Path.Combine(coursesPath, FileType.Video, Path.GetFileName(Security.DecryptForUI(Module.Path)));
                                                                filePath = string.Concat(filePath.Split(' '));
                                                                if (!System.IO.File.Exists(filePath))
                                                                {
                                                                    using (FileStream outputFileStream = new FileStream(filePath, FileMode.Create))
                                                                    {
                                                                        imgres.Content.CopyTo(outputFileStream);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _logger.Error(imgres.ToString());
                                                               // return null;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _logger.Error("File not exists");
                                                            //return null;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                        //throw ex;
                                    }
                                
                                }

                                if (IsCourseApplicable == 0 && (CourseInfo.CourseType.ToLower() != "classroom" || CourseInfo.CourseType.ToLower() != "vilt"))
                                {
                                    Module.Path = null;
                                    Module.LCMSId = null;
                                    Module.YoutubeVideoId = null;
                                    Module.ZipPath = null;

                                }
                                CourseModules.Add(Module);

                                int? SectionId = string.IsNullOrEmpty(row["SectionId"].ToString()) ? (int?)null : Convert.ToInt32(row["SectionId"].ToString());
                                if (SectionId != null)
                                {
                                    CourseSection Section = new CourseSection();
                                    Section.SectionTitle = row["SectionTitle"].ToString();
                                    Section.SectionDescription = row["SectionDescription"].ToString();
                                    Section.SectionNumber = string.IsNullOrEmpty(row["SectionNumber"].ToString()) ? 0 : Convert.ToInt32(row["SectionNumber"].ToString());
                                    Section.SectionId = SectionId;
                                    Sections.Add(Section);
                                }

                            }
                            CourseInfo.ModuleSequence = CourseModules;


                            // Adding Modules without Sections
                            CourseInfo.Modules = CourseModules.Where(m => m.SectionId == null).ToList();

                            //CourseInfo.Modules = CourseModules;
                            //Removing Duplicate sections
                            Sections = Sections.GroupBy(s => s.SectionId).Select(g => g.FirstOrDefault()).ToList();
                            //Adding modules with section
                            foreach (CourseSection Section in Sections)
                            {
                                Section.Modules = CourseModules.Where(m => m.SectionId == Section.SectionId).ToList();
                            }
                            CourseInfo.Sections = Sections.OrderBy(s => s.SectionNumber).ToList();


                            int CompletedModuleCount = (from module in CourseModules
                                                        where module.Status.ToLower().Equals(Status.Completed)
                                                        select module.ModuleId).Count();
                            int TotalModules = CourseModules.Count();
                            CourseInfo.ContentStatus = CompletedModuleCount == TotalModules && TotalModules != 0 ? Status.Completed : Status.InProgress;
                            CourseInfo.NumberofModules = TotalModules;

                            if (CourseInfo.IsAssessment || CourseInfo.IsFeedback)
                            {
                                TotalModules = TotalModules + 1;
                                if (CourseInfo.IsAssessment && !CourseInfo.IsFeedback && CourseInfo.AssessmentStatus != null)
                                {
                                    if (CourseInfo.AssessmentStatus.Equals("completed"))
                                    {
                                        CompletedModuleCount += 1;
                                    }
                                }
                                if (!CourseInfo.IsAssessment && CourseInfo.IsFeedback && CourseInfo.FeedbackStatus != null)
                                {
                                    if (CourseInfo.FeedbackStatus.Equals("completed"))
                                    {
                                        CompletedModuleCount += 1;
                                    }
                                }
                                if (CourseInfo.IsAssessment && CourseInfo.IsFeedback && CourseInfo.FeedbackStatus != null && CourseInfo.AssessmentStatus != null)
                                {
                                    if (CourseInfo.AssessmentStatus.Equals("completed") && CourseInfo.FeedbackStatus.Equals("completed"))
                                    {
                                        CompletedModuleCount += 1;
                                    }
                                }
                            }

                            CourseInfo.ProgressPercentage = (int)(((decimal)CompletedModuleCount / TotalModules) * 100);
                            if (CourseInfo.Status == "inprogress" && CourseInfo.ProgressPercentage == 0)
                            {
                                CourseInfo.ProgressPercentage = 5;
                            }

                            var result = (from cm in _db.Course
                                          join cma in _db.CourseModuleAssociation on cm.Id equals cma.CourseId
                                          join mm in _db.Module on cma.ModuleId equals mm.Id
                                          where
                                            cm.Id == courseId && cm.IsDeleted == false && cma.Isdeleted == false
                                            && mm.IsDeleted == false &&
                                            (cm.CourseType.ToLower() == "classroom" || cm.CourseType == "vilt" ||
                                             (cm.CourseType.ToLower() == "blended" && (mm.ModuleType.ToLower() == "classroom" || mm.ModuleType.ToLower() == "vilt"))
                                            )
                                          select cm.Id);
                            if (await result.CountAsync() > 0)
                                CourseInfo.IsShowViewBatches = true;
                            else
                                CourseInfo.IsShowViewBatches = false;

                            if (!string.IsNullOrEmpty(ToTimeZone) && (CourseInfo.CourseType.ToLower() == "classroom" || CourseInfo.CourseType.ToLower() == "vilt"))
                            {
                                foreach (APIModulesofCourses item in CourseInfo.Modules)
                                {
                                    string FromTimeZone = _db.UserMasterDetails.Where(u => u.UserMasterId == item.ScheduleCreatedBy).Select(u => u.TimeZone).FirstOrDefault();

                                    if (!string.IsNullOrEmpty(FromTimeZone))
                                    {
                                        if (FromTimeZone!=ToTimeZone)
                                        {
                                            FromTimeZone = _tzList.Where(a => a.Text == FromTimeZone).Select(a => a.Value).FirstOrDefault().ToString();
                                            //  FromTimeZone = TZConvert.WindowsToIana(FromTimeZone);
                                            ToTimeZone = _tzList.Where(a => a.Text == ToTimeZone).Select(a => a.Value).FirstOrDefault().ToString();
                                            // ToTimeZone = TZConvert.WindowsToIana(ToTimeZone); 
                                            item.Tz_StartDt = TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.StartDate + TimeSpan.Parse(item.StartTime)), FromTimeZone), ToTimeZone);
                                            item.Tz_EndDt = TimeZoneHelper.getLocaltimeFromUniversal(TimeZoneHelper.ConvertLocalToUTCwithTimeZone(Convert.ToDateTime(item.EndDate + TimeSpan.Parse(item.EndTime)), FromTimeZone), ToTimeZone);

                                            item.StartDate = Convert.ToDateTime(Convert.ToString(item.Tz_StartDt));
                                            item.EndDate = Convert.ToDateTime(Convert.ToString(item.Tz_EndDt));
                                            item.StartTime = TimeSpan.Parse(Convert.ToString(Convert.ToString(item.Tz_StartDt.TimeOfDay))).ToString(@"hh\:mm");
                                            item.EndTime = TimeSpan.Parse(Convert.ToString(Convert.ToString(item.Tz_EndDt.TimeOfDay))).ToString(@"hh\:mm");
                                        }
                                    }
                                }
                            }

                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return CourseInfo;
        }



        public async Task<ApiCourseInfo> GetModuleInfo(int userId, int courseId, int? moduleId)
        {
            ApiCourseInfo CourseInfo = await GetModuleInfoFromDb(userId, courseId, moduleId);
            if (CourseInfo == null)
                return null;
            return CourseInfo;
        }
        public async Task<ApiCourseInfo> GetModuleInfoFromDb(int userId, int courseId, int? moduleId)
        {
            ApiCourseInfo CourseInfo = new ApiCourseInfo();
            List<object> ModuleList = new List<object>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCourseModuleInfo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleId", SqlDbType.Int) { Value = moduleId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        if (dt.Rows.Count == 0)
                        {
                            reader.Dispose();
                            await dbContext.Database.CloseConnectionAsync();
                            return null;
                        }
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {

                                CourseInfo.CourseTitle = row["CourseTitle"].ToString();
                                CourseInfo.CourseType = row["CourseType"].ToString();
                                CourseInfo.CategoryName = row["CourseCategory"].ToString();
                                CourseInfo.LearningApproach = string.IsNullOrEmpty(row["CourseLearningApproach"].ToString()) ? false : bool.Parse(row["CourseLearningApproach"].ToString());
                                CourseInfo.CourseCode = row["CourseCode"].ToString();
                                CourseInfo.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : Convert.ToInt32(row["CourseId"].ToString());
                                CourseInfo.ThumbnailPath = row["CourseThumbnailPath"].ToString();
                                CourseInfo.IsAssessment = string.IsNullOrEmpty(row["CourseIsAssessment"].ToString()) ? false : bool.Parse(row["CourseIsAssessment"].ToString());
                                CourseInfo.IsFeedback = string.IsNullOrEmpty(row["CourseIsFeedback"].ToString()) ? false : bool.Parse(row["CourseIsFeedback"].ToString());
                                CourseInfo.IsAssignment = string.IsNullOrEmpty(row["IsAssignment"].ToString()) ? false : bool.Parse(row["IsAssignment"].ToString());
                                CourseInfo.IsPreAssessment = string.IsNullOrEmpty(row["CourseIsPreAssessment"].ToString()) ? false : bool.Parse(row["CourseIsPreAssessment"].ToString());
                                CourseInfo.AssessmentId = string.IsNullOrEmpty(row["CourseAssessmentId"].ToString().ToString()) ? 0 : Convert.ToInt32(row["CourseAssessmentId"].ToString().ToString());
                                CourseInfo.PreAssessmentId = string.IsNullOrEmpty(row["CoursePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["CoursePreAssessmentId"].ToString());
                                CourseInfo.FeedbackId = string.IsNullOrEmpty(row["CourseFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["CourseFeedbackId"].ToString());
                                CourseInfo.AssignmentId = string.IsNullOrEmpty(row["AssignmentId"].ToString()) ? 0 : Convert.ToInt32(row["AssignmentId"].ToString());
                                CourseInfo.AssessmentStatus = string.IsNullOrEmpty(row["CourseAssessmentStatus"].ToString()) ? null : row["CourseAssessmentStatus"].ToString();
                                CourseInfo.FeedbackStatus = string.IsNullOrEmpty(row["CourseFeedbackStatus"].ToString()) ? null : row["CourseFeedbackStatus"].ToString();
                                CourseInfo.Status = row["CourseStatus"].ToString();
                                CourseInfo.PreAssessmentStatus = row["PreAssessmentStatus"].ToString();
                                CourseInfo.Duration = string.IsNullOrEmpty(row["DurationInMinutes"].ToString()) ? 0 : Convert.ToInt32(row["DurationInMinutes"].ToString());

                                APIModuleInfo Module = new APIModuleInfo();
                                Module.ModuleId = string.IsNullOrEmpty(row["ModuleId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleId"].ToString());
                                Module.IsAssessment = string.IsNullOrEmpty(row["ModuleIsAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsAssessment"].ToString());
                                Module.IsPreAssessment = string.IsNullOrEmpty(row["ModuleIsPreAssessment"].ToString()) ? false : bool.Parse(row["ModuleIsPreAssessment"].ToString());
                                Module.IsFeedback = string.IsNullOrEmpty(row["ModuleIsFeedback"].ToString()) ? false : bool.Parse(row["ModuleIsFeedback"].ToString());
                                Module.AssessmentId = string.IsNullOrEmpty(row["ModuleAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleAssessmentId"].ToString());
                                Module.PreAssessmentId = string.IsNullOrEmpty(row["ModulePreAssessmentId"].ToString()) ? 0 : Convert.ToInt32(row["ModulePreAssessmentId"].ToString());
                                Module.FeedbackId = string.IsNullOrEmpty(row["ModuleFeedbackId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleFeedbackId"].ToString());
                                Module.ModuleName = row["ModuleModuleName"].ToString();
                                Module.ModuleType = row["ModuleModuleType"].ToString();
                                Module.Path = row["ModulePath"].ToString();
                                Module.Thumbnail = row["ModuleThumbnail"].ToString();
                                Module.Description = row["ModuleDescription"].ToString();
                                Module.IsMobileCompatible = string.IsNullOrEmpty(row["ModuleIsMobileCompatible"].ToString()) ? false : bool.Parse(row["ModuleIsMobileCompatible"].ToString());
                                Module.CreditPoints = string.IsNullOrEmpty(row["ModuleCreditPoints"].ToString()) ? 0 : Convert.ToInt32(row["ModuleCreditPoints"].ToString());
                                Module.LCMSId = string.IsNullOrEmpty(row["ModuleLCMSId"].ToString()) ? 0 : Convert.ToInt32(row["ModuleLCMSId"].ToString());
                                Module.YoutubeVideoId = row["ModuleYoutubeVideoId"].ToString();
                                if (dt.Columns.Contains("ExternalLCMSId"))
                                {
                                    if (!String.IsNullOrEmpty(row["ExternalLCMSId"].ToString()))
                                    {
                                        Module.ExternalLCMSId = Convert.ToString(row["ExternalLCMSId"]);
                                    }
                                }
                                Module.AssessmentStatus = string.IsNullOrEmpty(row["ModuleAssessmentStatus"].ToString()) ? null : row["ModuleAssessmentStatus"].ToString();
                                Module.PreAssessmentStatus = string.IsNullOrEmpty(row["ModulePreAssessmentStatus"].ToString()) ? null : row["ModuleAssessmentStatus"].ToString();
                                Module.FeedbackStatus = string.IsNullOrEmpty(row["ModuleFeedbackStatus"].ToString()) ? null : row["ModuleFeedbackStatus"].ToString();
                                Module.ContentStatus = string.IsNullOrEmpty(row["ModuleContentStatus"].ToString()) ? Status.Incompleted : row["ModuleContentStatus"].ToString();
                                Module.Status = string.IsNullOrEmpty(row["ModuleStatus"].ToString()) ? Status.Incompleted : row["ModuleStatus"].ToString();
                                Module.Duration = string.IsNullOrEmpty(row["ModuleDuration"].ToString()) ? (float?)null : float.Parse(row["ModuleDuration"].ToString());
                                Module.ZipPath = Convert.ToString(row["ZipPath"]);
                                Module.MimeType = row["MimeType"].ToString();
                                //ILT Classroom
                                Module.StartDate = Convert.ToString(row["StartDate"]);
                                Module.EndDate = Convert.ToString(row["EndDate"]);
                                Module.RegistrationEndDate = Convert.ToString(row["RegistrationEndDate"]);
                                Module.StartTime = Convert.ToString(row["StartTime"]);
                                Module.EndTime = Convert.ToString(row["EndTime"]);
                                Module.ScheduleCode = Convert.ToString(row["ScheduleCode"]);
                                Module.ScheduleID = string.IsNullOrEmpty(row["ScheduleId"].ToString()) ? 0 : Convert.ToInt32(row["ScheduleId"].ToString());
                                Module.TrainingRequestStatus = Convert.ToString(row["TrainingRequestStatus"]);
                                Module.PlaceName = Convert.ToString(row["PlaceName"]);
                                Module.Address = Convert.ToString(row["PostalAddress"]);
                                Module.City = Convert.ToString(row["City"]);
                                Module.InternalName = row["InternalName"].ToString();
                                Module.SectionId = string.IsNullOrEmpty(row["SectionId"].ToString()) ? (int?)null : Convert.ToInt32(row["SectionId"].ToString());
                                Module.SequenceNo = string.IsNullOrEmpty(row["SequenceNo"].ToString()) ? 0 : Convert.ToInt32(row["SequenceNo"].ToString());
                                Module.Location = row["Location"].ToString();
                                string ContentType = row["ContentType"].ToString();
                                Module.IsEmbed = string.IsNullOrEmpty(row["IsEmbed"].ToString()) ? false : bool.Parse(row["IsEmbed"].ToString());


                                if (!string.IsNullOrEmpty(ContentType))
                                {
                                    if (ContentType.ToLower().Contains("scorm"))
                                    {
                                        string DomainName = this._configuration["EmpoweredLmsPath"];
                                        Module.ZipPath = string.Concat(DomainName, Module.ZipPath);
                                    }

                                }
                                CourseInfo.Modules = Module;


                            }

                        }
                        reader.Dispose();
                        await dbContext.Database.CloseConnectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return CourseInfo;
        }
        public String GetCourseOrModuleStatus(bool IsAssessment, bool IsFeedback, string ContentStatus, string FeedbackStatus, string AssessmentStatus)
        {
            if (IsAssessment && IsFeedback)
            {
                if (!String.IsNullOrEmpty(ContentStatus) && !String.IsNullOrEmpty(FeedbackStatus) && !String.IsNullOrEmpty(AssessmentStatus))
                    if (ContentStatus.ToLower().Equals(Status.Completed) && FeedbackStatus.ToLower().Equals(Status.Completed) && AssessmentStatus.ToLower().Equals(Status.Completed))
                        return Status.Completed;
                return Status.InProgress;
            }
            if (IsAssessment && !IsFeedback)
            {
                if (!String.IsNullOrEmpty(ContentStatus) && !String.IsNullOrEmpty(AssessmentStatus))
                    if (ContentStatus.ToLower().Equals(Status.Completed) && AssessmentStatus.ToLower().Equals(Status.Completed))
                        return Status.Completed;
                return Status.InProgress;
            }
            if (!IsAssessment && IsFeedback)
            {
                if (!String.IsNullOrEmpty(ContentStatus) && !String.IsNullOrEmpty(FeedbackStatus))
                    if (ContentStatus.ToLower().Equals(Status.Completed) && FeedbackStatus.ToLower().Equals(Status.Completed))
                        return Status.Completed;
                return Status.InProgress;
            }
            if (!IsAssessment && !IsFeedback)
            {
                if (!String.IsNullOrEmpty(ContentStatus))
                    if (ContentStatus.ToLower().Equals(Status.Completed))
                        return Status.Completed;
                return Status.InProgress;
            }
            return Status.InProgress;
        }
        public async Task<string> GetCourseInfo(int courseId)
        {
            string coursetitle = await _db.Course.Where(p => p.Id == courseId).Select(p => p.Title).FirstOrDefaultAsync();
            return coursetitle;
        }
        public async Task<bool> IsShowCatlogue(string token)
        {
            JObject oJsonObject = new JObject();
            string Url = this._configuration[Configuration.MasterApi];
            Url += "ConfigurableParameters/GetValue/COURSES_CATALOGUE_ACCESSIBILITY";
            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ApiConfigurableParameters ConfigurableParameters = JsonConvert.DeserializeObject<ApiConfigurableParameters>(result);
                if (ConfigurableParameters.Value.ToLower().Equals("yes"))
                    return true;
            }
            return false;
        }

        public async Task<List<APIMyCourses>> GetMissionCourses(int userId, int page, int pageSize, string mission = null, int? categoryId = null, string CourseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null)
        {
            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[GetMissionCourses]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@Mission", SqlDbType.VarChar) { Value = mission });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = CourseStatus });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
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
                            APIMyCourses Course = new APIMyCourses();
                            Course.RewardPoint = string.IsNullOrEmpty(row["RewardPoint"].ToString()) ? 0 : int.Parse(row["RewardPoint"].ToString());
                            Course.Title = row["Title"].ToString();
                            Course.Mission = row["Mission"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CourseCreationdDays = string.IsNullOrEmpty(row["CourseCreationdDays"].ToString()) ? 0 : int.Parse(row["CourseCreationdDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.Status = string.IsNullOrEmpty(row["CourseStatus"].ToString()) ? "NotStarted" : row["CourseStatus"].ToString();
                            Course.CourseType = row["CourseType"].ToString();
                            Course.CourseStartDate = string.IsNullOrEmpty(row["CourseStartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseStartDate"].ToString());
                            Course.CourseCompleteDate = string.IsNullOrEmpty(row["CourseCompletedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseCompletedDate"].ToString());
                            Course.CourseStartDate = Course.CourseStartDate == DateTime.MinValue ? null : Course.CourseStartDate;
                            Course.CourseCompleteDate = Course.CourseCompleteDate == DateTime.MinValue ? null : Course.CourseCompleteDate;
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : bool.Parse(row["IsAssessment"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            MyCourses.Add(Course);
                        }
                        reader.Dispose();
                    }
                }
                return MyCourses;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return null;
        }

        public async Task<int> GetMissionCourseCount(int userId, string mission = null, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "[dbo].[GetMissionCoursesCount]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@Mission", SqlDbType.VarChar) { Value = mission });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = courseStatus });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                        }
                        reader.Dispose();
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

        public async Task<int> AddEBT(EBTDetails obj)
        {
            this._db.EBTDetails.Add(obj);
            await this._db.SaveChangesAsync();
            return 1;
        }

        public async Task<int> UpdateEBT(EBTDetails obj)
        {
            this._db.EBTDetails.Update(obj);
            await this._db.SaveChangesAsync();
            return 1;
        }

        public async Task<List<APIEBTDetails>> Get()
        {
            List<APIEBTDetails> objAPIEBTDetailsList = new List<APIEBTDetails>();
            List<EBTDetails> obj = await this._db.EBTDetails.ToListAsync();
            foreach (EBTDetails a in obj)
            {
                APIEBTDetails objAPIEBTDetails = new APIEBTDetails();
                objAPIEBTDetails.FromData = JsonConvert.DeserializeObject<FromData[]>(a.FromData);
                objAPIEBTDetails.ID = a.ID;
                objAPIEBTDetails.CourseID = a.CourseID;
                objAPIEBTDetails.Status = a.Status;
                objAPIEBTDetails.courseTitle = a.courseTitle;
                objAPIEBTDetails.UserID = a.UserId;
                objAPIEBTDetails.UserName = a.UserName;

                objAPIEBTDetailsList.Add(objAPIEBTDetails);
            }
            return objAPIEBTDetailsList;
        }

        public async Task<APIEBTDetails> GetByID(int id)
        {
            EBTDetails obj = await this._db.EBTDetails.Where(a => a.ID == id).FirstOrDefaultAsync();
            APIEBTDetails objAPIEBTDetails = new APIEBTDetails();
            objAPIEBTDetails.FromData = JsonConvert.DeserializeObject<FromData[]>(obj.FromData);
            objAPIEBTDetails.ID = obj.ID;
            objAPIEBTDetails.CourseID = obj.CourseID;
            objAPIEBTDetails.Status = obj.Status;
            objAPIEBTDetails.courseTitle = obj.courseTitle;
            objAPIEBTDetails.UserID = obj.UserId;
            objAPIEBTDetails.UserName = obj.UserName;
            objAPIEBTDetails.CreatedBy = obj.CreatedBy;
            objAPIEBTDetails.CreatedDate = obj.CreatedDate;
            objAPIEBTDetails.IsActive = obj.IsActive;
            return objAPIEBTDetails;
        }

        public async Task<APIEBTDetails> GetByUserId(int userId, int courseId)
        {
            EBTDetails ebtDetails = await this._db.EBTDetails.Where(a => a.UserId == userId && a.CourseID == courseId).FirstOrDefaultAsync();
            APIEBTDetails apiEBTDetails = new APIEBTDetails();
            if (ebtDetails == null)
                return null;

            apiEBTDetails.FromData = JsonConvert.DeserializeObject<FromData[]>(ebtDetails.FromData);
            apiEBTDetails.ID = ebtDetails.ID;
            apiEBTDetails.CourseID = ebtDetails.CourseID;
            apiEBTDetails.Status = ebtDetails.Status;
            apiEBTDetails.courseTitle = ebtDetails.courseTitle;
            apiEBTDetails.UserID = ebtDetails.UserId;
            apiEBTDetails.UserName = ebtDetails.UserName;
            apiEBTDetails.CreatedBy = ebtDetails.CreatedBy;
            apiEBTDetails.CreatedDate = ebtDetails.CreatedDate;
            apiEBTDetails.IsActive = ebtDetails.IsActive;
            return apiEBTDetails;
        }

        public async Task<ApiCourseStatitics> GetCourseStatitics(int userId)
        {
            return await this._db.UserCoursesStatistics
                                .Where(a => a.UserId == userId)
                                .Select(a => new ApiCourseStatitics
                                {
                                    CompletedCourseCount = a.Completed,
                                    InprogressCourseCount = a.Inprogress,
                                    NotStartedCourseCount = a.NotStarted,
                                    LastRefreshedDate = a.LastRefreshedDate
                                })
                                .FirstOrDefaultAsync();
        }
        public async Task<APIProgressStatusCountData> GetUserProgressStatusCountDataForDeLink(int userId)
        {
            var result = await (from uCStatatcs in this._db.UserCourseStatisticsDetails
                                where uCStatatcs.UserId == userId && uCStatatcs.CourseType != 3 // for skip classroom course
                                group uCStatatcs by uCStatatcs.UserId into empGroup
                                select new ApiCourseStatitics
                                {
                                    CompletedCourseCount = empGroup.Sum(x => x.Completed),
                                    InprogressCourseCount = empGroup.Sum(x => x.Inprogress),
                                    NotStartedCourseCount = empGroup.Sum(x => x.NotStarted),
                                    LastRefreshedDate = DateTime.Now.Date
                                }).FirstOrDefaultAsync();

            int inProgressCount = 0;
            int completedCount = 0;
            int notStartedCount = 0;
            DateTime lastRefreshedDate = new DateTime();

            if (result != null)
            {
                inProgressCount = result.InprogressCourseCount;
                completedCount = result.CompletedCourseCount;
                notStartedCount = result.NotStartedCourseCount;
                lastRefreshedDate = result.LastRefreshedDate;
            }
            double total = inProgressCount + completedCount + notStartedCount;
            double inProgressCountPercentage = 0;
            double completedCountPercentage = 0;
            double notStartedCountPercentage = 0;
            if (total > 0)
            {
                inProgressCountPercentage = Math.Round(inProgressCount * 100.00 / total, 2);
                completedCountPercentage = Math.Round(completedCount * 100.00 / total, 2);
                notStartedCountPercentage = Math.Round(notStartedCount * 100.00 / total, 2);
            }
            return new APIProgressStatusCountData
            {
                InprogressCount = inProgressCount,
                CompletedCount = completedCount,
                NotStartedCount = notStartedCount,
                InprogressCountPercentage = inProgressCountPercentage,
                CompletedCountPercentage = completedCountPercentage,
                NotStartedCountPercentage = notStartedCountPercentage,
                LastRefreshedDate = lastRefreshedDate
            };


        }

        public async Task<APIProgressStatusCountData> GetUserProgressStatusCountData(int userId)
        {
            ApiCourseStatitics courseStatitics = await GetCourseStatitics(userId);
            int inProgressCount = 0;
            int completedCount = 0;
            int notStartedCount = 0;
            DateTime lastRefreshedDate = new DateTime();

            if (courseStatitics != null)
            {
                inProgressCount = courseStatitics.InprogressCourseCount;
                completedCount = courseStatitics.CompletedCourseCount;
                notStartedCount = courseStatitics.NotStartedCourseCount;
                lastRefreshedDate = courseStatitics.LastRefreshedDate;
            }
            double total = inProgressCount + completedCount + notStartedCount;
            double inProgressCountPercentage = 0;
            double completedCountPercentage = 0;
            double notStartedCountPercentage = 0;
            if (total > 0)
            {
                inProgressCountPercentage = Math.Round(inProgressCount * 100.00 / total, 2);
                completedCountPercentage = Math.Round(completedCount * 100.00 / total, 2);
                notStartedCountPercentage = Math.Round(notStartedCount * 100.00 / total, 2);
            }
            return new APIProgressStatusCountData
            {
                InprogressCount = inProgressCount,
                CompletedCount = completedCount,
                NotStartedCount = notStartedCount,
                InprogressCountPercentage = inProgressCountPercentage,
                CompletedCountPercentage = completedCountPercentage,
                NotStartedCountPercentage = notStartedCountPercentage,
                LastRefreshedDate = lastRefreshedDate
            };
        }
        public async Task<int> PostCourseStatitics(int userId)
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "RefreshCourseCountData";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
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
            return 1;
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

        public async Task<int> CheckUserApplicabilityToCourse(int userId, int CourseId, string OrgCode, int? groupId = null)
        {
            if (groupId != null && groupId != 0)
            {
                APIScormGroup aPIScormGroup = await _nodalCourseRequests.GetUserforCompletion((int)groupId);
                if (aPIScormGroup == null)
                    return 0;
                userId = aPIScormGroup.UserId;
            }

            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "VerifyUserApplicablityToCourse";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = CourseId });
                        cmd.Parameters.Add(new SqlParameter("@OrgCode", SqlDbType.VarChar) { Value = OrgCode });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        else
                        {
                            reader.Dispose();
                            return 1;
                        }
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


        public async Task<Message> EnrollCourse(int userId, int CourseId, string orgCode)
        {

            APIAccessibility apiAccessibility = new APIAccessibility();
            AccessibilityRules objsad = new AccessibilityRules
            {
                AccessibilityRule = "UserId",
                Condition = "OR",
                ParameterValue = userId.ToString(),

            };
            apiAccessibility.CourseId = CourseId;
            apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
            apiAccessibility.AccessibilityRule[0] = objsad;
            await _accessibilityRule.SelfEnroll(apiAccessibility, userId, orgCode);
            return Message.Ok;
        }
        public async Task<List<APICourseTypeahead>> SearchCourses( int userId, string search = null)
        {
            var BusinessFilter_OnSocial = await _accessibilityRule.GetMasterConfigurableParameterValue("BusinessFilter_OnSocial");
            _logger.Info("BusinessFilter_OnSocial : " + BusinessFilter_OnSocial);

            if (Convert.ToString(string.IsNullOrEmpty(BusinessFilter_OnSocial) ? "no" : BusinessFilter_OnSocial).ToLower() == "no")
            {

                var courses = (from c in _db.Course
                               where
                              ((c.Title.Contains(search) || c.Code.Contains(search) || c.Metadata.Contains(search)) && c.IsActive == true && c.IsDeleted == false && c.IsShowInCatalogue == true)
                               select new APICourseTypeahead
                               {
                                   Title = c.Title,
                                   Id = c.Id,
                                   CourseType = c.CourseType
                               }
                           ).Take(12);
                return await courses.ToListAsync();
            }
            else
            {
                UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();

                var courses = (from c in _db.Course
                               join businessdetails in this._db.UserMasterDetails on c.CreatedBy equals businessdetails.UserMasterId

                               where
                              ((c.Title.Contains(search) || c.Code.Contains(search) || c.Metadata.Contains(search)) && c.IsActive == true && c.IsDeleted == false && c.IsShowInCatalogue == true)
                               && (userdetails.BusinessId == businessdetails.BusinessId)
                               select new APICourseTypeahead
                               {
                                   Title = c.Title,
                                   Id = c.Id,
                                   CourseType = c.CourseType
                               }
                             ).Take(12);
                return await courses.ToListAsync();
            }
        }

        public async Task<List<APIMyCourses>> GetCompletedCourses(int userId, int page, int pageSize, int? categoryId = null, string CourseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null)
        {

            List<APIMyCourses> MyCourses = new List<APIMyCourses>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetUserWiseCompletedCourses";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = CourseStatus });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@page", SqlDbType.Int) { Value = page });
                        cmd.Parameters.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });
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
                            APIMyCourses Course = new APIMyCourses();
                            Course.Title = row["Title"].ToString();
                            Course.Code = row["Code"].ToString();
                            Course.CategoryName = row["CategoryName"].ToString();
                            Course.CourseFee = string.IsNullOrEmpty(row["CourseFee"].ToString()) ? 0 : float.Parse(row["CourseFee"].ToString());
                            Course.CourseId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString());
                            Course.ThumbnailPath = row["ThumbnailPath"].ToString();
                            Course.Currency = row["Currency"].ToString();
                            Course.Description = row["Description"].ToString();
                            Course.CompletionPeriodDays = string.IsNullOrEmpty(row["CompletionPeriodDays"].ToString()) ? 0 : int.Parse(row["CompletionPeriodDays"].ToString());
                            Course.CourseCreationdDays = string.IsNullOrEmpty(row["CourseCreationdDays"].ToString()) ? 0 : int.Parse(row["CourseCreationdDays"].ToString());
                            Course.CategoryId = string.IsNullOrEmpty(row["CategoryId"].ToString()) ? (int?)null : int.Parse(row["CategoryId"].ToString());
                            Course.Status = string.IsNullOrEmpty(row["CourseStatus"].ToString()) ? "NotStarted" : row["CourseStatus"].ToString();
                            Course.CourseType = row["CourseType"].ToString();
                            Course.CourseStartDate = string.IsNullOrEmpty(row["CourseStartDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseStartDate"].ToString());
                            Course.CourseCompleteDate = string.IsNullOrEmpty(row["CourseCompletedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CourseCompletedDate"].ToString());
                            Course.CourseStartDate = Course.CourseStartDate == DateTime.MinValue ? null : Course.CourseStartDate;
                            Course.CourseCompleteDate = Course.CourseCompleteDate == DateTime.MinValue ? null : Course.CourseCompleteDate;
                            Course.SubCategoryId = string.IsNullOrEmpty(row["SubCategoryId"].ToString()) ? (int?)null : int.Parse(row["SubCategoryId"].ToString());
                            Course.CourseRating = string.IsNullOrEmpty(row["CourseRating"].ToString()) ? 0 : float.Parse(row["CourseRating"].ToString());
                            Course.IsFeedback = string.IsNullOrEmpty(row["IsFeedback"].ToString()) ? false : bool.Parse(row["IsFeedback"].ToString());
                            Course.IsAssessment = string.IsNullOrEmpty(row["IsAssessment"].ToString()) ? false : bool.Parse(row["IsAssessment"].ToString());
                            Course.IsPreAssessment = string.IsNullOrEmpty(row["IsPreAssessment"].ToString()) ? false : bool.Parse(row["IsPreAssessment"].ToString());
                            Course.IsCertificateIssued = string.IsNullOrEmpty(row["IsCertificateIssued"].ToString()) ? false : bool.Parse(row["IsCertificateIssued"].ToString());
                            Course.IsCourseApplicable = true;
                            Course.CourseApprovalStatus = "Enrolled";
                            Course.AssessmentPercentage = string.IsNullOrEmpty(row["AssessmentPercentage"].ToString()) ? "--" : row["AssessmentPercentage"].ToString();
                            Course.ScheduleRequestStatus = row["ScheduleRequestStatus"].ToString();
                            Course.DurationInMinutes = int.Parse(row["DurationInMinutes"].ToString());
                            MyCourses.Add(Course);
                        }
                        reader.Dispose();
                        return MyCourses;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<APICountAndDuration> CountCompletedCourses(int userId, int? categoryId = null, string courseStatus = null, string search = null, string courseType = null, int? subCategoryId = null, string sortBy = null)
        {
            APICountAndDuration obj = new APICountAndDuration();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetUserWiseCompletedCoursesCount";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
                        cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = search });
                        cmd.Parameters.Add(new SqlParameter("@CourseStatus", SqlDbType.VarChar) { Value = courseStatus });
                        cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.VarChar) { Value = courseType });
                        cmd.Parameters.Add(new SqlParameter("@subCategoryId", SqlDbType.Int) { Value = subCategoryId });
                        cmd.Parameters.Add(new SqlParameter("@sortBy", SqlDbType.VarChar) { Value = sortBy });

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
                            obj.Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                            obj.TotalTimeDuration = string.IsNullOrEmpty(row["TotalTime"].ToString()) ? 0 : int.Parse(row["TotalTime"].ToString());
                            obj.YearDescription = row["YearDescription"].ToString();
                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return obj;
        }

        public async Task<List<APIMonthwiseCompletion>> GetMonthWiseCompletion(int UserId)
        {
            List<APIMonthwiseCompletion> objList = new List<APIMonthwiseCompletion>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetMonthWiseCompletion";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

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
                            APIMonthwiseCompletion obj = new APIMonthwiseCompletion();

                            obj.Month = row["MonthText"].ToString();
                            obj.Status = row["statusText"].ToString();
                            obj.isCompleteInCurrentMonth = row["isCompleteInCurrentMonth"].ToString();

                            objList.Add(obj);
                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); throw ex;
            }
            return objList;
        }

        public async Task<int> GetCountMonthWiseCompletion(int UserId)
        {
            int Count = 0;
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCountForMonthWiseCompletion";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return 0;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return Count;
        }

        public async Task<string> EnrollmentTypeForUser(string token)
        {
            //// CODE OPTIMIZATION COMMENT. No Need to fetch all Paramters values. Instead use GetValue/COURSE_ENROLL API.
            JObject oJsonObject = new JObject();
            string Url = this._configuration[Configuration.MasterApi];
            Url += "ConfigurableParameters/GetValue/COURSE_ENROLL";
            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ApiConfigurableParameters ConfigurableParameters = JsonConvert.DeserializeObject<ApiConfigurableParameters>(result);
                return ConfigurableParameters.Value;
            }
            return "";
        }
        public async Task<string> CheckDiLink(string token)
        {
            try
            {
                JObject oJsonObject = new JObject();
                string Url = this._configuration[Configuration.MasterApi];
                Url += "ConfigurableParameters/GetValue/DELINKING_ILT";
                HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    ApiConfigurableParameters ConfigurableParameters = JsonConvert.DeserializeObject<ApiConfigurableParameters>(result);
                    return ConfigurableParameters.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return "";
        }


        public async Task<APIxAPICompletionDetails> checkxAPICompletion(int userId, int courseId, int moduleId, string orgnizationCode)
        {
            APIxAPICompletionDetails _APIxAPICompletionDetails = new APIxAPICompletionDetails();


            string UserUrl = _configuration[APIHelper.UserAPI];
            string NameById = "GetNameById";
            string ColumnName = "username";
            int Value = userId;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
            xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
            if (response.IsSuccessStatusCode)
            {
                var username = await response.Content.ReadAsStringAsync();
                _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
            }

            var Result = await (from lcms in this._db.LCMS
                                join module in this._db.Module on lcms.Id equals module.LCMSId
                                where module.Id == moduleId
                                select new
                                {
                                    lcms.ActivityID
                                }).FirstOrDefaultAsync();

            string DomainName = this._configuration["EmpoweredLmsPath"];

            dynamic xAPIPatah = new JObject();
            dynamic account = new JObject();
            account.name = _xAPIUserDetails.EmailId;
            account.homePage = DomainName;
            xAPIPatah.objectType = "Agent";
            xAPIPatah.account = account;
            xAPIPatah.name = _xAPIUserDetails.Name;


            string url = "http://192.168.91.136/data/xAPI/activities/state?stateId=TRACKING_DATA&activityId=" + Result.ActivityID + "&agent=" + JsonConvert.SerializeObject(xAPIPatah) + "&content_token=" + courseId + "-" + moduleId;
            string jsonobject = null;
            string token = _configuration[APIHelper.xAPIBasic];
            _APIxAPICompletionDetails = await ApiHelper.checkxAPICompletion(url, jsonobject, token);


            return _APIxAPICompletionDetails;

        }


        public async Task<APICareerJobRoles> PostCareerJobRoles(APICareerJobRoles apiCareerJobRoles, int UserId)
        {
            foreach (int jobroleid in apiCareerJobRoles.JobRoleId)
            {
                var checkForDuplicate = (from x in _db.CareerJobRoles.Where(x => x.JobRoleId == jobroleid && x.UserId == UserId)
                                         select x).ToList();
                if (checkForDuplicate.Count > 0)
                {
                    CareerJobRoles CareerJobRolesdeleted = (from x in _db.CareerJobRoles.Where(x => x.JobRoleId == jobroleid && x.UserId == UserId && x.IsDeleted == true)
                                                            select x).FirstOrDefault();

                    if (CareerJobRolesdeleted != null)
                    {
                        CareerJobRolesdeleted.IsDeleted = false;
                        CareerJobRolesdeleted.ModifiedBy = UserId;
                        CareerJobRolesdeleted.ModifiedDate = DateTime.UtcNow;
                        this._db.CareerJobRoles.Update(CareerJobRolesdeleted);
                        await this._db.SaveChangesAsync();
                    }

                    return null;
                }
                else
                {
                    CareerJobRoles careerJobRoles = new CareerJobRoles();
                    careerJobRoles.JobRoleId = jobroleid;
                    careerJobRoles.UserId = UserId;
                    careerJobRoles.IsDeleted = false;
                    careerJobRoles.IsActive = true;
                    careerJobRoles.CreatedBy = UserId;
                    careerJobRoles.CreatedDate = DateTime.UtcNow;

                    await this._db.CareerJobRoles.AddAsync(careerJobRoles);
                    await this._db.SaveChangesAsync();

                }
            }

            return apiCareerJobRoles;
        }
        public async Task<List<TypeAhead>> GetTypeAHead(string search = null)
        {

            IQueryable<TypeAhead> JobRoles = (from c in this._db.CompetencyJobRole
                                              where (search == null || c.Name.Contains(search)) && c.IsDeleted == false
                                              orderby c.Name
                                              select new TypeAhead
                                              {
                                                  Id = Convert.ToInt32(c.Id), //int? to int issue
                                                  Title = c.Name
                                              });

            return await JobRoles.ToListAsync();

        }

        public async Task<List<APICareerRoles>> GetCareerJobRoles(int UserId)
        {




            List<APINextJobRoles> blank = new List<APINextJobRoles>();

            List<APICareerRoles> JobRoles = await (from c in this._db.CareerJobRoles
                                                   join competencyjobrole in _db.CompetencyJobRole on c.JobRoleId equals competencyjobrole.Id
                                                   // join a in tmpArray on c.JobRoleId equals a.Key into countdata from a in countdata.DefaultIfEmpty()
                                                   where c.IsDeleted == false && c.UserId == UserId //&& competency.IsDeleted == false
                                                   select new APICareerRoles
                                                   {
                                                       JobRoleId = c.JobRoleId,
                                                       JobRoleName = competencyjobrole.Name,
                                                       Id = Convert.ToInt32(c.Id),
                                                   }).ToListAsync();

            foreach (APICareerRoles JobRole in JobRoles)
            {
                var skillcount = await (from competencyjobrole in _db.CompetencyJobRole
                                        join competency in _db.RoleCompetency on competencyjobrole.Id equals competency.JobRoleId
                                        join competencymaster in _db.CompetenciesMaster on competency.CompetencyId equals competencymaster.Id
                                        where competencymaster.IsDeleted == false && competency.IsDeleted == false && competencyjobrole.Id == JobRole.JobRoleId && competencyjobrole.IsDeleted == false
                                        select new
                                        {
                                            competency.Id
                                        }).CountAsync();

                JobRole.SkillCount = skillcount;
            }

            return JobRoles;

        }

        public async Task<APICareerRoles> GetUserJobRoleByUserId(int UserId)
        {
            APICareerRoles careerRoles = new APICareerRoles();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetUserCurrentJobRoleByUserId";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

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
                            careerRoles.JobRoleName = row["Name"].ToString();
                            careerRoles.JobRoleId = Convert.ToInt32(row["Id"]);
                        }
                        reader.Dispose();

                        List<APINextJobRoles> NextJobRoles = (from
                                                               nextcompetencyjobrole in _db.NextJobRoles
                                                              join competencyjobrole in _db.CompetencyJobRole on nextcompetencyjobrole.NextJobRoleId equals competencyjobrole.Id
                                                              where nextcompetencyjobrole.IsDeleted == false && nextcompetencyjobrole.JobRoleId == careerRoles.JobRoleId && competencyjobrole.IsDeleted == false

                                                              select new APINextJobRoles
                                                              {
                                                                  JobRoleId = nextcompetencyjobrole.NextJobRoleId,
                                                                  JobRoleName = competencyjobrole.Name,
                                                                  NumberOfPositions = competencyjobrole.NumberOfPositions
                                                              }).ToList();


                        var skillcount = await (from competencyjobrole in _db.CompetencyJobRole
                                                join competency in _db.RoleCompetency on competencyjobrole.Id equals competency.JobRoleId
                                                join competencymaster in _db.CompetenciesMaster on competency.CompetencyId equals competencymaster.Id
                                                where competencymaster.IsDeleted == false && competency.IsDeleted == false && competencyjobrole.Id == careerRoles.JobRoleId && competencyjobrole.IsDeleted == false
                                                select new
                                                {
                                                    competency.Id
                                                }).CountAsync();

                        careerRoles.SkillCount = skillcount;
                        careerRoles.NextJobRoles = NextJobRoles.ToArray();

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
            return careerRoles;

        }

        public async Task<APICompetencySkillSet> GetUserCurrentJobRoleCompetencies(int UserId, int? JobRoleID)
        {
            APICompetencySkillSet competencySkillSet = new APICompetencySkillSet();
            string[] performancerating = { "Good", "Average", "Poor" };

            Random rnd = new Random();

            if (JobRoleID == null)
            {
                APICareerRoles careerRoles = new APICareerRoles();
                try
                {
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            cmd.CommandText = "GetUserCurrentJobRoleByUserId";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

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
                                careerRoles.JobRoleName = row["Name"].ToString();
                                careerRoles.JobRoleId = Convert.ToInt32(row["Id"]);
                            }
                            reader.Dispose();

                            List<APICompetencySkillName> CompetencySkillSetList1 = (from competency in _db.RoleCompetency
                                                                                    join competencymaster in _db.CompetenciesMaster on competency.CompetencyId equals competencymaster.Id
                                                                                    where competencymaster.IsDeleted == false && competency.IsDeleted == false && competency.JobRoleId == careerRoles.JobRoleId && competency.IsDeleted == false
                                                                                    select new APICompetencySkillName
                                                                                    {
                                                                                        CompetencySkill = competencymaster.CompetencyName,
                                                                                        CompetencySkillId = competencymaster.Id,
                                                                                        PerformanceRating = (string)performancerating[rnd.Next(performancerating.Count())]

                                                                                    }).ToList();


                            competencySkillSet.CompetencySkill = CompetencySkillSetList1.ToArray();

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    throw;
                }
            }
            else
            {

                List<APICompetencySkillName> CompetencySkillSetList1 = (from competency in _db.RoleCompetency
                                                                        join competencymaster in _db.CompetenciesMaster on competency.CompetencyId equals competencymaster.Id
                                                                        where competencymaster.IsDeleted == false && competency.IsDeleted == false && competency.JobRoleId == JobRoleID && competency.IsDeleted == false
                                                                        select new APICompetencySkillName
                                                                        {
                                                                            CompetencySkill = competencymaster.CompetencyName,
                                                                            CompetencySkillId = competencymaster.Id,
                                                                            PerformanceRating = (string)performancerating[rnd.Next(performancerating.Count())]

                                                                        }).ToList();


                competencySkillSet.CompetencySkill = CompetencySkillSetList1.ToArray();


            }
            return competencySkillSet;

        }
        public async Task<APICompetencySkillNameV4> GetUserCurrentJobRoleCompetenciesV2(string Id,int Userid,int? JobRoleID = null)
        {
            APICompetencySkillNameV4 competencySkillSet = new APICompetencySkillNameV4();

            UserMaster userMaster = _db.UserMaster.Where(a => a.UserId == Id).FirstOrDefault();

            string[] performancerating = { "Good", "Average", "Poor" };

            Random rnd = new Random();

            if (JobRoleID == null)
            {
                APICareerRoles careerRoles = new APICareerRoles();
                try
                {
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            cmd.CommandText = "GetUserCurrentJobRoleByUserId";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userMaster.Id });

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
                                careerRoles.JobRoleName = row["Name"].ToString();
                                careerRoles.JobRoleId = Convert.ToInt32(row["Id"]);
                            }
                            reader.Dispose();

                            List<APICompetencySkillNameV3> CompetencySkillSetList1 = (from competency in _db.RoleCompetency
                                                                                    join competencymaster in _db.CompetenciesMaster on competency.CompetencyId equals competencymaster.Id
                                                                                    where competencymaster.IsDeleted == false && competency.IsDeleted == false && competency.JobRoleId == careerRoles.JobRoleId && competency.IsDeleted == false
                                                                                    select new APICompetencySkillNameV3
                                                                                    { 
                                                                                        CompetencySkill = competencymaster.CompetencyName,
                                                                                        CompetencySkillId = competencymaster.Id,
                                                                                        Level = competency.CompetencyLevelId

                                                                                    }).ToList();
                            if (CompetencySkillSetList1 != null)
                            {
                                var i = 0;
                                foreach (APICompetencySkillNameV3 aPICompetencySkillNameV3 in CompetencySkillSetList1)
                                {

                                    aPICompetencySkillNameV3.Id = i;
                                    CompetencyReviewParametersAssessment competencyReviewParametersResult = _db.CompetencyReviewParametersResult.Where(
                                        a => a.CompetencyId == aPICompetencySkillNameV3.CompetencySkillId && a.UserId == userMaster.Id
                                        && a.IsActive == true && a.IsDeleted == false
                                        ).FirstOrDefault();

                                    if (competencyReviewParametersResult != null)
                                    {
                                        aPICompetencySkillNameV3.IsSelfAssessmentGiven = true;
                                    }
                                    else
                                    {
                                        aPICompetencySkillNameV3.IsSelfAssessmentGiven = false;
                                    }

                                    CompetencyLevels aPICompetencyLevelsV2 = _db.CompetencyLevels.Where(a => a.Id == aPICompetencySkillNameV3.Level).FirstOrDefault();
                                    if (aPICompetencyLevelsV2 != null)
                                    {
                                        //string[] ssize = aPICompetencyLevelsV2.LevelName.Split(new char[0]);
                                        //aPICompetencySkillNameV3.LevelName = ssize[1];
                                        aPICompetencySkillNameV3.LevelName = aPICompetencyLevelsV2.LevelName;
                                    }
                                    else
                                    {
                                        aPICompetencySkillNameV3.LevelName = null;
                                    }
                                    i++;
                                }
                                competencySkillSet.CompetencySkill = CompetencySkillSetList1.ToArray();
                            }

                             

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    throw;
                }
            }
            else
            {

                List<APICompetencySkillNameV3> CompetencySkillSetList1 = (from competency in _db.RoleCompetency
                                                                        join competencymaster in _db.CompetenciesMaster on competency.CompetencyId equals competencymaster.Id
                                                                        where competencymaster.IsDeleted == false && competency.IsDeleted == false && competency.JobRoleId == JobRoleID && competency.IsDeleted == false
                                                                        select new APICompetencySkillNameV3
                                                                        {
                                                                            CompetencySkill = competencymaster.CompetencyName,
                                                                            CompetencySkillId = competencymaster.Id,
                                                                            Level = competency.CompetencyLevelId

                                                                        }).ToList();
                if(CompetencySkillSetList1!= null)
                {
                    var i = 0;
                    foreach (APICompetencySkillNameV3 aPICompetencySkillNameV3 in CompetencySkillSetList1)
                    {
                        aPICompetencySkillNameV3.Id = i;
                        CompetencyReviewParametersAssessment competencyReviewParametersResult = _db.CompetencyReviewParametersResult.Where(
                            a => a.CompetencyId == aPICompetencySkillNameV3.CompetencySkillId && a.UserId == userMaster.Id
                            && a.IsActive == true && a.IsDeleted == false
                            ).FirstOrDefault();

                        if (competencyReviewParametersResult != null)
                        {
                            aPICompetencySkillNameV3.IsSelfAssessmentGiven = true;
                        }
                        else
                        {
                            aPICompetencySkillNameV3.IsSelfAssessmentGiven = false;
                        }


                        CompetencyLevels aPICompetencyLevelsV2 = _db.CompetencyLevels.Where(a => a.Id == aPICompetencySkillNameV3.Level).FirstOrDefault();
                        if (aPICompetencyLevelsV2 != null)
                        {
                            //string[] ssize = aPICompetencyLevelsV2.LevelName.Split(new char[0]);
                            //aPICompetencySkillNameV3.LevelName = ssize[1];
                            aPICompetencySkillNameV3.LevelName = aPICompetencyLevelsV2.LevelName;
                        }
                        else
                        {
                            aPICompetencySkillNameV3.LevelName = null;
                        }
                        i++;

                    }

                   
                    competencySkillSet.CompetencySkill = CompetencySkillSetList1.ToArray();
                }
                
            }
            return competencySkillSet;

        }

        public async Task<List<MultiLangualContentInfo>> GetMultiLangualModules(int courseId, string token, int moduleId, string language = null)
        {
            if (language != null)
                language = language.ToLower().Equals("null") ? null : language;

            List<ApiGetSelectedLanguage> getSelectedLanguage = new List<ApiGetSelectedLanguage>();

            string Url = this._configuration[Configuration.MasterApi];
            Url += "Customer/GetSelectedLanguages";
            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, token);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                getSelectedLanguage = JsonConvert.DeserializeObject<List<ApiGetSelectedLanguage>>(result);

            }

            List<MultiLangualContentInfo> LangualModuleInfo = (from c in this._db.ModuleLcmsAssociation
                                                               join lcms in _db.LCMS on c.LCMSId equals lcms.Id
                                                               join CMA in _db.CourseModuleAssociation on c.ModuleId equals CMA.ModuleId
                                                               join module in _db.Module on c.ModuleId equals module.Id
                                                               where c.IsDeleted == false && c.ModuleId == moduleId && CMA.CourseId == courseId &&
                                                               (language == null || lcms.Language.StartsWith(language))
                                                               select new MultiLangualContentInfo
                                                               {
                                                                   ContentPath = lcms.Path,
                                                                   Language = lcms.Language,
                                                                   InternalName = lcms.InternalName,
                                                                   MimeType = lcms.MimeType,
                                                                   ModuleType = module.ModuleType,
                                                                   YoutubeVideoId = lcms.YoutubeVideoId
                                                               }).ToList();

            foreach (MultiLangualContentInfo item in LangualModuleInfo)
            {
                item.LanguageDisplay = getSelectedLanguage.Where(c => c.code.ToLower() == item.Language.ToLower()).Select(c => c.name).FirstOrDefault();
            }



            if (LangualModuleInfo == null)
                return null;


            return LangualModuleInfo;
        }

        public async Task<List<APINextJobRoles>> ViewPositions(int UserId)
        {
            List<APINextJobRoles> ViewPositions = (from competencyjobrole in _db.CompetencyJobRole
                                                   where competencyjobrole.IsDeleted == false && competencyjobrole.NumberOfPositions > 0

                                                   select new APINextJobRoles
                                                   {
                                                       JobRoleId = Convert.ToInt32(competencyjobrole.Id),
                                                       JobRoleName = competencyjobrole.Name,
                                                       NumberOfPositions = competencyjobrole.NumberOfPositions
                                                   }).ToList();


            return ViewPositions;

        }

        public async Task<List<MultiLangualContentInfo>> addUserPrefferedCourseLanguage(UserPrefferedCourseLanguage obj, string token)
        {
            try
            {
                UserPrefferedCourseLanguage existingvalue = _db.UserPrefferedCourseLanguage.Where(a => a.CourseId == obj.CourseId && a.ModuleId == obj.ModuleId && a.UserID == obj.UserID).FirstOrDefault();
                if (existingvalue != null)
                {
                    existingvalue.LanguageCode = obj.LanguageCode;
                    existingvalue.ModifiedDate = DateTime.UtcNow;
                    existingvalue.ModifiedBy = existingvalue.CreatedBy;
                    _db.UserPrefferedCourseLanguage.Update(existingvalue);
                    await this._db.SaveChangesAsync();

                    List<MultiLangualContentInfo> MultiLangualContentInfo = await this.GetMultiLangualModules(Convert.ToInt32(obj.CourseId), token, obj.ModuleId, obj.LanguageCode);

                    return MultiLangualContentInfo.ToList();
                }
                else
                {
                    _db.UserPrefferedCourseLanguage.Add(obj);
                    await this._db.SaveChangesAsync();
                    List<MultiLangualContentInfo> MultiLangualContentInfo = await this.GetMultiLangualModules(Convert.ToInt32(obj.CourseId), token, obj.ModuleId, obj.LanguageCode);

                    return MultiLangualContentInfo.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<string> GetUserPrefferedLanguage(int UserId, int courseId, int moduleId)
        {
            try
            {
                UserPrefferedCourseLanguage existingvalue = _db.UserPrefferedCourseLanguage.Where(a => a.CourseId == courseId && a.ModuleId == moduleId && a.UserID == UserId).FirstOrDefault();

                if (existingvalue != null)
                {
                    return existingvalue.LanguageCode;
                }
                else
                {
                    string LanguageCode = null;
                    using (var dbContext = this._customerConnection.GetDbContext())
                    {
                        using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            cmd.CommandText = "GetUserMypreferenceLanguage";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

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
                                LanguageCode = row["LanguageCode"].ToString();
                            }
                            reader.Dispose();

                            return LanguageCode;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }


        public async Task<bool?> GetIsmoduleMultilingual(int moduleId)
        {
            try
            {
                Module objmodule = _db.Module.Where(a => a.Id == moduleId).FirstOrDefault();

                if (objmodule != null)
                {
                    return objmodule.IsMultilingual;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }


        public async Task<bool> ManagerUserRelated(string userId, int managerId)
        {
            try
            {
                int count = (from u in _db.UserMaster
                             join umd in _db.UserMasterDetails on u.Id equals umd.UserMasterId
                             where u.IsDeleted == false && u.UserId == userId && umd.ReportsTo == (from u in _db.UserMaster where u.Id == managerId select u).FirstOrDefault().EmailId
                             select u).Count();
                if (count == 0)
                {

                    var userReportsTo = (from u in _db.UserMaster
                                         join umd in _db.UserMasterDetails on u.Id equals umd.UserMasterId
                                         where u.UserId == userId
                                         select umd.ReportsTo).Single();

                    var userReportsToId = (from u in _db.UserMaster
                                           where u.EmailId == userReportsTo
                                           select u.Id).Single();

                    var userReportsToL2 = (from u in _db.UserMaster
                                           join umd in _db.UserMasterDetails on u.Id equals umd.UserMasterId
                                           where u.Id == userReportsToId
                                           select umd.ReportsTo).Single();

                    var userReportsToL2Id = (from u in _db.UserMaster
                                             where u.EmailId == userReportsToL2
                                             select u.Id).Single();
                    if (userReportsToL2Id == managerId)
                    {
                        return true;
                    }

                }

                if (count != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }


        public async Task<IEnumerable<APIMyTeamCompetencyMasterCourse>> GetCompetencyMasterCourse(APIUserCourseDetails obj)
        {
            IEnumerable<APIMyTeamCompetencyMasterCourse> CompetencyMasterCourse = new List<APIMyTeamCompetencyMasterCourse>();
            try
            {

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@EncryptedUserID", obj.UserId);


                        IEnumerable<APIMyTeamCompetencyMasterCourse> Result = await SqlMapper.QueryAsync<APIMyTeamCompetencyMasterCourse>((SqlConnection)connection, "dbo.GetMyTeamCompetencyMasterCourse", parameters, null, null, CommandType.StoredProcedure);
                        CompetencyMasterCourse = Result.ToList();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return CompetencyMasterCourse;
        }


        public async Task<APIManagerEvaluationData> GetCompetencyMasterCourse(int  userId, APIGetManagerEvaluationCourses obj)
        {
            APIManagerEvaluationData data = new APIManagerEvaluationData();
            List<APIMyTeamCompetencyMasterCourse> CompetencyMasterCourse = new List<APIMyTeamCompetencyMasterCourse>();
            int TotalRecords = 0;
            try
            {

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserID", userId);
                        parameters.Add("@page", obj.page);
                        parameters.Add("@pageSize", obj.pageSize);


                        var Result = await SqlMapper.QueryAsync<APIMyTeamCompetencyMasterCourse>((SqlConnection)connection, "dbo.GetManagerEvaluationData", parameters, null, null, CommandType.StoredProcedure);
                        CompetencyMasterCourse = Result.ToList();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            try
            {

                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserID", userId);
                        parameters.Add("@page", obj.page);
                        parameters.Add("@pageSize", 0);


                        var Result = await SqlMapper.QueryAsync<int>((SqlConnection)connection, "[dbo].[GetManagerEvaluationData]", parameters, null, null, CommandType.StoredProcedure);
                        TotalRecords = Result.FirstOrDefault();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            data.data = CompetencyMasterCourse;
            data.TotalRecords = TotalRecords;
            return data;
        }
        public async Task<APICoursesDuration> GetCoursesDuration(int userId)
        {
            APICoursesDuration aPICoursesDuration = new APICoursesDuration();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCoursesDuration";
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
                            aPICoursesDuration.UserAssignedHours = string.IsNullOrEmpty(row["TotalTime1"].ToString()) ? 0 : decimal.Parse(row["TotalTime1"].ToString());
                            aPICoursesDuration.UserTimeSpent = string.IsNullOrEmpty(row["TotalTime"].ToString()) ? 0 : decimal.Parse(row["TotalTime"].ToString());

                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return aPICoursesDuration;
        }
        public async Task<APICoursesDuration> GetCoursesDurationByDate(int userId, CoursesDuration coursesDuration)
        {
            APICoursesDuration aPICoursesDuration = new APICoursesDuration();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCoursesDurationByDate";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = coursesDuration.StartDate });
                        cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = coursesDuration.EndDate });

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
                            aPICoursesDuration.UserAssignedHours = string.IsNullOrEmpty(row["TotalTime1"].ToString()) ? 0 : decimal.Parse(row["TotalTime1"].ToString());
                            aPICoursesDuration.UserTimeSpent = string.IsNullOrEmpty(row["TotalTime"].ToString()) ? 0 : decimal.Parse(row["TotalTime"].ToString());

                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return aPICoursesDuration;
        }

        public async Task<List<TnaEmployeeData>> GetCourseDetails(int UserId, string OrgCode, TnaCategories tnaCategory)
        {
            try
            {
                if (OrgCode == "sbil")
                {
                    List<TnaEmployeeData> tnaEmployeeData1 = new List<TnaEmployeeData>();
                    UserMasterDetails userMasterDetails = await _db.UserMasterDetails.Where(a => a.UserMasterId == UserId).FirstOrDefaultAsync();
                    if (tnaCategory.CategoryType == "0")
                    {
                        //var obj = (from ted in _db.TnaEmployeeData
                        //           join config in _db.Configure2 on ted.Grade equals config.Name
                        //           join umd in _db.UserMasterDetails on config.Id equals umd.ConfigurationColumn2
                        //           join u in _db.UserMaster on umd.UserMasterId equals u.Id
                        //           where u.Id == UserId
                        //           select ted);


                        //List<TnaEmployeeData> tnaEmployeeData = obj.ToListAsync().Result;
                        //return tnaEmployeeData;

                        if (userMasterDetails != null)
                        {
                            Business business = await _db.Business.Where(a => a.Id == userMasterDetails.BusinessId).FirstOrDefaultAsync();
                            Group group = await _db.Group.Where(a => a.Id == userMasterDetails.GroupId).FirstOrDefaultAsync();

                            List<TnaEmployeeData> tnaEmployeeData2 = _db.TnaEmployeeData.Where(
                                         a => a.CategoryType.ToLower() == "behavioral skills"
                                         ).ToList();

                            if (business != null && group != null)
                            {
                                if (business.Name.ToLower() == "marketing")
                                {

                                    tnaEmployeeData1 = _db.TnaEmployeeData.Where(
                                         a => a.DepartmentName.ToLower() == business.Name.ToLower() &&
                                         a.SubDepartmentName.ToLower() == group.Name.ToLower()
                                         ).ToList();

                                }
                                else if (business.Name.ToLower() != "marketing")
                                {
                                    tnaEmployeeData1 = _db.TnaEmployeeData.Where(
                                         a => a.DepartmentName.ToLower() == business.Name.ToLower()
                                         ).ToList();
                                }
                                for (int i = 0; i < tnaEmployeeData2.Count; i++)
                                {
                                    tnaEmployeeData1.Add(tnaEmployeeData2[i]);
                                }
                                return tnaEmployeeData1;
                            }
                            else if (business != null && group == null)
                            {
                                if (business.Name.ToLower() != "marketing")
                                {
                                    tnaEmployeeData1 = _db.TnaEmployeeData.Where(
                                        a => a.DepartmentName.ToLower() == business.Name.ToLower()
                                        ).ToList();
                                    for (int i = 0; i < tnaEmployeeData2.Count; i++)
                                    {
                                        tnaEmployeeData1.Add(tnaEmployeeData2[i]);
                                    }
                                    return tnaEmployeeData1;
                                }
                            }
                            return tnaEmployeeData2;
                        }
                        return tnaEmployeeData1;
                    }
                    else
                    {
                        if (userMasterDetails != null)
                        {
                            Business business = await _db.Business.Where(a => a.Id == userMasterDetails.BusinessId).FirstOrDefaultAsync();
                            Group group = await _db.Group.Where(a => a.Id == userMasterDetails.GroupId).FirstOrDefaultAsync();

                            if (tnaCategory.CategoryType.ToLower() == "behavioral skills")
                            {
                                tnaEmployeeData1 = _db.TnaEmployeeData.Where(
                                        a => a.CategoryType.ToLower() == tnaCategory.CategoryType
                                        ).ToList();
                                return tnaEmployeeData1;
                            }
                            else
                            {
                                if (business != null && group != null && business.Name.ToLower() == "marketing")
                                {
                                    tnaEmployeeData1 = _db.TnaEmployeeData.Where(
                                         a => a.DepartmentName.ToLower() == business.Name.ToLower() &&
                                         a.SubDepartmentName.ToLower() == group.Name.ToLower() &&
                                         a.CategoryType.ToLower() == tnaCategory.CategoryType
                                         ).ToList();
                                    return tnaEmployeeData1;
                                }
                                else if (business != null && group != null && business.Name.ToLower() != "marketing")
                                {
                                    tnaEmployeeData1 = _db.TnaEmployeeData.Where(
                                        a => a.DepartmentName.ToLower() == business.Name.ToLower() &&
                                        a.CategoryType.ToLower() == tnaCategory.CategoryType
                                        ).ToList();
                                    return tnaEmployeeData1;
                                }
                                else if (business != null && group == null && business.Name.ToLower() != "marketing")
                                {
                                    tnaEmployeeData1 = _db.TnaEmployeeData.Where(
                                        a => a.DepartmentName.ToLower() == business.Name.ToLower() &&
                                        a.CategoryType.ToLower() == tnaCategory.CategoryType
                                        ).ToList();
                                    return tnaEmployeeData1;
                                }
                            }
                            return tnaEmployeeData1;
                        }
                        //var obj = (from ted in _db.TnaEmployeeData
                        //           join config in _db.Configure2 on ted.Grade equals config.Name
                        //           join umd in _db.UserMasterDetails on config.Id equals umd.ConfigurationColumn2
                        //           join u in _db.UserMaster on umd.UserMasterId equals u.Id
                        //           where u.Id == UserId && ted.CategoryType == tnaCategory.CategoryType
                        //           select ted);

                        //List<TnaEmployeeData> tnaEmployeeData = obj.ToListAsync().Result;
                        return tnaEmployeeData1;
                    }
                }
                else
                {
                    if (tnaCategory.CategoryType == "0")
                    {
                        var obj = (from ted in _db.TnaEmployeeData
                                   select ted);

                        List<TnaEmployeeData> tnaEmployeeData = obj.ToListAsync().Result;
                        return tnaEmployeeData;
                    }
                    else
                    {
                        var obj = (from ted in _db.TnaEmployeeData
                                   where ted.CategoryType == tnaCategory.CategoryType
                                   select ted);

                        List<TnaEmployeeData> tnaEmployeeData = obj.ToListAsync().Result;
                        return tnaEmployeeData;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<Message> PostTnaEmployeeRequest(int UserId, List<TnaEmployeeNominateRequestPayload> tnaEmployeeNominateRequestPayload)
        {
            try
            {
                foreach (TnaEmployeeNominateRequestPayload request in tnaEmployeeNominateRequestPayload)
                {
                    TnaNominateRequestData oldData = _db.TnaNominateRequestData.Where(a => a.UserId == UserId && a.TnaEmployeeDataId == request.Id).FirstOrDefault();

                    if (oldData == null)
                    {
                        TnaNominateRequestData TnaEmployeeNominateRequestData = new TnaNominateRequestData();
                        TnaEmployeeNominateRequestData.UserId = UserId;
                        TnaEmployeeNominateRequestData.TnaEmployeeDataId = request.Id;
                        TnaEmployeeNominateRequestData.RequestStatus = "Requested";
                        TnaEmployeeNominateRequestData.IsDeleted = false;
                        TnaEmployeeNominateRequestData.IsActive = true;
                        TnaEmployeeNominateRequestData.CreatedBy = UserId;
                        TnaEmployeeNominateRequestData.CreatedDate = DateTime.UtcNow;
                        _db.TnaNominateRequestData.Add(TnaEmployeeNominateRequestData);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        oldData.ModifiedBy = UserId;
                        oldData.ModifiedDate = DateTime.UtcNow;
                        _db.TnaNominateRequestData.Update(oldData);
                        await this._db.SaveChangesAsync();
                    }
                }

                return Message.Ok;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        public async Task<int> GetUserIdByEmailId(string ConnectionString, string userId=null)
        {
            int Id = 0;
            try { 
            using (CourseContext dbcontext = _customerConnection.GetDbContext(ConnectionString))
            {
                var user = (from c in dbcontext.UserMaster
                            where c.IsActive == true && c.IsDeleted == false
                            && (c.UserId == Security.Encrypt(userId.ToLower()) && !string.IsNullOrEmpty(userId))
                                                  
                            select new 
                            {
                             Id= c.Id                               
                            }).AsNoTracking();
                var userlist = await user.FirstOrDefaultAsync();
               if(userlist!=null)
                Id = userlist.Id;
            }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

            return Id;
        }
        public async Task<int> GetCourseIdByCourseCode(string ConnectionString, string code = null)
        {
            int Id = 0;
            using (CourseContext dbcontext = _customerConnection.GetDbContext(ConnectionString))
            {
                var course = (from c in dbcontext.Course
                            where  c.IsDeleted == false
                            && (c.Code ==code) && !string.IsNullOrEmpty(code)
                            select new 
                            {
                                c.Id
                            }).AsNoTracking();
                var courselist = await course.FirstOrDefaultAsync();
                if (courselist!=null)
                    Id = courselist.Id;
               
            }
            return Id;
        }
        public APIResponse<MyCourseForApplicability> GetCourseDetailsForApplicability(int CourseId)
        {
            APIResponse<MyCourseForApplicability> aPIResponse = new APIResponse<MyCourseForApplicability>(); 
            if ( CourseId == 0)
            {
                return aPIResponse;
            }

            List<MyCourseForApplicability> course = (
                          from c in _db.Course
                          join cat in this._db.Category on c.CategoryId equals cat.Id into r
                          from category in r.DefaultIfEmpty()
                          join Cd in _db.CourseDetails on c.Id equals Cd.CourseID
                          into cds
                          from cds2 in cds.DefaultIfEmpty()
                          join ci in this._db.CourseInstructor on cds2.CourseInstructorID equals ci.Id
                          into ci1
                          from ci2 in ci1.DefaultIfEmpty()
                          join co in this._db.CourseOwner on cds2.CourseOwnerID equals co.Id
                          into co1
                          from co2 in co1.DefaultIfEmpty()
                          
                          where c.Id == CourseId
                          select new MyCourseForApplicability
                          {
                              Language = c.Language,
                              Category = category.Name,
                              Duration = c.DurationInMinutes,
                              Vender = c.ExternalProvider,
                              Level = c.CourseLevel,
                              courseInstructor = ci2.Name,
                              courseOwner = co2.Name
                          }).ToList();
            

            aPIResponse.Data.Records = course;
            aPIResponse.Data.RecordCount = course.Count();
            return aPIResponse;
        }

        public async Task<Message> EnrollCourseForTTgroup(int userId, int CourseId, string orgCode)
        {
            List<NodalCourseRequests> nodalCourseRequestsExistsList = await _db.NodalCourseRequests.Where(x => x.UserId == userId && x.CourseId==CourseId && x.IsDeleted == false && (x.IsApprovedByNodal == true || x.IsApprovedByNodal == null)).ToListAsync();
            if (nodalCourseRequestsExistsList.Count > 0)
            {
                if (nodalCourseRequestsExistsList.Where(x => x.IsApprovedByNodal == true).Count() > 0)
                    return Message.ApprovedRequestsExists;
                else
                    return Message.PendingRequestsExists;
            }
            else
            {
                Courses.API.Model.Course course = await _db.Course.Where(x => x.Id == CourseId && x.IsDeleted == false).FirstOrDefaultAsync();

                //Add course request
                NodalCourseRequests nodalCourseRequest = new NodalCourseRequests();
                nodalCourseRequest.CourseId = CourseId;
                nodalCourseRequest.UserId = userId;
                nodalCourseRequest.RequestType = Record.Individual;
                nodalCourseRequest.CreatedBy = nodalCourseRequest.ModifiedBy = userId;
                nodalCourseRequest.CreatedDate = nodalCourseRequest.ModifiedDate = DateTime.UtcNow;
                nodalCourseRequest.CourseFee = course.CourseFee;
                _db.NodalCourseRequests.Add(nodalCourseRequest);
                await _db.SaveChangesAsync();

                return Message.Ok;
            }
        }
    }
}
