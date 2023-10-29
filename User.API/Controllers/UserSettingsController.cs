//======================================
// <copyright file="UserSettingsController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Common;
using User.API.Helper;
using User.API.Helper.Log_API_Count;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.AuthorizePermissions;
using static User.API.Common.TokenPermissions;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/User")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class UserSettingsController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSettingsController));
        private IUserSettingsRepository _userSettingsRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        private IUserRepository _userRepository;
        public UserSettingsController(IUserSettingsRepository userSettingsRepository, IUserRepository userReposirotry,IIdentityService identityService, ITokensRepository tokensRepository) : base(identityService)
        {
            this._userSettingsRepository = userSettingsRepository;
            this._identitySvc = identityService;
            this._tokensRepository = tokensRepository;
            this._userRepository = userReposirotry;
        }

        #region UserSetting
        [AllowAnonymous]
        [HttpGet("Setting/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public async Task<IActionResult> Setting(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                var userSettings = await this._userSettingsRepository.GetAllUserSetting(page, pageSize, search, columnName);
                return this.Ok(userSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); 
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); }
        }

        [HttpPost("GetSetting")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> Setting([FromBody] APIUserSearch objAPIUserSearch)
        {
            try
            {
                var userSettings = await this._userSettingsRepository.GetAllUserSettings(objAPIUserSearch);
                return this.Ok(userSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            
            }
        }

        [HttpGet("UserSetting/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public async Task<IActionResult> UserSetting(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                var userSettings = await this._userSettingsRepository.GetAllSetting(page, pageSize, search, columnName);
                return this.Ok(userSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); }
        }

        [HttpPost("UserSetting")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> UserSetting([FromBody] APIUserSearch objAPIUserSearch)
        {
            try
            {
                var userSettings = await this._userSettingsRepository.GetAllSetting(objAPIUserSearch.Page, objAPIUserSearch.PageSize, objAPIUserSearch.Search, objAPIUserSearch.ColumnName);
                return this.Ok(userSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); 
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); }
        }

        [HttpGet("Setting/GetTotalRecords/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment + " " + Permissions.course_completion_status_report)]
        public async Task<IActionResult> GetSettingCount(string search = null, string columnName = null)
        {
            try
            {
                var count = await this._userSettingsRepository.GetSettingCount(search, columnName);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); }
        }

        [HttpPost("GetSettingCount")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.userconfiguration)]
        public async Task<IActionResult> GetSettingCount([FromBody] APIUserSearch objAPIUserSearch)
        {
            try
            {
                var count = await this._userSettingsRepository.GetSettingCount(objAPIUserSearch.Search, objAPIUserSearch.ColumnName);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) }); }
        }

        #endregion UserSetting

        [HttpGet("Settings/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.course_completion_status_report)]
        public async Task<IActionResult> Settings(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                var userSettings = await this._userSettingsRepository.GetAllSettings(page, pageSize, search, columnName);
                return this.Ok(userSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Settings")]
        public async Task<IActionResult> Settings([FromBody] APIUserSearch objAPIUserSearch)
        {
            try
            {
                var userSettings = await this._userSettingsRepository.GetAllSettings(objAPIUserSearch.Page, objAPIUserSearch.PageSize, objAPIUserSearch.Search, objAPIUserSearch.ColumnName);
                return this.Ok(userSettings);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Settings/GetTotalRecords/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment + " " + Permissions.course_completion_status_report)]
        public async Task<IActionResult> GetSettingsCount(string search = null, string columnName = null)
        {
            try
            {
                var count = await this._userSettingsRepository.GetSettingsCount(search, columnName);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Settings/GetTotalRecords")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.userconfiguration)]
        public async Task<IActionResult> GetSettingsCount([FromBody] APIUserSearch objAPIUserSearch)
        {
            try
            {
                var count = await this._userSettingsRepository.GetSettingsCount(objAPIUserSearch.Search, objAPIUserSearch.ColumnName);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("Setting/{id}")]
        
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.userconfiguration)]
        public async Task<IActionResult> Setting(int id)
        {

            var userSetting = await this._userSettingsRepository.GetUserSetting(id);
            return this.Ok(userSetting);
        }

        [HttpPost("SettingById")]
        
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.userconfiguration)]
        public async Task<IActionResult> Setting([FromBody] ApiGetUserById obj)
        {
            try
            {
                var userSetting = await this._userSettingsRepository.GetUserSetting(obj.ID);
                return this.Ok(userSetting);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("Setting")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> Setting([FromBody] UserSettings userSettings)
        {
            if (ModelState.IsValid)
            {

                if (await this._userSettingsRepository.Exist(userSettings.ChangedColumnName))
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                userSettings.CreatedBy = UserId;
                userSettings.CreatedDate = DateTime.UtcNow;
                await this._userSettingsRepository.Add(userSettings);
                return this.Ok();
            }
            return this.BadRequest(this.ModelState);

        }

        [HttpPost("Setting/{id}")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.userconfiguration)]
        public async Task<IActionResult> Setting(int id, [FromBody] UserSettings userSettings)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool valid1 = FileValidation.CheckForSQLInjection(userSettings.ChangedColumnName);
                    if (valid1 == true)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    UserSettings userSettingsObj = await this._userSettingsRepository.Get(u => u.IsDeleted == Record.NotDeleted && u.Id == id);
                    if (userSettingsObj == null)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotExist), Description = EnumHelper.GetEnumDescription(MessageType.NotExist) });

                    if ((userSettingsObj.FieldType != userSettings.FieldType && userSettingsObj.FieldType != null) || userSettingsObj.ConfiguredColumnName != userSettings.ConfiguredColumnName || (userSettings.Id != userSettingsObj.Id))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    //if (userSettings.IsMandatory == true && userSettingsObj.IsMandatory == false)
                    //{
                    //   // bool UserDataExist = await _userRepository.UserDataExist(userSettings.ConfiguredColumnName);
                    //    if (UserDataExist == false)
                    //        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                    //}

                    bool ExistColumnName = await _userSettingsRepository.ExistColumnName(userSettings.ConfiguredColumnName);  // use
                    if (ExistColumnName == true && userSettingsObj.IsConfigured == true && userSettings.IsConfigured == false)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                    userSettingsObj.ConfiguredColumnName = userSettings.ConfiguredColumnName;
                    userSettingsObj.IsConfigured = userSettings.IsConfigured;
                    userSettingsObj.IsMandatory = userSettings.IsMandatory;
                    userSettingsObj.IsShowInReport = userSettings.IsShowInReport;
                    userSettingsObj.IsShowInAnalyticalDashboard = userSettings.IsShowInAnalyticalDashboard;
                    userSettingsObj.ChangedColumnName = userSettings.ChangedColumnName;
                    userSettingsObj.FieldType = userSettings.FieldType;
                    userSettingsObj.ModifiedBy = UserId;
                    userSettingsObj.ModifiedDate = DateTime.UtcNow;
                    userSettingsObj.IsUnique = userSettings.IsUnique;
                    userSettingsObj.IsConfidential = userSettings.IsConfidential;
                    userSettingsObj.IsShowInReportFilter = userSettings.IsShowInReportFilter;
                    await this._userSettingsRepository.Update(userSettingsObj);
                    return this.Ok();
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("Setting")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.userconfiguration)]
        public async Task<IActionResult> SettingDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                UserSettings userSettings = await this._userSettingsRepository.Get(u => u.IsDeleted == Record.NotDeleted && u.Id == DecryptedId);
                if (userSettings == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                }
                userSettings.IsConfigured = false;
                await this._userSettingsRepository.Update(userSettings);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet]
        [Route("Setting/GetConfiguredColumnNames")]
        [PermissionRequired(Permissions.usermanagment + " " + Permissions.userconfiguration)]
        public async Task<IEnumerable<UserSettings>> GetConfiguredColumnNames()
        {
            try
            {
                return await this._userSettingsRepository.GetConfiguredColumnNames();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpGet]
        [Route("Setting/GetConfiguredColumnNamesOnly")]
        public async Task<IActionResult> GetConfiguredColumnNamesOnly()
        {
            try
            {
                return Ok(await this._userSettingsRepository.GetConfiguredColumnNamesOnly());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Setting/GetColumnsForAccessibilty")]
        public async Task<IActionResult> GetColumnsForAccessibilty()
        {
            try
            {
                return Ok(await this._userSettingsRepository.GetColumnsForAccessibilty(OrgCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Setting/GetColumnsForCompetencyJobRole")]
        [PermissionRequired(Permissions.usermanagment)]
        public async Task<IActionResult> GetColumnsForCompetencyJobRole()
        {
            try
            {
                return Ok(await this._userSettingsRepository.GetColumnsForCompetencyJobRole());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("Setting/GetConfiguredColumnRole")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfiguredColumnRole()
        {
            try
            {
                return Ok(await this._userSettingsRepository.GetConfiguredColumnRole(OrgCode, UserRole));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet]
        [Route("Setting/Exist/{changedName}")]
        [PermissionRequired(Permissions.userconfiguration)]
        public async Task<bool> SettingExist(string changedName)
        {
            try
            {
                return await this._userSettingsRepository.Exist(changedName);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpGet]
        [Route("Setting/ExistColumnDataInUserMaster/{columnName}")]
        [PermissionRequired(Permissions.userconfiguration)]
        public async Task<bool> SettingExistColumnDataInUserMaster(string columnName)
        {
            try
            {
                return await this._userSettingsRepository.ExistColumnName(columnName);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
        [HttpGet]
        [Route("Setting/Search")]
        public async Task<IEnumerable<UserSettings>> SettingSearch(string q)
        {
            try
            {
                IEnumerable<UserSettings> result = await this._userSettingsRepository.Search(q);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpGet]
        [Route("Setting/GetConfiguredColumnWithImplicitRoles")]
        [PermissionRequired(Permissions.userconfiguration + " " + Permissions.usermanagment)]
        public async Task<IActionResult> GetConfiguredColumnWithImplicitRoles()
        {
            try
            {
                return Ok(await this._userSettingsRepository.GetConfiguredColumnWithImplicitRoles(OrgCode, UserRole));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}

