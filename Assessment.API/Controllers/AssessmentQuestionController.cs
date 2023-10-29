using AspNet.Security.OAuth.Introspection;
using Assessment.API.APIModel;
using Assessment.API.APIModel.Assessment;
using Assessment.API.Common;
using Assessment.API.Helper;
using Assessment.API.Helper.Metadata;
using Assessment.API.Model.Assessment;
using Assessment.API.Model.Log_API_Count;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Services;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using static Assessment.API.Common.AuthorizePermissions;
using static Assessment.API.Common.TokenPermissions;

namespace Assessment.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/AssessmentQuestion")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class AssessmentQuestionController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentQuestionController));
        private IAssessmentQuestion _assessmentQuestion;
        private ICourseRepository _courseRepository;
        private IAsessmentQuestionOption _asessmentQuestionOption;
        private IAssessmentConfigurationSheets _assessmentConfigurationSheets;
        private IAssessmentSheetConfigurationDetails _assessmentSheetConfigurationDetails;
        private IAssessmentQuestionRejectedRepository _assessmentQuestionRejectedRepository;
        private IPostAssessmentResult _postAssessmentResult;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        IAzureStorage _azurestorage;

        public IConfiguration _configuration { get; set; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        IMyCoursesRepository _myCoursesRepository;
        private static readonly ILog logger = LogManager.GetLogger(typeof(AssessmentQuestionController));
        public AssessmentQuestionController(
            IWebHostEnvironment environment,
            IAssessmentQuestionDetails postQuestionDetails,
            IConfiguration configure,
            IHttpContextAccessor httpContextAccessor,
            IAssessmentConfigurationSheets assessmentConfigurationSheets,
            IAssessmentQuestionRejectedRepository assessmentQuestionRejectedRepository,
            IAssessmentSheetConfigurationDetails assessmentSheetConfigurationDetails,
            IAssessmentQuestion assessmentQuestion,
            IAsessmentQuestionOption asessmentQuestionOption,
            IPostAssessmentResult postAssessmentResult,
            ICourseRepository courseRepository,
            IMyCoursesRepository myCoursesRepository,
            IAzureStorage azurestorage,
        IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this._assessmentQuestion = assessmentQuestion;
            this._asessmentQuestionOption = asessmentQuestionOption;
            this._assessmentConfigurationSheets = assessmentConfigurationSheets;
            this._assessmentSheetConfigurationDetails = assessmentSheetConfigurationDetails;
            this._assessmentQuestionRejectedRepository = assessmentQuestionRejectedRepository;
            this._postAssessmentResult = postAssessmentResult;
            this._courseRepository = courseRepository;
            this._configuration = configure;
            this._identitySvc = _identitySvc;
            this.hostingEnvironment = environment;
            this._httpContextAccessor = httpContextAccessor;
            this._tokensRepository = tokensRepository;
            this._myCoursesRepository = myCoursesRepository;
            this._azurestorage = azurestorage;
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}/{isMemoQuestions?}")]
        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.AssessmentQuestion)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null, bool? isMemoQuestions = null)
        {
            try
            {
                IEnumerable<APIGetQuestionMaster> questionMaster = await this._assessmentQuestion.GetAllQuestionPagination(page, pageSize, search, columnName, isMemoQuestions);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetAssessmentQuestion")]
        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.AssessmentQuestion)]
        public async Task<IActionResult> Get([FromBody] APIAssessmentQuestionSearch apiAssessmentQuestionSearch)
        {
            try
            {
                IEnumerable<APIGetQuestionMaster> questionMaster = await this._assessmentQuestion.GetAllQuestionPagination(apiAssessmentQuestionSearch.Page, apiAssessmentQuestionSearch.PageSize, apiAssessmentQuestionSearch.Search, apiAssessmentQuestionSearch.ColumnName, apiAssessmentQuestionSearch.IsMemoQuestions);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.AssessmentQuestion)]
        [HttpGet("GetTotalRecords/{search:minlength(0)?}/{columnName?}")]
        public async Task<IActionResult> GCount(string search, string columnName)
        {
            try
            {
                var assessment = await this._assessmentQuestion.Count(search, columnName);
                return Ok(assessment);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.AssessmentQuestion)]
        [HttpPost("GetTotalRecords")]
        public async Task<IActionResult> GetTotalRecords([FromBody] APIAssessmentQuestionSearch apiAssessmentQuestionSearch)
        {
            try
            {
                var assessment = await this._assessmentQuestion.Count(apiAssessmentQuestionSearch.Search, apiAssessmentQuestionSearch.ColumnName);
                return Ok(assessment);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetActiveQuestions/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public async Task<IActionResult> GetActiveQuestions(int page, int pageSize, string? search = null, string? columnName = null)
        {
            try
            {
                IEnumerable<APIGetQuestionMaster> questionMaster = await this._assessmentQuestion.GetAllActiveQuestion(page, pageSize, search, columnName);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("ActiveQuestionsCount/{search:minlength(0)?}/{columnName?}")]
        public async Task<IActionResult> GetActiveQuestionsCount(string search, string columnName)
        {
            try
            {
                var assessment = await this._assessmentQuestion.ActiveQustionsCount(search, columnName);
                return Ok(assessment);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IEnumerable<AssessmentQuestion>> Get()
        {
            try
            {
                return await this._assessmentQuestion.GetAll(s => s.IsDeleted == false);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }


        [HttpGet("GetAssessmentQuestionConfigurations/{ids?}")]
        public async Task<IEnumerable<AssessmentQuestion>> GetAssessmentQuestionConfigurations(int ids)
        {
            try
            {
                return await this._assessmentQuestion.GetAll(s => s.IsDeleted == false);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpGet("{id}")]
        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.AssessmentQuestion)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var Question = await this._assessmentQuestion.GetAssessmentQuestionByID(id);
                return Ok(Question);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAssessmentCompetencySkill/{id}")]
        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.AssessmentQuestion)]
        public async Task<IActionResult> GetCompetencySkill(int id)
        {
            try
            {
                var Question = await this._assessmentQuestion.GetCompetencySkill(id);
                return Ok(Question);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAssessmentsQuestionBankReject")]
        [PermissionRequired(Permissions.AssessmentQuestion)]
        public async Task<IEnumerable<AssessmentQuestionRejected>> GetAssessmentsQuestionBankReject()
        {
            try
            {
                return await this._assessmentQuestionRejectedRepository.GetAll(e => e.IsDeleted == Record.NotDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpGet("GetAllAssessmentsQuestionBankReject/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.AssessmentQuestion)]
        public async Task<IActionResult> GetUserReject(int page, int pageSize, string? search = null)
        {
            try
            {
                var QuestionReject = await this._assessmentQuestionRejectedRepository.GetAllAssessmentsQuestionReject(page, pageSize, search);
                return Ok(QuestionReject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAssessmentsQuestionBankReject/GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.AssessmentQuestion)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var QuestionReject = await this._assessmentQuestionRejectedRepository.Count(search);
                return Ok(QuestionReject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> Post([FromBody] List<APIAssessmentQuestion> aPIAssessmentsQuestion)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                foreach (APIAssessmentQuestion aPIAssessmentQuestion in aPIAssessmentsQuestion)
                {
                    var question = aPIAssessmentQuestion.QuestionText;

                    Boolean val = await this._assessmentQuestion.ExistQuestionOption(aPIAssessmentQuestion);
                    if (val)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }

                    foreach (AssessmentOptions opt in aPIAssessmentQuestion.aPIassessmentOptions)
                    {
                        bool validvalue = false;
                        if (FileValidation.CheckForSQLInjection(question))
                            validvalue = true;
                        //else
                        //if (FileValidation.CheckForSQLInjectionWithRegEx(opt.OptionText))
                        //    validvalue = true;
                        if (validvalue == true)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidCharacterInputOption) });
                        }
                    }

                }

                foreach (APIAssessmentQuestion obj in aPIAssessmentsQuestion)
                {
                    if (obj.aPIassessmentOptions.Count() != obj.Options)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }


                    //Check  Duplicate Option text
                    var duplicateOptionText = obj.aPIassessmentOptions.GroupBy(x => x.OptionText)
                                  .Where(g => g.Count() > 1)
                                  .Select(y => y.Count())
                                  .ToList();

                    if (duplicateOptionText.Count() > 0)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateOptionText), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateOptionText) });
                    }


                    //Check ContentPath and OptionContentPath Not Null
                    if (obj.ContentType == CommonValidation.Image || obj.ContentType == CommonValidation.ImageText || obj.ContentType == CommonValidation.TextAudio || obj.ContentType == CommonValidation.TextVideo)
                    {
                        //Check Content Path 
                        if (string.IsNullOrEmpty(obj.ContentPath))
                        {
                            //ERROR
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                        else if (obj.ContentType == CommonValidation.Image)
                        {
                            foreach (var option in obj.aPIassessmentOptions)
                            {
                                if (string.IsNullOrEmpty(option.OptionContentPath))
                                {
                                    //ERROR
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                                }
                            }
                        }
                    }

                    if (obj.OptionType == CommonValidation.SingleSelection)
                    {
                        //Check Multiple IsCorrectAnswer true On SingleSelection 
                        var duplicateIsCorrectAnswer = obj.aPIassessmentOptions.GroupBy(x => x.IsCorrectAnswer)
                                      .Where(g => g.Count() > 1 && g.Key == true)
                                      .Select(y => y.Count())
                                      .ToList();

                        if (duplicateIsCorrectAnswer.Count() > 0)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }

                    }


                    //Check At Least One Correct Ans On Both SingleSelection  and MultiSelection
                    var atlestOneIsCorrectAnswer = obj.aPIassessmentOptions.Where(g => g.IsCorrectAnswer == true);

                    if (atlestOneIsCorrectAnswer.Count() == 0 && obj.ContentType != "subjective")
                    {

                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }


                }




                List<APIAssessmentQuestion> errorAssessment = await this._assessmentQuestion.PostQuestion(aPIAssessmentsQuestion, UserId, OrganisationCode);
                if (errorAssessment == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                else if (errorAssessment.Count > 0)
                    return Ok(errorAssessment);
                return Ok(true);
            }

            catch (Exception ex)

            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> Put(int id, [FromBody] APIAssessmentQuestion apiAssessmentQuestion)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                var question = apiAssessmentQuestion.QuestionText;

                bool val = await this._assessmentQuestion.ExistQuestionOptionUpdate(apiAssessmentQuestion, apiAssessmentQuestion.Id);
                if (val)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }

                if (apiAssessmentQuestion.aPIassessmentOptions.Count() != apiAssessmentQuestion.Options)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                //Check  Duplicate Option text
                var duplicateOptionText = apiAssessmentQuestion.aPIassessmentOptions.GroupBy(x => x.OptionText)
                              .Where(g => g.Count() > 1)
                             .Select(y => y.Count())
                              .ToList();

                if (duplicateOptionText.Count() > 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateOptionText), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateOptionText) });
                }


                //Check ContentPath and OptionContentPath Not Null
                if (apiAssessmentQuestion.ContentType == CommonValidation.Image || apiAssessmentQuestion.ContentType == CommonValidation.ImageText || apiAssessmentQuestion.ContentType == CommonValidation.TextAudio || apiAssessmentQuestion.ContentType == CommonValidation.TextVideo)
                {
                    //Check Content Path 
                    if (string.IsNullOrEmpty(apiAssessmentQuestion.ContentPath))
                    {
                        //ERROR
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    else if (apiAssessmentQuestion.ContentType == CommonValidation.Image)
                    {
                        foreach (var option in apiAssessmentQuestion.aPIassessmentOptions)
                        {
                            if (string.IsNullOrEmpty(option.OptionContentPath))
                            {
                                //ERROR
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                            }
                        }
                    }
                }

                //Check At Least One Correct Ans On Both SingleSelection  and MultiSelection
                var atlestOneIsCorrectAnswer = apiAssessmentQuestion.aPIassessmentOptions.Where(g => g.IsCorrectAnswer == true);

                if (atlestOneIsCorrectAnswer.Count() == 0 && apiAssessmentQuestion.ContentType != "subjective")
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                Message Result = await this._assessmentQuestion.UpdateAssessmentQuestion(id, apiAssessmentQuestion, UserId);

                if (Result == Message.NotFound)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (Result == Message.InvalidModel)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Message Result = await this._assessmentQuestion.DeleteQuestion(DecryptedId);
                if (Result == Message.Success)
                    return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(MessageType.Success) });
                if (Result == Message.NotFound)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (Result == Message.DependencyExist)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("AssessmentQuestion")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> AssessmentQuestion([FromBody] APIAssessmentConfiguration aPIAssessmentsQuestion)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int ConfigurationId = await _assessmentConfigurationSheets.ConfigureAssessment(aPIAssessmentsQuestion, UserId);
                if (ConfigurationId == -1)
                    return StatusCode(409, "Duplicate");
                if (ConfigurationId == -2)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                return Ok(ConfigurationId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("AssessmentQuestionEdit/{id}")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> AssessmentQuestionEdit(int id, [FromBody] APIAssessmentConfiguration aPIAssessmentQuestion)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Message Result = await _assessmentConfigurationSheets.UpdateConfiguration(id, aPIAssessmentQuestion, UserId, OrganisationCode);
                if (Result == Message.NotFound)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (Result == Message.Ok)
                    return Ok("Success");
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuestions/{CourseId}/{ModuleId}/{AssessmentSheetConfigId}/{IsPreAssessment}/{IsContentAssessment}/{IsAdaptiveLearning}")]
        public async Task<IActionResult> GetQuetions(APIStartAssessment startAssessment)
        {
            try
            {
                var Questions = await _assessmentQuestion.GetAssessmentQuestion(startAssessment.AssessmentSheetConfigID, OrganisationCode);

                ApiResponse StartAssesmentresponse = await this._postAssessmentResult.StartAssessment(startAssessment, UserId, OrganisationCode);

                if (StartAssesmentresponse.StatusCode == 400)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = StartAssesmentresponse.Description });
                }

                return Ok(Questions);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetQuestionsDetails/{CourseId}/{ModuleId}/{IsPreAssessment}/{IsContentAssessment}/{IsAdaptiveLearning}")]
        public async Task<IActionResult> GetQuetionsDetails(APIStartAssessment startAssessment)
        {
            try
            {
                int ConfigurationID = Convert.ToInt32(await _courseRepository.GetAssessmentConfigurationID(startAssessment.CourseId, startAssessment.ModuleId, OrganisationCode, startAssessment.IsPreAssessment, startAssessment.IsContentAssessment));

                var Questions = await _assessmentQuestion.GetAssessmentQuestion(ConfigurationID, OrganisationCode);

                startAssessment.AssessmentSheetConfigID = ConfigurationID;


                // ------- Check For Assessment Complete ------ //
                PostAssessmentResult postAssessmentResult = new PostAssessmentResult();
                postAssessmentResult = await this._postAssessmentResult.CheckForAssessmentCompletedByUser(startAssessment, UserId);

                // ------- Check For Assessment Complete ------ //

                ApiResponse StartAssesmentresponse = await this._postAssessmentResult.StartAssessment(startAssessment, UserId, OrganisationCode);  //task.run
                if (StartAssesmentresponse.StatusCode == 400)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = StartAssesmentresponse.Description });
                }
                if (StartAssesmentresponse.ResponseObject != null)
                {
                    foreach (var Question in Questions)
                    {
                        Question.PostAssessmentId = Convert.ToInt32(StartAssesmentresponse.ResponseObject);
                    }
                }

                return Ok(Questions);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetManagerEvaluationQuestionsDetails/{CourseId}/{ModuleId}")]
        public async Task<IActionResult> GetManagerEvaluationQuestionsDetails(APIStartManagerEvaluation startAssessment)
        {
            try
            {
                int ConfigurationID = Convert.ToInt32(await _courseRepository.GetManagerAssessmentConfigurationID(startAssessment.CourseId, startAssessment.ModuleId));

                var Questions = await _assessmentQuestion.GetAssessmentQuestion(ConfigurationID, OrganisationCode);

                return Ok(Questions);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAssessmentReview/{CourseId}/{ModuleId}/{AssessmentSheetConfigId}/{IsPreAssessment}/{IsContentAssessment}/{IsAdaptiveLearning}")]
        public async Task<IActionResult> GetAssessmentReview(APIStartAssessment startAssessment)
        {
            try
            {
                var Questions = await _assessmentQuestion.GetQuestionForReview(startAssessment, UserId);
                return Ok(Questions);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetEditConfigurationID/{ConfigurationId}")]
        public async Task<IActionResult> GetEditConfigurationID(int ConfigurationId)
        {
            try
            {
                return Ok(await _assessmentQuestion.GetAssessmentQuestionByConfigurationId(ConfigurationId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAssessmentHeader/{ConfigurationID}/{CourseID}/{ModuleID}/{isPreassessment?}/{isContentAssessment?}/{isAdaptiveLearning?}")]
        public async Task<IActionResult> GetAssessmentHeader(int ConfigurationID, int CourseID, int ModuleID, bool isPreAssessment = false, bool isContentAssessment = false, bool isAdaptiveLearning = false)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await _assessmentQuestion.GetAssessmentHeader(ConfigurationID, CourseID, ModuleID, UserId, isPreAssessment, isContentAssessment, isAdaptiveLearning, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAssessmentHeaderDetails/{CourseID}/{ModuleID}/{isPreassessment?}/{isContentAssessment?}/{isAdaptiveLearning?}")]
        public async Task<IActionResult> GetAssessmentHeaderDetails(int CourseID, int ModuleID, bool isPreAssessment = false, bool isContentAssessment = false, bool isAdaptiveLearning = false)
        {
            try
            {
                int ConfigurationID;
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (isPreAssessment == true && isContentAssessment == true)
                    return BadRequest();

                ConfigurationID = Convert.ToInt32(await _courseRepository.GetAssessmentConfigurationID(CourseID, ModuleID, OrganisationCode, isPreAssessment, isContentAssessment));

                return Ok(await _assessmentQuestion.GetAssessmentHeader(ConfigurationID, CourseID, ModuleID, UserId, isPreAssessment, isContentAssessment, isAdaptiveLearning, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAssessmentStatus/{CourseID}/{ModuleID?}/{isPreassessment?}/{isContentAssessment?}")]
        public async Task<IActionResult> GetAssessmentStatus(int CourseID, int? ModuleID, bool isPreAssessment = false, bool isContentAssessment = false)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await _assessmentQuestion.AssessmentStatus(CourseID, ModuleID, UserId, isPreAssessment, isContentAssessment));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AssessmentQuestionExist")]
        public async Task<bool> AssessmentQuestionExist([FromBody] APIQuestionsExits questions)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(questions.Questiontext))
                    return true;
                else
                    return await this._assessmentQuestion.Exist(questions.Questiontext);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }

        }

        [HttpDelete("DeleteConfigurationQuestions")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> DeleteConfigurationQuestions([FromQuery] string ids, string ConfigurationID)
        {
            try
            {
                int DecryptedConfigurationID = Convert.ToInt32(Security.Decrypt(ConfigurationID));
                string Decryptedids = Security.Decrypt(ids);
                int[] QuestionIds = Decryptedids.Split(",".ToCharArray()).Select(x => System.Int32.Parse(x.ToString())).ToArray();

                await _assessmentSheetConfigurationDetails.DeleteQuestion(DecryptedConfigurationID, QuestionIds, UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostAssessmentQuestionFile")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> PostAssessmentQuestionFile()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        if (FileValidation.IsValidImage(fileUpload) || FileValidation.IsValidLCMSVideo(fileUpload) || FileValidation.IsValidLCMSAudio(fileUpload))
                        {
                            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {

                                string filename = fileUpload.FileName;
                                string[] fileaary = filename.Split('.');
                                string fileextention = fileaary[1].ToLower();
                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrganisationCode, Record.AssessmentContent);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
                                using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                                {
                                    await fileUpload.CopyToAsync(fs);
                                }
                                if (String.IsNullOrEmpty(file))
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                }
                                string RelativePath = new System.Uri(file).AbsoluteUri;
                                string FileUrl = string.Concat(RelativePath.Substring(RelativePath.LastIndexOf("/" + OrganisationCode)));
                                return Ok(FileUrl);
                            }
                            else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrganisationCode, Record.AssessmentContent);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            string file = res.Blob.Name.ToString();

                                            return Ok("/" + file.Replace(@"\", "/"));
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
                                }
                            }
                        }
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                //Truncate UserMasterRejected Record    
                _assessmentQuestionRejectedRepository.Delete();
                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.XLSX : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        if (FileValidation.IsValidXLSX(fileUpload))
                        {
                            string filename = fileUpload.FileName;
                            string[] fileaary = filename.Split('.');
                            string fileextention = fileaary[1].ToLower();
                            string filex = Record.XLSX;
                            if (fileextention != filex)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });
                            }
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrganisationCode, customerCode, FileType);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));
                        }
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ExportSearch/{showAllData?}/{search?}/{columnName?}")]
        public async Task<IActionResult> ExportSearch(bool showAllData, string search, string columnName)
        {
            try
            {
                IEnumerable<APIGetQuestionMaster> aPIGetQuestionMaster = await this._assessmentQuestion.GetAllQuestionMaster(1, 65536, UserId, RoleCode, showAllData, search);

                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string sFileName = @"AssessmentQuestionBankWithData.xlsx";
                string ApiGatewayUrl = this._configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", ApiGatewayUrl, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, OrganisationCode, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("AssessmentQuestionBankWithData");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Question";
                    worksheet.Cells[1, 2].Value = "Options";

                    worksheet.Cells[1, 3].Value = "Option1";
                    worksheet.Cells[1, 4].Value = "Option2";
                    worksheet.Cells[1, 5].Value = "Option3";
                    worksheet.Cells[1, 6].Value = "Option4";
                    worksheet.Cells[1, 7].Value = "Option5";

                    worksheet.Cells[1, 8].Value = "Metadata";
                    worksheet.Cells[1, 9].Value = "Marks";
                    worksheet.Cells[1, 10].Value = "Status";

                    int row = 2, column = 1;
                    foreach (APIGetQuestionMaster question in aPIGetQuestionMaster)
                    {
                        worksheet.Cells[row, column++].Value = question.Question;
                        worksheet.Cells[row, column++].Value = question.OptionsCount;

                        worksheet.Cells[row, column++].Value = question.Option1;
                        worksheet.Cells[row, column++].Value = question.Option2;
                        worksheet.Cells[row, column++].Value = question.Option3;
                        worksheet.Cells[row, column++].Value = question.Option4;
                        worksheet.Cells[row, column++].Value = question.Option5;

                        worksheet.Cells[row, column++].Value = question.Metadata;
                        worksheet.Cells[row, column++].Value = question.Marks;
                        worksheet.Cells[row, column++].Value = question.Status == true ? "Active" : "Inactive";

                        row++;
                        column = 1;
                    }

                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [Route("SaveFileData")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> PostFile([FromBody] APIAssessmentFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string filefinal = sWebRootFolder + aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(filefinal));
                string resultString = await this._assessmentQuestion.ProcessImportFile(file, _assessmentQuestion, _assessmentQuestionRejectedRepository, _asessmentQuestionOption, UserId, OrganisationCode);
                return Ok(resultString);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }



        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"AssessmentQuestionBank.xlsx";
                string DomainName = this._configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }

                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("AssessmentQuestionBank");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Metadata*";
                    worksheet.Cells[1, 2].Value = "DifficultyLevel";
                    worksheet.Cells[1, 3].Value = "AnswerOptions*";
                    worksheet.Cells[1, 4].Value = "Marks*";
                    worksheet.Cells[1, 5].Value = "QuestionText*";
                    worksheet.Cells[1, 6].Value = "AnswerOption1*";
                    worksheet.Cells[1, 7].Value = "AnswerOption2*";
                    worksheet.Cells[1, 8].Value = "AnswerOption3";
                    worksheet.Cells[1, 9].Value = "AnswerOption4";
                    worksheet.Cells[1, 10].Value = "AnswerOption5";
                    worksheet.Cells[1, 11].Value = "CorrectAnswer1*";
                    worksheet.Cells[1, 12].Value = "CorrectAnswer2";
                    worksheet.Cells[1, 13].Value = "CorrectAnswer3";
                    worksheet.Cells[1, 14].Value = "CorrectAnswer4";
                    worksheet.Cells[1, 15].Value = "CorrectAnswer5";
                    worksheet.Cells[1, 16].Value = "CourseCode";
                    //column cell format set to text as 10% getting converted by excel to 0.1 internally
                    worksheet.Cells["E1:E2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["F1:F2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["G1:G2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["H1:H2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["I1:I2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["J1:J2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["K1:K2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["L1:L2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["M1:M2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["N1:N2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["O1:O2000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["P1:P2000"].Style.Numberformat.Format = "@";

                    using (var rngitems = worksheet.Cells["A1:P1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                    }

                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("ExportRejected")]
        [PermissionRequired(Permissions.AssessmentQuestion)]
        public async Task<IActionResult> ExportRejected()
        {
            try
            {
                var assessmentquestionrejected = await this._assessmentQuestion.GetAllAssessmentQuestionRejected();
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "AssessmentQuestionRejected.xlsx";
                string URL = string.Concat(DomainName, "/", sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Courses");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Section";
                    worksheet.Cells[1, 2].Value = "LearnerInstruction";
                    worksheet.Cells[1, 3].Value = "QuestionText";
                    worksheet.Cells[1, 4].Value = "DifficultyLevel";
                    worksheet.Cells[1, 5].Value = "ModelAnswer";
                    worksheet.Cells[1, 6].Value = "MediaFile";
                    worksheet.Cells[1, 7].Value = "AnswerAsImages";
                    worksheet.Cells[1, 8].Value = "Marks";
                    worksheet.Cells[1, 9].Value = "Status";
                    worksheet.Cells[1, 10].Value = "Question Text";
                    worksheet.Cells[1, 11].Value = "Question Style";
                    worksheet.Cells[1, 12].Value = "Question Type";
                    worksheet.Cells[1, 13].Value = "Metadata";
                    worksheet.Cells[1, 14].Value = "AnswerOptions";
                    worksheet.Cells[1, 15].Value = "Answer Option1";
                    worksheet.Cells[1, 16].Value = "Answer Option2";
                    worksheet.Cells[1, 17].Value = "Answer Option3";
                    worksheet.Cells[1, 18].Value = "Answer Option4";
                    worksheet.Cells[1, 19].Value = "Answer Option5";
                    worksheet.Cells[1, 20].Value = "Correct Answer1";
                    worksheet.Cells[1, 21].Value = "Correct Answer2";
                    worksheet.Cells[1, 22].Value = "Correct Answer3";
                    worksheet.Cells[1, 23].Value = "Correct Answer4";
                    worksheet.Cells[1, 24].Value = "Correct Answer5";
                    worksheet.Cells[1, 25].Value = "CourseCode";
                    worksheet.Cells[1, 26].Value = "Error Message";

                    int row = 2, column = 1;
                    foreach (AssessmentQuestionRejected assessmentquestionreject in assessmentquestionrejected)
                    {
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.Section;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.LearnerInstruction;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.QuestionText;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.DifficultyLevel;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.ModelAnswer;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.MediaFile;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.AnswerAsImages;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.Marks;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.Status;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.QuestionText;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.QuestionStyle;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.QuestionType;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.Metadata;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.AnswerOptions;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.AnswerOption1;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.AnswerOption2;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.AnswerOption3;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.AnswerOption4;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.AnswerOption5;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.CorrectAnswer1;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.CorrectAnswer2;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.CorrectAnswer3;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.CorrectAnswer4;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.CorrectAnswer5;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.CourseCode;
                        worksheet.Cells[row, column++].Value = assessmentquestionreject.ErrorMessage;


                        row++;
                        column = 1;
                    }
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;

                    }
                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAdaptiveQuestions/{courseId}")]
        public async Task<IActionResult> GetAdaptiveAssessmentQuestions(int courseId)
        {
            try
            {

                return Ok(await this._assessmentQuestion.GetAdaptiveAssessment(courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAdaptiveAssessmentHeader/{courseId}")]
        public async Task<IActionResult> GetAssessmentHeader(int courseId)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await this._assessmentQuestion.GetAdaptiveAssessmentHeader(courseId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostAssessmentQuestion")]
        public async Task<IActionResult> PostAssessmentQuestion([FromBody] APIPostAssessmentQuestionResult aPIPostAssessmentResult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ApiResponse response = await this._postAssessmentResult.PostAssessmentQuestion(aPIPostAssessmentResult, UserId, OrganisationCode);
                if (response.StatusCode == 404)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = response.Description });
                }
                if (response.StatusCode == 403)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = response.Description });
                }
                return Ok(response.ResponseObject);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("PostManagerEvaluation")]
        public async Task<IActionResult> PostManagerEvaluation([FromBody] APIPostManagerEvaluationResult aPIPostAssessmentResult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int ConfigurationID = Convert.ToInt32(await _courseRepository.GetManagerAssessmentConfigurationID(aPIPostAssessmentResult.CourseID, aPIPostAssessmentResult.ModuleID));
                aPIPostAssessmentResult.AssessmentSheetConfigID = ConfigurationID;
                ApiResponse response = await this._postAssessmentResult.PostManagerEvaluation(aPIPostAssessmentResult, OrganisationCode);
                if (response.StatusCode == 404)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = response.Description });
                }
                if (response.StatusCode == 403)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = response.Description });
                }
                return Ok(response.ResponseObject);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("PostAdaptiveAssessment")]
        public async Task<IActionResult> GetAdaptiveAssessmentQuestions([FromBody] APIPostAdaptiveAssessment apiPostAdaptiveAssessment)
        {
            try
            {
                if (apiPostAdaptiveAssessment.aPIPostQuestionDetails.Length <= 0)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                return Ok(await this._postAssessmentResult.PostAdaptiveAssessment(apiPostAdaptiveAssessment, UserId, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("MultiDeleteAssessmentQuestion")]
        [PermissionRequired(Permissions.assessmentquestion)]
        public async Task<IActionResult> MultiDeleteAssessmentQuestion([FromBody] APIDeleteAssessmentQuestion[] apideletemultipleque)
        {
            try
            {
                ApiResponse Responce = await this._assessmentQuestion.MultiDeleteAssessmentQuestion(apideletemultipleque);
                return Ok(Responce.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetReviewQuestionDetails/{PostAssessmentResultID}")]
        public async Task<IActionResult> GetReviewQuestionDetails(int PostAssessmentResultID)
        {
            try
            {
                var Questions = await _assessmentQuestion.GetReviewQuestion(PostAssessmentResultID);

                return Ok(Questions);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostSubjectiveAssessmentReview")]
        public async Task<IActionResult> PostSubjectiveAssessmentReview([FromBody] APIPostSubjectiveReview aPIPostAssessmentResult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ApiResponse response = await this._postAssessmentResult.PostSubjectiveAssessmentReview(aPIPostAssessmentResult, UserId, OrganisationCode);
                if (response.StatusCode == 404)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = response.Description });
                }
                if (response.StatusCode == 403)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = response.Description });
                }
                return Ok(response.ResponseObject);

            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetSubjectiveAssessment")]
        public async Task<IActionResult> GetSubjectiveAssessment([FromBody] APISubjectiveAssessmentToCheck apiAssessmentQuestionSearch)
        {
            try
            {

                IEnumerable<APIAssessmentDataForReview> questionMaster = await this._assessmentQuestion.GetAssessmentForReview(apiAssessmentQuestionSearch.Page, apiAssessmentQuestionSearch.PageSize, apiAssessmentQuestionSearch.Search, apiAssessmentQuestionSearch.ColumnName);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("CountGetSubjectiveAssessment")]
        public async Task<IActionResult> GetCountForReview([FromBody] APISubjectiveAssessmentToCheck apiAssessmentQuestionSearch)
        {
            try
            {
                var assessment = await this._assessmentQuestion.ReviewCountCount(apiAssessmentQuestionSearch.Search, apiAssessmentQuestionSearch.ColumnName);
                return Ok(assessment);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetAssessmentQuestionV2")]
        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.AssessmentQuestion)]
        public async Task<IActionResult> GetV2([FromBody] APIAssessmentQuestionSearch apiAssessmentQuestionSearch)
        {
            try
            {
                APITotalAssessmentQuestion questionMaster = await this._assessmentQuestion.GetAllQuestionPaginationV2(apiAssessmentQuestionSearch.Page, apiAssessmentQuestionSearch.PageSize, UserId, RoleCode, apiAssessmentQuestionSearch.showAllData, apiAssessmentQuestionSearch.Search, apiAssessmentQuestionSearch.ColumnName, apiAssessmentQuestionSearch.IsMemoQuestions);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetRecommondedCourses")]

        public async Task<IActionResult> GetRecommondedCourses()
        {
            try
            {
                List<APITrainingReommendationNeeds> data = await this._postAssessmentResult.GetLatestAssessmentSubmitted(UserId);

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SaveAssessmentPhotos")]
        public async Task<IActionResult> SaveAssessmentPhotos([FromBody] APIassessmentPhotos aPIassessmentPhotos)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    if (string.IsNullOrEmpty(aPIassessmentPhotos.ImageData))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    string[] str = aPIassessmentPhotos.ImageData.Split(',');
                    var bytes = Convert.FromBase64String(str[1]);
                    var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                    if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                    {
                        string fileDir = this._configuration["ApiGatewayWwwroot"];
                        fileDir = Path.Combine(fileDir, OrganisationCode);
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        fileDir = Path.Combine(fileDir, "AssessmenntPhotos", aPIassessmentPhotos.PostAssessmentId.ToString());
                        if (!Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        string file = Path.Combine(fileDir, DateTime.UtcNow.Ticks + ".png");
                        if (bytes.Length > 0)
                        {
                            using (var stream = new FileStream(file, FileMode.Create))
                            {
                                stream.Write(bytes, 0, bytes.Length);
                                stream.Flush();
                            }
                        }

                        if (string.IsNullOrEmpty(file))
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                        int lastindex = file.LastIndexOf(OrganisationCode);
                        file = file.Substring(lastindex).Replace(@"\", "/");
                        file = file.Replace(@"\""", "");
                        aPIassessmentPhotos.ImageData = file;
                        int num = this._assessmentQuestion.SaveAssessmentPhotos(aPIassessmentPhotos, UserId);
                        return this.Ok(file);
                    }
                    else
                    {
                        BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode, aPIassessmentPhotos.PostAssessmentId.ToString(), "assessmenntPhotos");
                        if (res != null)
                        {
                            if (res.Error == false)
                            {
                                string filePath = res.Blob.Name.ToString();
                                aPIassessmentPhotos.ImageData = filePath.Replace(@"\", "/");
                                int num = this._assessmentQuestion.SaveAssessmentPhotos(aPIassessmentPhotos, UserId);
                                return this.Ok(filePath.Replace(@"\", "/"));
                            }
                            else
                            {
                                _logger.Error(res.ToString());
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }



            }
            else
            {
                _logger.Error("Invalid Model data in assessment photo");
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("SaveCameraPhotos")]
        public async Task<IActionResult> SaveCameraPhotos([FromBody] APIassessmentPhotos aPIassessmentPhotos)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    if (string.IsNullOrEmpty(aPIassessmentPhotos.ImageData))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    string[] str = aPIassessmentPhotos.ImageData.Split(',');
                    var bytes = Convert.FromBase64String(str[1]);
                    var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                    if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                    {
                        string fileDir = this._configuration["ApiGatewayWwwroot"];
                        fileDir = Path.Combine(fileDir, OrganisationCode);
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        fileDir = Path.Combine(fileDir, aPIassessmentPhotos.CourseId + "_" + aPIassessmentPhotos.ModuleId, UserId.ToString());
                        if (!Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        string file = Path.Combine(fileDir, DateTime.UtcNow.Ticks + ".png");
                        if (bytes.Length > 0)
                        {
                            using (var stream = new FileStream(file, FileMode.Create))
                            {
                                stream.Write(bytes, 0, bytes.Length);
                                stream.Flush();
                            }
                        }

                        if (string.IsNullOrEmpty(file))
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                        int lastindex = file.LastIndexOf(OrganisationCode);
                        file = file.Substring(lastindex).Replace(@"\", "/");
                        file = file.Replace(@"\""", "");
                        aPIassessmentPhotos.ImageData = file;
                        int num = this._assessmentQuestion.SaveAssessmentPhotos(aPIassessmentPhotos, UserId);
                        return this.Ok(file);
                    }
                    else
                    {
                        BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode, UserId.ToString(), aPIassessmentPhotos.CourseId + "_" + aPIassessmentPhotos.ModuleId);
                        if (res != null)
                        {
                            if (res.Error == false)
                            {
                                string filePath = res.Blob.Name.ToString();
                                aPIassessmentPhotos.ImageData = filePath.Replace(@"\", "/");
                                int num = this._assessmentQuestion.SaveAssessmentPhotos(aPIassessmentPhotos, UserId);
                                return this.Ok(filePath.Replace(@"\", "/"));
                            }
                            else
                            {
                                _logger.Error(res.ToString());
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }



            }
            else
            {
                _logger.Error("Invalid Model data in assessment photo");
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCameraPhotos")]
        public async Task<IActionResult> GetCameraPhotos([FromBody] GetAPIassessmentPhotos getAPIassessmentPhotos)
        {
            if (ModelState.IsValid)
            {
                int EU_UserId = await _myCoursesRepository.GetUserDetailsByUserID(getAPIassessmentPhotos.Userid);
                CameraPhotosResponse cameraPhotosResponse = await this._assessmentQuestion.GetCameraPhotos(getAPIassessmentPhotos, EU_UserId);
                return Ok(cameraPhotosResponse);
            }
            else
            {
                _logger.Error("Invalid Model data in assessment photo");
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetCoursesForEvaluation")]
        public async Task<IActionResult> GetCoursesForEvaluation([FromBody] APIEvaulationPayload aPIEvaulationPayload)
        {
            if (ModelState.IsValid)
            {
                int EU_UserId = await _myCoursesRepository.GetUserDetailsByUserID(aPIEvaulationPayload.UserId);
                APITotalCourseForEvaluation aPITotalCourseForEvaluation = await this._assessmentQuestion.GetCoursesForCameraEvaluation(aPIEvaulationPayload.page, aPIEvaulationPayload.pageSize, EU_UserId);
                return Ok(aPITotalCourseForEvaluation);
            }
            else
            {
                _logger.Error("Invalid Model data in assessment photo");
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SaveStatusForCourseEvaluation")]
        public async Task<IActionResult> SaveStatusForCourseEvaluation([FromBody] CameraPhotosStatusForCourseEvaluation cameraPhotosStatusForCourseEvaluation)
        {
            if (ModelState.IsValid)
            {
                int EU_UserId = await _myCoursesRepository.GetUserDetailsByUserID(cameraPhotosStatusForCourseEvaluation.UserId);
                int result = await this._assessmentQuestion.SaveStatusForCourseEvaluation(cameraPhotosStatusForCourseEvaluation, EU_UserId, UserId);
                if (result == 0)
                {
                    return Ok();
                }
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
            else
            {
                _logger.Error("Invalid Model data in assessment photo");
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("AssessmentCompletionStatus/{CourseID}/{ModuleID}/{isPreassessment?}/{isContentAssessment?}/{isAdaptiveLearning?}")]
        public async Task<IActionResult> AssessmentCompletionStatus(int CourseID, int ModuleID, bool isPreAssessment = false, bool isContentAssessment = false, bool isAdaptiveLearning = false)
        {
            try
            {


                return Ok(await _assessmentQuestion.AssessmentCompletionStatus(CourseID, ModuleID, UserId, isPreAssessment, isContentAssessment, isAdaptiveLearning));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}