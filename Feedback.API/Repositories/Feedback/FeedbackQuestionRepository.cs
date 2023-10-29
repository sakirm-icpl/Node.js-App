using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using log4net;
using Feedback.API.Models;
using Feedback.API.APIModel;
using Feedback.API.Helper;
using Feedback.API.Common;
using Feedback.API.APIModel.Feedback;

namespace Feedback.API.Repositories
{
    public class FeedbackQuestionRepository : Repository<FeedbackQuestion>, IFeedbackQuestion
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FeedbackQuestionRepository));
        private FeedbackContext _db;
        IFeedbackOption _feedbackOption;
        private IConfiguration _configuration;
        IFeedbackSheetConfigurationDetails _feedbackSheetConfigurationDetails;
        public FeedbackQuestionRepository(FeedbackContext context, IFeedbackSheetConfigurationDetails feedbackSheetConfigurationDetails, IFeedbackOption feedbackOption, IConfiguration configuration) : base(context)
        {
            _db = context;
            _feedbackSheetConfigurationDetails = feedbackSheetConfigurationDetails;
            _feedbackOption = feedbackOption;
            this._configuration = configuration;
        }
        public bool Exists(string search)
        {
            if (_db.FeedbackQuestion.Count(f => f.QuestionText.Trim().ToLower().Equals(search.Trim().ToLower()) && f.IsDeleted == false) > 0)
                return true;
            return false;
        }

        public int LCMSCount(string? search = null, string? isEmoji = null, string? filter = null)
        {
            IQueryable<FeedbackQuestion> Query = null;
            if (!string.IsNullOrWhiteSpace(search))
                Query = _db.FeedbackQuestion.Where(f => f.QuestionText.StartsWith(search) && f.IsDeleted == false);
            else
                Query = _db.FeedbackQuestion.Where(r => r.IsDeleted == false);

            if (isEmoji != null)
            {
                Boolean Emoji = isEmoji.ToLower().Equals("true") ? true : false;
                Query = Query.Where(r => r.IsEmoji == Emoji);
            }

            return Query.Count();
        }

        public async Task<List<CourseFeedbackAPI>> GetActiveFeedbackQuestion(int page, int pageSize, string? isEmoji = null, string? search = null, string? filter = null)
        {


            IQueryable<CourseFeedbackAPI> Query = (from questions in _db.FeedbackQuestion
                                                   where questions.IsDeleted == false && questions.IsActive == true
                                                   select new CourseFeedbackAPI
                                                   {
                                                       QuestionType = questions.QuestionType,
                                                       QuestionText = questions.QuestionText,
                                                       NoOfOption = _db.FeedbackOption.Where(c => c.FeedbackQuestionID == questions.Id).Count(),
                                                       SubjectiveAnswerLimit = questions.SubjectiveAnswerLimit,
                                                       Status = (questions.IsActive == true) ? "Active" : "Inactive",
                                                       IsActive = questions.IsActive,
                                                       IsEmoji = questions.IsEmoji,
                                                       Id = questions.Id,
                                                       IsSubjective = questions.IsSubjective
                                                   });



            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.QuestionText.Contains(search));
            }

            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            List<CourseFeedbackAPI> feedbackOptions = await Query.ToListAsync();
            return feedbackOptions;
        }


        public int ActiveQuestionCount(string? search = null, string? columnName = null, string? isEmoji = null)
        {
            IQueryable<FeedbackQuestion> Query = null;
            if (!string.IsNullOrWhiteSpace(search))
                Query = _db.FeedbackQuestion.Where(f => f.QuestionText.Contains(search) && f.IsDeleted == false && f.IsActive == true);
            else
                Query = _db.FeedbackQuestion.Where(r => r.IsDeleted == false && r.IsActive == true);

            return Query.Count();
        }
        public async Task<List<CourseFeedbackAPI>> GetLCMS(int page, int pageSize, string? isEmoji = null, string? search = null, string? filter = null)
        {


            IQueryable<CourseFeedbackAPI> Query = (from questions in _db.FeedbackQuestion
                                                   where questions.IsDeleted == false

                                                   select new CourseFeedbackAPI
                                                   {
                                                       QuestionType = questions.QuestionType,
                                                       QuestionText = questions.QuestionText,
                                                       NoOfOption = _db.FeedbackOption.Where(c => c.FeedbackQuestionID == questions.Id).Count(),
                                                       SubjectiveAnswerLimit = questions.SubjectiveAnswerLimit,
                                                       Status = (questions.IsActive == true) ? "Active" : "Inactive",
                                                       IsActive = questions.IsActive,
                                                       IsEmoji = questions.IsEmoji,
                                                       Id = questions.Id
                                                   });

            if (isEmoji != null)
            {
                Boolean Emoji = isEmoji.ToLower().Equals("true") ? true : false;
                Query = Query.Where(r => r.IsEmoji == Emoji);
            }

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.QuestionText.StartsWith(search));
            }

            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            List<CourseFeedbackAPI> feedbackOptions = await Query.ToListAsync();
            return feedbackOptions;
        }


        public int Count(string? search = null, string? filter = null)
        {
            IQueryable<FeedbackQuestion> Query = null;
            if (!string.IsNullOrWhiteSpace(search))
                Query = _db.FeedbackQuestion.Where(f => f.QuestionText.Contains(search) && f.IsDeleted == false);
            else
                Query = _db.FeedbackQuestion.Where(r => r.IsDeleted == false);


            return Query.Count();
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
        public async Task<List<CourseFeedbackAPI>> GetPagination(int page, int pageSize, string? search = null, string? filter = null)
        {
            IQueryable<CourseFeedbackAPI> Query = (from questions in _db.FeedbackQuestion
                                                   where questions.IsDeleted == false

                                                   select new CourseFeedbackAPI
                                                   {
                                                       QuestionType = questions.QuestionType,
                                                       QuestionText = questions.QuestionText,
                                                       NoOfOption = _db.FeedbackOption.Where(c => c.FeedbackQuestionID == questions.Id).Count(),
                                                       SubjectiveAnswerLimit = questions.SubjectiveAnswerLimit,
                                                       Status = (questions.IsActive == true) ? "Active" : "Inactive",
                                                       IsActive = questions.IsActive,
                                                       IsEmoji = questions.IsEmoji,
                                                       Id = questions.Id,
                                                       ModifiedDate = questions.ModifiedDate
                                                   });

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.QuestionText.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.ModifiedDate);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);

            List<CourseFeedbackAPI> feedbackOptions = await Query.ToListAsync();
            return feedbackOptions;
        }

        public async Task<List<CourseFeedbackAPI>> Get(int page, int pageSize, int UserId, string userRole, bool showAllData = false, string? search = null, string? filter = null)
        {


            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == UserId).FirstOrDefaultAsync();

            var Query = (from questions in _db.FeedbackQuestion
                         join user in _db.UserMaster on questions.ModifiedBy equals user.Id
                         join umd in _db.UserMasterDetails on questions.CreatedBy equals umd.UserMasterId
                         join Options in _db.FeedbackOption on questions.Id equals Options.FeedbackQuestionID
                         into temp
                         from Options in temp.DefaultIfEmpty()
                         where questions.IsDeleted == false
                         select new //CourseFeedbackAPI
                         {
                             QuestionType = questions.QuestionType,
                             QuestionText = questions.QuestionText,
                             NoOfOption = _db.FeedbackOption.Where(c => c.FeedbackQuestionID == questions.Id).Count(),
                             SubjectiveAnswerLimit = questions.SubjectiveAnswerLimit,
                             Status = (questions.IsActive == true) ? "Active" : "Inactive",

                             IsActive = questions.IsActive,
                             IsEmoji = questions.IsEmoji,
                             Id = questions.Id,
                             Option = Options.OptionText,
                             IsSubjective = questions.IsSubjective,

                             UserName = user.UserName,
                             AreaId = umd.AreaId,
                             LocationId = umd.LocationId,
                             GroupId = umd.GroupId,
                             BusinessId = umd.BusinessId,
                             CreatedBy = questions.CreatedBy,
                         });

            if (search == "null")
                search = null;

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
                Query = Query.Where(r => r.CreatedBy == UserId);
            }
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.QuestionText.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);

            var Result = await Query.ToListAsync();
            List<CourseFeedbackAPI> QuestionList = new List<CourseFeedbackAPI>();

            foreach (var ques in Result)
            {

                CourseFeedbackAPI q = new CourseFeedbackAPI
                {
                    QuestionType = ques.QuestionType,
                    QuestionText = ques.QuestionText,
                    NoOfOption = ques.NoOfOption,

                    SubjectiveAnswerLimit = ques.SubjectiveAnswerLimit,
                    Status = ques.Status,
                    IsActive = ques.IsActive,
                    IsEmoji = ques.IsEmoji,
                    Id = ques.Id,
                    IsSubjective = ques.IsSubjective,
                    UserName = ques.UserName,
                };
                QuestionList.Add(q);
            }


            QuestionList = QuestionList.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();
            foreach (var ques in Result)
            {
                var question = QuestionList.Where(x => x.Id == ques.Id).FirstOrDefault();
                string option = ques.Option;

                if (string.IsNullOrEmpty(question.Option1))
                {
                    question.Option1 = option;
                }
                else if (string.IsNullOrEmpty(question.Option2))
                {
                    question.Option2 = option;
                }
                else if (string.IsNullOrEmpty(question.Option3))
                {
                    question.Option3 = option;
                }
                else if (string.IsNullOrEmpty(question.Option4))
                {
                    question.Option4 = option;
                }
                else if (string.IsNullOrEmpty(question.Option5))
                {
                    question.Option5 = option;
                }
                else if (string.IsNullOrEmpty(question.Option6))
                {
                    question.Option6 = option;
                }
                else if (string.IsNullOrEmpty(question.Option7))
                {
                    question.Option7 = option;
                }
                else if (string.IsNullOrEmpty(question.Option8))
                {
                    question.Option8 = option;
                }
                else if (string.IsNullOrEmpty(question.Option9))
                {
                    question.Option9 = option;
                }
                else if (string.IsNullOrEmpty(question.Option10))
                {
                    question.Option10 = option;
                }
            }
            return QuestionList;
        }

        public async Task<CourseFeedbackAPI> GetFeedback(int questionId)
        {

            var QusestionNAnwser = await (from Questions in _db.FeedbackQuestion
                                          join Options in _db.FeedbackOption on Questions.Id equals Options.FeedbackQuestionID into temp
                                          from Options in temp.DefaultIfEmpty()
                                          join course in _db.Course on Questions.CourseId equals course.Id into questioncourse
                                          from course in questioncourse.DefaultIfEmpty()
                                          where Questions.Id == questionId
                                          select new
                                          {
                                              AnswersCounter = Questions.AnswersCounter,
                                              Id = Questions.Id,
                                              Skip = Questions.IsAllowSkipping,
                                              QuestionText = Questions.QuestionText,
                                              QuestionType = Questions.QuestionType,
                                              Section = Questions.Section,
                                              SubjectiveAnswerLimit = Questions.SubjectiveAnswerLimit,
                                              Status = (Questions.IsActive == true) ? "active" : "inactive",
                                              IsActive = Questions.IsActive,
                                              IsEmoji = Questions.IsEmoji,
                                              OptionId = Options != null ? Options.Id : 0,
                                              OptionText = Options != null ? Options.OptionText : null,
                                              Rating = Options != null ? Options.Rating : 0,
                                              CourseId = Questions.CourseId,
                                              Name = course.Title,
                                              Metadata = Questions.Metadata
                                          }).ToListAsync();

            CourseFeedbackAPI FeedbackQuestion = (from Questions in QusestionNAnwser
                                                  select new CourseFeedbackAPI
                                                  {
                                                      AnswersCounter = Questions.AnswersCounter,
                                                      Id = Questions.Id,
                                                      Skip = Questions.Skip,
                                                      QuestionText = Questions.QuestionText,
                                                      QuestionType = Questions.QuestionType,
                                                      Section = Questions.Section,
                                                      SubjectiveAnswerLimit = Questions.SubjectiveAnswerLimit,
                                                      Status = Questions.Status,
                                                      IsActive = Questions.IsActive,
                                                      IsEmoji = Questions.IsEmoji,
                                                      CourseId = Questions.CourseId,
                                                      CourseTitle = Questions.Name,
                                                      Metadata = Questions.Metadata
                                                  }).FirstOrDefault();


            FeedbackQuestion.Options = (from Answers in QusestionNAnwser
                                        where Answers.OptionId != 0
                                        select new Option
                                        {
                                            Id = Answers.OptionId,
                                            option = Answers.OptionText,
                                            Rating = Answers.Rating
                                        }).ToArray();
            FeedbackQuestion.AnswersCounter = FeedbackQuestion.Options.Count();
            FeedbackQuestion.optionSelector = FeedbackQuestion.Options.Count();
            return FeedbackQuestion;
        }
        public async Task<IEnumerable<CourseFeedbackAPI>> GetFeedbackByCourseId(int CourseId)
        {
            IEnumerable<FeedbackSheetConfigurationDetails> feedbackQuestions = _db.FeedbackSheetConfigurationDetails.Where(c => c.ConfigurationSheetId == CourseId).ToList();
            List<CourseFeedbackAPI> feedbackQuestionApis = new List<CourseFeedbackAPI>();
            foreach (FeedbackSheetConfigurationDetails feedbackQuestion in feedbackQuestions)
            {
                CourseFeedbackAPI feedbackAPI = new CourseFeedbackAPI();
                if (feedbackQuestion != null)
                {
                    FeedbackQuestion feedbackq = _db.FeedbackQuestion.Where(c => c.Id == feedbackQuestion.FeedbackId).FirstOrDefault();

                    feedbackAPI.AnswersCounter = feedbackq.AnswersCounter;
                    feedbackAPI.Id = feedbackq.Id;
                    feedbackAPI.Skip = feedbackq.IsAllowSkipping;
                    feedbackAPI.QuestionText = feedbackq.QuestionText;
                    feedbackAPI.QuestionType = feedbackq.QuestionType;
                    feedbackAPI.Section = feedbackq.Section;
                    feedbackAPI.IsEmoji = feedbackq.IsEmoji;
                    feedbackAPI.SubjectiveAnswerLimit = feedbackq.SubjectiveAnswerLimit;
                    List<Option> options = new List<Option>();
                    List<FeedbackOption> feedbackOptions = await _feedbackOption.GetAll(o => o.FeedbackQuestionID == feedbackq.Id);
                    foreach (FeedbackOption option in feedbackOptions)
                    {
                        Option opt = new Option();
                        opt.option = option.OptionText;
                        opt.Id = option.Id;
                        options.Add(opt);
                    }
                    feedbackAPI.AnswersCounter = options.Count();
                    feedbackAPI.Options = options.ToArray();
                    feedbackQuestionApis.Add(feedbackAPI);
                }
            }
            if (feedbackQuestionApis.Count > 0)
                return feedbackQuestionApis;
            return null;

        }
        public async Task<IEnumerable<CourseFeedbackAPI>> GetFeedbackByConfigurationId(int configurationId, string OrgnisationCode)
        {
            List<CourseFeedbackAPI> FeedbackQuestions = new List<CourseFeedbackAPI>();
            string cacheKey = Constants.FEEDBACK_QUESTIONS + "_" + OrgnisationCode + "_" + Convert.ToString(configurationId);
            var cache = new CacheManager.CacheManager();
            if (cache.IsAdded(cacheKey))
            {
                FeedbackQuestions = cache.Get<List<CourseFeedbackAPI>>(cacheKey);
            }
            else
            {
                var QuetionsNAnswers =
                await (from configurationDetails in _db.FeedbackSheetConfigurationDetails
                       join questions in _db.FeedbackQuestion on configurationDetails.FeedbackId equals questions.Id
                       join options in _db.FeedbackOption on questions.Id equals options.FeedbackQuestionID into temp
                       from options in temp.DefaultIfEmpty()
                       where configurationDetails.ConfigurationSheetId == configurationId
                       orderby configurationDetails.SequenceNumber ascending
                       select new
                       {
                           QuestionId = questions.Id,
                           questions.AnswersCounter,
                           questions.IsAllowSkipping,
                           questions.QuestionText,
                           questions.QuestionType,
                           questions.Section,
                           questions.SubjectiveAnswerLimit,
                           questions.IsEmoji,
                           optionId = options == null ? 0 : options.Id,
                           OptionText = options == null ? null : options.OptionText
                       }).ToListAsync();

                FeedbackQuestions =
                (from Quetions in QuetionsNAnswers
                 group Quetions by Quetions.QuestionId into temp
                 let feedbackQuestions = temp.FirstOrDefault()
                 select new CourseFeedbackAPI
                 {
                     Id = feedbackQuestions.QuestionId,
                     AnswersCounter = feedbackQuestions.AnswersCounter,
                     Skip = feedbackQuestions.IsAllowSkipping,
                     QuestionText = feedbackQuestions.QuestionText,
                     QuestionType = feedbackQuestions.QuestionType,
                     Section = feedbackQuestions.Section,
                     IsEmoji = feedbackQuestions.IsEmoji,
                     SubjectiveAnswerLimit = feedbackQuestions.SubjectiveAnswerLimit
                 }).ToList();

                foreach (CourseFeedbackAPI Question in FeedbackQuestions)
                {
                    Question.Options = (from Answers in QuetionsNAnswers
                                        where Answers.QuestionId == Question.Id
                                        select new Option
                                        {
                                            Id = Answers.optionId,
                                            option = Answers.OptionText
                                        }).ToArray();
                    Question.NoOfOption = Question.Options.Count();
                    Question.AnswersCounter = (Question.NoOfOption == null) ? 0 : (int)Question.NoOfOption;
                }

            }
            return FeedbackQuestions;
        }
        public async Task<IEnumerable<CourseFeedbackAnsAPI>> GetFeedbackAnsByConfigurationId(int configurationId, int ModuleId, int courseId, int UserId, string OrgnisationCode)
        {
            List<CourseFeedbackAnsAPI> FeedbackQuestions = new List<CourseFeedbackAnsAPI>();


            List<FeedbackStatusDetail> feedbackStatusDetail = new List<FeedbackStatusDetail>();

            var feedbackStatus = await (from fbstatusd in _db.FeedbackStatusDetail
                                        join fbstatus in _db.FeedbackStatus on fbstatusd.FeedbackStatusID equals fbstatus.Id
                                        where fbstatusd.CreatedBy == UserId && fbstatus.CourseId == courseId && fbstatus.ModuleId == ModuleId
                                        orderby fbstatusd.FeedBackQuestionID ascending
                                        select new
                                        {
                                            fbstatusd.FeedBackOptionID,
                                            fbstatusd.FeedBackQuestionID,
                                            fbstatusd.SubjectiveAnswer,
                                            fbstatusd.Rating
                                        }).ToListAsync();

            var QuetionsNAnswers =
                await (from configurationDetails in _db.FeedbackSheetConfigurationDetails
                       join questions in _db.FeedbackQuestion on configurationDetails.FeedbackId equals questions.Id
                       join options in _db.FeedbackOption on questions.Id equals options.FeedbackQuestionID into temp
                       from options in temp.DefaultIfEmpty()
                       where configurationDetails.ConfigurationSheetId == configurationId
                       orderby configurationDetails.SequenceNumber ascending
                       select new
                       {
                           QuestionId = questions.Id,
                           questions.AnswersCounter,
                           questions.IsAllowSkipping,
                           questions.QuestionText,
                           questions.QuestionType,
                           questions.Section,
                           questions.SubjectiveAnswerLimit,
                           questions.IsEmoji,
                           questions.IsSubjective,
                           optionId = options == null ? 0 : options.Id,
                           OptionText = options == null ? null : options.OptionText
                       }).ToListAsync();

            FeedbackQuestions =
            (from Quetions in QuetionsNAnswers
             group Quetions by Quetions.QuestionId into temp
             let feedbackQuestions = temp.FirstOrDefault()
             select new CourseFeedbackAnsAPI
             {
                 Id = feedbackQuestions.QuestionId,
                 AnswersCounter = feedbackQuestions.AnswersCounter,
                 Skip = feedbackQuestions.IsAllowSkipping,
                 QuestionText = feedbackQuestions.QuestionText,
                 QuestionType = feedbackQuestions.QuestionType,
                 Section = feedbackQuestions.Section,
                 IsEmoji = feedbackQuestions.IsEmoji,
                 SubjectiveAnswerLimit = feedbackQuestions.SubjectiveAnswerLimit
             }).ToList();

            foreach (CourseFeedbackAnsAPI Question in FeedbackQuestions)
            {
                Question.Options = (from Answers in QuetionsNAnswers
                                    where Answers.QuestionId == Question.Id
                                    select new Option
                                    {
                                        Id = Answers.optionId,
                                        option = Answers.OptionText
                                    }).ToArray();
                Question.NoOfOption = Question.Options.Count();
                Question.AnswersCounter = (Question.NoOfOption == null) ? 0 : (int)Question.NoOfOption;
            }

            foreach (CourseFeedbackAnsAPI Question in FeedbackQuestions)
            {
                if (Question.QuestionType == "emoji")
                    Question.EmojiAns = feedbackStatus.Where(a => a.FeedBackQuestionID == Question.Id).Select(a => a.Rating).FirstOrDefault();

                if (Question.QuestionType == "subjective")
                    Question.SelectedAns = feedbackStatus.Where(a => a.FeedBackQuestionID == Question.Id).Select(a => a.SubjectiveAnswer).FirstOrDefault();

                if (Question.QuestionType == "objective")
                    Question.SelectedOptionId = feedbackStatus.Where(a => a.FeedBackQuestionID == Question.Id).Select(a => a.FeedBackOptionID).FirstOrDefault();


            }


            return FeedbackQuestions;
        }

        public bool QuestionExists(string question, int? id = null)
        {
            if (this._db.FeedbackQuestion.Count(Q =>
            Q.QuestionText.Trim().ToLower().Equals(question.Trim().ToLower())
            && Q.IsDeleted == false && (id == null || Q.Id != id)) > 0)
                return true;
            return false;
        }
        public async Task<string> ProcessImportFile(FileInfo file, IFeedbackQuestion _feedbackQuestion, IFeedbackQuestionRejectedRepository _feedbackQuestionRejectedRepository, IFeedbackOption _feedbackOption, int UserId)
        {
            string result;
            try
            {

                FeedbackQuestionImport.ProcessFile.Reset();
                int resultMessage = await FeedbackQuestionImport.ProcessFile.InitilizeAsync(file);
                if (resultMessage == 1)
                {
                    result = await FeedbackQuestionImport.ProcessFile.ProcessRecordsAsync(_feedbackQuestion, _feedbackQuestionRejectedRepository, _feedbackOption, UserId);
                    FeedbackQuestionImport.ProcessFile.Reset();
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
                    FeedbackQuestionImport.ProcessFile.Reset();
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
            List<FeedbackQuestion> Questions = new List<FeedbackQuestion>();
            foreach (int QuestionId in QuestionsId)
            {
                FeedbackQuestion Question = await this.Get(QuestionId);
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
        public async Task<bool> IsQuestionAssigned(int feedbackId)
        {
            int QuestionCount = await this._db.FeedbackSheetConfigurationDetails.Where(c => c.FeedbackId == feedbackId && c.IsDeleted == Record.NotDeleted).CountAsync();
            if (QuestionCount > 0)
                return true;
            return false;
        }
        public async Task<ApiResponse> MultiDeleteFeedbackQuestion(APIDeleteFeedbackQuestion[] apideletemultipleques)
        {
            int totalRecordForDelete = apideletemultipleques.Count();
            int totalRecordRejected = 0;
            List<APIDeleteFeedbackQuestion> FeedbackQueStatusList = new List<APIDeleteFeedbackQuestion>();
            foreach (APIDeleteFeedbackQuestion deletemultipleque in apideletemultipleques)
            {
                APIDeleteFeedbackQuestion feedbackqueStatus = new APIDeleteFeedbackQuestion();

                FeedbackQuestion Question = await this.Get(deletemultipleque.Id);

                if (await IsQuestionAssigned(deletemultipleque.Id))
                {
                    feedbackqueStatus.ErrMessage = Message.DependencyExist.ToString();
                    feedbackqueStatus.QuestionText = Question.QuestionText.ToString();
                    feedbackqueStatus.Id = Question.Id;
                    totalRecordRejected++;
                    FeedbackQueStatusList.Add(feedbackqueStatus);
                }
                else
                {
                    Question.IsDeleted = Record.Deleted;
                }
                await this.Update(Question);

            }

            string resultstring = (totalRecordForDelete - totalRecordRejected) + " records deleted out of " + totalRecordForDelete;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, FeedbackQueStatusList };
            return response;

        }

        public async Task<UserFeedbackQueTotalAPI> GetPaginationV2(int page, int pageSize, int userId, string userRole, bool showAllData = false, string? search = null, string? filter = null, bool? isemoji = null)
        {
            UserFeedbackQueTotalAPI userFeedbackQueTotalAPI = new UserFeedbackQueTotalAPI();
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
            List<CourseAuthorAssociation> coursesAuthor = await _db.CourseAuthorAssociation.Where(r => r.UserId == userId && r.IsDeleted == 0).ToListAsync();
            //  List<int> FieldIds = await GetDistinctGroupIds(userId);

            IQueryable<UserFeedbackQueAPI> Query = (from questions in _db.FeedbackQuestion
                                                    join user in _db.UserMaster on questions.ModifiedBy equals user.Id into um
                                                    from user in um.DefaultIfEmpty()
                                                    join umd in _db.UserMasterDetails on questions.CreatedBy equals umd.UserMasterId into umddetails
                                                    from umd in umddetails.DefaultIfEmpty()
                                                    join course in _db.Course on questions.CourseId equals course.Id into courses
                                                    from course in courses.DefaultIfEmpty()

                                                    where questions.IsDeleted == false // && FieldIds.Contains((int)umd.BusinessId)
                                                    select new UserFeedbackQueAPI
                                                    {
                                                        QuestionType = questions.QuestionType,
                                                        QuestionText = questions.QuestionText,
                                                        NoOfOption = _db.FeedbackOption.Where(c => c.FeedbackQuestionID == questions.Id).Count(),
                                                        SubjectiveAnswerLimit = questions.SubjectiveAnswerLimit,
                                                        Status = (questions.IsActive == true) ? "Active" : "Inactive",
                                                        IsActive = questions.IsActive,
                                                        IsEmoji = questions.IsEmoji,
                                                        Id = questions.Id,
                                                        ModifiedDate = questions.ModifiedDate,
                                                        UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                                                        AreaId = umd.AreaId,
                                                        LocationId = umd.LocationId,
                                                        GroupId = umd.GroupId,
                                                        BusinessId = umd.BusinessId,
                                                        CreatedBy = questions.CreatedBy,
                                                        Metadata = questions.Metadata,
                                                        CourseTitle = course.Title,
                                                        UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (questions.CreatedBy == userId) ? true : false : true
                                                    }).AsNoTracking();

            IQueryable<UserFeedbackQueAPI> authorQuery = (from course in _db.Course
                                                          join author in _db.CourseAuthorAssociation.Where(a => a.UserId == userId) on course.Id equals author.CourseId
                                                          join cma in _db.CourseModuleAssociation on author.CourseId equals cma.CourseId
                                                          from module in _db.Module
                                                          where (module.Id == cma.FeedbackId || module.Id == course.FeedbackId)
                                                          join lcms in _db.LCMS on module.LCMSId equals lcms.Id
                                                          join fsc in _db.FeedbackSheetConfiguration on lcms.FeedbackSheetConfigID equals fsc.Id
                                                          join fscd in _db.FeedbackSheetConfigurationDetails on fsc.Id equals fscd.ConfigurationSheetId
                                                          join questions in _db.FeedbackQuestion on fscd.FeedbackId equals questions.Id
                                                          join user in _db.UserMaster on questions.ModifiedBy equals user.Id into um
                                                          from user in um.DefaultIfEmpty()
                                                          join umd in _db.UserMasterDetails on questions.CreatedBy equals umd.UserMasterId into umddetails
                                                          from umd in umddetails.DefaultIfEmpty()

                                                          where questions.IsDeleted == false // && FieldIds.Contains((int)umd.BusinessId)
                                                          select new UserFeedbackQueAPI
                                                          {
                                                              QuestionType = questions.QuestionType,
                                                              QuestionText = questions.QuestionText,
                                                              NoOfOption = _db.FeedbackOption.Where(c => c.FeedbackQuestionID == questions.Id).Count(),
                                                              SubjectiveAnswerLimit = questions.SubjectiveAnswerLimit,
                                                              Status = (questions.IsActive == true) ? "Active" : "Inactive",
                                                              IsActive = questions.IsActive,
                                                              IsEmoji = questions.IsEmoji,
                                                              Id = questions.Id,
                                                              ModifiedDate = questions.ModifiedDate,
                                                              UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                                                              AreaId = umd.AreaId,
                                                              LocationId = umd.LocationId,
                                                              GroupId = umd.GroupId,
                                                              BusinessId = umd.BusinessId,
                                                              CreatedBy = questions.CreatedBy,
                                                              Metadata = questions.Metadata,
                                                              CourseTitle = course.Title,
                                                              UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (questions.CreatedBy == userId) ? true : false : true
                                                          }).AsNoTracking();

            if (filter == "null")
                filter = null;
            if (search == "null")
                search = null;


            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (filter.ToLower().Equals("metadata"))
                        authorQuery = authorQuery.Where(r => r.Metadata.Contains(search));
                    if (filter.ToLower().Equals("question"))
                        authorQuery = authorQuery.Where(r => r.QuestionText.Contains(search));
                    if (filter.ToLower().Equals("coursename"))
                        authorQuery = authorQuery.Where(r => r.CourseTitle.Contains(search));
                }
                else
                {
                    authorQuery = authorQuery.Where(r => r.QuestionText.Contains(search));

                }
            }

            if (isemoji != null)
            {
                authorQuery = authorQuery.Where(r => r.IsEmoji == (isemoji.Value == true) ? true : false);
            }
            //if (search != null)
            //  search = search.ToLower().Equals("null") ? null : search;

            if (!string.IsNullOrEmpty(search))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (filter.ToLower().Equals("metadata"))
                        Query = Query.Where(r => r.Metadata.Contains(search));
                    if (filter.ToLower().Equals("question"))
                        Query = Query.Where(r => r.QuestionText.Contains(search));
                    if (filter.ToLower().Equals("coursename"))
                        Query = Query.Where(r => r.CourseTitle.Contains(search));
                }
                else
                {
                    Query = Query.Where(r => r.QuestionText.Contains(search));
                }
            }
            if (isemoji != null)
            {
                Query = Query.Where(r => r.IsEmoji == (isemoji.Value == true) ? true : false);
            }
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
            //if (showAllData==false && (userRole == UserRoles.LA|| userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA))
            if (showAllData == false && (userRole != UserRoles.CA))
            {
                Query = Query.Where(r => r.CreatedBy == userId);
            }

            var queryResult = Query.Union(authorQuery);
            userFeedbackQueTotalAPI.TotalRecords = await queryResult.Distinct().CountAsync();

            queryResult = queryResult.OrderByDescending(r => r.ModifiedDate);
            if (page != -1)
                queryResult = queryResult.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                queryResult = queryResult.Take(pageSize);

            List<UserFeedbackQueAPI> feedbackQue = await queryResult.Distinct().ToListAsync();

            userFeedbackQueTotalAPI.Data = feedbackQue;


            return userFeedbackQueTotalAPI;
        }
        public async Task<List<int>> GetDistinctGroupIds(int loggedInUserId)
        {
            List<int> FieldIds = new List<int>();
            FieldIds.Add(1);
            FieldIds.Add(2);
            //IQueryable<int> Query = (from u in _db.DistributedAdminFieldAssociation
            //                                        where (u.UserId == loggedInUserId)
            //                                        select new 
            //                                        {
            //                                             u.FieldId
            //                                        });

            // return await Query.ToListAsync();
            return FieldIds;
        }
    }
}
