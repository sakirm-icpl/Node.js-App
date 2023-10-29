using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.APIModel
{
    public class APIGetAccessibilityRule
    {
        [MaxLength(150)]
        public string CourseTitle { get; set; }
        public List<Rules> AccessibilityRules { get; set; }
    }

    public class Rules
    {
        [MaxLength(100)]
        public string AccessibilityParameter { get; set; }
        [MaxLength(100)]
        public string AccessibilityValue { get; set; }
        [MaxLength(100)]
        public string AccessibilityValue2 { get; set; }
        [MaxLength(5)]
        public string Condition { get; set; }
        public int CreatedBy { get; set; }
        public string UserName { get; set; }
    }

    public class Title
    {
        [MaxLength(150)]
        public string Name { get; set; }
    }
    public class xAPIUserDetails
    {
        [MaxLength(150)]
        public string Name { get; set; }
        public string EmailId { get; set; }
    }
    
}
