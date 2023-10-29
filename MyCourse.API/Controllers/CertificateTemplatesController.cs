using AspNet.Security.OAuth.Introspection;
using MyCourse.API.APIModel;
using MyCourse.API.Common;
using MyCourse.API.Helper;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static MyCourse.API.Common.TokenPermissions;
using log4net;
using MyCourse.API.Model;
using System.Linq;
using System.Collections.Generic;
using MyCourse.API.APIModel.NodalManagement;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;
using MyCourse.API.Repositories;
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
    public class CertificateTemplatesController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CertificateTemplatesController));
        ICertificateTemplatesRepository _certificateTemplatesRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IIdentityService _identitySvc;
        ICourseRepository _courseRepository;
        IDevelopmentPlanRepository _devplan;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        public CertificateTemplatesController(IIdentityService identitySvc, ICertificateTemplatesRepository certificateTemplatesRepository, ICourseRepository courseRepository, ITokensRepository tokensRepository,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IDevelopmentPlanRepository devplan) : base(identitySvc)
        {
            this._certificateTemplatesRepository = certificateTemplatesRepository;
            this._courseRepository = courseRepository;
            this._tokensRepository = tokensRepository;
            this._identitySvc = identitySvc;
            _configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this._devplan = devplan;
        }


        [HttpGet("GetCertificateTemplateResult/{courseid}")]
        public async Task<IActionResult> GetCertificateTemplateResult(int courseid)
        {
            try
            {
                //get userid from token
                string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                if (identity == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }
                string userName = Security.Decrypt(_identitySvc.GetUserName());
                int tokenId = Convert.ToInt32(identity);
                string coursTitle = await _courseRepository.GetCourseNam(courseid);
                bool IsCourseCompleted = await this._certificateTemplatesRepository.IsCourseCompleted(courseid, tokenId);
                if (!IsCourseCompleted)
                {
                    return StatusCode(400, "Course Not Completed");
                }
                ApiCourseCompletionDetails CourseDetails = await this._certificateTemplatesRepository.GetCourseCompletionDetails(courseid, tokenId);
                DateTime comp = Convert.ToDateTime(CourseDetails.CompletionDate);
                var cdate = comp.ToString("dd MMMM yyyy");
                string CourseStartDate1 = null;
                if (!CourseDetails.StatusFromImage)
                {
                    return StatusCode(400, "Images captured not approved by supervisor");
                }
                if (CourseDetails.CourseStartDate != null)
                {
                    DateTime CourseStartDate = Convert.ToDateTime(CourseDetails.CourseStartDate);
                    CourseStartDate1 = CourseStartDate.ToString("dd MMMM yyyy");
                }
                else
                {
                    DateTime CourseStartDate = Convert.ToDateTime(CourseDetails.CourseStartDate);
                    CourseStartDate1 = null;
                }
                DateTime CourseEndDate = Convert.ToDateTime(CourseDetails.CourseEndDate);
                var CourseEndDate1 = CourseEndDate.ToString("dd MMMM yyyy");
                string SerialNumber = await this._certificateTemplatesRepository.AddCerificationDownloadDetails(tokenId, courseid, OrganisationCode, coursTitle);
                APICertificateTemplatesResult aPICertificateTemplatesResult = new APICertificateTemplatesResult();
                aPICertificateTemplatesResult.CourseId = courseid;
                aPICertificateTemplatesResult.UserId = tokenId;
                aPICertificateTemplatesResult.CourseTitle = CourseDetails.Title;
                aPICertificateTemplatesResult.UserName = userName;
                aPICertificateTemplatesResult.CompletionDate = cdate;
                aPICertificateTemplatesResult.Percentage = CourseDetails.TotalMarks;
                aPICertificateTemplatesResult.AssessmentResult = CourseDetails.AssessmentResult;
                aPICertificateTemplatesResult.Department = CourseDetails.Department;
                aPICertificateTemplatesResult.Position = CourseDetails.Position;
                aPICertificateTemplatesResult.Designation = CourseDetails.Designation;
                aPICertificateTemplatesResult.AuthorityName = CourseDetails.AuthorityName;
                aPICertificateTemplatesResult.CourseStartDate = CourseStartDate1;
                aPICertificateTemplatesResult.CourseEndDate = CourseEndDate1;
                aPICertificateTemplatesResult.CourseCode = CourseDetails.CourseCode;
                aPICertificateTemplatesResult.SearialNumber = SerialNumber;
                aPICertificateTemplatesResult.IssuedBy= CourseDetails.IssuedBy; 
                aPICertificateTemplatesResult.AuthorisedBy = CourseDetails.AuthorisedBy;
                aPICertificateTemplatesResult.Area = CourseDetails.Area;
                aPICertificateTemplatesResult.GroupName = CourseDetails.GroupName;

                string fileDir = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                fileDir = Path.Combine(fileDir, OrgCode, Record.Certificates);
                if (!Directory.Exists(fileDir))
                    Directory.CreateDirectory(fileDir);
                
                string file = Path.Combine(fileDir, userName + SerialNumber + ".pdf");
                string filePath = string.Concat(DomainName, OrgCode, "/", Record.Certificates, "/", userName + SerialNumber + ".pdf");
                aPICertificateTemplatesResult.CertificatePath = filePath;
                aPICertificateTemplatesResult.CertificateFile = userName + SerialNumber + ".pdf";
                
                if (CourseDetails.Grade == null || CourseDetails.Grade == "")
                { aPICertificateTemplatesResult.Grade = null; }
                else
                {
                    aPICertificateTemplatesResult.Grade = CourseDetails.Grade;
                }
                int index = CourseDetails.CertificateImageName.LastIndexOf('/');
                string CertificateImageName = null;
                if (index != -1)
                    CertificateImageName = CourseDetails.CertificateImageName.Substring(index + 1);

                aPICertificateTemplatesResult.CertificateImageName = CertificateImageName;
                var assets = Record.Assets;
                var img = Record.img;

                aPICertificateTemplatesResult.ImagePath = "/assets/img" + "/" + CourseDetails.CertificateImageName;
                aPICertificateTemplatesResult.EmployeeCode = CourseDetails.EmployeeCode;
                if (OrgCode!="csl")
                    aPICertificateTemplatesResult.EmployeeCode = Security.Decrypt(_identitySvc.GetUserId());
                

                aPICertificateTemplatesResult.RollNumber = CourseDetails.RollNumber;
                if (aPICertificateTemplatesResult.AssessmentResult != null && (aPICertificateTemplatesResult.AssessmentResult.ToLower() == "fail" || aPICertificateTemplatesResult.AssessmentResult.ToLower() == "failed"))
                {

                    return this.BadRequest("You have not cleared the assessment");
                }
              

                return this.Ok(aPICertificateTemplatesResult);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpPost("GetBulkCertificateTemplateResult")]
        public async Task<IActionResult> GetBulkCertificateTemplateResult([FromBody] CertificateBulk courseObj)
        {
            try
            {
                //get userid from token
                CourseCertificateDataFinal finalobj = new CourseCertificateDataFinal();
                List<APICertificateDownlodResult> objresult = new List<APICertificateDownlodResult>();
                CourseCertificateData allCourseDetails = await this._certificateTemplatesRepository.GetAllCourseCompletionDetails(courseObj.CourseId, courseObj.UserId, courseObj.page, courseObj.pagesize,UserId);
                foreach (ApiCourseCompletionDetails CourseDetails in allCourseDetails.data)
                {
                    DateTime comp = Convert.ToDateTime(CourseDetails.CompletionDate);
                    var cdate = comp.ToString("dd MMMM yyyy");
                    string CourseStartDate1 = null;
                    if (CourseDetails.CourseStartDate != null)
                    {
                        DateTime CourseStartDate = Convert.ToDateTime(CourseDetails.CourseStartDate);
                        CourseStartDate1 = CourseStartDate.ToString("dd MMMM yyyy");
                    }
                    else
                    {
                        DateTime CourseStartDate = Convert.ToDateTime(CourseDetails.CourseStartDate);
                        CourseStartDate1 = null;
                    }
                    DateTime CourseEndDate = Convert.ToDateTime(CourseDetails.CourseEndDate);
                    var CourseEndDate1 = CourseEndDate.ToString("dd MMMM yyyy");
                    string SerialNumber = await this._certificateTemplatesRepository.AddCerificationDownloadDetails(CourseDetails.UsId, courseObj.CourseId, OrganisationCode, CourseDetails.Title);
                    APICertificateDownlodResult aPICertificateTemplatesResult = new APICertificateDownlodResult();
                    aPICertificateTemplatesResult.CourseId = courseObj.CourseId;
                    aPICertificateTemplatesResult.Id = CourseDetails.Id;
                    aPICertificateTemplatesResult.UserId = CourseDetails.UserId;
                    aPICertificateTemplatesResult.UsId = CourseDetails.UsId;
                    aPICertificateTemplatesResult.CourseTitle = CourseDetails.Title;
                    aPICertificateTemplatesResult.UserName = CourseDetails.UserName;
                    aPICertificateTemplatesResult.CompletionDate = cdate;
                    aPICertificateTemplatesResult.Percentage = CourseDetails.TotalMarks;
                    aPICertificateTemplatesResult.AssessmentResult = CourseDetails.AssessmentResult;
                    aPICertificateTemplatesResult.Department = CourseDetails.Department;
                    aPICertificateTemplatesResult.Position = CourseDetails.Position;
                    aPICertificateTemplatesResult.Designation = CourseDetails.Designation;
                    aPICertificateTemplatesResult.AuthorityName = CourseDetails.AuthorityName;
                    aPICertificateTemplatesResult.CourseStartDate = CourseStartDate1;
                    aPICertificateTemplatesResult.CourseEndDate = CourseEndDate1;
                    aPICertificateTemplatesResult.CourseCode = CourseDetails.CourseCode;
                    aPICertificateTemplatesResult.SearialNumber = SerialNumber;
                    aPICertificateTemplatesResult.IssuedBy = CourseDetails.IssuedBy;
                    aPICertificateTemplatesResult.AuthorisedBy = CourseDetails.AuthorisedBy;
                    aPICertificateTemplatesResult.Area = CourseDetails.Area;
                    aPICertificateTemplatesResult.GroupName = CourseDetails.GroupName;

                    string fileDir = this._configuration["ApiGatewayWwwroot"];
                    string DomainName = this._configuration["ApiGatewayUrl"];
                    fileDir = Path.Combine(fileDir, OrgCode, Record.Certificates);
                    if (!Directory.Exists(fileDir))
                        Directory.CreateDirectory(fileDir);

                    string file = Path.Combine(fileDir, CourseDetails.UserName + SerialNumber + ".pdf");
                    string filePath = string.Concat(DomainName, OrgCode, "/", Record.Certificates, "/", CourseDetails.UserName + SerialNumber + ".pdf");
                    aPICertificateTemplatesResult.CertificatePath = filePath;
                    aPICertificateTemplatesResult.CertificateFile = CourseDetails.UserName + SerialNumber + ".pdf";

                    if (CourseDetails.Grade == null || CourseDetails.Grade == "")
                    { aPICertificateTemplatesResult.Grade = null; }
                    else
                    {
                        aPICertificateTemplatesResult.Grade = CourseDetails.Grade;
                    }
                    int index = CourseDetails.CertificateImageName.LastIndexOf('/');
                    string CertificateImageName = null;
                    if (index != -1)
                        CertificateImageName = CourseDetails.CertificateImageName.Substring(index + 1);

                    aPICertificateTemplatesResult.CertificateImageName = CertificateImageName;
                    var assets = Record.Assets;
                    var img = Record.img;

                    aPICertificateTemplatesResult.ImagePath = "/assets/img" + "/" + CourseDetails.CertificateImageName;
                    aPICertificateTemplatesResult.EmployeeCode = CourseDetails.EmployeeCode;
                    if (OrgCode != "csl")
                        aPICertificateTemplatesResult.EmployeeCode = Security.Decrypt(_identitySvc.GetUserId());


                    aPICertificateTemplatesResult.RollNumber = CourseDetails.RollNumber;
                    if (aPICertificateTemplatesResult.AssessmentResult != null && (aPICertificateTemplatesResult.AssessmentResult.ToLower() == "fail" || aPICertificateTemplatesResult.AssessmentResult.ToLower() == "failed"))
                    {
                        return this.BadRequest(CourseDetails.UserName + " have not cleared the assessment");
                    }
                    else
                    {
                        objresult.Add(aPICertificateTemplatesResult);
                    }

                }
                finalobj.data = objresult;
                finalobj.TotalRecords = allCourseDetails.TotalRecords;


                return this.Ok(finalobj);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpGet("GetDevPlanResult/{devPlanid}")]
        public async Task<IActionResult> GetDevPlanResult(int devPlanid)
        {
            try
            {
                //get userid from token
                string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                if (identity == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }
                string userName = Security.Decrypt(_identitySvc.GetUserName());
                int tokenId = Convert.ToInt32(identity);
                string devplanName = await _devplan.GetDevPlanNam(devPlanid);
                DevelopmentPlanDetails IsdevPlanCompleted = await this._devplan.IsdevPlanCompleted(devPlanid, UserId);
                if (!IsdevPlanCompleted.DevelopmentPlanCompleted)
                {
                    return StatusCode(400, "Developement Plan Not Completed");
                }
               APIDevelopementPlanResult devplanresult = new APIDevelopementPlanResult();
                devplanresult.DevelopementPlan = devplanName;
                devplanresult.UserId = tokenId;
                devplanresult.DevPlanId = devPlanid;
                devplanresult.UserName = userName;
                devplanresult.CompletionDate = IsdevPlanCompleted.cdate;



                devplanresult.EmployeeCode = Security.Decrypt(_identitySvc.GetUserId());
               
               

                return this.Ok(devplanresult);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpGet("GetGroupCertificateTemplateResult/{courseid}/{GroupId}")]
        public async Task<IActionResult> GetGroupCertificateTemplateResult(int courseid, int GroupId)
        {
            try
            {
                //get userid from token
                string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                if (identity == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }

                List<APINodalUserDetails> aPINodalUserDetails = await _certificateTemplatesRepository.GetGroupUsers(GroupId);
                List<APIGroupCertificateTemplatesResult> aPICertificateTemplatesResults = new List<APIGroupCertificateTemplatesResult>();
                string coursTitle = await _courseRepository.GetCourseNam(courseid);

                foreach (APINodalUserDetails item in aPINodalUserDetails)
                {
                    APIGroupCertificateTemplatesResult aPIGroupCertificateTemplatesResult = new APIGroupCertificateTemplatesResult();
                    aPIGroupCertificateTemplatesResult.CourseId = courseid;
                    aPIGroupCertificateTemplatesResult.UserId = item.UserId;
                    aPIGroupCertificateTemplatesResult.CourseTitle = coursTitle;
                    aPIGroupCertificateTemplatesResult.UserName = item.UserName;
                    aPIGroupCertificateTemplatesResult.EmployeeCode = item.EmployeeCode;
                    bool IsCourseCompleted = await this._certificateTemplatesRepository.IsCourseCompleted(courseid, item.UserId);
                    if (!IsCourseCompleted)
                    {
                        aPIGroupCertificateTemplatesResult.Status = "Course not completed.";
                    }
                    else
                    {
                        ApiCourseCompletionDetails CourseDetails = await this._certificateTemplatesRepository.GetCourseCompletionDetails(courseid, item.UserId);
                        DateTime comp = Convert.ToDateTime(CourseDetails.CompletionDate);
                        var cdate = comp.ToString("dd MMMM yyyy");
                        string CourseStartDate1 = null;
                        if (CourseDetails.CourseStartDate != null)
                        {
                            DateTime CourseStartDate = Convert.ToDateTime(CourseDetails.CourseStartDate);
                            CourseStartDate1 = CourseStartDate.ToString("dd MMMM yyyy");
                        }
                        else
                        {
                            DateTime CourseStartDate = Convert.ToDateTime(CourseDetails.CourseStartDate);
                            CourseStartDate1 = null;
                        }
                        DateTime CourseEndDate = Convert.ToDateTime(CourseDetails.CourseEndDate);
                        var CourseEndDate1 = CourseEndDate.ToString("dd MMMM yyyy");
                        string SerialNumber = await this._certificateTemplatesRepository.AddCerificationDownloadDetails(item.UserId, courseid, OrganisationCode, coursTitle);

                        aPIGroupCertificateTemplatesResult.CompletionDate = cdate;
                        aPIGroupCertificateTemplatesResult.Percentage = CourseDetails.TotalMarks;
                        aPIGroupCertificateTemplatesResult.AssessmentResult = CourseDetails.AssessmentResult;
                        aPIGroupCertificateTemplatesResult.Department = CourseDetails.Department;
                        aPIGroupCertificateTemplatesResult.Position = CourseDetails.Position;
                        aPIGroupCertificateTemplatesResult.Designation = CourseDetails.Designation;
                        aPIGroupCertificateTemplatesResult.AuthorityName = CourseDetails.AuthorityName;
                        aPIGroupCertificateTemplatesResult.CourseStartDate = CourseStartDate1;
                        aPIGroupCertificateTemplatesResult.CourseEndDate = CourseEndDate1;
                        aPIGroupCertificateTemplatesResult.SearialNumber = SerialNumber;
                        aPIGroupCertificateTemplatesResult.IssuedBy = CourseDetails.IssuedBy;
                        aPIGroupCertificateTemplatesResult.AuthorisedBy = CourseDetails.AuthorisedBy;
                        aPIGroupCertificateTemplatesResult.Area = CourseDetails.Area;
                        aPIGroupCertificateTemplatesResult.GroupName = CourseDetails.GroupName;

                        string fileDir = this._configuration["ApiGatewayWwwroot"];
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        fileDir = Path.Combine(fileDir, OrgCode, Record.Certificates);
                        if (!Directory.Exists(fileDir))
                            Directory.CreateDirectory(fileDir);

                        string file = Path.Combine(fileDir, item.UserName + SerialNumber + ".pdf");
                        string filePath = string.Concat(DomainName, OrgCode, "/", Record.Certificates, "/", item.UserName + SerialNumber + ".pdf");
                        aPIGroupCertificateTemplatesResult.CertificatePath = filePath;
                        aPIGroupCertificateTemplatesResult.CertificateFile = item.UserName + SerialNumber + ".pdf";

                        if (CourseDetails.Grade == null || CourseDetails.Grade == "")
                        { aPIGroupCertificateTemplatesResult.Grade = null; }
                        else
                        {
                            aPIGroupCertificateTemplatesResult.Grade = CourseDetails.Grade;
                        }
                        int index = CourseDetails.CertificateImageName.LastIndexOf('/');
                        string CertificateImageName = null;
                        if (index != -1)
                            CertificateImageName = CourseDetails.CertificateImageName.Substring(index + 1);

                        aPIGroupCertificateTemplatesResult.CertificateImageName = CertificateImageName;
                        var assets = Record.Assets;
                        var img = Record.img;
                        aPIGroupCertificateTemplatesResult.ImagePath = "/assets/img" + "/" + CourseDetails.CertificateImageName;
                        aPIGroupCertificateTemplatesResult.EmployeeCode = Security.Decrypt(_identitySvc.GetUserId());
                        aPIGroupCertificateTemplatesResult.RollNumber = CourseDetails.RollNumber;
                        if (aPIGroupCertificateTemplatesResult.AssessmentResult != null && (aPIGroupCertificateTemplatesResult.AssessmentResult.ToLower() == "fail" || aPIGroupCertificateTemplatesResult.AssessmentResult.ToLower() == "failed"))
                        {
                            aPIGroupCertificateTemplatesResult.Status = "You have not cleared the assessment.";
                        }
                        aPICertificateTemplatesResults.Add(aPIGroupCertificateTemplatesResult);
                    }
                }
                if (aPICertificateTemplatesResults.Count>0)
                    return Ok(aPICertificateTemplatesResults);
                else
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = "Please complete course to download certificates." });

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }
        [HttpPost]
        [Route("PostFileUpload")]
        public async Task<IActionResult> PostFileUpload([FromForm] APICertificateUpload aPICertificateUpload)
        {
            try
            {
                var request = httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });

                        if (FileValidation.IsValidPdf(fileUpload))
                        {
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            string DomainName = this._configuration["ApiGatewayUrl"];
                            fileDir = Path.Combine(fileDir, OrgCode, Record.Certificates);
                            if (!Directory.Exists(fileDir))
                                Directory.CreateDirectory(fileDir);

                            string file = Path.Combine(fileDir, aPICertificateUpload.File);
                            string filePath = string.Concat(DomainName, OrgCode, "/", Record.Certificates, "/", aPICertificateUpload.File);

                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                                await fileUpload.CopyToAsync(fs);

                            if (String.IsNullOrEmpty(file))
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });

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

        [HttpPost("GetCertificateDownloadStatus")]
        public async Task<IActionResult> GetCertificateDownloadStatus([FromBody] CertificateStatus courseObj)
        {
            try
            {
                //get userid from token
                
                return this.Ok(await this._certificateTemplatesRepository.GetCertificateDownloadStatus(courseObj.UserId, courseObj.CourseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

    }
}