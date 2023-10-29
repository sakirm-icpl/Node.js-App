using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Globalization;
using log4net;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Common;
using Assessment.API.Model;
using Assessment.API.Helper;
using Assessment.API.APIModel.Assessment;
using Assessment.API.Services;

namespace Assessment.API.Repositories
{
    public class PostAssessmentResultRepository : Repository<PostAssessmentResult>, IPostAssessmentResult
    {
        private AssessmentContext _db;
        private IConfiguration _configuration;
        private string url;
        IModuleCompletionStatusRepository _moduleCompletionStatusRepository;
        private readonly ICourseModuleAssociationRepository _courseModuleAssociationRepository;
        ICourseCompletionStatusRepository _courseCompletionStatusRepository;
        private ICourseRepository _courseRepository;
        private IAssessmentQuestion _assessmentQuestion;
        private IAssessmentConfigurationSheets _assessmentConfigurationSheets;
        private IAssessmentSheetConfigurationDetails _assessmentSheetConfigurationDetails;
        private IAssessmentQuestionDetails _postQuestionDetails;
        private IContentCompletionStatus _contentCompletionStatus;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private IRewardsPointRepository _rewardsPointRepository;
        IIdentityService _identitySv;
        INotification _notification;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PostAssessmentResultRepository));

        public PostAssessmentResultRepository(AssessmentContext context,
            IConfiguration configuration,
            IModuleCompletionStatusRepository moduleCompletionStatusRepository,
            ICourseModuleAssociationRepository courseModuleAssociationRepository,
            ICourseRepository courseRepository,
            IAssessmentQuestion assessmentQuestion,
            IAssessmentConfigurationSheets assessmentConfigurationSheets,
            IAssessmentSheetConfigurationDetails assessmentSheetConfigurationDetails,
            IAssessmentQuestionDetails postQuestionDetails,
            ICourseCompletionStatusRepository courseCompletionStatusRepository,
             ICustomerConnectionStringRepository customerConnectionStringRepository,
               IIdentityService identitySv,
               INotification notification,
               IRewardsPointRepository rewardsPointRepository,
        IContentCompletionStatus contentCompletionStatus) : base(context)
        {
            this._db = context;
            this._configuration = configuration;
            this._moduleCompletionStatusRepository = moduleCompletionStatusRepository;
            this._courseModuleAssociationRepository = courseModuleAssociationRepository;
            this._courseCompletionStatusRepository = courseCompletionStatusRepository;
            this._assessmentQuestion = assessmentQuestion;
            this._assessmentConfigurationSheets = assessmentConfigurationSheets;
            this._assessmentSheetConfigurationDetails = assessmentSheetConfigurationDetails;
            this._courseRepository = courseRepository;
            this._postQuestionDetails = postQuestionDetails;
            this._contentCompletionStatus = contentCompletionStatus;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            this._identitySv = identitySv;
            this._notification = notification;
            this._rewardsPointRepository = rewardsPointRepository;
        }
        public async Task<IEnumerable<APIPostAssessmentSubjectiveResult>> GetPostAssessmentResultById(int Id)
        {
            try
            {
                IQueryable<APIPostAssessmentSubjectiveResult> Query = from subjectiveAssessmentStatus in this._db.SubjectiveAssessmentStatus
                                                                      join postAssessmentResult in _db.PostAssessmentResult on subjectiveAssessmentStatus.AssessmentResultID equals postAssessmentResult.Id
                                                                      where postAssessmentResult.IsDeleted == false && subjectiveAssessmentStatus.Id == Id
                                                                      orderby postAssessmentResult.Id descending
                                                                      select new APIPostAssessmentSubjectiveResult
                                                                      {
                                                                          CourseID = subjectiveAssessmentStatus.CourseID,
                                                                          NoOfAttempts = postAssessmentResult.NoOfAttempts,
                                                                          MarksObtained = postAssessmentResult.MarksObtained,
                                                                          TotalMarks = postAssessmentResult.TotalMarks,
                                                                          PassingPercentage = postAssessmentResult.PassingPercentage,
                                                                          AssessmentPercentage = postAssessmentResult.AssessmentPercentage,
                                                                          AssessmentResult = postAssessmentResult.AssessmentResult,
                                                                          TotalNoOfQuestions = postAssessmentResult.TotalNoQuestions,
                                                                          PostAssessmentStatus = postAssessmentResult.PostAssessmentStatus,
                                                                          Id = postAssessmentResult.Id
                                                                      };


                Query = Query.OrderByDescending(v => v.Id);

                var Result = await Query.ToListAsync();
                foreach (APIPostAssessmentSubjectiveResult subjectiveResult in Result)
                {
                    subjectiveResult.CourseName = await _courseRepository.GetCourseNam(subjectiveResult.CourseID);
                }
                return Result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<int> GetNoOfAttempts(int UserId, int CourseId, int? ModuleId, bool isPreassessment, bool isContentAssessment)
        {
            // raw query
            ModuleId = ModuleId == null ? 0 : ModuleId;
            return await _db.PostAssessmentResult
                .Where(c => c.CreatedBy == UserId
                && c.CourseID == CourseId && c.ModuleId == ModuleId
                && c.IsContentAssessment == isContentAssessment && c.IsPreAssessment == isPreassessment
            )
            .OrderByDescending(p => p.Id)
            .Select(p => p.NoOfAttempts).FirstOrDefaultAsync();
        }

        public async Task<int> GetMaxNoOfAttempts(int AssessmentConfigId, string OrgCode)
        {
            var cache = new CacheManager.CacheManager();
            string cacheKeyConfig = CacheKeyNames.COURSE_MAX_ATTEMPTS + AssessmentConfigId + OrgCode;
            int maxAttempts = 0;
            try
            {
                if (cache.IsAdded(cacheKeyConfig))
                    maxAttempts = Convert.ToInt32(cache.Get<string>(cacheKeyConfig));
                else
                {
                    maxAttempts = await _db.AssessmentSheetConfiguration.AsNoTracking()
                    .Where(c => c.ID == AssessmentConfigId).Select(p => p.MaximumNoOfAttempts).FirstOrDefaultAsync();
                    cache.Add<string>(cacheKeyConfig, Convert.ToString(maxAttempts), System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception in function GetMaxNoOfAttempts :-" + Utilities.GetDetailedException(ex));
            }

            return maxAttempts;
        }


        public string GetAssessmentStatus(int UserId, int CourseId, int? ModuleID)
        {
            if (ModuleID != null)
                return _db.PostAssessmentResult.Where(c => c.CreatedBy == UserId && c.CourseID == CourseId && c.ModuleId == ModuleID).Select(m => m.PostAssessmentStatus).SingleOrDefault();
            return _db.PostAssessmentResult.Where(c => c.CreatedBy == UserId && c.CourseID == CourseId && c.ModuleId == null).Select(m => m.PostAssessmentStatus).SingleOrDefault();
        }

        public async Task AddModuleCompleteionStatus(int UserId, int CourseId, int ModuleId, string? OrgCode = null)
        {
            bool IsFeedbackOptional = await _db.Course.Where(x => x.Id == CourseId && x.IsActive == true && x.IsDeleted == false).Select(x => x.IsFeedbackOptional).FirstOrDefaultAsync();
            bool FeedbackExist = await _courseModuleAssociationRepository.IsFeedbackExist(CourseId, ModuleId);
            ModuleCompletionStatus ModuleStatus = new ModuleCompletionStatus();

            if (!FeedbackExist || IsFeedbackOptional)
                ModuleStatus.Status = Status.Completed;
            else
                ModuleStatus.Status = Status.InProgress;

            ModuleStatus.CourseId = CourseId;
            ModuleStatus.ModuleId = ModuleId;
            ModuleStatus.UserId = UserId;
            ModuleStatus.CreatedDate = DateTime.UtcNow;
            ModuleStatus.ModifiedDate = DateTime.UtcNow;
            if (ModuleId != 0)
                await _moduleCompletionStatusRepository.Post(ModuleStatus, null, null, OrgCode);
            else
            {
                CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
                courseCompletionStatus.CourseId = CourseId;
                courseCompletionStatus.UserId = UserId;
                await this._courseCompletionStatusRepository.Post(courseCompletionStatus, OrgCode);
            }
        }

        public async Task ReverseCourseStatusByMangerEvaluation(int UserId, int CourseId)
        {
            var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "ReverseCourseStatusByMangerEvaluation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = CourseId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    reader.Dispose();
                }
                connection.Close();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                throw ex;
            }
        }

        public async Task AddCompleteionStatusForAdaptiveCourse(int UserId, int CourseId)
        {
            List<CourseModuleAssociation> Modules = await this._db.CourseModuleAssociation.Where(cm => cm.CourseId == CourseId).ToListAsync();

            foreach (CourseModuleAssociation module in Modules)
            {
                ModuleCompletionStatus ModuleStatus = new ModuleCompletionStatus();
                ModuleStatus.CourseId = module.CourseId;
                ModuleStatus.ModuleId = module.ModuleId;
                ModuleStatus.UserId = UserId;
                ModuleStatus.CreatedDate = DateTime.UtcNow;
                ModuleStatus.ModifiedDate = DateTime.UtcNow;
                ModuleStatus.Status = Status.Completed;
                await _moduleCompletionStatusRepository.Post(ModuleStatus);
            }
        }
        public async Task AddCompleteionStatusForAdaptiveCourseModule(int UserId, int CourseId, int ModuleId)
        {
            CourseModuleAssociation Module = await this._db.CourseModuleAssociation.Where(cm => cm.CourseId == CourseId && cm.ModuleId == ModuleId).FirstOrDefaultAsync();
            ModuleCompletionStatus ModuleStatus = new ModuleCompletionStatus();
            ModuleStatus.CourseId = Module.CourseId;
            ModuleStatus.ModuleId = Module.ModuleId;
            ModuleStatus.UserId = UserId;
            ModuleStatus.CreatedDate = DateTime.UtcNow;
            ModuleStatus.ModifiedDate = DateTime.UtcNow;
            ModuleStatus.Status = Status.Completed;
            try
            {
                await _moduleCompletionStatusRepository.Post(ModuleStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
        }
        private async Task<int> AddContentAsAssessmentStatus(int UserId, int CourseId, int ModuleId, string postAssessmentStatus)
        {
            string ContentStatus = null;
            if (postAssessmentStatus == Status.Completed)
                ContentStatus = Status.Completed;
            else
                ContentStatus = Status.InProgress;
            ContentCompletionStatus ContentCompletion = new ContentCompletionStatus();
            ContentCompletion.CourseId = CourseId;
            ContentCompletion.ModuleId = ModuleId;
            ContentCompletion.CreatedDate = DateTime.UtcNow;
            ContentCompletion.UserId = UserId;
            ContentCompletion.Status = ContentStatus;
            await _contentCompletionStatus.Post(ContentCompletion);
            return 1;
        }

        private async Task<bool> GetDataForContentAssessment(int CourseId, string OrgCode)
        {
            bool contentFlag = false;
            try
            {
                var cache = new CacheManager.CacheManager();
                string cacheKeyConfig = CacheKeyNames.COURSE_MODULE_COUNT + CourseId + OrgCode;
                int ModuleCount;
                if (cache.IsAdded(cacheKeyConfig))
                    ModuleCount = Convert.ToInt32(cache.Get<string>(cacheKeyConfig));
                else
                {
                    ModuleCount = await _db.Course.AsNoTracking().Where(r => r.Id == CourseId && r.CourseType == CourseType.Assessment && r.IsAssessment == false && r.IsFeedback == false && r.IsAssignment == false && r.IsModuleHasAssFeed == false).Select(r => r.TotalModules).FirstOrDefaultAsync();
                    cache.Add<string>(cacheKeyConfig, Convert.ToString(ModuleCount), System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                }
                if (ModuleCount == 1) // assessment type course with only content as assessment 
                    contentFlag = true;
            }
            catch (Exception ex)
            {
                _logger.Error("Exception in function GetDataForContentAssessment :-" + Utilities.GetDetailedException(ex));
                throw;
            }
            return contentFlag;
        }

        private async Task<int> InsertSingleModuleCompletion(int UserId, int CourseId, int ModuleId, string postAssessmentStatus, string? OrgCode = null)
        {

            var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "InsertSingleModuleCompletion";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = CourseId });
                    cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = ModuleId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = postAssessmentStatus });

                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                    reader.Dispose();
                }
                if (postAssessmentStatus == "completed" && (OrgCode.ToLower().Equals("ent") || OrgCode.ToLower().Equals("iocl") || OrgCode.ToLower().Equals("ghfl") || OrgCode.ToLower().Equals("uco") || OrgCode.ToLower().Equals("hdfc")))
                {
                    Assessment.API.Model.Course Course = await _courseRepository.Get(CourseId);
                    await this._rewardsPointRepository.AddCourseReward(Course, UserId, OrgCode);
                    CourseCompletionStatus courseCompletionStatus = await _courseCompletionStatusRepository.Get(UserId, CourseId);
                    DateTime CourseAssignedDate = await this._courseCompletionStatusRepository.GetCourseAssignedDate(UserId, CourseId);
                    int DateDifference = (courseCompletionStatus.ModifiedDate.Date - CourseAssignedDate.Date).Days;

                    if (DateDifference != 0 || DateDifference == 0 && Course.IsApplicableToAll == false)
                    {
                        await this._rewardsPointRepository.AddCourseCompletionBaseOnDate(Course, UserId, DateDifference, OrgCode);
                    }
                    else
                    {
                        DateTime CreatedDate = Course.CreatedDate;
                        CourseCompletionStatus courseCompletionStatus1 = await _courseCompletionStatusRepository.Get(UserId, CourseId);
                        int DateDifferenceForCreated = (courseCompletionStatus1.ModifiedDate.Date - CreatedDate.Date).Days;
                        if (Course.IsApplicableToAll == true && DateDifferenceForCreated != 0 || DateDifferenceForCreated == 0)
                        {
                            await this._rewardsPointRepository.AddCourseCompletionBaseOnDate(Course, UserId, DateDifferenceForCreated, OrgCode);
                        }
                    }
                }
                connection.Close();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                throw ex;
            }
            return 1;
        }
        private async Task<int> InsertContentAsAssessmentStatus(int UserId, int CourseId, int ModuleId, string postAssessmentStatus)
        {
            string ContentStatus = null;
            if (postAssessmentStatus == Status.Completed)
                ContentStatus = Status.Completed;
            else
                ContentStatus = Status.InProgress;
            ContentCompletionStatus ContentCompletion = new ContentCompletionStatus();
            ContentCompletion.CourseId = CourseId;
            ContentCompletion.ModuleId = ModuleId;
            ContentCompletion.CreatedDate = DateTime.UtcNow;
            ContentCompletion.UserId = UserId;
            ContentCompletion.Status = ContentStatus;
            await _contentCompletionStatus.PostCompletion(ContentCompletion);
            return 1;
        }

        public async Task<ApiResponse> StartAssessment(APIStartAssessment apiStartAssessment, int userId, string OrganisationCode)
        {
            ApiResponse Response = new ApiResponse();

            //if (!await GetDataForContentAssessment(apiStartAssessment.CourseId, OrganisationCode)) // returns true for singlemodule assessment type with no ass feed assignment
            //{
            PostAssessmentResult PostAssessmentObj = new PostAssessmentResult();
            PostAssessmentObj.CourseID = apiStartAssessment.CourseId;
            PostAssessmentObj.ModuleId = apiStartAssessment.ModuleId;

            int Attempts = await this.GetNoOfAttempts(userId, apiStartAssessment.CourseId, apiStartAssessment.ModuleId, apiStartAssessment.IsPreAssessment, apiStartAssessment.IsContentAssessment);

            int MaxAttempts = await this.GetMaxNoOfAttempts(apiStartAssessment.AssessmentSheetConfigID, OrganisationCode);

            List<int> additionalAttempts;
            additionalAttempts = await (from attempts in _db.AssessmentAttemptManagement
                                        where (attempts.UserId == userId && attempts.CourseId == apiStartAssessment.CourseId && attempts.IsDeleted == false
                                        && attempts.IsActive == true && attempts.ModuleId == apiStartAssessment.ModuleId)
                                        select (attempts.AdditionalAttempts)).ToListAsync();

            MaxAttempts = MaxAttempts + additionalAttempts.Sum();
            if (MaxAttempts <= Attempts)
            {
                Response.StatusCode = 404;
                Response.Description = "Max number of attempt reached.";
                return Response;
            }
            if (Attempts == 0)
                PostAssessmentObj.NoOfAttempts = 1;
            else
                PostAssessmentObj.NoOfAttempts = Attempts + 1;

            if (apiStartAssessment.IsAdaptiveLearning)
                PostAssessmentObj.NoOfAttempts = 0;


            var Result = await (from AssessmentConfig in this._db.AssessmentSheetConfiguration
                                join ConfigurationDetail in this._db.AssessmentSheetConfigurationDetails on AssessmentConfig.ID equals ConfigurationDetail.AssessmentSheetConfigID
                                join Questions in this._db.AssessmentQuestion on ConfigurationDetail.QuestionID equals Questions.Id
                                where AssessmentConfig.ID == apiStartAssessment.AssessmentSheetConfigID && Questions.IsDeleted == false
                                select new
                                {
                                    AssessmentConfig.IsFixed,
                                    AssessmentConfig.NoOfQuestionsToShow,
                                    QMarks = Questions.Marks,
                                }).FirstOrDefaultAsync();

            int tmarks = 0;
            if (Result.IsFixed == true)
                tmarks = _assessmentQuestion.GetTotalMark(apiStartAssessment.AssessmentSheetConfigID, OrganisationCode);
            else
                tmarks = (Convert.ToInt32(Result.NoOfQuestionsToShow) * Convert.ToInt32(Result.QMarks));

            PostAssessmentObj.CreatedBy = userId;
            PostAssessmentObj.CreatedDate = DateTime.UtcNow;
            PostAssessmentObj.ModifiedDate = DateTime.UtcNow;
            PostAssessmentObj.IsContentAssessment = apiStartAssessment.IsContentAssessment;
            PostAssessmentObj.IsPreAssessment = apiStartAssessment.IsPreAssessment;
            PostAssessmentObj.TotalMarks = Convert.ToInt32(tmarks);
            PostAssessmentObj.TotalNoQuestions = _assessmentSheetConfigurationDetails.GetTotalQuestion(apiStartAssessment.AssessmentSheetConfigID, OrganisationCode);
            PostAssessmentObj.PassingPercentage = _assessmentConfigurationSheets.GetPassingPercentage(apiStartAssessment.AssessmentSheetConfigID, OrganisationCode);
            PostAssessmentObj.AssessmentPercentage = 0;

            if (apiStartAssessment.IsPreAssessment)
                PostAssessmentObj.PostAssessmentStatus = Status.Completed;
            //else if (PostAssessmentObj.NoOfAttempts == MaxAttempts)
            //    PostAssessmentObj.PostAssessmentStatus = Status.Completed;
            else
                PostAssessmentObj.PostAssessmentStatus = Status.Incompleted;

            PostAssessmentObj.AssessmentResult = Status.Failed;
            PostAssessmentObj.AssessmentStartTime = DateTime.UtcNow;
            PostAssessmentObj.CreatedDate = DateTime.Now;
            PostAssessmentObj.ModifiedDate = DateTime.Now;
            PostAssessmentObj.CreatedBy = userId;

            PostAssessmentObj.IsReviewedBySME = false;

            await this.Add(PostAssessmentObj);
            await this.UpdateModuleCompletions(PostAssessmentObj);

            // }
            int id = _db.PostAssessmentResult.OrderByDescending(a => a.Id).Where(a => a.CourseID == PostAssessmentObj.CourseID &&
            a.ModuleId == PostAssessmentObj.ModuleId && a.CreatedBy == userId
            ).Select(a => a.Id).FirstOrDefault();
            Response.StatusCode = 200;
            Response.ResponseObject = id;
            return Response;
        }


        public async Task<PostAssessmentResult> CheckForAssessmentCompleted(APIPostAssessmentQuestionResult aPIPostAssessmentResult, int UserId)
        {
            PostAssessmentResult postAssessmentResult = new PostAssessmentResult();
            postAssessmentResult = await this._db.PostAssessmentResult.Where(a => a.CourseID == aPIPostAssessmentResult.CourseID && a.ModifiedBy == UserId && a.PostAssessmentStatus == "completed" && a.IsDeleted == Record.NotDeleted
            && a.IsPreAssessment == aPIPostAssessmentResult.IsPreAssessment
            && a.IsContentAssessment == aPIPostAssessmentResult.IsContentAssessment
            && a.ModuleId == aPIPostAssessmentResult.ModuleID
            && a.IsAdaptiveAssessment == aPIPostAssessmentResult.IsAdaptiveLearning).FirstOrDefaultAsync();
            return postAssessmentResult;
        }

        public async Task<PostAssessmentResult> CheckForAssessmentCompletedByUser(APIStartAssessment aPIPostAssessmentResult, int UserId)
        {
            PostAssessmentResult postAssessmentResult = new PostAssessmentResult();
            postAssessmentResult = await this._db.PostAssessmentResult.Where(a => a.CourseID == aPIPostAssessmentResult.CourseId && a.ModifiedBy == UserId && a.PostAssessmentStatus == "completed" && a.IsDeleted == Record.NotDeleted
            && a.IsPreAssessment == aPIPostAssessmentResult.IsPreAssessment
            && a.IsContentAssessment == aPIPostAssessmentResult.IsContentAssessment
            && a.ModuleId == aPIPostAssessmentResult.ModuleId
            && a.IsAdaptiveAssessment == aPIPostAssessmentResult.IsAdaptiveLearning).FirstOrDefaultAsync();
            return postAssessmentResult;
        }

        public async Task<ApiResponse> PostAssessmentQuestion(APIPostAssessmentQuestionResult aPIPostAssessmentResult, int UserId, string? OrgCode = null)
        {
            ApiResponse Response = new ApiResponse();
            APIStartAssessment startAssessment = new APIStartAssessment();
            startAssessment.CourseId = aPIPostAssessmentResult.CourseID;
            startAssessment.ModuleId = aPIPostAssessmentResult.ModuleID;
            startAssessment.IsPreAssessment = aPIPostAssessmentResult.IsPreAssessment;
            startAssessment.IsContentAssessment = aPIPostAssessmentResult.IsContentAssessment;
            startAssessment.IsAdaptiveLearning = aPIPostAssessmentResult.IsAdaptiveLearning;

            var AssCompletionCriteria = await _courseRepository.GetMasterConfigurableParameterValue("Assessment_Inprogress_OnAttempts");
            var AssModuleCompletion = await _courseRepository.GetMasterConfigurableParameterValue("Assessment_Inprogress_Module");
            var AssContentCompetion = await _courseRepository.GetMasterConfigurableParameterValue("Assessment_Inprogress_Content");

            _logger.Debug("Assessment_Inprogress_OnAttempts :-" + AssCompletionCriteria);
            _logger.Debug("Assessment_Inprogress_Module :-" + AssModuleCompletion);
            _logger.Debug("Assessment_Inprogress_Content :-" + AssContentCompetion);



            #region "Add/Fetch assessment sheet configuration Cache"

            var cache = new CacheManager.CacheManager();

            string cacheKeyConfig = Constants.ASSESSMENT_CONFIG + aPIPostAssessmentResult.AssessmentSheetConfigID.ToString() + OrgCode;
            var assesmentConfiguration = new AssessmentSheetConfiguration();
            if (cache.IsAdded(cacheKeyConfig))
            {
                assesmentConfiguration = cache.Get<AssessmentSheetConfiguration>(cacheKeyConfig);
            }
            else
            {
                assesmentConfiguration = _db.AssessmentSheetConfiguration.AsNoTracking().First(x => x.ID == aPIPostAssessmentResult.AssessmentSheetConfigID && x.IsDeleted == false);
                cache.Add<AssessmentSheetConfiguration>(cacheKeyConfig, assesmentConfiguration);
            }

            #endregion
            PostAssessmentResult PostAssessmentobj = await this.GetPostAssessmentResult(startAssessment, UserId);
            if (PostAssessmentobj == null)
            {
                Response.StatusCode = 404;
                Response.Description = "Invalid data.";
                return Response;
            }

            PostAssessmentobj.AssessmentEndTime = DateTime.UtcNow;

            PostAssessmentobj.MarksObtained = CalculateObtainMark(aPIPostAssessmentResult, assesmentConfiguration, OrgCode);
            await this.AddAssessMentQuestionDetails(aPIPostAssessmentResult.aPIPostQuestionDetails, PostAssessmentobj.Id, UserId);
            decimal Percentage = (Convert.ToDecimal(PostAssessmentobj.MarksObtained) / Convert.ToDecimal(PostAssessmentobj.TotalMarks)) * 100;
            PostAssessmentobj.AssessmentPercentage = System.Math.Round(Percentage, 2);


            int MaxNoAttempt = assesmentConfiguration.MaximumNoOfAttempts;

            List<int> additionalAttempts = null;
            if (MaxNoAttempt < PostAssessmentobj.NoOfAttempts)
            {
                try
                {
                    additionalAttempts = await this._db.AssessmentAttemptManagement.Where(attempt => attempt.CourseId == aPIPostAssessmentResult.CourseID && attempt.UserId == UserId && attempt.IsDeleted == false).Select(attempt => attempt.AdditionalAttempts).ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    additionalAttempts = null;
                }
            }

            if (additionalAttempts != null && additionalAttempts.Count > 0)
                MaxNoAttempt = MaxNoAttempt + additionalAttempts.Sum();

            if ((PostAssessmentobj.PassingPercentage <= PostAssessmentobj.AssessmentPercentage) && aPIPostAssessmentResult.IsEvaluationBySME == false)
            {
                PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                PostAssessmentobj.AssessmentResult = Status.Passed;
            }
            else if (PostAssessmentobj.PassingPercentage > PostAssessmentobj.AssessmentPercentage && aPIPostAssessmentResult.IsPreAssessment == false && aPIPostAssessmentResult.IsEvaluationBySME == false)
            {
                PostAssessmentobj.PostAssessmentStatus = Status.Incompleted;
                PostAssessmentobj.AssessmentResult = Status.Failed;

                if (PostAssessmentobj.ModuleId == 0 && PostAssessmentobj.IsContentAssessment == false)
                {
                    if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts && Convert.ToString(AssCompletionCriteria).ToLower() == "no")
                    {
                        PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                    }
                }
                else if (PostAssessmentobj.ModuleId != 0 && PostAssessmentobj.IsContentAssessment == false)
                {
                    if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts && Convert.ToString(AssModuleCompletion).ToLower() == "no")
                    {
                        PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                    }
                }
                else if (PostAssessmentobj.IsContentAssessment == true)
                {
                    if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts && Convert.ToString(AssContentCompetion).ToLower() == "no")
                    {
                        PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                    }
                }
                else
                {
                    if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts)
                    {
                        PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                    }
                }
            }
            else if (aPIPostAssessmentResult.IsPreAssessment)
            {
                if ((PostAssessmentobj.AssessmentPercentage >= PostAssessmentobj.PassingPercentage))
                {
                    PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                    PostAssessmentobj.AssessmentResult = Status.Passed;
                }
                else
                {
                    PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                    PostAssessmentobj.AssessmentResult = Status.Failed;
                }
            }
            else if (aPIPostAssessmentResult.IsEvaluationBySME == true && aPIPostAssessmentResult.IsPreAssessment == false)
            {
                if (aPIPostAssessmentResult.aPIPostQuestionDetails.Where(x => x.OptionType == "subjective").Count() == 0)
                {
                    PostAssessmentobj.IsReviewedBySME = true;
                    if (PostAssessmentobj.PassingPercentage <= PostAssessmentobj.AssessmentPercentage)
                    {
                        PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                        PostAssessmentobj.AssessmentResult = Status.Passed;
                    }
                    else
                    {
                        PostAssessmentobj.PostAssessmentStatus = Status.Incompleted;
                        PostAssessmentobj.AssessmentResult = Status.Failed;
                        if (PostAssessmentobj.ModuleId == 0 && PostAssessmentobj.IsContentAssessment == false)
                        {
                            if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts && Convert.ToString(AssCompletionCriteria).ToLower() == "no")
                            {
                                PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                            }
                        }
                        else if (PostAssessmentobj.ModuleId != 0 && PostAssessmentobj.IsContentAssessment == false)
                        {
                            if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts && Convert.ToString(AssModuleCompletion).ToLower() == "no")
                            {
                                PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                            }
                        }
                        else if (PostAssessmentobj.IsContentAssessment == true)
                        {
                            if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts && Convert.ToString(AssContentCompetion).ToLower() == "no")
                            {
                                PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                            }
                        }
                        else
                        {
                            if (MaxNoAttempt == PostAssessmentobj.NoOfAttempts)
                            {
                                PostAssessmentobj.PostAssessmentStatus = Status.Completed;
                            }
                        }
                    }
                }
                else
                {
                    PostAssessmentobj.PostAssessmentStatus = Status.Incompleted;
                    PostAssessmentobj.AssessmentResult = Status.Failed;

                    try
                    {
                        List<ApiNotification> lstApiNotification = new List<ApiNotification>();
                        string token = _identitySv.GetToken();
                        // get username



                        APIUserReportsTo userdetails = await (from u in _db.UserMaster
                                                              join umd in _db.UserMasterDetails on u.Id equals umd.UserMasterId
                                                              where u.Id == PostAssessmentobj.CreatedBy
                                                              select new APIUserReportsTo
                                                              {
                                                                  username = u.UserName,
                                                                  reportsto = umd.ReportsTo
                                                              }).FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(userdetails.reportsto))
                        {
                            var userReportsToId = (from u in _db.UserMaster
                                                   where u.EmailId == userdetails.reportsto
                                                   select u.Id).Single();



                            Assessment.API.Model.Course courseInfo = await _db.Course.Where(c => c.Id == PostAssessmentobj.CourseID).FirstOrDefaultAsync();


                            string Message = "{UserName} has completed course '{CourseTitle}' , kindly evaluate assessment of the user.";
                            Message = Message.Replace("{CourseTitle}", courseInfo.Title);
                            Message = Message.Replace("{UserName}", userdetails.username);

                            ApiNotification Notification = new ApiNotification();
                            Notification.Title = "Request To Manager For Assessment Evaluation";
                            Notification.Message = Message;
                            Notification.Url = TlsUrl.AssEvaluation;
                            Notification.Type = Record.Assessment;
                            Notification.UserId = Convert.ToInt32(userReportsToId);
                            lstApiNotification.Add(Notification);

                            await _courseCompletionStatusRepository.ScheduleRequestNotificationTo_CommonBulk(lstApiNotification, token);
                            lstApiNotification.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw;
                    }
                }

            }
            else
            {
                PostAssessmentobj.PostAssessmentStatus = Status.Incompleted;
                PostAssessmentobj.AssessmentResult = Status.Failed;
            }

            PostAssessmentobj.IsPreAssessment = aPIPostAssessmentResult.IsPreAssessment;
            PostAssessmentobj.IsContentAssessment = aPIPostAssessmentResult.IsContentAssessment;
            PostAssessmentobj.ModifiedDate = DateTime.Now;
            PostAssessmentobj.ModifiedBy = UserId;

            await this.Update(PostAssessmentobj);

            if (assesmentConfiguration.IsEvaluationBySME == true && aPIPostAssessmentResult.aPIPostQuestionDetails.Where(x => x.OptionType == "subjective").Count() == 0)
                await this.UpdateModuleCompletions(PostAssessmentobj, OrgCode);
            else if (assesmentConfiguration.IsEvaluationBySME == false)
                await this.UpdateModuleCompletions(PostAssessmentobj, OrgCode);

            if (aPIPostAssessmentResult.IsPreAssessment)
            {
                await this._rewardsPointRepository.AddPreAssessment(PostAssessmentobj.CourseID, UserId, OrgCode, Convert.ToInt32(startAssessment.ModuleId));
            }
            if (!aPIPostAssessmentResult.IsPreAssessment && PostAssessmentobj.PostAssessmentStatus == Status.Completed && PostAssessmentobj.AssessmentResult == Status.Passed)
            {
                await this._rewardsPointRepository.AddPostAssessment(PostAssessmentobj.CourseID, UserId, OrgCode, Convert.ToInt32(startAssessment.ModuleId));
            }

            APIAssessmentPostResults postResult = new APIAssessmentPostResults();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            postResult.AssessmentResult = textInfo.ToTitleCase(PostAssessmentobj.AssessmentResult);
            postResult.PostAssessmentStatus = PostAssessmentobj.PostAssessmentStatus;
            postResult.MarksObtained = Math.Round(PostAssessmentobj.MarksObtained, 2);
            postResult.TotalMarks = PostAssessmentobj.TotalMarks;
            // 2 decimal places
            double val = 0;
            if (postResult.TotalMarks > 0)
            {
                val = System.Math.Round(((float)postResult.MarksObtained / postResult.TotalMarks * 100), 2);
            }
            postResult.Percentage = (float)val;

            Response.ResponseObject = postResult;
            Response.StatusCode = 200;
            return Response;

        }



        public async Task<ApiResponse> PostManagerEvaluation(APIPostManagerEvaluationResult aPIPostAssessmentResult, string OrgCode)
        {
            ApiResponse Response = new ApiResponse();

            PostAssessmentResult PostAssessmentobj = new PostAssessmentResult();

            PostAssessmentobj.CourseID = aPIPostAssessmentResult.CourseID;
            PostAssessmentobj.ModuleId = aPIPostAssessmentResult.ModuleID;
            PostAssessmentobj.CreatedBy = aPIPostAssessmentResult.UserId;
            PostAssessmentobj.MarksObtained = CalculateManagerEvaluationark(aPIPostAssessmentResult);
            PostAssessmentobj.IsPreAssessment = aPIPostAssessmentResult.IsPreAssessment;
            PostAssessmentobj.IsContentAssessment = aPIPostAssessmentResult.IsContentAssessment;
            PostAssessmentobj.IsManagerEvaluation = aPIPostAssessmentResult.IsManagerEvaluation;

            APIAssessmentPostResults postResult = new APIAssessmentPostResults();
            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@CourseID", aPIPostAssessmentResult.CourseID);
                    parameters.Add("@ModuleID", aPIPostAssessmentResult.ModuleID);
                    parameters.Add("@UserId", aPIPostAssessmentResult.UserId);
                    parameters.Add("@AssessmentSheetConfigID", aPIPostAssessmentResult.AssessmentSheetConfigID);
                    parameters.Add("@MarksObtained", PostAssessmentobj.MarksObtained);
                    parameters.Add("@IsManagerEvaluation", aPIPostAssessmentResult.IsManagerEvaluation);
                    postResult = await SqlMapper.QueryFirstOrDefaultAsync<APIAssessmentPostResults>((SqlConnection)connection, "[dbo].[InsertManagerEvaluationForCourse]", parameters, null, null, CommandType.StoredProcedure);
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            if (postResult.InsertedID == 0)
            {
                Response.StatusCode = 404;
                Response.Description = "Max number of attempt reached.";
                return Response;
            }

            PostAssessmentobj.PostAssessmentStatus = postResult.PostAssessmentStatus;
            PostAssessmentobj.MarksObtained = postResult.MarksObtained;
            PostAssessmentobj.TotalMarks = postResult.TotalMarks;
            PostAssessmentobj.Id = postResult.InsertedID;
            PostAssessmentobj.AssessmentResult = postResult.AssessmentResult;
            PostAssessmentobj.AssessmentPercentage = (decimal)postResult.Percentage;

            await this.AddAssessMentQuestionDetails(aPIPostAssessmentResult.aPIPostQuestionDetails, PostAssessmentobj.Id, aPIPostAssessmentResult.UserId);

            Assessment.API.Model.Course Course = await _courseRepository.Get(aPIPostAssessmentResult.CourseID);

            string UserUrl = _configuration[APIHelper.UserAPI];
            string NameById = "GetNameById";
            string ColumnName = "username";
            string Apiurl = UserUrl + NameById + "/" + OrgCode + "/" + ColumnName + "/" + aPIPostAssessmentResult.UserId;
            HttpResponseMessage response = await APIHelper.CallGetAPI(Apiurl);
            string EndUserName = null;
            if (response.IsSuccessStatusCode)
            {
                var result1 = await response.Content.ReadAsStringAsync();
                Title _Title = JsonConvert.DeserializeObject<Title>(result1);
                EndUserName = _Title == null ? null : _Title.Name;
            }


            if (PostAssessmentobj.AssessmentResult.ToLower() == "failed")
            {
                await this.UpdateModuleCompletionsForManager(PostAssessmentobj);

                string title = "Responce From Manager On Evaluation";
                string token = _identitySv.GetToken();
                int UserIDToSend = aPIPostAssessmentResult.UserId;
                string Type = Record.Course;
                string Message = " Dear {UserName} your manager has evaluated your performance for '{CourseTitle}', you need to retake the course based on your managers evaluation.";
                Message = Message.Replace("{CourseTitle}", Course.Title);
                Message = Message.Replace("{UserName}", EndUserName);
                await ScheduleRequestNotificationTo_Common(aPIPostAssessmentResult.CourseID, 0, title, token, UserIDToSend, Message, Type);

            }
            else
            {
                var MgrEval_Inprogress = await _courseRepository.GetMasterConfigurableParameterValue("MgrEval_Inprogress");

                _logger.Debug("MgrEval_Inprogress :-" + MgrEval_Inprogress);

                if (Convert.ToString(MgrEval_Inprogress).ToLower() == "yes")
                {
                    CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
                    courseCompletionStatus.CourseId = aPIPostAssessmentResult.CourseID;
                    courseCompletionStatus.UserId = aPIPostAssessmentResult.UserId;
                    await this._courseCompletionStatusRepository.Post(courseCompletionStatus, OrgCode);
                }

                string title = "Responce From Manager On Evaluation";
                string token = _identitySv.GetToken();
                int UserIDToSend = aPIPostAssessmentResult.UserId;
                string Type = Record.Course;
                string Message = " Dear {UserName} your manager has evaluated your performance for '{CourseTitle}' and your course status is marked as 'completed' on evaluation.";
                Message = Message.Replace("{CourseTitle}", Course.Title);
                Message = Message.Replace("{UserName}", EndUserName);
                await ScheduleRequestNotificationTo_Common(aPIPostAssessmentResult.CourseID, 0, title, token, UserIDToSend, Message, Type);

            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            postResult.AssessmentResult = textInfo.ToTitleCase(PostAssessmentobj.AssessmentResult);
            postResult.PostAssessmentStatus = PostAssessmentobj.PostAssessmentStatus;
            postResult.MarksObtained = PostAssessmentobj.MarksObtained;
            postResult.TotalMarks = PostAssessmentobj.TotalMarks;
            // 2 decimal places
            double val = System.Math.Round(((float)postResult.MarksObtained / postResult.TotalMarks * 100), 2);
            postResult.Percentage = (float)val;
            Response.ResponseObject = postResult;
            Response.StatusCode = 200;
            return Response;

        }

        public async Task<int> ScheduleRequestNotificationTo_Common(int courseId, int ScheduleId, string title, string token, int ReportsToID, string Message, string type, int? CourseId = null)
        {
            ApiNotification Notification = new ApiNotification();
            Notification.Title = title;
            Notification.Message = Message;
            Notification.Url = TlsUrl.NotificationAPost + courseId;
            Notification.Type = type;
            Notification.UserId = ReportsToID;
            await this._notification.ScheduleRequestNotificationTo_Common(Notification, token);
            return 1;

        }

        //this function is written by praffulla Kokadwar
        private double CalculateObtainMark(APIPostAssessmentQuestionResult aPIPostAssessmentResult, AssessmentSheetConfiguration assesmentConfiguration, string OrgCode)
        {
            double obtainedMarks = 0;
            int multipleMark = 0;
            int MarksAfterDeduction = 0;
            float NegativeMarkingPercentage = 0;
            bool? IsNegativePercentAllowed = false;
            bool? IsEvaluationBySME = false;
            try
            {
                APIPostQuestionDetails[] aPIPostQuestionDetailsDistinct = aPIPostAssessmentResult.aPIPostQuestionDetails
                                            .GroupBy(o => o.ReferenceQuestionID)
                                           .Select(o => o.FirstOrDefault()).ToArray();

                // ----- Check for Check Negative Percentage ------ //

                IsNegativePercentAllowed = assesmentConfiguration.IsNegativeMarking;
                IsEvaluationBySME = assesmentConfiguration.IsEvaluationBySME;

                if (IsNegativePercentAllowed == true)
                {
                    NegativeMarkingPercentage = (float)assesmentConfiguration.NegativeMarkingPercentage;
                    NegativeMarkingPercentage = (float)(NegativeMarkingPercentage / 100.00);
                }

                // ----- Check for Check Negative Percentage ------ //

                DataTable questionAnswers = GetAssessmentQuestionAnswers(aPIPostAssessmentResult.AssessmentSheetConfigID, OrgCode);
                DataRow[] dataRow = null;
                DataRow[] drQuestionAndAnswers;

                if (aPIPostQuestionDetailsDistinct.Count() != 0)
                {
                    foreach (APIPostQuestionDetails opt in aPIPostQuestionDetailsDistinct)
                    {
                        if (opt.OptionType.ToLower() != "subjective")
                        {
                            dataRow = questionAnswers.Select("QuestionID ='" + opt.ReferenceQuestionID + "' AND IsCorrectAnswer =" + true);
                            if (opt.OptionType == "MultipleSelection")
                            {

                                if (opt.OptionAnswerId.Distinct().Count() == dataRow.Length)
                                {
                                    foreach (int values in opt.OptionAnswerId)
                                    {
                                        drQuestionAndAnswers = questionAnswers.Select("QuestionID ='" + opt.ReferenceQuestionID + "' AND AnswerID = '" + values + "'");

                                        if (Convert.ToBoolean(drQuestionAndAnswers[0]["IsCorrectAnswer"]) == true)
                                            multipleMark = Convert.ToInt32(drQuestionAndAnswers[0]["Marks"]);
                                        else
                                        {
                                            var MarksTobeCut = Convert.ToInt32(drQuestionAndAnswers[0]["Marks"]);
                                            if (MarksTobeCut != 0)
                                                MarksAfterDeduction = MarksAfterDeduction + MarksTobeCut;
                                            multipleMark = 0;
                                            break;
                                        }
                                    }
                                    opt.Marks = multipleMark;
                                    obtainedMarks = obtainedMarks + multipleMark;
                                }
                            }
                            else
                            {
                                if (opt.OptionAnswerId.Count() > 0)
                                {
                                    drQuestionAndAnswers = questionAnswers.Select("QuestionID ='" + opt.ReferenceQuestionID + "' AND AnswerID = '" + opt.OptionAnswerId[0] + "'");

                                    // Below condition is added to added Subjective questions.
                                    if (drQuestionAndAnswers.Length == 0)
                                        continue;

                                    if (Convert.ToBoolean(drQuestionAndAnswers[0]["IsCorrectAnswer"]) == true)
                                    {
                                        var obtainMark = drQuestionAndAnswers == null ? 0 : Convert.ToInt32(drQuestionAndAnswers[0]["Marks"]);
                                        if (obtainMark != 0)
                                        {
                                            obtainedMarks = obtainedMarks + Convert.ToInt32(obtainMark);
                                            opt.Marks = obtainMark;
                                        }
                                    }
                                    else
                                    {
                                        var MarksTobeCut = Convert.ToInt32(drQuestionAndAnswers[0]["Marks"]);
                                        if (MarksTobeCut != 0)
                                            MarksAfterDeduction = MarksAfterDeduction + MarksTobeCut;
                                    }

                                }
                            }
                        }

                    }
                    obtainedMarks = (obtainedMarks - (NegativeMarkingPercentage * MarksAfterDeduction));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception in function CalculateObtainMark :-" + Utilities.GetDetailedException(ex));
                return 0;
            }
            return System.Math.Round(obtainedMarks, 2);
        }

        private double CalculateManagerEvaluationark(APIPostManagerEvaluationResult aPIPostAssessmentResult)
        {
            double obtainedMarks = 0;
            int multipleMark = 0;
            int MarksAfterDeduction = 0;
            float NegativeMarkingPercentage = 0;
            bool? IsNegativePercentAllowed = false;
            bool? IsEvaluationBySME = false;
            try
            {
                APIPostQuestionDetails[] aPIPostQuestionDetailsDistinct = aPIPostAssessmentResult.aPIPostQuestionDetails
                                            .GroupBy(o => o.ReferenceQuestionID)
                                           .Select(o => o.FirstOrDefault()).ToArray();

                // ----- Check for Check Negative Percentage ------ //

                IsNegativePercentAllowed = this._db.AssessmentSheetConfiguration.Where(a => a.ID == aPIPostAssessmentResult.AssessmentSheetConfigID).Select(a => a.IsNegativeMarking).FirstOrDefault();
                IsEvaluationBySME = this._db.AssessmentSheetConfiguration.Where(a => a.ID == aPIPostAssessmentResult.AssessmentSheetConfigID).Select(a => a.IsEvaluationBySME).FirstOrDefault();

                if (IsNegativePercentAllowed == true)
                {
                    NegativeMarkingPercentage = (float)this._db.AssessmentSheetConfiguration.Where(a => a.ID == aPIPostAssessmentResult.AssessmentSheetConfigID).Select(a => a.NegativeMarkingPercentage).FirstOrDefault();
                    NegativeMarkingPercentage = (float)(NegativeMarkingPercentage / 100.00);
                }


                if (aPIPostQuestionDetailsDistinct.Count() != 0)
                {
                    foreach (APIPostQuestionDetails opt in aPIPostQuestionDetailsDistinct)
                    {
                        if (opt.OptionType.ToLower() != "subjective")
                        {
                            if (opt.OptionType == "MultipleSelection")
                            {
                                if (opt.OptionAnswerId.Distinct().Count() == _assessmentQuestion.GetMultipleObtainedMarksCount(opt.ReferenceQuestionID))
                                {
                                    foreach (int values in opt.OptionAnswerId)
                                    {
                                        var obtianMark = _assessmentQuestion.GetObtainedMarks(opt.ReferenceQuestionID, values);
                                        if (obtianMark != 0)
                                        {
                                            multipleMark = Convert.ToInt32(obtianMark);
                                        }
                                        else
                                        {
                                            var MarksTobeCut = _assessmentQuestion.GetMarksForCut(opt.ReferenceQuestionID);
                                            if (MarksTobeCut != 0)
                                            {
                                                MarksAfterDeduction = MarksAfterDeduction + MarksTobeCut;
                                            }
                                            multipleMark = 0;
                                            break;
                                        }
                                    }
                                    opt.Marks = multipleMark;
                                    obtainedMarks = obtainedMarks + multipleMark;
                                }
                            }
                            else
                            {
                                if (opt.OptionAnswerId.Count() > 0)
                                {
                                    var obtianMark = _assessmentQuestion.GetObtainedMarks(opt.ReferenceQuestionID, opt.OptionAnswerId[0]);
                                    if (obtianMark != 0)
                                    {
                                        obtainedMarks = obtainedMarks + Convert.ToInt32(obtianMark);
                                        opt.Marks = obtainedMarks;
                                    }
                                    else
                                    {
                                        var MarksTobeCut = _assessmentQuestion.GetMarksForCut(opt.ReferenceQuestionID);
                                        if (MarksTobeCut != 0)
                                        {
                                            MarksAfterDeduction = MarksAfterDeduction + MarksTobeCut;
                                        }
                                    }

                                }
                            }
                        }

                    }
                    obtainedMarks = (obtainedMarks - (NegativeMarkingPercentage * MarksAfterDeduction));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }
            return obtainedMarks;
        }

        public DataTable ExecuteStoredProcedure(string spName, SqlParameter[] sqlParameters)
        {
            DataTable dt = new DataTable();
            var connection = this._db.Database.GetDbConnection();//Dont use using statment for connection
            try
            {
                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                    connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var item in sqlParameters)
                    {
                        cmd.Parameters.Add(new SqlParameter(item.ParameterName, item.DbType) { Value = item.Value });
                    }

                    DbDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    reader.Dispose();
                }
                connection.Close();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return dt;

        }

        public DataTable GetAssessmentQuestionAnswers(int assessmentSheetConfigID, string OrgCode)
        {
            var cache = new CacheManager.CacheManager();
            DataTable dt = null;
            string cacheKeyConfig = "AssessmentSheetConfigID:-" + assessmentSheetConfigID + OrgCode;
            try
            {
                if (cache.IsAdded(cacheKeyConfig))
                    dt = cache.Get<DataTable>(cacheKeyConfig);
                else
                {
                    SqlParameter[] sqlParameters = new SqlParameter[1];
                    sqlParameters[0] = new SqlParameter() { SqlDbType = SqlDbType.Int, Value = assessmentSheetConfigID, ParameterName = "@AssessmentID" };
                    dt = ExecuteStoredProcedure("[dbo].[GetAssessmentDetails]", sqlParameters);
                    cache.Add(cacheKeyConfig, dt, System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception in function GetAssessmentQuestionAnswers :-" + Utilities.GetDetailedException(ex));
            }

            return dt;
        }

        public async Task<APIAssessmentPostResults> PostAdaptiveAssessment(APIPostAdaptiveAssessment apiPostAdaptiveAssessment, int userId, string OrganisationCode)
        {

            //getting different moduleIds 
            int[] ApiModuleIds = apiPostAdaptiveAssessment.aPIPostQuestionDetails
                .Where(q => q.ModuleID != null).Select(q => (int)q.ModuleID).Distinct().ToArray();

            int CourseId = apiPostAdaptiveAssessment.aPIPostQuestionDetails.Select(c => c.CourseID).FirstOrDefault();
            int[] ModuleIds = this._db.CourseModuleAssociation.Where(q => q.CourseId == CourseId && q.IsAssessment == true).Select(q => q.ModuleId).ToArray();

            //Hash set used for matching all api module id with original module ids
            var DbModuleIds = new HashSet<int>(ModuleIds);
            bool IsModuleMatches = DbModuleIds.SetEquals(ApiModuleIds);
            if (!IsModuleMatches)
                return null;

            PostAssessmentResult PostAssessmentResult = new PostAssessmentResult();
            bool AdaptiveFailed = false;
            //Calculating result for each module
            foreach (int? ModuleId in ModuleIds)
            {
                List<APIPostAdaptiveQuestionDetails> ModuleAssessmentQuestion = apiPostAdaptiveAssessment.aPIPostQuestionDetails.Where(q => q.ModuleID == ModuleId).ToList();
                PostAssessmentResult ModulePostAssessmentResult = await CalculateModuleAssessmentMarks(ModuleAssessmentQuestion, userId, OrganisationCode);
                PostAssessmentResult.MarksObtained += ModulePostAssessmentResult.MarksObtained;
                PostAssessmentResult.TotalMarks += ModulePostAssessmentResult.TotalMarks;
                PostAssessmentResult.TotalNoQuestions += ModulePostAssessmentResult.TotalNoQuestions;
                PostAssessmentResult.PassingPercentage += ModulePostAssessmentResult.PassingPercentage / ModuleIds.Count();
                PostAssessmentResult.ModuleId = 0;
                PostAssessmentResult.CourseID = ModulePostAssessmentResult.CourseID;
                PostAssessmentResult.NoOfAttempts = 0;
                if (ModulePostAssessmentResult.PostAssessmentStatus == "incompleted")
                {
                    AdaptiveFailed = true;
                }
            }
            if (AdaptiveFailed)
            {
                PostAssessmentResult.PostAssessmentStatus = "incompleted";
                PostAssessmentResult.AssessmentResult = "failed";
            }
            else
            {
                PostAssessmentResult.PostAssessmentStatus = "completed";
                PostAssessmentResult.AssessmentResult = "passed";
            }
            PostAssessmentResult.CreatedBy = userId;
            PostAssessmentResult.CreatedDate = DateTime.UtcNow;
            PostAssessmentResult.ModifiedBy = userId;
            PostAssessmentResult.ModifiedDate = DateTime.UtcNow;
            PostAssessmentResult.NoOfAttempts = 1;
            PostAssessmentResult.AssessmentEndTime = DateTime.UtcNow;
            await AddPostAssessmentStatusForAdaptiveAssessment(PostAssessmentResult);

            APIAssessmentPostResults postAssessmentResultReponse = new APIAssessmentPostResults();
            postAssessmentResultReponse.AssessmentResult = PostAssessmentResult.AssessmentResult;
            postAssessmentResultReponse.MarksObtained = PostAssessmentResult.MarksObtained;
            postAssessmentResultReponse.PostAssessmentStatus = PostAssessmentResult.PostAssessmentStatus;
            postAssessmentResultReponse.TotalMarks = PostAssessmentResult.TotalMarks;

            double val = System.Math.Round(((float)PostAssessmentResult.MarksObtained / PostAssessmentResult.TotalMarks * 100), 2);
            postAssessmentResultReponse.Percentage = (float)val;

            return postAssessmentResultReponse;
        }

        public async Task<PostAssessmentResult> CalculateModuleAssessmentMarks(List<APIPostAdaptiveQuestionDetails> ModuleAssessmentQuestion, int userId, string OrganisationCode)
        {
            int ObtainedMarks = 0;
            List<AssessmentQuestionDetails> AssessmentQuestionDetailsList = new List<AssessmentQuestionDetails>();
            int courseId = 0;
            int moduleId = 0;
            foreach (APIPostAdaptiveQuestionDetails moduleQuestion in ModuleAssessmentQuestion)
            {
                moduleId = moduleQuestion.ModuleID.Value;
                courseId = moduleQuestion.CourseID;
                if (moduleQuestion.OptionAnswerId == null
                    || moduleQuestion.OptionAnswerId.Length == 0
                    || moduleQuestion.ReferenceQuestionID == null
                    || moduleQuestion.ReferenceQuestionID == 0)
                {
                    continue;
                }
                int Marks = 0;

                if (moduleQuestion.OptionType == "MultipleSelection")
                {
                    Marks = await _assessmentQuestion.GetMultipleAnwersMarks(moduleQuestion.ReferenceQuestionID, moduleQuestion.OptionAnswerId.ToList());
                }
                else
                {
                    Marks = _assessmentQuestion.GetObtainedMarks(moduleQuestion.ReferenceQuestionID, moduleQuestion.OptionAnswerId[0]);
                }
                AssessmentQuestionDetails AssessmentQuestionDetails = new AssessmentQuestionDetails();
                AssessmentQuestionDetails.ReferenceQuestionID = moduleQuestion.ReferenceQuestionID.Value;
                AssessmentQuestionDetails.OptionAnswerId = moduleQuestion.OptionAnswerId[0];
                AssessmentQuestionDetails.Marks = Marks;
                AssessmentQuestionDetails.CreatedBy = userId;
                AssessmentQuestionDetails.CreatedDate = DateTime.UtcNow;
                AssessmentQuestionDetailsList.Add(AssessmentQuestionDetails);
                ObtainedMarks += Marks;
            }
            PostAssessmentResult PostAssessmentResult = await CalculateModuleAssesmentResult(courseId, moduleId, userId, ObtainedMarks, OrganisationCode);

            foreach (AssessmentQuestionDetails AssessmentQuestionDetails in AssessmentQuestionDetailsList)
            {
                AssessmentQuestionDetails.AssessmentResultID = PostAssessmentResult.Id;
            }
            await _postQuestionDetails.AddRange(AssessmentQuestionDetailsList);
            return PostAssessmentResult;
        }

        public async Task<PostAssessmentResult> CalculateModuleAssesmentResult(int courseId, int moduleId, int userId, int ObtainedMarks, string OrganisationCode)
        {
            int? AssessmentConfigId = GetAssessementConfigurationId(courseId, moduleId);
            PostAssessmentResult postAssessmentResults = new PostAssessmentResult();
            postAssessmentResults.MarksObtained = ObtainedMarks;
            int tmarks = _assessmentQuestion.GetTotalMark(AssessmentConfigId.Value, OrganisationCode);
            Decimal Percentage = (Convert.ToDecimal(ObtainedMarks) / Convert.ToDecimal(tmarks)) * 100;
            postAssessmentResults.AssessmentPercentage = System.Math.Round(Percentage, 2);
            postAssessmentResults.TotalMarks = Convert.ToInt32(tmarks);
            postAssessmentResults.CourseID = courseId;
            postAssessmentResults.IsAdaptiveAssessment = true;
            postAssessmentResults.TotalNoQuestions = _assessmentSheetConfigurationDetails.GetTotalQuestion(AssessmentConfigId.Value, OrganisationCode);
            postAssessmentResults.PassingPercentage = _assessmentConfigurationSheets.GetPassingPercentage(AssessmentConfigId.Value, OrganisationCode);
            if (postAssessmentResults.PassingPercentage <= postAssessmentResults.AssessmentPercentage)
            {
                postAssessmentResults.PostAssessmentStatus = Status.Completed;
                postAssessmentResults.AssessmentResult = Status.Passed;
            }
            else
            {
                postAssessmentResults.PostAssessmentStatus = Status.Incompleted;
                postAssessmentResults.AssessmentResult = Status.Failed;
            }

            PostAssessmentResult ExistingPostAssessmentResult = await this._db.PostAssessmentResult.Where(cm => cm.CourseID == courseId && cm.CreatedBy == userId && cm.IsAdaptiveAssessment == true).FirstOrDefaultAsync();
            if (ExistingPostAssessmentResult == null)
            {
                postAssessmentResults.ModuleId = 0;
                postAssessmentResults.AssessmentStartTime = DateTime.UtcNow;
                postAssessmentResults.CreatedDate = DateTime.UtcNow;
                postAssessmentResults.ModifiedDate = DateTime.UtcNow;
                postAssessmentResults.CreatedBy = userId;
                postAssessmentResults.ModifiedBy = userId;
                postAssessmentResults.NoOfAttempts = 1;
                await this.Add(postAssessmentResults);
            }


            if (postAssessmentResults.PostAssessmentStatus == Status.Completed)
                await this.AddCompleteionStatusForAdaptiveCourseModule(userId, courseId, moduleId);
            return postAssessmentResults;
        }
        private async Task<int> AddPostAssessmentStatusForAdaptiveAssessment(PostAssessmentResult postAssessmentResult)
        {

            PostAssessmentResult ExistingPostAssessmentResult = await this._db.PostAssessmentResult.Where(cm => cm.CourseID == postAssessmentResult.CourseID && cm.CreatedBy == postAssessmentResult.CreatedBy && cm.IsAdaptiveAssessment == true).FirstOrDefaultAsync();
            if (ExistingPostAssessmentResult == null)
            {
                postAssessmentResult.Id = ExistingPostAssessmentResult.Id;
                Decimal Percentage = (Convert.ToDecimal(postAssessmentResult.MarksObtained) / Convert.ToDecimal(postAssessmentResult.TotalMarks)) * 100;
                postAssessmentResult.AssessmentPercentage = System.Math.Round(Percentage, 2);
                postAssessmentResult.IsAdaptiveAssessment = true;
                postAssessmentResult.ModifiedDate = DateTime.UtcNow;

                await this.Update(postAssessmentResult);
            }
            return 1;
        }
        public int? GetAssessementConfigurationId(int courseId, int moduleId)
        {
            int? AssessmentSheetConfigID =
                    (from courseModule in _db.CourseModuleAssociation
                     join assessment in _db.Module on courseModule.AssessmentId equals assessment.Id
                     join module in _db.Module on courseModule.ModuleId equals module.Id
                     join course in _db.Course on courseModule.CourseId equals course.Id
                     join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                     where (course.Id == courseId && module.Id == moduleId && course.IsDeleted == false &&
                     module.IsDeleted == false && assessment.IsDeleted == false)
                     select (lcms.AssessmentSheetConfigID)).FirstOrDefault();
            return AssessmentSheetConfigID;
        }

        public async Task<PostAssessmentResult> GetPostAssessmentResult(APIStartAssessment startAssessment, int userId)
        {
            if (startAssessment.ModuleId == null)
                startAssessment.ModuleId = 0;

            return await this._db.PostAssessmentResult
                .Where(result =>
                           result.CourseID == startAssessment.CourseId
                        && result.ModuleId == startAssessment.ModuleId
                        && result.IsAdaptiveAssessment == startAssessment.IsAdaptiveLearning
                        && result.IsContentAssessment == startAssessment.IsContentAssessment
                        && result.IsPreAssessment == startAssessment.IsPreAssessment
                        && result.CreatedBy == userId)
                        .OrderByDescending(p => p.Id).FirstOrDefaultAsync();
        }


        public async Task AddAssessMentQuestionDetails(APIPostQuestionDetails[] postQuestionDetails, int postAssessmentId, int userId)
        {
            List<AssessmentQuestionDetails> objAsssessmentQues = new List<AssessmentQuestionDetails>();

            foreach (APIPostQuestionDetails opt in postQuestionDetails)
            {
                int optioncount = opt.OptionAnswerId.Length;
                for (int i = 0; i < optioncount; i++)
                {
                    AssessmentQuestionDetails assessmentQuestions = new AssessmentQuestionDetails();
                    assessmentQuestions.AssessmentResultID = postAssessmentId;
                    assessmentQuestions.ReferenceQuestionID = opt.ReferenceQuestionID.Value;

                    if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                    {
                        assessmentQuestions.OptionAnswerId = opt.OptionAnswerId[i];
                    }
                    assessmentQuestions.Marks = opt.Marks;
                    assessmentQuestions.SelectedAnswer = opt.SelectedAnswer;
                    assessmentQuestions.CreatedBy = userId;
                    assessmentQuestions.CreatedDate = DateTime.UtcNow;
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objAsssessmentQues.Add(assessmentQuestions);
                    }
                }
                if (optioncount == 0 && opt.OptionType.ToLower() == "subjective")
                {
                    AssessmentQuestionDetails assessmentQuestions = new AssessmentQuestionDetails();
                    assessmentQuestions.AssessmentResultID = postAssessmentId;
                    assessmentQuestions.ReferenceQuestionID = opt.ReferenceQuestionID.Value;
                    assessmentQuestions.Marks = opt.Marks;
                    assessmentQuestions.SelectedAnswer = opt.SelectedAnswer;
                    assessmentQuestions.CreatedBy = userId;
                    assessmentQuestions.CreatedDate = DateTime.UtcNow;
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objAsssessmentQues.Add(assessmentQuestions);
                    }
                }
            }
            await _postQuestionDetails.AddRange(objAsssessmentQues);
        }

        public async Task<int> UpdateModuleCompletions(PostAssessmentResult PostAssessmentobj, string? OrgCode = null)
        {

            //if Assessment is content
            if (PostAssessmentobj.ModuleId != null
                && PostAssessmentobj.ModuleId != 0
                && PostAssessmentobj.IsContentAssessment == true
                && PostAssessmentobj.IsPreAssessment == false)
            {
                int ModuleCount = _db.Course.Where(r => r.Id == PostAssessmentobj.CourseID).Select(r => r.TotalModules).FirstOrDefault();

                if (ModuleCount == 1) // assessment type course with only content as assessment 
                {
                    int IsAssFeedAssignment = _db.Course.Where(r => r.Id == PostAssessmentobj.CourseID && r.IsAssessment == false && r.IsFeedback == false && r.IsAssignment == false).Count();

                    bool IsModuleHasAssFeed = _db.Course.Where(r => r.Id == PostAssessmentobj.CourseID).Select(r => r.IsModuleHasAssFeed).FirstOrDefault(); ;

                    if (IsAssFeedAssignment == 1 && IsModuleHasAssFeed == false)
                    {
                        await this.InsertSingleModuleCompletion(PostAssessmentobj.CreatedBy, PostAssessmentobj.CourseID, PostAssessmentobj.ModuleId.Value, PostAssessmentobj.PostAssessmentStatus, OrgCode);

                    }
                    else
                    {
                        await this.AddContentAsAssessmentStatus(PostAssessmentobj.CreatedBy, PostAssessmentobj.CourseID, PostAssessmentobj.ModuleId.Value, PostAssessmentobj.PostAssessmentStatus);
                    }
                }
                else
                {
                    await this.AddContentAsAssessmentStatus(PostAssessmentobj.CreatedBy, PostAssessmentobj.CourseID, PostAssessmentobj.ModuleId.Value, PostAssessmentobj.PostAssessmentStatus);
                }
                return 1;
            }

            //if Assessment is Adaptive
            if (PostAssessmentobj.IsAdaptiveAssessment && PostAssessmentobj.PostAssessmentStatus == Status.Completed)
            {
                await this.AddCompleteionStatusForAdaptiveCourse(PostAssessmentobj.CreatedBy, PostAssessmentobj.CourseID);
                return 1;
            }
            //for Module assessment 
            if (PostAssessmentobj.PostAssessmentStatus == Status.Completed
               && PostAssessmentobj.ModuleId != null
               && PostAssessmentobj.ModuleId != 0
               && PostAssessmentobj.IsContentAssessment == false
               && PostAssessmentobj.IsPreAssessment == false)
            {
                await this.AddModuleCompleteionStatus(PostAssessmentobj.CreatedBy, PostAssessmentobj.CourseID, PostAssessmentobj.ModuleId.Value, OrgCode);
                return 1;
            }
            //for Course assessment 
            else if (PostAssessmentobj.PostAssessmentStatus == Status.Completed
               && PostAssessmentobj.CourseID != 0
              && PostAssessmentobj.ModuleId == 0
              && PostAssessmentobj.IsContentAssessment == false
              && PostAssessmentobj.IsPreAssessment == false)
            {
                await this.AddModuleCompleteionStatus(PostAssessmentobj.CreatedBy, PostAssessmentobj.CourseID, PostAssessmentobj.ModuleId.Value, OrgCode);
                return 1;
            }
            return 0;
        }

        public async Task<int> UpdateModuleCompletionsForManager(PostAssessmentResult PostAssessmentobj)
        {
            if (PostAssessmentobj.AssessmentResult.ToLower() == Status.Failed
              && PostAssessmentobj.CourseID != 0
             && PostAssessmentobj.ModuleId != 0
             && PostAssessmentobj.IsManagerEvaluation == true)
            {
                await this.ReverseCourseStatusByMangerEvaluation(PostAssessmentobj.CreatedBy, PostAssessmentobj.CourseID);
                return 1;
            }
            return 0;
        }

        public async Task<ApiResponse> IsNoAttemptsCompleted(APIStartAssessment apiStartAssessment, int userId, string OrganisationCode)
        {
            ApiResponse Response = new ApiResponse();
            bool IsAssessmenExist = await this._assessmentConfigurationSheets.IsAssessmentConfigurationIdExist(apiStartAssessment);
            if (!IsAssessmenExist)
            {
                Response.StatusCode = 404;
                Response.Description = "Invalid Data";
                return Response;
            }

            PostAssessmentResult PostAssessmentObj = new PostAssessmentResult();
            PostAssessmentObj.CourseID = apiStartAssessment.CourseId;
            PostAssessmentObj.ModuleId = apiStartAssessment.ModuleId;
            int Attempts = await this.GetNoOfAttempts(userId, apiStartAssessment.CourseId, apiStartAssessment.ModuleId, apiStartAssessment.IsPreAssessment, apiStartAssessment.IsContentAssessment);

            if (Attempts == 0)
                PostAssessmentObj.NoOfAttempts = 1;
            else
                PostAssessmentObj.NoOfAttempts = Attempts + 1;

            if (apiStartAssessment.IsAdaptiveLearning)
                PostAssessmentObj.NoOfAttempts = 0;

            int tmarks = _assessmentQuestion.GetTotalMark(apiStartAssessment.AssessmentSheetConfigID, OrganisationCode);
            PostAssessmentObj.CreatedBy = userId;
            PostAssessmentObj.CreatedDate = DateTime.UtcNow;
            PostAssessmentObj.IsContentAssessment = apiStartAssessment.IsContentAssessment;
            PostAssessmentObj.IsPreAssessment = apiStartAssessment.IsPreAssessment;
            PostAssessmentObj.TotalMarks = Convert.ToInt32(tmarks);
            PostAssessmentObj.TotalNoQuestions = _assessmentSheetConfigurationDetails.GetTotalQuestion(apiStartAssessment.AssessmentSheetConfigID, OrganisationCode);
            PostAssessmentObj.PassingPercentage = _assessmentConfigurationSheets.GetPassingPercentage(apiStartAssessment.AssessmentSheetConfigID, OrganisationCode);
            PostAssessmentObj.AssessmentPercentage = 0;

            if (apiStartAssessment.IsPreAssessment)
                PostAssessmentObj.PostAssessmentStatus = Status.Completed;
            else
                PostAssessmentObj.PostAssessmentStatus = Status.Incompleted;

            PostAssessmentObj.AssessmentResult = Status.Failed;
            PostAssessmentObj.AssessmentStartTime = DateTime.UtcNow;
            PostAssessmentObj.CreatedDate = DateTime.Now;
            PostAssessmentObj.CreatedBy = userId;
            await this.Add(PostAssessmentObj);
            await this.UpdateModuleCompletions(PostAssessmentObj);
            Response.StatusCode = 200;
            return Response;
        }

        public async Task<ApiResponse> PostSubjectiveAssessmentReview(APIPostSubjectiveReview aPIPostAssessmentResult, int UserId, string? OrgCode = null)
        {
            ApiResponse Response = new ApiResponse();

            int subjectivemarks = Convert.ToInt32(aPIPostAssessmentResult.aPIPostQuestionDetails.Sum(a => a.ObtainedMarks));
            PostAssessmentResult PostAssessmentobj = await this.Get(aPIPostAssessmentResult.PostAssessmentResultId);
            if (PostAssessmentobj != null)
            {
                APIAssessmentPostResults postResult = new APIAssessmentPostResults();
                try
                {
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@PostAssessmentResultId", aPIPostAssessmentResult.PostAssessmentResultId);
                        parameters.Add("@subjectivemarks", subjectivemarks);

                        postResult = await SqlMapper.QueryFirstOrDefaultAsync<APIAssessmentPostResults>((SqlConnection)connection, "[dbo].[UpdateSubjectiveReviewDetails]", parameters, null, null, CommandType.StoredProcedure);
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                PostAssessmentobj.PostAssessmentStatus = postResult.PostAssessmentStatus;
                PostAssessmentobj.MarksObtained = postResult.MarksObtained;
                PostAssessmentobj.TotalMarks = postResult.TotalMarks;
                PostAssessmentobj.Id = postResult.InsertedID;
                PostAssessmentobj.AssessmentResult = postResult.AssessmentResult;
                PostAssessmentobj.AssessmentPercentage = (decimal)postResult.Percentage;

                await this.UpdateSubjectiveReview(aPIPostAssessmentResult.aPIPostQuestionDetails);
                await this.UpdateModuleCompletions(PostAssessmentobj, OrgCode);


                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                postResult.AssessmentResult = textInfo.ToTitleCase(PostAssessmentobj.AssessmentResult);
                postResult.PostAssessmentStatus = PostAssessmentobj.PostAssessmentStatus;
                postResult.MarksObtained = PostAssessmentobj.MarksObtained;
                postResult.TotalMarks = PostAssessmentobj.TotalMarks;

                double val = 0;
                if (postResult.TotalMarks > 0)
                {
                    val = System.Math.Round(((float)postResult.MarksObtained / postResult.TotalMarks * 100), 2);
                }

                postResult.Percentage = (float)val;
                Response.ResponseObject = postResult;
                Response.StatusCode = 200;
                return Response;
            }
            return Response;
        }

        public async Task UpdateSubjectiveReview(APISubjectiveAQReview[] postQuestionDetails)
        {

            List<AssessmentQuestionDetails> objAsssessmentQues = new List<AssessmentQuestionDetails>();

            foreach (APISubjectiveAQReview opt in postQuestionDetails)
            {

                AssessmentQuestionDetails data = await _postQuestionDetails.Get(opt.AssessmentQuestionDetailsId);
                if (data != null)
                {
                    data.Marks = opt.ObtainedMarks;
                    objAsssessmentQues.Add(data);
                }

            }

            await _postQuestionDetails.UpdateRange(objAsssessmentQues);
        }

        public async Task<List<APITrainingReommendationNeeds>> GetLatestAssessmentSubmitted(int UserId)
        {
            List<APITrainingReommendationNeeds> Response = new List<APITrainingReommendationNeeds>();
            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@userId", UserId);

                    IEnumerable<APITrainingReommendationNeeds> Result = await SqlMapper.QueryAsync<APITrainingReommendationNeeds>((SqlConnection)connection, "dbo.GetRecommondedCoursesByUserId", parameters, null, null, CommandType.StoredProcedure);
                    Response = Result.ToList();

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Response;
        }


    }



}

