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

}