using AspNet.Security.OAuth.Introspection;
using Evaluation.API.APIModel;
using Evaluation.API.APIModel;
using Evaluation.API.Common;
using Evaluation.API.Controllers;
using Evaluation.API.Helper;
using Evaluation.API.Repositories.Interfaces;
using Evaluation.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Evaluation.API.Model;
using Evaluation.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Evaluation.API.Common.AuthorizePermissions;
using static Evaluation.API.Common.TokenPermissions;
using log4net;
using System.IO;
using Evaluation.API.Model.Log_API_Count;

namespace Evaluation.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/c/ProcessEvaluation")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ProcessEvaluationController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessEvaluationController));
        private IProcessEvaluationQuestion _processEvaluationQuestion;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        public IConfiguration _configuration { get; set; }
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProcessEvaluationController(
            IHostingEnvironment environment,
            IConfiguration configure,
            IHttpContextAccessor httpContextAccessor,
            IProcessEvaluationQuestion processEvaluationQuestion,
        IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this._processEvaluationQuestion = processEvaluationQuestion;
            this._configuration = configure;
            this._identitySvc = _identitySvc;
            this.hostingEnvironment = environment;
            this._httpContextAccessor = httpContextAccessor;
            this._tokensRepository = tokensRepository;

        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}/{isMemoQuestions?}")]
        [PermissionRequired(Permissions.Processevaluation)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null, bool? isMemoQuestions = null)
        {
            try
            {
                IEnumerable<ProcessEvaluationQuestion> questionMaster = await this._processEvaluationQuestion.GetAllQuestionPagination(page, pageSize, search, columnName, isMemoQuestions);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetQuestionForEvaluation")]
        public async Task<IActionResult> GetQuestionForEvaluation()
        {
            try
            {
                IEnumerable<APIProcessEvaluationQuestion> questionMaster = await this._processEvaluationQuestion.GetQuestionForEvaluation();
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuestionForOEREvaluation")]
        public async Task<IActionResult> GetQuestionForOEREvaluation()
        {
            try
            {
                IEnumerable<APIProcessEvaluationQuestion> questionMaster = await this._processEvaluationQuestion.GetQuestionForOEREvaluation();
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetQuestionForEvaluationforChaiPoint")]
        public async Task<IActionResult> GetQuestionForEvaluationforChaiPoint([FromBody] apiGetQuestionforChaipoint apiGetQuestionforChaipoint)
        {
            try
            {
                IEnumerable<APIProcessEvaluationQuestion> questionMaster = await this._processEvaluationQuestion.GetQuestionForEvaluationforChaiPoint(apiGetQuestionforChaipoint);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuestionForPMSEvaluation/{Section?}")]
        public async Task<IActionResult> GetQuestionForPMSEvaluation(string? Section)
        {
            try
            {
                IEnumerable<PMSEvaluationPointResponse> questionMaster = await this._processEvaluationQuestion.GetQuestionForPMSEvaluation(Section, UserId);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuestionForKitchenAudit")]
        public async Task<IActionResult> GetQuestionForKitchenAudit()
        {
            try
            {
                IEnumerable<APIProcessEvaluationQuestion> questionMaster = await this._processEvaluationQuestion.GetQuestionForKitchenAudit();
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("GetTransactionID")]
        public async Task<IActionResult> GetTransactionID([FromBody] apiGetTransactionID apigettransactionID)
        {
            try
            {
                StringBuilder TransID = await this._processEvaluationQuestion.GetTransactionID(apigettransactionID, LoginId);
                return Ok(TransID.ToString());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetTransactionIDForOER")]
        public async Task<IActionResult> GetTransactionIDForOER([FromBody] apiGetTransactionID apigettransactionID)
        {
            try
            {
                StringBuilder TransID = await this._processEvaluationQuestion.GetTransactionIDForOER(apigettransactionID, LoginId);
                return Ok(TransID.ToString());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetProcessManagement")]
        public async Task<IActionResult> GetProcessManagement()
        {
            try
            {
                IEnumerable<APIProcessEvaluationManagement> questionMaster = await this._processEvaluationQuestion.GetProcessManagement(OrganisationCode, UserId);
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("LastSubmitedProcessResult")]
        public async Task<IActionResult> LastSubmitedProcessResult([FromBody] APILastSubmitedResult aPIgetresult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                APIPostProcessEvaluationDisplay response = await this._processEvaluationQuestion.LastSubmitedProcessResult(aPIgetresult, OrganisationCode);

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("LastSubmitedProcessResultKitchenAudit")]
        public async Task<IActionResult> LastSubmitedProcessResultKitchenAudit([FromBody] APILastSubmitedResultKitchenAudit aPIgetresult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                APIPostProcessEvaluationDisplay response = await this._processEvaluationQuestion.LastSubmitedProcessResultKitchenAudit(aPIgetresult, OrganisationCode);

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostProcessResult")]
        public async Task<IActionResult> PostProcessResult([FromBody] APIPostProcessEvaluationResult aPIPostProcessResult)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                ApiResponse response = await this._processEvaluationQuestion.PostProcessResult(aPIPostProcessResult, UserId, OrganisationCode);
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

        [HttpPost("PostProcessResultForChaipoint")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> PostProcessResultForChaipoint([FromBody] APIPostProcessEvaluationResultForChaipoint aPIPostProcessResult)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                ApiResponse response = await this._processEvaluationQuestion.PostProcessResultForChaipoint(aPIPostProcessResult, UserId, OrganisationCode);
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
        #region OERPostProcessResult
        [HttpPost("PostOERProcessResult")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> PostOERProcessResult([FromBody] APIPostProcessEvaluationResult aPIPostProcessResult)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                ApiResponse response = await this._processEvaluationQuestion.PostOERProcessResult(aPIPostProcessResult, UserId, OrganisationCode);
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
        #endregion
        #region LastSubmittedOERProcessResult
        [HttpPost("LastSubmitedOERProcessResult")]
        public async Task<IActionResult> LastSubmitedOERProcessResult([FromBody] APILastSubmitedResult aPIgetresult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                APIPostOERProcessEvaluationDisplay response = await this._processEvaluationQuestion.LastSubmitedOERProcessResult(aPIgetresult, OrganisationCode);

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion

        [HttpPost("GetSubmmitedId")]
        public async Task<IActionResult> GetSubmmitedId([FromBody] APIGetSubmmitedId aPIGetSubmmitedId)
        {
            try
            {
                int Id = await this._processEvaluationQuestion.PMSEvaluationSumbit(UserId, aPIGetSubmmitedId.ManagerId);
                return Ok(Id);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostPMSEvaluationResult")]
        public async Task<IActionResult> PostPMSEvaluationResult([FromBody] List<APIPMSEvaluationResult> aPIPMSEvaluationResult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await this._processEvaluationQuestion.PostPMSEvaluationResult(aPIPMSEvaluationResult, OrganisationCode);

                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost("GetPendingPMSEvaluation")]
        public async Task<IActionResult> GetPendingPMSEvaluation([FromBody] APIUserId aPIUser)
        {
            try
            {
                int i = Convert.ToInt32(Security.DecryptForUI(aPIUser.UserId));
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultList = await this._processEvaluationQuestion.GetPendingPMSEvaluation(i);

                return Ok(resultList);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetPendingPMSEvaluationById/{id}")]
        public async Task<IActionResult> GetPendingPMSEvaluationById(int id)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultList = await this._processEvaluationQuestion.GetPendingPMSEvaluationById(id);

                return Ok(resultList);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("PostPMSEvaluationResultManager")]
        public async Task<IActionResult> PostPMSEvaluationResultManager([FromBody] List<APIPMSEvaluationResultManager> aPIPMSEvaluationResult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await this._processEvaluationQuestion.PostPMSEvaluationResultManager(aPIPMSEvaluationResult);

                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("IsEvaluationSubmitted")]
        public async Task<IActionResult> IsEvaluationAvailable()
        {
            try
            {

                bool result = await this._processEvaluationQuestion.Exist(UserId);

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost]
        [Route("PostFileUpload")]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.FileType : request.Form[Record.FileType].ToString();

                if (request.Form.Files.Count > 0)
                {
                    List<string> imagePaths = new List<string>();
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }

                        if (FileValidation.IsValidImage(fileUpload))
                        {

                            string fileDir = this._configuration["ApiGatewayLXPFiles"];
                            fileDir = Path.Combine(fileDir, OrgCode);
                            fileDir = Path.Combine(fileDir, FileType.Image);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName.Trim());
                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            imagePaths.Add(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                        }
                        else
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }
                    return Ok(imagePaths);
                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        #region Critical Audit Evaluation

        [HttpGet("GetQuestionForCriticalAuditEvaluation")]
        public async Task<IActionResult> GetQuestionForCriticalAuditEvaluation()
        {
            try
            {
                IEnumerable<APICriticalAuditQuestion> questionMaster = await this._processEvaluationQuestion.GetQuestionForCriticalAuditEvaluation();
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostProcessCriticalAuditResult")]
        public async Task<IActionResult> PostProcessCriticalAuditResult([FromBody] APIPostProcessEvaluationResult aPIPostProcessResult)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                ApiResponse response = await this._processEvaluationQuestion.PostProcessCriticalAuditResult(aPIPostProcessResult, UserId, OrganisationCode);
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

        [HttpPost("LastSubmittedCriticalAuditResult")]
        public async Task<IActionResult> LastSubmittedCriticalAuditResult([FromBody] APILastSubmitedResult aPIgetresult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                APIPostProcessEvaluationDisplay response = await this._processEvaluationQuestion.LastSubmittedCriticalAuditResult(aPIgetresult, OrganisationCode);

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion


        #region Night Audit Evaluation

        [HttpGet("GetQuestionForNightAuditEvaluation")]
        public async Task<IActionResult> GetQuestionForNightAuditEvaluation()
        {
            try
            {
                IEnumerable<APINightAuditQuestion> questionMaster = await this._processEvaluationQuestion.GetQuestionForNightAuditEvaluation();
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostProcessNightAuditResult")]
        public async Task<IActionResult> PostProcessNightAuditResult([FromBody] APIPostProcessEvaluationResult aPIPostProcessResult)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                ApiResponse response = await this._processEvaluationQuestion.PostProcessNightAuditResult(aPIPostProcessResult, UserId, OrganisationCode);
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

        [HttpPost("LastSubmittedNightAuditResult")]
        public async Task<IActionResult> LastSubmittedNightAuditResult([FromBody] APILastSubmitedResult aPIgetresult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                APIPostProcessEvaluationDisplay response = await this._processEvaluationQuestion.LastSubmittedNightAuditResult(aPIgetresult, OrganisationCode);

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion


        #region Ops Audit Evaluation

        [HttpGet("GetQuestionForOpsAuditEvaluation")]
        public async Task<IActionResult> GetQuestionForOpsAuditEvaluation()
        {
            try
            {
                IEnumerable<APIOpsAuditQuestion> questionMaster = await this._processEvaluationQuestion.GetQuestionForOpsAuditEvaluation();
                return Ok(questionMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostProcessOpsAuditResult")]
        public async Task<IActionResult> PostProcessOpsAuditResult([FromBody] APIPostProcessEvaluationResult aPIPostProcessResult)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                ApiResponse response = await this._processEvaluationQuestion.PostProcessOpsAuditResult(aPIPostProcessResult, UserId, OrganisationCode);
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

        [HttpPost("LastSubmittedOpsAuditResult")]
        public async Task<IActionResult> LastSubmittedOpsAuditResult([FromBody] APILastSubmitedResult aPIgetresult)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                APIPostProcessEvaluationDisplay response = await this._processEvaluationQuestion.LastSubmittedOpsAuditResult(aPIgetresult, OrganisationCode);

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion

    }
}