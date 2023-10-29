using ILT.API.APIModel;
using ILT.API.Model;
using ILT.API.Model.ILT;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IScheduleVisibilityRule : IRepository<ScheduleVisibilityRule>
    {
        Task<List<object>> Get(int page, int pageSize, string search = null, string filter = null);
        Task<List<object>> GetSchedule(int page, int pageSize, string search = null, string filter = null);
        Task<int> Schedulecount(string search = null, string filter = null);
        Task<int> count(string search = null, string filter = null);
        Task<List<visibilityRules>> Post(APIScheduleVisibility apiAccessibility, int userId, string orgnizationCode = null, string Token = null);
        Task<int> DeleteRule(int roleId);
        Task<int> CourseCount(string search = null);
        //Task<List<APIScheduleVisibilityRules>> GetAccessibilityRules(int scheduleId, string orgnizationCode, string token, int Page, int PageSize);
        Task<int> GetAccessibilityRulesCount(int courseId);
    
       
        Task<bool> CheckValidData(string AccessibilityParameter1, string AccessibilityValue1, string AccessibilityParameter2, string AccessibilityValue2, int scheduleId, string AccessibilityValue11, string AccessibilityValue22);

        Task<List<CourseApplicableUser>> GetCourseApplicableUserList(int courseId);
        FileInfo GetApplicableUserListExcel(List<APIScheduleVisibilityRules> aPIAccessibilityRules, List<CourseApplicableUser> courseApplicableUsers, string CourseName, string ModuleNAme, string ScheduleCode, string OrgCode);
        Task<List<APIScheduleVisibilityRules>> GetAccessibilityRulesForExport(int courseId, string orgnizationCode, string token, string CourseName);

        Task<APIGetScheduleDetails> GetCourseModuleScheduleNames(int ScheduleId);
        Task<APITotalGetAllSchedule> GetSchedules(ApiGetSchedules apiGetSchedules, int userId, string userRole);
        Task<APIScheduleVisibilityRulesTotal> GetAccessibilityRules(int scheduleId, string orgnizationCode, string token, int Page, int PageSize, int userId, string userRole, bool showAllData);
    }
}
