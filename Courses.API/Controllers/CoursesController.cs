using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using Courses.API.APIModel;
using Courses.API.APIModel.TigerhallIntegration;
using Courses.API.Common;
using Courses.API.ExternalIntegration.EdCast;
using Courses.API.Helper;
using Courses.API.Helper.Metadata;
using Courses.API.Model;
using Courses.API.Model.Competency;
using Courses.API.Model.Log_API_Count;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Courses.API.Repositories.Interfaces.Competency;
using Courses.API.Services;
using ExternalIntegration.MetaData;
using ExternalIntegration.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.EnumHelper;
using static Courses.API.Common.TokenPermissions;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CoursesController : IdentityController
    {
        ICourseRepository _courseRepository;
        IEmail _email;
        ISMS _sms;
        ICourseModuleAssociationRepository _coursesModuleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IConfiguration _configuration;
        private readonly ITokensRepository _tokensRepository;
        private ICompetenciesMasterRepository _competenciesMasterRepository;
        private ICompetenciesMappingRepository _competenciesMappingRepository;
        private ISubSubCategoryRepository _subSubCategoryRepository;
        private ICourseGroupMappingRepository _courseTeamMappingRepository;
        private ICourseAuthorAssociation _courseAuthorAssociation;
        private CourseContext _db;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CoursesController));
        private ICustomerConnectionStringRepository _customerConnectionString;
        private IModuleRepository _moduleRepository;
        IAzureStorage _azurestorage;
        public CoursesController
            (ICourseRepository courseRepository,
             ICourseModuleAssociationRepository coursesModuleRepository,
             IEmail email,
             ISMS sMs,
             IWebHostEnvironment hostingEnvironment,
             IHttpContextAccessor httpContextAccessor,
             IIdentityService _identitySvc,
             IConfiguration configuration,
             ICompetenciesMasterRepository competenciesMasterRepository,
             ICompetenciesMappingRepository competenciesMappingRepository,
             ISubSubCategoryRepository subSubCategoryRepository
            , ITokensRepository tokensRepository, CourseContext context,
             ICourseGroupMappingRepository courseTeamMappingRepository,
             ICustomerConnectionStringRepository customerConnectionString,
             IModuleRepository moduleRepository,
             IAzureStorage azurestorage,
        ICourseAuthorAssociation courseAuthorAssociation) : base(_identitySvc)
        {
            _courseRepository = courseRepository;
            _coursesModuleRepository = coursesModuleRepository;
            _email = email;
            _sms = sMs;
            _hostingEnvironment = hostingEnvironment;
            this._httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            this._tokensRepository = tokensRepository;
            _competenciesMasterRepository = competenciesMasterRepository;
            _competenciesMappingRepository = competenciesMappingRepository;
            _subSubCategoryRepository = subSubCategoryRepository;
             _courseTeamMappingRepository = courseTeamMappingRepository;
            _db = context;
            _customerConnectionString = customerConnectionString;
            _moduleRepository = moduleRepository;
            this._courseAuthorAssociation = courseAuthorAssociation;
            this._azurestorage = azurestorage;
        }

        [HttpGet]
        public async Task<IActionResult> Get()

        {
            throw new Exception();
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                string connectionString = ConnectionString;
                if (await _courseRepository.Get(id) == null)
                {
                    return NotFound();
                }
                return Ok(await _courseRepository.Get(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetCourseDetails/{id:int}")]
        public async Task<IActionResult> GetCourseDetails(int id)
        {
            try
            {
                if (await _courseRepository.Get(id) == null)
                {
                    return NotFound();
                }
                Courses.API.Model.Course course = await _courseRepository.Get(id);
                if (course != null)
                {
                    var config = new MapperConfiguration(cfg =>
                            cfg.CreateMap<Courses.API.Model.Course, CourseDetails>()
                       );
                    IMapper mapper = config.CreateMapper();

                    CourseDetails courseDetails = mapper.Map<CourseDetails>(course);

                    if (course.CategoryId != null)
                    {
                        Category category = await _db.Category.Where(a => a.Id == course.CategoryId).FirstOrDefaultAsync();
                        if (category != null)
                        {
                            courseDetails.CategoryName = category.Name;
                        }

                    }
                    if (course.SubCategoryId != null)
                    {
                        SubCategory subCategory = await _db.SubCategory.Where(a => a.Id == course.SubCategoryId).FirstOrDefaultAsync();
                        if (subCategory != null)
                        {
                            courseDetails.SubCategoryName = subCategory.Name;
                        }
                    }
                    return Ok(courseDetails);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page?}/{pageSize?}/{categoryId?}/{IsActive?}/{search?}/{filter?}")]
        [Produces(typeof(List<APICourseCategory>))]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> Get(int? page = null, int? pageSize = null, int? categoryId = null, bool? IsActive = null, string search = null, string filter = null)
        {
            try
            {
                List<APIAllCourses> course = await _courseRepository.GetAll(page, pageSize, categoryId, IsActive, search, filter);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetTagged/{page?}/{pageSize?}/{search?}")]
        [Produces(typeof(List<APICourseTagged>))]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> GetTagged(int? page = null, int? pageSize = null, string search = null)
        {
            try
            {
                var TaggList = await _courseRepository.GetTagged(page, pageSize, search);
                return Ok(Mapper.Map<List<APICourseTagged>>(TaggList));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> Post([FromBody] APIModel.APICourse course)
        {
            try
            {
                string Action = "insert";
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (course.Points == null)
                    course.Points = 0;

                if (course.Mission == null && course.Points > 0)
                {
                    return BadRequest();
                }

                if (course.CanAutoActivated != null)
                {
                    if (Convert.ToBoolean(course.CanAutoActivated))
                    {
                        if (course.StartDate == null || course.EndDate == null)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.CourseAssignmentDatesRequired) });

                        DateTime stdt = (DateTime)course.StartDate;
                        DateTime StartDate = new DateTime(stdt.Year, stdt.Month, stdt.Day, stdt.Hour, stdt.Minute, 0);
                        DateTime CurrentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                        if (StartDate < CurrentDate)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.PastCourseAssignmentDates) });

                        if (course.StartDate > course.EndDate)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidCourseAssignmentDates) });

                        if (course.IsVisibleAfterExpiry == null)
                            course.IsVisibleAfterExpiry = true;
                    }
                    else
                    {
                        course.IsVisibleAfterExpiry = true;
                        course.StartDate = null;
                        course.EndDate = null;
                    }
                }
                else
                {
                    course.CanAutoActivated = false;
                    course.IsVisibleAfterExpiry = true;
                    course.StartDate = null;
                    course.EndDate = null;
                }

                if (await _courseRepository.Exist(course.Title, course.Code, IsInstitute))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DuplicateCourseCodeOrTitle), Description = EnumHelper.GetEnumDescription(MessageType.DuplicateCourseCodeOrTitle) });
                else
                {
                    Model.Course _course = Mapper.Map<Model.Course>(course);

                    _course.CreatedDate = DateTime.Now;
                    _course.ModifiedDate = DateTime.Now;
                    _course.CreatedBy = UserId;
                    _course.ModifiedBy = UserId;
                    if (course.PreAssessmentId != null && course.PreAssessmentId != 0)
                        _course.IsPreAssessment = true;
                    else
                        _course.IsPreAssessment = false;

                    if (course.AssessmentId != null && course.AssessmentId != 0)
                        _course.IsAssessment = true;
                    else
                        _course.IsAssessment = false;

                    if (course.ManagerEvaluationId != null && course.ManagerEvaluationId != 0)
                        _course.IsManagerEvaluation = true;
                    else
                        _course.IsManagerEvaluation = false;


                    _course.IsDashboardCourse = course.IsDashboardCourse;

                    if (course.OJTId != null && course.OJTId != 0)
                        _course.IsOJT = true;
                    else
                        _course.IsOJT = false;

                    if (course.FeedbackId != null && course.FeedbackId != 0)
                        _course.IsFeedback = true;
                    else
                        _course.IsFeedback = false;

                    _course.TotalModules = course.ModuleAssociation.Length;

                    _course.IsModuleHasAssFeed = course.ModuleAssociation.Where(w => w.IsAssessment == true || w.IsFeedback == true || w.IsPreAssessment == true).Any();
                    // check course has module level assessment or feedback

                    await _courseRepository.Add(_course);
                    await _courseRepository.InsertCourseMasterAuditLog(_course, Action);
                    int courseId = _course.Id;
                    course.Id = _course.Id;
                    if (course.ModuleAssociation.Length == 0 && (course.CourseType.ToLower() == "vilt" || course.CourseType.ToLower() == "webinar" || course.CourseType.ToLower() == "classroom"))
                    {
                        Module class_viltmodule = new Module();
                        class_viltmodule.IsActive = true;
                        class_viltmodule.Name = course.Code + "_" + course.Title;
                        class_viltmodule.ModuleType = course.CourseType.ToLower();
                        class_viltmodule.CourseType = course.CourseType.ToLower();
                        class_viltmodule.IsMultilingual = false;
                        class_viltmodule.CreatedDate = DateTime.UtcNow;
                        class_viltmodule.ModifiedDate = DateTime.UtcNow;
                        class_viltmodule.CreatedBy = UserId;
                        class_viltmodule.ModifiedBy = UserId;
                        class_viltmodule.CreditPoints = class_viltmodule.LCMSId = null;
                        await _moduleRepository.Add(class_viltmodule);

                        ApiCourseModuleAssociation m = new ApiCourseModuleAssociation();
                        List<ApiCourseModuleAssociation> mlist = new List<ApiCourseModuleAssociation>();
                        m.CourseId = courseId;
                        m.ModuleId = class_viltmodule.Id;
                        m.IsAssessment = m.IsFeedback = m.IsPreAssessment = m.IsModified = false;
                        m.AssessmentId = m.FeedbackId = m.PreAssessmentId = m.SectionId = 0;
                        m.CompletionPeriodDays = null;
                        mlist.Add(m);
                        course.ModuleAssociation = mlist.ToArray();
                    }


                    IList<CourseModuleAssociation> modules = new List<CourseModuleAssociation>();
                    int SequenceNo = 0;

                    foreach (ApiCourseModuleAssociation m in course.ModuleAssociation)
                    {
                        if (modules.Where(c => c.ModuleId == m.ModuleId).Count() == 0)
                        {
                            CourseModuleAssociation module = new CourseModuleAssociation();

                            if (m.PreAssessmentId != null && m.PreAssessmentId != 0)
                                m.IsPreAssessment = true;
                            else
                                m.IsPreAssessment = false;

                            if (m.AssessmentId != null && m.AssessmentId != 0)
                                m.IsAssessment = true;
                            else
                                m.IsAssessment = false;

                            if (m.FeedbackId != null && m.FeedbackId != 0)
                                m.IsFeedback = true;
                            else
                                m.IsFeedback = false;

                            module.Id = 0;
                            module.CourseId = courseId;
                            module.ModuleId = m.ModuleId;
                            module.IsPreAssessment = m.IsPreAssessment;
                            module.PreAssessmentId = m.PreAssessmentId;
                            module.IsAssessment = m.IsAssessment;
                            module.AssessmentId = m.AssessmentId;
                            module.IsFeedback = m.IsFeedback;
                            module.FeedbackId = m.FeedbackId;
                            module.SectionId = m.SectionId;
                            module.SequenceNo = ++SequenceNo;

                            if (m.CompletionPeriodDays != null)
                                module.CompletionPeriodDays = Convert.ToInt32(m.CompletionPeriodDays);
                            else
                                module.CompletionPeriodDays = null;
                            modules.Add(module);
                        }
                    }

                    await _coursesModuleRepository.AddRange(modules);
                    var enable_Edcast = await _courseRepository.GetMasterConfigurableParameterValue("Enable_Edcast");

                    _logger.Debug("Enable_Edcast :-" + enable_Edcast);
                    if (Convert.ToString(enable_Edcast).ToLower() == "yes" && _course.PublishCourse == true)
                    {
                        APIEdcastDetailsToken result = await _courseRepository.GetEdCastToken(_course.LxpDetails);
                        if (result != null)
                        {

                            APIEdCastTransactionDetails obj = await _courseRepository.PostCourseToClient(_course.Id, UserId, result.access_token, _course.LxpDetails);
                        }
                        else
                        {
                            _logger.Debug("Token null from edcast");
                        }
                    }

                    var DarwinboxPost = await _courseRepository.GetMasterConfigurableParameterValue("Darwinbox_Post");
                    _logger.Debug("Darwinbox_Post :-" + DarwinboxPost);
                    if (Convert.ToString(DarwinboxPost).ToLower() == "yes")
                    {

                        APIDarwinTransactionDetails obj = await _courseRepository.PostCourseToDarwinbox(_course.Id, UserId, OrgCode);

                    }


                    await _coursesModuleRepository.CourseModuleAssociationAuditlog(modules, Action);


                    if (course.CompetencySkillsData != null) 
                    {
                        foreach (APIJobRole aPIRoleCompetency in course.CompetencySkillsData)
                        {
                            int competenciesMasterid = 0;
                            if (aPIRoleCompetency.Id != 0)
                            {
                                competenciesMasterid = Convert.ToInt32(aPIRoleCompetency.Id);
                            }
                            if (competenciesMasterid == 0)
                            {
                                CompetenciesMaster competenciesMasterExists = await _db.CompetenciesMaster.Where(x => x.CompetencyName == aPIRoleCompetency.Name && x.IsDeleted == false).FirstOrDefaultAsync();
                                if (competenciesMasterExists == null)
                                {
                                    CompetenciesMaster competenciesMaster = new CompetenciesMaster();
                                    competenciesMaster.CompetencyName = aPIRoleCompetency.Name;
                                    competenciesMaster.CompetencyDescription = aPIRoleCompetency.Name;
                                    competenciesMaster.CategoryId = 0;
                                    competenciesMaster.CreatedBy = UserId;
                                    competenciesMaster.CreatedDate = DateTime.Now;
                                    competenciesMaster.IsActive = true;
                                    competenciesMaster.IsDeleted = false;
                                    competenciesMaster.ModifiedBy = UserId;
                                    competenciesMaster.ModifiedDate = DateTime.Now;
                                    await this._competenciesMasterRepository.Add(competenciesMaster);

                                    await this._competenciesMasterRepository.CompetenciesMasterAuditlog(competenciesMaster, Action);

                                    competenciesMasterid = competenciesMaster.Id;
                                }
                                else
                                    competenciesMasterid = competenciesMasterExists.Id;
                            }
                            if (await this._competenciesMappingRepository.Exists(courseId, competenciesMasterid))
                            {
                                continue;
                            }
                            CompetenciesMapping competenciesMapping = new CompetenciesMapping();
                            competenciesMapping.CourseCategoryId = _course.CategoryId;
                            competenciesMapping.CourseId = courseId;
                            competenciesMapping.CompetencyId = competenciesMasterid;
                            competenciesMapping.IsDeleted = false;
                            competenciesMapping.ModifiedBy = UserId;
                            competenciesMapping.ModifiedDate = DateTime.Now;
                            competenciesMapping.CreatedBy = UserId;
                            competenciesMapping.CreatedDate = DateTime.Now;
                            await this._competenciesMappingRepository.AddRecord(competenciesMapping);
                            await this._competenciesMappingRepository.CompetenciesMappingAuditlog(competenciesMapping, Action);
                        }
                    }

                    if (course.subsubCategoryId != null)
                    {
                        foreach (APISubSubCategory apiSubSubCategory in course.subsubCategoryId)
                        {
                            int subSubCategoryId = Convert.ToInt32(apiSubSubCategory.Id);

                            ExternalCourseCategoryAssociation subSubCategoryMapping = new ExternalCourseCategoryAssociation();
                            int categoryId = Convert.ToInt32(course.CategoryId);
                            int subcategoryId = Convert.ToInt32(course.SubCategoryId);

                            subSubCategoryMapping.CourseId = courseId;
                            subSubCategoryMapping.CategoryId = categoryId;
                            subSubCategoryMapping.SubCategoryId = subcategoryId;
                            subSubCategoryMapping.SubSubCategoryId = subSubCategoryId;
                            subSubCategoryMapping.IsDeleted = false;
                            subSubCategoryMapping.ModifiedDate = DateTime.Now;
                            subSubCategoryMapping.CreatedDate = DateTime.Now;
                            _db.ExternalCourseCategoryAssociation.Add(subSubCategoryMapping);
                        }
                    }

                    Course_Details courseDetails = new Course_Details();
                    courseDetails.CourseID = course.Id;
                    courseDetails.CourseType = course.CourseType;
                    courseDetails.CreatedDate = DateTime.Now;
                    courseDetails.ModifiedDate = DateTime.Now;
                    courseDetails.ModifiedBy = UserId;
                    courseDetails.CreatedBy = UserId;
                    courseDetails.IsDeleted = false;
                    courseDetails.IsRefresherMandatory = course.isRefresherMandatory;
                    courseDetails.CourseInstructorID = null;
                    courseDetails.CourseOwnerID = null;
                    courseDetails.MobileNativeDeeplink = null;

                    try
                    {
                        _db.CourseDetails.Add(courseDetails);
                        _db.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                        _logger.Error(ex.Message);
                    }

                    if (course.courseAdminIDs != null)
                    {
                        List<CourseAuthorAssociation> courseAuthorAssociations = new List<CourseAuthorAssociation>();
                        foreach (int courseAdminID in course.courseAdminIDs)
                        {
                            CourseAuthorAssociation courseAuthorAssociation = new CourseAuthorAssociation();
                            courseAuthorAssociation.CourseId = courseId;
                            courseAuthorAssociation.UserId = courseAdminID;
                            courseAuthorAssociation.CreatedBy = UserId;
                            courseAuthorAssociation.CreatedDate = DateTime.Now;
                            courseAuthorAssociation.IsDeleted = 0;
                            courseAuthorAssociations.Add(courseAuthorAssociation);
                        }
                        await this._courseAuthorAssociation.AddRange(courseAuthorAssociations);

                    }

                    //if (RestrictGroup != null)
                    //{
                    //    foreach (int item in course.RestrictGroup)
                    //    {
                    //        int groupId = 0;
                    //        if (item != 0)
                    //        {
                    //            groupId = Convert.ToInt32(item);
                    //        }

                    //        if (await this._courseTeamMappingRepository.Exists(courseId, groupId))
                    //        {
                    //            continue;
                    //        }
                    //        CourseTeamMapping courseTeamMapping = new CourseTeamMapping();

                    //        courseTeamMapping.CourseId = courseId;
                    //        courseTeamMapping.UserTeamId = groupId;
                    //        courseTeamMapping.IsDeleted = false;
                    //        courseTeamMapping.ModifiedBy = UserId;
                    //        courseTeamMapping.ModifiedDate = DateTime.Now;
                    //        courseTeamMapping.CreatedBy = UserId;
                    //        courseTeamMapping.CreatedDate = DateTime.Now;
                    //        await this._courseTeamMappingRepository.AddRecord(courseTeamMapping);

                    //    }
                    //}

                    _logger.Debug("Sending Notifications.");
                    if (_course.IsApplicableToAll && _course.IsActive)
                    {
                        _logger.Debug("Course is Applicable to All and IsActive");
                        _ = Task.Run(() => _email.SendCourseAddedNotification(_course.Id, _course.Title, _course.Code, Token));
                        var val = await _email.SendCourseApplicabilityEmail(courseId, OrganisationCode);
                        var val1 = _email.SendCourseApplicabilityPushNotification(courseId, OrganisationCode);
                        var SendSMSToUser = await _courseRepository.GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");

                        _logger.Debug("SendSMSToUser :-" + SendSMSToUser);
                        if (Convert.ToString(SendSMSToUser).ToLower() == "yes")
                        {
                            string urlSMS = _configuration[Configuration.NotificationApi];
                            urlSMS += "/CourseApplicabilitySMS";
                            JObject oJsonObjectSMS = new JObject();
                            oJsonObjectSMS.Add("CourseId", _course.Id);
                            oJsonObjectSMS.Add("OrganizationCode", OrganisationCode);
                            HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                        }
                    }
                }
                return Ok(new { Id = course.Id, Title = course.Title });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        private async Task<HttpResponseMessage> CallAPI(string url, JObject oJsonObject)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = url;
                var response = await client.PostAsync(url, new StringContent(oJsonObject.ToString(), Encoding.UTF8, "application/json"));
                return response;
            }

        }
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> Put(int id, [FromBody] APIModel.APICourse course)
        {
            try
            {
                bool Flagadd = false;
                string Action = "update";
                DateTime CourseCreationdate;
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (course.Mission == null && course.Points > 0)
                {
                    return BadRequest();
                }

                if (course.CanAutoActivated != null)
                {
                    if (Convert.ToBoolean(course.CanAutoActivated))
                    {
                        if (course.StartDate == null || course.EndDate == null)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.CourseAssignmentDatesRequired) });

                        DateTime stdt = (DateTime)course.StartDate;
                        DateTime StartDate = new DateTime(stdt.Year, stdt.Month, stdt.Day, stdt.Hour, stdt.Minute, 0);
                        DateTime CurrentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                       // if (StartDate < CurrentDate)
                         //   return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.PastCourseAssignmentDates) });

                        if (course.StartDate > course.EndDate)
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidCourseAssignmentDates) });

                        if (course.IsVisibleAfterExpiry == null)
                            course.IsVisibleAfterExpiry = true;
                    }
                    else
                    {
                        //using value received by UI + used with course completion days logic GetCourseInfo
                        course.IsVisibleAfterExpiry = course.IsVisibleAfterExpiry;
                        course.StartDate = null;
                        course.EndDate = null;
                    }
                }
                else
                {
                    //using value received by UI + used with course completion days logic GetCourseInfo
                    course.CanAutoActivated = false;
                    course.IsVisibleAfterExpiry = course.IsVisibleAfterExpiry;
                    course.StartDate = null;
                    course.EndDate = null;
                }

                Model.Course _course = await _courseRepository.Get(id);
                if (course.Code != _course.Code)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (course.IsFeedbackOptional != _course.IsFeedbackOptional && await _courseRepository.IsDependacyExist(id))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.CannotUpdate), Description = EnumHelper.GetEnumDescription(MessageType.CannotUpdate) });

                if (course.LearningApproach != _course.LearningApproach && await _courseRepository.IsDependacyExist(id))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.CannotUpdateLearningApproach), Description = EnumHelper.GetEnumDescription(MessageType.CannotUpdateLearningApproach) });

                if (_course == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (await _courseRepository.Exist(course.Title, course.Code, IsInstitute, course.Id))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                int createdBy = 0;
                int OldTotalModules = _course.TotalModules;
                createdBy = _db.Course.Where(c => c.Id == id).Select(c => c.CreatedBy).FirstOrDefault();
                Model.Course oldCourse = Mapper.Map<Model.Course>(_course);

                if (!_course.IsActive && course.IsActive)
                    CourseCreationdate = DateTime.UtcNow;
                else
                    CourseCreationdate = _course.CreatedDate;

                if (course.PreAssessmentId != null && course.PreAssessmentId != 0)
                    _course.IsPreAssessment = true;
                else
                    _course.IsPreAssessment = false;

                if (course.AssessmentId != null && course.AssessmentId != 0)
                    _course.IsAssessment = true;
                else
                    _course.IsAssessment = false;

                if (course.FeedbackId != null && course.FeedbackId != 0)
                    _course.IsFeedback = true;
                else
                    _course.IsFeedback = false;

                if (course.AssignmentId != null && course.AssignmentId != 0)
                    _course.IsAssignment = true;
                else
                    _course.IsAssignment = false;

                _course = Mapper.Map<Model.Course>(course);
                _course.ModifiedDate = DateTime.Now;
                _course.ModifiedBy = UserId;
                _course.CreatedDate = CourseCreationdate;
                _course.TotalModules = course.ModuleAssociation.Length;
                _course.CreatedBy = string.IsNullOrEmpty(Convert.ToString(createdBy)) ? UserId : createdBy;

                if (course.ManagerEvaluationId != null && course.ManagerEvaluationId != 0)
                    _course.IsManagerEvaluation = true;
                else
                    _course.IsManagerEvaluation = false;

                if (course.OJTId != null && course.OJTId != 0)
                    _course.IsOJT = true;
                else
                    _course.IsOJT = false;

                if (_course.TotalModules <= 0 && course.IsActive) // allow active course with content
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NoContentAdded), Description = EnumHelper.GetEnumDescription(MessageType.NoContentAdded) });
                }

                if (oldCourse.Title != _course.Title)
                {
                    if (_course.IsApplicableToAll)
                        await _courseRepository.UpdateCourseNotification(oldCourse, _course.Id, _course.Title, _course.Code, Token);
                }


                #region "Code added to rollback notifications in case the course is now made inActive. New Users should not get notifications."
                if ((oldCourse.IsApplicableToAll == true && _course.IsApplicableToAll == false))
                    await _courseRepository.UpdateCourseNotification(oldCourse, _course.Id, _course.Title, Token);
                #endregion

                #region "Code"
                if (oldCourse.IsActive == false && course.IsActive == true && course.isApplicableToAll == true)
                    _ = Task.Run(() => _email.SendCourseAddedNotification(_course.Id, _course.Title, _course.Code, Token));
                #endregion

                _course.IsModuleHasAssFeed = course.ModuleAssociation.Where(w => w.IsAssessment == true || w.IsFeedback == true || w.IsPreAssessment).Any();

                if (await _courseRepository.CreateCourseFromExisting(id))
                {
                    IEnumerable<ApiCourseModuleAssociation> existingmodules = await _courseRepository.GetModules(id);
                    if (_course.TotalModules == 0)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NoContentAdded), Description = EnumHelper.GetEnumDescription(MessageType.NoContentAdded) });

                    }
                    if (_course.TotalModules != OldTotalModules)
                    {
                        _course.Code = oldCourse.Code + "_" + DateTime.Now.ToString("ddMMyy_hhmm");
                        _course.Title = oldCourse.Title + "_" + DateTime.Now.ToString("ddMMyy_hhmm");
                        await _courseRepository.Update(_course);
                        _course.Code = oldCourse.Code;
                        _course.Title = oldCourse.Title;
                        _course.Id = 0;
                        await _courseRepository.Add(_course);
                        Flagadd = true;
                    }
                    else
                    {
                        // check for same modules count in both old and new but module replaced

                        int modulecount = course.ModuleAssociation.Count();
                        int matchcount = 0;
                        foreach (ApiCourseModuleAssociation item in existingmodules)
                        {
                            for (int i = 0; i < modulecount; i++)
                            {
                                if (course.ModuleAssociation[i].ModuleId == item.ModuleId)
                                {
                                    matchcount++;
                                }
                            }

                        }
                        if (matchcount == modulecount)
                        {
                            await _courseRepository.Update(_course);
                        }
                        else
                        {
                            _course.Code = oldCourse.Code + "_" + DateTime.Now.ToString("ddMMyy_hhmm");
                            _course.Title = oldCourse.Title + "_" + DateTime.Now.ToString("ddMMyy_hhmm");
                            await _courseRepository.Update(_course);
                            _course.Code = oldCourse.Code;
                            _course.Title = oldCourse.Title;
                            _course.Id = 0;
                            await _courseRepository.Add(_course);
                            Flagadd = true;
                        }
                    }
                }
                else
                {
                    await _courseRepository.Update(_course);
                }
                //Course Master Audit Log
                await _courseRepository.InsertCourseMasterAuditLog(_course, Action);
                //END
                int courseId = _course.Id;
                List<CourseModuleAssociation> modules = new List<CourseModuleAssociation>();
                List<CourseModuleAssociation> oldmodules = new List<CourseModuleAssociation>();
                int SequenceNo = 0;
                foreach (ApiCourseModuleAssociation m in course.ModuleAssociation)
                {
                    SequenceNo++;
                    if (m.IsModified == false && _course.Id == id)
                        continue;
                    if (modules.Where(c => c.ModuleId == m.ModuleId).Count() != 0) //used for checking duplicate modules in array 
                        continue;

                    CourseModuleAssociation module = await _coursesModuleRepository.Get(m.Id);
                    var oldModule = Mapper.Map<CourseModuleAssociation>(module);
                    oldmodules.Add(oldModule);
                    if (module == null)
                        module = new CourseModuleAssociation();
                    else
                    {
                        if (m.IsDeleted == true)
                        {
                            await _coursesModuleRepository.Remove(module);
                            continue;
                        }
                    }

                    if (m.PreAssessmentId != null && m.PreAssessmentId != 0)
                        m.IsPreAssessment = true;
                    else
                        m.IsPreAssessment = false;

                    if (m.AssessmentId != null && m.AssessmentId != 0)
                        m.IsAssessment = true;
                    else
                        m.IsAssessment = false;

                    if (m.FeedbackId != null && m.FeedbackId != 0)
                        m.IsFeedback = true;
                    else
                        m.IsFeedback = false;

                    module.CourseId = courseId;
                    module.ModuleId = m.ModuleId;
                    module.IsPreAssessment = m.IsPreAssessment;
                    module.PreAssessmentId = m.PreAssessmentId;
                    module.IsAssessment = m.IsAssessment;
                    module.AssessmentId = m.AssessmentId;
                    module.IsFeedback = m.IsFeedback;
                    module.FeedbackId = m.FeedbackId;
                    module.SectionId = m.SectionId;
                    module.SequenceNo = SequenceNo;
                    if (m.CompletionPeriodDays != null)
                        module.CompletionPeriodDays = Convert.ToInt32(m.CompletionPeriodDays);
                    else
                        module.CompletionPeriodDays = null;

                    if (Flagadd == true)
                    {
                        module.Id = 0;
                    }

                    modules.Add(module);
                }
                if (Flagadd == true)
                {
                    await _coursesModuleRepository.AddRange(modules);
                }
                else
                {
                    await _coursesModuleRepository.UpdateRange(modules);
                }


                var enable_Edcast = await _courseRepository.GetMasterConfigurableParameterValue("Enable_Edcast");
                _logger.Debug("Enable_Edcast :-" + enable_Edcast);
                if (Convert.ToString(enable_Edcast).ToLower() == "yes" && _course.PublishCourse == true)
                {
                    APIEdcastDetailsToken result = await _courseRepository.GetEdCastToken(_course.LxpDetails);
                    if (result != null)
                    {
                        APIEdCastTransactionDetails obj = await _courseRepository.PostCourseToClient(_course.Id, UserId, result.access_token, _course.LxpDetails);
                    }
                    else
                    {
                        _logger.Debug("Token null from edcast");
                    }
                }

                var DarwinboxPost = await _courseRepository.GetMasterConfigurableParameterValue("Darwinbox_Post");
                _logger.Debug("Darwinbox_Post :-" + DarwinboxPost);
                if (Convert.ToString(DarwinboxPost).ToLower() == "yes")
                {

                    APIDarwinTransactionDetails obj = await _courseRepository.PostCourseToDarwinbox(_course.Id, UserId, OrgCode);

                }

                // CourseMaster Audit Log 
                await _coursesModuleRepository.CourseModuleAuditlog(modules, Action);
                //END

                await this._courseRepository.AddCourseHistory(oldCourse, _course, oldmodules, modules);

                // change for add competency skill mapping - start   //
                List<int> competencyskillIdlist = new List<int>();
                if (course.CompetencySkillsData != null)
                {
                    foreach (APIJobRole aPIRoleCompetency in course.CompetencySkillsData)
                    {

                        int competenciesMasterid = 0;
                        if (aPIRoleCompetency.Id != 0)
                        {
                            competenciesMasterid = Convert.ToInt32(aPIRoleCompetency.Id);
                        }

                        if (competenciesMasterid == 0)
                        {
                            CompetenciesMaster competenciesMaster = new CompetenciesMaster();
                            competenciesMaster.CompetencyName = aPIRoleCompetency.Name;
                            competenciesMaster.CompetencyDescription = aPIRoleCompetency.Name;
                            competenciesMaster.CategoryId = 0;
                            competenciesMaster.CreatedBy = UserId;
                            competenciesMaster.CreatedDate = DateTime.Now;
                            competenciesMaster.IsActive = true;
                            competenciesMaster.IsDeleted = false;
                            competenciesMaster.ModifiedBy = UserId;
                            competenciesMaster.ModifiedDate = DateTime.Now;
                            await this._competenciesMasterRepository.Add(competenciesMaster);

                            await this._competenciesMasterRepository.CompetenciesMasterAuditlog(competenciesMaster, Action);

                            competenciesMasterid = competenciesMaster.Id;
                        }

                        competencyskillIdlist.Add(Convert.ToInt32(competenciesMasterid));

                        if (await this._competenciesMappingRepository.Exists(courseId, competenciesMasterid))
                        {
                            continue;
                        }
                        CompetenciesMapping competenciesMapping = new CompetenciesMapping();

                        competenciesMapping.CourseCategoryId = _course.CategoryId;
                        competenciesMapping.CourseId = courseId;
                        competenciesMapping.CompetencyId = competenciesMasterid;
                        competenciesMapping.IsDeleted = false;
                        competenciesMapping.ModifiedBy = UserId;
                        competenciesMapping.ModifiedDate = DateTime.UtcNow;
                        competenciesMapping.CreatedBy = UserId;
                        competenciesMapping.CreatedDate = DateTime.UtcNow;
                        await this._competenciesMappingRepository.AddRecord(competenciesMapping);
                        await this._competenciesMappingRepository.CompetenciesMappingAuditlog(competenciesMapping, Action);
                    }

                }
                int[] CurrentCompetencies = competencyskillIdlist.ToArray();
                int[] aPIOldCompetenciesId = await this._competenciesMappingRepository.getCompIdByCourseId(Convert.ToInt32(courseId));
                this._competenciesMappingRepository.FindElementsNotInArray(CurrentCompetencies, aPIOldCompetenciesId, Convert.ToInt32(courseId));

               
                
                var apiOldSubSubCategoryId = await this._courseRepository.GetSubSubCategory(courseId);
                int[] oldsubsubcatId = new int[apiOldSubSubCategoryId.Count()];
                int count = 0;
                foreach (APISubSubCategory subsubcat in apiOldSubSubCategoryId)
                {
                    oldsubsubcatId[count] = subsubcat.Id;
                    count++;
                }
                List<int> subSubCategoryIdlist = new List<int>();
                if (course.subsubCategoryId != null)
                { 
                    foreach (APISubSubCategory apiSubSubCategory in course.subsubCategoryId)
                    {
                        int apisubsubctegoryid = 0;
                        if (apiSubSubCategory.Id != 0)
                        {
                            apisubsubctegoryid = Convert.ToInt32(apiSubSubCategory.Id);
                        }

                        subSubCategoryIdlist.Add(apisubsubctegoryid);

                        int subSubCategoryId = Convert.ToInt32(apiSubSubCategory.Id);

                        if (!oldsubsubcatId.Contains(apiSubSubCategory.Id))
                        {
                            ExternalCourseCategoryAssociation subSubCategoryMapping = new ExternalCourseCategoryAssociation();
                            int categoryId = Convert.ToInt32(course.CategoryId);
                            int subcategoryId = Convert.ToInt32(course.SubCategoryId);

                            subSubCategoryMapping.CourseId = courseId;
                            subSubCategoryMapping.CategoryId = categoryId;
                            subSubCategoryMapping.SubCategoryId = subcategoryId;
                            subSubCategoryMapping.SubSubCategoryId = subSubCategoryId;
                            subSubCategoryMapping.IsDeleted = false;
                            subSubCategoryMapping.ModifiedDate = DateTime.Now;
                            subSubCategoryMapping.CreatedDate = DateTime.Now;
                            _db.ExternalCourseCategoryAssociation.Add(subSubCategoryMapping);
                        }
                    }
                }
                int[] CurrentSubSubCategory = subSubCategoryIdlist.ToArray();
                this._subSubCategoryRepository.FindElementsNotInArray(CurrentSubSubCategory, oldsubsubcatId, Convert.ToInt32(courseId));





                if (course.courseAdminIDs != null)
                {
                    List<CourseAuthorAssociation> courseAuthorAssociations = new List<CourseAuthorAssociation>();
                    foreach (int courseAdminID in course.courseAdminIDs)
                    {
                        CourseAuthorAssociation author = await this._courseAuthorAssociation.RecordExists(courseId, courseAdminID);
                        if (author != null)
                        {
                            if (author.IsDeleted == 1)
                            {
                                author.IsDeleted = 0;
                                await _courseAuthorAssociation.Update(author);
                            }
                            else
                            { continue; }
                        }
                        else
                        {
                            CourseAuthorAssociation courseAuthorAssociation = new CourseAuthorAssociation();
                            courseAuthorAssociation.CourseId = courseId;
                            courseAuthorAssociation.UserId = courseAdminID;
                            courseAuthorAssociation.CreatedBy = UserId;
                            courseAuthorAssociation.CreatedDate = DateTime.Now;
                            courseAuthorAssociation.IsDeleted = 0;
                            courseAuthorAssociations.Add(courseAuthorAssociation);
                        }
                    }
                    await this._courseAuthorAssociation.AddRange(courseAuthorAssociations);
                    int[] CurrentAuthors = course.courseAdminIDs.ToArray();
                    int[] oldAuthors = await this._courseAuthorAssociation.getAuthorsCourseId(Convert.ToInt32(courseId));
                    this._courseAuthorAssociation.FindElementsNotInArray(CurrentAuthors, oldAuthors, Convert.ToInt32(courseId));

                }


                // change for add competency skill mapping  -- end  //


                // change for add course team mapping - start   //

                //List<int> TeamIdlist = new List<int>();
                //if (course.RestrictGroup != null)
                //{
                //    foreach (int item in course.RestrictGroup)
                //    {

                //        int groupId = 0;
                //        if (item != 0)
                //        {
                //            groupId = Convert.ToInt32(item);
                //        }


                //        TeamIdlist.Add(Convert.ToInt32(groupId));

                //        if (await this._courseTeamMappingRepository.Exists(courseId, groupId))
                //        {
                //            continue;
                //        }
                //        CourseTeamMapping courseTeamMapping = new CourseTeamMapping();

                //        courseTeamMapping.CourseId = courseId;
                //        courseTeamMapping.UserTeamId = groupId;
                //        courseTeamMapping.IsDeleted = false;
                //        courseTeamMapping.ModifiedBy = UserId;
                //        courseTeamMapping.ModifiedDate = DateTime.Now;
                //        courseTeamMapping.CreatedBy = UserId;
                //        courseTeamMapping.CreatedDate = DateTime.Now;
                //        await this._courseTeamMappingRepository.AddRecord(courseTeamMapping);

                //    }

                //}
                //int[] CurrentTeams = TeamIdlist.ToArray();
                //int[] aPIOldTeamId = await this._courseTeamMappingRepository.getCompIdByCourseId(Convert.ToInt32(courseId));
                //this._courseTeamMappingRepository.FindElementsNotInArray(CurrentTeams, aPIOldTeamId, Convert.ToInt32(courseId));

                // change for add competency skill mapping  -- end  //

                if (course.IsActive)
                {
                    var EmailNotificationOnCourseEdit = await _courseRepository.GetMasterConfigurableParameterValue("EMAIL_NOTIFICATION_ON_COURSE_EDIT");
                    _logger.Debug("Value of config EMAIL_NOTIFICATION_ON_COURSE_EDIT: " + EmailNotificationOnCourseEdit);
                    if (Convert.ToString(EmailNotificationOnCourseEdit).ToLower() == "yes")
                    {
                        _logger.Debug("Sending Course Applicability Email");
                        var val = await _email.SendCourseApplicabilityEmail(courseId, OrganisationCode);
                        _logger.Debug("Sending Course Applicability Push Notification");
                        var val1 = _email.SendCourseApplicabilityPushNotification(courseId, OrganisationCode);
                    }
                }

                var SendSMSToUser = _courseRepository.GetMasterConfigurableParameterValue("SMS_FOR_APPLICABILITY");
                if (Convert.ToString(SendSMSToUser).ToLower() == "yes" && course.IsActive == true && _course.IsActive == false && course.isApplicableToAll == true && _course.IsApplicableToAll == false)
                {
                    string urlSMS = _configuration[Configuration.NotificationApi];
                    urlSMS += "/CourseApplicabilitySMS";
                    JObject oJsonObjectSMS = new JObject();
                    oJsonObjectSMS.Add("CourseId", _course.Id);
                    oJsonObjectSMS.Add("OrganizationCode", OrganisationCode);
                    HttpResponseMessage responsesSMS = CallAPI(urlSMS, oJsonObjectSMS).Result;
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedcourseId = Convert.ToInt32(Security.Decrypt(id));
                Message Result = await this._courseRepository.DeleteCourse(DecryptedcourseId, Token);
                if (Result == Message.Success)
                    return Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(MessageType.Success) });
                else if (Result == Message.NotFound)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                else if (Result == Message.DependencyExist)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = "Course is in use" });
                else if (Result == Message.CannotDelete)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.CannotDelete), Description = EnumHelper.GetEnumDescription(MessageType.CannotDelete) });

                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("IsCourseForAllowModification/{id}")]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> CheckCourseForAllowModification(int id)
        {
            try
            {
                return Ok(await _courseRepository.CheckCourseForAllowModification(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("GetCoursesModules/{id}")]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> GetCoursesModules(int id)
        {
            try
            {
                return Ok(await _courseRepository.GetModules(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TypeAhead/{search?}/{categoryid?}/{courseType?}")]
        [PermissionRequired(Permissions.coursewise_reminder + " " + Permissions.course_completion_status_report + " " + Permissions.usermanagment + " "
            + Permissions.attendance_summary_report + " " + Permissions.coursewise_completion_status_report + " " + Permissions.feedback_status_report + " " + Permissions.feedback_aggregation_report + Permissions.training_atendance_report + " " + Permissions.user_learning_report)]
        public async Task<IActionResult> Get(string search = null, int? categoryid = null, string courseType = null)
        {
            try
            {
                List<APICourseDTO> course = await _courseRepository.GetCourse(categoryid, search, courseType);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TypeAheadReport/{search?}/{categoryid?}/{courseType?}")]
        //[PermissionRequired(Permissions.coursewise_reminder + " " + Permissions.course_completion_status_report + " " + Permissions.usermanagment + " "
        // + Permissions.attendance_summary_report + " " + Permissions.coursewise_completion_status_report + " " + Permissions.feedback_status_report + " " + Permissions.feedback_aggregation_report + Permissions.training_atendance_report + " " + Permissions.user_learning_report + " " + Permissions.ProgramAttendanceReport)]
        public async Task<IActionResult> GetReportCourse(string search = null, string courseType = null)
        {
            try
            {
                List<APICoursesData> course = await _courseRepository.GetReportCourse(UserId, RoleCode, search, courseType);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("SearchCourseTypeAhead/{search?}/{courseType?}")]
        public async Task<IActionResult> SearchCourseTypeAhead(string search = null, string courseType = null)
        {
            try
            {
                List<APICourseDTO> course = await _courseRepository.CourseTypehead(search, courseType);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("ILTCoursesTypeAhead/{search?}/{categoryid?}/{courseType?}")]
        public async Task<IActionResult> getILTCourses(string search = null, int? categoryid = null)
        {
            try
            {
                List<APICourseDTO> course = await _courseRepository.GetILTCourse(categoryid, search);
                if (course.Count == 0)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseTypeahead/{categoryid?}")]
        public async Task<IActionResult> GetCourseTypeahead(int? categoryid)
        {
            try
            {
                List<APICourseDTO> course = await _courseRepository.GetCourseTypeahead(categoryid);
                if (course.Count == 0)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ApplicableToAllCourseTypeahead/{search?}")]
        public async Task<IActionResult> ApplicableToAllCourseTypeahead(string search = null, int? categoryid = null)
        {
            try
            {
                List<APICourseDTO> course = await _courseRepository.ApplicableToAllCourseTypeahead(search);
                if (course.Count == 0)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("count/{categoryId?}/{IsActive?}/{search?}/{filter?}")]
        [Produces(typeof(List<APICourseCategory>))]
        public async Task<IActionResult> GetCount(int? categoryId = null, bool? IsActive = null, string search = null, string filter = null)
        {
            try
            {
                int count = await _courseRepository.Count(categoryId, IsActive, search, filter);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("UploadThumbnail")]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> UploadThumbnail()
        {
            try
            {
                string fileName = string.Empty;
                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile uploadedFile in request.Form.Files)
                    {
                        string filePath = this._configuration["ApiGatewayWwwroot"];
                        filePath = Path.Combine(filePath, OrganisationCode);
                        string DomainName = this._configuration["ApiGatewayUrl"];
                        filePath = Path.Combine(filePath, FileType.Thumbnail);
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                        fileName = string.Concat(fileName.Split(' '));
                        filePath = Path.Combine(filePath, fileName);
                        filePath = string.Concat(filePath.Split(' '));
                        Courses.API.Common.Helper helper = new Courses.API.Common.Helper();
                        await helper.SaveFile(uploadedFile, filePath);
                        var uri = new System.Uri(filePath);
                        filePath = uri.AbsoluteUri;
                        String ThumbPath = string.Concat(DomainName, filePath.Substring(filePath.LastIndexOf(OrganisationCode)));
                        return Ok(ThumbPath);
                    }
                }
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetModules/{courseId}")]
        public async Task<IActionResult> GetModules(int courseId)
        {
            try
            {
                List<APIModuleTypeAhead> course = await _courseRepository.GetCourseModules(courseId);
                if (course.Count == 0)
                {
                    return NotFound();
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("DeleteCoursesModules")]
        public async Task<IActionResult> DeleteCoursesModule([FromQuery] string CourseId, string ModuleId)
        {
            try
            {
                int DecryptedCourseId = Convert.ToInt32(Security.Decrypt(CourseId));
                int DecryptedModuleId = Convert.ToInt32(Security.Decrypt(ModuleId));
                IQueryable<Courses.API.Model.CourseModuleAssociation> coursesModuleAssociation = _coursesModuleRepository.GetAssociationCourseModule(DecryptedCourseId, DecryptedModuleId);
                if (coursesModuleAssociation == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                else if (await _courseRepository.CheckCourseForAllowModification(DecryptedCourseId) == false)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotAllowedDelete), Description = EnumHelper.GetEnumDescription(MessageType.NotAllowedDelete) });
                }
                await _coursesModuleRepository.Remove(coursesModuleAssociation.FirstOrDefault());
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseName/{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> CourseName(int courseId)
        {
            try
            {
                string name = await _courseRepository.GetCourseNam(courseId);
                return Ok(name);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetModuleName/{courseId}")]
        [AllowAnonymous]

        public async Task<IActionResult> GetModuleNam(int courseId)
        {
            try
            {
                string name = await _courseRepository.GetModuleName(courseId);
                return Ok(name);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("Upload")]
        public async Task<IActionResult> ProfilePictureUpload([FromBody] APIPictureProfile pictureProfile)

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
                    string fileDir = this._configuration["ApiGatewayWwwroot"];
                    fileDir = Path.Combine(fileDir, OrganisationCode);
                    string DomainName = this._configuration["ApiGatewayUrl"];
                    fileDir = Path.Combine(fileDir, "thumbnail", "course");
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
                    BlobResponseDto res = await _azurestorage.CreateBlob( bytes, OrgCode, "course","thumbnail");
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
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("CourseNameList/{courseId}")]
        public async Task<IActionResult> CourseNameList(string courseId)
        {
            try
            {
                return Ok(await _courseRepository.GetCourseNameList(courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetCourseId/{courseCode}")]
        public async Task<IActionResult> GetCourseId(string courseCode)
        {
            try
            {
                return Ok(await _courseRepository.GetCourseId(courseCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetModulesAssessment")]
        public async Task<IActionResult> GetModulesAssessment()
        {
            try
            {
                List<APICourseDTO> course = await _courseRepository.GetModulesAssessment();
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpGet("GetAssessmentConfigurationID/{CourseId}/{modelId}/{isPreassessment?}/{isContentAssessment?}")]
        public async Task<IActionResult> GetAssessmentConfigurationID(int CourseId, int modelId, bool isPreAssessment = false, bool isContentAssessment = false)
        {
            try
            {
                if (isPreAssessment == true && isContentAssessment == true)
                    return BadRequest();

                return Ok(await _courseRepository.GetAssessmentConfigurationID(CourseId, modelId, OrganisationCode, isPreAssessment, isContentAssessment));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetFeedbackConfigurationID/{modelId}")]
        public async Task<IActionResult> GetFeedbackConfigurationID(int modelId)
        {
            try
            {
                string ID = await _courseRepository.GetFeedbackConfigurationID(modelId);
                return Ok(ID);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TypeAheadForAccessibility/{search?}/{categoryid?}")]
        public async Task<IActionResult> GetTypeAheadForAccessibility(string search = null, int? categoryid = null)
        {
            try
            {
                return Ok(await _courseRepository.GetCourseForAccessibility(categoryid, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCoursesAssessmentFeedbackName/{assessmentId?}/{feedbackId?}/{preassessmentId?}/{assignmentId?}/{managerEvaluationId?}/{ojtId?}")]
        public async Task<IActionResult> GetCoursesAssessmentFeedback(int? assessmentId = null, int? feedbackId = null, int? preassessmentId = null, int? assignmentId = null, int? managerEvaluationId = null, int? ojtId = null)
        {
            try
            {
                return Ok(await _courseRepository.GetCoursesAssessmentFeedbackName(assessmentId, feedbackId, preassessmentId, assignmentId, managerEvaluationId,ojtId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCoursesAssignment/{assignmentId?}")]
        public async Task<IActionResult> GetCoursesAssignment(int? assignmentId = null)
        {
            try
            {
                return Ok(await _courseRepository.GetCoursesAssignment(assignmentId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("CourseCode")]
        public async Task<IActionResult> GetCoursesAssessmentFeedback()
        {
            try
            {
                CourseCode CourseCode = await _courseRepository.GetCourseCode();
                string Code = CourseCode.Prefix + CourseCode.AutoNumber;
                return Ok(Code);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("CourseTypeAheadAssessment/{search?}/{categoryid?}/{courseType?}")]
        public async Task<IActionResult> GetAssessmentCourse(string search = null, int? categoryid = null, string courseType = null)
        {
            try
            {
                List<object> course = await _courseRepository.GetAssessmentCourse(categoryid, search, courseType);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TypeAheadAssessmentCourse/{search?}/{categoryid?}")]
        public async Task<IActionResult> GetAssessmentTypeCourse(string search = null, int? categoryid = null)
        {
            try
            {
                List<object> course = await _courseRepository.GetAssessmentTypeCourse(categoryid, search);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("TypeAheadAuthority/{search?}/{categoryid?}")]
        public async Task<IActionResult> TypeAheadAuthority(string search = null, int? categoryid = null)
        {
            try
            {
                List<object> course = await _courseRepository.TypeAheadAuthority(categoryid, search);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("CourseTypeAheadAssessmentReport/{search?}/{categoryid?}/{courseType?}")]
        public async Task<IActionResult> GetAssessmentCourseReport(string search = null, int? categoryid = null, string courseType = null)
        {
            try
            {
                List<object> course = await _courseRepository.GetAssessmentCourseReport(categoryid, search, courseType);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("CourseTypeAheadFeedback/{search?}/{categoryid?}/{courseType?}")]
        public async Task<IActionResult> CourseTypeAheadFeedback(string search = null, int? categoryid = null, string courseType = null)
        {
            try
            {
                List<object> course = await _courseRepository.GetAssessmentCourse(categoryid, search, courseType);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Export/{categoryId?}/{IsActive?}/{filter?}/{search?}/{showAllData?}")]
        public async Task<IActionResult> Export(int? categoryId = null, bool? IsActive = null, string filter = null, string search = null, bool showAllData = false)
        {
            try
            {
                var courses = await this._courseRepository.GetAllCourses(UserId, showAllData, categoryId, IsActive, filter, search);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "Courses.xlsx";
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
                    worksheet.Cells[1, 1].Value = "Course Code";
                    worksheet.Cells[1, 2].Value = "Category Name";
                    worksheet.Cells[1, 3].Value = "Title";
                    worksheet.Cells[1, 4].Value = "Course Type";
                    worksheet.Cells[1, 5].Value = "Description";
                    worksheet.Cells[1, 6].Value = "Language";
                    worksheet.Cells[1, 7].Value = "Is Certificate Available";
                    worksheet.Cells[1, 8].Value = "Is Active";
                    worksheet.Cells[1, 9].Value = "Is PreAssessment Available";
                    worksheet.Cells[1, 10].Value = "Is Assessment Available";
                    worksheet.Cells[1, 11].Value = "Is Feedback Available";
                    worksheet.Cells[1, 12].Value = "Total Modules";
                    worksheet.Cells[1, 13].Value = "Sub Category Name";
                    worksheet.Cells[1, 14].Value = "Expiry Date";
                    worksheet.Cells[1, 15].Value = "Created By";
                    worksheet.Cells[1, 16].Value = "Created Date";
                    worksheet.Cells[1, 17].Value = "IsApplicable To All";
                    worksheet.Cells[1, 18].Value = "Modified By";
                    worksheet.Cells[1, 19].Value = "Course Duration";


                    int row = 2, column = 1;
                    foreach (APICourses course in courses)
                    {
                        DateTime DateValue = new DateTime();

                        worksheet.Cells[row, column++].Value = course.Code;
                        worksheet.Cells[row, column++].Value = course.CategoryName;
                        worksheet.Cells[row, column++].Value = course.Title;
                        worksheet.Cells[row, column++].Value = course.CourseType;
                        worksheet.Cells[row, column++].Value = course.Description;
                        worksheet.Cells[row, column++].Value = course.Language;
                        worksheet.Cells[row, column++].Value = course.IsCertificateIssued == true ? "Yes" : "No";
                        worksheet.Cells[row, column++].Value = course.IsActive == true ? "Yes" : "No";
                        worksheet.Cells[row, column++].Value = course.IsPreAssessment == true ? "Yes" : "No";
                        worksheet.Cells[row, column++].Value = course.IsAssessment == true ? "Yes" : "No";
                        worksheet.Cells[row, column++].Value = course.IsFeedback == true ? "Yes" : "No";
                        worksheet.Cells[row, column++].Value = course.TotalModules;
                        worksheet.Cells[row, column++].Value = course.SubCategoryName;
                        if (course.ExpiryDate != null)
                        {
                            DateValue = Convert.ToDateTime(course.ExpiryDate);
                            worksheet.Cells[row, column++].Value = DateValue.ToString("MMM dd, yyyy");
                        }
                        else
                        {
                            worksheet.Cells[row, column++].Value = null;
                        }
                        worksheet.Cells[row, column++].Value = course.CreatedBy;
                        if (course.CreatedDate != null)
                        {
                            DateValue = Convert.ToDateTime(course.CreatedDate);
                            worksheet.Cells[row, column++].Value = DateValue.ToString("MMM dd, yyyy");
                        }
                        worksheet.Cells[row, column++].Value = course.IsApplicableToAll;
                        worksheet.Cells[row, column++].Value = course.ModifiedBy;
                        worksheet.Cells[row, column++].Value = course.DurationInMinutes;

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
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCourseWiseEmailReminder/{page:int}/{pageSize:int}")]
        public async Task<IActionResult> GetCourseWiseEmailReminder(int page, int pageSize)
        {
            try
            {
                List<APICourseWiseEmailReminder> courses = await _courseRepository.GetPagination(page, pageSize);
                return Ok(courses);
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetCourseWiseSMSReminder/{page:int}/{pageSize:int}")]
        public async Task<IActionResult> GetCourseWiseSMSReminder(int page, int pageSize)
        {
            try
            {
                List<APICourseWiseSMSReminder> courses = await _courseRepository.GetPaginationSMS(page, pageSize);
                return Ok(courses);
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("CountCourseWiseEmailReminder")]
        public async Task<IActionResult> GetCountCourseWiseEmailReminder()
        {
            try
            {
                return Ok(await _courseRepository.GetCountCourseWiseEmailReminder());
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("CountCourseWiseSMSReminder")]
        public async Task<IActionResult> GetCountCourseWiseSMSReminder()
        {
            try
            {
                return Ok(await _courseRepository.GetCountCourseWiseSMSReminder());
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AddAssignmentDetails")]
        public async Task<IActionResult> AddAssignmentDetails([FromForm] ApiAssignmentDetails apiAssignmentDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError) });

                bool validText = FileValidation.CheckForSQLInjection(apiAssignmentDetails.TextAnswer);
                if (validText == true)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                SearchAssignmentDetails searchAssignmentDetails = new SearchAssignmentDetails();
                searchAssignmentDetails.CourseId = apiAssignmentDetails.CourseId;
                searchAssignmentDetails.AssignmentId = apiAssignmentDetails.AssignmentId;
                searchAssignmentDetails.UserId = UserId;
                bool submitted = await this._courseRepository.IsAssignmentSubmitted(searchAssignmentDetails);

                if (!submitted)
                {


                    var request = _httpContextAccessor.HttpContext.Request;
                    if (request.Form.Files.Count > 0)
                    {
                        foreach (IFormFile uploadedFile in request.Form.Files)
                        {
                            if ((uploadedFile.ContentType.Contains(FileType.Video)) && (FileValidation.IsValidLCMSVideo(uploadedFile)))
                            {
                                apiAssignmentDetails.FileType = FileType.Video;
                                apiAssignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Video, OrganisationCode);
                            }
                            else if ((uploadedFile.ContentType.Contains(FileType.Audio)) && (FileValidation.IsValidLCMSAudio(uploadedFile)))
                            {
                                apiAssignmentDetails.FileType = FileType.Audio;
                                apiAssignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Audio, OrganisationCode);
                            }
                            else if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                            {
                                apiAssignmentDetails.FileType = FileType.Image;
                                apiAssignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Image, OrganisationCode);
                            }
                            else
                            {
                                foreach (string docType in FileType.Doc)
                                {
                                    if ((uploadedFile.ContentType.Contains(docType)) && (FileValidation.IsValidLCMSDocument(uploadedFile)))
                                    {
                                        apiAssignmentDetails.FileType = docType;
                                        apiAssignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, docType, OrganisationCode);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(apiAssignmentDetails.TextAnswer))
                        {
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.AssignmentNotSubmitted), Description = EnumHelper.GetEnumDescription(MessageType.AssignmentNotSubmitted) });
                        }
                    }
                    apiAssignmentDetails.UserId = UserId;
                    apiAssignmentDetails.Status = Record.Pending;
                    await this._courseRepository.AddAssignmentDetails(apiAssignmentDetails);
                    return Ok(apiAssignmentDetails);
                }
                else
                {
                    //if submitted then check Rejected Status

                    var assignmentDetails = await this._courseRepository.GetAssignmentDetailsById(searchAssignmentDetails);


                    if (assignmentDetails.Status == Record.Rejected)
                    {
                        var request = _httpContextAccessor.HttpContext.Request;
                        if (request.Form.Files.Count > 0)
                        {
                            foreach (IFormFile uploadedFile in request.Form.Files)
                            {
                                if ((uploadedFile.ContentType.Contains(FileType.Video)) && (FileValidation.IsValidLCMSVideo(uploadedFile)))
                                {
                                    assignmentDetails.FileType = FileType.Video;
                                    assignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Video, OrganisationCode);
                                }
                                else if ((uploadedFile.ContentType.Contains(FileType.Audio)) && (FileValidation.IsValidLCMSAudio(uploadedFile)))
                                {
                                    assignmentDetails.FileType = FileType.Audio;
                                    assignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Audio, OrganisationCode);
                                }
                                else if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                                {
                                    assignmentDetails.FileType = FileType.Image;
                                    assignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, FileType.Image, OrganisationCode);
                                }
                                else
                                {
                                    foreach (string docType in FileType.Doc)
                                    {
                                        if ((uploadedFile.ContentType.Contains(docType)) && (FileValidation.IsValidLCMSDocument(uploadedFile)))
                                        {
                                            assignmentDetails.FileType = docType;
                                            assignmentDetails.FilePath = await this._courseRepository.SaveFile(uploadedFile, docType, OrganisationCode);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(apiAssignmentDetails.TextAnswer))
                            {
                                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.AssignmentNotSubmitted), Description = EnumHelper.GetEnumDescription(MessageType.AssignmentNotSubmitted) });
                            }
                        }
                        assignmentDetails.UserId = UserId;
                        assignmentDetails.Status = Record.Pending;
                        assignmentDetails.TextAnswer = apiAssignmentDetails.TextAnswer;

                        await this._courseRepository.UpdateRejectedAssignmentDetails(assignmentDetails);
                        return Ok(assignmentDetails);
                    }
                    else
                    {
                        return Ok("Assignment already submitted ");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetAssignmentDetails")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> GetAssignmentDetails([FromBody] SearchAssignmentDetails searchAssignmentDetails)
        {
            try
            {
                if (searchAssignmentDetails.SearchText != null)
                    searchAssignmentDetails.SearchText = searchAssignmentDetails.SearchText.ToLower().Equals("null") ? null : searchAssignmentDetails.SearchText;
                if (searchAssignmentDetails.ColumnName != null)
                    searchAssignmentDetails.ColumnName = searchAssignmentDetails.ColumnName.ToLower().Equals("null") ? null : searchAssignmentDetails.ColumnName;


                List<ApiAssignmentInfo> apiAssignmentInfo = new List<ApiAssignmentInfo>();
                apiAssignmentInfo = await this._courseRepository.GetAssignmentDetails(UserId, searchAssignmentDetails);
                return Ok(apiAssignmentInfo);
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return Ok("");
            }
        }

        [HttpPost("GetAssignmentDetailsCount")]
        [PermissionRequired(Permissions.assignment_management)]
        public async Task<IActionResult> GetAssignmentDetailsCount([FromBody] SearchAssignmentDetails searchAssignmentDetails)
        {
            try
            {
                if (searchAssignmentDetails.SearchText != null)
                    searchAssignmentDetails.SearchText = searchAssignmentDetails.SearchText.ToLower().Equals("null") ? null : searchAssignmentDetails.SearchText;
                if (searchAssignmentDetails.ColumnName != null)
                    searchAssignmentDetails.ColumnName = searchAssignmentDetails.ColumnName.ToLower().Equals("null") ? null : searchAssignmentDetails.ColumnName;

                int Count = await this._courseRepository.GetAssignmentDetailsCount(UserId, searchAssignmentDetails);
                return Ok(Count);
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return Ok("");
            }
        }

        [HttpPost("CheckAssignmentSubmitted")]
        public async Task<IActionResult> CheckAssignmentSubmitted([FromBody] SearchAssignmentDetails searchAssignmentDetails)
        {
            try
            {
                searchAssignmentDetails.UserId = UserId;
                if (searchAssignmentDetails.CourseId != null && searchAssignmentDetails.AssignmentId != null)
                {
                    bool submitted = await this._courseRepository.IsAssignmentSubmitted(searchAssignmentDetails);
                    return Ok(submitted);
                }
                else
                {
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetAssignmentDetailsById")]
        public async Task<IActionResult> GetAssignmentDetailsById([FromBody] SearchAssignmentDetails searchAssignmentDetails)
        {
            try
            {
                ApiAssignmentDetails apiAssignmentDetails = new ApiAssignmentDetails();
                searchAssignmentDetails.UserId = UserId;

                apiAssignmentDetails = await this._courseRepository.GetAssignmentDetailsById(searchAssignmentDetails);
                return Ok(apiAssignmentDetails);
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return Ok("");
            }
        }


        [HttpGet("GetCourseTypewiseCount")]
        public async Task<IActionResult> GetCourseTypewiseCountNew()
        {
            try
            {
                return Ok(await this._courseRepository.GetCourseTypewiseCountNew());
            }

            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetEmailConfiguration")]
        public async Task<bool> GetEmailConfiguration()
        {
            try
            {
                bool result = await this._courseRepository.IsEmailConfigured(OrganisationCode);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpPost("AddCourseWiseEmailReminder")]
        public async Task<IActionResult> AddCourseWiseEmailReminder([FromBody] APICourseWiseEmailReminder aPICourseWiseEmail)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError) });

                Model.CourseWiseEmailReminder courseWiseEmail = Mapper.Map<Model.CourseWiseEmailReminder>(aPICourseWiseEmail);
                await this._courseRepository.Addcoursewise(courseWiseEmail, UserId, OrganisationCode);
                return Ok(courseWiseEmail);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AddCourseWiseSMSReminder")]
        public async Task<IActionResult> AddCourseWiseSMSReminder([FromBody] APICourseWiseSMSReminder aPICourseWiseSMS)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError) });
                }
                CourseWiseSMSReminder courseWiseSMS = new CourseWiseSMSReminder();
                courseWiseSMS.CourseId = aPICourseWiseSMS.CourseId;
                courseWiseSMS.CreatedBy = aPICourseWiseSMS.CreatedBy;
                courseWiseSMS.CreatedDate = aPICourseWiseSMS.CreatedDate;
                courseWiseSMS.Id = aPICourseWiseSMS.Id;
                courseWiseSMS.IsDeleted = aPICourseWiseSMS.IsDeleted;
                courseWiseSMS.ModifiedBy = aPICourseWiseSMS.ModifiedBy;
                courseWiseSMS.ModifiedDate = aPICourseWiseSMS.ModifiedDate;
                await this._courseRepository.AddcourseWiseSMS(courseWiseSMS, UserId, OrganisationCode);
                return Ok(courseWiseSMS);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("AddCourseUserWiseEmailReminder")]
        public async Task<IActionResult> AddUserCourseWiseEmailReminder([FromBody] APICourseWiseEmailReminderNew aPICourseWiseEmail)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseMessage { StatusCode = 500, Message = EnumHelper.GetEnumName(MessageType.InternalServerError) });

                Model.UserWiseCourseEmailReminder courseWiseEmail = Mapper.Map<Model.UserWiseCourseEmailReminder>(aPICourseWiseEmail);
                await this._courseRepository.AddUsercoursewise(courseWiseEmail, UserId, OrganisationCode);
                return Ok(courseWiseEmail);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpPost("AlisonCourseDetailsByUserEmail")]
        public async Task<IActionResult> AlisonCourseDetailsForUser([FromBody] JsonElement jObject)
        {

            string userEmail = jObject.GetProperty("userEmail").ToString();
            List<APIAlisonCoursesDetails> apiCoursesDetailsList = new List<APIAlisonCoursesDetails>();
            try
            {
                AlisonServices alisonServices = new AlisonServices();
                apiCoursesDetailsList = await alisonServices.GetMyCoursesDetailed(userEmail);
                //return Ok( new { coursename = "Hello", courseStatus = "completed" });
                return Ok(apiCoursesDetailsList);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpGet("GetCompetencySkillByCourseId/{CourseId}")]
        public async Task<IActionResult> GetEmailConfiguration(int CourseId)
        {
            var CompetencySkill = await this._courseRepository.GetCompetencySkill(CourseId);
            return Ok(CompetencySkill);
        }

        [HttpGet("GetSubSubCategoryByCourseId/{CourseId}")]
        public async Task<IActionResult> GetSubSubCategoryByCourseId(int CourseId)
        {
            var SubSubCategory = await this._courseRepository.GetSubSubCategory(CourseId);
            return Ok(SubSubCategory);
        }

        [HttpGet("GetAuthorsByCourseId/{CourseId}")]
        public async Task<IActionResult> GetAuthorsByCourseId(int CourseId)
        {
            var authors = await this._courseAuthorAssociation.GetAuthorsByCourseId(CourseId);
            return Ok(authors);
        }

        [HttpGet("GetCourseIdByCourseCategory/{id?}")]
        public async Task<IActionResult> GetCourseIdByCourseCategory(int? id = null)
        {
            try
            {
                return Ok(await this._courseRepository.GetCourseIdByCourseCategory(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetPrerequisiteCourse/{CourseId}")]
        public async Task<IActionResult> GetPrerequisiteCourseByCourseId(int CourseId)
        {
            try
            {
                var CompetencySkill = await this._courseRepository.GetPrerequisiteCourseByCourseId(CourseId);
                return Ok(CompetencySkill);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetPrerequisiteCourseStatus")]
        public async Task<IActionResult> GetPrerequisiteCourseStatus([FromBody] APIPreRequisiteCourseStatus preRequisiteCourseStatus)
        {
            try
            {
                var CompetencySkill = await this._courseRepository.GetPrerequisiteCourseStatus(preRequisiteCourseStatus, UserId);
                return Ok(CompetencySkill);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("PostExternalCourse")]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> PostExternalCourse([FromBody] APIModel.APIxternalCourse course)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (course.Mission == null && course.Points > 0)
                {
                    return BadRequest();
                }

                if (await _courseRepository.Exist(course.Title, course.Code, IsInstitute))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                else
                {
                    Model.Course _course = Mapper.Map<Model.Course>(course);

                    _course.CreatedDate = DateTime.UtcNow;
                    _course.ModifiedDate = DateTime.UtcNow;
                    _course.CreatedBy = UserId;
                    if (course.PreAssessmentId != null && course.PreAssessmentId != 0)
                        _course.IsPreAssessment = true;
                    else
                        _course.IsPreAssessment = false;

                    if (course.AssessmentId != null && course.AssessmentId != 0)
                        _course.IsAssessment = true;
                    else
                        _course.IsAssessment = false;

                    if (course.ManagerEvaluationId != null && course.ManagerEvaluationId != 0)
                        _course.IsManagerEvaluation = true;
                    else
                        _course.IsManagerEvaluation = false;


                    if (course.FeedbackId != null && course.FeedbackId != 0)
                        _course.IsFeedback = true;
                    else
                        _course.IsFeedback = false;

                    _course.TotalModules = 1;

                    _course.IsModuleHasAssFeed = false;

                    await _courseRepository.Add(_course);

                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("PutExternalCourse/{id}")]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> PutExternalCourse(int id, [FromBody] APIModel.APIxternalCourse course)
        {
            try
            {
                DateTime CourseCreationdate;
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (course.Mission == null && course.Points > 0)
                {
                    return BadRequest();
                }

                Model.Course _course = await _courseRepository.Get(id);
                if (course.Code != _course.Code)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                if (_course == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                if (await _courseRepository.Exist(course.Title, course.Code, IsInstitute, course.Id))
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });

                Model.Course oldCourse = Mapper.Map<Model.Course>(_course);

                if (!_course.IsActive && course.IsActive)
                    CourseCreationdate = DateTime.UtcNow;
                else
                    CourseCreationdate = _course.CreatedDate;

                if (course.PreAssessmentId != null && course.PreAssessmentId != 0)
                    _course.IsPreAssessment = true;
                else
                    _course.IsPreAssessment = false;

                if (course.AssessmentId != null && course.AssessmentId != 0)
                    _course.IsAssessment = true;
                else
                    _course.IsAssessment = false;

                if (course.FeedbackId != null && course.FeedbackId != 0)
                    _course.IsFeedback = true;
                else
                    _course.IsFeedback = false;

                if (course.AssignmentId != null && course.AssignmentId != 0)
                    _course.IsAssignment = true;
                else
                    _course.IsAssignment = false;

                _course = Mapper.Map<Model.Course>(course);
                _course.ModifiedDate = DateTime.UtcNow;
                _course.ModifiedBy = UserId;
                _course.CreatedDate = CourseCreationdate;
                _course.TotalModules = 1;


                if (course.ManagerEvaluationId != null && course.ManagerEvaluationId != 0)
                    _course.IsManagerEvaluation = true;
                else
                    _course.IsManagerEvaluation = false;

                if (_course.TotalModules <= 0 && course.IsActive) // allow active course with content
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NoContentAdded), Description = EnumHelper.GetEnumDescription(MessageType.NoContentAdded) });
                }

                if (oldCourse.Title != _course.Title)
                {
                    if (_course.IsApplicableToAll)
                        await _courseRepository.UpdateCourseNotification(oldCourse, _course.Id, _course.Title, _course.Code, Token);
                }
                _course.IsModuleHasAssFeed = false;


                await _courseRepository.Update(_course);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("AddCourseLog")]
        public async Task<IActionResult> AddCourseLog([FromBody] CourseLog courseLog)
        {
            try
            {
                courseLog.UserId = UserId;

                await _courseRepository.AddCourseLog(courseLog);
                return Ok(courseLog);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message.ToString() });
            }
        }

        [HttpGet("IsRetrainindDaysEnable/{courseId}")]
        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> IsRetrainindDaysEnable(int courseId)
        {
            try
            {
                return Ok(await _courseRepository.IsRetrainindDaysEnable(courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseName/{course}")]
        public async Task<IActionResult> GetCourseName(string course)
        {
            try
            {
                string[] values = course.Split(',');

                int[] courseId = values.Select(int.Parse).ToArray();

                List<string> str = await _courseRepository.GetCourseName(courseId);

                return Ok(string.Join(",", str));

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [AllowAnonymous]
        [HttpPost("GetNodalCourses")]
        public async Task<IActionResult> GetNodalCourses([FromBody] APINodalCourses aPINodalCourses)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string OrgCode = Security.DecryptForUI(aPINodalCourses.OrgCode);
                string ConnectionString = await this._customerConnectionString.GetConnectionStringByOrgnizationCodeNew(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                var course = await _courseRepository.GetNodalCourses(aPINodalCourses, ConnectionString);
                if (course == null)
                    return NotFound();
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetNodalCoursesForGroupAdmin")]
        public async Task<IActionResult> GetNodalCoursesForGroupAdmin([FromBody] APINodalCoursesGroupAdmin aPINodalCoursesGroupAdmin)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var course = await _courseRepository.GetNodalCoursesForGroupAdmin(aPINodalCoursesGroupAdmin);
                if (course == null)
                    return NotFound();
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetNodalCoursesTypeahead/{Search?}")]
        public async Task<IActionResult> GetNodalCourseTypeahead(string Search = null)
        {
            try
            {
                var courses = await _courseRepository.GetNodalCourseTypeahead(Search);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetNodalCoursesDropdown")]
        public async Task<IActionResult> GetNodalCoursesDropdown()
        {
            try
            {
                var courses = await _courseRepository.GetNodalCoursesDropdown();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpPost("synccourses")]
        public async Task<IActionResult> GetCourseDetailsTigerhall([FromBody] List<APITigerhallCourses> aPITigerhall)
        {
            try
            {
                _logger.Debug("Entering GetCourseDetailsTigerhall ");
                var CourseDetails = await _courseRepository.GetCourseDetailsTigerhall(aPITigerhall);
                if (CourseDetails == null)
                    return NotFound();
                return Ok(CourseDetails);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAssessmentOptionReview/{courseId:int}")]
        public async Task<IActionResult> GetAssessmentOptionReview(int courseId)
        {
            try
            {
                return Ok(await _courseRepository.assessmentReview(courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCourseData")]

        [PermissionRequired(Permissions.coursemanage)]
        public async Task<IActionResult> GetV2([FromBody] ApiGetCourse apiGetCourse)
        {
            try
            {
                APITotalCoursesView course = await _courseRepository.GetAllV2(apiGetCourse, UserId, RoleCode);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseDetailsForUpdate/{id:int}")]
        public async Task<IActionResult> GetCourseDetailsById(int id)
        {
            try
            {

                return Ok(await _courseRepository.GetCourseDetailsById(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [AllowAnonymous]
        [HttpPost("GetLMSCourses")]
        public async Task<IActionResult> GetLMSCourses([FromBody] APITtGrCourses aPITtGrCourses)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (aPITtGrCourses.AccessKey != "0facd20d3e34c4183be814a09409729")
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidRequest), Description = EnumHelper.GetEnumDescription(MessageType.InvalidRequest) });

                // string OrgCode = Security.DecryptForUI(aPITtGrCourses.OrgCode);
                string OrgCode = aPITtGrCourses.OrgCode;
                string ConnectionString = await this._customerConnectionString.GetConnectionStringByOrgnizationCodeNew(OrgCode);
                if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                    || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                    return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

                var course = await _courseRepository.GetLMSCourses(aPITtGrCourses, ConnectionString);
                if (course == null)
                    return NotFound();
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCourseVendorDetails")]
        public async Task<IActionResult> GetCourseVendorDetails([FromBody] APIGetVendor aPIGetVendor)
        {
            try
            {
                var vendor = await _courseRepository.GetCourseVendorDetails(aPIGetVendor.Vendor_Type);
                if (vendor == null)
                    return NoContent();
                return Ok(vendor);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AdditionalResourceForCourse")]
        public async Task<IActionResult> PostAdditionalResourceForCourse([FromBody] APIAdditionalResourceForCourse data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _courseRepository.PostAdditionalResourceForCourse(data,UserId));      
                }
                else
                {
                    return this.BadRequest(ModelState);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("AdditionalResourceForCourse/{courseCode}")]
        public async Task<IActionResult> GetAdditionalResourceForCourse(string courseCode)
        {
            try
            {      
                return Ok(await _courseRepository.GetAdditionalResourceForCourse(courseCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UpdateAdditionalResource")]
        public async Task<IActionResult> UpdateAdditionalResourceForCourse([FromBody] APIAdditionalResourceForCourse data)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    return Ok(await _courseRepository.UpdateAdditionalResourceForCourse(data, UserId));
                }
                else
                {
                    return this.BadRequest(ModelState);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete("DeleteAdditionalResource")]
        public async Task<IActionResult> DeleteAdditionalResourceForCourse([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                return Ok(await _courseRepository.DeleteAdditionalResourceForCourse(DecryptedId, UserId));      
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }






    }
}

