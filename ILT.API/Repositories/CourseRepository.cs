using AutoMapper;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using ILT.API.Services;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
//using ILT.API.APIModel.NodalManagement;
//using ILT.API.APIModel.TigerhallIntegration;
using ILT.API.ExternalIntegration.EdCast;
using Microsoft.AspNetCore.Hosting;
using ILT.API.Repositories.Interfaces;
using ILT.API.Model.EdCastAPI;
using System.Text;
using static ILT.API.Models.CourseContext;
using static ILT.API.Common.EnumHelper;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;

namespace ILT.API.Repositories
{
    public class CourseRepository : Repository<Model.Course>, ICourseRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRepository));
        private CourseContext _db;
        INotification _notification;
        private IConfiguration _configuration;
        IIdentityService _identitySv;
        ICustomerConnectionStringRepository _customerConnection;
        IEmail _email;
       // private IMyCoursesRepository _myCoursesRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
       IEdCastTransactionDetails _edCastTransactionDetails;
        //IDarwinboxTransactionDetails _darwinboxTransactionDetails;
        //IDarwinboxConfiguration _darwinboxConfiguration;
        IAzureStorage _azurestorage;
        public CourseRepository(CourseContext context, IAzureStorage azurestorage, INotification notification, IConfiguration configuration, IEmail email, ICustomerConnectionStringRepository customerConnection, IIdentityService identitySv, /*IMyCoursesRepository myCoursesRepository,*/ IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IEdCastTransactionDetails edCastTransactionDetails 
            /*IDarwinboxTransactionDetails darwinboxTransactionDetails,*/ /*IDarwinboxConfiguration darwinboxConfiguration*/) : base(context)
        {
            this._db = context;
            _httpContextAccessor = httpContextAccessor;
            this._notification = notification;
            this._configuration = configuration;
            this._identitySv = identitySv;
            this._customerConnection = customerConnection;
            this._email = email;
           // this._myCoursesRepository = myCoursesRepository;
            this._hostingEnvironment = hostingEnvironment;
            this._edCastTransactionDetails = edCastTransactionDetails;
            //this._darwinboxTransactionDetails = darwinboxTransactionDetails;
            //this._darwinboxConfiguration = darwinboxConfiguration;
            this._azurestorage = azurestorage;
        }
        public void ChangeDbContext(string connectionString)
        {
            this._db = DbContextFactory.Create(connectionString);
            this._context = this._db;
            this._entities = this._context.Set<Model.Course>();
        }
    //public async Task<List<APIAllCourses>> GetAll(int? page = null, int? pageSize = null, int? categoryId = null, bool? status = null, string search = null, string filter = null)
    //    {
    //        var Query = (from courses in _db.Course
    //                     where courses.IsDeleted == false
    //                     select new APIAllCourses
    //                     {
    //                         AdminName = courses.AdminName,
    //                         AssessmentId = courses.AssessmentId,
    //                         AssignmentId = courses.AssignmentId,
    //                         CategoryId = courses.CategoryId,
    //                         Code = courses.Code,
    //                         CompletionPeriodDays = courses.CompletionPeriodDays,
    //                         CourseAdminID = courses.CourseAdminID,
    //                         CourseFee = courses.CourseFee,
    //                         CourseType = courses.CourseType,
    //                         CourseURL = courses.CourseURL,
    //                         CreatedBy = courses.CreatedBy,
    //                         CreatedDate = courses.CreatedDate,
    //                         CreditsPoints = courses.CreditsPoints,
    //                         Currency = courses.Currency,
    //                         Description = courses.Description,
    //                         DurationInMinutes = courses.DurationInMinutes,
    //                         ExternalProvider = courses.ExternalProvider,
    //                         FeedbackId = courses.FeedbackId,
    //                         Id = courses.Id,
    //                         IsAchieveMastery = courses.IsAchieveMastery,
    //                         IsActive = courses.IsActive,
    //                         IsAdaptiveLearning = courses.IsAdaptiveLearning,
    //                         IsApplicableToAll = courses.IsApplicableToAll,
    //                         IsAssessment = courses.IsAssessment,
    //                         IsAssignment = courses.IsAssignment,
    //                         IsCertificateIssued = courses.IsCertificateIssued,
    //                         IsDeleted = courses.IsDeleted,
    //                         IsDiscussionBoard = courses.IsDiscussionBoard,
    //                         IsExternalProvider = courses.IsExternalProvider,
    //                         IsFeedback = courses.IsFeedback,
    //                         IsManagerEvaluation = courses.IsManagerEvaluation,
    //                         IsMemoCourse = courses.IsMemoCourse,
    //                         IsModuleHasAssFeed = courses.IsModuleHasAssFeed,
    //                         IsPreAssessment = courses.IsPreAssessment,
    //                         IsRetraining = courses.IsRetraining,
    //                         IsSection = courses.IsSection,
    //                         IsShowInCatalogue = courses.IsShowInCatalogue,
    //                         Language = courses.Language,
    //                         LearningApproach = courses.LearningApproach,
    //                         ManagerEvaluationId = courses.ManagerEvaluationId,
    //                         MemoId = courses.MemoId,
    //                         Metadata = courses.Metadata,
    //                         Mission = courses.Mission,
    //                         ModifiedBy = courses.ModifiedBy,
    //                         ModifiedDate = courses.ModifiedDate,
    //                         noOfDays = courses.noOfDays,
    //                         Points = courses.Points,
    //                         PreAssessmentId = courses.PreAssessmentId,
    //                         prerequisiteCourse = courses.prerequisiteCourse,
    //                         RowGuid = courses.RowGuid,
    //                         SubCategoryId = courses.SubCategoryId,
    //                         ThumbnailPath = courses.ThumbnailPath,
    //                         Title = courses.Title,
    //                         TotalModules = courses.TotalModules,
    //                         prerequisiteCourseName = _db.Course.Where(x => x.Id == courses.prerequisiteCourse).Select(x => x.Title.ToString()).FirstOrDefault(),
    //                         IsFeedbackOptional = courses.IsFeedbackOptional,
    //                         GroupCourseFee = courses.GroupCourseFee,
    //                         IsVisibleAfterExpiry = courses.IsVisibleAfterExpiry,
    //                         IsDashboardCourse = courses.IsDashboardCourse,
    //                         isAssessmentReview = courses.isAssessmentReview,
    //                         PublishCourse= courses.PublishCourse,
    //                         IsPrivateContent= courses.IsPrivateContent
    //                     }


    //                   );

    //        if (filter == "null")
    //            filter = null;
    //        if (search == "null")
    //            search = null;
    //        if (!string.IsNullOrEmpty(search))
    //        {
    //            if (!string.IsNullOrEmpty(filter))
    //            {
    //                if (filter.ToLower().Equals("code"))
    //                    Query = Query.Where(r => r.Code.StartsWith(search));
    //                if (filter.ToLower().Equals("title"))
    //                    Query = Query.Where(r => r.Title.Contains(search));
    //                if (filter.ToLower().Equals("description"))
    //                    Query = Query.Where(r => r.Description.Contains(search));
    //                if (filter.ToLower().Equals("language"))
    //                    Query = Query.Where(r => r.Language.StartsWith(search));
    //                if (filter.ToLower().Equals("coursetype") && search.ToLower().Equals("assessment"))
    //                {
    //                    Query = Query.Where(r => r.CourseType.Equals("Certification"));
    //                }
    //                else
    //                {
    //                    if (filter.ToLower().Equals("coursetype"))
    //                        Query = Query.Where(r => r.CourseType.StartsWith(search));
    //                }
    //                if (filter.ToLower().Equals("topic"))
    //                    Query = Query.Where(r => r.Metadata.Contains(search));
    //            }
    //            else
    //            {
    //                Query = Query.Where(r => r.Title.Contains(search) || r.Metadata.Contains(search) || r.Code.Contains(search));
    //            }
    //        }

    //        if (categoryId != null)
    //            Query = Query.Where(r => r.CategoryId == categoryId);
    //        if (status != null)
    //            Query = Query.Where(r => r.IsActive == status);

    //        Query = Query.OrderByDescending(r => r.Id);
    //        if (page != -1 && page != null)
    //            Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
    //        if (pageSize != -1 && pageSize != null)
    //            Query = Query.Take(Convert.ToInt32(pageSize));
    //        var course = await Query.ToListAsync();
    //        return course;
    //    }

