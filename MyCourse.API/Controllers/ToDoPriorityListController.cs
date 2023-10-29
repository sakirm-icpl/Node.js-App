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
using static MyCourse.API.Common.AuthorizePermissions;
using static MyCourse.API.Common.TokenPermissions;
using MyCourse.API.Helper;
using log4net;
using MyCourse.API.Model.Log_API_Count;

namespace MyCourse.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/ToDoPriorityList")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ToDoPriorityListController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ToDoPriorityListController));
        IToDoPriorityList _toDoPriorityList;
        private readonly ITokensRepository _tokensRepository;
        public ToDoPriorityListController(IIdentityService identitySvc, IToDoPriorityList toDoPriorityList, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this._toDoPriorityList = toDoPriorityList;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet("{id}")]
        [PermissionRequired(Permissions.todo)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await _toDoPriorityList.GetById(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [PermissionRequired(Permissions.todo)]
        public async Task<IActionResult> Post([FromBody] ApiToDoPriorityList apiToDoModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (apiToDoModel.ScheduleDate < DateTime.Now.Date)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else if (apiToDoModel.EndDate < apiToDoModel.ScheduleDate)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                }
                ToDoPriorityList objToDoList = new ToDoPriorityList();
                objToDoList = Mapper.Map<ToDoPriorityList>(apiToDoModel);
                objToDoList.CreatedBy = UserId;
                objToDoList.CreatedDate = DateTime.UtcNow;
                await _toDoPriorityList.Add(objToDoList);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("{id}")]
        [PermissionRequired(Permissions.todo)]
        public async Task<IActionResult> Put(int id, [FromBody] ApiToDoPriorityList apiToDoModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ToDoPriorityList objToDoList = await this._toDoPriorityList.Get(id);
                if (objToDoList == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                //13. Missing Server Side Validation // 
                if (objToDoList.Type != apiToDoModel.Type || objToDoList.RefId != apiToDoModel.RefId)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                objToDoList.IsDeleted = apiToDoModel.IsDeleted;
                objToDoList.ScheduleDate = apiToDoModel.ScheduleDate;
                objToDoList.EndDate = apiToDoModel.EndDate;
                objToDoList.Priority = apiToDoModel.Priority;
                objToDoList.RefId = apiToDoModel.RefId;
                objToDoList.Type = apiToDoModel.Type;
                objToDoList.ModifiedBy = UserId;
                objToDoList.ModifiedDate = DateTime.UtcNow;
                await _toDoPriorityList.Update(objToDoList);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.todo)]
        public async Task<IActionResult> GetAll(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await _toDoPriorityList.GetAll(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("count/{search?}")]
        [PermissionRequired(Permissions.todo)]
        public async Task<IActionResult> GetCount(string search = null)
        {
            try
            {
                return Ok(await _toDoPriorityList.Count(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetToDoList")]
        public async Task<IActionResult> GetToDoList()
        {
            try
            {
                return Ok(await _toDoPriorityList.GetToDoList(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetQuizToDoList")]
        public async Task<IActionResult> GetQuizToDoList()
        {
            try
            {
                return Ok(await _toDoPriorityList.GetQuizToDoList(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetSurveyToDoList")]
        public async Task<IActionResult> GetSurveyToDoList()
        {
            try
            {
                return Ok(await _toDoPriorityList.GetSurveyToDoList(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetCourseToDoList")]
        public async Task<IActionResult> GetCourseToDoList()
        {
            try
            {
                return Ok(await _toDoPriorityList.GetCourseToDoList(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetRefName")]
        public async Task<IActionResult> GetRefName()
        {
            try
            {
                return Ok(await _toDoPriorityList.GetSurveyToDoList(UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}