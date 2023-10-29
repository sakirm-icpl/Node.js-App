using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Competency.API.APIModel.Competency;
using Competency.API.Common;
using Competency.API.Model.Competency;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Competency.API.Common.TokenPermissions;
using Competency.API.Helper;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using Competency.API.Helper.Metadata;
using OfficeOpenXml;
using Competency.API.APIModel;

namespace Competency.API.Controllers.Competency
{
    [Route("api/v1/comp/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class RolewiseCoursesMappingController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RolewiseCoursesMappingController));
        private IRolewiseCoursesMapping _rolewiseCoursesMapping;
        private readonly ITokensRepository _tokensRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RolewiseCoursesMappingController(IRolewiseCoursesMapping rolewiseCoursesMapping, IIdentityService _identitySvc, ITokensRepository tokensRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(_identitySvc)
        {
            this._rolewiseCoursesMapping = rolewiseCoursesMapping;
            this._tokensRepository = tokensRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var competenciesMapping = await this._rolewiseCoursesMapping.GetAllCoursesMapping();
                return Ok(Mapper.Map<List<APIRolewiseCoursesMappingDetails>>(competenciesMapping));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}/{filter?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string filter = null)
        {
            try
            {
                var CompetencyLevels = await this._rolewiseCoursesMapping.GetAllRoleCoursesMapping(page, pageSize, search, filter);
                return Ok(Mapper.Map<List<APIRolewiseCoursesMappingDetails>>(CompetencyLevels));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetTotalRecords/{search:minlength(0)?}/{filter?}")]
        public async Task<IActionResult> GetCount(string search, string filter = null)
        {
            try
            {
                var Count = await this._rolewiseCoursesMapping.Count(search, filter);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APIRolewiseCoursesMapping aPIRolewiseCoursesMapping)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }
                else
                {

                    if (await this._rolewiseCoursesMapping.Exists(aPIRolewiseCoursesMapping.JobRoleId, aPIRolewiseCoursesMapping.CourseId))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                    else
                    {
                        RolewiseCourseMapping rolewiseCourseMapping = new RolewiseCourseMapping();
                        rolewiseCourseMapping.CourseId = aPIRolewiseCoursesMapping.CourseId;
                        rolewiseCourseMapping.JobRoleId = aPIRolewiseCoursesMapping.JobRoleId;
                        rolewiseCourseMapping.ApplicableFromDays = aPIRolewiseCoursesMapping.ApplicableFromDays;
                        rolewiseCourseMapping.IsDeleted = false;
                        rolewiseCourseMapping.IsActive = true;
                        rolewiseCourseMapping.ModifiedBy = UserId;
                        rolewiseCourseMapping.ModifiedDate = DateTime.UtcNow;
                        rolewiseCourseMapping.CreatedBy = UserId;
                        rolewiseCourseMapping.CreatedDate = DateTime.UtcNow;
                        await this._rolewiseCoursesMapping.AddRecord(rolewiseCourseMapping);
                    }


                    return Ok(aPIRolewiseCoursesMapping);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APIRolewiseCoursesMapping aPIRolewiseCoursesMapping)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await this._rolewiseCoursesMapping.Exists(aPIRolewiseCoursesMapping.JobRoleId, aPIRolewiseCoursesMapping.CourseId, aPIRolewiseCoursesMapping.Id))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                    else
                    {
                        RolewiseCourseMapping rolewiseCourseMapping = await _rolewiseCoursesMapping.Get(id);
                        rolewiseCourseMapping.JobRoleId = aPIRolewiseCoursesMapping.JobRoleId;
                        rolewiseCourseMapping.CourseId = aPIRolewiseCoursesMapping.CourseId;
                        rolewiseCourseMapping.ApplicableFromDays = aPIRolewiseCoursesMapping.ApplicableFromDays;
                        rolewiseCourseMapping.ModifiedBy = UserId;
                        rolewiseCourseMapping.ModifiedDate = DateTime.UtcNow;
                        await this._rolewiseCoursesMapping.UpdateRecord(rolewiseCourseMapping);
                        return this.Ok(rolewiseCourseMapping);
                    }
                }
                else
                {
                    return this.BadRequest(this.ModelState);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                RolewiseCourseMapping rolewiseCourseMapping = await this._rolewiseCoursesMapping.Get(DecryptedId);

                if (ModelState.IsValid && rolewiseCourseMapping != null)
                {
                    rolewiseCourseMapping.IsDeleted = true;
                    await this._rolewiseCoursesMapping.Update(rolewiseCourseMapping);
                }

                if (rolewiseCourseMapping == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #region Bulk Upload role wise courses


        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"RolewiseCoursesMapping.xlsx";
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

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("RolewiseCoursesMapping");

                    worksheet.Cells[1, 1].Value = "Role*";

                    worksheet.Cells[1, 2].Value = "Course*";

                    worksheet.Cells[1, 3].Value = "Assign From Days*";


                    package.Save();
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "RolewiseCoursesMapping.xlsx");
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

                ApiResponse response = await this._rolewiseCoursesMapping.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
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
