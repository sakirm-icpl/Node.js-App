using AspNet.Security.OAuth.Introspection;
using CourseApplicability.API.APIModel;
using CourseApplicability.API.Common;
using CourseApplicability.API.Controllers;
using CourseApplicability.API.Helper.Metadata;
using CourseApplicability.API.Model.Log_API_Count;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Services;
using Courses.API.APIModel;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static CourseApplicability.API.Common.AuthorizePermissions;
using static CourseApplicability.API.Common.TokenPermissions;

namespace CourseApplicability.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/a/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class NodalCourseRequestsController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NodalCourseRequestsController));
        private INodalCourseRequestsRepository _courseRequestRepository;

        public NodalCourseRequestsController(IIdentityService identityService,
                INodalCourseRequestsRepository courseRequestRepository) : base(identityService)
        {
            _courseRequestRepository = courseRequestRepository;
        }

        [HttpGet("GetCourseRequests/{Page}/{PageSize}/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> GetCourseRequests(int Page, int PageSize, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _courseRequestRepository.GetCourseRequests(UserId, Page, PageSize, Search, SearchText, false, OrgCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCourseRequestsCount/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> GetCourseRequestsCount(string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _courseRequestRepository.GetCourseRequestsCount(UserId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ExportCourseRequests/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> ExportCourseRequests(string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                FileInfo ExcelFile;
                ExcelFile = await this._courseRequestRepository.ExportCourseRequests(UserId, OrganisationCode, Search, SearchText, true);

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(data, FileContentType.Excel, FileName.CourseRequests);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetRequestedCourseGroups/{Page}/{PageSize}/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> GetRequestedCourseGroups(int Page, int PageSize, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _courseRequestRepository.GetRequestedCourseGroups(UserId, Page, PageSize, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetRequestedCourseGroupsCount/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> GetRequestedCourseGroupsCount(string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _courseRequestRepository.GetRequestedCourseGroupsCount(UserId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetGroupCourseRequests/{GroupId}/{Page}/{PageSize}/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> GetGroupCourseRequests(int GroupId, int Page, int PageSize, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _courseRequestRepository.GetGroupCourseRequests(UserId, Page, PageSize, GroupId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetGroupCourseRequestsCount/{GroupId}/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> GetGroupCourseRequestsCount(int GroupId, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _courseRequestRepository.GetGroupCourseRequestsCount(UserId, GroupId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ExportGroupCourseRequests/{GroupId?}/{Search?}/{SearchText?}")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> ExportGroupCourseRequests(int? GroupId = null, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                FileInfo ExcelFile;
                ExcelFile = await this._courseRequestRepository.ExportGroupCourseRequests(UserId, OrganisationCode, GroupId, Search, SearchText, true);

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(data, FileContentType.Excel, FileName.CourseRequests);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("ProcessCourseRequest")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> ProcessCourseRequest([FromBody] APINodalRequest aPINodalRequests)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _courseRequestRepository.ProcessCourseRequest(UserId, aPINodalRequests, OrgCode);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("ProcessCourseGroupRequest")]
        [PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> ProcessCourseGroupRequest([FromBody] List<APINodalRequest> aPINodalRequests)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _courseRequestRepository.ProcessCourseGroupRequest(UserId, aPINodalRequests, OrgCode);
                if (result.StatusCode != 200)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("DeleteCourseRequest")]
        //[PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> DeleteCourseRequest([FromBody] APICourseRequestDelete aPICourseRequestDelete)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _courseRequestRepository.DeleteCourseRequest(UserId, aPICourseRequestDelete);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SelfRegisterCourse")]
        //[PermissionRequired(Permissions.course_registration_requests)]
        public async Task<IActionResult> SelfRegisterCourse([FromBody] APISelfCourseRequest aPISelfCourseRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _courseRequestRepository.SelfRegisterCourse(UserId, OrgCode, aPISelfCourseRequest);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("InitCourse/{CourseId}/{GroupId}")]
        public async Task<IActionResult> InitCourse(int CourseId, int GroupId)
        {
            try
            {
                var result = await _courseRequestRepository.InitCourse(UserId, CourseId, GroupId);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GroupCourseCompletion")]
        public async Task<IActionResult> GroupCourseCompletion([FromBody] List<APIGroupCourseCompletion> aPIGroupCourseCompletion)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _courseRequestRepository.GroupCourseCompletion(UserId, aPIGroupCourseCompletion);
                if (result != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
