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
using System.Threading.Tasks;
using static ILT.API.Common.AuthorizePermissions;
using static ILT.API.Common.TokenPermissions;
using ILT.API.Helper;
using log4net;
using ILT.API.Model.Log_API_Count;

namespace ILT.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/TopicMaster")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class TopicMasterController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TopicMasterController));
        ITopicMaster _ITopicMaster;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration { get; }
        private readonly ITokensRepository _tokensRepository;
        public TopicMasterController(ITopicMaster ITopicMaster, IHttpContextAccessor IHttpContextAccessor,
                                     IIdentityService identitySvc, IConfiguration configuration, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _ITopicMaster = ITopicMaster;
            _httpContextAccessor = IHttpContextAccessor;
            _configuration = configuration;
            this._tokensRepository = tokensRepository;
        }

       
        [HttpGet("GetTopics/{searchText?}")]
        [PermissionRequired(Permissions.topics)]
        public async Task<IActionResult> GetTopics(string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._ITopicMaster.GetTopics(searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }

       
        [HttpGet("Count/{searchText?}")]
        [PermissionRequired(Permissions.topics)]
        public async Task<IActionResult> GetTopicsCount(string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._ITopicMaster.GetTopicsCount(searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }

        [HttpGet("{page}/{pageSize}/{searchText?}")]
        [PermissionRequired(Permissions.topics)]
        public async Task<IActionResult> Get(int page, int pageSize, string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._ITopicMaster.GetAllTopics(page, pageSize, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TotalCount/{searchText?}")]
        [PermissionRequired(Permissions.topics)]
        public async Task<IActionResult> GetCount(string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._ITopicMaster.GetTopicsCount(searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.topics)]
        public async Task<IActionResult> PostTopics([FromBody] APITopicMaster obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _ITopicMaster.Exists(obj.TopicName))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                else
                {
                    TopicMaster topicMaster = new TopicMaster();
                    topicMaster.TopicName = obj.TopicName;
                    topicMaster.IsActive = true;
                    topicMaster.IsDeleted = false;
                    topicMaster.CreatedBy = UserId;
                    topicMaster.CreatedDate = DateTime.UtcNow;
                    topicMaster.ModifiedBy = UserId;
                    topicMaster.ModifiedDate = DateTime.UtcNow;

                    await _ITopicMaster.Add(topicMaster);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }

        [HttpPost("{Id}")]
        [PermissionRequired(Permissions.topics)]
        public async Task<IActionResult> UpdateTopic(int Id, [FromBody] TopicMaster obj)
        {
            try
            {
                TopicMaster topicMaster = await _ITopicMaster.Get(Id);

                if (topicMaster == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (await _ITopicMaster.Exists(obj.TopicName))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                else
                {
                    topicMaster.TopicName = obj.TopicName;
                    await _ITopicMaster.Update(topicMaster);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.topics)]
        public async Task<IActionResult> DeleteTopic([FromQuery]string id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                TopicMaster topicMaster = await _ITopicMaster.Get(DecryptedId);
                if (topicMaster == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                bool IsExistsInAssociation = await _ITopicMaster.CheckForExistance(DecryptedId);

                if (IsExistsInAssociation == false)
                {
                    topicMaster.IsDeleted = true;
                    await _ITopicMaster.Update(topicMaster);
                    return Ok();
                }
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }
    }
}