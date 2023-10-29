// ======================================
// <copyright file="CompetencyCategoryController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
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
using System.Linq;
using System.Threading.Tasks;
using static Competency.API.Common.AuthorizePermissions;
using static Competency.API.Common.TokenPermissions;
using Competency.API.Helper;
using log4net;
using System.IO;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using Competency.API.Helper.Metadata;
using Microsoft.AspNetCore.Http;
using Competency.API.APIModel;

namespace Competency.API.Controllers.Competency
{
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CompetencyCategoryController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencyCategoryController));
        private ICompetencyCategoryRepository competencyCategoryRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetencyCategoryController(IConfiguration configure, IHttpContextAccessor httpContextAccessor, ICompetencyCategoryRepository competencyCategoryController, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this.competencyCategoryRepository = competencyCategoryController;
            this._tokensRepository = tokensRepository;
            _configuration = configure;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IEnumerable<CompetencyCategory>> Get()
        {
            try
            {
                IEnumerable<CompetencyCategory> categories = await this.competencyCategoryRepository.GetAll(s => s.IsDeleted == false);
                return categories.OrderBy(c => c.Category);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.CompetenciesCategories)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<CompetencyCategory> competencyCategory = await this.competencyCategoryRepository.GetAllCompetencyCategory(page, pageSize, search);
                return Ok(competencyCategory);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.CompetenciesCategories)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this.competencyCategoryRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        [PermissionRequired(Permissions.CompetenciesCategories)]
        public async Task<CompetencyCategory> Get(int id)
        {
            try
            {
                return await this.competencyCategoryRepository.Get(s => s.IsDeleted == false && s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.CompetenciesCategories)]
        public async Task<IActionResult> Post([FromBody] CompetencyCategory competencyCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CompetencyCategory compentencyCat = new CompetencyCategory();
                    if (await this.competencyCategoryRepository.Exists(competencyCategory.CategoryName, competencyCategory.Category))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        compentencyCat.CategoryName = competencyCategory.CategoryName;
                        compentencyCat.Category = competencyCategory.Category;
                        compentencyCat.IsDeleted = false;
                        compentencyCat.IsActive = true;
                        compentencyCat.CreatedBy = UserId;
                        compentencyCat.CreatedDate = DateTime.UtcNow;
                        await competencyCategoryRepository.Add(compentencyCat);
                        return Ok(compentencyCat);
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
        [PermissionRequired(Permissions.CompetenciesCategories)]
        public async Task<IActionResult> Put(int id, [FromBody] CompetencyCategory competencyCategory)
        {
            try
            {
                CompetencyCategory compentencyCat = await this.competencyCategoryRepository.Get(s => s.IsDeleted == false && s.Id == id);

                if (compentencyCat == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (compentencyCat.CategoryName != competencyCategory.CategoryName)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (ModelState.IsValid && compentencyCat != null)
                {
                    if (await this.competencyCategoryRepository.Exists(competencyCategory.CategoryName, competencyCategory.Category, id))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        compentencyCat.CategoryName = competencyCategory.CategoryName;
                        compentencyCat.Category = competencyCategory.Category;
                        compentencyCat.IsDeleted = false;
                        compentencyCat.IsActive = true;
                        compentencyCat.CreatedBy = 1;
                        compentencyCat.ModifiedBy = 1;
                        compentencyCat.ModifiedDate = DateTime.UtcNow;
                        await this.competencyCategoryRepository.Update(compentencyCat);
                    }
                }

                return this.Ok(compentencyCat);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }


        [HttpDelete]
        [PermissionRequired(Permissions.CompetenciesCategories)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CompetencyCategory compentencyCat = await this.competencyCategoryRepository.Get(DecryptedId);
                if (compentencyCat == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                if (competencyCategoryRepository.IsDependacyExist(DecryptedId))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                compentencyCat.IsDeleted = true;
                await this.competencyCategoryRepository.Update(compentencyCat);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        /// <summary>
        /// Search specific SupportManagement.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IEnumerable<CompetencyCategory>> Search(string q)
        {
            try
            {
                IEnumerable<CompetencyCategory> result = await this.competencyCategoryRepository.Search(q);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


        [HttpGet]
        [Route("GetCompetencySpiderchart")]
        public async Task<IActionResult> GetCompetencySpiderchart()
        {
            try
            {
                IEnumerable<APICompetencyChart> competencyChart = await this.competencyCategoryRepository.GetCompetencySpiderchart(UserId);
                if (competencyChart != null)
                {
                    float[] Counts = competencyChart.Select(c => c.AssessmentPercentage).ToArray();
                    string[] CompetencyName = competencyChart.Select(c => c.CompetencyName).ToArray();
                    return Ok(new { Counts, CompetencyName });
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


        #region bulk upload competency category
        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"CompetencyCategory.xlsx";
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

                    worksheet.Cells[1, 1].Value = "Category Code*";
                    worksheet.Cells[1, 2].Value = "Category Name*";


                    /*worksheet.Cells["F1:F2000"].Style.Numberformat.Format = "@";*/

                    package.Save();
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "CompetencyCategory.xlsx");
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

                ApiResponse response = await this.competencyCategoryRepository.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
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
