using AspNet.Security.OAuth.Introspection;
using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using Assessment.API.Common;
using Assessment.API.Controllers;
using Assessment.API.Helper;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Assessment.API.Common.AuthorizePermissions;
using static Assessment.API.Common.TokenPermissions;
using log4net;
using Assessment.API.Model.Log_API_Count;
using Assessment.API.Common;
using Assessment.API.Helper;
using Assessment.API.Model.Log_API_Count;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Services;
using static Assessment.API.Common.AuthorizePermissions;
using static Assessment.API.Common.TokenPermissions;

namespace Assessment.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    public class AssessmentConfigurationController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AssessmentConfigurationController));
        private IAssessmentConfiguration _assessmentConfiguration;
        private readonly IIdentityService _identitySvc;
        private readonly ITokensRepository _tokensRepository;
        public AssessmentConfigurationController(IAssessmentConfiguration assessmentCon, IIdentityService identitySvc, ITokensRepository tokensRepository) : base(identitySvc)
        {
            this._assessmentConfiguration = assessmentCon;
            this._identitySvc = identitySvc;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IEnumerable<AssessmentConfiguration>> Get()
        {
            try
            {
                return await this._assessmentConfiguration.GetAll(s => s.IsDeleted == false);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }


        [HttpGet("{page:int}/{pageSize:int}/{search?}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null)
        {
            try
            {
                IEnumerable<AssessmentConfiguration> assessmentCon = await this._assessmentConfiguration.GetAllAssessmentConfiguration(page, pageSize, search);
                return Ok(assessmentCon);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("{id}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<AssessmentConfiguration> Get(int id)
        {
            try
            {
                return await this._assessmentConfiguration.Get(s => s.IsDeleted == false && s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        [HttpPost]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Post([FromBody] APIAssessmentConfigurationParameters aPIAssessmentConfigurationParameters)
        {

            try
            {
                AssessmentConfiguration ObjAssessment = new AssessmentConfiguration();
                //get userid from token
                string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                if (identity == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }

                int tokenId = Convert.ToInt32(identity);

                if (ModelState.IsValid)
                {
                    ObjAssessment.Attribute = aPIAssessmentConfigurationParameters.Attribute;
                    ObjAssessment.Value = aPIAssessmentConfigurationParameters.Value;
                    ObjAssessment.Code = aPIAssessmentConfigurationParameters.Code;
                    ObjAssessment.CreatedDate = DateTime.UtcNow;
                    ObjAssessment.IsDeleted = false;
                    ObjAssessment.CreatedBy = tokenId;
                    await _assessmentConfiguration.Add(ObjAssessment);
                    return Ok(ObjAssessment);

                }
                else
                    return this.BadRequest(this.ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpPost("{id}")]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Put(int id, [FromBody] List<APIAssessmentConfigurationParameters> assessmentConfigurationParameter)
        {

            try
            {
                //get userid from token
                string identity = Security.Decrypt(_identitySvc.GetUserIdentity());
                if (identity == null)
                {
                    return StatusCode(401, Record.InvalidUserID);
                }

                int tokenId = Convert.ToInt32(identity);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    foreach (APIAssessmentConfigurationParameters aPIAssessmentConfigurationParameters in assessmentConfigurationParameter)
                    {
                        AssessmentConfiguration assessmentConfiguration = await this._assessmentConfiguration.Get(aPIAssessmentConfigurationParameters.Id.Value);

                        assessmentConfiguration.ModifiedDate = DateTime.UtcNow;
                        assessmentConfiguration.Attribute = aPIAssessmentConfigurationParameters.Attribute;
                        assessmentConfiguration.Value = aPIAssessmentConfigurationParameters.Value;
                        assessmentConfiguration.ModifiedBy = tokenId;
                        await this._assessmentConfiguration.Update(assessmentConfiguration);

                    }
                    return this.Ok(assessmentConfigurationParameter);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { StatusCode = 500, Message = "Error", Description = "Exception Occurs" + ex.Message });
            }
        }

        [HttpDelete]
        [PermissionRequired(Permissions.lcmsmanage)]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                AssessmentConfiguration assessmentConfi = await this._assessmentConfiguration.Get(DecryptedId);

                if (ModelState.IsValid && assessmentConfi != null)
                {
                    assessmentConfi.IsDeleted = true;
                    await this._assessmentConfiguration.Update(assessmentConfi);
                }

                if (assessmentConfi == null)
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