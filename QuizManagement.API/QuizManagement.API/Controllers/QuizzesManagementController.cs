// ======================================
// <copyright file="QuizzesManagementController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using QuizManagement.API.APIModel;
using QuizManagement.API.Common;
using QuizManagement.API.Helper;
using QuizManagement.API.Models;
using QuizManagement.API.Repositories.Interfaces;
using QuizManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static QuizManagement.API.Common.AuthorizePermissions;
using static QuizManagement.API.Common.TokenPermissions;
using log4net;
using QuizManagement.API.Helper.Log_API_Count;

namespace QuizManagement.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class QuizzesManagementController : IdentityController
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QuizzesManagementController));
        private IQuizzesManagementRepository quizzesManagementRepository;
        private IQuizQuestionMasterRepository quizQuestionMasterRepository;
        private IQuizOptionMasterRepository quizOptionMasterRepository;
        private IQuizResultRepository quizResultRepository;
        private IQuizResultDetailRepository quizResultDetailRepository;
        private IRewardsPointRepository _rewardsPointRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;

        public QuizzesManagementController(IQuizzesManagementRepository quizzesManagementController,
            IQuizQuestionMasterRepository quizQuestionMasterController,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration confugu,
            IRewardsPointRepository rewardsPointRepository,
            IQuizOptionMasterRepository quizOptionMasterController,
            IQuizResultRepository quizResultController,
            IQuizResultDetailRepository quizResultDetailController,
            IIdentityService identitySvc,
            ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.quizzesManagementRepository = quizzesManagementController;
            this.quizQuestionMasterRepository = quizQuestionMasterController;
            this.quizOptionMasterRepository = quizOptionMasterController;
            this.quizResultRepository = quizResultController;
            this.quizResultDetailRepository = quizResultDetailController;
            this.hostingEnvironment = environment;
            this._configuration = confugu;
            this._httpContextAccessor = httpContextAccessor;
            this._identitySvc = identitySvc;
            this._rewardsPointRepository = rewardsPointRepository;
            this._tokensRepository = tokensRepository;
        }

        // GET: api/<controller>
        [HttpGet]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<QuizzesManagement> quizzesManagement = await this.quizzesManagementRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIQuizzesManagement>>(quizzesManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("QuizQuestionMaster")]
      //  [AllowAnonymous]
        public async Task<IActionResult> GetQuizQuestionMaster()
        {
            try
            {
                IEnumerable<APIQuizQuestionMergered> quizQuestionMaster = await this.quizQuestionMasterRepository.GetAllQuizQuestionMaster();
                return Ok((quizQuestionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("QuizOptionMaster")]
        public async Task<IActionResult> GetQuizOptionMaster()
        {
            try
            {
                List<QuizOptionMaster> quizOptionMaster = await this.quizOptionMasterRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIQuizQuestionMaster>>(quizOptionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<QuizzesManagement> quizzesManagement = await this.quizzesManagementRepository.GetAllQuizzesManagement(page, pageSize, search);
                return Ok(Mapper.Map<List<APIQuizzesManagement>>(quizzesManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllQuizzesManagementForEndUser")]
        public async Task<IActionResult> GetAllQuizzesManagementForEndUser()
        {
            try
            {
                IEnumerable<QuizzesManagement> quizzesManagement = await this.quizzesManagementRepository.GetAllQuizzesManagementForEndUser(UserId);
                return Ok(Mapper.Map<List<APIQuizzesManagement>>(quizzesManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllQuizzesManagementForEndUserCount")]
        public async Task<IActionResult> GetAllQuizzesManagementForEndUserCount()
        {
            try
            {
                int quizzesManagement = await this.quizzesManagementRepository.GetAllQuizzesManagementForEndUserCount(UserId);
                return Ok(quizzesManagement);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("QuizOptionMaster/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> GetQuizOptionMaster(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<QuizOptionMaster> quizOptionMaster = await this.quizOptionMasterRepository.GetAllQuizOptionMaster(page, pageSize, search);
                return Ok(Mapper.Map<List<APIQuizOptionMaster>>(quizOptionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int quizzesManagement = await this.quizzesManagementRepository.Count(search);
                return Ok(quizzesManagement);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("QuizQuestionMaster/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> GetQuizQuestionMaster(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<APIQuizQuestionMergered> quizQuestionMaster = await this.quizQuestionMasterRepository.GetAllQuizQuestionMaster(page, pageSize, search);
                return Ok((quizQuestionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("QuizQuestionMaster/GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> QuizQuestionMasterCount(string search)
        {
            try
            {
                int quizQuestionMaster = await this.quizQuestionMasterRepository.Count(search);
                return Ok(quizQuestionMaster);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("QuizOptionMaster/GetTotalRecords/{search:minlength(0)?}")]
        public async Task<IActionResult> QuizOptionMasterCount(string search)
        {
            try
            {
                int quizOptionMaster = await this.quizOptionMasterRepository.Count(search);
                return Ok(quizOptionMaster);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await this.quizzesManagementRepository.GetQuiz(id));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("QuizQuestionMaster/{id}")]
        public async Task<IActionResult> GetQuizQuestionMaster(int id)
        {
            try
            {
                QuizQuestionMaster quizQuestionMaster = await this.quizQuestionMasterRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APIQuizQuestionMaster>(quizQuestionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("QuizOptionMaster/{id}")]
        public async Task<IActionResult> GetQuizOptionMaster(int id)
        {
            try
            { 
            QuizOptionMaster quizOptionMaster = await this.quizOptionMasterRepository.Get(s => s.IsDeleted == false && s.Id == id);
            return Ok(Mapper.Map<APIQuizOptionMaster>(quizOptionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("Exist/{search:minlength(0)?}")]
        public async Task<bool> Exists(string search)
        {
            try
            {
                return await this.quizzesManagementRepository.Exist(search);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;  
            }
        }
        //[HttpGet("Exists/QuizQuestionMaster/{quizzId}/{search?}")]
        //public async Task<bool> ExistsQuizQuestionMaster(int quizzId, string search)
        //{

        //    return await this.quizQuestionMasterRepository.Exist(quizzId, search);
        //}[FromBody] JObject data

        [HttpPost("Exists/QuizQuestionMaster")]
       // [AllowAnonymous]
        public async Task<bool> ExistsQuizQuestionMaster([FromBody] QuizQuestionMaster data)
        {
            try
            {
                int quizzId = Convert.ToInt32(data.QuizId.ToString());
                string search = data.Question.ToString();
                // string search = data.Question("question").ToString();
                // int quizzId = quizId;
                return await this.quizQuestionMasterRepository.Exist(quizzId, search);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;      
            }
        }

        [HttpGet("GetQuizQuestionByQuizId/{surveyid}")]
        
        public async Task<IActionResult> GetQuizQuestionByQuizId(int surveyid)
        {
            try
            {
                IEnumerable<QuizQuestionMaster> questionMaster = await this.quizQuestionMasterRepository.GetAllQuizQuestion(surveyid);
                return Ok(Mapper.Map<List<APIQuizQuestionMaster>>(questionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuizOptionByQuizQuestion/{qustionId}")]
        
        public async Task<IActionResult> GetQuizOptionByQuizQuestion(int qustionId)
        {
            try
            {
                IEnumerable<QuizOptionMaster> quizOption = await this.quizOptionMasterRepository.GetAllQuizOption(qustionId);
                return Ok(Mapper.Map<List<APIQuizOptionMaster>>(quizOption));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByQuizId/{quizId}")]
       // [AllowAnonymous]
        public async Task<IActionResult> GetByQuizId(int quizId)
        {
            try
            {
                if (quizzesManagementRepository.Get(quizId) == null)
                {
                    return NotFound();
                }
                APIResponse response = await quizQuestionMasterRepository.GetQuizByQuizId(quizId, UserId);
                if (response.StatusCode == 200)
                {
                    return Ok(response.ResponseObject);
                }
                if (response.StatusCode == 119)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.EmptyQuiz), Description = response.Description });
                }
                if (response.StatusCode == 304)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, response.Description);
                    // return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = response.Description });
                }
                return NotFound();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetQuizzTyped/{quizz}")]
        [Produces("application/json")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> SearchQuizz(string quizz)
        {
            try
            {
                return this.Ok(await this.quizzesManagementRepository.SearchQuizz(quizz));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ExistsQuiz/{quizId:int}")]

        public async Task<bool> ExistsPoll(int quizId)
        {
            try
            {
                return await this.quizzesManagementRepository.ExistsQuiz(quizId, UserId);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;   
            }
        }
        // POST api/<controller>
        [HttpPost] //problem
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> Post([FromBody] APIQuizzesManagement aPIQuizzesManagement)
        {
            try
            {
                string GuidNo = null;
                if (ModelState.IsValid)
                {
                    GuidNo = Convert.ToString(Guid.NewGuid());
                    if ((aPIQuizzesManagement.ApplicabilityParameter != "0") && (aPIQuizzesManagement.ApplicabilityParameter == null || aPIQuizzesManagement.ApplicabilityParameterValueId == null))
                    //if ((aPIQuizzesManagement.ApplicabilityParameter == null || aPIQuizzesManagement.ApplicabilityParameter == "0") && (aPIQuizzesManagement.ApplicabilityParameterValue != null || aPIQuizzesManagement.ApplicabilityParameterValueId != null))
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    if ((aPIQuizzesManagement.ApplicabilityParameter == "UserId" || aPIQuizzesManagement.ApplicabilityParameter == "MobileNumber" || aPIQuizzesManagement.ApplicabilityParameter == "EmailId") && aPIQuizzesManagement.TargetResponseCount != 1)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    if (await this.quizzesManagementRepository.existQuiz(aPIQuizzesManagement.QuizTitle, null))
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }

                    QuizzesManagement quizzesManagement = new QuizzesManagement
                    {
                        QuizTitle = aPIQuizzesManagement.QuizTitle,
                        Date = DateTime.UtcNow
                    };
                    if (aPIQuizzesManagement.ApplicabilityParameter != "UserId" || aPIQuizzesManagement.ApplicabilityParameter != "MobileNumber" || aPIQuizzesManagement.ApplicabilityParameter != "EmailId")
                        quizzesManagement.TargetResponseCount = aPIQuizzesManagement.TargetResponseCount;
                    else
                        quizzesManagement.TargetResponseCount = 1;
                    quizzesManagement.Status = aPIQuizzesManagement.Status;
                    quizzesManagement.IsDeleted = false;
                    quizzesManagement.ModifiedBy = UserId;
                    quizzesManagement.ModifiedDate = DateTime.UtcNow;
                    quizzesManagement.CreatedBy = UserId;
                    quizzesManagement.CreatedDate = DateTime.UtcNow;

                    if (aPIQuizzesManagement.ApplicabilityParameter == "0" && aPIQuizzesManagement.ApplicabilityParameterValue == null)
                    {
                        quizzesManagement.IsApplicableToAll = true;
                    }
                    await quizzesManagementRepository.Add(quizzesManagement);
                    if (aPIQuizzesManagement.ApplicabilityParameter != null && aPIQuizzesManagement.ApplicabilityParameterValueId != null && quizzesManagement.IsApplicableToAll == false)
                    {
                        await quizzesManagementRepository.AddQuizApplicabilityParameter(quizzesManagement.Id, aPIQuizzesManagement.ApplicabilityParameter, aPIQuizzesManagement.ApplicabilityParameterValue, aPIQuizzesManagement.ApplicabilityParameterValueId.Value, GuidNo);
                    }
                    if (quizzesManagement.Status && quizzesManagement.IsApplicableToAll == true)
                    {
                        await quizzesManagementRepository.SendNotification(quizzesManagement.QuizTitle, Token, quizzesManagement.Id);
                    }
                    else
                    {
                        APINotifications Notification = new APINotifications();
                        Notification.Title = Record.QuizNotification;
                        Notification.Type = Record.Quiz;
                        string Url = this._configuration[Configuration.NotificationApi];
                        Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.QuizNotification;
                        HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
                        string result = await response.Content.ReadAsStringAsync();
                        Notification.Message = JsonConvert.DeserializeObject(result).ToString();
                        Notification.Message = Notification.Message.Replace("[quizTitle]", quizzesManagement.QuizTitle);
                        bool IsApplicableToAll = quizzesManagement.IsApplicableToAll;
                        Notification.Url = "social/";
                        Notification.UserId = Convert.ToInt32(aPIQuizzesManagement.ApplicabilityParameterValueId);
                        Notification.IsRead = false;
                        Notification.QuizId = quizzesManagement.Id;
                        if (quizzesManagement.Status == true)
                        {
                            int notificationID = await quizzesManagementRepository.SendNotificationForQuizAndSurvey(Notification, IsApplicableToAll);
                            await quizzesManagementRepository.SendNotificationForQuizSurvey(Notification, IsApplicableToAll,GuidNo, notificationID, UserId);
                        }
                    }
                    if (quizzesManagement.Status == true)
                    {
                        quizzesManagementRepository.SendQuizApplicabilityPushNotification(quizzesManagement.Id, OrgCode);
                    }
                   
                    return Ok(quizzesManagement);

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost("QuizQuestionMaster")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> PostQuizQuestionMaster([FromBody] List<APIQuizQuestionMergered> aPIQuizQuestionMergered)
        {
            try
            {
                List<APIQuizQuestionMergered> aPIQuizQuestionMergereds = new List<APIQuizQuestionMergered>();
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    foreach (APIQuizQuestionMergered quizQuestionMergered in aPIQuizQuestionMergered)
                    {
                        Boolean result = await this.quizzesManagementRepository.ExistsQuizInResult(quizQuestionMergered.QuizId);
                        if (quizQuestionMergered.Question != null)
                        {
                            quizQuestionMergered.Question = (quizQuestionMergered.Question).Trim();
                        }
                        if (String.IsNullOrEmpty(quizQuestionMergered.Question))
                        {
                            return StatusCode(400, "Question is empty!");
                        }

                        if (result == true)
                        {
                            return StatusCode(304, MessageType.QuizInUse);
                        }
                        Boolean questionExist = await this.quizzesManagementRepository.isQuestionExist(quizQuestionMergered.QuizId, quizQuestionMergered.Id, quizQuestionMergered.Question);
                        if (questionExist == true)
                        {
                            return StatusCode(409, "Question already exist!");
                        }

                        QuizQuestionMaster quizQuestionMaster = new QuizQuestionMaster();

                        //bool validvalue = false;
                        //if (FileValidation.CheckForSQLInjection(quizQuestionMergered.Question))
                        //    validvalue = true;

                        //else if (!string.IsNullOrEmpty(quizQuestionMergered.Hint))
                        //    if (FileValidation.CheckForSQLInjection(quizQuestionMergered.Hint))
                        //        validvalue = true;

                        //foreach (APIQuizOptionMaster options in quizQuestionMergered.aPIQuizOptionMaster)
                        //{
                        //    if (FileValidation.CheckForSQLInjection(options.AnswerText))
                        //        validvalue = true;
                        //}

                        List<string> termsList = new List<string>();
                        foreach (APIQuizOptionMaster options in quizQuestionMergered.aPIQuizOptionMaster)
                        {

                            if (options.AnswerText.Trim() != "")
                            {
                                termsList.Add(options.AnswerText.Trim());
                            }

                        }
                        string[] Quizoptions;
                        Quizoptions = termsList.ToArray();
                        if (Quizoptions.Distinct().Count() != Quizoptions.Count())
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateOptions), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateOptions) });
                        }

                        //if (validvalue == true)
                        //{
                        //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        //}

                        if (quizQuestionMergered.NoOfOption != quizQuestionMergered.aPIQuizOptionMaster.Count())
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }

                        quizQuestionMaster.Question = quizQuestionMergered.Question;
                        quizQuestionMaster.PicturePath = quizQuestionMergered.PicturePath;
                        quizQuestionMaster.QuizId = quizQuestionMergered.QuizId;
                        quizQuestionMaster.Hint = quizQuestionMergered.Hint;
                        quizQuestionMaster.Mark = quizQuestionMergered.Mark;
                        quizQuestionMaster.AnswersArePictures = quizQuestionMergered.AnswersArePictures;
                        quizQuestionMaster.RandomizeSequence = quizQuestionMergered.RandomizeSequence;
                        quizQuestionMaster.IsDeleted = false;
                        quizQuestionMaster.CreatedBy = UserId;
                        quizQuestionMaster.CreatedDate = DateTime.UtcNow;
                        quizQuestionMaster.ModifiedBy = UserId;
                        quizQuestionMaster.ModifiedDate = DateTime.UtcNow;

                        if (quizQuestionMergered.aPIQuizOptionMaster.Length < 2)
                        {
                            aPIQuizQuestionMergereds.Add(quizQuestionMergered);
                        }
                        else
                        {
                            await quizQuestionMasterRepository.Add(quizQuestionMaster);
                            List<QuizOptionMaster> quizOptionMasters = new List<QuizOptionMaster>();
                            foreach (APIQuizOptionMaster opt in quizQuestionMergered.aPIQuizOptionMaster)
                            {
                                QuizOptionMaster quizOptionMaster = new QuizOptionMaster
                                {
                                    QuizQuestionId = quizQuestionMaster.Id,
                                    AnswerText = opt.AnswerText,
                                    AnswerPicturePath = opt.AnswerPicturePath,
                                    IsCorrectAnswer = opt.IsCorrectAnswer,
                                    IsDeleted = false,
                                    CreatedBy = UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedBy = UserId,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                quizOptionMasters.Add(quizOptionMaster);
                            }
                            await quizOptionMasterRepository.AddRange(quizOptionMasters);
                        }

                    }
                }

                if (aPIQuizQuestionMergereds.Count != 0)
                    return Ok(aPIQuizQuestionMergereds);
                return Ok(aPIQuizQuestionMergered);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // POST api/<controller>
        [HttpPost("QuizResult")]
        public async Task<IActionResult> Post([FromBody] APIQuizResultMerged aPIQuizResultMerged)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    QuizzesManagement objQuiz = new QuizzesManagement();
                    objQuiz = await this.quizzesManagementRepository.Get(aPIQuizResultMerged.QuizId);
                    if (objQuiz.Status == false)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    List<QuizQuestionMaster> objConfig = new List<QuizQuestionMaster>();
                    objConfig = await this.quizzesManagementRepository.GetQuestionByQuizId(aPIQuizResultMerged.QuizId);

                    if (objConfig.Count() != aPIQuizResultMerged.aPIQuizResultDetail.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    APIQuizResultDetail[] objDetails = aPIQuizResultMerged.aPIQuizResultDetail;

                    int flag = 0;
                    foreach (QuizQuestionMaster valConfig in objConfig)
                    {
                        for (int i = 0; i < objDetails.Length; i++)
                        {
                            if (objDetails[i].QuizQuestionId == valConfig.Id)
                            {
                                flag = flag + 1;
                            }
                        }
                    }
                    if (flag != objConfig.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    bool result = await quizzesManagementRepository.isSubmittedQuiz(aPIQuizResultMerged.QuizId, UserId);
                    if (result == true)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateQuizResponse), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateQuizResponse) });
                    }
                    QuizResult quizResult = new QuizResult
                    {
                        QuizId = aPIQuizResultMerged.QuizId,
                        QuizResultStatus = "Completed",
                        UserId = UserId,
                        IsDeleted = false,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await quizResultRepository.Add(quizResult);

                    List<QuizResultDetail> quizResultDetails = new List<QuizResultDetail>();
                    foreach (APIQuizResultDetail opt in aPIQuizResultMerged.aPIQuizResultDetail)
                    {
                        QuizResultDetail quizResultDetail = new QuizResultDetail
                        {
                            QuizResultId = quizResult.Id,
                            QuizQuestionId = opt.QuizQuestionId,
                            QuizOptionId = opt.QuizOptionId,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow
                        };
                        quizResultDetails.Add(quizResultDetail);
                    }
                    await quizResultDetailRepository.AddRange(quizResultDetails);
                    await this._rewardsPointRepository.QuizAttemptedRewardPoint(UserId, quizResult.QuizId, false, objQuiz.QuizTitle, objQuiz.CreatedDate, OrgCode);
                    return Ok(aPIQuizResultMerged);

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // PUT api/<controller>/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> Put(int id, [FromBody] APIQuizzesManagement aPIQuizzesManagement)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                QuizzesManagement quizzesManagement = await this.quizzesManagementRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (quizzesManagement == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (quizzesManagement.QuizTitle != aPIQuizzesManagement.QuizTitle)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                //if ((aPIQuizzesManagement.ApplicabilityParameter == null || aPIQuizzesManagement.ApplicabilityParameter == "0") && (aPIQuizzesManagement.ApplicabilityParameterValue != null || aPIQuizzesManagement.ApplicabilityParameterValueId != null))
                //{
                //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                //}

                if ((aPIQuizzesManagement.ApplicabilityParameter != "0") && (aPIQuizzesManagement.ApplicabilityParameter == null || aPIQuizzesManagement.ApplicabilityParameterValueId == null))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                if ((aPIQuizzesManagement.ApplicabilityParameter == "UserId" || aPIQuizzesManagement.ApplicabilityParameter == "MobileNumber" || aPIQuizzesManagement.ApplicabilityParameter == "EmailId") && aPIQuizzesManagement.TargetResponseCount != 1)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                //if (quizzesManagement.QuizTitle != aPIQuizzesManagement.QuizTitle)
                //{
                //    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                //}

                if (await this.quizzesManagementRepository.existQuiz(aPIQuizzesManagement.QuizTitle, aPIQuizzesManagement.Id))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }

                Boolean result = await this.quizzesManagementRepository.ExistsQuizInResult(id);
                if (result == true)
                {
                    return StatusCode(304, MessageType.QuizInUse);
                }
                if (ModelState.IsValid && quizzesManagement != null)
                {
                    //quizzesManagement.QuizTitle = aPIQuizzesManagement.QuizTitle;
                    if (aPIQuizzesManagement.ApplicabilityParameter != "UserId" || aPIQuizzesManagement.ApplicabilityParameter != "MobileNumber" || aPIQuizzesManagement.ApplicabilityParameter != "EmailId")
                        quizzesManagement.TargetResponseCount = aPIQuizzesManagement.TargetResponseCount;
                    bool previousQuizStatus = quizzesManagement.Status;
                    quizzesManagement.Status = aPIQuizzesManagement.Status;
                    quizzesManagement.ModifiedBy = UserId;
                    quizzesManagement.ModifiedDate = DateTime.UtcNow;

                    // Changed the comparision to 0 as applicability parameters is sent 0 and not null.
                    if (aPIQuizzesManagement.ApplicabilityParameter == "0" && aPIQuizzesManagement.ApplicabilityParameterValue == null)
                        quizzesManagement.IsApplicableToAll = true;
                    else
                        quizzesManagement.IsApplicableToAll = false;

                    #region "Send notification when Survey status is made true from false"
                    if (previousQuizStatus == false && aPIQuizzesManagement.Status == true)
                    {
                        if (quizzesManagement.IsApplicableToAll == true)
                        {
                            APINotifications Notification = new APINotifications();
                            Notification.Title = Record.QuizNotification;
                            Notification.Type = Record.Quiz;
                            string Url = this._configuration[Configuration.NotificationApi];
                            Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.QuizNotification;
                            HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
                            string result1 = await response.Content.ReadAsStringAsync();
                            Notification.Message = JsonConvert.DeserializeObject(result1).ToString();
                            Notification.Message = Notification.Message.Replace("[quizTitle]", quizzesManagement.QuizTitle);
                            bool IsApplicableToAll = quizzesManagement.IsApplicableToAll;
                            Notification.Url = "social/";
                            Notification.UserId = Convert.ToInt32(aPIQuizzesManagement.ApplicabilityParameterValueId);
                            Notification.IsRead = false;
                            Notification.QuizId = quizzesManagement.Id;
                            await quizzesManagementRepository.SendNotificationForQuizAndSurvey(Notification, IsApplicableToAll);
                        }
                    }
                    
                    #region "Code added to remove Quiz Notification as the Quiz is now made inactive"
                    else if (previousQuizStatus == true && aPIQuizzesManagement.Status == false)
                    {

                        int notificationId = await quizzesManagementRepository.GetNotificationId(aPIQuizzesManagement.QuizTitle);
                        string Url = this._configuration[Configuration.NotificationApi];
                        Url = Url + "/tlsNotification/" + notificationId;
                        HttpResponseMessage response = await ApiHelper.CallDeleteAPI(Url, Token);
                    }
                    #endregion
                    #endregion

                    await this.quizzesManagementRepository.Update(quizzesManagement);
                    string GuidNo = Convert.ToString(Guid.NewGuid());
                    await quizzesManagementRepository.UpdateQuizApplicability(quizzesManagement.Id, aPIQuizzesManagement.ApplicabilityParameter, aPIQuizzesManagement.ApplicabilityParameterValue, aPIQuizzesManagement.ApplicabilityParameterValueId, GuidNo);
                    
                    if (quizzesManagement.Status == true && quizzesManagement.IsApplicableToAll==false)
                    {
                        APINotifications Notification = new APINotifications();
                        Notification.Title = Record.QuizNotification;
                        Notification.Type = Record.Quiz;
                        string Url = this._configuration[Configuration.NotificationApi];
                        Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.QuizNotification;
                        HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
                        string result1 = await response.Content.ReadAsStringAsync();
                        Notification.Message = JsonConvert.DeserializeObject(result1).ToString();
                        Notification.Message = Notification.Message.Replace("[quizTitle]", quizzesManagement.QuizTitle);
                        bool IsApplicableToAll = quizzesManagement.IsApplicableToAll;
                        Notification.Url = "social/";
                        Notification.UserId = Convert.ToInt32(aPIQuizzesManagement.ApplicabilityParameterValueId);
                        Notification.IsRead = false;
                        Notification.QuizId = quizzesManagement.Id;
                        if (quizzesManagement.Status == true)
                        {
                            int aPINotification1 = await quizzesManagementRepository.SendNotificationForQuizAndSurvey(Notification, IsApplicableToAll);
                            await quizzesManagementRepository.SendNotificationForQuizSurvey(Notification, IsApplicableToAll, GuidNo, aPINotification1, UserId);
                        }
                    }
                    if (quizzesManagement.Status == true)
                    {
                        quizzesManagementRepository.SendQuizApplicabilityPushNotification(quizzesManagement.Id, OrgCode);
                    }
                }
                return Ok(quizzesManagement);

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // PUT api/<controller>/5
        [HttpPost("QuizQuestionMaster/{id}")]
        //[AllowAnonymous]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> Put(int id, [FromBody] APIQuizQuestionMergered aPIQuizQuestionMergered)
        {
            try
            {
                Boolean result = await this.quizzesManagementRepository.ExistsQuizQuestionInResult(id);
                if (result == true)
                {
                    return StatusCode(304, MessageType.QuizInUse);
                }
                QuizQuestionMaster quizQuestionMaster = await quizQuestionMasterRepository.Get(id);
                if (quizQuestionMaster == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                List<APIQuizQuestionMergered> aPIQuizQuestionMergereds = new List<APIQuizQuestionMergered>();
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    //bool validvalue = false;

                    //if (FileValidation.CheckForSQLInjection(aPIQuizQuestionMergered.Question))
                    //    validvalue = true;


                    //else if (!string.IsNullOrEmpty(aPIQuizQuestionMergered.Hint))
                    //    if (FileValidation.CheckForSQLInjection(aPIQuizQuestionMergered.Hint))
                    //        validvalue = true;

                    //foreach (APIQuizOptionMaster options in aPIQuizQuestionMergered.aPIQuizOptionMaster)
                    //{
                    //    if (FileValidation.CheckForSQLInjection(options.AnswerText))
                    //        validvalue = true;
                    //}

                    //if (validvalue == true)
                    //{
                    //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    //}


                    if (aPIQuizQuestionMergered.Question != null)
                    {
                        aPIQuizQuestionMergered.Question = (aPIQuizQuestionMergered.Question).Trim();
                    }
                    if (String.IsNullOrEmpty(aPIQuizQuestionMergered.Question))
                    {
                        return StatusCode(400, "Question is empty!");
                    }

                    Boolean questionExist = await this.quizzesManagementRepository.isQuestionExist(aPIQuizQuestionMergered.QuizId, aPIQuizQuestionMergered.Id, aPIQuizQuestionMergered.Question);
                    if (questionExist == true)
                    {
                        return StatusCode(409, "Question already exist!");
                    }
                    quizQuestionMaster.Question = aPIQuizQuestionMergered.Question;
                    quizQuestionMaster.PicturePath = aPIQuizQuestionMergered.PicturePath;
                    quizQuestionMaster.QuizId = aPIQuizQuestionMergered.QuizId;
                    quizQuestionMaster.Hint = aPIQuizQuestionMergered.Hint;
                    quizQuestionMaster.Mark = aPIQuizQuestionMergered.Mark;
                    quizQuestionMaster.AnswersArePictures = aPIQuizQuestionMergered.AnswersArePictures;
                    quizQuestionMaster.RandomizeSequence = aPIQuizQuestionMergered.RandomizeSequence;
                    quizQuestionMaster.ModifiedBy = UserId;
                    quizQuestionMaster.ModifiedDate = DateTime.UtcNow;

                    if (aPIQuizQuestionMergered.aPIQuizOptionMaster.Length < 2)
                    {
                        aPIQuizQuestionMergereds.Add(aPIQuizQuestionMergered);
                    }
                    else
                    {
                        await quizQuestionMasterRepository.Update(quizQuestionMaster);
                        //  List<QuizOptionMaster> quizOptionMasters = new List<QuizOptionMaster>();
                        List<QuizOptionMaster> Options = await quizOptionMasterRepository.GetAll(g => g.QuizQuestionId == id);
                        await quizOptionMasterRepository.RemoveRange(Options);
                        foreach (APIQuizOptionMaster opt in aPIQuizQuestionMergered.aPIQuizOptionMaster)
                        {
                            if (opt != null)
                            {
                                QuizOptionMaster quizOptionMaster = new QuizOptionMaster
                                {
                                    QuizQuestionId = quizQuestionMaster.Id,
                                    AnswerText = opt.AnswerText,
                                    AnswerPicturePath = opt.AnswerPicturePath,
                                    IsCorrectAnswer = opt.IsCorrectAnswer,
                                    CreatedBy = UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedBy = UserId,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                await quizOptionMasterRepository.Add(quizOptionMaster);
                                // quizOptionMasters.Add(quizOptionMaster);
                            }

                        }
                        //await quizOptionMasterRepository.AddRange(quizOptionMasters);
                    }

                }
                return Ok(aPIQuizQuestionMergered);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // PUT api/<controller>/5
        [HttpPost("QuizOptionMaster/{id}")]
        public async Task<IActionResult> PutQuizOptionMaster(int id, [FromBody] APIQuizOptionMaster aPIQuizOptionMaster)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                QuizOptionMaster QuizOptionMaster = await this.quizOptionMasterRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (QuizOptionMaster == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (QuizOptionMaster != null)
                {
                    QuizOptionMaster.QuizQuestionId = Convert.ToInt16(aPIQuizOptionMaster.QuizQuestionId);
                    QuizOptionMaster.AnswerText = aPIQuizOptionMaster.AnswerText;
                    QuizOptionMaster.AnswerPicturePath = aPIQuizOptionMaster.AnswerPicturePath;
                    QuizOptionMaster.IsCorrectAnswer = aPIQuizOptionMaster.IsCorrectAnswer;
                    QuizOptionMaster.ModifiedBy = UserId;
                    QuizOptionMaster.ModifiedDate = DateTime.UtcNow;
                    await this.quizOptionMasterRepository.Update(QuizOptionMaster);
                }
                else
                {
                    return BadRequest(ModelState);
                }
                return Ok(aPIQuizOptionMaster);

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> PostLoginImagesUpload()
        {
            try
            {

                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                string pageType = string.IsNullOrEmpty(request.Form[Record.Option]) ? "4.5" : request.Form[Record.Option].ToString();
                string imageType = string.IsNullOrEmpty(request.Form[Record.ImageType]) ? Record.png : request.Form[Record.ImageType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        string fileDir = this._configuration["ApiGatewayLXPFiles"];
                        fileDir = Path.Combine(fileDir, OrgCode, "Social");
                        fileDir = Path.Combine(fileDir, pageType, imageType);
                        if (!Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + Record.png);
                        using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                        {
                            await fileUpload.CopyToAsync(fs);
                        }
                        if (String.IsNullOrEmpty(file))
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                        }
                        return Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                QuizzesManagement quizzesManagement = await this.quizzesManagementRepository.Get(DecryptedId);
                Boolean result = await this.quizzesManagementRepository.ExistsQuizInResult(DecryptedId);
                if (result == true)
                {
                    return StatusCode(304, MessageType.QuizInUse);
                }
                if (ModelState.IsValid && quizzesManagement != null)
                {
                    quizzesManagement.IsDeleted = true;
                    await this.quizzesManagementRepository.Update(quizzesManagement);

                    #region  "Code added to remove Quiz notificaition because the Quiz is now deleted"
                    int notificationId = await quizzesManagementRepository.GetNotificationId(quizzesManagement.QuizTitle);
                    string Url = this._configuration[Configuration.NotificationApi];
                    Url = Url + "/tlsNotification/" + notificationId;
                    HttpResponseMessage response = await ApiHelper.CallDeleteAPI(Url, Token);
                    #endregion
                }

                if (quizzesManagement == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        // DELETE api/<controller>/5
        //[HttpDelete("QuizQuestionMaster/{id}")]
        //public async Task<IActionResult> QuizQuestionMasterDelete(int id)
        //{
        //    QuizQuestionMaster quizQuestionMaster = await this.quizQuestionMasterRepository.Get(id);

        //    if (ModelState.IsValid && quizQuestionMaster != null)
        //    {
        //        quizQuestionMaster.IsDeleted = true;
        //        await this.quizQuestionMasterRepository.Update(quizQuestionMaster);
        //    }

        //    if (quizQuestionMaster == null)
        //        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
        //    return this.Ok();

        //}
        [HttpGet("GetQuizQuestion/{questionId:int}")]
       // [AllowAnonymous]
       // [PermissionRequired(Permissions.QuizManagement)]

        public async Task<IActionResult> GetSurveyQuestions(int questionId)
        {
            try
            {

                APIQuizQuestionMergered SurveyQuestion = await quizQuestionMasterRepository.GetQuiz(questionId);
                return Ok(SurveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError)
                    });
            }
        }

        [HttpDelete("QuizQuestionMaster")]
        [PermissionRequired(Permissions.QuizManagement)]
        public async Task<IActionResult> DeleteQuizQuestionMaster([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Boolean isQuestionDeleted = await this.quizzesManagementRepository.isQuestionDeleted(DecryptedId);
                if (isQuestionDeleted == true)
                {
                    //return StatusCode(304,"Question already deleted!");
                    return this.BadRequest(new ResponseMessage { Message = "Question already deleted!" });
                }

                QuizQuestionMaster quizQuestionMaster = await quizQuestionMasterRepository.Get(DecryptedId);
                if (quizQuestionMaster == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                Boolean result = await this.quizzesManagementRepository.ExistsQuizQuestionInResult(quizQuestionMaster.Id);
                if (result == true)
                {
                    return StatusCode(304, MessageType.QuizInUse);
                }

                quizQuestionMaster.IsDeleted = true;
                await quizQuestionMasterRepository.Update(quizQuestionMaster);
                List<QuizOptionMaster> quizOptionMasters = await quizOptionMasterRepository.GetAll(f => f.QuizQuestionId == quizQuestionMaster.Id);
                foreach (QuizOptionMaster feedbackOption in quizOptionMasters)
                {
                    feedbackOption.IsDeleted = true;
                    await quizOptionMasterRepository.Update(feedbackOption);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        /// <summary>
        /// Search specific quizzesManagement.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<QuizzesManagement> quizzesManagement = await this.quizzesManagementRepository.Search(q);
                return Ok(Mapper.Map<List<APIQuizzesManagement>>(quizzesManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        /// <summary>
        /// Search specific QuizQuestionMaster.
        /// </summary>
        [HttpGet]
        [Route("QuizQuestionMaster/Search/{q}")]
        public async Task<IActionResult> QuizQuestionMasterSearch(string q)
        {
            try
            {
                IEnumerable<QuizQuestionMaster> quizQuestionMaster = await this.quizQuestionMasterRepository.Search(q);
                return Ok(Mapper.Map<List<APIQuizQuestionMaster>>(quizQuestionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        /// <summary>
        /// Search specific QuizQuestionMaster.
        /// </summary>
        [HttpGet]
        [Route("QuizOptionMaster/Search/{q}")]
        public async Task<IActionResult> QuizOptionMasterSearch(string q)
        {
            try
            {
                IEnumerable<QuizOptionMaster> quizOptionMaster = await this.quizOptionMasterRepository.Search(q);
                return Ok(Mapper.Map<List<APIQuizOptionMaster>>(quizOptionMaster));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TypeAhead/{search?}")]
        public async Task<IActionResult> GetTypeAHead(string search = null)
        {

            try
            {
                search = search.Trim();

                List<TypeAhead> course = await this.quizzesManagementRepository.GetTypeAHead(search);
                return Ok(course);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuizQuestionTypeAhead/{quizeid:int}")]
        public async Task<IActionResult> GetQuizQuestion(int quizeid)
        {
            try
            {
                APIResponse responce = await this.quizzesManagementRepository.GetQuizQuestion(quizeid);
                if (responce.StatusCode == 200)
                {
                    return Ok(responce.ResponseObject);
                }
                return NotFound();

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TypeAheadQuizReport/{search?}")]
        public async Task<IActionResult> GetTypeAHeadQuizReport(string search = null)
        {

            try
            {
                //List<TypeAhead> course = await this.quizzesManagementRepository.GetTypeAHeadQuizReport(search);
                return this.Ok(await this.quizzesManagementRepository.GetTypeAHeadQuizReport(search));
                //if (course.Count == 0)
                //{
                //    return Ok();
                //}
                //return Ok(course);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuizQuestionOptionDetails/{QuizID}")]
        public async Task<IActionResult> GetQuizQuestionOptionDetails(int QuizID)
        {
            try
            {
                return Ok(await this.quizzesManagementRepository.GetQuizQuestionOptionDetails(QuizID, UserId));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }
}
