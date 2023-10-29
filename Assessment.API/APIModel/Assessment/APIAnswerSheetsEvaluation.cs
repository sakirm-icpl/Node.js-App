namespace Assessment.API.APIModel
{
    public class APIAnswerSheetsEvaluation
    {
        public int? Id { get; set; }
        public int AnswerSheetId { get; set; }
        public int QuestionId { get; set; }
        public int Marks { get; set; }
        public string? Remarks { get; set; }
    }
    public class APISubjectiveAssessmentStatus
    {
        public int Id { get; set; }
        public int AssessmentResultID { get; set; }
        public int HeaderID { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public int CheckerID { get; set; }
        public string? Status { get; set; }
    }

    public class APIPostAssessmentSubjectiveResult
    {
        public int Id { get; set; }
        public int AssessmentHeaderID { get; set; }
        public int CourseID { get; set; }
        public string? CourseName { get; set; }
        public int NoOfAttempts { get; set; }
        public double? MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public decimal AssessmentPercentage { get; set; }
        public string? AssessmentResult { get; set; }
        public int TotalNoOfQuestions { get; set; }
        public string? PostAssessmentStatus { get; set; }
        public string? Section { get; set; }
        public string? Grade { get; set; }
    }

    public class APIAssessmentSubjectiveQuestion
    {
        public int Id { get; set; }
        public int ReferenceQuestionID { get; set; }
        public int Marks { get; set; }
        public int OptionAnswerId { get; set; }
        public string? SelectedAnswer { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public string? Question { get; set; }
        public string? MediaFile { get; set; }
        public string? ModelAnswer { get; set; }
        public int AssignedMarks { get; set; }
        public int GivenMarks { get; set; }
        public string? Remarks { get; set; }
        public int AnswerSheetId { get; set; }
    }

    public class APIPostSubjectiveAssessmentResultMerged
    {
        public int AssessmentId { get; set; }
        public int AssessmentResultID { get; set; }
        public int HeaderID { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public int CheckerID { get; set; }
        public string? Status { get; set; }
        public int TotalMarks { get; set; }
        public decimal AssessmentPercentage { get; set; }
        public string? PostAssessmentStatus { get; set; }
        public APIAssessmentSubjectiveQuestion[]? aPIAssessmentSubjectiveQuestion { get; set; }
    }
}
