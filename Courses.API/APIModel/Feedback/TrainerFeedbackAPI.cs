using System.ComponentModel.DataAnnotations;

namespace Feedback.API.APIModel
{
    public class TrainerFeedbackAPI
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string TrainingType { get; set; }
        public int QuestionNumber { get; set; }
        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; }
        [MaxLength(10)]
        public string QuestionType { get; set; }
        [Required]
        public Option[] Options { get; set; }
        [Required]
        public string Section { get; set; }
        [Required]
        public string SessionType { get; set; }
        public int Limit { get; set; }
        public bool Skip { get; set; }
        public int AnsCount { get; set; }
        [MaxLength(20)]
        public string Status { get; set; }
        [Required]
        public string FeedbackLevel { get; set; }
    }
}
