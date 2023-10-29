using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseReport.API.APIModel;
using CourseReport.API.Common;
using CourseReport.API.Helper;
using CourseReport.API.Helper.MetaData;
using CourseReport.API.Model;
using CourseReport.API.Repositories.Interface;
using CourseReport.API.Service.Interface;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CourseReport.API.Service;
using static CourseReport.API.Common.AuthorizePermissions;
using static CourseReport.API.Common.TokenPermissions;
using log4net;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using CourseReport.API.APIModel;
using CourseReport.API.Helper.Log_API_Count;

namespace CourseReport.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token
    [TokenRequired()]
    public class CourseReportController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseReportController));

        private IConfiguration _configuration;
        private readonly ICourseReportService _coursereportService;
        private readonly ITokensRepository _tokensRepository;

       

        public CourseReportController(IIdentityService identityService, ICourseReportService coursereportService, ITokensRepository tokensRepository, IConfiguration configuration) : base(identityService)
        {
            this._configuration = configuration;
            this._coursereportService = coursereportService;
            this._tokensRepository = tokensRepository;
        }

        #region CourseWiseCompletion
        [HttpPost("GetCourseWiseCompletion")]
        [PermissionRequired(Permissions.CoursewiseCompletionStatusReport)]
        public async Task<IActionResult> GetCourseWiseCompletionReport([FromBody] APICourseWiseCompletionModule courseWiseCompletionModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    courseWiseCompletionModule.UserId = UserId;
                    return Ok(await this._coursereportService.GetCourseWiseCompletionReport(courseWiseCompletionModule, OrgCode));
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

        [HttpGet("GetCourseWiseCompletionReports/{page:int}/{pageSize:int}")]
        public async Task<IActionResult> GetCourseWiseCompletionReports(int page, int pageSize)
        {
            try
            {
                return Ok(await this._coursereportService.GetCourseWiseCompletionReports(page,pageSize,UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("PostCourseWiseCompletionReport")]
        public async Task<IActionResult> PostCourseWiseCompletionReport([FromBody] APIPostCourseWiseCompletionReport data )
        {
            try
            {
                    int id = await this._coursereportService.PostCourseWiseCompletionReport(data, UserId);
                    return Ok(id);
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("GetCourseWiseCompletionCount")]
        [PermissionRequired(Permissions.CoursewiseCompletionStatusReport)]
        public async Task<IActionResult> GetCourseWiseCompletionReportCount([FromBody] APICourseWiseCompletionModule courseWiseCompletionModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    courseWiseCompletionModule.PageSize = 0;
                    courseWiseCompletionModule.UserId = UserId;


                    int RecordCount = 0;
                    System.Collections.Generic.IEnumerable<APICourseWiseCompletionReport> Result = await this._coursereportService.GetCourseWiseCompletionReport(courseWiseCompletionModule, OrgCode);
                    RecordCount = Result.Select(c => c.TotalRecordCount).FirstOrDefault();
                    return Ok(RecordCount);

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

        [HttpPost("ExportCourseWiseCompletionReportV2")]
        [PermissionRequired(Permissions.CoursewiseCompletionStatusReport)]
        public async Task<IActionResult> ExportGetCourseWiseCompletionReportV2([FromBody] APICourseWiseCompletionModule courseWiseCompletionModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FileInfo ExcelFile;

                    var httpClient = new HttpClient();
                    var BaseUrl = this._configuration["UserApi"] + "User/Settings/1/20";
                    var response = await APIHelper.CallGetAPI(BaseUrl, Token);
                    var contents = await response.Content.ReadAsStringAsync();


                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    List<APIUserSetting> userSetting = JsonSerializer.Deserialize<List<APIUserSetting>>(contents, options);

                    courseWiseCompletionModule.StartIndex = 1;
                    courseWiseCompletionModule.UserId = UserId;
                    ExcelFile = await this._coursereportService.ExportCourseWiseCompletionReport(courseWiseCompletionModule, OrgCode, userSetting);

                    APIUpdateCourseWiseCompletionReport data = new APIUpdateCourseWiseCompletionReport();
                    if (ExcelFile.Name.Contains(OrgCode))
                    {
                        data.ExportPath = ExcelFile.Name;
                    }
                    else {
                        data.ExportPath =OrgCode+"/"+ ExcelFile.Name;
                    }
                    data.Id = (int)courseWiseCompletionModule.Id;
                   
                    string  exportPath = await this._coursereportService.UpdateCourseWiseCompletionReport(data, UserId);

                    return Ok(exportPath);
                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }

                    if (courseWiseCompletionModule.ExportAs == "csv")
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

        [HttpPost("ExportCourseWiseCompletionReport")]
        [PermissionRequired(Permissions.CoursewiseCompletionStatusReport)]
        public async Task<IActionResult> ExportGetCourseWiseCompletionReport([FromBody] APICourseWiseCompletionModule courseWiseCompletionModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FileInfo ExcelFile;

                    var httpClient = new HttpClient();
                    var BaseUrl = this._configuration["UserApi"] + "User/Settings/1/20";
                    var response = await APIHelper.CallGetAPI(BaseUrl, Token);
                    var contents = await response.Content.ReadAsStringAsync();


                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    List<APIUserSetting> userSetting = JsonSerializer.Deserialize<List<APIUserSetting>>(contents, options);

                    courseWiseCompletionModule.StartIndex = 1;
                    courseWiseCompletionModule.UserId = UserId;
                    ExcelFile = await this._coursereportService.ExportCourseWiseCompletionReport(courseWiseCompletionModule, OrgCode, userSetting);

                   
                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }

                    if (courseWiseCompletionModule.ExportAs == "csv")
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


        [HttpPost("UpdateOnDownloadReport")]
         [PermissionRequired(Permissions.CoursewiseCompletionStatusReport)]
        public async Task<IActionResult> UpdateOnDownloadReport([FromBody] APIReportDownload downloadData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    ExportCourseCompletionDetailReport data = await this._coursereportService.UpdateonDownloadReport(downloadData.Id);

                    return Ok(data);
                   
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

        #region   ModeratorwiseSubjectContentSummary_Report
        [HttpPost("GetModeratorwiseSubjectContentSummary")]
        [PermissionRequired(Permissions.ModeratorwiseSubjectSummaryReport)]
        public async Task<IActionResult> GetModeratorwiseSubjectContentSummary([FromBody] APIModeratorwiseSubjectSummaryModule ModeratorSubjectSummary)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    return Ok(await this._coursereportService.GetModeratorwiseSubjectSummaryReport(ModeratorSubjectSummary));

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
        [HttpPost("GetModeratorwiseSubjectContentSummaryCount")]
        [PermissionRequired(Permissions.ModeratorwiseSubjectSummaryReport)]
        public async Task<IActionResult> ModeratorwiseSubjectContentSummaryCount([FromBody] APIModeratorwiseSubjectSummaryModule subjectSummaryModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    subjectSummaryModule.PageSize = 0;

                    int RecordCount = 0;
                    System.Collections.Generic.IEnumerable<ApiModeratorwiseSubjectSummaryReport> Result = await this._coursereportService.GetModeratorwiseSubjectSummaryReport(subjectSummaryModule);
                    RecordCount = Result.Select(c => c.TotalRecordCount).Count();
                    return Ok(RecordCount);

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
        [HttpPost("ExportModeratorwiseSubjectSummaryReport")]
        [PermissionRequired(Permissions.ModeratorwiseSubjectSummaryReport)]
        public async Task<IActionResult> ExportModeratorwiseSubjectSummaryReport([FromBody] APIModeratorwiseSubjectSummaryModule subjectSummaryReport)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FileInfo ExcelFile;
                    subjectSummaryReport.StartIndex = 1;
                    ExcelFile = await this._coursereportService.ExportModeratorwiseSubjectSummaryReport(subjectSummaryReport);

                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }
                    Response.ContentType = FileContentType.Excel;
                    return File(
                            fileData,
                            FileContentType.Excel,
                            ExcelFile.Name);

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

        #region ModeratorwiseSubjectContentDetails
        [HttpPost("GetModeratorwiseSubjectContentDetails")]
        [PermissionRequired(Permissions.ModeratorwiseSubjectDetailsReport)]
        public async Task<IActionResult> GetModeratorwiseSubjectContentDetails([FromBody] APIModeratorwiseSubjectDetailsModule ModeratorSubjectDetails)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    return Ok(await this._coursereportService.GetModeratorwiseSubjectDetailsReport(ModeratorSubjectDetails));

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
        [HttpPost("GetModeratorwiseSubjectContentDetailsCount")]
        [PermissionRequired(Permissions.ModeratorwiseSubjectDetailsReport)]
        public async Task<IActionResult> ModeratorwiseSubjectContentDetailsCount([FromBody] APIModeratorwiseSubjectDetailsModule subjectDetailsModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    subjectDetailsModule.PageSize = 0;

                    int RecordCount = 0;
                    System.Collections.Generic.IEnumerable<ApiModeratorwiseSubjectDeailsReport> Result = await this._coursereportService.GetModeratorwiseSubjectDetailsReport(subjectDetailsModule);
                    RecordCount = Result.Select(c => c.TotalRecordCount).Count();
                    return Ok(RecordCount);

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
        [HttpPost("ExportModeratorwiseSubjectDetailsReport")]
        [PermissionRequired(Permissions.ModeratorwiseSubjectDetailsReport)]
        public async Task<IActionResult> ExportModeratorwiseSubjectDetailsReport([FromBody] APIModeratorwiseSubjectDetailsModule subjectDetailsReport)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FileInfo ExcelFile;
                    subjectDetailsReport.StartIndex = 1;
                    ExcelFile = await this._coursereportService.ExportModeratorwiseSubjectDetailsReport(subjectDetailsReport);

                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }
                    Response.ContentType = FileContentType.Excel;
                    return File(
                            fileData,
                            FileContentType.Excel,
                            ExcelFile.Name);

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

        #region User Learning Report

        [HttpPost("GetUserLearningReport")]
        [PermissionRequired(Permissions.UserLearningReport)]
        public async Task<IActionResult> GetUserLearningReport([FromBody] APIUserLearningReportModule UserLearningReportModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._coursereportService.GetUserLearningReport(UserLearningReportModule, UserId, OrgCode));
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

        [HttpPost("GetUserLearningCount")]
        [PermissionRequired(Permissions.UserLearningReport)]
        public async Task<IActionResult> GetUserLearningReportCount([FromBody] APIUserLearningReportModule UserLearningReportModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UserLearningReportModule.PageSize = 0;

                    int RecordCount = 0;
                    System.Collections.Generic.IEnumerable<APIUserLearningReport> Result = await this._coursereportService.GetUserLearningReport(UserLearningReportModule, UserId, OrgCode);
                    RecordCount = Result.Select(c => c.TotalRecordCount).FirstOrDefault();
                    return Ok(RecordCount);

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

        [HttpPost("ExportUserLearningReport")]
        [PermissionRequired(Permissions.UserLearningReport)]
        public async Task<IActionResult> ExportGetUserLearningReport([FromBody] APIUserLearningReportModule UserLearningReportModule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FileInfo ExcelFile;
                    UserLearningReportModule.StartIndex = 1;
                    ExcelFile = await this._coursereportService.ExportUserLearningReport(UserLearningReportModule, UserId, OrgCode);

                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }

                    if (UserLearningReportModule.ExportAs == "csv")
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

        #region CourseRatingReport

        [HttpPost("CourseRatingReport")]


        [PermissionRequired(Permissions.CourseRatingReport)]
        public async Task<IActionResult> CourseRatingReport([FromBody] APICourseRatingReport aPICourseRatingReport)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await this._coursereportService.CourseRatingReport(aPICourseRatingReport));
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

        [HttpPost("CourseRatingReportCount")]
        [PermissionRequired(Permissions.CourseRatingReport)]
        public async Task<IActionResult> GetCourseRatingReportCount([FromBody] APICourseRatingReport aPICourseRatingReport)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    aPICourseRatingReport.PageSize = 0;
                    int Result = await this._coursereportService.GetCourseRatingReportCount(aPICourseRatingReport);
                    //RecordCount = Result.Select(c => c.TotalRecordCount).FirstOrDefault();
                    return Ok(Result);

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

        [HttpPost("ExportCourseRatingReport")]
        [PermissionRequired(Permissions.CourseRatingReport)]
        public async Task<IActionResult> ExportCourseRatingReport([FromBody] APICourseRatingReport aPICourseRatingReport)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FileInfo ExcelFile;
                    aPICourseRatingReport.StartIndex = 1;
                    ExcelFile = await this._coursereportService.ExportCourseRatingReport(aPICourseRatingReport, OrgCode);

                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }
                    if (aPICourseRatingReport.ExportAs == "csv")
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

        #region UserwiseCourseStatusReport
        [HttpPost("ExportUserwiseCourseStatusReport")]
        [PermissionRequired(Permissions.UserwiseCourseStatusReport)]
        public async Task<IActionResult> ExportUserwiseCourseStatusReport([FromBody] APIUserwiseCourseStatusReport aPIUserwiseCourseStatusReport)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return this.BadRequest(ModelState);
                }

                FileInfo ExcelFile;
                ExcelFile = await _coursereportService.ExportUserwiseCourseStatusReport(aPIUserwiseCourseStatusReport);

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }

                if (aPIUserwiseCourseStatusReport.ExportAs == "csv")
                {
                    Response.ContentType = FileContentType.ExcelCSV;
                    return File(
                            data,
                            FileContentType.ExcelCSV,
                            ExcelFile.Name);
                }
                else
                {
                    Response.ContentType = FileContentType.Excel;
                    return File(
                            data,
                            FileContentType.Excel,
                            ExcelFile.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError), StatusCode = StatusCodes.Status500InternalServerError });
            }
        }

        #endregion

        #region Tcns Retraining Report
        [HttpPost("TcnsRetrainingReport")]
        public async Task<IActionResult> GetTcnsRetrainingReport([FromBody] APITcnsRetrainingReport tcnsRetrainingReport)
        {
            try
            {
                FileInfo ExcelFile;

                ExcelFile = await this._coursereportService.GetTcnsRetrainingReport(tcnsRetrainingReport,OrgCode);

                FileStream Fs = ExcelFile.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }

                if (tcnsRetrainingReport.ExportAs == "csv")
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
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        #endregion
    }
}
