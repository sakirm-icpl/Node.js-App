namespace MyCourse.API.APIModel
{
    public class APIAssessmentCompletionDetails
    {
        public string Result { get; set; }
        public int NoOfAttempts { get; set; }
        public float? Score { get; set; }
        public string CompletionStatus { get; set; }
    }
}
