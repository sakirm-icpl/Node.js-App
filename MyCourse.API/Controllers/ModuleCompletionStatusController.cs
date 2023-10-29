using AspNet.Security.OAuth.Introspection;
using AutoMapper;
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
using MyCourse.API.Helper;
using log4net;
using MyCourse.API.Model.Log_API_Count;

namespace MyCourse.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/ModuleCompletionStatus")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ModuleCompletionStatusController : IdentityController

    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleCompletionStatusController));
        IModuleCompletionStatusRepository _moduleCompletionStatusRepository;
        ICourseCompletionStatusRepository _courseCompletionStatusRepository;
        private readonly ITokensRepository _tokensRepository;
        public ModuleCompletionStatusController(IModuleCompletionStatusRepository moduleCompletionStatusRepository, ICourseCompletionStatusRepository courseCompletionStatusRepository, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            _moduleCompletionStatusRepository = moduleCompletionStatusRepository;
            _courseCompletionStatusRepository = courseCompletionStatusRepository;
            this._tokensRepository = tokensRepository;
        }
        
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _moduleCompletionStatusRepository.GetAll());
            }
            catch(Exception ex)
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
                return Ok(await _moduleCompletionStatusRepository.Get(id));
            }
            catch(Exception ex)
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
                ModuleCompletionStatus ModuleCompletionStatus = await _moduleCompletionStatusRepository.Get(userId, courseId, moduleId);
                return Ok(ModuleCompletionStatus);
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

       
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]APIModuleCompletionStatus apiModuleCompletionStatus)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    ModuleCompletionStatus moduleCompletionStatus = Mapper.Map<Model.ModuleCompletionStatus>(apiModuleCompletionStatus);
                    moduleCompletionStatus.CreatedDate = DateTime.UtcNow;
                    moduleCompletionStatus.ModifiedDate = DateTime.UtcNow;
                    moduleCompletionStatus.UserId = this.UserId;
                    await _moduleCompletionStatusRepository.Post(moduleCompletionStatus);
                    return Ok(apiModuleCompletionStatus);
                }
                return BadRequest(ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

      
        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]APIModuleCompletionStatus apiModuleCompletionStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                ModuleCompletionStatus ModuleCompletionStatus = await _moduleCompletionStatusRepository.Get(id);
                if (ModuleCompletionStatus == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                ModuleCompletionStatus = Mapper.Map<Model.ModuleCompletionStatus>(apiModuleCompletionStatus);
                ModuleCompletionStatus.UserId = this.UserId;
                await _moduleCompletionStatusRepository.Update(ModuleCompletionStatus);
                return Ok();
            }
            catch(Exception ex)
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
                ModuleCompletionStatus ModuleCompletionStatus = await _moduleCompletionStatusRepository.Get(DecryptedId);
                if (ModuleCompletionStatus == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                ModuleCompletionStatus.IsDeleted = true;
                ModuleCompletionStatus.UserId = this.UserId;
                await _moduleCompletionStatusRepository.Update(ModuleCompletionStatus);
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); 
            }
        }

        [HttpGet("GetModuleStatus/{courseId}/{moduleId}")]
        public async Task<IActionResult> GetStatus(int courseId, int moduleId)
        {
            try
            {
                return Ok(await _moduleCompletionStatusRepository.GetStatus(courseId, moduleId, UserId));
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseCompletionStatus/{courseId}")]
        public async Task<IActionResult> GetCourseCompletionStatus(int courseId)
        {
            try
            {
                return Ok(await _courseCompletionStatusRepository.GetStatus(courseId));
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); 
            }
        }


        [HttpGet("GetCourseCompletionDate/{courseId}")]
        public async Task<IActionResult> GetCourseCompletionDate(int courseId, int userId)
        {
            try
            {
                return Ok(await _courseCompletionStatusRepository.GetCourseCompletionDate(courseId, userId));
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetModuleStatus/{page:int}/{pageSize:int}/{courseId}/{status?}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetModuleStatus(int page, int pageSize, int courseId, string status = null)
        {
            try
            {
                return Ok(await _moduleCompletionStatusRepository.GetModuleStatus(UserId, courseId, page, pageSize, status));
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
      
        [HttpGet("GetModuleStatusCount/{courseId}/{status?}")]
        public async Task<IActionResult> GetModuleStatusCount(int courseId, string status = null)
        {
            try
            {
                return Ok(await _moduleCompletionStatusRepository.GetModuleCount(UserId, courseId, status));
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}