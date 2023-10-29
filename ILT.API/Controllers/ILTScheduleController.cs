using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using ILT.API.APIModel;

using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Helper.Metadata;
using ILT.API.Model.ILT;
using ILT.API.Repositories.Interfaces;
//using ILT.API.Repositories.Interfaces.ILT;
using ILT.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static ILT.API.Common.AuthorizePermissions;
using static ILT.API.Common.TokenPermissions;
using log4net;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using ILT.API.ExternalIntegration.EdCast;
//using Google.Apis.Calendar.v3;
//using Google.Apis.Services;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Calendar.v3.Data;
//using System.Threading;
//using Google.Apis.Util.Store;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using ILT.API.Model.Log_API_Count;
using ILT.API.Model;

namespace ILT.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/ILTSchedule")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ILTScheduleController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTScheduleController));
        IILTSchedule _IILTSchedule;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IConfiguration _configuration { get; }
        private readonly ITokensRepository _tokensRepository;
        private readonly ICourseRepository _courseRepository;
        IAzureStorage _azurestorage;

        //static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        //static string ApplicationName = "GSuitDemo";


        public ILTScheduleController(IILTSchedule IILTSchedule, IEmail email, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor IHttpContextAccessor,
                                     IIdentityService identitySvc, IConfiguration configuration, ITokensRepository tokensRepository, IAzureStorage azurestorage,
                                     ICourseRepository courseRepository) : base(identitySvc)
        {
            this._azurestorage = azurestorage;
            _IILTSchedule = IILTSchedule;
            _httpContextAccessor = IHttpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            this._tokensRepository = tokensRepository;
            _courseRepository = courseRepository;
        }

        [HttpGet("{page}/{pageSize}/{search?}/{searchText?}")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string searchText = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTSchedule.GetAllActiveSchedules(page, pageSize, UserId, OrganisationCode, null, search, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Count/{search?}/{searchText?}/{showAllData?}")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> GetCount(string search = null, string searchText = null,bool showAllData=false)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._IILTSchedule.GetAllActiveSchedulesCount(UserId, OrganisationCode, search, searchText, showAllData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("GetScheduleDetailsByID")]
        public async Task<IActionResult> Get([FromBody] APIScheduleID objScheduleID)
        {
            try
            {
                return Ok(await _IILTSchedule.GetByID(objScheduleID.scheduleID));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetByModuleId/{id}")]
        public async Task<IActionResult> GetByModuleID(int id)
        {
            try
            {
                return Ok(await _IILTSchedule.GetByModuleID(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAcademyTypeAhead/{trainerType}/{search?}")]
        public async Task<IActionResult> GetAcademyTypeAhead(string trainerType, string search = null)
        {
            try
            {
                return Ok(await _IILTSchedule.GetAcademyTypeAhead(trainerType, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAcademyData/{trainerType}")]
        public async Task<IActionResult> GetAcademyData(string trainerType)
        {
            try
            {
                return Ok(await _IILTSchedule.GetAcademyData(trainerType));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("ScheduleCode")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> GetScheduleCode()
        {
            try
            {
                ScheduleCode ScheduleCode = await _IILTSchedule.GetScheduleCode(UserId);
                string Code = "SC" + ScheduleCode.Id;
                return Ok(Code);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("CancelScheduleCode")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> CancelScheduleCode([FromBody] APIScheduleCode aPIScheduleCode)
        {
            try
            {
                if(!ModelState.IsValid)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                await _IILTSchedule.CancelScheduleCode(aPIScheduleCode, UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("CheckModuleName/{moduleName}")]
        public async Task<IActionResult> CheckModuleName(string moduleName)
        {
            try
            {
                bool result = await _IILTSchedule.CheckModuleName(moduleName);
                if (result == false)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "Invalid Module" });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> Post([FromBody] APIILTSchedular aPIILTSchedular)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int DateDiff = Convert.ToInt32((aPIILTSchedular.EndDate.Date - aPIILTSchedular.StartDate.Date).TotalDays);
                if (DateDiff < 0)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "Start date must be less than end date." });

                if (aPIILTSchedular.PlaceID == 0 && aPIILTSchedular.WebinarType == null)
                {
                    if (aPIILTSchedular.PlaceName == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    if (_IILTSchedule.ValidateSQLInjection(aPIILTSchedular))
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    if (Convert.ToDouble(aPIILTSchedular.SeatCapacity) > 10000)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "Seat capacity must be less than equal to 10000." });
                }

                if (aPIILTSchedular.ModuleId == 0)
                {
                    if (aPIILTSchedular.ModuleName == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (aPIILTSchedular.AcademyAgencyID == 0)
                {
                    if (aPIILTSchedular.AcademyAgencyName == "")
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                int TotalDays = 0;
                int TotalHolidays = 0;

                TotalDays = aPIILTSchedular.HolidayList.Count();

                foreach (HolidayList holidayList in aPIILTSchedular.HolidayList)
                {
                    if (holidayList.IsHoliday == true)
                        TotalHolidays = TotalHolidays + 1;
                }

                if (TotalDays == TotalHolidays)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "All holidays are not allowed." });

                foreach (HolidayList holiday in aPIILTSchedular.HolidayList)
                {
                    if (holiday.IsHoliday == true)
                    {
                        if (holiday.Reason == null)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "For holiday reason must be compulsary." });
                    }
                }

                ApiResponse result = await _IILTSchedule.PostILT(aPIILTSchedular, UserId, OrganisationCode);
                if (result.StatusCode == 200)
                    return Ok(result);
                else
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }

        [HttpPost("UpdateILTSchedule")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> Put([FromBody] APIILTSchedular aPIILTSchedular)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int DateDiff = Convert.ToInt32((aPIILTSchedular.EndDate.Date - aPIILTSchedular.StartDate.Date).TotalDays);
                if (DateDiff < 0)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "Start date must be less than end date." });

                if (aPIILTSchedular.PlaceID == 0 && aPIILTSchedular.WebinarType == null)
                {
                    if (aPIILTSchedular.PlaceName == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    if (_IILTSchedule.ValidateSQLInjection(aPIILTSchedular))
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    if (Convert.ToDouble(aPIILTSchedular.SeatCapacity) > 10000)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "Seat capacity must be less than equal to 10000." });
                }

                if (aPIILTSchedular.ModuleId == 0)
                {
                    if (aPIILTSchedular.ModuleName == null)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (aPIILTSchedular.AcademyAgencyID == 0)
                {
                    if (aPIILTSchedular.AcademyAgencyName == "")
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                int TotalDays = 0;
                int TotalHolidays = 0;

                TotalDays = aPIILTSchedular.HolidayList.Count();

                foreach (HolidayList holidayList in aPIILTSchedular.HolidayList)
                {
                    if (holidayList.IsHoliday == true)
                        TotalHolidays = TotalHolidays + 1;
                }

                if (TotalDays == TotalHolidays)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "All holidays are not allowed." });

                foreach (HolidayList holiday in aPIILTSchedular.HolidayList)
                {
                    if (holiday.IsHoliday == true)
                    {
                        if (holiday.Reason == null)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "For holiday reason must be compulsary." });
                    }
                }

                ApiResponse result = await _IILTSchedule.PutILT(aPIILTSchedular, UserId, OrganisationCode);
                if (result.StatusCode == 200)
                    return Ok(result);
                else
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = result.Description });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }
        [HttpGet("ExportFormat")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> ExportImportFormat()
        {
            try
            {
                var result = await _IILTSchedule.ExportImportFormat(OrganisationCode);
                Response.ContentType = FileContentType.Excel;
                return File(result, FileContentType.Excel, FileName.ILTScheduleImportFormat);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SaveFileData")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> PostFile([FromBody] APIILTScheduleImport aPIILTScheduleImport)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ApiResponse response = await this._IILTSchedule.ProcessImportFile(aPIILTScheduleImport, UserId, OrganisationCode);
                return Ok(response.ResponseObject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }
        [HttpGet("ExportRejected")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> ExportRejected()
        {
            try
            {
                FileInfo ExcelFile;
                ExcelFile = await this._IILTSchedule.ExportILTScheduleReject(OrganisationCode);

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(data, FileContentType.Excel, FileName.ILTBatchRejected);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("testZoom/{code}")]
        public async Task<IActionResult> zoomcreate(string code)
        {
            try
            {
                var result = await _IILTSchedule.CreateZoomMeeting(code);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return NoContent();
            }
        }

        [HttpGet("GetZoomConfiguration")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> GetZoomConfiguration()
        {
            try
            {
                var result = await _IILTSchedule.GetZoomConfiguration();
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpPost("{CancellationSchedule}")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> CancellationSchedule([FromBody] ScheduleCancellation obj)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int ScheduleID =  Convert.ToInt32(Security.DecryptForUI(obj.ScheduleID));
                ILTSchedule objILTSchedule = await this._IILTSchedule.Get(ScheduleID);
                if (objILTSchedule == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                int result = await _IILTSchedule.CancellationSchedule(ScheduleID, obj.Reason, OrganisationCode);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UploadLogo")]
        [PermissionRequired(Permissions.iltshedules)]

        public async Task<IActionResult> UploadLogo([FromBody] APIPictureProfile pictureProfile)

        {
            try
            {
                if (string.IsNullOrEmpty(pictureProfile.Base64String))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                string[] str = pictureProfile.Base64String.Split(',');
                var bytes = Convert.FromBase64String(str[1]);
                var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                _logger.Info("pdf EnableBlobStorage : " + EnableBlobStorage);

                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                { 
                    string ApiGatewayPath = this._configuration["ApiGatewayWwwroot"];
                    ApiGatewayPath = Path.Combine(ApiGatewayPath, OrganisationCode);
                    string fileDir = Path.Combine(ApiGatewayPath, "iltlogo");
                    if (!Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }
                    string file = Path.Combine(fileDir, DateTime.UtcNow.Ticks + ".png");
                    if (bytes.Length > 0)
                    {
                        using (var stream = new FileStream(file, FileMode.Create))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Flush();
                        }
                    }

                    if (string.IsNullOrEmpty(file))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    file = file.Substring(file.LastIndexOf(OrganisationCode)).Replace(@"\", "/");
                    file = file.Replace(@"\""", "");
                    return this.Ok(file);
                }
                else
               {
                    BlobResponseDto res = await _azurestorage.CreateBlob(bytes, OrgCode, null, "iltlogo");
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            string filePath = res.Blob.Name.ToString();
                            return this.Ok(filePath.Replace(@"\", "/"));
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{Id}")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> Put(int Id, [FromBody] APIILTSchedular aPIILTSchedular)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ILTSchedule objILTSchedule = await this._IILTSchedule.Get(Id);
                if (objILTSchedule == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                objILTSchedule = Mapper.Map<ILTSchedule>(aPIILTSchedular);
                objILTSchedule.ID = Id;
                objILTSchedule.ModifiedBy = UserId;
                objILTSchedule.ModifiedDate = DateTime.UtcNow;
                objILTSchedule.IsDeleted = false;

                await this._IILTSchedule.Update(objILTSchedule);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                ILTSchedule objILTSchedule = await this._IILTSchedule.Get(DecryptedId);
                if (objILTSchedule == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                objILTSchedule.IsDeleted = true;

                await this._IILTSchedule.Update(objILTSchedule);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet]
        [Route("Export/{showAllData?}")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> Export(bool showAllData = false)
        {
            try
            {
                var schedules = await this._IILTSchedule.GetAllSchedulesForExport(UserId, OrganisationCode,showAllData);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "Schedule.xlsx";
                string URL = string.Concat(DomainName, "/", sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                string batchwiseNomination = await _courseRepository.GetConfigurationValueAsync("ENABLE_BATCHWISE_NOMINATION", OrganisationCode);
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    if (!string.Equals(OrganisationCode.ToLower(), "sbil"))
                    {
                        // add a new worksheet to the empty workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ILT Schedules");
                        //First add the headers
                        worksheet.Cells[1, 1].Value = "ScheduleCode";
                        if(!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower()=="yes")
                        {
                            worksheet.Cells[1, 2].Value = "CourseName";
                            worksheet.Cells[1, 3].Value = "CourseCode";
                            worksheet.Cells[1, 4].Value = "BatchCode";
                            worksheet.Cells[1, 5].Value = "BatchName";
                            worksheet.Cells[1, 6].Value = "ModuleName";
                            worksheet.Cells[1, 7].Value = "StartDate";
                            worksheet.Cells[1, 8].Value = "EndDate";
                            worksheet.Cells[1, 9].Value = "StartTime";
                            worksheet.Cells[1, 10].Value = "EndTime";
                            worksheet.Cells[1, 11].Value = "AcademyTrainerID";
                            worksheet.Cells[1, 12].Value = "AcademyTrainerName";
                            worksheet.Cells[1, 13].Value = "TrainerType";
                            worksheet.Cells[1, 14].Value = "City";
                            worksheet.Cells[1, 15].Value = "ContactPersonName";
                            worksheet.Cells[1, 16].Value = "Cost";
                            worksheet.Cells[1, 17].Value = "Currency";
                            worksheet.Cells[1, 18].Value = "PlaceName";
                            worksheet.Cells[1, 19].Value = "Schedule Type";
                            worksheet.Cells[1, 20].Value = "Last Modified";
                            worksheet.Cells[1, 21].Value = "Nomination Count";

                            if (string.Equals(OrganisationCode.ToLower(), "sbil"))
                                worksheet.Cells[1, 22].Value = "Region";

                            if (string.Equals(OrganisationCode.ToLower(), "sbig"))
                                worksheet.Cells[1, 23].Value = "TopicName";
                        }
                        else
                        {
                            worksheet.Cells[1, 2].Value = "CourseName";
                            worksheet.Cells[1, 3].Value = "CourseCode";
                            worksheet.Cells[1, 4].Value = "ModuleName";
                            worksheet.Cells[1, 5].Value = "StartDate";
                            worksheet.Cells[1, 6].Value = "EndDate";
                            worksheet.Cells[1, 7].Value = "StartTime";
                            worksheet.Cells[1, 8].Value = "EndTime";
                            worksheet.Cells[1, 9].Value = "AcademyTrainerID";
                            worksheet.Cells[1, 10].Value = "AcademyTrainerName";
                            worksheet.Cells[1, 11].Value = "TrainerType";
                            worksheet.Cells[1, 12].Value = "City";
                            worksheet.Cells[1, 13].Value = "ContactPersonName";
                            worksheet.Cells[1, 14].Value = "Cost";
                            worksheet.Cells[1, 15].Value = "Currency";
                            worksheet.Cells[1, 16].Value = "PlaceName";
                            worksheet.Cells[1, 17].Value = "Schedule Type";
                            worksheet.Cells[1, 18].Value = "Last Modified";
                            worksheet.Cells[1, 19].Value = "Nomination Count";

                            if (string.Equals(OrganisationCode.ToLower(), "sbil"))
                                worksheet.Cells[1, 20].Value = "Region";

                            if (string.Equals(OrganisationCode.ToLower(), "sbig"))
                                worksheet.Cells[1, 21].Value = "TopicName";
                        }
                        

                        int row = 2, column = 1;
                        foreach (APIILTSchedularExport schedule in schedules)
                        {
                            DateTime DateValue = new DateTime();

                            worksheet.Cells[row, column++].Value = schedule.ScheduleCode;
                            worksheet.Cells[row, column++].Value = schedule.CourseName;
                            worksheet.Cells[row, column++].Value = schedule.CourseCode;
                            if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                            {
                                worksheet.Cells[row, column++].Value = schedule.BatchCode;
                                worksheet.Cells[row, column++].Value = schedule.BatchName;
                            }
                            worksheet.Cells[row, column++].Value = schedule.ModuleName;
                            if (schedule.StartDate != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.StartDate);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("dd MMMM yyyy");
                            }
                            if (schedule.EndDate != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.EndDate);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("dd MMMM yyyy");
                            }
                            if (schedule.StartTime != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.StartTime);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("HH:mm");
                            }
                            if (schedule.EndTime != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.EndTime);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("HH:mm");
                            }
                            worksheet.Cells[row, column++].Value = schedule.AcademyTrainerID;
                            worksheet.Cells[row, column++].Value = schedule.AcademyTrainerName;
                            worksheet.Cells[row, column++].Value = schedule.TrainerType;

                            worksheet.Cells[row, column++].Value = schedule.City;
                            worksheet.Cells[row, column++].Value = schedule.ContactPersonName;
                            worksheet.Cells[row, column++].Value = schedule.Cost;
                            worksheet.Cells[row, column++].Value = schedule.Currency;
                            worksheet.Cells[row, column++].Value = schedule.PlaceName;
                            worksheet.Cells[row, column++].Value = schedule.ScheduleType;
                            worksheet.Cells[row, column++].Value = schedule.LastModified;
                            worksheet.Cells[row, column++].Value = schedule.NominationCount;

                            if (string.Equals(OrganisationCode.ToLower(), "sbil"))
                                worksheet.Cells[row, column++].Value = schedule.Region;

                            if (string.Equals(OrganisationCode.ToLower(), "sbig"))
                                worksheet.Cells[row, column++].Value = schedule.TopicName;

                            row++;
                            column = 1;
                        }
                        using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;

                        }

                    }

                    else
                    {
                        // add a new worksheet to the empty workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ILT Schedules");
                        //First add the headers
                        worksheet.Cells[1, 1].Value = "ScheduleCode";
                        if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                        {
                            worksheet.Cells[1, 2].Value = "CourseName";
                            worksheet.Cells[1, 3].Value = "CourseCode";
                            worksheet.Cells[1, 4].Value = "BatchCode";
                            worksheet.Cells[1, 5].Value = "BatchName";
                            worksheet.Cells[1, 6].Value = "ModuleName";
                            worksheet.Cells[1, 7].Value = "StartDate";
                            worksheet.Cells[1, 8].Value = "EndDate";
                            worksheet.Cells[1, 9].Value = "StartTime";
                            worksheet.Cells[1, 10].Value = "EndTime";
                            worksheet.Cells[1, 11].Value = "AcademyTrainerID";
                            worksheet.Cells[1, 12].Value = "AcademyTrainerName";
                            worksheet.Cells[1, 13].Value = "ConductedBy";
                            worksheet.Cells[1, 14].Value = "Department Of Trainer";
                            worksheet.Cells[1, 15].Value = "SubFunction Of Trainer";
                            worksheet.Cells[1, 16].Value = "City";
                            worksheet.Cells[1, 17].Value = "ContactPersonName";
                            worksheet.Cells[1, 18].Value = "Cost";
                            worksheet.Cells[1, 19].Value = "Currency";
                            worksheet.Cells[1, 20].Value = "PlaceName";

                            if (string.Equals(OrganisationCode.ToLower(), "sbil"))
                                worksheet.Cells[1, 21].Value = "Region";

                            if (string.Equals(OrganisationCode.ToLower(), "sbig"))
                                worksheet.Cells[1, 22].Value = "TopicName";
                        }
                        else
                        {
                            worksheet.Cells[1, 2].Value = "CourseName";
                            worksheet.Cells[1, 3].Value = "CourseCode";
                            worksheet.Cells[1, 4].Value = "ModuleName";
                            worksheet.Cells[1, 5].Value = "StartDate";
                            worksheet.Cells[1, 6].Value = "EndDate";
                            worksheet.Cells[1, 7].Value = "StartTime";
                            worksheet.Cells[1, 8].Value = "EndTime";
                            worksheet.Cells[1, 9].Value = "AcademyTrainerID";
                            worksheet.Cells[1, 10].Value = "AcademyTrainerName";
                            worksheet.Cells[1, 11].Value = "ConductedBy";
                            worksheet.Cells[1, 12].Value = "Department Of Trainer";
                            worksheet.Cells[1, 13].Value = "SubFunction Of Trainer";
                            worksheet.Cells[1, 14].Value = "City";
                            worksheet.Cells[1, 15].Value = "ContactPersonName";
                            worksheet.Cells[1, 16].Value = "Cost";
                            worksheet.Cells[1, 17].Value = "Currency";
                            worksheet.Cells[1, 18].Value = "PlaceName";

                            if (string.Equals(OrganisationCode.ToLower(), "sbil"))
                                worksheet.Cells[1, 19].Value = "Region";

                            if (string.Equals(OrganisationCode.ToLower(), "sbig"))
                                worksheet.Cells[1, 20].Value = "TopicName";
                        }
                        

                        int row = 2, column = 1;
                        foreach (APIILTSchedularExport schedule in schedules)
                        {
                            DateTime DateValue = new DateTime();

                            worksheet.Cells[row, column++].Value = schedule.ScheduleCode;
                            worksheet.Cells[row, column++].Value = schedule.CourseName;
                            worksheet.Cells[row, column++].Value = schedule.CourseCode;
                            if (!string.IsNullOrEmpty(batchwiseNomination) && batchwiseNomination.ToLower() == "yes")
                            {
                                worksheet.Cells[row, column++].Value = schedule.BatchCode;
                                worksheet.Cells[row, column++].Value = schedule.BatchName;
                            }
                            worksheet.Cells[row, column++].Value = schedule.ModuleName;
                            if (schedule.StartDate != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.StartDate);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("dd MMMM yyyy");
                            }
                            if (schedule.EndDate != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.EndDate);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("dd MMMM yyyy");
                            }
                            if (schedule.StartTime != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.StartTime);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("HH:mm");
                            }
                            if (schedule.EndTime != null)
                            {
                                DateValue = Convert.ToDateTime(schedule.EndTime);
                                worksheet.Cells[row, column++].Value = DateValue.ToString("HH:mm");
                            }
                            worksheet.Cells[row, column++].Value = schedule.AcademyTrainerID;
                            worksheet.Cells[row, column++].Value = schedule.AcademyTrainerName;
                            worksheet.Cells[row, column++].Value = schedule.ConductedBy;
                            worksheet.Cells[row, column++].Value = schedule.DepartmentOfTrainer;
                            worksheet.Cells[row, column++].Value = schedule.SubFunctionOfTrainer;
                            worksheet.Cells[row, column++].Value = schedule.City;
                            worksheet.Cells[row, column++].Value = schedule.ContactPersonName;
                            worksheet.Cells[row, column++].Value = schedule.Cost;
                            worksheet.Cells[row, column++].Value = schedule.Currency;
                            worksheet.Cells[row, column++].Value = schedule.PlaceName;

                            if (string.Equals(OrganisationCode.ToLower(), "sbil"))
                                worksheet.Cells[row, column++].Value = schedule.Region;

                            if (string.Equals(OrganisationCode.ToLower(), "sbig"))
                                worksheet.Cells[row, column++].Value = schedule.TopicName;

                            row++;
                            column = 1;
                        }
                        using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                        {
                            rngitems.Style.Font.Bold = true;

                        }

                    }
                    package.Save();
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
            }
            return null;
        }

        [HttpPost("AddUserAsTrainer")]
        public async Task<IActionResult> AddUserAsTrainer([FromBody] APIUserAsTrainer aPIUserAsTrainer)
        {
            try
            {
                aPIUserAsTrainer.TrainerId = Security.DecryptForUI(aPIUserAsTrainer.TrainerId);
                bool flag = await this._IILTSchedule.AddUserAsTrainer(aPIUserAsTrainer, UserId);
                if (flag == false)
                {
                    return StatusCode(409, "Duplicate Record");
                }
                return StatusCode(200, "Record Saved");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUserAsTrainer")]
        public async Task<IActionResult> GetUserAsTrainer([FromBody] APIGetUserAsTrainer aPIGetUserAsTrainer)
        {
            try
            {
                if (aPIGetUserAsTrainer.search != null)
                    aPIGetUserAsTrainer.search = aPIGetUserAsTrainer.search.ToLower().Equals("null") ? null : aPIGetUserAsTrainer.search;
                return Ok(await this._IILTSchedule.GetUserAsTrainer(aPIGetUserAsTrainer));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCountUserAsTrainer")]
        public async Task<IActionResult> GetCountUserAsTrainer([FromBody] APIGetUserAsTrainer aPIGetUserAsTrainer)
        {
            try
            {
                if (aPIGetUserAsTrainer.search != null)
                    aPIGetUserAsTrainer.search = aPIGetUserAsTrainer.search.ToLower().Equals("null") ? null : aPIGetUserAsTrainer.search;
                return Ok(await this._IILTSchedule.GetUserAsTrainerCount(aPIGetUserAsTrainer));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("TeamsAccessToken")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> AddTeamsAccessToken([FromBody] TeamsAccessToken obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (obj.TeamsToken != null)
                {
                    TeamsAccessToken reso = await this._IILTSchedule.addUpdateTeamsAccessToken(obj);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #region "BigBlueButton Code"
        [HttpGet("StartBBBMeeting/{meetingID}")]
        public async Task<Response> StartBigBlueButtonMeeting(string meetingID)
        {
            try
            {
                return (await this._IILTSchedule.StartBigBlueMeeting(meetingID, UserId, UserName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new Response { StatusCode = HttpStatusCode.InternalServerError, Message = ex.Message };
            }
        }

        [HttpDelete("EndBBBMeeting")]
        public async Task<Response> EndBigBlueButtonMeeting([FromQuery]string meetingID)
        {
            try
            {
                string DecryptedmeetingID = Security.Decrypt(meetingID);
                return (await this._IILTSchedule.StartBigBlueMeeting(DecryptedmeetingID, UserId, UserName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new Response { StatusCode = HttpStatusCode.InternalServerError, Message = ex.Message };
            }
        }

        [HttpDelete("markbbbmeetingattendance")]
        public async Task<Response> MarkBigBlueButtonMeetingAttandance([FromQuery]string meetingID)
        {
            try
            {
                string DecryptedmeetingID = Security.Decrypt(meetingID);
                return (await this._IILTSchedule.StartBigBlueMeeting(DecryptedmeetingID, UserId, UserName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new Response { StatusCode = HttpStatusCode.InternalServerError, Message = ex.Message };
            }
        }
        #endregion
        [HttpPost("TeamsMeeting")]
        public async Task<IActionResult> TeamsMeeting([FromBody]Teams teams)
        {
            if (ModelState.IsValid)
            {
                List<TeamsScheduleDetails> teamsScheduleDetails = new List<TeamsScheduleDetails>();
                try
                {
                    teamsScheduleDetails = await _IILTSchedule.PostTeamsMeeting(teams, UserId, OrganisationCode);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return BadRequest(ex);
                }
                return Ok(teamsScheduleDetails);
            }
            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "Invalid Data Please Check" });
        }

        [HttpPost("GsuitMeeting")]
        public async Task<IActionResult> GsuitMeeting([FromBody] MeetDetails teams)
        {
            GoogleMeetRessponce googleMeetDetails = new GoogleMeetRessponce();
            try
            {
                googleMeetDetails = await _IILTSchedule.PostGsuitMeet(teams, UserId, OrganisationCode);
               

                if (googleMeetDetails == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Google Meeting Details Not Found" });
                }
                if (googleMeetDetails.meetDetails == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Google Meeting Details Not Found" });
                }
                if (googleMeetDetails.meetDetails.HangoutLink == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Google Meeting Join URL Not Found" });
                }
                if (googleMeetDetails.Id == 200)
                {
                    return Ok(googleMeetDetails.meetDetails);
                }
                else 
                {
                    return BadRequest (new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = googleMeetDetails.Status });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(ex);
            }
            return Ok(googleMeetDetails);
        }
        [HttpPost("UpdateGsuitMeeting")]
        public async Task<IActionResult> UpdateGsuitMeeting([FromBody] UpdateGsuitMeeting teams)
        {
            GoogleMeetRessponce googleMeetDetails = new GoogleMeetRessponce();
            try
            {
                googleMeetDetails = await _IILTSchedule.UpdateGsuitMeet(teams, UserId, OrganisationCode);


                if (googleMeetDetails == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Google Meeting Details Not Found" });
                }
                if (googleMeetDetails.meetDetails == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Google Meeting Details Not Found" });
                }
                if (googleMeetDetails.meetDetails.HangoutLink == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Google Meeting Join URL Not Found" });
                }
                if (googleMeetDetails.Id == 200)
                {
                    return Ok(googleMeetDetails.meetDetails);
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = googleMeetDetails.Status });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(ex);
            }
            return Ok(googleMeetDetails);
        }

        [HttpPost("GetTeamsMeetingReport")]
        public async Task<IActionResult> GetTeamsMeetingReport([FromBody]TeamsScheduleCode teamsScheduleCode)
        {
            try
            {

                List<TeamsScheduleDetailsV2> teamsScheduleDetailsV2s = await _IILTSchedule.GetTeamsDetails(teamsScheduleCode.Code);
                if(teamsScheduleDetailsV2s == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Teams Meeting Details Not Found" });
                }
                foreach(TeamsScheduleDetailsV2 result in teamsScheduleDetailsV2s)
                {
                    if (result.JoinUrl == null)
                    {
                        return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Teams Meeting Join URL Not Found" });
                    }
                    string Username = _IILTSchedule.GetMeetingScheduleEmail(result.UserWebinarId);
                    if (Username == null)
                    {
                        return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Teams Meeting Host Details Not Found" });
                    }
                    MeetingsReportData meeting = await _IILTSchedule.GetMeetingDetails(result.MeetingId, Username, teamsScheduleCode.Code);
                    if (meeting == null)
                    {
                        return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = "Teams Meeting is calender event report cannot be generated" });
                    }
                    return Ok(meeting);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpPost("GetTeamsMeetingReportForId")]
        public async Task<IActionResult> GetTeamsMeetingReportForId([FromBody] MeetingReportId meetingReportId)
        {
            List<TeamsScheduleDetailsV2> result = await _IILTSchedule.GetTeamsDetails(meetingReportId.Code);
            if (result == null)
            {
                return NotFound();
            }
            string Username = _IILTSchedule.GetMeetingScheduleEmail(result[0].UserWebinarId);
            if (Username == null)
            {
                return NotFound();
            }
            Meeting teamsMeeting = await _IILTSchedule.GetTeamsMeetingReportForId(meetingReportId.meetingId, meetingReportId.reportId,Username);
            return Ok(teamsMeeting);
        }
        [HttpPost("GetTeamsMeetingByScheduleCode")]
        public async Task<IActionResult> GetTeamsMeetingByScheduleCode([FromBody] TeamsScheduleCode teamsScheduleCode)
        {
            List<TeamsScheduleDetailsV2> result = await _IILTSchedule.GetTeamsDetails(teamsScheduleCode.Code);
            return Ok(result);
        }
        [HttpPost("UpdateTeamsMeeting")]
        public async Task<List<TeamsScheduleDetails>> UpdateTeamsMeeting([FromBody] UpdateTeamsV2 updateTeams)
        {
            List < TeamsScheduleDetails> teamsScheduleDetailsv1 = new List<TeamsScheduleDetails>();
            TeamsScheduleDetails teamsScheduleDetails = new TeamsScheduleDetails();
            try
            {
                AuthenticationResult authenticationResult = await _IILTSchedule.GetTeamsToken();
                foreach (UpdateTeams teams in updateTeams.updateTeams)
                {
                     await _IILTSchedule.UpdateTeamsMeeting(teams, UserId, OrganisationCode, teams.id, authenticationResult,updateTeams.HolidayList);
                }

                teamsScheduleDetailsv1 = await _IILTSchedule.CancelRemoveDatesSchedule(updateTeams.updateTeams, UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return teamsScheduleDetailsv1;
        }
        [HttpPost("ExportTeamsMeetingReportForId")]
        public IActionResult ExportTeamsMeetingReportForId([FromBody] ExportTeams exportTeamsMeetingReports)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    FileInfo ExcelFile;
                    if(exportTeamsMeetingReports == null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    ExcelFile = this._IILTSchedule.ExportTeamsMeetingReportForId(exportTeamsMeetingReports.exportTeamsMeetingReport,exportTeamsMeetingReports.ExportAs);

                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }

                    if (exportTeamsMeetingReports.ExportAs == "csv")
                    {
                        Response.ContentType = FileContentType.ExcelCSV;
                        return File(
                                fileData,
                                FileContentType.ExcelCSV,
                                ExcelFile.Name);
                    }
                    else
                    {
                        Response.ContentType = FileContentType.Excel;
                        return File(
                                fileData,
                                FileContentType.Excel,
                                ExcelFile.Name);
                    }

                }
                else
                    return this.BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetTeamsMeetingSchedule")]
        public async Task<IActionResult> GetTeamsMeetingSchedule([FromBody] TeamsScheduleCode teamsScheduleCode)
        {
            List<ILTSchedule> result = await _IILTSchedule.GetTeamsMeetingsDetails(teamsScheduleCode.Code);
            return Ok(result);
        }
        #region "Zoom Meeting"
        [HttpPost("ZoomMeeting")]
        public async Task<APIZoomMeetingResponce> ZoomMeeting([FromBody] Teams teams)
        {
            APIZoomMeetingResponce aPIZoomMeetingResponce = new APIZoomMeetingResponce();
            if (teams.Username == null)
            {
                return aPIZoomMeetingResponce;
            }

            try
            {
                teams.Username = Security.DecryptForUI(teams.Username);
                APIZoomDetailsToken obj = new APIZoomDetailsToken();
                string Token = this._IILTSchedule.ZoomToken();
                obj.access_token = Token;
                aPIZoomMeetingResponce = await _IILTSchedule.CallZoomMeetings(obj, teams, UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return aPIZoomMeetingResponce;
        }
        [HttpPost("UpdateZoomMeeting")]
        public async Task<int> UpdateZoomMeeting([FromBody] UpdateZoom zoom)
        {
            try
            {
                int response = await _IILTSchedule.UpdateZoomMeeting(zoom, UserId);
                if (response == 204)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return -1;
        }
        [HttpPost("GetZoomMeetingByScheduleCode")]
        public async Task<IActionResult> GetZoomMeetingByScheduleCode([FromBody] TeamsScheduleCode scheduleCode)
        {
            ZoomMeetingDetailsV2 result = await _IILTSchedule.GetZoomDetails(scheduleCode.Code);
            return Ok(result);
        }
        [HttpPost("GetZoomMeetingReportForId")]
        public async Task<IActionResult> GetZoomMeetingReportForId([FromBody] TeamsScheduleCode scheduleCode)
        {
            if(scheduleCode.Code == null)
            {
                return null;
            }
            ZoomMeetingDetails result = await _IILTSchedule.GetZoomDetails(scheduleCode.Code);
            if (result == null)
            {
                return NotFound();
            }
            if (result.UniqueMeetingId == null)
            {
                return NotFound();
            }
            ZoomMeetingDetailsForReport zoomMeetingDetailsForReport = await _IILTSchedule.ZoomMeetingDetailsForReport(Convert.ToInt64(result.UniqueMeetingId));
            if(zoomMeetingDetailsForReport == null)
            {
                return NotFound();
            }
            ZoomMeetingParticipants zoomMeetingParticipants = await _IILTSchedule.GetZoomMeetingParticipants(Convert.ToInt64(result.UniqueMeetingId));
            if (zoomMeetingParticipants == null)
            {
                return NotFound();
            }
            ZoomReport zoomReport = new ZoomReport();
            zoomReport.zoomMeetingParticipants = zoomMeetingParticipants;
            zoomReport.ZoomMeetingDetailsForReport = zoomMeetingDetailsForReport;
            return Ok(zoomReport);
        }
        [HttpPost("ExportZoomMeetingReportForId")]
        public IActionResult ExportZoomMeetingReportForId([FromBody] ExportZoom exportZoom)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    FileInfo ExcelFile;
                    if (exportZoom == null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                    ExcelFile = this._IILTSchedule.ExportZoomMeetingReportForId(exportZoom.zoomReports,exportZoom.ExportAs);

                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }

                    if (exportZoom.ExportAs == "csv")
                    {
                        Response.ContentType = FileContentType.ExcelCSV;
                        return File(
                                fileData,
                                FileContentType.ExcelCSV,
                                ExcelFile.Name);
                    }
                    else
                    {
                        Response.ContentType = FileContentType.Excel;
                        return File(
                                fileData,
                                FileContentType.Excel,
                                ExcelFile.Name);
                    }

                }
                else
                    return this.BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        #endregion
        [HttpPost("GetWebinarMeetingSchedule")]
        public async Task<IActionResult> GetWebinarMeetingSchedule([FromBody] WebinarSchedule webinarSchedule)
        {
            List<ILTSchedule> result = await _IILTSchedule.GetWebinarMeetingsDetails(webinarSchedule.Code);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("testEdcast/{courseID}")]
        public async Task<IActionResult> testEdcast(int courseID)
        {
            try
            {
                APIEdcastDetailsToken result = await _courseRepository.GetEdCastToken();
                if (result != null)
                {

                    APIEdCastTransactionDetails obj = await _courseRepository.PostCourseToClient(courseID, 1,result.access_token);
                    
                    return Ok(obj);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return NoContent();
            }
        }      

        [HttpPost("GetScheduleData")]
        [PermissionRequired(Permissions.iltshedules)]
        public async Task<IActionResult> GetV2([FromBody] ApiScheduleGet apiScheduleGet)
        {
            try
            {
                if (apiScheduleGet.search != null)
                    apiScheduleGet.search = apiScheduleGet.search.ToLower().Equals("null") ? null : apiScheduleGet.search;
                if (apiScheduleGet.searchText != null)
                    apiScheduleGet.searchText = apiScheduleGet.searchText.ToLower().Equals("null") ? null : apiScheduleGet.searchText;
                return Ok(await this._IILTSchedule.GetAllActiveSchedulesV2(apiScheduleGet, UserId, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetGsuitDetailsBySchedule")]
        public async Task<IActionResult> GetGsuitMeetingByScheduleCode([FromBody] ScheduleDetails schedule)
        {
            GoogleMeetDetailsV2 result = await _IILTSchedule.GetGsuitDetails(schedule.Id);
            return Ok(result);
        }
    }

}