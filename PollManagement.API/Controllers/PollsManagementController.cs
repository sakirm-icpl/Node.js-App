// ======================================
// <copyright file="PollsManagementController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
//using AutoMapper.Configuration;
using PollManagement.API.APIModel;
using PollManagement.API.Common;
using PollManagement.API.Helper;
using PollManagement.API.Models;
using PollManagement.API.Repositories.Interfaces;
using PollManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static PollManagement.API.Common.AuthorizePermissions;
using static PollManagement.API.Common.TokenPermissions;
using Microsoft.Extensions.Configuration;
using log4net;
using PollManagement.API.Helper.Log_API_Count;

namespace PollManagement.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class PollsManagementController : IdentityController
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(PollsManagementController));
        private IPollsManagementRepository pollsManagementRepository;
        private IPollsResultRepository pollsResultRepository;
        private IRewardsPointRepository _rewardsPointRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        private IQuizzesManagementRepository quizzesManagementRepository;
        public IConfiguration _configuration;
        public PollsManagementController(IPollsManagementRepository pollsManagementController,
            IPollsResultRepository pollsResultController,
            IRewardsPointRepository rewardsPointRepository,
        IIdentityService identitySvc, IQuizzesManagementRepository quizzesManagementRepository, IConfiguration confugu, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.pollsManagementRepository = pollsManagementController;
            this.pollsResultRepository = pollsResultController;
            this._rewardsPointRepository = rewardsPointRepository;
            this._identitySvc = identitySvc;
            this._tokensRepository = tokensRepository;
            this.quizzesManagementRepository = quizzesManagementRepository;
            this._configuration = confugu;
        }

        // GET: api/<controller>
        [HttpGet]
        [PermissionRequired(Permissions.OpinionPollManagement)]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<PollsManagement> pollsManagement = await this.pollsManagementRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIPollsManagement>>(pollsManagement.OrderByDescending(s => s.ModifiedDate)));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.OpinionPollManagement)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<PollsManagement> pollsManagement = await this.pollsManagementRepository.GetAllPollsManagement(page, pageSize, search);
                return Ok(Mapper.Map<List<APIPollsManagement>>(pollsManagement));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllPollsManagementForEndUser")]
        //[AllowAnonymous]
        public async Task<IActionResult> GetAllPollsManagementForEndUser()
        {
            try
            {
                return Ok(await this.pollsManagementRepository.GetAllPollsManagement(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllPollsManagementForEndUserCount")]
       // [AllowAnonymous]
        public async Task<IActionResult> GetAllPollsManagementForEndUserCount()
        {
            try
            {
                int pollsManagement = await this.pollsManagementRepository.GetAllPollsManagementCount(UserId);
                return Ok(pollsManagement);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.OpinionPollManagement)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int pollsManagement = await this.pollsManagementRepository.Count(search);
                return Ok(pollsManagement);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotal/{id}")]
        public async Task<JsonResult> GetTotal(int id)
        {
            try
            {
                return await this.pollsManagementRepository.GetTotal(id, UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;

            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [PermissionRequired(Permissions.OpinionPollManagement)]
       // [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await this.pollsManagementRepository.GetPollManagement(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("Exists/{search:minlength(0)?}")]
        public async Task<bool> Exists(string search)
        {
            try
            {
                return await this.pollsManagementRepository.Exist(search);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpGet("ExistsPoll/{pollid:int}")]
        public async Task<bool> ExistsPoll(int pollid, int userid)
        {
            try
            {
                return await this.pollsManagementRepository.ExistPoll(pollid, UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpGet("GetCount")]
        public async Task<IActionResult> GetTotalCount()
        {
            try
            {
                int newsUpdates = await this.pollsManagementRepository.GetCount();
                return Ok(newsUpdates);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost]
        [PermissionRequired(Permissions.OpinionPollManagement)]
        public async Task<IActionResult> Post([FromBody] APIPollsManagement aPIPollsManagement)
        {
            try
            {
                //if ((aPIPollsManagement.ApplicabilityParameter == null || aPIPollsManagement.ApplicabilityParameter == "0"))
                //if ((aPIPollsManagement.ApplicabilityParameter == null || aPIPollsManagement.ApplicabilityParameter == "0") && (aPIPollsManagement.ParameterValue != null || aPIPollsManagement.ParameterValueId != null) ) // apurva's condtion me comment keli
                if ((aPIPollsManagement.ApplicabilityParameter != "0") && (aPIPollsManagement.ParameterValue == null || aPIPollsManagement.ParameterValueId == null))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if ((aPIPollsManagement.ApplicabilityParameter == "UserId" || aPIPollsManagement.ApplicabilityParameter == "MobileNumber" || aPIPollsManagement.ApplicabilityParameter == "EmailId") && aPIPollsManagement.TargetResponseCount != 1)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                PollsManagement pollsManagement = new PollsManagement();
                if (await this.pollsManagementRepository.Existquestion(aPIPollsManagement.Question, null))
                {
                    return StatusCode(409, "Duplicate Question!");
                }
                else
                {
                    //Validation for duplicate options
                    string[] options;
                    options = new string[] { aPIPollsManagement.Option1.Trim(), aPIPollsManagement.Option2.Trim(), aPIPollsManagement.Option3.Trim(), aPIPollsManagement.Option4.Trim(), aPIPollsManagement.Option5.Trim() };

                    List<string> termsList = new List<string>();
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (options[i] != "")
                        {
                            termsList.Add(options[i]);
                        }
                    }
                    options = termsList.ToArray();
                    if (options.Distinct().Count() != options.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateOptions), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateOptions) });
                    }

                    pollsManagement.StartDate = aPIPollsManagement.StartDate;
                    pollsManagement.ValidityDate = aPIPollsManagement.ValidityDate;
                    if (aPIPollsManagement.ApplicabilityParameter != "UserId" || aPIPollsManagement.ApplicabilityParameter != "MobileNumber" || aPIPollsManagement.ApplicabilityParameter != "EmailId")
                        pollsManagement.TargetResponseCount = aPIPollsManagement.TargetResponseCount;
                    else
                        pollsManagement.TargetResponseCount = 1;
                    pollsManagement.Question = aPIPollsManagement.Question;
                    pollsManagement.Option1 = aPIPollsManagement.Option1;
                    pollsManagement.Option2 = aPIPollsManagement.Option2;
                    pollsManagement.Option3 = aPIPollsManagement.Option3;
                    pollsManagement.Option4 = aPIPollsManagement.Option4;
                    pollsManagement.Option5 = aPIPollsManagement.Option5;
                    pollsManagement.Status = aPIPollsManagement.Status;
                    pollsManagement.IsDeleted = false;
                    pollsManagement.ModifiedBy = UserId;
                    pollsManagement.ModifiedDate = DateTime.UtcNow;
                    pollsManagement.CreatedBy = UserId;
                    pollsManagement.CreatedDate = DateTime.UtcNow;
                    if (aPIPollsManagement.ApplicabilityParameter == "0" && aPIPollsManagement.ParameterValue == null)
                    {
                        pollsManagement.IsApplicableToAll = true;
                    }
                    await pollsManagementRepository.Add(pollsManagement);
                    if (pollsManagement.Status == true && pollsManagement.IsApplicableToAll == true)
                    {
                        APINotifications Notification = new APINotifications();
                        Notification.Title = Record.PollNotification;
                        Notification.Type = Record.Poll;
                        string Url = this._configuration[Configuration.NotificationApi];
                        Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.PollNotification;
                        HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
                        string result = await response.Content.ReadAsStringAsync();
                        Notification.Message = JsonConvert.DeserializeObject(result).ToString();
                        Notification.Message = Notification.Message.Replace("[pollquestion]", pollsManagement.Question);
                        bool IsApplicableToAll = pollsManagement.IsApplicableToAll;
                        Notification.Url = "social/";
                        Notification.UserId = Convert.ToInt32(aPIPollsManagement.ParameterValueId);
                        Notification.IsRead = false;
                        await quizzesManagementRepository.SendNotificationForQuizAndSurvey(Notification, IsApplicableToAll);
                    }
                    else if (aPIPollsManagement.ApplicabilityParameter != null && aPIPollsManagement.ParameterValue != null && pollsManagement.IsApplicableToAll == false)
                    {
                        int ApplicabilityId = await pollsManagementRepository.AddPollsApplicability(pollsManagement.Id, aPIPollsManagement.ApplicabilityParameter, aPIPollsManagement.ParameterValue, aPIPollsManagement.ParameterValueId.Value);

                        APINotifications Notification = new APINotifications();
                        Notification.Title = Record.PollNotification;
                        Notification.Type = Record.Poll;
                        string Url = this._configuration[Configuration.NotificationApi];
                        Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.PollNotification;
                        HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
                        string result = await response.Content.ReadAsStringAsync();
                        Notification.Message = JsonConvert.DeserializeObject(result).ToString();
                        Notification.Message = Notification.Message.Replace("[pollquestion]", pollsManagement.Question);
                        bool IsApplicableToAll = pollsManagement.IsApplicableToAll;
                        Notification.Url = "social/";
                        Notification.UserId = Convert.ToInt32(aPIPollsManagement.ParameterValueId);
                        Notification.IsRead = false;
                        if (pollsManagement.Status == true && pollsManagement.IsApplicableToAll == false)
                        {
                            int aPINotification1 = await quizzesManagementRepository.SendNotificationForQuizAndSurvey(Notification, IsApplicableToAll);

                            // int UserId = aPINotification1.Select(c => c.UserId).FirstOrDefault();
                            int NotificationId = aPINotification1;
                            await quizzesManagementRepository.SendNotificationCustomizeSurvey(Notification, IsApplicableToAll, Convert.ToString(ApplicabilityId) , NotificationId, Notification.UserId);
                        }
                    }
                    if (pollsManagement.Status == true)
                    {
                        quizzesManagementRepository.SendPollApplicabilityPushNotification(pollsManagement.Id, OrgCode);
                    }

                    //if (pollsManagement.Status)
                    //    await pollsManagementRepository.SendNotification(pollsManagement.Question, Token);
                    return Ok(aPIPollsManagement);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost("PollsResult")] //need to be checked
        public async Task<IActionResult> PostPollsResult([FromBody] APIPollsResult aPIPollsResult)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (aPIPollsResult.Option1 == null && aPIPollsResult.Option2 == null && aPIPollsResult.Option3 == null && aPIPollsResult.Option4 == null && aPIPollsResult.Option5 == null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    PollsManagement objPoll = await pollsManagementRepository.Get(aPIPollsResult.PollsId);
                    if (objPoll.Status == false)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    if (!(string.IsNullOrEmpty(aPIPollsResult.Option5)))
                    {
                        if (objPoll.Option5 != aPIPollsResult.Option5)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                    }
                    else if (!(string.IsNullOrEmpty(aPIPollsResult.Option4)))
                    {
                        if (objPoll.Option4 != aPIPollsResult.Option4)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                    }
                    else if (!(string.IsNullOrEmpty(aPIPollsResult.Option3)))
                    {
                        if (objPoll.Option3 != aPIPollsResult.Option3)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                    }
                    else if (!(string.IsNullOrEmpty(aPIPollsResult.Option2)))
                    {
                        if (objPoll.Option2 != aPIPollsResult.Option2)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                    }
                    else if (!(string.IsNullOrEmpty(aPIPollsResult.Option1)))
                    {
                        if (objPoll.Option1 != aPIPollsResult.Option1)
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                        }
                    }

                    bool NewRecord = false;
                    PollsResult pollsResult = await pollsResultRepository.GetUserWisePoll(aPIPollsResult.PollsId, UserId);
                    if (pollsResult == null)
                    {
                        NewRecord = true;
                        pollsResult = new PollsResult();
                    }
                    else
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicatePollResponse), Description = EnumHelper.GetEnumDescription(MessageType.DuplicatePollResponse) });
                    }
                    pollsResult.PollsId = aPIPollsResult.PollsId;
                    pollsResult.UserId = UserId;
                    pollsResult.Option1 = aPIPollsResult.Option1;
                    pollsResult.Option2 = aPIPollsResult.Option2;
                    pollsResult.Option3 = aPIPollsResult.Option3;
                    pollsResult.Option4 = aPIPollsResult.Option4;
                    pollsResult.Option5 = aPIPollsResult.Option5;
                    pollsResult.IsDeleted = false;
                    pollsResult.ModifiedBy = UserId;
                    pollsResult.ModifiedDate = DateTime.UtcNow;
                    pollsResult.CreatedBy = UserId;
                    pollsResult.CreatedDate = DateTime.UtcNow;
                    if (NewRecord)
                    {
                        await pollsResultRepository.Add(pollsResult);
                        await _rewardsPointRepository.PollsResponseRewardPoint(UserId, aPIPollsResult.PollsId, objPoll.Question, OrgCode);
                    }
                    else
                    {
                        pollsResult.PollsId = aPIPollsResult.PollsId;
                        pollsResult.UserId = UserId;
                        pollsResult.Option1 = aPIPollsResult.Option1;
                        pollsResult.Option2 = aPIPollsResult.Option2;
                        pollsResult.Option3 = aPIPollsResult.Option3;
                        pollsResult.Option4 = aPIPollsResult.Option4;
                        pollsResult.Option5 = aPIPollsResult.Option5;
                        pollsResult.IsDeleted = false;
                        pollsResult.ModifiedBy = UserId;
                        pollsResult.ModifiedDate = DateTime.UtcNow;
                        pollsResult.CreatedBy = UserId;
                        pollsResult.CreatedDate = DateTime.UtcNow;
                        await pollsResultRepository.Update(pollsResult);
                    }
                    return Ok(pollsResult);
                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // POST api/<controller>
        [HttpGet("PollsManagementRead/{id:int}")]
        public async Task<IActionResult> PostCounter(int id)
        {

            try
            {
                int referenceId = 0;
                referenceId = id;
                string functionCode = "TLS0810";
                string category = "Normal";
                int point = 1;
                int userId = UserId;
                await this.pollsManagementRepository.RewardPointSave(functionCode, category, referenceId, point, userId);
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        // PUT api/<controller>/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.OpinionPollManagement)]
        public async Task<IActionResult> Put(int id, [FromBody] APIPollsManagement aPIPollsManagement)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                PollsManagement pollsManagement = await this.pollsManagementRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (pollsManagement == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                Boolean result = await this.pollsManagementRepository.ExistsInResult(id);
                if (result == true)
                {
                    return StatusCode(304, MessageType.PollInUse);
                }

                if (await this.pollsManagementRepository.Existquestion(aPIPollsManagement.Question, aPIPollsManagement.Id))
                {

                    return StatusCode(409, MessageType.Duplicate);
                }
                if (pollsManagement != null)
                {
                    //if ((aPIPollsManagement.ApplicabilityParameter == null || aPIPollsManagement.ApplicabilityParameter == "0"))
                    // if ((aPIPollsManagement.ApplicabilityParameter == null || aPIPollsManagement.ApplicabilityParameter == "0") && (aPIPollsManagement.ParameterValue != null || aPIPollsManagement.ParameterValueId != null))
                    if ((aPIPollsManagement.ApplicabilityParameter != "0") && (aPIPollsManagement.ParameterValue == null || aPIPollsManagement.ParameterValueId == null))
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    if ((aPIPollsManagement.ApplicabilityParameter == "UserId" || aPIPollsManagement.ApplicabilityParameter == "MobileNumber" || aPIPollsManagement.ApplicabilityParameter == "EmailId") && aPIPollsManagement.TargetResponseCount != 1)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    string[] options;
                    options = new string[] { aPIPollsManagement.Option1.Trim(), aPIPollsManagement.Option2.Trim(), aPIPollsManagement.Option3.Trim(), aPIPollsManagement.Option4.Trim(), aPIPollsManagement.Option5.Trim() };

                    List<string> termsList = new List<string>();
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (options[i] != "")
                        {
                            termsList.Add(options[i]);
                        }

                    }
                    options = termsList.ToArray();
                    if (options.Distinct().Count() != options.Count())
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateOptions), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateOptions) });
                    }
                    pollsManagement.StartDate = aPIPollsManagement.StartDate;
                    pollsManagement.ValidityDate = aPIPollsManagement.ValidityDate;
                    if (aPIPollsManagement.ApplicabilityParameter != "UserId" || aPIPollsManagement.ApplicabilityParameter != "MobileNumber" || aPIPollsManagement.ApplicabilityParameter != "EmailId")
                        pollsManagement.TargetResponseCount = aPIPollsManagement.TargetResponseCount;
                    pollsManagement.Question = aPIPollsManagement.Question;
                    pollsManagement.Option1 = aPIPollsManagement.Option1;
                    pollsManagement.Option2 = aPIPollsManagement.Option2;
                    pollsManagement.Option3 = aPIPollsManagement.Option3;
                    pollsManagement.Option4 = aPIPollsManagement.Option4;
                    pollsManagement.Option5 = aPIPollsManagement.Option5;
                    pollsManagement.Status = aPIPollsManagement.Status;
                    pollsManagement.ModifiedBy = UserId;
                    pollsManagement.ModifiedDate = DateTime.UtcNow;
                    if (aPIPollsManagement.ApplicabilityParameter == null && aPIPollsManagement.ParameterValue == null)
                    {
                        pollsManagement.IsApplicableToAll = true;
                    }
                    await this.pollsManagementRepository.Update(pollsManagement);

                    int ApplicabilityId = await pollsManagementRepository.UpdatePollsApplicability(pollsManagement.Id, aPIPollsManagement.ApplicabilityParameter, aPIPollsManagement.ParameterValue, aPIPollsManagement.ParameterValueId);

                    if (pollsManagement.Status == true && pollsManagement.IsApplicableToAll == true)
                    {
                        APINotifications Notification = new APINotifications();
                        Notification.Title = Record.PollNotification;
                        Notification.Type = Record.Poll;
                        string Url = this._configuration[Configuration.NotificationApi];
                        Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.PollNotification;
                        HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
                        string result1 = await response.Content.ReadAsStringAsync();
                        Notification.Message = JsonConvert.DeserializeObject(result1).ToString();
                        Notification.Message = Notification.Message.Replace("[pollquestion]", pollsManagement.Question);
                        bool IsApplicableToAll = pollsManagement.IsApplicableToAll;
                        Notification.Url = "social/";
                        Notification.UserId = Convert.ToInt32(aPIPollsManagement.ParameterValueId);
                        Notification.IsRead = false;
                        await quizzesManagementRepository.SendNotificationForQuizAndSurvey(Notification, IsApplicableToAll);
                    }
                    else if (aPIPollsManagement.ApplicabilityParameter != null && aPIPollsManagement.ParameterValue != null && pollsManagement.IsApplicableToAll == false)
                    {

                        APINotifications Notification = new APINotifications();
                        Notification.Title = Record.PollNotification;
                        Notification.Type = Record.Poll;
                        string Url = this._configuration[Configuration.NotificationApi];
                        Url = Url + "/tlsNotification/GetNotificationMessage/" + Record.PollNotification;
                        HttpResponseMessage response = await ApiHelper.CallGetAPI(Url, Token);
                        string result1 = await response.Content.ReadAsStringAsync();
                        Notification.Message = JsonConvert.DeserializeObject(result1).ToString();
                        Notification.Message = Notification.Message.Replace("[pollquestion]", pollsManagement.Question);
                        bool IsApplicableToAll = pollsManagement.IsApplicableToAll;
                        Notification.Url = "social/";
                        Notification.UserId = Convert.ToInt32(aPIPollsManagement.ParameterValueId);
                        Notification.IsRead = false;
                        if (pollsManagement.Status == true && pollsManagement.IsApplicableToAll == false)
                        {
                            int aPINotification1 = await quizzesManagementRepository.SendNotificationForQuizAndSurvey(Notification, IsApplicableToAll);

                            // int UserId = aPINotification1.Select(c => c.UserId).FirstOrDefault();
                            int NotificationId = aPINotification1;
                            await quizzesManagementRepository.SendNotificationCustomizeSurvey(Notification, IsApplicableToAll, Convert.ToString(ApplicabilityId), NotificationId, Notification.UserId);
                        }
                    }
                    if (pollsManagement.Status == true)
                    {
                        quizzesManagementRepository.SendPollApplicabilityPushNotification(pollsManagement.Id, OrgCode);
                    }
                }

                return Ok(pollsManagement);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.OpinionPollManagement)]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                PollsManagement pollsManagement = await this.pollsManagementRepository.Get(DecryptedId);

                Boolean result = await this.pollsManagementRepository.ExistsInResult(DecryptedId);

                if (ModelState.IsValid && pollsManagement != null)
                {
                    pollsManagement.IsDeleted = true;
                    await this.pollsManagementRepository.Update(pollsManagement);
                }

                if (pollsManagement == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        /// <summary>
        /// Search specific PollsManagement.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<PollsManagement> pollsManagement = await this.pollsManagementRepository.Search(q);
                return Ok(Mapper.Map<List<APIPollsManagement>>(pollsManagement));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("PollsQuestion/OpinionPollReportTypeHead/{title:minlength(0)?}")]
        public async Task<IActionResult> SurveyReportTypeHead(string title)
        {
            try
            {
                return this.Ok(await this.pollsManagementRepository.OpinionPollsReportTypeHead(title));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
