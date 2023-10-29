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
using Exception = System.Exception;
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
    public class OrganizationMessagesController : IdentityController
    {
        
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OrganizationMessagesController));
        private IOrganizationMessagesRepository organizationmessagesRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IIdentityService _identitySvc;

        public OrganizationMessagesController(IOrganizationMessagesRepository organizationmessagesController, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this.organizationmessagesRepository = organizationmessagesController;
            this._tokensRepository = tokensRepository;
            this._identitySvc = identitySvc;

        }
        [HttpGet]
        [PermissionRequired(Permissions.OrganizationMessages)]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<OrganizationMessages> announcements = await this.organizationmessagesRepository.GetAll(s => s.IsDeleted == false);
                return Ok(Mapper.Map<List<APIOrganizationMessages>>(announcements));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.OrganizationMessages)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                List<OrganizationMessages> organizationmessages = await this.organizationmessagesRepository.GetAllOrganizationMessages(page, pageSize, search);
                return Ok(Mapper.Map<List<OrganizationMessages>>(organizationmessages));
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        [PermissionRequired(Permissions.OrganizationMessages)]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                int organizationmessages = await this.organizationmessagesRepository.CountOrganizationMessages(search);
                return Ok(organizationmessages);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("{id}")]
        [PermissionRequired(Permissions.OrganizationMessages)]
        public async Task<IActionResult> Put(int id, [FromBody]APIOrganizationMessages aPIOrganizationMessages)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Models.OrganizationMessages organizationmessages = await this.organizationmessagesRepository.Get(s => s.IsDeleted == false && s.Id == id);
                if (organizationmessages == null)
                {
                    return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                if (organizationmessages != null)
                {
                    organizationmessages.MessageHeading = aPIOrganizationMessages.MessageHeading;
                    organizationmessages.MessageDescription = aPIOrganizationMessages.MessageDescription;
                    organizationmessages.MessageFrom = aPIOrganizationMessages.MessageFrom;
                    organizationmessages.ProfilePicture = aPIOrganizationMessages.ProfilePicture;
                    organizationmessages.Status = aPIOrganizationMessages.Status;
                    organizationmessages.ModifiedBy = UserId;
                    organizationmessages.ModifiedDate = DateTime.UtcNow;
                    organizationmessages.CreatedBy = UserId;
                    organizationmessages.CreatedDate = DateTime.UtcNow;
                    organizationmessages.ShowToAll = aPIOrganizationMessages.ShowToAll;
                    await this.organizationmessagesRepository.Update(organizationmessages);
                }
                return Ok(organizationmessages);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost]
        [PermissionRequired(Permissions.OrganizationMessages)]
        public async Task<IActionResult> Post([FromBody]APIOrganizationMessages aPIOrganizationMessages)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    bool result = await this.organizationmessagesRepository.ExistOrganizationMessage(aPIOrganizationMessages.MessageDescription);
                    if (result)
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                    OrganizationMessages organizationmessages = new OrganizationMessages
                    {
                        MessageDescription = aPIOrganizationMessages.MessageDescription,
                        MessageHeading = aPIOrganizationMessages.MessageHeading,
                        MessageFrom = aPIOrganizationMessages.MessageFrom,
                        ProfilePicture = aPIOrganizationMessages.ProfilePicture,
                        IsDeleted = false,
                        ModifiedBy = UserId,
                      
                    };
                    organizationmessages.Status = aPIOrganizationMessages.Status;
                    organizationmessages.ModifiedDate = DateTime.UtcNow;
                    organizationmessages.ShowToAll = aPIOrganizationMessages.ShowToAll;
                    organizationmessages.CreatedBy = UserId;
                    organizationmessages.CreatedDate = DateTime.UtcNow;
                    await organizationmessagesRepository.Add(organizationmessages);
                    return Ok(organizationmessages);

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpDelete]
        [PermissionRequired(Permissions.OrganizationMessages)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                OrganizationMessages organizationmessages = await this.organizationmessagesRepository.Get(DecryptedId);

                if (ModelState.IsValid && organizationmessages != null)
                {

                    organizationmessages.Status = false;
                    organizationmessages.IsDeleted = true;
                    await this.organizationmessagesRepository.Update(organizationmessages);
                }

                if (organizationmessages == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

    }
}