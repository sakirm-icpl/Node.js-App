using AspNet.Security.OAuth.Introspection;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;
using com.pakhee.common;
using MyCourse.API.APIModel;
using MyCourse.API.APIModel.Competency;
using MyCourse.API.Common;
//using MyCourses.API.ExternalIntegration.EdCast;
using MyCourse.API.Helper;
using MyCourse.API.Model;
//using MyCourses.API.Model.EdCastAPI;
using MyCourse.API.Model.Log_API_Count;
//using MyCourses.API.Model.TNA;
//using MyCourses.API.Models;
using MyCourse.API.Repositories;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Services;
using MyCourse.API.Validations;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyCourse.API.APIModel;
using System.Threading.Tasks;
using static MyCourse.API.Common.EnumHelper;
using static MyCourse.API.Common.TokenPermissions;
using static MyCourse.API.Model.ResponseModels;

namespace MyCourse.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/MyCourses")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class MyCourseController : IdentityController
    {
        IMyCoursesRepository _myCoursesRepository;
        ICourseRepository _courseRepository;
        private ICoursesEnrollRequestRepository _coursesEnrollRequestRepository;
        private ICoursesEnrollRequestDetailsRepository _coursesEnrollRequestDetailsRepository;
        private readonly ITokensRepository _tokensRepository;
        private ICustomerConnectionStringRepository _customerConnectionString;
        private IAccessibilityRule _accessibilityRule;
        IAzureStorage _azurestorage;

        public IConfiguration _configuration { get; }
        private readonly IContentCompletionStatus _contentCompletionStatus;
        private static readonly ILog logger = LogManager.GetLogger(typeof(MyCourseController));
        public MyCourseController(IMyCoursesRepository myCoursesRepository, IAzureStorage azurestorage, ICourseRepository courseRepository, ICustomerConnectionStringRepository customerConnectionString, ICoursesEnrollRequestRepository coursesEnrollRequestRepository, ICoursesEnrollRequestDetailsRepository coursesEnrollRequestDetailsRepository, IAccessibilityRule accessibilityRule, IIdentityService _identitySvc, ITokensRepository tokensRepository, IConfiguration confugu, IContentCompletionStatus contentCompletionStatus, ILogger<MyCourseController> logger) : base(_identitySvc)
        {
            this._myCoursesRepository = myCoursesRepository;
            this._coursesEnrollRequestRepository = coursesEnrollRequestRepository;
            this._coursesEnrollRequestDetailsRepository = coursesEnrollRequestDetailsRepository;
            this._tokensRepository = tokensRepository;
            this._configuration = confugu;
            this._contentCompletionStatus = contentCompletionStatus;
            this._customerConnectionString = customerConnectionString;
            this._accessibilityRule = accessibilityRule;
            this._courseRepository = courseRepository;
            this._azurestorage = azurestorage;

        }

        [HttpGet("{page:int}/{pageSize:int}/{categoryId?}/{search?}/{status?}/{courseType?}/{subCategoryId?}/{subSubCategoryId?}/{isShowCatalogue?}/{sortBy?}/{CompetencyCategoryID?}/{CompetencyFilter?}/{JobRoleId?}/{CompetencyId?}/{provider?}/{IsActive?}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(int page, int pageSize, int? categoryId = null, string search = null, string status = null, [CourseType] string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null,string IsActive=null)
        {
            logger.Debug(string.Format("myCourse Get method for {0} ", UserId));

            try
            {
                return Ok(await this.GetUserCoursesData(UserId, page, pageSize, categoryId,  search, status, courseType, subCategoryId, subSubCategoryId, isShowCatalogue, sortBy, CompetencyCategoryID, CompetencyFilter, JobRoleId, CompetencyId, provider, IsActive));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        private async Task<List<APIMyCourses>> GetUserCoursesData(int Id, int page, int pageSize, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null,string IsActive=null)
        {

            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            if (status != null)
                status = status.ToLower().Equals("null") ? null : status;
            if (courseType != null)

                courseType = courseType.ToLower().Equals("null") ? null : courseType;
            if (provider != null)
                provider = provider.ToLower().Equals("null") ? null : provider;
            if (IsActive != null)
                IsActive = IsActive.ToLower().Equals("null") ? null : IsActive;
            
            if (CompetencyFilter != null)
                CompetencyFilter = CompetencyFilter.ToLower().Equals("null") ? null : CompetencyFilter;
            if (isShowCatalogue == true)
            {
                if (await _myCoursesRepository.IsShowCatlogue(Token))
                {
                    return await this._myCoursesRepository.GetAllCourse(Id, page, pageSize, categoryId, search, courseType, subCategoryId, subSubCategoryId, sortBy, CompetencyCategoryID, isShowCatalogue, status, provider, IsActive);
                }

            }
            else if (isShowCatalogue == null && OrganisationCode.Contains("dwtc"))
            {
                return await this._myCoursesRepository.GetAllCourse(Id, page, pageSize, categoryId, search, courseType, subCategoryId, subSubCategoryId, sortBy, CompetencyCategoryID, isShowCatalogue, null, provider, IsActive);
            }
            return await this._myCoursesRepository.Get(Id, page, pageSize, categoryId, status, search, courseType, subCategoryId, subSubCategoryId, sortBy, CompetencyFilter, JobRoleId, CompetencyId, provider,IsActive);
        }

        [AllowAnonymous]
        [HttpGet("GetCourseCatlogue/{orgcode}/{page:int}/{pageSize:int}/{categoryId?}/{search?}/{status?}/{courseType?}/{subCategoryId?}/{isShowCatalogue?}/{sortBy?}/{CompetencyCategoryID?}/{CompetencyFilter?}/{JobRoleId?}/{CompetencyId?}/{provider?}")]
        [Produces("application/json")]
        public async Task<IActionResult> Getcoursecatlogue(string orgcode, int page, int pageSize, int? categoryId = null, string search = null, string status = null, [CourseType] string courseType = null, int? subCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null)
        {
            try
            {
                if (orgcode == "gr8skills" || orgcode == "e2erail" || orgcode == "ent" || orgcode == "quantum")
                    return Ok(await this.getcoursecatloguedata(orgcode, page, pageSize, categoryId, search, status, courseType, subCategoryId, isShowCatalogue, sortBy, CompetencyCategoryID, CompetencyFilter, JobRoleId, CompetencyId, provider));

                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }


        private async Task<List<APIMyCourses>> getcoursecatloguedata(string orgcode, int page, int pageSize, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null)
        {

            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            if (status != null)
                status = status.ToLower().Equals("null") ? null : status;
            if (courseType != null)
                courseType = courseType.ToLower().Equals("null") ? null : courseType;
            if (provider != null)
                provider = provider.ToLower().Equals("null") ? null : provider;

            if (CompetencyFilter != null)
                CompetencyFilter = CompetencyFilter.ToLower().Equals("null") ? null : CompetencyFilter;

            return await this._myCoursesRepository.GetAllCatalogCourse(orgcode, page, pageSize, categoryId, search, courseType, subCategoryId, sortBy, CompetencyCategoryID, isShowCatalogue, status, provider);

        }



        [HttpPost("GetAllUserCourses")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllUserCourses([FromBody] APIUserCourseDetails obj)
        {
            try
            {
                return Ok(await this.GetAllUserCoursesData(obj));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        private async Task<List<APIMyCourses>> GetAllUserCoursesData(APIUserCourseDetails obj)
        {
            if (obj.search != null)
                obj.search = obj.search.Equals("null") ? null : obj.search;
            if (obj.status != null)
                obj.status = obj.status.Equals("null") ? null : obj.status;
            if (obj.courseType != null)
                obj.courseType = obj.courseType.Equals("null") ? null : obj.courseType;

            if (obj.isShowCatalogue == true)
            {
                if (await _myCoursesRepository.IsShowCatlogue(Token))
                {
                    return await this._myCoursesRepository.GetAllUserCourseData(obj);
                }
            }
            else 
            {
                int id = this._myCoursesRepository.GetIdFromUserId(obj.UserId);
                return await this._myCoursesRepository.Get(id,obj.page, obj.pageSize, obj.categoryId, obj.status, obj.search, obj.courseType, obj.subCategoryId,obj.subsubCategoryId, obj.sortBy, null, null, obj.CompetencyCategoryID, obj.provider);
            }
            return await this._myCoursesRepository.GetAllCourseDetails(obj);
        }


        [HttpGet("count/{categoryId?}/{search:minlength(0)?}/{status?}/{courseType?}/{subCategoryId?}/{subSubCategoryId?}/{isShowCatalogue?}/{sortBy?}/{CompetencyCategoryID?}/{CompetencyFilter?}/{JobRoleId?}/{CompetencyId?}/{provider?}/{IsActive?}")]
        public async Task<IActionResult> GetCount(int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null,string IsActive=null)
        {
            try
            {
                return Ok(await this.GetUserCoursesCountData(UserId, categoryId, search, status, courseType, subCategoryId, subSubCategoryId,isShowCatalogue, sortBy, CompetencyCategoryID, CompetencyFilter, JobRoleId, CompetencyId, provider, IsActive));

            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        private async Task<int> GetUserCoursesCountData(int Id, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, int? subSubCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null, string IsActive=null)
        {

            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            if (status != null)
                status = status.ToLower().Equals("null") ? null : status;
            if (courseType != null)
                courseType = courseType.ToLower().Equals("null") ? null : courseType;
            if (CompetencyFilter != null)
                CompetencyFilter = CompetencyFilter.ToLower().Equals("null") ? null : CompetencyFilter;
            if (provider != null)
                provider = provider.ToLower().Equals("null") ? null : provider;
            if (IsActive != null)
                IsActive = IsActive.ToLower().Equals("null") ? null : IsActive;
            
            if (isShowCatalogue == true)
            {
                if (await _myCoursesRepository.IsShowCatlogue(Token))
                {
                    return await this._myCoursesRepository.GetAllCourseCount(Id, categoryId, search, courseType, subCategoryId, subSubCategoryId, sortBy, CompetencyCategoryID, isShowCatalogue, provider, IsActive);
                }
            }
            else if (isShowCatalogue == null && OrganisationCode.Contains("dwtc"))
            {
                return await this._myCoursesRepository.GetAllCourseCount(Id, categoryId, search, courseType, subCategoryId, subSubCategoryId, sortBy, CompetencyCategoryID, isShowCatalogue, provider, IsActive);
            }
            return await this._myCoursesRepository.Count(Id, categoryId, status, search, courseType, subCategoryId, subSubCategoryId, sortBy, CompetencyFilter, JobRoleId, CompetencyId, provider, IsActive);
        }


        [AllowAnonymous]
        [HttpGet("GetCourseCatlogueCount/{orgcode}/{categoryId?}/{search:minlength(0)?}/{status?}/{courseType?}/{subCategoryId?}/{isShowCatalogue?}/{sortBy?}/{CompetencyCategoryID?}/{CompetencyFilter?}/{JobRoleId?}/{CompetencyId?}/{provider?}")]
        public async Task<IActionResult> GetcourseCatlogueCount(string orgcode, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null)
        {
            try
            {
                if (orgcode == "gr8skills" || orgcode == "e2erail" || orgcode == "ent" || orgcode == "quantum")
                    return Ok(await this.GetAllcourseCatlogueCount(orgcode, categoryId, search, status, courseType, subCategoryId, isShowCatalogue, sortBy, CompetencyCategoryID, CompetencyFilter, JobRoleId, CompetencyId, provider));
                return BadRequest();
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        private async Task<int> GetAllcourseCatlogueCount(string orgcode, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, bool? isShowCatalogue = null, string sortBy = null, int? CompetencyCategoryID = null, string CompetencyFilter = null, int? JobRoleId = null, int? CompetencyId = null, string provider = null)
        {

            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            if (status != null)
                status = status.ToLower().Equals("null") ? null : status;
            if (courseType != null)
                courseType = courseType.ToLower().Equals("null") ? null : courseType;
            if (CompetencyFilter != null)
                CompetencyFilter = CompetencyFilter.ToLower().Equals("null") ? null : CompetencyFilter;
            if (provider != null)
                provider = provider.ToLower().Equals("null") ? null : provider;

            return await this._myCoursesRepository.GetAllCatalogCourseCount(orgcode, categoryId, search, courseType, subCategoryId, sortBy, CompetencyCategoryID, isShowCatalogue, provider);


        }


        [HttpPost("GetUserCoursesCount")]
        public async Task<IActionResult> GetUserCoursesCount([FromBody] APIUserCourseDetails obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int id = await _myCoursesRepository.GetUserDetailsByUserID(obj.UserId);
                    return Ok(await this.GetUserCoursesCountData(id, obj.categoryId, obj.search, obj.status, obj.courseType, obj.subCategoryId, obj.subsubCategoryId, obj.isShowCatalogue, obj.sortBy));
                }
                else
                    return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("Getmodule/{courseId:int}/{groupId?}")]
        public async Task<IActionResult> Getmodule(int courseId, string groupId = null)
        {
            try
            {
                int? groupIdNew = null;
                if (string.IsNullOrEmpty(groupId) || groupId == "null" || groupId == "undefined" || groupId == "\"null\"")
                    groupIdNew = null;
                else
                {
                    int gId;
                    if (int.TryParse(groupId, out gId))
                    {
                        if (gId > 0)
                            groupIdNew = Convert.ToInt32(groupId);
                    }
                }

                return Ok(await this._myCoursesRepository.GetModule(this.UserId, courseId, OrganisationCode, groupIdNew));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetModuleInfo/{courseId:int}/{moduleId:int}")]
        public async Task<IActionResult> GetModuleInfo(int courseId, int moduleId)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetModuleInfo(this.UserId, courseId, moduleId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetProgressStatusCount")]
        public async Task<IActionResult> GetProgressStatusCount()
        {
            try
            {
                int InprogressCount = await this._myCoursesRepository.Count(this.UserId, null, Status.InProgress, null, null);
                int CompletedCount = await this._myCoursesRepository.Count(this.UserId, null, Status.Completed, null, null);
                int NotStartedCount = await this._myCoursesRepository.NotStartedCount(this.UserId, null, null, null);
                return Ok(new { InprogressCount, CompletedCount, NotStartedCount });
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserProgressStatusCount/{id:int}")]
        public async Task<IActionResult> GetUserProgressStatusCount(int id)
        {
            try
            {
                return Ok(await _myCoursesRepository.GetUserProgressStatusCountData(id));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetUserProgress")]
        public async Task<IActionResult> GetUserProgress([FromBody] APIGetUserID GetUser)
        {
            try
            {
                int id = await _myCoursesRepository.GetUserDetailsByUserID(GetUser.UserId);
                return Ok(await _myCoursesRepository.GetUserProgressStatusCountData(id));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("RefreshCourseProgressStatusbyUserId")]
        public async Task<IActionResult> RefreshCourseProgressStatusbyUserId([FromBody] APIGetUserID GetUser)
        {
            try
            {
                int id = await _myCoursesRepository.GetUserDetailsByUserID(GetUser.UserId);
                await this._myCoursesRepository.PostCourseStatitics(id);
                return Ok(await _myCoursesRepository.GetUserProgressStatusCountData(id));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserProgressStatusCount")]
        public async Task<IActionResult> GetUserProgressStatusCount()
        {
            try
            {
                if (await _myCoursesRepository.CheckDiLink(Token) == "Yes")
                {
                    return Ok(await _myCoursesRepository.GetUserProgressStatusCountDataForDeLink(UserId));
                }
                else
                    return Ok(await _myCoursesRepository.GetUserProgressStatusCountData(UserId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("RefreshCourseProgressStatus")]
        public async Task<IActionResult> RefreshCourseProgressStatus()
        {
            try
            {
                await this._myCoursesRepository.PostCourseStatitics(UserId);
                return Ok(await _myCoursesRepository.GetUserProgressStatusCountData(UserId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetProgressStatusDuration")]
        public async Task<IActionResult> GetProgressStatusDuration()
        {
            try
            {
                int InprogressDuration = await this._myCoursesRepository.GetProgressStatusDuration(this.UserId, null, Status.InProgress, null, null);
                int CompletedDuration = await this._myCoursesRepository.GetProgressStatusDuration(this.UserId, null, Status.Completed, null, null);
                int NotStartedDuration = await this._myCoursesRepository.GetNotStartedDuration(this.UserId, null, null, null);

                int totalDuration = InprogressDuration + CompletedDuration + NotStartedDuration;

                TimeSpan ts = TimeSpan.FromMinutes(totalDuration);
                string DurationInMinutes = string.Format("{0}.{1}", ts.Hours, ts.Minutes);

                ts = TimeSpan.FromMinutes(CompletedDuration);
                string CompletedDurationInMinutes = string.Format("{0}.{1}", ts.Hours, ts.Minutes);
                return Ok(new { DurationInMinutes, CompletedDurationInMinutes });
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("CourseProgress/{page:int}/{pageSize:int}/{categoryId?}/{search?}/{status?}/{courseType?}/{subCategoryId?}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetCourseProgress(int page, int pageSize, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                if (status != null)
                    status = status.ToLower().Equals("null") ? null : status;
                if (courseType != null)
                    courseType = courseType.ToLower().Equals("null") ? null : courseType;
                if (!string.IsNullOrEmpty(status) || status.ToLower().Equals(Status.Completed) || status.ToLower().Equals(Status.InProgress))
                    return Ok(await this._myCoursesRepository.GetCourseProgress(this.UserId, page, pageSize, status, courseType, subCategoryId, categoryId, search));

                return NotFound();
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetMissionCourses/{page:int}/{pageSize:int}/{Mission?}/{categoryId?}/{search?}/{status?}/{courseType?}/{subCategoryId?}/{sortBy?}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetMissionCourses(int page, int pageSize, string mission = null, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, bool? isShowCatalogue = null, string sortBy = null)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetMissionCourses(UserId, page, pageSize, mission, categoryId, status, search, courseType, subCategoryId, sortBy));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetMissionCourses/count/{mission?}/{categoryId?}/{search:minlength(0)?}/{status?}/{courseType?}/{subCategoryId?}/{isShowCatalogue?}")]
        public async Task<IActionResult> GetCount(int? categoryId = null, string mission = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, bool? isShowCatalogue = null)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetMissionCourseCount(this.UserId, mission, categoryId, status, search, courseType, subCategoryId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("EBTDetails")]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await this._myCoursesRepository.Get());
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("EBTDetails/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetByID(id));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("EBTDetails")]
        public async Task<IActionResult> Post([FromBody] APIEBTDetails objEBTDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                EBTDetails objEBT = new EBTDetails();
                objEBT.FromData = JsonConvert.SerializeObject(objEBTDetails.FromData);
                objEBT.CourseID = objEBTDetails.CourseID;
                objEBT.courseTitle = objEBTDetails.courseTitle;
                objEBT.UserName = UserName;
                objEBT.UserId = UserId;
                objEBT.Status = objEBTDetails.Status;
                objEBT.CreatedBy = UserId;
                objEBT.CreatedDate = DateTime.UtcNow;
                objEBT.ModifiedBy = UserId;
                objEBT.ModifiedDate = DateTime.UtcNow;
                objEBT.IsActive = true;
                objEBT.IsDeleted = false;
                return Ok(await _myCoursesRepository.AddEBT(objEBT));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UpdateEBTDetails")]
        public async Task<IActionResult> Put([FromBody] APIEBTDetails objEBTDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int ID = objEBTDetails.ID;
                APIEBTDetails objAPIEBT = await this._myCoursesRepository.GetByID(ID);
                if (objAPIEBT == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                EBTDetails objEBT = new EBTDetails();
                objEBT.FromData = JsonConvert.SerializeObject(objEBTDetails.FromData);
                objEBT.ID = objEBTDetails.ID;
                objEBT.Status = objEBTDetails.Status;
                objEBT.CourseID = objAPIEBT.CourseID;
                objEBT.courseTitle = objAPIEBT.courseTitle;
                objEBT.UserName = UserName;
                objEBT.UserId = UserId;
                objEBT.CreatedBy = objAPIEBT.CreatedBy;
                objEBT.CreatedDate = objAPIEBT.CreatedDate;
                objEBT.IsActive = objAPIEBT.IsActive;
                objEBT.ModifiedBy = UserId;
                objEBT.ModifiedDate = DateTime.UtcNow;
                int result = await _myCoursesRepository.UpdateEBT(objEBT);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("EBTDetailsByUserId/{courseId}")]
        public async Task<IActionResult> GetByUserId(int courseId)
        {
            try
            {
                APIEBTDetails aPIEBTDetails = await this._myCoursesRepository.GetByUserId(UserId, courseId);
                if (aPIEBTDetails == null)
                    return Ok("null");
                else
                    return Ok(aPIEBTDetails);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("IsCourseApplicableToUser/{courseId:int}/{groupId?}")]
        public async Task<IActionResult> IsCourseApplicableToUser(int courseId, string groupId = null)
        {
            try
            {
                int? groupIdNew = null;
                if (string.IsNullOrEmpty(groupId) || groupId == "null" || groupId == "undefined" || groupId == "\"null\"")
                    groupIdNew = null;
                else
                {
                    int gId;
                    if(int.TryParse(groupId, out gId))
                    {
                        if(gId>0)
                            groupIdNew = Convert.ToInt32(groupId);
                    }
                }

                return Ok(await this._myCoursesRepository.CheckUserApplicabilityToCourse(this.UserId, courseId, OrgCode, groupIdNew));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("enrollCourse/{courseId:int}")]
        public async Task<IActionResult> enrollCourse(int courseId)
        {
            if (externalUser == true || (RoleCode.ToLower() == "au" && OrganisationCode.ToLower().Contains("hdfc")))
            {
                return BadRequest("User does not have access to this function!");
            }
            else
            {
                string enrollType = "SelfEnroll";

                if (!OrganisationCode.ToLower().Contains("hdfc"))
                {
                    enrollType = await _myCoursesRepository.EnrollmentTypeForUser(Token);
                }

                if (enrollType == "SelfEnroll")
                {

                    var enroll = await this._myCoursesRepository.EnrollCourse(this.UserId, courseId,OrganisationCode);
                    
                    if (!OrganisationCode.ToLower().Contains("hdfc"))
                    {
                        string coursetitle = await this._myCoursesRepository.GetCourseInfo(courseId);
                        if (await _coursesEnrollRequestRepository.IsExist(courseId, UserId, "Requested"))
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                        }

                        CoursesEnrollRequest course_req = new CoursesEnrollRequest();
                        course_req.UserId = UserId;
                        course_req.UserName = UserName;
                        course_req.CourseId = courseId;
                        course_req.CourseTitle = coursetitle;
                        course_req.Date = DateTime.Now;
                        course_req.Status = "Enrolled";

                        await this._coursesEnrollRequestRepository.Add(course_req);

                        int courseRequestId = course_req.Id;

                        CoursesEnrollRequestDetails course_reqDetails = new CoursesEnrollRequestDetails();
                        course_reqDetails.CoursesEnrollRequestId = courseRequestId;
                        course_reqDetails.ActionTakenBy = UserId;
                        course_reqDetails.Reason = "";
                        course_reqDetails.DateCreated = DateTime.UtcNow;
                        course_reqDetails.Status = "Self-Enrolled";

                        await this._coursesEnrollRequestDetailsRepository.Add(course_reqDetails);
                    }

                    var Dev_Plan = await _courseRepository.GetMasterConfigurableParameterValue("Enable_IDP");
                    logger.Debug("Dev_Plan :-" + Dev_Plan);
                    if (Convert.ToString(Dev_Plan).ToLower() == "yes")
                    {

                        object obj = await _courseRepository.PostDevelopementPlan(courseId, UserId, UserName);
                    }
                    var DarwinboxPost = await _courseRepository.GetMasterConfigurableParameterValue("Darwinbox_Post");
                    logger.Debug("Darwinbox_Post :-" + DarwinboxPost);
                    if (Convert.ToString(DarwinboxPost).ToLower() == "yes")
                    {
                        DarwinboxTransactionDetails obj = await _courseRepository.PostCourseStatusToDarwinbox(courseId, UserId, "enroll", OrgCode);
                    }

                    return Ok(enroll);

                }
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NoAccess), Description = EnumHelper.GetEnumDescription(MessageType.NoAccess) });
            }
        }

        [AllowAnonymous]
        [HttpPost("enrollCourseUsers")]
        public async Task<IActionResult> enrollCourseUsers([FromBody] APIDarwinCoursEnroll aPIDarwinCoursEnroll)
        {
            if (!ModelState.IsValid)            
                return BadRequest(ModelState);
            

            string OrgCode = aPIDarwinCoursEnroll.orgCode;
            string ConnectionString = await this._customerConnectionString.GetConnectionStringByOrgnizationCodeNew(OrgCode);
            if (ConnectionString.Equals(ApiStatusCode.Unauthorized.ToString())
                || ConnectionString.Equals(ApiStatusCode.BadRequest.ToString()))
                return StatusCode(401, "Invalid Organization Code! Please contact System Administrator to know your Organization Code!");

            int userid = 0; int courseId= 0;
            string dwkey=null;
            using (CourseContext dbcontext = _customerConnectionString.GetDbContext(ConnectionString))
            {
                
                var key = (from c in dbcontext.DarwinboxConfiguration
                            select new
                            {
                                apikey = c.APIKey
                            }).AsNoTracking();
                var apikey = await key.FirstOrDefaultAsync();
                if (apikey != null)
                    dwkey=apikey.apikey.ToString();
            }

            if (aPIDarwinCoursEnroll.api_key!= dwkey)
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NoAccess), Description = EnumHelper.GetEnumDescription(MessageType.NoAccess) });
            
            if(string.IsNullOrEmpty(aPIDarwinCoursEnroll.userId)  )               
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidRequest), Description = EnumHelper.GetEnumDescription(MessageType.InvalidRequest) });

            if (!string.IsNullOrEmpty(aPIDarwinCoursEnroll.userId))
            {
                userid = await this._myCoursesRepository.GetUserIdByEmailId(ConnectionString, aPIDarwinCoursEnroll.userId);
            }
            courseId= await this._myCoursesRepository.GetCourseIdByCourseCode(ConnectionString,aPIDarwinCoursEnroll.courseCode);

            if ( courseId == 0)
            {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.CodeNotExists), Description = EnumHelper.GetEnumDescription(MessageType.CodeNotExists)+ " "+ aPIDarwinCoursEnroll.courseCode });
            }
            if (userid != 0 && courseId != 0)
            {
                APIAccessibility apiAccessibility = new APIAccessibility();
                AccessibilityRules objsad = new AccessibilityRules
                {
                    AccessibilityRule = "UserId",
                    Condition = "OR",
                    ParameterValue = userid.ToString(),

                };
                apiAccessibility.CourseId = courseId;
                apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
                apiAccessibility.AccessibilityRule[0] = objsad;
                var enroll = await this._accessibilityRule.DbSelfEnroll(apiAccessibility,userid, ConnectionString);
                string message = null ;
                
                DarwinboxTransactionDetails obj = await _courseRepository.PostCourseStatusToDarwinbox(courseId, userid, "enroll", OrgCode, ConnectionString );
                        if (obj.Tran_Status == "1")
                        {
                            message = aPIDarwinCoursEnroll.userId + " successfully enrolled for the " + aPIDarwinCoursEnroll.courseCode;

                        }
                        else
                        { 
                            message = obj.ResponseMessage+" "+ aPIDarwinCoursEnroll.userId  +" "+ aPIDarwinCoursEnroll.courseCode;

                        }
                 
                return Ok(new { message });
            }
            else {
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotAvailable), Description = EnumHelper.GetEnumDescription(MessageType.DataNotAvailable) });
            }
        }
       

        [HttpGet("getCoursesTypeahead/{search?}")]
        public async Task<IActionResult> SearchCourse(string search = null)
        {
            if (externalUser == true)
            {
                return BadRequest("You don't have access");
            }

            List<APICourseTypeahead> course = await _myCoursesRepository.SearchCourses(search);
            if (course.Count == 0)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpGet("GetForCompletedCourses/{page:int}/{pageSize:int}/{categoryId?}/{search?}/{status?}/{courseType?}/{subCategoryId?}/{sortBy?}/{CompetencyCategoryID?}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetForCompletedCourses(int page, int pageSize, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null)
        {
            return Ok(await this.GetUserCoursesDataCompletedCourses(UserId, page, pageSize, categoryId, search, status, courseType, subCategoryId, sortBy, CompetencyCategoryID));
        }

        private async Task<List<APIMyCourses>> GetUserCoursesDataCompletedCourses(int Id, int page, int pageSize, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null)
        {
            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            if (status != null)
                status = status.ToLower().Equals("null") ? null : status;
            if (courseType != null)
                courseType = courseType.ToLower().Equals("null") ? null : courseType;

            return await this._myCoursesRepository.GetCompletedCourses(Id, page, pageSize, categoryId, status, search, courseType, subCategoryId, sortBy);
        }

        [HttpGet("CountCompletedCourses/{categoryId?}/{search:minlength(0)?}/{status?}/{courseType?}/{subCategoryId?}/{sortBy?}/{CompetencyCategoryID?}")]
        public async Task<IActionResult> GetCountForCompletedCourses(int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null)
        {
            try
            {
                return Ok(await this.GetUserCoursesCountDataCompletedCourses(UserId, categoryId, search, status, courseType, subCategoryId, sortBy, CompetencyCategoryID));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        private async Task<APICountAndDuration> GetUserCoursesCountDataCompletedCourses(int Id, int? categoryId = null, string search = null, string status = null, string courseType = null, int? subCategoryId = null, string sortBy = null, int? CompetencyCategoryID = null)
        {
            if (search != null)
                search = search.ToLower().Equals("null") ? null : search;
            if (status != null)
                status = status.ToLower().Equals("null") ? null : status;
            if (courseType != null)
                courseType = courseType.ToLower().Equals("null") ? null : courseType;

            return await this._myCoursesRepository.CountCompletedCourses(Id, categoryId, status, search, courseType, subCategoryId, sortBy);
        }

        [HttpGet("GetMonthWiseCompletion")]
        public async Task<IActionResult> GetMonthWiseCompletion()
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetMonthWiseCompletion(UserId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCountMonthWiseCompletion")]
        public async Task<IActionResult> GetCountMonthWiseCompletion()
        {
            try
            {
                int Count = await this._myCoursesRepository.GetCountMonthWiseCompletion(UserId);
                return Ok(new { Count });
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetContent")]
        public IActionResult GetContent()
        {
            try
            {
                var file = Path.Combine("C:/Publish/ApiGateway",
                                "LXPFiles", "images", "Test.png");

                return PhysicalFile(file, "image/png");
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("DownloadContent")]
        public IActionResult DownloadContent()
        {
            try
            {
                var file = Path.Combine("C:/Publish/ApiGateway",
                        "LXPFiles", "video", "Test.mp4");
                return PhysicalFile(file, "video/mp4");
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("DownloadZipContent")]
        public IActionResult DownloadZipContent()
        {
            try
            {
                var file = Path.Combine("C:/Publish/ApiGateway",
                                "LXPFiles", "zip", "Test.zip");

                return PhysicalFile(file, "application/x-zip-compressed");
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetContentFile/{courseId:int}/{moduleId:int}/{language?}")]
        public async Task<IActionResult> GetContentFile(int courseId, int moduleId, string language = null)
        {
            try
            {

                var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

                if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
                {
                    if (language != null)
                        language = language.ToLower().Equals("null") ? null : language;

                    bool? ismultilingual = await this._myCoursesRepository.GetIsmoduleMultilingual(moduleId);

                    if (ismultilingual == true)
                    {
                        string languageCode = await this._myCoursesRepository.GetUserPrefferedLanguage(UserId, courseId, moduleId);
                        if (!string.IsNullOrEmpty(languageCode.Trim()))
                        {
                            language = languageCode.ToLower().Equals("null") ? "en" : languageCode;
                        }
                        else
                        {
                            language = "en";
                        }
                    }


                    if (string.IsNullOrEmpty(language))  // default data
                    {
                        ApiCourseInfo CourseInfo = await this._myCoursesRepository.GetModuleInfo(this.UserId, courseId, moduleId);


                        if (CourseInfo.Modules.ModuleType.ToLower() == "scorm" || CourseInfo.Modules.ModuleType.ToLower() == "nonscorm")
                        {
                            var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "zip", CourseInfo.Modules.InternalName);

                            return PhysicalFile(file, CourseInfo.Modules.MimeType);
                        }

                        else if (CourseInfo.Modules.ModuleType.ToLower() == "document")
                        {
                            if (CourseInfo.Modules.MimeType == "image/jpeg" || (CourseInfo.Modules.MimeType == "image/png" || CourseInfo.Modules.MimeType == "image/bmp"))
                            {
                                var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "image", CourseInfo.Modules.InternalName);

                                return PhysicalFile(file, CourseInfo.Modules.MimeType);
                            }
                            var file3 = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "Pdf", CourseInfo.Modules.InternalName);

                            return PhysicalFile(file3, CourseInfo.Modules.MimeType);
                        }
                        else if (CourseInfo.Modules.ModuleType.ToLower() == "video")
                        {
                            var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "video", CourseInfo.Modules.InternalName);
                            return PhysicalFile(file, CourseInfo.Modules.MimeType);
                        }
                        else if (CourseInfo.Modules.ModuleType.ToLower() == "audio")
                        {
                            var file = Path.Combine(this._configuration["CoursePath"],
                                        OrganisationCode, "Courses", "audio", CourseInfo.Modules.InternalName);

                            return PhysicalFile(file, CourseInfo.Modules.MimeType);
                        }


                        var file2 = Path.Combine(this._configuration["CoursePath"], "LXPFiles", "images", "Test.png");
                        return PhysicalFile(file2, "image/png");

                    }
                    else  //  for given language
                    {
                        List<MultiLangualContentInfo> LangualContentInfo = await this._myCoursesRepository.GetMultiLangualModules(courseId, Token, moduleId, language);
                        if (LangualContentInfo != null)  // got data for given language
                        {
                            MultiLangualContentInfo getlanguagedata = LangualContentInfo.FirstOrDefault();

                            if (getlanguagedata.ModuleType.ToLower() == "scorm" || getlanguagedata.ModuleType.ToLower() == "nonscorm")
                            {
                                var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "zip", getlanguagedata.InternalName);

                                return PhysicalFile(file, getlanguagedata.MimeType);
                            }

                            else if (getlanguagedata.ModuleType.ToLower() == "document")
                            {
                                if (getlanguagedata.MimeType == "image/jpeg" || (getlanguagedata.MimeType == "image/png" || getlanguagedata.MimeType == "image/bmp"))
                                {
                                    var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "image", getlanguagedata.InternalName);

                                    return PhysicalFile(file, getlanguagedata.MimeType);
                                }
                                var file3 = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "Pdf", getlanguagedata.InternalName);

                                return PhysicalFile(file3, getlanguagedata.MimeType);
                            }
                            else if (getlanguagedata.ModuleType.ToLower() == "video")
                            {
                                var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "video", getlanguagedata.InternalName);


                                return PhysicalFile(file, getlanguagedata.MimeType);
                            }
                            else if (getlanguagedata.ModuleType.ToLower() == "audio")
                            {

                                var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "audio", getlanguagedata.InternalName);

                                return PhysicalFile(file, getlanguagedata.MimeType);
                            }

                            var file2 = Path.Combine(this._configuration["CoursePath"], "LXPFiles", "images", "Test.png");
                            return PhysicalFile(file2, "image/png");
                        }
                        else   //no data for given language
                        {
                            ApiCourseInfo CourseInfo = await this._myCoursesRepository.GetModuleInfo(this.UserId, courseId, moduleId);


                            if (CourseInfo.Modules.ModuleType.ToLower() == "scorm")
                            {
                                var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "zip", CourseInfo.Modules.InternalName);

                                return PhysicalFile(file, CourseInfo.Modules.MimeType);
                            }

                            else if (CourseInfo.Modules.ModuleType.ToLower() == "document")
                            {
                                if (CourseInfo.Modules.MimeType == "image/jpeg" || (CourseInfo.Modules.MimeType == "image/png" || CourseInfo.Modules.MimeType == "image/bmp"))
                                {
                                    var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "image", CourseInfo.Modules.InternalName);

                                    return PhysicalFile(file, CourseInfo.Modules.MimeType);
                                }
                                var file3 = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "Pdf", CourseInfo.Modules.Path);

                                return PhysicalFile(file3, CourseInfo.Modules.MimeType);
                            }
                            else if (CourseInfo.Modules.ModuleType.ToLower() == "video")
                            {
                                var file = Path.Combine(this._configuration["CoursePath"], OrganisationCode, "Courses", "video", CourseInfo.Modules.InternalName);

                                return PhysicalFile(file, CourseInfo.Modules.MimeType);
                            }
                            else if (CourseInfo.Modules.ModuleType.ToLower() == "audio")
                            {
                                var file = Path.Combine(this._configuration["CoursePath"],
                                         OrganisationCode, "Courses", "audio", CourseInfo.Modules.InternalName);

                                return PhysicalFile(file, CourseInfo.Modules.MimeType);
                            }
                            var file2 = Path.Combine(this._configuration["CoursePath"], "LXPFiles", "images", "Test.png");
                            return PhysicalFile(file2, "image/png");

                        }

                    }
                }
                else
                {                  
                    if (language != null)
                        language = language.ToLower().Equals("null") ? null : language;

                    bool? ismultilingual = await this._myCoursesRepository.GetIsmoduleMultilingual(moduleId);

                    if (ismultilingual == true)
                    {
                        string languageCode = await this._myCoursesRepository.GetUserPrefferedLanguage(UserId, courseId, moduleId);
                        if (!string.IsNullOrEmpty(languageCode.Trim()))
                        {
                            language = languageCode.ToLower().Equals("null") ? "en" : languageCode;
                        }
                        else
                        {
                            language = "en";
                        }
                    }


                    if (string.IsNullOrEmpty(language))  // default data
                    {
                        ApiCourseInfo CourseInfo = await this._myCoursesRepository.GetModuleInfo(this.UserId, courseId, moduleId);


                        if (CourseInfo.Modules.ModuleType.ToLower() == "scorm" || CourseInfo.Modules.ModuleType.ToLower() == "nonscorm")
                        {
                            var file = Path.Combine( OrganisationCode, "Courses", "zip", CourseInfo.Modules.InternalName);
                            logger.Info(file);
                            return await BlobFile(file, CourseInfo.Modules.InternalName);
                        }

                        else if (CourseInfo.Modules.ModuleType.ToLower() == "document")
                        {
                            if (CourseInfo.Modules.MimeType == "image/jpeg" || (CourseInfo.Modules.MimeType == "image/png" || CourseInfo.Modules.MimeType == "image/bmp"))
                            {
                                var file = Path.Combine( OrganisationCode, "Courses", "image", CourseInfo.Modules.InternalName);
                                logger.Info(file);

                                return await BlobFile(file, CourseInfo.Modules.InternalName);
                            }
                            var file3 = Path.Combine(OrganisationCode, "Courses", "Pdf", CourseInfo.Modules.InternalName);
                            logger.Info(file3);

                            return await BlobFile(file3, CourseInfo.Modules.InternalName);
                        }                        
                        
                        else if (CourseInfo.Modules.ModuleType.ToLower() == "video")
                        {
                            var file = Path.Combine( OrganisationCode, "Courses", "video", CourseInfo.Modules.InternalName);
                            logger.Info(file);
                            return await BlobFile(file, CourseInfo.Modules.InternalName);
                        }
                        else if (CourseInfo.Modules.ModuleType.ToLower() == "audio")
                        {
                            var file = Path.Combine(OrganisationCode, "Courses", "audio", CourseInfo.Modules.InternalName);
                            logger.Info(file);
                            return await BlobFile(file, CourseInfo.Modules.InternalName);
                        }

                        var file2 = Path.Combine(this._configuration["CoursePath"], "LXPFiles", "images", "Test.png");
                        logger.Info(file2);
                        return PhysicalFile(file2, "image/png");

                    }
                    else  //  for given language
                    {
                        List<MultiLangualContentInfo> LangualContentInfo = await this._myCoursesRepository.GetMultiLangualModules(courseId, Token, moduleId, language);
                        if (LangualContentInfo != null)  // got data for given language
                        {
                            MultiLangualContentInfo getlanguagedata = LangualContentInfo.FirstOrDefault();

                            if (getlanguagedata.ModuleType.ToLower() == "scorm" || getlanguagedata.ModuleType.ToLower() == "nonscorm")
                            {
                                var file = Path.Combine( OrganisationCode, "Courses", "zip", getlanguagedata.InternalName);
                                logger.Info(file);
                                return await BlobFile(file, getlanguagedata.InternalName);
                            }

                            else if (getlanguagedata.ModuleType.ToLower() == "document")
                            {
                                if (getlanguagedata.MimeType == "image/jpeg" || (getlanguagedata.MimeType == "image/png" || getlanguagedata.MimeType== "image/bmp"))
                                {
                                    var file = Path.Combine( OrganisationCode, "Courses", "image", getlanguagedata.InternalName);
                                    logger.Info(file);
                                    return await BlobFile(file, getlanguagedata.InternalName);
                                }
                                var file3 = Path.Combine( OrganisationCode, "Courses", "Pdf", getlanguagedata.InternalName);
                                logger.Info(file3);

                                return await BlobFile(file3, getlanguagedata.InternalName);
                            }
                            else if (getlanguagedata.ModuleType.ToLower() == "video")
                            {
                                var file = Path.Combine( OrganisationCode, "Courses", "video", getlanguagedata.InternalName);
                                logger.Info(file);

                                return await BlobFile(file, getlanguagedata.InternalName);
                            }
                            else if (getlanguagedata.ModuleType.ToLower() == "audio")
                            {

                                var file = Path.Combine( OrganisationCode, "Courses", "audio", getlanguagedata.InternalName);
                                logger.Info(file);
                                return await BlobFile(file, getlanguagedata.InternalName);
                            }

                            var file2 = Path.Combine(this._configuration["CoursePath"], this._configuration["BlobContainerName"], "LXPFiles", "images", "Test.png");
                            logger.Info(file2);
                            return PhysicalFile(file2, "image/png");
                        }
                        else   //no data for given language
                        {
                            ApiCourseInfo CourseInfo = await this._myCoursesRepository.GetModuleInfo(this.UserId, courseId, moduleId);


                            if (CourseInfo.Modules.ModuleType.ToLower() == "scorm")
                            {
                                var file = Path.Combine( OrganisationCode, "Courses", "zip", CourseInfo.Modules.InternalName);
                                logger.Info(file);
                                return await BlobFile(file, CourseInfo.Modules.InternalName);
                            }

                            else if (CourseInfo.Modules.ModuleType.ToLower() == "document")
                            {
                                if (CourseInfo.Modules.MimeType == "image/jpeg" || (CourseInfo.Modules.MimeType == "image/png" ||  CourseInfo.Modules.MimeType == "image/bmp"))
                                {
                                    var file = Path.Combine( OrganisationCode, "Courses", "image", CourseInfo.Modules.InternalName);
                                    logger.Info(file);
                                    return await BlobFile(file, CourseInfo.Modules.InternalName);
                                }
                                
                                var file3 = Path.Combine( OrganisationCode, "Courses", "Pdf", CourseInfo.Modules.InternalName);
                                logger.Info(file3);
                                return await BlobFile(file3, CourseInfo.Modules.InternalName);
                            }
                            else if (CourseInfo.Modules.ModuleType.ToLower() == "video")
                            {
                                var file = Path.Combine( OrganisationCode, "Courses", "video", CourseInfo.Modules.InternalName);
                                logger.Info(file);
                                return await BlobFile(file, CourseInfo.Modules.InternalName);
                            }
                            else if (CourseInfo.Modules.ModuleType.ToLower() == "audio")
                            {
                                var file = Path.Combine(OrganisationCode, "Courses", "audio", CourseInfo.Modules.InternalName);
                                logger.Info(file);
                                return await BlobFile(file, CourseInfo.Modules.InternalName);
                            }
                            var file2 = Path.Combine(this._configuration["CoursePath"], this._configuration["BlobContainerName"], "LXPFiles", "images", "Test.png");
                            logger.Info(file2);
                            return PhysicalFile(file2, "image/png");

                        }

                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        private async Task<IActionResult> BlobFile(string file, string FileName)
        {
            BlobDto imgres = await _azurestorage.DownloadAsync(file);
            if (imgres != null)
            {
                if (!string.IsNullOrEmpty(imgres.Name))
                {
                    using (var stream = new MemoryStream())
                    {
                        return File(imgres.Content, imgres.ContentType, FileName);
                    }
                }
                else
                {
                    
                    logger.Error(imgres.ToString());
                    return null;
                }
            }
            else
            {
                logger.Error("File not exists");
                return null;
            }
        }


[HttpGet("checkxAPICompletion/{courseId:int}/{moduleId:int}")]
        public async Task<IActionResult> checkxAPICompletion(int courseId, int moduleId)
        {
            try
            {

                APIxAPICompletionDetails _APIxAPICompletionDetails = await this._myCoursesRepository.checkxAPICompletion(UserId, courseId, moduleId, OrganisationCode);

                if (_APIxAPICompletionDetails != null)
                {
                    if (_APIxAPICompletionDetails._hasStarted == "true" && _APIxAPICompletionDetails._isComplete == "false")
                    {
                        ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                        ContentCompletionStatus.CourseId = courseId;
                        ContentCompletionStatus.ModuleId = moduleId;
                        ContentCompletionStatus.Status = Status.InProgress;
                        ContentCompletionStatus.UserId = UserId;
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        ContentCompletionStatus.IsDeleted = false;
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                        ContentCompletionStatus.UserId = UserId;


                        await _contentCompletionStatus.Post(ContentCompletionStatus);
                    }
                    else if (_APIxAPICompletionDetails._isComplete == "true")
                    {
                        //completed

                        ContentCompletionStatus ContentCompletionStatus = new ContentCompletionStatus();
                        ContentCompletionStatus.CourseId = courseId;
                        ContentCompletionStatus.ModuleId = moduleId;
                        ContentCompletionStatus.Status = Status.Completed;
                        ContentCompletionStatus.UserId = UserId;
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        ContentCompletionStatus.IsDeleted = false;
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                        ContentCompletionStatus.UserId = UserId;

                        await _contentCompletionStatus.Post(ContentCompletionStatus);
                    }

                    return Ok();
                }
                else
                {
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest();
            }
            return Ok();
        }


        [HttpPost("AddCareerJobRoles")]
        [Produces("application/json")]
        public async Task<IActionResult> AddCareerJobRoles([FromBody] APICareerJobRoles _careerJobRoles)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(await this._myCoursesRepository.PostCareerJobRoles(_careerJobRoles, UserId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("JobRoleTypeAhead/{search?}")]
        public async Task<IActionResult> GetTypeAHead(string search = null)
        {

            try
            {
                search = search.Trim();

                List<TypeAhead> course = await this._myCoursesRepository.GetTypeAHead(search);
                return Ok(course);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCareerJobRolesNames")]
        public async Task<IActionResult> GetCareerJobRoles()
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetCareerJobRoles(UserId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserJobRoleByUserId")]
        public async Task<IActionResult> GetUserJobRoleByUserId()
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetUserJobRoleByUserId(UserId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetUserCurrentJobRoleCompetencies/{JobroleId:int?}")]
        public async Task<IActionResult> GetUserCurrentJobRoleCompetencies(int? JobroleId)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetUserCurrentJobRoleCompetencies(UserId, JobroleId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetUserCurrentJobRoleCompetenciesV2")]
        public async Task<IActionResult> GetUserCurrentJobRoleCompetenciesV2([FromBody] AssessmentStatus assessmentStatus)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetUserCurrentJobRoleCompetenciesV2(assessmentStatus.UserId,UserId,assessmentStatus.JobroleId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ViewPositions")]
        public async Task<IActionResult> ViewPositions()
        {
            try
            {
                return Ok(await this._myCoursesRepository.ViewPositions(UserId));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetMultiLangualModules/{courseId:int}/{moduleId:int}/{Language?}")]
        public async Task<IActionResult> GetMultiLangualModules(int courseId, int moduleId, string Language = null)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetMultiLangualModules(courseId, Token, moduleId, Language));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AddUserPrefferedCourseLanguage")]
        public async Task<IActionResult> AddUserPrefferedCourseLanguage([FromBody] UserPrefferedCourseLanguage data)
        {
            try
            {
                UserPrefferedCourseLanguage obj = new UserPrefferedCourseLanguage();
                obj.CourseId = data.CourseId;
                obj.ModuleId = data.ModuleId;
                obj.UserID = UserId;
                obj.LanguageCode = data.LanguageCode;
                obj.CreatedBy = UserId;
                obj.CreatedDate = DateTime.UtcNow;
                obj.IsActive = true;
                obj.IsDeleted = false;
                return Ok(await this._myCoursesRepository.addUserPrefferedCourseLanguage(obj, Token));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("GetCompetencyMasterCourseDetails")]
        [Produces("application/json")]
        public async Task<IActionResult> GetCompetencyMasterCourse([FromBody] APIUserCourseDetails obj)
        {
            try
            {
                try
                {
                    if (obj.IsMobile != null && obj.IsMobile == true)
                    {
                        obj.UserId = Security.DecryptForUI(obj.UserId);
                        obj.UserId = Security.Encrypt(obj.UserId);
                    }
                }
                catch (Exception ex) { logger.Error(Utilities.GetDetailedException(ex));  }

                try
                {
                    if ( IsiOS == true)
                    {

                        string Key = "a1b2c3d4e5f6g7h8";
                        string _initVector = "1234123412341234";

                        obj.UserId = CryptLib.decrypt(obj.UserId, Key, _initVector);
                        obj.UserId = Security.Encrypt(obj.UserId);
                    }
                }
                catch (Exception ex) { logger.Error(Utilities.GetDetailedException(ex)); }

                try
                {
                    Boolean related = await this._myCoursesRepository.ManagerUserRelated(obj.UserId, UserId);

                    if (related == false)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.UserNotInTeam), Description = EnumHelper.GetEnumDescription(MessageType.UserNotInTeam) });
                    }
                }
                catch (Exception ex) { logger.Error(Utilities.GetDetailedException(ex)); }

                return Ok(await this._myCoursesRepository.GetCompetencyMasterCourse(obj));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("GetManagerEvaluationData")]
        [Produces("application/json")]
        public async Task<IActionResult> GetManagerEvaluationData([FromBody] APIGetManagerEvaluationCourses obj)
        {
            try
            {
               
                return Ok(await this._myCoursesRepository.GetCompetencyMasterCourse(UserId,obj));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("AssignMasterTest")]
        public async Task<IActionResult> AssignMasterTest([FromBody] APIAssignMasterTest obj)

        {
            try
            {
                int UserID = await _myCoursesRepository.GetUserDetailsByUserID(obj.UserId);
                var enroll = await this._myCoursesRepository.EnrollCourse(UserID, obj.CourseID,OrganisationCode);

                return Ok(enroll);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetCoursesDuration")]
        public async Task<IActionResult> GetCoursesDuration()
        {


            try
            {
                APICoursesDuration aPICoursesDuration = await this._myCoursesRepository.GetCoursesDuration(UserId);
                return Ok(aPICoursesDuration);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpPost("GetCoursesDurationByDate")]
        public async Task<IActionResult> GetCoursesDurationByDate([FromBody] CoursesDuration coursesDuration)
        {


            try
            {
                APICoursesDuration aPICoursesDuration = await this._myCoursesRepository.GetCoursesDurationByDate(UserId, coursesDuration);
                return Ok(aPICoursesDuration);
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpPost("CourseDetails/{UserId}")]
        public async Task<IActionResult> GetCourseDetails(int UserId, [FromBody] TnaCategories tnaCategory)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetCourseDetails(UserId, OrgCode, tnaCategory));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpPost("CourseDetails")]
        public async Task<IActionResult> GetCourseDetails([FromBody] TnaCategories tnaCategory)
        {
            try
            {
                return Ok(await this._myCoursesRepository.GetCourseDetails(UserId, OrgCode, tnaCategory));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpPost("TnaNominateRequest")]
        public async Task<IActionResult> PostTnaEmployeeRequest([FromBody] List<TnaEmployeeNominateRequestPayload> tnaEmployeeNominateRequestPayload)
        {
            try
            {
                return Ok(await this._myCoursesRepository.PostTnaEmployeeRequest(UserId, tnaEmployeeNominateRequestPayload));
            }
            catch (Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpGet("GetCourseDetailsForApplicability/{CourseId:int?}")]
        public IActionResult GetCourseDetailsForApplicability(int CourseId)
        {
            try
            {
                APIResponse<MyCourseForApplicability> aPIResponse = _myCoursesRepository.GetCourseDetailsForApplicability(CourseId);
                return Ok(aPIResponse);
            }
            catch(Exception ex)
            {
                logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest();
            }
        }
    }
}