//Dont comment
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
            }
            return value;
        }
        //public async Task<string> GetMasterConfigurableParameterValueOrganization(string configurationCode, string orgcode)
        //{
        //    string value = null; //default value
        //    try
        //    {
        //        var OrgnizationConnectionString = await this.OrgnizationConnectionString(orgcode); //todo move to cache

        //        using (var dbContext = this._customerConnection.GetDbContext(OrgnizationConnectionString))
        //        {
        //            using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
        //            {
        //                cmd.CommandText = "GetConfigurableParameterValue";
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configurationCode });
        //                DbDataReader reader = await cmd.ExecuteReaderAsync();
        //                DataTable dt = new DataTable();
        //                dt.Load(reader);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    value = dt.Rows[0]["Value"].ToString();
        //                }
        //                reader.Dispose();
        //            }                   
        //        }                
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //    }
        //    return value;
        //}

        //public async Task<bool?> IsPublishedCourse(int courseId)
        //{
        //    bool? value = false; //default value
        //    try
        //    {
        //        value = await _db.Course.Where(a => a.Id == courseId).Select(a => a.PublishCourse).FirstOrDefaultAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw ex;
        //    }
        //    return value;
        //}

        //public async Task<int> Count(int? categoryId = null, bool? status = null, string search = null, string filter = null)
        //{
        //    IQueryable<Courses.API.Model.Course> Query = _db.Course.Where(c => c.IsDeleted == false);

        //    if (filter == "null")
        //        filter = null;
        //    if (search == "null")
        //        search = null;
        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        if (!string.IsNullOrEmpty(filter))
        //        {
        //            if (filter.ToLower().Equals("code"))
        //                Query = Query.Where(r => r.Code.StartsWith(search));
        //            if (filter.ToLower().Equals("title"))
        //                Query = Query.Where(r => r.Title.Contains(search));
        //            if (filter.ToLower().Equals("description"))
        //                Query = Query.Where(r => r.Description.Contains(search));
        //            if (filter.ToLower().Equals("language"))
        //                Query = Query.Where(r => r.Language.StartsWith(search));
        //            if (filter.ToLower().Equals("coursetype"))
        //                Query = Query.Where(r => r.CourseType.StartsWith(search));
        //            if (filter.ToLower().Equals("topic"))
        //                Query = Query.Where(r => r.Metadata.Contains(search));
        //        }
        //        else
        //        {
        //            Query = Query.Where(r => r.Title.Contains(search) || r.Metadata.Contains(search) || r.Code.Contains(search));
        //        }
        //    }


        //    if (status != null)
        //    {
        //        Query = Query.Where(r => r.IsActive == status);
        //    }

        //    if (categoryId != null)
        //    {
        //        Query = Query.Where(r => r.CategoryId == categoryId);
        //    }

        //    return await Query.Select(C => C.Id).CountAsync();
        //}


        //public async Task<List<APICourseDTO>> GetCourse(int? categoryId = null, string search = null, string courseType = null)
        //{
        //    var courses = (from c in _db.Course
        //                   join cat in _db.Category on c.CategoryId equals cat.Id
        //                   into ps
        //                   from cat in ps.DefaultIfEmpty()
        //                   where (
        //                   (search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //                   && c.IsDeleted == false && c.IsActive == true)
        //                   && (cat.Id == categoryId || categoryId == null
        //                   )
        //                   && (c.CourseType == courseType || courseType == null
        //                   )
        //                   orderby c.Title
        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   }
        //                into t1
        //                   group t1 by new { t1.Title } into result
        //                   select new APICourseDTO
        //                   {
        //                       Id = result.FirstOrDefault().Id,
        //                       Title = result.FirstOrDefault().Title
        //                   });
        //    return await courses.ToListAsync();
        //}

        //public async Task<List<APICoursesData>> GetReportCourse(int userId, string role,string search = null, string courseType = null)
        //{
            
        //    var courses = (from Course in this._db.Course
        //                   orderby Course.Title
        //                   where Course.IsActive == true && Course.IsDeleted == Record.NotDeleted
        //                                && (search == null || Course.Title.StartsWith(search) || Course.Code.StartsWith(search))
        //                                 && (Course.CourseType == courseType || courseType == null)

        //                   select new APICoursesData
        //                   {

        //                       Id = Course.Id,
        //                       Title = Course.Title
        //                   }).AsNoTracking();

           
            
        //    var authorCourses = (from Course in this._db.Course
        //                         join author in this._db.CourseAuthorAssociation on Course.Id equals author.CourseId
        //                   orderby Course.Title
        //                   where Course.IsActive == true && Course.IsDeleted == Record.NotDeleted && author.IsDeleted==0 && author.UserId==userId
        //                                && (search == null || Course.Title.StartsWith(search) || Course.Code.StartsWith(search))
        //                                 && (Course.CourseType == courseType || courseType == null)

        //                   select new APICoursesData
        //                   {
        //                       Id = Course.Id,
        //                       Title = Course.Title
        //                   }).AsNoTracking();

        //    var authorCreated = (from Course in this._db.Course                                
        //                         orderby Course.Title
        //                         where Course.IsActive == true && Course.IsDeleted == Record.NotDeleted &&  Course.CreatedBy == userId
        //                                      && (search == null || Course.Title.StartsWith(search) || Course.Code.StartsWith(search))
        //                                       && (Course.CourseType == courseType || courseType == null)

        //                         select new APICoursesData
        //                         {
        //                             Id = Course.Id,
        //                             Title = Course.Title
        //                         }).AsNoTracking();




        //    //var authorCourses = (_db.Course

        //    //              .OrderBy(c => c.Title)
        //    //              .Where(c => c.IsDeleted == false &&
        //    //              (search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //    //              && (c.CourseType == courseType || courseType == null))
        //    //              .GroupBy(g => new { g.Id, g.Title })
        //    //             .Select(s => new APICourseDTO
        //    //             {

        //    //                 Id = s.Max(f => f.Id),
        //    //                 Title = s.Max(a => a.Title)
        //    //             }));

        //    if (role.ToLower() != "eu")
        //    {
        //        return await courses.Distinct().ToListAsync();
        //    }
        //    else 
        //    {
        //        return await authorCourses.Union(authorCreated).Distinct().ToListAsync();
        //    }
        //}

        //public async Task<List<APICourseDTO>> CourseTypehead(string search = null, string filter = null)
        //{
        //    if (filter == "null")
        //        filter = null;
        //    if (search == "null")
        //        search = null;

        //    var Query = (from Course in this._db.Course
        //                 where Course.IsActive == true && Course.IsDeleted == Record.NotDeleted
        //                 select new APICourseDTO
        //                 {
        //                     Id = Course.Id,
        //                     Title = Course.Title
        //                 });
        //    if (!string.IsNullOrEmpty(search))
        //        Query = Query.Where(a => a.Title.StartsWith(search));

        //    Query = Query.OrderByDescending(r => r.Id);
        //    return await Query.ToListAsync();
        //}

        //public async Task<List<APICourseDTO>> GetILTCourse(int? categoryId = null, string search = null)
        //{
        //    var courses = (from c in _db.Course
        //                   join cat in _db.Category on c.CategoryId equals cat.Id
        //                   into ps
        //                   from cat in ps.DefaultIfEmpty()
        //                   where (
        //                   (search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //                   && c.IsDeleted == false)
        //                   && (cat.Id == categoryId || categoryId == null
        //                   )
        //                   && (c.CourseType != "elearning" || c.CourseType == "Certification"
        //                   )
        //                   orderby c.Title
        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   }
        //                    into t1
        //                   group t1 by new { t1.Title } into result
        //                   select new APICourseDTO
        //                   {
        //                       Id = result.FirstOrDefault().Id,
        //                       Title = result.FirstOrDefault().Title
        //                   });
        //    return await courses.ToListAsync();
        //}

        //public async Task<List<APICourseDTO>> GetCourseTypeahead(int? categoryid)
        //{
        //    var courses = (from c in _db.Course
        //                   where (c.CategoryId == categoryid || categoryid == null && c.IsDeleted == false)

        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   } into t1
        //                   group t1 by new { t1.Title } into result
        //                   select new APICourseDTO
        //                   {
        //                       Id = result.FirstOrDefault().Id,
        //                       Title = result.FirstOrDefault().Title
        //                   });
        //    return await courses.ToListAsync();
        //}

        //public async Task<List<APICourseTypeahead>> SearchCourses(string search = null)
        //{
        //    var courses = (from c in _db.Course
        //                   where
        //                  (c.Title.StartsWith(search) || search == null && c.IsDeleted == false)
        //                   select new APICourseTypeahead
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   }
        //                   );
        //    return await courses.ToListAsync();
        //}

        //public async Task<List<APICourseDTO>> ApplicableToAllCourseTypeahead(string search = null)
        //{
            
        //    if (search == "null")
        //        search = null;

        //    var Query = (from Course in this._db.Course
        //                 where Course.IsActive == true && Course.IsDeleted == Record.NotDeleted && Course.IsApplicableToAll==true
        //                 select new APICourseDTO
        //                 {
        //                     Id = Course.Id,
        //                     Title = Course.Title
        //                 });
        //    if (!string.IsNullOrEmpty(search))
        //        Query = Query.Where(a => a.Title.StartsWith(search));

        //    Query = Query.OrderByDescending(r => r.Id);
        //    return await Query.ToListAsync();
        //}

        //public async Task<string> GetCourseCodeById(int? courseId)
        //{
        //    return await _db.Course.Where(r => r.Id == courseId && r.IsDeleted == false).Select(c => c.Code).FirstOrDefaultAsync();
        //}
        //public async Task<List<APIModuleTypeAhead>> GetCourseModules(int courseId)
        //{
        //    return await _db.CourseModuleAssociation.
        //        Join(_db.Module, cm => cm.ModuleId, m => m.Id, (cm, m) =>
        //              new { CorseModule = cm, Module = m })
        //    .Where(m => m.Module.IsDeleted == false && m.CorseModule.CourseId == courseId).
        //    Select(c => new APIModuleTypeAhead
        //    {
        //        Id = c.Module.Id,
        //        Name = c.Module.Name
        //    }).ToListAsync();
        //}

        //public async Task<string> GetCourseNam(int? id)
        //{
        //    return await (from c in _db.Course
        //                  where c.IsDeleted == false && c.Id == id
        //                  select c.Title).FirstOrDefaultAsync();
        //}

        //public async Task<string> GetAssessmentConfigurationID(int? courseId, int? moduleId, string orgCode, bool isPreAssessment = false, bool isContentAssessment = false)
        //{
        //    int? AssessmentSheetConfigID = 0;

        //    var cache = new CacheManager.CacheManager();
        //    string cacheKeyConfig = Constants.AssessmentSheetConfigurationId + "-" + orgCode.ToUpper() + Convert.ToString(courseId) + Convert.ToString(moduleId) + Convert.ToString(isPreAssessment) + Convert.ToString(isContentAssessment);
        //    if (cache.IsAdded(cacheKeyConfig.ToUpper()))
        //        AssessmentSheetConfigID = Convert.ToInt32(cache.Get<string>(cacheKeyConfig.ToUpper()));
        //    else
        //    {
        //        if (moduleId == 0)
        //        {
        //            if (isPreAssessment)
        //            {
        //                AssessmentSheetConfigID =
        //                await (from cours in _db.Course
        //                       join assessment in _db.Module on cours.PreAssessmentId equals assessment.Id
        //                       join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
        //                       where (cours.Id == courseId && cours.IsDeleted == false && assessment.IsDeleted == false)
        //                       select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
        //            }
        //            else
        //            {
        //                AssessmentSheetConfigID =
        //                    await (from cours in _db.Course
        //                           join assessment in _db.Module on cours.AssessmentId equals assessment.Id
        //                           join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
        //                           where (cours.Id == courseId && cours.IsDeleted == false && assessment.IsDeleted == false)
        //                           select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
        //            }
        //        }
        //        else
        //        {
        //            if (isPreAssessment)
        //            {
        //                AssessmentSheetConfigID =
        //                await (from courseModule in _db.CourseModuleAssociation
        //                       join assessment in _db.Module on courseModule.PreAssessmentId equals assessment.Id
        //                       join module in _db.Module on courseModule.ModuleId equals module.Id
        //                       join course in _db.Course on courseModule.CourseId equals course.Id
        //                       join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
        //                       where (course.Id == courseId && module.Id == moduleId && course.IsDeleted == false &&
        //                       module.IsDeleted == false && assessment.IsDeleted == false)
        //                       select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
        //            }
        //            else if (isContentAssessment)
        //            {

        //                try
        //                {
        //                    using (var dbContext = this._customerConnection.GetDbContext())
        //                    {
        //                        using (var connection = dbContext.Database.GetDbConnection())
        //                        {
        //                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                                connection.Open();

        //                            using (var cmd = connection.CreateCommand())
        //                            {
        //                                cmd.CommandText = "GetAssessmentConfigIdByModuleIDAndCourseID";
        //                                cmd.CommandType = CommandType.StoredProcedure;
        //                                cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.BigInt) { Value = moduleId });
        //                                cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Bit) { Value = courseId });

        //                                DbDataReader reader = await cmd.ExecuteReaderAsync();
        //                                DataTable dt = new DataTable();
        //                                dt.Load(reader);
        //                                if (dt.Rows.Count > 0)
        //                                {
        //                                    foreach (DataRow row in dt.Rows)
        //                                    {
        //                                        AssessmentSheetConfigID = Convert.ToInt32(row["AssessmentSheetConfigID"].ToString());
        //                                    }
        //                                }
        //                                reader.Dispose();
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.Error(Utilities.GetDetailedException(ex));
        //                    throw (ex);
        //                }
        //            }
        //            else
        //            {
        //                AssessmentSheetConfigID =
        //                await (from courseModule in _db.CourseModuleAssociation
        //                       join assessment in _db.Module on courseModule.AssessmentId equals assessment.Id
        //                       join module in _db.Module on courseModule.ModuleId equals module.Id
        //                       join course in _db.Course on courseModule.CourseId equals course.Id
        //                       join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
        //                       where (course.Id == courseId && module.Id == moduleId && course.IsDeleted == false &&
        //                       module.IsDeleted == false && assessment.IsDeleted == false)
        //                       select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
        //            }
        //        }
        //        _logger.Debug("Adding config key :- " + cacheKeyConfig.ToUpper() + " Value :- " + Convert.ToString(AssessmentSheetConfigID));
        //        cache.Add(cacheKeyConfig.ToUpper(), Convert.ToString(AssessmentSheetConfigID), DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
        //    }
        //    return AssessmentSheetConfigID.ToString();
        //}

        //public async Task<string> GetManagerAssessmentConfigurationID(int? courseId, int? moduleId)
        //{
        //    int? AssessmentSheetConfigID = 0;
        //    try
        //    {
        //        AssessmentSheetConfigID =
        //        await (from course in _db.Course
        //               join assessment in _db.Module on course.ManagerEvaluationId equals assessment.Id
        //               join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
        //               where (course.Id == courseId && course.IsDeleted == false && assessment.IsDeleted == false)
        //               select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw;
        //    }
        //    return AssessmentSheetConfigID.ToString();
        //}

        //public async Task<string> GetFeedbackConfigurationID(int? id)
        //{
        //    string ID = null;
        //    var Course = await (from a in _db.Module
        //                        join c in _db.LCMS on a.LCMSId equals c.Id
        //                        where (a.Id == id && a.IsDeleted == false)
        //                        select (c.FeedbackSheetConfigID)).FirstOrDefaultAsync();
        //    ID = Course.ToString();
        //    return ID;
        //}

        //public async Task<IEnumerable<object>> GetCourseNameList(string id)

        //{
        //    var CourseId = id.Split(',');
        //    var Course = (from c in _db.Course
        //                  where CourseId.Contains(c.Id.ToString()) && c.IsDeleted == false
        //                  select new
        //                  {
        //                      c.Title,
        //                      c.Id
        //                  }).AsEnumerable();
        //    return Course;
        //}
        //public async Task<string> GetModuleName(int? id)
        //{
        //    var Modulename = await (from c in _db.Module
        //                            where c.IsDeleted == false && c.Id == id
        //                            select c.Name).SingleOrDefaultAsync();
        //    return Modulename;
        //}

        //public async Task AddModules(int courseid, int[] moduleids)
        //{
        //    IList<CourseModuleAssociation> modules = new List<CourseModuleAssociation>();
        //    CourseModuleAssociation module = new CourseModuleAssociation();

        //    foreach (int moduleid in moduleids)
        //    {
        //        module.CourseId = courseid;
        //        module.ModuleId = moduleid;
        //    }

        //    using (var context = _db)
        //    {
        //        context.CourseModuleAssociation.AddRange(modules);
        //    }
        //    await _db.SaveChangesAsync();
        //}

        //public async Task<IEnumerable<ApiCourseModuleAssociation>> GetModules(int courseid)
        //{
        //    var GetModuleQuery = await (from course in _db.Course
        //                                join corseModuleAssociation in _db.CourseModuleAssociation on course.Id equals corseModuleAssociation.CourseId
        //                                join module in _db.Module on corseModuleAssociation.ModuleId equals module.Id
        //                                join assess in _db.Module on corseModuleAssociation.AssessmentId equals assess.Id into assess
        //                                from assessment in assess.DefaultIfEmpty()
        //                                join feed in _db.Module on corseModuleAssociation.FeedbackId equals feed.Id into feed
        //                                from feedback in feed.DefaultIfEmpty()
        //                                join pre in _db.Module on corseModuleAssociation.PreAssessmentId equals pre.Id into pre
        //                                from preassass in pre.DefaultIfEmpty()
        //                                join sec in _db.Section on corseModuleAssociation.SectionId equals sec.Id into sec
        //                                from section in sec.DefaultIfEmpty()
        //                                where corseModuleAssociation.CourseId == courseid && module.IsDeleted == false
        //                                orderby corseModuleAssociation.SequenceNo ascending
        //                                select new ApiCourseModuleAssociation
        //                                {
        //                                    Id = corseModuleAssociation.Id,
        //                                    ModuleId = module.Id,
        //                                    AssessmentId = corseModuleAssociation.AssessmentId,
        //                                    FeedbackId = corseModuleAssociation.FeedbackId,
        //                                    PreAssessmentId = corseModuleAssociation.PreAssessmentId,
        //                                    IsAssessment = corseModuleAssociation.IsAssessment,
        //                                    IsPreAssessment = corseModuleAssociation.IsPreAssessment,
        //                                    IsFeedback = corseModuleAssociation.IsFeedback,
        //                                    FeedbackName = feedback.Name,
        //                                    AssessmentName = assessment.Name,
        //                                    PreAssessmentName = preassass.Name,
        //                                    Name = module.Name,
        //                                    ModuleType = module.ModuleType,
        //                                    SectionId = section.Id,
        //                                    SectionTitle = section.Title,
        //                                    SequenceNo = corseModuleAssociation.SequenceNo,
        //                                    CompletionPeriodDays = corseModuleAssociation.CompletionPeriodDays,
        //                                    StartDate = course.StartDate,
        //                                    EndDate = course.EndDate,
        //                                    CanAutoActivated = course.CanAutoActivated,
        //                                    IsVisibleAfterExpiry = course.IsVisibleAfterExpiry
        //                                }).ToListAsync();
        //    return GetModuleQuery;

        //}

        //public async Task<bool> CheckCourseForAllowModification(int courseid)
        //{
        //    bool AllowModify = true;
        //    ApiConfigurableParameters apiConfigurableParameters = _db.configurableParameters.Where(a => a.Code == "ALLOW_TO_DELETE_MODULE").FirstOrDefault();
        //    if(apiConfigurableParameters != null)
        //    {
        //        if (apiConfigurableParameters.Value.ToLower() == "yes")
        //        {
        //            return AllowModify;
        //        }
        //    }
            
        //    var CourseForAllowModification = await (from courseCompletion in _db.CourseCompletionStatus
        //                                            where courseCompletion.CourseId == courseid
        //                                            select new
        //                                            { courseCompletion.Id }).CountAsync();
        //    if (CourseForAllowModification > 0)
        //        AllowModify = false;


        //    return AllowModify;

        //}

        //public async Task<bool> CreateCourseFromExisting(int courseid)
        //{
        //    bool AllowModify = false;
        //    ApiConfigurableParameters apiConfigurableParameters = _db.configurableParameters.Where(a => a.Code == "ALLOW_TO_DELETE_MODULE").FirstOrDefault();
        //    if (apiConfigurableParameters != null)
        //    {
        //        if (apiConfigurableParameters.Value.ToLower() == "yes")
        //        {
        //            var CourseForAllowModification = await (from courseCompletion in _db.CourseCompletionStatus
        //                                                    where courseCompletion.CourseId == courseid
        //                                                    select new
        //                                                    { courseCompletion.Id }).CountAsync();
        //            if (CourseForAllowModification > 0)
        //                AllowModify = true;
        //            return AllowModify;
        //        }
        //    }
        //    return AllowModify;
        //}
        //public async Task<string> GetCourseId(string courseCode)
        //{
        //    return await _db.Course.Where(c => String.Equals(c.Code, courseCode, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Id.ToString()).FirstOrDefaultAsync();
        //}

        //public async Task<List<APICourseDTO>> GetModulesAssessment()
        //{
        //    var Modules = (from c in _db.Module
        //                   join f in _db.LCMS on c.LCMSId equals f.Id
        //                   where f.AssessmentSheetConfigID != null && c.IsDeleted == false
        //                   select new APICourseDTO
        //                   {
        //                       Title = c.Name,
        //                       Id = f.AssessmentSheetConfigID.Value,
        //                   }
        //                  );
        //    return await Modules.ToListAsync();
        //}
        //public async Task<List<APICourseDTO>> GetCourseForAccessibility(int? categoryId, string search)
        //{
        //    var courses = (from c in _db.Course
        //                   join cat in _db.Category on c.CategoryId equals cat.Id
        //                   into ps
        //                   from cat in ps.DefaultIfEmpty()
        //                   where ((c.IsDeleted == false && c.IsApplicableToAll == false)
        //                   && (cat.Id == categoryId || categoryId == null) && (c.Title.Contains(search) || search == null))

        //                   select new APICourseDTO
        //                   {
        //                       Id = c.Id,
        //                       Title = c.Title
        //                   });


        //    return await courses.OrderBy(c => c.Title).ToListAsync();
        //}

        //public async Task<bool> Exist(string title, string code, string Isinstitute, int? courseId = null)
        //{
        //    code = code.ToLower();
        //    title = title.ToLower();
        //    int Count = 0;


        //    if (courseId != null)
        //    {
        //        if (Isinstitute.ToLower() == "false")
        //        {
        //            Count = await (from c in _db.Course
        //                           where c.Id != courseId && c.IsDeleted == false && (c.Title.ToLower().Equals(title) || c.Code.ToLower().Equals(code))
        //                           select new
        //                           { c.Id }).CountAsync();
        //        }
        //        else
        //        {
        //            Count = await (from c in _db.Course
        //                           where c.Id != courseId && c.IsDeleted == false && (c.Code.ToLower().Equals(code))
        //                           select new
        //                           { c.Id }).CountAsync();
        //        }

        //    }
        //    else
        //    {
        //        if (Isinstitute.ToLower() == "false")
        //        {
        //            Count = await (from c in _db.Course
        //                           where c.IsDeleted == false && (c.Title.ToLower().Equals(title) || c.Code.ToLower().Equals(code))
        //                           select new
        //                           { c.Id }).CountAsync();
        //        }
        //        else
        //        {
        //            Count = await (from c in _db.Course
        //                           where c.IsDeleted == false && (c.Code.ToLower().Equals(code))
        //                           select new
        //                           { c.Id }).CountAsync();
        //        }
        //    }

        //    if (Count > 0)
        //        return true;
        //    return false;

        //}
        //public async Task<bool> IsAssementExist(int CourseID)
        //{
        //    int? AssessmentId = await _db.Course.Where(r => r.Id == CourseID).Select(c => c.AssessmentId).SingleOrDefaultAsync();
        //    if (AssessmentId == null || AssessmentId == 0)
        //        return false;
        //    return true;
        //}
        //public async Task<bool> IsFeedbackExist(int CourseID)
        //{
        //    int? FeedbackId = await _db.Course.Where(r => r.Id == CourseID).Select(c => c.FeedbackId).SingleOrDefaultAsync();
        //    if (FeedbackId == null || FeedbackId == 0)
        //        return false;
        //    return true;
        //}
        //public async Task<Object> GetCoursesAssessmentFeedbackName(int? assessmentId = null, int? feedbackId = null, int? preassessmentId = null, int? assignmentId = null, int? managerEvaluationId = null, int? ojtId =null)
        //{
        //    return await (from Assessment in _db.Module
        //                  select new
        //                  {
        //                      AssessmentName = _db.Module.Where(assessment => assessment.Id == assessmentId).Select(assessment => assessment.Name).FirstOrDefault(),
        //                      FeedbackName = _db.Module.Where(Feedback => Feedback.Id == feedbackId).Select(Feedback => Feedback.Name).FirstOrDefault(),
        //                      PreAssessmentName = _db.Module.Where(PreAssessment => PreAssessment.Id == preassessmentId).Select(PreAssessment => PreAssessment.Name).FirstOrDefault(),
        //                      AssignmentName = _db.Module.Where(Assignment => Assignment.Id == assignmentId).Select(Assignment => Assignment.Name).FirstOrDefault(),
        //                      ManagerEvaluationName = _db.Module.Where(assessment => assessment.Id == managerEvaluationId).Select(assessment => assessment.Name).FirstOrDefault(),
        //                      ojtName = _db.Module.Where(OJT => OJT.Id == ojtId).Select(OJT => OJT.Name).FirstOrDefault(),
        //                  }).FirstOrDefaultAsync();
        //}

        //public async Task<object> GetCoursesAssignment(int? assignmentId = null)
        //{
        //    var LcmsId = await _db.Module.Where(Assignment => Assignment.Id == assignmentId).Select(Assignment => Assignment.LCMSId).FirstOrDefaultAsync();

        //    return await (from Assignment in _db.LCMS
        //                  where (Assignment.Id == LcmsId && Assignment.IsDeleted == false)
        //                  select new
        //                  {
        //                      Assignment.Name,
        //                      Assignment.Description
        //                  }
        //                 ).FirstOrDefaultAsync();

        //}


        //public async Task<CourseCode> GetCourseCode()
        //{
        //    CourseCode CourseCode = new CourseCode();

        //    int maxAutonumber = _db.CourseCode.OrderByDescending(p => p.Id).FirstOrDefault().AutoNumber;
        //    string Prefix = _db.CourseCode.OrderByDescending(p => p.Id).FirstOrDefault().Prefix;
        //    if (maxAutonumber == 0)
        //        maxAutonumber = 1000;


        //    CourseCode.AutoNumber = maxAutonumber + 1;
        //    CourseCode.Prefix = Prefix;
        //    _db.CourseCode.Add(CourseCode);
        //    await _db.SaveChangesAsync();
        //    return CourseCode;
        //}
        //public async Task<int> SendCourseAddedNotification(int courseId, string title, string courseCode, string token)
        //{
        //    ApiNotification Notification = new ApiNotification();
        //    Notification.Title = title;
        //    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
        //    Notification.Message = Notification.Message.Replace("{course}", title);
        //    Notification.Url = TlsUrl.NotificationAPost + courseId;
        //    Notification.Type = Record.Course;
        //    await this._notification.SendNotification(Notification, token);
        //    return 1;
        //}
        //public async Task<int> GetNotificationId(string title)
        //{
        //    var connection = this._db.Database.GetDbConnection();//Dont use Using statement for Connection variable
        //    int id = 0;
        //    try
        //    {
        //        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //            connection.Open();
        //        using (var cmd = connection.CreateCommand())
        //        {
        //            cmd.CommandText = "GetIdNotifications";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = title });
        //            DbDataReader reader = await cmd.ExecuteReaderAsync();
        //            DataTable dt = new DataTable();
        //            dt.Load(reader);
        //            if (dt.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : Convert.ToInt32(row["Id"].ToString());

        //                }
        //            }
        //            reader.Dispose();
        //        }
        //        connection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        if (connection.State == ConnectionState.Open)
        //            connection.Close();
        //    }
        //    return id;
        //}
        //public async Task<int> UpdateCourseNotification(Courses.API.Model.Course oldCourse, int courseId, string title, string courseCode, string token)
        //{
        //    ApiNotification Notification = new ApiNotification();
        //    Notification.Title = title;
        //    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
        //    Notification.Message = Notification.Message.Replace("{course}", title);
        //    Notification.Url = TlsUrl.NotificationAPost + courseId;
        //    Notification.Type = Record.Course;
        //    int id = await GetNotificationId(oldCourse.Title);
        //    await this._notification.UpdateNotification(id, Notification, token);
        //    return 1;
        //}

        //public async Task<int> UpdateCourseNotification(Model.Course oldCourse, int courseId, string title, string token)
        //{
        //    ApiNotification Notification = new ApiNotification();
        //    Notification.Title = title;
        //    Notification.Message = this._configuration[Configuration.CourseNotification].ToString();
        //    Notification.Message = Notification.Message.Replace("{course}", title);
        //    Notification.Url = TlsUrl.NotificationAPost + courseId;
        //    Notification.Type = Record.Course;
        //    int id = await GetNotificationId(oldCourse.Title);
        //    await this._notification.DeleteNotification(id, token);
        //    return 1;
        //}


        //public async Task<string> GetCourseName(int courseId)
        //{
        //    var Coursename = await (from c in _db.Course
        //                            where c.IsDeleted == false && c.Id == courseId
        //                            select c.Title).SingleOrDefaultAsync();
        //    return Coursename;
        //}
        //public async Task<bool> IsDependacyExist(int courseId)
        //{
        //    int Count = await this._db.CourseCompletionStatus.Where(c => c.CourseId == courseId && c.IsDeleted == Record.NotDeleted).CountAsync();
        //    if (Count > 0)
        //        return true;
        //    return false;
        //}
        //public async Task<Message> DeleteCourse(int courseId, string Token)
        //{
        //    Courses.API.Model.Course Course = await this._db.Course.Where(c => c.Id == courseId && c.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
        //    if (Course == null)
        //        return Message.NotFound;

        //    if (Course.IsActive == true)
        //        return Message.CannotDelete;

        //    Course.IsDeleted = Record.Deleted;
        //    await this.Update(Course);
        //    List<Courses.API.Model.CourseModuleAssociation> CourseMOduleAssociation = this._db.CourseModuleAssociation.Where(cm => cm.CourseId == Course.Id).ToList();

        //    foreach (var s in CourseMOduleAssociation)
        //    {
        //        s.Isdeleted = true;
        //    }

        //    #region "Code added to rollback notifications in case the course is now deleted and was made applicable to all."
        //    if (Course.IsApplicableToAll == true)
        //        await this.UpdateCourseNotification(Course, Course.Id, Course.Title, Token);
        //    #endregion


        //    this._db.CourseModuleAssociation.UpdateRange(CourseMOduleAssociation);
        //    this._db.SaveChanges();
        //    return Message.Success;
        //}
        //public async Task<int> AddCourseHistory(Courses.API.Model.Course oldCourse, Courses.API.Model.Course newCourse,
        //    List<CourseModuleAssociation> oldModule, List<CourseModuleAssociation> newModule)
        //{
        //    string OldCourseObj = JsonConvert.SerializeObject(oldCourse);
        //    string NewCourseObj = JsonConvert.SerializeObject(newCourse);
        //    string OldModuleObj = JsonConvert.SerializeObject(oldModule);
        //    string NewModuleObj = JsonConvert.SerializeObject(newModule);
        //    JObject oJsonObject = new JObject();
        //    oJsonObject.Add("oldCourse", OldCourseObj);
        //    oJsonObject.Add("newCourse", NewCourseObj);
        //    oJsonObject.Add("oldModule", OldModuleObj);
        //    oJsonObject.Add("newModule", NewModuleObj);
        //    oJsonObject.Add("courseId", oldCourse.Id);
        //    string Url = this._configuration[Configuration.AuditApi];
        //    Url = Url + "Audit/AddCourseDetails";
        //    string token = this._identitySv.GetToken();
        //    HttpResponseMessage response = await ApiHelper.CallAPI(Url, oJsonObject, token);
        //    return 1;
        //}
        //public async Task<List<object>> CourseTypeAheadFeedback(int? categoryId = null, string search = null, string courseType = null)
        //{
        //    var courses = (from c in _db.Course
        //                   join ma in _db.CourseModuleAssociation on c.Id equals ma.CourseId
        //                   join md in _db.Module on ma.ModuleId equals md.Id
        //                   // join lcms in _db.LCMS on md.LCMSId equals lcms.Id
        //                   join cat in _db.Category on c.CategoryId equals cat.Id
        //                   into ps
        //                   from cat in ps.DefaultIfEmpty()
        //                   where (c.IsActive == true && c.IsDeleted == false &&
        //                   (search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //                   && c.IsDeleted == false && (c.IsFeedback == true || ma.FeedbackId != 0 || md.ModuleType == "Feedback" || ma.IsFeedback == true))
        //                   && (cat.Id == categoryId || categoryId == null
        //                   )
        //                   && (c.CourseType == courseType || courseType == null
        //                   )
        //                   orderby c.Title
        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   });

        //    return await courses.ToListAsync<object>();
        //}
        //public async Task<List<object>> GetAssessmentCourse(int? categoryId = null, string search = null, string courseType = null)
        //{
        //    var courses = (from c in _db.Course
        //                   join ma in _db.CourseModuleAssociation on c.Id equals ma.CourseId
        //                   join md in _db.Module on ma.ModuleId equals md.Id
        //                   //join lcms in _db.LCMS on md.LCMSId equals lcms.Id
        //                   join cat in _db.Category on c.CategoryId equals cat.Id
        //                   into ps
        //                   from cat in ps.DefaultIfEmpty()
        //                   where (c.IsActive == true && c.IsDeleted == false &&
        //                   (search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //                   && c.IsDeleted == false && (c.IsAssessment == true || c.IsFeedback == true ||
        //                   ma.AssessmentId != 0 || md.ModuleType == "Assessment" || ma.IsFeedback == true))
        //                   && (cat.Id == categoryId || categoryId == null
        //                   )
        //                   && (c.CourseType == courseType || courseType == null
        //                   )
        //                   orderby c.Title
        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   });

        //    return await courses.ToListAsync<object>();
        //}

        //public async Task<List<object>> GetAssessmentTypeCourse(int? categoryId = null, string search = null)
        //{
        //    var courses = (from c in _db.Course

        //                   where (c.IsActive == true && c.IsDeleted == false &&
        //                   (search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //                   && c.IsDeleted == false)
        //                   && c.CourseType == "Certification"
        //                   orderby c.Title
        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   }

        //                   );
        //    return await courses.ToListAsync<object>();
        //}

        //public async Task<List<object>> TypeAheadAuthority(int? categoryId = null, string search = null)
        //{
        //    var courses = (from c in _db.Course

        //                   where (c.IsActive == true && c.IsDeleted == false &&
        //                   (search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //                   && c.IsDeleted == false)
        //                   orderby c.Title
        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   }

        //                   );
        //    return await courses.ToListAsync<object>();
        //}
        //public async Task<List<object>> GetAssessmentCourseReport(int? categoryId = null, string search = null, string courseType = null)
        //{

        //    var courses = (from c in _db.Course
        //                   where ((search == null || c.Title.StartsWith(search) || c.Code.StartsWith(search))
        //                   && c.IsDeleted == false)
        //                   orderby c.Title
        //                   select new
        //                   {
        //                       Title = c.Title,
        //                       Id = c.Id,
        //                   });

        //    return await courses.ToListAsync<object>();
        //}

        //public async Task<IEnumerable<APICourses>> GetAllCourses(int userId, bool showAllData = false, int? categoryId = null, bool? IsActive = null, string filter = null, string search = null )
        //{
        //    List<APICourses> apiCourseList = new List<APICourses>();
        //    try
        //    {
        //        using (var dbContext = this._customerConnection.GetDbContext())
        //        {
        //            using (var connection = dbContext.Database.GetDbConnection())
        //            {
        //                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                    connection.Open();
        //                APICourses apiCourse = null;
        //                using (var cmd = connection.CreateCommand())
        //                {
        //                    cmd.CommandText = "GetAllCoursesExport";
                            
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        //                    cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.Bit) { Value = showAllData });
        //                    DbDataReader reader = await cmd.ExecuteReaderAsync();
        //                    DataTable dt = new DataTable();
        //                    dt.Load(reader);

        //                    if (dt.Rows.Count > 0)
        //                    {
        //                        foreach (DataRow row in dt.Rows)
        //                        {
        //                            apiCourse = new APICourses
        //                            {
        //                                Code = row["Code"].ToString(),
        //                                CategoryName = row["CategoryName"].ToString(),
        //                                Title = row["Title"].ToString(),
        //                                CourseType = row["CourseType"].ToString(),
        //                                Description = row["Description"].ToString(),
        //                                Language = row["Language"].ToString(),
        //                                IsCertificateIssued = Convert.ToBoolean(row["IsCertificateIssued"]),
        //                                IsActive = Convert.ToBoolean(row["IsActive"]),
        //                                IsPreAssessment = Convert.ToBoolean(row["IsPreAssessment"]),
        //                                IsAssessment = Convert.ToBoolean(row["IsAssessment"]),
        //                                IsFeedback = Convert.ToBoolean(row["IsFeedback"]),
        //                                TotalModules = Convert.ToInt16(row["TotalModules"]),
        //                                SubCategoryName = row["SubCategoryName"].ToString(),
        //                                ExpiryDate = string.IsNullOrEmpty(row["ExpiryDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["ExpiryDate"].ToString()),
        //                                CreatedBy = row["CreatedBy"].ToString(),
        //                                CreatedDate = string.IsNullOrEmpty(row["CreatedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CreatedDate"].ToString()),
        //                                IsApplicableToAll = Convert.ToBoolean(row["IsApplicableToAll"]) == true ? "Yes" : "No",
        //                                ModifiedBy = row["ModifiedBy"].ToString(),
        //                                DurationInMinutes = row["DurationInMinutes"].ToString(),
        //                            };
        //                            apiCourseList.Add(apiCourse);
        //                        }
        //                    }
        //                    reader.Dispose();
        //                }
        //                connection.Close();
        //                return apiCourseList;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw ex;
        //    }

        //}
        //public async Task<List<APICourseWiseEmailReminder>> GetPagination(int page, int pageSize)
        //{
        //    IQueryable<APICourseWiseEmailReminder> Query = (from coursewise in _db.CourseWiseEmailReminder
        //                                                    join course in _db.Course on coursewise.CourseId equals course.Id
        //                                                    where coursewise.IsDeleted == false
        //                                                    select new APICourseWiseEmailReminder
        //                                                    {
        //                                                        Id = coursewise.Id,
        //                                                        CourseId = coursewise.CourseId,
        //                                                        CourseName = course.Title,
        //                                                        CreatedDate = coursewise.CreatedDate,
        //                                                        TotalUserCount = coursewise.TotalUserCount
        //                                                    });


        //    Query = Query.OrderByDescending(r => r.Id);
        //    if (page != -1)
        //        Query = Query.Skip((page - 1) * pageSize);
        //    if (pageSize != -1)
        //        Query = Query.Take(pageSize);

        //    List<APICourseWiseEmailReminder> courseemail = await Query.ToListAsync();
        //    return courseemail;
        //}


        //public async Task<List<APICourseWiseSMSReminder>> GetPaginationSMS(int page, int pageSize)
        //{
        //    IQueryable<APICourseWiseSMSReminder> Query = (from coursewiseSMS in _db.CourseWiseSMSReminder
        //                                                    join course in _db.Course on coursewiseSMS.CourseId equals course.Id
        //                                                    where coursewiseSMS.IsDeleted == false
        //                                                    select new APICourseWiseSMSReminder
        //                                                    {
        //                                                        Id = coursewiseSMS.Id,
        //                                                        CourseId = coursewiseSMS.CourseId,
        //                                                        CourseName = course.Title,
        //                                                        CreatedDate = coursewiseSMS.CreatedDate,
        //                                                        TotalUserCount = coursewiseSMS.TotalUserCount
        //                                                    });


        //    Query = Query.OrderByDescending(r => r.Id);
        //    if (page != -1)
        //        Query = Query.Skip((page - 1) * pageSize);
        //    if (pageSize != -1)
        //        Query = Query.Take(pageSize);

        //    List<APICourseWiseSMSReminder> courseemail = await Query.ToListAsync();
        //    return courseemail;
        //}





        //public async Task<int> GetCountCourseWiseEmailReminder()

        //{
        //    IQueryable<APICourseWiseEmailReminder> Query = (from coursewise in _db.CourseWiseEmailReminder
        //                                                    join course in _db.Course on coursewise.CourseId equals course.Id
        //                                                    where coursewise.IsDeleted == false
        //                                                    select new APICourseWiseEmailReminder
        //                                                    {
        //                                                        Id = coursewise.Id,
        //                                                        CourseId = coursewise.CourseId,
        //                                                    });

        //    return Query.Count();
        //}


        //public async Task<int> GetCountCourseWiseSMSReminder()

        //{
        //    IQueryable<APICourseWiseSMSReminder> Query = (from coursewise in _db.CourseWiseSMSReminder
        //                                                    join course in _db.Course on coursewise.CourseId equals course.Id
        //                                                    where coursewise.IsDeleted == false
        //                                                    select new APICourseWiseSMSReminder
        //                                                    {
        //                                                        Id = coursewise.Id,
        //                                                        CourseId = coursewise.CourseId,
        //                                                    });

        //    return Query.Count();
        //}

        //public async Task<CourseWiseEmailReminder> Addcoursewise(CourseWiseEmailReminder courseWiseEmail, int UserId, string OrganisationCode)
        //{
        //    try
        //    {
        //        courseWiseEmail.CreatedBy = UserId;
        //        courseWiseEmail.CreatedDate = DateTime.UtcNow;
        //        courseWiseEmail.ModifiedBy = UserId;
        //        courseWiseEmail.ModifiedDate = DateTime.UtcNow;
        //        courseWiseEmail.TotalUserCount = await GetAbsentUserDetails(courseWiseEmail.CourseId, true);
        //        this._db.CourseWiseEmailReminder.Add(courseWiseEmail);
        //        await this._db.SaveChangesAsync();


        //        Task.Run(() => _email.SendRemainderMailForCourse(courseWiseEmail.CourseId, OrganisationCode));

        //        return (courseWiseEmail);

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return courseWiseEmail;
        //    }
        //}

        //public async Task<UserWiseCourseEmailReminder> AddUsercoursewise(UserWiseCourseEmailReminder courseWiseEmail, int UserId, string OrganisationCode)
        //{
        //    try
        //    {
        //        courseWiseEmail.CreatedBy = UserId;
        //        courseWiseEmail.CreatedDate = DateTime.UtcNow;
        //        courseWiseEmail.ModifiedBy = UserId;
        //        courseWiseEmail.ModifiedDate = DateTime.UtcNow;
        //        courseWiseEmail.TotalUserCount = await GetAbsentUserDetails(courseWiseEmail.CourseId, true);
        //        this._db.UserWiseCourseEmailReminder.Add(courseWiseEmail);
        //        await this._db.SaveChangesAsync();


        //        Task.Run(() => _email.SendRemainderMailForUserWiseCourse(courseWiseEmail.UserId, courseWiseEmail.CourseId, OrganisationCode));

        //        return (courseWiseEmail);

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return courseWiseEmail;
        //    }
        //}

        //public async Task<CourseWiseSMSReminder> AddcourseWiseSMS(CourseWiseSMSReminder courseWiseSMS, int UserId, string OrganisationCode)
        //{
        //    courseWiseSMS.CreatedBy = UserId;
        //    courseWiseSMS.CreatedDate = DateTime.Now;
        //    courseWiseSMS.ModifiedBy = UserId;
        //    courseWiseSMS.ModifiedDate = DateTime.Now;
        //    courseWiseSMS.TotalUserCount = await GetTotalApplicableUserCount(courseWiseSMS.CourseId, true);
        //    this._db.CourseWiseSMSReminder.Add(courseWiseSMS);
        //    await this._db.SaveChangesAsync();
        //    // SMS Notification
        //    _ = Task.Run(() => _email.SendRemainderSMSForCourse(courseWiseSMS.CourseId, OrganisationCode));

        //    return (courseWiseSMS);
        //}

        //public async Task<int> GetTotalApplicableUserCount(int CourseID, bool flag = false)
        //{
        //    CourseWiseSMSReminder UserCount = new CourseWiseSMSReminder();
        //    try
        //    {
        //        using (var dbContext = this._customerConnection.GetDbContext())
        //        {
        //            using (var connection = dbContext.Database.GetDbConnection())
        //            {
        //                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                    connection.Open();

        //                DynamicParameters parameters = new DynamicParameters();
        //                parameters.Add("@CourseId", CourseID);
        //                parameters.Add("@FlagCount", flag);

        //                var Result = await SqlMapper.QueryAsync<CourseWiseSMSReminder>((SqlConnection)connection, "[dbo].[GetUsersForRemainderSMS]", parameters, null, null, CommandType.StoredProcedure);
        //                UserCount = Result.FirstOrDefault();
        //                connection.Close();

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw ex;
        //    }
        //    return UserCount.TotalUserCount;
        //}

        //public async Task<List<APIJobRole>> GetCompetencySkill(int CourseId)
        //{

        //    List<APIJobRole> resultCompetencySkill = (from compmapping in this._db.CompetenciesMapping
        //                                              join c in this._db.CompetenciesMaster on compmapping.CompetencyId equals c.Id
        //                                              where c.IsDeleted == false && compmapping.IsDeleted == false && compmapping.CourseId == CourseId
        //                                              select new APIJobRole
        //                                              {
        //                                                  Id = c.Id,
        //                                                  Name = c.CompetencyName

        //                                              }).ToList();

        //    return resultCompetencySkill;
        //}

        //public async Task<List<APISubSubCategory>> GetSubSubCategory(int CourseId)
        //{

        //    List<APISubSubCategory> resultSubSubCategory = (from course in this._db.Course
        //                                              join sscexternal in this._db.ExternalCourseCategoryAssociation on course.Id equals sscexternal.CourseId
        //                                              join ssc in this._db.SubSubCategory on sscexternal.SubSubCategoryId equals ssc.Id
        //                                              where course.IsDeleted == false && sscexternal.IsDeleted == false && sscexternal.CourseId == CourseId
        //                                              select new APISubSubCategory
        //                                              {
        //                                                  Id = ssc.Id,
        //                                                  Name = ssc.Name

        //                                              }).ToList();

        //    return resultSubSubCategory;
        //}

        //public async Task<ApiAssignmentDetails> AddAssignmentDetails(ApiAssignmentDetails apiAssignmentDetails)
        //{
        //    try
        //    {
        //        Model.AssignmentDetails assignmentDetails = Mapper.Map<Model.AssignmentDetails>(apiAssignmentDetails);
        //        assignmentDetails.CreatedBy = apiAssignmentDetails.UserId;
        //        assignmentDetails.CreatedDate = DateTime.UtcNow;
        //        assignmentDetails.ModifiedBy = apiAssignmentDetails.UserId;
        //        assignmentDetails.ModifiedDate = DateTime.UtcNow;
        //        assignmentDetails.IsActive = true;
        //        assignmentDetails.IsDeleted = false;
        //        assignmentDetails.Remark = "";
        //        this._db.AssignmentDetails.Add(assignmentDetails);
        //        await this._db.SaveChangesAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return apiAssignmentDetails;
        //    }

        //    return apiAssignmentDetails;
        //}

        //public async Task<ApiAssignmentDetails> UpdateRejectedAssignmentDetails(ApiAssignmentDetails apiAssignmentDetails)
        //{
        //    try
        //    {
        //        Model.AssignmentDetails assignmentDetails = Mapper.Map<Model.AssignmentDetails>(apiAssignmentDetails);
        //        assignmentDetails.ModifiedDate = DateTime.UtcNow;
        //        assignmentDetails.CreatedDate = apiAssignmentDetails.CreatedDate;
        //        assignmentDetails.CreatedBy = apiAssignmentDetails.UserId;
        //        assignmentDetails.IsActive = true;
        //        assignmentDetails.IsDeleted = false;
        //        this._db.AssignmentDetails.Update(assignmentDetails);
        //        await this._db.SaveChangesAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return apiAssignmentDetails;
        //    }
        //    return apiAssignmentDetails;
        //}


        //public async Task<AssignmentDetails> GetAssignmentDetail(int id)
        //{
        //    AssignmentDetails assignmentDetail = await this._db.AssignmentDetails.Where(Assignment => Assignment.Id == id).FirstOrDefaultAsync();
        //    return assignmentDetail;
        //}


        //public async Task<List<ApiAssignmentInfo>> GetAssignmentDetails(int loginUserId, SearchAssignmentDetails searchAssignmentDetails)
        //{

        //    if (!string.IsNullOrEmpty(searchAssignmentDetails.ColumnName) && (searchAssignmentDetails.ColumnName.ToLower().Equals("emailid") || searchAssignmentDetails.ColumnName.ToLower().Equals("userid") || searchAssignmentDetails.ColumnName.ToLower().Equals("mobilenumber")))
        //    {
        //        if (!string.IsNullOrEmpty(searchAssignmentDetails.SearchText))
        //        {
        //            searchAssignmentDetails.SearchText = Security.Encrypt(searchAssignmentDetails.SearchText.ToLower());
        //        }
        //    }
        //    List<ApiAssignmentInfo> apiAssignmentInfo = new List<ApiAssignmentInfo>();
        //    var connection = this._db.Database.GetDbConnection();//Dont use Using statement for Connection variable            
        //    try
        //    {
        //        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //            connection.Open();
        //        using (var cmd = connection.CreateCommand())
        //        {
        //            cmd.CommandText = "GetAssignmentDetails";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.Int) { Value = searchAssignmentDetails.Page });
        //            cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = searchAssignmentDetails.PageSize });
        //            cmd.Parameters.Add(new SqlParameter("@loginUserId", SqlDbType.Int) { Value = loginUserId });
        //            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = searchAssignmentDetails.UserId });
        //            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = searchAssignmentDetails.CourseId });
        //            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = searchAssignmentDetails.Status });
        //            cmd.Parameters.Add(new SqlParameter("@ColumnName", SqlDbType.NVarChar) { Value = searchAssignmentDetails.ColumnName });
        //            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchAssignmentDetails.SearchText });

        //            DbDataReader reader = await cmd.ExecuteReaderAsync();
        //            DataTable dt = new DataTable();
        //            dt.Load(reader);
        //            if (dt.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    ApiAssignmentInfo apiAssignment = new ApiAssignmentInfo();
        //                    apiAssignment.AssignmentId = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : Convert.ToInt32(row["Id"].ToString());
        //                    apiAssignment.CourseId = string.IsNullOrEmpty(row["CourseId"].ToString()) ? 0 : Convert.ToInt32(row["CourseId"].ToString());
        //                    apiAssignment.UserId = string.IsNullOrEmpty(row["UserId"].ToString()) ? 0 : Convert.ToInt32(row["UserId"].ToString());
        //                    apiAssignment.UserName = row["UserName"].ToString();
        //                    apiAssignment.UserNameId = Security.Decrypt(row["UserNameID"].ToString());
        //                    apiAssignment.CourseName = row["Title"].ToString();
        //                    apiAssignment.AssignmentName = row["Name"].ToString();
        //                    apiAssignment.FilePath = row["FilePath"].ToString();
        //                    apiAssignment.FileType = row["FileType"].ToString();
        //                    apiAssignment.TextAnswer = row["TextAnswer"].ToString();
        //                    apiAssignment.Status = row["Status"].ToString();
        //                    apiAssignment.Remark = row["Remark"].ToString();
        //                    apiAssignment.ModifiedDate = string.IsNullOrEmpty(row["ModifiedDate"].ToString()) ? null : row["ModifiedDate"].ToString();
        //                    apiAssignment.CreatedDate = string.IsNullOrEmpty(row["CreatedDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(row["CreatedDate"].ToString());
        //                    apiAssignmentInfo.Add(apiAssignment);
        //                }
        //            }
        //            reader.Dispose();
        //        }
        //        connection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        if (connection.State == ConnectionState.Open)
        //            connection.Close();
        //        string exception = ex.Message;
        //    }

        //    return apiAssignmentInfo;
        //}


        //public async Task<int> GetAssignmentDetailsCount(int loginUserId, SearchAssignmentDetails searchAssignmentDetails)
        //{
        //    int Count = 0;
        //    var connection = this._db.Database.GetDbConnection();//Dont use Using statement for Connection variable            
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(searchAssignmentDetails.ColumnName) && (searchAssignmentDetails.ColumnName.ToLower().Equals("emailid") || searchAssignmentDetails.ColumnName.ToLower().Equals("userid") || searchAssignmentDetails.ColumnName.ToLower().Equals("mobilenumber")))
        //        {
        //            if (!string.IsNullOrEmpty(searchAssignmentDetails.SearchText))
        //            {
        //                searchAssignmentDetails.SearchText = Security.Encrypt(searchAssignmentDetails.SearchText.ToLower());
        //            }
        //        }

        //        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //            connection.Open();
        //        using (var cmd = connection.CreateCommand())
        //        {
        //            cmd.CommandText = "GetAssignmentDetailsCount";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@loginUserId", SqlDbType.Int) { Value = loginUserId });
        //            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = searchAssignmentDetails.UserId });
        //            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = searchAssignmentDetails.CourseId });
        //            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = searchAssignmentDetails.Status });
        //            cmd.Parameters.Add(new SqlParameter("@ColumnName", SqlDbType.NVarChar) { Value = searchAssignmentDetails.ColumnName });
        //            cmd.Parameters.Add(new SqlParameter("@SearchText", SqlDbType.NVarChar) { Value = searchAssignmentDetails.SearchText });

        //            DbDataReader reader = await cmd.ExecuteReaderAsync();
        //            DataTable dt = new DataTable();
        //            dt.Load(reader);
        //            if (dt.Rows.Count > 0)
        //            {
        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    Count = string.IsNullOrEmpty(row["Count"].ToString()) ? 0 : int.Parse(row["Count"].ToString());
        //                }
        //            }
        //            reader.Dispose();
        //        }
        //        connection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        if (connection.State == ConnectionState.Open)
        //            connection.Close();
        //        string exception = ex.Message;
        //    }
        //    return Count;
        //}

        //public async Task<ApiAssignmentDetails> GetAssignmentDetailsById(SearchAssignmentDetails searchAssignmentDetails)
        //{
        //    var assignmentDetails = await this._db.AssignmentDetails.Where(Assignment => Assignment.UserId == searchAssignmentDetails.
        //    UserId && Assignment.CourseId == searchAssignmentDetails.CourseId).FirstOrDefaultAsync();
        //    ApiAssignmentDetails apiAssignmentDetails = Mapper.Map<ApiAssignmentDetails>(assignmentDetails);
        //    apiAssignmentDetails.CreatedDate = assignmentDetails.CreatedDate;
        //    apiAssignmentDetails.CreatedBy = assignmentDetails.CreatedBy;
        //    apiAssignmentDetails.ModifiedBy = assignmentDetails.ModifiedBy;
        //    apiAssignmentDetails.ModifiedDate = assignmentDetails.ModifiedDate;
        //    return apiAssignmentDetails;
        //}

        //public async Task<bool> IsAssignmentSubmitted(SearchAssignmentDetails searchAssignmentDetails)
        //{
        //    var count = await this._db.AssignmentDetails.Where(Assignment => Assignment.AssignmentId == searchAssignmentDetails.AssignmentId
        //     && Assignment.CourseId == searchAssignmentDetails.CourseId
        //     && Assignment.UserId == searchAssignmentDetails.UserId).CountAsync();

        //    if (count > 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}


