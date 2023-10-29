using System.ComponentModel.DataAnnotations;

namespace Feedback.API.APIModel
{
    public class SubmitFeedback
    {
        [Required]
        public int QuestionID { get; set; }
        public int? OptionID { get; set; }
        public string SubjectiveAnswer { get; set; }
        [Required]
        public string QuestionType { get; set; }
       [Required]  
        public int CourseId { get; set; }
        [Required]
        public int ModuleId { get; set; }
        public int? Rating { get; set; }
        public int? DPId { get; set; }
        public bool IsOJT { get; set; } = false;
    }
    public class SubmitFinalFeedback
    {
       
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int ModuleId { get; set; }
       
    }

}
