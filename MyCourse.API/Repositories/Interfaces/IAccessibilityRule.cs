using MyCourse.API.APIModel;
//using Courses.API.ExternalIntegration.EdCast;
using MyCourse.API.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IAccessibilityRule : IRepository<AccessibilityRule>
    {
        //Task<bool> Exists(string name);
        //Task<List<object>> Get(int page, int pageSize, string search = null, string filter = null);
        //Task<int> count(string search = null, string filter = null);
        //Task<List<AccessibilityRules>> Post(APIAccessibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null);
        Task<List<AccessibilityRules>> SelfEnroll(APIAccessibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null);
        Task<List<AccessibilityRules>> DbSelfEnroll(APIAccessibility apiAccessibility, int userId, string connectionString = null);
       // Task<List<Object>> GetRules(int userId);
       // Task<List<string>> GetCourseName(int page, int pageSize, string search = null);
       // Task<int> CourseCount(string search = null);
       // Task<List<APIAccessibilityRules>> GetAccessibilityRules(int courseId, string orgnizationCode, string token, int Page, int PageSize);
       // Task<int> GetAccessibilityRulesCount(int courseId);
       // Task<int> UpdateRule(APIAccessibilityRules apiAccessibilityRules, int id);
       // Task<APIAccessibilityRules> GetAccessibilityRule(int ruleId, string orgnizationCode);
       // Task<int> DeleteRule(int roleId);
       // Task<AccessibilityRule> GetAccessibility(int id);      
       // Task<ApiResponse> ProcessImportFile(FileInfo file,  ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode);
       // Task<ApiResponse> ProcessGroupImportFile(FileInfo file,  ICustomerConnectionStringRepository _customerConnectionRepository, int userid, string GroupName, IConfiguration _configuration, string orgcode);
       // Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int courseid, string AccessibilityValue11, string AccessibilityValue22);

       // Task<bool> CheckValidDatacategory(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, string AccessibilityParameter3, string AccessibilityValue3, int CategoryId, int? Subcategoryid);
       // Task<bool> CheckValidDataForUserGroup(ApiAccebilityRuleUserGroup accebilityRuleUserGroup);
       // Task<List<CourseApplicableUser>> GetCourseApplicableUserList(int courseId);
       // FileInfo GetApplicableUserListExcel(List<APIAccessibilityRules> aPIAccessibilityRules, List<CourseApplicableUser> courseApplicableUsers, string CourseName, string OrgCode);
       // Task<List<APIAccessibilityRules>> GetAccessibilityRulesForExport(int courseId, string orgnizationCode, string token, string CourseName);

       // Task<string> GetCourseNames(int courseId);
       // Task<List<ApiNotification>> GetCountByCourseIdAndUserId(int Url);
        Task<int> SendNotificationCourseApplicability(ApiNotification apiNotification, bool IsApplicabletoall);
        Task  SendDataForApplicableNotifications(int notificationId, DataTable dtUserIDs, int createdBy);
       // Task<List<AccessibilityRules>> PostCategory(APICategoryAccessibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null);
       // Task<List<object>> GetCategoryData(int page, int pageSize, string search = null, string filter = null);
       // Task<int> categorycount(string search = null, string filter = null);
       // Task<List<APICategoryAccessibilityRules>> GetCategoryAccessibilityRules(int CategoryId, string orgnizationCode, string token, int Page, int PageSize);
       // Task<int> GetCategoryAccessibilityRulesCount(int categoryid);
       // Task<List<UserGroup>> GetAllUserGroups();
       // Task<int> DeleteCategoryRule(int roleId);
        //Task<bool> CategoryRuleExist(AccessibilityRule accessibilityRule);
       // Task<bool> GroupNameExists(string GroupName);
       // Task<string> GetCategoryNames(int categoryId);
       // Task<List<APICategoryAccessibilityRules>> GetCategoryAccessibilityRulesForExport(int CategoryId, string orgnizationCode, string token, string CategoryName);
       // Task<List<CategoryApplicableUser>> GetCategoryApplicableUserList(int categoryid);
       // FileInfo GetCategoryApplicableUserListExcel(List<APICategoryAccessibilityRules> aPIAccessibilityRules, List<CategoryApplicableUser> courseApplicableUsers, string CategoryName, string OrgCode);
       // Task<Category> GetCategoryId(string categoryName);
       // Task<APISubCategory> GetSubCategoryId(string categoryName, int Id);
       //Task<ApiResponse> ProcessImportCategory(FileInfo file, ICourseRepository courseRepository, IAccessibilityRule _accessibilityRule, IAccessibilityRuleRejectedRepository _IAccessibilityRuleRejected, ICustomerConnectionStringRepository _customerConnectionRepository, int userid, IConfiguration _configuration, string orgcode);
       // Task<bool> GetAccessibilityRulesForCategory(int categoryId, int? subcategoryId, int? config2, int? config3, int? UserId, int? LanguageId);
       // Task<AccessibilityRule> GetAccessibilityRules1(int Id);
       // Task<Category> GetCategoryNameById(int Id);

       // Task<SubCategory> GetSubCategoryNameById(int Id);
        Task<bool> RuleExist(AccessibilityRule accessibilityRule);
       // List<AccessibilityRule> GetRuleByUserTeams(int courseId);
        List<CourseApplicableUser> GetUsersForUserTeam(int? Id);
       // Task<APIEdCastTransactionDetails> CourseAssignment(int courseID, int userId, string status, string assignedDate = null,string dueDate = null, string token = null, string LxpDetails=null);
       // Task<ApplicabilityTotalAPI> GetV2(APICourseApplicability aPICourseApplicability, int userId, string userRole);
       // Task<List<APIAccessibilityRules>> GetAccessibilityRulesV2(int courseId, string orgnizationCode, string token, int Page, int PageSize, int userId, string userRole);
        Task<string> GetMasterConfigurableParameterValue(string configurationCode);
    }
}