// Dont Comment
        public async Task<string> SaveFile(IFormFile uploadedFile, string fileType, string OrganizationCode)
        {
            try
            {

                var EnableBlobStorage = await GetMasterConfigurableParameterValue("Enable_BlobStorage");

                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                {
                    var request = _httpContextAccessor.HttpContext.Request;
                    string FilePath = string.Empty;
                    string ReturnFilePath = string.Empty;
                    string FileName = string.Empty;
                    string coursesPath = this._configuration["ApiGatewayWwwroot"];

                    coursesPath = Path.Combine(coursesPath, OrganizationCode, fileType);
                    ReturnFilePath = Path.Combine(OrganizationCode, fileType);

                    if (!Directory.Exists(coursesPath))
                    {
                        Directory.CreateDirectory(coursesPath);
                    }
                    FileName = DateTime.Now.Ticks + uploadedFile.FileName.Trim();
                    FilePath = Path.Combine(coursesPath, FileName);
                    ReturnFilePath = Path.Combine(ReturnFilePath, FileName);

                    FilePath = string.Concat(FilePath.Split(' '));
                    ReturnFilePath = string.Concat(ReturnFilePath.Split(' '));

                    using (var fs = new FileStream(Path.Combine(FilePath), FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fs);
                    }
                    ReturnFilePath = "/" + ReturnFilePath;
                    return ReturnFilePath;
                }
                else
                {
                    try
                    {
                        BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, OrganizationCode, fileType);
                        if (res != null)
                        {
                            if (res.Error == false)
                            {
                                string file = res.Blob.Name.ToString();
                                file = "/" + file;
                                return file;
                            }
                            else
                            {
                                _logger.Error(res.ToString());
                                
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        return null;
                    }
                   
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return "";
            }
        }


        //public async Task<int> GetAbsentUserDetails(int CourseID, bool flag = false)
        //{
        //    CourseWiseEmailReminder apiAbsentUserDetails = new CourseWiseEmailReminder();
        //    try
        //    {
        //        using (var dbContext = this._customerConnection.GetDbContext())
        //        {
        //            using (var connection = dbContext.Database.GetDbConnection())
        //            {
        //                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                    connection.Open();

        //                DynamicParameters parameters = new DynamicParameters();
        //                parameters.Add("@CourseId", CourseID);
        //                parameters.Add("@FlagCount", flag);

        //                var Result = await SqlMapper.QueryAsync<CourseWiseEmailReminder>((SqlConnection)connection, "[dbo].[GetUsersForRemainderMail]", parameters, null, null, CommandType.StoredProcedure);
        //                apiAbsentUserDetails = Result.FirstOrDefault();
        //                connection.Close();

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw ex;
        //    }
        //    return apiAbsentUserDetails.TotalUserCount;
        //}

        //public async Task<APICourseTypewithCount> GetCourseTypewiseCount()
        //{
        //    try
        //    {
        //        List<APICourseTypewithCount> Query = await (from p in this._db.Course
        //                                                    group p by p.CourseType into g
        //                                                    select new APICourseTypewithCount { CourseType = g.Key, Count = g.Count() }).Distinct().ToListAsync();

        //        APICourseTypewithCount aPICourseTypewithCount = new APICourseTypewithCount();
        //        foreach (APICourseTypewithCount item in Query)
        //        {
        //            if (item.CourseType.ToString().ToLower() == "vilt")
        //                aPICourseTypewithCount.WebinarCount = item.Count;
        //            if (item.CourseType.ToString().ToLower() == "elearning")
        //                aPICourseTypewithCount.ElearningCount = item.Count;
        //            if (item.CourseType.ToString().ToLower() == "blended")
        //                aPICourseTypewithCount.BlendedCount = item.Count;
        //            if (item.CourseType.ToString().ToLower() == "classroom")
        //                aPICourseTypewithCount.ClassroomCount = item.Count;
        //            if (item.CourseType.ToString().ToLower() == "certification")
        //                aPICourseTypewithCount.CertificationCount = item.Count;
        //        }

        //        return aPICourseTypewithCount;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return null;
        //    }
        //}

        //public async Task<Courses.API.Model.Course> GetCourseInfoByCourseCode(string CourseCode)
        //{
        //    Courses.API.Model.Course courseInfo = _db.Course.Where(c => c.Code == CourseCode).FirstOrDefault();
        //    return courseInfo;
        //}

        //public async Task<Courses.API.Model.Course> GetCourseInfoByCourseName(string courseName)
        //{
        //    Courses.API.Model.Course courseInfo = _db.Course.Where(c => c.Title == courseName).FirstOrDefault();
        //    return courseInfo;
        //}
        //public async Task<APICourseTypeCount> GetCourseTypewiseCountNew()
        //{
        //    try
        //    {

        //        APICourseTypeCount aPICourseTypewithCount = new APICourseTypeCount();
        //        aPICourseTypewithCount.ElearningCount = await this.Count(null, null, "elearning", "coursetype");
        //        aPICourseTypewithCount.ClassroomCount = await this.Count(null, null, "classroom", "coursetype");
        //        aPICourseTypewithCount.WebinarCount = await this.Count(null, null, "vilt", "coursetype");
        //        aPICourseTypewithCount.BlendedCount = await this.Count(null, null, "blended", "coursetype");
        //        aPICourseTypewithCount.AssessmentCount = await this.Count(null, null, "certification", "coursetype");


        //        return aPICourseTypewithCount;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        return null;
        //    }
        //}

        //public async Task<bool> IsEmailConfigured(string orgCode)
        //{
        //    bool IsEmailConfigured = false;
        //    string EmailConfigurationName = "EMAIL_NOTIFICATION";
        //    try
        //    {
        //        using (var dbContext = this._customerConnection.GetDbContext())
        //        {
        //            using (var connection = dbContext.Database.GetDbConnection())
        //            {
        //                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        //                    connection.Open();

        //                using (var cmd = connection.CreateCommand())
        //                {
        //                    cmd.CommandText = "GetConfigurableParameterValue";
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = EmailConfigurationName });
        //                    DbDataReader reader = await cmd.ExecuteReaderAsync();
        //                    DataTable dt = new DataTable();
        //                    dt.Load(reader);
        //                    if (dt.Rows.Count > 0)
        //                    {
        //                        foreach (DataRow row in dt.Rows)
        //                        {
        //                            string value = row["Value"].ToString();
        //                            if (value != null)
        //                            {
        //                                if (value.ToLower().Equals("yes"))
        //                                {
        //                                    IsEmailConfigured = true;
        //                                }
        //                                else
        //                                {
        //                                    IsEmailConfigured = false;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    reader.Dispose();
        //                }
        //                connection.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw;
        //    }
        //    return IsEmailConfigured;
        //}


        //public async Task<List<APICourseCategoryTypeahead>> GetCourseIdByCourseCategory(int? Id = null)
        //{
        //    List<APICourseCategoryTypeahead> resultCourseSkill = (from Course in this._db.Course

        //                                                          where Course.IsActive == true && (Course.CategoryId == Id || Id == null)
        //                                                          select new APICourseCategoryTypeahead
        //                                                          {
        //                                                              ID = Course.Id,
        //                                                              Name = Course.Title

        //                                                          }).ToList();

        //    return resultCourseSkill;
        //}

        //public async Task<APIJobRole> GetPrerequisiteCourseByCourseId(int CourseId)
        //{

        //    APIJobRole resultPrerequisiteCourse = (from Courses in this._db.Course
        //                                           join c in this._db.Course on Courses.prerequisiteCourse equals c.Id
        //                                           where c.IsDeleted == false && Courses.Id == CourseId
        //                                           select new APIJobRole
        //                                           {
        //                                               Id = c.Id,
        //                                               Name = c.Title

        //                                           }).FirstOrDefault();

        //    return resultPrerequisiteCourse;
        //}

        //public async Task<APIPrerequisiteCourseStatus> GetPrerequisiteCourseStatus(APIPreRequisiteCourseStatus preRequisiteCourseStatus, int UserId)
        //{

        //    APIPrerequisiteCourseStatus resultPrerequisiteStatus = (from Courses in this._db.Course
        //                                                            join c in this._db.Course on Courses.prerequisiteCourse equals c.Id
        //                                                            where c.IsDeleted == false && Courses.Id == preRequisiteCourseStatus.CourseID
        //                                                            select new APIPrerequisiteCourseStatus
        //                                                            {
        //                                                                Id = c.Id,
        //                                                                Name = c.Title

        //                                                            }).FirstOrDefault();


        //    try
        //    {
        //        string status = (from Comp in _db.CourseCompletionStatus
        //                         where (Comp.CourseId == resultPrerequisiteStatus.Id && Comp.UserId == UserId)
        //                         select Comp.Status).FirstOrDefault();

        //        resultPrerequisiteStatus.CourseStatus = string.IsNullOrEmpty(status) ? "notstarted" : status;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw ex;
        //    }

        //    return resultPrerequisiteStatus;
        //}

        //public async Task<List<APICourseTagged>> GetTagged(int? page = null, int? pageSize = null, string search = null)
        //{
        //    try
        //    {
        //        APICourseTagged aPICourseTaggeds = new APICourseTagged();
        //        List<APICourseTagged> aPICourseTaggedsList = new List<APICourseTagged>();

        //        var TaggList = await _db.Course.Where(r => r.Metadata != null && r.Metadata != "" && r.IsDeleted == false && r.IsActive == true).Select(r => r.Metadata).Distinct().ToListAsync();
        //        int id = 1;
        //        for (int i = 0; i < TaggList.Count(); i++)
        //        {

        //            aPICourseTaggeds = new APICourseTagged();
        //            string Tagg = TaggList[i];
        //            if (Tagg.Contains(","))
        //            {
        //                string[] TaggArr = Tagg.Split(",");
        //                for (int j = 0; j < TaggArr.Length; j++)
        //                {
        //                    aPICourseTaggeds = new APICourseTagged();
        //                    aPICourseTaggeds.Id = id;
        //                    id++;
        //                    aPICourseTaggeds.TagName = TaggArr[j];

        //                    aPICourseTaggedsList.Add(aPICourseTaggeds);
        //                }
        //            }
        //            else
        //            {
        //                aPICourseTaggeds.Id = id;
        //                id++;
        //                aPICourseTaggeds.TagName = TaggList[i];
        //                aPICourseTaggedsList.Add(aPICourseTaggeds);
        //            }

        //        }
        //        List<APICourseTagged> apiCourse = aPICourseTaggedsList.OrderBy(r => r.TagName).GroupBy(r => r.TagName).Select(r => r.First()).ToList();
        //        return apiCourse;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw;
        //    }
        //}

        //public async Task<CourseLog> AddCourseLog(CourseLog courseLog)
        //{
        //    try
        //    {
        //        this._db.CourseLog.Add(courseLog);
        //        await this._db.SaveChangesAsync();
        //        return (courseLog);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw ex;
        //    }
        //}
