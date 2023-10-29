// ======================================
// <copyright file="CompetencyLevelsController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

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
using static Competency.API.Common.AuthorizePermissions;
using static Competency .API.Common.TokenPermissions;
using Competency.API.Helper;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using OfficeOpenXml;
using Competency.API.Helper.Metadata;
using Competency.API.APIModel;

namespace Competency.API.Controllers.Competency
{
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CompetencyLevelsController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencyLevelsController));
        private ICompetencyLevelsRepository competencyLevelsRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetencyLevelsController(ICompetencyLevelsRepository competencyLevelsController, IIdentityService _identitySvc, ITokensRepository tokensRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(_identitySvc)
        {
            this.competencyLevelsRepository = competencyLevelsController;
            this._tokensRepository = tokensRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpGet]
        [PermissionRequired(Permissions.Competencieslevel)]
        public async Task<IActionResult> Get()
        {
            try
            {
                IEnumerable<APICompetencyLevels> competencyLevels = await this.competencyLevelsRepository.GetCompetencyLevels();
                return Ok(competencyLevels);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.Competencieslevel)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                var competencyLevels = await this.competencyLevelsRepository.GetAllCompetencyLevels(page, pageSize, search);
                return Ok(competencyLevels);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetNextLevel/{competencyID:int}")]
        public async Task<IActionResult> GetNextLevel(int competencyID)
        {
            try
            {
                var competencyLevels = await this.competencyLevelsRepository.GetNextLevel(competencyID);
                return Ok(competencyLevels);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByCompetencyMasterId/{catId?}/{CompId?}")]
       
        public async Task<IActionResult> GetById(int? catId, int CompId)
        {
            try
            {
                var competencyLevels = await this.competencyLevelsRepository.GetAllCompetencyLevelsCat(catId, CompId);
                return Ok(Mapper.Map<List<APICompetencyLevels>>(competencyLevels));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.Competencieslevel)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this.competencyLevelsRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        [PermissionRequired(Permissions.Competencieslevel)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var competencyLevels = await this.competencyLevelsRepository.GetCompetencyLevels(id);
                return Ok(competencyLevels);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost]
        [PermissionRequired(Permissions.Competencieslevel)]
        public async Task<IActionResult> Post([FromBody] APICompetencyLevels aPICompetencyLevels)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await this.competencyLevelsRepository.Exists(aPICompetencyLevels.LevelName, aPICompetencyLevels.CompetencyId))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        CompetencyLevels competencyLevel = Mapper.Map<CompetencyLevels>(aPICompetencyLevels);
                        competencyLevel.IsDeleted = false;
                        competencyLevel.ModifiedBy = 1;
                        competencyLevel.ModifiedDate = DateTime.UtcNow;
                        competencyLevel.CreatedBy = 1;
                        competencyLevel.CreatedDate = DateTime.UtcNow;
                        await competencyLevelsRepository.Add(competencyLevel);
                        return Ok(competencyLevel);
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

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.Competencieslevel)]
        public async Task<IActionResult> Put(int id, [FromBody] APICompetencyLevels aPICompetencyLevels)
        {
            try
            {
                CompetencyLevels competencyLevels = await this.competencyLevelsRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (competencyLevels == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (competencyLevels.CompetencyId != aPICompetencyLevels.CompetencyId || competencyLevels.CategoryId != aPICompetencyLevels.CategoryId || competencyLevels.LevelName != aPICompetencyLevels.LevelName)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                if (ModelState.IsValid && competencyLevelsRepository != null)
                {
                    competencyLevels.LevelName = aPICompetencyLevels.LevelName;
                    competencyLevels.CompetencyId = aPICompetencyLevels.CompetencyId;
                    competencyLevels.CategoryId = aPICompetencyLevels.CategoryId == null ? 0 : Convert.ToInt32(aPICompetencyLevels.CategoryId);
                    competencyLevels.DetailedDescriptionOfLevel = aPICompetencyLevels.DetailedDescriptionOfLevel;
                    competencyLevels.BriefDescriptionCompetencyLevel = aPICompetencyLevels.BriefDescriptionCompetencyLevel;
                    competencyLevels.ModifiedBy = 1;
                    competencyLevels.ModifiedDate = DateTime.UtcNow;
                    await this.competencyLevelsRepository.Update(competencyLevels);
                }

                return this.Ok(competencyLevels);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }


        [HttpDelete]
        [PermissionRequired(Permissions.Competencieslevel)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CompetencyLevels competencyLevels = await this.competencyLevelsRepository.Get(DecryptedId);
                if (competencyLevels == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });

                if (competencyLevelsRepository.IsDependacyExist(DecryptedId))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                competencyLevels.IsDeleted = true;
                await this.competencyLevelsRepository.Update(competencyLevels);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        #region Bulk Upload for Competency Levels

        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"CompetenciesLevel.xlsx";
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

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CompetenciesLevel");

                    worksheet.Cells[1, 1].Value = "Category";

                    worksheet.Cells[1, 2].Value = "Competency*";

                    worksheet.Cells[1, 3].Value = "Competency Level*";

                    worksheet.Cells[1, 4].Value = "Level Description*";


                    package.Save();
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "CompetenciesLevel.xlsx");
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

                ApiResponse response = await this.competencyLevelsRepository.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
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
