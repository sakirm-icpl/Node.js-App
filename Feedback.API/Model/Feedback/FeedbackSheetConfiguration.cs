using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("FeedbackSheetConfiguration", Schema = "Course")]

    public class FeedbackSheetConfiguration : BaseModel
    {
        public int Id { get; set; }
        public bool IsEmoji { get; set; }
    }
}
