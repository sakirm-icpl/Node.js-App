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
    [Route("api/v1/User")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    public class ConfigurationController : IdentityController
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ConfigurationController));
        private readonly IIdentityService _identitySvc;
        private ILocationRepository _locationRepository;
        private IBusinessRepository _businessRepository;
        private IAreaRepository _areaRepository;
        private IGroupRepository _groupRepository;
        private IHRMSRepository _hrmsRepository;
        private IUserSettingsRepository _userSettingsRepository;
        private IConfigure1Repository _configure1Repository;
        private IConfigure2Repository _configure2Repository;
        private IConfigure3Repository _configure3Repository;
        private IConfigure4Repository _configure4Repository;
        private IConfigure5Repository _configure5Repository;
        private IConfigure6Repository _configure6Repository;
        private IConfigure7Repository _configure7Repository;
        private IConfigure8Repository _configure8Repository;
        private IConfigure9Repository _configure9Repository;
        private IConfigure10Repository _configure10Repository;
        private IConfigure11Repository _configure11Repository;
        private IConfigure12Repository _configure12Repository;
        public ConfigurationController(
            IIdentityService identityService,
            ILocationRepository locationRepository,
            IBusinessRepository businessRepository,
            IAreaRepository areaRepository,
            IGroupRepository groupRepository,
            IHRMSRepository hrmsRepository,
            IUserSettingsRepository userSettingsRepository,
            IConfigure1Repository configure1Repository,
            IConfigure2Repository configure2Repository,
            IConfigure3Repository configure3Repository,
            IConfigure4Repository configure4Repository,
            IConfigure5Repository configure5Repository,
            IConfigure6Repository configure6Repository,
            IConfigure7Repository configure7Repository,
            IConfigure8Repository configure8Repository,
            IConfigure9Repository configure9Repository,
            IConfigure10Repository configure10Repository,
            IConfigure11Repository configure11Repository,
            IConfigure12Repository configure12Repository
        ) : base(identityService)
        {
            this._identitySvc = identityService;
            this._locationRepository = locationRepository;
            this._businessRepository = businessRepository;
            this._userSettingsRepository = userSettingsRepository;
            this._groupRepository = groupRepository;
            this._areaRepository = areaRepository;
            this._hrmsRepository = hrmsRepository;
            this._configure1Repository = configure1Repository;
            this._configure2Repository = configure2Repository;
            this._configure3Repository = configure3Repository;
            this._configure4Repository = configure4Repository;
            this._configure5Repository = configure5Repository;
            this._configure6Repository = configure6Repository;
            this._configure7Repository = configure7Repository;
            this._configure8Repository = configure8Repository;
            this._configure9Repository = configure9Repository;
            this._configure10Repository = configure10Repository;
            this._configure11Repository = configure11Repository;
            this._configure12Repository = configure12Repository;
        }
        [HttpGet("Configuration/{columnName}/{search?}/{page:int?}/{pageSize:int?}")]
        public async Task<IActionResult> Get(string columnName, string search = null, int? page = null, int? pageSize = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;

                return this.Ok(await this._configure1Repository.GetConfiguration(columnName, search, page, pageSize,OrgCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ConfigurationGroup/{columnName}/{search?}/{page:int?}/{pageSize:int?}")]
        public async Task<IActionResult> GetConfigurationGroup(string columnName, string search = null, int? page = null, int? pageSize = null)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;

                return this.Ok(await this._configure1Repository.GetConfigurationGroup(columnName, search, page, pageSize));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Configuration/GetTotalRecords/{columnName}/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetCount(string search, string columnName)
        {
            try
            {
                if (search != null)
                    search = search.ToLower().Equals("null") ? null : search;
                var count = await this._configure1Repository.GetConfigurationCount(columnName, search);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Configuration/GetConfigurationColumnDatailsById/{columnName}/{id}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfigurationColumnDatailsById(string columnName, int id)
        {
            try
            {
                var configurationColumn = await this._configure1Repository.GetConfigurationColumnDatails(columnName, id);
                return this.Ok(configurationColumn);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("Configuration/{columnName}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> UpdateConfigurationColumnDatails(string columnName, [FromBody] TypeHeadDto configurationColumnDetails)
        {
            try
            {
                var configurationColumn = await this._configure1Repository.UpdateConfigurationColumnDetails(columnName, configurationColumnDetails);
                if (configurationColumn != "true")
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = configurationColumn });
                }
                return this.Ok(configurationColumn);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("Configuration")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> DeleteConfigurationColumnDatails([FromQuery]string columnName,string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                columnName = Security.Decrypt(columnName);

                var configurationColumn = await this._configure1Repository.DeleteConfigurationColumnDetails(columnName, DecryptedId);
                if (configurationColumn != "true")
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = configurationColumn });
                }
                return this.Ok(configurationColumn);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("ConfigurationName/{columnName}/{search?}/{page:int?}/{pageSize:int?}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfigurationName(string columnName, string search = null, int? page = null, int? pageSize = null)
        {
            try
            {
                IEnumerable<TypeHeadDto> Result = await this._configure1Repository.GetConfiguration(columnName, search, page, pageSize);
                return this.Ok(Result.Select(t => t.Name).ToList());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAllLocations/{search}")]
        
       [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllLocations(string search)
        {
            try
            {
                var business = await this._locationRepository.GetAllLocations(search);
                return this.Ok(business);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpGet("GetLocations")]
        
       [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                var locations = await this._locationRepository.GetLocationNames();
                return this.Ok(locations.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllBusinesss/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllBusiness(string search)
        {
            try
            {
                var business = await this._businessRepository.GetAllBusiness(search);
                return this.Ok(business);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetBusinesss")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetBusiness()
        {
            try
            {
                var business = await this._businessRepository.GetBusinessNames();
                return this.Ok(business.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllAreas/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllAreas(string search)
        {
            try
            {
                var business = await this._areaRepository.GetAllAreas(search);
                return this.Ok(business);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAreas")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetArea()
        {
            try
            {
                var area = await this._areaRepository.GetAreaNames();
                return this.Ok(area.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllGroups/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllGroups(string search)
        {
            try
            {
                var group = await this._groupRepository.GetAllGroups(search);
                return this.Ok(group);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

      
        [HttpGet("GetGroups")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetGroup()
        {
            try
            {
                var group = await this._groupRepository.GetGroupNames();
                return this.Ok(group.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration1/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration1(string search)
        {
            try
            {
                var Configuration1 = await this._configure1Repository.GetAllConfiguration1(search);
                return this.Ok(Configuration1);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetConfiguration1")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfiguration1()
        {
            try
            {
                var group = await this._configure1Repository.GetConfigurationNames();
                return this.Ok(group.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration2/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration2(string search)
        {
            try
            {
                var Configuration2 = await this._configure2Repository.GetAllConfiguration2(search);
                return this.Ok(Configuration2);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetConfiguration2")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfiguration2()
        {
            try
            {
                var group = await this._configure2Repository.GetConfigurationNames();
                return this.Ok(group.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration3/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration3(string search)
        {
            try
            {
                var Configuration3 = await this._configure3Repository.GetAllConfiguration3(search);
                return this.Ok(Configuration3);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetConfiguration3")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfiguration3()
        {
            try
            {
                var group = await this._configure3Repository.GetConfigurationNames();
                return this.Ok(group.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration4/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration4(string search)
        {
            try
            {
                var Configuration4 = await this._configure4Repository.GetAllConfiguration4(search);
                return this.Ok(Configuration4);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetConfiguration4")]
        public async Task<IActionResult> GetConfiguration4()
        {
            try
            {
                var group = await this._configure4Repository.GetConfigurationNames();
                return this.Ok(group.ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration5/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration5(string search)
        {
            try
            {
                var Configuration5 = await this._configure5Repository.GetAllConfiguration5(search);
                return this.Ok(Configuration5);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration6/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration6(string search)
        {
            try
            {
                var Configuration6 = await this._configure6Repository.GetAllConfiguration6(search);
                return this.Ok(Configuration6);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration7/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration7(string search)
        {
            try
            {
                var Configuration7 = await this._configure7Repository.GetAllConfiguration7(search);
                return this.Ok(Configuration7);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllConfiguration8/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration8(string search)
        {
            try
            {
                var Configuration8 = await this._configure8Repository.GetAllConfiguration8(search);
                return this.Ok(Configuration8);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllConfiguration9/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration9(string search)
        {
            try
            {
                var Configuration9 = await this._configure9Repository.GetAllConfiguration9(search);
                return this.Ok(Configuration9);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllConfiguration10/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration10(string search)
        {
            try
            {
                var Configuration10 = await this._configure10Repository.GetAllConfiguration10(search);
                return this.Ok(Configuration10);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllConfiguration11/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration11(string search)
        {
            try
            {
                var Configuration11 = await this._configure11Repository.GetAllConfiguration11(search);
                return this.Ok(Configuration11);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetAllConfiguration12/{search}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetAllConfiguration12(string search)
        {
            try
            {
                var Configuration12 = await this._configure12Repository.GetAllConfiguration12(search);
                return this.Ok(Configuration12);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetConfiguration12")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfiguration12()
        {
            try
            {
                var Configuration12 = await this._configure12Repository.GetConfiguration12();
                return this.Ok(Configuration12);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
