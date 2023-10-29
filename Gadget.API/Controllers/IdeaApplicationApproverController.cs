using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Gadget.API.APIModel;
using Gadget.API.Helper;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static Gadget.API.Common.TokenPermissions;
using log4net;
using Gadget.API.Helper.Log_API_Count;

namespace Gadget.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/g/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class IdeaApplicationApproverController : IdentityController
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IdeaApplicationApproverController));
        private readonly IIdentityService _identitySvc;
        private IIdeaApplicationApproverRepository _ideaApplicationrepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        public IdeaApplicationApproverController(IIdentityService identitySvc, IIdeaApplicationApproverRepository ideaApplicationrepository, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(identitySvc)
        {

            _identitySvc = identitySvc;
            _ideaApplicationrepository = ideaApplicationrepository;
            _httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> SaveJury([FromBody] APIIdeaAssignIdea aPIIdea)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                int CheckDuplicate = await _ideaApplicationrepository.CheckDuplicateInsert(aPIIdea.UserId, aPIIdea.Region, aPIIdea.Jurylevel);
                if (CheckDuplicate >= 1)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                IdeaAssignJury _ideaapprove = Mapper.Map<IdeaAssignJury>(aPIIdea);
                _ideaapprove.CreatedBy = UserId;
                _ideaapprove.ModifiedBy = UserId;
                _ideaapprove.IsDeleted = false;
                _ideaapprove.CreatedDate = DateTime.Now;
                _ideaapprove.ModifiedDate = DateTime.Now;
                await _ideaApplicationrepository.Add(_ideaapprove);

                return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumName(MessageType.Success) });

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("GetAllAssignedJury/{page}/{pagesize}/{search}/{searchText}")]
        public async Task<IActionResult> GetAllJury(int page, int pagesize, string search, string searchText)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await _ideaApplicationrepository.GetAllAssignJuryDetails(UserId, page, pagesize, search, searchText));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllAssignedJuryCount/{search}/{searchText}")]
        public async Task<IActionResult> GetAllJuryCount(string search, string searchText)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await _ideaApplicationrepository.GetAllAssignJuryDetailsCount(search, searchText));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("DeleteJury/{Id}")]
        public async Task<IActionResult> AssignJuryDelete(int Id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                APIResponse result = await _ideaApplicationrepository.DeleteJury(Id);
                return Ok(result);
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AssignApplicationToJuries")]
        public async Task<IActionResult> AssignApplicationToJuries()
        {
            try
            {

                int res = await _ideaApplicationrepository.AssignApplicationToJuries();
                if (res == 1)
                {
                    return Ok();
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });



                }
                return Ok();
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
            //}
            //else
            //    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
        }
        [HttpGet("GetAllApplicationForJueryy/{page}/{pagesize}/{search}/{searchText}")]
        public async Task<IActionResult> GetAllApplicationForJuery(int page, int pagesize,string search, string searchText)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await _ideaApplicationrepository.GetAllApplicationForJuery(UserId, page, pagesize, search, searchText));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("GetAllApplicationForJueryyCount/{filter?}/{search?}")]
        public async Task<IActionResult> GetCount(string filter = null, string search = null)
        {
            try
            {
                if (filter != null)
                    filter = filter.ToLower().Equals("null") ? null : filter;
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                return Ok(await _ideaApplicationrepository.GetAllApplicationForJueryCount(UserId, filter, search));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpPost("PostScoreJury")]
        public async Task<IActionResult> PostScoreForJury([FromBody]IdeaApplicationJuryAssocation juryAssocation )
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                int CheckOk = await _ideaApplicationrepository.CheckandUpdate(juryAssocation,UserId);
                if (CheckOk != 1)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.JuryScoreAlreadySubmitted), Description = EnumHelper.GetEnumDescription(MessageType.JuryScoreAlreadySubmitted) });
                }
                return Ok(await _ideaApplicationrepository.CheckProgressApplicationStatus(juryAssocation.JuryId,juryAssocation.ApplicationId));

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
        }
        [HttpPost("PostStatusAdmin")]
        public async Task<IActionResult> PostStatusfromAdmin([FromBody]APIApplicationStatusFromAdmin statusFromAdmin)
        {

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(await this._ideaApplicationrepository.PostStatusforadmin(statusFromAdmin));
 
            }
           catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                throw ex;
            }
        }
    }
}
