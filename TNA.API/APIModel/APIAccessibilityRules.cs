using TNA.API.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TNA.API.APIModel
{
    public class APIAccessibilityRules
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int CourseId { get; set; }
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
        public DateTime? EdCast_due_date { get; set; }
        public DateTime? EdCast_assigned_date { get; set; }
        public string UserName { get; set; }

        public bool UserCreated { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class APICategoryAccessibilityRules
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue1 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId1 { get; set; }
        [MaxLength(5)]
        [CommonValidationAttribute(AllowValue = new string[] { "AND", "OR" })]
        public string Condition1 { get; set; }
        [MaxLength(5)]
        [CommonValidationAttribute(AllowValue = new string[] { "AND", "OR" })]
        public string Condition2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue2 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter3 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue3 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId3 { get; set; }
        public string ErrorMessage { get; set; }
        public string CategoryCode { get; set; }
        public string SubCategoryCode { get; set; }
    }


    public class APIScheduleVisibilityRules
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int ScheduleId { get; set; }
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

    public class APIScheduleVisibilityRulesWithUserdata
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int ScheduleId { get; set; }
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
        public string? UserName { get; set; }
        public int? CreatedBy { get; set; }
        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
        public bool? UserCreated { get; set; }
    }

    public class APIScheduleVisibilityRulesTotal
    {
        public List<APIScheduleVisibilityRulesWithUserdata> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
