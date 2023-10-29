using Evaluation.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Evaluation.API.Model
{
    public class ProcessEvaluationOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public int RefQuestionID { get; set; }

    }

    public class OEREvaluationOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public int RefQuestionID { get; set; }

    }
    public class CriticalAuditOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public int RefQuestionID { get; set; }

    }

    public class NightAuditOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public int RefQuestionID { get; set; }

    }

    public class OpsAuditOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public int RefQuestionID { get; set; }

    }
}
