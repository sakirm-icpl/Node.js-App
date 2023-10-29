using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Helper;
using User.API.Helper.Log_API_Count;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static User.API.Common.AuthorizePermissions;
using static User.API.Common.TokenPermissions;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/u/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class NodalGroupManagementController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserController));
        private INodalGroupManagementRepository _groupManagementRepository;
        private IConfiguration _configuration;
        public NodalGroupManagementController(IIdentityService identityService,
            INodalGroupManagementRepository groupManagementRepository,
            IConfiguration configuration):base(identityService)
        {
            _groupManagementRepository = groupManagementRepository;
            _configuration = configuration;
        }

        [HttpGet("GroupCode")]
        [AllowAnonymous]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetGroupCode()
        {
            try
            {
                GroupCode batchCode = await _groupManagementRepository.GetGroupCode(UserId);
                string Code = "GROUP" + batchCode.Id;
                Code = JsonConvert.SerializeObject(Code);
                return Ok(Code);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("CancelGroupCode")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> CancelGroupCode([FromBody] APIGroupCode aPIGroupCode)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                await _groupManagementRepository.CancelGroupCode(aPIGroupCode, UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCourseGroups/{Page}/{PageSize}/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetCourseGroups(int Page, int PageSize, string Search=null, string SearchText=null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetCourseGroups(UserId, Page, PageSize, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCourseGroupsCount/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetCourseGroupsCount(string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetCourseGroupsCount(UserId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCourseGroupUsers/{GroupId}/{Page}/{PageSize}/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetCourseGroupUsers(int GroupId, int Page, int PageSize, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetCourseGroupUsers(UserId, GroupId, Page, PageSize, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCourseGroupUsersCount/{GroupId}/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetCourseGroupUsersCount(int GroupId, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetCourseGroupUsersCount(UserId, GroupId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("ExportFormat")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> ExportImportFormat()
        {
            try
            {
                var result = await _groupManagementRepository.ExportImportFormat(OrgCode);
                Response.ContentType = FileContentType.Excel;
                return File(result, FileContentType.Excel, APIFileName.GroupImportFormat);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [Route("SaveFileData")]
        //[PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> PostFile([FromBody] APINodalUserGroups aPINodalUserGroups)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                ApiResponse response = await _groupManagementRepository.ProcessImportFile(aPINodalUserGroups, UserId, OrgCode);
                return Ok(response.ResponseObject);
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }
        [HttpPost("DeleteUser")]
        //[PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> DeleteUser([FromBody] APINodalUserDelete aPINodalUserDelete)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                var response = await _groupManagementRepository.DeleteUser(aPINodalUserDelete, UserId);
                if (response != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = response });

                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }
        [HttpPost("DeleteGroup")]
        //[PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> DeleteGroup([FromBody] APINodalGroupDelete aPINodalGroupDelete)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                var response = await _groupManagementRepository.DeleteGroup(aPINodalGroupDelete, UserId);
                if (response != "Success")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = response });

                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }
        [HttpGet("GetCourseRegistrations/{Page}/{PageSize}/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetCourseRegistrations(int Page, int PageSize, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetCourseRegistrations(UserId, Page, PageSize, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCourseRegistrationsCount/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetCourseRegistrationsCount(string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetCourseRegistrationsCount(UserId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetApprovedCourseGroupUsers/{GroupId}/{Page}/{PageSize}/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetApprovedCourseGroupUsers(int GroupId, int Page, int PageSize, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetApprovedCourseGroupUsers(UserId, GroupId, Page, PageSize, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetApprovedCourseGroupUsersCount/{GroupId}/{Search?}/{SearchText?}")]
        //[PermissionRequired(Permissions.iltbatch)]
        public async Task<IActionResult> GetApprovedCourseGroupUsers(int GroupId, string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                var result = await _groupManagementRepository.GetApprovedCourseGroupUsersCount(UserId, GroupId, Search, SearchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Pay/{RequestId}")]
        [AllowAnonymous]
        public async Task<IActionResult> MakePayment(string RequestId)
        {
            try
            {
                string DecRequestId = string.Empty;
                try
                {
                    DecRequestId = Security.Decrypt(RequestId);
                }
                catch(Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                //if(!string.IsNullOrEmpty(DecRequestId))
                //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                APIPaymentRequestData result = await _groupManagementRepository.MakePayment(UserId, DecRequestId);

                string formId = "onePayForm";

                StringBuilder htmlForm = new StringBuilder();
                htmlForm.AppendLine("<html>");
                htmlForm.AppendLine(String.Format("<body onload='document.forms[\"{0}\"].submit()'>", formId));
                htmlForm.AppendLine(String.Format("<form id='{0}' method='POST' action='{1}'>", formId, result.Url));
                htmlForm.AppendLine(String.Format("<input type='hidden' name='reqData' id='reqData' value='{0}' />", result.requestData));
                htmlForm.AppendLine(String.Format("<input type='hidden' name='merchantId' id='merchantId' value='{0}' />", result.merchantId));
                htmlForm.AppendLine("</form>");
                htmlForm.AppendLine("</body>");
                htmlForm.AppendLine("</html>");

                string sWebRootFolder = _configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
                string sFileName = Guid.NewGuid() + ".html";
                string DomainName = _configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                    file.Delete();
                
                System.IO.File.WriteAllText(Path.Combine(sWebRootFolder, sFileName), htmlForm.ToString());
                return Redirect(URL);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new ContentResult() { Content = JsonConvert.SerializeObject(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }), ContentType = "application/json", StatusCode =400 };
            }
        }
    }
}
