using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Models
{
    [Table("AssessmentQuestion", Schema = "Course")]

    public class AssessmentQuestion : CommonFields
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? OptionType { get; set; }
        [MaxLength(50)]
        public string? Section { get; set; }
        [MaxLength(200)]
        public string? LearnerInstruction { get; set; }
        [Required]
        [MaxLength(1000)]
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string? QuestionText { get; set; }

        [MaxLength(50)]
        public string? DifficultyLevel { get; set; }

        [MaxLength(500)]
        public string? ModelAnswer { get; set; }
        [MaxLength(200)]
        public string? MediaFile { get; set; }
        [MaxLength(50)]
        public string? AnswerAsImages { get; set; }
        [Required]
        public int Marks { get; set; }
        [Required]
        public bool Status { get; set; }
        [MaxLength(100)]
        public string? QuestionStyle { get; set; }
        [MaxLength(50)]
        public string? QuestionType { get; set; }
        [MaxLength(200)]
        public string? Metadata { get; set; }
        public bool IsMemoQuestion { get; set; }
        [MaxLength(20)]
        public string? ContentType { get; set; }
        public string? ContentPath { get; set; }
        public int? CourseId { get; set; }


    }

}
