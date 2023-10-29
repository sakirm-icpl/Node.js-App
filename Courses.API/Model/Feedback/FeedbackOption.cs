using Courses.API.Model;

namespace Feedback.API.Model
{
    public class FeedbackOption : BaseModel
    {
        public int Id { get; set; }
        public int FeedbackQuestionID { get; set; }
        public string OptionText { get; set; }
        public int Rating { get; set; }
    }
}
