using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using log4net;
using Assessment.API.Common;
using Assessment.API.Model;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.CacheManager;

namespace Assessment.API.Repositories
{
    public class AssessmentSheetConfigurationRepository : Repository<AssessmentSheetConfiguration>, IAssessmentConfigurationSheets
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentSheetConfigurationRepository));
        private AssessmentContext _db;
        private IAssessmentSheetConfigurationDetails _assessmentSheetConfigurationDetails;
        private ILCMSRepository _lcmsRepository;
        private ICustomerConnectionStringRepository _customerConnection;
        IModuleRepository _moduleRepository;
        public AssessmentSheetConfigurationRepository(
            AssessmentContext context,
            IAssessmentSheetConfigurationDetails assessmentSheetConfigurationDetails, ICustomerConnectionStringRepository customerConnection,
        ILCMSRepository lcmsRepository, IModuleRepository moduleRepository) : base(context)
        {
            this._db = context;
            this._assessmentSheetConfigurationDetails = assessmentSheetConfigurationDetails;
            this._lcmsRepository = lcmsRepository;
            this._moduleRepository = moduleRepository;
            this._customerConnection = customerConnection;
        }
        public int GetPassingPercentage(int assessmentSheetConfigID, string OrgCode)
        {
            int passingPercentage;
            var cache = new CacheManager.CacheManager();
            string cacheKeyConfig = CacheKeyNames.ASSESSMENT_PASSING_PERCENTAGE + assessmentSheetConfigID + OrgCode;
            if (cache.IsAdded(cacheKeyConfig))
                passingPercentage = Convert.ToInt32(cache.Get<string>(cacheKeyConfig));
            else
            {
                passingPercentage = (from a in _db.AssessmentSheetConfiguration.AsNoTracking()
                                     where a.ID == assessmentSheetConfigID
                                     select a.PassingPercentage).FirstOrDefault();
                cache.Add(cacheKeyConfig, passingPercentage.ToString(), System.DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
            }
            return passingPercentage;

        }

        public async Task<int> ValidateAssessment(APIAssessmentConfiguration apiAssessmentQuestion, string contentType)
        {
            if (await _lcmsRepository.Exist(apiAssessmentQuestion.Name, contentType))
            {
                return -1;
            }
            if (await _moduleRepository.Exists(apiAssessmentQuestion.Name))
            {
                return -1;
            }
            bool? isFixed = apiAssessmentQuestion.IsFixed;
            if (isFixed != null)
                if (!isFixed.Value)
                    if (apiAssessmentQuestion.NoOfQuestionsToShow == null || apiAssessmentQuestion.NoOfQuestionsToShow == 0)
                        return -2;
            return 1;
        }
        public async Task<int> ConfigureAssessment(APIAssessmentConfiguration aPIAssessmentQuestion, int UserId)
        {
            try
            {
                string contentType = "assessment";
                if (aPIAssessmentQuestion.IsMemo)
                    contentType = "memo";
                else
                    contentType = "assessment";

                int assessmentaheetconfigid = 0;
                aPIAssessmentQuestion.Name = aPIAssessmentQuestion.Name.Trim();
                int IsValid = await ValidateAssessment(aPIAssessmentQuestion, contentType);
                if (IsValid != 1)
                    return IsValid;

                bool? isFixed = aPIAssessmentQuestion.IsFixed;
                if (aPIAssessmentQuestion.NoOfQuestionsToShow == null)
                {
                    if (isFixed == null)
                    {
                        isFixed = true;
                    }
                }

                AssessmentSheetConfiguration questionConfi = new AssessmentSheetConfiguration();
                questionConfi.MaximumNoOfAttempts = aPIAssessmentQuestion.MaximumNoOfAttempts;
                questionConfi.PassingPercentage = aPIAssessmentQuestion.PassingPercentage;
                questionConfi.Durations = aPIAssessmentQuestion.Durations;
                questionConfi.IsFixed = isFixed;
                questionConfi.NoOfQuestionsToShow = aPIAssessmentQuestion.NoOfQuestionsToShow;
                questionConfi.IsNegativeMarking = aPIAssessmentQuestion.IsNegativeMarking;
                questionConfi.IsRandomQuestion = aPIAssessmentQuestion.IsRandomQuestion;
                questionConfi.NegativeMarkingPercentage = aPIAssessmentQuestion.NegativeMarkingPercentage;
                questionConfi.CreatedBy = UserId;
                questionConfi.CreatedDate = DateTime.UtcNow;
                questionConfi.ModifiedBy = UserId;
                questionConfi.ModifiedDate = DateTime.UtcNow;
                questionConfi.IsEvaluationBySME = aPIAssessmentQuestion.IsEvaluationBySME;
                await this.Add(questionConfi);
                assessmentaheetconfigid = questionConfi.ID;
                int sequenceNumber = 1;
                aPIAssessmentQuestion.aPIQuestionConfiguration.ToList().ForEach(list =>
                {
                    list.SequenceNumber = sequenceNumber;
                    sequenceNumber++;
                });
                List<AssessmentSheetConfigurationDetails> assessmentQuestions = new List<AssessmentSheetConfigurationDetails>();
                var QList = (from qlist in aPIAssessmentQuestion.aPIQuestionConfiguration
                             orderby qlist.QuestionID ascending
                             group qlist by qlist.QuestionID into q
                             select q.FirstOrDefault()).ToList();

                foreach (APIQuestionConfiguration opt in QList)
                {
                    AssessmentSheetConfigurationDetails assessmentQuestion = new AssessmentSheetConfigurationDetails();
                    assessmentQuestion.QuestionID = opt.QuestionID;
                    assessmentQuestion.AssessmentSheetConfigID = questionConfi.ID;
                    assessmentQuestion.SequenceNumber = opt.SequenceNumber;
                    assessmentQuestion.CreatedBy = UserId;
                    assessmentQuestion.CreatedDate = DateTime.UtcNow;
                    assessmentQuestion.ModifiedDate = DateTime.UtcNow;
                    assessmentQuestion.ModifiedBy = UserId;
                    assessmentQuestions.Add(assessmentQuestion);
                }

                await _assessmentSheetConfigurationDetails.AddRange(assessmentQuestions);

                LCMS lcms = new LCMS();
                lcms.Description = aPIAssessmentQuestion.Description;
                lcms.MetaData = aPIAssessmentQuestion.MetaData;
                lcms.Name = aPIAssessmentQuestion.Name;
                lcms.AssessmentSheetConfigID = questionConfi.ID;
                lcms.Duration = aPIAssessmentQuestion.Durations;
                lcms.Ismodulecreate = aPIAssessmentQuestion.Ismodulecreate;
                if (aPIAssessmentQuestion.IsMemo)
                    lcms.ContentType = "memo";
                else
                    lcms.ContentType = "assessment";

                lcms.CreatedBy = UserId;
                lcms.ModifiedBy = UserId;
                lcms.CreatedDate = DateTime.UtcNow;
                lcms.IsActive = true;
                lcms.IsNested = false;
                await _lcmsRepository.Add(lcms);

                if (aPIAssessmentQuestion.Ismodulecreate)
                {
                    Module module = new Module();
                    module.IsActive = true;
                    module.LCMSId = lcms.Id;
                    module.Name = aPIAssessmentQuestion.Name;
                    module.Description = aPIAssessmentQuestion.MetaData;
                    module.CourseType = "Assessment";
                    module.ModuleType = "Assessment";
                    module.CreatedDate = DateTime.UtcNow;
                    module.ModifiedDate = DateTime.UtcNow;
                    module.CreatedBy = UserId;
                    module.ModifiedBy = UserId;
                    module.IsMultilingual = false;
                    await _moduleRepository.Add(module);

                }
                return lcms.Id;

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw (ex);
            }
        }

        public async Task<Message> UpdateConfiguration(int id, APIAssessmentConfiguration apiAssessmentQuestion, int UserId, string OrgCode)
        {

            var cache = new CacheManager.CacheManager();
            string cacheKeyQuestions = Constants.ASSESSMENT_QUESTIONS + apiAssessmentQuestion.assessmentaheetconfigid.ToString() + OrgCode;
            string cacheKeyConfig = Constants.ASSESSMENT_CONFIG + apiAssessmentQuestion.assessmentaheetconfigid.ToString() + OrgCode;
            string cacheKeyAssHeaders = Constants.ASSESSMENT_HEADER + OrgCode + apiAssessmentQuestion.assessmentaheetconfigid.ToString();//+ CourseId.ToString() + ModuleId.ToString();

            if (cache.IsAdded(cacheKeyAssHeaders))
            {
                cache.Remove(cacheKeyAssHeaders);
            }
            if (cache.IsAdded(cacheKeyConfig))
            {
                cache.Remove(cacheKeyConfig);
            }
            if (cache.IsAdded(cacheKeyQuestions))
            {
                cache.Remove(cacheKeyQuestions);
            }

            AssessmentSheetConfiguration questionConfi = await this.Get((int)apiAssessmentQuestion.assessmentaheetconfigid);
            LCMS lcms = await _lcmsRepository.GetLcmsByAssessmentConfigureId(questionConfi.ID);
            if (questionConfi == null || lcms == null)
                return Message.NotFound;

            bool? isFixed = apiAssessmentQuestion.IsFixed;
            if (apiAssessmentQuestion.NoOfQuestionsToShow == null)
            {
                if (isFixed == null)
                {
                    isFixed = true;
                }
            }

            if (isFixed != null)
                if (!isFixed.Value)
                    if (apiAssessmentQuestion.NoOfQuestionsToShow == null || apiAssessmentQuestion.NoOfQuestionsToShow == 0)
                        return Message.InvalidModel;

            if (questionConfi.MaximumNoOfAttempts > apiAssessmentQuestion.MaximumNoOfAttempts)
            {
                return Message.DependencyExist;
            }
            questionConfi.MaximumNoOfAttempts = apiAssessmentQuestion.MaximumNoOfAttempts;
            questionConfi.PassingPercentage = apiAssessmentQuestion.PassingPercentage;
            questionConfi.Durations = apiAssessmentQuestion.Durations;
            questionConfi.IsFixed = isFixed;
            questionConfi.NoOfQuestionsToShow = apiAssessmentQuestion.NoOfQuestionsToShow;
            questionConfi.CreatedBy = UserId;
            questionConfi.CreatedDate = DateTime.UtcNow;
            questionConfi.ModifiedBy = UserId;
            questionConfi.ModifiedDate = DateTime.UtcNow;
            questionConfi.IsNegativeMarking = apiAssessmentQuestion.IsNegativeMarking;
            questionConfi.IsRandomQuestion = apiAssessmentQuestion.IsRandomQuestion;
            questionConfi.NegativeMarkingPercentage = apiAssessmentQuestion.NegativeMarkingPercentage;
            questionConfi.IsEvaluationBySME = apiAssessmentQuestion.IsEvaluationBySME;
            await this.Update(questionConfi);
            int assessmentaheetconfigid = questionConfi.ID;
            int sequenceNumber = 1;
            apiAssessmentQuestion.aPIQuestionConfiguration.ToList().ForEach(list =>
            {
                list.SequenceNumber = sequenceNumber;
                sequenceNumber++;
            });
            List<AssessmentSheetConfigurationDetails> assessmentQuestions = new List<AssessmentSheetConfigurationDetails>();
            var QList = (from qlist in apiAssessmentQuestion.aPIQuestionConfiguration
                         orderby qlist.QuestionID ascending
                         group qlist by qlist.QuestionID into q
                         select q.FirstOrDefault()).ToList();
            List<AssessmentSheetConfigurationDetails> ListQuestion = _assessmentSheetConfigurationDetails.GetConfigurations(assessmentaheetconfigid);
            _assessmentSheetConfigurationDetails.RemoveQuestions(ListQuestion);
            ListQuestion = new List<AssessmentSheetConfigurationDetails>();

            foreach (APIQuestionConfiguration opt in QList)
            {
                AssessmentSheetConfigurationDetails assessmentQuestion = new AssessmentSheetConfigurationDetails();
                assessmentQuestion.QuestionID = opt.QuestionID;
                assessmentQuestion.AssessmentSheetConfigID = assessmentaheetconfigid;
                assessmentQuestion.SequenceNumber = opt.SequenceNumber;
                assessmentQuestion.CreatedBy = UserId;
                assessmentQuestion.CreatedDate = DateTime.UtcNow;
                assessmentQuestion.ModifiedDate = DateTime.UtcNow;
                assessmentQuestion.ModifiedBy = UserId;
                ListQuestion.Add(assessmentQuestion);
            }
            await _assessmentSheetConfigurationDetails.AddRange(ListQuestion);

            lcms.Description = apiAssessmentQuestion.Description;
            lcms.MetaData = apiAssessmentQuestion.MetaData;
            lcms.Name = apiAssessmentQuestion.Name;
            lcms.AssessmentSheetConfigID = questionConfi.ID;
            lcms.Duration = apiAssessmentQuestion.Durations;
            if (apiAssessmentQuestion.IsMemo)
                lcms.ContentType = "memo";
            else
                lcms.ContentType = "assessment";
            lcms.CreatedBy = UserId;
            lcms.ModifiedBy = UserId;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.IsActive = true;
            lcms.Ismodulecreate = apiAssessmentQuestion.Ismodulecreate;
            await _lcmsRepository.Update(lcms);
            return Message.Ok;
        }

        public async Task<bool> IsAssessmentConfigurationIdExist(APIStartAssessment aPIStartAssessment)
        {
            int? AssessmentSheetConfigID = null;
            if (aPIStartAssessment.ModuleId == null || aPIStartAssessment.ModuleId == 0)
            {
                if (aPIStartAssessment.IsPreAssessment)
                {
                    AssessmentSheetConfigID =
                        await (from course in _db.Course
                               join assessment in _db.Module on course.PreAssessmentId equals assessment.Id
                               join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                               where (course.Id == aPIStartAssessment.CourseId && course.IsDeleted == false && assessment.IsDeleted == false)
                               select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                }
                else
                {
                    AssessmentSheetConfigID =
                       await (from course in _db.Course
                              join assessment in _db.Module on course.AssessmentId equals assessment.Id
                              join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                              where (course.Id == aPIStartAssessment.CourseId && course.IsDeleted == false && assessment.IsDeleted == false)
                              select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                }
            }
            else
            {
                if (aPIStartAssessment.IsPreAssessment)
                {
                    AssessmentSheetConfigID =
                       await (from courseModule in _db.CourseModuleAssociation
                              join assessment in _db.Module on courseModule.PreAssessmentId equals assessment.Id
                              join module in _db.Module on courseModule.ModuleId equals module.Id
                              join course in _db.Course on courseModule.CourseId equals course.Id
                              join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                              where (course.Id == aPIStartAssessment.CourseId && module.Id == aPIStartAssessment.ModuleId && course.IsDeleted == false &&
                              module.IsDeleted == false && assessment.IsDeleted == false)
                              select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                }
                else if (aPIStartAssessment.IsContentAssessment)
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
                                    cmd.CommandText = "GetAssessmentConfigIdByModuleIDAndCourseID";
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.BigInt) { Value = aPIStartAssessment.ModuleId });
                                    cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Bit) { Value = aPIStartAssessment.CourseId });

                                    DbDataReader reader = await cmd.ExecuteReaderAsync();
                                    DataTable dt = new DataTable();
                                    dt.Load(reader);
                                    if (dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            AssessmentSheetConfigID = Convert.ToInt32(row["AssessmentSheetConfigID"].ToString());
                                        }
                                    }
                                    reader.Dispose();
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw (ex);
                    }
                }
                else
                {
                    AssessmentSheetConfigID =
                      await (from courseModule in _db.CourseModuleAssociation
                             join assessment in _db.Module on courseModule.AssessmentId equals assessment.Id
                             join module in _db.Module on courseModule.ModuleId equals module.Id
                             join course in _db.Course on courseModule.CourseId equals course.Id
                             join lcms in _db.LCMS on assessment.LCMSId equals lcms.Id
                             where (course.Id == aPIStartAssessment.CourseId && module.Id == aPIStartAssessment.ModuleId && course.IsDeleted == false &&
                             module.IsDeleted == false && assessment.IsDeleted == false)
                             select (lcms.AssessmentSheetConfigID)).FirstOrDefaultAsync();
                }
            }
            if (AssessmentSheetConfigID == null)
            {
                return false;
            }
            if (AssessmentSheetConfigID == aPIStartAssessment.AssessmentSheetConfigID)
            {
                return true;
            }
            return false;
        }
    }
}
