using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;

using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Helper.Metadata;
using ILT.API.Model.ILT;
using ILT.API.Repositories.Interfaces;
//using ILT.API.Repositories.Interfaces.ILT;
using ILT.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static ILT.API.Common.AuthorizePermissions;
using static ILT.API.Common.TokenPermissions;
using log4net;
using System.Linq;
using ILT.API.Model.Log_API_Count;

namespace ILT.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/ILTTrainingAttendance")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ILTTrainingAttendanceController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTTrainingAttendanceController));
        IILTTrainingAttendance _IILTTrainingAttendance;
        private readonly ICourseModuleAssociationRepository _courseModuleAssociationRepository;
        private readonly IModuleCompletionStatusRepository _moduleCompletionStatus;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokensRepository _tokensRepository;
        // ---- Attendance By 0.Web 1.OTP 2.QR Code ------- //
        public IConfiguration _configuration { get; set; }
        public ILTTrainingAttendanceController(IILTTrainingAttendance IILTTrainingAttendance, IHttpContextAccessor httpContextAccessor, ICourseModuleAssociationRepository courseModuleAssociationRepository, IModuleCompletionStatusRepository moduleCompletionStatus, IConfiguration configure, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _IILTTrainingAttendance = IILTTrainingAttendance;
            _courseModuleAssociationRepository = courseModuleAssociationRepository;
            _moduleCompletionStatus = moduleCompletionStatus;
            _configuration = configure;
            this._httpContextAccessor = httpContextAccessor;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet("GetScheduleDetails/{ModuleId}/{CourseId}")]
        public async Task<IActionResult> Get(int ModuleId, int? CourseId)
        {
            try
            {
                return Ok(await this._IILTTrainingAttendance.GetByModuleId(ModuleId, CourseId, UserId, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page}/{pageSize}")]
        [Produces(typeof(List<APIILTScheduleDetails>))]
        public async Task<IActionResult> GetCourseAndSchedule(int page, int pageSize)
        {
            try
            {
                return Ok(await this._IILTTrainingAttendance.GetCourseAndSchedule(page, pageSize, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetILTCourses/{page}/{pageSize}")]
        [Produces(typeof(List<APIILTScheduleDetails>))]
        public async Task<IActionResult> GetILTCourses(int page, int pageSize)
        {
            try
            {
                return Ok(await this._IILTTrainingAttendance.GetILTCourses(page, pageSize, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetILTSchedules/{page}/{pageSize}/{CourseId}/{ModuleId}")]
        [Produces(typeof(List<APIILTScheduleDetails>))]
        public async Task<IActionResult> GetILTCourses(int page, int pageSize, int CourseId, int ModuleId)
        {
            try
            {
                return Ok(await this._IILTTrainingAttendance.GetILTScheduleDetails(page, pageSize, UserId, ModuleId, CourseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page}/{pageSize}/{search?}/{searchText?}/{showAllData?}")]
        [Produces(typeof(List<APITrainingAttendanceForAll>))]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> GetAllDetails(int page, int pageSize, string search = null, string searchText = null,bool  showAllData =false )
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTTrainingAttendance.GetAllDetails(page, pageSize,UserId, search, searchText));

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("Count/{search?}/{searchText?}/{showAllData?}")]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> GetAllDetailsCount(string search = null, string searchText = null, bool showAllData = false)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTTrainingAttendance.GetAllDetailsCount(UserId, search, searchText, showAllData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUsersForAttendance")]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> GetAllUsersForAttendance([FromBody] APIGetTrainingNomination objGetTrainingNomination)
        {
            try
            {
                if (objGetTrainingNomination.search != null)
                    objGetTrainingNomination.search = objGetTrainingNomination.search.ToLower().Equals("null") ? null : objGetTrainingNomination.search;
                if (objGetTrainingNomination.searchText != null)
                    objGetTrainingNomination.searchText = objGetTrainingNomination.searchText.ToLower().Equals("null") ? null : objGetTrainingNomination.searchText;
                return Ok(await this._IILTTrainingAttendance.GetAllUsersForAttendance(objGetTrainingNomination.scheduleID, objGetTrainingNomination.courseId, objGetTrainingNomination.page, objGetTrainingNomination.pageSize, objGetTrainingNomination.search, objGetTrainingNomination.searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetWebinarUsersForAttendance/{scheduleID}/{courseID}/{page}/{pageSize}/{search?}/{searchText?}")]
        public async Task<IActionResult> GetWebinarUsersForAttendance(int scheduleID, int courseID, int page, int pageSize, string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTTrainingAttendance.GetWebinarUsersForAttendance(scheduleID, courseID, page, pageSize, search, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUsersCountForAttendance")]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> GetUsersCountForAttendance([FromBody] APIGetTrainingNomination objGetTrainingNomination)
        {
            try
            {
                if (objGetTrainingNomination.search != null)
                    objGetTrainingNomination.search = objGetTrainingNomination.search.ToLower().Equals("null") ? null : objGetTrainingNomination.search;
                if (objGetTrainingNomination.searchText != null)
                    objGetTrainingNomination.searchText = objGetTrainingNomination.searchText.ToLower().Equals("null") ? null : objGetTrainingNomination.searchText;
                return Ok(await this._IILTTrainingAttendance.GetUsersCountForAttendance(objGetTrainingNomination.scheduleID, objGetTrainingNomination.courseId, objGetTrainingNomination.search, objGetTrainingNomination.searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("RegenerateOTP")]
        public async Task<IActionResult> RegenerateOTP([FromBody] APIILTTrainingAttendance aPIILTTrainingAttendance)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._IILTTrainingAttendance.GetRegeneratedOTP(aPIILTTrainingAttendance, OrganisationCode));
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> Post([FromBody] List<APIILTTrainingAttendance> aPIILTTrainingAttendance)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                    
                ApiResponseILT result = await _IILTTrainingAttendance.PostUserAttendance(aPIILTTrainingAttendance, UserId, OrganisationCode, LoginId, UserName, RoleCode);
                if (result.StatusCode != 200)
                    return BadRequest(result);
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("PostAttendance")]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> PostAttendance([FromBody] APIILTTrainingAttendance aPIILTTrainingAttendance)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ApiResponse result = await _IILTTrainingAttendance.PostUserAttendance(aPIILTTrainingAttendance, UserId);
                if (result.StatusCode == 400)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("UpdateILTTrainingAttendance")]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> Update([FromBody] List<APIILTTrainingAttendance> aPIILTTrainingAttendance)
        {
            try
            {
                bool IsWeb = aPIILTTrainingAttendance.Select(w => w.isWeb).FirstOrDefault();
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var result = await this._IILTTrainingAttendance.UpdateILTAttendance(aPIILTTrainingAttendance, UserId, LoginId, OrganisationCode, UserName, RoleCode);
                if(IsWeb==false)
                {
                    if (result.StatusCode != 200)
                    {
                        if(result.Description == "Attendance for the user already exists.")
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = result.Description });
                        else
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
                    }    
                    else
                        return Ok(true);
                }
                else
                {
                    if (result.StatusCode != 200)
                        return BadRequest(result);
                    else
                        return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"ILTAttendance.xlsx";
                string DomainName = this._configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }

                using (ExcelPackage package = new ExcelPackage(file))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ILTAttendance");

                    worksheet.Cells[1, 1].Value = "CourseCode*";
                    worksheet.Cells[1, 2].Value = "ModuleName*";
                    worksheet.Cells[1, 3].Value = "ScheduleCode*";
                    worksheet.Cells[1, 4].Value = "UserId*";
                    worksheet.Cells[1, 5].Value = "IsPresent*";
                    worksheet.Cells[1, 6].Value = "IsWaiver*";
                    worksheet.Cells[1, 7].Value = "AttendanceDate*";

                    worksheet.Cells["G1:G2000"].Style.Numberformat.Format = "@";

                    package.Save();
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "ILTAttendance");
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
                
                ApiResponse response = await this._IILTTrainingAttendance.ProcessImportFile(aPIDataMigration, UserId, OrganisationCode, LoginId, UserName);
                return Ok(response.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }


        [HttpPost]
        [Route("ExportTrainingAttendance")]
        [PermissionRequired(Permissions.iltattendance)]
        public async Task<IActionResult> Export([FromBody] APIAttendance attendance)
        {
            try
            {
                var ilttrainingattendance = await this._IILTTrainingAttendance.GetAttendanceWiseReport(attendance);

                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"Attendance.xlsx";
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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Attendance");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "CourseName";
                    worksheet.Cells[1, 2].Value = "ModuleName";
                    worksheet.Cells[1, 3].Value = "PlaceName";
                    worksheet.Cells[1, 4].Value = "ScheduleCode";


                    int row = 2, column = 1;

                    foreach (APIAttendance attendexport in ilttrainingattendance)
                    {
                        worksheet.Cells[row, column++].Value = attendexport.CourseName;
                        worksheet.Cells[row, column++].Value = attendexport.ModuleName;
                        worksheet.Cells[row, column++].Value = attendexport.PlaceName;
                        worksheet.Cells[row, column++].Value = attendexport.ScheduleCode;

                        if (row == 2)
                            break;

                    }
                    worksheet.Cells[row, column++].Value = "\n";
                    worksheet.Cells[4, 2].Value = "\nUser Attendance Details\n";

                    worksheet.Cells[5, 1].Value = "UserId";
                    worksheet.Cells[5, 2].Value = "UserName";
                    worksheet.Cells[5, 3].Value = "EmailId";
                    worksheet.Cells[5, 4].Value = "MobileNumber";
                    worksheet.Cells[5, 5].Value = "AttendanceDate";
                    worksheet.Cells[5, 6].Value = "AttendanceStatus";

                    int row1 = 6, column1 = 1;
                    foreach (APIAttendance attendanceexport in ilttrainingattendance)
                    {
                        worksheet.Cells[row1, column1++].Value = Security.Decrypt(attendanceexport.UserId);
                        worksheet.Cells[row1, column1++].Value = attendanceexport.UserName;
                        worksheet.Cells[row1, column1++].Value = Security.Decrypt(attendanceexport.EmailId);
                        worksheet.Cells[row1, column1++].Value = Security.Decrypt(attendanceexport.MobileNumber);
                        worksheet.Cells[row1, column1++].Value = attendanceexport.AttendanceDate;
                        worksheet.Cells[row1, column1++].Value = attendanceexport.AttendanceStatus;// == true?"Yes":"No";


                        column1 = 1;
                        row1++;
                    }
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;

                    }
                    using (var rngitems = worksheet.Cells["A5:G5"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;

                    }
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


        [HttpPost("GetDetailsForUserAttendance")]
        public async Task<IActionResult> GetDetailsForUserAttendance([FromBody] APIGetDetailsForUserAttendance objGetDetailsForUserAttendance)
        {
            try
            {
                return Ok(await this._IILTTrainingAttendance.GetDetailsForUserAttendance(objGetDetailsForUserAttendance));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetApplicationDateFormat")]
        public async Task<IActionResult> GetApplicationDateFormat()
        {
            try
            {
                var result = await this._IILTTrainingAttendance.GetApplicationDateFormat(OrganisationCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}