
using Courses.API.Model;
using System.ComponentModel.DataAnnotations;

namespace Feedback.API.Model
{
    public class FeedbackStatus : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        [Required]
        [MaxLength(20)]
        public string Status { get; set; }
        public int? DPId { get; set; }
        public bool IsOJT { get; set; } = false;

    }
}
