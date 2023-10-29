using AspNet.Security.OAuth.Introspection;
using MyCourse.API.APIModel;
using MyCourse.API.Common;
using MyCourse.API.Helper;
using MyCourse.API.Helper.Metadata;
using MyCourse.API.Model;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static MyCourse.API.Common.AuthorizePermissions;
using static MyCourse.API.Common.TokenPermissions;
using log4net;
using MyCourse.API.Model.Log_API_Count;
using MyCourse.API.Repositories;

namespace MyCourse.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    [TokenRequired()]
    public class AssignmentDetailsController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssignmentDetailsController));
        IAssignmentDetailsRepository _assignmentRepository;
        private readonly ICourseCompletionStatusRepository _courseCompletionStatusRepository;
        ICourseRepository _courseRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration { get; set; }
        private readonly IRewardsPointRepository _rewardsPointRepository;

        public AssignmentDetailsController(
            IIdentityService _identitySvc,
            IAssignmentDetailsRepository assignmentRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configure,
             ICourseCompletionStatusRepository courseCompletionStatusRepository,
              ICourseRepository courseRepository, IRewardsPointRepository rewardsPointRepository
            ) : base(_identitySvc)
        {
            _assignmentRepository = assignmentRepository;
            _courseRepository = courseRepository;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configure;
            _courseCompletionStatusRepository = courseCompletionStatusRepository;
            this._rewardsPointRepository = rewardsPointRepository;
        }

        [HttpPost("UpdateAdssignmentDetail")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> UpdateAdssignmentDetails([FromBody] ApiAssignmentDetails apiAssignmentDetails)
        {
            try
            {

                if ((apiAssignmentDetails.Status == Record.Approved || apiAssignmentDetails.Status == Record.Rejected) && !(string.IsNullOrEmpty(apiAssignmentDetails.Remark)))
                {
                    bool validQuestion = FileValidation.CheckForSQLInjection(apiAssignmentDetails.Remark);
                    if (validQuestion == true)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }

                    AssignmentDetails assignmentDetail = await this._assignmentRepository.GetAssignmentDetail(apiAssignmentDetails.Id);
                    //Check Assignment already Approved 
                    if (assignmentDetail.Status != Record.Approved)
                    {
                        assignmentDetail.Remark = apiAssignmentDetails.Remark;

                        if (apiAssignmentDetails.Status == Record.Approved)
                            assignmentDetail.Status = Record.Approved;
                        else if (apiAssignmentDetails.Status == Record.Rejected)
                            assignmentDetail.Status = Record.Rejected;

                        await this._assignmentRepository.Update(assignmentDetail);

                        if (apiAssignmentDetails.Status == Record.Approved)
                        {
                            //Add Course Completion Status
                            CourseCompletionStatus courseCompletionStatus = new CourseCompletionStatus();
                            courseCompletionStatus.CourseId = assignmentDetail.CourseId;
                            courseCompletionStatus.UserId = assignmentDetail.UserId;
                            courseCompletionStatus.Status = Status.Completed;
                            await _courseCompletionStatusRepository.Post(courseCompletionStatus, OrganisationCode);

                        }
                        return Ok(true);
                    }
                    else
                    {
                        await this._rewardsPointRepository.AddAssignmentDetailsRewardReward(assignmentDetail.UserId, assignmentDetail.Id, assignmentDetail.CourseId, OrganisationCode);
                        return Ok("Assignment already approved");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(apiAssignmentDetails.Remark))
                        return Ok("Remark required");
                    else
                        return Ok("Invalid Status");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("GetAssignmentDetailsExcel")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> GetAssignmentDetails([FromBody] SearchAssignmentDetails searchAssignmentDetails)
        {
            try
            {
                List<ApiAssignmentInfo> apiAssignmentInfo = new List<ApiAssignmentInfo>();
                apiAssignmentInfo = await this._assignmentRepository.GetAssignmentDetails(UserId, searchAssignmentDetails);
                FileInfo ExcelFile = this._assignmentRepository.GetReportExcel(apiAssignmentInfo);
                var Fs = ExcelFile.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, ExcelFile.Name);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return Ok("");
            }
        }


        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                //Truncate AssignmentRejected Record    
                this._assignmentRepository.TRUNCATEAssignmentDetailsRejected();
                var request = _httpContextAccessor.HttpContext.Request;
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
                            string filename = fileUpload.FileName;
                            string[] fileaary = filename.Split('.');
                            string fileextention = fileaary[1].ToLower();
                            string filex = Record.XLSX;
                            if (fileextention != filex)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });
                            }
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrganisationCode, FileType);
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
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [Route("SaveFileData")]
        [PermissionRequired(Permissions.assignment_management)]
        [HttpPost]
        public async Task<IActionResult> PostFile([FromBody] APIAssignmentFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string filefinal = sWebRootFolder + aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(filefinal));
                string resultString = await this._assignmentRepository.ProcessImportFile(file, _assignmentRepository, _courseRepository,
                    //_asessmentQuestionOption,
                    UserId, OrganisationCode);
                return Ok(resultString);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet]
        [Route("Export")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> Export()
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string sFileName = @"Assignment.xlsx";
                string DomainName = this._configuration["ApiGatewayUrl"];
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }

                using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    OfficeOpenXml.ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Assignment");
                    //First add the headers

                    worksheet.Cells[1, 1].Value = "CourseCode*";
                    worksheet.Cells[1, 2].Value = "UserId*";
                    worksheet.Cells[1, 3].Value = "Status*";
                    worksheet.Cells[1, 4].Value = "Remark*";
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


        [HttpGet("GetAssignmentDetailsReject/{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> GetUserReject(int page, int pageSize, string search = null)
        {
            try
            {
                var assignmentRejected = await this._assignmentRepository.GetAssignmentDetailsRejected(page, pageSize, search);
                return Ok(assignmentRejected);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAssignmentDetailsRejectCount/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var QuestionReject = await this._assignmentRepository.Count(search);
                return Ok(QuestionReject);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }
}