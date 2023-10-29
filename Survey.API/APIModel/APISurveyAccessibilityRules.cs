using Survey.API.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APISurveyAccessibilityRules
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int SurveyManagementId { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue1 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId1 { get; set; }
        [MaxLength(5)]
        [CommonValidationAttribute(AllowValue = new string[] { "AND", "OR" })]
        public string Condition1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue2 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId2 { get; set; }
        public string ErrorMessage { get; set; }
    }
}
