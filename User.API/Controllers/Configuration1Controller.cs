using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Common;
using User.API.Helper;
using User.API.Helper.Log_API_Count;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.AuthorizePermissions;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/u/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    public class Configuration1Controller : IdentityController
    {
        private readonly IIdentityService _identitySvc;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configuration1Controller));
        private IConfigure1Repository _configure1Repository;

        public Configuration1Controller(
            IIdentityService identityService,
            IConfigure1Repository configure1Repository

        ) : base(identityService)
        {
            this._identitySvc = identityService;
            this._configure1Repository = configure1Repository;
        }
        [HttpGet("{page:int?}/{pageSize:int?}/{search?}")]
        [PermissionRequired(Permissions.config1)]
        public async Task<IActionResult> Get(int? page = null, int? pageSize = null, string search = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;

                return this.Ok(await this._configure1Repository.GetConfiguration(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.config1)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                var count = await this._configure1Repository.GetConfiguration1Count(search);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("UpdateConfigurationColumnDatails")]
        [PermissionRequired(Permissions.config1)]
        public async Task<IActionResult> UpdateConfigurationColumnDatails([FromBody] ApiConfiguration1 configurationColumnDetails)
        {
            try
            {
                var configurationColumn = await this._configure1Repository.PutConfigurationColumnDetails(configurationColumnDetails);
                if (configurationColumn != "true")
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumName(MessageType.Duplicate) });
                }
                return this.Ok(configurationColumn);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [PermissionRequired(Permissions.config1)]
        public async Task<IActionResult> PostCofigurationColumnDetails([FromBody] ApiConfiguration1 configuration1Details)
        {
            try
            {
                var configurationColumn = await this._configure1Repository.PostConfigurationColumnDetails(configuration1Details);
                if (configurationColumn != "true")
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumName(MessageType.Duplicate) });
                }
                return this.Ok(configurationColumn);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.config1)]
        public async Task<IActionResult> DeleteConfigurationColumnDatails([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                var configurationColumn = await this._configure1Repository.DeleteConfiguration1Details(DecryptedId);
                if (configurationColumn != "true")
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumName(MessageType.DependancyExist) });
                }
                return this.Ok(configurationColumn);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }
}