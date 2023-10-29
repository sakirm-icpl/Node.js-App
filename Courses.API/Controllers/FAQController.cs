using AspNet.Security.OAuth.Introspection;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Courses.API.Common.TokenPermissions;
using Courses.API.Helper;
using log4net;
using System;
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/FAQ")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    //added to check expired token 
    [TokenRequired()]
    //Time being added role based
    [Authorize(Roles = "CA")]
    public class FAQController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FAQController));
        IFaqRepository _faqRepository;
        private readonly ITokensRepository _tokensRepository;
        public FAQController(IFaqRepository faqRepository, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this._faqRepository = faqRepository;
            this._tokensRepository = tokensRepository;
        }
    
        [HttpGet("{lcmsId}")]
        public async Task<IActionResult> Get(int lcmsId)
        {
            try
            {
                return Ok(await this._faqRepository.GetApiFaqByLcmsId(lcmsId));
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
       
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ApiFaq apiFaq)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Message Result = await this._faqRepository.PostFaq(apiFaq, UserId);

                if (Result == Message.Duplicate)
                    return StatusCode(409, "Duplicate");

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); 
            }
        }
       
        [HttpPost("{lcmsId}")]
        public async Task<IActionResult> Put(int lcmsId, [FromBody] ApiFaq apiFaq)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                apiFaq.LcmsId = lcmsId;
                Message Result = await this._faqRepository.PutFaq(apiFaq, UserId);

                if (Result == Message.Duplicate)
                    return this.BadRequest(new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.Duplicate),
                        Description = EnumHelper.GetEnumDescription(MessageType.Duplicate)
                    });
                if (Result == Message.NotFound)
                    return this.BadRequest(new ResponseMessage
                    {
                        Message = EnumHelper.GetEnumName(MessageType.NotFound),
                        Description = EnumHelper.GetEnumDescription(MessageType.NotFound)
                    });

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); 
            }
        }
    }
}