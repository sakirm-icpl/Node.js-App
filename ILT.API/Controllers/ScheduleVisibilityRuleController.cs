using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.Helper;
using ILT.API.Helper.Metadata;
using ILT.API.Model;
using ILT.API.Model.ILT;
using ILT.API.Model.Log_API_Count;
using ILT.API.Repositories.Interfaces;
//using ILT.API.Repositories.Interfaces.ILT;
using ILT.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ILT.API.Common.AuthorizePermissions;
using static ILT.API.Common.TokenPermissions;

namespace ILT.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/i/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ScheduleVisibilityRuleController : IdentityController
    {
        private IScheduleVisibilityRule _scheduleRule;
        private IAccebilityRuleUserGroup _accebilityRuleUserGroup;
        private IUserGroup _userGroup;
        private IAccessibilityRuleRejectedRepository _scheduleRuleRejectedRepository;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicabilityGroupTemplate _applicabilityGroupTemplateRepository;
        private readonly ITokensRepository _tokensRepository;
        ICourseRepository _courseRepository;
        public IConfiguration _configuration { get; set; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleController));
        public ScheduleVisibilityRuleController(IScheduleVisibilityRule scheduleRule,
            IAccebilityRuleUserGroup accebilityRuleUserGroup,
            IUserGroup userGroup,
            IAccessibilityRuleRejectedRepository accessibilityRuleRejectedRepository,
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment,
            IIdentityService identitySvc, IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnectionStringRepository,
            IApplicabilityGroupTemplate applicabilityGroupTemplateRepository,
            ITokensRepository tokensRepository, ICourseRepository courseRepository) : base(identitySvc)
        {
            _scheduleRule = scheduleRule;
            _scheduleRuleRejectedRepository = accessibilityRuleRejectedRepository;
            _httpContextAccessor = httpContextAccessor;
            hostingEnvironment = environment;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
            _applicabilityGroupTemplateRepository = applicabilityGroupTemplateRepository;
            this._tokensRepository = tokensRepository;
            _courseRepository = courseRepository;
            _accebilityRuleUserGroup = accebilityRuleUserGroup;
            _userGroup = userGroup;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _scheduleRule.GetAll());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {

                return Ok(await _scheduleRule.Get(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

       

        [HttpGet("getTotalCourseRecords/{search:minlength(0)?}/{columnName?}")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> GetCourseCount(string search = null)
        {
            try
            {

                int count = await _scheduleRule.CourseCount(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _scheduleRule.Get(page, pageSize, search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getTotalRecords/{search:minlength(0)?}/{columnName?}")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> GetCount(string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _scheduleRule.count(search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("GetSchedules/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> GetSchedules(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _scheduleRule.GetSchedule(page, pageSize, search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetSchedules")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> GetSchedules([FromBody] ApiGetSchedules apiGetSchedules)
        {
            try
            {
                if (apiGetSchedules.search != null)
                    apiGetSchedules.search = apiGetSchedules.search.ToLower().Equals("null") ? null : apiGetSchedules.search;
                if (apiGetSchedules.columnName != null)
                    apiGetSchedules.columnName = apiGetSchedules.columnName.ToLower().Equals("null") ? null : apiGetSchedules.columnName;
                return Ok(await this._scheduleRule.GetSchedules(apiGetSchedules, UserId, RoleCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("getScheduleTotalRecords/{search:minlength(0)?}/{columnName?}")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> Schedulecount(string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _scheduleRule.Schedulecount(search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("GetScheduleRules")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> GetRules([FromBody] APIGetScheduleRulesWithShowAll aPIGetScheduleRulesWithShowAll)
        {
            try
            {
                return Ok(await _scheduleRule.GetAccessibilityRules(aPIGetScheduleRulesWithShowAll.scheduleId, OrganisationCode, Token, aPIGetScheduleRulesWithShowAll.page, aPIGetScheduleRulesWithShowAll.pageSize,UserId,RoleCode, aPIGetScheduleRulesWithShowAll.showAllData));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

       


        [HttpPost("GetScheduleRulesCount")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> GetRulesCount([FromBody] APIGetScheduleRules objAPIGetRules)
        {
            try
            {
                return Ok(await _scheduleRule.GetAccessibilityRulesCount(objAPIGetRules.scheduleId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

    
      
        [HttpPost]
        [Route("PostScheduleRegistration")]
        [Produces("application/json")]
       [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> Post([FromBody] APIScheduleVisibilityRules[] rules)
        {
            try
            {
                _logger.Debug("In Post Multiple" + OrganisationCode);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<APIScheduleVisibilityRules> rejectedRules = new List<APIScheduleVisibilityRules>();
                foreach (APIScheduleVisibilityRules rule in rules)
                {
                    bool isvalid = await _scheduleRule.CheckValidData(rule.AccessibilityParameter1, rule.AccessibilityValue1, rule.AccessibilityParameter2, rule.AccessibilityValue2, rule.ScheduleId, rule.AccessibilityValue11, rule.AccessibilityValue22);
                    if (!isvalid)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                foreach (APIScheduleVisibilityRules rule in rules)
                {
                    APIScheduleVisibility apiAccessibility = ConvertToAPIAccessibility(rule);
                    _logger.Debug("entering Post");
                    List<visibilityRules> result = await _scheduleRule.Post(apiAccessibility, UserId, OrganisationCode, Token);
                    _logger.Debug("out of post");
                    if (result != null)
                        rejectedRules.Add(rule);

                }
                if (rejectedRules != null)


                {

                    _logger.Debug("rejected rule ! = null");
                    return Ok(rejectedRules);
                }
                _logger.Debug("returning ok");
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        private APIScheduleVisibility ConvertToAPIAccessibility(APIScheduleVisibilityRules apiAccessibilityRules)
        {
            APIScheduleVisibility apiAccessibility = new APIScheduleVisibility
            {
                ScheduleId = apiAccessibilityRules.ScheduleId
            };

            if (apiAccessibilityRules.AccessibilityParameter2 == null)
                apiAccessibility.AccessibilityRule = new visibilityRules[1];
            else
                apiAccessibility.AccessibilityRule = new visibilityRules[2];


            if (apiAccessibilityRules.AccessibilityParameter1 != null)
            {
                visibilityRules AccessibilityRules = new visibilityRules
                {
                    AccessibilityRule = apiAccessibilityRules.AccessibilityParameter1,
                    ParameterValue = apiAccessibilityRules.AccessibilityValue1,
                    ParameterValue2 = apiAccessibilityRules.AccessibilityValue11,
                    Condition = apiAccessibilityRules.Condition1 == null ? "null" : apiAccessibilityRules.Condition1
                };
                apiAccessibility.AccessibilityRule[0] = AccessibilityRules;
            }
            if (apiAccessibilityRules.AccessibilityParameter2 != null)
            {
                visibilityRules AccessibilityRules = new visibilityRules
                {
                    AccessibilityRule = apiAccessibilityRules.AccessibilityParameter2,
                    ParameterValue = apiAccessibilityRules.AccessibilityValue2,
                    ParameterValue2 = apiAccessibilityRules.AccessibilityValue22,
                    Condition = apiAccessibilityRules.Condition1
                };
                apiAccessibility.AccessibilityRule[1] = AccessibilityRules;
            }
            return apiAccessibility;
        }


        [HttpPost("GetScheduleRegiLimit_Export")]
        public async Task<IActionResult> GetScheduleRegiLimit_Export([FromBody] APIGetScheduleRules schApplicableUser)
        {
            try
            {
                FileInfo ExcelFile;
                int Id = schApplicableUser.scheduleId;
                APIGetScheduleDetails obj = await this._scheduleRule.GetCourseModuleScheduleNames(Id);
                List<APIScheduleVisibilityRules> accessibilityRules = new List<APIScheduleVisibilityRules>();
                accessibilityRules = await this._scheduleRule.GetAccessibilityRulesForExport(schApplicableUser.scheduleId, OrganisationCode, Token, obj.CourseName);

                List<CourseApplicableUser> UserList = new List<CourseApplicableUser>();
                UserList = await this._scheduleRule.GetCourseApplicableUserList(schApplicableUser.scheduleId);
                ExcelFile = this._scheduleRule.GetApplicableUserListExcel(accessibilityRules, UserList, obj.CourseName,obj.ModuleName,obj.ScheduleCode, OrganisationCode);

                var fs = ExcelFile.OpenRead();
                byte[] fileData = null;
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    fileData = reader.ReadBytes((int)fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, ExcelFile.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [Route("PostRegistrationLimitByTeam")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> PostByApplicabilityTemplate([FromBody] APIScheduleVisibility[] apiAccessibility)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<visibilityRules> resultset = new List<visibilityRules>();
                    List<visibilityRules> postconditionset = new List<visibilityRules>();
                    foreach (APIScheduleVisibility apidataAccessibility in apiAccessibility)
                    {
                        //APIScheduleVisibilityTemplate ApiRule = await _applicabilityGroupTemplateRepository.GetVisibilityTeamTemplate(Convert.ToInt32(apidataAccessibility.UserTeamId), apidataAccessibility.ScheduleId);
                        visibilityRules AccessibilityRule = new visibilityRules();
                        AccessibilityRule.Condition = "and";
                        AccessibilityRule.AccessibilityRule = "aaa";
                        postconditionset.Add(AccessibilityRule);
                        apidataAccessibility.AccessibilityRule = postconditionset.ToArray();
                        List<visibilityRules> result = await _scheduleRule.Post(apidataAccessibility, UserId, OrganisationCode, Token);

                        if (result != null)
                            resultset.AddRange(result);
                    }
                  //  if (resultset == null)
                        return Ok("Success");


                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
          
        }

        [HttpDelete]
        [Route("ScheduleVisibilityDelete")]
        [PermissionRequired(Permissions.Scheduleapplicability)]
        public async Task<IActionResult> DeleteScheduleVisibility([FromQuery] string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await _scheduleRule.DeleteRule(DecryptedId);
                if (Result == 1)
                    return Ok();
                else
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.DataNotAvailable), Description = EnumHelper.GetEnumDescription(MessageType.DataNotAvailable) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }




    }
}
