using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.TokenPermissions;
using Courses.API.Helper;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;
using System.Collections.Generic;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/AuthoringMaster")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class AuthoringMasterController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthoringMasterController));
        IAuthoringMaster _authoringMaster;
        IAzureStorage _azurestorage;
        ICourseRepository _courseRepository;
        private readonly ITokensRepository _tokensRepository; 
        private readonly IHttpContextAccessor _httpContextAccessor; 
        public IConfiguration _configuration { get; }
        public AuthoringMasterController(IIdentityService identitySvc, 
            IAuthoringMaster authoringMaster,
            IAzureStorage azurestorage,
            ITokensRepository tokensRepository,
            ICourseRepository courseRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config) : base(identitySvc)
        {
            this._azurestorage = azurestorage;
            this._courseRepository = courseRepository;
            this._authoringMaster = authoringMaster;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = config;
            this._tokensRepository = tokensRepository;
        }


        [HttpPost]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Post([FromBody] ApiAuthoringMaster apiAuthoringMaster)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else if (await this._authoringMaster.NameExists(apiAuthoringMaster.Name))
                {
                    return this.StatusCode(409, "Duplicate Entry! Name Already Exists");
                }
                //else if (await this._authoringMaster.SkillExists(apiAuthoringMaster.Skills))
                //{
                //    return this.StatusCode(409, "Duplicate Entry! Skill Already Exists");
                //}
                else
                {
                    ApiResponse reso = await this._authoringMaster.PostAuthoringDetailsToLcms(apiAuthoringMaster, UserId);
                    if (reso.StatusCode == 200)
                    {
                        AuthoringMaster objAuthoringMaster = new AuthoringMaster();
                        objAuthoringMaster = Mapper.Map<AuthoringMaster>(apiAuthoringMaster);
                        objAuthoringMaster.CreatedBy = UserId;
                        objAuthoringMaster.CreatedDate = DateTime.UtcNow;
                        objAuthoringMaster.IsDeleted = false;
                        objAuthoringMaster.IsActive = true;
                        objAuthoringMaster.LCMSId = Convert.ToInt32(reso.ResponseObject);

                        await _authoringMaster.Add(objAuthoringMaster);

                        int id = objAuthoringMaster.Id;
                        return Ok(id);
                    }

                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("AuthoringMasterDetails")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Post([FromBody] ApiAuthoringMasterDetails apiAuthoringMasterDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ApiAuthoringMasterDetails errorAssessment = await this._authoringMaster.PostAuthoringDetails(apiAuthoringMasterDetails, UserId);
                if (errorAssessment == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                else
                    return Ok(errorAssessment);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Put(int id, [FromBody] ApiAuthoringMaster apiAuthoringMaster)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                AuthoringMaster objAuthoringMaster = await this._authoringMaster.Get(id);
                if (objAuthoringMaster == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else if (await this._authoringMaster.NameExists(apiAuthoringMaster.Name, objAuthoringMaster.Id))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                else
                {
                    objAuthoringMaster.IsDeleted = apiAuthoringMaster.IsDeleted;
                    objAuthoringMaster.Name = apiAuthoringMaster.Name;
                    objAuthoringMaster.Skills = apiAuthoringMaster.Skills;
                    objAuthoringMaster.Description = apiAuthoringMaster.Description;
                    objAuthoringMaster.ModuleType = apiAuthoringMaster.ModuleType;
                    objAuthoringMaster.MetaData = apiAuthoringMaster.MetaData;
                    objAuthoringMaster.ModifiedBy = UserId;
                    objAuthoringMaster.ModifiedDate = DateTime.UtcNow;
                    await _authoringMaster.Update(objAuthoringMaster);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> GetAll(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await _authoringMaster.GetAll(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetDetailsByAuthoringId/{id}/{page:int}/{pageSize:int}/{courseId:int?}/{search?}")]
        public async Task<IActionResult> GetDetailsByAuthoringId(int id, int page, int pageSize, int courseId = 0, string search = null)
        {
            try
            {
                return Ok(await _authoringMaster.GetDetailsByAuthoringId(id, page, pageSize, UserId, courseId, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("AuthoringMasterDetails/AuthoringInteractiveVideoPopupsHistory")]
        public async Task<IActionResult> Post([FromBody] ApiAuthoringInteractiveVideoPopupsHistory apiAuthoringInteractiveVideoPopupsHistory)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await this._authoringMaster.PostAuthoringInteractiveVideoPopupsHistory(apiAuthoringInteractiveVideoPopupsHistory, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetPageNumber/{id}")]
        public async Task<IActionResult> GetPageNumber(int id)
        {
            try
            {
                return Ok(await _authoringMaster.GetPageNumber(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAuthoringMasterIdByModuleID/{ModuleId}")]
        public async Task<IActionResult> GetAuthoringMasterIdByModuleID(int ModuleId)
        {
            try
            {
                return Ok(await _authoringMaster.GetAuthoringMasterIdByModuleID(ModuleId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("count/{search?}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> GetCount(string search = null)
        {
            try
            {
                return Ok(await _authoringMaster.Count(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                AuthoringMaster AuthoringMaster = await _authoringMaster.Get(DecryptedId);
                if (await _authoringMaster.IsDependacyExist(DecryptedId))
                {
                    return StatusCode(503, new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                else if (AuthoringMaster == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                AuthoringMaster.IsDeleted = true;
                AuthoringMaster.ModifiedBy = this.UserId;
                AuthoringMaster.ModifiedDate = DateTime.UtcNow;
                await _authoringMaster.Update(AuthoringMaster);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("AuthoringMasterDetails")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> DeleteAuthoringMasterDetails([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                AuthoringMasterDetails objAuthoringMasterDetails = await _authoringMaster.GetAuthoringMasterDetails(DecryptedId);


                if (objAuthoringMasterDetails != null)
                {
                    return Ok(await _authoringMaster.DeleteAuthoringMasterDetails(objAuthoringMasterDetails));
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost("AuthoringDetails/{AuthoringDetailsId}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> PutAuthoringDetailsId(int AuthoringDetailsId, [FromBody] ApiAuthoringMasterDetailsUpdate apiAuthoringMasterDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                AuthoringMasterDetails objAuthoringMasterDetailsOld = await _authoringMaster.GetAuthoringMasterDetails(AuthoringDetailsId);

                if (objAuthoringMasterDetailsOld != null)
                {
                    return Ok(await _authoringMaster.UpdateAuthoringMasterDetails(objAuthoringMasterDetailsOld, apiAuthoringMasterDetails, UserId));
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [Route("AuthoringMasterDetails/DeleteAuthoringInteractiveVideoPopups")]
        public async Task<IActionResult> DeleteAuthoringInteractiveVideoPopups([FromQuery] string id)
        {
            try
            {
                return Ok(await _authoringMaster.DeleteAuthoringInteractiveVideoPopups(Convert.ToInt32(Security.Decrypt(id)), UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("AuthoringDetails/UpdatePageSequence/{AuthoringMasterId}")]
        public async Task<IActionResult> BulkPermissionUpdate(int AuthoringMasterId, [FromBody] List<ApiAuthoringMasterDetails> authoringMasterDetailsList)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                List<AuthoringMasterDetails> updatedData = await this._authoringMaster.UpdatePageSequence(AuthoringMasterId, authoringMasterDetailsList, UserId);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("Microlearning/PostFileUpload")]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string code = "microlearning";
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        if (fileUpload.ContentType.Contains(FileType.Image))
                        {
                            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                            string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? FileType.png : request.Form[Record.FileType].ToString();
                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                string fileDir = this._configuration["ApiGatewayWwwroot"];
                                fileDir = Path.Combine(fileDir, OrganisationCode, code, fileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + fileType);
                                using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                                {
                                    await fileUpload.CopyToAsync(fs);
                                }
                                if (String.IsNullOrEmpty(file))
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                }
                                return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));
                            }
                            else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrganisationCode, fileType, code);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            string file = res.Blob.Name.ToString();
                                            file = "/" + file;
                                            //string[] name = res.Blob.Name.Split('\\');
                                            //fileName = name[name.Length - 1];
                                            return Ok(file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }
                        else if (fileUpload.ContentType.Contains(FileType.Video))
                        {
                            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                            string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? FileType.mp4 : request.Form[Record.FileType].ToString();
                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {  
                                string fileDir = this._configuration["ApiGatewayWwwroot"];
                                fileDir = Path.Combine(fileDir, OrganisationCode, code, fileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + fileType);
                                using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                                {
                                    await fileUpload.CopyToAsync(fs);
                                }
                                if (String.IsNullOrEmpty(file))
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                }
                                return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));
                            }
                            else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrganisationCode, fileType, code);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            string file = res.Blob.Name.ToString();
                                            file = "/" + file;
                                            //string[] name = res.Blob.Name.Split('\\');
                                            //fileName = name[name.Length - 1];
                                            return Ok(file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }
                        else if (fileUpload.ContentType.Contains(FileType.Audio))
                        {
                            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                            string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? FileType.mp3 : request.Form[Record.FileType].ToString();
                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                string fileDir = this._configuration["ApiGatewayWwwroot"];
                                fileDir = Path.Combine(fileDir, OrganisationCode, code, fileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + fileType);
                                using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                                {
                                    await fileUpload.CopyToAsync(fs);
                                }
                                if (String.IsNullOrEmpty(file))
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                }
                                return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));
                            }
                            else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrganisationCode, fileType, code);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            string file = res.Blob.Name.ToString();
                                            file = "/" + file;
                                            //string[] name = res.Blob.Name.Split('\\');
                                            //fileName = name[name.Length - 1];
                                            return Ok(file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                        }
                        else
                        {   
                            foreach (string docType in FileType.Doc)
                            {
                                var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                                if (fileUpload.ContentType.Contains(docType))
                                {
                                    string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? FileType.Document : request.Form[Record.FileType].ToString();
                                    if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                                    {
                                        
                                        string fileDir = this._configuration["ApiGatewayWwwroot"];
                                        fileDir = Path.Combine(fileDir, OrganisationCode, code, fileType);
                                        if (!Directory.Exists(fileDir))
                                        {
                                            Directory.CreateDirectory(fileDir);
                                        }
                                        string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
                                        using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                                        {
                                            await fileUpload.CopyToAsync(fs);
                                        }
                                        if (String.IsNullOrEmpty(file))
                                        {
                                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                        }
                                        return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));

                                    }
                                    else
                                    {
                                        try
                                        {
                                            BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrganisationCode, fileType,code );
                                            if (res != null)
                                            {
                                                if (res.Error == false)
                                                {
                                                    string file = res.Blob.Name.ToString();
                                                    file = "/" + file;
                                                    //string[] name = res.Blob.Name.Split('\\');
                                                    //fileName = name[name.Length - 1];
                                                    return Ok(file.Replace(@"\", "/")) ;
                                                }
                                                else
                                                {
                                                    _logger.Error(res.ToString());
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpPost("PostConvertPPT")]
        public async Task<IActionResult> ConvertPPT([FromBody] AuthoringMasterDetailsPPT postData)
        {
            string targetFileExtension = "jpg";
            postData.PageNumber = 1;
            Application application = new Application();
            Presentation pptPresentation = null;

            BlobDto blobRes = new BlobDto();
            string newFileFullPath = string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(postData.Path);

            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                postData.Path = this._configuration["ApiGatewayWwwroot"] + postData.Path;
            }
            else
            {
                blobRes = await _azurestorage.DownloadAsync(postData.Path.Replace("\\\\", "\\"));
                if (blobRes == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumName(MessageType.InternalServerError) });
                }
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(postData.Path) && !string.IsNullOrWhiteSpace(postData.Path))
                {
                    string outputFolder = this._configuration["ApiGatewayWwwroot"] + "\\" + OrgCode + "\\microlearning\\jpg";

                    if (!Directory.Exists(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                    }
                    newFileFullPath = Path.Join(outputFolder, fileName);

                    _logger.Debug(newFileFullPath);

                    if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                    {
                        if (System.IO.File.Exists(postData.Path))
                        {
                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                pptPresentation = application.Presentations.Open(postData.Path, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
                                if (pptPresentation != null && pptPresentation.Slides.Count > 0)
                                {
                                    switch (targetFileExtension)
                                    {
                                        case "pdf":
                                            return await this.ExportPptToPdf(pptPresentation, newFileFullPath, targetFileExtension);
                                        case "jpg":
                                        case "png":
                                            return await this.ExportPptToImage(pptPresentation.Slides, newFileFullPath, fileName, targetFileExtension, postData);
                                        default:
                                            throw new Exception("Can't convert Ppt file to this extension.");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {


                        pptPresentation = application.Presentations.Open(blobRes.Uri, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
                        if (pptPresentation != null && pptPresentation.Slides.Count > 0)
                        {

                            switch (targetFileExtension)
                            {
                                case "pdf":
                                    return await ExportPptToPdf(pptPresentation, newFileFullPath, targetFileExtension);
                                case "jpg":
                                case "png":
                                    return await ExportPptToImageBlob(pptPresentation.Slides, newFileFullPath, fileName, targetFileExtension, postData);
                                default:
                                    throw new Exception("Can't convert Ppt file to this extension.");
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = Utilities.GetDetailedException(ex) });
            }
            finally
            {
                if (pptPresentation != null)
                {
                    pptPresentation.Close();
                    Utilities.ReleaseObject(pptPresentation);
                }

                if (application != null)
                {
                    application.Quit();
                    Utilities.ReleaseObject(application);
                }
            }
            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });

        }



        private async Task<IActionResult> ExportPptToImage(Slides slides, string outputPath, string fileName, string targetFileExtension, AuthoringMasterDetailsPPT postData)
        {
            int count = 1;
            foreach (Slide slide in slides)
            {
                slide.Export(outputPath + count + "." + targetFileExtension, targetFileExtension);

                ApiAuthoringMasterDetails Data = new ApiAuthoringMasterDetails();

                Data.AuthoringMasterId = postData.AuthoringMasterId;
                Data.Title = postData.Title + count;
                Data.PageNumber = postData.PageNumber + (count - 1);
                Data.Content = this._configuration["ApiGatewayUrl"]+ "/org-content/" + OrgCode +"/microlearning/jpg/" + fileName + count + "." + targetFileExtension;
                if (postData.IsDesktopLogin == 1)
                {
                    Data.Content = " <img class=\"w-100\" style=\"width:100%\" src=\"" + Data.Content + "\">";

                }
                else
                {
                    Data.Content = " <div class = \"mobview\" >  <div class=\"imgDiv\" style=\"margin: 3px 0;\">  <img class=\"w-100\" style=\"width:100%\" src=\"" + Data.Content + "\"> </div> </div>";

                }
                Data.PageType = postData.PageType;

                count++;

                await Post(Data);
            }

            return Ok();
        }

        private async Task<IActionResult> ExportPptToPdf(Presentation pptPresentation, string outputPath, string targetFileExtension)
        {
            //Can set the properties based on your need.
            pptPresentation.ExportAsFixedFormat(string.Format("{0}.{1}", outputPath, targetFileExtension),
                PpFixedFormatType.ppFixedFormatTypePDF,
                PpFixedFormatIntent.ppFixedFormatIntentPrint,
                MsoTriState.msoFalse,
                PpPrintHandoutOrder.ppPrintHandoutVerticalFirst,
                PpPrintOutputType.ppPrintOutputSlides,
                MsoTriState.msoFalse, null,
                PpPrintRangeType.ppPrintAll, string.Empty,
                true, true, true, true, false, Type.Missing);

            ApiAuthoringMasterDetails Data = new ApiAuthoringMasterDetails();
            await Post(Data);

            return Ok();
        }

        private async Task<IActionResult> ExportPptToImageBlob(Slides slides, string outputPath, string fileName, string targetFileExtension, AuthoringMasterDetailsPPT postData)
        {
            int count = 1;
            foreach (Slide slide in slides)
            {
                slide.Export(outputPath + count + "." + targetFileExtension, targetFileExtension);

                if (System.IO.File.Exists(outputPath + count + "." + targetFileExtension))
                {

                    byte[] imgdata = System.IO.File.ReadAllBytes(outputPath + count + "." + targetFileExtension);

                    string filePath = null;
                    BlobResponseDto res = await _azurestorage.CreateBlob(imgdata, OrgCode, "microlearning", "jpg", "jpg");
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            //filePath = res.Blob.Name.Replace(@"\", "/").ToString();
                            filePath = res.Blob.Uri.ToString();
                            System.IO.File.Delete(outputPath + count + "." + targetFileExtension);
                        }
                        else
                        {
                            _logger.Error(res.ToString());

                        }
                    }

                    ApiAuthoringMasterDetails Data = new ApiAuthoringMasterDetails();

                    Data.AuthoringMasterId = postData.AuthoringMasterId;
                    Data.Title = postData.Title + count;
                    Data.PageNumber = postData.PageNumber + (count - 1);
                    // Data.Content = this._configuration["ApiGatewayUrl"] + "/org-content/" + filePath;
                    Data.Content = filePath;
                    if (postData.IsDesktopLogin == 1)
                    {
                        Data.Content = " <img class=\"w-100\" style=\"width:100%\" src=\"" + Data.Content + "\">";

                    }
                    else
                    {
                        Data.Content = " <div class = \"mobview\" >  <div class=\"imgDiv\" style=\"margin: 3px 0;\">  <img class=\"w-100\" style=\"width:100%\" src=\"" + Data.Content + "\"> </div> </div>";

                    }
                    Data.PageType = postData.PageType;

                    count++;

                    await Post(Data);
                }
            }

            return Ok();
        }

    }
}