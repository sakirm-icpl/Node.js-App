using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Helper;
using Courses.API.Helper.Metadata;
using Courses.API.Model;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using static Courses.API.Common.TokenPermissions;
using log4net;
using Microsoft.AspNetCore.Http;
using static Courses.API.Common.AuthorizePermissions;
using Microsoft.AspNetCore.StaticFiles;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Model;
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CourseCertificateAssociationController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseCertificateAssociationController));
        ICourseCertificateAssociationRepository _courseCertificateAssociationRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IIdentityService _identitySvc;
        private readonly IHttpContextAccessor _httpContextAccessor;
        ICourseRepository _courseRepository;
        IAzureStorage _azurestorage;

        public IConfiguration _configuration { get; }
        public CourseCertificateAssociationController
            (ICourseRepository courseRepository,ICourseCertificateAssociationRepository courseCertificateAssociationRepository, IConfiguration configuration, ITokensRepository tokensRepository, IIdentityService identitySvc, IAzureStorage azurestorage, IHttpContextAccessor httpContextAccessor) : base(identitySvc)
        {
            this._azurestorage = azurestorage;
            this._courseRepository = courseRepository;
            this._courseCertificateAssociationRepository = courseCertificateAssociationRepository;
            this._tokensRepository = tokensRepository;
            this._identitySvc = identitySvc;
            this._httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IEnumerable<APICourseCertificateAssociation>> Get(int page, int pageSize, string search = null)
        {
            try
            {
                return await this._courseCertificateAssociationRepository.GetAll(page, pageSize, search);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]

        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int Count = await this._courseCertificateAssociationRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APICourseCertificateAssociation apICourseCertificateAssociation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var orgcode = Record.OrgCode;
                    string OrgCode = Security.Decrypt(_identitySvc.GetOrgCode());
                    CourseCertificateAssociation CourseCertificateAssociation = await this._courseCertificateAssociationRepository.Get(id);
                    if (CourseCertificateAssociation == null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                    }
                    if (await this._courseCertificateAssociationRepository.Exists(apICourseCertificateAssociation.CertificateImageName, apICourseCertificateAssociation.CourseID, id, OrgCode))
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                    }
                    CourseCertificateAssociation.CourseID = apICourseCertificateAssociation.CourseID;
                    CourseCertificateAssociation.CertificateImageName = apICourseCertificateAssociation.CertificateImageName;

                    await this._courseCertificateAssociationRepository.Update(CourseCertificateAssociation);
                    return Ok(apICourseCertificateAssociation);
                }
                return this.Ok(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCertificateNames/{title?}")]
        public async Task<IActionResult> GetAllCertificateNames(string title = null)
        {
            try
            {
                List<APICourseCertificateTypeHead> aPICourseCertificateTypeHeads = new List<APICourseCertificateTypeHead>();
                List<APICourseCertificateTypeHead> certificateNames = await _courseCertificateAssociationRepository.GetAllCertificateNames(title);
                foreach (APICourseCertificateTypeHead aPICourseCertificateTypeHead in certificateNames)
                {
                    APICourseCertificateTypeHead aPICourseCertificateType = new APICourseCertificateTypeHead();
                    aPICourseCertificateType.Name = aPICourseCertificateTypeHead.Name;
                    int index = aPICourseCertificateType.Name.LastIndexOf('/');
                    string CertificateImageName = null;
                    if (index != -1)
                        CertificateImageName = aPICourseCertificateType.Name.Substring(index + 1);
                    else if (index == -1)
                        CertificateImageName = aPICourseCertificateType.Name.Substring(index + 1);
                    aPICourseCertificateType.Name = CertificateImageName;
                    aPICourseCertificateType.Id = aPICourseCertificateTypeHead.Id;
                    aPICourseCertificateTypeHeads.Add(aPICourseCertificateType);
                }

                return Ok(aPICourseCertificateTypeHeads);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APICourseCertificateAssociation aPICourseCertificateAssociation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CourseCertificateAssociation coursecertificateAssociation = new CourseCertificateAssociation();
                    var orgcode = Record.OrgCode;
                    string OrgCode = Security.Decrypt(_identitySvc.GetOrgCode());
                    if (await this._courseCertificateAssociationRepository.Exists(aPICourseCertificateAssociation.CertificateImageName, aPICourseCertificateAssociation.CourseID, null, OrgCode))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        CourseCertificateAssociation obj = await this._courseCertificateAssociationRepository.CertificateExists(aPICourseCertificateAssociation.CourseID);
                        if (obj != null)
                        {
                            obj.CertificateImageName = aPICourseCertificateAssociation.CertificateImageName;
                            obj.Date = DateTime.UtcNow;
                            await _courseCertificateAssociationRepository.Update(obj);
                            return Ok(obj);
                        }
                        else
                        {
                            coursecertificateAssociation.CertificateImageName = aPICourseCertificateAssociation.CertificateImageName;
                            coursecertificateAssociation.CourseID = aPICourseCertificateAssociation.CourseID;
                            coursecertificateAssociation.CourseName = aPICourseCertificateAssociation.CourseName;
                            coursecertificateAssociation.Date = DateTime.UtcNow;
                            await _courseCertificateAssociationRepository.Add(coursecertificateAssociation);
                            return Ok(coursecertificateAssociation);
                        }
                    }

                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("PostImageUpload")]
        [PermissionRequired(Permissions.CourseCertificateMapping)]
        public async Task<IActionResult> PostImage()
        {

            try
            {
                string FileName = null;
                HttpRequest request = _httpContextAccessor.HttpContext.Request;
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.Image : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }


                        
                       
                        // allowed images
                        if (FileValidation.IsValidImage(fileUpload))
                        {
                           // var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
                            //if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            //{
                                string fileDir = this._configuration["CoursePathForExtract"];
                                fileDir = Path.Combine(fileDir, "img", OrganisationCode);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, fileUpload.FileName);

                                using (FileStream fs = new FileStream(Path.Combine(file), FileMode.Create))
                                {
                                    await fileUpload.CopyToAsync(fs);
                                }
                                if (String.IsNullOrEmpty(file))
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                }
                                string Filename = file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/");
                                return Ok(Filename);
                            //}
                            /*else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload,  "img",OrganisationCode);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            string file = res.Blob.Name.ToString();
                                            file = file.Replace(@"\", "/");
                                            return Ok(file.Substring(file.LastIndexOf("/" + OrganisationCode)));
                                            //return Ok("/" + file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }

                            }*/
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
      

        [HttpPost("CourseCertificateAuthority")]
        public async Task<IActionResult> Post([FromBody] APICourseCertificateAuthority aPICourseCertificateAuthority)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }
                else
                {
                    CourseCertificateAuthority coursecertificatauthoritye = new CourseCertificateAuthority();
                    coursecertificatauthoritye.CourseId = aPICourseCertificateAuthority.CourseId;
                    coursecertificatauthoritye.UserID = aPICourseCertificateAuthority.UserID;
                    if (aPICourseCertificateAuthority.DesignationID == 0)
                    {
                        coursecertificatauthoritye.DesignationID = null;
                    }
                    else
                    {
                        coursecertificatauthoritye.DesignationID = Convert.ToInt32(aPICourseCertificateAuthority.DesignationID);
                    }
                    coursecertificatauthoritye.IsDeleted = false;
                    coursecertificatauthoritye.IsActive = true;
                    coursecertificatauthoritye.ModifiedBy = UserId;
                    coursecertificatauthoritye.ModifiedDate = DateTime.UtcNow;
                    coursecertificatauthoritye.CreatedBy = UserId;
                    coursecertificatauthoritye.CreatedDate = DateTime.UtcNow;
                    await _courseCertificateAssociationRepository.AddcourseCertificate(coursecertificatauthoritye);

                    return Ok(aPICourseCertificateAuthority);

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }


        [HttpGet("GetAllCourseCertificateAssociation/{page:int}/{pageSize:int}/{search?}/{filter?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string filter = null)
        {
            try
            {
                var CompetencyLevels = await this._courseCertificateAssociationRepository.GetAllCourseCertificateAssociation(page, pageSize, search, filter);
                return Ok(Mapper.Map<List<APICourseCertificateAuthorityDetails>>(CompetencyLevels));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAllCourseCertificateAssociationCount/{search:minlength(0)?}/{filter?}")]
        public async Task<IActionResult> GetAllCourseCertificateAssociationCount(string search = null, string filter = null)
        {
            try
            {
                int count = await this._courseCertificateAssociationRepository.GetAllCourseCertificateAssociationCount(search, filter);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpDelete("DeleteCourseCertificateAssociation")]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await _courseCertificateAssociationRepository.DeleteRule(DecryptedId);
                if (Result == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                else
                    return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost]
        [Route("PostFileUpload")]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string fileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.FileType : request.Form[Record.FileType].ToString();

                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {

                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }


                        if (FileValidation.IsValidPdf(fileUpload) || FileValidation.IsValidImage(fileUpload))
                        {
                            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                            {
                                string fileDir = this._configuration["ApiGatewayLXPFiles"];
                                fileDir = Path.Combine(fileDir, OrgCode);
                                fileDir = Path.Combine(fileDir, fileType);
                                if (!Directory.Exists(fileDir))
                                {
                                    Directory.CreateDirectory(fileDir);
                                }
                                string file = Path.Combine(fileDir, DateTime.Now.Ticks + fileUpload.FileName);
                                using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                                {
                                    await fileUpload.CopyToAsync(fs);
                                }
                                if (String.IsNullOrEmpty(file))
                                {
                                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                                }
                                return Ok(file.Substring(file.LastIndexOf("\\" + OrgCode)).Replace(@"\", "/"));
                            }
                            else
                            {
                                try
                                {
                                    BlobResponseDto res = await _azurestorage.UploadAsync(fileUpload, OrgCode, fileType);
                                    if (res != null)
                                    {
                                        if (res.Error == false)
                                        {
                                            string file = res.Blob.Name.ToString();

                                            return Ok("/" + file.Replace(@"\", "/"));
                                        }
                                        else
                                        {
                                            _logger.Error(res.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
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

        [HttpPost("PostCertificationUpload")]
        public async Task<IActionResult> PostCertificationUpload([FromBody] ApiCertificationUpload apiCertificationUpload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                CertificationUpload certicationUpload = await this._courseCertificateAssociationRepository.CerticationUpload(apiCertificationUpload, UserId);
                return Ok(certicationUpload);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserCertificatesByUserId/{page:int}/{pageSize:int}/{filter?}/{search?}")]

        public async Task<IEnumerable<CertificationUpload>> GetUserCertificatesByUserId(int page, int pageSize, string filter = null, string search = null)
        {
            try
            {
                IEnumerable<CertificationUpload> userCertificates = await this._courseCertificateAssociationRepository.GetUserCertificatesByUserId(UserId, page, pageSize, filter,search);
                return userCertificates;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpGet("GetUserCertificatesByUserIdCount/{filter?}/{search:minlength(0)?}")]

        public async Task<IActionResult> GetUserCertificatesByUserIdCount(string filter, string search)
        {
            try
            {
                int Count = await this._courseCertificateAssociationRepository.GetUserCertificatesByUserIdCount(UserId,filter, search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("EditIntExtCourseCertificate/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ApiCertificationUpload apiCertificationUpload)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //Todo
                CertificationUpload courseIntExtCertificate = await this._courseCertificateAssociationRepository.GetCertificateById(id);
                if (courseIntExtCertificate == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                //Todo
                else
                {
                    if (apiCertificationUpload.Username == null)
                    {
                        courseIntExtCertificate.Username = courseIntExtCertificate.Username;
                    }
                    else
                    {
                        courseIntExtCertificate.Username = apiCertificationUpload.Username;
                    }
                    courseIntExtCertificate.Category = apiCertificationUpload.Category;
                    courseIntExtCertificate.Type = apiCertificationUpload.Type;
                    courseIntExtCertificate.TrainingCode = apiCertificationUpload.TrainingCode;
                    courseIntExtCertificate.TrainingName = apiCertificationUpload.TrainingName;
                    courseIntExtCertificate.TrainingMode = apiCertificationUpload.TrainingMode;
                    courseIntExtCertificate.TotalNoHours = apiCertificationUpload.TotalNoHours;
                    courseIntExtCertificate.NoofSessions = apiCertificationUpload.NoofSessions;
                    courseIntExtCertificate.StartDate = apiCertificationUpload.StartDate;
                    courseIntExtCertificate.EndDate = apiCertificationUpload.EndDate;
                    courseIntExtCertificate.BatchNo = apiCertificationUpload.BatchNo;
                    courseIntExtCertificate.PassPercentage = apiCertificationUpload.PassPercentage;
                    courseIntExtCertificate.TestScore = apiCertificationUpload.TestScore;
                    courseIntExtCertificate.Result = apiCertificationUpload.Result;
                    courseIntExtCertificate.Cost = apiCertificationUpload.Cost;
                    courseIntExtCertificate.Remark = apiCertificationUpload.Remark;
                    courseIntExtCertificate.PartnerName = apiCertificationUpload.PartnerName;
                    courseIntExtCertificate.CertificatePath = apiCertificationUpload.CertificatePath;

                    courseIntExtCertificate.ModifiedDate = DateTime.UtcNow;
                    courseIntExtCertificate.ModifiedBy = UserId;
/*                    courseIntExtCertificate.CreatedDate = DateTime.UtcNow;
                    courseIntExtCertificate.CreatedBy = UserId;*/
                    courseIntExtCertificate.IsDeleted = false;

                    if (apiCertificationUpload.UserId == null) {
                        courseIntExtCertificate.UserId = courseIntExtCertificate.UserId;
                    }
                    else
                    {
                        courseIntExtCertificate.UserId = (int)apiCertificationUpload.UserId;
                    }

                    await _courseCertificateAssociationRepository.UpdateIntExtCourseCertificate(courseIntExtCertificate);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("DeleteIntExtCourseCertificate/{id}")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            try
            {
                //int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await _courseCertificateAssociationRepository.DeleteCertificate(id, UserId);
                if (Result == 0)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                else
                    return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("EditCourseCertificateAssociation/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APICourseCertificateAuthority aPICourseCertificateAuthority)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                CourseCertificateAuthority courseCertificateAuthority = await this._courseCertificateAssociationRepository.GetCourseCertificateAuthorities(id);
                if (courseCertificateAuthority == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else
                {
                    courseCertificateAuthority.IsActive = true;
                    courseCertificateAuthority.CreatedBy = UserId;
                    courseCertificateAuthority.IsDeleted = false;
                    courseCertificateAuthority.ModifiedDate = DateTime.Now;
                    courseCertificateAuthority.DesignationID = aPICourseCertificateAuthority.DesignationID;
                    courseCertificateAuthority.CourseId = aPICourseCertificateAuthority.CourseId;
                    courseCertificateAuthority.UserID = aPICourseCertificateAuthority.UserID;
                    courseCertificateAuthority.Id = aPICourseCertificateAuthority.Id;
                    courseCertificateAuthority.ModifiedBy = UserId;
                    courseCertificateAuthority.ModifiedDate = DateTime.UtcNow;
                    await _courseCertificateAssociationRepository.UpdatecourseCertificate(courseCertificateAuthority);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #region Training Details Catalog

        [HttpPost("PostTrainingDetailsCatalog")]
        public async Task<IActionResult> PostTrainingDetailsCatalog([FromBody] APITrainingDetailsCatalog apiTrainingDetailsCatalog)
        {
            try
            {

                TrainingDetailsCatalog trainingDetailsCatalog = await this._courseCertificateAssociationRepository.PostTrainingDetailsCatalog(apiTrainingDetailsCatalog, UserId);
                return Ok(trainingDetailsCatalog);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTrainingDetails/{page:int}/{pageSize:int}/{filter?}/{search?}")]
        public async Task<IEnumerable<TrainingDetailsCatalog>> GetTrainingDetails(int page, int pageSize, string filter = null, string search = null)
        {
            try
            {
                IEnumerable<TrainingDetailsCatalog> trainingDetails = await this._courseCertificateAssociationRepository.GetTrainingDetails(UserId, page, pageSize, filter, search);
                return trainingDetails;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return null;
            }
        }

        [HttpGet("GetTrainingDetailsCount/{filter?}/{search:minlength(0)?}")]

        public async Task<IActionResult> GetTrainingDetailsCount(string filter, string search)
        {
            try
            {
                int Count = await this._courseCertificateAssociationRepository.GetTrainingDetailsCount(UserId, filter, search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("EditTrainingDetails/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APITrainingDetailsCatalog apiTrainingDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                TrainingDetailsCatalog trainingDetailsCatalog = await this._courseCertificateAssociationRepository.GetTrainingDetailsById(id);
                if (trainingDetailsCatalog == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else
                {

                    trainingDetailsCatalog.TrainingCode = apiTrainingDetails.TrainingCode;
                    trainingDetailsCatalog.TrainingName = apiTrainingDetails.TrainingName;

                    trainingDetailsCatalog.ModifiedDate = DateTime.UtcNow;
                    trainingDetailsCatalog.ModifiedBy = UserId;
                    /*                    courseIntExtCertificate.CreatedDate = DateTime.UtcNow;
                                        courseIntExtCertificate.CreatedBy = UserId;*/
                    trainingDetailsCatalog.IsDeleted = false;

                    await _courseCertificateAssociationRepository.UpdateTrainingDetailsCatalog(trainingDetailsCatalog);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("DeleteTrainingDetails/{id}")]
        public async Task<IActionResult> DeleteTrainingDetails(int id)
        {
            try
            {
                //int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await _courseCertificateAssociationRepository.TrainingDetails(id, UserId);
                if (Result == 0)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                else
                    return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("TypeAheadTrainingCatalog/{search?}")]
        public async Task<IActionResult> GetTrainingNameTypeAhead(string search = null)
        {
            try
            {
                List<APITrainingDetailsTypeAhead> trainingList = await _courseCertificateAssociationRepository.GetTrainingNameTypeAhead(search);
                return Ok(trainingList);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        #endregion

        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()
        {
            try
            {
                var CourseCertificates = await this._courseCertificateAssociationRepository.GetAllCourseCertificateAuthoritiesForExport(UserId);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "Certificate.xlsx";
                string URL = string.Concat(DomainName, "/", sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Courses");
                    //First add the headers

                    worksheet.Cells[1, 1].Value = "UserId";
                    worksheet.Cells[1, 2].Value = "CourseName";
                    worksheet.Cells[1, 3].Value = "AuthoristionName";
                    worksheet.Cells[1, 4].Value = "AuthoristionDesignationName";
                    worksheet.Cells[1, 5].Value = "CreatedDate";

                    int row = 2, column = 1;
                    foreach (APICourseCertificateExport CourseCertificate in CourseCertificates)
                    {
                        DateTime DateValue = new DateTime();

                        worksheet.Cells[row, column++].Value = CourseCertificate.UserId;
                        worksheet.Cells[row, column++].Value = CourseCertificate.CourseName;
                        worksheet.Cells[row, column++].Value = CourseCertificate.AuthoristionName;
                        worksheet.Cells[row, column++].Value = CourseCertificate.AuthoristionDesignationName;
                        worksheet.Cells[row, column++].Value = CourseCertificate.CreatedDate;

                        row++;
                        column = 1;
                    }
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
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
            }
            return null;
        }
        [HttpPost("DownloadCertificate")]
        public async Task<IActionResult> DownloadFile([FromBody] APIDownloadFile aPIDownloadFile)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                CertificationUpload courseIntExtCertificate = await this._courseCertificateAssociationRepository.GetCertificateById(aPIDownloadFile.Id);
                if (courseIntExtCertificate == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                if (!string.IsNullOrEmpty(courseIntExtCertificate.CertificatePath))
                {
                    string fileDir = this._configuration["ApiGatewayLXPFiles"];
                    string[] file = courseIntExtCertificate.CertificatePath.Split('/');
                    string filePath = Path.Combine(fileDir, file[1], file[2], file[3]);
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(filePath, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    return File(bytes, contentType, Path.GetFileName(filePath));
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ExportCertificate/{Search?}/{SearchText?}")]
        
        public async Task<IActionResult> ExportExtCertificateReport(string Search = null, string SearchText = null)
        {
            try
            {
                if (Search != null)
                    Search = Search.ToLower().Equals("null") ? null : Search;
                if (SearchText != null)
                    SearchText = SearchText.ToLower().Equals("null") ? null : SearchText;

                FileInfo ExcelFile;
                ExcelFile = await this._courseCertificateAssociationRepository.ExportExtCertificateReport(UserId, OrganisationCode, Search, SearchText, true);

                FileStream fs = ExcelFile.OpenRead();
                byte[] data = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    data = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(data, FileContentType.Excel, FileName.ExtCertificateReport);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}
