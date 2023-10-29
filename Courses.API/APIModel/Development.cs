using Courses.API.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class Development
    {
        public string DevelopmentCode { get; set; }
        public string DevelopmentName { get; set; }
        public string? AboutPlan { get; set; }
        public bool? EnforceLinearApproach { get; set; }
        public bool? AllowLearningAfterExpiry { get; set; }
        public string? UploadThumbnail { get; set; }
        public int? CountOfMappedCourses { get; set; }
        public int? TargetCompletion { get; set; }
        public int? TotalCreditPoints { get; set; }
        public bool? Status { get; set; }
        public int? NumberOfRules { get; set; }
        public int? NumberofMembers { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool isIdp { get; set; } = false;
        public string Metadata { get; set; }
        public int? FeedbackId { get; set; }
        public DevelopmentPlanCourses[] developmentPlanCourses { get; set; } 
    }
    public class DevelopmentPlanForCourse
    {
        public int Id { get; set; }
        public string DevelopmentCode { get; set; }
        public string DevelopmentName { get; set; }
        public string? AboutPlan { get; set; }
        public bool? EnforceLinearApproach { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? AllowLearningAfterExpiry { get; set; }
        public string? UploadThumbnail { get; set; }
        public int? CountOfMappedCourses { get; set; }
        public int? TargetCompletion { get; set; }
        public int? TotalCreditPoints { get; set; }
        public int? NumberOfRules { get; set; }
        public int? NumberofMembers { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Metadata { get; set; }
        public int? FeedbackId { get; set; }
       
    }
    public class APIDevelopmentPlanForCourse
    {
        public int Id { get; set; }
        public string DevelopmentCode { get; set; }
        public string DevelopmentName { get; set; }
        public string? AboutPlan { get; set; }
        public bool? EnforceLinearApproach { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? AllowLearningAfterExpiry { get; set; }
        public string? UploadThumbnail { get; set; }
        public int? CountOfMappedCourses { get; set; }
        public int? TargetCompletion { get; set; }
        public int? TotalCreditPoints { get; set; }
        public int? NumberOfRules { get; set; }
        public int? NumberofMembers { get; set; }
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Metadata { get; set; }
        public string progressStatus { get; set; }
        public int? FeedbackId { get; set; }
        public string FeedbackName { get; set; }
    }
    public class DevelopmentPlanCourses
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int sequenceNo { get; set; }

    }
    public class CourseMappingToDevelopment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int sequenceNo { get; set; }
        public int DevelopmentPlanId { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
    public class DevelopmentPlanPage
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string ColumnName { get; set; }
        public string Search { get; set; }
        public bool isIdp { get; set; } = false;
    }
    public class DevelopmentPlanEU
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int? DevPlanID { get; set; }
        public string search { get; set; }
    }
   

public class DevelopmentCoursesDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public int TotalModules { get; set; }
        public int CompletionPeriodDays { get; set; }
        public float creditsPoints { get; set; }
        public int sequenceNo { get; set; }
    }

    public class APIDevelopmentPlanType
    {
        public int Id { get; set; }
        public string DevelopmentCode { get; set; }
        public string DevelopmentName { get; set; }
    }

    public class MappingParameters
    {
        public int Id { get; set; }
        public string AccessibilityParameter1 { get; set; }
        public string AccessibilityParameter2 { get; set; }
        public string AccessibilityValue1 { get; set; }
        public string AccessibilityValue2 { get; set; }
        public string condition1 { get; set; }
        public int DevelopmentPlanid { get; set; }
        public int AccessibilityValueId2 { get; set; }
        public int AccessibilityValueId1 { get; set; }
    }
    public class APIAccessibilityRulesDevelopment
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int DevelopmentPlanid { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue11 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId1 { get; set; }
        [MaxLength(5)]
        [CommonValidationAttribute(AllowValue = new string[] { "AND", "OR" })]
        public string Condition1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue22 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId2 { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class Mappingparameter
    {
        public int DevelopmentPlanid { get; set; }
        public string AccessibilityParameter1 { get; set; }
        public string ParameterValue1 { get; set; }
        public string AccessibilityParameter2 { get; set; }
        public string ParameterValue2 { get; set; }
    }

    public class UserDevelopmentPlanMapping
    {
        public int Id { get; set; }
        public int DevelopmentPlanid { get; set; }
        public string ConditionForRules { get; set; }
        public bool MappingStatus { get; set; }
        public int? UserID { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string? EmailID { get; set; }
        public string? MobileNumber { get; set; }
        public int? Business { get; set; }
        public int? Group { get; set; }
        public int? Area { get; set; }
        public int? Location { get; set; }
        public int? UserTeamId { get; set; }
        public int? ConfigurationColumn1 { get; set; }
        public int? ConfigurationColumn2 { get; set; }
        public int? ConfigurationColumn3 { get; set; }
        public int? ConfigurationColumn4 { get; set; }
        public int? ConfigurationColumn5 { get; set; }
        public int? ConfigurationColumn6 { get; set; }
        public int? ConfigurationColumn7 { get; set; }
        public int? ConfigurationColumn8 { get; set; }
        public int? ConfigurationColumn9 { get; set; }
        public int? ConfigurationColumn10 { get; set; }
        public int? ConfigurationColumn11 { get; set; }
        public int? ConfigurationColumn12 { get; set; }
        public int? ConfigurationColumn13 { get; set; }
        public int? ConfigurationColumn14 { get; set; }
        public int? ConfigurationColumn15 { get; set; }
    }

    public class DevelopmentPlanUserMapping
    {
        public int Id { get; set; }
        public int DevelopmentPlanId { get; set; }
        public int UserId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class DevelopmentId
    {
        public int developmentid { get; set; }
    }
    public class APIGetRulesFormapping
    {
        public int DevelopmentPlanId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class RulesForDevelopment
    {
        public int Id { get; set; }
        public string AccessibilityParameter { get; set; }

        public string AccessibilityValue { get; set; }

        public string AccessibilityValue2 { get; set; }

        public string Condition { get; set; }
    }
    public class APIGetRulesDevelopmentPlan
    {
        public int DevelopmentPlanId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
    public class DevelopmentPlanApplicableUser
    {
        public static int DevelopmentPlanid { get; internal set; }
        public string UserID { get; internal set; }
        public string UserName { get; internal set; }
    }
    public class UserApplicableDevPlanTotal
    {
        public int TotalRecords { get; set; }
        public List<UserApplicableDevPlan> data { get; set; }
    }
    public class UserApplicableDevPlan
    {     
        public int Id { get; set; }
        public string AboutPlan { get;  set; }
        public string DevelopmentName { get;  set; }
        public string DevelopmentCode { get;  set; }
        public string UploadThumbnail { get; set; }
        public string Status { get; set; }
        public int? FeedbackId { get; set; }
        public bool EnableFeedback { get; set; }
        public bool EnforceLinearApproach { get; set; }
    }
    public class DevPlanCoursesList
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }       
        public string ThumbnailPath { get; set; }
        public string Status { get; set; }
        public bool IsExternalProvider { get; set; }
        public string CourseURL { get; set; }
        public int sequenceNo { get; set; }

    }
    public class DevelopmentPlanCount
    {
        public int DevelopmentPlanId { get; set; }
    }
    public class DevelopmentPlanDetails
    {
        public bool DevelopmentPlanCompleted { get; set; }
        public string cdate { get; set; }

    }
}
