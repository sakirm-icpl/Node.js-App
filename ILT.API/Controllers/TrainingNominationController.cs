using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Helper.Metadata;
using ILT.API.Model;
using ILT.API.Repositories.Interfaces;
using ILT.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
using ILT.API.Model.Log_API_Count;

namespace ILT.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/TrainingNomination")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class TrainingNominationController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TrainingNominationController));
        ITrainingNomination _ITrainingNomination;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokensRepository _tokensRepository;
        ICourseRepository _courseRepository;
        public IConfiguration _configuration { get; }
        public TrainingNominationController(ITrainingNomination ITrainingNomination,
            IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor,
            IIdentityService identitySvc,
            IConfiguration configure, ITokensRepository tokensRepository,
            ICourseRepository courseRepository) : base(identitySvc)
        {
            _ITrainingNomination = ITrainingNomination;
            this.hostingEnvironment = environment;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configure;
            this._tokensRepository = tokensRepository;
            _courseRepository = courseRepository;
        }


        [HttpGet("{page}/{pageSize}/{searchParameter?}/{searchText?}")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> Get(int page, int pageSize, string searchParameter = null, string searchText = null)
        {
            try
            {
                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._ITrainingNomination.GetAllActiveSchedules(page, pageSize, OrganisationCode, UserId, searchParameter, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("Count/{searchParameter?}/{searchText?}/{showAllData?}")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetCount(string searchParameter = null, string searchText = null,bool showAllData=false)
        {
            try
            {
                if (searchParameter != null)
                    searchParameter = searchParameter.ToLower().Equals("null") ? null : searchParameter;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._ITrainingNomination.GetAllActiveSchedulesCount(OrganisationCode, UserId, searchParameter, searchText, showAllData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await _ITrainingNomination.GetByID(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByModuleId/{ModuleId}/{CourseId}")]
        public async Task<IActionResult> GetByModuleId(int ModuleId, int? CourseId)
        {
            try
            {
                return Ok(await this._ITrainingNomination.GetByModuleId(ModuleId, CourseId, UserId, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetNominateUserDetails")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetNominateUserDetails([FromBody] APIGetTrainingNomination objGetTrainingNomination)
        {
            try
            {
                if (objGetTrainingNomination.search != null)
                    objGetTrainingNomination.search = objGetTrainingNomination.search.ToLower().Equals("null") ? null : objGetTrainingNomination.search;
                if (objGetTrainingNomination.searchText != null)
                    objGetTrainingNomination.searchText = objGetTrainingNomination.searchText.ToLower().Equals("null") ? null : objGetTrainingNomination.searchText;
                return Ok(await this._ITrainingNomination.GetNominateUserDetails(objGetTrainingNomination.scheduleID, objGetTrainingNomination.courseId, objGetTrainingNomination.page, objGetTrainingNomination.pageSize, objGetTrainingNomination.search, objGetTrainingNomination.searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetNominateUserCount")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetNominateUserCount([FromBody] APIGetTrainingNomination objGetTrainingNomination)
        {
            try
            {
                if (objGetTrainingNomination.search != null)
                    objGetTrainingNomination.search = objGetTrainingNomination.search.ToLower().Equals("null") ? null : objGetTrainingNomination.search;
                if (objGetTrainingNomination.searchText != null)
                    objGetTrainingNomination.searchText = objGetTrainingNomination.searchText.ToLower().Equals("null") ? null : objGetTrainingNomination.searchText;
                return Ok(await this._ITrainingNomination.GetNominateUserCount(objGetTrainingNomination.scheduleID, objGetTrainingNomination.courseId, objGetTrainingNomination.search, objGetTrainingNomination.searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUsersForNomination")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetUsersForNomination([FromBody] APIGetTrainingNomination objGetTrainingNomination)
        {
            try
            {
                if (objGetTrainingNomination.search != null)
                    objGetTrainingNomination.search = objGetTrainingNomination.search.ToLower().Equals("null") ? null : objGetTrainingNomination.search;
                if (objGetTrainingNomination.searchText != null)
                    objGetTrainingNomination.searchText = objGetTrainingNomination.searchText.ToLower().Equals("null") ? null : objGetTrainingNomination.searchText;

                return Ok(await this._ITrainingNomination.getAllUsersForSectionalAdmin(objGetTrainingNomination.scheduleID, objGetTrainingNomination.courseId, objGetTrainingNomination.moduleId, UserId, objGetTrainingNomination.page, objGetTrainingNomination.pageSize, OrganisationCode.ToLower(), objGetTrainingNomination.search, objGetTrainingNomination.searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUsersCountForNomination")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetUsersCountForNomination([FromBody] APIGetTrainingNomination objGetTrainingNomination)
        {
            try
            {
                if (objGetTrainingNomination.search != null)
                    objGetTrainingNomination.search = objGetTrainingNomination.search.ToLower().Equals("null") ? null : objGetTrainingNomination.search;
                if (objGetTrainingNomination.searchText != null)
                    objGetTrainingNomination.searchText = objGetTrainingNomination.searchText.ToLower().Equals("null") ? null : objGetTrainingNomination.searchText;

                return Ok(await this._ITrainingNomination.GetUsersCountForSectionalAdmin(objGetTrainingNomination.scheduleID, objGetTrainingNomination.courseId, objGetTrainingNomination.moduleId, UserId, OrganisationCode, objGetTrainingNomination.search, objGetTrainingNomination.searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseNameTypeAhead/{search?}")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetCourseName(string search = null)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._ITrainingNomination.GetCourseName(search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetRoleCourseNameTypeAhead/{search?}")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetRoleCourseName(string search = null)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._ITrainingNomination.GetRoleCourseName(UserId,RoleCode,search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetRoleWiseCourseTypeAhead/{search?}")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetRoleAllCourseName(string search = null)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._ITrainingNomination.GetRoleAllCourseName(UserId, RoleCode, search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetScheduleTrainerCourses/{search?}")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetScheduleTrainerCourses(string search = null)
        {
            try
            {
                ApiResponse result = new ApiResponse();
                result = await this._ITrainingNomination.GetScheduleTrainerCourses(UserId, RoleCode, search);
                return Ok(result.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("NominateUser/{id}/{moduleId}/{courseId}/{BatchId}")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> NominateUser(int id, int moduleId, int courseId, [FromBody] List<APIUserData> userID, int BatchId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ApiResponse checkValidData = await _ITrainingNomination.checkValidData(id, moduleId, courseId);
                if (checkValidData.StatusCode == 410 || checkValidData.StatusCode == 411)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = checkValidData.Description });

                ApiResponseILT result = await _ITrainingNomination.PostNominateUser(id, moduleId, courseId, userID, UserId, OrganisationCode, BatchId);
                if (result.StatusCode == 200)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("DeleteUserNomination")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> Delete([FromBody] APIDeleteUserNomination aPIDeleteUserNomination)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int userId = Convert.ToInt32(Security.DecryptForUI(aPIDeleteUserNomination.UserIdEncrypted));

                ApiResponse result = await _ITrainingNomination.DeleteNominateUsers(aPIDeleteUserNomination.moduleId, aPIDeleteUserNomination.courseId, aPIDeleteUserNomination.scheduleID, userId, OrgCode);
                if (result.StatusCode == 505)
                    return StatusCode(505, "You can't delete the Nomination as attendance for this schedule is available");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #region BulkUpload TrainingNomination

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFile()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"TrainingNomination.xlsx";
                string DomainName = this._configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrganisationCode);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Training Nomination");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Course Code*";
                    if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                    {
                        worksheet.Cells[1, 2].Value = "Batch Code*";
                        worksheet.Cells[1, 3].Value = "Module Name";
                        worksheet.Cells[1, 4].Value = "Schedule Code";
                        worksheet.Cells[1, 5].Value = "UserId*";
                    }
                    else
                    {
                        worksheet.Cells[1, 2].Value = "Module Name*";
                        worksheet.Cells[1, 3].Value = "Schedule Code*";
                        worksheet.Cells[1, 4].Value = "UserId*";
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

        [HttpPost("PostFileUpload")]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                _ITrainingNomination.Delete();
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


        [HttpPost("SaveFileData")]
        public async Task<IActionResult> PostFile([FromBody] APITrainingNominationPath aPITrainingNominationPath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(this.ModelState);

                ApiResponse response = await this._ITrainingNomination.ProcessImportFile(aPITrainingNominationPath, UserId, OrganisationCode);
                return Ok(response.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }
        #endregion

        [HttpGet("GetScheduleByModuleId/{id}")]
        public async Task<IActionResult> GetScheduleByModuleId(int id)
        {
            try
            {
                return Ok(await this._ITrainingNomination.GetScheduleByModuleId(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetScheduleByModuleId_Attendance/{ModuleId}/{CourseId}")]
        public async Task<IActionResult> GetScheduleByModuleId_AttendanceReport(int ModuleId, int? CourseId)
        {
            try
            {
                return Ok(await this._ITrainingNomination.GetScheduleByModuleId_AttendanceReport(ModuleId, CourseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("Export")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> Export([FromBody] APINominatedUsersForExport nomination)
        {
            try
            {
                var export = await this._ITrainingNomination.GetNominatedWiseReport(nomination);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"Nomination.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrganisationCode);
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Nomination");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Course Code";
                    worksheet.Cells[1, 2].Value = "Course Name";
                    if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                    {
                        worksheet.Cells[1, 3].Value = "Batch Code";
                        worksheet.Cells[1, 4].Value = "Batch Name";
                        worksheet.Cells[1, 5].Value = "Module Name";
                        worksheet.Cells[1, 6].Value = "Place Name";
                        worksheet.Cells[1, 7].Value = "Schedule Code";
                        worksheet.Cells[1, 8].Value = "Start Date";
                        worksheet.Cells[1, 9].Value = "End Date";
                        worksheet.Cells[1, 10].Value = "Start Time";
                        worksheet.Cells[1, 11].Value = "End Time";
                    }
                    else
                    {
                        worksheet.Cells[1, 3].Value = "Module Name";
                        worksheet.Cells[1, 4].Value = "Place Name";
                        worksheet.Cells[1, 5].Value = "Schedule Code";
                        worksheet.Cells[1, 6].Value = "Start Date";
                        worksheet.Cells[1, 7].Value = "End Date";
                        worksheet.Cells[1, 8].Value = "Start Time";
                        worksheet.Cells[1, 9].Value = "End Time";
                    }
                    
                    int row = 2, column = 1;
                    DateTime dateValue = new DateTime();
                    DateTime outputDateTimeValue;
                    foreach (APINominatedUsersForExport nominated in export)
                    {
                        worksheet.Cells[row, column++].Value = nominated.Code;

                        worksheet.Cells[row, column++].Value = nominated.CourseName;
                        if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                        {
                            worksheet.Cells[row, column++].Value = nominated.BatchCode;
                            worksheet.Cells[row, column++].Value = nominated.BatchName;
                        }
                        worksheet.Cells[row, column++].Value = nominated.ModuleName;
                        worksheet.Cells[row, column++].Value = nominated.PlaceName;
                        worksheet.Cells[row, column++].Value = nominated.ScheduleCode;
                        if (DateTime.TryParse(nominated.StartDate.ToString(), out outputDateTimeValue))
                        {
                            dateValue = outputDateTimeValue;

                            if (dateValue == DateTime.MinValue)
                                worksheet.Cells[row, column++].Value = string.Empty;
                            else
                                worksheet.Cells[row, column++].Value = dateValue.ToString("MMM dd, yyyy").ToString();
                        }
                        else
                        {
                            worksheet.Cells[row, column++].Value = string.Empty;
                        }
                        if (DateTime.TryParse(nominated.EndDate.ToString(), out outputDateTimeValue))
                        {
                            dateValue = outputDateTimeValue;

                            if (dateValue == DateTime.MinValue)
                                worksheet.Cells[row, column++].Value = string.Empty;
                            else
                                worksheet.Cells[row, column++].Value = dateValue.ToString("MMM dd, yyyy").ToString();
                        }
                        else
                        {
                            worksheet.Cells[row, column++].Value = string.Empty;
                        }
                        if (DateTime.TryParse(nominated.StartTime.ToString(), out outputDateTimeValue))
                        {
                            dateValue = outputDateTimeValue;

                            if (dateValue == DateTime.MinValue)
                                worksheet.Cells[row, column++].Value = string.Empty;
                            else
                                worksheet.Cells[row, column++].Value = dateValue.ToString("hh:mm tt").ToString();
                        }
                        else
                        {
                            worksheet.Cells[row, column++].Value = string.Empty;
                        }
                        if (DateTime.TryParse(nominated.EndTime.ToString(), out outputDateTimeValue))
                        {
                            dateValue = outputDateTimeValue;

                            if (dateValue == DateTime.MinValue)
                                worksheet.Cells[row, column++].Value = string.Empty;
                            else
                                worksheet.Cells[row, column++].Value = dateValue.ToString("hh:mm tt").ToString();
                        }
                        else
                        {
                            worksheet.Cells[row, column++].Value = string.Empty;
                        }

                        if (row == 2)
                            break;
                    }
                    //  empty row
                    worksheet.Cells[row, column++].Value = "\n";
                    worksheet.Cells[4, 2].Value = "\nUser Nomination Details\n";
                    worksheet.Cells[4, 2].Style.Font.Bold = true;
                    // add user Data
                    worksheet.Cells[5, 1].Value = "User Name";
                    worksheet.Cells[5, 2].Value = "User Id";
                    worksheet.Cells[5, 3].Value = "Email Id";
                    worksheet.Cells[5, 4].Value = "Mobile Number";

                    int row2 = 6, column2 = 1;

                    foreach (APINominatedUsersForExport nominations in export)
                    {
                        worksheet.Cells[row2, column2++].Value = nominations.UserName;
                        worksheet.Cells[row2, column2++].Value = Security.Decrypt(nominations.UserId);
                        worksheet.Cells[row2, column2++].Value = Security.Decrypt(nominations.EmailId);
                        worksheet.Cells[row2, column2++].Value = Security.Decrypt(nominations.MobileNumber);

                        column2 = 1;
                        row2++;
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


        [HttpGet("GetAllTrainingNominationReject/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetAllTrainingNominationReject(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<APITrainingNominationRejected> trainingNominationRejected = await this._ITrainingNomination.GetAllTrainingNominationReject(page, pageSize, search);
                return Ok(trainingNominationRejected);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ExportTrainingNominationReject/{search?}")]
        public async Task<IActionResult> ExportTrainingNominationReject(string search = null)
        {
            try
            {
                FileInfo ExcelFile;
                ExcelFile = await this._ITrainingNomination.ExportTrainingNominationReject(search);

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(data, FileContentType.Excel, FileName.TrainingNominationRejected);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TrainingNominationReject/GetTotalRecords/{search:minlength(0)?}")]

        public async Task<IActionResult> GetCountForTrainingNomination(string search)
        {
            try
            {
                int trainingnominationrejected = await this._ITrainingNomination.Count(search);
                return Ok(trainingnominationrejected);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetNominationData")]
        [PermissionRequired(Permissions.iltnominate)]
        public async Task<IActionResult> GetV2([FromBody] ApiNominationGet apiNominationGet)
        {
            try
            {
                if (apiNominationGet.searchParameter != null)
                    apiNominationGet.searchParameter = apiNominationGet.searchParameter.ToLower().Equals("null") ? null : apiNominationGet.searchParameter;
                if (apiNominationGet.searchText != null)
                    apiNominationGet.searchText = apiNominationGet.searchText.ToLower().Equals("null") ? null : apiNominationGet.searchText;
                return Ok(await this._ITrainingNomination.GetAllActiveSchedulesV2(apiNominationGet, OrganisationCode, UserId ));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}




