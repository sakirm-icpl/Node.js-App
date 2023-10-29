using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Model.ILT;
using ILT.API.Repositories.Interfaces;
//using ILT.API.Repositories.Interfaces.ILT;
using ILT.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static ILT.API.Common.TokenPermissions;
using ILT.API.Helper;
using log4net;
using ILT.API.Model.Log_API_Count;

namespace ILT.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/i/ILTOnlineSetting")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class ILTOnlineSettingController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTOnlineSettingController));
        private readonly ITokensRepository _tokensRepository;
        public IILTOnlineSetting _iLTOnlineSettingRepository;
        public ILTOnlineSettingController(ITokensRepository tokensRepository, IIdentityService identitySvc, IILTOnlineSetting iLTOnlineSettingRepository) : base(identitySvc)
        {
            this._tokensRepository = tokensRepository;
            this._iLTOnlineSettingRepository = iLTOnlineSettingRepository;
        }


        [HttpGet("{page}/{pageSize}/{searchText?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._iLTOnlineSettingRepository.GetAllOnlineSetting(page, pageSize, searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("TotalCount/{searchText?}")]
        public async Task<IActionResult> GetCount(string searchText = null)
        {
            try
            {
                if (searchText != null)
                    searchText = searchText.ToLower().Equals("null") ? null : searchText;
                return Ok(await this._iLTOnlineSettingRepository.GetILTOnlineSettingCount(searchText));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost]
        public async Task<IActionResult> PostTopics([FromBody] APIILTOnlineSetting obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int Id = await _iLTOnlineSettingRepository.Exists(obj.Type);
                if (Id != 0)
                {
                    ILTOnlineSetting onlineSetting = await _iLTOnlineSettingRepository.Get(Id);
                    onlineSetting.UserID = obj.UserID;
                    onlineSetting.ClientID = obj.ClientID;
                    onlineSetting.ClientSecret = obj.ClientSecret;
                    onlineSetting.Password = obj.Password;
                    onlineSetting.RedirectUri = obj.RedirectUri;
                    onlineSetting.Type = obj.Type;
                    onlineSetting.TeamsAuthority = obj.TeamsAuthority;
                    await _iLTOnlineSettingRepository.Update(onlineSetting);

                }
                else
                {
                    ILTOnlineSetting onlineSetting = new ILTOnlineSetting();
                    onlineSetting.ID = obj.ID;
                    onlineSetting.UserID = obj.UserID;
                    onlineSetting.ClientID = obj.ClientID;
                    onlineSetting.ClientSecret = obj.ClientSecret;
                    onlineSetting.Password = obj.Password;
                    onlineSetting.RedirectUri = obj.RedirectUri;
                    onlineSetting.Type = obj.Type;
                    onlineSetting.TeamsAuthority = obj.TeamsAuthority;
                    await _iLTOnlineSettingRepository.Add(onlineSetting);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = ex.Message });
            }
        }
    }
}