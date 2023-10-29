using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using AutoMapper;
using Assessment.API.Common;
using Assessment.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Assessment.API.Common.TokenPermissions;
using Assessment.API.Helper;
using log4net;
using Assessment.API.Model.Log_API_Count;

namespace Assessment.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class AnswerSheetsEvaluationController : Controller
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AnswerSheetsEvaluationController));
        private IAnswerSheetsEvaluationRepository answerSheetsEvaluationRepository;
        private IPostAssessmentResult postAssessmentResultRepository;
        private ISubjectiveAssessmentStatus subjectiveAssessmentStatusRepository;
        private IAssessmentQuestionDetails assessmentQuestionDetailsRepository;
        private readonly ITokensRepository _tokensRepository;
        public AnswerSheetsEvaluationController(IAssessmentQuestionDetails assessmentQuestionDetailsController, ISubjectiveAssessmentStatus subjectiveAssessmentStatusController, IAnswerSheetsEvaluationRepository answerSheetsEvaluationController, IPostAssessmentResult postAssessmentResultController, ITokensRepository tokensRepository)
        {
            this.answerSheetsEvaluationRepository = answerSheetsEvaluationController;
            this.postAssessmentResultRepository = postAssessmentResultController;
            this.subjectiveAssessmentStatusRepository = subjectiveAssessmentStatusController;
            this.assessmentQuestionDetailsRepository = assessmentQuestionDetailsController;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var answerSheetsEvaluation = await this.answerSheetsEvaluationRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIAnswerSheetsEvaluation>>(answerSheetsEvaluation));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                var answerSheetsEvaluation = await this.answerSheetsEvaluationRepository.GetAllAnswerSheetsEvaluation(page, pageSize, search);
                return Ok(Mapper.Map<List<APIAnswerSheetsEvaluation>>(answerSheetsEvaluation));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var answerSheetsEvaluation = await this.answerSheetsEvaluationRepository.Count(search);
                return Ok(answerSheetsEvaluation);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var answerSheetsEvaluation = await this.answerSheetsEvaluationRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APIAnswerSheetsEvaluation>(answerSheetsEvaluation));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("SubjectiveAssessmentStatus")]
        public async Task<IActionResult> GetAssessmentSheetByUserId()
        {
            try
            {
                var subjectiveAssessmentStatus = await this.subjectiveAssessmentStatusRepository.GetAssessmentSheetByUserId();
                return Ok((subjectiveAssessmentStatus));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("PostAssessmentResult/{id:int}")]
        public async Task<IActionResult> GetPostAssessmentResultById(int id)
        {
            try
            {
                var postAssessmentResult = await this.postAssessmentResultRepository.GetPostAssessmentResultById(id);
                return Ok((postAssessmentResult));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("PostAssessmentQuestionDetails/{id:int}")]
        public async Task<IActionResult> GetPostAssessmentQuestionDetailsById(int id)
        {
            try
            {
                var postAssessmentQuestionDetails = await this.assessmentQuestionDetailsRepository.GetPostAssessmentQuestionDetailsById(id);
                return Ok((postAssessmentQuestionDetails));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APIPostSubjectiveAssessmentResultMerged aPIPostSubjectiveAssessmentResultMerged)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    PostAssessmentResult postAssessmentResult = await this.postAssessmentResultRepository.Get(aPIPostSubjectiveAssessmentResultMerged.AssessmentResultID);
                    postAssessmentResult.AssessmentPercentage = aPIPostSubjectiveAssessmentResultMerged.AssessmentPercentage;
                    postAssessmentResult.MarksObtained = aPIPostSubjectiveAssessmentResultMerged.TotalMarks;
                    decimal passingPer = postAssessmentResult.PassingPercentage;
                    if (aPIPostSubjectiveAssessmentResultMerged.AssessmentPercentage >= passingPer)
                    {
                        postAssessmentResult.PostAssessmentStatus = "Complete";
                        postAssessmentResult.AssessmentResult = "Pass";
                    }
                    else
                    {
                        postAssessmentResult.PostAssessmentStatus = "InComplete";
                        postAssessmentResult.AssessmentResult = "Fail";
                    }
                    postAssessmentResult.ModifiedBy = 1;
                    postAssessmentResult.ModifiedDate = DateTime.UtcNow;
                    await postAssessmentResultRepository.Update(postAssessmentResult);

                    SubjectiveAssessmentStatus subjectiveAssessmentStatus = await this.subjectiveAssessmentStatusRepository.Get(aPIPostSubjectiveAssessmentResultMerged.AssessmentId);

                    var answerSheetsEval = await this.assessmentQuestionDetailsRepository.Count(aPIPostSubjectiveAssessmentResultMerged.AssessmentResultID);

                    int count = (aPIPostSubjectiveAssessmentResultMerged.aPIAssessmentSubjectiveQuestion).Count();
                    if (answerSheetsEval == count)
                    {
                        subjectiveAssessmentStatus.Status = "CHECKED";
                    }
                    else
                    {
                        subjectiveAssessmentStatus.Status = "PENDING";
                    }
                    subjectiveAssessmentStatus.CheckerID = 1;
                    await subjectiveAssessmentStatusRepository.Update(subjectiveAssessmentStatus);

                    List<AnswerSheetsEvaluation> answerSheetsEvaluations = new List<AnswerSheetsEvaluation>();
                    foreach (APIAssessmentSubjectiveQuestion opt in aPIPostSubjectiveAssessmentResultMerged.aPIAssessmentSubjectiveQuestion)
                    {
                        AnswerSheetsEvaluation answerSheetsEvaluation = new AnswerSheetsEvaluation();
                        answerSheetsEvaluation.AnswerSheetId = opt.AnswerSheetId;
                        answerSheetsEvaluation.QuestionId = opt.ReferenceQuestionID;
                        answerSheetsEvaluation.Marks = opt.GivenMarks;
                        answerSheetsEvaluation.Remarks = opt.Remarks;
                        answerSheetsEvaluation.IsDeleted = false;
                        answerSheetsEvaluation.CreatedBy = 1;
                        answerSheetsEvaluation.CreatedDate = DateTime.UtcNow;
                        answerSheetsEvaluation.ModifiedBy = 1;
                        answerSheetsEvaluation.ModifiedDate = DateTime.UtcNow;
                        answerSheetsEvaluations.Add(answerSheetsEvaluation);
                    }
                    await answerSheetsEvaluationRepository.AddRange(answerSheetsEvaluations);

                }

                return Ok(aPIPostSubjectiveAssessmentResultMerged);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + " Message:" + ex.Message });
            }
        }

        [HttpPost("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        private async Task<IActionResult> Delete(int id)
        {
            AnswerSheetsEvaluation answerSheetsEvaluation = await this.answerSheetsEvaluationRepository.Get(id);

            if (ModelState.IsValid && answerSheetsEvaluation != null)
            {
                answerSheetsEvaluation.IsDeleted = true;
                await this.answerSheetsEvaluationRepository.Update(answerSheetsEvaluation);
            }

            if (answerSheetsEvaluation == null)
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            return this.Ok();

        }

        /// <summary>
        /// Search specific AnswerSheetsEvaluation.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                var answerSheetsEvaluation = await this.answerSheetsEvaluationRepository.Search(q);
                return Ok(Mapper.Map<List<APIAnswerSheetsEvaluation>>(answerSheetsEvaluation));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
