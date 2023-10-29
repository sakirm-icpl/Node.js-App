//======================================
// <copyright file="ModuleController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
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
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    [TokenRequired()]
    public class ModuleController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleController));
        IModuleRepository _moduleRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly ICourseModuleAssociationRepository _courseModuleAssociation;
        private IConfiguration _configuration;
        private readonly ITokensRepository _tokensRepository;
        public ModuleController(IModuleRepository moduleRepository,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostingEnvironment,
            IAttachmentRepository attachmentRepository,
            IIdentityService _identitySvc,
            ICourseModuleAssociationRepository courseModuleAssociation,
            IConfiguration configuration, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this._hostingEnvironment = hostingEnvironment;
            this._httpContextAccessor = httpContextAccessor;
            this._attachmentRepository = attachmentRepository;
            this._moduleRepository = moduleRepository;
            this._courseModuleAssociation = courseModuleAssociation;
            this._configuration = configuration;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> Get()
        {
            try
            {

                if (_moduleRepository.Count() == 0)
                {
                    return NotFound();
                }
                return Ok(await _moduleRepository.GetAll());
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
                if (await _moduleRepository.Get(id) == null)
                {
                    return NotFound();
                }
                return Ok(await _moduleRepository.Get(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (columnName != null)
                    columnName = columnName.ToLower().Equals("null") ? null : columnName;
                return Ok(await _moduleRepository.Get(page, pageSize, search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getTotalRecords/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> GetCount(string search = null, string columnName = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (columnName != null)
                    columnName = columnName.ToLower().Equals("null") ? null : columnName;

                int count = await _moduleRepository.count(search, columnName);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getByModuleType/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.modulemanage + " " + Permissions.coursemanage)]
        public async Task<IActionResult> GetModule(int page, int pageSize, string search = null, string columnName = null, string searchstring = null)
        {
            try
            {
                return Ok(await _moduleRepository.Get(page, pageSize, search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getByModuleTypeTotalRecords/{moduleType?}/{columnName?}/{search?}")]
        [PermissionRequired(Permissions.modulemanage + " " + Permissions.coursemanage)]
        public async Task<IActionResult> GetModuleTypeCount(string moduleType = null, string columnName = null, string search = null)
        {
            try
            {
                int count = await _moduleRepository.coursesmodule_count(moduleType, columnName, search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getforCourse/{page:int}/{pageSize:int}/{moduletype?}/{columnName?}/{search?}")]
        [PermissionRequired(Permissions.modulemanage + " " + Permissions.coursemanage)]
        public async Task<IActionResult> GetForCourse(int page, int pageSize, string moduletype = null, string columnName = null, string search = null)
        {
            try
            {
                List<Module> module = await _moduleRepository.GetForCourse(page, pageSize, moduletype, columnName, search);
                return Ok(Mapper.Map<List<APIModule>>(module));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getforAssessmentCourse/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.modulemanage + " " + Permissions.coursemanage)]
        public async Task<IActionResult> getforAssessmentCourse(int page, int pageSize, string search = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                List<APIModule> module = await _moduleRepository.GetForAssessmentCourse(page, pageSize, search);
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetModuleTypeAhead/{search}/{searchText}")]
        public async Task<IActionResult> GetByModuleType(string search, string searchText)
        {
            try
            {
                List<TypeAhead> module = await _moduleRepository.GetModelTypeAhead(search, searchText);
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetModuleTypeAheadList/{courseId}/{search?}")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> GetModuleTypeAhead(int courseId, string search = null)
        {
            try
            {
                List<TypeAhead> module = await this._courseModuleAssociation.GetModelTypeAhead(courseId, search);
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetModulesILTByCourse/{courseId}")]
        public async Task<IActionResult> GetByModuleTypeILT(int courseId)
        {
            try
            {
                List<ILTCourseTypeAhead> module = await _moduleRepository.GetModuleByCourse(courseId, UserId, OrganisationCode);
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> Post([FromBody] APIModuleInput objmodule)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                Module module = new Module();
                module = Mapper.Map<Module>(objmodule);
                if (await _moduleRepository.Exists(module.Name))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.Description = objmodule.Description;
                await _moduleRepository.Add(module);

                if (module.LCMSId != null && module.IsMultilingual == false)
                {
                    LCMS lcmsdata = await _moduleRepository.getLcmsById((int)module.LCMSId, objmodule.Metadata);
                }
                int ModuleId = module.Id;
                foreach (var obj in objmodule.MultilingualLCMSId)
                {
                    ModuleLcmsAssociation moduleLcms = new ModuleLcmsAssociation();
                    moduleLcms.CreatedDate = DateTime.UtcNow;
                    moduleLcms.ModifiedDate = DateTime.UtcNow;
                    moduleLcms.CreatedBy = UserId;
                    moduleLcms.ModuleId = ModuleId;
                    moduleLcms.LCMSId = Convert.ToInt32(obj);
                    await _moduleRepository.AddModuleLcmsData(moduleLcms);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AddModuleLCMSAssociation")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> PostModuleLcmsAss([FromBody] ModuleLcmsAssociation moduleLcms)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                moduleLcms.CreatedDate = DateTime.UtcNow;
                moduleLcms.ModifiedDate = DateTime.UtcNow;
                moduleLcms.CreatedBy = UserId;
                await _moduleRepository.AddModuleLcmsData(moduleLcms);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetModuleLCMSAssociation/{moduleId}")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> GetModuleLcmsAss(int Moduleid)
        {
            try
            {
                return Ok(await _moduleRepository.GetModuleLcmsData(Moduleid));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("{id}")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> Put(int id, [FromBody] APIModuleInput module)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                if (await _moduleRepository.Get(id) == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (await _moduleRepository.Exists(module.Name, module.Id))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                if (module.IsActive == false)
                {
                    if (await _moduleRepository.IsDependacyExist(id))
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                Module objmodule = await _moduleRepository.Get(id);
                objmodule.LCMSId=module.LCMSId;
                objmodule.Name=module.Name;
                objmodule.ModuleType=module.ModuleType;
                objmodule.IsMultilingual=module.IsMultilingual; 
                objmodule.IsActive=module.IsActive;
                objmodule.ModifiedBy = UserId;
                objmodule.Description =module.Description;
               objmodule.ModifiedDate = DateTime.UtcNow;

                await _moduleRepository.Update(objmodule);
                await _moduleRepository.UpdateModuleLcmsAssociation(module.MultilingualLCMSId, objmodule.Id);

                if (module.LCMSId != null && module.IsMultilingual==false)
                {
                    LCMS lcmsdata = await _moduleRepository.getLcmsById((int)objmodule.LCMSId, module.Metadata);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Module courses = await _moduleRepository.Get(DecryptedId);
                if (courses == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (await _moduleRepository.IsDependacyExist(DecryptedId))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                courses.IsDeleted = true;
                await _moduleRepository.Update(courses);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UploadThumbnail")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> UploadThumbnail()
        {

            string fileName = string.Empty;
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile uploadedFile in request.Form.Files)
                    {
                        string filePath = this._configuration["ApiGatewayWwwroot"];
                        filePath = Path.Combine(filePath, OrganisationCode);
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        filePath = Path.Combine(filePath, FileType.Thumbnail);
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                        fileName = string.Concat(fileName.Split(' '));
                        filePath = Path.Combine(filePath, fileName);
                        filePath = string.Concat(filePath.Split(' '));
                        Courses.API.Common.Helper helper = new Courses.API.Common.Helper();
                        await helper.SaveFile(uploadedFile, filePath);
                        var uri = new System.Uri(filePath);
                        filePath = uri.AbsoluteUri;
                        String ThumbPath = string.Concat(DomainName, filePath.Substring(filePath.LastIndexOf(OrganisationCode)));
                        return Ok(ThumbPath);
                    }
                }
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }


        [HttpGet("GetFeedbackConfigurationId/{courseId:int}/{feedbackId:int}/{moduleId:int}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> GetFeedbackConfigurationId(int courseId, int feedbackId, int? moduleId = null)
        {
            try
            {
                return Ok(await _moduleRepository.GetFeedbackConfigurationId(courseId, feedbackId, moduleId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UploadModule")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> UploadModule()
        {

            string coursesPath = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile uploadedFile in request.Form.Files)
                    {
                        if (uploadedFile.ContentType.Contains("video"))
                            return Ok(await SaveVideo(uploadedFile));
                        if (uploadedFile.ContentType.Contains("pdf"))
                            return Ok(await SavePdf(uploadedFile));
                        if (uploadedFile.ContentType.Contains("image"))
                            return Ok(await SaveImage(uploadedFile));
                        if (uploadedFile.ContentType.Equals("application/zip") || uploadedFile.ContentType.Equals("application/x-zip-compressed"))
                            return Ok(await UploadZip(uploadedFile));
                        return Ok();
                    }
                }
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.StackTrace });
            }
        }

        [HttpPost("ModuleSequence/{courseId}")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> ModuleSequence(int courseId, [FromBody] List<ApiCourseModuleSequence> apiCourseModule)
        {
            try
            {
                int Result = await _courseModuleAssociation.AdjustSequence(apiCourseModule, courseId);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        private async Task<Dictionary<string, string>> UploadZip(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string coursesPath = this._configuration["ApiGatewayLXPFiles"];
            string coursesPathForZip = this._configuration["ApiGatewayWwwroot"];
            coursesPath = Path.Combine(coursesPath, OrganisationCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            coursesPath = Path.Combine(coursesPath, Record.Courses, "zip");
            if (!Directory.Exists(coursesPath))
            {
                Directory.CreateDirectory(coursesPath);
            }
            fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
            fileName = string.Concat(fileName.Split(' '));
            filePath = Path.Combine(coursesPath, fileName);
            filePath = string.Concat(filePath.Split(' '));
            await SaveFile(uploadedFile, filePath);
            string extractPath = Path.Combine(coursesPathForZip, Path.GetFileNameWithoutExtension(fileName));
            ZipExtactor extactor = new ZipExtactor();
            extactor.UnzipFile(extractPath, filePath);
            string startPage = extactor.GetRelativePath(extractPath, "");
            Attachment attach = new Attachment();
            attach.OriginalFileName = uploadedFile.FileName;
            attach.InternalName = fileName;
            attach.AttachmentPath = filePath;
            attach.MimeType = uploadedFile.ContentType;
            attach.IsDeleted = false;
            attach.CreatedDate = DateTime.UtcNow;
            attach.ModifiedDate = DateTime.UtcNow;
            attach.CreatedBy = UserId;
            await this._attachmentRepository.Add(attach);
            var uri = new System.Uri(startPage);
            startPage = uri.AbsoluteUri;
            String coursePath = string.Concat(DomainName, startPage.Substring(startPage.LastIndexOf(OrganisationCode)));
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("coursePath", coursePath);
            response.Add("attachmentId", attach.Id.ToString());
            return response;
        }
        private async Task<Dictionary<string, string>> SaveVideo(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string coursesPath = this._configuration["ApiGatewayWwwroot"];
            coursesPath = Path.Combine(coursesPath, OrganisationCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            coursesPath = Path.Combine(coursesPath, Record.Courses);
            if (!Directory.Exists(coursesPath))
            {
                Directory.CreateDirectory(coursesPath);
            }
            fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
            fileName = string.Concat(fileName.Split(' '));
            filePath = Path.Combine(coursesPath, FileType.Video);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = Path.Combine(coursesPath, FileType.Video, fileName);
            filePath = string.Concat(filePath.Split(' '));
            await SaveFile(uploadedFile, filePath);
            Attachment attach = new Attachment();
            attach.OriginalFileName = uploadedFile.FileName;
            attach.InternalName = fileName;
            attach.AttachmentPath = filePath;
            attach.MimeType = uploadedFile.ContentType;
            attach.IsDeleted = false;
            attach.CreatedDate = DateTime.UtcNow;
            attach.ModifiedDate = DateTime.UtcNow;
            attach.CreatedBy = UserId;
            await this._attachmentRepository.Add(attach);
            var uri = new System.Uri(filePath);
            filePath = uri.AbsoluteUri;
            Dictionary<string, string> response = new Dictionary<string, string>();
            String coursePath = string.Concat(DomainName, filePath.Substring(filePath.LastIndexOf(OrganisationCode)));
            response.Add("coursePath", coursePath);
            response.Add("attachmentId", attach.Id.ToString());
            return response;
        }
        private async Task<Dictionary<string, string>> SavePdf(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string coursesPath = this._configuration["ApiGatewayWwwroot"];
            coursesPath = Path.Combine(coursesPath, OrganisationCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            coursesPath = Path.Combine(coursesPath, Record.Courses);
            if (!Directory.Exists(coursesPath))
            {
                Directory.CreateDirectory(coursesPath);
            }
            fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
            fileName = string.Concat(fileName.Split(' '));
            filePath = Path.Combine(coursesPath, FileType.Pdf);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = Path.Combine(coursesPath, FileType.Pdf, fileName);
            filePath = string.Concat(filePath.Split(' '));
            await SaveFile(uploadedFile, filePath);
            Attachment attach = new Attachment();
            attach.OriginalFileName = uploadedFile.FileName;
            attach.InternalName = fileName;
            attach.AttachmentPath = filePath;
            attach.MimeType = uploadedFile.ContentType;
            attach.IsDeleted = false;
            attach.CreatedDate = DateTime.UtcNow;
            attach.ModifiedDate = DateTime.UtcNow;
            attach.CreatedBy = UserId;
            await this._attachmentRepository.Add(attach);
            var uri = new System.Uri(filePath);
            filePath = uri.AbsoluteUri;
            Dictionary<string, string> response = new Dictionary<string, string>();
            String coursePath = string.Concat(DomainName, filePath.Substring(filePath.LastIndexOf(OrganisationCode)));
            response.Add("coursePath", coursePath);
            response.Add("attachmentId", attach.Id.ToString());
            return response;
        }
        private async Task<Dictionary<string, string>> SaveImage(IFormFile uploadedFile)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string coursesPath = this._configuration["ApiGatewayWwwroot"];
            coursesPath = Path.Combine(coursesPath, OrganisationCode);
            string DomainName = this._configuration["ApiGatewayUrl"];
            coursesPath = Path.Combine(coursesPath, Record.Courses);
            if (!Directory.Exists(coursesPath))
            {
                Directory.CreateDirectory(coursesPath);
            }
            fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
            fileName = string.Concat(fileName.Split(' '));
            filePath = Path.Combine(coursesPath, FileType.Image);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = Path.Combine(coursesPath, FileType.Image, fileName);
            filePath = string.Concat(filePath.Split(' '));
            await SaveFile(uploadedFile, filePath);
            Attachment attach = new Attachment();
            attach.OriginalFileName = uploadedFile.FileName;
            attach.InternalName = fileName;
            attach.AttachmentPath = filePath;
            attach.MimeType = uploadedFile.ContentType;
            attach.IsDeleted = false;
            attach.CreatedBy = UserId;
            attach.CreatedDate = DateTime.UtcNow;
            attach.ModifiedDate = DateTime.UtcNow;
            await this._attachmentRepository.Add(attach);
            var uri = new System.Uri(filePath);
            filePath = uri.AbsoluteUri;
            Dictionary<string, string> response = new Dictionary<string, string>();
            String coursePath = string.Concat(DomainName, filePath.Substring(filePath.LastIndexOf(OrganisationCode)));
            if (request.IsHttps)
                coursePath = string.Concat("https://", coursePath);
            else
                coursePath = string.Concat("http://", coursePath);
            response.Add("coursePath", coursePath);
            response.Add("attachmentId", attach.Id.ToString());
            return response;
        }

        private async Task<bool> SaveFile(IFormFile uploadedFile, string filePath)
        {
            try
            {
                using (var fs = new FileStream(Path.Combine(filePath), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fs);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }

        [HttpGet("GetModulesAllCourses/{courseId}")]
        public async Task<IActionResult> GetByModuleTypeeAll(int courseId)
        {
            try
            {
                List<TypeAhead> module = await _moduleRepository.GetModuleByCourses(courseId);
                return Ok(Mapper.Map<List<TypeAhead>>(module));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("GetModulesForAssessmentCourses/{courseId}")]
        public async Task<IActionResult> GetModulesForAssessmentCourses(int courseId)
        {
            try
            {
                List<TypeAhead> module = await _moduleRepository.GetModulesForAssessmentCourses(courseId);
                return Ok(Mapper.Map<List<TypeAhead>>(module));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetTopicAllModules/{ModuleId}")]
        public async Task<IActionResult> GetByTopicTypeeAll(int ModuleId)
        {
            try
            {
                List<TypeAhead> Topic = await _moduleRepository.GetTopicByModules(ModuleId);
                return Ok(Mapper.Map<List<TypeAhead>>(Topic));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("GetModuleData")]
        [PermissionRequired(Permissions.modulemanage)]
        public async Task<IActionResult> GetV2([FromBody] ApiCourseModule apiCourseModule)
        {
            try
            {
                if (apiCourseModule.search != null)
                    apiCourseModule.search = apiCourseModule.search.ToLower().Equals("null") ? null : apiCourseModule.search;
                if (apiCourseModule.searchString != null)
                    apiCourseModule.searchString = apiCourseModule.searchString.ToLower().Equals("null") ? null : apiCourseModule.searchString;
                if (apiCourseModule.columnName != null)
                    apiCourseModule.columnName = apiCourseModule.columnName.ToLower().Equals("null") ? null : apiCourseModule.columnName;
                return Ok(await _moduleRepository.GetV2(apiCourseModule, UserId, RoleCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}