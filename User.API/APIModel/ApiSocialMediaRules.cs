using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using User.API.Validation;

namespace User.API.APIModel
{
    public class ApiSocialMediaRules
    {
        public string Path { get; set; }
        public int? Id { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue1 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId1 { get; set; }
        [MaxLength(5)]
        [/*CommonValidationAttribute*/CommonValidationAttribute(AllowValue = new string[] { "AND", "OR" })]
        public string Condition1 { get; set; }
        [MaxLength(50)]
        public string AccessibilityParameter2 { get; set; }
        [MaxLength(50)]
        public string AccessibilityValue2 { get; set; }
        [Range(0, int.MaxValue)]
        public int AccessibilityValueId2 { get; set; }
        public string ErrorMessage { get; set; }
        public string MsgTemplate { get; set; }
        public string Medium { get; set; }
        public string Subject { get; set; }
        public string EmployeeCode { get; set; }
    }
    public class CustNotificationRules
    {
        public string Medium { get; set; }
        public string Message { get; set; }
        public string EmployeeCode { get; set; }
        public string Stauts { get; set; }
        public string Subject { get; set; }
        public string AccessibilityParameter1 { get; set; }
        public string AccessibilityValue1 { get; set; }
    }
}
