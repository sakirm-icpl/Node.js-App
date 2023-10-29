using CourseApplicability.API.APIModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApplicability.API.Repositories.Interfaces
{
    public interface IDevelopmentPlanRepository
    {
       
        Task<List<APIDevelopmentPlanType>> GetDevelopmentPlanAccessibility(string search);
        Task<List<Mappingparameter>> CheckmappingStatus(MappingParameters mappingParameters, int UserId);
        Task<List<MappingParameters>> GetAccessibilityRules(int DevelopmentPlanId, string orgnizationCode, string token, int Page, int PageSize);
        Task<int> GetAccessibilityRulesCount(int DevelopmentPlanId);
        Task<int> DeleteRule(int roleId);
        Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int DevelopmentPlanId);

        Task<string> GetDevelopmentPlanName(int DevelopmentPlanId);
        Task<List<APIAccessibilityRulesDevelopment>> GetAccessibilityRulesForExport(int DevelopmentPlanId, string orgnizationCode, string token, string DevelopmentPlanName);
     
        Task<List<DevelopmentPlanApplicableUser>> GetDevelopmentPlanApplicableUserList(int DevelopmentPlanId);
        FileInfo GetApplicableUserListExcel(List<APIAccessibilityRulesDevelopment> aPIAccessibilityRules, List<DevelopmentPlanApplicableUser> courseApplicableUsers, string DevelopmentPlanName, string OrgCode);
        

    }
}

