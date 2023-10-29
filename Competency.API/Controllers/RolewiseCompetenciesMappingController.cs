using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Competency.API.APIModel.Competency;
using Competency.API.Common;
using Competency.API.Model.Competency;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using Competency.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Competency.API.Common.AuthorizePermissions;
using static Competency.API.Common.TokenPermissions;
using Competency.API.Helper;
using log4net;

namespace Competency.API.Controllers.Competency
{
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [PermissionRequired(Permissions.jobrole)]
    public class RolewiseCompetenciesMappingController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RolewiseCompetenciesMappingController));
        private IRolewiseCompetenciesMappingRepository rolewiseCompetenciesMappingRepository;
        private readonly ITokensRepository _tokensRepository;
        public RolewiseCompetenciesMappingController(IRolewiseCompetenciesMappingRepository rolewiseCompetenciesMappingController, IIdentityService _identitySvc, ITokensRepository tokensRepository) : base(_identitySvc)
        {
            this.rolewiseCompetenciesMappingRepository = rolewiseCompetenciesMappingController;
            this._tokensRepository = tokensRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var competenciesMapping = await this.rolewiseCompetenciesMappingRepository.GetAllCompetenciesMapping();
                return Ok(Mapper.Map<List<APIRolewiseCompetenciesMapping>>(competenciesMapping));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                var CompetencyLevels = await this.rolewiseCompetenciesMappingRepository.GetAllCompetenciesMapping(page, pageSize, search);
                return Ok(Mapper.Map<List<APIRolewiseCompetenciesMapping>>(CompetencyLevels));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetTotalRecords/{search:minlength(0)?}")]
        public async Task<IActionResult> GetCount(string search)
        {
            try
            {
                var Count = await this.rolewiseCompetenciesMappingRepository.Count(search);
                return this.Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var competenciesMapping = await this.rolewiseCompetenciesMappingRepository.GetAllCompetenciesMapping(id);
                return Ok(Mapper.Map<List<APIRolewiseCompetenciesMapping>>(competenciesMapping));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetRecordByRoleId/{roleid:int}")]
        public async Task<IActionResult> GetRecordByRoleId(int roleid)
        {
            try
            {
                var competenciesMapping = await this.rolewiseCompetenciesMappingRepository.GetAllByRoleCompetenciesMapping(roleid);
                return Ok(Mapper.Map<List<APIRolewiseCompetenciesMapping>>(competenciesMapping));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APIRoleCompetenciesMappingMerge aPIRoleCompetenciesMappingMerge)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }
                else
                {
                    foreach (RoleCompetenciesMappingRecord roleCompetenciesMappingRecord in aPIRoleCompetenciesMappingMerge.rolecompetenciesMappingRecord)
                    {
                        if (await this.rolewiseCompetenciesMappingRepository.Exists(aPIRoleCompetenciesMappingMerge.RoleId, roleCompetenciesMappingRecord.CompetencyCategoryId, roleCompetenciesMappingRecord.CompetencyId))
                        {
                        }
                        else
                        {
                            RolewiseCompetenciesMapping rolewiseCompetenciesMapping = new RolewiseCompetenciesMapping();
                            rolewiseCompetenciesMapping.CompetencyCategoryId = roleCompetenciesMappingRecord.CompetencyCategoryId;
                            rolewiseCompetenciesMapping.RoleId = aPIRoleCompetenciesMappingMerge.RoleId;
                            rolewiseCompetenciesMapping.RoleName = aPIRoleCompetenciesMappingMerge.RoleName;
                            rolewiseCompetenciesMapping.CompetencyId = roleCompetenciesMappingRecord.CompetencyId;
                            rolewiseCompetenciesMapping.IsDeleted = false;
                            rolewiseCompetenciesMapping.ModifiedBy = 1;
                            rolewiseCompetenciesMapping.ModifiedDate = DateTime.UtcNow;
                            rolewiseCompetenciesMapping.CreatedBy = 1;
                            rolewiseCompetenciesMapping.CreatedDate = DateTime.UtcNow;
                            await this.rolewiseCompetenciesMappingRepository.AddRecord(rolewiseCompetenciesMapping);
                        }
                    }

                    return Ok(aPIRoleCompetenciesMappingMerge);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] APIRoleCompetenciesMappingMerge aPIRoleCompetenciesMappingMerge)
        {
            try
            {


                if (ModelState.IsValid)
                {

                    var Count = await this.rolewiseCompetenciesMappingRepository.CountRole(id);

                    int countRecord = Count;
                    for (int i = 0; i < countRecord; i++)
                    {
                        RolewiseCompetenciesMapping account = await this.rolewiseCompetenciesMappingRepository.GetRecordRole(id);

                        if (ModelState.IsValid && account != null)
                        {
                            await this.rolewiseCompetenciesMappingRepository.Remove(account);
                        }

                    }

                    foreach (RoleCompetenciesMappingRecord roleCompetenciesMappingRecord in aPIRoleCompetenciesMappingMerge.rolecompetenciesMappingRecord)
                    {
                        if (await this.rolewiseCompetenciesMappingRepository.Exists(aPIRoleCompetenciesMappingMerge.RoleId, roleCompetenciesMappingRecord.CompetencyCategoryId, roleCompetenciesMappingRecord.CompetencyId))
                        {
                        }
                        else
                        {
                            RolewiseCompetenciesMapping rolewiseCompetenciesMapping = new RolewiseCompetenciesMapping();
                            rolewiseCompetenciesMapping.CompetencyCategoryId = roleCompetenciesMappingRecord.CompetencyCategoryId;
                            rolewiseCompetenciesMapping.RoleId = aPIRoleCompetenciesMappingMerge.RoleId;
                            rolewiseCompetenciesMapping.RoleName = aPIRoleCompetenciesMappingMerge.RoleName;
                            rolewiseCompetenciesMapping.CompetencyId = roleCompetenciesMappingRecord.CompetencyId;
                            rolewiseCompetenciesMapping.IsDeleted = false;
                            rolewiseCompetenciesMapping.ModifiedBy = 1;
                            rolewiseCompetenciesMapping.ModifiedDate = DateTime.UtcNow;
                            rolewiseCompetenciesMapping.CreatedBy = 1;
                            rolewiseCompetenciesMapping.CreatedDate = DateTime.UtcNow;
                            await this.rolewiseCompetenciesMappingRepository.AddRecord(rolewiseCompetenciesMapping);
                        }
                    }
                    return this.Ok(aPIRoleCompetenciesMappingMerge);
                }

                else
                    return BadRequest(ModelState);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                RolewiseCompetenciesMapping rolewiseCompetenciesMapping = await this.rolewiseCompetenciesMappingRepository.Get(DecryptedId);

                if (ModelState.IsValid && rolewiseCompetenciesMapping != null)
                {
                    rolewiseCompetenciesMapping.IsDeleted = true;
                    await this.rolewiseCompetenciesMappingRepository.Update(rolewiseCompetenciesMapping);
                }

                if (rolewiseCompetenciesMapping == null)
                    return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Fail), Description = EnumHelper.GetEnumDescription(MessageType.Fail) });
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }
    }
}
