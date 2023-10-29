using AspNet.Security.OAuth.Introspection;
//using TNA.API.APIModel.TNA;
using TNA.API.Common;
using TNA.API.Repositories.Interfaces;
//using Courses.API.Repositories.Interfaces.TNA;
using TNA.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static TNA.API.Common.TokenPermissions;
using TNA.API.Helper;
using log4net;
using TNA.API.Model.Log_API_Count;
using TNA.API.APIModel;

namespace TNA.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    // Schedule Enrollment Request Levels (Without BeSpoke)
    // EU-1, LM-2, TA-3, BU-4, HR1-5, HR2-6
    [Produces("application/json")]
    [Route("api/v1/ScheduleEnrollmentRequest")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ScheduleEnrollmentRequestController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ScheduleEnrollmentRequestController));
        ICourseScheduleEnrollmentRequest _ICourseScheduleEnrollmentRequest;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokensRepository _tokensRepository;
        IEmail _emailRepository;
        public ScheduleEnrollmentRequestController(ICourseScheduleEnrollmentRequest ICourseScheduleEnrollmentRequest,
                                     IIdentityService identitySvc,
                                     IEmail emailRepository, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _ICourseScheduleEnrollmentRequest = ICourseScheduleEnrollmentRequest;
            _emailRepository = emailRepository;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet("GetModuleCompletionStatus/{ModuleID}")]
        public async Task<IActionResult> GetModuleCompletionStatus(int ModuleID)
        {
            try
            {
                  return Ok(await this._ICourseScheduleEnrollmentRequest.GetModuleCompletionStatus(ModuleID, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseRequestedFrom/{courseID}")]
        public async Task<IActionResult> GetCourseRequestedFrom(int CourseID)
        {
            try
            {
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetCourseRequestedFrom(CourseID, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseNameTypeAhead/{type}/{search?}")]
        public async Task<IActionResult> GetCourseName(string type, string search = null)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._ICourseScheduleEnrollmentRequest.GetCourseName(type, search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByModuleID/{moduleId}/{courseId}")]
        public async Task<IActionResult> GetByModuleID(int moduleId, int courseId)
        {
            try
            {
                var result = await this._ICourseScheduleEnrollmentRequest.GetAllRequestDetails(moduleId, courseId, UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

   

        [HttpGet("GetUsersForNomination/{level}/{scheduleID}/{courseId}/{moduleId}/{page}/{pageSize}/{search?}/{searchText?}")]
        public async Task<IActionResult> GetUsersForNomination(int Level, int scheduleID, int courseId, int moduleId, int page, int pageSize, string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;

                return Ok(await this._ICourseScheduleEnrollmentRequest.GetUsersForNomination(Level, scheduleID, courseId, moduleId, UserId, LoginId, page, pageSize, search, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetUsersCountForNomination/{level}/{scheduleID}/{courseId}/{moduleId}/{search?}/{searchText?}")]
        public async Task<IActionResult> GetUsersCountForNomination(int Level, int scheduleID, int courseId, int moduleId, string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;

                return Ok(await this._ICourseScheduleEnrollmentRequest.GetUsersCountForNomination(Level, scheduleID, courseId, moduleId, UserId, LoginId, search, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

 
        [HttpPost]
        public async Task<IActionResult> RequestSchedule([FromBody] APICourseScheduleEnrollmentRequest obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool checkDuplicate = await _ICourseScheduleEnrollmentRequest.CheckDuplicate(obj, UserId);
                    if (checkDuplicate == true)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                    ApiResponse result = await _ICourseScheduleEnrollmentRequest.PostRequestSchedule(obj, UserId, UserName, OrganisationCode);
                    return Ok(result);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #region GetEnrollmentRequestDetails

        [HttpGet("GetAllScheduleEnrollmentRequestLevelTwo/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllScheduleEnrollmentRequestLevelTwo(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetAllScheduleEnrollmentRequestLevelTwo(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetScheduleEnrollmentRequestLevelTwoCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetScheduleEnrollmentRequestLevelTwoCount(string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetScheduleEnrollmentRequestLevelTwoCount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllScheduleEnrollmentRequestLevelThree/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllScheduleEnrollmentRequestLevelThree(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetAllScheduleEnrollmentRequestLevelThree(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetScheduleEnrollmentRequestLevelThreeCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetScheduleEnrollmentRequestLevelThreeCount(string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetScheduleEnrollmentRequestLevelThreeCount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllScheduleEnrollmentRequestLevelFour/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetAllScheduleEnrollmentRequestLevelFour(page, pageSize, UserId, LoginId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetScheduleEnrollmentRequestLevelFourCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetScheduleEnrollmentRequestLevelFourCount(string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetScheduleEnrollmentRequestLevelFourCount(UserId, LoginId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllScheduleEnrollmentRequestLevelFive/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllScheduleEnrollmentRequestLevelFive(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetAllScheduleEnrollmentRequestLevelFive(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetScheduleEnrollmentRequestLevelFiveCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetScheduleEnrollmentRequestLevelFiveCount(string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetScheduleEnrollmentRequestLevelFiveCount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllScheduleEnrollmentRequestLevelSix/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllScheduleEnrollmentRequestLevelSix(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetAllScheduleEnrollmentRequestLevelSix(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetScheduleEnrollmentRequestLevelSixCount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetScheduleEnrollmentRequestLevelSixCount(string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetScheduleEnrollmentRequestLevelSixCount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion

        [HttpPost("ActionsByAdminsLMBU/{level}")]
        public async Task<IActionResult> ActionsByAdmins(int level, [FromBody] APIActionsByAdmins obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._ICourseScheduleEnrollmentRequest.ActionsByAdmins(level, obj, UserId, UserName, OrganisationCode));
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("ActionsByAdminsHR/{level}")]
        public async Task<IActionResult> ActionsByAdminsHR(int level, [FromBody] APIActionsByAdmins obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._ICourseScheduleEnrollmentRequest.ActionsByAdminsHR(level, obj, UserId, OrganisationCode));
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("NominateUsersForAdminsLMTA/{level}")]
        public async Task<IActionResult> NominateUsersForAdmins(int level, [FromBody] NominateUsersForAdmins obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._ICourseScheduleEnrollmentRequest.NominateUsersForAdmins(level, obj, UserId, OrganisationCode));
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("NominateUsersForAdminsBUHR/{level}")]
        public async Task<IActionResult> NominateUsersForAdminsBUHR(int level, [FromBody] NominateUsersForAdmins obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._ICourseScheduleEnrollmentRequest.NominateUsersForAdminsBUHR(level, obj, UserId, OrganisationCode));
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllDetailsForEndUser/{scheduleRequestId}")]
        public async Task<IActionResult> GetAllDetailsForEndUser(int scheduleRequestId)
        {
            try
            {
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetAllDetailsForEndUser(scheduleRequestId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
       
        [HttpPost("BeSpokeEnrollRequest")]
        public async Task<IActionResult> BeSpokeEnrollRequest([FromBody]APIBeSpokeSearch beSpokeSearch)
        {
            try
            {
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetScheduleEnrollRequest(beSpokeSearch, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("BeSpokeRequestCount")]
        public async Task<IActionResult> RequestCount([FromBody]APIBeSpokeSearch search)
        {
            try
            {
                return Ok(await this._ICourseScheduleEnrollmentRequest.GetScheduleEnrollRequestCount(UserId, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }
}