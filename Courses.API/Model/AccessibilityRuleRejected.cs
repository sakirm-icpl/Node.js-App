using Assessment.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class AccessibilityRuleRejected : CommonFields
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public string CourseCode { get; set; }
        [MinLength(2), MaxLength(10)]
        public string UserName { get; set; }
        [Required]
        public string ErrorMessage { get; set; }
    }
}
