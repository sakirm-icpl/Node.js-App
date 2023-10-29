using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories
{
    public interface IDevelopmentPlanRepository
    {
        Task<int> SaveDevelopmentPlan(DevelopmentPlanForCourse development, DevelopmentPlanCourses[] developmentPlanCourses,int UserId, bool isIdp = false);
        Task<IEnumerable<APIDevelopmentPlanForCourse>> GetAllDevelopmentPlan(int page, int pageSize, string search = null, string columnName = null, int? userId = null,bool isIdp = false);
        Task<int> GetAllDevelopmentPlanCount(int page, int pageSize, string search = null, string columnName = null, int? userId = null, bool isIdp = false);
        int DeleteDevelopmentPlan(string DevelopmentCode);
        DevelopmentPlanForCourse GetDevelopmentPlanByTeamsCode(string DevelopmentCode);
        Task<int> UpdateDevelopmentPlan(Development development, int UserId);
        Task<List<DevelopmentCoursesDetails>> getCourseDetailsByDevelopmentId(int developmentId);
        Task<List<APIDevelopmentPlanType>> GetDevelopmentPlanAccessibility(string search);
        Task<List<Mappingparameter>> CheckmappingStatus(MappingParameters mappingParameters, int UserId);
        Task<List<MappingParameters>> GetAccessibilityRules(int DevelopmentPlanId, string orgnizationCode, string token, int Page, int PageSize);
        Task<int> GetAccessibilityRulesCount(int DevelopmentPlanId);
        Task<int> DeleteRule(int roleId);
        Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int DevelopmentPlanId);

        Task<string> GetDevelopmentPlanName(int DevelopmentPlanId);
        Task<List<APIAccessibilityRulesDevelopment>> GetAccessibilityRulesForExport(int DevelopmentPlanId, string orgnizationCode, string token, string DevelopmentPlanName);
        List<UserDevelopmentPlanMapping> GetRuleByUserTeams(int developmentPlanid);
        Task<List<DevelopmentPlanApplicableUser>> GetDevelopmentPlanApplicableUserList(int DevelopmentPlanId);
        FileInfo GetApplicableUserListExcel(List<APIAccessibilityRulesDevelopment> aPIAccessibilityRules, List<DevelopmentPlanApplicableUser> courseApplicableUsers, string DevelopmentPlanName, string OrgCode);
        Task<UserApplicableDevPlanTotal> GetUserDevPlan(int userId, int page, int pageSize, string search = null, int? devplanID = null);
        Task<List<DevPlanCoursesList>> GetDevPlanCourses(int DevPlanId, int UserId);
        Task<string> GetDevPlanNam(int? id);
        Task<DevelopmentPlanDetails> IsdevPlanCompleted(int id, int userid);
        Task<List<APICourseDTO>> GetDevelopementPlan(string search = null);
        Task<bool> GetDevPlanForSequence(int DevPlanId);

    }
}
