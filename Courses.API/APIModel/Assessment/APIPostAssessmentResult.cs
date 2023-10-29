namespace Assessment.API.APIModel
{
    public class APIPostAssessmentResult
    {
        public int Id { get; set; }
        public int AssessmentHeaderID { get; set; }
        public int CourseID { get; set; }
        public int NoOfAttempts { get; set; }
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public decimal AssessmentPercentage { get; set; }
        public string AssessmentResult { get; set; }
        public int TotalNoOfQuestions { get; set; }
        public string PostAssessmentStatus { get; set; }
        public string Section { get; set; }
        public APIPostQuestionDetails[] aPIPostQuestionDetails { get; set; }
        public bool IsPreAssessment { get; set; }
    }

    public class APIPostQuestionDetails
    {
        public int Id { get; set; }
        public int? ReferenceQuestionID { get; set; }
        public double? Marks { get; set; }
        public int?[] OptionAnswerId { get; set; }
        public string SelectedAnswer { get; set; }
        public string OptionType { get; set; }

    }
    public class APIAssessmentResultHeader
    {
        public string PostAssessmentStatus { get; set; }
        public int NoOfAttempts { get; set; }
        public double MarksObtained { get; set; }       
        public decimal AssessmentPercentage { get; set; }
        public string AssessmentResult { get; set; }
        public bool? IsReviewedBySME { get; set; }
    }

    public class APIMultipleAnswer
    {
        public int[] AnswersID { get; set; }
    }

    public class APIPostAssessmentQuestionResult
    {
        public int Id { get; set; }
        public int NoOfAttempts { get; set; }
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public decimal AssessmentPercentage { get; set; }
        public string AssessmentResult { get; set; }
        public int TotalNoOfQuestions { get; set; }
        public string PostAssessmentStatus { get; set; }
        public string Section { get; set; }
        public int CourseID { get; set; }
        public int? ModuleID { get; set; }
        public int AssessmentSheetConfigID { get; set; }
        public APIPostQuestionDetails[] aPIPostQuestionDetails { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsContentAssessment { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        public bool? IsEvaluationBySME { get; set; }
        public int DurationInSeconds { get; set; }
    }


    public class APIPostManagerEvaluationResult
    {
        public int Id { get; set; }
        public int NoOfAttempts { get; set; }
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public decimal AssessmentPercentage { get; set; }
        public string AssessmentResult { get; set; }
        public int TotalNoOfQuestions { get; set; }
        public string PostAssessmentStatus { get; set; }
        public string Section { get; set; }
        public int CourseID { get; set; }
        public int? ModuleID { get; set; }
        public int AssessmentSheetConfigID { get; set; }
        public APIPostQuestionDetails[] aPIPostQuestionDetails { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsContentAssessment { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        public bool? IsEvaluationBySME { get; set; }
        public bool IsManagerEvaluation { get; set; }
        public int UserId { get; set; }
    }

    public class APIPostAdaptiveAssessment
    {
        public int Id { get; set; }
        public string Section { get; set; }
        public APIPostAdaptiveQuestionDetails[] aPIPostQuestionDetails { get; set; }
    }
    public class APIPostAdaptiveQuestionDetails
    {
        public int Id { get; set; }
        public int? ReferenceQuestionID { get; set; }
        public int?[] OptionAnswerId { get; set; }
        public string OptionType { get; set; }
        public int CourseID { get; set; }
        public int? ModuleID { get; set; }
    }
    public class APIStartAssessment
    {
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public int AssessmentSheetConfigID { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsContentAssessment { get; set; }
        public bool IsAdaptiveLearning { get; set; }
    }

    public class APIStartManagerEvaluation
    {
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public int AssessmentSheetConfigID { get; set; }
     
    }

    public class APIDeleteAssessmentQuestion
    {

        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string ErrMessage { get; set; }
        public int totalRecordInsert { get; set; }
        public int totalRecordRejected { get; set; }

    }

    public class APIPostSubjectiveReview
    {
        public int PostAssessmentResultId { get; set; }
        public APISubjectiveAQReview[] aPIPostQuestionDetails { get; set; }
        public bool IsContentAssessment { get; set; }

    }

    public class APISubjectiveAssessmentToCheck
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string ColumnName { get; set; }

    }

    public class APIAssessmentDataForReview
    {
        public int TotalRecordCount { get; set; }
        public int CourseID { get; set; }
        public int PostAssessmentResultID { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }

    }
    public class APIUserReportsTo
    {
        public string username { get; set; }
        public string reportsto { get; set; }
    }

}
