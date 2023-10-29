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
    [Route("api/v1/Gamification")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class GamificationController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GamificationController));
        IGamificationRepository _gamificationRepositry;
        private readonly ITokensRepository _tokensRepository;
        public GamificationController(IIdentityService _identitySvc, IGamificationRepository gamificationRepositry, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this._gamificationRepositry = gamificationRepositry;
            this._tokensRepository = tokensRepository;
        }
      
        
        [HttpGet("MissionCounts")]
        public async Task<IActionResult> GetMissionCount()
        {
            try
            {
                ApiResponse Response = await _gamificationRepositry.GetMissionCounts(UserId);
                return Ok(Response.ResponseObject);
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); 
            }
        }
        
        
        [HttpGet("DayWiseMissionCount/{noOfDays}/{missionType?}")]
        public async Task<IActionResult> DayWiseMissionCount(int noOfDays, string missionType)
        {
            try
            {
                ApiResponse Response = await _gamificationRepositry.GetDaywiseMissionCounts(UserId, noOfDays, missionType);
                return Ok(Response.ResponseObject);
            }
            catch(Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); 
            }
        }
    }
}