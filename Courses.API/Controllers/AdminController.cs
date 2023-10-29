using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Model;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.TokenPermissions;
using Courses.API.Helper;
using log4net;
using Courses.API.Model.Log_API_Count;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/Admin")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class AdminController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdminController));
        IAuthoringMaster _authoringMaster;
        private readonly ITokensRepository _tokensRepository;
        public AdminController(IIdentityService identitySvc, IAuthoringMaster authoringMaster, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this._authoringMaster = authoringMaster;
            this._tokensRepository = tokensRepository;
        }


        [HttpPost]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Post([FromBody] ApiAuthoringMaster apiAuthoringMaster)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else if (await this._authoringMaster.Exists(apiAuthoringMaster.Name, apiAuthoringMaster.Skills))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                else
                {
                    ApiResponse reso = await this._authoringMaster.PostAuthoringDetailsToLcms(apiAuthoringMaster, UserId);
                    if (reso.StatusCode == 200)
                    {
                        AuthoringMaster objAuthoringMaster = new AuthoringMaster();
                        objAuthoringMaster = Mapper.Map<AuthoringMaster>(apiAuthoringMaster);
                        objAuthoringMaster.CreatedBy = UserId;
                        objAuthoringMaster.CreatedDate = DateTime.UtcNow;
                        objAuthoringMaster.IsDeleted = false;
                        objAuthoringMaster.IsActive = true;
                        objAuthoringMaster.LCMSId = Convert.ToInt32(reso.ResponseObject);

                        await _authoringMaster.Add(objAuthoringMaster);

                        int id = objAuthoringMaster.Id;
                        return Ok(id);
                    }

                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost]
        [Route("AuthoringMasterDetails")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Post([FromBody] ApiAuthoringMasterDetails apiAuthoringMasterDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                ApiAuthoringMasterDetails errorAssessment = await this._authoringMaster.PostAuthoringDetails(apiAuthoringMasterDetails, UserId);
                if (errorAssessment == null)
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                else
                    return Ok(errorAssessment);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("{id}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Put(int id, [FromBody] ApiAuthoringMaster apiAuthoringMaster)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                AuthoringMaster objAuthoringMaster = await this._authoringMaster.Get(id);
                if (objAuthoringMaster == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                else if (await this._authoringMaster.Exists(apiAuthoringMaster.Name, apiAuthoringMaster.Skills, objAuthoringMaster.Id))
                {
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Duplicate), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                }
                else
                {

                    objAuthoringMaster.IsDeleted = apiAuthoringMaster.IsDeleted;
                    objAuthoringMaster.Name = apiAuthoringMaster.Name;
                    objAuthoringMaster.Skills = apiAuthoringMaster.Skills;
                    objAuthoringMaster.Description = apiAuthoringMaster.Description;
                    objAuthoringMaster.ModifiedBy = UserId;
                    objAuthoringMaster.ModifiedDate = DateTime.UtcNow;
                    await _authoringMaster.Update(objAuthoringMaster);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> GetAll(int page, int pageSize, string search = null)
        {
            try
            {
                return Ok(await _authoringMaster.GetAll(page, pageSize, search));

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetDetailsByAuthoringId/{id}/{page:int}/{pageSize:int}/{courseId:int?}/{search?}")]
        public async Task<IActionResult> GetDetailsByAuthoringId(int id, int page, int pageSize, int courseId = 0, string search = null)
        {
            try
            {
                return Ok(await _authoringMaster.GetDetailsByAuthoringId(id, page, pageSize, UserId, courseId, search));

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpGet("GetPageNumber/{id}")]
        public async Task<IActionResult> GetPageNumber(int id)
        {
            try
            {
                return Ok(await _authoringMaster.GetPageNumber(id));


            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetAuthoringMasterIdByModuleID/{ModuleId}")]
        public async Task<IActionResult> GetAuthoringMasterIdByModuleID(int ModuleId)
        {
            try
            {
                return Ok(await _authoringMaster.GetAuthoringMasterIdByModuleID(ModuleId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("count/{search?}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> GetCount(string search = null)
        {
            try
            {
                return Ok(await _authoringMaster.Count(search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                AuthoringMaster AuthoringMaster = await _authoringMaster.Get(DecryptedId);
                if (await _authoringMaster.IsDependacyExist(DecryptedId))
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                else if (AuthoringMaster == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }

                AuthoringMaster.IsDeleted = true;
                AuthoringMaster.ModifiedBy = this.UserId;
                AuthoringMaster.ModifiedDate = DateTime.UtcNow;
                await _authoringMaster.Update(AuthoringMaster);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete("AuthoringMasterDetails")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> DeleteAuthoringMasterDetails([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                AuthoringMasterDetails objAuthoringMasterDetails = await _authoringMaster.GetAuthoringMasterDetails(DecryptedId);


                if (objAuthoringMasterDetails != null)
                {
                    return Ok(await _authoringMaster.DeleteAuthoringMasterDetails(objAuthoringMasterDetails));
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }


        [HttpPost("AuthoringDetails/{AuthoringDetailsId}")]
        [PermissionRequired(Permissions.authoring)]
        public async Task<IActionResult> PutAuthoringDetailsId(int AuthoringDetailsId, [FromBody] ApiAuthoringMasterDetailsUpdate apiAuthoringMasterDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                AuthoringMasterDetails objAuthoringMasterDetailsOld = await _authoringMaster.GetAuthoringMasterDetails(AuthoringDetailsId);

                if (objAuthoringMasterDetailsOld != null)
                {
                    return Ok(await _authoringMaster.UpdateAuthoringMasterDetails(objAuthoringMasterDetailsOld, apiAuthoringMasterDetails, UserId));
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    }
}