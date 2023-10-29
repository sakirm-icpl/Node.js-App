using Feedback.API.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("TrainerFeedback", Schema = "Course")]
    public class TrainerFeedback : BaseModel
    {

        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        [Required]
        public string? Section { get; set; }
        [Required]
        [MaxLength(500)]
        public string? QuestionText { get; set; }
        [MaxLength(10)]
        public string? QuestionType { get; set; }
        [Required]
        public string? Option1 { get; set; }
        [Required]
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }
        public string? Option4 { get; set; }
        public string? Option5 { get; set; }
        public string? Limit { get; set; }
        [Required]
        public string? FeedbackLevel { get; set; }
        [Required]
        [MaxLength(50)]
        public string? SessionType { get; set; }
        public string? Counter { get; set; }
        [MaxLength(20)]
        public string? Status { get; set; }
    }
}
