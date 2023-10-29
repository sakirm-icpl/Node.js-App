using AspNet.Security.OAuth.Introspection;
using MyCourse.API.APIModel;
using MyCourse.API.Common;
using MyCourse.API.Helper;
using MyCourse.API.Helper.Metadata;
using MyCourse.API.Repositories.Interfaces;
//using MyCourse.API.Repositories.Interfaces.ILT;
using MyCourse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.IO;
using System.Threading.Tasks;
using static MyCourse.API.Common.AuthorizePermissions;
using static MyCourse.API.Common.TokenPermissions;
using log4net;
using MyCourse.API.Model.Log_API_Count;

namespace MyCourse.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class DataMigrationController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DataMigrationController));
        IDataMigration _dataMigration;

        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IILTSchedule _IILTSchedule;
        public IConfiguration _configuration { get; }
        private readonly ITokensRepository _tokensRepository;
        public DataMigrationController(IWebHostEnvironment environment,
            IConfiguration configure, IHttpContextAccessor httpContextAccessor,
            IIdentityService _identitySvc, IDataMigration dataMigration, IILTSchedule IILTSchedule,
            ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this._configuration = configure;
            this.hostingEnvironment = environment;
            this._httpContextAccessor = httpContextAccessor;
            this._dataMigration = dataMigration;
            this._IILTSchedule = IILTSchedule;
            this._tokensRepository = tokensRepository;
        }

        [HttpPost]
        [Route("SaveFileData")]
        [PermissionRequired(Permissions.pastcourseCompletion)]
        public async Task<IActionResult> PostFile([FromBody] APIDataMigrationFilePath aPIDataMigration)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        string filefinal = sWebRootFolder + aPIDataMigration.Path;
                        FileInfo file = new FileInfo(Path.Combine(filefinal));
                        ApiResponse response = await this._dataMigration.ProcessImportFile(file, _dataMigration, UserId, OrgCode);

                        return Ok(response.ResponseObject);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        string exception = ex.Message;
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }

        }


        [HttpPost]
        [Route("SaveCompetencyMasterData")]
        [PermissionRequired(Permissions.CompetencyDataMigration)]
        public async Task<IActionResult> SaveCompetencyMasterData([FromBody] APIDataMigrationFilePath aPIDataMigration)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        string filefinal = sWebRootFolder + aPIDataMigration.Path;
                        FileInfo file = new FileInfo(Path.Combine(filefinal));
                        ApiResponse response = await this._dataMigration.ProcessImportFile_Competency(file, _dataMigration, UserId);
                        return Ok(response.ResponseObject);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        string exception = ex.Message;
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }

        }




        [HttpPost]
        [Route("PostFileUpload")]
        [Authorize(Roles = "CA")]
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
        [Route("SaveAssessmentFileData")]
        [Authorize(Roles = "CA")]
        public async Task<IActionResult> PostAssessmentFile([FromBody] APIDataMigrationFilePath aPIDataMigration)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string sWebRootFolder = hostingEnvironment.WebRootPath;
                        string filefinal = sWebRootFolder + aPIDataMigration.Path;
                        FileInfo file = new FileInfo(Path.Combine(filefinal));
                        ApiResponse response = await this._dataMigration.ProcessAssessmentImportFile(file, _dataMigration, UserId);
                        return Ok(response.ResponseObject);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }

        }

        [HttpPost]
        [Route("SaveCourseModuleFileData")]
        [PermissionRequired(Permissions.CourseModulemanage)]
        public async Task<IActionResult> PostCourseModuleFile([FromBody] APIDataMigrationFilePath aPIDataMigration)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        string filefinal = sWebRootFolder + aPIDataMigration.Path;
                        FileInfo file = new FileInfo(Path.Combine(filefinal));
                        ApiResponse response = await this._dataMigration.ProcessCourseModuleImportFile(file, _dataMigration, UserId);
                        return Ok(response);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }


        [HttpPost]
        [Route("PostILTScheduleFile")]
        [PermissionRequired(Permissions.schedule)]
        public async Task<ActionResult> PostILTScheduleFile([FromBody] APIDataMigrationFilePath apiDataMigrationFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);
                
                ApiResponse response = await this._dataMigration.ILTScheduleImportFile(apiDataMigrationFilePath, UserId, OrganisationCode);
                return Ok(response.ResponseObject);
                   
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
               return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }


        [HttpGet]
        [Route("ExportPastCourseCompletion")]
        [PermissionRequired(Permissions.pastcourseCompletion)]
        public async Task<IActionResult> ExportPastCourseCompletion()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"PastCourseCompletion.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PastCourseCompletion");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "CourseCode";
                    worksheet.Cells[1, 2].Value = "UserId";
                    worksheet.Cells[1, 3].Value = "ScheduleCode";
                    worksheet.Cells[1, 4].Value = "StartDate";
                    worksheet.Cells[1, 5].Value = "LastActivityDate";
                    worksheet.Cells[1, 6].Value = "External_AssessmentMarksObtained";
                    worksheet.Cells[1, 7].Value = "External_NoOfAttempts";
                    worksheet.Cells[1, 8].Value = "External_AssessmentResult";
                    worksheet.Cells[1, 9].Value = "Inbuilt_AssessmentMarks";
                    worksheet.Cells[1, 10].Value = "Inbuilt_Result";


                    worksheet.Cells["D1:D5000"].Style.Numberformat.Format = "@";
                    worksheet.Cells["E1:E5000"].Style.Numberformat.Format = "@";

                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("ExportCompetency")]
        [PermissionRequired(Permissions.CompetencyDataMigration)]
        public async Task<IActionResult> ExportCompetency()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"CompetencyDataMigration.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));

                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CompetencyDataMigration");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Category";
                    worksheet.Cells[1, 2].Value = "CategoryName";
                    worksheet.Cells[1, 3].Value = "CompetencyName";
                    worksheet.Cells[1, 4].Value = "CompetencyDescription";
                    worksheet.Cells[1, 5].Value = "CompetencyLevel";
                    worksheet.Cells[1, 6].Value = "LevelDescription";
                    worksheet.Cells[1, 7].Value = "CourseCode";

                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("ExportCourseModule")]
        [PermissionRequired(Permissions.CourseModulemanage)]

        public async Task<IActionResult> ExportCourseModule()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"CourseModuleData.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CourseModuleData");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "CourseName*";
                    worksheet.Cells[1,1].Style.Font.Bold = true;
                    worksheet.Cells[1, 2].Value = "CourseType*";
                    worksheet.Cells[1, 2].Style.Font.Bold = true;
                    worksheet.Cells[1, 3].Value = "ModuleType*";
                    worksheet.Cells[1, 3].Style.Font.Bold = true;
                    worksheet.Cells[1, 4].Value = "IsApplicableToAll*";
                    worksheet.Cells[1, 4].Style.Font.Bold = true;
                    worksheet.Cells[1, 5].Value = "MetaData*";
                    worksheet.Cells[1, 5].Style.Font.Bold = true;
                    worksheet.Cells[1, 6].Value = "ContentType*";
                    worksheet.Cells[1, 6].Style.Font.Bold = true;
                    worksheet.Cells[1, 7].Value = "CourseCode*";
                    worksheet.Cells[1, 7].Style.Font.Bold = true;
                    worksheet.Cells[1, 8].Value = "CourseCatagoryName";
                    worksheet.Cells[1, 9].Value = "CourseSubCatagoryName";
                    worksheet.Cells[1, 10].Value = "CourseDesciption";
                    worksheet.Cells[1, 11].Value = "ModuleName*";
                    worksheet.Cells[1, 11].Style.Font.Bold = true;
                    worksheet.Cells[1, 12].Value = "IsActive*";
                    worksheet.Cells[1, 12].Style.Font.Bold = true;
                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("ExportSchedule")]
        [PermissionRequired(Permissions.schedule)]
        public async Task<IActionResult> ExportSchedule()
        {
            try
            {
                var result = await _dataMigration.ExportImportFormat(OrganisationCode);
                Response.ContentType = FileContentType.Excel;
                return File(result, FileContentType.Excel, FileName.ILTScheduleImportFormat);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet]
        [Route("ExportTrainingRecommondations")]
        [PermissionRequired(Permissions.schedule)]
        public async Task<IActionResult> ExportTrainingRecommondations()
        {
            try
            {
                var result = await _dataMigration.TrainingReommendationFormat(OrganisationCode);
                Response.ContentType = FileContentType.Excel;
                return File(result, FileContentType.Excel, FileName.TrainingReommendationNeeds);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostTrainingRecommondations")]
        [PermissionRequired(Permissions.schedule)]
        public async Task<ActionResult> PostTrainingRecommondations([FromBody] APIDataMigrationFilePath apiDataMigrationFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                ApiResponse response = await this._dataMigration.TrainingRecommondationImportFile(apiDataMigrationFilePath, UserId, OrganisationCode);
                return Ok(response.ResponseObject);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }
    }
}