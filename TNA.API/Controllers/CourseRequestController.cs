using AspNet.Security.OAuth.Introspection;
//using Courses.API.APIModel.TNA;
using TNA.API.Common;
using TNA.API.Helper;
using TNA.API.Helper.Metadata;
using TNA.API.Model.Log_API_Count;
using TNA.API.Model;
using TNA.API.Repositories.Interfaces;
//using TNA.API.Repositories.Interfaces.TNA;
using TNA.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using static TNA.API.Common.TokenPermissions;
using TNA.API.APIModel;

namespace TNA.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/CourseRequest")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CourseRequestController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseRequestController));
        ICourseRequest _ICourseRequest;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IEmail _emailRepository;
        private readonly ITokensRepository _tokensRepository;
        private static readonly ILog logger = LogManager.GetLogger(typeof(CourseRequestController));
        public CourseRequestController(ICourseRequest ICourseRequest,
                                     IIdentityService identitySvc,
                                     IEmail emailRepository, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _ICourseRequest = ICourseRequest;
            _emailRepository = emailRepository;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet("{TNAYearId}")]
        public async Task<IActionResult> Get(int TNAYearId)
        {
            try
            {
                return Ok(await this._ICourseRequest.GetAllRequestDetails(TNAYearId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetMultipleRole")]
        public async Task<IActionResult> GetMultipleRole()
        {
            try
            {
                return Ok(await this._ICourseRequest.GetMultipleRoleForUser(UserId, LoginId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Count/{TNAYearId}")]
        public async Task<IActionResult> Count(int TNAYearId, string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._ICourseRequest.GetAllRequestCount(TNAYearId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequest/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}/{RoleName?}")]
        public async Task<IActionResult> GetAllRequest(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null, string RoleName = null)
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
                return Ok(await this._ICourseRequest.GetAllRequest(page, pageSize, UserId, userName, search, searchText, status, LoginId, RoleName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestCount/{username?}/{search?}/{searchText?}/{status?}/{RoleName?}")]
        public async Task<IActionResult> GetAllCourseRequestCount(string userName = null, string search = null, string searchText = null, string status = null, string RoleName = null)
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
                return Ok(await this._ICourseRequest.GetAllCourseRequestCount(UserId, userName, search, searchText, status, LoginId, RoleName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestForTA/{page}/{pageSize}/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllCourseRequestForTA(int page, int pageSize, string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseRequest.GetAllRequestForTA(page, pageSize, UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseRequestForTACount/{username?}/{search?}/{searchText?}/{status?}")]
        public async Task<IActionResult> GetAllCourseRequestForTACount(string userName = null, string search = null, string searchText = null, string status = null)
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
                return Ok(await this._ICourseRequest.GetAllCourseRequestForTACount(UserId, userName, search, searchText, status));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("checkforRequestStatus/{courseID}/{userid?}")]
        public async Task<IActionResult> GetCourseStatus(int courseID, int? userid = null)
        {
            try
            {
                if (userid != null)
                {
                    return Ok(await this._ICourseRequest.GetCourseStatus(courseID, userid));
                }
                return Ok(await this._ICourseRequest.GetCourseStatus(courseID, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("RequestForCourse/{CourseID}")]
        public async Task<IActionResult> Post(int CourseID)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ApiResponse result = await _ICourseRequest.RequestForCourse(CourseID, UserId);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetHistoryData/{userID}")]
        public async Task<IActionResult> GetHistoryData(int userID)
        {
            try
            {
                return Ok(await this._ICourseRequest.GetHistoryData(userID));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetDataFromHRToBUHead/{TNAYearId}/{username?}/{search?}/{searchText?}")]
        public async Task<IActionResult> GetDataFromHRToBUHead(int TNAYearId, string userName = null, string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                return Ok(await _ICourseRequest.GetDataFromHRToBUHead(TNAYearId, userName, search, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetSubordinateUsersTypeAhead/{search?}")]
        public async Task<IActionResult> GetSubordinateUsers(string search = null)
        {
            try
            {
                return Ok(await _ICourseRequest.GetSubordinateUsers(UserId, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("DataFromHRToBUHeadExport/{username?}/{search?}/{searchText?}/{Role}")]
        public async Task<IActionResult> DataFromHRToBUHead(int Role, string userName = null, string search = null, string searchText = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return this.BadRequest(ModelState);

                }
                if (userName != null)
                    userName = userName.ToLower().Equals("null") ? null : userName;
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;

                FileInfo ExcelFile = await this._ICourseRequest.GetDataMigrationReport(UserId, LoginId, Role, userName, search, searchText);
                var Fs = ExcelFile.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(
                        fileData,
                        FileContentType.Excel,
                        ExcelFile.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllCourseDetailsByUserID/{UserID}/{Role}")]
        public async Task<IActionResult> GetAllCourseDetailsByUserID(int UserID, int Role)
        {
            try
            {
                return Ok(await this._ICourseRequest.GetAllCourseDetailsByUserID(UserID, UserId, Role));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllDetailsForEndUser/{courseRequestId}")]
        public async Task<IActionResult> GetAllDetailsForEndUser(int courseRequestId)
        {
            try
            {
                return Ok(await this._ICourseRequest.GetAllDetailsForEndUser(courseRequestId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllDepartmentUnderBU")]
        public async Task<IActionResult> GetAllDepartmentUnderBU()
        {
            try
            {
                return Ok(await this._ICourseRequest.GetAllDepartmentUnderBU(LoginId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] int[] CourseID)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int isExpiry = await this._ICourseRequest.CheckForTNAYearExpiry();
                if (isExpiry == 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "You can't request for this year(Year Expired)" });
                }

                int duplicateResult = await _ICourseRequest.DuplicateCourseRequestCheck(CourseID, UserId);
                if (duplicateResult != 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = "You have already requested this course" });
                }

                int courseRequestSend = await _ICourseRequest.isCourseRequestSendByUser(UserId);
                if (courseRequestSend > 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = "Request Already Sent to Line Manager. You are not allowed to Request again." });
                }

                ApiResponse result = await _ICourseRequest.PostRequest(CourseID, UserId, UserName);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostOtherCourses")]
        public async Task<IActionResult> PostOtherCourses([FromBody] JObject Data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int isExpiry = await this._ICourseRequest.CheckForTNAYearExpiry();
                if (isExpiry == 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "You can't request for this year(Year Expired)" });
                }

                string CourseName = Data.GetValue("CourseName").ToString();
                string CourseDescription = Data.GetValue("CourseDescription").ToString();

                int checkForSystemCourse = await _ICourseRequest.CheckForSystemCourse(CourseName);
                if (checkForSystemCourse != 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = "This Course is already available in catalogue." });
                }

                int duplicateResult = await _ICourseRequest.DuplicateOtherCourseRequestCheck(CourseName, UserId);
                if (duplicateResult != 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = "You have already requested this course" });
                }

                int courseRequestSend = await _ICourseRequest.isCourseRequestSendByUser(UserId);
                if (courseRequestSend > 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = "Request Already Sent to Line Manager. You are not allowed to Request again." });
                }

                ApiResponse result = await _ICourseRequest.PostOtherCourses(CourseName, CourseDescription, UserId);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostCourseRequest")]
        public async Task<IActionResult> PostCourseRequest([FromBody] int[] data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int isExpiry = await this._ICourseRequest.CheckForTNAYearExpiry();
                if (isExpiry == 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "You can't submit the request for this year(Year Expired)" });
                }

                ApiResponse result = await _ICourseRequest.PostCourseRequest(data, UserId, UserName, OrganisationCode);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostCourseRequestFromLMToHR")]
        public async Task<IActionResult> PostCourseRequestFromLMToHR([FromBody] int[] data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ApiResponse result = await _ICourseRequest.PostCourseRequestFromLMToHR(data, UserId, UserName, OrganisationCode);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostCourseRequestFromHRToBU")]
        public async Task<IActionResult> PostCourseRequestFromHRToBU([FromBody] int[] data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ApiResponse result = await _ICourseRequest.PostCourseRequestFromHRToBU(data, UserId, UserName);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostForNominate")]
        public async Task<IActionResult> PostForNominate([FromBody] APICourseRequest obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int isExpiry = await this._ICourseRequest.CheckForTNAYearExpiry();
                if (isExpiry == 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "You can't nominate for this year(Year Expired)" });
                }

                if (obj.ReasonForRejection == "" || obj.ReasonForRejection == null)
                    return BadRequest(new ResponseMessage { Message = "Reason is mandatory field.Please enter a valid input.", Description = "Reason is mandatory field.Please enter a valid input." });

                int courseRequestSend = await _ICourseRequest.isCourseRequestSend(obj, UserId, UserName);
                if (courseRequestSend > 0)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = "Request Already Sent." });
                }

                ApiResponse result = await _ICourseRequest.PostForNominate(obj, UserId, UserName);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostForAmendment/{Role}")]
        public async Task<IActionResult> PostForAmendment([FromBody] APIAmendment obj, int Role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (obj.Comment == "" || obj.Comment == null)
                    return BadRequest(new ResponseMessage { Message = "Comments is mandatory field.Please enter a valid input.", Description = "Comments is mandatory field.Please enter a valid input." });

                ApiResponse result = await _ICourseRequest.PostForAmendment(obj, UserId, UserName, Role);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = result.Description, Description = result.Description });
                }
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostFromHRToBUHead/{search?}/{searchText?}")]
        public async Task<IActionResult> PostFromHRToBUHead([FromBody] int[] CourseRequestId, string search = null, string searchText = null)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ApiResponse result = await _ICourseRequest.PostFromHRToBUHead(CourseRequestId, UserId, UserName, search, searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostForLineManager/{id}/{Role}")]
        public async Task<IActionResult> Put(int id, [FromBody] APICoursesRequestDetails obj, int Role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (obj.ReasonForRejection == "" || obj.ReasonForRejection == null)
                    return BadRequest(new ResponseMessage { Message = "Reason is mandatory field.Please enter a valid input.", Description = "Reason is mandatory field.Please enter a valid input." });

                ApiResponse result = await _ICourseRequest.Put(id, obj, UserId, UserName, Role);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = result.Description, Description = result.Description });
                }
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostCourseRequestIds")]
        public async Task<IActionResult> PostCourseRequests([FromBody] APITNAPostAcceptReject requestPayload)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //int isAccept = 0;
                ApiResponse result = await _ICourseRequest.PostCourseRequestIds(requestPayload.Id, UserId, UserName, OrganisationCode, requestPayload.isAccept);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = result.Description, Description = result.Description });
                }
                return Ok(result.Description);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
     
        [HttpPost("EnrollAll")]
        public async Task<IActionResult> EnrollAll()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ApiResponse result = await _ICourseRequest.EnrollAll(UserId, UserName);
            if (result.StatusCode == 400)
            {
                return BadRequest(new ResponseMessage { Message = result.Description, Description = result.Description });
            }
            return Ok(result.Description);
        }

        [HttpDelete("DeleteCourseRequest")]
        public async Task<IActionResult> DeleteCourseRequest([FromQuery]string id)
        {
            try
            {
                int DecryptedcourseRequestId  = Convert.ToInt32(Security.Decrypt(id));
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                CourseRequest objCourseRequest = await this._ICourseRequest.Get(DecryptedcourseRequestId);
                if (objCourseRequest == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                ApiResponse result = await _ICourseRequest.DeleteCourseRequest(DecryptedcourseRequestId, UserId);

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