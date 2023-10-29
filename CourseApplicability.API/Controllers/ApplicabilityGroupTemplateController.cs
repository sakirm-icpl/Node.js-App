using AspNet.Security.OAuth.Introspection;
using Courses.API.APIModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using CourseApplicability.API.Model.Log_API_Count;
using static CourseApplicability.API.Common.TokenPermissions;
using CourseApplicability.API.Controllers;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Services;
using CourseApplicability.API.Common;
using CourseApplicability.API.Helper;
using CourseApplicability.API.Model;
using CourseApplicability.API.APIModel;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/ApplicabilityGroupTemplate")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    // [PermissionRequired(Permissions.courseapplicability+" "+Permissions.SurveyManagement+" "+Permissions.SurveyQuestionManagement)]
    public class ApplicabilityGroupTemplateController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ApplicabilityGroupTemplateController));
        private readonly IApplicabilityGroupTemplate _applicabilityGroupTemplate;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration { get; set; }
        private readonly IWebHostEnvironment hostingEnvironment;
        public ApplicabilityGroupTemplateController(IApplicabilityGroupTemplate applicabilityGroupTemplate,
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment,
            IIdentityService identitySvc, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionStringRepository, ITokensRepository tokensRepository) : base(identitySvc)
        {
            _applicabilityGroupTemplate = applicabilityGroupTemplate;
            _httpContextAccessor = httpContextAccessor;
            hostingEnvironment = environment;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
            this._tokensRepository = tokensRepository;
        }

        [HttpGet("GetCount/{search:minlength(0)?}/{columnName?}")]
        public async Task<IActionResult> GetCount(string search = null, string columnName = null)
        {
            try
            {
                int count = await _applicabilityGroupTemplate.Count(search, columnName);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetAllGroupTemplate/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        public async Task<IActionResult> GetAllGroupTemplate(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _applicabilityGroupTemplate.GetAllGroupTemplate(page, pageSize, search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("GetApplicabilityGroupTemplateDetails/{TemplateId:int}/")]
        public async Task<IActionResult> GetApplicabilityGroupTemplateDetails(int TemplateId)
        {
            try
            {
                return Ok(await _applicabilityGroupTemplate.GetApplicabilityGroupTemplateDetails(TemplateId, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));

                ApplicabilityGroupTemplate rule = await this._applicabilityGroupTemplate.Get(DecryptedId);
                if (rule == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                if (await _applicabilityGroupTemplate.TemplateDependancy(DecryptedId))

                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DependancyExist), Description = EnumHelper.GetEnumDescription(MessageType.DependancyExist) });
                }
                rule.IsDeleted = true;

                await this._applicabilityGroupTemplate.Update(rule);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] APIApplicabilityGroupTemplate applicabilityGroupTemplate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var str = (from item in applicabilityGroupTemplate.ApplicabilityGroupRule
                               select item
                          ).Where(s => s.ApplicabilityRule != null && s.ParameterValue != null && s.Condition != null).ToArray();

                    applicabilityGroupTemplate.ApplicabilityGroupRule = (ApplicabilityGroupRules[])str;

                    Message ValidationMessage = ValidateRules(applicabilityGroupTemplate.ApplicabilityGroupRule);
                    if (ValidationMessage == Message.InvalidModel)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ErrorMessage.ModelError });
                    }
                    else if (ValidationMessage == Message.SameData)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ErrorMessage.SameData });
                    }
                    List<ApplicabilityGroupRules> result = await this._applicabilityGroupTemplate.Post(applicabilityGroupTemplate, UserId);

                    if (result == null)
                        return Ok("Success");

                    if (result.Count > 0)
                    {
                        int index = 0;
                        foreach (var item in result)
                        {
                            applicabilityGroupTemplate.ApplicabilityGroupRule[index].ApplicabilityRule = item.ApplicabilityRule;
                            applicabilityGroupTemplate.ApplicabilityGroupRule[index].ParameterValue = item.ParameterValue;
                        }
                    }
                    return BadRequest(applicabilityGroupTemplate);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        private Message ValidateRules(ApplicabilityGroupRules[] applicabilityGroupRules)
        {
            foreach (var applicabilityGroupRule in applicabilityGroupRules)
            {
                if (applicabilityGroupRule.ApplicabilityRule == null || applicabilityGroupRule.ParameterValue == null)
                    return Message.InvalidModel;
            }
            string json = JsonConvert.SerializeObject(applicabilityGroupRules);
            JArray jsonArray = JArray.Parse(json);

            var duplicate = (from item in applicabilityGroupRules
                             select item.ApplicabilityRule
                         ).GroupBy(s => s).Select(group => new { Word = group.Key, Count = group.Count() }).Where(x => x.Count >= 2);
            if (duplicate.Count() > 0)
            {
                return Message.SameData;
            }

            return Message.Ok;
        }
        [HttpGet("GetAllGroupTemplate")]
        public async Task<IActionResult> GetAllGroupTemplate()
        {
            try
            {
                return Ok(await _applicabilityGroupTemplate.GetAllGroupTemplate());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
    }
}
