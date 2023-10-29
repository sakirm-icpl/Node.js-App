using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Gadget.API.Common.TokenPermissions;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static Gadget.API.Common.AuthorizePermissions;
using Gadget.API.Models;
using AutoMapper;
using Gadget.API.Helper;
using Gadget.API.APIModel;
using Gadget.API.Metadata;
using System.IO;
using Microsoft.Extensions.Configuration;
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
    public class ProjectController : IdentityController
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProjectController));
        private readonly IIdentityService _identitySvc;
        private IProjectRepository _projectRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        public ProjectController(IIdentityService identitySvc, IProjectRepository projectRepository, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(identitySvc)
        {
            
            _identitySvc = identitySvc;
            _projectRepository = projectRepository;
            _httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
        }
        [HttpPost]
        // [PermissionRequired(Permissions.OpinionPollManagement)]
        public async Task<IActionResult> Post([FromBody] APIProjectMaster projectmaser)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int CountByUser = await _projectRepository.GetCountByUser(UserId);

                if (CountByUser >= 3)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.CompletedAttempts), Description = EnumHelper.GetEnumDescription(MessageType.CompletedAttempts) });


                ProjectMaster _projectmaster = Mapper.Map<ProjectMaster>(projectmaser);
                _projectmaster.CreatedBy = UserId;
                _projectmaster.CreatedDate = DateTime.UtcNow;
                _projectmaster.IsDeleted = false;
                _projectmaster.RowGuid = Guid.NewGuid();

                await _projectRepository.Add(_projectmaster);
                return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.SubmittedAttempts), Description = CountByUser.ToString() });
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                
            }
            return Ok();
        }

        [HttpPost("GetTileDetails")]
        // [PermissionRequired(Permissions.MediaLibrary)]
        public async Task<IActionResult> Get([FromBody] APIGetTileDetails tagdata)
        {

            List<APITileDetails> mediaLibrary = await this._projectRepository.GetTileDetails(tagdata.Tag);
            if (mediaLibrary.Count > 0)
                return Ok(Mapper.Map<List<APITileDetails>>(mediaLibrary));

            return NoContent();
        }
        [HttpPost("SaveTeamMember")]
        public async Task<IActionResult> SaveTeamMemberDetails([FromBody] APIProjectTeamDetails Teamdetails )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int CountByUser = await _projectRepository.GetCountByUserApplication(UserId);

                if (CountByUser >= 3)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.CompletedAttemptsofApplication), Description = EnumHelper.GetEnumDescription(MessageType.CompletedAttempts) });


                APIGetProjectTeam result = await this._projectRepository.SaveTeamMemberDetails(Teamdetails, UserId);
                if (result == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.ApplicationCode });
                }
                result.CountByUser = CountByUser.ToString();
                return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            
        }
        [HttpPost("PostFileUpload")]
        public async Task<IActionResult> UploadApplicationFile([FromForm] APIProjectFileforUpload aPIProjectFileforUpload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else
            {
                //if ( aPIProjectFileforUpload.FileType != null)
                //{
                    var request = _httpContextAccessor.HttpContext.Request;
                    if (request.Form.Files.Count > 0)
                    {
                        foreach (IFormFile uploadedFile in request.Form.Files)
                        {
                        if (uploadedFile.Length <= 2000000)
                        {
                            if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                            {
                                aPIProjectFileforUpload.FileType = FileType.Image;
                                aPIProjectFileforUpload.FilePath = await this._projectRepository.SaveFile(uploadedFile, FileType.Image, OrgCode, aPIProjectFileforUpload.ApplicationCode);


                            }
                            else
                            {
                                foreach (string docType in FileType.Doc)
                                {
                                    if ((uploadedFile.ContentType.Contains(docType)) && (FileValidation.IsValidLCMSDocument(uploadedFile)))
                                    {
                                        aPIProjectFileforUpload.FileType = docType;
                                        aPIProjectFileforUpload.FilePath = await this._projectRepository.SaveFile(uploadedFile, docType, OrgCode, aPIProjectFileforUpload.ApplicationCode);


                                    }
                                }
                            }
                        }
                        else
                        {
                            return StatusCode(412, "File size is too large,please choose file upto 2MB.");
                        }
                        }
                    }
                return Ok(aPIProjectFileforUpload);

                //}
                //return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = aPIProjectFileforUpload.FilePath });

            }

        }
        [HttpPost("SaveApplicationDetails")]
        public async Task<IActionResult> SaveProjectApplication([FromBody]APISaveProjectApplication aPISaveProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (aPISaveProject.ApplicationCode != null || aPISaveProject.beforeFilePath != null || aPISaveProject.afterFilePath != null)
            {
                var result = await this._projectRepository.SaveProjectApplication(aPISaveProject, UserId);
                if (result == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.ApplicationCode });
                }
                return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = "" });
            }
             return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "" });
            
        }
        [HttpGet("GetUserAppDetails")]
        public async Task<IActionResult> Get()
        {
            APIGetProjectAppDetails GetAppDetails = await this._projectRepository.GetProjectUserAppDetails(UserId);
            if (GetAppDetails.ApplicationCode == null )
            {
                return StatusCode(204,"Data is null");
            }
            return Ok(Mapper.Map<APIGetProjectAppDetails>(GetAppDetails));

        }
        [HttpGet("GetUserProjectReport")]
        public async Task<IActionResult> GetUserReport()
        {
            List<APIGetUserProjectReport> projectreport = await this._projectRepository.GetUserProjectReport(UserId);
            if (projectreport.Count > 0)
                return Ok(Mapper.Map<List<APIGetUserProjectReport>>(projectreport));

            return NoContent();
        }
        [HttpPost("GetUserFormDetails")]
        public async Task<IActionResult> GetUserFormDetails([FromBody]APIGetFormDetails aPIGetForm)
        {
            if (aPIGetForm.Type.Contains("Nomination"))
            {
                APIProjectMaster projectMaster = await this._projectRepository.GetNominationUserDetails(aPIGetForm.Id);
                if (projectMaster != null)
                    return Ok(Mapper.Map<APIProjectMaster>(projectMaster));

            }
            if (aPIGetForm.Type.Contains("Application Form"))
            {
                APIGetProjectAppDetails aPISave = await this._projectRepository.GetApplicationDetailsbyId(aPIGetForm.Id,UserId);
                if (aPISave != null)
                    return Ok(Mapper.Map<APIGetProjectAppDetails>(aPISave));
            }
            return NoContent();
        }

        [HttpGet("GetAll/{page}/{pageSize}/{filter?}/{search?}")]
        [Produces(typeof(List<APIGetUserProjectReport>))]
       // [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> Get(int page , int pageSize , string filter = null, string search = null)
        {

            if (filter != null)
                filter = filter.ToLower().Equals("null") ? null : filter;
            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            List<APIGetAllProjectApplicationList> aPISaves = await _projectRepository.GetAll(UserId,page, pageSize, filter, search);
            return Ok(aPISaves);
        }
        [HttpGet("GetAllCount/{filter?}/{search?}")]
        public async Task<IActionResult> GetCount(string filter = null, string search = null)
        {

            if (filter != null)
                filter = filter.ToLower().Equals("null") ? null : filter;
            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            return Ok(await _projectRepository.GetAllCount(filter, search));

        }
        [HttpPost("GetSocialFile")]
        //[PermissionRequired(Permissions.)]
        public IActionResult GetFile([FromBody] APIFileInfo socialfile)
        {
            try
            {
                //var file =  Path.Combine("E:/development/Services/Source/Services/Gadget/Gadget.API",
                //             "LXPFiles", socialfile); 
                if (!ModelState.IsValid || socialfile.socialfile == null)
                {
                    return BadRequest(ModelState);
                }

                if (Path.GetExtension(socialfile.socialfile) == ".jpeg")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/jpeg");
                }
                else if (Path.GetExtension(socialfile.socialfile).ToLower() == ".jpg")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/jpeg");

                }
                else if (Path.GetExtension(socialfile.socialfile) == ".png")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "image/png");
                }
                else if (Path.GetExtension(socialfile.socialfile).ToLower() == ".pdf")
                {
                    return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/pdf");
                }
                else
                {
                    var res = socialfile.socialfile;
                    return this.Ok(res);
                    //return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/pdf");
                    //return PhysicalFile(this._configuration["ApiGatewayLXPFiles"] + socialfile.socialfile, "application/pdf");
                }

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
            //return PhysicalFile(" C:/Publish/ApiGateway/LXPFiles/" + socialfile.socialfile, "application/pdf");
            //return PhysicalFile("E:/development/Services/Source/Services/Gadget/Gadget.API/LXPFiles" + socialfile.socialfile, "application/pdf");
        }

    }
}