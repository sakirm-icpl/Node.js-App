using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Globalization;
using log4net;
using Microsoft.EntityFrameworkCore.Internal;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Assessment.API.Common;
using Assessment.API.Helper;
using Assessment.API.Model.Competency;
using Assessment.API.Repositories.Interfaces.Competency;
using Assessment.API.APIModel;
using Assessment.API.APIModel.Assessment;
using Assessment.API.APIModel.Refactored;
using Assessment.API.Model;
using Feedback.API.APIModel;
using Assessment.API.Model.Assessment;

namespace Assessment.API.Repositories
{
    public class AssessmentQuestionRepository : Repository<AssessmentQuestion>, IAssessmentQuestion
    {
        private string url;
        private AssessmentContext _db;
        private IConfiguration _configuration;
        private IAsessmentQuestionOption _asessmentQuestionOption;
        private readonly IWebHostEnvironment hostingEnvironment;
        private ICourseRepository _courseRepository;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;

        private IAssessmentQuestionRejectedRepository _assessmentQuestionBankRejectedRepository;
        private IMyCoursesRepository _myCoursesRepository;
        private ICompetenciesMasterRepository _competenciesMasterRepository;
        private ICompetenciesAssessmentMappingRepository _competenciesAssessmentRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentQuestionRepository));

        public AssessmentQuestionRepository(IWebHostEnvironment environment, ICustomerConnectionStringRepository customerConnectionStringRepository, ICompetenciesMasterRepository competenciesMasterRepository, ICompetenciesAssessmentMappingRepository competenciesAssessmentRepository, AssessmentContext context, IConfiguration configuration, IAsessmentQuestionOption asessmentQuestionOption, IAssessmentQuestionRejectedRepository assessmentQuestionRejectedRepository, ICourseRepository courseRepository, IMyCoursesRepository myCoursesRepository) : base(context)
        {
            this._db = context;
            _configuration = configuration;
            this.hostingEnvironment = environment;
            this._asessmentQuestionOption = asessmentQuestionOption;
            this._assessmentQuestionBankRejectedRepository = assessmentQuestionRejectedRepository;
            this._courseRepository = courseRepository;
            this._myCoursesRepository = myCoursesRepository;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            _competenciesMasterRepository = competenciesMasterRepository;
            _competenciesAssessmentRepository = competenciesAssessmentRepository;
        }
        public async Task<int> Count(string? search = null, string? columnName = null)
        {
            IQueryable<AssessmentQuestion> Query = _db.AssessmentQuestion.Where(r => r.IsDeleted == false);

            if (columnName == "null")
                columnName = null;
            if (search == "null" || search.ToLower() == "undefined")
                search = null;

            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(columnName))
                {
                    if (columnName.ToLower().Equals("metadata"))
                        Query = Query.Where(r => r.Metadata.Contains(search));
                    if (columnName.ToLower().Equals("question"))
                        Query = Query.Where(r => r.QuestionText.Contains(search));
                }
                else
                {
                    Query = Query.Where(r => r.Metadata.Contains(search) || r.QuestionText.Contains(search));
                }
            }
            return await Query.Select(q => q.Id).CountAsync();
        }

        public async Task<IEnumerable<APIGetQuestionMaster>> GetAllQuestionPagination(int page, int pageSize, string? search = null, string? columnName = null, bool? isMemoQuestions = null)
        {
            IQueryable<APIGetQuestionMaster> Query = from ques in this._db.AssessmentQuestion
                                                     orderby ques.Id descending
                                                     where ques.IsDeleted == false
                                                     select new APIGetQuestionMaster
                                                     {
                                                         Id = ques.Id,
                                                         OptionType = ques.OptionType,
                                                         Marks = ques.Marks,
                                                         OptionsCount = _db.AssessmentQuestionOption.Where(x => x.QuestionID == ques.Id && x.IsDeleted == false).Count(),
                                                         DifficultyLevel = ques.DifficultyLevel,
                                                         Status = ques.Status,
                                                         QuestionId = ques.Id,
                                                         Question = ques.QuestionText,
                                                         Section = ques.Section,
                                                         Metadata = ques.Metadata,
                                                         IsDeleted = ques.IsDeleted,
                                                         IsMemoQuestion = ques.IsMemoQuestion
                                                     };

            if (columnName == "null")
                columnName = null;
            if (search == "null" || search?.ToLower() == "undefined")
                search = null;


            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(columnName))
                {
                    if (columnName.ToLower().Equals("metadata"))
                        Query = Query.Where(r => r.Metadata.Contains(search));
                    if (columnName.ToLower().Equals("question"))
                        Query = Query.Where(r => r.Question.Contains(search));
                }
                else
                {
                    Query = Query.Where(r => r.Metadata.Contains(search) || r.Question.Contains(search));
                }
            }

            if (isMemoQuestions != null)
                Query = Query.Where(q => q.IsMemoQuestion == isMemoQuestions);

            Query = Query.OrderByDescending(v => v.Id);
            if (page != -1)
            {
                Query = Query.Skip((page - 1) * pageSize);
            }
            if (pageSize != -1)
            {
                Query = Query.Take(pageSize);
            }
            return await Query.ToListAsync();
        }
        public async Task<IEnumerable<APIGetQuestionMaster>> GetAllQuestionMaster(int page, int pageSize, int UserId, string userRole, bool showAllData = false, string? search = null, string? columnName = null, bool? isMemoQuestions = null)
        {
            try
            {
                List<APIGetQuestionMaster> QuestionList = new List<APIGetQuestionMaster>();

                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Search", search);
                    parameters.Add("@IsMemoquestion", isMemoQuestions);
                    parameters.Add("@UserId", UserId);
                    parameters.Add("@ShowAllData", showAllData);
                    var Result = await SqlMapper.QueryAsync<APIGetQuestionMaster>((SqlConnection)connection, "[dbo].[AssessmentQuestionData_Export]", parameters, null, null, CommandType.StoredProcedure);
                    QuestionList = Result.ToList();
                    connection.Close();
                }

                return QuestionList;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }
        public async Task<IEnumerable<APIGetQuestionMaster>> GetAllActiveQuestion(int page, int pageSize, string? search = null, string? columnName = null, bool? isMemoQuestions = null)
        {
            try
            {
                IQueryable<APIGetQuestionMaster> Query = from ques in this._db.AssessmentQuestion
                                                         orderby ques.Id descending
                                                         where ques.IsDeleted == false &&
                                                         ques.Status == true
                                                         select new APIGetQuestionMaster
                                                         {
                                                             OptionType = ques.OptionType,
                                                             Marks = ques.Marks,
                                                             OptionsCount = _db.AssessmentQuestionOption.Where(x => x.QuestionID == ques.Id && x.IsDeleted == false).Count(),
                                                             DifficultyLevel = ques.DifficultyLevel,
                                                             Status = ques.Status,
                                                             Id = ques.Id,
                                                             Question = ques.QuestionText,
                                                             Section = ques.Section,
                                                             Metadata = ques.Metadata,
                                                             IsDeleted = ques.IsDeleted,
                                                             IsMemoQuestion = ques.IsMemoQuestion,
                                                             QuestionId = ques.Id,
                                                         };

                if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(columnName))
                {
                    if (columnName.ToLower().Equals("metadata"))
                        Query = Query.Where(r => r.Metadata.Contains(search));
                    if (columnName.ToLower().Equals("question"))
                        Query = Query.Where(r => r.Question.Contains(search));
                }
                if (!string.IsNullOrEmpty(search))
                    Query = Query.Where(v => v.Metadata.Contains(search) || v.Question.Contains(search));
                if (isMemoQuestions != null)
                    Query = Query.Where(q => q.IsMemoQuestion == isMemoQuestions);
                else
                    Query = Query.Where(q => q.IsMemoQuestion == false);
                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                }
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<int> ActiveQustionsCount(string? search = null, string? columnName = null, bool? isMemoQuestions = null)
        {
            IQueryable<AssessmentQuestion> Query = this._db.AssessmentQuestion.Where(q => q.IsDeleted == false && q.Status == true);

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(columnName))
            {
                if (columnName.ToLower().Equals("metadata"))
                    Query = Query.Where(r => r.Metadata.Contains(search));
                if (columnName.ToLower().Equals("question"))
                    Query = Query.Where(r => r.QuestionText.Contains(search));
            }
            if (!string.IsNullOrEmpty(search))
                Query = Query.Where(v => v.Metadata.Contains(search) || v.QuestionText.Contains(search));
            if (isMemoQuestions != null)
                Query = Query.Where(v => v.IsMemoQuestion == isMemoQuestions);
            else
                Query = Query.Where(v => v.IsMemoQuestion == false);
            return await Query.Select(q => q.Id).CountAsync();
        }

        public async Task<APIAssessmentQuestion> GetAssessmentQuestionByID(int QuestionId)
        {

            AssessmentQuestion questionBank = _db.AssessmentQuestion.Where(c => c.Id == QuestionId).FirstOrDefault();
            APIAssessmentQuestion questionBankAPI = new APIAssessmentQuestion();



            var QusestionNAnwser = await (from Questions in _db.AssessmentQuestion
                                          join Options in _db.AssessmentQuestionOption on Questions.Id equals Options.QuestionID into temp
                                          from Options in temp.DefaultIfEmpty()
                                          join course in _db.Course on Questions.CourseId equals course.Id into questioncourse
                                          from course in questioncourse.DefaultIfEmpty()
                                          where Questions.Id == QuestionId
                                          select new
                                          {
                                              LearnerInstruction = Questions.LearnerInstruction,
                                              DifficultyLevel = Questions.DifficultyLevel,
                                              Marks = Questions.Marks,
                                              ModelAnswer = Questions.ModelAnswer,
                                              QuestionText = Questions.QuestionText,
                                              AnswerAsImages = Questions.AnswerAsImages,
                                              QuestionStyle = Questions.QuestionStyle,
                                              QuestionType = Questions.QuestionType,
                                              Section = Questions.Section,
                                              Id = Questions.Id,
                                              Metadata = Questions.Metadata,
                                              Status = Questions.Status,
                                              OptionType = Questions.OptionType,
                                              IsMemoQuestion = Questions.IsMemoQuestion,
                                              ContentType = Questions.ContentType,
                                              ContentPath = Questions.ContentPath,
                                              CourseId = Questions.CourseId,
                                              CourseTitle = course.Title,
                                              OptionId = Options != null ? Options.Id : 0,
                                              OptionText = Options != null ? Options.OptionText : null,
                                              IsCorrectAnswer = Options.IsCorrectAnswer,
                                              UploadImage = Options.ContentPath
                                          }).ToListAsync();

            APIAssessmentQuestion AssessmentQuestion = (from Questions in QusestionNAnwser
                                                        select new APIAssessmentQuestion
                                                        {
                                                            LearnerInstruction = Questions.LearnerInstruction,
                                                            DifficultyLevel = Questions.DifficultyLevel,
                                                            Marks = Questions.Marks,
                                                            ModelAnswer = Questions.ModelAnswer,
                                                            QuestionText = Questions.QuestionText,
                                                            AnswerAsImages = Questions.AnswerAsImages,
                                                            QuestionStyle = Questions.QuestionStyle,
                                                            QuestionType = Questions.QuestionType,
                                                            Section = Questions.Section,
                                                            Id = Questions.Id,
                                                            Metadata = Questions.Metadata,
                                                            Status = Questions.Status,
                                                            OptionType = Questions.OptionType,
                                                            IsMemoQuestion = Questions.IsMemoQuestion,
                                                            ContentType = Questions.ContentType,
                                                            ContentPath = Questions.ContentPath,
                                                            Options = _db.AssessmentQuestionOption.Where(x => x.QuestionID == QuestionId && x.IsDeleted == false).Count(),
                                                            CourseId = Questions.CourseId,
                                                            CourseTitle = Questions.CourseTitle,

                                                        }).FirstOrDefault();


            AssessmentQuestion.aPIassessmentOptions = (from Answers in QusestionNAnwser
                                                       where Answers.OptionId != 0
                                                       select new AssessmentOptions
                                                       {
                                                           AssessmentOptionID = Answers.OptionId,
                                                           OptionText = Answers.OptionText,
                                                           IsCorrectAnswer = Answers.IsCorrectAnswer,
                                                           UploadImage = Answers.UploadImage,
                                                           OptionContentPath = Answers.UploadImage,
                                                           OptionContentType = Answers.ContentType
                                                       }).ToArray();
            AssessmentQuestion.Options = AssessmentQuestion.aPIassessmentOptions.Count();

            return AssessmentQuestion;

        }

        public async Task<List<APIJobRole>> GetCompetencySkill(int QuestionId)
        {

            List<APIJobRole> resultCompetencySkill = (from AssessmentCompetenciesMapping in this._db.AssessmentCompetenciesMapping
                                                      join c in this._db.CompetenciesMaster on AssessmentCompetenciesMapping.CompetencyId equals c.Id
                                                      where c.IsDeleted == false && AssessmentCompetenciesMapping.IsDeleted == false && AssessmentCompetenciesMapping.AssessmentQuestionId == QuestionId
                                                      select new APIJobRole
                                                      {
                                                          Id = c.Id,
                                                          Name = c.CompetencyName

                                                      }).ToList();

            return resultCompetencySkill;
        }

        public async Task<int> GetTotalOtionsCount(int ID)
        {
            return await this._db.AssessmentQuestionOption.Where(x => x.QuestionID == ID).CountAsync();
        }

        private async Task<HttpResponseMessage> CallAPI(string url)
        {
            using (var client = new HttpClient())
            {

                string apiUrl = this.url;

                var response = await client.GetAsync(url);


                return response;
            }
        }

        public bool QuestionExists(string question)
        {
            if (this._db.AssessmentQuestion.Count(x => x.QuestionText == question && x.IsDeleted == false) > 0)
                return true;
            return false;
        }



        public async Task<IEnumerable<APIAssessmentQuestion>> GetAssessmentQuestionOld(int ConfigureId)
        {
            try
            {

                List<APIGetAssessmentQuestionOptions> QuestionNAnswers = new List<APIGetAssessmentQuestionOptions>();
                try
                {
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@AssesmentConfiguration", ConfigureId);
                        var Result = await SqlMapper.QueryAsync<APIGetAssessmentQuestionOptions>((SqlConnection)connection, "[dbo].[GetAssessmentQuestionOption]", parameters, null, null, CommandType.StoredProcedure);
                        QuestionNAnswers = Result.ToList();
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                bool? IsFixed = QuestionNAnswers.Select(q => q.IsFixed).FirstOrDefault();
                int? NoOfQuestionsToShow = QuestionNAnswers.Select(q => q.NoOfQuestionsToShow).FirstOrDefault();

                //Assign questions to api 
                List<APIAssessmentQuestion> AssessmentQuestions = (from Questions in QuestionNAnswers
                                                                   group Questions by Questions.QuestionId into temp
                                                                   let Questions = temp.FirstOrDefault()
                                                                   select new APIAssessmentQuestion
                                                                   {
                                                                       LearnerInstruction = Questions.LearnerInstruction,
                                                                       DifficultyLevel = Questions.DifficultyLevel,
                                                                       Marks = Questions.Marks,
                                                                       ModelAnswer = Questions.ModelAnswer,
                                                                       QuestionText = Questions.QuestionText,
                                                                       AnswerAsImages = Questions.AnswerAsImages,
                                                                       QuestionType = Questions.QuestionType,
                                                                       QuestionStyle = Questions.QuestionStyle,
                                                                       Section = Questions.Section,
                                                                       Metadata = Questions.Metadata,
                                                                       Status = Questions.Status,
                                                                       OptionType = Questions.OptionType,
                                                                       Id = Questions.QuestionId,
                                                                       ContentPath = Questions.ContentPath,
                                                                       ContentType = Questions.ContentType,
                                                                   }).ToList();
                //Assign options for each question 
                foreach (APIAssessmentQuestion Question in AssessmentQuestions)
                {
                    Question.aPIassessmentOptions = (from Answers in QuestionNAnswers
                                                     where Answers.QId == Question.Id
                                                     select new AssessmentOptions
                                                     {
                                                         AssessmentOptionID = Answers.OptionsId,
                                                         OptionText = Answers.OptionText,
                                                         OptionContentPath = Answers.OptionContentPath,
                                                         OptionContentType = Answers.OptionContentType,
                                                     }).ToArray();
                }
                if (!Convert.ToBoolean(IsFixed))
                {
                    AssessmentQuestions.Shuffle();
                }

                if (IsFixed != null)
                    if (!IsFixed.Value)
                        AssessmentQuestions = AssessmentQuestions.Take(NoOfQuestionsToShow.Value).ToList();

                if (AssessmentQuestions != null)
                    AssessmentQuestions = AssessmentQuestions.Distinct().ToList();

                return AssessmentQuestions;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        static AssessmentConfiguration lockObject = new AssessmentConfiguration();
        public async Task<IEnumerable<APIAssessmentQuestion>> GetAssessmentQuestion(int ConfigureId, string OrgCode)
        {
            try
            {
                var cache = new CacheManager.CacheManager();
                var AssessmentQuestions = new List<APIAssessmentQuestion>();
                string cacheKeyQuestions = Constants.ASSESSMENT_QUESTIONS + ConfigureId.ToString() + OrgCode;
                string cacheKeyConfig = Constants.ASSESSMENT_CONFIG + ConfigureId.ToString() + OrgCode;
                var assesmentConfiguration = new AssessmentSheetConfiguration();
                if (cache.IsAdded(cacheKeyConfig))
                {
                    assesmentConfiguration = cache.Get<AssessmentSheetConfiguration>(cacheKeyConfig);
                }
                else
                {
                    assesmentConfiguration = _db.AssessmentSheetConfiguration.AsNoTracking().First(x => x.ID == ConfigureId && x.IsDeleted == false);
                    cache.Add<AssessmentSheetConfiguration>(cacheKeyConfig, assesmentConfiguration);
                }

                if (cache.IsAdded(cacheKeyQuestions))
                {
                    AssessmentQuestions = cache.Get<List<APIAssessmentQuestion>>(cacheKeyQuestions);
                }
                else
                {
                    lock (lockObject)
                    {
                        if (cache.IsAdded(cacheKeyQuestions))
                        {
                            AssessmentQuestions = cache.Get<List<APIAssessmentQuestion>>(cacheKeyQuestions);
                        }
                        else
                        {
                            AssessmentQuestions = (from AssesmentConfiguration in _db.AssessmentSheetConfiguration
                                                   join AssesmentConfigurationDetails in this._db.AssessmentSheetConfigurationDetails on AssesmentConfiguration.ID equals AssesmentConfigurationDetails.AssessmentSheetConfigID
                                                   join Questions in _db.AssessmentQuestion on AssesmentConfigurationDetails.QuestionID equals Questions.Id
                                                   where AssesmentConfiguration.ID == ConfigureId
                                                   && Questions.IsDeleted == false
                                                   && AssesmentConfigurationDetails.IsDeleted == false
                                                   && AssesmentConfiguration.IsDeleted == false
                                                   select new APIAssessmentQuestion
                                                   {
                                                       LearnerInstruction = Questions.LearnerInstruction,
                                                       DifficultyLevel = Questions.DifficultyLevel,
                                                       Marks = Questions.Marks,
                                                       ModelAnswer = Questions.ModelAnswer,
                                                       QuestionText = Questions.QuestionText,
                                                       AnswerAsImages = Questions.AnswerAsImages,
                                                       QuestionType = Questions.QuestionType,
                                                       QuestionStyle = Questions.QuestionStyle,
                                                       Section = Questions.Section,
                                                       ContentType = Questions.ContentType,
                                                       ContentPath = Questions.ContentPath,
                                                       Metadata = Questions.Metadata,
                                                       Status = Questions.Status,
                                                       OptionType = Questions.OptionType,
                                                       Id = Questions.Id,
                                                       SequenceNumber = AssesmentConfigurationDetails.SequenceNumber
                                                   }).ToList();
                            // Get all questions and answers from database.

                            cache.Add<List<APIAssessmentQuestion>>(cacheKeyQuestions, AssessmentQuestions);
                            AssessmentQuestions = AssessmentQuestions.Distinct().ToList();
                        }
                    }

                }

                if (AssessmentQuestions != null)
                    AssessmentQuestions = AssessmentQuestions.Distinct().ToList();

                bool? IsFixed = assesmentConfiguration.IsFixed;
                int? NoOfQuestionsToShow = assesmentConfiguration.NoOfQuestionsToShow;

                if (!Convert.ToBoolean(IsFixed))
                {
                    AssessmentQuestions.Shuffle();
                }
                else
                {
                    if (Convert.ToBoolean(assesmentConfiguration.IsRandomQuestion))
                        AssessmentQuestions.Shuffle();
                    else
                        AssessmentQuestions = AssessmentQuestions.OrderBy(o => o.SequenceNumber).ToList();
                }

                if (IsFixed != null)
                    if (!IsFixed.Value)
                        AssessmentQuestions = AssessmentQuestions.Take(NoOfQuestionsToShow.Value).ToList();

                var selectedIds = AssessmentQuestions.Select(x => x.Id);
                var options = _db.AssessmentQuestionOption.Where(x => x.IsDeleted == false && selectedIds.Contains(x.QuestionID))
                    .Select(x => new AssessmentOptions()
                    {
                        QuestionId = x.QuestionID,
                        AssessmentOptionID = x.Id,
                        OptionText = x.OptionText,
                        OptionContentPath = x.ContentPath,
                        OptionContentType = x.ContentType
                    }).ToList();

                //// OPTIMIZATION COMMENT Not sure if this value is used anywhere
                //Assign options for each question 
                foreach (APIAssessmentQuestion Question in AssessmentQuestions)
                {
                    Question.aPIassessmentOptions = options.Where(x => x.QuestionId == Question.Id).ToArray();
                }


                return AssessmentQuestions;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        public async Task<IEnumerable<APIAssessmentQuestionConfiguration>> GetAssessmentQuestionByConfigurationId(int ConfigureId)
        {
            return await (from AssesmentConfiguration in _db.AssessmentSheetConfiguration
                          join AssesmentConfigurationDetails in this._db.AssessmentSheetConfigurationDetails on AssesmentConfiguration.ID equals AssesmentConfigurationDetails.AssessmentSheetConfigID
                          join Questions in _db.AssessmentQuestion on AssesmentConfigurationDetails.QuestionID equals Questions.Id
                          join Lcms in _db.LCMS on AssesmentConfiguration.ID equals Lcms.AssessmentSheetConfigID
                          where AssesmentConfiguration.ID == ConfigureId
                          && Questions.IsDeleted == false
                          && AssesmentConfigurationDetails.IsDeleted == false
                          && AssesmentConfiguration.IsDeleted == false
                          && Lcms.IsDeleted == false
                          orderby AssesmentConfigurationDetails.SequenceNumber ascending
                          select new APIAssessmentQuestionConfiguration
                          {
                              OptionType = Questions.OptionType,
                              Question = Questions.QuestionText,
                              QuestionID = Questions.Id,
                              maximumNoOfAttempts = AssesmentConfiguration.MaximumNoOfAttempts,
                              passingPercentage = AssesmentConfiguration.PassingPercentage,
                              Duration = AssesmentConfiguration.Durations,
                              Id = AssesmentConfiguration.ID,
                              QuestionType = Questions.QuestionType,
                              MetaData = Lcms.MetaData,
                              Name = Lcms.Name,
                              Marks = Questions.Marks,
                              IsFixed = AssesmentConfiguration.IsFixed,
                              NoOfQuestionsToShow = AssesmentConfiguration.NoOfQuestionsToShow,
                              IsNegativeMarking = AssesmentConfiguration.IsNegativeMarking,
                              IsRandomQuestion = AssesmentConfiguration.IsRandomQuestion,
                              NegativeMarkingPercentage = AssesmentConfiguration.NegativeMarkingPercentage,
                              Section = Questions.Section,
                              IsEvaluationBySME = AssesmentConfiguration.IsEvaluationBySME
                          }).ToListAsync();
        }
        public async Task<IEnumerable<APIAssessmentReview>> GetQuestionForReview(APIStartAssessment aPIStartAssessment, int UserId)
        {
            //Get all questions and answers from database.
            List<APIAssessmentReview> aPIAssessmentQuestionOptionDetails = new List<APIAssessmentReview>();
            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "dbo.GetAssessmentQuestionReview";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = aPIStartAssessment.CourseId });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId });
                        cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = aPIStartAssessment.ModuleId });
                        cmd.Parameters.Add(new SqlParameter("@IsPreAssessment", SqlDbType.Int) { Value = aPIStartAssessment.IsPreAssessment });
                        cmd.Parameters.Add(new SqlParameter("@IsContentAssessment", SqlDbType.Int) { Value = aPIStartAssessment.IsContentAssessment });
                        cmd.Parameters.Add(new SqlParameter("@IsAdaptiveLearning", SqlDbType.Int) { Value = aPIStartAssessment.IsAdaptiveLearning });
                        cmd.Parameters.Add(new SqlParameter("@AssessmentSheetConfigID", SqlDbType.Int) { Value = aPIStartAssessment.AssessmentSheetConfigID });
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
                            APIAssessmentReview obj = new APIAssessmentReview();

                            obj.Id = string.IsNullOrEmpty(row["id"].ToString()) ? 0 : int.Parse(row["id"].ToString());
                            obj.QuestionId = string.IsNullOrEmpty(row["QuestionId0"].ToString()) ? 0 : int.Parse(row["QuestionId0"].ToString());
                            obj.QuestionType = row["QuestionType"].ToString();
                            obj.QuestionText = row["QuestionText"].ToString();
                            obj.QuestionStyle = row["QuestionStyle"].ToString();
                            obj.AnswerAsImages = row["AnswerAsImages"].ToString();
                            obj.Section = row["Section"].ToString();
                            obj.ContentType = row["ContentType0"].ToString();
                            obj.ContentPath = row["ContentPath0"].ToString();
                            obj.Metadata = row["Metadata"].ToString();
                            obj.Status = string.IsNullOrEmpty(row["Status"].ToString()) ? false : bool.Parse(row["Status"].ToString());
                            obj.OptionId = string.IsNullOrEmpty(row["OptionId"].ToString()) ? 0 : int.Parse(row["OptionId"].ToString());
                            obj.OptionText = row["OptionText"].ToString();
                            obj.OptionType = row["OptionType"].ToString();
                            obj.OptionContentPath = row["OptionContentPath"].ToString();
                            obj.OptionContentType = row["OptionContentType"].ToString();
                            obj.IsCorrectAnswer = string.IsNullOrEmpty(row["IsCorrectAnswer"].ToString()) ? false : bool.Parse(row["IsCorrectAnswer"].ToString());
                            obj.SelectedAnswer = row["SelectedAnswer"].ToString();

                            aPIAssessmentQuestionOptionDetails.Add(obj);
                        }
                        reader.Dispose();
                    }
                    return aPIAssessmentQuestionOptionDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<APIAssessmentMaster> GetAssessmentHeader(int ConfigurationId, int CourseId, int ModuleId, int userId, bool isPreAssessment = false, bool isContentAssessment = false, bool isAdaptiveLearning = false, string OrganisationCode = "")
        {
            try
            {
                var AssessmentMaster = new List<APIAssessmentHeaderDetails>();

                //Cache logic removed for course title getting wrong if same assessment sheet config id is used between multiple courses.

                //var cache = new CacheManager();

                //string cacheKeyAssHeaders = Constants.ASSESSMENT_HEADER + OrganisationCode + ConfigurationId.ToString() + "_" + CourseId.ToString() + "_" + ModuleId.ToString();

                //if (cache.IsAdded(cacheKeyAssHeaders))
                //{
                //    AssessmentMaster = cache.Get<List<APIAssessmentHeaderDetails>>(cacheKeyAssHeaders);
                //}
                //else
                //{
                try
                {
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@AssesmentConfiguration", ConfigurationId);
                        parameters.Add("@CourseID", CourseId);
                        parameters.Add("@ModuleID", ModuleId);
                        var Result = await SqlMapper.QueryAsync<APIAssessmentHeaderDetails>((SqlConnection)connection, "[dbo].[GetAssessmentHeaderDetails]", parameters, null, null, CommandType.StoredProcedure);
                        AssessmentMaster = Result.ToList();
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                //cache.Add<List<APIAssessmentHeaderDetails>>(cacheKeyAssHeaders, AssessmentMaster);
                //}

                List<int> additionalAttempts;

                additionalAttempts = await (from attempts in _db.AssessmentAttemptManagement
                                            where (attempts.UserId == userId && attempts.CourseId == CourseId && attempts.IsDeleted == false
                                            && attempts.IsActive == true && attempts.ModuleId == ModuleId)
                                            select (attempts.AdditionalAttempts)).ToListAsync();

                var AssessmentHeader = AssessmentMaster.FirstOrDefault();
                if (AssessmentHeader == null)
                    return null;

                APIAssessmentMaster Header = new APIAssessmentMaster();
                Header.ConfigurationId = ConfigurationId;
                Header.CourseTitle = AssessmentHeader.CourseTitle;
                Header.CourseCode = AssessmentHeader.Code;
                Header.DurationInMins = AssessmentHeader.Durations;
                Header.ModuleTitle = AssessmentHeader.ModuleTitle;
                Header.PassingPercentage = (float)AssessmentHeader.PassingPercentage;
                Header.thumbnailPath = AssessmentHeader.ThumbnailPath;
                if (isPreAssessment == true || isAdaptiveLearning == true) //2614
                {
                    Header.TotalNumberOfAttempts = 1;
                }
                else
                {
                    if (ModuleId > 0)
                    {

                        Header.TotalNumberOfAttempts = AssessmentHeader.TotalAttempt + (additionalAttempts.Sum());
                    }
                    else
                        Header.TotalNumberOfAttempts = AssessmentHeader.TotalAttempt + (additionalAttempts.Sum());
                }
                Header.IsNegativeMarking = AssessmentHeader.IsNegativeMarking;
                if (AssessmentHeader.NegativeMarkingPercentage != null)
                    Header.NegativeMarkingPercentage = (float)(AssessmentHeader.NegativeMarkingPercentage / 100.00);
                else
                    Header.NegativeMarkingPercentage = null;


                Header.IsEvaluationBySME = AssessmentHeader.IsEvaluationBySME;

                if (AssessmentHeader.IsFixed == true)
                {
                    Header.NoofQuestion = AssessmentMaster.Count();
                    Header.TotalMarks = AssessmentMaster.Where(q => q.Section == "objective" && q.IsEvaluationBySME == false).Sum(q => q.QMarks);
                    if (Header.IsEvaluationBySME == true)
                    {
                        Header.TotalMarks = AssessmentMaster.Where(q => q.Section == "objective" && q.IsEvaluationBySME == true).Sum(q => q.QMarks);
                        Header.TotalMarks += AssessmentMaster.Where(q => q.Section == "subjective" && q.IsEvaluationBySME == true).Sum(q => q.QMarks);
                    }
                }
                else
                {
                    Header.NoofQuestion = AssessmentHeader.NoOfQuestionsToShow;
                    Header.TotalMarks = (Convert.ToInt32(AssessmentHeader.NoOfQuestionsToShow) * Convert.ToInt32(AssessmentHeader.QMarks));
                }

                List<APIAssessmentResultHeader> Postresultlist = new List<APIAssessmentResultHeader>();
                try
                {
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@CourseID", CourseId);
                        parameters.Add("@ModuleID", ModuleId);
                        parameters.Add("@UserId", userId);
                        parameters.Add("@IsPreAssessment", isPreAssessment);
                        parameters.Add("@IsContentAssessment", isContentAssessment);
                        parameters.Add("@IsAdaptiveLearning", isAdaptiveLearning);

                        var Result = await SqlMapper.QueryAsync<APIAssessmentResultHeader>((SqlConnection)connection, "[dbo].[GetPostAssessmentHeaderDetails]", parameters, null, null, CommandType.StoredProcedure);
                        Postresultlist = Result.ToList();
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                var Postresult = Postresultlist.FirstOrDefault();


                if (isAdaptiveLearning || isPreAssessment)
                    Header.NoOfAttempts = 1;

                if (Postresultlist.Count > 0)
                {
                    string PostAssessmentStatus = Postresult.PostAssessmentStatus;

                    Header.NoOfAttempts = Postresult.NoOfAttempts;   // No of attempts by user 
                    if (PostAssessmentStatus == Status.Completed)
                    {
                        double? TotalMarks = Postresult.MarksObtained;
                        decimal percentage = Postresult.AssessmentPercentage;

                        Header.ObtainedMarks = TotalMarks;
                        double val = System.Math.Round(((float)percentage), 2);
                        Header.Percentage = (float)val;

                    }
                    else
                    {
                        double? TotalMarks = Postresult.MarksObtained;
                        decimal percentage = Postresult.AssessmentPercentage;

                        Header.ObtainedMarks = TotalMarks;
                        double val = System.Math.Round(((float)percentage), 2);
                        Header.Percentage = (float)val;
                    }


                    Header.AssessmentStatus = PostAssessmentStatus;

                    int UserAttempts = Postresult.NoOfAttempts;

                    Header.PassingMarks = 0;

                    if (isAdaptiveLearning || isPreAssessment)
                        Header.NoOfAttempts = 0;
                    else
                        Header.NoOfAttempts = Header.TotalNumberOfAttempts - UserAttempts;

                    if (Header.NoOfAttempts < 0)
                        Header.NoOfAttempts = 0;

                    Header.Status = Postresult.AssessmentResult;
                    Header.IsReviewedBySME = Postresult.IsReviewedBySME;
                    Header.Result = Header.Status;

                }
                else
                {
                    Header.NoOfAttempts = Header.TotalNumberOfAttempts;
                }

                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                if (Header.Status != null)
                    Header.Status = textInfo.ToTitleCase(Header.Status);


                if (Header.Result != null)
                    Header.Result = textInfo.ToTitleCase(Header.Result);

                if (Postresultlist.Count > 0)
                {
                    //if (Header.IsNegativeMarking == true)
                    //{
                    List<APIAssessmentHeaderDetails> AssessmentMast = new List<APIAssessmentHeaderDetails>();
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            cmd.CommandText = "dbo.GetAssessmentQuestionReview";
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = CourseId });
                            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.Int) { Value = ModuleId });
                            cmd.Parameters.Add(new SqlParameter("@IsPreAssessment", SqlDbType.Int) { Value = isPreAssessment });
                            cmd.Parameters.Add(new SqlParameter("@IsContentAssessment", SqlDbType.Int) { Value = isContentAssessment });
                            cmd.Parameters.Add(new SqlParameter("@IsAdaptiveLearning", SqlDbType.Int) { Value = isAdaptiveLearning });
                            cmd.Parameters.Add(new SqlParameter("@AssessmentSheetConfigID", SqlDbType.Int) { Value = false });

                            await dbContext.Database.OpenConnectionAsync();
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                return Header;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                APIAssessmentHeaderDetails obj = new APIAssessmentHeaderDetails();

                                obj.QuestionId = string.IsNullOrEmpty(row["QuestionId0"].ToString()) ? 0 : int.Parse(row["QuestionId0"].ToString());
                                obj.IsCorrectAnswer = string.IsNullOrEmpty(row["IsCorrectAnswer"].ToString()) ? false : bool.Parse(row["IsCorrectAnswer"].ToString());
                                obj.SelectedAnswer = row["SelectedAnswer"].ToString();
                                obj.OptionId = string.IsNullOrEmpty(row["OptionId"].ToString()) ? 0 : int.Parse(row["OptionId"].ToString());

                                AssessmentMast.Add(obj);
                            }
                            reader.Dispose();
                        }

                    }
                    Header.NoOfQuestionAttempted = AssessmentMast.Where(s => s.SelectedAnswer == "Yes").Select(a => a.QuestionId).Distinct().Count();
                    Header.CorrectAnswerCount = AssessmentMast.Where(s => s.IsCorrectAnswer == true && s.SelectedAnswer == "Yes").Count();
                    Header.InCorrectAnswerCount = Header.NoOfQuestionAttempted - Header.CorrectAnswerCount;
                    //}
                }
                return Header;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }

        // Check Assessment status
        public async Task<bool> AssessmentStatus(int courseId, int? moduleId, int userId, bool isPreAssessment = false, bool isContentAssessment = false)
        {
            string Result = await (from c in _db.PostAssessmentResult
                                   where c.CreatedBy == userId && c.CourseID == courseId && c.PostAssessmentStatus == Status.Completed
                                   && c.IsPreAssessment == isPreAssessment && c.IsContentAssessment == isContentAssessment
                                   && c.ModuleId == moduleId
                                   select c.PostAssessmentStatus
                            ).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(Result))
                return true;
            return false;
        }

        public int GetTotalMark(int assessmentSheetConfigID, string OrgCode)
        {
            try
            {
                var cache = new CacheManager.CacheManager();
                string cacheKeyConfig = "TotalMarks:IsEvaluationBySME" + assessmentSheetConfigID + OrgCode;
                bool? IsEvaluationBySME;
                if (cache.IsAdded(cacheKeyConfig))
                {
                    if (cache.Get<string>(cacheKeyConfig) == "")
                        IsEvaluationBySME = null;
                    else
                        IsEvaluationBySME = Convert.ToBoolean(cache.Get<string>(cacheKeyConfig));
                }
                else
                {
                    IsEvaluationBySME = this._db.AssessmentSheetConfiguration.Where(a => a.ID == assessmentSheetConfigID).AsNoTracking().Select(a => a.IsEvaluationBySME).FirstOrDefault();
                    if (IsEvaluationBySME == null)
                        cache.Add(cacheKeyConfig, "", System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                    else
                        cache.Add(cacheKeyConfig, IsEvaluationBySME.ToString(), System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));

                }

                if (IsEvaluationBySME == null)
                    IsEvaluationBySME = true;
                try
                {
                    int totalMarks;
                    if (IsEvaluationBySME == false)
                    {
                        cacheKeyConfig = "TotalMarks:" + assessmentSheetConfigID + OrgCode;
                        if (cache.IsAdded(cacheKeyConfig))
                            totalMarks = Convert.ToInt32(cache.Get<string>(cacheKeyConfig));
                        else
                        {
                            totalMarks = (from a in _db.AssessmentQuestion
                                          join c in _db.AssessmentSheetConfigurationDetails
                                          on a.Id equals c.QuestionID
                                          where c.IsDeleted == false && c.AssessmentSheetConfigID == assessmentSheetConfigID && a.IsDeleted == false
                                          select new
                                          {
                                              marks = a.Marks
                                          }).Sum(s => s.marks);
                            cache.Add(cacheKeyConfig, totalMarks.ToString(), System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                        }
                    }
                    else
                    {
                        totalMarks = totalMarks = (from a in _db.AssessmentQuestion
                                                   join c in _db.AssessmentSheetConfigurationDetails
                                             on a.Id equals c.QuestionID
                                                   where c.IsDeleted == false && c.AssessmentSheetConfigID == assessmentSheetConfigID && a.IsDeleted == false
                                                   select new
                                                   {
                                                       marks = a.Marks
                                                   }).Sum(s => s.marks);
                    }

                    return totalMarks;
                }
                catch (Exception ex)
                {
                    _logger.Error("Exception in function GetTotalMark :-" + Utilities.GetDetailedException(ex));
                    return 1;
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Exception in function GetTotalMark :-" + Utilities.GetDetailedException(ex));
                return 1;
            }
        }

        public int GetObtainedMarks(int? referenceQuestionID, int? optionAnswerId)
        {
            return (from questionAssessment in this._db.AssessmentQuestion
                    join questionOtions in _db.AssessmentQuestionOption on questionAssessment.Id equals questionOtions.QuestionID
                    where questionOtions.QuestionID == referenceQuestionID && questionOtions.Id == optionAnswerId && questionOtions.IsCorrectAnswer == true
                    select questionAssessment.Marks).FirstOrDefault();
        }


        public List<QuestionAnswerAssessement> GetMultipleAnwersMarksList(List<int?> referenceQuestionID, List<int?> optionAnswerId)
        {
            return (from questionAssessment in this._db.AssessmentQuestion
                    join questionOtions in _db.AssessmentQuestionOption on questionAssessment.Id equals questionOtions.QuestionID
                    where referenceQuestionID.Contains(questionOtions.QuestionID)
                    select new QuestionAnswerAssessement()
                    {
                        OptionType = questionAssessment.OptionType,
                        Marks = questionAssessment.Marks,
                        QuestionID = questionOtions.QuestionID,
                        AnswerId = questionOtions.Id,
                        IsCorrectAnswer = questionOtions.IsCorrectAnswer,
                    }).ToList();
        }


        public int GetMarksForCut(int? referenceQuestionID)
        {
            return (from questionAssessment in this._db.AssessmentQuestion
                    join questionOtions in _db.AssessmentQuestionOption on questionAssessment.Id equals questionOtions.QuestionID
                    where questionOtions.QuestionID == referenceQuestionID
                    select questionAssessment.Marks).FirstOrDefault();
        }

        public int GetMultipleObtainedMarks(int? referenceQuestionID, int? optionAnswerId)
        {
            return (from questionAssessment in this._db.AssessmentQuestion
                    join questionOtions in _db.AssessmentQuestionOption on questionAssessment.Id equals questionOtions.QuestionID
                    where questionOtions.QuestionID == referenceQuestionID && questionOtions.Id == optionAnswerId && questionOtions.IsCorrectAnswer == true
                    select questionAssessment.Marks).FirstOrDefault();
        }

        public int GetMultipleObtainedMarksCount(int? referenceQuestionID)
        {
            return (from questionAssessment in this._db.AssessmentQuestion
                    join questionOtions in _db.AssessmentQuestionOption on questionAssessment.Id equals questionOtions.QuestionID
                    where questionOtions.QuestionID == referenceQuestionID && questionOtions.IsCorrectAnswer == true
                    select questionOtions.IsCorrectAnswer == true).Count();
        }

        public async Task<bool> Exist(string changedName)

        {
            var count = this._db.AssessmentQuestion.Where(s => string.Equals(s.QuestionText, changedName, StringComparison.CurrentCultureIgnoreCase) && s.IsDeleted == false).Count();
            if (count > 0)
                return true;
            return false;
        }

        public async Task<bool> ExistQuestionOption(APIAssessmentQuestion objAPIAssessmentQuestion)

        {
            // check question and metadata exists in db
            var count = this._db.AssessmentQuestion.Where(s => string.Equals(s.QuestionText.ToLower(), objAPIAssessmentQuestion.QuestionText.ToLower()) && s.IsDeleted == false && string.Equals(Convert.ToString(s.Metadata).ToLower(), objAPIAssessmentQuestion.Metadata.ToLower())).Count();
            if (count > 0)
            {
                List<AssessmentQuestion> APIAssessmentQuestionOldlist = this._db.AssessmentQuestion.Where(s => s.QuestionText == objAPIAssessmentQuestion.QuestionText && s.IsDeleted == false).OrderByDescending(y => y.Id).ToList();

                var selectedIds = APIAssessmentQuestionOldlist.Select(x => x.Id);
                var options = _db.AssessmentQuestionOption.Where(x => x.IsDeleted == false && selectedIds.Contains(x.QuestionID))
                    .Select(x => new AssessmentQuestionOption()
                    {
                        QuestionID = x.QuestionID,
                        Id = x.Id,
                        OptionText = x.OptionText,
                        IsCorrectAnswer = x.IsCorrectAnswer
                    }).ToList();

                foreach (AssessmentQuestion oldque in APIAssessmentQuestionOldlist)
                {
                    //get Options list by OldquestionID
                    List<AssessmentQuestionOption> assoption = options.Where(x => x.QuestionID == oldque.Id).ToList();
                    //check option count
                    if (assoption.Count == objAPIAssessmentQuestion.aPIassessmentOptions.Count())
                    {
                        int matchcount = 0;

                        foreach (AssessmentOptions opt in objAPIAssessmentQuestion.aPIassessmentOptions)
                        {
                            bool exists = assoption.Where(w => w.OptionText.ToLower().Contains(opt.OptionText.ToLower())).Any();
                            if (exists)
                            {
                                matchcount++;
                            }
                            else // break if option not exists
                            {
                                break;
                            }

                            if (assoption.Count == matchcount)
                            {
                                return true;
                            }
                        }//foreach
                    } //if                 
                }//foreach

            }
            return false;
        }


        public async Task<bool> ExistQuestionOptionUpdate(APIAssessmentQuestion objAPIAssessmentQuestion, int id)

        {
            var count = this._db.AssessmentQuestion.Where(s => s.QuestionText.ToLower() == objAPIAssessmentQuestion.QuestionText.ToLower() && s.Id != id && s.IsDeleted == false).Count();

            if (count > 0)
            {
                List<AssessmentQuestion> APIAssessmentQuestionOldlist = this._db.AssessmentQuestion.Where(s => s.QuestionText == objAPIAssessmentQuestion.QuestionText && s.Id != id && s.IsDeleted == false).OrderByDescending(y => y.Id).ToList();

                foreach (AssessmentQuestion oldque in APIAssessmentQuestionOldlist)
                {
                    //get Options list by OldquestionID
                    List<AssessmentQuestionOption> assoption = this._db.AssessmentQuestionOption.Where(s => s.QuestionID == oldque.Id && s.IsDeleted == false).ToList();

                    //check option count
                    if (assoption.Count == objAPIAssessmentQuestion.aPIassessmentOptions.Count())
                    {
                        int matchcount = 0;

                        foreach (AssessmentOptions opt in objAPIAssessmentQuestion.aPIassessmentOptions)
                        {
                            bool exists = assoption.Where(w => w.OptionText.ToLower().Contains(opt.OptionText.ToLower())).Any();
                            if (exists)
                            {
                                matchcount++;
                            }
                            else // break if option not exists
                            {
                                break;
                            }
                            if (assoption.Count == matchcount)
                            {
                                return true;
                            }
                        }//foreach
                    } //if                 
                }//foreach

            }
            return false;
        }

        public async Task<string> ProcessImportFile(FileInfo file, IAssessmentQuestion _assessmentQuestion, IAssessmentQuestionRejectedRepository _questionRejectedRepository, IAsessmentQuestionOption _asessmentQuestionOption, int userid, string OrganisationCode)
        {
            string result;
            try
            {

                AssessmentsQuestionImport.ProcessFile.Reset();
                int resultMessage = await AssessmentsQuestionImport.ProcessFile.InitilizeAsync(file);
                if (resultMessage == 1)
                {
                    result = await AssessmentsQuestionImport.ProcessFile.ProcessRecordsAsync(_assessmentQuestion, _questionRejectedRepository, _asessmentQuestionOption, userid, OrganisationCode);
                    AssessmentsQuestionImport.ProcessFile.Reset();
                    return result;
                }
                else if (resultMessage == 2)
                {
                    result = Record.CannotContainNewLineCharacters;
                    AssessmentsQuestionImport.ProcessFile.Reset();
                    return result;
                }
                else
                {
                    result = Record.FileInvalid;
                    AssessmentsQuestionImport.ProcessFile.Reset();
                    return result;
                }


            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Record.FileInvalid;
        }
        public async Task<int> DeleteQuestion(int[] QuestionsId, int UserId)
        {
            List<AssessmentQuestion> Questions = new List<AssessmentQuestion>();

            List<AssessmentQuestion> Questionsold = _db.AssessmentQuestion.Where(x => x.IsDeleted == false && QuestionsId.Contains(x.Id)).ToList();

            foreach (int QuestionId in QuestionsId)
            {
                AssessmentQuestion Question = Questionsold.Where(x => x.Id == QuestionId).FirstOrDefault();
                //await this.Get(QuestionId);
                if (Question != null)
                {
                    Question.IsDeleted = true;
                    Questions.Add(Question);
                }
            }
            if (Questions.Count > 0)
                await this.UpdateRange(Questions);
            return 1;
        }
        public AssessmentQuestion GetQuestionsById(int? questionId, int UserId)
        {
            return this._db.AssessmentQuestion.Where(c => c.Id == questionId && c.CreatedBy == UserId).FirstOrDefault();
        }

        public async Task<bool> IsQuestionAssigned(int questionId)
        {
            int QuestionCount = await this._db.AssessmentSheetConfigurationDetails.Where(c => c.QuestionID == questionId && c.IsDeleted == Record.NotDeleted).CountAsync();
            if (QuestionCount > 0)
                return true;
            return false;
        }
        public async Task<List<APIAssessmentQuestion>> PostQuestion(List<APIAssessmentQuestion> aPIAssessmentsQuestion, int UserId, string OrganisationCode)
        {
            foreach (var aPIQuestion in aPIAssessmentsQuestion)
            {

                List<APIAssessmentQuestion> errorAssessment = new List<APIAssessmentQuestion>();
                foreach (APIAssessmentQuestion aPIAssessmentQuestion in aPIAssessmentsQuestion)
                {
                    AssessmentQuestion questionAssessments = new AssessmentQuestion();
                    questionAssessments.LearnerInstruction = aPIAssessmentQuestion.LearnerInstruction;
                    questionAssessments.DifficultyLevel = aPIAssessmentQuestion.DifficultyLevel;
                    questionAssessments.Marks = aPIAssessmentQuestion.Marks;
                    questionAssessments.ModelAnswer = aPIAssessmentQuestion.ModelAnswer;
                    questionAssessments.OptionType = aPIAssessmentQuestion.OptionType;
                    questionAssessments.QuestionText = aPIAssessmentQuestion.QuestionText;
                    questionAssessments.QuestionStyle = aPIAssessmentQuestion.QuestionStyle;
                    questionAssessments.AnswerAsImages = aPIAssessmentQuestion.AnswerAsImages;
                    questionAssessments.QuestionType = aPIAssessmentQuestion.QuestionType;
                    questionAssessments.Metadata = aPIAssessmentQuestion.Metadata;
                    questionAssessments.Section = aPIAssessmentQuestion.Section;
                    questionAssessments.Status = aPIAssessmentQuestion.Status;
                    questionAssessments.ContentType = aPIAssessmentQuestion.ContentType;
                    questionAssessments.ContentPath = aPIAssessmentQuestion.ContentPath;
                    questionAssessments.CreatedBy = UserId;
                    questionAssessments.CreatedDate = DateTime.UtcNow;
                    questionAssessments.ModifiedBy = UserId;
                    questionAssessments.ModifiedDate = DateTime.UtcNow;
                    questionAssessments.IsMemoQuestion = aPIAssessmentQuestion.IsMemoQuestion;
                    questionAssessments.CourseId = aPIAssessmentQuestion.CourseId;
                    if (aPIAssessmentQuestion.Section.Equals(FileType.Objective))
                    {
                        if (aPIAssessmentQuestion.aPIassessmentOptions.Length < 2)
                        {
                            errorAssessment.Add(aPIAssessmentQuestion);
                        }
                        else
                        {
                            await this.Add(questionAssessments);

                            foreach (var aPIRoleCompetency in aPIQuestion.CompetencySkillsData)
                            {
                                int competenciesMasterid = 0;
                                if (aPIRoleCompetency.Id != 0)
                                {
                                    competenciesMasterid = Convert.ToInt32(aPIRoleCompetency.Id);
                                }
                                if (competenciesMasterid == 0)
                                {
                                    CompetenciesMaster competenciesMaster = new CompetenciesMaster();
                                    competenciesMaster.CompetencyName = aPIRoleCompetency.Name;
                                    competenciesMaster.CompetencyDescription = aPIRoleCompetency.Name;
                                    competenciesMaster.CategoryId = 0;
                                    competenciesMaster.CreatedBy = UserId;
                                    competenciesMaster.CreatedDate = DateTime.Now;
                                    competenciesMaster.IsActive = true;
                                    competenciesMaster.IsDeleted = false;
                                    competenciesMaster.ModifiedBy = UserId;
                                    competenciesMaster.ModifiedDate = DateTime.Now;
                                    await this._competenciesMasterRepository.Add(competenciesMaster);
                                    competenciesMasterid = competenciesMaster.Id;
                                }
                                if (await this._competenciesAssessmentRepository.Exists(questionAssessments.Id, competenciesMasterid))
                                {
                                    continue;
                                }
                                AssessmentCompetenciesMapping competenciesMapping = new AssessmentCompetenciesMapping();
                                competenciesMapping.AssessmentQuestionId = questionAssessments.Id;
                                competenciesMapping.CompetencyId = competenciesMasterid;
                                competenciesMapping.IsDeleted = false;
                                competenciesMapping.ModifiedBy = UserId;
                                competenciesMapping.ModifiedDate = DateTime.UtcNow;
                                competenciesMapping.CreatedBy = UserId;
                                competenciesMapping.CreatedDate = DateTime.UtcNow;
                                await this._competenciesAssessmentRepository.AddRecord(competenciesMapping);
                            }

                            List<AssessmentQuestionOption> AssessmentQues = new List<AssessmentQuestionOption>();
                            foreach (AssessmentOptions opt in aPIAssessmentQuestion.aPIassessmentOptions)
                            {

                                AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                assessmentOptiones.OptionText = opt.OptionText;
                                assessmentOptiones.IsCorrectAnswer = opt.IsCorrectAnswer;
                                assessmentOptiones.QuestionID = questionAssessments.Id;
                                assessmentOptiones.ContentPath = opt.OptionContentPath;
                                assessmentOptiones.ContentType = opt.OptionContentType;
                                assessmentOptiones.CreatedBy = UserId;
                                assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                assessmentOptiones.ModifiedBy = UserId;
                                if (!string.IsNullOrEmpty(opt.OptionContentType))
                                {
                                    assessmentOptiones.OptionText = "option";
                                }
                                if (!string.IsNullOrEmpty(opt.OptionText))
                                {
                                    AssessmentQues.Add(assessmentOptiones);
                                }
                            }
                            await _asessmentQuestionOption.AddRange(AssessmentQues);
                        }
                    }
                    else if (aPIAssessmentQuestion.Section.Equals(FileType.Subjective))
                    {
                        await this.Add(questionAssessments);
                    }
                }
                return errorAssessment;

            }

            return null;


        }
        public async Task<Message> UpdateAssessmentQuestion(int id, APIAssessmentQuestion apiAssessmentQuestion, int UserId)
        {

            AssessmentQuestion Question = await this.Get(id);
            if (Question == null)
                return Message.NotFound;
            Question = Mapper.Map<AssessmentQuestion>(apiAssessmentQuestion);
            Question.CreatedBy = UserId;
            Question.CreatedDate = DateTime.UtcNow;
            Question.ModifiedBy = UserId;
            Question.ModifiedDate = DateTime.UtcNow;

            await this.Update(Question);

            AssessmentCompetenciesMapping competenciesAssMapping = new AssessmentCompetenciesMapping();
            competenciesAssMapping = _db.AssessmentCompetenciesMapping.Where(a => a.AssessmentQuestionId == Question.Id).FirstOrDefault();

            if (competenciesAssMapping != null)
            {
                competenciesAssMapping.IsDeleted = true;
                await this._competenciesAssessmentRepository.UpdateRecord(competenciesAssMapping);
            }

            foreach (var aPIassCompetency in apiAssessmentQuestion.CompetencySkillsData)
            {
                int competenciesMasterid = 0;
                if (aPIassCompetency.Id != 0)
                {
                    competenciesMasterid = Convert.ToInt32(aPIassCompetency.Id);

                }
                if (competenciesMasterid == 0)
                {
                    CompetenciesMaster competenciesMaster = new CompetenciesMaster();
                    competenciesMaster.CompetencyName = aPIassCompetency.Name;
                    competenciesMaster.CompetencyDescription = aPIassCompetency.Name;
                    competenciesMaster.CategoryId = 0;
                    competenciesMaster.CreatedBy = UserId;
                    competenciesMaster.CreatedDate = DateTime.Now;
                    competenciesMaster.IsActive = true;
                    competenciesMaster.IsDeleted = false;
                    competenciesMaster.ModifiedBy = UserId;
                    competenciesMaster.ModifiedDate = DateTime.Now;
                    await this._competenciesMasterRepository.Add(competenciesMaster);
                    competenciesMasterid = competenciesMaster.Id;
                }

                if (competenciesAssMapping != null)
                {
                    competenciesAssMapping.CompetencyId = competenciesMasterid;
                    competenciesAssMapping.IsDeleted = false;
                    await this._competenciesAssessmentRepository.UpdateRecord(competenciesAssMapping);
                }
                else
                {
                    AssessmentCompetenciesMapping competenciesMapping = new AssessmentCompetenciesMapping();
                    competenciesMapping.AssessmentQuestionId = Question.Id;
                    competenciesMapping.CompetencyId = competenciesMasterid;
                    competenciesMapping.IsDeleted = false;
                    competenciesMapping.ModifiedBy = UserId;
                    competenciesMapping.ModifiedDate = DateTime.UtcNow;
                    competenciesMapping.CreatedBy = UserId;
                    competenciesMapping.CreatedDate = DateTime.UtcNow;
                    await this._competenciesAssessmentRepository.AddRecord(competenciesMapping);
                }


            }



            if (apiAssessmentQuestion.Section.Equals(FileType.Objective))
            {
                if (apiAssessmentQuestion.aPIassessmentOptions.Length < 2)
                {
                    return Message.InvalidModel;
                }
            }

            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            dbContext.Database.ExecuteSqlCommand("Update Course.AssessmentQuestionOption set IsDeleted = 1 where QuestionID = " + Question.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            foreach (AssessmentOptions opt1 in apiAssessmentQuestion.aPIassessmentOptions)
            {

                AssessmentQuestionOption Option1 = await this._db.AssessmentQuestionOption.Where(q => q.Id == opt1.AssessmentOptionID).FirstOrDefaultAsync();

                if (Option1 != null)
                {

                    Option1.OptionText = opt1.OptionText;
                    Option1.IsCorrectAnswer = opt1.IsCorrectAnswer;
                    Option1.QuestionID = Question.Id;
                    Option1.ContentPath = opt1.OptionContentPath;
                    Option1.ContentType = opt1.OptionContentType;
                    Option1.ModifiedBy = UserId;
                    Option1.ModifiedDate = DateTime.UtcNow;
                    Option1.CreatedBy = UserId;
                    Option1.IsDeleted = false;
                    Option1.CreatedDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(opt1.OptionContentType))
                    {
                        Option1.OptionText = "option";
                    }
                    await _asessmentQuestionOption.Update(Option1);
                }
                else
                {
                    AssessmentQuestionOption OptionNew = new AssessmentQuestionOption();
                    OptionNew.OptionText = opt1.OptionText;
                    OptionNew.IsCorrectAnswer = opt1.IsCorrectAnswer;
                    OptionNew.QuestionID = Question.Id;
                    OptionNew.ContentPath = opt1.OptionContentPath;
                    OptionNew.ContentType = opt1.OptionContentType;
                    OptionNew.ModifiedBy = UserId;
                    OptionNew.ModifiedDate = DateTime.UtcNow;
                    OptionNew.CreatedBy = UserId;
                    OptionNew.IsDeleted = false;
                    OptionNew.CreatedDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(opt1.OptionContentType))
                    {
                        OptionNew.OptionText = "option";
                    }
                    await _asessmentQuestionOption.Add(OptionNew);
                }
            }

            return Message.Ok;
        }
        public async Task<Message> DeleteQuestion(int questionId)
        {
            AssessmentQuestion Question = await this.Get(questionId);
            if (Question == null)
                return Message.NotFound;
            if (await IsQuestionAssigned(questionId))
                return Message.DependencyExist;

            Question.IsDeleted = Record.Deleted;
            await this.Update(Question);

            return Message.Success;
        }
        public async Task<IEnumerable<APIAdaptiveAssessmentQuestion>> GetAdaptiveAssessment(int courseId)
        {
            var CourseModules = await (from CourseModuleAssociation in this._db.CourseModuleAssociation
                                       join Assessment in this._db.Module on CourseModuleAssociation.AssessmentId equals Assessment.Id
                                       into tempAssessment
                                       from Assessment in tempAssessment.DefaultIfEmpty()
                                       join Lcms in this._db.LCMS on Assessment.LCMSId equals Lcms.Id
                                       into tempLcms
                                       from Lcms in tempLcms.DefaultIfEmpty()
                                       where CourseModuleAssociation.CourseId == courseId
                                       select new
                                       {
                                           ConfigurationId = Lcms.AssessmentSheetConfigID,
                                           ModuleId = CourseModuleAssociation.ModuleId,
                                           CourseId = CourseModuleAssociation.CourseId
                                       }).ToListAsync();
            CourseModules = CourseModules.Where(cid => cid.ConfigurationId != null).ToList();
            List<APIAdaptiveAssessmentQuestion> AdaptiveQuestions = new List<APIAdaptiveAssessmentQuestion>();

            foreach (var CourseModule in CourseModules)
            {
                //Get all questions and answers from database.
                var QuestionNAnswers = await (from AssesmentConfiguration in _db.AssessmentSheetConfiguration
                                              join AssesmentConfigurationDetails in this._db.AssessmentSheetConfigurationDetails on AssesmentConfiguration.ID equals AssesmentConfigurationDetails.AssessmentSheetConfigID
                                              join Questions in _db.AssessmentQuestion on AssesmentConfigurationDetails.QuestionID equals Questions.Id
                                              join Options in _db.AssessmentQuestionOption on Questions.Id equals Options.QuestionID
                                              where AssesmentConfiguration.ID == CourseModule.ConfigurationId
                                              && Questions.IsDeleted == false
                                              && Options.IsDeleted == false
                                              && AssesmentConfigurationDetails.IsDeleted == false
                                              && AssesmentConfiguration.IsDeleted == false
                                              select new
                                              {
                                                  Questions.ContentPath,
                                                  Questions.ContentType,
                                                  Questions.LearnerInstruction,
                                                  Questions.DifficultyLevel,
                                                  Questions.Marks,
                                                  Questions.ModelAnswer,
                                                  Questions.QuestionText,
                                                  Questions.AnswerAsImages,
                                                  Questions.QuestionType,
                                                  Questions.QuestionStyle,
                                                  Questions.Section,
                                                  Questions.Metadata,
                                                  Questions.Status,
                                                  Questions.OptionType,
                                                  QuestionId = Questions.Id,
                                                  OptionsId = Options.Id,
                                                  Options.IsCorrectAnswer,
                                                  Options.OptionText,
                                                  OptionContentPath = Options.ContentType,
                                                  OptionContentType = Options.ContentType,
                                                  QId = Options.QuestionID,
                                                  AssesmentConfiguration.IsFixed,
                                                  AssesmentConfiguration.NoOfQuestionsToShow
                                              }).ToListAsync();

                bool? IsFixed = QuestionNAnswers.Select(q => q.IsFixed).FirstOrDefault();
                int? NoOfQuestionsToShow = QuestionNAnswers.Select(q => q.NoOfQuestionsToShow).FirstOrDefault();

                //Assign questions to api 
                List<APIAdaptiveAssessmentQuestion> ModuleAssessmentQuestions = (from Questions in QuestionNAnswers
                                                                                 group Questions by Questions.QuestionId into temp
                                                                                 let Questions = temp.FirstOrDefault()
                                                                                 select new APIAdaptiveAssessmentQuestion
                                                                                 {
                                                                                     LearnerInstruction = Questions.LearnerInstruction,
                                                                                     DifficultyLevel = Questions.DifficultyLevel,
                                                                                     Marks = Questions.Marks,
                                                                                     ModelAnswer = Questions.ModelAnswer,
                                                                                     QuestionText = Questions.QuestionText,
                                                                                     AnswerAsImages = Questions.AnswerAsImages,
                                                                                     QuestionType = Questions.QuestionType,
                                                                                     QuestionStyle = Questions.QuestionStyle,
                                                                                     Section = Questions.Section,
                                                                                     Metadata = Questions.Metadata,
                                                                                     Status = Questions.Status,
                                                                                     OptionType = Questions.OptionType,
                                                                                     Id = Questions.QuestionId,
                                                                                     CourseId = CourseModule.CourseId,
                                                                                     ModuleId = CourseModule.ModuleId,
                                                                                     ContentPath = Questions.ContentPath,
                                                                                     ContentType = Questions.ContentType
                                                                                 }).ToList();
                //Assign options for each question 
                foreach (APIAdaptiveAssessmentQuestion Question in ModuleAssessmentQuestions)
                {
                    Question.aPIassessmentOptions = (from Answers in QuestionNAnswers
                                                     where Answers.QId == Question.Id
                                                     select new AssessmentOptions
                                                     {
                                                         AssessmentOptionID = Answers.OptionsId,
                                                         OptionText = Answers.OptionText,
                                                         OptionContentPath = Answers.ContentPath,
                                                         OptionContentType = Answers.OptionContentType
                                                     }).ToArray();
                }
                ModuleAssessmentQuestions.Shuffle();

                if (IsFixed != null)
                    if (!IsFixed.Value)
                        ModuleAssessmentQuestions = ModuleAssessmentQuestions.Take(NoOfQuestionsToShow.Value).ToList();

                AdaptiveQuestions.AddRange(ModuleAssessmentQuestions);
            }
            return AdaptiveQuestions;
        }
        public async Task<int> GetMultipleAnwersMarks(int? QuestionID, List<int?> selectedOptionId)
        {
            List<int> selectedOption = selectedOptionId.Where(q => q != null).Select(q => (int)q).ToList();
            var Result = await (from AssessmentQuestion in this._db.AssessmentQuestion
                                join AssessmentOption in _db.AssessmentQuestionOption on AssessmentQuestion.Id equals AssessmentOption.QuestionID
                                where AssessmentOption.QuestionID == QuestionID && AssessmentOption.IsCorrectAnswer == true
                                select
                                new
                                {
                                    OptionsId = AssessmentOption.Id,
                                    Marks = AssessmentQuestion.Marks
                                }).ToListAsync();

            List<int> CorrectOptionsList = Result.Select(o => o.OptionsId).ToList();
            int Marks = Result.Select(o => o.Marks).FirstOrDefault();

            //Hash set used for checking multiple questions match with selected quetions
            var CorrectOptions = new HashSet<int>(CorrectOptionsList);
            bool IsCorrect = CorrectOptions.SetEquals(selectedOption);

            if (IsCorrect)
                return Marks;

            return 0;
        }

        public async Task<APIAssessmentMaster> GetAdaptiveAssessmentHeader(int courseId, int userId)
        {
            try
            {
                int?[] ConfigurationId = await (from CourseModuleAssociation in this._db.CourseModuleAssociation
                                                join Assessment in this._db.Module on CourseModuleAssociation.AssessmentId equals Assessment.Id
                                                into tempAssessment
                                                from Assessment in tempAssessment.DefaultIfEmpty()
                                                join Lcms in this._db.LCMS on Assessment.LCMSId equals Lcms.Id
                                                into tempLcms
                                                from Lcms in tempLcms.DefaultIfEmpty()
                                                where CourseModuleAssociation.CourseId == courseId
                                                select Lcms.AssessmentSheetConfigID
                                           ).ToArrayAsync();
                ConfigurationId = ConfigurationId.Where(cid => cid != null).ToArray();
                if (ConfigurationId.Length == 0)
                    return null;
                var Result = await (from AssessmentConfig in this._db.AssessmentSheetConfiguration
                                    join ConfigurationDetail in this._db.AssessmentSheetConfigurationDetails on AssessmentConfig.ID equals ConfigurationDetail.AssessmentSheetConfigID
                                    join Questions in this._db.AssessmentQuestion on ConfigurationDetail.QuestionID equals Questions.Id
                                    join Course in this._db.Course on courseId equals Course.Id
                                    where ConfigurationId.Contains(AssessmentConfig.ID) && Questions.IsDeleted == false
                                    select new
                                    {
                                        AssessmentConfig.Durations,
                                        AssessmentConfig.PassingPercentage,
                                        TotalAttempt = AssessmentConfig.MaximumNoOfAttempts,
                                        CourseTitle = Course.Title,
                                        QMarks = Questions.Marks,
                                        AssessmentConfig.NoOfQuestionsToShow,
                                        AssessmentConfig.IsFixed,
                                        AssessmentConfig.ID,
                                        Course.ThumbnailPath
                                    }).ToListAsync();



                APIAssessmentMaster Header = new APIAssessmentMaster();
                foreach (int configureId in ConfigurationId)
                {
                    var AssessmentHeader = Result.Where(AssessmentConfig => AssessmentConfig.ID == configureId).FirstOrDefault();
                    Header.CourseTitle = AssessmentHeader.CourseTitle;
                    Header.DurationInMins += AssessmentHeader.Durations;
                    Header.PassingPercentage += (float)AssessmentHeader.PassingPercentage;
                    Header.NoOfAttempts = 1;
                    Header.TotalMarks += Result.Where(s => s.ID == configureId).Sum(q => q.QMarks);
                    bool? IsFixed = Result.Where(s => s.ID == configureId).Select(a => a.IsFixed).FirstOrDefault();
                    int? NoOfQuestionsToShow = Result.Where(s => s.ID == configureId).Select(a => a.NoOfQuestionsToShow).FirstOrDefault();

                    Header.thumbnailPath = AssessmentHeader.ThumbnailPath;
                    Header.TotalNumberOfAttempts = AssessmentHeader.TotalAttempt;

                    if (IsFixed != null)
                    {
                        if (!IsFixed.Value)
                            Header.NoofQuestion += NoOfQuestionsToShow.Value;
                        else
                            Header.NoofQuestion += Result.Where(s => s.ID == configureId).Count();
                    }
                    else
                        Header.NoofQuestion += Result.Where(s => s.ID == configureId).Count();
                }

                Header.PassingPercentage = Header.PassingPercentage / ConfigurationId.Count();

                var PostAssessmentResult = await _db.PostAssessmentResult
                    .Where(c => c.CreatedBy == userId && c.CourseID == courseId
                    && c.IsAdaptiveAssessment == true)
                    .OrderByDescending(c => c.Id).ToListAsync();


                if (PostAssessmentResult != null && PostAssessmentResult.Count > 0)
                {
                    double? TotalMarks = (from marks in PostAssessmentResult
                                          select marks.MarksObtained
                                  ).FirstOrDefault();


                    string PostAssessmentStatus = (from PostAssessment in PostAssessmentResult
                                                   select PostAssessment.PostAssessmentStatus
                                  ).FirstOrDefault();


                    Header.AssessmentStatus = PostAssessmentStatus;

                    Header.ObtainedMarks = TotalMarks;
                    int UserAttempts = (from UserAttempt in PostAssessmentResult
                                        select UserAttempt.NoOfAttempts).FirstOrDefault();
                    Header.PassingMarks = (int)(Header.TotalMarks * Header.Percentage) / 100;

                    Header.NoOfAttempts = 0;

                    Header.Status = (from status in PostAssessmentResult
                                     select status.AssessmentResult).FirstOrDefault();

                    Header.Result = (from status in PostAssessmentResult
                                     select status.AssessmentResult).FirstOrDefault();

                    double val = System.Math.Round(((float)Header.ObtainedMarks / Header.TotalMarks * 100), 2);
                    Header.Percentage = (float)val;
                }

                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                if (Header.Status != null)
                    Header.Status = textInfo.ToTitleCase(Header.Status);

                if (Header.Result != null)
                    Header.Result = textInfo.ToTitleCase(Header.Result);

                return Header;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<ApiResponse> MultiDeleteAssessmentQuestion(APIDeleteAssessmentQuestion[] apideletemultipleques)
        {
            int totalRecordForDelete = apideletemultipleques.Count();
            int totalRecordRejected = 0;
            List<APIDeleteAssessmentQuestion> AssessmentQueStatusList = new List<APIDeleteAssessmentQuestion>();
            foreach (APIDeleteAssessmentQuestion deletemultipleque in apideletemultipleques)
            {
                APIDeleteAssessmentQuestion assessmentqueStatus = new APIDeleteAssessmentQuestion();

                AssessmentQuestion Question = await this.Get(deletemultipleque.Id);

                if (await IsQuestionAssigned(deletemultipleque.Id))
                {
                    assessmentqueStatus.ErrMessage = Message.DependencyExist.ToString();
                    assessmentqueStatus.QuestionText = Question.QuestionText.ToString();
                    assessmentqueStatus.Id = Question.Id;
                    totalRecordRejected++;
                    AssessmentQueStatusList.Add(assessmentqueStatus);
                }
                else
                {
                    Question.IsDeleted = Record.Deleted;
                    await this.Update(Question);
                }
            }

            string resultstring = (totalRecordForDelete - totalRecordRejected) + "record deleted out of " + totalRecordForDelete;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, AssessmentQueStatusList };
            return response;

        }

        public async Task<bool> IsContentCompleted(int userId, int courseId, int? moduleId, bool isPreassessment)
        {
            if (moduleId == 0 && courseId > 0 && isPreassessment == false)
            {
                APIMyCoursesModule CourseInfo = await this._myCoursesRepository.GetModule(userId, courseId);

                if (CourseInfo.ContentStatus.ToLower() != "completed")
                    return false;
            }

            if (moduleId != 0 && courseId != 0 && isPreassessment == false)
            {
                ApiCourseInfo CourseInfo = await this._myCoursesRepository.GetModuleInfo(userId, courseId, moduleId);

                if (CourseInfo.Modules.ContentStatus.ToLower() != "completed")
                    return false;

            }
            return true;
        }
        public async Task<List<AssessmentQuestionRejected>> GetAllAssessmentQuestionRejected()
        {
            try
            {
                using (var context = this._db)
                {
                    var result = (from assessmentquestionrejected in context.AssessmentQuestionRejected

                                  select new AssessmentQuestionRejected
                                  {
                                      Section = assessmentquestionrejected.Section,
                                      LearnerInstruction = assessmentquestionrejected.LearnerInstruction,
                                      QuestionText = assessmentquestionrejected.QuestionText,
                                      ErrorMessage = assessmentquestionrejected.ErrorMessage,
                                      DifficultyLevel = assessmentquestionrejected.DifficultyLevel,
                                      Time = assessmentquestionrejected.Time,
                                      ModelAnswer = assessmentquestionrejected.ModelAnswer,
                                      MediaFile = assessmentquestionrejected.MediaFile,
                                      AnswerAsImages = assessmentquestionrejected.AnswerAsImages,
                                      Marks = assessmentquestionrejected.Marks,
                                      Status = assessmentquestionrejected.Status,
                                      QuestionStyle = assessmentquestionrejected.QuestionStyle,
                                      QuestionType = assessmentquestionrejected.QuestionType,
                                      AnswerOptions = assessmentquestionrejected.AnswerOptions,
                                      AnswerOption1 = assessmentquestionrejected.AnswerOption1,
                                      AnswerOption2 = assessmentquestionrejected.AnswerOption2,
                                      AnswerOption3 = assessmentquestionrejected.AnswerOption3,
                                      AnswerOption4 = assessmentquestionrejected.AnswerOption4,
                                      AnswerOption5 = assessmentquestionrejected.AnswerOption5,
                                      CorrectAnswer1 = assessmentquestionrejected.CorrectAnswer1,
                                      CorrectAnswer2 = assessmentquestionrejected.CorrectAnswer2,
                                      CorrectAnswer3 = assessmentquestionrejected.CorrectAnswer3,
                                      CorrectAnswer4 = assessmentquestionrejected.CorrectAnswer4,
                                      CorrectAnswer5 = assessmentquestionrejected.CorrectAnswer5
                                  });
                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;

        }

        public async Task<IEnumerable<APISubjectiveAQReview>> GetReviewQuestion(int ConfigureId)
        {

            IQueryable<APISubjectiveAQReview> Query = (from par in _db.PostAssessmentResult
                                                       join aqd in _db.AssessmentQuestionDetails on par.Id equals aqd.AssessmentResultID
                                                       join aqu in _db.AssessmentQuestion on aqd.ReferenceQuestionID equals aqu.Id
                                                       where par.Id == ConfigureId && aqu.OptionType == "subjective"
                                                       select new APISubjectiveAQReview
                                                       {
                                                           QuestionText = aqu.QuestionText,
                                                           SelectedAnswer = aqd.SelectedAnswer,
                                                           Marks = aqu.Marks,
                                                           Id = aqu.Id,
                                                           AssessmentQuestionDetailsId = aqd.Id,
                                                           MarksObtained = Convert.ToInt32(par.MarksObtained),
                                                           TotalMarks = par.TotalMarks,
                                                           PassingPercentage = par.PassingPercentage

                                                       });

            return Query;
        }

        public async Task<IEnumerable<APIAssessmentDataForReview>> GetAssessmentForReview(int page, int pageSize, string? search = null, string? columnName = null)
        {
            List<APIAssessmentDataForReview> qm = new List<APIAssessmentDataForReview>();

            try
            {


                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();

                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();

                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Page", page);
                    parameters.Add("@PageSize", pageSize);
                    parameters.Add("@Search", search);
                    parameters.Add("@ColumnName", columnName);

                    var Result = await SqlMapper.QueryAsync<APIAssessmentDataForReview>((SqlConnection)connection, "[dbo].[GetAssessmentForReview]", parameters, null, null, CommandType.StoredProcedure);
                    qm = Result.ToList();
                    connection.Close();
                }

                return qm;
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
            }

            return qm;
        }

        public async Task<int> ReviewCountCount(string? search = null, string? columnName = null)
        {

            int count = 0;
            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@Page", 1);
                        parameters.Add("@PageSize", 0);
                        parameters.Add("@Search", search);
                        parameters.Add("@ColumnName", columnName);


                        var Result = await SqlMapper.QueryAsync<APIAssessmentDataForReview>((SqlConnection)connection, "[dbo].[GetAssessmentForReview]", parameters, null, null, CommandType.StoredProcedure);
                        count = Result.Select(x => x.TotalRecordCount).FirstOrDefault();

                        connection.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return count;

        }

        public async Task<APITotalAssessmentQuestion> GetAllQuestionPaginationV2(int page, int pageSize, int userId, string userRole, bool showAllData = false, string? search = null, string? columnName = null, bool? isMemoQuestions = null)
        {

            APITotalAssessmentQuestion aPITotalAssessmentQuestion = new APITotalAssessmentQuestion();
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();

            IQueryable<APIGetAssessmentQuestion> Query = (from ques in this._db.AssessmentQuestion
                                                          join user in _db.UserMaster on ques.ModifiedBy equals user.Id into um
                                                          from user in um.DefaultIfEmpty()
                                                          join umd in _db.UserMasterDetails on ques.CreatedBy equals umd.UserMasterId into umddetails
                                                          from umd in umddetails.DefaultIfEmpty()
                                                          join course in _db.Course on ques.CourseId equals course.Id into courses
                                                          from course in courses.DefaultIfEmpty()

                                                          orderby ques.Id descending
                                                          where ques.IsDeleted == false
                                                          select new APIGetAssessmentQuestion
                                                          {
                                                              Id = ques.Id,
                                                              OptionType = ques.OptionType,
                                                              Marks = ques.Marks,
                                                              OptionsCount = _db.AssessmentQuestionOption.Where(x => x.QuestionID == ques.Id && x.IsDeleted == false).Count(),
                                                              DifficultyLevel = ques.DifficultyLevel,
                                                              Status = ques.Status,
                                                              QuestionId = ques.Id,
                                                              Question = ques.QuestionText,
                                                              Section = ques.Section,
                                                              Metadata = ques.Metadata,
                                                              IsDeleted = ques.IsDeleted,
                                                              IsMemoQuestion = ques.IsMemoQuestion,
                                                              UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                                                              AreaId = umd.AreaId,
                                                              LocationId = umd.LocationId,
                                                              GroupId = umd.GroupId,
                                                              BusinessId = umd.BusinessId,
                                                              CreatedBy = ques.CreatedBy,
                                                              CourseTitle = course.Title,
                                                              UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (ques.CreatedBy == userId) ? true : false : true
                                                          }).AsNoTracking();


            IQueryable<APIGetAssessmentQuestion> authorQuery = (from course in _db.Course
                                                                join author in _db.CourseAuthorAssociation.Where(a => a.UserId == userId) on course.Id equals author.CourseId
                                                                join cma in _db.CourseModuleAssociation on author.CourseId equals cma.CourseId
                                                                from module in _db.Module
                                                                where (module.Id == cma.FeedbackId || module.Id == course.FeedbackId)
                                                                join lcms in _db.LCMS on module.LCMSId equals lcms.Id
                                                                join asc in _db.AssessmentSheetConfiguration on lcms.AssessmentSheetConfigID equals asc.ID
                                                                join ascd in _db.AssessmentSheetConfigurationDetails on asc.ID equals ascd.AssessmentSheetConfigID
                                                                join ques in _db.AssessmentQuestion on ascd.QuestionID equals ques.Id
                                                                join user in _db.UserMaster on ques.ModifiedBy equals user.Id into um
                                                                from user in um.DefaultIfEmpty()
                                                                join umd in _db.UserMasterDetails on ques.CreatedBy equals umd.UserMasterId into umddetails
                                                                from umd in umddetails.DefaultIfEmpty()

                                                                orderby ques.Id descending
                                                                where ques.IsDeleted == false
                                                                select new APIGetAssessmentQuestion
                                                                {
                                                                    Id = ques.Id,
                                                                    OptionType = ques.OptionType,
                                                                    Marks = ques.Marks,
                                                                    OptionsCount = _db.AssessmentQuestionOption.Where(x => x.QuestionID == ques.Id && x.IsDeleted == false).Count(),
                                                                    DifficultyLevel = ques.DifficultyLevel,
                                                                    Status = ques.Status,
                                                                    QuestionId = ques.Id,
                                                                    Question = ques.QuestionText,
                                                                    Section = ques.Section,
                                                                    Metadata = ques.Metadata,
                                                                    IsDeleted = ques.IsDeleted,
                                                                    IsMemoQuestion = ques.IsMemoQuestion,
                                                                    UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                                                                    AreaId = umd.AreaId,
                                                                    LocationId = umd.LocationId,
                                                                    GroupId = umd.GroupId,
                                                                    BusinessId = umd.BusinessId,
                                                                    CreatedBy = ques.CreatedBy,
                                                                    CourseTitle = course.Title,
                                                                    UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (ques.CreatedBy == userId) ? true : false : true
                                                                }).AsNoTracking();


            if (columnName == "null")
                columnName = null;
            if (search == "null")
                search = null;

            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(columnName))
                {
                    if (columnName.ToLower().Equals("metadata"))
                        authorQuery = authorQuery.Where(r => r.Metadata.Contains(search));
                    if (columnName.ToLower().Equals("question"))
                        authorQuery = authorQuery.Where(r => r.Question.Contains(search));
                    if (columnName.ToLower().Equals("coursename"))
                        authorQuery = authorQuery.Where(r => r.CourseTitle.Contains(search));
                }
                else
                {
                    authorQuery = authorQuery.Where(r => r.Metadata.Contains(search) || r.Question.Contains(search));
                }
            }

            if (isMemoQuestions != null)
                authorQuery = authorQuery.Where(q => q.IsMemoQuestion == isMemoQuestions);



            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(columnName))
                {
                    if (columnName.ToLower().Equals("metadata"))
                        Query = Query.Where(r => r.Metadata.Contains(search));
                    if (columnName.ToLower().Equals("question"))
                        Query = Query.Where(r => r.Question.Contains(search));
                    if (columnName.ToLower().Equals("coursename"))
                        Query = Query.Where(r => r.CourseTitle.Contains(search));
                }
                else
                {
                    Query = Query.Where(r => r.Metadata.Contains(search) || r.Question.Contains(search));
                }
            }

            if (isMemoQuestions != null)
                Query = Query.Where(q => q.IsMemoQuestion == isMemoQuestions);

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
            if (showAllData == false && (userRole != UserRoles.CA))
            {
                Query = Query.Where(r => r.CreatedBy == userId);
            }

            var queryResult = Query.Union(authorQuery);
            aPITotalAssessmentQuestion.TotalRecords = await queryResult.Distinct().CountAsync();


            queryResult = queryResult.OrderByDescending(v => v.Id);
            if (page != -1)
            {
                queryResult = queryResult.Skip((page - 1) * pageSize);
            }
            if (pageSize != -1)
            {
                queryResult = queryResult.Take(pageSize);
            }

            List<APIGetAssessmentQuestion> assessmentQue = await queryResult.ToListAsync();

            aPITotalAssessmentQuestion.data = assessmentQue;
            return aPITotalAssessmentQuestion;
        }
        public async Task<APIFQCourse> courseCodeExists(string coursecode)
        {

            var qcourse = (from course in _db.Course
                           where course.Code == coursecode && course.IsDeleted == Record.NotDeleted
                           select new APIFQCourse
                           {
                               CourseId = course.Id,
                               Tilte = course.Title
                           }).AsNoTracking();
            APIFQCourse cdata = await qcourse.FirstOrDefaultAsync();
            return cdata;
        }
        public int SaveAssessmentPhotos(APIassessmentPhotos aPIassessmentPhotos, int Userid)
        {
            try
            {
                if (aPIassessmentPhotos != null)
                {
                    CameraPhotos assessmentPhotos = new CameraPhotos();

                    assessmentPhotos.PostAssessmentId = aPIassessmentPhotos.PostAssessmentId;
                    assessmentPhotos.PhotoPath = aPIassessmentPhotos.ImageData;
                    assessmentPhotos.CreatedDate = DateTime.Now;
                    assessmentPhotos.ModifiedDate = DateTime.Now;
                    assessmentPhotos.CreatedBy = Userid;
                    assessmentPhotos.ModifiedBy = Userid;
                    assessmentPhotos.CourseId = aPIassessmentPhotos.CourseId;
                    assessmentPhotos.ModuleId = aPIassessmentPhotos.ModuleId;

                    this._db.CameraPhotos.Add(assessmentPhotos);
                    this._db.SaveChanges();

                    CameraEvaluation cameraEvaluation1 = this._db.CameraEvaluation.Where(
                        a => a.CourseId == aPIassessmentPhotos.CourseId &&
                        a.ModuleId == aPIassessmentPhotos.ModuleId &&
                        a.UserId == Userid &&
                        a.PostAssessmentId == aPIassessmentPhotos.PostAssessmentId
                        ).FirstOrDefault();
                    if (cameraEvaluation1 == null)
                    {
                        CameraEvaluation cameraEvaluation = new CameraEvaluation();
                        cameraEvaluation.CourseId = aPIassessmentPhotos.CourseId;
                        cameraEvaluation.ModuleId = aPIassessmentPhotos.ModuleId;
                        cameraEvaluation.UserId = Userid;
                        cameraEvaluation.CreatedBy = 0;
                        cameraEvaluation.ModifiedBy = 0;
                        cameraEvaluation.CreatedDate = DateTime.Now;
                        cameraEvaluation.ModifiedDate = DateTime.Now;
                        cameraEvaluation.IsDeleted = false;
                        cameraEvaluation.PostAssessmentId = aPIassessmentPhotos.PostAssessmentId;
                        cameraEvaluation.Status = "Not Submitted";

                        this._db.CameraEvaluation.Add(cameraEvaluation);
                        this._db.SaveChanges();
                    }

                    return 0;
                }
                return -1;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return -2;
            }
        }
        public async Task<CameraPhotosResponse> GetCameraPhotos(GetAPIassessmentPhotos getAPIassessmentPhotos, int Userid)
        {

            CameraPhotosResponse cameraPhotosResponse = new CameraPhotosResponse();

            IQueryable<photodate> Query = (from camera in this._db.CameraPhotos
                                           where camera.CourseId == getAPIassessmentPhotos.CourseId
                                           && camera.ModuleId == getAPIassessmentPhotos.ModuleId
                                           && camera.CreatedBy == Userid
                                           select new photodate
                                           {
                                               Photos = camera.PhotoPath,
                                               date = camera.CreatedDate
                                           }
                                           ).AsNoTracking();

            cameraPhotosResponse.photodates = Query.ToList();


            cameraPhotosResponse.CourseName = await this._db.Course.Where(a => a.Id == getAPIassessmentPhotos.CourseId).
                Select(a => a.Title).FirstOrDefaultAsync();

            cameraPhotosResponse.ModuleName = await this._db.Module.Where(a => a.Id == getAPIassessmentPhotos.ModuleId).
                Select(a => a.Name).FirstOrDefaultAsync();

            return cameraPhotosResponse;
        }
        public async Task<APITotalCourseForEvaluation> GetCoursesForCameraEvaluation(int page, int pageSize, int userId)
        {
            if (userId == 0)
            {
                return null;
            }

            APITotalCourseForEvaluation aPITotalRequest = new APITotalCourseForEvaluation();

            IQueryable<CameraPhotosForCourseEvaluation> Query = (from cameraEvaluation in this._db.CameraEvaluation
                                                                 join course in this._db.Course on cameraEvaluation.CourseId equals course.Id
                                                                 join module in this._db.Module on cameraEvaluation.ModuleId equals module.Id into r
                                                                 from moduleinfo in r.DefaultIfEmpty()
                                                                 where cameraEvaluation.UserId == userId
                                                                 orderby cameraEvaluation.Id descending
                                                                 select new CameraPhotosForCourseEvaluation
                                                                 {
                                                                     CourseId = course.Id,
                                                                     CourseName = course.Title,
                                                                     ModuleId = moduleinfo.Id,
                                                                     ModuleName = moduleinfo.Name,
                                                                     PostAssessmentId = cameraEvaluation.PostAssessmentId,
                                                                     Status = cameraEvaluation.Status
                                                                 }

                ).AsNoTracking();

            aPITotalRequest.TotalRecords = await Query.CountAsync();

            if (page != -1)
            {
                Query = Query.Skip((page - 1) * pageSize);
            }
            if (pageSize != -1)
            {
                Query = Query.Take(pageSize);
            }

            List<CameraPhotosForCourseEvaluation> cameraPhotosForCourseEvaluations = await Query.ToListAsync();

            aPITotalRequest.data = cameraPhotosForCourseEvaluations;
            return aPITotalRequest;


        }
        public async Task<int> SaveStatusForCourseEvaluation(CameraPhotosStatusForCourseEvaluation cameraPhotosStatusForCourseEvaluation, int UserId, int ManagerId)
        {
            CameraEvaluation cameraEvaluation = await this._db.CameraEvaluation.Where(a =>
                a.CourseId == cameraPhotosStatusForCourseEvaluation.CourseId &&
                a.ModuleId == cameraPhotosStatusForCourseEvaluation.ModuleId &&
                a.UserId == UserId &&
                a.PostAssessmentId == cameraPhotosStatusForCourseEvaluation.PostAssessmentId &&
                a.Status == "Not Submitted"
            ).FirstOrDefaultAsync();
            if (cameraEvaluation != null)
            {
                cameraEvaluation.CreatedBy = ManagerId;
                cameraEvaluation.ModifiedBy = ManagerId;
                cameraEvaluation.ModifiedDate = DateTime.Now;
                cameraEvaluation.Status = cameraPhotosStatusForCourseEvaluation.Status;

                this._db.CameraEvaluation.Update(cameraEvaluation);
                this._db.SaveChanges();

                return 0;
            }
            return -1;
        }

        public async Task<bool> AssessmentCompletionStatus(int courseId, int moduleId, int userId, bool isPreAssessment = false, bool isContentAssessment = false, bool isAdaptiveLearning = false)
        {
            try
            {

                string Result = await (from c in _db.PostAssessmentResult
                                       where c.CreatedBy == userId && c.CourseID == courseId && c.PostAssessmentStatus == Status.Completed
                                       && c.IsPreAssessment == isPreAssessment && c.IsContentAssessment == isContentAssessment && c.IsAdaptiveAssessment == isAdaptiveLearning
                                       && c.ModuleId == moduleId && c.PostAssessmentStatus == "completed"
                                       select c.PostAssessmentStatus
                            ).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(Result))
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }

    }
}
