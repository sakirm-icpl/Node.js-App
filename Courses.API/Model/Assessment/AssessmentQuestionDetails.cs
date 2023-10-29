using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
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
        public string SelectedAnswer { get; set; }
        public bool IsCorrectAnswer { get; set; }
    }
}
