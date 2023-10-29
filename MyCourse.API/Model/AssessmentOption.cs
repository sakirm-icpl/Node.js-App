using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class AssessmentOption : CommonFields
    {
        public int AssessmentOptionID { get; set; }
        [Required]
        public int AssessmentQuestionID { get; set; }
        [MaxLength(500)]
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        [MaxLength(500)]
        public string UploadImage { get; set; }
    }
}
