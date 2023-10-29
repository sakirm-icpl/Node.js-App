using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APIGetSurveyAccessibilityRule
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
        [MaxLength(5)]
        public string Condition { get; set; }
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

    public class ConfiguredColumns
    {
        public string ConfiguredColumnName { get; set; }
        public string ChangedColumnName { get; set; }
    }

    public class APIGetRules
    {
        public int SurveyManagementId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
}
