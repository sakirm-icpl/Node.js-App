using AspNet.Security.OAuth.Introspection;
using ILT.API.APIModel;
using ILT.API.Common;
using ILT.API.ExternalIntegration.EdCast;
using ILT.API.Helper;
using ILT.API.Helper.Metadata;
using ILT.API.Model;
using ILT.API.Model.Log_API_Count;
using ILT.API.Repositories.Interfaces;
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
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class AccessibiltyRuleController : IdentityController
    {
        private IAccessibilityRule _accessibilityRule;
        private IAccebilityRuleUserGroup _accebilityRuleUserGroup;
        private IUserGroup _userGroup;
        private IAccessibilityRuleRejectedRepository _accessibilityRuleRejectedRepository;
        private readonly ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicabilityGroupTemplate _applicabilityGroupTemplateRepository;
        private readonly ITokensRepository _tokensRepository;
        ICourseRepository _courseRepository;
        public IConfiguration _configuration { get; set; }
        private readonly IWebHostEnvironment hostingEnvironment;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleController));
        public AccessibiltyRuleController(IAccessibilityRule accessibilityRule,
            IAccebilityRuleUserGroup accebilityRuleUserGroup,
            IUserGroup userGroup,
            IAccessibilityRuleRejectedRepository accessibilityRuleRejectedRepository,
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment,
            IIdentityService identitySvc, IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnectionStringRepository,
            IApplicabilityGroupTemplate applicabilityGroupTemplateRepository,
            ITokensRepository tokensRepository, ICourseRepository courseRepository) : base(identitySvc)
        {
            _accessibilityRule = accessibilityRule;
            _accessibilityRuleRejectedRepository = accessibilityRuleRejectedRepository;
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
                return Ok(await _accessibilityRule.GetAll());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{id}")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {

                return Ok(await _accessibilityRule.Get(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [PermissionRequired(Permissions.courseapplicability)]
        [HttpGet("CourseName/{page:int}/{pageSize:int}/{search?}")]
        public async Task<IActionResult> GetCourseName(int page, int pageSize, string search = null)
        {
            try
            {

                return Ok(await _accessibilityRule.GetCourseName(page, pageSize, search));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getTotalCourseRecords/{search:minlength(0)?}/{columnName?}")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetCourseCount(string search = null)
        {
            try
            {

                int count = await _accessibilityRule.CourseCount(search);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> Get(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _accessibilityRule.Get(page, pageSize, search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCourseApplicability")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetV2([FromBody] APICourseApplicability aPICourseApplicability)
        {
            try
            {
                return Ok(await _accessibilityRule.GetV2(aPICourseApplicability,UserId,RoleCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getTotalRecords/{search:minlength(0)?}/{columnName?}")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetCount(string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _accessibilityRule.count(search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet("getCategoryData/{page:int}/{pageSize:int}/{search?}/{columnName?}")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetcategoryData(int page, int pageSize, string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _accessibilityRule.GetCategoryData(page, pageSize, search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet("getCategoryTotalRecords/{search:minlength(0)?}/{columnName?}")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetCategoryCount(string search = null, string columnName = null)
        {
            try
            {
                return Ok(await _accessibilityRule.categorycount(search, columnName));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("GetAllUserGroups")]
        public async Task<IActionResult> GetAllUserGroups()
        {
            try
            {
                return Ok(await _accessibilityRule.GetAllUserGroups());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetRules")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetRules([FromBody] APIGetRules objAPIGetRules)
        {
            try
            {
                return Ok(await _accessibilityRule.GetAccessibilityRules(objAPIGetRules.courseId, OrganisationCode, Token, objAPIGetRules.page, objAPIGetRules.pageSize));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCategoryRules")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetCategoryRules([FromBody] APIGetCategoryRules objAPIGetRules)
        {
            try
            {
                return Ok(await _accessibilityRule.GetCategoryAccessibilityRules(objAPIGetRules.CategoryId, OrganisationCode, Token, objAPIGetRules.page, objAPIGetRules.pageSize));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }



        [HttpPost("GetRulesCount")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetRulesCount([FromBody] APIGetRules objAPIGetRules)
        {
            try
            {
                return Ok(await _accessibilityRule.GetAccessibilityRulesCount(objAPIGetRules.courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCategoryRulesCount")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetCategoryRulesCount([FromBody] APIGetCategoryRules objAPIGetRules)
        {
            try
            {
                return Ok(await _accessibilityRule.GetCategoryAccessibilityRulesCount(objAPIGetRules.CategoryId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpGet("GetRule/{ruleId:int}/")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetRule(int ruleId)
        {
            try
            {
                return Ok(await _accessibilityRule.GetAccessibilityRule(ruleId, OrganisationCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Produces("application/json")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> Post([FromBody] APIAccessibilityRules apiAccessibilityRules)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Message ValidationMessage = ValidateRules(apiAccessibilityRules);
                    if (ValidationMessage == Message.InvalidModel)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ErrorMessage.ModelError });
                    }
                    else if (ValidationMessage == Message.SameData)
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ErrorMessage.SameData });
                    }

                    APIAccessibility apiAccessibility = ConvertToAPIAccessibility(apiAccessibilityRules);
                    List<AccessibilityRules> result = await _accessibilityRule.Post(apiAccessibility, UserId);

                    if (result == null)
                        return Ok("Success");

                    apiAccessibilityRules = new APIAccessibilityRules
                    {
                        AccessibilityParameter1 = result.ElementAt(0).AccessibilityRule,
                        AccessibilityValue1 = result.ElementAt(0).ParameterValue,
                        Condition1 = result.ElementAt(0).Condition,
                        ErrorMessage = EnumHelper.GetEnumName(MessageType.Duplicate)
                    };
                    if (result.Count == 2)
                    {
                        apiAccessibilityRules.AccessibilityParameter2 = result.ElementAt(1).AccessibilityRule;
                        apiAccessibilityRules.AccessibilityValue2 = result.ElementAt(1).ParameterValue;
                    }
                    return BadRequest(apiAccessibilityRules);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostMultiple")]
        [Produces("application/json")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> Post([FromBody] APIAccessibilityRules[] rules)
        {
            try
            {
                _logger.Debug("In Post Multiple" + OrganisationCode);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<APIAccessibilityRules> rejectedRules = new List<APIAccessibilityRules>();
                foreach (APIAccessibilityRules rule in rules)
                {
                    if(rule.AccessibilityParameter1 == "UserName")
                    {
                        rule.AccessibilityParameter1 = "UserId";
                    }
                    if(rule.AccessibilityParameter2 == "UserName")
                    {
                        rule.AccessibilityParameter2 = "UserId";
                    }
                    bool isvalid = await _accessibilityRule.CheckValidData(rule.AccessibilityParameter1, rule.AccessibilityValue1, rule.AccessibilityParameter2, rule.AccessibilityValue2, rule.CourseId, rule.AccessibilityValue11, rule.AccessibilityValue22);
                    if (!isvalid)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                foreach (APIAccessibilityRules rule in rules)
                {
                    APIAccessibility apiAccessibility = ConvertToAPIAccessibility(rule);
                    _logger.Debug("entering Post");
                    List<AccessibilityRules> result = await _accessibilityRule.Post(apiAccessibility, UserId, OrganisationCode, Token);
                    _logger.Debug("out of post");
                    if (result != null)
                        rejectedRules.Add(rule);

                }
                int byTeam=0;
                foreach (APIAccessibilityRules rule in rules)
                {
                    rule.AccessibilityParameter1 = "UserTeamId";
                    byTeam++;
                }
                if (byTeam > 0)
                {
                    //var enable_Edcast = await _courseRepository.GetMasterConfigurableParameterValue("Enable_Edcast");
                    //bool? PublishCourse = false;
                    //PublishCourse = await _courseRepository.IsPublishedCourse(rules[0].CourseId);
                    //_logger.Debug("Enable_Edcast :-" + enable_Edcast);
                    //if (Convert.ToString(enable_Edcast).ToLower() == "yes" && PublishCourse == true)
                    //{
                    //    APIEdcastDetailsToken result = await _courseRepository.GetEdCastToken();
                    //    if (result != null)
                    //    {

                    //        APIEdCastTransactionDetails obj = await _courseRepository.PostCourseToClient(rules[0].CourseId, UserId, result.access_token);
                    //    }
                    //    else
                    //    {
                    //        _logger.Debug("Token null from edcast");
                    //    }
                    //}
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

        [HttpPost]
        [Route("PostMultipleCategory")]
        [Produces("application/json")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> PostCategoryApplicability([FromBody] APICategoryAccessibilityRules[] rules)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<APICategoryAccessibilityRules> rejectedRules = new List<APICategoryAccessibilityRules>();
                foreach (APICategoryAccessibilityRules rule in rules)
                {
                    bool isvalid = await _accessibilityRule.CheckValidDatacategory(rule.AccessibilityParameter1, rule.AccessibilityValue1, rule.AccessibilityParameter2, rule.AccessibilityValue2, rule.AccessibilityParameter3, rule.AccessibilityValue3, rule.CategoryId, rule.SubCategoryId);
                    if (!isvalid)
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
                }
                foreach (APICategoryAccessibilityRules rule in rules)
                {

                    APICategoryAccessibility apiAccessibility = ConvertToAPICategoryAccessibility(rule);
                    List<AccessibilityRules> result = await _accessibilityRule.PostCategory(apiAccessibility, UserId, OrganisationCode, Token);

                    if (result != null)
                        rejectedRules.Add(rule);


                }
                if (rejectedRules != null)
                {
                    return Ok(rejectedRules);
                }
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost]
        [Route("PostByApplicabilityTemplate")]
        [PermissionRequired(Permissions.applicabilitytemplate + " " + Permissions.courseapplicability)]
        public async Task<IActionResult> PostByApplicabilityTemplate([FromBody] APIAccessibility apiAccessibility)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    APIApplicabilityGroupTemplate ApiRule = await _applicabilityGroupTemplateRepository.GetApplicabilityGroupTemplate(Convert.ToInt32(apiAccessibility.GroupTemplateId));

                    int i = 0;
                    apiAccessibility.AccessibilityRule = new AccessibilityRules[ApiRule.ApplicabilityGroupRule.ToArray().Count()];
                    foreach (var rule in ApiRule.ApplicabilityGroupRule)
                    {
                        AccessibilityRules accessibilityRules = new AccessibilityRules
                        {
                            AccessibilityRule = rule.ApplicabilityRule,
                            ParameterValue = rule.ParameterValue,
                            Condition = rule.Condition
                        };
                        apiAccessibility.AccessibilityRule[i] = accessibilityRules;
                        i++;
                    }
                    List<AccessibilityRules> result = await _accessibilityRule.Post(apiAccessibility, UserId, OrganisationCode, Token);

                    if (result == null)
                        return Ok("Success");

                    APIAccessibilityRules apiAccessibilityRules = new APIAccessibilityRules
                    {
                        AccessibilityParameter1 = result.ElementAt(0).AccessibilityRule,
                        AccessibilityValue1 = result.ElementAt(0).ParameterValue,
                        Condition1 = result.ElementAt(0).Condition,
                        ErrorMessage = EnumHelper.GetEnumName(MessageType.Duplicate)
                    };
                    if (result.Count == 2)
                    {
                        apiAccessibilityRules.AccessibilityParameter2 = result.ElementAt(1).AccessibilityRule;
                        apiAccessibilityRules.AccessibilityValue2 = result.ElementAt(1).ParameterValue;
                    }
                    return StatusCode(409, apiAccessibilityRules);
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostMultipleByGroup")]
        public async Task<IActionResult> PostMultipleByGroup([FromBody] ApiAccebilityRuleUserGroup apiAccebilityRuleUserGroup)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                bool isvalid = await _accessibilityRule.CheckValidDataForUserGroup(apiAccebilityRuleUserGroup);
                if (!isvalid)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });

                List<UserGroup> users = await _userGroup.GetAllUsersOfGroup(apiAccebilityRuleUserGroup.UserGroupId);

                foreach (UserGroup user in users)
                {
                    AccebilityRuleUserGroup accebilityRuleUserGroup = new AccebilityRuleUserGroup();

                    accebilityRuleUserGroup.UserGroupId = user.Id;
                    accebilityRuleUserGroup.CourseId = apiAccebilityRuleUserGroup.CourseId;
                    accebilityRuleUserGroup.StartDate = apiAccebilityRuleUserGroup.StartDate;
                    accebilityRuleUserGroup.EndDate = apiAccebilityRuleUserGroup.EndDate;
                    accebilityRuleUserGroup.CreatedBy = UserId;
                    accebilityRuleUserGroup.CreatedDate = DateTime.Now;
                    accebilityRuleUserGroup.IsDeleted = false;
                    accebilityRuleUserGroup.IsActive = true;

                    await this._accebilityRuleUserGroup.Add(accebilityRuleUserGroup);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostFileUpload")]
        [PermissionRequired(Permissions.applicabilityImport)]
        public async Task<IActionResult> PostFileUpload()
        {
            try
            {
                _accessibilityRuleRejectedRepository.Delete();

                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.XLSX : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        if (FileValidation.IsValidXLSX(fileUpload))
                        {
                            string filename = fileUpload.FileName;
                            string[] fileaary = filename.Split('.');
                            string fileextention = fileaary[1].ToLower();
                            string filex = Record.XLSX;
                            if (fileextention != filex)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });
                            }
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrganisationCode, Record.Courses);
                            fileDir = Path.Combine(fileDir, customerCode, FileType);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));
                        }
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("SaveFileData")]
        [PermissionRequired(Permissions.applicabilityImport)]
        public async Task<ApiResponse> PostFile([FromBody] APIAccessibilityRuleFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new ApiResponse() { StatusCode = 400, Description = "Invalid post request" };

                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string filefinal = sWebRootFolder + aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(filefinal));

                return await _accessibilityRule.ProcessImportFile(file, _customerConnectionStringRepository, UserId, _configuration, OrganisationCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new ApiResponse() { StatusCode = 400, Description = "Error while importing course applicability file. Please contact support." };
            }
        }


        [HttpPost]
        [Route("SaveUserGroupData")]
        public async Task<ApiResponse> SaveGroupData([FromBody] UserGroupImportPayload aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new ApiResponse() { StatusCode = 400, Description = "Invalid post request" };

                bool DuplicateName = await _accessibilityRule.GroupNameExists(aPIFilePath.GroupName);
                if(DuplicateName)
                    return new ApiResponse() { StatusCode = 400, Description = "Name Already Exists!" };

                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string filefinal = sWebRootFolder + aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(filefinal));

                return await _accessibilityRule.ProcessGroupImportFile(file, _customerConnectionStringRepository, UserId, aPIFilePath.GroupName, _configuration, OrganisationCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new ApiResponse() { StatusCode = 400, Description = "Error while importing course applicability file. Please contact support." };
            }
        }

        [HttpPost]
        [Route("ExportCourseApplicabilityReport")]
        [Route("ExportUserGroupImportReport")]
        [PermissionRequired(Permissions.applicabilityImport)]
        public async Task<IActionResult> DownloadCourseApplicabilityReport([FromBody] APIAccessibilityRuleFilePath aPIFilePath)
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, aPIFilePath.Path));

                if (!file.Exists)
                    return BadRequest(new ResponseMessage { Message = "File does not exits." });

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet]
        [Route("Export")]
        [PermissionRequired(Permissions.courseapplicability)]

        public async Task<IActionResult> Export()

        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"SetApplicability.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SetApplicability");
                    //First add the headers

                    worksheet.Cells[1, 1].Value = "CourseCode";
                    worksheet.Cells[1, 2].Value = "UserId";

                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                return File(fileData, FileContentType.Excel, "SetApplicability");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpPost("Rules/{id}")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> RulesPut(int id, [FromBody] APIAccessibilityRules accessibilityRule)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Message ValidationMessage = ValidatePutRules(accessibilityRule);
                if (ValidationMessage == Message.InvalidModel)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ErrorMessage.ModelError });
                }
                else if (ValidationMessage == Message.SameData)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = ErrorMessage.SameData });
                }
                int result = await _accessibilityRule.UpdateRule(accessibilityRule, id);
                if (result == 0)
                {
                    return NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpDelete("Rule")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> RuleDelete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await _accessibilityRule.DeleteRule(DecryptedId);
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


        [HttpDelete("DeleteCategoryRule")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> DeleteCategoryRule([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                int Result = await _accessibilityRule.DeleteRule(DecryptedId);
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

        [HttpDelete]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> Delete([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                AccessibilityRule accessibilityRule = await _accessibilityRule.Get(DecryptedId);
                if (accessibilityRule == null)
                {
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
                }
                accessibilityRule.IsDeleted = true;
                await _accessibilityRule.Update(accessibilityRule);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        private Message ValidateRules(APIAccessibilityRules apiAccessibilityRules)
        {
            if (apiAccessibilityRules.AccessibilityParameter1 == null && apiAccessibilityRules.AccessibilityParameter2 == null)
                return Message.InvalidModel;
            if (apiAccessibilityRules.AccessibilityParameter1 != null && apiAccessibilityRules.AccessibilityValue1 == null)
                return Message.InvalidModel;
            if (apiAccessibilityRules.AccessibilityParameter2 != null && apiAccessibilityRules.AccessibilityValue2 == null)
                return Message.InvalidModel;
            if (apiAccessibilityRules.AccessibilityParameter1 != null && apiAccessibilityRules.AccessibilityParameter2 != null)
            {
                if (apiAccessibilityRules.Condition1 == null)
                    return Message.InvalidModel;
                if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals(apiAccessibilityRules.AccessibilityParameter2.ToLower()))
                    if (apiAccessibilityRules.AccessibilityValueId1 == apiAccessibilityRules.AccessibilityValueId2)
                        return Message.SameData;
            }
            if (apiAccessibilityRules.AccessibilityParameter1 != null && !apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("emailid"))
            {
                bool isNumeric = int.TryParse(apiAccessibilityRules.AccessibilityValue1, out int n);
                if (!isNumeric)
                    return Message.InvalidModel;
            }
            if (apiAccessibilityRules.AccessibilityParameter2 != null && !apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("emailid"))
            {
                bool isNumeric = int.TryParse(apiAccessibilityRules.AccessibilityValue2, out int n);
                if (!isNumeric)
                    return Message.InvalidModel;
            }
            return Message.Ok;
        }
        private Message ValidatePutRules(APIAccessibilityRules apiAccessibilityRules)
        {
            if (apiAccessibilityRules.AccessibilityParameter1 == null && apiAccessibilityRules.AccessibilityParameter2 == null)
                return Message.InvalidModel;
            if (apiAccessibilityRules.AccessibilityParameter1 != null && apiAccessibilityRules.AccessibilityValueId1 == 0)
                return Message.InvalidModel;
            if (apiAccessibilityRules.AccessibilityParameter2 != null && apiAccessibilityRules.AccessibilityValueId2 == 0)
                return Message.InvalidModel;
            if (apiAccessibilityRules.AccessibilityParameter1 != null && apiAccessibilityRules.AccessibilityParameter2 != null)
            {
                if (apiAccessibilityRules.Condition1 == null)
                    return Message.InvalidModel;
                if (apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals(apiAccessibilityRules.AccessibilityParameter2.ToLower()))
                    if (apiAccessibilityRules.AccessibilityValue1.ToLower().Equals(apiAccessibilityRules.AccessibilityValue2.ToLower()))
                        return Message.SameData;
            }
            if (apiAccessibilityRules.AccessibilityParameter1 != null && apiAccessibilityRules.AccessibilityParameter1.ToLower().Equals("emailid"))
            {
                if (apiAccessibilityRules.AccessibilityValue1 == null)
                    return Message.InvalidModel;
            }
            if (apiAccessibilityRules.AccessibilityParameter2 != null && apiAccessibilityRules.AccessibilityParameter2.ToLower().Equals("emailid"))
            {
                if (apiAccessibilityRules.AccessibilityValue2 == null)
                    return Message.InvalidModel;
            }
            return Message.Ok;
        }
        private APIAccessibility ConvertToAPIAccessibility(APIAccessibilityRules apiAccessibilityRules)
        {
            APIAccessibility apiAccessibility = new APIAccessibility
            {
                CourseId = apiAccessibilityRules.CourseId,
                EdCast_due_date = apiAccessibilityRules.EdCast_due_date,
                EdCast_assigned_date = apiAccessibilityRules.EdCast_assigned_date
            };

            if (apiAccessibilityRules.AccessibilityParameter2 == null)
                apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
            else
                apiAccessibility.AccessibilityRule = new AccessibilityRules[2];


            if (apiAccessibilityRules.AccessibilityParameter1 != null)
            {
                AccessibilityRules AccessibilityRules = new AccessibilityRules
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
                AccessibilityRules AccessibilityRules = new AccessibilityRules
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
        private APICategoryAccessibility ConvertToAPICategoryAccessibility(APICategoryAccessibilityRules apiAccessibilityRules)
        {
            APICategoryAccessibility apiAccessibility = new APICategoryAccessibility
            {
                CategoryId = apiAccessibilityRules.CategoryId
            };

            if (apiAccessibilityRules.SubCategoryId != null)
            {
                apiAccessibility.SubCategoryId = apiAccessibilityRules.SubCategoryId;
            }

            if (apiAccessibilityRules.AccessibilityParameter3 == null && apiAccessibilityRules.AccessibilityParameter2 == null && apiAccessibilityRules.AccessibilityParameter1 != null)
                apiAccessibility.AccessibilityRule = new AccessibilityRules[1];
            else if (apiAccessibilityRules.AccessibilityParameter2 != null && apiAccessibilityRules.AccessibilityParameter1 != null && apiAccessibilityRules.AccessibilityParameter3 == null)
                apiAccessibility.AccessibilityRule = new AccessibilityRules[2];
            else
                apiAccessibility.AccessibilityRule = new AccessibilityRules[3];


            if (apiAccessibilityRules.AccessibilityParameter1 != null)
            {
                AccessibilityRules AccessibilityRules = new AccessibilityRules
                {
                    AccessibilityRule = apiAccessibilityRules.AccessibilityParameter1,
                    ParameterValue = apiAccessibilityRules.AccessibilityValue1,
                    Condition = apiAccessibilityRules.Condition1 == null ? "null" : apiAccessibilityRules.Condition1
                };
                apiAccessibility.AccessibilityRule[0] = AccessibilityRules;
            }
            if (apiAccessibilityRules.AccessibilityParameter2 != null)
            {
                AccessibilityRules AccessibilityRules = new AccessibilityRules
                {
                    AccessibilityRule = apiAccessibilityRules.AccessibilityParameter2,
                    ParameterValue = apiAccessibilityRules.AccessibilityValue2,
                    Condition = apiAccessibilityRules.Condition1
                };
                apiAccessibility.AccessibilityRule[1] = AccessibilityRules;
            }
            if (apiAccessibilityRules.AccessibilityParameter3 != null)
            {
                AccessibilityRules AccessibilityRules = new AccessibilityRules
                {
                    AccessibilityRule = apiAccessibilityRules.AccessibilityParameter3,
                    ParameterValue = apiAccessibilityRules.AccessibilityValue3,
                    Condition = apiAccessibilityRules.Condition1
                };
                apiAccessibility.AccessibilityRule[2] = AccessibilityRules;
            }
            return apiAccessibility;
        }


        [HttpGet]
        [Route("CourseApplicabilityImport")]
        public IActionResult CourseApplicabilityImport()
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "CourseApplicabilityImport.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CourseApplicabilityImport");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "CourseCode";
                    worksheet.Cells[1, 2].Value = "UserId";
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                    }
                    package.Save();
                }

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        [HttpGet]
        [Route("UserGroupImport")]
        public IActionResult UserGroupImport()
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "UserGroupImport.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("UserGroupImport");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "UserId";
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                    }
                    package.Save();
                }

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("GetCourseApplicableUserList_Export")]
        public async Task<IActionResult> GetCourseApplicableUserList_Export([FromBody] APIGetRules courseApplicableUser)
        {
            try
            {
                FileInfo ExcelFile;
                int Id = courseApplicableUser.courseId;
                var CourseName = await this._accessibilityRule.GetCourseNames(Id);
                List<APIAccessibilityRules> accessibilityRules = new List<APIAccessibilityRules>();
                List<CourseApplicableUser> UserListForUserTeam = new List<CourseApplicableUser>();
                List<CourseApplicableUser> UserList = new List<CourseApplicableUser>();

                accessibilityRules = await this._accessibilityRule.GetAccessibilityRulesForExport(courseApplicableUser.courseId, OrganisationCode, Token, CourseName);
                List<AccessibilityRule> accessibilityRule = this._accessibilityRule.GetRuleByUserTeams(courseApplicableUser.courseId);
                UserList = await this._accessibilityRule.GetCourseApplicableUserList(courseApplicableUser.courseId);

                if (accessibilityRule != null)
                {
                    foreach(AccessibilityRule accessibilityRule1 in accessibilityRule)
                    {
                        List<CourseApplicableUser> UserListForUserTeam1 = this._accessibilityRule.GetUsersForUserTeam(accessibilityRule1.UserTeamId);
                        foreach(CourseApplicableUser courseApplicableUser1 in UserListForUserTeam1)
                        {
                            UserListForUserTeam.Add(courseApplicableUser1);
                        }
                    }
                    foreach(CourseApplicableUser courseApplicableUser2 in UserListForUserTeam)
                    {
                        if (!UserList.Contains(courseApplicableUser2))
                        {
                            UserList.Add(courseApplicableUser2);
                        }
                    }
                }

                ExcelFile = this._accessibilityRule.GetApplicableUserListExcel(accessibilityRules, UserList, CourseName, OrganisationCode);

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


        [HttpPost("GetCategoryApplicableUserList_Export")]
        public async Task<IActionResult> GetCategoryApplicableUserList_Export([FromBody] APIGetCategoryRules courseApplicableUser)
        {
            try
            {

                FileInfo ExcelFile;
                int Id = courseApplicableUser.CategoryId;
                var CategoryName = await this._accessibilityRule.GetCategoryNames(Id);
                List<APICategoryAccessibilityRules> accessibilityRules = new List<APICategoryAccessibilityRules>();
                accessibilityRules = await this._accessibilityRule.GetCategoryAccessibilityRulesForExport(courseApplicableUser.CategoryId, OrganisationCode, Token, CategoryName);

                List<CategoryApplicableUser> UserList = new List<CategoryApplicableUser>();
                UserList = await this._accessibilityRule.GetCategoryApplicableUserList(courseApplicableUser.CategoryId);
                ExcelFile = this._accessibilityRule.GetCategoryApplicableUserListExcel(accessibilityRules, UserList, CategoryName, OrganisationCode);

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


        [HttpGet]
        [Route("ExportCategory")]
        public async Task<IActionResult> ExportCategory()

        {
            try
            {

                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = @"SubjectApplicability.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("SetApplicability");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "Course";
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 2].Value = "Semester";
                    worksheet.Cells[1, 2].Style.Font.Bold = true;
                    worksheet.Cells[1, 3].Value = "Subject" + Record.Strar;
                    worksheet.Cells[1, 3].Style.Font.Bold = true;
                    worksheet.Cells[1, 4].Value = "Unit";
                    worksheet.Cells[1, 4].Style.Font.Bold = true;
                    worksheet.Cells[1, 5].Value = "UserId";
                    worksheet.Cells[1, 5].Style.Font.Bold = true;
                    worksheet.Cells[1, 6].Value = "Languages";
                    worksheet.Cells[1, 6].Style.Font.Bold = true;
                    package.Save(); //Save the workbook.
                }
                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("PostSaveFileData")]
        [PermissionRequired(Permissions.applicabilityImport)]
        public async Task<IActionResult> PostSaveFileData([FromBody] APIAccessibilityRuleFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)

                    return this.BadRequest(this.ModelState);
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string filefinal = sWebRootFolder + aPIFilePath.Path;
                FileInfo file = new FileInfo(Path.Combine(filefinal));
                ApiResponse response = await this._accessibilityRule.ProcessImportCategory(file, _courseRepository, _accessibilityRule, _accessibilityRuleRejectedRepository, _customerConnectionStringRepository, UserId, _configuration, OrganisationCode);
                return Ok(response.ResponseObject);

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }

        }

        [HttpPost]
        [Route("CategoryPostFileUpload")]
        [PermissionRequired(Permissions.applicabilityImport)]
        public async Task<IActionResult> CategoryPostFileUpload()
        {
            try
            {
                _accessibilityRuleRejectedRepository.Delete();

                var request = _httpContextAccessor.HttpContext.Request;
                string customerCode = string.IsNullOrEmpty(request.Form[Record.CustomerCode]) ? "4.5" : request.Form[Record.CustomerCode].ToString();
                string FileType = string.IsNullOrEmpty(request.Form[Record.FileType]) ? Record.XLSX : request.Form[Record.FileType].ToString();
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile fileUpload in request.Form.Files)
                    {
                        if (fileUpload.Length < 0 || fileUpload == null)
                        {
                            return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                        }
                        if (FileValidation.IsValidXLSX(fileUpload))
                        {
                            string filename = fileUpload.FileName;
                            string[] fileaary = filename.Split('.');
                            string fileextention = fileaary[1].ToLower();
                            string filex = Record.XLSX;
                            if (fileextention != filex)
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = Record.InvalidFile });

                            }
                            string fileDir = this._configuration["ApiGatewayWwwroot"];
                            fileDir = Path.Combine(fileDir, OrganisationCode, Record.Courses);
                            fileDir = Path.Combine(fileDir, customerCode, FileType);
                            if (!Directory.Exists(fileDir))
                            {
                                Directory.CreateDirectory(fileDir);
                            }
                            string file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + FileType);
                            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                            {
                                await fileUpload.CopyToAsync(fs);
                            }
                            if (String.IsNullOrEmpty(file))
                            {
                                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.PathNotFoundFile });
                            }
                            return Ok(file.Substring(file.LastIndexOf("\\" + OrganisationCode)).Replace(@"\", "/"));
                        }
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = Record.InvalidFile });
                    }

                }
                return this.NotFound(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpGet]
        [Route("UnassignCourseApplicabilityImport")]
        public IActionResult DeleteApplicabilityImport()
        {
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                sWebRootFolder = Path.Combine(sWebRootFolder, OrganisationCode);
                string DomainName = this._configuration["ApiGatewayUrl"];
                string sFileName = "CourseApplicabilityUnassignImport.xlsx";
                string URL = string.Format("{0}{1}/{2}", DomainName, OrganisationCode, sFileName);
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                using (ExcelPackage package = new ExcelPackage(file))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CourseUnAssignImport");
                    //First add the headers
                    worksheet.Cells[1, 1].Value = "CourseCode";
                    worksheet.Cells[1, 2].Value = "UserId";
                    using (var rngitems = worksheet.Cells["A1:BH1"])//Applying Css for header
                    {
                        rngitems.Style.Font.Bold = true;
                    }
                    package.Save();
                }

                var Fs = file.OpenRead();
                byte[] fileData = null;
                using (BinaryReader binaryReader = new BinaryReader(Fs))
                {
                    fileData = binaryReader.ReadBytes((int)Fs.Length);
                }
                Response.ContentType = FileContentType.Excel;
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                if (file.Exists)
                {
                    file.Delete();
                    file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                }
                return File(fileData, FileContentType.Excel);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost]
        [Route("ApplicabilityDelete")]
        [PermissionRequired(Permissions.applicabilityImport)]
        public async Task<ApiResponse> PostFileDeleteApplicability([FromBody] APIAccessibilityRuleFilePath aPIFilePath)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new ApiResponse() { StatusCode = 400, Description = EnumHelper.GetEnumDescription(MessageType.InvalidPostRequest)  };

                return await new AccessibilityRuleImport.ProcessFile().ProcessRecordsDeleteApplicabilityAsync(aPIFilePath.Path, _customerConnectionStringRepository, UserId, _configuration, OrganisationCode);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return new ApiResponse() { StatusCode = 400, Description = EnumHelper.GetEnumDescription(FileMessages.FileErrorInImportUnassignCourse) };
            }
        }

        [HttpPost("GetUserRules")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetRulesV2([FromBody] APIGetRules objAPIGetRules)
        {
            try
            {
                return Ok(await _accessibilityRule.GetAccessibilityRulesV2(objAPIGetRules.courseId, OrganisationCode, Token, objAPIGetRules.page, objAPIGetRules.pageSize,UserId,RoleCode));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        [HttpPost("GetUserRulesCount")]
        [PermissionRequired(Permissions.courseapplicability)]
        public async Task<IActionResult> GetRulesCountV2([FromBody] APIGetRules objAPIGetRules)
        {
            try
            {
                return Ok(await _accessibilityRule.GetAccessibilityRulesCount(objAPIGetRules.courseId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


    }
}
