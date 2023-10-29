using AutoMapper;
using Evaluation.API.APIModel;
using Evaluation.API.Model;
using Evaluation.API.Models;
using Evaluation.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Evaluation.API.Repositories;
using Evaluation.API.Model;
using Microsoft.Extensions.Hosting;
using Evaluation.API.APIModel;
using Evaluation.API.Repositories.Interface;
using Evaluation.API.APIModel;
using Evaluation.API.Repositories.Interfaces.Competency;
using log4net;
using Evaluation.API.Helper;
namespace Evaluation.API.Repositories
{
    public class ProcessEvaluationRepository : Repository<ProcessEvaluationQuestion>,IProcessEvaluationQuestion
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessEvaluationRepository));
        private CourseContext _db;
        private IConfiguration _configuration;
        private readonly IHostingEnvironment hostingEnvironment;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;


        public ProcessEvaluationRepository(IHostingEnvironment environment, ICustomerConnectionStringRepository customerConnectionStringRepository ,
            //ICompetenciesMasterRepository competenciesMasterRepository, ICompetenciesAssessmentMappingRepository competenciesAssessmentRepository,
            CourseContext context, IConfiguration configuration, ICourseRepository courseRepository)
            //, IMyCoursesRepository myCoursesRepository)
            : base(context)
        {
            this._db = context;
            _configuration = configuration;
            this.hostingEnvironment = environment;
            _customerConnectionStringRepository = customerConnectionStringRepository;
        }


        public async Task<StringBuilder> GetTransactionID(apiGetTransactionID apigettransactionID, string LoginID)
        {
            StringBuilder transid = new StringBuilder(LoginID);

            if (!string.IsNullOrEmpty(apigettransactionID.Region))
            {
                transid.Append("_" + apigettransactionID.Region);
            }
            if (!string.IsNullOrEmpty(apigettransactionID.EvalUser))
            {
                transid.Append("_" + apigettransactionID.EvalUser);
            }



            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "dbo.GetTransactionIdForProcessEval";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@EvalDate", SqlDbType.DateTime) { Value = apigettransactionID.DateforEvaluation });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = apigettransactionID.EvalUserID });

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
                            transid = transid.Append("_" + row["transid"].ToString());
                        }
                        reader.Dispose();
                    }
                    return transid;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

            return transid;
        }

        public async Task<StringBuilder> GetTransactionIDForOER(apiGetTransactionID apigettransactionID, string LoginID)
        {
            StringBuilder transid = new StringBuilder(LoginID);

            if (!string.IsNullOrEmpty(apigettransactionID.Region))
            {
                transid.Append("_" + apigettransactionID.Region);
            }
            if (!string.IsNullOrEmpty(apigettransactionID.EvalUser))
            {
                transid.Append("_" + apigettransactionID.EvalUser);
            }



            try
            {
                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "dbo.GetTransactionIdForOERProcessEval";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@EvalDate", SqlDbType.DateTime) { Value = apigettransactionID.DateforEvaluation });
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = apigettransactionID.EvalUserID });

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
                            transid = transid.Append("_" + row["transid"].ToString());
                        }
                        reader.Dispose();
                    }
                    return transid;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }

            return transid;
        }

        public async Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForEvaluation() 
        {

            var ProcessQuestions = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions_single = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions_Subjective = new List<APIProcessEvaluationQuestion>();

            ProcessQuestions_Subjective = await (from ques in this._db.ProcessEvaluationQuestion
                                                 orderby ques.Id descending
                                                 where ques.IsDeleted == false && ques.OptionType == "Subjective"
                                                 select new APIProcessEvaluationQuestion
                                                 {
                                                     Id = ques.Id,
                                                     OptionType = ques.OptionType,
                                                     Marks = ques.Marks,
                                                     Status = ques.Status,
                                                     QuestionText = ques.QuestionText,
                                                     Section = ques.Section,
                                                     Category = ques.Category,
                                                     AllowNA = ques.AllowNA,
                                                     IsRequired = ques.IsRequired,
                                                     IsSubquestion = ques.IsSubquestion,
                                                     AllowTextReply = ques.AllowTextReply,
                                                     OptionCount = 0
                                                 }).OrderBy(c => c.Id).ToListAsync();

            ProcessQuestions_single = await (from ques in this._db.ProcessEvaluationQuestion
                                             orderby ques.Id descending
                                             where ques.IsDeleted == false && ques.OptionType == "SingleSelection"
                                             select new APIProcessEvaluationQuestion
                                             {
                                                 Id = ques.Id,
                                                 OptionType = ques.OptionType,
                                                 Marks = ques.Marks,
                                                 Status = ques.Status,
                                                 QuestionText = ques.QuestionText,
                                                 Section = ques.Section,
                                                 Category = ques.Category,
                                                 AllowNA = ques.AllowNA,
                                                 IsRequired = ques.IsRequired,
                                                 IsSubquestion = ques.IsSubquestion,
                                                 AllowTextReply = ques.AllowTextReply,
                                                 OptionCount = _db.ProcessEvaluationOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                             }).OrderBy(c => c.Id).ToListAsync();

            var ProcessQuestions_multiple = new List<APIProcessEvaluationQuestion>();

            ProcessQuestions_multiple = await (from ques in this._db.ProcessEvaluationQuestion
                                               orderby ques.Id descending
                                               where ques.IsDeleted == false && ques.OptionType == "MultipleSelection"
                                               select new APIProcessEvaluationQuestion
                                               {
                                                   Id = ques.Id,
                                                   OptionType = ques.OptionType,
                                                   Marks = ques.Marks,
                                                   Status = ques.Status,
                                                   QuestionText = ques.QuestionText,
                                                   Section = ques.Section,
                                                   Category = ques.Category,
                                                   AllowNA = ques.AllowNA,
                                                   IsRequired = ques.IsRequired,
                                                   IsSubquestion = ques.IsSubquestion,
                                                   AllowTextReply = ques.AllowTextReply,
                                                   OptionCount = _db.ProcessEvaluationOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                               }).OrderBy(c => c.Id).ToListAsync();


            var selectedIds_single = ProcessQuestions_single.Select(x => x.Id);

            var option_single_result = _db.ProcessEvaluationOption.Where(x => x.IsDeleted == false && selectedIds_single.Contains(x.QuestionID))
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.QuestionID,
                    ProcessQuestionOptionID = x.Id,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsCorrectAnswer,
                    RefQuestionID = x.RefQuestionID
                }).ToList();

            var options_single = option_single_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = (decimal)ProcessQuestions_single.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault()
                }).ToList();


            var selectedIds_multiple = ProcessQuestions_multiple.Select(x => x.Id);

            var options_multiple_result = _db.ProcessEvaluationOption.Where(x => x.IsDeleted == false && selectedIds_multiple.Contains(x.QuestionID))
               .Select(x => new APIPEQuestionOption()
               {
                   ProcessQuestionID = x.QuestionID,
                   ProcessQuestionOptionID = x.Id,
                   OptionText = x.OptionText,
                   IsAnswer = x.IsCorrectAnswer,
                   RefQuestionID = x.RefQuestionID,
               }).ToList();


            var options_multiple = options_multiple_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = Math.Round((decimal)ProcessQuestions_multiple.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault() / (decimal)ProcessQuestions_multiple.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.OptionCount).FirstOrDefault(), 2)
                }).ToList();





            //Assign options for each question 
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_single)
            {
                int correctoptioncount = 0;
                correctoptioncount = options_single.Where(x => x.ProcessQuestionID == Question.Id && x.IsAnswer == true).Count();
                if (correctoptioncount > 1)
                {
                    Question.Allcorrect = true;
                }
                else
                {
                    Question.Allcorrect = false;
                }

                if (Question.Section == "Extreme Violation")
                {
                    APIPEQuestionOption[] aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                    foreach (APIPEQuestionOption opt in aPIOptions)
                    {
                        if (opt.OptionText.ToLower() == "yes")
                        {
                            opt.DevidedMarks = 0;
                        }
                        else if (opt.OptionText.ToLower() == "no")
                        {
                            opt.DevidedMarks = -10;
                        }
                    }
                    Question.aPIOptions = aPIOptions;
                }
                else
                {
                    Question.aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                }
            }
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_multiple)
            {
                Question.aPIOptions = options_multiple.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
            }
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_Subjective)
            {
                List<APIPEQuestionOption> obj = new List<APIPEQuestionOption>();
                Question.aPIOptions = obj.ToArray();
            }
            ProcessQuestions = ProcessQuestions_single.Concat(ProcessQuestions_multiple).Concat(ProcessQuestions_Subjective).ToList();
            return ProcessQuestions.OrderBy(x => x.Id);
        }
        public async Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForEvaluationforChaiPoint(apiGetQuestionforChaipoint apiGetQuestionforChaipoint)
        {

            var ProcessQuestions = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions_single = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions_Subjective = new List<APIProcessEvaluationQuestion>();

            ProcessQuestions_Subjective = await (from ques in this._db.ProcessEvaluationQuestion
                                                 orderby ques.Id descending
                                                 where ques.IsDeleted == false && ques.OptionType == "Subjective" && ques.Section == apiGetQuestionforChaipoint.section
                                                 select new APIProcessEvaluationQuestion
                                                 {
                                                     Id = ques.Id,
                                                     OptionType = ques.OptionType,
                                                     Marks = ques.Marks,
                                                     Status = ques.Status,
                                                     QuestionText = ques.QuestionText,
                                                     Section = ques.Section,
                                                     Category = ques.Category,
                                                     AllowNA = ques.AllowNA,
                                                     IsRequired = ques.IsRequired,
                                                     IsSubquestion = ques.IsSubquestion,
                                                     AllowTextReply = ques.AllowTextReply,
                                                     OptionCount = 0
                                                 }).OrderBy(c => c.Id).ToListAsync();

            ProcessQuestions_single = await (from ques in this._db.ProcessEvaluationQuestion
                                             orderby ques.Id descending
                                             where ques.IsDeleted == false && ques.OptionType == "SingleSelection" && ques.Section == apiGetQuestionforChaipoint.section
                                             select new APIProcessEvaluationQuestion
                                             {
                                                 Id = ques.Id,
                                                 OptionType = ques.OptionType,
                                                 Marks = ques.Marks,
                                                 Status = ques.Status,
                                                 QuestionText = ques.QuestionText,
                                                 Section = ques.Section,
                                                 Category = ques.Category,
                                                 AllowNA = ques.AllowNA,
                                                 IsRequired = ques.IsRequired,
                                                 IsSubquestion = ques.IsSubquestion,
                                                 AllowTextReply = ques.AllowTextReply,
                                                 OptionCount = _db.ProcessEvaluationOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                             }).OrderBy(c => c.Id).ToListAsync();

            var ProcessQuestions_multiple = new List<APIProcessEvaluationQuestion>();

            ProcessQuestions_multiple = await (from ques in this._db.ProcessEvaluationQuestion
                                               orderby ques.Id descending
                                               where ques.IsDeleted == false && ques.OptionType == "MultipleSelection" && ques.Section == apiGetQuestionforChaipoint.section
                                               select new APIProcessEvaluationQuestion
                                               {
                                                   Id = ques.Id,
                                                   OptionType = ques.OptionType,
                                                   Marks = ques.Marks,
                                                   Status = ques.Status,
                                                   QuestionText = ques.QuestionText,
                                                   Section = ques.Section,
                                                   Category = ques.Category,
                                                   AllowNA = ques.AllowNA,
                                                   IsRequired = ques.IsRequired,
                                                   IsSubquestion = ques.IsSubquestion,
                                                   AllowTextReply = ques.AllowTextReply,
                                                   OptionCount = _db.ProcessEvaluationOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                               }).OrderBy(c => c.Id).ToListAsync();


            var selectedIds_single = ProcessQuestions_single.Select(x => x.Id);

            var option_single_result = _db.ProcessEvaluationOption.Where(x => x.IsDeleted == false && selectedIds_single.Contains(x.QuestionID))
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.QuestionID,
                    ProcessQuestionOptionID = x.Id,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsCorrectAnswer,
                    RefQuestionID = x.RefQuestionID
                }).ToList();

            var options_single = option_single_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = (decimal)ProcessQuestions_single.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault()
                }).ToList();


            var selectedIds_multiple = ProcessQuestions_multiple.Select(x => x.Id);

            var options_multiple_result = _db.ProcessEvaluationOption.Where(x => x.IsDeleted == false && selectedIds_multiple.Contains(x.QuestionID))
               .Select(x => new APIPEQuestionOption()
               {
                   ProcessQuestionID = x.QuestionID,
                   ProcessQuestionOptionID = x.Id,
                   OptionText = x.OptionText,
                   IsAnswer = x.IsCorrectAnswer,
                   RefQuestionID = x.RefQuestionID,
               }).ToList();


            var options_multiple = options_multiple_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = Math.Round((decimal)ProcessQuestions_multiple.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault() / (decimal)ProcessQuestions_multiple.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.OptionCount).FirstOrDefault(), 2)
                }).ToList();





            //Assign options for each question 
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_single)
            {
                int correctoptioncount = 0;
                correctoptioncount = options_single.Where(x => x.ProcessQuestionID == Question.Id && x.IsAnswer == true).Count();
                if (correctoptioncount > 1)
                {
                    Question.Allcorrect = true;
                }
                else
                {
                    Question.Allcorrect = false;
                }

                if (Question.Section == "Extreme Violation")
                {
                    APIPEQuestionOption[] aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                    foreach (APIPEQuestionOption opt in aPIOptions)
                    {
                        if (opt.OptionText.ToLower() == "yes")
                        {
                            opt.DevidedMarks = 0;
                        }
                        else if (opt.OptionText.ToLower() == "no")
                        {
                            opt.DevidedMarks = -10;
                        }
                    }
                    Question.aPIOptions = aPIOptions;
                }
                else
                {
                    Question.aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                }
            }
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_multiple)
            {
                Question.aPIOptions = options_multiple.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
            }
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_Subjective)
            {
                List<APIPEQuestionOption> obj = new List<APIPEQuestionOption>();
                Question.aPIOptions = obj.ToArray();
            }
            ProcessQuestions = ProcessQuestions_single.Concat(ProcessQuestions_multiple).Concat(ProcessQuestions_Subjective).ToList();
            return ProcessQuestions.OrderBy(x => x.Id);
        }

        public async Task<IEnumerable<PMSEvaluationPointResponse>> GetQuestionForPMSEvaluation(string Section, int userId)
        {

            var ProcessQuestions = new List<APIPMSEvaluationQuestion>();
            var ProcessQuestions_single = new List<APIPMSEvaluationQuestion>();
            var ProcessQuestions_Subjective = new List<APIPMSEvaluationQuestion>();
            var Points = new List<PMSEvaluationPoint>();

            string userrole = (from u in this._db.UserMaster
                               where u.Id == userId
                               select u.UserRole).FirstOrDefault().ToString();

                ProcessQuestions_Subjective = await (from ques in this._db.PMSEvaluationQuestion
                                                     orderby ques.Id descending
                                                     where ques.IsDeleted == false && ques.Section == Section
                                                     select new APIPMSEvaluationQuestion
                                                     {
                                                         Id = ques.Id,
                                                         OptionType = ques.OptionType,
                                                         Marks = ques.Marks,
                                                         Status = ques.Status,
                                                         QuestionText = ques.QuestionText,
                                                         Section = ques.Section,
                                                         Category = ques.Category,
                                                         AllowNA = ques.AllowNA,
                                                         IsRequired = ques.IsRequired,
                                                         IsSubquestion = ques.IsSubquestion,
                                                         Metadata = ques.Metadata,
                                                         AllowTextReply = ques.AllowTextReply,
                                                         ObservableBehaviorCompetency = ques.ObservableBehaviorCompetency,
                                                         DefinitionOfTheMeasure = ques.DefinitionOfTheMeasure
                                                     }).OrderBy(c => c.Id).ToListAsync();

                Points = await (from points in this._db.PMSEvaluationPoints
                                orderby points.Id descending
                                where points.Section == Section
                                select new PMSEvaluationPoint
                                {
                                    Id = points.Id,
                                    PointText = points.PointText,
                                    Section = points.Section,
                                    RefQuestionId = points.RefQuestionId,
                                }
                    ).OrderBy(c => c.Id).ToListAsync();


            List<PMSEvaluationPointResponse> finalResponse = new List<PMSEvaluationPointResponse>();

            foreach (APIPMSEvaluationQuestion item in ProcessQuestions_Subjective)
            {
                PMSEvaluationPointResponse obj = new PMSEvaluationPointResponse();
                obj.Id = item.Id;
                obj.Section = item.Section;
                obj.QuestionType = item.OptionType;
                obj.DefinitionOfTheMeasure = item.DefinitionOfTheMeasure;
                obj.ObservableBehaviorCompetency = item.ObservableBehaviorCompetency;

                /*                if (item.OptionType.ToLower() == "subjective")
                                {
                                    obj.QuestionText = item.QuestionText;
                                }
                                else
                                {
                                    obj.QuestionText = null;
                                }*/
                obj.QuestionText = item.QuestionText;

                List<string> temp = new List<string>();
                foreach (var i in Points)
                {
                    if (item.Id == i.RefQuestionId)
                    {
                        temp.Add(i.PointText);
                    }
                }
                obj.PointsText = temp;
                finalResponse.Add(obj);
            }

/*            if (userrole == "EU")
            {
                if (finalResponse.Any()) //prevent IndexOutOfRangeException for empty list
                {
                    finalResponse.RemoveAt(finalResponse.Count - 1);
                }
            }*/


            ProcessQuestions = ProcessQuestions_Subjective.ToList();
            return finalResponse.ToList().OrderBy(c => c.Id);
        }

        public async Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForKitchenAudit()
        {
            var ProcessQuestions_single = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions_Subjective = new List<APIProcessEvaluationQuestion>();

            ProcessQuestions_Subjective = await (from ques in this._db.KitchenAuditQuestion
                                                 orderby ques.Id descending
                                                 where ques.IsDeleted == false && ques.OptionType == "Subjective"
                                                 select new APIProcessEvaluationQuestion
                                                 {
                                                     Id = ques.Id,
                                                     OptionType = ques.OptionType,
                                                     Marks = ques.Marks,
                                                     Status = ques.Status,
                                                     QuestionText = ques.QuestionText,
                                                     Section = ques.Section,
                                                     Category = ques.Category,
                                                     AllowNA = ques.AllowNA,
                                                     IsRequired = ques.IsRequired,
                                                     IsSubquestion = ques.IsSubquestion,
                                                     AllowTextReply = ques.AllowTextReply,
                                                     OptionCount = 0
                                                 }).OrderBy(c => c.Id).ToListAsync();

            ProcessQuestions_single = await (from ques in this._db.KitchenAuditQuestion
                                             orderby ques.Id descending
                                             where ques.IsDeleted == false && ques.OptionType == "SingleSelection"
                                             select new APIProcessEvaluationQuestion
                                             {
                                                 Id = ques.Id,
                                                 OptionType = ques.OptionType,
                                                 Marks = ques.Marks,
                                                 Status = ques.Status,
                                                 QuestionText = ques.QuestionText,
                                                 Section = ques.Section,
                                                 Category = ques.Category,
                                                 AllowNA = ques.AllowNA,
                                                 IsRequired = ques.IsRequired,
                                                 IsSubquestion = ques.IsSubquestion,
                                                 AllowTextReply = ques.AllowTextReply,
                                                 OptionCount = _db.KitchenAuditOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                             }).OrderBy(c => c.Id).ToListAsync();

            var selectedIds_single = ProcessQuestions_single.Select(x => x.Id);

            var option_single_result = _db.KitchenAuditOption.Where(x => x.IsDeleted == false && selectedIds_single.Contains(x.QuestionID))
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.QuestionID,
                    ProcessQuestionOptionID = x.Id,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsCorrectAnswer,
                    RefQuestionID = x.RefQuestionID
                }).ToList();

            var options_single = option_single_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = (decimal)ProcessQuestions_single.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault()
                }).ToList();

            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_single)
            {
                int correctoptioncount = 0;
                correctoptioncount = options_single.Where(x => x.ProcessQuestionID == Question.Id && x.IsAnswer == true).Count();
                if (correctoptioncount > 1)
                {
                    Question.Allcorrect = true;
                }
                else
                {
                    Question.Allcorrect = false;
                }

                if (Question.Section == "Extreme Violation")
                {
                    APIPEQuestionOption[] aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                    foreach (APIPEQuestionOption opt in aPIOptions)
                    {
                        if (opt.OptionText.ToLower() == "yes")
                        {
                            opt.DevidedMarks = 0;
                        }
                        else if (opt.OptionText.ToLower() == "no")
                        {
                            opt.DevidedMarks = -10;
                        }
                    }
                    Question.aPIOptions = aPIOptions;
                }
                else
                {
                    Question.aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                }
            }
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_Subjective)
            {
                List<APIPEQuestionOption> obj = new List<APIPEQuestionOption>();
                Question.aPIOptions = obj.ToArray();
            }
            ProcessQuestions = ProcessQuestions_single.Concat(ProcessQuestions_Subjective).ToList();
            return ProcessQuestions.OrderBy(x => x.Id);

        }

        public async Task<IEnumerable<ProcessEvaluationQuestion>> GetAllQuestionPagination(int page, int pageSize, string search = null, string columnName = null, bool? isMemoQuestions = null)
        {
            IQueryable<ProcessEvaluationQuestion> Query = from ques in this._db.ProcessEvaluationQuestion
                                                          orderby ques.Id descending
                                                          where ques.IsDeleted == false
                                                          select new ProcessEvaluationQuestion
                                                          {
                                                              Id=ques.Id,
                                                              OptionType = ques.OptionType,
                                                              Marks = ques.Marks,
                                                              Status = ques.Status,
                                                              QuestionText = ques.QuestionText,
                                                              Section = ques.Section,
                                                              Metadata = ques.Metadata,
                                                              IsDeleted = ques.IsDeleted
                                                          };

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

        public async Task<IEnumerable<APIProcessEvaluationQuestion>> GetQuestionForOEREvaluation()
        {

            var ProcessQuestions = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions_single = new List<APIProcessEvaluationQuestion>();
            var ProcessQuestions_Subjective = new List<APIProcessEvaluationQuestion>();

            ProcessQuestions_Subjective = await (from ques in this._db.OEREvaluationQuestion
                                                 orderby ques.Id descending
                                                 where ques.IsDeleted == false && ques.OptionType == "Subjective"
                                                 select new APIProcessEvaluationQuestion
                                                 {
                                                     Id = ques.Id,
                                                     OptionType = ques.OptionType,
                                                     Marks = ques.Marks,
                                                     Status = ques.Status,
                                                     QuestionText = ques.QuestionText,
                                                     Section = ques.Section,
                                                     Category = ques.Category,
                                                     AllowNA = ques.AllowNA,
                                                     IsRequired = ques.IsRequired,
                                                     IsSubquestion = ques.IsSubquestion,
                                                     AllowTextReply = ques.AllowTextReply,
                                                     OptionCount = 0
                                                 }).OrderBy(c => c.Id).ToListAsync();

            ProcessQuestions_single = await (from ques in this._db.OEREvaluationQuestion
                                             orderby ques.Id descending
                                             where ques.IsDeleted == false && ques.OptionType == "SingleSelection"
                                             select new APIProcessEvaluationQuestion
                                             {
                                                 Id = ques.Id,
                                                 OptionType = ques.OptionType,
                                                 Marks = ques.Marks,
                                                 Status = ques.Status,
                                                 QuestionText = ques.QuestionText,
                                                 Section = ques.Section,
                                                 Category = ques.Category,
                                                 AllowNA = ques.AllowNA,
                                                 IsRequired = ques.IsRequired,
                                                 IsSubquestion = ques.IsSubquestion,
                                                 AllowTextReply = ques.AllowTextReply,
                                                 OptionCount = _db.OEREvaluationOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                             }).OrderBy(c => c.Id).ToListAsync();

            var ProcessQuestions_multiple = new List<APIProcessEvaluationQuestion>();

            ProcessQuestions_multiple = await (from ques in this._db.OEREvaluationQuestion
                                               orderby ques.Id descending
                                               where ques.IsDeleted == false && ques.OptionType == "MultipleSelection"
                                               select new APIProcessEvaluationQuestion
                                               {
                                                   Id = ques.Id,
                                                   OptionType = ques.OptionType,
                                                   Marks = ques.Marks,
                                                   Status = ques.Status,
                                                   QuestionText = ques.QuestionText,
                                                   Section = ques.Section,
                                                   Category = ques.Category,
                                                   AllowNA = ques.AllowNA,
                                                   IsRequired = ques.IsRequired,
                                                   IsSubquestion = ques.IsSubquestion,
                                                   AllowTextReply = ques.AllowTextReply,
                                                   OptionCount = _db.OEREvaluationOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                               }).OrderBy(c => c.Id).ToListAsync();


            var selectedIds_single = ProcessQuestions_single.Select(x => x.Id);

            var option_single_result = _db.OEREvaluationOption.Where(x => x.IsDeleted == false && selectedIds_single.Contains(x.QuestionID))
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.QuestionID,
                    ProcessQuestionOptionID = x.Id,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsCorrectAnswer,
                    RefQuestionID = x.RefQuestionID
                }).ToList();

            var options_single = option_single_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = (decimal)ProcessQuestions_single.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault()
                }).ToList();


            var selectedIds_multiple = ProcessQuestions_multiple.Select(x => x.Id);

            var options_multiple_result = _db.OEREvaluationOption.Where(x => x.IsDeleted == false && selectedIds_multiple.Contains(x.QuestionID))
               .Select(x => new APIPEQuestionOption()
               {
                   ProcessQuestionID = x.QuestionID,
                   ProcessQuestionOptionID = x.Id,
                   OptionText = x.OptionText,
                   IsAnswer = x.IsCorrectAnswer,
                   RefQuestionID = x.RefQuestionID,
               }).ToList();


            var options_multiple = options_multiple_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = Math.Round((decimal)ProcessQuestions_multiple.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault() / (decimal)ProcessQuestions_multiple.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.OptionCount).FirstOrDefault(), 2)
                }).ToList();





            //Assign options for each question 
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_single)
            {
                int correctoptioncount = 0;
                correctoptioncount = options_single.Where(x => x.ProcessQuestionID == Question.Id && x.IsAnswer == true).Count();
                if (correctoptioncount > 1)
                {
                    Question.Allcorrect = true;
                }
                else
                {
                    Question.Allcorrect = false;
                }

                if (Question.Section == "Extreme Violation")
                {
                    APIPEQuestionOption[] aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                    foreach (APIPEQuestionOption opt in aPIOptions)
                    {
                        if (opt.OptionText.ToLower() == "yes")
                        {
                            opt.DevidedMarks = 0;
                        }
                        else if (opt.OptionText.ToLower() == "no")
                        {
                            opt.DevidedMarks = -10;
                        }
                    }
                    Question.aPIOptions = aPIOptions;
                }
                else
                {
                    Question.aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                }
            }
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_multiple)
            {
                Question.aPIOptions = options_multiple.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
            }
            foreach (APIProcessEvaluationQuestion Question in ProcessQuestions_Subjective)
            {
                List<APIPEQuestionOption> obj = new List<APIPEQuestionOption>();
                Question.aPIOptions = obj.ToArray();
            }
            ProcessQuestions = ProcessQuestions_single.Concat(ProcessQuestions_multiple).Concat(ProcessQuestions_Subjective).ToList();
            return ProcessQuestions.OrderBy(x => x.Id);
        }

        public async Task<IEnumerable<APIProcessEvaluationManagement>> GetProcessManagement(string OrganisationCode, int UserId)
        {
            string UserUrl = _configuration[APIHelper.UserAPI];
            string NameById = "GetNameById";
            string ColumnName = "username";
            int Value = UserId;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnName + "/" + Value);
            xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
            if (response.IsSuccessStatusCode)
            {
                var username = await response.Content.ReadAsStringAsync();
                _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);

            }
            IQueryable<APIProcessEvaluationManagement> Query = from ques in this._db.ProcessEvaluationManagement
                                                               orderby ques.Id descending
                                                               where ques.IsDeleted == false
                                                               select new APIProcessEvaluationManagement
                                                               {
                                                                   Title = ques.Title,
                                                                   CreatedBy = _xAPIUserDetails.Name,
                                                                   Id = ques.Id,
                                                                   Objective = ques.Objective
                                                               };


            return await Query.ToListAsync();
        }

        public async Task<int> PMSEvaluationSumbit(int UserId, string ManagerId)
        {

            PMSEvaluationSubmit pMSEvaluationSubmit = new PMSEvaluationSubmit();
            pMSEvaluationSubmit.Userid = UserId;
            pMSEvaluationSubmit.ManagerId = Convert.ToInt32(Security.DecryptForUI(ManagerId));
            pMSEvaluationSubmit.Date = DateTime.Now;
            pMSEvaluationSubmit.Status = "Pending";

            await this._db.PMSEvaluationSubmit.AddAsync(pMSEvaluationSubmit);
            await this._db.SaveChangesAsync();

            return pMSEvaluationSubmit.Id;
        }

        public async Task<ApiResponse> PostProcessResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null)
        { 
            if (aPIPostAssessmentResult.evaluationType == "partner")
            {
                ApiResponse Response = new ApiResponse();

                KitchenAuditResult postprocessresult = new KitchenAuditResult();

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;


                postprocessresult.ManagementId = aPIPostAssessmentResult.ManagementId;
                postprocessresult.MarksObtained = aPIPostAssessmentResult.obtainedMarks;
                postprocessresult.Percentage = aPIPostAssessmentResult.obtainedPercentage;
                postprocessresult.Result = aPIPostAssessmentResult.Result;
                postprocessresult.TransactionId = aPIPostAssessmentResult.TransactionId;
                postprocessresult.TotalMarks = aPIPostAssessmentResult.TotalMarks;
                postprocessresult.supervisorId = aPIPostAssessmentResult.supervisorId;

                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.KitchenAuditResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchName = null;
                }
                else
                {
                    int noOfAttempts = _db.KitchenAuditResult.Where(r => r.BranchName == aPIPostAssessmentResult.BranchName && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchName = aPIPostAssessmentResult.BranchName;
                }
                postprocessresult.CreatedBy = UserId;
                postprocessresult.CreatedDate = DateTime.UtcNow;
                postprocessresult.ModifiedBy = UserId;
                postprocessresult.ModifiedDate = DateTime.UtcNow;
                postprocessresult.StarRating = aPIPostAssessmentResult.StarRating;
                postprocessresult.CountOfExtremeViolation = aPIPostAssessmentResult.CountOfExtremeViolation;
                postprocessresult.EvaluationDate = aPIPostAssessmentResult.EvaluationDate;
                postprocessresult.AuditType = aPIPostAssessmentResult.AuditType;

                await this._db.KitchenAuditResult.AddAsync(postprocessresult);
                await this._db.SaveChangesAsync();

                try
                {
                    await this.AddKitchenAuditQuestionDetails(aPIPostAssessmentResult.APIPostProcessQuestionDetails, postprocessresult.Id, UserId);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                string url = _configuration[Configuration.NotificationApi];

                url = url + "/MailForEvaluationResult";
                JObject oJsonObject = new JObject();
                oJsonObject.Add("CourseId", postprocessresult.Id);
                oJsonObject.Add("OrganizationCode", OrgCode);
                HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

                Response.ResponseObject = postprocessresult;
                Response.StatusCode = 200;
                return Response;
            }
            else
            {
                ApiResponse Response = new ApiResponse();

                ProcessResult postprocessresult = new ProcessResult();

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;


                postprocessresult.ManagementId = aPIPostAssessmentResult.ManagementId;
                postprocessresult.MarksObtained = aPIPostAssessmentResult.obtainedMarks;
                postprocessresult.Percentage = aPIPostAssessmentResult.obtainedPercentage;
                postprocessresult.Result = aPIPostAssessmentResult.Result;
                postprocessresult.TransactionId = aPIPostAssessmentResult.TransactionId;
                postprocessresult.TotalMarks = aPIPostAssessmentResult.TotalMarks;
                postprocessresult.supervisorId = aPIPostAssessmentResult.supervisorId;

                if (OrgCode == "lenexis" || OrgCode == "ent")
                {
                    if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                    {
                        int noOfAttempts = _db.ProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                        postprocessresult.NoOfAttempts = noOfAttempts;
                        postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                        postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                    }
                    else
                    {
                        int noOfAttempts = _db.ProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                        postprocessresult.NoOfAttempts = noOfAttempts;
                        postprocessresult.UserId = null;
                        postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                    {
                        int noOfAttempts = _db.ProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                        postprocessresult.NoOfAttempts = noOfAttempts;
                        postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                        postprocessresult.BranchId = null;
                    }
                    else
                    {
                        int noOfAttempts = _db.ProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                        postprocessresult.NoOfAttempts = noOfAttempts;
                        postprocessresult.UserId = null;
                        postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                    }
                }

                postprocessresult.CreatedBy = UserId;
                postprocessresult.CreatedDate = DateTime.UtcNow;
                postprocessresult.ModifiedBy = UserId;
                postprocessresult.ModifiedDate = DateTime.UtcNow;
                postprocessresult.StarRating = aPIPostAssessmentResult.StarRating;
                postprocessresult.CountOfExtremeViolation = aPIPostAssessmentResult.CountOfExtremeViolation;
                postprocessresult.EvaluationDate = aPIPostAssessmentResult.EvaluationDate;
                postprocessresult.AuditType = aPIPostAssessmentResult.AuditType;
                postprocessresult.AuditorName = null;
                postprocessresult.RegionName = null;
                postprocessresult.SiteName = null;
                postprocessresult.StaffName = null;
                postprocessresult.RestaurantManagerID = Convert.ToInt32(aPIPostAssessmentResult.RestaurantManagerID);

                await this._db.ProcessResult.AddAsync(postprocessresult);
                try
                {
                    await this._db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                try
                {
                    await this.AddProcessQuestionDetails(aPIPostAssessmentResult.APIPostProcessQuestionDetails, postprocessresult.Id, UserId, OrgCode);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                string url = _configuration[Configuration.NotificationApi];

                url = url + "/MailForEvaluationResult";
                JObject oJsonObject = new JObject();
                oJsonObject.Add("CourseId", postprocessresult.Id);
                oJsonObject.Add("OrganizationCode", OrgCode);
                HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

                Response.ResponseObject = postprocessresult;
                Response.StatusCode = 200;
                return Response;
            }
        }
        public async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }

        }

        public async Task<ApiResponse> PostProcessResultForChaipoint(APIPostProcessEvaluationResultForChaipoint aPIPostAssessmentResult, int UserId, string OrgCode = null)
        {
            ApiResponse Response = new ApiResponse();

            ProcessResult postprocessresult = new ProcessResult();

            if (aPIPostAssessmentResult.UserId == "null")
                aPIPostAssessmentResult.UserId = null;


            postprocessresult.ManagementId = aPIPostAssessmentResult.ManagementId;
            postprocessresult.MarksObtained = aPIPostAssessmentResult.obtainedMarks;
            postprocessresult.Percentage = aPIPostAssessmentResult.obtainedPercentage;
            postprocessresult.Result = aPIPostAssessmentResult.Result;
            postprocessresult.TransactionId = aPIPostAssessmentResult.TransactionId;
            postprocessresult.TotalMarks = aPIPostAssessmentResult.TotalMarks;
            postprocessresult.supervisorId = aPIPostAssessmentResult.supervisorId;

            if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
            {
                int noOfAttempts = _db.ProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                postprocessresult.NoOfAttempts = noOfAttempts;
                postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                postprocessresult.BranchId = null;
            }
            else
            {
                int noOfAttempts = _db.ProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                postprocessresult.NoOfAttempts = noOfAttempts;
                postprocessresult.UserId = null;
                postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
            }
            postprocessresult.CreatedBy = UserId;
            postprocessresult.CreatedDate = DateTime.UtcNow;
            postprocessresult.ModifiedBy = UserId;
            postprocessresult.ModifiedDate = DateTime.UtcNow;
            postprocessresult.StarRating = aPIPostAssessmentResult.StarRating;
            postprocessresult.CountOfExtremeViolation = aPIPostAssessmentResult.CountOfExtremeViolation;
            postprocessresult.EvaluationDate = aPIPostAssessmentResult.EvaluationDate;
            postprocessresult.AuditType = aPIPostAssessmentResult.AuditType;
            postprocessresult.AuditorName = aPIPostAssessmentResult.auditorName;
            postprocessresult.RegionName = aPIPostAssessmentResult.regionName;
            postprocessresult.SiteName = aPIPostAssessmentResult.siteName;
            postprocessresult.StaffName = aPIPostAssessmentResult.staffName;

            await this._db.ProcessResult.AddAsync(postprocessresult);
            await this._db.SaveChangesAsync();

            try
            {
                await this.AddProcessQuestionDetails(aPIPostAssessmentResult.APIPostProcessQuestionDetails, postprocessresult.Id, UserId, OrgCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            string url = _configuration[Configuration.NotificationApi];

            url = url + "/MailForEvaluationResult";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", postprocessresult.Id);
            oJsonObject.Add("OrganizationCode", OrgCode);
            HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

            Response.ResponseObject = postprocessresult;
            Response.StatusCode = 200;
            return Response;
        }

        public async Task<APIPostProcessEvaluationDisplay> LastSubmitedProcessResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode)
        { 
            APIPostProcessEvaluationDisplay _resultdata = null;
            try
            {
                ProcessResult result = null;

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;
                if (aPIPostAssessmentResult.BranchId == "null")
                    aPIPostAssessmentResult.BranchId = null;


                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    result = _db.ProcessResult.Where(x => x.UserId == aPIPostAssessmentResult.UserId.ToString()).OrderByDescending(x => x.Id).FirstOrDefault();
                }
                else
                {
                    result = _db.ProcessResult.Where(x => x.BranchId == Convert.ToInt32(aPIPostAssessmentResult.BranchId)).OrderByDescending(x => x.Id).FirstOrDefault();

                }
                _resultdata = Mapper.Map<APIPostProcessEvaluationDisplay>(result);

                List<ProcessResultDetails> ansdetails = _db.ProcessResultDetails.Where(x => x.EvalResultID == _resultdata.Id).ToList();

                List<int> distquestion = ansdetails.Select(x => x.QuestionID).Distinct().ToList();

                List<APIQuestionDetails> ansdetailslist = new List<APIQuestionDetails>();

                foreach (int item in distquestion)
                {

                    ProcessResultDetails ans = ansdetails.Where(x => x.QuestionID == item).FirstOrDefault();
                    int?[] ansid = ansdetails.Where(x => x.QuestionID == item).Select(x => x.OptionAnswerId).ToArray();
                    APIQuestionDetails ansdetail = new APIQuestionDetails();
                    ansdetail.ImprovementAnswer = ans.ImprovementAnswer;
                    ansdetail.ReferenceQuestionID = ans.QuestionID;
                    ansdetail.SelectedAnswer = ans.SelectedAnswer;
                    ansdetail.OptionAnswerId = ansid;
                    ansdetailslist.Add(ansdetail);
                }
                _resultdata.aPIQuestionDetails = ansdetailslist.OrderBy(x => x.ReferenceQuestionID).ToArray();
                _resultdata.ProcessManagement = _db.ProcessEvaluationManagement.Where(x => x.Id == result.ManagementId).Select(x => x.Title).FirstOrDefault();

                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = "username";
                int Value = result.CreatedBy;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnName + "/" + Value);
                xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                if (response.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                }

                string ColumnNamesupervisor = "username";
                int Valuesupervisor = result.CreatedBy;
                HttpResponseMessage responsesup = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnNamesupervisor + "/" + Value);
                xAPIUserDetails _xAPIUserDetailssup = new xAPIUserDetails();
                if (responsesup.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetailssup = JsonConvert.DeserializeObject<xAPIUserDetails>(username);

                }
                _resultdata.supervisorName = _xAPIUserDetailssup.Name;
                _resultdata.EvaluationByUserId = _xAPIUserDetails.Name;
                _resultdata.EvaluationDate = result.CreatedDate.ToString();
                return _resultdata;
            }
            catch (Exception ex)
            { _logger.Error(Utilities.GetDetailedException(ex));
            }
            return _resultdata;
        }

        public async Task<APIPostProcessEvaluationDisplay> LastSubmitedProcessResultKitchenAudit(APILastSubmitedResultKitchenAudit aPIPostAssessmentResult, string OrganisationCode)
        {
            APIPostProcessEvaluationDisplay _resultdata = null;
            try
            {
                KitchenAuditResult result = null;

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;
                if (aPIPostAssessmentResult.BranchName == "null")
                    aPIPostAssessmentResult.BranchName = null;


                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    result = _db.KitchenAuditResult.Where(x => x.UserId == aPIPostAssessmentResult.UserId.ToString()).OrderByDescending(x => x.Id).FirstOrDefault();
                }
                else
                {
                    result = _db.KitchenAuditResult.Where(x => x.BranchName == aPIPostAssessmentResult.BranchName).OrderByDescending(x => x.Id).FirstOrDefault();

                }
               
                 _resultdata = Mapper.Map<APIPostProcessEvaluationDisplay>(result);

                _resultdata.supervisorName = result.supervisorId;

                List<KitchenAuditResultDetails> ansdetails = _db.KitchenAuditResultDetails.Where(x => x.EvalResultID == _resultdata.Id).ToList();

                List<int> distquestion = ansdetails.Select(x => x.QuestionID).Distinct().ToList();

                List<APIQuestionDetails> ansdetailslist = new List<APIQuestionDetails>();

                foreach (int item in distquestion)
                {

                    KitchenAuditResultDetails ans = ansdetails.Where(x => x.QuestionID == item).FirstOrDefault();
                    int?[] ansid = ansdetails.Where(x => x.QuestionID == item).Select(x => x.OptionAnswerId).ToArray();
                    APIQuestionDetails ansdetail = new APIQuestionDetails();
                    ansdetail.ImprovementAnswer = ans.ImprovementAnswer;
                    ansdetail.ReferenceQuestionID = ans.QuestionID;
                    ansdetail.SelectedAnswer = ans.SelectedAnswer;
                    ansdetail.OptionAnswerId = ansid;
                    ansdetailslist.Add(ansdetail);
                }
                _resultdata.aPIQuestionDetails = ansdetailslist.OrderBy(x => x.ReferenceQuestionID).ToArray();
                _resultdata.ProcessManagement = _db.ProcessEvaluationManagement.Where(x => x.Id == result.ManagementId).Select(x => x.Title).FirstOrDefault();

                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = "username";
                int Value = result.CreatedBy;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnName + "/" + Value);
                xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                if (response.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                }

                string ColumnNamesupervisor = "username";
                int Valuesupervisor = result.CreatedBy;
                HttpResponseMessage responsesup = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnNamesupervisor + "/" + Value);
                xAPIUserDetails _xAPIUserDetailssup = new xAPIUserDetails();
                if (responsesup.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetailssup = JsonConvert.DeserializeObject<xAPIUserDetails>(username);

                }
            //    _resultdata.supervisorName = _xAPIUserDetailssup.Name;
                _resultdata.EvaluationByUserId = _xAPIUserDetails.Name;
                _resultdata.EvaluationDate = result.CreatedDate.ToString();
                return _resultdata;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return _resultdata;
        }
        
        public async Task AddProcessQuestionDetails(APIPostProcessQuestionDetails[] postQuestionDetails, int postResultId, int userId, string OrgCode)
        { 
            List<ProcessResultDetails> objEvaluationQues = new List<ProcessResultDetails>();

            foreach (APIPostProcessQuestionDetails opt in postQuestionDetails)
            {
                int optioncount = opt.OptionAnswerId.Length;
                if (OrgCode.ToLower() == "lenexis")
                {
                    ProcessResultDetails processResultdetails = new ProcessResultDetails();
                    processResultdetails.EvalResultID = postResultId;
                    processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                    processResultdetails.Marks = opt.Marks;
                    if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                    {
                        processResultdetails.OptionAnswerId = opt.OptionAnswerId[0];
                    }
                    if (opt.OptionAnswerId.Length == 0)
                    {
                        processResultdetails.SelectedAnswer = "NA";
                        processResultdetails.ImprovementAnswer = "NA";
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                    }
                    if (opt.files != null)
                    {

                        foreach (string filepath in opt.files)
                        {
                            if (processResultdetails.FilePath1 == null)
                            {
                                processResultdetails.FilePath1 = filepath;
                            }
                            else if (processResultdetails.FilePath2 == null)
                            {
                                processResultdetails.FilePath2 = filepath;
                            }
                            else if (processResultdetails.FilePath3 == null)
                            {
                                processResultdetails.FilePath3 = filepath;
                            }
                            else if (processResultdetails.FilePath4 == null)
                            {
                                processResultdetails.FilePath4 = filepath;
                            }
                            else if (processResultdetails.FilePath5 == null)
                            {
                                processResultdetails.FilePath5 = filepath;
                            }
                            else if (processResultdetails.FilePath6 == null)
                            {
                                processResultdetails.FilePath6 = filepath;
                            }
                            else if (processResultdetails.FilePath7 == null)
                            {
                                processResultdetails.FilePath7 = filepath;
                            }
                            else if (processResultdetails.FilePath8 == null)
                            {
                                processResultdetails.FilePath8 = filepath;
                            }
                            else if (processResultdetails.FilePath9 == null)
                            {
                                processResultdetails.FilePath9 = filepath;
                            }
                            else if (processResultdetails.FilePath10 == null)
                            {
                                processResultdetails.FilePath10 = filepath;
                            }
                            else
                            {
                                processResultdetails.FilePath10 = filepath;
                            }
                        }
                    }
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objEvaluationQues.Add(processResultdetails);
                    }
                }
                else
                {
                   
                    for (int i = 0; i < optioncount; i++)
                    {
                        ProcessResultDetails processResultdetails = new ProcessResultDetails();
                        processResultdetails.EvalResultID = postResultId;
                        processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                        processResultdetails.Marks = opt.Marks;
                        if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                        {
                            processResultdetails.OptionAnswerId = opt.OptionAnswerId[i];
                        }

                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                        if (opt.files != null)
                        {

                            foreach (string filepath in opt.files)
                            {
                                if (processResultdetails.FilePath1 == null)
                                {
                                    processResultdetails.FilePath1 = filepath;
                                }
                                else if (processResultdetails.FilePath2 == null)
                                {
                                    processResultdetails.FilePath2 = filepath;
                                }
                                else if (processResultdetails.FilePath3 == null)
                                {
                                    processResultdetails.FilePath3 = filepath;
                                }
                                else if (processResultdetails.FilePath4 == null)
                                {
                                    processResultdetails.FilePath4 = filepath;
                                }
                                else if (processResultdetails.FilePath5 == null)
                                {
                                    processResultdetails.FilePath5 = filepath;
                                }
                                else
                                {
                                    processResultdetails.FilePath6 = filepath;
                                }
                            }
                        }
                        if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                        {
                            objEvaluationQues.Add(processResultdetails);
                        }
                    }
                }
                if (optioncount == 0 && opt.OptionType.ToLower() == "subjective")
                {
                    ProcessResultDetails processResultdetails = new ProcessResultDetails();
                    processResultdetails.EvalResultID = postResultId;
                    processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                    processResultdetails.Marks = opt.Marks;
                    processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                    processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                    processResultdetails.CreatedBy = userId;
                    processResultdetails.CreatedDate = DateTime.UtcNow;
                    if (opt.files != null)
                    {

                        foreach (string filepath in opt.files)
                        {
                            if (processResultdetails.FilePath1 == null)
                            {
                                processResultdetails.FilePath1 = filepath;
                            }
                            else if (processResultdetails.FilePath2 == null)
                            {
                                processResultdetails.FilePath2 = filepath;
                            }
                            else if (processResultdetails.FilePath3 == null)
                            {
                                processResultdetails.FilePath3 = filepath;
                            }
                            else if (processResultdetails.FilePath4 == null)
                            {
                                processResultdetails.FilePath4 = filepath;
                            }
                            else if (processResultdetails.FilePath5 == null)
                            {
                                processResultdetails.FilePath5 = filepath;
                            }
                            else
                            {
                                processResultdetails.FilePath6 = filepath;
                            }
                        }
                    }
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objEvaluationQues.Add(processResultdetails);
                    }
                }
            }
            _db.ProcessResultDetails.AddRange(objEvaluationQues);
            _db.SaveChanges();
        }

        public async Task AddKitchenAuditQuestionDetails(APIPostProcessQuestionDetails[] postQuestionDetails, int postResultId, int userId)
        {
            List<KitchenAuditResultDetails> objEvaluationQues = new List<KitchenAuditResultDetails>();

            foreach (APIPostProcessQuestionDetails opt in postQuestionDetails)
            {

                    KitchenAuditResultDetails processResultdetails = new KitchenAuditResultDetails();
                    processResultdetails.EvalResultID = postResultId;
                    processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                    processResultdetails.Marks = opt.Marks;
                    
                    if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                    {
                       processResultdetails.OptionAnswerId = opt.OptionAnswerId[0];
                    }
                    else
                    {
                        processResultdetails.OptionAnswerId = null;
                    }

                    processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                    processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                    processResultdetails.CreatedBy = userId;
                    processResultdetails.CreatedDate = DateTime.UtcNow;
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objEvaluationQues.Add(processResultdetails);
                    }
                
               
            }
            _db.KitchenAuditResultDetails.AddRange(objEvaluationQues);
            _db.SaveChanges();
        }

        #region OERProcessResult

        public async Task<ApiResponse> PostOERProcessResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null)
        {
            
            
               ApiResponse Response = new ApiResponse();

                OERProcessResult postprocessresult = new OERProcessResult();

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;


                postprocessresult.ManagementId = aPIPostAssessmentResult.ManagementId;
                postprocessresult.MarksObtained = aPIPostAssessmentResult.obtainedMarks;
                postprocessresult.Percentage = aPIPostAssessmentResult.obtainedPercentage;
                postprocessresult.Result = aPIPostAssessmentResult.Result;
                postprocessresult.TransactionId = aPIPostAssessmentResult.TransactionId;
                postprocessresult.TotalMarks = aPIPostAssessmentResult.TotalMarks;
                postprocessresult.supervisorId = aPIPostAssessmentResult.supervisorId;

                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.OERProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchId= null;
                }
                else
                {
                    int noOfAttempts = _db.OERProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
                postprocessresult.CreatedBy = UserId;
                postprocessresult.CreatedDate = DateTime.UtcNow;
                postprocessresult.ModifiedBy = UserId;
                postprocessresult.ModifiedDate = DateTime.UtcNow;
                postprocessresult.StarRating = aPIPostAssessmentResult.StarRating;
                postprocessresult.CountOfExtremeViolation = aPIPostAssessmentResult.CountOfExtremeViolation;
                postprocessresult.EvaluationDate = aPIPostAssessmentResult.EvaluationDate;
                postprocessresult.AuditType = aPIPostAssessmentResult.AuditType;

                await this._db.OERProcessResult.AddAsync(postprocessresult);
                await this._db.SaveChangesAsync();

                try
                {
                    await this.AddOERProcessQuestionDetails(aPIPostAssessmentResult.APIPostProcessQuestionDetails, postprocessresult.Id, UserId);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                string url = _configuration[Configuration.NotificationApi];

                url = url + "/MailForEvaluationResult";
                JObject oJsonObject = new JObject();
                oJsonObject.Add("CourseId", postprocessresult.Id);
                oJsonObject.Add("OrganizationCode", OrgCode);
                HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

                Response.ResponseObject = postprocessresult;
                Response.StatusCode = 200;
                return Response;
        }
        public async Task AddOERProcessQuestionDetails(APIPostProcessQuestionDetails[] postQuestionDetails, int postResultId, int userId)
        {
            List<OERProcessResultDetails> objEvaluationQues = new List<OERProcessResultDetails>();

            foreach (APIPostProcessQuestionDetails opt in postQuestionDetails)
            {

                OERProcessResultDetails processResultdetails = new OERProcessResultDetails();
                processResultdetails.EvalResultID = postResultId;
                processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                processResultdetails.Marks = opt.Marks;

                if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                {
                    processResultdetails.OptionAnswerId = opt.OptionAnswerId[0];
                }
                else
                {
                    processResultdetails.OptionAnswerId = null;
                }

                processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                processResultdetails.CreatedBy = userId;
                processResultdetails.CreatedDate = DateTime.UtcNow;
                if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                {
                    objEvaluationQues.Add(processResultdetails);
                }


            }
            _db.OERProcessResultDetails.AddRange(objEvaluationQues);
            _db.SaveChanges();
        }
        #endregion
        public async Task<APIPostOERProcessEvaluationDisplay> LastSubmitedOERProcessResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode)
        {
            APIPostOERProcessEvaluationDisplay _resultdata = null;
            try
            {
                OERProcessResult result = null;

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;
                if (aPIPostAssessmentResult.BranchId == "null")
                    aPIPostAssessmentResult.BranchId = null;


                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    result = _db.OERProcessResult.Where(x => x.UserId == aPIPostAssessmentResult.UserId.ToString()).OrderByDescending(x => x.Id).FirstOrDefault();
                }
                else
                {
                    result = _db.OERProcessResult.Where(x => x.BranchId == Convert.ToInt32(aPIPostAssessmentResult.BranchId)).OrderByDescending(x => x.Id).FirstOrDefault();

                }
                _resultdata = Mapper.Map<APIPostOERProcessEvaluationDisplay>(result);

                List<OERProcessResultDetails> ansdetails = _db.OERProcessResultDetails.Where(x => x.EvalResultID == _resultdata.Id).ToList();

                List<int> distquestion = ansdetails.Select(x => x.QuestionID).Distinct().ToList();

                List<APIQuestionDetails> ansdetailslist = new List<APIQuestionDetails>();

                foreach (int item in distquestion)
                {

                    OERProcessResultDetails ans = ansdetails.Where(x => x.QuestionID == item).FirstOrDefault();
                    int?[] ansid = ansdetails.Where(x => x.QuestionID == item).Select(x => x.OptionAnswerId).ToArray();
                    APIQuestionDetails ansdetail = new APIQuestionDetails();
                    ansdetail.ImprovementAnswer = ans.ImprovementAnswer;
                    ansdetail.ReferenceQuestionID = ans.QuestionID;
                    ansdetail.SelectedAnswer = ans.SelectedAnswer;
                    ansdetail.OptionAnswerId = ansid;
                    ansdetailslist.Add(ansdetail);
                }
                _resultdata.aPIQuestionDetails = ansdetailslist.OrderBy(x => x.ReferenceQuestionID).ToArray();
                _resultdata.ProcessManagement = _db.ProcessEvaluationManagement.Where(x => x.Id == result.ManagementId).Select(x => x.Title).FirstOrDefault();

                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = "username";
                int Value = result.CreatedBy;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnName + "/" + Value);
                xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                if (response.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                }

                string ColumnNamesupervisor = "username";
                int Valuesupervisor = result.CreatedBy;
                HttpResponseMessage responsesup = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnNamesupervisor + "/" + Value);
                xAPIUserDetails _xAPIUserDetailssup = new xAPIUserDetails();
                if (responsesup.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetailssup = JsonConvert.DeserializeObject<xAPIUserDetails>(username);

                }
                _resultdata.supervisorName = _xAPIUserDetailssup.Name;
                _resultdata.EvaluationByUserId = _xAPIUserDetails.Name;
                _resultdata.EvaluationDate = result.CreatedDate.ToString();
                return _resultdata;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return _resultdata;
        }



        public async Task PostPMSEvaluationResult(List<APIPMSEvaluationResult> apiPMSEvaluationResult, string OrganisationCode)
        {
            List<PMSEvaluationResult> resultList = new List<PMSEvaluationResult>();

            foreach (APIPMSEvaluationResult apiresult in apiPMSEvaluationResult)
            {
                PMSEvaluationResult result = new PMSEvaluationResult();
                result.SubmittedId = apiresult.SubmittedId;
                result.QuestionId = apiresult.QuestionId;
                result.QuestionType = apiresult.QuestionType;

                if (apiresult.QuestionType == null)
                {
                    result.UserFeedback = null;
                    result.ManagerFeedback = null;
                    result.SubjectiveUserFeedback = null;
                    result.SubjectiveManagerFeedback = null;
                }
                else if (apiresult.QuestionType.ToLower() == "singleselection")
                {
                    result.SubjectiveUserFeedback = null;
                    result.SubjectiveManagerFeedback = null;
                    result.UserFeedback = apiresult.UserFeedback;
                    result.ManagerFeedback = apiresult.ManagerFeedback;
                }
                else
                {
                    result.UserFeedback = null;
                    result.ManagerFeedback = null;
                    result.SubjectiveUserFeedback = apiresult.SubjectiveUserFeedback;
                    result.SubjectiveManagerFeedback = apiresult.SubjectiveManagerFeedback;
                }

                result.QuestionText = apiresult.QuestionText;
                result.Section = apiresult.Section;


                resultList.Add(result);
            }            

            _db.PMSEvaluationResult.AddRange(resultList);
            _db.SaveChanges();
        }

        //GetPendingPMSEvaluation
        public async Task<IEnumerable<PendingPMSEvaluation>> GetPendingPMSEvaluation(int userId)
        {
            List<PendingPMSEvaluation> pendingList = new List<PendingPMSEvaluation>();
            var pendingPMSevals = new List<PMSEvaluationSubmit>();
            pendingPMSevals = await (from submits in this._db.PMSEvaluationSubmit
                                   where submits.Userid == userId 
                                   select new PMSEvaluationSubmit
                                   { 
                                        Id = submits.Id,
                                        Userid = submits.Userid,
                                        ManagerId = submits.ManagerId,
                                        Date = submits.Date,
                                        Status = submits.Status
                                   }).OrderBy(c => c.Id).ToListAsync();

            foreach (PMSEvaluationSubmit obj in pendingPMSevals)
            {
                PendingPMSEvaluation newObj = new PendingPMSEvaluation();
                newObj.Id = obj.Id;
                newObj.UserId = obj.Userid;
                newObj.ManagerId = obj.ManagerId;
                newObj.Date = obj.Date;
                newObj.Status = obj.Status;
                newObj.Username = GetUserNamebyUserId(obj.Userid);

                pendingList.Add(newObj);

            }

            return pendingList;


        }

        public string GetUserNamebyUserId(int uId) 
        {
            string username = (from user in this._db.UserMaster
                         where user.Id == uId && user.IsDeleted == false
                         select user.UserName).First().ToString();

            return username;
        }

        public async Task<IEnumerable<PMSEvaluationResultPointResponse>> GetPendingPMSEvaluationById(int id)
        {


            var resultList = new List<PMSEvaluationResult>();
            var Points = new List<PMSEvaluationPoint>();

            resultList = await (from ques in this._db.PMSEvaluationResult
                            orderby ques.Id descending
                            where ques.SubmittedId == id
                            select new PMSEvaluationResult
                            {
                                Id = ques.Id,
                                SubmittedId = ques.SubmittedId,
                                QuestionId = ques.QuestionId,
                                QuestionType = ques.QuestionType,
                                QuestionText = ques.QuestionText,
                                UserFeedback = ques.UserFeedback,
                                SubjectiveUserFeedback = ques.SubjectiveUserFeedback,
                                ManagerFeedback = ques.ManagerFeedback,
                                SubjectiveManagerFeedback = ques.SubjectiveManagerFeedback,
                                Section = ques.Section

                            }).OrderBy(c => c.Id).ToListAsync();

            string band = resultList[1].Section;

            List<PMSObsAndMeasureByBand> quesDet = GetObsAndMeasureByBand(band);

            Points = await (from points in this._db.PMSEvaluationPoints
                            orderby points.Id descending
                            where points.Section == band
                            select new PMSEvaluationPoint
                            {
                                Id = points.Id,
                                PointText = points.PointText,
                                Section = points.Section,
                                RefQuestionId = points.RefQuestionId,
                            }
                ).OrderBy(c => c.Id).ToListAsync();



            List<PMSEvaluationResultPointResponse> finalResponse = new List<PMSEvaluationResultPointResponse>();
            int count = 0;
            foreach (PMSEvaluationResult item in resultList)
            {
                PMSEvaluationResultPointResponse obj = new PMSEvaluationResultPointResponse();
                
                obj.Id = item.Id;
                obj.SubmittedId = item.SubmittedId;
                obj.QuestionId = item.QuestionId;
                obj.QuestionType = item.QuestionType;
                obj.QuestionText = item.QuestionText;
                obj.UserFeedback = item.UserFeedback;
                obj.SubjectiveUserFeedback = item.SubjectiveUserFeedback;
                obj.ManagerFeedback = item.ManagerFeedback;
                obj.SubjectiveManagerFeedback = item.SubjectiveManagerFeedback;
                obj.Section = item.Section;

                obj.ObservableBehaviorCompetency = quesDet[count].ObservableBehaviorCompetency;
                obj.DefinitionOfTheMeasure = quesDet[count].DefinitionOfTheMeasure;

                List<string> temp = new List<string>();
                foreach (var i in Points)
                {
                    if (item.QuestionId == i.RefQuestionId)
                    {
                        temp.Add(i.PointText);
                    }
                }
                obj.PointsText = temp;
                count++;
                finalResponse.Add(obj);
            }


            
            return finalResponse.ToList().OrderBy(c => c.Id);
        }

        public async Task PostPMSEvaluationResultManager(List<APIPMSEvaluationResultManager> apiPMSEvaluationResult)
        {
            //List<PMSEvaluationResult> resultList = new List<PMSEvaluationResult>();

            foreach (APIPMSEvaluationResultManager apiresult in apiPMSEvaluationResult)
            {
                //PMSEvaluationResult result = new PMSEvaluationResult();

                PMSEvaluationResult updateObj = GetPMSResultById(apiresult.Id);
                if (updateObj!=null)
                {
                    if (apiresult.QuestionType == null)
                    {
                        updateObj.UserFeedback = null;
                        updateObj.ManagerFeedback = apiresult.ManagerFeedback;
                        updateObj.SubjectiveUserFeedback = null;
                        updateObj.SubjectiveManagerFeedback = apiresult.SubjectiveManagerFeedback;
                    }
                    else if (apiresult.QuestionType.ToLower() == "singleselection")
                    {
                        updateObj.SubjectiveUserFeedback = null;
                        updateObj.SubjectiveManagerFeedback = null;
                        updateObj.UserFeedback = apiresult.UserFeedback;
                        updateObj.ManagerFeedback = apiresult.ManagerFeedback;
                    }
                    else
                    {
                        updateObj.UserFeedback = null;
                        updateObj.ManagerFeedback = null;
                        updateObj.SubjectiveUserFeedback = apiresult.SubjectiveUserFeedback;
                        updateObj.SubjectiveManagerFeedback = apiresult.SubjectiveManagerFeedback;
                    }

                }



                this._db.PMSEvaluationResult.Update(updateObj);
                //resultList.Add(result);
            }

            _db.SaveChanges();

            PMSEvaluationSubmit submitRow= GetPMSSubmittedById(apiPMSEvaluationResult[0].SubmittedId);
            if (submitRow!= null)
            {
                submitRow.Status = "Completed";
            }
            this._db.PMSEvaluationSubmit.Update(submitRow);
            _db.SaveChanges();
            
        }

        public PMSEvaluationResult GetPMSResultById(int uId)
        {
            PMSEvaluationResult res = (from pms in this._db.PMSEvaluationResult
                               where pms.Id == uId
                               select pms).FirstOrDefault();

            return res;
        }

        public PMSEvaluationSubmit GetPMSSubmittedById(int uId)
        {
            PMSEvaluationSubmit res = (from pms in this._db.PMSEvaluationSubmit
                                       where pms.Id == uId
                                       select pms).FirstOrDefault();

            return res;
        }

        public List<PMSObsAndMeasureByBand> GetObsAndMeasureByBand(string band)
        {
            List<PMSObsAndMeasureByBand> res = new List<PMSObsAndMeasureByBand>();


            res = (from entry in this._db.PMSEvaluationQuestion
                        where entry.Section == band
                        select new PMSObsAndMeasureByBand
                        {
                            Id = entry.Id,
                            ObservableBehaviorCompetency= entry.ObservableBehaviorCompetency,
                            DefinitionOfTheMeasure = entry.DefinitionOfTheMeasure
                        }).ToList();

            return res;
        }

        public async Task<bool> Exist(int UserId)
        {
            int Count = 0;

            Count = await (from c in _db.PMSEvaluationSubmit
                           where c.Userid == UserId
                           select new
                           { c.Id }).CountAsync();

            if (Count > 0)
                return true;
            return false;
        }

        #region Critical Audit Evaluation

        public async Task<IEnumerable<APICriticalAuditQuestion>> GetQuestionForCriticalAuditEvaluation()
        {
            var ProcessQuestions = new List<APICriticalAuditQuestion>();
            var ProcessQuestions_single = new List<APICriticalAuditQuestion>();

            ProcessQuestions_single = await (from ques in this._db.CriticalAuditQuestion
                                             orderby ques.Id descending
                                             where ques.IsDeleted == false && ques.OptionType == "SingleSelection"
                                             select new APICriticalAuditQuestion
                                             {
                                                 Id = ques.Id,
                                                 OptionType = ques.OptionType,
                                                 Marks = ques.Marks,
                                                 Status = ques.Status,
                                                 QuestionText = ques.QuestionText,
                                                 Section = ques.Section,
                                                 Category = ques.Category,
                                                 AllowNA = ques.AllowNA,
                                                 IsRequired = ques.IsRequired,
                                                 IsSubquestion = ques.IsSubquestion,
                                                 AllowTextReply = ques.AllowTextReply,
                                                 OptionCount = _db.CriticalAuditOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                             }).OrderBy(c => c.Id).ToListAsync();

            var selectedIds_single = ProcessQuestions_single.Select(x => x.Id);

            var option_single_result = _db.CriticalAuditOption.Where(x => x.IsDeleted == false && selectedIds_single.Contains(x.QuestionID))
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.QuestionID,
                    ProcessQuestionOptionID = x.Id,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsCorrectAnswer,
                    RefQuestionID = x.RefQuestionID
                }).ToList();

            var options_single = option_single_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = (decimal)ProcessQuestions_single.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault()
                }).ToList();

            //Assign options for each question 
            foreach (APICriticalAuditQuestion Question in ProcessQuestions_single)
            {
                int correctoptioncount = 0;
                correctoptioncount = options_single.Where(x => x.ProcessQuestionID == Question.Id && x.IsAnswer == true).Count();
                if (correctoptioncount > 1)
                {
                    Question.Allcorrect = true;
                }
                else
                {
                    Question.Allcorrect = false;
                }

                if (Question.Section == "Extreme Violation")
                {
                    APIPEQuestionOption[] aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                    foreach (APIPEQuestionOption opt in aPIOptions)
                    {
                        if (opt.OptionText.ToLower() == "yes")
                        {
                            opt.DevidedMarks = 0;
                        }
                        else if (opt.OptionText.ToLower() == "no")
                        {
                            opt.DevidedMarks = -10;
                        }
                    }
                    Question.aPIOptions = aPIOptions;
                }
                else
                {
                    Question.aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                }
            }
            
            ProcessQuestions = ProcessQuestions_single.ToList();
            return ProcessQuestions.OrderBy(x => x.Id);
           
        }

        public async Task<ApiResponse> PostProcessCriticalAuditResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null)
        {
            ApiResponse Response = new ApiResponse();

            CriticalAuditProcessResult postprocessresult = new CriticalAuditProcessResult();

            if (aPIPostAssessmentResult.UserId == "null")
                aPIPostAssessmentResult.UserId = null;


            postprocessresult.ManagementId = aPIPostAssessmentResult.ManagementId;
            postprocessresult.MarksObtained = aPIPostAssessmentResult.obtainedMarks;
            postprocessresult.Percentage = aPIPostAssessmentResult.obtainedPercentage;
            postprocessresult.Result = aPIPostAssessmentResult.Result;
            postprocessresult.TransactionId = aPIPostAssessmentResult.TransactionId;
            postprocessresult.TotalMarks = aPIPostAssessmentResult.TotalMarks;
            postprocessresult.supervisorId = aPIPostAssessmentResult.supervisorId;

            if (OrgCode == "lenexis" || OrgCode == "ent")
            {
                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.CriticalAuditProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
                else
                {
                    int noOfAttempts = _db.CriticalAuditProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.CriticalAuditProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchId = null;
                }
                else
                {
                    int noOfAttempts = _db.CriticalAuditProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
            }

            postprocessresult.CreatedBy = UserId;
            postprocessresult.CreatedDate = DateTime.UtcNow;
            postprocessresult.ModifiedBy = UserId;
            postprocessresult.ModifiedDate = DateTime.UtcNow;
            postprocessresult.StarRating = aPIPostAssessmentResult.StarRating;
            postprocessresult.CountOfExtremeViolation = aPIPostAssessmentResult.CountOfExtremeViolation;
            postprocessresult.EvaluationDate = aPIPostAssessmentResult.EvaluationDate;
            postprocessresult.AuditType = aPIPostAssessmentResult.AuditType;
            postprocessresult.AuditorName = null;
            postprocessresult.RegionName = null;
            postprocessresult.SiteName = null;
            postprocessresult.StaffName = null;
            postprocessresult.RestaurantManagerID = Convert.ToInt32(aPIPostAssessmentResult.RestaurantManagerID);

            await this._db.CriticalAuditProcessResult.AddAsync(postprocessresult);
            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            try
            {
                await this.AddCriticalAuditProcessQuestionDetails(aPIPostAssessmentResult.APIPostProcessQuestionDetails, postprocessresult.Id, UserId, OrgCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            string url = _configuration[Configuration.NotificationApi];

            url = url + "/MailForEvaluationResult";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", postprocessresult.Id);
            oJsonObject.Add("OrganizationCode", OrgCode);
            HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

            Response.ResponseObject = postprocessresult;
            Response.StatusCode = 200;
            return Response;
        }

        public async Task AddCriticalAuditProcessQuestionDetails(APIPostProcessQuestionDetails[] postQuestionDetails, int postResultId, int userId, string OrgCode)
        {
            List<CriticalAuditProcessResultDetails> objEvaluationQues = new List<CriticalAuditProcessResultDetails>();

            foreach (APIPostProcessQuestionDetails opt in postQuestionDetails)
            {
                int optioncount = opt.OptionAnswerId.Length;
                if (OrgCode.ToLower() == "lenexis")
                {
                    CriticalAuditProcessResultDetails processResultdetails = new CriticalAuditProcessResultDetails();
                    processResultdetails.EvalResultID = postResultId;
                    processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                    
                    if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                    {
                        processResultdetails.Marks = opt.Marks;
                        processResultdetails.OptionAnswerId = opt.OptionAnswerId[0];
                    }
                    if (opt.OptionAnswerId.Length == 0)
                    {
                        processResultdetails.SelectedAnswer = "NA";
                        processResultdetails.ImprovementAnswer = "NA";
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                        processResultdetails.Marks = 0;
                    }
                    else
                    {
                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                        processResultdetails.Marks = opt.Marks;
                    }
                    if (opt.files != null)
                    {

                        foreach (string filepath in opt.files)
                        {
                            if (processResultdetails.FilePath1 == null)
                            {
                                processResultdetails.FilePath1 = filepath;
                            }
                            else if (processResultdetails.FilePath2 == null)
                            {
                                processResultdetails.FilePath2 = filepath;
                            }
                            else if (processResultdetails.FilePath3 == null)
                            {
                                processResultdetails.FilePath3 = filepath;
                            }
                            else if (processResultdetails.FilePath4 == null)
                            {
                                processResultdetails.FilePath4 = filepath;
                            }
                            else if (processResultdetails.FilePath5 == null)
                            {
                                processResultdetails.FilePath5 = filepath;
                            }
                            else
                            {
                                processResultdetails.FilePath6 = filepath;
                            }
                        }
                    }
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objEvaluationQues.Add(processResultdetails);
                    }
                }
                else
                {

                    for (int i = 0; i < optioncount; i++)
                    {
                        CriticalAuditProcessResultDetails processResultdetails = new CriticalAuditProcessResultDetails();
                        processResultdetails.EvalResultID = postResultId;
                        processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                        processResultdetails.Marks = opt.Marks;
                        if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                        {
                            processResultdetails.OptionAnswerId = opt.OptionAnswerId[i];
                        }

                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                        if (opt.files != null)
                        {

                            foreach (string filepath in opt.files)
                            {
                                if (processResultdetails.FilePath1 == null)
                                {
                                    processResultdetails.FilePath1 = filepath;
                                }
                                else if (processResultdetails.FilePath2 == null)
                                {
                                    processResultdetails.FilePath2 = filepath;
                                }
                                else if (processResultdetails.FilePath3 == null)
                                {
                                    processResultdetails.FilePath3 = filepath;
                                }
                                else if (processResultdetails.FilePath4 == null)
                                {
                                    processResultdetails.FilePath4 = filepath;
                                }
                                else if (processResultdetails.FilePath5 == null)
                                {
                                    processResultdetails.FilePath5 = filepath;
                                }
                                else
                                {
                                    processResultdetails.FilePath6 = filepath;
                                }
                            }
                        }
                        if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                        {
                            objEvaluationQues.Add(processResultdetails);
                        }
                    }
                }
                
            }
            _db.CriticalAuditProcessResultDetails.AddRange(objEvaluationQues);
            _db.SaveChanges();
        }

        public async Task<APIPostProcessEvaluationDisplay> LastSubmittedCriticalAuditResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode)
        {
            APIPostProcessEvaluationDisplay _resultdata = null;
            try
            {
                CriticalAuditProcessResult result = null;

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;
                if (aPIPostAssessmentResult.BranchId == "null")
                    aPIPostAssessmentResult.BranchId = null;


                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    result = _db.CriticalAuditProcessResult.Where(x => x.UserId == aPIPostAssessmentResult.UserId.ToString()).OrderByDescending(x => x.Id).FirstOrDefault();
                }
                else
                {
                    result = _db.CriticalAuditProcessResult.Where(x => x.BranchId == Convert.ToInt32(aPIPostAssessmentResult.BranchId)).OrderByDescending(x => x.Id).FirstOrDefault();

                }
                _resultdata = Mapper.Map<APIPostProcessEvaluationDisplay>(result);

                List<CriticalAuditProcessResultDetails> ansdetails = _db.CriticalAuditProcessResultDetails.Where(x => x.EvalResultID == _resultdata.Id).ToList();

                List<int> distquestion = ansdetails.Select(x => x.QuestionID).Distinct().ToList();

                List<APIQuestionDetails> ansdetailslist = new List<APIQuestionDetails>();

                foreach (int item in distquestion)
                {

                    CriticalAuditProcessResultDetails ans = ansdetails.Where(x => x.QuestionID == item).FirstOrDefault();
                    int?[] ansid = ansdetails.Where(x => x.QuestionID == item).Select(x => x.OptionAnswerId).ToArray();
                    APIQuestionDetails ansdetail = new APIQuestionDetails();
                    ansdetail.ImprovementAnswer = ans.ImprovementAnswer;
                    ansdetail.ReferenceQuestionID = ans.QuestionID;
                    ansdetail.SelectedAnswer = ans.SelectedAnswer;
                    ansdetail.OptionAnswerId = ansid;
                    ansdetailslist.Add(ansdetail);
                }
                _resultdata.aPIQuestionDetails = ansdetailslist.OrderBy(x => x.ReferenceQuestionID).ToArray();
                _resultdata.ProcessManagement = _db.ProcessEvaluationManagement.Where(x => x.Id == result.ManagementId).Select(x => x.Title).FirstOrDefault();

                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = "username";
                int Value = result.CreatedBy;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnName + "/" + Value);
                xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                if (response.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                }

                string ColumnNamesupervisor = "username";
                int Valuesupervisor = result.CreatedBy;
                HttpResponseMessage responsesup = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnNamesupervisor + "/" + Value);
                xAPIUserDetails _xAPIUserDetailssup = new xAPIUserDetails();
                if (responsesup.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetailssup = JsonConvert.DeserializeObject<xAPIUserDetails>(username);

                }
                _resultdata.supervisorName = _xAPIUserDetailssup.Name;
                _resultdata.EvaluationByUserId = _xAPIUserDetails.Name;
                _resultdata.EvaluationDate = result.CreatedDate.ToString();
                return _resultdata;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return _resultdata;
        }

        #endregion


        #region Night Audit Evaluation

        public async Task<IEnumerable<APINightAuditQuestion>> GetQuestionForNightAuditEvaluation()
        {
            var ProcessQuestions = new List<APINightAuditQuestion>();
            var ProcessQuestions_single = new List<APINightAuditQuestion>();

            ProcessQuestions_single = await (from ques in this._db.NightAuditQuestion
                                             orderby ques.Id descending
                                             where ques.IsDeleted == false && ques.OptionType == "SingleSelection"
                                             select new APINightAuditQuestion
                                             {
                                                 Id = ques.Id,
                                                 OptionType = ques.OptionType,
                                                 Marks = ques.Marks,
                                                 Status = ques.Status,
                                                 QuestionText = ques.QuestionText,
                                                 Section = ques.Section,
                                                 Category = ques.Category,
                                                 AllowNA = ques.AllowNA,
                                                 IsRequired = ques.IsRequired,
                                                 IsSubquestion = ques.IsSubquestion,
                                                 AllowTextReply = ques.AllowTextReply,
                                                 OptionCount = _db.NightAuditOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                             }).OrderBy(c => c.Id).ToListAsync();

            var selectedIds_single = ProcessQuestions_single.Select(x => x.Id);

            var option_single_result = _db.NightAuditOption.Where(x => x.IsDeleted == false && selectedIds_single.Contains(x.QuestionID))
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.QuestionID,
                    ProcessQuestionOptionID = x.Id,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsCorrectAnswer,
                    RefQuestionID = x.RefQuestionID
                }).ToList();

            var options_single = option_single_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = (decimal)ProcessQuestions_single.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault()
                }).ToList();

            //Assign options for each question 
            foreach (APINightAuditQuestion Question in ProcessQuestions_single)
            {
                int correctoptioncount = 0;
                correctoptioncount = options_single.Where(x => x.ProcessQuestionID == Question.Id && x.IsAnswer == true).Count();
                if (correctoptioncount > 1)
                {
                    Question.Allcorrect = true;
                }
                else
                {
                    Question.Allcorrect = false;
                }

                if (Question.Section == "Extreme Violation")
                {
                    APIPEQuestionOption[] aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                    foreach (APIPEQuestionOption opt in aPIOptions)
                    {
                        if (opt.OptionText.ToLower() == "yes")
                        {
                            opt.DevidedMarks = 0;
                        }
                        else if (opt.OptionText.ToLower() == "no")
                        {
                            opt.DevidedMarks = -10;
                        }
                    }
                    Question.aPIOptions = aPIOptions;
                }
                else
                {
                    Question.aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                }
            }

            ProcessQuestions = ProcessQuestions_single.ToList();
            return ProcessQuestions.OrderBy(x => x.Id);

        }

        public async Task<ApiResponse> PostProcessNightAuditResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null)
        {
            ApiResponse Response = new ApiResponse();

            NightAuditProcessResult postprocessresult = new NightAuditProcessResult();

            if (aPIPostAssessmentResult.UserId == "null")
                aPIPostAssessmentResult.UserId = null;


            postprocessresult.ManagementId = aPIPostAssessmentResult.ManagementId;
            postprocessresult.MarksObtained = aPIPostAssessmentResult.obtainedMarks;
            postprocessresult.Percentage = aPIPostAssessmentResult.obtainedPercentage;
            postprocessresult.Result = aPIPostAssessmentResult.Result;
            postprocessresult.TransactionId = aPIPostAssessmentResult.TransactionId;
            postprocessresult.TotalMarks = aPIPostAssessmentResult.TotalMarks;
            postprocessresult.supervisorId = aPIPostAssessmentResult.supervisorId;

            if (OrgCode == "lenexis" || OrgCode == "ent")
            {
                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.NightAuditProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
                else
                {
                    int noOfAttempts = _db.NightAuditProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.NightAuditProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchId = null;
                }
                else
                {
                    int noOfAttempts = _db.NightAuditProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
            }

            postprocessresult.CreatedBy = UserId;
            postprocessresult.CreatedDate = DateTime.UtcNow;
            postprocessresult.ModifiedBy = UserId;
            postprocessresult.ModifiedDate = DateTime.UtcNow;
            postprocessresult.StarRating = aPIPostAssessmentResult.StarRating;
            postprocessresult.CountOfExtremeViolation = aPIPostAssessmentResult.CountOfExtremeViolation;
            postprocessresult.EvaluationDate = aPIPostAssessmentResult.EvaluationDate;
            postprocessresult.AuditType = aPIPostAssessmentResult.AuditType;
            postprocessresult.AuditorName = null;
            postprocessresult.RegionName = null;
            postprocessresult.SiteName = null;
            postprocessresult.StaffName = null;
            postprocessresult.RestaurantManagerID = Convert.ToInt32(aPIPostAssessmentResult.RestaurantManagerID);

            await this._db.NightAuditProcessResult.AddAsync(postprocessresult);
            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            try
            {
                await this.AddNightAuditProcessQuestionDetails(aPIPostAssessmentResult.APIPostProcessQuestionDetails, postprocessresult.Id, UserId, OrgCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            string url = _configuration[Configuration.NotificationApi];

            url = url + "/MailForEvaluationResult";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", postprocessresult.Id);
            oJsonObject.Add("OrganizationCode", OrgCode);
            HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

            Response.ResponseObject = postprocessresult;
            Response.StatusCode = 200;
            return Response;
        }

        public async Task AddNightAuditProcessQuestionDetails(APIPostProcessQuestionDetails[] postQuestionDetails, int postResultId, int userId, string OrgCode)
        {
            List<NightAuditProcessResultDetails> objEvaluationQues = new List<NightAuditProcessResultDetails>();

            foreach (APIPostProcessQuestionDetails opt in postQuestionDetails)
            {
                int optioncount = opt.OptionAnswerId.Length;
                if (OrgCode.ToLower() == "lenexis")
                {
                    NightAuditProcessResultDetails processResultdetails = new NightAuditProcessResultDetails();
                    processResultdetails.EvalResultID = postResultId;
                    processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                    processResultdetails.Marks = opt.Marks;
                    if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                    {
                        processResultdetails.OptionAnswerId = opt.OptionAnswerId[0];
                    }
                    if (opt.OptionAnswerId.Length == 0)
                    {
                        processResultdetails.SelectedAnswer = "NA";
                        processResultdetails.ImprovementAnswer = "NA";
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                    }
                    if (opt.files != null)
                    {

                        foreach (string filepath in opt.files)
                        {
                            if (processResultdetails.FilePath1 == null)
                            {
                                processResultdetails.FilePath1 = filepath;
                            }
                            else if (processResultdetails.FilePath2 == null)
                            {
                                processResultdetails.FilePath2 = filepath;
                            }
                            else if (processResultdetails.FilePath3 == null)
                            {
                                processResultdetails.FilePath3 = filepath;
                            }
                            else if (processResultdetails.FilePath4 == null)
                            {
                                processResultdetails.FilePath4 = filepath;
                            }
                            else if (processResultdetails.FilePath5 == null)
                            {
                                processResultdetails.FilePath5 = filepath;
                            }
                            else
                            {
                                processResultdetails.FilePath6 = filepath;
                            }
                        }
                    }
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objEvaluationQues.Add(processResultdetails);
                    }
                }
                else
                {

                    for (int i = 0; i < optioncount; i++)
                    {
                        NightAuditProcessResultDetails processResultdetails = new NightAuditProcessResultDetails();
                        processResultdetails.EvalResultID = postResultId;
                        processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                        processResultdetails.Marks = opt.Marks;
                        if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                        {
                            processResultdetails.OptionAnswerId = opt.OptionAnswerId[i];
                        }

                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                        if (opt.files != null)
                        {

                            foreach (string filepath in opt.files)
                            {
                                if (processResultdetails.FilePath1 == null)
                                {
                                    processResultdetails.FilePath1 = filepath;
                                }
                                else if (processResultdetails.FilePath2 == null)
                                {
                                    processResultdetails.FilePath2 = filepath;
                                }
                                else if (processResultdetails.FilePath3 == null)
                                {
                                    processResultdetails.FilePath3 = filepath;
                                }
                                else if (processResultdetails.FilePath4 == null)
                                {
                                    processResultdetails.FilePath4 = filepath;
                                }
                                else if (processResultdetails.FilePath5 == null)
                                {
                                    processResultdetails.FilePath5 = filepath;
                                }
                                else
                                {
                                    processResultdetails.FilePath6 = filepath;
                                }
                            }
                        }
                        if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                        {
                            objEvaluationQues.Add(processResultdetails);
                        }
                    }
                }

            }
            _db.NightAuditProcessResultDetails.AddRange(objEvaluationQues);
            _db.SaveChanges();
        }

        public async Task<APIPostProcessEvaluationDisplay> LastSubmittedNightAuditResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode)
        {
            APIPostProcessEvaluationDisplay _resultdata = null;
            try
            {
                NightAuditProcessResult result = null;

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;
                if (aPIPostAssessmentResult.BranchId == "null")
                    aPIPostAssessmentResult.BranchId = null;


                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    result = _db.NightAuditProcessResult.Where(x => x.UserId == aPIPostAssessmentResult.UserId.ToString()).OrderByDescending(x => x.Id).FirstOrDefault();
                }
                else
                {
                    result = _db.NightAuditProcessResult.Where(x => x.BranchId == Convert.ToInt32(aPIPostAssessmentResult.BranchId)).OrderByDescending(x => x.Id).FirstOrDefault();

                }
                _resultdata = Mapper.Map<APIPostProcessEvaluationDisplay>(result);

                List<NightAuditProcessResultDetails> ansdetails = _db.NightAuditProcessResultDetails.Where(x => x.EvalResultID == _resultdata.Id).ToList();

                List<int> distquestion = ansdetails.Select(x => x.QuestionID).Distinct().ToList();

                List<APIQuestionDetails> ansdetailslist = new List<APIQuestionDetails>();

                foreach (int item in distquestion)
                {

                    NightAuditProcessResultDetails ans = ansdetails.Where(x => x.QuestionID == item).FirstOrDefault();
                    int?[] ansid = ansdetails.Where(x => x.QuestionID == item).Select(x => x.OptionAnswerId).ToArray();
                    APIQuestionDetails ansdetail = new APIQuestionDetails();
                    ansdetail.ImprovementAnswer = ans.ImprovementAnswer;
                    ansdetail.ReferenceQuestionID = ans.QuestionID;
                    ansdetail.SelectedAnswer = ans.SelectedAnswer;
                    ansdetail.OptionAnswerId = ansid;
                    ansdetailslist.Add(ansdetail);
                }
                _resultdata.aPIQuestionDetails = ansdetailslist.OrderBy(x => x.ReferenceQuestionID).ToArray();
                _resultdata.ProcessManagement = _db.ProcessEvaluationManagement.Where(x => x.Id == result.ManagementId).Select(x => x.Title).FirstOrDefault();

                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = "username";
                int Value = result.CreatedBy;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnName + "/" + Value);
                xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                if (response.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                }

                string ColumnNamesupervisor = "username";
                int Valuesupervisor = result.CreatedBy;
                HttpResponseMessage responsesup = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnNamesupervisor + "/" + Value);
                xAPIUserDetails _xAPIUserDetailssup = new xAPIUserDetails();
                if (responsesup.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetailssup = JsonConvert.DeserializeObject<xAPIUserDetails>(username);

                }
                _resultdata.supervisorName = _xAPIUserDetailssup.Name;
                _resultdata.EvaluationByUserId = _xAPIUserDetails.Name;
                _resultdata.EvaluationDate = result.CreatedDate.ToString();
                return _resultdata;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return _resultdata;
        }


        #endregion

        #region Ops Audit Evaluation

        public async Task<IEnumerable<APIOpsAuditQuestion>> GetQuestionForOpsAuditEvaluation()
        {
            var ProcessQuestions = new List<APIOpsAuditQuestion>();
            var ProcessQuestions_single = new List<APIOpsAuditQuestion>();

            ProcessQuestions_single = await (from ques in this._db.OpsAuditQuestion
                                             orderby ques.Id descending
                                             where ques.IsDeleted == false && ques.OptionType == "SingleSelection"
                                             select new APIOpsAuditQuestion
                                             {
                                                 Id = ques.Id,
                                                 OptionType = ques.OptionType,
                                                 Marks = ques.Marks,
                                                 Status = ques.Status,
                                                 QuestionText = ques.QuestionText,
                                                 Section = ques.Section,
                                                 Category = ques.Category,
                                                 AllowNA = ques.AllowNA,
                                                 IsRequired = ques.IsRequired,
                                                 IsSubquestion = ques.IsSubquestion,
                                                 AllowTextReply = ques.AllowTextReply,
                                                 OptionCount = _db.NightAuditOption.Where(x => x.QuestionID == ques.Id).Select(x => x.Id).Count()
                                             }).OrderBy(c => c.Id).ToListAsync();

            var selectedIds_single = ProcessQuestions_single.Select(x => x.Id);

            var option_single_result = _db.OpsAuditOption.Where(x => x.IsDeleted == false && selectedIds_single.Contains(x.QuestionID))
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.QuestionID,
                    ProcessQuestionOptionID = x.Id,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsCorrectAnswer,
                    RefQuestionID = x.RefQuestionID
                }).ToList();

            var options_single = option_single_result
                .Select(x => new APIPEQuestionOption()
                {
                    ProcessQuestionID = x.ProcessQuestionID,
                    ProcessQuestionOptionID = x.ProcessQuestionOptionID,
                    OptionText = x.OptionText,
                    IsAnswer = x.IsAnswer,
                    RefQuestionID = x.RefQuestionID,
                    DevidedMarks = (decimal)ProcessQuestions_single.Where(c => c.Id == x.ProcessQuestionID).Select(c => c.Marks).FirstOrDefault()
                }).ToList();

            //Assign options for each question 
            foreach (APIOpsAuditQuestion Question in ProcessQuestions_single)
            {
                int correctoptioncount = 0;
                correctoptioncount = options_single.Where(x => x.ProcessQuestionID == Question.Id && x.IsAnswer == true).Count();
                if (correctoptioncount > 1)
                {
                    Question.Allcorrect = true;
                }
                else
                {
                    Question.Allcorrect = false;
                }

                if (Question.Section == "Extreme Violation")
                {
                    APIPEQuestionOption[] aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                    foreach (APIPEQuestionOption opt in aPIOptions)
                    {
                        if (opt.OptionText.ToLower() == "yes")
                        {
                            opt.DevidedMarks = 0;
                        }
                        else if (opt.OptionText.ToLower() == "no")
                        {
                            opt.DevidedMarks = -10;
                        }
                    }
                    Question.aPIOptions = aPIOptions;
                }
                else
                {
                    Question.aPIOptions = options_single.Where(x => x.ProcessQuestionID == Question.Id).ToArray();
                }
            }

            ProcessQuestions = ProcessQuestions_single.ToList();
            return ProcessQuestions.OrderBy(x => x.Id);

        }

        public async Task<ApiResponse> PostProcessOpsAuditResult(APIPostProcessEvaluationResult aPIPostAssessmentResult, int UserId, string OrgCode = null)
        {
            ApiResponse Response = new ApiResponse();

            OpsAuditProcessResult postprocessresult = new OpsAuditProcessResult();

            if (aPIPostAssessmentResult.UserId == "null")
                aPIPostAssessmentResult.UserId = null;


            postprocessresult.ManagementId = aPIPostAssessmentResult.ManagementId;
            postprocessresult.MarksObtained = aPIPostAssessmentResult.obtainedMarks;
            postprocessresult.Percentage = aPIPostAssessmentResult.obtainedPercentage;
            postprocessresult.Result = aPIPostAssessmentResult.Result;
            postprocessresult.TransactionId = aPIPostAssessmentResult.TransactionId;
            postprocessresult.TotalMarks = aPIPostAssessmentResult.TotalMarks;
            postprocessresult.supervisorId = aPIPostAssessmentResult.supervisorId;

            if (OrgCode == "lenexis" || OrgCode == "ent")
            {
                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.OpsAuditProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
                else
                {
                    int noOfAttempts = _db.OpsAuditProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    int noOfAttempts = _db.OpsAuditProcessResult.Where(r => r.UserId == aPIPostAssessmentResult.UserId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = aPIPostAssessmentResult.UserId;
                    postprocessresult.BranchId = null;
                }
                else
                {
                    int noOfAttempts = _db.OpsAuditProcessResult.Where(r => r.BranchId == aPIPostAssessmentResult.BranchId && r.ManagementId == aPIPostAssessmentResult.ManagementId).Select(x => x.NoOfAttempts).FirstOrDefault();

                    postprocessresult.NoOfAttempts = noOfAttempts;
                    postprocessresult.UserId = null;
                    postprocessresult.BranchId = aPIPostAssessmentResult.BranchId;
                }
            }

            postprocessresult.CreatedBy = UserId;
            postprocessresult.CreatedDate = DateTime.UtcNow;
            postprocessresult.ModifiedBy = UserId;
            postprocessresult.ModifiedDate = DateTime.UtcNow;
            postprocessresult.StarRating = aPIPostAssessmentResult.StarRating;
            postprocessresult.CountOfExtremeViolation = aPIPostAssessmentResult.CountOfExtremeViolation;
            postprocessresult.EvaluationDate = aPIPostAssessmentResult.EvaluationDate;
            postprocessresult.AuditType = aPIPostAssessmentResult.AuditType;
            postprocessresult.AuditorName = null;
            postprocessresult.RegionName = null;
            postprocessresult.SiteName = null;
            postprocessresult.StaffName = null;
            postprocessresult.RestaurantManagerID = Convert.ToInt32(aPIPostAssessmentResult.RestaurantManagerID);

            await this._db.OpsAuditProcessResult.AddAsync(postprocessresult);
            try
            {
                await this._db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            try
            {
                await this.AddOpsAuditProcessQuestionDetails(aPIPostAssessmentResult.APIPostProcessQuestionDetails, postprocessresult.Id, UserId, OrgCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }

            string url = _configuration[Configuration.NotificationApi];

            url = url + "/MailForEvaluationResult";
            JObject oJsonObject = new JObject();
            oJsonObject.Add("CourseId", postprocessresult.Id);
            oJsonObject.Add("OrganizationCode", OrgCode);
            HttpResponseMessage responses = CallAPI(url, oJsonObject).Result;

            Response.ResponseObject = postprocessresult;
            Response.StatusCode = 200;
            return Response;
        }

        public async Task AddOpsAuditProcessQuestionDetails(APIPostProcessQuestionDetails[] postQuestionDetails, int postResultId, int userId, string OrgCode)
        {
            List<OpsAuditProcessResultDetails> objEvaluationQues = new List<OpsAuditProcessResultDetails>();

            foreach (APIPostProcessQuestionDetails opt in postQuestionDetails)
            {
                int optioncount = opt.OptionAnswerId.Length;
                if (OrgCode.ToLower() == "lenexis")
                {
                    OpsAuditProcessResultDetails processResultdetails = new OpsAuditProcessResultDetails();
                    processResultdetails.EvalResultID = postResultId;
                    processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                    processResultdetails.Marks = opt.Marks;
                    if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                    {
                        processResultdetails.OptionAnswerId = opt.OptionAnswerId[0];
                    }
                    if (opt.OptionAnswerId.Length == 0)
                    {
                        processResultdetails.SelectedAnswer = "NA";
                        processResultdetails.ImprovementAnswer = "NA";
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                    }
                    if (opt.files != null)
                    {

                        foreach (string filepath in opt.files)
                        {
                            if (processResultdetails.FilePath1 == null)
                            {
                                processResultdetails.FilePath1 = filepath;
                            }
                            else if (processResultdetails.FilePath2 == null)
                            {
                                processResultdetails.FilePath2 = filepath;
                            }
                            else if (processResultdetails.FilePath3 == null)
                            {
                                processResultdetails.FilePath3 = filepath;
                            }
                            else if (processResultdetails.FilePath4 == null)
                            {
                                processResultdetails.FilePath4 = filepath;
                            }
                            else if (processResultdetails.FilePath5 == null)
                            {
                                processResultdetails.FilePath5 = filepath;
                            }
                            else
                            {
                                processResultdetails.FilePath6 = filepath;
                            }
                        }
                    }
                    if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                    {
                        objEvaluationQues.Add(processResultdetails);
                    }
                }
                else
                {

                    for (int i = 0; i < optioncount; i++)
                    {
                        OpsAuditProcessResultDetails processResultdetails = new OpsAuditProcessResultDetails();
                        processResultdetails.EvalResultID = postResultId;
                        processResultdetails.QuestionID = opt.ReferenceQuestionID.Value;
                        processResultdetails.Marks = opt.Marks;
                        if (opt.OptionAnswerId != null && opt.OptionAnswerId.Length != 0)
                        {
                            processResultdetails.OptionAnswerId = opt.OptionAnswerId[i];
                        }

                        processResultdetails.SelectedAnswer = opt.SelectedAnswer;
                        processResultdetails.ImprovementAnswer = opt.ImprovementAnswer;
                        processResultdetails.CreatedBy = userId;
                        processResultdetails.CreatedDate = DateTime.UtcNow;
                        if (opt.files != null)
                        {

                            foreach (string filepath in opt.files)
                            {
                                if (processResultdetails.FilePath1 == null)
                                {
                                    processResultdetails.FilePath1 = filepath;
                                }
                                else if (processResultdetails.FilePath2 == null)
                                {
                                    processResultdetails.FilePath2 = filepath;
                                }
                                else if (processResultdetails.FilePath3 == null)
                                {
                                    processResultdetails.FilePath3 = filepath;
                                }
                                else if (processResultdetails.FilePath4 == null)
                                {
                                    processResultdetails.FilePath4 = filepath;
                                }
                                else if (processResultdetails.FilePath5 == null)
                                {
                                    processResultdetails.FilePath5 = filepath;
                                }
                                else
                                {
                                    processResultdetails.FilePath6 = filepath;
                                }
                            }
                        }
                        if (opt.ReferenceQuestionID != null && opt.ReferenceQuestionID != 0)
                        {
                            objEvaluationQues.Add(processResultdetails);
                        }
                    }
                }

            }
            _db.OpsAuditProcessResultDetails.AddRange(objEvaluationQues);
            _db.SaveChanges();
        }

        public async Task<APIPostProcessEvaluationDisplay> LastSubmittedOpsAuditResult(APILastSubmitedResult aPIPostAssessmentResult, string OrganisationCode)
        {
            APIPostProcessEvaluationDisplay _resultdata = null;
            try
            {
                OpsAuditProcessResult result = null;

                if (aPIPostAssessmentResult.UserId == "null")
                    aPIPostAssessmentResult.UserId = null;
                if (aPIPostAssessmentResult.BranchId == "null")
                    aPIPostAssessmentResult.BranchId = null;


                if (!string.IsNullOrEmpty(aPIPostAssessmentResult.UserId))
                {
                    result = _db.OpsAuditProcessResult.Where(x => x.UserId == aPIPostAssessmentResult.UserId.ToString()).OrderByDescending(x => x.Id).FirstOrDefault();
                }
                else
                {
                    result = _db.OpsAuditProcessResult.Where(x => x.BranchId == Convert.ToInt32(aPIPostAssessmentResult.BranchId)).OrderByDescending(x => x.Id).FirstOrDefault();

                }
                _resultdata = Mapper.Map<APIPostProcessEvaluationDisplay>(result);

                List<OpsAuditProcessResultDetails> ansdetails = _db.OpsAuditProcessResultDetails.Where(x => x.EvalResultID == _resultdata.Id).ToList();

                List<int> distquestion = ansdetails.Select(x => x.QuestionID).Distinct().ToList();

                List<APIQuestionDetails> ansdetailslist = new List<APIQuestionDetails>();

                foreach (int item in distquestion)
                {

                    OpsAuditProcessResultDetails ans = ansdetails.Where(x => x.QuestionID == item).FirstOrDefault();
                    int?[] ansid = ansdetails.Where(x => x.QuestionID == item).Select(x => x.OptionAnswerId).ToArray();
                    APIQuestionDetails ansdetail = new APIQuestionDetails();
                    ansdetail.ImprovementAnswer = ans.ImprovementAnswer;
                    ansdetail.ReferenceQuestionID = ans.QuestionID;
                    ansdetail.SelectedAnswer = ans.SelectedAnswer;
                    ansdetail.OptionAnswerId = ansid;
                    ansdetailslist.Add(ansdetail);
                }
                _resultdata.aPIQuestionDetails = ansdetailslist.OrderBy(x => x.ReferenceQuestionID).ToArray();
                _resultdata.ProcessManagement = _db.ProcessEvaluationManagement.Where(x => x.Id == result.ManagementId).Select(x => x.Title).FirstOrDefault();

                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                string ColumnName = "username";
                int Value = result.CreatedBy;
                HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnName + "/" + Value);
                xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
                if (response.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
                }

                string ColumnNamesupervisor = "username";
                int Valuesupervisor = result.CreatedBy;
                HttpResponseMessage responsesup = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + OrganisationCode + "/" + ColumnNamesupervisor + "/" + Value);
                xAPIUserDetails _xAPIUserDetailssup = new xAPIUserDetails();
                if (responsesup.IsSuccessStatusCode)
                {
                    var username = await response.Content.ReadAsStringAsync();
                    _xAPIUserDetailssup = JsonConvert.DeserializeObject<xAPIUserDetails>(username);

                }
                _resultdata.supervisorName = _xAPIUserDetailssup.Name;
                _resultdata.EvaluationByUserId = _xAPIUserDetails.Name;
                _resultdata.EvaluationDate = result.CreatedDate.ToString();
                return _resultdata;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return _resultdata;
        }


        #endregion

    }

}
