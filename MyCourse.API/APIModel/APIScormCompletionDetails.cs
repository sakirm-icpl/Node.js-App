namespace MyCourse.API.APIModel
{
    public class APIScormCompletionDetails
    {
        public string Result { get; set; }
        public int NoOfAttempts { get; set; }
        public float? Score { get; set; }
        public string SessionTime { get; set; }
        public string CompletionStatus { get; set; }
        public APIAssessmentCompletionDetails AssessmentDetails { get; set; }
    }
}
