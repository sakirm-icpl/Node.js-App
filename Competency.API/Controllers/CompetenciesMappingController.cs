using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Competency.API.APIModel;
using Competency.API.APIModel.Competency;
using Competency.API.Common;
using Competency.API.Model.Competency;
using Competency.API.Repositories.Interfaces;
using Competency.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Competency.API.Common.AuthorizePermissions;
using static Competency.API.Common.TokenPermissions;
using Competency.API.Helper;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using OfficeOpenXml;
using Competency.API.Helper.Metadata;
using Competency.API.Repositories.Interfaces.Competency;

namespace Competency.API.Controllers.Competency
{
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CompetenciesMappingController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesMappingController));
        IMyCoursesRepository _myCoursesRepository;
        private ICompetenciesMappingRepository competenciesMappingRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetenciesMappingController(IMyCoursesRepository myCoursesRepository, ICompetenciesMappingRepository competenciesMappingController, IIdentityService _identitySvc, ITokensRepository tokensRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(_identitySvc)
        {
            this._myCoursesRepository = myCoursesRepository;
            this.competenciesMappingRepository = competenciesMappingController;
            this._tokensRepository = tokensRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var competenciesMapping = await this.competenciesMappingRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APICompetenciesMapping>>(competenciesMapping));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this.competenciesMappingRepository.GetAllCompetenciesMapping(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }


        [HttpGet("GetByCourseId/{courseId:int}")]
        public async Task<IActionResult> GetByCourseId(int courseId)
        {
            try
            {
                var CompetencyLevels = await this.competenciesMappingRepository.GetAllCompetenciesMappingByCourse(courseId);
                return Ok(Mapper.Map<List<APICompetenciesMapping>>(CompetencyLevels));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("CompetencyWiseCourses/{comId:int?}")]
        public async Task<IActionResult> GetCompetencyWiseCourses(int? comId)
        {
            try
            {

                var CompetencyLevels = await this.competenciesMappingRepository.GetCompetencyWiseCourses(comId);
                return Ok(Mapper.Map<List<APICompetencyWiseCourses>>(CompetencyLevels));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this.competenciesMappingRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var competenciesMapping = await this.competenciesMappingRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<List<APICompetenciesMapping>>(competenciesMapping));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Post([FromBody] APICompetenciesMapping competenciesMappingRecord)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }
                if (await this.competenciesMappingRepository.Exists(competenciesMappingRecord.CourseId, competenciesMappingRecord.CompetencyId))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                CompetenciesMapping competenciesMapping = new CompetenciesMapping();
                competenciesMapping.CompetencyCategoryId = competenciesMappingRecord.CompetencyCategoryId == null ? 0 : Convert.ToInt32(competenciesMappingRecord.CompetencyCategoryId);
                competenciesMapping.CompetencySubCategoryId = competenciesMappingRecord.CompetencySubCategoryId == null ? 0 : Convert.ToInt32(competenciesMappingRecord.CompetencySubCategoryId);
                competenciesMapping.CompetencySubSubCategoryId = competenciesMappingRecord.CompetencySubSubCategoryId == null ? 0 : Convert.ToInt32(competenciesMappingRecord.CompetencySubSubCategoryId);
                competenciesMapping.CourseCategoryId = competenciesMappingRecord.CourseCategoryId;
                competenciesMapping.CourseId = competenciesMappingRecord.CourseId;
                competenciesMapping.ModuleId = competenciesMappingRecord.ModuleId;
                competenciesMapping.CompetencyId = competenciesMappingRecord.CompetencyId;
                competenciesMapping.IsDeleted = false;
                competenciesMapping.ModifiedBy = UserId;
                competenciesMapping.ModifiedDate = DateTime.UtcNow;
                competenciesMapping.CreatedBy = UserId;
                competenciesMapping.CreatedDate = DateTime.UtcNow;
                competenciesMapping.CompetencyLevelId = competenciesMappingRecord.CompetencyLevelId;
                await this.competenciesMappingRepository.AddRecord(competenciesMapping);
                return Ok(competenciesMappingRecord);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Put(int id, [FromBody] APICompetenciesMapping aPICompetenciesMappingMerge)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await this.competenciesMappingRepository.Exists(aPICompetenciesMappingMerge.CourseId, aPICompetenciesMappingMerge.CompetencyId, aPICompetenciesMappingMerge.Id))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                    else
                    {
                        CompetenciesMapping competenciesMapping = new CompetenciesMapping();
                        competenciesMapping.CompetencyCategoryId = aPICompetenciesMappingMerge.CompetencyCategoryId == null ? 0 : Convert.ToInt32(aPICompetenciesMappingMerge.CompetencyCategoryId);
                        competenciesMapping.CompetencySubCategoryId = aPICompetenciesMappingMerge.CompetencySubCategoryId == null ? 0 : Convert.ToInt32(aPICompetenciesMappingMerge.CompetencySubCategoryId);
                        competenciesMapping.CompetencySubSubCategoryId = aPICompetenciesMappingMerge.CompetencySubSubCategoryId == null ? 0 : Convert.ToInt32(aPICompetenciesMappingMerge.CompetencySubSubCategoryId);
                        competenciesMapping.CourseCategoryId = aPICompetenciesMappingMerge.CourseCategoryId;
                        competenciesMapping.CourseId = aPICompetenciesMappingMerge.CourseId;
                        competenciesMapping.ModuleId = aPICompetenciesMappingMerge.ModuleId;
                        competenciesMapping.CompetencyId = aPICompetenciesMappingMerge.CompetencyId;
                        competenciesMapping.IsDeleted = false;
                        competenciesMapping.ModifiedBy = 1;
                        competenciesMapping.ModifiedDate = DateTime.UtcNow;
                        competenciesMapping.CreatedBy = 1;
                        competenciesMapping.CreatedDate = DateTime.UtcNow;
                        competenciesMapping.Id = aPICompetenciesMappingMerge.Id;
                        competenciesMapping.CompetencyLevelId = aPICompetenciesMappingMerge.CompetencyLevelId;
                        await this.competenciesMappingRepository.Update(competenciesMapping);
                    }
                    return this.Ok(aPICompetenciesMappingMerge);
                }

                else
                    return BadRequest(ModelState);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CompetenciesMapping competenciesMapping = await this.competenciesMappingRepository.Get(DecryptedId);
                if (competenciesMapping == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                competenciesMapping.IsDeleted = true;
                await this.competenciesMappingRepository.Update(competenciesMapping);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAllCategoryWiseCompetencies/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetAllCategoryWiseCompetencies(int page, int pageSize, string search = null)
        {
            try
            {
                int? jobroleid = null;
                APICareerRoles CareerRoles = await this._myCoursesRepository.GetUserJobRoleByUserId(UserId);
                if (CareerRoles != null)
                    jobroleid = CareerRoles.JobRoleId;
                return Ok(await this.competenciesMappingRepository.GetAllCategoryWiseCompetencies(page, pageSize, jobroleid, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAllCategoryWiseCompetenciesCount/{search?}")]
        public async Task<IActionResult> GetAllCategoryWiseCompetenciesCount(string search = null)
        {
            try
            {
                int? jobroleid = null;
                APICareerRoles CareerRoles = await this._myCoursesRepository.GetUserJobRoleByUserId(UserId);
                if (CareerRoles != null)
                    jobroleid = CareerRoles.JobRoleId;
                return Ok(await this.competenciesMappingRepository.GetAllCategoryWiseCompetenciesCount(search, jobroleid));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetCompetanciesDetail/{competencyMappingId}/{courseId}/{moduleId?}")]
        public async Task<IActionResult> GetCompetanciesDetail(int competencyMappingId, int courseId, int? moduleId)
        {
            try
            {
                moduleId = moduleId == null ? 0 : moduleId;
                return Ok(await this.competenciesMappingRepository.GetCompetanciesDetail(competencyMappingId, courseId, moduleId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        #region Bulk Upload for Competencies Mapping

        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"CompetenciesMapping.xlsx";
                string DomainName = this._configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);

                if (!Directory.Exists(sWebRootFolder))
                {
                    Directory.CreateDirectory(sWebRootFolder);
                }

                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }

                using (ExcelPackage package = new ExcelPackage(file))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CompetenciesMapping");

                    worksheet.Cells[1, 1].Value = "Course*";

                    worksheet.Cells[1, 2].Value = "Category";

                    worksheet.Cells[1, 3].Value = "Competency*";

                    worksheet.Cells[1, 4].Value = "Competency Level";


                    package.Save();
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "CompetenciesMapping.xlsx");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostFileUpload")]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.XLSX : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }

                        if (FileValidation.IsValidXLSX(fileUpload))
                        {
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrganisationCode);
                            string DomainName = this._configuration["ApiGatewayUrl"];
                            fileDir = Path.Combine(fileDir, customerCode, FileType);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
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
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
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

        [HttpPost]
        [Route("SaveFileData")]
        public async Task<IActionResult> PostFile([FromBody] APIDataMigrationFilePath aPIDataMigration)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                ApiResponse response = await this.competenciesMappingRepository.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
                return Ok(response.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }

        #endregion
    }
}
