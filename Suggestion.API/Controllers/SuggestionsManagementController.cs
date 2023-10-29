// ======================================
// <copyright file="SuggestionsManagementController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Suggestion.API.APIModel;
using Suggestion.API.Common;
using Suggestion.API.Helper;
using Suggestion.API.Models;
using Suggestion.API.Repositories.Interfaces;
using Suggestion.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Suggestion.API.Common.AuthorizePermissions;
using static Suggestion.API.Common.TokenPermissions;
using log4net;
using Suggestion.API.Helper.Log_API_Count;

namespace Suggestion.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class SuggestionsManagementController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SuggestionsManagementController));
        private ISuggestionsManagementRepository suggestionsManagementRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        private IRewardsPointRepository _rewardsPointRepository;
        public SuggestionsManagementController(ISuggestionsManagementRepository suggestionsManagementController, IIdentityService identitySvc, ITokensRepository tokensRepository, IRewardsPointRepository rewardsPointRepository) : base(identitySvc)
        {
            this.suggestionsManagementRepository = suggestionsManagementController;
            this._identitySvc = identitySvc;
            this._tokensRepository = tokensRepository;
            this._rewardsPointRepository = rewardsPointRepository;
        }

        // GET: api/<controller>
        [HttpGet]
        [PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<SuggestionsManagement> suggestionsManagement = await this.suggestionsManagementRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APISuggestionsManagement>>(suggestionsManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        //[PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                SuggestionsManagement suggestionsManagement = new SuggestionsManagement();

                int createdBy = suggestionsManagement.CreatedBy;


                List<APISuggestionsManagement> suggestionsManagement1 = await this.suggestionsManagementRepository.GetUserNameById(page, pageSize, search);

                return Ok(suggestionsManagement1);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetAllSuggestionsById")]
       
        public async Task<IActionResult> GetAllSuggestions([FromBody]APIUpdateSuggestions aPIUpdateSuggestions)
        {
            try
            {
                List<APISuggestionsManagement> suggestionsManagement1 = await this.suggestionsManagementRepository.GetAllSuggestions(Convert.ToInt32(aPIUpdateSuggestions.Id));

                return Ok(suggestionsManagement1);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        //[HttpPut("UpdateSuggestionDetail")]
        //public async Task<IActionResult> UpdateSuggestions([FromBody]APIUpdateSuggestions aPIMySuggestion)
        //{

        //    SuggestionsManagement mySuggestion = await this.suggestionsManagementRepository.GetSuggestionDetail(Convert.ToInt32(aPIMySuggestion.Id));
        //    if (aPIMySuggestion.ApprovalStatus == Record.Approved)
        //        mySuggestion.ApprovalStatus = Record.Approved;
        //    else if (aPIMySuggestion.ApprovalStatus == Record.Rejected)
        //        mySuggestion.ApprovalStatus = Record.Rejected;

        //    await this.suggestionsManagementRepository.Update(mySuggestion);

        //    return Ok("status changed successfully");


        //}


        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        //[PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int suggestionsManagement = await this.suggestionsManagementRepository.Count(search);
                return Ok(suggestionsManagement);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetAllSuggestionsCount/{search?}/{searchText?}")]
     
        public async Task<IActionResult> GetAllSuggestionsCount([FromBody]APIUpdateSuggestions aPIUpdateSuggestions, string search=null,string searchText=null)
        {
            try
            {
                int suggestionsManagement = await this.suggestionsManagementRepository.GetAllSuggestionsCount(Convert.ToInt32(aPIUpdateSuggestions.Id), search, searchText);
                return Ok(suggestionsManagement);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        // GET api/<controller>/5
        [HttpGet("{id}")]
        //[PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                SuggestionsManagement suggestionsManagement = await this.suggestionsManagementRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APISuggestionsManagement>(suggestionsManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        // POST api/<controller>
        [HttpPost]
        //[PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Post([FromBody]APISuggestionsManagement aPISuggestionsManagement)
        {
            try
            {
               
                if (ModelState.IsValid)
                {
                    SuggestionsManagement suggestionsManagement = new SuggestionsManagement
                    {
                        Date = aPISuggestionsManagement.Date,
                        SuggestionDate = aPISuggestionsManagement.SuggestionDate,
                        Suggestion = aPISuggestionsManagement.Suggestion,
                        ContextualAreaofBusiness = aPISuggestionsManagement.ContextualAreaofBusiness,
                        BriefResponse = aPISuggestionsManagement.BriefResponse,
                        DetailedResponse = aPISuggestionsManagement.DetailedResponse,
                        Status = aPISuggestionsManagement.Status,
                        IsDeleted = false,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await suggestionsManagementRepository.Add(suggestionsManagement);
                    await this._rewardsPointRepository.SuggestionManagementRewardPoint(UserId, suggestionsManagement.Id, suggestionsManagement.Suggestion, OrgCode);
                    return Ok(suggestionsManagement);
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
        [PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Put(int id, [FromBody]APISuggestionsManagement aPISuggestionsManagement)
        {
            try
            {
                SuggestionsManagement suggestionsManagement = await this.suggestionsManagementRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (suggestionsManagement == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (ModelState.IsValid && suggestionsManagement != null)
                {
                    suggestionsManagement.Date = aPISuggestionsManagement.Date;
                    suggestionsManagement.SuggestionDate = aPISuggestionsManagement.SuggestionDate;
                    suggestionsManagement.Suggestion = aPISuggestionsManagement.Suggestion;
                    suggestionsManagement.ContextualAreaofBusiness = aPISuggestionsManagement.ContextualAreaofBusiness;
                    suggestionsManagement.BriefResponse = aPISuggestionsManagement.BriefResponse;
                    suggestionsManagement.DetailedResponse = aPISuggestionsManagement.DetailedResponse;
                    suggestionsManagement.Status = aPISuggestionsManagement.Status;
                    suggestionsManagement.ModifiedBy = UserId;
                    suggestionsManagement.ModifiedDate = DateTime.UtcNow;
                    await this.suggestionsManagementRepository.Update(suggestionsManagement);
                }

                return Ok(suggestionsManagement);

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.MySuggestion)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                SuggestionsManagement suggestionsManagement = await this.suggestionsManagementRepository.Get(DecryptedId);

                if (ModelState.IsValid && suggestionsManagement != null)
                {
                    suggestionsManagement.IsDeleted = true;
                    await this.suggestionsManagementRepository.Update(suggestionsManagement);
                }

                if (suggestionsManagement == null)
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
        /// Search specific MediaLibrary.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<SuggestionsManagement> suggestionsManagement = await this.suggestionsManagementRepository.Search(q);
                return Ok(Mapper.Map<List<APISuggestionsManagement>>(suggestionsManagement));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        
    }
}
