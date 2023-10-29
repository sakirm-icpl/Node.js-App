namespace Courses.API.Model.Feedback
{
    public class UserWiseFeedbackAggregation
    {
        public int ID { get; set; }
        public int CourseId { get; set; }
        public int FeedbackQuestionID { get; set; }
        public int? FeedbackOptionID { get; set; }
        public bool IsDeleted { get; set; }
        public int CourseRating { get; set; }
    }
}
