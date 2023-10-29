using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Competency.API.APIModel;
using Competency.API.Common;
using Competency.API.Model;
using Competency.API.Model.Competency;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
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
using AutoMapper.Configuration;
using System.IO;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;

namespace Competency.API.Controllers.Competency
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class JobRoleController : IdentityController
    {
        ICourseRepository _courseRepository;
        IAzureStorage _azurestorage;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobRoleController));
        private IJobRoleRepository jobrolerepository;
        private readonly ITokensRepository _tokensRepository;
        private IJdUploadRepository jdUploadRepository;
        public IConfiguration _configuration { get; }
        private IRoleCompetenciesRepository _roleCompetencies;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ICompetenciesMasterRepository _competenciesMasterRepository;
        public JobRoleController(ICourseRepository courseRepository, IAzureStorage azurestorage, IJobRoleRepository _jobrolerepository, IJdUploadRepository _jdUploadRepository, IIdentityService _identitySvc, ITokensRepository tokensRepository,
                   IRoleCompetenciesRepository roleCompetencies, IConfiguration confugu, IHttpContextAccessor httpContextAccessor, ICompetenciesMasterRepository competenciesMasterRepository) : base(_identitySvc)
        {
            _courseRepository = courseRepository;
            this._azurestorage = azurestorage;
            jobrolerepository = _jobrolerepository;
            jdUploadRepository = _jdUploadRepository;
            this._configuration = confugu;
            this._tokensRepository = tokensRepository;
            this._httpContextAccessor = httpContextAccessor;
            this._roleCompetencies = roleCompetencies;
            this._competenciesMasterRepository = competenciesMasterRepository;
        }


        [HttpPost]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.categorymanage)]
        public async Task<IActionResult> Post([FromBody] APICompetencyJobRole competencyjobrole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!await this.jobrolerepository.ExistsForJobRole(competencyjobrole.Name))
                    {
                        CompetencyJobRole competencyJobRole = Mapper.Map<CompetencyJobRole>(competencyjobrole);

                        competencyJobRole.IsDeleted = false;
                        competencyJobRole.IsActive = true;
                        competencyJobRole.CreatedBy = UserId;
                        competencyJobRole.CreatedDate = DateTime.UtcNow;
                        competencyJobRole.NumberOfPositions = competencyjobrole.NumberOfPositions;
                        competencyJobRole.CourseId = competencyjobrole.CourseId;
                        await jobrolerepository.Add(competencyJobRole);
                        if (competencyjobrole.FilePath != null && competencyjobrole.FileType != null)
                        {
                            CompetencyJdUpload competencyJdUpload = new CompetencyJdUpload();
                            competencyJdUpload.CompetencyJobRoleId = competencyJobRole.Id;
                            competencyJdUpload.FilePath = competencyjobrole.FilePath;
                            competencyJdUpload.FileType = competencyjobrole.FileType;
                            competencyJdUpload.FileVersion = 1;
                            competencyJdUpload.CreatedDate = DateTime.Now;
                            await jdUploadRepository.Add(competencyJdUpload);
                        }

                        int? Id = competencyJobRole.Id;

                        foreach (var aPIRoleCompetency in competencyjobrole.CompetencySkillsData)
                        {

                            int competenciesMasterid = 0;
                            if (aPIRoleCompetency.Id != 0)
                            {
                                competenciesMasterid = Convert.ToInt32(aPIRoleCompetency.Id);
                            }

                            if (competenciesMasterid == 0)
                            {
                                CompetenciesMaster competenciesMaster = new CompetenciesMaster();
                                competenciesMaster.CompetencyName = aPIRoleCompetency.Name;
                                competenciesMaster.CompetencyDescription = aPIRoleCompetency.Name;
                                competenciesMaster.CategoryId = 0;
                                competenciesMaster.CreatedBy = UserId;
                                competenciesMaster.CreatedDate = DateTime.Now;
                                competenciesMaster.IsActive = true;
                                competenciesMaster.IsDeleted = false;
                                competenciesMaster.ModifiedBy = UserId;
                                competenciesMaster.ModifiedDate = DateTime.Now;
                                await this._competenciesMasterRepository.Add(competenciesMaster);
                                competenciesMasterid = competenciesMaster.Id;
                            }

                            RoleCompetency roleCompetency = new RoleCompetency();
                            roleCompetency.JobRoleId = Convert.ToInt32(Id);
                            roleCompetency.CompetencyId = competenciesMasterid;  
                            roleCompetency.CompetencyCategoryId = 0;
                            roleCompetency.CompetencyLevelId = 0;
                            roleCompetency.IsDeleted = false;
                            roleCompetency.IsActive = true;
                            roleCompetency.CreatedBy = UserId;
                            roleCompetency.CreatedDate = DateTime.UtcNow;

                            if (await this.jobrolerepository.Exists(roleCompetency.JobRoleId, Convert.ToInt32(roleCompetency.CompetencyId)))
                            {
                                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                            }
                            else
                            {
                                await this._roleCompetencies.Add(roleCompetency);
                            }
                        }

                        int[] _NextJobRoles = competencyjobrole.NextJobRoles;


                        foreach (var apinextJobRoles in _NextJobRoles)
                        {
                            NextJobRoles nextJobRoles = new NextJobRoles();
                            nextJobRoles.JobRoleId = Convert.ToInt32(Id);
                            nextJobRoles.NextJobRoleId = apinextJobRoles;
                            nextJobRoles.UserId = UserId;
                            nextJobRoles.CreatedBy = UserId;
                            nextJobRoles.CreatedDate = DateTime.UtcNow;
                            nextJobRoles.IsActive = true;
                            nextJobRoles.IsDeleted = false;
                            nextJobRoles.ModifiedBy = UserId;
                            nextJobRoles.ModifiedDate = DateTime.UtcNow;

                            NextJobRoles errorAssessment = await this.jobrolerepository.PostNextJobRoleDetails(nextJobRoles);

                        }
                        return Ok(competencyjobrole);
                    }

                    else
                    {
                        return StatusCode(409, "JobRole Already Exists");
                    }
                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.jobrole)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.FileType : request.Form[Record.FileType].ToString();

                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }

                        if (FileValidation.IsValidPdf(fileUpload))
                        {
                            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            { 
                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrgCode);
                                fileDir = Path.Combine(fileDir, fileType);
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
                                return Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                            }
                            else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode, fileType);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                             string file = res.Blob.Name.ToString();

                                            return Ok("/" + file.Replace(@"\", "/"));
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

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.categorymanage)]
        public async Task<IActionResult> Put(int id, [FromBody] APICompetencyJobRole competencyjobrole)
        {
            try
            {

                CompetencyJobRole jobrole = await this.jobrolerepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (jobrole == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (ModelState.IsValid && jobrole != null)
                {
                    jobrole.Name = competencyjobrole.Name;
                    jobrole.Description = competencyjobrole.Description;

                    jobrole.RoleColumn1 = competencyjobrole.RoleColumn1;
                    jobrole.RoleColumn1value = competencyjobrole.RoleColumn1value;
                    jobrole.RoleColumn2 = competencyjobrole.RoleColumn2;
                    jobrole.RoleColumn2value = competencyjobrole.RoleColumn2value;
                    jobrole.NumberOfPositions = competencyjobrole.NumberOfPositions;
                    jobrole.CourseId = competencyjobrole.CourseId;
                    jobrole.ModifiedBy = UserId;
                    jobrole.ModifiedDate = DateTime.UtcNow;
                    await this.jobrolerepository.Update(jobrole);

                    CompetencyJdUpload jdUpload = await this.jdUploadRepository.GetCompetencyJdUpload(id);

                    if (competencyjobrole.FilePath != null && competencyjobrole.FileType != null)
                    {
                        CompetencyJdUpload NewJdUpload = new CompetencyJdUpload();

                        NewJdUpload.CompetencyJobRoleId = id;
                        NewJdUpload.FilePath = competencyjobrole.FilePath;
                        NewJdUpload.FileType = competencyjobrole.FileType;
                        NewJdUpload.FileVersion = jdUpload.FileVersion + 1;
                        NewJdUpload.CreatedDate = DateTime.Now;
                        await jdUploadRepository.Add(NewJdUpload);
                    }

                    List<int> competencyskillIdlist = new List<int>();

                    foreach (var aPIRoleCompetency in competencyjobrole.CompetencySkillsData)
                    {

                        int competenciesMasterid = 0;
                        if (aPIRoleCompetency.Id != 0)
                        {
                            competenciesMasterid = Convert.ToInt32(aPIRoleCompetency.Id);
                        }

                        if (competenciesMasterid == 0)
                        {
                            CompetenciesMaster competenciesMaster = new CompetenciesMaster();
                            competenciesMaster.CompetencyName = aPIRoleCompetency.Name;
                            competenciesMaster.CompetencyDescription = aPIRoleCompetency.Name;
                            competenciesMaster.CategoryId = 0;
                            competenciesMaster.CreatedBy = UserId;
                            competenciesMaster.CreatedDate = DateTime.Now;
                            competenciesMaster.IsActive = true;
                            competenciesMaster.IsDeleted = false;
                            competenciesMaster.ModifiedBy = UserId;
                            competenciesMaster.ModifiedDate = DateTime.Now;
                            await this._competenciesMasterRepository.Add(competenciesMaster);
                            competenciesMasterid = competenciesMaster.Id;
                        }

                        competencyskillIdlist.Add(Convert.ToInt32(competenciesMasterid));

                        if (!await this.jobrolerepository.Exists(Convert.ToInt32(jobrole.Id), Convert.ToInt32(competenciesMasterid)))
                        {
                            RoleCompetency roleCompetency = new RoleCompetency();
                            roleCompetency.JobRoleId = Convert.ToInt32(jobrole.Id);
                            roleCompetency.CompetencyId = competenciesMasterid;
                            roleCompetency.CompetencyCategoryId = 0;
                            roleCompetency.CompetencyLevelId = 0;
                            roleCompetency.IsDeleted = false;
                            roleCompetency.IsActive = true;
                            roleCompetency.CreatedBy = UserId;
                            roleCompetency.CreatedDate = DateTime.UtcNow;

                            await this._roleCompetencies.Add(roleCompetency);
                        }
                        else
                        {
                            continue;
                        }

                    }
                    int[] roleCompetencies = competencyskillIdlist.ToArray();
                    int[] aPIJobRoleForIds = await this._roleCompetencies.getIdByJobRoleId(Convert.ToInt32(competencyjobrole.Id));
                    this._competenciesMasterRepository.FindElementsNotInArray(roleCompetencies, aPIJobRoleForIds, Convert.ToInt32(jobrole.Id));


                    int[] _NextJobRoles = competencyjobrole.NextJobRoles;
                    await this.jobrolerepository.UpdateNextJobrole(_NextJobRoles, Convert.ToInt32(jobrole.Id), UserId);
                }
                return this.Ok(competencyjobrole);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }
 
        [HttpPost("GetAllJdUpload")]
        [PermissionRequired(Permissions.JdUpload)]
        public async Task<IActionResult> GetAllDeletedUser([FromBody] APIJdUpload apiJdUpload)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (apiJdUpload.Search != null)
                        apiJdUpload.Search = apiJdUpload.Search.ToLower().Equals("null") ? null : apiJdUpload.Search;
                    if (apiJdUpload.ColumnName != null)
                        apiJdUpload.ColumnName = apiJdUpload.ColumnName.ToLower().Equals("null") ? null : apiJdUpload.ColumnName;


                    IEnumerable<APICompetencyJdUpload> records = await this.jdUploadRepository.GetAllJdUpload(apiJdUpload.Page, apiJdUpload.PageSize, apiJdUpload.Search, apiJdUpload.ColumnName);
                    return this.Ok(records);
                }
                else
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest();
            }
        }
        [HttpGet("GetAllJdCount")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.iltnominate)]
        public async Task<IActionResult> GetDeletedUserCount()
        {
            try
            {
                var count = await this.jdUploadRepository.GetAllJdCount();
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.categorymanage)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                var competencyJobRole = await this.jobrolerepository.GetCompetencyJobRole(OrganisationCode, page, pageSize, search);
                return Ok(competencyJobRole);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetJobRoleDetailsById/{Id}")]
        public async Task<IActionResult> GetJobRoleDetailsById(int Id)
        {
            try
            {
                var competencyJobRole = await this.jobrolerepository.GetCompetencyJobRoleById(OrganisationCode, Id);
                return Ok(competencyJobRole);
            }
            catch (Exception ex)
            {

                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetJobRoleNames")]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.categorymanage)]
        public async Task<IActionResult> GetJobRoleNames()
        {
            try
            {
                var competencyJobRole = await this.jobrolerepository.GetAllJobRoles();

                return Ok(competencyJobRole);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetMasterTestCourseDetails/{JobRoleId?}")]
        public async Task<IActionResult> GetMasterTestCourseDetails(int? JobRoleId)
        {
            try
            {
                var mastercoursedetails = await this.jobrolerepository.GetMasterTestCourseDetails(UserId, JobRoleId);
                if (mastercoursedetails != null)
                {
                    return Ok(mastercoursedetails);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.categorymanage)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this.jobrolerepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.JobRole + " " + Permissions.categorymanage)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));

                CompetencyJobRole CompetencyJobRole = await jobrolerepository.Get(DecryptedId);
                if (await jobrolerepository.IsDependacyExist(DecryptedId))
                {
                    return StatusCode(503, "Dependancy exist!");
                }
                else if (CompetencyJobRole == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                CompetencyJobRole.IsDeleted = true;
                CompetencyJobRole.ModifiedBy = this.UserId;
                CompetencyJobRole.ModifiedDate = DateTime.UtcNow;
                await jobrolerepository.Update(CompetencyJobRole);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTypeAheadForCompetenciesMaster/{search?}")]
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
                return Ok(await jobrolerepository.GetTypeAheadForJobRole(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetTypeAhead")]
        public async Task<IActionResult> GetTypeAhead()
        {
            try
            {
                return Ok(await jobrolerepository.GetTypeAhead());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete("DeleteUserCarrerJobrole")]
        public async Task<IActionResult> DeleteUserJobrole([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                bool isdeleted = await this.jobrolerepository.DeleteUserCarrerJobRole(DecryptedId, UserId);
                if (isdeleted == true)
                {
                    return Ok(isdeleted);
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("SendNotificationForManagerApproval")]
        public async Task<IActionResult> SendNotificationForManagerApproval()
        {
            try
            {
                return Ok(await jobrolerepository.SendNotificationForManagerApproval());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}