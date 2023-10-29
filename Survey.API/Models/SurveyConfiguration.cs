namespace Survey.API.Models
{
    public class SurveyConfiguration : CommonFields
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int LcmsId { get; set; }
        public int? IsRoot { get; set; }
        public int? SequenceNumber { get; set; }
    }
}
