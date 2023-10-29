using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.TokenPermissions;
using log4net;
using Courses.API.Repositories;
using Courses.API.APIModel.ThirdPartyIntegration;
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/LCMS")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class LCMSController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LCMSController));
        ILCMSRepository _lcmsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IConfiguration _configuration;
        private readonly ILcmsQuestionAssociation _lcmsQuestionAssociation;
        private readonly ITokensRepository _tokensRepository;
        IModuleRepository _moduleRepository;

        public LCMSController(ILCMSRepository lcmsRepository,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration,
             IModuleRepository moduleRepository,
            ILcmsQuestionAssociation lcmsQuestionAssociation,
            IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _lcmsRepository = lcmsRepository;
            _lcmsQuestionAssociation = lcmsQuestionAssociation;
            _moduleRepository = moduleRepository;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Get()
        {
            try
            {
                if (await _lcmsRepository.Count() == 0)
                {
                    return NotFound();
                }
                return Ok(_lcmsRepository.GetAll());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var Lcms = await _lcmsRepository.Get(id);
                if (Lcms == null)
                {
                    return NotFound();
                }
                LCMSAPI lCMSAPI = Mapper.Map<LCMSAPI>(Lcms);
                lCMSAPI.IsBuiltInAssesment = Lcms.IsBuiltInAssesment.ToString();
                lCMSAPI.IsMobileCompatible = Lcms.IsMobileCompatible.ToString();
                lCMSAPI.Duration = Lcms.Duration.ToString();
                lCMSAPI.ZipPath = ""; 
                lCMSAPI.InternalName = "";
                lCMSAPI.Path = Lcms.Path;
                lCMSAPI.ExternalLCMSId = Lcms.ExternalLCMSId;
                if (Lcms.ContentType.ToLower().Contains("scorm"))
                {
                    int index = Lcms.Path.LastIndexOf('/');
                    string Path = null;
                    if (index != -1)
                        Path = Lcms.Path.Substring(index + 1);
                    lCMSAPI.StartPagePath = Path;
                    lCMSAPI.ScormType = Lcms.ContentType;
                    lCMSAPI.ContentType = Lcms.ContentType;
                }

                return Ok(lCMSAPI);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}/{contentType?}")]
        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.SurveyManagement)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string contentType = null)
        {
            try
            {
                List<LCMS> lcms = await _lcmsRepository.Get(page, pageSize, search, contentType);
                return Ok(lcms);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("getTotalRecords/{search:minlength(0)?}/{contentType?}")]
        [PermissionRequired(Permissions.lcmsmanage + " " + Permissions.SurveyManagement)]
        public async Task<IActionResult> GetCount(string search = null, string contentType = null)
        {
            try
            {
                int count = await _lcmsRepository.Count(search, contentType);

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("VimeoVideo")]
        public async Task<IActionResult> PostVimeo([FromBody]VimeoVideo vimeoVideo)
        {
            try
            {
                if(vimeoVideo == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidPostRequest), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                int result = await _lcmsRepository.SaveVimeoLink(vimeoVideo, UserId);
                if(result == 1)
                {
                    return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success),StatusCode = 200});
                }
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidPostRequest), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpPost("GetVimeoVideo")]
        public async Task<IActionResult> GetVimeoVideo([FromBody]LCMSID lCMSID)
        {
            try
            {
                if (lCMSID == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateContent), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateContent) });
                }
                VimeoLink vimeoLink = await _lcmsRepository.GetVimeoVideo(lCMSID);
                if (vimeoLink == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateContent), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateContent) });

                }
                return Ok(vimeoLink);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetVimeoToken")]
        public async Task<IActionResult> GetVimeoToken()
        {
            try
            {
                VimeoConfiguration vimeoConfiguration = await _lcmsRepository.GetVimeoToken();
                if(vimeoConfiguration == null)
                {
                    return NoContent();
                }
                return Ok(vimeoConfiguration);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpPost]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Post([FromForm] LCMSAPI lcmsApi)
        {
            try
            {
                string CoursesPath = string.Empty;
                string FilePath = string.Empty;
                string FileName = string.Empty;
                int Value;
                if (ModelState.IsValid)
                {

                    if (await _lcmsRepository.FileExist(lcmsApi.Name, lcmsApi.ContentType))
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateContent), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateContent) }); ;

                    }
                    var request = _httpContextAccessor.HttpContext.Request;
                    if (request.Form.Files.Count > 0)
                    {                        
                        foreach (IFormFile uploadedFile in request.Form.Files)
                        {
                            LCMS Lcms = new LCMS();
                            Lcms.ContentType = lcmsApi.ContentType;
                            Lcms.Description = lcmsApi.Description;
                            Lcms.Language = lcmsApi.Language;
                            Lcms.MetaData = lcmsApi.MetaData;
                            Lcms.Name = lcmsApi.Name;
                            Lcms.Version = lcmsApi.Version;
                            Lcms.Duration = lcmsApi.Duration == null ? 0 : float.Parse(lcmsApi.Duration.ToString());
                            Lcms.IsBuiltInAssesment = lcmsApi.IsBuiltInAssesment == null ? false : Convert.ToBoolean(lcmsApi.IsBuiltInAssesment.ToString());
                            Lcms.IsMobileCompatible = lcmsApi.IsMobileCompatible == null ? false : Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
                            Lcms.CreatedBy = UserId;
                            Lcms.ModifiedBy = UserId;
                            Lcms.CreatedDate = DateTime.UtcNow;
                            Lcms.IsActive = lcmsApi.IsActive;
                            Lcms.Ismodulecreate = lcmsApi.Ismodulecreate;
                            Lcms.SubContentType = lcmsApi.subContentType;
                            if ((uploadedFile.ContentType.Contains(FileType.Video)) && (FileValidation.IsValidLCMSVideo(uploadedFile)))
                            {
                                var KPOINT = await _lcmsRepository.GetMasterConfigurableParameterValue("KPOINT");
                                if(KPOINT != null)
                                {
                                    if(KPOINT.ToLower() == "yes" && lcmsApi.ContentType.ToLower() == "kpoint")
                                    {
                                        int ret = await this._lcmsRepository.GetAuthenticationKpoint(uploadedFile, uploadedFile.FileName.Trim(), uploadedFile.FileName.Trim(), Lcms.Description, Lcms, UserId);
                                        if(ret == 1)
                                        {
                                             return Ok();
                                        }
                                    }
                                    else
                                    {
                                        var Result = await this._lcmsRepository.SaveVideo(uploadedFile, Lcms, UserId, OrganisationCode);
                                        if (Result.StatusCode == StatusCodes.Status409Conflict)
                                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = Result.Description });
                                        return Ok(1);
                                    }
                                }
                                else
                                {
                                    var Result = await this._lcmsRepository.SaveVideo(uploadedFile, Lcms, UserId, OrganisationCode);
                                    if (Result.StatusCode == StatusCodes.Status409Conflict)
                                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = Result.Description });
                                    return Ok(1);
                                }
                            }
                            else if ((uploadedFile.ContentType.Contains(FileType.Audio)) && (FileValidation.IsValidLCMSAudio(uploadedFile)))
                            {
                                var Result = await this._lcmsRepository.SaveAudio(uploadedFile, Lcms, UserId, OrganisationCode);
                                if (Result.StatusCode == StatusCodes.Status409Conflict)
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = Result.Description });
                                return Ok(1);
                            }
                            else if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                            {
                                var Result = await this._lcmsRepository.SaveImage(uploadedFile, Lcms, UserId, OrganisationCode);
                                if (Result.StatusCode == StatusCodes.Status409Conflict)
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = Result.Description });
                                return Ok(1);
                            }
                            else if (uploadedFile.ContentType.Equals(FileType.h5p))
                            {                                
                                var Result = await this._lcmsRepository.SaveH5P(uploadedFile, lcmsApi, UserId, OrganisationCode);
                                if (Result.StatusCode == StatusCodes.Status400BadRequest)
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Result.Description });
                                return Ok(Result.Description);
                            }
                            else if ((uploadedFile.ContentType.Equals(FileType.AppZip) || uploadedFile.ContentType.Equals(FileType.AppXZipFile) || uploadedFile.ContentType.Equals(FileType.AppXZip)) && (FileValidation.IsValidLCMSZip(uploadedFile)))
                            {
                                var Result = await this._lcmsRepository.SaveZip(uploadedFile, lcmsApi, UserId, OrganisationCode);
                                if (Result.StatusCode == StatusCodes.Status400BadRequest)
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Result.Description });
                                return Ok(1);
                            }
                            else if ((uploadedFile.ContentType.Equals(FileType.h5p) && FileValidation.IsValidH5PZip(uploadedFile)))
                            {
                                var Result = await this._lcmsRepository.SaveZip(uploadedFile, lcmsApi, UserId, OrganisationCode);
                                if (Result.StatusCode == StatusCodes.Status400BadRequest)
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Result.Description });
                                return Ok(1);
                            }
                            else
                            {
                                foreach (string docType in FileType.Doc)
                                {
                                    if ((uploadedFile.ContentType.Contains(docType)) && (FileValidation.IsValidLCMSDocument(uploadedFile)))
                                    {
                                        var Result = await this._lcmsRepository.SavePdf(uploadedFile, Lcms, UserId, OrganisationCode);
                                        if (Result.StatusCode == StatusCodes.Status409Conflict)
                                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = Result.Description });
                                        return Ok(1);
                                    }
                                }
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                            }
                        }
                    }
                    else
                    {
                        if (lcmsApi.ContentType.ToLower().Equals("youtube"))
                        {

                            if (await _lcmsRepository.ExistYouTubeLink(lcmsApi.Name, lcmsApi.ContentType, lcmsApi.YoutubeVideoId))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateContent), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateContent) }); ;
                            }
                            else
                            {
                                Value = await this._lcmsRepository.AddYoutubeFile(lcmsApi, UserId);
                                if (Value == 1)
                                    return Ok(1);
                                else if (Value == 0)
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                            }
                        }
                    }
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });

                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Put(int id, [FromBody] LCMSAPI lcmsApi)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                LCMS lcmsEdit = await _lcmsRepository.Get(id);
                if (lcmsEdit == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                if (await _lcmsRepository.Exist(lcmsApi.Name, lcmsApi.ContentType, id))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateContent), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateContent) }); ;

                if (lcmsApi.ContentType.ToLower().Equals("youtube"))
                {

                    if (await _lcmsRepository.ExistYouTubeLink(lcmsApi.Name, lcmsApi.ContentType, lcmsApi.YoutubeVideoId, id))
                    {
                        return StatusCode(409, Json("Duplicate"));
                    }
                    else
                    {
                        lcmsEdit.Description = lcmsApi.Description;
                        lcmsEdit.Language = lcmsApi.Language;
                        lcmsEdit.MetaData = lcmsApi.MetaData;
                        lcmsEdit.Name = lcmsApi.Name;
                        lcmsEdit.Duration = lcmsApi.Duration == null ? 0 : float.Parse(lcmsApi.Duration.ToString());
                        lcmsEdit.IsBuiltInAssesment = lcmsApi.IsBuiltInAssesment == null ? false : Convert.ToBoolean(lcmsApi.IsBuiltInAssesment.ToString());
                        lcmsEdit.IsMobileCompatible = lcmsApi.IsMobileCompatible == null ? false : Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
                        lcmsEdit.ModifiedBy = UserId;
                        lcmsEdit.ModifiedDate = DateTime.UtcNow;
                        lcmsEdit.YoutubeVideoId = lcmsApi.YoutubeVideoId;
                        lcmsEdit.IsActive = lcmsApi.IsActive;
                        if (lcmsApi.ScormType != null)
                        {
                            lcmsEdit.ContentType = lcmsApi.ScormType;
                        }
                        await this._lcmsRepository.Update(lcmsEdit);
                        return Ok();
                    }
                }
                else
                {

                    lcmsEdit.Description = lcmsApi.Description;
                    lcmsEdit.Language = lcmsApi.Language;
                    lcmsEdit.MetaData = lcmsApi.MetaData;
                    lcmsEdit.Name = lcmsApi.Name;
                    lcmsEdit.Duration = lcmsApi.Duration == null ? 0 : float.Parse(lcmsApi.Duration.ToString());
                    lcmsEdit.IsBuiltInAssesment = lcmsApi.IsBuiltInAssesment == null ? false : Convert.ToBoolean(lcmsApi.IsBuiltInAssesment.ToString());
                    lcmsEdit.IsMobileCompatible = lcmsApi.IsMobileCompatible == null ? false : Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
                    lcmsEdit.ModifiedBy = UserId;
                    lcmsEdit.ModifiedDate = DateTime.UtcNow;
                    lcmsEdit.YoutubeVideoId = lcmsApi.YoutubeVideoId;
                    lcmsEdit.IsActive = lcmsApi.IsActive;
                    if (lcmsApi.ScormType != null)
                    {
                        lcmsEdit.ContentType = lcmsApi.ScormType;
                    }
                    await this._lcmsRepository.Update(lcmsEdit);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpPost("Feedback")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> FeedbackPost([FromBody] LCMSAPI lcmsApi)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _lcmsRepository.Exist(lcmsApi.Name, "feedback"))
                    {
                        return StatusCode(409, Json("Duplicate"));
                    }
                    await this._lcmsRepository.AddFeedback(lcmsApi, UserId);
                    return Ok();
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpPost("Feedback/{id}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> FeedbacktPut(int id, [FromBody] LCMSAPI lcmsApi)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var Lcms = await _lcmsRepository.Get(id);
                    if (Lcms != null)
                    {
                        Lcms.Id = id;
                        Lcms.MetaData = lcmsApi.MetaData;
                        Lcms.Name = lcmsApi.Name;
                        await _lcmsRepository.Update(Lcms);
                        return Ok(lcmsApi);
                    }
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpPost("Survey")]
        public async Task<IActionResult> SurveyPost([FromBody] LCMSAPI lcmsApi)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _lcmsRepository.Exist(lcmsApi.Name, "survey"))
                {
                    return StatusCode(409, "Duplicate");
                }
                await this._lcmsRepository.AddSurvey(lcmsApi, UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpPost("Survey/{id}")]
        public async Task<IActionResult> SurveyPut(int id, [FromBody] LCMSAPI lcmsApi)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var Lcms = await _lcmsRepository.Get(id);
                if (Lcms == null)
                {
                    return BadRequest(
                        new ResponseMessage
                        {
                            Message = EnumHelper.GetEnumName(MessageType.InvalidData),
                            Description = EnumHelper.GetEnumDescription(MessageType.InvalidData)
                        });
                }

                if (await _lcmsRepository.ExistByType(lcmsApi.Name, lcmsApi.ContentType, lcmsApi.Id))
                {
                    return BadRequest(
                        new ResponseMessage
                        {
                            Message = EnumHelper.GetEnumName(MessageType.Duplicate),
                            Description = EnumHelper.GetEnumDescription(MessageType.Duplicate)
                        });
                }
                Lcms.Id = id;
                Lcms.MetaData = lcmsApi.MetaData;
                Lcms.Name = lcmsApi.Name;
                await _lcmsRepository.Update(Lcms);
                return Ok(lcmsApi);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Message Result = await this._lcmsRepository.DeleteLcms(DecryptedId);
                if (Result == Message.Success)
                    return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(MessageType.Success) });
                if (Result == Message.NotFound)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (Result == Message.DependencyExist)
                    return StatusCode(409, "Dependency Exists");

                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
        }

        [HttpGet("GetMediaCount/{showAllData?}")]
        //[PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> GetMediaCount( bool showAllData = false)
        {
            try
            {

                return Ok(await _lcmsRepository.MediaCount(UserId, showAllData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpGet("GetMedia/{page:int}/{pageSize:int}/{search?}/{metaData?}")]
        public async Task<IActionResult> GetMedia(int page, int pageSize, string search = null, string metaData = null)
        {
            try
            {

                return Ok(await _lcmsRepository.Media(page, pageSize, search, metaData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpGet("GetSurveyMedia/{page:int}/{pageSize:int}/{search?}/{metaData?}")]
        public async Task<IActionResult> GetSurveyMedia(int page, int pageSize, string search = null, string metaData = null)
        {
            try
            {

                return Ok(await _lcmsRepository.SurveyMedia(page, pageSize, search, metaData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpGet("GetMedia/getTotalRecords/{search:minlength(0)?}/{metaData?}")]
        public async Task<IActionResult> GetMediaCount(string search = null, string metaData = null)
        {
            try
            {

                int count = await _lcmsRepository.GetMediaCount(search, metaData);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpGet("GetSurveyMedia/getTotalRecords/{search:minlength(0)?}/{metaData?}")]
        public async Task<IActionResult> GetSurveyMediaCount(string search = null, string metaData = null)
        {
            try
            {

                int count = await _lcmsRepository.GetSurveyMediaCount(search, metaData);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpPost("UploadThumbnail")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> UploadThumbnail()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile uploadedFile in request.Form.Files)
                    {
                        string CoursesPath = this._configuration["ApiGatewayWwwroot"];
                        CoursesPath = Path.Combine(CoursesPath, OrganisationCode, Record.Courses);
                        if (!Directory.Exists(CoursesPath))
                        {
                            Directory.CreateDirectory(CoursesPath);
                        }
                        string ThumbnailPath = Path.Combine(CoursesPath, Record.Thumbnail);
                        if (!Directory.Exists(ThumbnailPath))
                        {
                            Directory.CreateDirectory(ThumbnailPath);
                        }
                        string fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                        fileName = string.Concat(fileName.Split(' '));
                        string filePath = Path.Combine(ThumbnailPath, fileName);
                        filePath = string.Concat(filePath.Split(' '));
                        await this._lcmsRepository.SaveFile(uploadedFile, filePath);
                        var uri = new System.Uri(filePath);
                        filePath = uri.AbsoluteUri;
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        string FPath = new System.Uri(filePath).AbsoluteUri;
                        string ThumbPath = string.Concat(DomainName, FPath.Substring(FPath.LastIndexOf(OrganisationCode)));
                        return Ok(ThumbPath);
                    }
                }
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }

        }

        [HttpPost("ExternalLink")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> ExternalLink([FromBody] LCMSAPI lcmsApi)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                lcmsApi.ContentType = ContentType.ExternalLink;
                if (await _lcmsRepository.ExistByTypeExternalLink(lcmsApi.Name, ContentType.ExternalLink, lcmsApi.Path, lcmsApi.Id))
                {
                    return StatusCode(409, Json("Duplicate"));

                }
                LCMS lcms = new LCMS();
                lcms.ContentType = ContentType.ExternalLink;
                lcms.MetaData = lcmsApi.MetaData;
                lcms.Name = lcmsApi.Name;
                lcms.CreatedBy = UserId;
                lcms.ModifiedBy = UserId;
                lcms.CreatedDate = DateTime.UtcNow;
                lcms.ModifiedDate = DateTime.UtcNow;
                lcms.Path = lcmsApi.Path;
                lcms.Description = lcmsApi.Description;
                lcms.IsMobileCompatible = lcmsApi.IsMobileCompatible == null ? false : Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
                lcms.CreatedDate = DateTime.UtcNow;
                lcms.IsEmbed = lcmsApi.IsEmbed;
                lcms.IsExternalContent = true;
                lcms.SubContentType = "external-link";
                await this._lcmsRepository.Add(lcms);

                if (lcmsApi.Ismodulecreate)
                {
                    Module module = new Module();
                    module.IsActive = true;
                    module.LCMSId = lcms.Id;
                    module.Name = lcms.Name;
                    module.Description = lcms.Name;
                    module.CourseType = "elearning";
                    module.ModuleType = "externalLink";
                    module.CreatedDate = DateTime.UtcNow;
                    module.ModifiedDate = DateTime.UtcNow;
                    module.CreatedBy = UserId;
                    module.ModifiedBy = UserId;
                    module.IsMultilingual = false;
                    await _moduleRepository.Add(module);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("ExternalLink/{id}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> ExternalLinkUpdate(int id, [FromBody] LCMSAPI lcmsApi)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                LCMS lcms = await _lcmsRepository.Get(id);
                if (lcms == null || lcms.Id != id)
                    return NotFound();
                if (await _lcmsRepository.ExistByTypeExternalLink(lcmsApi.Name, lcmsApi.ContentType, lcmsApi.Path, lcmsApi.Id))
                {
                    return StatusCode(409, Json("Duplicate"));
                }
                lcms.ContentType = ContentType.ExternalLink;
                lcms.MetaData = lcmsApi.MetaData;
                lcms.Name = lcmsApi.Name;
                lcms.Description = lcmsApi.Description;
                lcms.CreatedBy = UserId;
                lcms.ModifiedBy = UserId;
                lcms.ModifiedDate = DateTime.UtcNow;
                lcms.Path = lcmsApi.Path;
                lcms.IsMobileCompatible = Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
                lcms.CreatedDate = DateTime.UtcNow;
                await this._lcmsRepository.Update(lcms);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("LcmsSurvey")]
        public async Task<IActionResult> LcmsSurveyPost([FromBody] LCMSAPI lcmsApi)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (await _lcmsRepository.ExistByType(lcmsApi.Name, lcmsApi.ContentType, lcmsApi.Id))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                LCMS lcms = new LCMS();
                lcms.ContentType = ContentType.Survey;
                lcms.MetaData = lcmsApi.MetaData;
                lcms.Name = lcmsApi.Name;
                lcms.IsNested = lcmsApi.IsNested;
                lcms.CreatedBy = UserId;
                lcms.ModifiedBy = UserId;
                lcms.CreatedDate = DateTime.UtcNow;
                await this._lcmsRepository.Add(lcms);
                return Ok(lcms.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("xAPILaunchData/{id}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> GetxAPILaunchData(int id)
        {
            try
            {
                var result = await _lcmsRepository.GetxAPILaunchData(id, OrganisationCode, UserId);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpPost("GetLCMSMedia")]
        public async Task<IActionResult> GetMediaV2([FromBody]ApiGetLCMSMedia apiGetLCMSMedia)
        {
            try
            {

                return Ok(await _lcmsRepository.MediaV2(apiGetLCMSMedia,UserId,RoleCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpGet("GetExternalLinkVendor")]
        public async Task<IActionResult> GetExternalLinkVendor()
        {
            try
            {

                return Ok(await _lcmsRepository.GetExternalLinkVendor());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }
        [HttpGet("GetVideoSubContent")]
        public async Task<IActionResult> GetVideoSubContent()
        {
            try
            {

                return Ok(await _lcmsRepository.GetVideoSubContent());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace + "Message" + ex.Message });
            }
        }

        [HttpGet("GetXtTokenForKPoint")]
        public IActionResult GetXtTokenForKPoint()
        {
            XtTokenKpoint xtTokenKpoint = new XtTokenKpoint();

            string xtToken = _lcmsRepository.KPointToken(UserId);

            if(xtToken != null)
            {
                xtTokenKpoint.XtToken = xtToken;
                return Ok(xtTokenKpoint);
            }
            else
            {
                return BadRequest();
            }
        }

    }
}