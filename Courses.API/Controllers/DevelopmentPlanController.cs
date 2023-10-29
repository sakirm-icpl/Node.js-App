using AutoMapper.Configuration;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Helper.Metadata;
using Courses.API.Model;
using Courses.API.Model.Log_API_Count;
using Courses.API.Models;
using Courses.API.Repositories;
using Courses.API.Repositories.Interfaces;
using Courses.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Courses.API.Common.AuthorizePermissions;
using static Courses.API.Common.TokenPermissions;

namespace Courses.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/c/[controller]")]
    [Authorize]
    [TokenRequired()]
    public class DevelopmentPlanController : IdentityController
    {

        private readonly IIdentityService _identitySvc;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DevelopmentPlanController));
        private readonly IDevelopmentPlanRepository _developmentPlanRepository;
        private IAccessibilityRule _accessibilityRule;
        private ICourseRepository _CoursesRepository;

        public DevelopmentPlanController(IIdentityService identitySvc,
                                         IDevelopmentPlanRepository developmentPlanRepository,
                                         IAccessibilityRule accessibilityRule,
                                         ICourseRepository CoursesRepository
                                        ) : base(identitySvc)
        {
            _identitySvc = identitySvc;
            _developmentPlanRepository = developmentPlanRepository;
            _accessibilityRule = accessibilityRule;
            _CoursesRepository = CoursesRepository;
        }

        #region "DevelopmentPlan"
        [HttpGet("DevPlanCode/{isIdp}")]
        public async Task<IActionResult> GetDevelopementPlanCode(bool isIdp=false)
        {
            try
            {
                DevelopementPlanCode CourseCode = await _CoursesRepository.DevPlanCode(isIdp,OrgCode);
                string Code = CourseCode.Prefix + CourseCode.AutoNumber;
                return Ok(Code);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }

        [HttpPost("SaveDevelopmentPlan")]
        public async Task<IActionResult> SaveDevelopmentPlan([FromBody] Development development)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                if (development == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Data Not Found", Description = "Please enter data of development plan" });
                }
                if (development.DevelopmentCode == null || string.IsNullOrEmpty(development.DevelopmentCode))
                {
                    return BadRequest(new ResponseMessage { Message = "Development Code Not Found", Description = "Please check Development Code" });
                }
                if (development.DevelopmentName == null || string.IsNullOrEmpty(development.DevelopmentName))
                {
                    return BadRequest(new ResponseMessage { Message = "Development Name Not Found", Description = "Please check Development Name" });
                }
                if (development.EnforceLinearApproach == null)
                {
                    return BadRequest(new ResponseMessage { Message = "EnforceLinearApproach Not Found", Description = "Please check Enforce Linear Approach" });
                }
                if (development.AllowLearningAfterExpiry == null)
                {
                    return BadRequest(new ResponseMessage { Message = "AllowLearningAfterExpiry Not Found", Description = "Please check Allow Learning After Expiry" });
                }
                if (development.TargetCompletion == null)
                {
                    return BadRequest(new ResponseMessage { Message = "TargetCompletion Not Found", Description = "Please check Target Completion" });
                }
                if (development.TotalCreditPoints == null)
                {
                    return BadRequest(new ResponseMessage { Message = "TotalCreditPoints Not Found", Description = "Please check Total Credit Points" });
                }
                if (development.CountOfMappedCourses == null)
                {
                    return BadRequest(new ResponseMessage { Message = "CountOfMappedCourses Not Found", Description = "Please check Count Of Mapped Courses" });
                }

                DevelopmentPlanForCourse developmentPlanForCourse = new DevelopmentPlanForCourse();

                developmentPlanForCourse.CreatedDate = DateTime.Now;
                developmentPlanForCourse.ModifiedDate = DateTime.Now;
                developmentPlanForCourse.ModifiedBy = UserId;
                developmentPlanForCourse.CreatedBy = UserId;
                developmentPlanForCourse.TargetCompletion = development.TargetCompletion;
                developmentPlanForCourse.TotalCreditPoints = development.TotalCreditPoints;
                developmentPlanForCourse.IsDeleted = false;
                developmentPlanForCourse.Status = development.Status;
                developmentPlanForCourse.AboutPlan = development.AboutPlan;
                developmentPlanForCourse.FeedbackId = development.FeedbackId;
                if (development.isIdp)
                {
                    developmentPlanForCourse.NumberofMembers = 1;
                    developmentPlanForCourse.NumberOfRules = 1;
                }
                else {
                    developmentPlanForCourse.NumberofMembers = 0;
                    developmentPlanForCourse.NumberOfRules = 0;
                }
                developmentPlanForCourse.EnforceLinearApproach = development.EnforceLinearApproach;
                developmentPlanForCourse.AllowLearningAfterExpiry = development.AllowLearningAfterExpiry;
                developmentPlanForCourse.CountOfMappedCourses = development.CountOfMappedCourses;
                developmentPlanForCourse.DevelopmentCode = development.DevelopmentCode;
                developmentPlanForCourse.DevelopmentName = development.DevelopmentName;
                developmentPlanForCourse.UploadThumbnail = development.UploadThumbnail;
                developmentPlanForCourse.StartDate = development.StartDate;
                developmentPlanForCourse.EndDate = development.EndDate;
                developmentPlanForCourse.Metadata = development.Metadata;

                int Result = await this._developmentPlanRepository.SaveDevelopmentPlan(developmentPlanForCourse, development.developmentPlanCourses, UserId, development.isIdp);

                if (Result == 0)
                {
                    return Ok(new ResponseMessage { Message = "Development Plan save", Description = "Development Plan Saved Successfully" });
                }
                else if(Result == -4)
                {
                    return Conflict(new ResponseMessage { Message = "Duplicate Development Plan Code" });
                }
                else if(Result == -5)
                {
                    return Conflict(new ResponseMessage { Message = "Duplicate Development Plan Name" });
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = "Failed To Save Development Plan" });
                }
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("GetAllDevelopmentPlan")]
        public async Task<IActionResult> GetUserTeams([FromBody] DevelopmentPlanPage developmentPlanPage)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                IEnumerable<APIDevelopmentPlanForCourse> developmentPlanForCourses = await _developmentPlanRepository.GetAllDevelopmentPlan(developmentPlanPage.Page, developmentPlanPage.PageSize, developmentPlanPage.Search, developmentPlanPage.ColumnName, UserId, developmentPlanPage.isIdp);
                return Ok(developmentPlanForCourses);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("GetAllDevelopmentPlanCount")]
        public async Task<IActionResult> GetUserTeamsCount([FromBody] DevelopmentPlanPage developmentPlanPage)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                int Count = await _developmentPlanRepository.GetAllDevelopmentPlanCount(developmentPlanPage.Page, developmentPlanPage.PageSize, developmentPlanPage.Search, developmentPlanPage.ColumnName, UserId, developmentPlanPage.isIdp);
                return Ok(Count);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpDelete("DeleteDevelopmentPlan")]
        public IActionResult DeleteDevelopmentPlan([FromQuery] string developmentCode)
        {
            try
            {
                if (developmentCode == null || string.IsNullOrEmpty(developmentCode))
                {
                    return BadRequest(new ResponseMessage { Message = "Teams code is null" });
                }
                int Result = this._developmentPlanRepository.DeleteDevelopmentPlan(developmentCode);

                if (Result == 0)
                {
                    return Ok(new ResponseMessage { Message = "Record Deleted Successfully" });
                }
                else if (Result == -2)
                {
                    return BadRequest(new ResponseMessage { Message = "Development Code is not available", Description = "Development Code is invalid" });
                }
                else
                {
                    return BadRequest(new ResponseMessage { Message = "Development Code is null", Description = "Development Code is null" });
                }
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("UpdateDevelopmentPlan")]
        public async Task<IActionResult> UpdateDevelopmentPlan([FromBody] Development development)
        {
            try
            {
                if (!ModelState.IsValid)
                    return this.BadRequest(this.ModelState);

                if (development == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Data Not Found", Description = "Please enter data of user team" });
                }
                if (development.DevelopmentCode == null || string.IsNullOrEmpty(development.DevelopmentCode))
                {
                    return BadRequest(new ResponseMessage { Message = "Development Code Not Found", Description = "Please check Development Code" });
                }
                if (development.DevelopmentName == null || string.IsNullOrEmpty(development.DevelopmentName))
                {
                    return BadRequest(new ResponseMessage { Message = "Development Name Not Found", Description = "Please check Development Name" });
                }
                DevelopmentPlanForCourse userTeams1 = _developmentPlanRepository.GetDevelopmentPlanByTeamsCode(development.DevelopmentCode);
                if (userTeams1 == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Invalid Teams Code", Description = "Please check team code" });
                }
                int Result = await _developmentPlanRepository.UpdateDevelopmentPlan(development, UserId);
                if (Result == -4)
                {
                    return BadRequest(new ResponseMessage { Message = "Duplicate Teams Name", Description = "Teams Name is invalid name should be unique" });
                }
                if (Result == 0)
                {
                    return Ok(new ResponseMessage { Message = "User Team Update", Description = "User Team Updated Successfully" });
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("GetCourseDetailsById")]
        public async Task<List<DevelopmentCoursesDetails>> GetdevelopmentCourseDetails([FromBody] DevelopmentId developmentId)
        {
            if (developmentId.developmentid == 0)
            {
                return null;
            }
            return await _developmentPlanRepository.getCourseDetailsByDevelopmentId(developmentId.developmentid);
        }
        
        [HttpPost("GetDevelopmentPlanUserCount")]
        public async Task<IActionResult> GetDevelopmentPlanUserCount([FromBody]DevelopmentPlanCount developmentPlanCount)
        {
            try
            {
                if (developmentPlanCount == null )
                {
                    return BadRequest(new ResponseMessage { Message = "Development Plan is null" });
                }
                List<DevelopmentPlanApplicableUser> Result = await this._developmentPlanRepository.GetDevelopmentPlanApplicableUserList(developmentPlanCount.DevelopmentPlanId);

                if (Result != null)
                {
                    return Ok(Result.Count());
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
        #endregion

        #region "Accessibility"
        [HttpGet("TypeAheadForDevelopmentPlan/{search?}")]
        public async Task<IActionResult> GetTypeAheadForDevelopmentPlan(string search = null)
        {
            try
            {
                return Ok(await _developmentPlanRepository.GetDevelopmentPlanAccessibility(search));
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        [HttpPost("SaveDevelopmentPlanApplicability")]
        public async Task<IActionResult> SaveDevelopmentPlanApplicability([FromBody] MappingParameters[] mappingParameters)
        {
            List<Mappingparameter> reject = new List<Mappingparameter>();
            if (mappingParameters == null)
            {
                return BadRequest(new ResponseMessage { Message = "Invalid Data" });
            }
            foreach (MappingParameters rule in mappingParameters)
            {
                bool isvalid = false;

                isvalid = await _developmentPlanRepository.CheckValidData(rule.AccessibilityParameter1, rule.AccessibilityValue1, rule.AccessibilityParameter2, rule.AccessibilityValue2, rule.DevelopmentPlanid);

                if (!isvalid)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidData), Description = EnumHelper.GetEnumDescription(MessageType.InvalidData) });
            }
            foreach (MappingParameters mappingParameters1 in mappingParameters)
            {
                List<Mappingparameter> Result = await _developmentPlanRepository.CheckmappingStatus(mappingParameters1, UserId);

                if (Result != null)
                {
                    foreach (Mappingparameter mapping in Result)
                    {
                        reject.Add(mapping);
                    }
                }
            }
            if (reject.Count != 0)
            {
                if (reject[0].AccessibilityParameter1 == null)
                {
                    return BadRequest(new ResponseMessage { Message = "Invalid Development plan id" });
                }
                return (BadRequest(reject));
            }

            return Ok();
        }

        [HttpPost("GetRules")]
        public async Task<IActionResult> GetRules([FromBody] APIGetRulesFormapping objAPIGetRules)
        {
            try
            {
                return Ok(await _developmentPlanRepository.GetAccessibilityRules(objAPIGetRules.DevelopmentPlanId,OrganisationCode,Token, objAPIGetRules.page, objAPIGetRules.pageSize));
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
        [HttpPost("GetRulesCount")]
        public async Task<IActionResult> GetRulesCount([FromBody] APIGetRulesFormapping objAPIGetRules)
        {
            try
            {
                return Ok(await _developmentPlanRepository.GetAccessibilityRulesCount(objAPIGetRules.DevelopmentPlanId));
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message);
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
        [HttpDelete("DeleteRule")]
        public async Task<IActionResult> RuleDelete([FromQuery] string id)
        {
            try
            {
                int Id = Convert.ToInt32(id);
                int Result = await _developmentPlanRepository.DeleteRule(Id);
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

        [HttpPost("GetDevelopmentPlanUserList_Export")]
        public async Task<IActionResult> GetDevelopmentPlanUserList_Export([FromBody] APIGetRulesDevelopmentPlan aPIGetRulesDevelopmentPlan)
        {
            try
            {
                FileInfo ExcelFile;
                int Id = aPIGetRulesDevelopmentPlan.DevelopmentPlanId;
                var DevelopmentPlanName = await this._developmentPlanRepository.GetDevelopmentPlanName(Id);

                List<APIAccessibilityRulesDevelopment> accessibilityRules = new List<APIAccessibilityRulesDevelopment>();
                List<DevelopmentPlanApplicableUser> UserList = new List<DevelopmentPlanApplicableUser>();

                accessibilityRules = await this._developmentPlanRepository.GetAccessibilityRulesForExport(aPIGetRulesDevelopmentPlan.DevelopmentPlanId, OrganisationCode, Token, DevelopmentPlanName);
                UserList = await this._developmentPlanRepository.GetDevelopmentPlanApplicableUserList(aPIGetRulesDevelopmentPlan.DevelopmentPlanId);

                

                List<DevelopmentPlanApplicableUser> UserList1 = UserList.GroupBy(p => new { p.UserID, p.UserName })
                .Select(g => g.First())
                .ToList(); 
                ExcelFile = this._developmentPlanRepository.GetApplicableUserListExcel(accessibilityRules, UserList1, DevelopmentPlanName, OrganisationCode);

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

        #endregion

        #region DevPlanEU
        [HttpPost("EndUserDevPlan")]       
        public async Task<IActionResult> Get([FromBody] DevelopmentPlanEU developmentPlanEU)
        {
            _logger.Debug(string.Format("DevPlan Get method for {0} ", UserId));

            try
            {
                if (developmentPlanEU.search != null)
                    developmentPlanEU.search = developmentPlanEU.search.ToLower().Equals("null") ? null : developmentPlanEU.search;
                return Ok(await this._developmentPlanRepository.GetUserDevPlan(UserId, developmentPlanEU.page, developmentPlanEU.pageSize, developmentPlanEU.search, developmentPlanEU.DevPlanID));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpGet("GetDevPlanCourses/{DevPlanId:int}")]
        public async Task<IActionResult> GetDevPlanCourses(int DevPlanId)
        {
           
            try
            {                
                return Ok(await this._developmentPlanRepository.GetDevPlanCourses(DevPlanId,UserId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpGet("GetDevPlanForSequence/{DevPlanId:int}")]
        public async Task<IActionResult> GetDevPlanForSequence(int DevPlanId)
        {

            try
            {
                return Ok(await this._developmentPlanRepository.GetDevPlanForSequence(DevPlanId));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Ok();
        }

        [HttpGet("TypeAheadReport/{search?}")]
        public async Task<IActionResult> GetDevelopementPlan(string search = null)
        {
            try
            {
                List<APICourseDTO> course = await _developmentPlanRepository.GetDevelopementPlan(search);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex)); return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }


        #endregion
    }
}
