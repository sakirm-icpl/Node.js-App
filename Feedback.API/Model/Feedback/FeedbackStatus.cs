using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("FeedbackStatus", Schema = "Course")]

    public class FeedbackStatus : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        [Required]
        [MaxLength(20)]
        public string? Status { get; set; }
        public int? DPId { get; set; }
        public bool IsOJT { get; set; } = false;

    }
}
