// ======================================
// <copyright file="AnnouncementsController.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Gadget.API.APIModel;
using Gadget.API.Common;
using Gadget.API.Helper;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Gadget.API.Common.AuthorizePermissions;
using static Gadget.API.Common.TokenPermissions;
using log4net;
using Gadget.API.Helper.Log_API_Count;

namespace Gadget.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class AnnouncementsController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AnnouncementsController));
        private IAnnouncementsRepository announcementsRepository;
        private IMyAnnouncementRepository myAnnouncementRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        public AnnouncementsController(IAnnouncementsRepository announcementsController, IMyAnnouncementRepository myannouncementController, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.announcementsRepository = announcementsController;
            this.myAnnouncementRepository = myannouncementController;
            this._identitySvc = identitySvc;
            this._tokensRepository = tokensRepository;
        }
        // GET: api/<controller>
        [HttpGet]
        [PermissionRequired(Permissions.Announcements)]
        public async Task<IActionResult> Get()
        {
            try
            {

                List<Announcements> announcements = await this.announcementsRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIAnnouncements>>(announcements));
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.Announcements)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {

                IEnumerable<Announcements> announcements = await this.announcementsRepository.GetAllAnnouncements(page, pageSize, search);
                return Ok(Mapper.Map<List<APIAnnouncements>>(announcements));
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllAnnouncementsForEndUser")]
        public async Task<IActionResult> GetAllAnnouncementsForEndUser()
        {
            try
            {

                IEnumerable<Announcements> announcements = await this.announcementsRepository.GetAllAnnouncementsForEndUser();
                return Ok(Mapper.Map<List<APIAnnouncements>>(announcements));
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.Announcements)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int announcements = await this.announcementsRepository.Count(search);
                return Ok(announcements);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                Announcements announcements = await this.announcementsRepository.Get(s => s.IsDeleted == false && s.Id == id);
                return Ok(Mapper.Map<APIAnnouncements>(announcements));

            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
        [HttpGet("GetCount")]
        public async Task<IActionResult> GetTotalCount()
        {
            try
            {
                int announcement = await this.announcementsRepository.GetCount();
                return Ok(announcement);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost]
        [PermissionRequired(Permissions.Announcements)]
        public async Task<IActionResult> Post([FromBody]APIAnnouncements aPIAnnouncements)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    Announcements announcements = new Announcements
                    {
                        Date = aPIAnnouncements.Date,
                        Announcement = aPIAnnouncements.Announcement,
                        FromDate = aPIAnnouncements.FromDate,
                        ToDate = aPIAnnouncements.ToDate,
                        TotalReadCount = aPIAnnouncements.TotalReadCount,
                        IsDeleted = false,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await announcementsRepository.Add(announcements);
                    return Ok(announcements);

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // POST api/<controller>
        [HttpPost("MyAnnouncement")]
        public async Task<IActionResult> PostMyAnnouncement([FromBody]APIMyAnnouncement aPIMyAnnouncement)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    Announcements announcements = await this.announcementsRepository.Get(s => s.IsDeleted == false && s.Id == aPIMyAnnouncement.AnnouncementId);

                    if (announcements == null)
                    {
                        return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                    }
                    else
                    {
                        int totalCount = announcements.TotalReadCount;
                        announcements.TotalReadCount = totalCount + 1;
                        await this.announcementsRepository.Update(announcements);
                    }
                    MyAnnouncement myannouncements = new MyAnnouncement
                    {
                        Date = DateTime.UtcNow,
                        AnnouncementId = aPIMyAnnouncement.AnnouncementId,
                        UserId = UserId,
                        FunctionCode = aPIMyAnnouncement.FunctionCode,
                        IsDeleted = false,
                        ModifiedBy = UserId,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedBy = UserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await myAnnouncementRepository.Add(myannouncements);
                    return Ok(myannouncements);

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // PUT api/<controller>/5
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.Announcements)]
        public async Task<IActionResult> Put(int id, [FromBody]APIAnnouncements aPIAnnouncements)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Announcements announcements = await this.announcementsRepository.Get(s => s.IsDeleted == false && s.Id == id);
                if (announcements == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (announcements != null)
                {
                    announcements.Date = aPIAnnouncements.Date;
                    announcements.Date = aPIAnnouncements.Date;
                    announcements.Announcement = aPIAnnouncements.Announcement;
                    announcements.FromDate = aPIAnnouncements.FromDate;
                    announcements.ToDate = aPIAnnouncements.ToDate;
                    announcements.TotalReadCount = aPIAnnouncements.TotalReadCount;
                    announcements.ModifiedBy = UserId;
                    announcements.ModifiedDate = DateTime.UtcNow;
                    await this.announcementsRepository.Update(announcements);
                }

                return Ok(announcements);
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [PermissionRequired(Permissions.Announcements)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Announcements announcements = await this.announcementsRepository.Get(DecryptedId);

                if (ModelState.IsValid && announcements != null)
                {
                    announcements.IsDeleted = true;
                    await this.announcementsRepository.Update(announcements);
                }

                if (announcements == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        /// <summary>
        /// Search specific Announcements.
        /// </summary>
        [HttpGet]
        [Route("Search/{q}")]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                IEnumerable<Announcements> announcements = await this.announcementsRepository.Search(q);
                return Ok(Mapper.Map<List<APIAnnouncements>>(announcements));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
