using Courses.API.Model;
using System.ComponentModel.DataAnnotations;

namespace Feedback.API.Model
{
    public class FeedbackQuestion : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string Section { get; set; }
        [RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        public string QuestionText { get; set; }
        [MaxLength(20)]
        public string QuestionType { get; set; }
        public int? SubjectiveAnswerLimit { get; set; }
        public bool IsAllowSkipping { get; set; }
        public bool IsEmoji { get; set; }
        public bool IsSubjective { get; set; }
        public int AnswersCounter { get; set; }

        public int? CourseId { get; set; }
        [MaxLength(200)]
        public string Metadata { get; set; }
    }
}
