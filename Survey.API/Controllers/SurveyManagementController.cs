// ======================================
// <copyright file="SurveyManagementController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Survey.API.APIModel;
using Survey.API.Common;
using Survey.API.Helper;
using Survey.API.Metadata;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using Survey.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Survey.API.Common.AuthorizePermissions;
using static Survey.API.Common.TokenPermissions;
using log4net;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Survey.API.Helper.Log_API_Count;

namespace Survey.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class SurveyManagementController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SurveyManagementController));
        private ISurveyManagementRepository surveyManagementRepository;
        private ISurveyConfigurationRepository surveyConfigurationRepository;
        private ISurveyQuestionRepository surveyQuestionRepository;
        private ISurveyOptionRepository surveyOptionRepository;
        private ISurveyOptionNestedRepository surveyOptionNestedRepository;
        private ISurveyResultRepository surveyResultRepository;
        private ISurveyResultDetailRepository surveyResultDetailRepository;
        private IQuizzesManagementRepository quizzesManagementRepository;
        public IConfiguration _configuration { get; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identitySvc;
        private readonly IRewardsPointRepository _rewardsPointRepository;
        private readonly ITokensRepository _tokensRepository;
        private ISurveyQuestionRejectedRepository _surveyQuestionRejectedRepository;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        public SurveyManagementController(ISurveyManagementRepository surveyManagementController
            , ISurveyConfigurationRepository surveyConfigurationRepository
            , ISurveyQuestionRepository surveyQuestionController
            , IHttpContextAccessor httpContextAccessor
            , IWebHostEnvironment environment
            , IConfiguration confugu
            , ISurveyOptionRepository surveyOptionController,
            ISurveyOptionNestedRepository surveyOptionNestedRepository,
            ISurveyResultRepository surveyResultController,
            ISurveyResultDetailRepository surveyResultDetailController,
            ISurveyQuestionRejectedRepository surveyQuestionRejectedRepository,
            IRewardsPointRepository rewardsPointRepository,
            IIdentityService identitySvc,
           IQuizzesManagementRepository quizzesManagementController,
            ICustomerConnectionStringRepository customerConnectionStringRepository,
        ITokensRepository tokensRepository) : base(identitySvc)
        {
           this.quizzesManagementRepository = quizzesManagementController;
            this.surveyManagementRepository = surveyManagementController;
            this.surveyConfigurationRepository = surveyConfigurationRepository;
            this.surveyQuestionRepository = surveyQuestionController;
            this.surveyOptionRepository = surveyOptionController;
            this.surveyOptionNestedRepository = surveyOptionNestedRepository;
            this.surveyResultRepository = surveyResultController;
            this.surveyResultDetailRepository = surveyResultDetailController;
            this.hostingEnvironment = environment;
            this._configuration = confugu;
            this._httpContextAccessor = httpContextAccessor;
            this._identitySvc = identitySvc;
            this._rewardsPointRepository = rewardsPointRepository;
            this._tokensRepository = tokensRepository;
            this._surveyQuestionRejectedRepository = surveyQuestionRejectedRepository;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
        }

        // GET: api/<controller>
        [HttpGet]

       [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<SurveyManagement> surveyManagement = await this.surveyManagementRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APISurveyManagement>>(surveyManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyQuestion")]
        public async Task<IActionResult> GetSurveyQuestion()
        {
            try
            {
                List<SurveyQuestion> surveyQuestion = await this.surveyQuestionRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APISurveyQuestion>>(surveyQuestion));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyOption")]

        public async Task<IActionResult> GetSurveyOption()
        {
            try
            {
                List<SurveyOption> surveyOption = await this.surveyOptionRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APISurveyOption>>(surveyOption));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.SurveyManagement)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<SurveyManagement> surveyManagement = await this.surveyManagementRepository.GetAllSurveyManagement(UserId, UserRole, page, pageSize, search);
                return Ok(Mapper.Map<List<APISurveyManagement>>(surveyManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("SurveyQuestion/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> GetSurveyQuestion(int page, int pageSize, string search = null)
        {
            try
            {
                List<APISurveyMergeredModel> surveyQuestion = await this.surveyQuestionRepository.GetAllSurveyQuestion(UserId, UserRole, page, pageSize, search);
                return Ok(surveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("NestedSurveyQuestion/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> NestedSurveyQuestion(int page, int pageSize, string search = null)
        {
            try
            {
                List<APISurveyMergeredModel> surveyQuestion = await this.surveyQuestionRepository.GetNestedSurveyQuestion(UserId, UserRole, page, pageSize, search);
                return Ok(surveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("SurveyQuestion/GetSurveyQuestion")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> GetSurveyQuestion([FromBody] ApiSurveyQuestionSearch surveyQuestionSearch)
        {
            try
            {
                List<APISurveyMergeredModel> surveyQuestion = await this.surveyQuestionRepository.GetAllSurveyQuestion(UserId, UserRole, surveyQuestionSearch.Page, surveyQuestionSearch.PageSize, surveyQuestionSearch.Search);
                return Ok(surveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyOption/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetSurveyOption(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<SurveyOption> surveyOption = await this.surveyOptionRepository.GetAllSurveyOption(page, pageSize, search);
                return Ok(Mapper.Map<List<APISurveyOption>>(surveyOption));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.SurveyManagement)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int surveyManagement = await this.surveyManagementRepository.Count(UserId, UserRole, search);
                return Ok(surveyManagement);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("SurveyQuestion/GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> SurveyQuestionCount(string search)
        {
            try
            {
                int surveyQuestion = await this.surveyQuestionRepository.Count(UserId, UserRole, search);
                return Ok(surveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        
        [HttpGet("NestedSurveyQuestion/GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> NestedSurveyQuestionCount(string search)
        {
            try
            {
                int surveyQuestion = await this.surveyQuestionRepository.NestedCount(UserId, UserRole, search);
                return Ok(surveyQuestion);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("SurveyQuestion/GetTotalRecords")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> SearchSurveyQuestions([FromBody] ApiSurveyQuestionSearch surveyQuestionSearch)
        {
            try
            {
                int surveyQuestion = await this.surveyQuestionRepository.Count(UserId, UserRole, surveyQuestionSearch.Search);
                return Ok(surveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyOption/GetTotalRecords/{search:minlength(0)?}")]
        public async Task<IActionResult> SurveyOptionCount(string search)
        {
            try
            {
                int surveyOption = await this.surveyOptionRepository.Count(search);
                return Ok(surveyOption);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // GET api/<controller>/5
        [HttpGet("{id}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await this.surveyManagementRepository.GetSurveyMangementLcms(id));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("SurveyQuestion/{id}")]
        public async Task<IActionResult> GetSurveyQuestion(int id)
        {
            try
            {
                SurveyQuestion surveyQuestion = await this.surveyQuestionRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APISurveyQuestion>(surveyQuestion));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("SurveyOption/{id}")]
        public async Task<IActionResult> GetSurveyOption(int id)
        {
            try
            {
                SurveyOption surveyOption = await this.surveyOptionRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APISurveyOption>(surveyOption));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Exists/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<bool> Exists(string search)
        {
            try
            {
                return await this.surveyManagementRepository.Exist(search);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;  
            }
        }
        [HttpGet("SurveyQuestion/Exists/id/{search:minlength(0)?}")]
        public async Task<bool> ExistsSurveyQuestion(int id, string search)
        {
            try
            {
                return await this.surveyQuestionRepository.Exist(id, search);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;      
            }
        }
        [HttpPost("SurveyQuestion/QuestionExists")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> SurveyQuestionExists([FromBody] ApiSurveyQuestionExist surveyQuestion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this.surveyQuestionRepository.QuestionExist(surveyQuestion.Question, surveyQuestion.Section));
                }
                else
                    return Ok(ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyOption/Exists/id/{search:minlength(0)?}")]
        public async Task<bool>ExistsSurveyOption(int id, string search)
        {
            try
            {
                return await this.surveyOptionRepository.Exist(id, search);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;   
            }
        }

        [HttpGet("GetAllSurveyManagementForEndUser")]
        public async Task<IActionResult> GetAllSurveyManagementForEndUser()
        {
            try
            {
                IEnumerable<SurveyManagement> surveyManagement = await this.surveyManagementRepository.GetAllSurveyManagement(UserId);
                return Ok(Mapper.Map<List<APISurveyManagement>>(surveyManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllSurveyManagementForEndUserCount")]
        public async Task<IActionResult> GetAllSurveyManagementForEndUserCount()
        {
            try
            {
                int surveyManagement = await this.surveyManagementRepository.GetAllSurveyManagementCount(UserId);
                return Ok(surveyManagement);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetSurveyQuestionBySurveyId/{surveyid}")]
        public async Task<IActionResult> GetSurveyQuestionBySurveyId(int surveyid)
        {
            try
            {
                IEnumerable<SurveyQuestion> surveyQuestion = await this.surveyQuestionRepository.GetAllSurveyQuestion(surveyid);
                return Ok(Mapper.Map<List<APISurveyQuestion>>(surveyQuestion));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetSurveyOptionBySurveyQuestion/{qustionId}")]
        public async Task<IActionResult> GetSurveyOptionBySurveyQuestion(int qustionId)
        {
            try
            {
                IEnumerable<SurveyOption> surveyOption = await this.surveyOptionRepository.GetAllSurveyOption(qustionId);
                return Ok(Mapper.Map<List<APISurveyOption>>(surveyOption));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // GET: api/values
        [HttpGet("SurveyQuestion/SurveyTyped/{survey:minlength(0)?}")]
        [Produces("application/json")]
        public async Task<IActionResult> SearchSurvey(string survey)
        {
            try
            {
                return this.Ok(await this.surveyManagementRepository.SearchSurvey(survey));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyQuestion/SurveyTypedTODO/{survey:minlength(0)?}")]
        [Produces("application/json")]
        public async Task<IActionResult> SearchSurveyTODO(string survey)
        {
            try
            {
                return this.Ok(await this.surveyManagementRepository.SearchSurveyTODO(survey));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("ExistsSurvey/{surveyId:int}")]
        public async Task<bool> ExistsPoll(int surveyId)
        {
            try
            {
                return await this.surveyManagementRepository.ExistsSurvey(surveyId, UserId);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;    
            }
        }

        [HttpGet("GetSurveyQuestions/{questionId:int}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> GetSurveyQuestions(int questionId)
        {
            try
            {

                APISurveyMergeredModel SurveyQuestion = await surveyQuestionRepository.GetSurvey(questionId);
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

        [HttpGet("GetBySurveyId/{surveyId}")]
        public async Task<IActionResult> GetBySurveyId(int surveyId)
        {
            try
            {

                // return Ok(await surveyQuestionRepository.GetSurveyBySurveyId(surveyId,UserId));

                APIResponse response = await surveyQuestionRepository.GetSurveyBySurveyId(surveyId, UserId);
                if (response.StatusCode == 200)
                {
                    return Ok(response.ResponseObject);
                }
                if (response.StatusCode == 119)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = response.Description });
                }
                if (response.StatusCode == 304)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, response.Description);
                }
                return NotFound();

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

        [HttpGet("GetMultipleBySurveyId/{surveyId}")]
        public async Task<IActionResult> GetMultipleBySurveyId(int surveyId)
        {
            try
            {

                // return Ok(await surveyQuestionRepository.GetSurveyBySurveyId(surveyId,UserId));

                APIResponse response = await surveyQuestionRepository.GetMultipleBySurveyId(surveyId, UserId, OrgCode);
                if (response.StatusCode == 200)
                {
                    return Ok(response.ResponseObject);
                }
                if (response.StatusCode == 119)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = response.Description });
                }
                if (response.StatusCode == 304)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, response.Description);
                }
                return NotFound();

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
        // POST api/<controller>
        [HttpPost]
        [PermissionRequired(Permissions.SurveyManagement)]
        public async Task<IActionResult> Post([FromBody] APISurveyManagement aPISurveyManagement)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SurveyManagement objSurvey = await this.surveyManagementRepository.CheckForDuplicate(aPISurveyManagement.SurveySubject);
                    if (objSurvey != null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }

                    SurveyManagement surveyManagement = new SurveyManagement
                    {
                        Date = DateTime.UtcNow,
                        SurveySubject = aPISurveyManagement.SurveySubject.Trim(),
                        StartDate = aPISurveyManagement.StartDate,
                        ValidityDate = aPISurveyManagement.ValidityDate,
                        SurveyPurpose = aPISurveyManagement.SurveyPurpose,
                        AverageRespondTime = aPISurveyManagement.AverageRespondTime
                    };
                    surveyManagement.TargetResponseCount = aPISurveyManagement.TargetResponseCount;
                    surveyManagement.Status = aPISurveyManagement.Status;
                    surveyManagement.IsDeleted = false;
                    surveyManagement.ModifiedBy = UserId;
                    surveyManagement.ModifiedDate = DateTime.UtcNow;
                    surveyManagement.CreatedBy = UserId;
                    surveyManagement.CreatedDate = DateTime.UtcNow;
                    surveyManagement.LcmsId = aPISurveyManagement.LcmsId;
                    surveyManagement.IsApplicableToAll = aPISurveyManagement.IsApplicableToAll;

                    await surveyManagementRepository.Add(surveyManagement);
                    int surveymanagementId = surveyManagement.Id;
                    if (surveyManagement.Status && surveyManagement.IsApplicableToAll)
                    {
                        await surveyManagementRepository.SendNotification(surveyManagement.SurveySubject, Token, surveyManagement.Id);
                        var val1 = surveyManagementRepository.SendSurveyApplicabilityPushNotification(surveymanagementId, OrgCode);
                    }
                    return Ok(surveyManagement);
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
        [HttpPost("SurveyQuestion")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> PostSurveyQuestion([FromBody] List<APISurveyMergeredModel> aPISurveyMergeredModel)
        {
            try
            {

                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                foreach (APISurveyMergeredModel surveyMergeredModelAPI in aPISurveyMergeredModel)
                {
                    if (await this.surveyQuestionRepository.existsQuestion(surveyMergeredModelAPI.Question, null))
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }

                    if (surveyMergeredModelAPI.Section == "Objective")
                    {
                        if (surveyMergeredModelAPI.options < 2 || surveyMergeredModelAPI.options > 10)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidOptionRange), Description = EnumHelper.GetEnumDescription(MessageType.InvalidOptionRange) });
                        }
                        if (surveyMergeredModelAPI.options != surveyMergeredModelAPI.aPISurveyOption.Count())
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.OptionAndOptionsMissMatch), Description = EnumHelper.GetEnumDescription(MessageType.OptionAndOptionsMissMatch) });
                        }
                    }
                    else
                    {
                        surveyMergeredModelAPI.options = 0; //Subjective 
                    }
                }
                APIResponse response = await surveyQuestionRepository.AddSurvey(aPISurveyMergeredModel, UserId);
                if (response.StatusCode == 400)
                {
                    return BadRequest(new
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InvalidData),
                        Description = response.Description,
                        InvalidObject = response.ResponseObject
                    });
                }
                return Ok(response);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost("SurveyResult")]
        public async Task<IActionResult> Postold([FromBody] APISurveyQuestionOption aPISurveyResultMerged)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool applicabletouser = false;
                    bool isAlreadySubmitted = false;

                    SurveyManagement objSurvey = new SurveyManagement();
                    objSurvey = await this.surveyManagementRepository.Get(aPISurveyResultMerged.surveyResultId);
                    if (objSurvey.Status == false)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    List<SurveyConfiguration> objConfig = new List<SurveyConfiguration>();
                    objConfig = await this.surveyManagementRepository.GetDetailsFromSuveryID(objSurvey.LcmsId);

                    if (objConfig.Count() != aPISurveyResultMerged.answerDeatils.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    APISurveyAnswerDetails[] objDetails = aPISurveyResultMerged.answerDeatils;


                    int flag = 0;
                    foreach (SurveyConfiguration valConfig in objConfig)
                    {
                        for (int i = 0; i < objDetails.Length; i++)
                        {
                            if (objDetails[i].surveyQuestionId == valConfig.QuestionId)
                            {
                                flag = flag + 1;
                            }
                        }
                    }
                    if (flag != objConfig.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }


                    isAlreadySubmitted = await this.surveyManagementRepository.ExistsSurvey(aPISurveyResultMerged.surveyResultId, UserId);
                    if (isAlreadySubmitted)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.SurveyAlreadySubmitted) });
                    }
                    List<SurveyResultDetail> surveyResultDetailse = new List<SurveyResultDetail>();
                    //foreach (APISurveyResultDetail opt in aPISurveyResultMerged.aPISurveyResultDetail)
                    //{
                    //    bool validvalue = false;
                    //    if (!string.IsNullOrEmpty(opt.SubjectiveAnswer))
                    //    {
                    //        if (FileValidation.CheckForSQLInjection(opt.SubjectiveAnswer))
                    //            validvalue = true;
                    //        if (validvalue == true)
                    //        {
                    //            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    //        }
                    //    }
                    //}
                    IEnumerable<SurveyManagement> SurveyManagementList = await this.surveyManagementRepository.GetAllSurveyManagement(UserId);
                    foreach (SurveyManagement obj in SurveyManagementList)
                    {

                        if (obj.Id == aPISurveyResultMerged.surveyResultId && applicabletouser == false)
                        {
                            applicabletouser = true;
                        }
                    }


                    if (applicabletouser == true)
                    {

                        SurveyResult surveyResult = new SurveyResult
                        {
                            SurveyId = aPISurveyResultMerged.surveyResultId,
                            SurveyResultStatus = "Completed",
                            UserId = UserId,
                            IsDeleted = false,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow
                        };
                        await surveyResultRepository.Add(surveyResult);
                        List<SurveyResultDetail> surveyResultDetails = new List<SurveyResultDetail>();
                        foreach (APISurveyAnswerDetails opt in aPISurveyResultMerged.answerDeatils)
                        {
                            if (opt.section != "Subjective")
                            {
                                foreach (int optionid in opt.surveyOptionId)
                                {

                                    SurveyResultDetail surveyResultDetail = new SurveyResultDetail
                                    {
                                        SurveyResultId = surveyResult.Id,
                                        Section = opt.section,
                                        ServeyQuestionId = opt.surveyQuestionId,
                                        ServeyOptionId = optionid,
                                        SubjectiveAnswer = opt.subjectiveAnswer,
                                        CreatedBy = UserId,
                                        CreatedDate = DateTime.UtcNow,
                                        ModifiedBy = UserId,
                                        ModifiedDate = DateTime.UtcNow
                                    };
                                    surveyResultDetails.Add(surveyResultDetail);
                                }
                            }
                            else
                            {


                                SurveyResultDetail surveyResultDetail = new SurveyResultDetail
                                {
                                    SurveyResultId = surveyResult.Id,
                                    Section = opt.section,
                                    ServeyQuestionId = opt.surveyQuestionId,
                                    SubjectiveAnswer = opt.subjectiveAnswer,
                                    CreatedBy = UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedBy = UserId,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                surveyResultDetails.Add(surveyResultDetail);

                            }
                        }


                        await surveyResultDetailRepository.AddRange(surveyResultDetails);
                        int IsFirstSurvey = await surveyResultRepository.SubmittedSurveyCount(aPISurveyResultMerged.surveyResultId);
                        await this._rewardsPointRepository.AddSurveySubmitReward(UserId, surveyResult.SurveyId, IsFirstSurvey, objSurvey.SurveySubject, objSurvey.CreatedDate, OrgCode);
                        return Ok(aPISurveyResultMerged);
                    }
                    else
                    {
                        return this.BadRequest(this.ModelState);
                    }

                    //}
                    //else
                    //{
                    //    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    //}
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

        [HttpPost("SurveyResult/PostMultiple")]
        public async Task<IActionResult> Post([FromBody] APISurveyQuestionOption aPISurveyResultMerged)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool applicabletouser = false;
                    bool isAlreadySubmitted = false;

                    SurveyManagement objSurvey = new SurveyManagement();
                    objSurvey = await this.surveyManagementRepository.Get(aPISurveyResultMerged.surveyResultId);
                    if (objSurvey.Status == false)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    List<SurveyConfiguration> objConfig = new List<SurveyConfiguration>();
                    objConfig = await this.surveyManagementRepository.GetDetailsFromSuveryID(objSurvey.LcmsId);

                    if (objConfig.Count() != aPISurveyResultMerged.answerDeatils.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    APISurveyAnswerDetails[] objDetails = aPISurveyResultMerged.answerDeatils;


                    int flag = 0;
                    foreach (SurveyConfiguration valConfig in objConfig)
                    {
                        for (int i = 0; i < objDetails.Length; i++)
                        {
                            if (objDetails[i].surveyQuestionId == valConfig.QuestionId)
                            {
                                flag = flag + 1;
                            }
                        }
                    }
                    if (flag != objConfig.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }


                    isAlreadySubmitted = await this.surveyManagementRepository.ExistsSurvey(aPISurveyResultMerged.surveyResultId, UserId);
                    if (isAlreadySubmitted)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.SurveyAlreadySubmitted) });
                    }
                    List<SurveyResultDetail> surveyResultDetailse = new List<SurveyResultDetail>();
                    //foreach (APISurveyResultDetail opt in aPISurveyResultMerged.aPISurveyResultDetail)
                    //{
                    //    bool validvalue = false;
                    //    if (!string.IsNullOrEmpty(opt.SubjectiveAnswer))
                    //    {
                    //        if (FileValidation.CheckForSQLInjection(opt.SubjectiveAnswer))
                    //            validvalue = true;
                    //        if (validvalue == true)
                    //        {
                    //            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    //        }
                    //    }
                    //}
                    IEnumerable<SurveyManagement> SurveyManagementList = await this.surveyManagementRepository.GetAllSurveyManagement(UserId);
                    foreach (SurveyManagement obj in SurveyManagementList)
                    {

                        if (obj.Id == aPISurveyResultMerged.surveyResultId && applicabletouser == false)
                        {
                            applicabletouser = true;
                        }
                    }


                    if (applicabletouser == true)
                    {

                        SurveyResult surveyResult = new SurveyResult
                        {
                            SurveyId = aPISurveyResultMerged.surveyResultId,
                            SurveyResultStatus = "Completed",
                            UserId = UserId,
                            IsDeleted = false,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow
                        };
                        await surveyResultRepository.Add(surveyResult);
                        List<SurveyResultDetail> surveyResultDetails = new List<SurveyResultDetail>();
                        foreach (APISurveyAnswerDetails opt in aPISurveyResultMerged.answerDeatils)
                        {
                            if (opt.section != "Subjective")
                            {
                                foreach (int optionid in opt.surveyOptionId)
                                {

                                    SurveyResultDetail surveyResultDetail = new SurveyResultDetail
                                    {
                                        SurveyResultId = surveyResult.Id,
                                        Section = opt.section,
                                        ServeyQuestionId = opt.surveyQuestionId,
                                        ServeyOptionId = optionid,
                                        SubjectiveAnswer = opt.subjectiveAnswer,
                                        CreatedBy = UserId,
                                        CreatedDate = DateTime.UtcNow,
                                        ModifiedBy = UserId,
                                        ModifiedDate = DateTime.UtcNow
                                    };
                                    surveyResultDetails.Add(surveyResultDetail);
                                }
                            }
                            else
                            {


                                SurveyResultDetail surveyResultDetail = new SurveyResultDetail
                                {
                                    SurveyResultId = surveyResult.Id,
                                    Section = opt.section,
                                    ServeyQuestionId = opt.surveyQuestionId,
                                    SubjectiveAnswer = opt.subjectiveAnswer,
                                    CreatedBy = UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedBy = UserId,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                surveyResultDetails.Add(surveyResultDetail);

                            }
                        }


                        await surveyResultDetailRepository.AddRange(surveyResultDetails);
                        int IsFirstSurvey = await surveyResultRepository.SubmittedSurveyCount(aPISurveyResultMerged.surveyResultId);
                        await this._rewardsPointRepository.AddSurveySubmitReward(UserId, surveyResult.SurveyId, IsFirstSurvey, objSurvey.SurveySubject, objSurvey.CreatedDate, OrgCode);
                        return Ok(aPISurveyResultMerged);
                    }
                    else
                    {
                        return this.BadRequest(this.ModelState);
                    }

                    //}
                    //else
                    //{
                    //    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    //}
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
        [HttpGet("SurveyRead/{id:int}")]
        public async Task<IActionResult> PostCounter(int id)
        {

            try
            {
                int referenceId = 0;
                referenceId = id;
                string functionCode = "TLS0830";
                string category = "Normal";
                int point = 1;
                int userId = UserId;


                int readToday = await this.surveyManagementRepository.ReadToady(id);
                if (readToday > 0)
                {

                    string categoryRead = "Bonus";
                    int pointRead = 2;
                    await this.surveyManagementRepository.RewardPointSave(functionCode, categoryRead, referenceId, pointRead, userId);
                }
                // Reward Point for First Responce
                int firstResponse = await this.surveyManagementRepository.FirstResponse(id);
                if (firstResponse == 1)
                {

                    string categoryRead = "Condition";
                    int pointRead = 5;
                    await this.surveyManagementRepository.RewardPointSave(functionCode, categoryRead, referenceId, pointRead, userId);
                }

                await this.surveyManagementRepository.RewardPointSave(functionCode, category, referenceId, point, userId);
                return Ok();



            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // PUT api/<controller>/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.SurveyManagement)]
        public async Task<IActionResult> Put(int id, [FromBody] APISurveyManagementPut aPISurveyManagement)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                SurveyManagement surveyManagement = await this.surveyManagementRepository.Get(s => s.IsDeleted == false && s.Id == id);
                if (surveyManagement == null)
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                if (await this.surveyManagementRepository.existSurvey(aPISurveyManagement.SurveySubject, aPISurveyManagement.Id))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                #region "Code added to remove Survey Notification as the Survey is now made inactive"
                if (aPISurveyManagement.IsApplicableToAll == false && surveyManagement.IsApplicableToAll == true)
                {
                    int notificationId = await quizzesManagementRepository.GetNotificationId(aPISurveyManagement.SurveySubject);
                    string Url = this._configuration[Configuration.NotificationApi];
                    Url = Url + "/tlsNotification/" + notificationId;
                    HttpResponseMessage response = await ApiHelper.CallDeleteAPI(Url, Token);
                }
                #endregion

                #region "Code added to send Survey Notification as the Survey is now made active. Previously it was inactive"
                if (aPISurveyManagement.Status == true && surveyManagement.Status == false && aPISurveyManagement.IsApplicableToAll)
                {
                    await surveyManagementRepository.SendNotification(surveyManagement.SurveySubject, Token, surveyManagement.Id);
                    var val1 = surveyManagementRepository.SendSurveyApplicabilityPushNotification(surveyManagement.Id, OrgCode);
                }
                #endregion

                surveyManagement.Date = aPISurveyManagement.Date;
                surveyManagement.StartDate = aPISurveyManagement.StartDate;
                surveyManagement.ValidityDate = aPISurveyManagement.ValidityDate;
                surveyManagement.SurveyPurpose = aPISurveyManagement.SurveyPurpose;
                surveyManagement.AverageRespondTime = aPISurveyManagement.AverageRespondTime;
                surveyManagement.TargetResponseCount = aPISurveyManagement.TargetResponseCount;
                surveyManagement.Status = aPISurveyManagement.Status;
                surveyManagement.ModifiedBy = UserId;
                surveyManagement.LcmsId = aPISurveyManagement.LcmsId;
                surveyManagement.ModifiedDate = DateTime.UtcNow;
                surveyManagement.IsApplicableToAll = aPISurveyManagement.IsApplicableToAll;

                await this.surveyManagementRepository.Update(surveyManagement);



                return Ok(surveyManagement);

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // PUT api/<controller>/5
        [HttpPost("SurveyQuestion/{id}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> PutSurveyQuestion(int id, [FromBody] APISurveyMergeredModel aPISurveyMergeredModel)
        {
            try
            {
                Boolean result = await this.surveyManagementRepository.IsQuestionUsed(id);
                if (result == true)
                {
                    return StatusCode(304, MessageType.SurveyInUse);
                }
                SurveyQuestion surveyQuestion = await surveyQuestionRepository.Get(id);
                if (surveyQuestion == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                List<APISurveyMergeredModel> surveyMergeredModelapi = new List<APISurveyMergeredModel>();
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}
                //else if (FileValidation.CheckForSQLInjection(aPISurveyMergeredModel.Question) == true)
                //{
                //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                //}

                if (await this.surveyQuestionRepository.existsQuestion(aPISurveyMergeredModel.Question, aPISurveyMergeredModel.Id))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                if (surveyQuestion.Section != aPISurveyMergeredModel.Section)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                //if (aPISurveyMergeredModel.options != aPISurveyMergeredModel.aPISurveyOption.Count())
                //{
                //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                //}

                //else
                //{
                surveyQuestion.AllowSkipAswering = aPISurveyMergeredModel.AllowSkipAnswering;
                surveyQuestion.Question = aPISurveyMergeredModel.Question;
                //surveyQuestion.Section = aPISurveyMergeredModel.Section;
                surveyQuestion.Status = aPISurveyMergeredModel.Status;
                surveyQuestion.CreatedDate = DateTime.UtcNow;
                surveyQuestion.ModifiedBy = UserId;
                surveyQuestion.ModifiedDate = DateTime.UtcNow;
                //surveyQuestion.IsMultipleChoice = aPISurveyMergeredModel.IsMultipleChoice;
                //if(surveyQuestion.IsMultipleChoice==true)
                //{
                //    surveyQuestion.OptionType = "MultipleSelection";
                //}
                //else
                //{
                //    surveyQuestion.OptionType = "SingleSelection";
                //}
                if (surveyQuestion.Section.ToLower() == "objective")
                {
                    if (aPISurveyMergeredModel.options != aPISurveyMergeredModel.aPISurveyOption.Count())
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    if (aPISurveyMergeredModel.options < 2 || aPISurveyMergeredModel.options > 10)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidOptionRange), Description = EnumHelper.GetEnumDescription(MessageType.InvalidOptionRange) });
                    }

                    if (aPISurveyMergeredModel.aPISurveyOption.Length < 2)
                    {
                        surveyMergeredModelapi.Add(aPISurveyMergeredModel);
                    }
                    else
                    {
                        await surveyQuestionRepository.Update(surveyQuestion);
                        // List<SurveyOption> surveyOptions = new List<SurveyOption>();

                        //   int OptionCount = aPISurveyMergeredModel.aPISurveyOption.Where(o => !string.IsNullOrEmpty(o.OptionText)).Count();
                        //  SurveyOption[] surveyOptions = new SurveyOption[OptionCount];
                        List<SurveyOption> Options = await surveyOptionRepository.GetAll(g => g.QuestionId == id);
                        await surveyOptionRepository.RemoveRange(Options);
                        // int i = 0;
                        foreach (APISurveyOption opt in aPISurveyMergeredModel.aPISurveyOption)
                        {
                            if (opt != null)
                            {
                                SurveyOption surveyOption = new SurveyOption
                                {
                                    OptionText = opt.OptionText,
                                    QuestionId = surveyQuestion.Id,
                                    CreatedBy = UserId,
                                    CreatedDate = surveyQuestion.CreatedDate,
                                    ModifiedBy = UserId,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                surveyOption.QuestionId = id;
                                // surveyOptions[i] = surveyOption;
                                //  i++;
                                await surveyOptionRepository.Add(surveyOption);

                            }
                        }
                        // await surveyOptionRepository.AddRange(surveyOptions);
                    }
                }
                if (surveyQuestion.Section.ToLower() == "subjective")
                {
                    await surveyQuestionRepository.Update(surveyQuestion);
                }
                //}

                return Ok(surveyMergeredModelapi);

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("SurveyOption/{id}")]
        public async Task<IActionResult> PutSurveyOption(int id, [FromBody] APISurveyOption aPISurveyOption)
        {

            try
            {

                SurveyOption surveyOption = await this.surveyOptionRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (surveyOption == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (ModelState.IsValid && surveyOption != null)
                {
                    surveyOption.QuestionId = Convert.ToInt16(aPISurveyOption.Id);
                    surveyOption.OptionText = aPISurveyOption.OptionText;
                    surveyOption.ModifiedBy = UserId;
                    surveyOption.ModifiedDate = DateTime.UtcNow;
                    await this.surveyOptionRepository.Update(surveyOption);
                }

                return Ok(aPISurveyOption);

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.SurveyManagement)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
            SurveyManagement surveyManagement = await this.surveyManagementRepository.Get(DecryptedId);
            Boolean result = await this.surveyManagementRepository.ExistsInResult(DecryptedId);
            if (result == true)
            {
                return StatusCode(304, MessageType.SurveyInUse);
            }
            if (ModelState.IsValid && surveyManagement != null)
            {
                surveyManagement.IsDeleted = true;
                await this.surveyManagementRepository.Update(surveyManagement);
                #region "Code added to remove Survey Notification as the Survey is now deleted"
                int notificationId = await quizzesManagementRepository.GetNotificationId(surveyManagement.SurveySubject);
                string Url = this._configuration[Configuration.NotificationApi];
                Url = Url + "/tlsNotification/" + notificationId;
                HttpResponseMessage response = await ApiHelper.CallDeleteAPI(Url, Token);

                #endregion

            }

            if (surveyManagement == null)
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            return this.Ok();

        }

        // DELETE api/<controller>/5
        [HttpDelete("SurveyQuestion")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> SurveyQuestionDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                SurveyQuestion surveyQuestion = await surveyQuestionRepository.Get(DecryptedId);
                if (surveyQuestion == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                Boolean result = await this.surveyManagementRepository.IsQuestionUsed(DecryptedId);
                if (result == true)
                {
                    return StatusCode(304, MessageType.SurveyInUse);
                }
                surveyQuestion.IsDeleted = true;
                await surveyQuestionRepository.Update(surveyQuestion);
                List<SurveyOption> surveyOptions = await surveyOptionRepository.GetAll(f => f.QuestionId == surveyQuestion.Id);
                foreach (SurveyOption surveyOption in surveyOptions)
                {
                    surveyOption.IsDeleted = true;
                    await surveyOptionRepository.Update(surveyOption);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("SurveyOption")]
        public async Task<IActionResult> SurveyOptionDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                SurveyOption surveyOption = await this.surveyOptionRepository.Get(DecryptedId);
                if (ModelState.IsValid && surveyOption != null)
                {
                    surveyOption.IsDeleted = true;
                    await this.surveyOptionRepository.Update(surveyOption);
                }
                if (surveyOption == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        /// <summary>
        /// Search specific SurveyManagement.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<SurveyManagement> surveyManagement = await this.surveyManagementRepository.Search(q);
                return Ok(Mapper.Map<List<APISurveyManagement>>(surveyManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        /// <summary>
        /// Search specific surveyQuestion.
        /// </summary>
        [HttpGet]
        [Route("SurveyQuestion/Search/{q}")]
        public async Task<IActionResult> SurveyQuestionSearch(string q)
        {
            try
            {
                IEnumerable<SurveyQuestion> surveyQuestion = await this.surveyQuestionRepository.Search(q);
                return Ok(Mapper.Map<List<APISurveyQuestion>>(surveyQuestion));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        /// <summary>
        /// Search specific surveyQuestion.
        /// </summary>
        [HttpGet]
        [Route("SurveyOption/Search/{q}")]
        public async Task<IActionResult> SurveyOptionSearch(string q)
        {
            try
            {
                IEnumerable<SurveyOption> surveyOption = await this.surveyOptionRepository.Search(q);
                return Ok(Mapper.Map<List<APISurveyOption>>(surveyOption));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        /// <summary>
        /// Search specific surveyQuestion.
        /// </summary>
        [HttpGet]
        [Route("GetActiveQuestions/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetActiveQuestions(int page, int pageSize, string search)
        {
            try
            {
                List<APISurveyMergeredModel> surveyQuestion = await this.surveyQuestionRepository.GetActiveQuestions(page, pageSize, search);
                return Ok(surveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetActiveQuestionsCount/{search?}")]
        public async Task<IActionResult> GetActiveQuestionsCount(string search)
        {
            try
            {
                int surveyQuestion = await this.surveyQuestionRepository.GetActiveQuestionsCount(search);
                return Ok(surveyQuestion);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // POST api/<controller>
        [HttpPost("SurveyLcms")]

        public async Task<IActionResult> SurveyLcmsPost([FromBody] ApiSurveyLcms apiSurveyLcms)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InvalidData),
                        Description = EnumHelper.GetEnumDescription(MessageType.InvalidData)
                    });
                int Result = await this.surveyManagementRepository.AddSurvey(apiSurveyLcms, UserId);
                if (Result == 0)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                }
                if (Result == -1)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyLcms/{lcmsId}")]
        public async Task<IActionResult> GetSurveyLcms(int lcmsId)
        {
            try
            {

                return Ok(await this.surveyManagementRepository.GetLcmsSurvey(lcmsId));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SurveyLcms/{lcmsId}")]

        public async Task<IActionResult> SurveyLcmsPut([FromBody] ApiSurveyLcms apiSurveyLcms, int lcmsId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.InvalidData),
                        Description = EnumHelper.GetEnumDescription(MessageType.InvalidData)
                    });
                int Result = await this.surveyManagementRepository.UpdateSurvey(apiSurveyLcms, UserId);
                if (Result == 0)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                }
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetSurveyQuestionTypeAhead/{surveyid:int}")]
        public async Task<IActionResult> GetSurveyQuestionTypeAhead(int surveyid)

        {
            try
            {
                APIResponse responce = await this.surveyManagementRepository.GetSurveyQuestionTypeAhead(surveyid);
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

        [HttpGet("SurveyQuestion/SurveyReportTypeHead/{survey:minlength(0)?}")]

        public async Task<IActionResult> SurveyReportTypeHead(string survey)
        {
            try
            {
                return this.Ok(await this.surveyManagementRepository.SurveyReportTypeHead(survey));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SurveyQuestion/SurveyNotApplicableTypeAhead/{survey:minlength(0)?}")]

        public async Task<IActionResult> SurveyNotApplicableTypeAhead(string survey)
        {
            try
            {
                return this.Ok(await this.surveyManagementRepository.SurveyNotApplicableTypeAhead(survey));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {

                _surveyQuestionRejectedRepository.Delete();
                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                //string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
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
                            fileDir = Path.Combine(fileDir, OrgCode, FileType);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                            using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            // return Ok(file.Substring(file.LastIndexOf("\\" )).Replace(@"\", "/"));
                            return Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                        }
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
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
        [HttpPost]
        [Route("SaveFileData")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> PostFile([FromBody] APISurveyFilePath aPIFilePath)
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
                        string resultString = await this.surveyManagementRepository.ProcessImportFile(file, surveyManagementRepository, surveyQuestionRepository, surveyOptionRepository, _surveyQuestionRejectedRepository, UserId);
                        return Ok(resultString);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error( Utilities.GetDetailedException(ex));
                        string exception = ex.Message;
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }

        }

        [HttpGet]
        [Route("Export")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> Export()
        {
            try
            {
                List<SurveyQuestionRejected> survey = await this.surveyManagementRepository.GetAllSurvey();
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                //string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "SurveyQuestionsImport.xlsx";
                //string URL = string.Concat(DomainName, "/", sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SurveyQuestionsImport");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Question" + Record.Strar;
                    worksheet.Cells[1, 2].Value = "Active Question" + Record.Strar;
                    worksheet.Cells[1, 3].Value = "Objective Question" + Record.Strar;
                    worksheet.Cells[1, 4].Value = "Options" + Record.Strar;
                    worksheet.Cells[1, 5].Value = "EnterOption1";
                    worksheet.Cells[1, 6].Value = "EnterOption2";
                    worksheet.Cells[1, 7].Value = "EnterOption3";
                    worksheet.Cells[1, 8].Value = "EnterOption4";
                    worksheet.Cells[1, 9].Value = "EnterOption5";
                    worksheet.Cells[1, 10].Value = "EnterOption6";
                    worksheet.Cells[1, 11].Value = "EnterOption7";
                    worksheet.Cells[1, 12].Value = "EnterOption8";
                    worksheet.Cells[1, 13].Value = "EnterOption9";
                    worksheet.Cells[1, 14].Value = "EnterOption10";
                    worksheet.Cells[1, 15].Value = "IsMultipleChoice";
                    int row = 2, column = 1;
                    foreach (SurveyQuestionRejected surveys in survey)
                    {
                        worksheet.Cells[row, column++].Value = surveys.Question;
                        worksheet.Cells[row, column++].Value = surveys.ActiveQuestion;
                        worksheet.Cells[row, column++].Value = surveys.ObjectiveQuestion;
                        worksheet.Cells[row, column++].Value = surveys.Options;
                        //if (surveys.Options==0)
                        //{
                        //    surveys.Options =Convert.ToInt32(null);
                        //}
                        // worksheet.Cells[row, column++].Value = nominate.AcademyAgencyID;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption1;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption2;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption3;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption4;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption5;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption6;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption7;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption8;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption9;
                        worksheet.Cells[row, column++].Value = surveys.EnterOption10;
                        //worksheet.Cells[row, column++].Value = surveys.IsMultipleChoice;
                        row++;
                        column = 1;

                    }
                    using (ExcelRange rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;

                    }
                    package.Save(); //Save the workbook.
                }
                FileStream Fs = file.OpenRead();
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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("MultiDeleteSurveyQuestion")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> MultiDeleteAssessmentQuestion([FromBody] APIDeleteSurveyQuestion[] apideletemultipleque)
        {
            try
            {
                APIResponse Response = await this.surveyQuestionRepository.MultiDeleteSurveyQuestion(apideletemultipleque);
                return Ok(Response.ResponseObject);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllSurveyQuestionReject/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> GetUserReject(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<SurveyQuestionRejected> feedbackQuestionReject = await this._surveyQuestionRejectedRepository.GetAllSurveyQuestionReject(page, pageSize, search);
                return Ok(feedbackQuestionReject);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("SurveyQuestionReject/GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.SurveyQuestionManagement)]
        public async Task<IActionResult> GetCountForSurvey(string search)
        {
            try
            {
                int userReject = await this._surveyQuestionRejectedRepository.Count(search);
                return Ok(userReject);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [Route("PostMultiple")]
        [Produces("application/json")]

        public async Task<IActionResult> Post([FromBody] APISurveyAccessibilityRules[] rules)
        {
            try
            {
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<APISurveyAccessibilityRules> rejectedRules = new List<APISurveyAccessibilityRules>();
               
                foreach (APISurveyAccessibilityRules rule in rules)
                {
                    bool isvalid = await surveyManagementRepository.CheckValidData(rule.AccessibilityParameter1, rule.AccessibilityValue1, rule.AccessibilityParameter2, rule.AccessibilityValue2, rule.SurveyManagementId);
                    if (!isvalid)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                foreach (APISurveyAccessibilityRules rule in rules)
                {
                    // APIAccessibilityRules apiAccessibilityRules = new APIAccessibilityRules();
                    //  apiAccessibilityRules = rule;

                    APISurveyAccessibility apiAccessibility = ConvertToAPIAccessibility(rule);
                    List<AccessibilitySurveyRules> result = await surveyManagementRepository.Post(apiAccessibility, UserId,OrgCode, Token);

                    if (result != null)
                        rejectedRules.Add(rule);

                }
                int SurveyId = rules.Select(x => x.SurveyManagementId).FirstOrDefault();
                if (await surveyManagementRepository.IsSurveyActive(SurveyId))
                {
                    try
                    {
                        string url = _configuration[Configuration.NotificationApi];
                        url += "/SurveyApplicabilityPushNotification";
                        JObject Pushnotification = new JObject();
                        Pushnotification.Add("SurveyManagementId", SurveyId);
                        Pushnotification.Add("organizationCode", OrgCode);
                        HttpResponseMessage responses1 = await ApiHelper.CallAPI(url, Pushnotification);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                    }
                }
                if (rejectedRules != null)
                {
                    return Ok(rejectedRules);
                }
                return Ok();

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        private APISurveyAccessibility ConvertToAPIAccessibility(APISurveyAccessibilityRules apiAccessibilityRules)
        {
            APISurveyAccessibility apisurveyAccessibility = new APISurveyAccessibility
            {
                SurveyManagementId = apiAccessibilityRules.SurveyManagementId
            };

            if (apiAccessibilityRules.AccessibilityParameter2 == null)
                apisurveyAccessibility.AccessibilityRule = new AccessibilitySurveyRules[1];
            else
                apisurveyAccessibility.AccessibilityRule = new AccessibilitySurveyRules[2];


            if (apiAccessibilityRules.AccessibilityParameter1 != null)
            {
                AccessibilitySurveyRules AccessibilityRules = new AccessibilitySurveyRules
                {
                    AccessibilityRule = apiAccessibilityRules.AccessibilityParameter1,
                    ParameterValue = apiAccessibilityRules.AccessibilityValue1,
                    Condition = apiAccessibilityRules.Condition1 == null ? "null" : apiAccessibilityRules.Condition1
                };
                apisurveyAccessibility.AccessibilityRule[0] = AccessibilityRules;
            }
            if (apiAccessibilityRules.AccessibilityParameter2 != null)
            {
                AccessibilitySurveyRules AccessibilityRules = new AccessibilitySurveyRules
                {
                    AccessibilityRule = apiAccessibilityRules.AccessibilityParameter2,
                    ParameterValue = apiAccessibilityRules.AccessibilityValue2,
                    Condition = apiAccessibilityRules.Condition1
                };
                apisurveyAccessibility.AccessibilityRule[1] = AccessibilityRules;
            }
            return apisurveyAccessibility;
        }

        [HttpPost("GetRules")]

        public async Task<IActionResult> GetRules([FromBody] APIGetRules objAPIGetRules)
        {
            try
            {
                return Ok(await surveyManagementRepository.GetAccessibilityRules(objAPIGetRules.SurveyManagementId, OrgCode, Token, objAPIGetRules.page, objAPIGetRules.pageSize));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetRulesCount")]

        public async Task<IActionResult> GetRulesCount([FromBody] APIGetRules objAPIGetRules)
        {
            try
            {
                return Ok(await surveyManagementRepository.GetAccessibilityRulesCount(objAPIGetRules.SurveyManagementId));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetSurveyApplicableUserList_Export")]
        public async Task<IActionResult> GetCourseApplicableUserList_Export([FromBody] APIGetRules surveyApplicableUser)
        {
            try
            {

                int Id = surveyApplicableUser.SurveyManagementId;
                var surveysubject = await this.surveyManagementRepository.GetSurveySubject(Id);
                FileInfo ExcelFile;
                //surveyApplicableUser.page = 1;
                //surveyApplicableUser.pageSize = 10;
                List<APISurveyAccessibilityRules> UserList = new List<APISurveyAccessibilityRules>();
                UserList = await this.surveyManagementRepository.GetAccessibilityRulesForExport(surveyApplicableUser.SurveyManagementId, OrgCode, Token);

                List<SurveyApplicableUser> surveyApplicableUsers = new List<SurveyApplicableUser>();
                surveyApplicableUsers = await this.surveyManagementRepository.GetSurveyApplicableUserList(surveyApplicableUser.SurveyManagementId);
                ExcelFile = this.surveyManagementRepository.GetSurveyApplicableExcel(UserList, surveyApplicableUsers, surveysubject, OrgCode);


                var fs = ExcelFile.OpenRead();
                byte[] fileData = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    fileData = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, ExcelFile.Name);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetSurveyAccessibility/{page:int}/{pageSize:int}/{search?}/{columnName?}")]

        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                return Ok(await surveyManagementRepository.GetSurveyAccessibility(page, pageSize, search, columnName));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetSurveyAccessibility/getTotalRecords/{search:minlength(0)?}/{columnName?}")]

        public async Task<IActionResult> GetCount(string search = null, string columnName = null)
        {
            try
            {
                //int count = await _accessibilityRule.count(search, columnName);
                return Ok(await surveyManagementRepository.count(search, columnName));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("Rule")]

        public async Task<IActionResult> RuleDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await surveyManagementRepository.DeleteRule(DecryptedId);
                if (Result == 1)
                    return Ok();
                else
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotAvailable), Description = EnumHelper.GetEnumDescription(MessageType.DataNotAvailable) });
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("BulkSurveyApplicability")]     
        public async Task<APIResponse> PostFileDeleteApplicability([FromBody] APISurveyFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new APIResponse() { StatusCode = 400, Description = EnumHelper.GetEnumDescription(MessageType.InvalidPostRequest) };

                return await new ProcessFile().ProcessRecordsAsync(aPIFilePath.Path, _customerConnectionStringRepository, UserId, _configuration, OrgCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new APIResponse() { StatusCode = 400, Description = EnumHelper.GetEnumDescription(FileMessages.FileErrorInImport) };
            }
        }

        [HttpGet]
        [Route("SurveyApplicabilityImport")]
        public IActionResult SurveyApplicabilityImport()
        {
            try
            {
                return new  ProcessFile().GenerateSurveyApplicabilityAsync(this, UserId, _configuration, OrgCode);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // -------------------------------------- Nested Survey --------------------------------------


        [HttpGet("GetSurveyQuestionsByLcmsId/{lcmsId}")]
        public async Task<IActionResult> GetSurveyQuestionsByLcmsId(int lcmsId)
        {
            try
            {
                return Ok(await this.surveyManagementRepository.GetSurveyQuestionsByLcmsId(lcmsId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpGet("GetQuestionByQuestionId/{QuestionId}")]
        public async Task<IActionResult> GetQuetionByQuestionId(int QuestionId)
        {
            try
            {
                return Ok(await this.surveyManagementRepository.GetQuestionByQuestionId(QuestionId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpGet("GetOptionsByLcmsQuestion/{LcmsId}/{QuestionId}")]
        public async Task<IActionResult> GetOptionsByLcmsQuestion(int LcmsId, int QuestionId)
        {
            try
            {
                return Ok(await this.surveyManagementRepository.GetOptionsByLcmsQuestion(LcmsId, QuestionId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpGet("GetOptionByLcmsOption/{LcmsId}/{OptionId}")]
        public async Task<IActionResult> GetOptionByLcmsOption(int LcmsId, int OptionId)
        {
            try
            {
                return Ok(await this.surveyManagementRepository.GetOptionByLcmsOption(LcmsId, OptionId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpGet("GetRootQuestion/{LcmsId}")]
        public async Task<IActionResult> GetRootQuestion(int LcmsId)
        {
            try
            {
                return Ok(await this.surveyManagementRepository.GetRootQuestion(LcmsId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpPost("AddRootQuestion")]
        public async Task<IActionResult> AddRootQuestion([FromBody] APINestedSurveyQuestions questionData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                SurveyConfiguration surveyConfiguration = await this.surveyConfigurationRepository.Get(s => s.LcmsId == questionData.LcmsId && s.QuestionId == questionData.QuestionId && s.IsDeleted == false);
                if (surveyConfiguration == null)
                    return NotFound();
                surveyConfiguration.IsRoot = 1;

                surveyConfiguration.ModifiedDate = DateTime.UtcNow;
                surveyConfiguration.ModifiedBy = UserId;

                await this.surveyConfigurationRepository.Update(surveyConfiguration);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpPost("AddNextQuestion")]
        public async Task<IActionResult> AddNextQuestion([FromBody] APINestedSurveyOptions optionData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                SurveyOptionNested surveyOptionNested = new SurveyOptionNested();
                surveyOptionNested.OptionId = optionData.Id;
                surveyOptionNested.LcmsId = optionData.LcmsId;
                surveyOptionNested.NextQuestionId = optionData.NextQuestionId;

                await this.surveyOptionNestedRepository.Add(surveyOptionNested);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpPost("DeleteNextQuestion")]
        public async Task<IActionResult> DeleteNextQuestion([FromBody] APINestedSurveyOptions optionData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                SurveyOptionNested surveyOptionNested = await this.surveyOptionNestedRepository.Get(s => s.OptionId == optionData.Id && s.LcmsId == optionData.LcmsId);
                await this.surveyOptionNestedRepository.Remove(surveyOptionNested);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpPost("SubmitNestedSurvey")]
        public async Task<IActionResult> SubmitNestedSurvey([FromBody] APINestedSurveyResult apiNestedSurveyResult)
        {
            try
            {
                await this.surveyManagementRepository.SubmitNestedSurvey(apiNestedSurveyResult, UserId);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        [HttpGet("IsSurveyNested/{LcmsId}")]
        public async Task<IActionResult> IsSurveyNested(int LcmsId)
        {
            try
            {
                bool? IsNested = await this.surveyManagementRepository.IsSurveyNested(LcmsId);
                return Ok(IsNested);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }
    }
}
