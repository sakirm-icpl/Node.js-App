using AspNet.Security.OAuth.Introspection;
using Feedback.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data;
using static Courses.API.Common.AuthorizePermissions;
using log4net;
using static Feedback.API.Common.TokenPermissions;
using Feedback.API.Model.Log_API_Count;
using Feedback.API.Common;
using Feedback.API.Model;
using Feedback.API.APIModel;
using Feedback.API.Helper;
using Feedback.API.Services;
using Feedback.API.Helper.Metadata;
using Feedback.API.APIModel.Assessment;
using Feedback.API.APIModel.Feedback;

namespace Feedback.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class CourseFeedbackController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseFeedbackController));
        IFeedbackOption _feedbackOption;
        IFeedbackQuestion _feedbackQuestion;
        IFeedbackQuestionRejectedRepository _feedbackQuestionRejectedRepository;
        IFeedbackStatus _feedbackStatus;
        IFeedbackStatusDetail _feedbackStatusDetail;
        IFeedbackSheetConfiguration _feedbackSheetConfiguration;
        IFeedbackSheetConfigurationDetails _feedbackSheetConfigurationDetails;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
       
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRewardsPointRepository _rewardsPointRepository;
        private readonly ITokensRepository _tokensRepository;

        public CourseFeedbackController(IWebHostEnvironment environment,  IConfiguration configure, ICustomerConnectionStringRepository customerConnectionStringRepository, IHttpContextAccessor httpContextAccessor, IFeedbackSheetConfiguration feedbackSheetConfiguration, IFeedbackSheetConfigurationDetails feedbackSheetConfigurationDetails, IFeedbackOption feedbackOption, IFeedbackQuestion feedbackQuestion, IFeedbackStatus feedbackStatus, IFeedbackStatusDetail feedbackStatusDetail, IFeedbackQuestionRejectedRepository feedbackQuestionRejectedRepository, IIdentityService _identitySvc, IRewardsPointRepository rewardsPointRepository, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            _feedbackOption = feedbackOption;
            _feedbackQuestion = feedbackQuestion;
            _feedbackStatus = feedbackStatus;
            _feedbackStatusDetail = feedbackStatusDetail;
            _feedbackSheetConfiguration = feedbackSheetConfiguration;
            _feedbackSheetConfigurationDetails = feedbackSheetConfigurationDetails;
            this._feedbackQuestionRejectedRepository = feedbackQuestionRejectedRepository;
            this._configuration = configure;
            this.hostingEnvironment = environment;
            this._httpContextAccessor = httpContextAccessor;
            this._rewardsPointRepository = rewardsPointRepository;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            this._tokensRepository = tokensRepository;
        }


        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {
                if (_feedbackQuestion.Count() == 0)
                {
                    return NotFound();
                }
                List<FeedbackQuestion> feedbackQuestion = await _feedbackQuestion.GetAll(f => f.IsDeleted == false);
                return Ok(feedbackQuestion);
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetActiveQuestions/{page:int}/{pageSize:int}/{isEmoji?}/{search?}/{columnName?}")]
        public async Task<IActionResult> GetActiveQuestions(int page, int pageSize, string isEmoji, string? search = null, string? columnName = null)
        {
            try
            {
                List<CourseFeedbackAPI> feedbackQuestion = await _feedbackQuestion.GetActiveFeedbackQuestion(page, pageSize, isEmoji, search, columnName);
                return Ok(feedbackQuestion);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetActiveQuestionsCount/{isEmoji?}/{search:minlength(0)?}/{columnName?}")]
        public IActionResult GetActiveQuestionsCount(string? isEmoji = null, string? search = null, string? columnName = null)
        {
            try
            {
                int count = _feedbackQuestion.ActiveQuestionCount(search, columnName, isEmoji);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByCourseId/{courseId}")]
        public async Task<IActionResult> GetByCourseId(int courseId)
        {
            try
            {
                return Ok(await _feedbackQuestion.GetFeedbackByCourseId(courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetFeedbackByConfigurationId/{configId}")]
        public async Task<IActionResult> GetFeedbackByConfigurationId(int configId)
        {
            try
            {
                return Ok(await _feedbackQuestion.GetFeedbackByConfigurationId(configId, OrganisationCode));
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
                return Ok(await _feedbackQuestion.GetFeedback(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("LCMS/{page:int}/{pageSize:int}/{isEmoji?}/{search?}/{columnName?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string isEmoji, string? search = null, string? columnName = null)
        {

            try
            {
                List<CourseFeedbackAPI> feedbackQuestion = await _feedbackQuestion.GetLCMS(page, pageSize, isEmoji, search, columnName);
                return Ok(feedbackQuestion);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("LCMS/getTotalRecords/{isEmoji?}/{search:minlength(0)?}/{columnName?}")]
        public IActionResult GetCount(string? search = null, string? isEmoji = null, string? columnName = null)
        {
            try
            {
                int count = _feedbackQuestion.LCMSCount(search, isEmoji);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string? search = null, string? columnName = null)
        {
            try
            {
                List<CourseFeedbackAPI> feedbackQuestion = await _feedbackQuestion.GetPagination(page, pageSize, search, columnName);
                return Ok(feedbackQuestion);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCourseFeedback")]
        public async Task<IActionResult> GetCourseFeedback([FromBody] APICourseFeedbackSearch courseFeedbackSearch)
        {
            try
            {
                List<CourseFeedbackAPI> feedbackQuestion = await _feedbackQuestion.GetPagination(courseFeedbackSearch.Page, courseFeedbackSearch.PageSize, courseFeedbackSearch.Search, courseFeedbackSearch.ColumnName);
                return Ok(feedbackQuestion);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




        [HttpGet("ExportSearch/{showAllData?}/{isEmoji?}/{search?}/{columnName?}")]
        public async Task<IActionResult> ExportSearch(bool showAllData, string isEmoji, string search, string columnName)
        {
            try
            {
                List<CourseFeedbackAPI> feedbackQuestion = await this._feedbackQuestion.Get(1, 65536, UserId, RoleCode, showAllData, isEmoji, search);

                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"FeedbackQuestionBankWithData.xlsx";
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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("FeedbackQuestionBankWithData");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Question Text";
                    worksheet.Cells[1, 2].Value = "Options";
                    worksheet.Cells[1, 3].Value = "Option1";
                    worksheet.Cells[1, 4].Value = "Option2";
                    worksheet.Cells[1, 5].Value = "Option3";
                    worksheet.Cells[1, 6].Value = "Option4";
                    worksheet.Cells[1, 7].Value = "Option5";

                    worksheet.Cells[1, 8].Value = "Option6";
                    worksheet.Cells[1, 9].Value = "Option7";
                    worksheet.Cells[1, 10].Value = "Option8";
                    worksheet.Cells[1, 11].Value = "Option9";
                    worksheet.Cells[1, 12].Value = "Option10";
                    worksheet.Cells[1, 13].Value = "Status?";

                    int row = 2, column = 1;
                    foreach (CourseFeedbackAPI question in feedbackQuestion)
                    {
                        worksheet.Cells[row, column++].Value = question.QuestionText;

                        if (question.IsSubjective == true && question.IsEmoji == true)
                        {
                            worksheet.Cells[row, column++].Value = string.Empty;
                        }
                        else
                        {
                            worksheet.Cells[row, column++].Value = question.Options;
                        }
                        worksheet.Cells[row, column++].Value = question.Option1;
                        worksheet.Cells[row, column++].Value = question.Option2;
                        worksheet.Cells[row, column++].Value = question.Option3;
                        worksheet.Cells[row, column++].Value = question.Option4;
                        worksheet.Cells[row, column++].Value = question.Option5;

                        worksheet.Cells[row, column++].Value = question.Option6;
                        worksheet.Cells[row, column++].Value = question.Option7;
                        worksheet.Cells[row, column++].Value = question.Option8;
                        worksheet.Cells[row, column++].Value = question.Option9;
                        worksheet.Cells[row, column++].Value = question.Option10;

                        worksheet.Cells[row, column++].Value = question.IsActive == true ? "Active" : "Inactive";
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

        [HttpGet("getTotalRecords/{search:minlength(0)?}/{columnName?}")]
        public IActionResult GetCount(string? search = null, string? columnName = null)
        {
            try
            {
                int count = _feedbackQuestion.Count(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetTotalRecordCount")]
        public IActionResult GetTotalRecordCount([FromBody] APICourseFeedbackSearch courseFeedbackSearch)
        {
            try
            {
                int count = _feedbackQuestion.Count(courseFeedbackSearch.Search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetFeedbackQuestionReject")]
        public async Task<IEnumerable<FeedbackQuestionRejected>> GetFeedbackQuestionReject()
        {
            try
            {
                return await this._feedbackQuestionRejectedRepository.GetAll(e => e.IsDeleted == Record.NotDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpGet("GetAllFeedbackQuestionReject/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetUserReject(int page, int pageSize, string? search = null)
        {
            try
            {
                var feedbackQuestionReject = await this._feedbackQuestionRejectedRepository.GetAllFeedbackQuestionReject(page, pageSize, search);
                return Ok(feedbackQuestionReject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("FeedbackQuestionReject/GetTotalRecords/{search:minlength(0)?}")]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var userReject = await this._feedbackQuestionRejectedRepository.Count(search);
                return Ok(userReject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
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
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @FedbackImportField.ImportFeedback;
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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(FedbackImportField.FeedbackQuestion);
                    //First add the headers
                    worksheet.Cells[1, 1].Value = FedbackImportField.QuestionText + Record.Strar;
                    worksheet.Cells[1, 2].Value = FedbackImportField.IsEmoji + Record.Strar;
                    worksheet.Cells[1, 3].Value = FedbackImportField.IsSubjective + Record.Strar;
                    worksheet.Cells[1, 4].Value = FedbackImportField.NoOfOptions + Record.Hash;
                    worksheet.Cells[1, 5].Value = FedbackImportField.Option1 + Record.Hash;
                    worksheet.Cells[1, 6].Value = FedbackImportField.Option2 + Record.Hash;
                    worksheet.Cells[1, 7].Value = FedbackImportField.Option3;
                    worksheet.Cells[1, 8].Value = FedbackImportField.Option4;
                    worksheet.Cells[1, 9].Value = FedbackImportField.Option5;

                    worksheet.Cells[1, 10].Value = FedbackImportField.Option6;
                    worksheet.Cells[1, 11].Value = FedbackImportField.Option7;
                    worksheet.Cells[1, 12].Value = FedbackImportField.Option8;
                    worksheet.Cells[1, 13].Value = FedbackImportField.Option9;
                    worksheet.Cells[1, 14].Value = FedbackImportField.Option10;
                    worksheet.Cells[1, 15].Value = FedbackImportField.CourseCode;
                    worksheet.Cells[1, 16].Value = FedbackImportField.Metadata;


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

        async Task<bool> IsDuplicateObjectiveQuestion(FeedbackQuestion feedbackQuestion, List<FeedbackOption> feedbackOptions)
        {
            List<FeedbackQuestion> allFeedbackQuestions = (List<FeedbackQuestion>)await _feedbackQuestion.GetAll();
            List<FeedbackOption> allFeedbackOptions = (List<FeedbackOption>)await _feedbackOption.GetAll();

            List<FeedbackQuestion> tempDuplicate = allFeedbackQuestions.Where(q => q.QuestionText.Equals(feedbackQuestion.QuestionText, StringComparison.InvariantCultureIgnoreCase) && q.Id != feedbackQuestion.Id && q.IsDeleted == false).ToList<FeedbackQuestion>();
            if (tempDuplicate == null)
                return false;

            foreach (FeedbackQuestion currentFQ in tempDuplicate)
            {
                List<FeedbackOption> dbOptionList = allFeedbackOptions.Where(o => (o.FeedbackQuestionID == currentFQ.Id) && (o.IsDeleted == false)).ToList<FeedbackOption>();
                int similarOption = 0;
                foreach (FeedbackOption Option in feedbackOptions)
                {
                    var presentOption = dbOptionList.Where(o => o.OptionText.Equals(Option.OptionText, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault<FeedbackOption>();
                    if (presentOption != null)
                        similarOption++;
                    else
                        break;
                }
                if (similarOption == dbOptionList.Count && similarOption > 0)
                    return true;

            }
            return false;
        }


        [HttpPost]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> Post([FromBody] List<CourseFeedbackAPI> courseFeedbackApis)
        {
            try
            {
                List<CourseFeedbackAPI> errorFeedBacks = new List<CourseFeedbackAPI>();
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                else
                {
                    foreach (CourseFeedbackAPI courseFeedbackApi in courseFeedbackApis)
                    {
                        if (courseFeedbackApi.optionSelector != courseFeedbackApi.Options.Count())
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                        bool validvalue = FileValidation.CheckForSQLInjection(courseFeedbackApi.QuestionText);
                        if (validvalue == true)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }

                        //Check  Duplicate Option text
                        var duplicateOptionText = courseFeedbackApi.Options.GroupBy(x => x.option)
                                      .Where(g => g.Count() > 1)
                                      .Select(y => y.Count())
                                      .ToList();

                        if (duplicateOptionText.Count() > 0)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateOptionText), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateOptionText) });
                        }

                        FeedbackQuestion feedbackQuestion = new FeedbackQuestion();
                        feedbackQuestion.AnswersCounter = courseFeedbackApi.AnswersCounter;
                        feedbackQuestion.IsAllowSkipping = courseFeedbackApi.Skip;
                        feedbackQuestion.IsEmoji = courseFeedbackApi.IsEmoji;
                        feedbackQuestion.QuestionText = courseFeedbackApi.QuestionText;
                        feedbackQuestion.QuestionType = courseFeedbackApi.QuestionType;
                        feedbackQuestion.Section = courseFeedbackApi.Section;
                        feedbackQuestion.SubjectiveAnswerLimit = courseFeedbackApi.SubjectiveAnswerLimit;
                        feedbackQuestion.CreatedBy = UserId;
                        feedbackQuestion.CreatedDate = DateTime.UtcNow;
                        feedbackQuestion.ModifiedBy = UserId;
                        feedbackQuestion.ModifiedDate = DateTime.UtcNow;
                        feedbackQuestion.IsActive = courseFeedbackApi.IsActive;
                        feedbackQuestion.CourseId = courseFeedbackApi.CourseId;
                        feedbackQuestion.Metadata = courseFeedbackApi.Metadata;
                        if (courseFeedbackApi.QuestionType.Equals("objective"))
                        {
                            if (courseFeedbackApi.Options.Count(o => !string.IsNullOrEmpty(o.option)) < 2)
                            {
                                courseFeedbackApi.Error = "Minimum 2 options required";
                                errorFeedBacks.Add(courseFeedbackApi);
                            }
                            else
                            {
                                await _feedbackQuestion.Add(feedbackQuestion);
                                List<FeedbackOption> feedbackOptionsList = new List<FeedbackOption>();
                                var options = courseFeedbackApi.Options.Where(o => !string.IsNullOrEmpty(o.option));
                                foreach (Option opt in options)
                                {
                                    if (!string.IsNullOrEmpty(opt.option))
                                    {
                                        FeedbackOption feedbackOption = new FeedbackOption();
                                        feedbackOption.OptionText = opt.option;
                                        feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                        feedbackOption.CreatedBy = UserId;
                                        feedbackOption.CreatedDate = DateTime.UtcNow;
                                        feedbackOption.ModifiedBy = UserId;
                                        feedbackOption.ModifiedDate = DateTime.UtcNow;
                                        feedbackOption.Rating = opt.Rating;
                                        feedbackOptionsList.Add(feedbackOption);
                                    }
                                }

                                if (!await IsDuplicateObjectiveQuestion(feedbackQuestion, feedbackOptionsList))
                                    await _feedbackOption.AddRange(feedbackOptionsList);
                                else
                                {
                                    await _feedbackQuestion.Remove(feedbackQuestion);
                                    courseFeedbackApi.Error = "Question with Duplicate Option";
                                    errorFeedBacks.Add(courseFeedbackApi);
                                }
                            }
                        }
                        else if (courseFeedbackApi.QuestionType.Equals("subjective"))
                        {
                            if (courseFeedbackApi.Options.Count(o => !string.IsNullOrEmpty(o.option)) > 0)
                            {
                                courseFeedbackApi.Error = "Options are not required";
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                            }
                            if (courseFeedbackApi.SubjectiveAnswerLimit == 0 || courseFeedbackApi.SubjectiveAnswerLimit == null)
                            {
                                errorFeedBacks.Add(courseFeedbackApi);
                            }
                            else
                            {
                                await _feedbackQuestion.Add(feedbackQuestion);
                            }
                        }
                        else if (courseFeedbackApi.QuestionType.Equals("emoji"))
                        {
                            await _feedbackQuestion.Add(feedbackQuestion);
                        }
                    }
                }

                if (errorFeedBacks.Count != 0)
                    return Ok(errorFeedBacks);
                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + " Message:" + ex.Message });
            }
        }


        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                //Truncate UserMasterRejected Record    
                _feedbackQuestionRejectedRepository.Delete();
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


        [HttpPost]
        [Route("SaveFileData")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> PostFile([FromBody] APIFeedbackFilePath aPIFilePath)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                        string filefinal = sWebRootFolder + aPIFilePath.Path;
                        FileInfo file = new FileInfo(Path.Combine(filefinal));
                        string resultString = await this._feedbackQuestion.ProcessImportFile(file, _feedbackQuestion, _feedbackQuestionRejectedRepository, _feedbackOption, UserId);
                        return Ok(resultString);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        string exception = ex.Message;
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }

        }


        [HttpPost("{id}")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> Put(int id, [FromBody] CourseFeedbackAPI courseFeedbackApi)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bool validQuestion = FileValidation.CheckForSQLInjection(courseFeedbackApi.QuestionText);
                if (validQuestion == true)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (courseFeedbackApi.optionSelector != courseFeedbackApi.Options.Count())
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                //Check  Duplicate Option text
                var duplicateOptionText = courseFeedbackApi.Options.GroupBy(x => x.option)
                              .Where(g => g.Count() > 1)
                              .Select(y => y.Count())
                              .ToList();

                if (duplicateOptionText.Count() > 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateOptionText), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateOptionText) });
                }

                FeedbackQuestion feedbackQuestion = await _feedbackQuestion.Get(id);
                //Check Previous Question Type 
                if (courseFeedbackApi.QuestionType != feedbackQuestion.QuestionType)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (feedbackQuestion == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                List<CourseFeedbackAPI> errorFeedBacks = new List<CourseFeedbackAPI>();
                feedbackQuestion.AnswersCounter = courseFeedbackApi.AnswersCounter;
                feedbackQuestion.IsAllowSkipping = courseFeedbackApi.Skip;
                feedbackQuestion.QuestionText = courseFeedbackApi.QuestionText;
                feedbackQuestion.QuestionType = courseFeedbackApi.QuestionType;
                feedbackQuestion.Section = courseFeedbackApi.Section;
                feedbackQuestion.IsEmoji = courseFeedbackApi.IsEmoji;
                feedbackQuestion.SubjectiveAnswerLimit = courseFeedbackApi.SubjectiveAnswerLimit;
                //feedbackQuestion.CreatedBy = UserId;
                //feedbackQuestion.CreatedDate = DateTime.UtcNow;
                feedbackQuestion.ModifiedBy = UserId;
                feedbackQuestion.ModifiedDate = DateTime.UtcNow;
                feedbackQuestion.IsActive = courseFeedbackApi.IsActive;
                feedbackQuestion.IsDeleted = false;
                feedbackQuestion.CourseId = courseFeedbackApi.CourseId;
                feedbackQuestion.Metadata = courseFeedbackApi.Metadata;
                if (courseFeedbackApi.QuestionType.Equals("objective"))
                {
                    if (courseFeedbackApi.Options.Length < 2)
                    {
                        errorFeedBacks.Add(courseFeedbackApi);
                    }
                    else
                    {
                        List<FeedbackOption> feedbackOptionsList = new List<FeedbackOption>();
                        var options = courseFeedbackApi.Options.Where(o => !string.IsNullOrEmpty(o.option));
                        foreach (Option opt in options)
                        {
                            FeedbackOption feedbackOption = new FeedbackOption();
                            feedbackOption.OptionText = opt.option;
                            feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                            feedbackOption.CreatedBy = UserId;
                            feedbackOption.CreatedDate = feedbackQuestion.CreatedDate;
                            feedbackOption.ModifiedBy = UserId;
                            feedbackOption.ModifiedDate = DateTime.UtcNow;
                            feedbackOption.IsDeleted = false;
                            feedbackOption.Rating = opt.Rating;
                            feedbackOptionsList.Add(feedbackOption);
                        }
                        if (!await IsDuplicateObjectiveQuestion(feedbackQuestion, feedbackOptionsList))
                        {
                            IEnumerable<FeedbackOption> allOptions = await _feedbackOption.GetAll();
                            var oldOptions = allOptions.Where(o => o.FeedbackQuestionID == feedbackQuestion.Id).ToList<FeedbackOption>();
                            _feedbackOption.RemoveRange(oldOptions);
                            await _feedbackQuestion.Update(feedbackQuestion);
                            await _feedbackOption.UpdateRange(feedbackOptionsList);
                        }
                        else
                        {
                            courseFeedbackApi.Error = "Question with Duplicate Option";
                            errorFeedBacks.Add(courseFeedbackApi);
                        }
                    }
                }
                else if (courseFeedbackApi.QuestionType.Equals("subjective"))
                {
                    if (courseFeedbackApi.SubjectiveAnswerLimit == 0 || courseFeedbackApi.SubjectiveAnswerLimit == null)
                    {
                        await _feedbackQuestion.Update(feedbackQuestion);
                        errorFeedBacks.Add(courseFeedbackApi);
                    }
                    else
                    {
                        await _feedbackQuestion.Update(feedbackQuestion);
                        return Ok(feedbackQuestion);
                    }
                }
                else if (courseFeedbackApi.QuestionType.Equals("emoji"))
                {
                    await _feedbackQuestion.Update(feedbackQuestion);
                }
                return Ok(errorFeedBacks);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                FeedbackQuestion feedbackQuestion = await _feedbackQuestion.Get(DecryptedId);
                if (feedbackQuestion == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (await _feedbackStatusDetail.IsDependacyExist(DecryptedId))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                feedbackQuestion.IsDeleted = true;
                await _feedbackQuestion.Update(feedbackQuestion);
                List<FeedbackOption> feedbackOptions = await _feedbackOption.GetAll(f => f.FeedbackQuestionID == feedbackQuestion.Id);
                foreach (FeedbackOption feedbackOption in feedbackOptions)
                {
                    feedbackOption.ModifiedBy = UserId;
                    feedbackOption.IsDeleted = true;
                    await _feedbackOption.Update(feedbackOption);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [Route("QuestionExists")]
        public IActionResult QuestionExists([FromBody] APIQuestionsExits question)
        {

            return this.Ok(_feedbackQuestion.QuestionExists(question.Questiontext, question.Id));
        }


        [HttpGet("FeedbackStatus")]
        public async Task<IActionResult> FeedbackStatus()
        {
            try
            {
                List<FeedbackStatus> feedbackStatus = await _feedbackStatus.GetAll(f => f.IsDeleted == false);
                return Ok(feedbackStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("FeedbackStatus/{id}")]
        public async Task<IActionResult> FeedbackStatus(int id)
        {
            try
            {
                FeedbackStatus feedbackStatus = await _feedbackStatus.Get(id);
                return Ok(feedbackStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("FeedbackStatus/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public async Task<IActionResult> FeedbackStatus(int page, int pageSize, string? search = null, string? columnName = null)
        {
            try
            {

                List<FeedbackStatus> feedbackStatus = await _feedbackStatus.Get(page, pageSize, search);
                return Ok(feedbackStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("FeedbackStatus/getTotalRecords/{search:minlength(0)?}/{columnName?}")]
        public async Task<IActionResult> FeedbackStatusCount(string? search = null, string? columnName = null)
        {
            try
            {
                int count = await _feedbackStatus.Count(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("FeedbackStatus")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> FeedbackStatusPost([FromBody] FeedbackStatus feedbackStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                else
                {
                    feedbackStatus.CreatedBy = UserId;
                    feedbackStatus.CreatedDate = DateTime.UtcNow;
                    feedbackStatus.ModifiedBy = UserId;
                    feedbackStatus.ModifiedDate = DateTime.UtcNow;
                    await _feedbackStatus.Add(feedbackStatus);
                }
                return Ok(feedbackStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("FeedbackStatus/{id}")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> FeedbackStatusPut(int id, [FromBody] FeedbackStatus feedbackStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                else
                {
                    FeedbackStatus feedStatus = await _feedbackStatus.Get(id);
                    if (feedStatus == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                    feedbackStatus.ModifiedBy = UserId;
                    feedbackStatus.ModifiedDate = DateTime.UtcNow;
                    await _feedbackStatus.Update(feedbackStatus);

                }
                return Ok(feedbackStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete("FeedbackStatus")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> FeedbackStatusDelete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                FeedbackStatus feedbackStatus = await _feedbackStatus.Get(DecryptedId);
                if (feedbackStatus == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                feedbackStatus.ModifiedBy = UserId;
                feedbackStatus.ModifiedDate = DateTime.UtcNow;
                feedbackStatus.IsDeleted = true;
                await _feedbackStatus.Update(feedbackStatus);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("FeedbackStatusDetail")]
        public async Task<IActionResult> FeedbackStatusDetail()
        {
            try
            {
                List<FeedbackStatusDetail> feedbackStatusDetails = await _feedbackStatusDetail.GetAll(f => f.IsDeleted == false);
                return Ok(feedbackStatusDetails);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpGet("FeedbackStatusDetail/{id}")]
        public async Task<IActionResult> FeedbackStatusDetail(int id)
        {
            try
            {
                FeedbackStatusDetail feedbackStatusDetail = await _feedbackStatusDetail.Get(id);
                return Ok(feedbackStatusDetail);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("FeedbackStatusDetail/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public async Task<IActionResult> FeedbackStatusDetail(int page, int pageSize, string? search = null, string? columnName = null)
        {

            try
            {
                List<FeedbackStatusDetail> feedbackStatusDetails = await _feedbackStatusDetail.Get(page, pageSize, search);
                return Ok(feedbackStatusDetails);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("FeedbackStatusDetail/getTotalRecords/{search:minlength(0)?}/{columnName?}")]
        public async Task<IActionResult> FeedbackStatusDetailCount(string? search = null, string? columnName = null)
        {
            try
            {
                int count = await _feedbackStatusDetail.Count(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("SubmitFeedback")]
        public async Task<IActionResult> FeedbackStatusDetailPost([FromBody] List<SubmitFeedback> SubmitFeedbacks)
        {
            try
            {
                FeedbackStatus feedbackStatus = new FeedbackStatus();

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                else
                {
                    int CourseId = SubmitFeedbacks.Select(c => c.CourseId).FirstOrDefault();
                    int ModuleId = SubmitFeedbacks.Select(c => c.ModuleId).FirstOrDefault();
                    int? DPId = SubmitFeedbacks.Select(c => c.DPId).FirstOrDefault();
                    bool IsOJT = SubmitFeedbacks.Select(c => c.IsOJT).FirstOrDefault();
                    if (_feedbackStatus.Exists(CourseId, ModuleId, UserId, DPId, IsOJT))
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                    foreach (SubmitFeedback submitFeedbackAns in SubmitFeedbacks)
                    {
                        if (DPId == null)
                        {
                            if (await _feedbackStatus.IsContentCompletedforFeeback(this.UserId, submitFeedbackAns.CourseId, submitFeedbackAns.ModuleId, IsOJT) == false)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Incomplete), Description = EnumHelper.GetEnumDescription(MessageType.Incomplete) });
                            }
                        }
                        else
                        {
                            if (await _feedbackStatus.IsDevPlanCompletedforFeeback(this.UserId, submitFeedbackAns.DPId) == false)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Incomplete), Description = EnumHelper.GetEnumDescription(MessageType.Incomplete) });
                            }
                        }
                        bool validvalue = false;
                        if (!string.IsNullOrEmpty(submitFeedbackAns.SubjectiveAnswer))
                        {
                            if (FileValidation.CheckForSQLInjection(submitFeedbackAns.SubjectiveAnswer))
                                validvalue = true;
                            if (validvalue == true)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                            }
                        }
                    }

                    feedbackStatus.Status = Status.Completed;
                    feedbackStatus.CreatedBy = UserId;
                    feedbackStatus.CreatedDate = DateTime.Now;
                    feedbackStatus.ModifiedBy = UserId;
                    feedbackStatus.ModifiedDate = DateTime.Now;
                    feedbackStatus.CourseId = CourseId;
                    feedbackStatus.ModuleId = ModuleId;
                    feedbackStatus.DPId = DPId;
                    feedbackStatus.IsOJT = IsOJT;
                    await _feedbackStatus.Add(feedbackStatus);
                    if (DPId == null)
                    {
                        if (ModuleId == 0)
                        {
                            await this._feedbackStatus.AddCourseCompleteionStatus(UserId, CourseId, ModuleId, OrganisationCode);
                            await this._rewardsPointRepository.AddCourseFeedbackReward(UserId, feedbackStatus.Id, CourseId, OrganisationCode);
                        }
                        else
                        {
                            await this._feedbackStatus.AddModuleCompleteionStatus(UserId, CourseId, ModuleId, OrganisationCode);
                            await this._rewardsPointRepository.AddModuleFeedbackReward(UserId, feedbackStatus.Id, CourseId, ModuleId, OrganisationCode);
                        }
                    }

                    foreach (SubmitFeedback submitFeedback in SubmitFeedbacks)
                    {
                        FeedbackStatusDetail feedbackStatusDetail = new FeedbackStatusDetail();

                        if (submitFeedback.QuestionType.ToLower().Equals("objective"))
                            feedbackStatusDetail.FeedBackOptionID = submitFeedback.OptionID;


                        if (submitFeedback.QuestionType.ToLower().Equals("subjective"))
                            feedbackStatusDetail.SubjectiveAnswer = submitFeedback.SubjectiveAnswer;

                        feedbackStatusDetail.FeedbackStatusID = feedbackStatus.Id;
                        feedbackStatusDetail.FeedBackQuestionID = submitFeedback.QuestionID;
                        feedbackStatusDetail.CreatedBy = UserId;
                        feedbackStatusDetail.CreatedDate = DateTime.UtcNow;
                        feedbackStatusDetail.ModifiedBy = UserId;
                        feedbackStatusDetail.ModifiedDate = DateTime.UtcNow;
                        feedbackStatusDetail.Rating = submitFeedback.Rating;
                        await _feedbackStatusDetail.Add(feedbackStatusDetail);

                        if (OrganisationCode.ToLower() == "ivp")
                            if (submitFeedback.QuestionType.ToLower().Equals("objective"))
                                await this._feedbackStatusDetail.AddForFeedbackAggregationReport(submitFeedback, UserName);

                    }

                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("SubmitFeedback/{id}")]
        public async Task<IActionResult> FeedbackStatusDetailPut(int id, [FromBody] SubmitFeedback submitFeedback)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                else
                {
                    FeedbackStatusDetail feedStatusDetail = await _feedbackStatusDetail.Get(id);
                    if (feedStatusDetail == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                    bool validvalue = false;
                    if (!string.IsNullOrEmpty(submitFeedback.SubjectiveAnswer))
                    {
                        if (FileValidation.CheckForSQLInjection(submitFeedback.SubjectiveAnswer))
                            validvalue = true;
                        if (validvalue == true)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                    }


                    if (submitFeedback.QuestionType.ToLower().Equals("objective"))
                        feedStatusDetail.FeedBackOptionID = submitFeedback.OptionID;
                    if (submitFeedback.QuestionType.ToLower().Equals("subjective"))
                        feedStatusDetail.SubjectiveAnswer = submitFeedback.SubjectiveAnswer;
                    feedStatusDetail.FeedBackQuestionID = submitFeedback.QuestionID;
                    feedStatusDetail.ModifiedBy = UserId;
                    feedStatusDetail.ModifiedDate = DateTime.UtcNow;
                    feedStatusDetail.Rating = submitFeedback.Rating;
                    await _feedbackStatusDetail.Update(feedStatusDetail);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete("FeedbackStatusDetail")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> FeedbackStatusDetailDelete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                FeedbackStatusDetail feedbackStatusDetail = await _feedbackStatusDetail.Get(DecryptedId);
                if (feedbackStatusDetail == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                feedbackStatusDetail.ModifiedBy = UserId;
                feedbackStatusDetail.ModifiedDate = DateTime.UtcNow;
                feedbackStatusDetail.IsDeleted = true;
                await _feedbackStatusDetail.Update(feedbackStatusDetail);
                FeedbackStatus feedbackStatus = await _feedbackStatus.Get(feedbackStatusDetail.FeedbackStatusID);
                feedbackStatus.IsDeleted = true;
                feedbackStatus.ModifiedBy = UserId;
                feedbackStatus.ModifiedDate = DateTime.UtcNow;
                await _feedbackStatus.Update(feedbackStatus);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw;
            }
        }


        [HttpPost("Feedback")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> Feedback([FromBody] List<APIFeedbackConfigurationSheet> aPIFeedbackConfigurationSheet)
        {
            try
            {
                int? ConfigurationfeedbackId = null;
                List<APIFeedbackConfigurationSheet> errorAssessment = new List<APIFeedbackConfigurationSheet>();
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    foreach (APIFeedbackConfigurationSheet aPIAssessmentQuestion in aPIFeedbackConfigurationSheet)
                    {
                        FeedbackSheetConfiguration coursefeedback = new FeedbackSheetConfiguration();
                        coursefeedback.CreatedBy = UserId;
                        coursefeedback.CreatedDate = DateTime.UtcNow;
                        coursefeedback.ModifiedBy = UserId;
                        coursefeedback.ModifiedDate = DateTime.UtcNow;
                        coursefeedback.IsEmoji = aPIAssessmentQuestion.IsEmoji;
                        await _feedbackSheetConfiguration.Add(coursefeedback);
                        ConfigurationfeedbackId = coursefeedback.Id;

                        List<FeedbackSheetConfigurationDetails> FeedbackSheetDetails = new List<FeedbackSheetConfigurationDetails>();
                        var QList = (from qlist in aPIAssessmentQuestion.aPIFeedbackConfiguration
                                     group qlist by qlist.FeedbackId into q
                                     select q.FirstOrDefault()).ToList();
                        int SequenceNumber = 1;
                        foreach (APIFeedbackConfiguration opt in QList)
                        {
                            FeedbackSheetConfigurationDetails feedbackCourse = new FeedbackSheetConfigurationDetails();
                            feedbackCourse.FeedbackId = opt.FeedbackId;
                            feedbackCourse.ConfigurationSheetId = coursefeedback.Id;
                            feedbackCourse.CreatedBy = UserId;
                            feedbackCourse.CreatedDate = DateTime.UtcNow;
                            feedbackCourse.ModifiedDate = DateTime.UtcNow;
                            feedbackCourse.ModifiedBy = UserId;
                            feedbackCourse.SequenceNumber = SequenceNumber;
                            FeedbackSheetDetails.Add(feedbackCourse);
                            SequenceNumber++;
                        }
                        await _feedbackSheetConfigurationDetails.AddRange(FeedbackSheetDetails);
                    }
                }
                return Ok(new { ConfigurationfeedbackId });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage
                {
                    Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                    Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                });
            }
        }


        [HttpGet("GetFeedbackQuestions/{configurationId}")]
        public async Task<IActionResult> GetFeedbackQuestions(int configurationId)
        {
            try
            {

                List<CourseFeedbackAPI> CourseFeedback = await _feedbackStatusDetail.GetFeedback(configurationId);
                return Ok(CourseFeedback);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                    });
            }
        }


        [HttpGet("GetByConfigurationID/{ConfigurationID}")]
        public async Task<IActionResult> GetByConfigurationID(int ConfigurationID)
        {

            return Ok(await _feedbackSheetConfiguration.GetEditFeedbackConfigurationID(ConfigurationID));
        }


        [HttpPost("FeedbackQuestionEdit/{id}")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> FeedbackQuestionEdit(int id, [FromBody] APIFeedbackConfigurationSheet aPIAssessmentsQuestion)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                FeedbackSheetConfiguration FeedbackSheetConfiguration = await _feedbackSheetConfiguration.Get(aPIAssessmentsQuestion.FeedbackConfiID);
                if (FeedbackSheetConfiguration == null)
                    return BadRequest(new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.NotExist),
                        Description = EnumHelper.GetEnumDescription(MessageType.NotExist)
                    });

                FeedbackSheetConfiguration.IsEmoji = aPIAssessmentsQuestion.IsEmoji;
                await _feedbackSheetConfiguration.Update(FeedbackSheetConfiguration);

                //Removing Duplicate options
                var QList = (from qlist in aPIAssessmentsQuestion.aPIFeedbackConfiguration
                             group qlist by qlist.FeedbackId into q
                             select q.FirstOrDefault()).ToList();

                await _feedbackSheetConfigurationDetails.DeleteQuestionsByConfigId(FeedbackSheetConfiguration.Id);
                List<FeedbackSheetConfigurationDetails> feedbackQuestion = new List<FeedbackSheetConfigurationDetails>();
                int SequenceNumber = 1;
                foreach (APIFeedbackConfiguration opt in QList)
                {
                    FeedbackSheetConfigurationDetails feedbackQuestions = new FeedbackSheetConfigurationDetails();
                    feedbackQuestions.FeedbackId = opt.FeedbackId;
                    feedbackQuestions.ConfigurationSheetId = FeedbackSheetConfiguration.Id;
                    feedbackQuestions.CreatedBy = UserId;
                    feedbackQuestions.CreatedDate = DateTime.UtcNow;
                    feedbackQuestions.ModifiedDate = DateTime.UtcNow;
                    feedbackQuestions.ModifiedBy = UserId;
                    feedbackQuestions.SequenceNumber = SequenceNumber;
                    feedbackQuestion.Add(feedbackQuestions);
                    SequenceNumber++;
                }
                await _feedbackSheetConfigurationDetails.AddRange(feedbackQuestion);
                return Ok(FeedbackSheetConfiguration.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                     new ResponseMessage
                     {
                         Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                         Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                     });
            }
        }
        private async Task<IActionResult> DeleteQuestions(string ids)
        {

            int[] QuestionIds = ids.Split(",".ToCharArray()).Select(x => System.Int32.Parse(x.ToString())).ToArray();
            await _feedbackQuestion.DeleteQuestion(QuestionIds, UserId);
            return Ok();
        }


        [HttpDelete("DeleteConfiguredQuestions")]
        [PermissionRequired(Permissions.FeedbackQuestion)]
        public async Task<IActionResult> DeleteConfiguredQuestions([FromQuery] string configId, string QuestionIds)
        {
            try
            {
                int DecryptedconfigId = Convert.ToInt32(Security.Decrypt(configId));
                string DecryptedQuestionIds = Security.Decrypt(QuestionIds);

                int[] Questions = DecryptedQuestionIds.Split(",".ToCharArray()).Select(x => System.Int32.Parse(x.ToString())).ToArray();
                await _feedbackSheetConfigurationDetails.DeleteConfiguredQuestions(Questions, DecryptedconfigId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpGet("IsFeedbackSubmited/{CourseId}/{moduleId}/{IsOJT?}")]
        public async Task<IActionResult> IsOJTFeedbackSubmited(int courseId, int moduleId, bool IsOJT = false)
        {
            try
            {
                return Ok(await _feedbackStatus.IsFeedbackSubmitted(courseId, moduleId, UserId, IsOJT));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                     new ResponseMessage
                     {
                         Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                         Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                     });
            }
        }

        [HttpGet("IsFeedbackSubmited/{CourseId}/{moduleId}")]
        public async Task<IActionResult> IsFeedbackSubmited(int courseId, int moduleId, bool IsOJT = false)
        {
            try
            {
                return Ok(await _feedbackStatus.IsFeedbackSubmitted(courseId, moduleId, UserId, IsOJT = false));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                     new ResponseMessage
                     {
                         Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                         Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                     });
            }
        }

        [HttpGet("IsIdpFeedbackSubmited/{DpId}")]
        public async Task<IActionResult> IsIdpFeedbackSubmited(int DpId)
        {
            try
            {
                return Ok(await _feedbackStatus.IsIdpFeedbackSubmited(DpId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                     new ResponseMessage
                     {
                         Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                         Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                     });
            }
        }


        [HttpPost("MultiDeleteFeedbackQuestion")]

        public async Task<IActionResult> MultiDeleteFeedbackQuestion([FromBody] APIDeleteFeedbackQuestion[] apideletemultipleque)
        {
            try
            {
                ApiResponse Responce = await this._feedbackQuestion.MultiDeleteFeedbackQuestion(apideletemultipleque);
                return Ok(Responce.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("SubmitFinalFeedback")]
        public async Task<IActionResult> FeedbackfinalPost([FromBody] SubmitFinalFeedback SubmitFinalFeedbacks)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                else
                {
                    int CourseId = SubmitFinalFeedbacks.CourseId;
                    int ModuleId = SubmitFinalFeedbacks.ModuleId;

                    FeedbackStatus feedbackStatus = await _feedbackStatus.FeedbackStatusExists(CourseId, ModuleId, UserId);

                    if (feedbackStatus != null)
                    {

                        feedbackStatus.Status = Status.Completed;
                        feedbackStatus.ModifiedBy = UserId;
                        feedbackStatus.ModifiedDate = DateTime.UtcNow;

                        await _feedbackStatus.Update(feedbackStatus);
                    }
                    if (ModuleId == 0)
                    {
                        await this._feedbackStatus.AddCourseCompleteionStatus(UserId, CourseId, ModuleId, OrganisationCode);
                        await this._rewardsPointRepository.AddCourseFeedbackReward(UserId, feedbackStatus.Id, CourseId, OrganisationCode);
                    }
                    else
                    {
                        await this._feedbackStatus.AddModuleCompleteionStatus(UserId, CourseId, ModuleId, OrganisationCode);
                        await this._rewardsPointRepository.AddModuleFeedbackReward(UserId, feedbackStatus.Id, CourseId, ModuleId, OrganisationCode);
                    }



                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




        [HttpPost("SubmitFeedbackQuestion")]
        public async Task<IActionResult> FeedbackQuestionStatusDetailPost([FromBody] List<SubmitFeedback> SubmitFeedbacks)
        {
            try
            {
                FeedbackStatus feedbackStatus = new FeedbackStatus();

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                else
                {
                    int CourseId = SubmitFeedbacks.Select(c => c.CourseId).FirstOrDefault();
                    int ModuleId = SubmitFeedbacks.Select(c => c.ModuleId).FirstOrDefault();

                    FeedbackStatus fs = await _feedbackStatus.FeedbackStatusExists(CourseId, ModuleId, UserId);

                    if (fs != null)
                    {

                        foreach (SubmitFeedback submitFeedbackAns in SubmitFeedbacks)
                        {

                            bool validvalue = false;
                            if (!string.IsNullOrEmpty(submitFeedbackAns.SubjectiveAnswer))
                            {
                                if (FileValidation.CheckForSQLInjection(submitFeedbackAns.SubjectiveAnswer))
                                    validvalue = true;
                                if (validvalue == true)
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                                }
                            }
                        }

                        foreach (SubmitFeedback submitFeedback in SubmitFeedbacks)
                        {
                            FeedbackStatusDetail feedbackStatusDetail = new FeedbackStatusDetail();

                            if (submitFeedback.QuestionType.ToLower().Equals("objective"))
                                feedbackStatusDetail.FeedBackOptionID = submitFeedback.OptionID;


                            if (submitFeedback.QuestionType.ToLower().Equals("subjective"))
                                feedbackStatusDetail.SubjectiveAnswer = submitFeedback.SubjectiveAnswer;

                            feedbackStatusDetail.FeedbackStatusID = fs.Id;
                            feedbackStatusDetail.FeedBackQuestionID = submitFeedback.QuestionID;
                            feedbackStatusDetail.CreatedBy = UserId;
                            feedbackStatusDetail.CreatedDate = DateTime.UtcNow;
                            feedbackStatusDetail.ModifiedBy = UserId;
                            feedbackStatusDetail.ModifiedDate = DateTime.UtcNow;
                            feedbackStatusDetail.Rating = submitFeedback.Rating;
                            await _feedbackStatusDetail.Add(feedbackStatusDetail);


                        }
                    }
                    else
                    {

                        if (await _feedbackStatus.IsContentCompletedforFeeback(this.UserId, CourseId, ModuleId) == false)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Incomplete), Description = EnumHelper.GetEnumDescription(MessageType.Incomplete) });
                        }

                        foreach (SubmitFeedback submitFeedbackAns in SubmitFeedbacks)
                        {

                            bool validvalue = false;
                            if (!string.IsNullOrEmpty(submitFeedbackAns.SubjectiveAnswer))
                            {
                                if (FileValidation.CheckForSQLInjection(submitFeedbackAns.SubjectiveAnswer))
                                    validvalue = true;
                                if (validvalue == true)
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                                }
                            }
                        }

                        feedbackStatus.Status = Status.InProgress;
                        feedbackStatus.CreatedBy = UserId;
                        feedbackStatus.CreatedDate = DateTime.UtcNow;
                        feedbackStatus.ModifiedBy = UserId;
                        feedbackStatus.ModifiedDate = DateTime.UtcNow;
                        feedbackStatus.CourseId = CourseId;
                        feedbackStatus.ModuleId = ModuleId;
                        await _feedbackStatus.Add(feedbackStatus);
                        if (ModuleId == 0)
                        {
                            await this._feedbackStatus.AddCourseCompleteionStatus(UserId, CourseId, ModuleId, OrganisationCode);
                            await this._rewardsPointRepository.AddCourseFeedbackReward(UserId, feedbackStatus.Id, CourseId, OrganisationCode);
                        }
                        else
                        {
                            await this._feedbackStatus.AddModuleFeedbackCompleteionStatus(UserId, CourseId, ModuleId, OrganisationCode);
                            //await this._rewardsPointRepository.AddModuleFeedbackReward(UserId, feedbackStatus.Id, CourseId, ModuleId, OrganisationCode);
                        }

                        foreach (SubmitFeedback submitFeedback in SubmitFeedbacks)
                        {
                            FeedbackStatusDetail feedbackStatusDetail = new FeedbackStatusDetail();

                            if (submitFeedback.QuestionType.ToLower().Equals("objective"))
                                feedbackStatusDetail.FeedBackOptionID = submitFeedback.OptionID;


                            if (submitFeedback.QuestionType.ToLower().Equals("subjective"))
                                feedbackStatusDetail.SubjectiveAnswer = submitFeedback.SubjectiveAnswer;

                            feedbackStatusDetail.FeedbackStatusID = feedbackStatus.Id;
                            feedbackStatusDetail.FeedBackQuestionID = submitFeedback.QuestionID;
                            feedbackStatusDetail.CreatedBy = UserId;
                            feedbackStatusDetail.CreatedDate = DateTime.UtcNow;
                            feedbackStatusDetail.ModifiedBy = UserId;
                            feedbackStatusDetail.ModifiedDate = DateTime.UtcNow;
                            feedbackStatusDetail.Rating = submitFeedback.Rating;
                            await _feedbackStatusDetail.Add(feedbackStatusDetail);


                        }

                    }

                    // return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                    //if (_feedbackStatus.Exists(CourseId, ModuleId, UserId))
                    //  return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });


                    //if (OrganisationCode.ToLower() == "ivp")
                    //    if (submitFeedback.QuestionType.ToLower().Equals("objective"))
                    //        await this._feedbackStatusDetail.AddForFeedbackAggregationReport(submitFeedback, UserName);

                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetFeedbackAnsByConfigurationId/{configId}/{courseId}/{moduleId}")]
        public async Task<IActionResult> GetFeedbackAnsByConfigurationId(int configId, int courseId, int moduleId)
        {
            try
            {
                return Ok(await _feedbackQuestion.GetFeedbackAnsByConfigurationId(configId, moduleId, courseId, UserId, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetFeedbackQuestionData")]
        public async Task<IActionResult> GetV2([FromBody] APICourseFeedbackSearch aPICourseFeedbackSearch)
        {
            try
            {
                UserFeedbackQueTotalAPI feedbackQuestion = await _feedbackQuestion.GetPaginationV2(aPICourseFeedbackSearch.Page, aPICourseFeedbackSearch.PageSize, UserId, RoleCode, aPICourseFeedbackSearch.showAllData, aPICourseFeedbackSearch.Search, aPICourseFeedbackSearch.ColumnName, aPICourseFeedbackSearch.IsEmoji);
                return Ok(feedbackQuestion);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }
}
