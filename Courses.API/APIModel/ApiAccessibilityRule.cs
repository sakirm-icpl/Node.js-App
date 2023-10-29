using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIAccessibility
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int CourseId { get; set; }
        public string GroupTemplateId { get; set; }
        public string GroupTemplateName { get; set; }
        public DateTime? EdCast_due_date { get; set; }
        public int? RetrainingDays { get; set; }
        public DateTime? EdCast_assigned_date { get; set; }
        public AccessibilityRules[] AccessibilityRule { get; set; }
    }

    public class APICategoryAccessibility
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }        
        public AccessibilityRules[] AccessibilityRule { get; set; }
    }

    public class AccessibilityRules
    {
        [MaxLength(50)]
        public string AccessibilityRule { get; set; }
        [MaxLength(50)]
        public string ParameterValue { get; set; }
        [MaxLength(50)]
        public string ParameterValue2 { get; set; }
        [MaxLength(5)]
        public string Condition { get; set; }
    }

    public class APIAccessibilityRuleFilePath
    {
        public string Path { get; set; }
        public int totalRecordInsert { get; set; }
        public int totalRecordRejected { get; set; }
        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
    }

    public class APIGetRules
    {
        public int courseId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class APIGetCourse
    {
        public int courseId { get; set; }
        public string CourseTitle { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
        [Required]
        public int CourseID { get; set; }
        [Required]
        public int UserID { get; set; }
        public string CourseCode { get; set; }
        public string DeviceNo { get; set; }
        public bool CourseStatus { get; set; }

    }
    public class APIGetCategoryRules
    {
        public int CategoryId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }

    public class UserGroupImportPayload
    {
        public string GroupName { get; set; }
        public string Path { get; set; }
    }

    public class ApiAccebilityRuleUserGroup
    {
        public int UserGroupId { get; set; }
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class APIScheduleVisibility
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int ScheduleId { get; set; }
        public string UserTeamId { get; set; }
        public string UserTeamName { get; set; }
        public visibilityRules[] AccessibilityRule { get; set; }
    }

    public class visibilityRules
    {
        [MaxLength(50)]
        public string AccessibilityRule { get; set; }
        [MaxLength(50)]
        public string ParameterValue { get; set; }
        [MaxLength(50)]
        public string ParameterValue2 { get; set; }
        [MaxLength(5)]
        public string Condition { get; set; }
    }

    public class APIGetScheduleRules
    {
        public int scheduleId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class APIGetScheduleRulesWithShowAll
    {
        public int scheduleId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public bool showAllData { get; set; }
    }
    public class APIGetScheduleDetails
    {
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public string ScheduleCode { get; set; }
    }

    public class ApiScheduleTeamPost
    {
        public int UserTeamId { get; set; }
        public int ScheduleId { get; set; }
    
    }

    public class APICourseApplicability
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public string search { get; set; }
        public string filter { get; set; }
        public bool showAllData { get; set; }
    }

}
