using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model.Feedback
{
    [Table("UserWiseFeedbackAggregation", Schema = "Course")]

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
