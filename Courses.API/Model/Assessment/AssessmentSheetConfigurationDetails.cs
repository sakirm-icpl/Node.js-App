namespace Assessment.API.Models
{
    public class AssessmentSheetConfigurationDetails : CommonFields
    {
        public int ID { get; set; }
        public int QuestionID { get; set; }
        public int AssessmentSheetConfigID { get; set; }
        public int? SequenceNumber { get; set; }
    }
}
