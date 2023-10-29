// ======================================
// <copyright file="ThoughtForDayController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Publication.API.APIModel;
using Publication.API.Common;
using Publication.API.Helper;
using Publication.API.Models;
using Publication.API.Repositories.Interfaces;
using Publication.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Publication.API.Common.AuthorizePermissions;
using static Publication.API.Common.TokenPermissions;
using log4net;
using Publication.API.Helper.Log_API_Count;

namespace Publication.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ThoughtForDayController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ThoughtForDayController));
        private IThoughtForDayRepository thoughtForDayRepository;
        private IThoughtForDayCounterRepository thoughtForDayCounterRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        public ThoughtForDayController(IThoughtForDayRepository thoughtForDayController, IThoughtForDayCounterRepository thoughtForDayCounterController, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.thoughtForDayRepository = thoughtForDayController;
            this.thoughtForDayCounterRepository = thoughtForDayCounterController;
            this._identitySvc = identitySvc;
            this._tokensRepository = tokensRepository;
        }

        // GET: api/<controller>
        [HttpGet]
        [PermissionRequired(Permissions.Thoughtfortheday)]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<ThoughtForDay> thoughtForDay = await this.thoughtForDayRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIThoughtForDay>>(thoughtForDay));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
        }
            [HttpGet("ThoughtForDayCounter")]
            
            public async Task<IActionResult> GetCounter()
            {
                try
                {
                    List<ThoughtForDayCounter> thoughtForDayCounter = await this.thoughtForDayCounterRepository.GetAll(s => s.IsDeleted == false);
                    return Ok(Mapper.Map<List<APIThoughtForDayCounter>>(thoughtForDayCounter));
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
            [HttpGet("{page:int}/{pageSize:int}/{search?}")]
            [PermissionRequired(Permissions.Thoughtfortheday)]
            public async Task<IActionResult> Get(int page, int pageSize, string search = null)
            {
                try
                {
                    IEnumerable<ThoughtForDay> thoughtForDay = await this.thoughtForDayRepository.GetAllThoughtForDay(page, pageSize, search);
                    return Ok(Mapper.Map<List<APIThoughtForDay>>(thoughtForDay));
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }

            [HttpGet("ThoughtForDayCounter/{page:int}/{pageSize:int}/{search?}")]
            public async Task<IActionResult> GetCounter(int page, int pageSize, string search = null)
            {
                try
                {
                    IEnumerable<ThoughtForDayCounter> thoughtForDayCounter = await this.thoughtForDayCounterRepository.GetAllThoughtForDayCounter(page, pageSize, search);
                    return Ok(Mapper.Map<List<APIThoughtForDayCounter>>(thoughtForDayCounter));
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }

            [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
            //[PermissionRequired(Permissions.Thoughtfortheday)]
            public async Task<IActionResult> GetCount(string search)
            {
                try
                {
                    int thoughtForDay = await this.thoughtForDayRepository.Count(search);
                    return Ok(thoughtForDay);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }

            [HttpGet("GetTotalRecordsThoughtForDate/{search:minlength(0)?}")]
            public async Task<IActionResult> GetCountThoughtForDate(string search)
            {
                try
                {
                    int thoughtForDay = await this.thoughtForDayRepository.CountDate(search);
                    return Ok(thoughtForDay);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }
            [HttpGet("GetThoughtForDate/{search:minlength(0)?}")]
            public async Task<IActionResult> GetThoughtForDate(string search)
            {
                try
                {
                    ThoughtForDay thoughtForDay = await this.thoughtForDayRepository.GetThoughtForDate(search);
                    return Ok(thoughtForDay);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }

            }
            // GET api/<controller>/5
            [HttpGet("{id}")]
            [PermissionRequired(Permissions.Thoughtfortheday)]
            public async Task<IActionResult> Get(int id)
            {
                try
                {
                    ThoughtForDay thoughtForDay = await this.thoughtForDayRepository.Get(s => s.IsDeleted == false && s.Id == id);
                    return Ok(Mapper.Map<APIThoughtForDay>(thoughtForDay));
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }

            [HttpGet("Exists/{search}")]
            public async Task<bool> Exists(string search)
            {
                try
                {
                    return await this.thoughtForDayRepository.Exist(search);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    throw ex;
                }
            }

            // POST api/<controller>
            [HttpPost]
            [PermissionRequired(Permissions.Thoughtfortheday)]
            public async Task<IActionResult> Post([FromBody] APIThoughtForDay aPIThoughtForDay)
            {

                try
                {

                    if (ModelState.IsValid)
                    {

                        bool exit = await this.thoughtForDayRepository.Exists(aPIThoughtForDay.ForDate);
                        if (exit == true)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                        }


                        ThoughtForDay thoughtForDay = new ThoughtForDay
                        {
                            Date = DateTime.UtcNow,
                            Thought = aPIThoughtForDay.Thought,
                            ForDate = aPIThoughtForDay.ForDate,
                            TotalLikesForDate = aPIThoughtForDay.TotalLikesForDate,
                            TotalDeslikesForDate = aPIThoughtForDay.TotalDeslikesForDate,
                            TotalLikes = aPIThoughtForDay.TotalLikes,
                            IsDeleted = false,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow
                        };
                        await thoughtForDayRepository.Add(thoughtForDay);
                        return Ok(thoughtForDay);

                    }
                    else
                        return this.BadRequest(this.ModelState);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }

        // POST api/<controller>
        [HttpPost("ThoughtForDayCounter")]
        public async Task<IActionResult> PostCounter([FromBody] APIThoughtForDayCounter aPIThoughtForDayCounter)
        {
            {
                try
                {
                    bool exit = await this.thoughtForDayRepository.Exist(UserId, aPIThoughtForDayCounter.ThoughtForDayId);

                    ThoughtForDay thoughtForDay = await this.thoughtForDayRepository.Get(s => s.IsDeleted == false && s.Id == aPIThoughtForDayCounter.ThoughtForDayId);
                    if (ModelState.IsValid)
                    {
                        ThoughtForDayCounter thoughtForDayCounter = new ThoughtForDayCounter
                        {
                            Date = DateTime.UtcNow,
                            UserAction = aPIThoughtForDayCounter.UserAction,
                            UserId = UserId,
                            ThoughtForDayId = aPIThoughtForDayCounter.ThoughtForDayId,
                            IsDeleted = false,
                            ModifiedBy = UserId,
                            ModifiedDate = DateTime.UtcNow,
                            CreatedBy = UserId,
                            CreatedDate = DateTime.UtcNow
                        };
                        if (exit == false)
                        {
                            await thoughtForDayCounterRepository.Add(thoughtForDayCounter);
                            if (aPIThoughtForDayCounter.UserAction == true)
                            {
                                thoughtForDay.TotalLikesForDate = thoughtForDay.TotalLikesForDate + 1;
                            }
                            else
                            {
                                thoughtForDay.TotalDeslikesForDate = thoughtForDay.TotalDeslikesForDate + 1;
                            }
                            await this.thoughtForDayRepository.Update(thoughtForDay);
                            int referenceId = thoughtForDayCounter.Id;
                            if (referenceId != 0)
                            {
                                string functionCode = "TLS0750";
                                string category = "Normal";
                                int point = 1;
                                int userId = UserId;
                                await this.thoughtForDayCounterRepository.RewardPointSave(functionCode, category, referenceId, point, userId);
                            }
                        }

                        return Ok(thoughtForDayCounter);

                    }
                    else
                        return this.BadRequest(this.ModelState);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                }
            }

            // PUT api/<controller>/5
        }    
            [HttpPost("{id}")]
                [PermissionRequired(Permissions.Thoughtfortheday)]
                public async Task<IActionResult> Put(int id, [FromBody] APIThoughtForDay aPIThoughtForDay)
                {

                    try
                    {
                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }
                        ThoughtForDay thoughtForDay = await this.thoughtForDayRepository.Get(s => s.IsDeleted == false && s.Id == id);

                        if (thoughtForDay == null)
                        {
                            return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                        }
                        if (thoughtForDay != null)
                        {
                            //if (thoughtForDay.ForDate != aPIThoughtForDay.ForDate)
                            //{
                            //    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                            //}

                            thoughtForDay.Date = aPIThoughtForDay.Date;
                            thoughtForDay.Thought = aPIThoughtForDay.Thought;
                            thoughtForDay.ForDate = aPIThoughtForDay.ForDate;
                            thoughtForDay.TotalLikesForDate = 0;
                            thoughtForDay.TotalDeslikesForDate = 0;
                            thoughtForDay.TotalLikes = 0;
                            thoughtForDay.ModifiedBy = UserId;
                            thoughtForDay.ModifiedDate = DateTime.UtcNow;
                            await this.thoughtForDayRepository.Update(thoughtForDay);
                        }

                        return Ok(thoughtForDay);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                    }
                }

                // DELETE api/<controller>/5
                [HttpDelete]
                [PermissionRequired(Permissions.Thoughtfortheday)]
                public async Task<IActionResult> Delete([FromQuery]string id)
                {
                    try
                    {
                        int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                        ThoughtForDay thoughtForDay = await this.thoughtForDayRepository.Get(DecryptedId);

                        if (ModelState.IsValid && thoughtForDay != null)
                        {
                            thoughtForDay.IsDeleted = true;
                            await this.thoughtForDayRepository.Update(thoughtForDay);
                        }

                        if (thoughtForDay == null)
                            return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                        return this.Ok();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                    }


                }

                /// <summary>
                /// Search specific SupportManagement.
                /// </summary>
                [HttpGet]
                [Route("Search/{q}")]
                public async Task<IActionResult> Search(string q)
                {
                    try
                    {
                        IEnumerable<ThoughtForDay> thoughtForDay = await this.thoughtForDayRepository.Search(q);
                        return Ok(Mapper.Map<List<APIThoughtForDay>>(thoughtForDay));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
                    }
                }

            }
        }