// Dont comment
        public async Task<string> GetConfigurationValueAsync(string configurationCode, string orgCode, string defaultValue = "")
        {
            DataTable dtConfigurationValues;
            string configValue;
            try
            {
                var cache = new CacheManager.CacheManager();
                string cacheKeyConfig = (Constants.CONFIGURABLE_VALUES + "-" + orgCode).ToUpper();

                if (cache.IsAdded(cacheKeyConfig))
                    dtConfigurationValues = cache.Get<DataTable>(cacheKeyConfig);
                else
                {
                    dtConfigurationValues = this.GetAllConfigurableParameterValue();
                    cache.Add(cacheKeyConfig, dtConfigurationValues, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                }
                DataRow[] dr = dtConfigurationValues.Select("Code ='" + configurationCode + "'");
                if (dr.Length > 0)
                    configValue = Convert.ToString(dr[0]["Value"]);
                else
                    configValue = defaultValue;
                _logger.Debug(string.Format("Value for Config {0} for Organization {1} is {2} ", configurationCode, orgCode, configValue));
            }
            catch (System.Exception ex)
            {
                _logger.Error(string.Format("Exception in function GetConfigurationValueAsync :- {0} for Organisation {1}", Utilities.GetDetailedException(ex), orgCode));
                return null;
            }
            return configValue;
        }
        public DataTable GetAllConfigurableParameterValue()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetAllConfigurableParameterValues";
                        cmd.CommandType = CommandType.StoredProcedure;
                        DbDataReader reader = cmd.ExecuteReader();
                        dt.Load(reader);

                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (System.Exception ex)
            { _logger.Error("Exception in function GetAllConfigurableParameterValue :-" + Utilities.GetDetailedException(ex)); }

            return dt;
        }

        //public async Task<bool> IsRetrainindDaysEnable(int courseId)
        //{
        //    var count = this._db.CourseCompletionStatus.Where(c => c.CourseId == courseId && c.IsDeleted == Record.NotDeleted).Count();
        //    if (count > 0)
        //        return true;

        //    var hcount = this._db.CourseCompletionStatusHistory.Where(c => c.CourseId == courseId && c.IsDeleted == Record.NotDeleted).Count();
        //    if (hcount > 0)
        //        return true;

        //    return false;

        //}

        public async Task<List<string>> GetCourseName(int[] courseId)
        {
            return await this._db.Course.Where(x => courseId.Contains(x.Id)).Select(x => x.Title).ToListAsync();
        }
        //public async Task<APICourseResponse> GetNodalCourses(APINodalCourses aPINodalCourses, string ConnectionString)
        //{
        //    APICourseResponse aPICourseResponse = new APICourseResponse();
        //    List<APINodalCourseInfo> aPINodalCourseInfos = new List<APINodalCourseInfo>();
        //    using (CourseContext context = _customerConnection.GetDbContext(ConnectionString))
        //    {

        //        var courses = (from c in context.Course
        //                       join cm in context.CourseModuleAssociation on c.Id equals cm.CourseId
        //                       join m in context.Module on cm.ModuleId equals m.Id
        //                       where c.IsActive == true && c.IsDeleted == false
        //                       && cm.Isdeleted == false && m.IsDeleted == false
        //                       && m.ModuleType == "SCORM"
        //                       group new { c, cm, m } by new
        //                       {
        //                           c.Id,
        //                           c.Code,
        //                           c.Title,
        //                           c.CourseType,
        //                           c.Description,
        //                           c.ThumbnailPath,
        //                           c.Currency,
        //                           c.CourseFee
        //                       } into CourseGrp
        //                       orderby CourseGrp.Key.Title
        //                       select new APINodalCourseInfo
        //                       {
        //                           Id = CourseGrp.Key.Id,
        //                           Code = CourseGrp.Key.Code,
        //                           Title = CourseGrp.Key.Title,
        //                           CourseType = CourseGrp.Key.CourseType,
        //                           Description = CourseGrp.Key.Description != null ? CourseGrp.Key.Description : "No information provided.",
        //                           ThumbnailPath = CourseGrp.Key.ThumbnailPath,
        //                           Currency = CourseGrp.Key.Currency,
        //                           Cost = CourseGrp.Key.CourseFee
        //                       });

        //        if (!string.IsNullOrEmpty(aPINodalCourses.Search) && !string.IsNullOrEmpty(aPINodalCourses.SearchText))
        //        {
        //            if (aPINodalCourses.Search.ToLower().Equals("coursecode"))
        //                courses = courses.Where(x => x.Code.StartsWith(aPINodalCourses.SearchText));
        //            else if (aPINodalCourses.Search.ToLower().Equals("coursetitle"))
        //                courses = courses.Where(x => x.Title.StartsWith(aPINodalCourses.SearchText));
        //        }

        //        aPINodalCourseInfos = await courses.Skip((aPINodalCourses.Page - 1) * aPINodalCourses.PageSize).Take(aPINodalCourses.PageSize).ToListAsync();
        //        aPICourseResponse.TotalRecords = await courses.Select(x => x.Id).CountAsync();
        //        aPICourseResponse.aPINodalCourseInfos = aPINodalCourseInfos;
        //        return aPICourseResponse;
        //    }
        //}
        //public async Task<APICourseDetailsTigerhall> GetCourseDetailsTigerhall(List<APITigerhallCourses> courses)
        //{
        //    APICourseDetailsTigerhall CourseInfo = new APICourseDetailsTigerhall();
        //    List<string> successCourseID = new List<string>();
        //    List<string> failedCourseID = new List<string>();
        //    successDataPoints sID = new successDataPoints();
        //    failedDataPoints fID = new failedDataPoints();
        //    try
        //    {
        //        string ConnectionString = this._configuration.GetConnectionString("DefaultConnection");
        //        foreach (var item in courses)
        //        {
        //            foreach (var course in item.courses)
        //            {
        //                foreach (var content in course.contents)
        //                {
        //                    using (var dbContext = this._customerConnection.GetDbContext(ConnectionString))
        //                    {
        //                        using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
        //                        {
        //                            _logger.Debug("Inserting Records for TigerHall Courses : " + course.courseId.ToString());
        //                            cmd.CommandText = "InsertTigerhallCourses";
        //                            cmd.CommandType = CommandType.StoredProcedure;
        //                            cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.NVarChar) { Value = course.courseId });
        //                            cmd.Parameters.Add(new SqlParameter("@courseName", SqlDbType.NVarChar) { Value = course.courseName });
        //                            cmd.Parameters.Add(new SqlParameter("@totalContentPieces", SqlDbType.Int) { Value = course.totalContentPieces });
        //                            cmd.Parameters.Add(new SqlParameter("@contentId", SqlDbType.NVarChar) { Value = content.contentId });
        //                            cmd.Parameters.Add(new SqlParameter("@contentType", SqlDbType.NVarChar) { Value = content.contentType });
        //                            cmd.Parameters.Add(new SqlParameter("@inAppLink", SqlDbType.NVarChar) { Value = content.inAppLink });
        //                            cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar) { Value = content.category.id });
        //                            cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar) { Value = content.category.name });
        //                            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true });
        //                            await dbContext.Database.OpenConnectionAsync();
        //                            DbDataReader reader = await cmd.ExecuteReaderAsync();
        //                            DataTable dt = new DataTable();
        //                            dt.Load(reader);
        //                            foreach (DataRow row in dt.Rows)
        //                            {
        //                                if (row["SuccessCourseID"] != DBNull.Value)
        //                                {
        //                                    successCourseID.Add(row["SuccessCourseID"].ToString());
        //                                }
        //                                if (row["failCourseID"] != DBNull.Value)
        //                                {
        //                                    failedCourseID.Add(row["failCourseID"].ToString());
        //                                }
        //                            }
        //                            reader.Dispose();
        //                            await dbContext.Database.CloseConnectionAsync();
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (successCourseID.Count > 0)
        //        {
        //            successCourseID = successCourseID.Distinct().ToList();
        //            sID.CourseID = successCourseID;
        //            CourseInfo.successDataPoints = new List<successDataPoints>();
        //            CourseInfo.successDataPoints.Add(sID);

        //        }
        //        if (failedCourseID.Count > 0)
        //        {
        //            fID.CourseID = failedCourseID;
        //            CourseInfo.failedDataPoints = new List<failedDataPoints>();
        //            CourseInfo.failedDataPoints.Add(fID);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //    }
        //    return CourseInfo;
        //}
        //public async Task<APICourseResponse> GetNodalCoursesForGroupAdmin(APINodalCoursesGroupAdmin aPINodalCoursesGroupAdmin)
        //{
        //    APICourseResponse aPICourseResponse = new APICourseResponse();
        //    List<APINodalCourseInfo> aPINodalCourseInfos = new List<APINodalCourseInfo>();

        //    var courses = (from c in _db.Course
        //                   join cm in _db.CourseModuleAssociation on c.Id equals cm.CourseId
        //                   join m in _db.Module on cm.ModuleId equals m.Id
        //                   where c.IsActive == true && c.IsDeleted == false
        //                   && cm.Isdeleted == false && m.IsDeleted == false
        //                   && m.ModuleType == "SCORM"
        //                   group new { c, cm, m } by new
        //                   {
        //                       c.Id,
        //                       c.Code,
        //                       c.Title,
        //                       c.CourseType,
        //                       c.Description,
        //                       c.ThumbnailPath,
        //                       c.Currency,
        //                       c.CourseFee
        //                   } into CourseGrp
        //                   orderby CourseGrp.Key.Title
        //                   select new APINodalCourseInfo
        //                   {
        //                       Id = CourseGrp.Key.Id,
        //                       Code = CourseGrp.Key.Code,
        //                       Title = CourseGrp.Key.Title,
        //                       CourseType = CourseGrp.Key.CourseType,
        //                       Description = CourseGrp.Key.Description != null ? CourseGrp.Key.Description : "No information provided.",
        //                       ThumbnailPath = CourseGrp.Key.ThumbnailPath,
        //                       Currency = CourseGrp.Key.Currency,
        //                       Cost = CourseGrp.Key.CourseFee
        //                   });

        //    if (!string.IsNullOrEmpty(aPINodalCoursesGroupAdmin.Search) && !string.IsNullOrEmpty(aPINodalCoursesGroupAdmin.SearchText))
        //    {
        //        if (aPINodalCoursesGroupAdmin.Search.ToLower().Equals("coursecode"))
        //            courses = courses.Where(x => x.Code.StartsWith(aPINodalCoursesGroupAdmin.SearchText));
        //        else if (aPINodalCoursesGroupAdmin.Search.ToLower().Equals("coursetitle"))
        //            courses = courses.Where(x => x.Title.StartsWith(aPINodalCoursesGroupAdmin.SearchText));
        //    }

        //    aPINodalCourseInfos = await courses.Skip((aPINodalCoursesGroupAdmin.Page - 1) * aPINodalCoursesGroupAdmin.PageSize).Take(aPINodalCoursesGroupAdmin.PageSize).ToListAsync();
        //    aPICourseResponse.TotalRecords = await courses.Select(x => x.Id).CountAsync();
        //    aPICourseResponse.aPINodalCourseInfos = aPINodalCourseInfos;
        //    return aPICourseResponse;
        //}
        //public async Task<List<APINodalCourseTypeahead>> GetNodalCourseTypeahead(string Search = null)
        //{
        //    List<APINodalCourseTypeahead> aPINodalCourseTypeahead = new List<APINodalCourseTypeahead>();
        //    var courses = (from c in _db.Course
        //                   join cm in _db.CourseModuleAssociation on c.Id equals cm.CourseId
        //                   join m in _db.Module on cm.ModuleId equals m.Id
        //                   where c.IsActive == true && c.IsDeleted == false
        //                   && cm.Isdeleted == false && m.IsDeleted == false
        //                   && m.ModuleType == "SCORM"
        //                   group new { c, cm, m } by new
        //                   {
        //                       c.Id,
        //                       c.Title
        //                   } into CourseGrp
        //                   orderby CourseGrp.Key.Title
        //                   select new APINodalCourseTypeahead
        //                   {
        //                       CourseId = CourseGrp.Key.Id,
        //                       CourseTitle = CourseGrp.Key.Title
        //                   });

        //    if (Search != null)
        //        courses = courses.Where(c => c.CourseTitle.ToLower().StartsWith(Search.ToLower()));

        //    aPINodalCourseTypeahead = await courses.ToListAsync();
        //    return aPINodalCourseTypeahead;
        //}
        //public async Task<List<APINodalCourseTypeahead>> GetNodalCoursesDropdown()
        //{
        //    List<APINodalCourseTypeahead> aPINodalCourseTypeahead = new List<APINodalCourseTypeahead>();
        //    var courses = (from c in _db.Course
        //                   join cm in _db.CourseModuleAssociation on c.Id equals cm.CourseId
        //                   join m in _db.Module on cm.ModuleId equals m.Id
        //                   where c.IsActive == true && c.IsDeleted == false
        //                   && cm.Isdeleted == false && m.IsDeleted == false
        //                   && m.ModuleType == "SCORM"
        //                   group new { c, cm, m } by new
        //                   {
        //                       c.Id,
        //                       c.Title
        //                   } into CourseGrp
        //                   orderby CourseGrp.Key.Title
        //                   select new APINodalCourseTypeahead
        //                   {
        //                       CourseId = CourseGrp.Key.Id,
        //                       CourseTitle = CourseGrp.Key.Title
        //                   });

        //    return await courses.ToListAsync();
        //}
        public async Task InsertCourseMasterAuditLog(Model.Course course, string action)
        {

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
                            cmd.CommandText = "InsertCourseMasterAuditlog";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = course.Id });
                            cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = course.CategoryId });
                            cmd.Parameters.Add(new SqlParameter("@Code", SqlDbType.NVarChar) { Value = course.Code });
                            cmd.Parameters.Add(new SqlParameter("@CompletionPeriodDays", SqlDbType.Int) { Value = course.CompletionPeriodDays });
                            cmd.Parameters.Add(new SqlParameter("@CourseAdminID", SqlDbType.Int) { Value = course.CourseAdminID });
                            cmd.Parameters.Add(new SqlParameter("@CourseFee", SqlDbType.Real) { Value = course.CourseFee });
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = course.CreatedBy });
                            cmd.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = course.CreatedDate });
                            cmd.Parameters.Add(new SqlParameter("@CreditsPoints", SqlDbType.Real) { Value = course.CreditsPoints });
                            cmd.Parameters.Add(new SqlParameter("@Currency", SqlDbType.NVarChar) { Value = course.Currency });
                            cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar) { Value = course.Description });
                            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = course.IsActive });
                            cmd.Parameters.Add(new SqlParameter("@IsCertificateIssued", SqlDbType.Bit) { Value = course.IsCertificateIssued });
                            cmd.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = course.IsDeleted });
                            cmd.Parameters.Add(new SqlParameter("@IsPreAssessment", SqlDbType.Bit) { Value = course.IsPreAssessment });
                            cmd.Parameters.Add(new SqlParameter("@Language", SqlDbType.NVarChar) { Value = course.Language });
                            cmd.Parameters.Add(new SqlParameter("@LearningApproach", SqlDbType.Bit) { Value = course.LearningApproach });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedBy", SqlDbType.Int) { Value = course.ModifiedBy });
                            cmd.Parameters.Add(new SqlParameter("@ModifiedDate", SqlDbType.DateTime2) { Value = course.ModifiedDate });
                            cmd.Parameters.Add(new SqlParameter("@ThumbnailPath", SqlDbType.NVarChar) { Value = course.ThumbnailPath });
                            cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar) { Value = course.Title });
                            cmd.Parameters.Add(new SqlParameter("@AssessmentId", SqlDbType.Int) { Value = course.AssessmentId });
                            cmd.Parameters.Add(new SqlParameter("@FeedbackId", SqlDbType.Int) { Value = course.FeedbackId });
                            cmd.Parameters.Add(new SqlParameter("@IsAssessment", SqlDbType.Bit) { Value = course.IsAssessment });
                            cmd.Parameters.Add(new SqlParameter("@IsFeedback", SqlDbType.Bit) { Value = course.IsFeedback });
                            cmd.Parameters.Add(new SqlParameter("@PreAssessmentId", SqlDbType.Int) { Value = course.PreAssessmentId });
                            cmd.Parameters.Add(new SqlParameter("@IsApplicableToAll", SqlDbType.Bit) { Value = course.IsApplicableToAll });
                            cmd.Parameters.Add(new SqlParameter("@CourseType", SqlDbType.NVarChar) { Value = course.CourseType });
                            cmd.Parameters.Add(new SqlParameter("@AdminName", SqlDbType.NVarChar) { Value = course.AdminName });
                            cmd.Parameters.Add(new SqlParameter("@SubCategoryId", SqlDbType.Int) { Value = course.SubCategoryId });
                            cmd.Parameters.Add(new SqlParameter("@Metadata", SqlDbType.NVarChar) { Value = course.Metadata });
                            cmd.Parameters.Add(new SqlParameter("@IsSection", SqlDbType.Bit) { Value = course.IsSection });
                            cmd.Parameters.Add(new SqlParameter("@IsDiscussionBoard", SqlDbType.Bit) { Value = course.IsDiscussionBoard });
                            cmd.Parameters.Add(new SqlParameter("@IsMemoCourse", SqlDbType.Bit) { Value = course.IsMemoCourse });
                            cmd.Parameters.Add(new SqlParameter("@MemoId", SqlDbType.Int) { Value = course.MemoId });
                            cmd.Parameters.Add(new SqlParameter("@Mission", SqlDbType.NVarChar) { Value = course.Mission });
                            cmd.Parameters.Add(new SqlParameter("@Points", SqlDbType.Int) { Value = course.Points });
                            cmd.Parameters.Add(new SqlParameter("@IsAchieveMastery", SqlDbType.Bit) { Value = course.IsAchieveMastery });
                            cmd.Parameters.Add(new SqlParameter("@IsAdaptiveLearning", SqlDbType.Bit) { Value = course.IsAdaptiveLearning });
                            cmd.Parameters.Add(new SqlParameter("@DurationInMinutes", SqlDbType.Int) { Value = course.DurationInMinutes });
                            cmd.Parameters.Add(new SqlParameter("@TotalModules", SqlDbType.Int) { Value = course.TotalModules });
                            cmd.Parameters.Add(new SqlParameter("@IsShowInCatalogue", SqlDbType.Bit) { Value = course.IsShowInCatalogue });
                            cmd.Parameters.Add(new SqlParameter("@RowGuid", SqlDbType.UniqueIdentifier) { Value = course.RowGuid });
                            cmd.Parameters.Add(new SqlParameter("@AssignmentId", SqlDbType.Int) { Value = course.AssignmentId });
                            cmd.Parameters.Add(new SqlParameter("@IsAssignment", SqlDbType.Bit) { Value = course.IsAssignment });
                            cmd.Parameters.Add(new SqlParameter("@IsModuleHasAssFeed", SqlDbType.Bit) { Value = course.IsModuleHasAssFeed });
                            cmd.Parameters.Add(new SqlParameter("@IsManagerEvaluation", SqlDbType.Bit) { Value = course.IsManagerEvaluation });
                            cmd.Parameters.Add(new SqlParameter("@ManagerEvaluationId", SqlDbType.Int) { Value = course.ManagerEvaluationId });
                            cmd.Parameters.Add(new SqlParameter("@prerequisiteCourse", SqlDbType.Int) { Value = course.prerequisiteCourse });
                            cmd.Parameters.Add(new SqlParameter("@IsRetraining", SqlDbType.Bit) { Value = course.IsRetraining });
                            cmd.Parameters.Add(new SqlParameter("@noOfDays", SqlDbType.Int) { Value = course.noOfDays });
                            cmd.Parameters.Add(new SqlParameter("@CourseURL", SqlDbType.NVarChar) { Value = course.CourseURL });
                            cmd.Parameters.Add(new SqlParameter("@ExternalProvider", SqlDbType.NVarChar) { Value = course.ExternalProvider });
                            cmd.Parameters.Add(new SqlParameter("@IsExternalProvider", SqlDbType.Bit) { Value = course.IsExternalProvider });
                            //cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = course.StartDate });
                            //cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = course.EndDate });
                            //cmd.Parameters.Add(new SqlParameter("@CanAutoActivated", SqlDbType.Bit) { Value = course.CanAutoActivated });
                            cmd.Parameters.Add(new SqlParameter("@IsFeedbackOptional", SqlDbType.Bit) { Value = course.IsFeedbackOptional });
                            cmd.Parameters.Add(new SqlParameter("@GroupCourseFee", SqlDbType.Real) { Value = course.GroupCourseFee });
                            cmd.Parameters.Add(new SqlParameter("@Action", SqlDbType.NVarChar) { Value = action });

                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            reader.Dispose();
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
        public async Task<AssessmentReview> assessmentReview(int courseId)
        {
            AssessmentReview assessmentReview = new AssessmentReview();
            assessmentReview.isAssessmentReview = false;
            try
            {
                Model.Course course = await _db.Course.Where(a => a.Id == courseId).FirstOrDefaultAsync();
                if (course != null)
                {
                    if (course.isAssessmentReview == 1)
                    {
                        assessmentReview.isAssessmentReview = true;
                    }
                    else
                    {
                        assessmentReview.isAssessmentReview = false;
                    }
                }
                return assessmentReview;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }
        
        public async Task<APIEdcastDetailsToken> GetEdCastToken(string LxpDetails=null)
        {
            APIEdcastDetailsToken objtoken = new APIEdcastDetailsToken();
            try
            {
                APIEdcastToken gettokendetails = new APIEdcastToken();
                //var rootPath = _hostingEnvironment.ContentRootPath; 
                //var fullPath = Path.Combine(rootPath, ConstantEdCast.JsonFilePath); 
                //var jsonData = System.IO.File.ReadAllText(fullPath);
                string url = null;
                EdCastConfiguration edCastConfiguration = await _db.EdCastConfiguration.Where(a => a.LxpDetails == LxpDetails).FirstOrDefaultAsync();
                if (edCastConfiguration == null)
                {
                    EdCastTransactionDetails obj = new EdCastTransactionDetails();
                    obj.TransactionID = null;
                    obj.Http_method = ConstantEdCast.HTTPMETHOD;
                    obj.Payload = null;
                    obj.Tran_Status = ConstantEdCast.Trans_Error;
                    obj.ResponseMessage = "Please set Edcast " + LxpDetails + " configuration.";
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.CreatedBy = 1;
                    obj.RequestUrl = url;
                    obj.External_Id = null;
                    await _edCastTransactionDetails.Add(obj);
                    return objtoken;
                }
                

                //if (string.IsNullOrWhiteSpace(jsonData))
                //return null; 

                //var myJObject = JObject.Parse(jsonData);              

                
                gettokendetails.client_id = edCastConfiguration.LmsClientID;
                gettokendetails.client_secret = edCastConfiguration.LmsClientSecrete;
                gettokendetails.grant_type = ConstantEdCast.grant_type;

                url= edCastConfiguration.LmsHost;
                url = url + "/api/oauth/token";

                JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(gettokendetails));

                objtoken = await ApiHelper.GetTokenForEdcastLMS(oJsonObject,url);
                if (objtoken != null)
                {
                    if (objtoken.access_token != null)
                    {
                        EdCastTransactionDetails obj = new EdCastTransactionDetails();
                        obj.TransactionID = null;
                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
                        obj.Payload = JsonConvert.SerializeObject(gettokendetails);
                        obj.Tran_Status = ConstantEdCast.Trans_Success;
                        obj.ResponseMessage = null;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = 1;
                        obj.RequestUrl = url;
                        obj.External_Id = null;
                        await _edCastTransactionDetails.Add(obj);
                    }
                    else 
                    {                       
                            EdCastTransactionDetails obj = new EdCastTransactionDetails();
                            obj.TransactionID = null;
                            obj.Http_method = ConstantEdCast.HTTPMETHOD;
                            obj.Payload = JsonConvert.SerializeObject(gettokendetails);
                            obj.Tran_Status = ConstantEdCast.Trans_Error;
                            obj.ResponseMessage = objtoken.error;
                            obj.CreatedDate = DateTime.UtcNow;
                            obj.CreatedBy = 1;
                            obj.RequestUrl = url;
                            obj.External_Id = null;
                            await _edCastTransactionDetails.Add(obj);                        
                    }
                }

                
            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return objtoken;
        }

        public static string[] GetArraystring(string metadata)
        {
            string[] strArrayOne = new string[] { "" };
            if (metadata != null)
            {
                strArrayOne = metadata.Split(',');
            }
            return strArrayOne;
        }
        public async Task<APIEdCastTransactionDetails> PostCourseToClient(int courseID, int userId, string token = null, string LxpDetails = null)
        {
            APIEdCastTransactionDetails objCourseResponce = new APIEdCastTransactionDetails();
            try
            {

                string url = null;

                EdCastConfiguration edCastConfiguration = await _db.EdCastConfiguration.Where(a => a.LxpDetails == LxpDetails).FirstOrDefaultAsync();
                if (edCastConfiguration == null)
                {
                    EdCastTransactionDetails obj = new EdCastTransactionDetails();
                    obj.TransactionID = null;
                    obj.Http_method = ConstantEdCast.HTTPMETHOD;
                    obj.Payload = null;
                    obj.Tran_Status = ConstantEdCast.Trans_Error;
                    obj.ResponseMessage = "Please set Edcast " + LxpDetails + " configuration.";
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.CreatedBy = userId;
                    obj.RequestUrl = url;
                    obj.External_Id = courseID.ToString();
                    await _edCastTransactionDetails.Add(obj);
                    objCourseResponce.error = obj.ResponseMessage;
                    objCourseResponce.message = obj.ResponseMessage;
                    objCourseResponce.data.id = courseID.ToString();
                    objCourseResponce.data.http_method = ConstantEdCast.HTTPMETHOD;
                    objCourseResponce.data.payload = null;
                    return objCourseResponce;


                }
                //if (string.IsNullOrWhiteSpace(jsonData))
                //    return null;

                //var myJObject = JObject.Parse(jsonData);


                //string domainname = myJObject.SelectToken(ConstantEdCast.EmpoweredHost).Value<string>();
                //string courseUrl = myJObject.SelectToken(ConstantEdCast.EmpoweredCourseUrl).Value<string>();

                // RestrictGroup = _db.AccessibilityRule.Where(a => a.CourseId == courseID && a.IsDeleted == false).Select(a => a.UserTeamId).ToArray();

                //RestrictGroup = await (from ar in this._db.AccessibilityRule
                //             join team in this._db.UserTeams on ar.UserTeamId equals team.Id
                //             where ar.IsDeleted == false && ar.CourseId == courseID
                //              select(Convert.ToInt32(team.TeamCode))).ToArrayAsync();


                //int[] channelIds= await (from cgm in this._db.CourseGroupMapping
                //                         join cg in this._db.CourseGroup on cgm.GroupId equals cg.Id
                //       where cgm.IsDeleted == false && cgm.CourseId == courseID
                //       select (Convert.ToInt32(cg.GroupCode))).ToArrayAsync();


                //   int?[] channelIds = _db.CourseGroupMapping.Where(a => a.CourseId == courseID && a.IsDeleted == false).Select(a => (int?) a.GroupId).ToArray(); 

                var Query = (from courses in _db.Course
                             join moduleAssociaton in _db.CourseModuleAssociation on courses.Id equals moduleAssociaton.CourseId
                             join module in _db.Module on moduleAssociaton.ModuleId equals module.Id

                             where courses.IsDeleted == false && courses.Id == courseID
                             select new APIcoursePost
                             {
                                 name = courses.Title,
                                 description = courses.Description,
                                 url = edCastConfiguration.CourseUrl.Replace("courseid", Convert.ToString(courses.Id)),
                                 external_id = courses.Id.ToString(),
                                 course_type = courses.CourseType == "elearning" ? "Course" : courses.CourseType.ToLower(),
                                 content_type = courses.CourseType == "elearning" ? "Course" : courses.CourseType.ToLower(),
                                 is_private = courses.IsPrivateContent == null ? false : Convert.ToBoolean(courses.IsPrivateContent),
                                 duration_sec = (courses.DurationInMinutes * 60).ToString(),
                                 published_at = courses.CreatedDate,
                                 status = courses.IsActive == true ? "active" : "archived",
                                 image_url = courses.ThumbnailPath.Replace("../../..", edCastConfiguration.EmpoweredHost),
                                 // group_ids= RestrictGroup,
                                 // channel_ids= channelIds,
                                 tags = GetArraystring(courses.Metadata)
                             }
                       );
                var course_Data = await Query.FirstOrDefaultAsync();
                JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(course_Data));
                url = edCastConfiguration.LmsHost;
                url = url + "/api/developer/v2/courses.json";
                string Body = JsonConvert.SerializeObject(oJsonObject);
                objCourseResponce = await ApiHelper.PostEdcastAPI(url, Body, token);

                if (objCourseResponce != null)
                {
                    if (objCourseResponce.error == null)
                    {
                        EdCastTransactionDetails obj = new EdCastTransactionDetails();
                        obj.TransactionID = objCourseResponce.data.id;
                        obj.Http_method = objCourseResponce.data.http_method;
                        obj.Payload = JsonConvert.SerializeObject(objCourseResponce.data.payload);
                        obj.Tran_Status = objCourseResponce.data.status;
                        obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? objCourseResponce.error : objCourseResponce.message;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = userId;
                        obj.RequestUrl = url;
                        obj.External_Id = objCourseResponce.data.payload.external_id;
                        await _edCastTransactionDetails.Add(obj);
                    }
                    else
                    {
                        EdCastTransactionDetails obj = new EdCastTransactionDetails();
                        obj.TransactionID = null;
                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
                        obj.Payload = JsonConvert.SerializeObject(course_Data);
                        obj.Tran_Status = ConstantEdCast.Trans_Error;
                        obj.ResponseMessage = objCourseResponce.error;
                        obj.CreatedDate = DateTime.UtcNow;
                        obj.CreatedBy = userId;
                        obj.RequestUrl = url;
                        obj.External_Id = null;
                        await _edCastTransactionDetails.Add(obj);

                    }
                }


            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return objCourseResponce;
        }

        //public async Task<APIDarwinTransactionDetails> PostCourseToDarwinbox(int courseID, int userId, string orgcode)
        //{
        //    APIDarwinTransactionDetails objCourseResponce = new APIDarwinTransactionDetails();
        //    try
        //    {


        //        string url = null;

        //        DarwinboxConfiguration darwinconfiguration = new DarwinboxConfiguration();
        //        var cache = new CacheManager.CacheManager();
        //        string cacheKeyConfig = Constants.DarwinboxConfiguration + "-" + orgcode.ToUpper();
        //        if (cache.IsAdded(cacheKeyConfig.ToUpper()))
        //        {
        //            darwinconfiguration = cache.Get<DarwinboxConfiguration>(cacheKeyConfig.ToUpper());
        //        }
        //        else
        //        {
        //            var data = await this._darwinboxConfiguration.GetAll();
        //            darwinconfiguration = data.FirstOrDefault();
        //            cache.Add(cacheKeyConfig.ToUpper(), darwinconfiguration, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
        //        }
        //        if (darwinconfiguration == null)
        //        {
        //            DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //            obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //            obj.Payload = null;
        //            obj.Tran_Status = objCourseResponce.status;
        //            obj.ResponseMessage = "Please set Darwinbox configuration.";
        //            obj.CreatedDate = DateTime.UtcNow;
        //            obj.CreatedBy = userId;
        //            obj.RequestUrl = url;
        //            obj.External_Id = courseID.ToString();
        //            await _darwinboxTransactionDetails.Add(obj);
        //            return objCourseResponce;
        //        }


        //        var Query = (from courses in _db.Course
        //                     join moduleAssociaton in _db.CourseModuleAssociation on courses.Id equals moduleAssociaton.CourseId
        //                     join module in _db.Module on moduleAssociaton.ModuleId equals module.Id

        //                     where courses.IsDeleted == false && courses.Id == courseID
        //                     select new APICoursePostToDarwinbox
        //                     {
        //                         learning_activity_code = courses.Code,
        //                         learning_activity_name = courses.Title,
        //                         learning_activity_description = courses.Description,
        //                         currency = courses.Currency,
        //                         vendor = darwinconfiguration.Vendor,
        //                         program = darwinconfiguration.Program,
        //                         allow_search_to = 1,
        //                         cost = Convert.ToString(courses.CourseFee),
        //                         available_from = DateTime.Now.ToString(),
        //                         content_link = darwinconfiguration.DarwinboxCourseUrl.Replace("courseid", Convert.ToString(courses.Id)),
        //                     }
        //               ).AsNoTracking();
        //        List<APICoursePostToDarwinbox> course_Data = await Query.Distinct().ToListAsync();

        //        if (course_Data.Count() == 0)
        //        {
        //            DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //            obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //            obj.Payload = null;
        //            obj.Tran_Status = objCourseResponce.status;
        //            obj.ResponseMessage = "Vendor not selected.";
        //            obj.CreatedDate = DateTime.UtcNow;
        //            obj.CreatedBy = userId;
        //            obj.RequestUrl = url;
        //            obj.External_Id = courseID.ToString();
        //            await _darwinboxTransactionDetails.Add(obj);
        //            return objCourseResponce;
        //        }

        //        APICoursePostAPIkey aPICoursePostAPIkey = new APICoursePostAPIkey();
        //        aPICoursePostAPIkey.api_key = darwinconfiguration.Create_LA;
        //        aPICoursePostAPIkey.activity = course_Data.ToArray();



        //        JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPICoursePostAPIkey));
        //        url = darwinconfiguration.DarwinboxHost;
        //        url = url + "/lmsapi/createlearningactivity";
        //        string Body = JsonConvert.SerializeObject(oJsonObject);
        //        objCourseResponce = await ApiHelper.PostDarwinboxAPI(url, Body, darwinconfiguration.Username, darwinconfiguration.Password);

        //        if (objCourseResponce != null)
        //        {
        //            if (objCourseResponce.errors.Count() == 0)
        //            {
        //                DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //                obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                obj.Payload = Body;
        //                obj.Tran_Status = objCourseResponce.status;
        //                obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? string.Join(",", objCourseResponce.errors) : objCourseResponce.message;
        //                obj.CreatedDate = DateTime.UtcNow;
        //                obj.CreatedBy = userId;
        //                obj.RequestUrl = url;
        //                obj.External_Id = courseID.ToString();
        //                await _darwinboxTransactionDetails.Add(obj);

        //            }
        //            else
        //            {
        //                DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //                obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                obj.Payload = JsonConvert.SerializeObject(course_Data);
        //                obj.Tran_Status = ConstantEdCast.Trans_Error;
        //                obj.ResponseMessage = string.Join(",", objCourseResponce.errors);
        //                obj.CreatedDate = DateTime.UtcNow;
        //                obj.CreatedBy = userId;
        //                obj.RequestUrl = url;
        //                obj.External_Id = courseID.ToString();
        //                await _darwinboxTransactionDetails.Add(obj);

        //            }
        //        }


        //    }

        //    catch (Exception ex)

        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //    }
        //    return objCourseResponce;
        //}
        //public async Task<DarwinboxTransactionDetails> PostCourseStatusToDarwinbox(int courseID, int userId, string status, string orgcode, string connectionstring = null)
        //{
        //    APIDarwinTransactionDetails objCourseResponce = new APIDarwinTransactionDetails();
        //    DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();
        //    if (!string.IsNullOrEmpty(connectionstring))
        //        ChangeDbContext(connectionstring);

        //    string ProvidedUserId = await _db.UserMaster.Where(a => a.Id == userId).Select(a => a.ProvidedUserId).FirstOrDefaultAsync();
        //    if (string.IsNullOrEmpty(ProvidedUserId))
        //    {

        //        obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //        obj.Payload = null;
        //        obj.Tran_Status = ConstantEdCast.Trans_Error;
        //        obj.ResponseMessage = "Invalid User provided";
        //        obj.CreatedDate = DateTime.UtcNow;
        //        obj.CreatedBy = userId;
        //        obj.RequestUrl = "Enroll";
        //        obj.External_Id = courseID.ToString();
        //        _db.DarwinboxTransactionDetails.Add(obj);
        //        await _db.SaveChangesAsync();
        //        return obj;
        //    }
        //    else
        //    {

        //        try
        //        {

        //            string url = null;

        //            DarwinboxConfiguration darwinconfiguration = new DarwinboxConfiguration();
        //            var cache = new CacheManager.CacheManager();
        //            string cacheKeyConfig = Constants.DarwinboxConfiguration + "-" + orgcode.ToUpper();
        //            if (cache.IsAdded(cacheKeyConfig.ToUpper()))
        //            {
        //                darwinconfiguration = cache.Get<DarwinboxConfiguration>(cacheKeyConfig.ToUpper());
        //            }
        //            else
        //            {
        //                var data = await this._darwinboxConfiguration.GetAll();
        //                darwinconfiguration = data.FirstOrDefault();
        //                cache.Add(cacheKeyConfig.ToUpper(), darwinconfiguration, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
        //            }
        //            if (darwinconfiguration == null)
        //            {

        //                obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                obj.Payload = null;
        //                obj.Tran_Status = objCourseResponce.status;
        //                obj.ResponseMessage = "Please set Darwinbox configuration.";
        //                obj.CreatedDate = DateTime.UtcNow;
        //                obj.CreatedBy = userId;
        //                obj.RequestUrl = url;
        //                obj.External_Id = courseID.ToString();
        //                _db.DarwinboxTransactionDetails.Add(obj);
        //                await _db.SaveChangesAsync();
        //                return obj;
        //            }


        //            List<APIUpdateCourseStatusToDB> emp_Data = new List<APIUpdateCourseStatusToDB>();
        //            if (status != "enroll")
        //            {

        //                int modulecount = await _db.CourseModuleAssociation.Where(a => a.CourseId == courseID).CountAsync();
        //                int completedmodulecount = await _db.ModuleCompletionStatus.Where(a => a.CourseId == courseID && a.UserId == userId && a.Status == "completed").CountAsync();
        //                string CompletionPer = "5";
        //                CompletionPer = Convert.ToString((int)(completedmodulecount / modulecount) * 100);
        //                var Query = (from courses in _db.Course
        //                             join ccs in _db.CourseCompletionStatus on courses.Id equals ccs.CourseId
        //                             join User in _db.UserMaster on ccs.UserId equals User.Id

        //                             where courses.IsDeleted == false && courses.Id == courseID && ccs.UserId == userId
        //                             select new APIUpdateCourseStatusToDB
        //                             {
        //                                 employee_id = string.IsNullOrEmpty(User.ProvidedUserId) ? "" : Security.Decrypt(User.ProvidedUserId),
        //                                 activity_id = courses.Code,
        //                                 start_date = ccs.CreatedDate.ToString(),
        //                                 complete_date = status == "completed" ? ccs.ModifiedDate.ToString() : null,
        //                                 action = status == "completed" ? "complete" : "start",
        //                                 last_updated_on = ccs.ModifiedDate.ToString(),
        //                                 completion_percentage = CompletionPer
        //                             }).AsNoTracking();
        //                emp_Data = await Query.Distinct().ToListAsync();
        //            }
        //            else
        //            {
        //                var Query = (from courses in _db.Course
        //                             where courses.IsDeleted == false && courses.Id == courseID
        //                             select new APIUpdateCourseStatusToDB
        //                             {
        //                                 employee_id = string.IsNullOrEmpty(ProvidedUserId) ? "" : Security.Decrypt(ProvidedUserId),
        //                                 activity_id = courses.Code,
        //                                 enrolled_on = Convert.ToString(DateTime.UtcNow),
        //                                 action = status

        //                             }).AsNoTracking();
        //                emp_Data = await Query.Distinct().ToListAsync();
        //            }

        //            if (emp_Data.Count() == 0)
        //            {

        //                obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                obj.Payload = null;
        //                obj.Tran_Status = ConstantEdCast.Trans_Error;
        //                obj.ResponseMessage = "Employee or course not found.";
        //                obj.CreatedDate = DateTime.UtcNow;
        //                obj.CreatedBy = userId;
        //                obj.RequestUrl = url;
        //                obj.External_Id = courseID.ToString();
        //                _db.DarwinboxTransactionDetails.Add(obj);
        //                await _db.SaveChangesAsync();
        //                return obj;
        //            }

        //            APICourseStatusPostAPIkey aPICourseStatusPostAPIkey = new APICourseStatusPostAPIkey();
        //            aPICourseStatusPostAPIkey.api_key = darwinconfiguration.Update_LA;
        //            aPICourseStatusPostAPIkey.employees = emp_Data.ToArray();

        //            JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPICourseStatusPostAPIkey));
        //            url = darwinconfiguration.DarwinboxHost;
        //            url = url + "/lmsapi/updateuserlearningactivities";
        //            string Body = JsonConvert.SerializeObject(oJsonObject);
        //            objCourseResponce = await ApiHelper.PostDarwinboxAPI(url, Body, darwinconfiguration.Username, darwinconfiguration.Password);

        //            if (objCourseResponce != null)
        //            {
        //                if (objCourseResponce.status == "0")
        //                {
        //                    obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                    obj.Payload = Body;
        //                    obj.Tran_Status = objCourseResponce.status;
        //                    obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? string.Join(",", objCourseResponce.error) : objCourseResponce.message;
        //                    obj.CreatedDate = DateTime.UtcNow;
        //                    obj.CreatedBy = userId;
        //                    obj.RequestUrl = url;
        //                    obj.External_Id = courseID.ToString();
        //                    _db.DarwinboxTransactionDetails.Add(obj);
        //                    await _db.SaveChangesAsync();
        //                }
        //                else
        //                {
        //                    if (objCourseResponce.error.Count() == 0)
        //                    {
        //                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                        obj.Payload = Body;
        //                        obj.Tran_Status = objCourseResponce.status;
        //                        obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? string.Join(",", objCourseResponce.error) : objCourseResponce.message;
        //                        obj.CreatedDate = DateTime.UtcNow;
        //                        obj.CreatedBy = userId;
        //                        obj.RequestUrl = url;
        //                        obj.External_Id = courseID.ToString();
        //                        _db.DarwinboxTransactionDetails.Add(obj);
        //                        await _db.SaveChangesAsync();

        //                    }
        //                    else
        //                    {


        //                        obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                        obj.Payload = JsonConvert.SerializeObject(emp_Data);
        //                        obj.Tran_Status = ConstantEdCast.Trans_Error;
        //                        obj.ResponseMessage = string.Join(",", objCourseResponce.error);
        //                        obj.CreatedDate = DateTime.UtcNow;
        //                        obj.CreatedBy = userId;
        //                        obj.RequestUrl = url;
        //                        obj.External_Id = courseID.ToString();
        //                        _db.DarwinboxTransactionDetails.Add(obj);
        //                        await _db.SaveChangesAsync();

        //                    }
        //                }
        //            }


        //        }

        //        catch (Exception ex)

        //        {
        //            _logger.Error(Utilities.GetDetailedException(ex));
        //        }
        //    }
        //    return obj;
        //}
        //private async Task<string> OrgnizationConnectionString(string organizationCode)
        //{
        //    string OrgnizationConnectionString = await this._customerConnection.GetConnectionStringByOrgnizationCode(organizationCode);
        //    if (OrgnizationConnectionString.Equals(ApiStatusCode.Unauthorized.ToString()))
        //        return string.Empty; //return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

        //    if (OrgnizationConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
        //        return string.Empty;

        //    return OrgnizationConnectionString;
        //}
        //public async Task<APIDarwinTransactionDetails> PostEnrollCourseToDarwinbox(int courseID, int userId, string status, string orgcode)
        //{
        //    APIDarwinTransactionDetails objCourseResponce = new APIDarwinTransactionDetails();

        //    var OrgnizationConnectionString = await this.OrgnizationConnectionString(orgcode); //todo move to cache

        //    using (var dbContext = this._customerConnection.GetDbContext(OrgnizationConnectionString))
        //    {
        //    }
        //    string ProvidedUserId = await _db.UserMaster.Where(a => a.Id == userId).Select(a => a.ProvidedUserId).FirstOrDefaultAsync();
        //    if (string.IsNullOrEmpty(ProvidedUserId))
        //    {
        //        DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //        obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //        obj.Payload = null;
        //        obj.Tran_Status = ConstantEdCast.Trans_Error;
        //        obj.ResponseMessage = "Invalid User provided";
        //        obj.CreatedDate = DateTime.UtcNow;
        //        obj.CreatedBy = userId;
        //        obj.RequestUrl = "Enroll";
        //        obj.External_Id = courseID.ToString();
        //        await _darwinboxTransactionDetails.Add(obj);
        //        return objCourseResponce;
        //    }
        //    else
        //    {

        //        try
        //        {

        //            var rootPath = _hostingEnvironment.ContentRootPath;
        //            var fullPath = Path.Combine(rootPath, ConstantEdCast.JsonFilePath);
        //            var jsonData = System.IO.File.ReadAllText(fullPath);

        //            string url = null;

        //            if (string.IsNullOrWhiteSpace(jsonData))
        //                return null;

        //            var myJObject = JObject.Parse(jsonData);
        //            string username = myJObject.SelectToken(ConstantEdCast.Username).Value<string>();
        //            string password = myJObject.SelectToken(ConstantEdCast.Password).Value<string>();
        //            string update_LA = myJObject.SelectToken(ConstantEdCast.update_LA).Value<string>();
        //            string AllowedOrgCode = myJObject.SelectToken(ConstantEdCast.AllowedOrgCode).Value<string>();

        //            if (!AllowedOrgCode.Contains(orgcode))
        //            {
        //                DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //                obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                obj.Payload = null;
        //                obj.Tran_Status = ConstantEdCast.Trans_Error;
        //                obj.ResponseMessage = "Invalid OrgCde ";
        //                obj.CreatedDate = DateTime.UtcNow;
        //                obj.CreatedBy = userId;
        //                obj.RequestUrl = url;
        //                obj.External_Id = courseID.ToString();
        //                await _darwinboxTransactionDetails.Add(obj);
        //                return objCourseResponce;
        //            }
        //            List<APIUpdateCourseStatusToDB> emp_Data = new List<APIUpdateCourseStatusToDB>();



        //            var Query = (from courses in _db.Course

        //                         where courses.IsDeleted == false && courses.Id == courseID
        //                         select new APIUpdateCourseStatusToDB
        //                         {
        //                             employee_id = string.IsNullOrEmpty(ProvidedUserId) ? "" : Security.Decrypt(ProvidedUserId),
        //                             activity_id = courses.Code,
        //                             enrolled_on = Convert.ToString(DateTime.UtcNow),
        //                             action = status

        //                         }).AsNoTracking();
        //            emp_Data = await Query.Distinct().ToListAsync();


        //            if (emp_Data.Count() == 0)
        //            {
        //                DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //                obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                obj.Payload = null;
        //                obj.Tran_Status = ConstantEdCast.Trans_Error;
        //                obj.ResponseMessage = "Employee or course not found.";
        //                obj.CreatedDate = DateTime.UtcNow;
        //                obj.CreatedBy = userId;
        //                obj.RequestUrl = url;
        //                obj.External_Id = courseID.ToString();
        //                await _darwinboxTransactionDetails.Add(obj);
        //                return objCourseResponce;
        //            }

        //            APICourseStatusPostAPIkey aPICourseStatusPostAPIkey = new APICourseStatusPostAPIkey();
        //            aPICourseStatusPostAPIkey.api_key = update_LA;
        //            aPICourseStatusPostAPIkey.employees = emp_Data.ToArray();

        //            JObject oJsonObject = JObject.Parse(JsonConvert.SerializeObject(aPICourseStatusPostAPIkey));
        //            url = myJObject.SelectToken(ConstantEdCast.DarwinboxHost).Value<string>();
        //            url = url + "/lmsapi/updateuserlearningactivities";
        //            string Body = JsonConvert.SerializeObject(oJsonObject);
        //            objCourseResponce = await ApiHelper.PostDarwinboxAPI(url, Body, username, password);

        //            if (objCourseResponce != null)
        //            {
        //                if (objCourseResponce.error.Count() == 0)
        //                {
        //                    DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //                    obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                    obj.Payload = Body;
        //                    obj.Tran_Status = objCourseResponce.status;
        //                    obj.ResponseMessage = String.IsNullOrEmpty(objCourseResponce.message) ? string.Join(",", objCourseResponce.error) : objCourseResponce.message;
        //                    obj.CreatedDate = DateTime.UtcNow;
        //                    obj.CreatedBy = userId;
        //                    obj.RequestUrl = url;
        //                    obj.External_Id = courseID.ToString();
        //                    await _darwinboxTransactionDetails.Add(obj);

        //                }
        //                else
        //                {
        //                    DarwinboxTransactionDetails obj = new DarwinboxTransactionDetails();

        //                    obj.Http_method = ConstantEdCast.HTTPMETHOD;
        //                    obj.Payload = JsonConvert.SerializeObject(emp_Data);
        //                    obj.Tran_Status = ConstantEdCast.Trans_Error;
        //                    obj.ResponseMessage = string.Join(",", objCourseResponce.error);
        //                    obj.CreatedDate = DateTime.UtcNow;
        //                    obj.CreatedBy = userId;
        //                    obj.RequestUrl = url;
        //                    obj.External_Id = courseID.ToString();
        //                    await _darwinboxTransactionDetails.Add(obj);

        //                }
        //            }


        //        }

        //        catch (Exception ex)

        //        {
        //            _logger.Error(Utilities.GetDetailedException(ex));
        //        }
        //    }
        //    return objCourseResponce;
        //}


        //public async Task<APITotalCoursesView> GetAllV2(ApiGetCourse apiGetCourse, int userId, string userRole)
        //{
        //    APITotalCoursesView aPITotalCoursesView = new APITotalCoursesView();
        //    UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();

        //    var Query = (from courses in _db.Course
        //                 join user in _db.UserMaster on courses.ModifiedBy equals user.Id into um
        //                 from user in um.DefaultIfEmpty()
        //                 join umd in _db.UserMasterDetails on courses.CreatedBy equals umd.UserMasterId into umddetails
        //                 from umd in umddetails.DefaultIfEmpty()
        //                 where courses.IsDeleted == false
        //                 select new APIAllCoursesView
        //                 {
        //                     Code = courses.Code,
        //                     Id = courses.Id,
        //                     IsActive = courses.IsActive,
        //                     IsAssessment = courses.IsAssessment,
        //                     IsFeedback = courses.IsFeedback,
        //                     Title = courses.Title,
        //                     Description = courses.Description,
        //                     Language = courses.Language,
        //                     Metadata = courses.Metadata,
        //                     CategoryId = courses.CategoryId,
        //                     CourseType = courses.CourseType,
        //                     UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
        //                     AreaId = umd.AreaId,
        //                     LocationId = umd.LocationId,
        //                     GroupId = umd.GroupId,
        //                     BusinessId = umd.BusinessId,
        //                     CreatedBy = courses.CreatedBy,
        //                     UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (courses.CreatedBy == userId) ? true : false : true
        //                 }).AsNoTracking();

        //    var authorQuery = (from courses in _db.Course
        //                       join courseAuthor in _db.CourseAuthorAssociation on courses.Id equals courseAuthor.CourseId
        //                       join user in _db.UserMaster on courses.ModifiedBy equals user.Id into um
        //                       from user in um.DefaultIfEmpty()
        //                       join umd in _db.UserMasterDetails on courses.CreatedBy equals umd.UserMasterId into umddetails
        //                       from umd in umddetails.DefaultIfEmpty()
        //                       where courses.IsDeleted == false && courseAuthor.UserId == userId
        //                       select new APIAllCoursesView
        //                       {
        //                           Code = courses.Code,
        //                           Id = courses.Id,
        //                           IsActive = courses.IsActive,
        //                           IsAssessment = courses.IsAssessment,
        //                           IsFeedback = courses.IsFeedback,
        //                           Title = courses.Title,
        //                           Description = courses.Description,
        //                           Language = courses.Language,
        //                           Metadata = courses.Metadata,
        //                           CategoryId = courses.CategoryId,
        //                           CourseType = courses.CourseType,
        //                           UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
        //                           AreaId = umd.AreaId,
        //                           LocationId = umd.LocationId,
        //                           GroupId = umd.GroupId,
        //                           BusinessId = umd.BusinessId,
        //                           CreatedBy = courses.CreatedBy,
        //                           UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (courses.CreatedBy == userId) ? true : false : true
        //                       }).AsNoTracking();

        //    if (apiGetCourse.filter == "null")
        //        apiGetCourse.filter = null;
        //    if (apiGetCourse.search == "null")
        //        apiGetCourse.search = null;

        //    if (!string.IsNullOrEmpty(apiGetCourse.search))
        //    {
        //        if (!string.IsNullOrEmpty(apiGetCourse.filter))
        //        {
        //            if (apiGetCourse.filter.ToLower().Equals("code"))
        //                Query = Query.Where(r => r.Code.StartsWith(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("title"))
        //                Query = Query.Where(r => r.Title.Contains(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("description"))
        //                Query = Query.Where(r => r.Description.Contains(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("language"))
        //                Query = Query.Where(r => r.Language.StartsWith(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("coursetype") && apiGetCourse.search.ToLower().Equals("assessment"))
        //            {
        //                Query = Query.Where(r => r.CourseType.Equals("Certification"));
        //            }
        //            else
        //            {
        //                if (apiGetCourse.filter.ToLower().Equals("coursetype"))
        //                    Query = Query.Where(r => r.CourseType.StartsWith(apiGetCourse.search));
        //            }
        //            if (apiGetCourse.filter.ToLower().Equals("topic"))
        //                Query = Query.Where(r => r.Metadata.Contains(apiGetCourse.search));
        //        }
        //        else
        //        {
        //            Query = Query.Where(r => r.Title.Contains(apiGetCourse.search) || r.Metadata.Contains(apiGetCourse.search) || r.Code.Contains(apiGetCourse.search));
        //        }
        //    }

        //    if (apiGetCourse.categoryId != null)
        //        Query = Query.Where(r => r.CategoryId == apiGetCourse.categoryId);
        //    if (apiGetCourse.IsActive != null)
        //        Query = Query.Where(r => r.IsActive == apiGetCourse.IsActive);

        //    // author chnages

        //    if (!string.IsNullOrEmpty(apiGetCourse.search))
        //    {
        //        if (!string.IsNullOrEmpty(apiGetCourse.filter))
        //        {
        //            if (apiGetCourse.filter.ToLower().Equals("code"))
        //                authorQuery = authorQuery.Where(r => r.Code.StartsWith(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("title"))
        //                authorQuery = authorQuery.Where(r => r.Title.Contains(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("description"))
        //                authorQuery = authorQuery.Where(r => r.Description.Contains(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("language"))
        //                authorQuery = authorQuery.Where(r => r.Language.StartsWith(apiGetCourse.search));
        //            if (apiGetCourse.filter.ToLower().Equals("coursetype") && apiGetCourse.search.ToLower().Equals("assessment"))
        //            {
        //                authorQuery = authorQuery.Where(r => r.CourseType.Equals("Certification"));
        //            }
        //            else
        //            {
        //                if (apiGetCourse.filter.ToLower().Equals("coursetype"))
        //                    authorQuery = authorQuery.Where(r => r.CourseType.StartsWith(apiGetCourse.search));
        //            }
        //            if (apiGetCourse.filter.ToLower().Equals("topic"))
        //                authorQuery = authorQuery.Where(r => r.Metadata.Contains(apiGetCourse.search));
        //        }
        //        else
        //        {
        //            authorQuery = authorQuery.Where(r => r.Title.Contains(apiGetCourse.search) || r.Metadata.Contains(apiGetCourse.search) || r.Code.Contains(apiGetCourse.search));
        //        }
        //    }

        //    if (apiGetCourse.categoryId != null)
        //        authorQuery = authorQuery.Where(r => r.CategoryId == apiGetCourse.categoryId);
        //    if (apiGetCourse.IsActive != null)
        //        authorQuery = authorQuery.Where(r => r.IsActive == apiGetCourse.IsActive);



        //    if (userRole == UserRoles.BA)
        //    {
        //        Query = Query.Where(r => r.BusinessId == userdetails.BusinessId);
        //    }
        //    if (userRole == UserRoles.GA)
        //    {
        //        Query = Query.Where(r => r.GroupId == userdetails.GroupId);
        //    }
        //    if (userRole == UserRoles.LA)
        //    {
        //        Query = Query.Where(r => r.LocationId == userdetails.LocationId);
        //    }
        //    if (userRole == UserRoles.AA)
        //    {
        //        Query = Query.Where(r => r.AreaId == userdetails.AreaId);
        //    }
        //    if (apiGetCourse.showAllData == false && (userRole != UserRoles.CA))
        //    {
        //        Query = Query.Where(r => r.CreatedBy == userId);
        //    }
        //    var queryResult = Query.Union(authorQuery);
        //    aPITotalCoursesView.TotalRecords = await queryResult.Distinct().CountAsync();

        //    queryResult = queryResult.OrderByDescending(r => r.Id);
        //    if (apiGetCourse.page != -1)
        //        queryResult = queryResult.Skip((Convert.ToInt32(apiGetCourse.page) - 1) * Convert.ToInt32(apiGetCourse.pageSize));
        //    if (apiGetCourse.pageSize != -1)
        //        queryResult = queryResult.Take(Convert.ToInt32(apiGetCourse.pageSize));
        //    var course = await queryResult.Distinct().ToListAsync();
        //    aPITotalCoursesView.Data = course;
        //    return aPITotalCoursesView;
        //}

        //public async Task<APIAllCourses> GetCourseDetailsById(int id)
        //{
        //    var Query = (from courses in _db.Course
        //                 join vendor in _db.CourseVendorDetail on courses.VendorId equals vendor.Id into ven
        //                 from vendor in ven.DefaultIfEmpty()
        //                 join CourseDetails in _db.CourseDetails on courses.Id equals CourseDetails.CourseID into cd
        //                 from CourseDetails in cd.DefaultIfEmpty()
        //                 where courses.IsDeleted == false && courses.Id == id
        //                 select new APIAllCourses
        //                 {
        //                     AdminName = courses.AdminName,
        //                     AssessmentId = courses.AssessmentId,
        //                     AssignmentId = courses.AssignmentId,
        //                     CategoryId = courses.CategoryId,
        //                     Code = courses.Code,
        //                     CompletionPeriodDays = courses.CompletionPeriodDays,
        //                     CourseAdminID = courses.CourseAdminID,
        //                     CourseFee = courses.CourseFee,
        //                     CourseType = courses.CourseType,
        //                     CourseURL = courses.CourseURL,
        //                     CreatedBy = courses.CreatedBy,
        //                     CreatedDate = courses.CreatedDate,
        //                     CreditsPoints = courses.CreditsPoints,
        //                     Currency = courses.Currency,
        //                     Description = courses.Description,
        //                     DurationInMinutes = courses.DurationInMinutes,
        //                     ExternalProvider = courses.ExternalProvider,
        //                     FeedbackId = courses.FeedbackId,
        //                     Id = courses.Id,
        //                     IsAchieveMastery = courses.IsAchieveMastery,
        //                     IsActive = courses.IsActive,
        //                     IsAdaptiveLearning = courses.IsAdaptiveLearning,
        //                     IsApplicableToAll = courses.IsApplicableToAll,
        //                     IsApplicableToExternal = courses.IsApplicableToExternal,
        //                     IsAssessment = courses.IsAssessment,
        //                     IsAssignment = courses.IsAssignment,
        //                     IsCertificateIssued = courses.IsCertificateIssued,
        //                     IsDeleted = courses.IsDeleted,
        //                     IsDiscussionBoard = courses.IsDiscussionBoard,
        //                     IsExternalProvider = courses.IsExternalProvider,
        //                     IsFeedback = courses.IsFeedback,
        //                     IsManagerEvaluation = courses.IsManagerEvaluation,
        //                     IsMemoCourse = courses.IsMemoCourse,
        //                     IsModuleHasAssFeed = courses.IsModuleHasAssFeed,
        //                     IsPreAssessment = courses.IsPreAssessment,
        //                     IsRetraining = courses.IsRetraining,
        //                     IsSection = courses.IsSection,
        //                     IsShowInCatalogue = courses.IsShowInCatalogue,
        //                     Language = courses.Language,
        //                     LearningApproach = courses.LearningApproach,
        //                     ManagerEvaluationId = courses.ManagerEvaluationId,
        //                     MemoId = courses.MemoId,
        //                     Metadata = courses.Metadata,
        //                     Mission = courses.Mission,
        //                     ModifiedBy = courses.ModifiedBy,
        //                     ModifiedDate = courses.ModifiedDate,
        //                     noOfDays = courses.noOfDays,
        //                     Points = courses.Points,
        //                     PreAssessmentId = courses.PreAssessmentId,
        //                     prerequisiteCourse = courses.prerequisiteCourse,
        //                     RowGuid = courses.RowGuid,
        //                     SubCategoryId = courses.SubCategoryId,
        //                     ThumbnailPath = courses.ThumbnailPath,
        //                     Title = courses.Title,
        //                     TotalModules = courses.TotalModules,
        //                     prerequisiteCourseName = _db.Course.Where(x => x.Id == courses.prerequisiteCourse).Select(x => x.Title.ToString()).FirstOrDefault(),
        //                     IsFeedbackOptional = courses.IsFeedbackOptional,
        //                     GroupCourseFee = courses.GroupCourseFee,
        //                     IsVisibleAfterExpiry = courses.IsVisibleAfterExpiry,
        //                     IsDashboardCourse = courses.IsDashboardCourse,
        //                     isAssessmentReview = courses.isAssessmentReview,
        //                     PublishCourse = courses.PublishCourse,
        //                     IsPrivateContent = courses.IsPrivateContent,
        //                     VendorId = courses.VendorId,
        //                     VendorName = vendor.Name,
        //                     VendorType = vendor.Type,
        //                     LxpDetails = courses.LxpDetails,
        //                     OJTId = courses.OJTId,
        //                     IsOJT = courses.IsOJT,
        //                     isVisibleAssessmentDetails = courses.isVisibleAssessmentDetails,
        //                     IsRefresherMandatory = CourseDetails.IsRefresherMandatory,

        //                 }).AsNoTracking();

        //    var course = await Query.FirstOrDefaultAsync();
        //    return course;
        //}

        //public async Task<APILmsCourseResponse> GetLMSCourses(APITtGrCourses aPITtGrCourses, string ConnectionString)
        //{
        //    APILmsCourseResponse aPILmsCourseResponse = new APILmsCourseResponse();

        //    using (CourseContext context = _customerConnection.GetDbContext(ConnectionString))
        //    {

        //        var courses = (from c in context.Course
        //                       where c.IsActive == true && c.IsDeleted == false

        //                       orderby c.Title
        //                       select new APILmsCourseInfo
        //                       {
        //                           Id = c.Id,
        //                           Code = c.Code,
        //                           Title = c.Title,
        //                           CourseType = c.CourseType,
        //                           Description = c.Description != null ? c.Description : "No information provided.",
        //                           ThumbnailPath = c.ThumbnailPath,
        //                           Currency = c.Currency,
        //                           Cost = c.CourseFee,
        //                           DurationInMin = c.DurationInMinutes
        //                       }).AsNoTracking();

        //        if (!string.IsNullOrEmpty(aPITtGrCourses.Search) && !string.IsNullOrEmpty(aPITtGrCourses.SearchText))
        //        {
        //            if (aPITtGrCourses.Search.ToLower().Equals("coursecode"))
        //                courses = courses.Where(x => x.Code.StartsWith(aPITtGrCourses.SearchText));
        //            else if (aPITtGrCourses.Search.ToLower().Equals("coursetitle"))
        //                courses = courses.Where(x => x.Title.StartsWith(aPITtGrCourses.SearchText));
        //        }


        //        aPILmsCourseResponse.TotalRecords = await courses.CountAsync();

        //        courses = courses.OrderByDescending(r => r.Id);
        //        if (aPITtGrCourses.Page != -1)
        //            courses = courses.Skip((Convert.ToInt32(aPITtGrCourses.Page) - 1) * Convert.ToInt32(aPITtGrCourses.PageSize));
        //        if (aPITtGrCourses.PageSize != -1)
        //            courses = courses.Take(Convert.ToInt32(aPITtGrCourses.PageSize));
        //        var course = await courses.ToListAsync();
        //        aPILmsCourseResponse.Data = course;
        //        return aPILmsCourseResponse;

        //    }
        //}

        //public async Task<object> GetCourseVendorDetails(string Vendor_Type)
        //{
        //    var Query = (from vendor in _db.CourseVendorDetail
        //                 where vendor.Type == Vendor_Type && vendor.IsDeleted == Record.NotDeleted && vendor.IsActive == Record.Active
        //                 select new
        //                 {
        //                     vendor.Code,
        //                     vendor.Name,
        //                     vendor.Id
        //                 }).AsNoTracking();

        //    var vendors = await Query.ToListAsync();
        //    return vendors;
        //}

        //public async Task<object> PostDevelopementPlan(int courseId, int UserId, string UserName)
        //{
        //    try
        //    {
        //        bool addFlag = false;
        //        DevelopmentPlanForCourse olddevelopmentPlanForCourse = await _db.DevelopmentPlanForCourse.Where(a => a.CreatedBy == UserId && a.IsDeleted == false).FirstOrDefaultAsync();
        //        if (olddevelopmentPlanForCourse == null)
        //        {
        //            addFlag = true;
        //        }
        //        if (olddevelopmentPlanForCourse != null)
        //        {
        //            var completedcourses = (from course in _db.CourseMappingToDevelopment
        //                                    join ccs in _db.CourseCompletionStatus on course.CourseId equals ccs.CourseId
        //                                    where course.DevelopmentPlanId == olddevelopmentPlanForCourse.Id && ccs.UserId == UserId && ccs.Status == "completed"
        //                                    select new
        //                                    {
        //                                        ccs.Id
        //                                    }).AsNoTracking();
        //            int completedcoursecount = await completedcourses.CountAsync();
        //            if (olddevelopmentPlanForCourse.CountOfMappedCourses == completedcoursecount)
        //            {
        //                addFlag = true;
        //            }
        //        }
        //        if (addFlag == true)
        //        {
        //            DevelopmentPlanForCourse development = new DevelopmentPlanForCourse();
        //            DevelopementPlanCode CourseCode = await this.DevPlanCode(true, null);
        //            development.DevelopmentCode = CourseCode.Prefix + CourseCode.AutoNumber;
        //            development.DevelopmentName = "IDP" + "_" + DateTime.Now.ToString("ddMMyyhhmmss");
        //            development.Status = true;
        //            development.CountOfMappedCourses = 1;
        //            development.AboutPlan = UserName + " " + " developement plan";
        //            development.TargetCompletion = 0;
        //            development.TotalCreditPoints = 0;
        //            development.EnforceLinearApproach = false;
        //            development.NumberOfRules = 0;
        //            development.NumberofMembers = 0;
        //            development.AllowLearningAfterExpiry = false;
        //            development.CreatedDate = DateTime.Now;
        //            development.ModifiedDate = DateTime.Now;
        //            development.ModifiedBy = UserId;
        //            development.CreatedBy = UserId;
        //            development.IsDeleted = false;
        //            development.UploadThumbnail = "../../../assets/img/course-default-images/04.jpg";

        //            await _db.DevelopmentPlanForCourse.AddAsync(development);
        //            await _db.SaveChangesAsync();

        //            CourseMappingToDevelopment developmentPlanCourse1 = new CourseMappingToDevelopment();

        //            developmentPlanCourse1.CourseId = courseId;
        //            developmentPlanCourse1.CreatedBy = UserId;
        //            developmentPlanCourse1.CreatedDate = DateTime.Now;
        //            developmentPlanCourse1.ModifiedDate = DateTime.Now;
        //            developmentPlanCourse1.ModifiedBy = UserId;
        //            developmentPlanCourse1.IsDeleted = false;
        //            developmentPlanCourse1.DevelopmentPlanId = development.Id;

        //            await _db.CourseMappingToDevelopment.AddAsync(developmentPlanCourse1);
        //            await _db.SaveChangesAsync();

        //            UserDevelopmentPlanMapping userDevelopmentPlanMapping = new UserDevelopmentPlanMapping();

        //            userDevelopmentPlanMapping.UserID = Convert.ToInt32(UserId);
        //            userDevelopmentPlanMapping.ConditionForRules = "AND";
        //            userDevelopmentPlanMapping.DevelopmentPlanid = development.Id;
        //            userDevelopmentPlanMapping.CreatedBy = UserId;
        //            userDevelopmentPlanMapping.ModifiedBy = UserId;
        //            userDevelopmentPlanMapping.CreatedDate = DateTime.Now;
        //            userDevelopmentPlanMapping.ModifiedDate = DateTime.Now;


        //            this._db.UserDevelopmentPlanMapping.Add(userDevelopmentPlanMapping);
        //            this._db.SaveChanges();

        //            development.NumberOfRules++;
        //            development.ModifiedBy = UserId;
        //            development.ModifiedDate = DateTime.Now;
        //            development.NumberofMembers = 1;
        //            this._db.DevelopmentPlanForCourse.Update(development);
        //            this._db.SaveChanges();
        //        }
        //        else
        //        {
        //            olddevelopmentPlanForCourse.CountOfMappedCourses = olddevelopmentPlanForCourse.CountOfMappedCourses + 1;
        //            _db.DevelopmentPlanForCourse.Update(olddevelopmentPlanForCourse);
        //            await _db.SaveChangesAsync();

        //            CourseMappingToDevelopment developmentPlanCourse1 = new CourseMappingToDevelopment();

        //            developmentPlanCourse1.CourseId = courseId;
        //            developmentPlanCourse1.CreatedBy = UserId;
        //            developmentPlanCourse1.CreatedDate = DateTime.Now;
        //            developmentPlanCourse1.ModifiedDate = DateTime.Now;
        //            developmentPlanCourse1.ModifiedBy = UserId;
        //            developmentPlanCourse1.IsDeleted = false;
        //            developmentPlanCourse1.DevelopmentPlanId = olddevelopmentPlanForCourse.Id;

        //            await _db.CourseMappingToDevelopment.AddAsync(developmentPlanCourse1);
        //            await _db.SaveChangesAsync();
        //        }
        //        return olddevelopmentPlanForCourse;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        throw ex;
        //    }
        //}

        //public async Task<DevelopementPlanCode> DevPlanCode(bool isIdp = false, string orgcode = null)
        //{
        //    DevelopementPlanCode DevelopementPlanCode = new DevelopementPlanCode();
        //    int maxAutonumber = 0;
        //    string Prefix = null;
        //    try
        //    {
        //        maxAutonumber = _db.DevelopementPlanCode.OrderByDescending(p => p.Id).FirstOrDefault().AutoNumber;

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(Utilities.GetDetailedException(ex));
        //        // throw ex;
        //    }
        //    //string Prefix = _db.DevelopementPlanCode.OrderByDescending(p => p.Id).FirstOrDefault().Prefix;
        //    if (isIdp)
        //    {
        //        //if (orgcode == null)
        //        //{
        //        //    Prefix = "IDP_";
        //        //}
        //        //else if(orgcode.ToLower() == "darwin")
        //        //{
        //        //    Prefix = "ILP_";
        //        //}
        //        //else
        //        //{
        //        //    Prefix = "IDP_";
        //        //}
        //        Prefix = "IDP_";
        //    }
        //    else
        //    {
        //        if (orgcode == null)
        //        {
        //            Prefix = "DP_";
        //        }
        //        else if (orgcode.ToLower() == "darwin")
        //        {
        //            Prefix = "LP_";
        //        }
        //        else
        //        {
        //            Prefix = "DP_";
        //        }

        //    }
        //    if (maxAutonumber == 0)
        //        maxAutonumber = 1000;


        //    DevelopementPlanCode.AutoNumber = maxAutonumber + 1;
        //    DevelopementPlanCode.Prefix = Prefix;
        //    _db.DevelopementPlanCode.Add(DevelopementPlanCode);
        //    await _db.SaveChangesAsync();
        //    return DevelopementPlanCode;
        //}

        //public async Task<AdditionalResourceForCourse> PostAdditionalResourceForCourse(APIAdditionalResourceForCourse data, int UserId)
        //{
        //    AdditionalResourceForCourse resourses = new AdditionalResourceForCourse();
        //    resourses.PathLink = data.PathLink;
        //    resourses.CourseCode = data.CourseCode;
        //    resourses.ContentType = data.ContentType;
        //    resourses.ModifiedBy = UserId;
        //    resourses.CreatedBy = UserId;
        //    resourses.CreatedDate = DateTime.UtcNow;
        //    resourses.ModifiedDate = DateTime.UtcNow;
        //    resourses.IsDeleted = false;
        //    await _db.AdditionalResourceForCourse.AddAsync(resourses);
        //    await _db.SaveChangesAsync();
        //    return resourses;
        //}

        //public async Task<List<AdditionalResourceForCourse>> GetAdditionalResourceForCourse(string courseCode)
        //{
        //    try
        //    {
        //        List<AdditionalResourceForCourse> additionalResourceForCourses = await _db.AdditionalResourceForCourse.Where(a => a.CourseCode == courseCode && a.IsDeleted == false).ToListAsync();
        //        return additionalResourceForCourses;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

        //public async Task<AdditionalResourceForCourse> UpdateAdditionalResourceForCourse(APIAdditionalResourceForCourse data, int UserId)
        //{
        //    AdditionalResourceForCourse additionalResourceForCourse = await _db.AdditionalResourceForCourse.Where(a => a.Id == data.Id).FirstOrDefaultAsync();


        //    additionalResourceForCourse.ModifiedBy = UserId;
        //    additionalResourceForCourse.ModifiedDate = DateTime.Now;
        //    additionalResourceForCourse.PathLink = data.PathLink;
        //    additionalResourceForCourse.CourseCode = data.CourseCode;
        //    additionalResourceForCourse.ContentType = data.ContentType;

        //    this._db.AdditionalResourceForCourse.Update(additionalResourceForCourse);
        //    this._db.SaveChangesAsync();
        //    return additionalResourceForCourse;
        //}

        //public async Task<AdditionalResourceForCourse> DeleteAdditionalResourceForCourse(int id, int UserId)
        //{
        //    AdditionalResourceForCourse additionalResourceForCourse = _db.AdditionalResourceForCourse.Where(a => a.Id == id).FirstOrDefault();


        //    additionalResourceForCourse.ModifiedBy = UserId;
        //    additionalResourceForCourse.ModifiedDate = DateTime.Now;
        //    additionalResourceForCourse.IsDeleted = true;

        //    _db.AdditionalResourceForCourse.Update(additionalResourceForCourse);
        //    await _db.SaveChangesAsync();
        //    return additionalResourceForCourse;
        //}

    }
}