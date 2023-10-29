using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using com.pakhee.common;
using MyCourse.API.APIModel;
using MyCourse.API.Common;
using MyCourse.API.Model;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static MyCourse.API.Common.TokenPermissions;
using static MyCourse.API.Helper.Security;
using MyCourse.API.Helper;
using log4net;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using MyCourse.API.Model.Log_API_Count;

namespace MyCourse.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/ContentCompletionStatus")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ContentCompletionStatusController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ContentCompletionStatusController));
        IContentCompletionStatus _contentCompletionStatus;
      //  ICourseApplicability _courseApplicability;
        private readonly ITokensRepository _tokensRepository;
        private readonly IScormVarResultRepository _scormVarResultRepository;
        private readonly INodalCourseRequestsRepository _nodalCourseRequestsRepository;
        private readonly IConfiguration configuration;
        public ContentCompletionStatusController(IContentCompletionStatus contentCompletionStatus, /*ICourseApplicability courseApplicability,*/ IIdentityService _identitySvc, ITokensRepository tokensRepository, IScormVarResultRepository scormVarResultRepository, INodalCourseRequestsRepository nodalCourseRequestsRepository, IConfiguration configuration) : base(_identitySvc)
        {
            _contentCompletionStatus = contentCompletionStatus;
          //  _courseApplicability = courseApplicability;
            this._tokensRepository = tokensRepository;
            _scormVarResultRepository = scormVarResultRepository;
            _nodalCourseRequestsRepository = nodalCourseRequestsRepository;
            this.configuration = configuration;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _contentCompletionStatus.GetAll());
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
                return Ok(await _contentCompletionStatus.Get(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{userId}/{courseId:int}/{moduleId:int}")]
        public async Task<IActionResult> Get(int userId, int courseId, int moduleId)
        {
            try
            {
                ContentCompletionStatus ContentCompletionStatus = await _contentCompletionStatus.Get(userId, courseId, moduleId);
                return Ok(ContentCompletionStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

  
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APIContentCompletionStatus apiContentCompletionStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!IsiOS)
                {
                    if (!DecryptForUI(apiContentCompletionStatus.Status).Contains(apiContentCompletionStatus.CourseId.ToString()))
                    {
                        return BadRequest(ModelState);
                    }

                    if (DecryptForUI(apiContentCompletionStatus.Status).Contains("completed"))
                    {
                        apiContentCompletionStatus.Status = "completed";
                    }
                    else if (DecryptForUI(apiContentCompletionStatus.Status).Contains("inprogress"))
                    {
                        apiContentCompletionStatus.Status = "inprogress";
                    }
                    else
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }
                else
                {
                    string Key = "a1b2c3d4e5f6g7h8";
                    string _initVector = "1234123412341234";

                    if (!CryptLib.decrypt((apiContentCompletionStatus.Status), Key, _initVector).Contains(apiContentCompletionStatus.CourseId.ToString()))
                    {
                        return BadRequest(ModelState);
                    }

                    if (CryptLib.decrypt((apiContentCompletionStatus.Status), Key, _initVector).Contains("completed"))
                    {
                        apiContentCompletionStatus.Status = "completed";
                    }
                    else if (CryptLib.decrypt((apiContentCompletionStatus.Status), Key, _initVector).Contains("inprogress"))
                    {
                        apiContentCompletionStatus.Status = "inprogress";
                    }
                    else
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    }
                }

                if (string.Equals(OrgCode, configuration["IAAOrgCode"], StringComparison.InvariantCultureIgnoreCase) && apiContentCompletionStatus.GroupId!=null && apiContentCompletionStatus.GroupId>0)
                {
                    APIScormGroup aPIScormGroups = await _nodalCourseRequestsRepository.GetUserforCompletion((int)apiContentCompletionStatus.GroupId);

                    bool IsCompleted = await _scormVarResultRepository.IsContentCompleted(aPIScormGroups.UserId, apiContentCompletionStatus.CourseId, apiContentCompletionStatus.ModuleId);
                    if (IsCompleted)
                    {
                        ContentCompletionStatus ContentCompletionStatus = Mapper.Map<Model.ContentCompletionStatus>(apiContentCompletionStatus);
                        ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                        ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                        ContentCompletionStatus.UserId = aPIScormGroups.UserId;
                        await _contentCompletionStatus.Post(ContentCompletionStatus, null, null, OrganisationCode);
                        return Ok(apiContentCompletionStatus);
                    }
                    else
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.ContentNotCompleted) });
                    }
                }
                else
                {
                    ContentCompletionStatus ContentCompletionStatus = Mapper.Map<Model.ContentCompletionStatus>(apiContentCompletionStatus);
                    ContentCompletionStatus.CreatedDate = DateTime.UtcNow;
                    ContentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                    ContentCompletionStatus.UserId = this.UserId;
                    await _contentCompletionStatus.Post(ContentCompletionStatus, null, null, OrganisationCode);
                    return Ok(apiContentCompletionStatus);
                }
                
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APIContentCompletionStatus apiContentCompletionStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ContentCompletionStatus ContentCompletionStatus = await _contentCompletionStatus.Get(id);
                if (ContentCompletionStatus == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                ContentCompletionStatus = Mapper.Map<Model.ContentCompletionStatus>(apiContentCompletionStatus);
                ContentCompletionStatus.UserId = this.UserId;
                await _contentCompletionStatus.Update(ContentCompletionStatus);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                ContentCompletionStatus ContentCompletionStatus = await _contentCompletionStatus.Get(DecryptedId);
                if (ContentCompletionStatus == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                ContentCompletionStatus.IsDeleted = true;
                ContentCompletionStatus.UserId = this.UserId;
                await _contentCompletionStatus.Update(ContentCompletionStatus);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetContentStatus/{courseId}/{moduleId}")]
        public async Task<IActionResult> GetStatus(int courseId, int moduleId)
        {
            try
            {
                return Ok(await _contentCompletionStatus.GetStatus(courseId, moduleId, UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("SaveKpointStatus")]
        public async Task<IActionResult> SaveKpointStatus([FromBody] APIContentCompletionStatusForKpoint aPIContentCompletionStatusForKpoint)
        {
            KpointStatus kpointStatus = await _contentCompletionStatus.SaveKpointStatus(aPIContentCompletionStatusForKpoint,UserId);

            if(kpointStatus != null)
            {
                ContentCompletionStatus contentCompletionStatus = new ContentCompletionStatus();
                contentCompletionStatus.CreatedDate = DateTime.UtcNow;
                contentCompletionStatus.ModifiedDate = DateTime.UtcNow;
                contentCompletionStatus.UserId = this.UserId;
                contentCompletionStatus.CourseId = aPIContentCompletionStatusForKpoint.CourseId;
                contentCompletionStatus.ModuleId = aPIContentCompletionStatusForKpoint.ModuleId;
                contentCompletionStatus.ScheduleId = 0;
                contentCompletionStatus.IsDeleted = false;
                contentCompletionStatus.Location = "";

                if (kpointStatus.watch_duration == kpointStatus.duration && kpointStatus.duration != 0)
                {
                    contentCompletionStatus.Status = "Completed";
                    await _contentCompletionStatus.Post(contentCompletionStatus, null, null, OrganisationCode);
                    return Ok();
                }
                else if(kpointStatus.watch_duration == 0)
                {
                    return BadRequest();
                }
                else if (kpointStatus.watch_duration > 0 && kpointStatus.watch_duration < kpointStatus.duration)
                {
                    contentCompletionStatus.Status = "inprogress";
                    await _contentCompletionStatus.Post(contentCompletionStatus, null, null, OrganisationCode);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }

        [HttpPost("GetKpointReport")]
        public async Task<IActionResult> GetKpointReport([FromBody] APIForKpointReport aPIForKpointReport)
        {
            List<KPointReportV2> kPointReports = await _contentCompletionStatus.getKpointReport(aPIForKpointReport,UserId);
            return Ok(kPointReports);
        }

    }
}