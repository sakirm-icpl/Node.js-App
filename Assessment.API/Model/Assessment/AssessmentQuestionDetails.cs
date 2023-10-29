using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Models
{
    [Table("AssessmentQuestionDetails", Schema = "Course")]

    public class AssessmentQuestionDetails : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int AssessmentResultID { get; set; }
        [Required]
        public int ReferenceQuestionID { get; set; }
        public double? Marks { get; set; }
        public int? OptionAnswerId { get; set; }
        [MaxLength(500)]
        public string? SelectedAnswer { get; set; }
        public bool IsCorrectAnswer { get; set; }
    }
}
