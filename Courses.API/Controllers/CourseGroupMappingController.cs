using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.APIModel.Competency;
using Courses.API.Common;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.TokenPermissions;
using Courses.API.Helper;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using OfficeOpenXml;
using Courses.API.Helper.Metadata;
using Courses.API.Model;
using Courses.API.Model.EdCastAPI;
using Courses.API.ExternalIntegration.EdCast;
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers.Competency
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/c/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class CourseGroupMappingController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseGroupMappingController));

        private ICourseGroupMappingRepository _courseGroupMappingRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICourseRepository _courseRepository;

        public CourseGroupMappingController(ICourseGroupMappingRepository courseGroupMappingRepository, IIdentityService _identitySvc, ITokensRepository tokensRepository, ICourseRepository courseRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(_identitySvc)
        {
          
            this._courseGroupMappingRepository = courseGroupMappingRepository;
            this._tokensRepository = tokensRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _courseRepository = courseRepository;
        }

      


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
      //  [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await this._courseGroupMappingRepository.GetAllCourseGroupMapping(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
     //   [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this._courseGroupMappingRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetGroupByCourseId/{courseId:int}/{page:int}/{pageSize:int}")]
        public async Task<IActionResult> GetByCourseId(int courseId, int page, int pageSize)
        {
            try
            {
                var CompetencyLevels = await this._courseGroupMappingRepository.GetAllGroupsMappingByCourse(courseId, page, pageSize);
                return Ok(Mapper.Map<List<APICourseGroupMappings>>(CompetencyLevels));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetCountGroupByCourseId/{courseId:int}")]
     //   [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> GetGroupByCourseId(int courseId )
        {
            try
            {
                var Count = await this._courseGroupMappingRepository.GetCountGroupByCourseId(courseId);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
       // [PermissionRequired(Permissions.CompetenciesMappingg)]
        public async Task<IActionResult> Post([FromBody] APICourseGroupMappingsData courseGroupMappingRecord)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }
               
                List<CourseGroupMapping> courseGroupMappings = new List<CourseGroupMapping>();


                List<int> GroupIdlist = new List<int>();
                if (courseGroupMappingRecord.GroupId != null)
                {
                    foreach (int item in courseGroupMappingRecord.GroupId)
                    {

                        int groupId = 0;
                        if (item != 0)
                        {
                            groupId = Convert.ToInt32(item);
                        }
                        GroupIdlist.Add(Convert.ToInt32(groupId));

                        CourseGroupMapping courseGroupMappingExisting = await this._courseGroupMappingRepository.Exists(courseGroupMappingRecord.CourseId, groupId);

                        if (courseGroupMappingExisting!= null )
                        {
                            if (courseGroupMappingExisting.IsDeleted == true)
                            {
                                courseGroupMappingExisting.IsDeleted = false;
                                courseGroupMappingExisting.ModifiedDate = DateTime.UtcNow;
                                courseGroupMappingExisting.ModifiedBy = UserId;
                                await this._courseGroupMappingRepository.Update(courseGroupMappingExisting);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        CourseGroupMapping courseGroupMapping = new CourseGroupMapping();
                        courseGroupMapping.GroupId = item;
                        courseGroupMapping.CourseId = courseGroupMappingRecord.CourseId;
                        courseGroupMapping.IsDeleted = false;
                        courseGroupMapping.IsActive = true;
                        courseGroupMapping.ModifiedBy = UserId;
                        courseGroupMapping.ModifiedDate = DateTime.UtcNow;
                        courseGroupMapping.CreatedBy = UserId;
                        courseGroupMapping.CreatedDate = DateTime.UtcNow;
                        courseGroupMappings.Add(courseGroupMapping);
                        
                    }
                }
                await this._courseGroupMappingRepository.AddRange(courseGroupMappings);
                foreach(CourseGroupMapping courseGroupMapping1 in courseGroupMappings)
                {
                    this._courseGroupMappingRepository.UpdateCourseGroupCourseCount(courseGroupMapping1.GroupId);
                }

                //int[] CurrentGroups = GroupIdlist.ToArray();
                //int[] aPIOldGroupId = await this._courseGroupMappingRepository.getGroupIdByCourseId(Convert.ToInt32(courseGroupMappingRecord.CourseId));
                //this._courseGroupMappingRepository.FindElementsNotInArray(CurrentGroups, aPIOldGroupId, Convert.ToInt32(courseGroupMappingRecord.CourseId));

                //var enable_Edcast = await _courseRepository.GetMasterConfigurableParameterValue("Enable_Edcast");
                //bool? PublishCourse =false;
                //PublishCourse = await _courseRepository.IsPublishedCourse(courseGroupMappingRecord.CourseId);
                // _logger.Debug("Enable_Edcast :-" + enable_Edcast);
                //if (Convert.ToString(enable_Edcast).ToLower() == "yes" && PublishCourse == true)
                //{
                //    APIEdcastDetailsToken result = await _courseRepository.GetEdCastToken();
                //    if (result != null)
                //    {                       

                //        APIEdCastTransactionDetails obj = await _courseRepository.PostCourseToClient(courseGroupMappingRecord.CourseId, UserId, result.access_token);
                //    }
                //    else
                //    {
                //        _logger.Debug("Token null from edcast");
                //    }
                //}

                return Ok(courseGroupMappingRecord);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpGet("GetCourseGroupTypeAhead/{search?}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string search = null)
        {
            try
            {
                List<APICourseGroup> coursegroup = await _courseGroupMappingRepository.GetCourseGroupTypeAhead(search);
                return Ok(coursegroup);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [Route("GroupMappingDelete")]
        public async Task<IActionResult> GroupMappingDelete([FromBody] APICourseGroupMappingsDelete courseGroupMappingRecord)
        {
            try
            {
               
                    int Result = await _courseGroupMappingRepository.GroupMappingDelete(courseGroupMappingRecord.Id);
                    if (Result == 1)
                        return Ok();
                    else
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotAvailable), Description = EnumHelper.GetEnumDescription(MessageType.DataNotAvailable) });
              
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostCourseGroup")]
        public IActionResult PostCourseGroup([FromBody] CourseGroup courseGroup)
        {
            if(courseGroup == null)
            {
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "Data is null",Description="Json data is null" });
            }
            if(courseGroup.GroupCode == null)
            {
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "GroupCode is null", Description = "Group Code value is null" });
            }
            if (courseGroup.GroupName == null)
            {
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "GroupName is null", Description = "Group Name value is null" });
            }

            int Result = _courseGroupMappingRepository.SaveCourseGroup(courseGroup,UserId);

            if(Result == 0)
            {
                return Ok(new ResponseMessage { StatusCode = 200, Message = "Group Save Successfully", Description = "Course Group Save Successfully" });
            }
            else if(Result == -1)
            {
                return Conflict(new ResponseMessage { StatusCode = 409, Message = "GroupCode is Duplicate", Description = "Please Check Group Code is Duplicate" });

            }
            else if(Result == -2)
            {
                return Conflict(new ResponseMessage { StatusCode = 409, Message = "GroupName is Duplicate", Description = "Please Check Group Name is Duplicate" });
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpGet("GetCourseGroup/{page:int}/{pageSize:int}/{search?}")]

        public IActionResult GetCourseGroup(int page, int pageSize, string search = null)
        {
            try
            {
                if(page == 0 || pageSize == 0)
                {
                    return BadRequest(new ResponseMessage { StatusCode = 400, Message = "0 value for page or pageSize" });
                }
                return Ok(this._courseGroupMappingRepository.GetAllCourseGroup(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpGet("GetCourseGroupCount/{search:minlength(0)?}")]

        public IActionResult GetCourseGroupCount(string search)
        {
            try
            {
                var Count = this._courseGroupMappingRepository.GetAllCourseGroupCount(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("DeleteCourseGroup")]
        public IActionResult DeleteCourseGroup([FromQuery] string CourseGroupCode)
        {
            try
            {
                if (CourseGroupCode == null || string.IsNullOrEmpty(CourseGroupCode))
                {
                    return BadRequest(new ResponseMessage { Message = "Teams code is null" });
                }
                int Result = this._courseGroupMappingRepository.DeleteCourseGroup(CourseGroupCode);

                if (Result == 0)
                {
                    return Ok(new ResponseMessage { Message = "Record Deleted Successfully" });
                }
                else if (Result == -1)
                {
                    return NotFound(new ResponseMessage { Message = "Course Group Code is not available", Description = "Course Group Code is invalid" });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
        [HttpPost]
        [Route("UpdateCourseGroup")]
        public IActionResult UpdateCourseGroup([FromBody] CourseGroup courseGroup)
        {
            if (courseGroup == null)
            {
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "Data is null", Description = "Json data is null" });
            }
            if (courseGroup.GroupCode == null)
            {
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "GroupCode is null", Description = "Group Code value is null" });
            }
            if (courseGroup.GroupName == null)
            {
                return BadRequest(new ResponseMessage { StatusCode = 400, Message = "GroupName is null", Description = "Group Name value is null" });
            }

            int Result = _courseGroupMappingRepository.UpdateCourseGroup(courseGroup, UserId);

            if (Result == 0)
            {
                return Ok(new ResponseMessage { StatusCode = 200, Message = "Group Update Successfully", Description = "Course Group Update Successfully" });
            }
            else if (Result == -2)
            {
                return Conflict(new ResponseMessage { StatusCode = 409, Message = "GroupCode is Duplicate", Description = "Please Check Group Code is Duplicate" });

            }
            else
            {
                return BadRequest(new ResponseMessage { StatusCode = 409, Message = "Course Group Not Found", Description = "Please Check Course Group Not Found" });
            }
        }

      
    }
}
