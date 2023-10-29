using AspNet.Security.OAuth.Introspection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Helper;
using User.API.Helper.Log_API_Count;
using User.API.Models;
using User.API.Repositories.Interfaces;
using User.API.Services;
using static User.API.Common.TokenPermissions;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]

    public class DesignationRoleController : IdentityController
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(DesignationRoleController));
        private IDesignationRoleRepository _designationRoleRepository;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;

        public DesignationRoleController(IIdentityService identityService, ITokensRepository tokensRepository, IDesignationRoleRepository designationRoleRepository) : base(identityService)
        {
            this._designationRoleRepository = designationRoleRepository;
            this._identitySvc = identityService;
            this._tokensRepository = tokensRepository;

        }
        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IEnumerable<DesignationRoleMapping>> Get(int page, int pageSize, string search = null)
        {
            try
            {
                return await this._designationRoleRepository.GetAllDesignations(page, pageSize, search);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]

        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this._designationRoleRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APIDesignationRoleMapping aPIDesignation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DesignationRoleMapping designationrole = new DesignationRoleMapping();

                    if (aPIDesignation != null)
                    {

                        designationrole.UserRole = aPIDesignation.UserRole;
                        designationrole.Designation = aPIDesignation.Designation;

                        await _designationRoleRepository.Add(designationrole);
                        return Ok(aPIDesignation);
                    }
                    else
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost("{id}")]

        public async Task<IActionResult> Put(int id, [FromBody] APIDesignationRoleMapping aPIDesignationRole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DesignationRoleMapping designationrole = await this._designationRoleRepository.Get(id);
                    if (designationrole == null)
                    {
                        return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                    }
                    designationrole.UserRole = aPIDesignationRole.UserRole;
                    designationrole.Designation = aPIDesignationRole.Designation;

                    await this._designationRoleRepository.Update(designationrole);
                    return Ok(aPIDesignationRole);
                }
                return this.Ok(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}