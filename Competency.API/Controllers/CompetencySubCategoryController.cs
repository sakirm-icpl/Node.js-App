using AspNet.Security.OAuth.Introspection;
using Competency.API.APIModel.Competency;
using AutoMapper;
using Competency.API.Common;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using Competency.API.Helper.Metadata;
using System.Threading.Tasks;
using static Competency.API.Common.TokenPermissions;
using static Competency.API.Model.ResponseModels;
using Competency.API.APIModel;

namespace Competency.API.Controllers.Competency
{

    [Route("api/v1/comp/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CompetencySubCategoryController : IdentityController
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencySubCategoryController));
        private ICompetencySubCategoryRepository competencySubCategoryRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetencySubCategoryController(IConfiguration configure, IHttpContextAccessor httpContextAccessor, ICompetencySubCategoryRepository competencySubCategoryController, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this.competencySubCategoryRepository = competencySubCategoryController;
            _configuration = configure;
            _httpContextAccessor = httpContextAccessor;

        }

        [HttpGet("get/{page:int}/{pageSize:int}/{categoryId:int?}/{search?}")]
        public async Task<IActionResult> Get(int page, int pageSize, int categoryId = 0, string search = null)
        {
            try
            {
                var competencySubCategory = await this.competencySubCategoryRepository.GetAllCompetencySubCategory(page, pageSize, categoryId, search);
                return Ok(competencySubCategory);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("filterbyCategoryID/{categoryId:int?}")]
        public async Task<IActionResult> GetByCategoryId(int categoryId = 0)
        {
            try
            {
                var competencySubCategory = await this.competencySubCategoryRepository.GetByCategoryId(categoryId);
                return Ok(competencySubCategory);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("{id}")]
        public async Task<IApiResponse> Get(int id)
        {
            try
            {
                CompetencySubCategory result = new CompetencySubCategory();
                result = await this.competencySubCategoryRepository.Get(s => s.IsDeleted == false && s.Id == id);
                APIResponseSingle<CompetencySubCategoryResult> aPIResponseSingle = new APIResponseSingle<CompetencySubCategoryResult>();

                var config = new MapperConfiguration(cfg =>
                           cfg.CreateMap<CompetencySubCategory, CompetencySubCategoryResult>()
                      );
                IMapper mapper = config.CreateMapper();
                CompetencySubCategoryResult result1 = mapper.Map<CompetencySubCategoryResult>(result);
                aPIResponseSingle.Record = result1;
                return aPIResponseSingle;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpPost]
        public async Task<IApiResponse> Post([FromBody] CompetencySubCategory competencySubCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CompetencySubCategory compentencySubCat = new CompetencySubCategory();
                    if (await this.competencySubCategoryRepository.Exists(competencySubCategory.SubcategoryCode, competencySubCategory.SubcategoryDescription))
                        return new APIResposeNo { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Content = EnumHelper.GetEnumDescription(MessageType.Duplicate), StatusCode = 409 };
                    else
                    {
                        compentencySubCat.CategoryId = competencySubCategory.CategoryId;
                        compentencySubCat.SubcategoryCode = competencySubCategory.SubcategoryCode;
                        compentencySubCat.SubcategoryDescription = competencySubCategory.SubcategoryDescription;
                        compentencySubCat.IsDeleted = false;
                        compentencySubCat.IsActive = true;
                        compentencySubCat.CreatedBy = UserId;
                        compentencySubCat.CreatedDate = DateTime.UtcNow;
                        compentencySubCat.ModifiedDate = DateTime.UtcNow;
                        try
                        {
                            await competencySubCategoryRepository.Add(compentencySubCat);

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.InnerException);
                            throw;
                        }
                        APIResposeYes aPIResposeYes = new APIResposeYes();
                        aPIResposeYes.Message = "Record Inserted Successfully";
                        aPIResposeYes.Content = "Record Inserted Successfully";
                        return aPIResposeYes;
                    }
                }
                else
                    return new APIResposeNo { Message = "Invalid Data", Content = "Invalid Data" };
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new APIResposeNo { };
            }
        }

        [HttpPost("Update")]
        public async Task<IApiResponse> Put([FromBody] CompetencySubCategoryResult competencySubCategory)
        {
            try
            {
                CompetencySubCategory compentencySubCat = await this.competencySubCategoryRepository.Get(s => s.IsDeleted == false && s.Id != competencySubCategory.Id && s.SubcategoryCode == competencySubCategory.SubcategoryCode);

                if (compentencySubCat != null)
                {
                    return new APIResposeNo { Message = "Subcategory Code Already Exists", Content = "Subcategory Code Already Exists", StatusCode = 409 };
                }
                compentencySubCat = await this.competencySubCategoryRepository.Get(s => s.IsDeleted == false && s.Id != competencySubCategory.Id && s.SubcategoryDescription == competencySubCategory.SubcategoryDescription);

                if (compentencySubCat != null)
                {
                    return new APIResposeNo { Message = "Subcategory Description Already Exists", Content = "Subcategory Description Already Exists", StatusCode = 409 };
                }
                compentencySubCat = await this.competencySubCategoryRepository.Get(s => s.IsDeleted == false && s.Id == competencySubCategory.Id);

                if (ModelState.IsValid && compentencySubCat != null)
                {

                    compentencySubCat.SubcategoryCode = competencySubCategory.SubcategoryCode;
                    compentencySubCat.SubcategoryDescription = competencySubCategory.SubcategoryDescription;
                    compentencySubCat.CategoryId = competencySubCategory.CategoryId;
                    compentencySubCat.IsDeleted = false;
                    compentencySubCat.IsActive = true;
                    compentencySubCat.CreatedBy = 1;
                    compentencySubCat.ModifiedBy = 1;
                    compentencySubCat.ModifiedDate = DateTime.UtcNow;
                    await this.competencySubCategoryRepository.Update(compentencySubCat);

                } else
                {
                    return new APIResposeNo { Message = "Invalid Data", Content = "Invalid Data", StatusCode = 400 };
                }

                APIResposeYes aPIResposeYes = new APIResposeYes();
                aPIResposeYes.Message = "Record Updated Successfully";
                aPIResposeYes.Content = "Record Updated Successfully";
                return aPIResposeYes;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new APIResposeNo { StatusCode = 500, Message = "Error", Content = "Exception Occurs" + ex.Message };
            }
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                //int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CompetencySubCategory compentencySubCat = await this.competencySubCategoryRepository.Get(id);

                if (compentencySubCat == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                //if (competencySubCategoryRepository.IsDependacyExist(compentencySubCat.CategoryId))
                //{
                //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                //}
                compentencySubCat.IsDeleted = true;
                await this.competencySubCategoryRepository.Update(compentencySubCat);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Search/{categoryId?}/{search}")]
        public async Task<IActionResult> Search(int categoryId, string search)
        {
            try
            {
                var result = await this.competencySubCategoryRepository.Search(categoryId, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


        #region bulk upload competency sub category
        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"CompetencySubCategory.xlsx";
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

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CompetencySubCategory");

                    worksheet.Cells[1, 1].Value = "CategoryName";
                    worksheet.Cells[1, 2].Value = "SubCategoryCode";
                    worksheet.Cells[1, 3].Value = "SubCategoryDescription";



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
                return File(fileData, FileContentType.Excel, "CompetencySubCategory.xlsx");
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

                ApiResponse response = await this.competencySubCategoryRepository.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
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
