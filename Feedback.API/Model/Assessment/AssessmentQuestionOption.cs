using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Models
{
    [Table("AssessmentQuestionOption", Schema = "Course")]

    public class AssessmentQuestionOption : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int QuestionID { get; set; }
        [MaxLength(500)]
        public string? OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        [MaxLength(500)]
        public string? UploadImage { get; set; }
        public string? ContentType { get; set; }
        public string? ContentPath { get; set; }
    }
}
