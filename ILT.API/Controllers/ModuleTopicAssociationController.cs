using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Model.ILT;
using ILT.API.Repositories.Interfaces;
//using ILT.API.Repositories.Interfaces.ILT;
using ILT.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ILT.API.Common.TokenPermissions;
using ILT.API.Helper;
using log4net;
using ILT.API.Model.Log_API_Count;

namespace ILT.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/ModuleTopicAssociation")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ModuleTopicAssociationController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleTopicAssociationController));
        IModuleTopicAssociation _IModuleTopicAssociation;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration { get; }
        private readonly ITokensRepository _tokensRepository;
        public ModuleTopicAssociationController(IModuleTopicAssociation IModuleTopicAssociation, IHttpContextAccessor IHttpContextAccessor,
                                     IIdentityService identitySvc, IConfiguration configuration, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _IModuleTopicAssociation = IModuleTopicAssociation;
            _httpContextAccessor = IHttpContextAccessor;
            _configuration = configuration;
            this._tokensRepository = tokensRepository;
        }

       
        [HttpGet("GetModuleTypeAhead/{search?}")]
        public async Task<IActionResult> GetModuleTypeAhead(string search = null)
        {
            try
            {
                return Ok(await this._IModuleTopicAssociation.GetModuleTypeAhead(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

       
        [HttpGet("{page}/{pageSize}/{searchText?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IModuleTopicAssociation.Get(page, pageSize, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

       
        [HttpGet("Count/{searchText?}")]
        public async Task<IActionResult> GetCount(string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IModuleTopicAssociation.GetCount(searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

       
        [HttpGet("GetTopicDetailsByModuleId/{moduleId}")]
        public async Task<IActionResult> GetTopicDetailsByModuleId(int moduleId)
        {
            try
            {
                return Ok(await this._IModuleTopicAssociation.GetTopicDetailsByModuleId(moduleId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> PostAssociation([FromBody] APIModuleTopicAssociation aPIModuleTopicAssociation)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _IModuleTopicAssociation.PostAssociation(aPIModuleTopicAssociation, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }

       
        [HttpDelete("DeleteTopic")]
        public async Task<IActionResult> DeleteTopic([FromQuery]string id)
        {
            try
            {
                int DecryptedmoduleId = Convert.ToInt32(Security.Decrypt(id));
                List<ModuleTopicAssociation> moduleTopicAssociationList = await _IModuleTopicAssociation.IsExists(DecryptedmoduleId);
                if (moduleTopicAssociationList == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                bool CheckInAttendance = await _IModuleTopicAssociation.CheckInAttendance(DecryptedmoduleId);
                if (CheckInAttendance == true)
                {
                    return StatusCode(406, "Dependancy exist! ");
                }

                foreach (ModuleTopicAssociation item in moduleTopicAssociationList)
                {
                    ModuleTopicAssociation moduleTopic = new ModuleTopicAssociation();
                    moduleTopic.ID = item.ID;
                    moduleTopic.ModuleId = item.ModuleId;
                    moduleTopic.TopicId = item.TopicId;
                    moduleTopic.IsActive = true;
                    moduleTopic.IsDeleted = true;
                    await _IModuleTopicAssociation.Update(moduleTopic);
                }
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}