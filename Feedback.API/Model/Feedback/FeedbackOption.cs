using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("FeedbackOption", Schema = "Course")]

    public class FeedbackOption : BaseModel
    {
        public int Id { get; set; }
        public int FeedbackQuestionID { get; set; }
        public string? OptionText { get; set; }
        public int Rating { get; set; }
    }
}
