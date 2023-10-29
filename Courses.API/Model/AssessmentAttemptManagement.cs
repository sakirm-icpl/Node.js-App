using System.ComponentModel.DataAnnotations;
namespace Courses.API.Model
{
    public class AssessmentAttemptManagement : BaseModel
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
        public bool IsExhausted { get; set; }
        public int? ModuleId { get; set; }

    }
}
