using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Feedback.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Courses.API.Common.AuthorizePermissions;
using log4net;
using Feedback.API.Common;
using Feedback.API.Helper;
using Feedback.API.Model.Log_API_Count;
using Feedback.API.Services;
using static Feedback.API.Common.TokenPermissions;
using Feedback.API.Model;
using Feedback.API.APIModel;

namespace Feedback.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired("Discontinued")]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    //Remove this controller
    public class TrainerFeedbackController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TrainerFeedbackController));
        ITrainerFeedbackRepository _trainerFeedback;
        private readonly ITokensRepository _tokensRepository;
        public TrainerFeedbackController(ITrainerFeedbackRepository trainerFeedback, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            _trainerFeedback = trainerFeedback;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            try
            {
                if (_trainerFeedback.Count() == 0)
                {
                    return NotFound();
                }
                return Ok(_trainerFeedback.GetAll());
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
                if (_trainerFeedback.Get(id) == null)
                {
                    return NotFound();
                }
                return Ok(_trainerFeedback.GetFeedback(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public IActionResult Get(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {

                List<TrainerFeedback> trainerFeedback = _trainerFeedback.Get(page, pageSize, search);
                return Ok(trainerFeedback);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("getTotalRecords/{search:minlength(0)?}/{columnName?}")]
        public IActionResult GetCount(string search = null, string columnName = null)
        {
            try
            {
                int count = _trainerFeedback.Count(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        public IActionResult Post([FromBody] List<TrainerFeedbackAPI> trainerFeedbackAPIs)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    foreach (TrainerFeedbackAPI trainerFeedbackAPI in trainerFeedbackAPIs)
                    {
                        TrainerFeedback trainerFeedback = Mapper.Map<TrainerFeedback>(trainerFeedbackAPI);
                        trainerFeedback.Option1 = trainerFeedbackAPI.Options.Length > 0 ? trainerFeedbackAPI.Options[0].option : null;
                        trainerFeedback.Option2 = trainerFeedbackAPI.Options.Length > 1 ? trainerFeedbackAPI.Options[1].option : null;
                        trainerFeedback.Option3 = trainerFeedbackAPI.Options.Length > 2 ? trainerFeedbackAPI.Options[2].option : null;
                        trainerFeedback.Option4 = trainerFeedbackAPI.Options.Length > 3 ? trainerFeedbackAPI.Options[3].option : null;
                        trainerFeedback.Option5 = trainerFeedbackAPI.Options.Length > 4 ? trainerFeedbackAPI.Options[4].option : null;
                        trainerFeedback.CreatedDate = DateTime.UtcNow;
                        trainerFeedback.ModifiedDate = DateTime.UtcNow;
                        _trainerFeedback.Add(trainerFeedback);
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
        public async Task<IActionResult> Put(int id, [FromBody] TrainerFeedbackAPI trainerFeedbackAPI)
        {
            try
            {
                TrainerFeedback trainerFeed = await _trainerFeedback.Get(id);
                if (trainerFeed == null)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                TrainerFeedback trainerFeedback = Mapper.Map<TrainerFeedback>(trainerFeedbackAPI);
                trainerFeedback.Option1 = trainerFeedbackAPI.Options.Length > 0 ? trainerFeedbackAPI.Options[0].option : null;
                trainerFeedback.Option2 = trainerFeedbackAPI.Options.Length > 1 ? trainerFeedbackAPI.Options[1].option : null;
                trainerFeedback.Option3 = trainerFeedbackAPI.Options.Length > 2 ? trainerFeedbackAPI.Options[2].option : null;
                trainerFeedback.Option4 = trainerFeedbackAPI.Options.Length > 3 ? trainerFeedbackAPI.Options[3].option : null;
                trainerFeedback.Option5 = trainerFeedbackAPI.Options.Length > 4 ? trainerFeedbackAPI.Options[4].option : null;
                trainerFeedback.ModifiedDate = DateTime.UtcNow;
                await _trainerFeedback.Update(trainerFeedback);
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
                TrainerFeedback trainerFeedback = await _trainerFeedback.Get(DecryptedId);
                if (trainerFeedback == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                trainerFeedback.IsDeleted = true;
                await _trainerFeedback.Update(trainerFeedback);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}