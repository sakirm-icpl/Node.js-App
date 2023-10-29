using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.APIModel
{
    public class APITestResultQuestionWiseGraphModule
    {
        [Required]
        public int QuizId { get; set; }
        [Required]
        public int QuestionId { get; set; }
    }
}
