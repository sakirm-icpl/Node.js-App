//======================================
// <copyright file="KeyAreaSettingController.cs" company="Enthralltech Pvt. Ltd.">
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
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class KeyAreaSettingController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(KeyAreaSettingController));
        private IKeyAreaSettingRepository keyAreaSettingRepository;
        private IRewardsPoint _rewardsPoint;
        private readonly ITokensRepository _tokensRepository;

        public KeyAreaSettingController(IKeyAreaSettingRepository keyAreaSettingController,
            IRewardsPoint rewardsPoint,
            ITokensRepository tokensRepository,
            IIdentityService identitySvc) : base(identitySvc)
        {
            this.keyAreaSettingRepository = keyAreaSettingController;
            this._rewardsPoint = rewardsPoint;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> Get()
        {
            try
            {
                IEnumerable<APIKeyAreaSetting> keyArea = await this.keyAreaSettingRepository.GetAllRecordKeyArea();
                return Ok(keyArea);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("KeyAreaSettingRecord")]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> GetKeyAreaSettingRecord()
        {
            try
            {
                IEnumerable<APIKeyAreaSetting> roleResponsibility = await this.keyAreaSettingRepository.GetAllKeyAreaSettingRecord();
                if (roleResponsibility != null)
                    await this._rewardsPoint.KeyAreaSettingsReadRewardPoint(UserId);
                return Ok(roleResponsibility);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<APIKeyAreaSetting> aPIKeyArea = await this.keyAreaSettingRepository.GetAllKeyArea(UserId, page, pageSize, search);
                return Ok(aPIKeyArea);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var count = await this.keyAreaSettingRepository.Count(UserId, search);
                return this.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                IEnumerable<APIKeyAreaSetting> keyArea = await this.keyAreaSettingRepository.GetKeyArea(id);
                return Ok(keyArea);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("GetKeyAreaSetting")]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> GetKeyAreaSetting()
        {
            try
            {

                IEnumerable<APIKeyAreaSetting> keyAreaSetting = await this.keyAreaSettingRepository.GetKeyAreaSetting(UserId);
                return Ok(keyAreaSetting);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> Post([FromBody] KeyAreaSetting keyAreaSetting)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    KeyAreaSetting keyAreaSett = new KeyAreaSetting();
                    if (await this.keyAreaSettingRepository.Exist(keyAreaSetting.KeyResultAreaDescription, keyAreaSetting.UserId))
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    keyAreaSett.UserId = keyAreaSetting.UserId;
                    keyAreaSett.KeyAreaUserId = keyAreaSetting.KeyAreaUserId;
                    keyAreaSett.KeyResultAreaDescription = keyAreaSetting.KeyResultAreaDescription;
                    keyAreaSett.AdditionalDescription = keyAreaSetting.AdditionalDescription;
                    keyAreaSett.CreatedBy = UserId;
                    keyAreaSett.CreatedDate = DateTime.UtcNow;
                    keyAreaSett.ModifiedBy = UserId;
                    keyAreaSett.ModifiedDate = DateTime.UtcNow;
                    await this.keyAreaSettingRepository.Add(keyAreaSett);
                    return this.Ok(keyAreaSett);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> Put(int id, [FromBody] APIKeyAreaSetting keyAreaSetting)
        {
            try
            {

                KeyAreaSetting keyareaSetting = await this.keyAreaSettingRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == id);

                //make sure same user should update the record
                if (keyareaSetting.CreatedBy != UserId)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }

                //Description should not be empty
                if (string.IsNullOrEmpty(keyAreaSetting.KeyResultAreaDescription) || string.IsNullOrEmpty(keyAreaSetting.AdditionalDescription))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }

                if (ModelState.IsValid && keyareaSetting != null)
                {
                    keyareaSetting.KeyAreaUserId = keyAreaSetting.KeyAreaUserId;
                    keyareaSetting.KeyResultAreaDescription = keyAreaSetting.KeyResultAreaDescription;
                    keyareaSetting.AdditionalDescription = keyAreaSetting.AdditionalDescription;
                    keyareaSetting.ModifiedBy = UserId;
                    keyareaSetting.ModifiedDate = DateTime.UtcNow;
                    await this.keyAreaSettingRepository.Update(keyareaSetting);
                    return this.Ok(keyareaSetting);
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                KeyAreaSetting keyAreaSetting = await this.keyAreaSettingRepository.Get(r => r.IsDeleted == Record.NotDeleted && r.Id == DecryptedId);
                if (keyAreaSetting == null)
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }

                if (keyAreaSetting.CreatedBy == UserId)
                    keyAreaSetting.IsDeleted = Record.Deleted;
                else
                    return StatusCode(304, "Record cannot be deleted, created by another admin");

                await this.keyAreaSettingRepository.Update(keyAreaSetting);
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("Search/{q}")]
        [PermissionRequired(Permissions.KeyResultAreaSettings)]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<KeyAreaSetting> keyAreaSetting = await this.keyAreaSettingRepository.Search(q);
                return this.Ok(keyAreaSetting);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetMyKeyAreaSetting/{page:int}/{pageSize:int}")]
        public async Task<IActionResult> GetMyKeyAreaSetting(int page, int pageSize)
        {
            try
            {
                IEnumerable<APIKeyAreaSetting> keyAreaSetting = await this.keyAreaSettingRepository.GetMyKeyAreaSetting(UserId, page, pageSize);
                return Ok(keyAreaSetting);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetMyKeyAreaSettingCount")]
        public async Task<IActionResult> GetMyKeyAreaSettingCount()
        {
            try
            {
                int count = await this.keyAreaSettingRepository.GetMyKeyAreaSettingCount(UserId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
