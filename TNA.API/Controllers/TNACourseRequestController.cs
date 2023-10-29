using AspNet.Security.OAuth.Introspection;
using TNA.API.Common;
using TNA.API.Model;
using TNA.API.Repositories.Interfaces;
//using Courses.API.Repositories.Interfaces.TNA;
using TNA.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static TNA.API.Common.AuthorizePermissions;
using TNA.API.Helper;
using log4net;
using System.Collections.Generic;
using TNA.API.Model.Log_API_Count;
using TNA.API.APIModel;

namespace TNA.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/TNACourseRequest")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    public class TNACourseRequestController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TNACourseRequestController));
        ITNACourseRequest _ITNACourseRequest;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokensRepository _tokensRepository;
        public TNACourseRequestController(ITNACourseRequest ITNACourseRequest,
                                     IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _ITNACourseRequest = ITNACourseRequest;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet("GetTNAYear")]
        public async Task<IActionResult> GetTNAYear()
        {
            try
            {
                return Ok(await this._ITNACourseRequest.GetTNAYear());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelOne/{page}/{pageSize}/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.linemananagerapproval)]
        public async Task<IActionResult> GetAllCourseRequestLevelOne(int page, int pageSize, int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelOne(page, pageSize, TNAYearId, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelOneCount/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.linemananagerapproval)]
        public async Task<IActionResult> GetAllCourseRequestLevelOneCount(int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelOneCount(TNAYearId, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelTwo/{page}/{pageSize}/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.RequestNomination)]
        public async Task<IActionResult> GetAllCourseRequestForTA(int page, int pageSize, int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelTwo(page, pageSize, TNAYearId, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelTwoCount/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.RequestNomination)]
        public async Task<IActionResult> GetAllCourseRequestForTACount(int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelTwoCount(TNAYearId, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelThree/{page}/{pageSize}/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.HRResponsetorequest)]
        public async Task<IActionResult> GetAllCourseRequestLevelThree(int page, int pageSize, int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelThree(page, pageSize, TNAYearId, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelThreeCount/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.HRResponsetorequest)]
        public async Task<IActionResult> GetAllCourseRequestLevelThreeCount(int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelThreeCount(TNAYearId, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelFour/{page}/{pageSize}/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.BuisnessHeadResponsetorequest)]
        public async Task<IActionResult> GetAllCourseRequestLevelFour(int page, int pageSize, int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelFour(page, pageSize, TNAYearId, UserId, userName, LoginId, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestLevelFourCount/{TNAYearId}/{username?}/{search?}/{searchText?}/{status?}")]
        [PermissionRequired(Permissions.BuisnessHeadResponsetorequest)]
        public async Task<IActionResult> GetAllCourseRequestLevelFourCount(int TNAYearId, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ITNACourseRequest.GetAllCourseRequestLevelFourCount(TNAYearId, UserId, userName, LoginId, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("AllTNAYear")]
        public async Task<IActionResult> AllTNAYear()
        {
            try
            {
                return Ok(await this._ITNACourseRequest.GetAllTNAYear());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostTNAYear")]
        public async Task<IActionResult> PostTNAYear([FromBody] JObject TNAYear)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                string Year = TNAYear.GetValue("TNAYear").ToString().Trim();
                Regex regex = new Regex("^[0-9-]*$");
                if (!regex.IsMatch(Year))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotNumeric), Description = EnumHelper.GetEnumDescription(MessageType.NotNumeric) });
                }
                else if (Year == "")
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                else
                {
                    int result = await this._ITNACourseRequest.PostTNAYear(Year, UserId);
                    if (result == 0)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                }
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpDelete("DeleteCourseRequest")]
        public async Task<IActionResult> DeleteCourseRequest([FromQuery]string courseRequestId, string IsNominate)
        {
            try
            {
                int DecryptedcourseRequestId = Convert.ToInt32(Security.Decrypt(courseRequestId));
                bool DecryptedIsNominate = Convert.ToBoolean(Security.Decrypt(IsNominate));
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                CourseRequest objCourseRequest = await this._ITNACourseRequest.Get(DecryptedcourseRequestId);
                if (objCourseRequest == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                ApiResponse result = await _ITNACourseRequest.DeleteCourseRequest(DecryptedcourseRequestId, UserId, DecryptedIsNominate);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllTnaRequests/{page}/{pageSize}/{searchUser}")]
        public async Task<IActionResult> GetAllRequestedBatches(int page, int pageSize, string searchUser = null)
        {
            try
            {
                if (searchUser != null)
                    searchUser = searchUser.ToLower().Equals("null") ? null : searchUser;
                return Ok(await this._ITNACourseRequest.GetAllRequestedBatches(UserId, page, pageSize, searchUser));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllTnaRequestsCount/{searchUser}")]
        public async Task<IActionResult> GetAllRequestedBatchesCount(string searchUser = null)
        {
            try
            {
                if (searchUser != null)
                    searchUser = searchUser.ToLower().Equals("null") ? null : searchUser;
                return Ok(await this._ITNACourseRequest.GetAllRequestedBatchesCount(UserId, searchUser));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTnaRequestsByUser/{UserId}")]
        public async Task<IActionResult> GetTnaRequestsByUser(int UserId)
        {
            try
            {
                return Ok(await this._ITNACourseRequest.GetTnaRequestsByUser(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostTnaSupervisedRequest/{SelectedUserId}")]
        public async Task<IActionResult> PostTnaTnaSupervisedRequest(int SelectedUserId, [FromBody] List<TnaEmployeeNominateRequestPayload> TnaSupervisedRequest)
        {
            try
            {
                return Ok(await this._ITNACourseRequest.PostTnaTnaSupervisedRequest(SelectedUserId, UserId, TnaSupervisedRequest));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetTnaRequestsDetails/{page}/{pageSize}/{searchData}")]
        public async Task<IActionResult> GetTnaRequestsDetails(int page, int pageSize, string searchData = null)
        {
            try
            {
                if (searchData != null)
                    searchData = searchData.ToLower().Equals("null") ? null : searchData;
                return Ok(await this._ITNACourseRequest.GetTnaRequestsDetails(UserId, page, pageSize, searchData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTnaRequestsDetailsCount/{searchData}")]
        public async Task<IActionResult> GetTnaRequestsDetailsCount(string searchData = null)
        {
            try
            {
                if (searchData != null)
                    searchData = searchData.ToLower().Equals("null") ? null : searchData;
                return Ok(await this._ITNACourseRequest.GetTnaRequestsDetailsCount(UserId, searchData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
