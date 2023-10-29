using AspNet.Security.OAuth.Introspection;
using TNA.API.Common;
using TNA.API.Repositories.Interfaces;
using TNA.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static TNA.API.Common.TokenPermissions;
using log4net;
using TNA.API.Model.Log_API_Count;
using TNA.API.APIModel;

namespace TNA.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    // BeSpoke Enrollment Request Levels 
    // EU-1, HR1-2, LM-3, BU-4, HR2-5
    [Produces("application/json")]
    [Route("api/v1/BespokeEnrollmentRequest")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class BespokeEnrollmentRequestController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BespokeEnrollmentRequestController));
        IBespokeEnrollmentRequest _IBespokeEnrollmentRequest;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokensRepository _tokensRepository;
        IEmail _emailRepository;
        public BespokeEnrollmentRequestController(IBespokeEnrollmentRequest IBespokeEnrollmentRequest,
                                     IIdentityService identitySvc, IEmail emailRepository, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _IBespokeEnrollmentRequest = IBespokeEnrollmentRequest;
            _emailRepository = emailRepository;
            this._tokensRepository = tokensRepository;
        }

        #region GetBeSpokeEnrollmentRequestDetails

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelTwo/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllBeSpokeEnrollmentRequestLevelTwo(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelTwo(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelTwoCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllBeSpokeEnrollmentRequestLevelTwoCount(string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelTwoCount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelThree/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllBeSpokeEnrollmentRequestLevelThree(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelThree(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelThreeCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllBeSpokeEnrollmentRequestLevelThreeCount(string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelThreeCount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelFour/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllScheduleEnrollmentRequestLevelFour(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelFour(page, pageSize, UserId, LoginId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelFourCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllBeSpokeEnrollmentRequestLevelFourCount(string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelFourCount(UserId, LoginId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelFive/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllBeSpokeEnrollmentRequestLevelFive(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelFive(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBeSpokeEnrollmentRequestLevelFiveCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllBeSpokeEnrollmentRequestLevelFiveCount(string userName = null, string search = null, string searchText = null, string status = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                return Ok(await this._IBespokeEnrollmentRequest.GetAllBeSpokeEnrollmentRequestLevelFiveCount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion

        [HttpPost("ActionsByAdminsHRLM/{level}")]
        public async Task<IActionResult> ActionsByAdminsHRLM(int level, [FromBody] APIActionsByAdmins obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._IBespokeEnrollmentRequest.ActionsByAdminsHRLM(level, obj, UserId, UserName));
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("ActionsByAdminsBUHRFinal/{level}")]
        public async Task<IActionResult> ActionsByAdminsBUHRFinal(int level, [FromBody] APIActionsByAdmins obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._IBespokeEnrollmentRequest.ActionsByAdminsBUHRFinal(level, obj, UserId));
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
        }
    }
}