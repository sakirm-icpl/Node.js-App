using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class ApiAssessmentAttemptManagement
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string CourseName { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public int AdditionalAttempts { get; set; }
        [Range(0, 100, ErrorMessage = "Additional Attempts must be between 1 to 100")]
        public bool IsExhausted { get; set; }
        public int ModuleId { get; set; }
    }
}
