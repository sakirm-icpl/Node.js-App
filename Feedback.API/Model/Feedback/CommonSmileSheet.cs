using Feedback.API.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("CommonSmileSheet", Schema = "Course")]
    public class CommonSmileSheet : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(10)]
        public string? QuestionNumber { get; set; }
        [Required]
        [MaxLength(20)]
        public string? Section { get; set; }
        [Required]
        public string? QuestionText { get; set; }
        [MaxLength(20)]
        public string? QuestionType { get; set; }
        [Required]
        public bool ShowEmoji { get; set; }
        [Required]
        public string? Answer1 { get; set; }
        [Required]
        public string? Answer2 { get; set; }
        public string? Answer3 { get; set; }
        public string? Answer4 { get; set; }
        public string? Answer5 { get; set; }
        public int LimitAnswer { get; set; }
        public bool Skip { get; set; }
        [Required]
        [MaxLength(20)]
        public string? FeedbackLevel { get; set; }
        [Required]
        [MaxLength(20)]
        public string? TrainingType { get; set; }
        public int AnswerCounter { get; set; }
        [MaxLength(20)]
        public string? Status { get; set; }
    }
}
