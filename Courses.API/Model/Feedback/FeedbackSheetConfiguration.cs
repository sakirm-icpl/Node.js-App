using Courses.API.Model;

namespace Feedback.API.Model
{
    public class FeedbackSheetConfiguration : BaseModel
    {
        public int Id { get; set; }
        public bool IsEmoji { get; set; }
    }
}
