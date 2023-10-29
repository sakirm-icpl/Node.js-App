using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APISurveyAccessibility
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int SurveyManagementId { get; set; }
        public string GroupTemplateId { get; set; }
        public string GroupTemplateName { get; set; }
        public AccessibilitySurveyRules[] AccessibilityRule { get; set; }
    }

    public class AccessibilitySurveyRules
    {
        [MaxLength(50)]
        public string AccessibilityRule { get; set; }
        [MaxLength(50)]
        public string ParameterValue { get; set; }
        [MaxLength(5)]
        public string Condition { get; set; }
    }
}
