namespace Courses.API.APIModel.Assessment
{
    public class APIAssessmentPostResults
    {
        public string AssessmentResult { get; set; }
        public string PostAssessmentStatus { get; set; }
        public double MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public float Percentage { get; set; }
        public int InsertedID { get; set; }
    }
}
