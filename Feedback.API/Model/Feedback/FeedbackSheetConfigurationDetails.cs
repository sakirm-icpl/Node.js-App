using System.ComponentModel.DataAnnotations.Schema;

namespace Feedback.API.Model
{
    [Table("FeedbackSheetConfigurationDetails", Schema = "Course")]

    public class FeedbackSheetConfigurationDetails : BaseModel
    {
        public int Id { get; set; }
        public int ConfigurationSheetId { get; set; }
        public int FeedbackId { get; set; }
        public int? SequenceNumber { get; set; }
    }
}
