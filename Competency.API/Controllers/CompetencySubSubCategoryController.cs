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
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CompetencySubSubCategoryController : IdentityController
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencySubCategoryController));
        private ICompetencySubSubCategoryRepository competencySubSubCategoryRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompetencySubSubCategoryController(IConfiguration configure, IHttpContextAccessor httpContextAccessor, ICompetencySubSubCategoryRepository competencySubSubCategoryController, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this.competencySubSubCategoryRepository = competencySubSubCategoryController;
            _configuration = configure;
            _httpContextAccessor = httpContextAccessor;

        }

        [HttpGet("get/{page:int}/{pageSize:int}/{categoryId:int?}/{subcategoryId:int?}/{search?}")]
        public async Task<IActionResult> Get(int page, int pageSize, int categoryId = 0,int subcategoryId=0, string search = null)
        {
            try
            {
                var competencySubSubCategory = await this.competencySubSubCategoryRepository.GetAllCompetencySubSubCategory(page, pageSize, categoryId, subcategoryId, search);
                return Ok(competencySubSubCategory);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        public async Task<IApiResponse> Post([FromBody] CompetencySubSubCategory competencySubSubCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CompetencySubSubCategory compentencySubCat = new CompetencySubSubCategory();
                    if (await this.competencySubSubCategoryRepository.Exists(competencySubSubCategory.SubSubcategoryCode, competencySubSubCategory.SubSubcategoryDescription))
                        return new APIResposeNo { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Content = EnumHelper.GetEnumDescription(MessageType.Duplicate), StatusCode = 409 };
                    else
                    {
                        compentencySubCat.CategoryId = competencySubSubCategory.CategoryId;
                        compentencySubCat.SubCategoryId = competencySubSubCategory.SubCategoryId;
                        compentencySubCat.SubSubcategoryCode = competencySubSubCategory.SubSubcategoryCode;
                        compentencySubCat.SubSubcategoryDescription = competencySubSubCategory.SubSubcategoryDescription;
                        compentencySubCat.IsDeleted = false;
                        compentencySubCat.IsActive = true;
                        compentencySubCat.CreatedBy = UserId;
                        compentencySubCat.CreatedDate = DateTime.UtcNow;
                        compentencySubCat.ModifiedDate = DateTime.UtcNow;
                        try
                        {
                            await competencySubSubCategoryRepository.Add(compentencySubCat);

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
        public async Task<IApiResponse> Put([FromBody] CompetencySubSubCategoryResult competencySubCategory)
        {
            try
            {
                CompetencySubSubCategory compentencySubCat = await this.competencySubSubCategoryRepository.Get(s => s.IsDeleted == false && s.Id != competencySubCategory.Id && s.SubSubcategoryCode == competencySubCategory.SubSubcategoryCode);

                if (compentencySubCat != null)
                {
                    return new APIResposeNo { Message = "Subset Code Already Exists", Content = "Subset Code Already Exists", StatusCode = 409 };
                }
                compentencySubCat = await this.competencySubSubCategoryRepository.Get(s => s.IsDeleted == false && s.Id != competencySubCategory.Id && s.SubSubcategoryDescription == competencySubCategory.SubSubcategoryDescription);

                if (compentencySubCat != null)
                {
                    return new APIResposeNo { Message = "Subset Description Already Exists", Content = "Subset Description Already Exists", StatusCode = 409 };
                }
                compentencySubCat = await this.competencySubSubCategoryRepository.Get(s => s.IsDeleted == false && s.Id == competencySubCategory.Id);

                if (ModelState.IsValid && compentencySubCat != null)
                {

                    compentencySubCat.SubSubcategoryCode = competencySubCategory.SubSubcategoryCode;
                    compentencySubCat.SubSubcategoryDescription = competencySubCategory.SubSubcategoryDescription;
                    compentencySubCat.CategoryId = competencySubCategory.CategoryId;
                    compentencySubCat.SubCategoryId = competencySubCategory.SubCategoryId;
                    compentencySubCat.IsDeleted = false;
                    compentencySubCat.IsActive = true;
                    compentencySubCat.CreatedBy = 1;
                    compentencySubCat.ModifiedBy = 1;
                    compentencySubCat.ModifiedDate = DateTime.UtcNow;
                    await this.competencySubSubCategoryRepository.Update(compentencySubCat);

                }
                else
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

        [HttpGet("{id}")]
        public async Task<IApiResponse> Get(int id)
        {
            try
            {
                CompetencySubSubCategory result = new CompetencySubSubCategory();
                result = await this.competencySubSubCategoryRepository.Get(s => s.IsDeleted == false && s.Id == id);
                APIResponseSingle<CompetencySubSubCategoryResult> aPIResponseSingle = new APIResponseSingle<CompetencySubSubCategoryResult>();

                var config = new MapperConfiguration(cfg =>
                           cfg.CreateMap<CompetencySubSubCategory, CompetencySubSubCategoryResult>()
                      );
                IMapper mapper = config.CreateMapper();
                CompetencySubSubCategoryResult result1 = mapper.Map<CompetencySubSubCategoryResult>(result);
                aPIResponseSingle.Record = result1;
                return aPIResponseSingle;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                //int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CompetencySubSubCategory compentencySubCat = await this.competencySubSubCategoryRepository.Get(id);

                if (compentencySubCat == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                //if (competencySubCategoryRepository.IsDependacyExist(compentencySubCat.CategoryId))
                //{
                //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                //}
                compentencySubCat.IsDeleted = true;
                await this.competencySubSubCategoryRepository.Update(compentencySubCat);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
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
                string sFileName = @"CompetencySubSubCategory.xlsx";
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

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CompetencySubSubCategory");

                    worksheet.Cells[1, 1].Value = "CategoryName";
                    worksheet.Cells[1, 2].Value = "SubCategory";
                    worksheet.Cells[1, 3].Value = "SubsetCode";
                    worksheet.Cells[1, 4].Value = "SubsetDescription";



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
                return File(fileData, FileContentType.Excel, "CompetencySubSubCategory.xlsx");
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

                ApiResponse response = await this.competencySubSubCategoryRepository.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
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
