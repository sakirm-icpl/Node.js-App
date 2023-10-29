// ======================================
// <copyright file="CompetenciesMasterController.cs" company="Enthralltech Pvt. Ltd.">
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
using static Competency.API.Common.TokenPermissions;
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
    public class CompetenciesMasterController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesMasterController));
        private ICompetenciesMasterRepository competenciesMasterRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetenciesMasterController(ICompetenciesMasterRepository competenciesMasterController, IIdentityService _identitySvc, ITokensRepository tokensRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(_identitySvc)
        {
            this.competenciesMasterRepository = competenciesMasterController;
            this._tokensRepository = tokensRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> Get()
        {
            try
            {
                IEnumerable<APICompetenciesMaster> competenciesMaster = await this.competenciesMasterRepository.GetCompetenciesMaster();
                return Ok(competenciesMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                var competenciesMaster = await this.competenciesMasterRepository.GetCompetenciesMaster(page, pageSize, search);
                return Ok(competenciesMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByCategoryId/{catId:int?}")]
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> GetCompetancyMaster(int? catId = null)
        {
            try
            {
                var competenciesMaster = await this.competenciesMasterRepository.GetCompetenciesMasterByID(catId);
                return Ok(Mapper.Map<List<APICompetenciesMaster>>(competenciesMaster));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this.competenciesMasterRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var competenciesMaster = await this.competenciesMasterRepository.GetCompetenciesMaster(id);
                return Ok(competenciesMaster);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost]
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> Post([FromBody] APICompetenciesMaster competenciesMaster)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    if (await competenciesMasterRepository.ExistsRecord(competenciesMaster.Id, competenciesMaster.CompetencyName))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        CompetenciesMaster competency_Mas = Mapper.Map<CompetenciesMaster>(competenciesMaster);
                        competency_Mas.IsDeleted = false;
                        competency_Mas.IsActive = true;
                        competency_Mas.CreatedBy = 1;
                        competency_Mas.CreatedDate = DateTime.UtcNow;
                        await competenciesMasterRepository.Add(competency_Mas);
                        return Ok(competency_Mas);
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
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> Put(int id, [FromBody] APICompetenciesMaster competenciesMaster)
        {
            try
            {

                CompetenciesMaster competencyMas = await this.competenciesMasterRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (competencyMas == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (competencyMas.CategoryId != competenciesMaster.CategoryId || competencyMas.CompetencyName != competenciesMaster.CompetencyName)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (ModelState.IsValid && competencyMas != null)
                {
                    if (await competenciesMasterRepository.ExistsRecord(id, competenciesMaster.CompetencyName))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        competencyMas.CompetencyName = competenciesMaster.CompetencyName;
                        competencyMas.CompetencyDescription = competenciesMaster.CompetencyDescription;
                        competencyMas.CategoryId = competenciesMaster.CategoryId == null ? 0 : Convert.ToInt32(competenciesMaster.CategoryId);
                        competencyMas.ModifiedBy = 1;
                        competencyMas.ModifiedDate = DateTime.UtcNow;
                        await this.competenciesMasterRepository.Update(competencyMas);
                    }
                }

                return this.Ok(competencyMas);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.CompetenciesMaster)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CompetenciesMaster competencyMas = await this.competenciesMasterRepository.Get(DecryptedId);
                if (competencyMas == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                if (competenciesMasterRepository.IsDependacyExist(DecryptedId))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                competencyMas.IsDeleted = true;
                await this.competenciesMasterRepository.Update(competencyMas);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        #region bulk upload competency master

        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"CompetenciesMaster.xlsx";
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

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CompetencyCategory");

                    worksheet.Cells[1, 1].Value = "Category";
                    worksheet.Cells[1, 2].Value = "Competency Name*";
                    worksheet.Cells[1, 3].Value = "Competency Description*";


                    package.Save();
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "CompetenciesMaster.xlsx");
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

                ApiResponse response = await this.competenciesMasterRepository.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
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
