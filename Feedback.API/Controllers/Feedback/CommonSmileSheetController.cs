using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using log4net;
using Feedback.API.Helper;
using Feedback.API.Model.Log_API_Count;
using static Feedback.API.Common.TokenPermissions;
using static Courses.API.Common.AuthorizePermissions;
using Feedback.API.Common;
using Feedback.API.Repositories.Interfaces;
using Feedback.API.Services;
using Feedback.API.Model;

namespace Feedback.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/CommonSmile")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired(Permissions.lcmsmanage)]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class CommonSmileSheetController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CommonSmileSheetController));
        ICommonSmile _commonSmile;
        private readonly ITokensRepository _tokensRepository;
        public CommonSmileSheetController(ICommonSmile commonSmile, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            _commonSmile = commonSmile;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            try
            {
                
                var commonSmile = _commonSmile.GetAll();
                if (commonSmile == null)
                {
                    return NotFound();
                }
                return Ok(commonSmile);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var commonSmile = _commonSmile.Get(id);
                if (commonSmile == null)
                {
                    return NotFound();
                }
                return Ok(commonSmile);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public IActionResult Get(int page, int pageSize, string? search = null, string? columnName = null)
        {
            try
            {

                List<CommonSmileSheet> commonSmileSheet = _commonSmile.Get(page, pageSize, search);
                return Ok(commonSmileSheet);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getTotalRecords/{search:minlength(0)?}/{columnName?}")]
        public IActionResult GetCount(string? search = null, string? columnName = null)
        {
            try
            {
                int count = _commonSmile.Count(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        public IActionResult Post([FromBody] CommonSmileSheet commonSmileSheet)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    if (_commonSmile.Exists(commonSmileSheet.QuestionText))
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    else
                    {
                        commonSmileSheet.CreatedDate = DateTime.UtcNow;
                        commonSmileSheet.ModifiedDate = DateTime.UtcNow;
                        _commonSmile.Add(commonSmileSheet);
                    }

                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CommonSmileSheet commonSmileSheet)
        {
            try
            {
                CommonSmileSheet commonSmile = await _commonSmile.Get(id);
                if (commonSmile == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                await _commonSmile.Update(commonSmileSheet);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete]

        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                CommonSmileSheet commonSmile = await _commonSmile.Get(DecryptedId);
                if (commonSmile == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                commonSmile.IsDeleted = true;
                await _commonSmile.Update(commonSmile);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Exists")]
        public IActionResult Exists(string q)
        {
            try
            {
                bool code = _commonSmile.Exists(q);
                if (code == true)
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetSectionTypehead/{search?}")]
        public IActionResult GetSectionTypehead(string? search = null)
        {
            try
            {
                return Ok(_commonSmile.GetSectionTypehead(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
