using Courses.API.Model;

namespace Feedback.API.Model
{
    public class FeedbackStatusDetail : BaseModel
    {
        public int Id { get; set; }
        public int FeedbackStatusID { get; set; }
        public int FeedBackQuestionID { get; set; }
        public int? FeedBackOptionID { get; set; }
        public string SubjectiveAnswer { get; set; }
        public int? Rating { get; set; }
    }
}
