using Courses.API.Model;
using System.ComponentModel.DataAnnotations;
namespace Feedback.API.Model
{
    public class FeedbackQuestionRejected : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string CourseCode { get; set; }
        [MaxLength(250)]
        public string Section { get; set; }
        [MaxLength(2000)]
        public string QuestionText { get; set; }
        [MaxLength(250)]
        public string QuestionType { get; set; }
        [MaxLength(250)]
        public string SubjectiveAnswerLimit { get; set; }
        [MaxLength(250)]
        public string IsAllowSkipping { get; set; }
        [MaxLength(250)]
        public string AnswersCounter { get; set; }
        [MaxLength(250)]
        public string NoOfOptions { get; set; }
        [MaxLength(250)]
        public string Option1 { get; set; }
        [MaxLength(250)]
        public string Option2 { get; set; }
        [MaxLength(250)]
        public string Option3 { get; set; }
        [MaxLength(250)]
        public string Option4 { get; set; }
        [MaxLength(250)]
        public string Option5 { get; set; }

        [MaxLength(250)]
        public string Option6 { get; set; }
        [MaxLength(250)]
        public string Option7 { get; set; }
        [MaxLength(250)]
        public string Option8 { get; set; }
        [MaxLength(250)]
        public string Option9 { get; set; }
        [MaxLength(250)]
        public string Option10 { get; set; }

        [MaxLength(2000)]
        public string ErrorMessage { get; set; }
        [MaxLength(200)]
        public string IsEmoji { get; set; }
        public string IsSubjective { get; set; }
        public string Metadata { get; set; }
    }
}
