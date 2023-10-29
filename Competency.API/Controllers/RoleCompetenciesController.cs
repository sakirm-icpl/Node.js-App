using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using Competency.API.APIModel;
using Competency.API.Common;
using Competency.API.Model;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces;
using Competency.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Competency.API.Common.AuthorizePermissions;
using static Competency.API.Common.TokenPermissions;
using Competency.API.Helper;
using log4net;
using Microsoft.Extensions.Configuration;
using System.IO;
using Competency.API.Helper.Metadata;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;

namespace Competency.API.Controllers.Competency
{
    [Produces("application/json")]
    [Route("api/v1/comp/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class RoleCompetenciesController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RoleCompetenciesController));
        private readonly ITokensRepository _tokensRepository;
        private IRoleCompetenciesRepository _roleCompetencies;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleCompetenciesController(IIdentityService _identitySvc, CourseContext context, ITokensRepository tokensRepository, IRoleCompetenciesRepository roleCompetencies, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(_identitySvc)
        {
            this._tokensRepository = tokensRepository;
            this._roleCompetencies = roleCompetencies;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.RoleCompetency)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._roleCompetencies.GetRoleCompetency(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]

        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this._roleCompetencies.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("GetCompetencySkills/{search?}")]
        public async Task<IActionResult> Get(string search = null)
        {
            try
            {
                if (search != null)
                {
                    search = search.Trim();
                }
                else
                {
                    search = null;
                }
                return Ok(await this._roleCompetencies.GetRoleCompetencyForSearch(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.RoleCompetency)]
        public async Task<IActionResult> Post([FromBody] APIRoleCompetency aPIRoleCompetency)
        {
            try
            {
                if (ModelState.IsValid && aPIRoleCompetency != null)
                {
                    RoleCompetency objroleCompetency = await this._roleCompetencies.CheckForDuplicate(aPIRoleCompetency.JobRoleId,
                        Convert.ToInt32(aPIRoleCompetency.CompetencyLevelId), Convert.ToInt32(aPIRoleCompetency.CompetencyCategoryId), aPIRoleCompetency.CompetencyId, OrganisationCode);
                    if (objroleCompetency != null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    }
                    else
                    {
                        if (aPIRoleCompetency.CompetencyCategoryId == null)
                        {
                            RoleCompetency roleCompetency = new RoleCompetency();
                            roleCompetency.CompetencyCategoryId = aPIRoleCompetency.CompetencyCategoryId;
                            roleCompetency.CompetencyId = aPIRoleCompetency.CompetencyId;
                            roleCompetency.CompetencyLevelId = Convert.ToInt32(aPIRoleCompetency.CompetencyLevelId);
                            roleCompetency.IsDeleted = false;
                            roleCompetency.JobRoleId = aPIRoleCompetency.JobRoleId;
                            roleCompetency.ModifiedDate = DateTime.UtcNow;
                            roleCompetency.ModifiedBy = 1;
                            roleCompetency.CreatedBy = 1;
                            roleCompetency.CreatedDate = DateTime.UtcNow;
                            await _roleCompetencies.Add(roleCompetency);
                            return Ok(aPIRoleCompetency);
                        }
                        else
                        {
                            RoleCompetency roleCompetency = new RoleCompetency();
                            roleCompetency.CompetencyCategoryId = aPIRoleCompetency.CompetencyCategoryId;
                            roleCompetency.CompetencyId = aPIRoleCompetency.CompetencyId;
                            roleCompetency.CompetencyLevelId = Convert.ToInt32(aPIRoleCompetency.CompetencyLevelId);
                            roleCompetency.JobRoleId = aPIRoleCompetency.JobRoleId;
                            roleCompetency.ModifiedDate = DateTime.UtcNow;
                            roleCompetency.ModifiedBy = 1;
                            roleCompetency.CreatedBy = 1;
                            roleCompetency.CreatedDate = DateTime.UtcNow;
                            await _roleCompetencies.Add(roleCompetency);
                            return Ok(aPIRoleCompetency);
                        }

                    }
                }
                else
                { return this.BadRequest(this.ModelState); }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.RoleCompetency)]
        public async Task<IActionResult> Put(int id, [FromBody] APIRoleCompetency aPIRoleCompetency)
        {
            try
            {

                RoleCompetency rolecompetency = await this._roleCompetencies.Get(s => s.IsDeleted == false && s.Id == id);

                if (rolecompetency == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (ModelState.IsValid && rolecompetency != null)
                {

                    rolecompetency.CompetencyId = aPIRoleCompetency.CompetencyId;
                    rolecompetency.CompetencyCategoryId = aPIRoleCompetency.CompetencyCategoryId;
                    rolecompetency.JobRoleId = aPIRoleCompetency.JobRoleId;
                    if (rolecompetency.CompetencyLevelId == 0 || rolecompetency.CompetencyLevelId == null)
                    {
                        rolecompetency.CompetencyLevelId = Convert.ToInt32(null);
                    }
                    else
                    {
                        rolecompetency.CompetencyLevelId = Convert.ToInt32(aPIRoleCompetency.CompetencyLevelId);
                    }
                    await this._roleCompetencies.Update(rolecompetency);

                }

                return this.Ok(rolecompetency);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.RoleCompetency)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                RoleCompetency rolecompetency = await this._roleCompetencies.Get(DecryptedId);
                if (rolecompetency == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                rolecompetency.IsDeleted = true;
                await this._roleCompetencies.Update(rolecompetency);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #region Bulk Upload Role Competencies

        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"RoleCompetencies.xlsx";
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

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("RoleCompetencies");

                    worksheet.Cells[1, 1].Value = "Role*";

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
                return File(fileData, FileContentType.Excel, "RoleCompetencies.xlsx");
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

                ApiResponse response = await this._roleCompetencies.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
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