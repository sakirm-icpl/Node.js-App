using Courses.API.Model;

namespace Feedback.API.Model
{
    public class FeedbackSheetConfigurationDetails : BaseModel
    {
        public int Id { get; set; }
        public int ConfigurationSheetId { get; set; }
        public int FeedbackId { get; set; }
        public int? SequenceNumber { get; set; }
    }
}
