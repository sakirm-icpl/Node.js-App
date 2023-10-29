using AutoMapper.Configuration;
using CourseApplicability.API.APIModel;
using CourseApplicability.API.Common;
using CourseApplicability.API.Helper.Metadata;
using CourseApplicability.API.Model.Log_API_Count;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CourseApplicability.API.Common.TokenPermissions;

namespace CourseApplicability.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/a/[controller]")]
    [Authorize]
    [TokenRequired()]
    public class DevelopmentPlanApplicabilityController : IdentityController
    {

        private readonly IIdentityService _identitySvc;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DevelopmentPlanApplicabilityController));
        private readonly IDevelopmentPlanRepository _developmentPlanRepository;

        public DevelopmentPlanApplicabilityController(IIdentityService identitySvc,
                                         IDevelopmentPlanRepository developmentPlanRepository
                                        ) : base(identitySvc)
        {
            _identitySvc = identitySvc;
            _developmentPlanRepository = developmentPlanRepository;
        }
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
                return Ok(await _developmentPlanRepository.GetAccessibilityRules(objAPIGetRules.DevelopmentPlanId, OrganisationCode, Token, objAPIGetRules.page, objAPIGetRules.pageSize));
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
    }
}
