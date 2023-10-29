using System.ComponentModel.DataAnnotations;

namespace Assessment.API.APIModel
{
    public class APIAssessmentHeader
    {
        public int Id { get; set; }
        [Required]
        public int CourseID { get; set; }
        public int ModuleID { get; set; }
        public int NumberOfQuestionMaintained { get; set; }
        public int NumberOfObjectiveQuestions { get; set; }
        public int NumberOfSubjectiveQuestions { get; set; }
        public int TotalMarkOfAllQuestions { get; set; }
        public int TotalMarksOfObjectiveQuestions { get; set; }
        public int TotalMarksOfSubjectiveQuestions { get; set; }
        public int NumberOfObjectiveQuestionsAsked { get; set; }
        public int NumberOfSubjectiveQuestionsAsked { get; set; }
        public bool RandomizedQuestion { get; set; }
        public bool AdaptiveAssessment { get; set; }
        public bool NegativeMarking { get; set; }
        public bool AllowSkipQuestion { get; set; }
        public int PassingScore { get; set; }
        public int NumberOfAttempts { get; set; }
        public bool IsPreAssessment { get; set; }
    }

    public class APIAssessmentQuestions
    {
        public int Id { get; set; }
        public int AssessmentHeaderID { get; set; }
        public string? Section { get; set; }
        public string? LearnerInstruction { get; set; }
        public int SubjectiveAnswerLimit { get; set; }
        public string? QuestionText { get; set; }
        public string? DifficultyLevel { get; set; }
        public AssessmentOptions[]? AssessmentOptions { get; set; }
        public string? Time { get; set; }
        public string? ModelAnswer { get; set; }
        public string? MediaFile { get; set; }
        public string? AnswerAsImages { get; set; }
        public int Marks { get; set; }
        public bool Status { get; set; }
        public bool RandomizedQuestion { get; set; }
    }

    public class AssessmentOptions
    {
        public int? AssessmentOptionID { get; set; }
        [Required]
        [MaxLength(300)]
        public string? OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public string? UploadImage { get; set; }
        public string? OptionContentType { get; set; }
        public string? OptionContentPath { get; set; }
        public double? NegativeMarkingPercentage { get; set; }
        public int QuestionId { get; set; } //Added - Rahul Aggarwal
    }

    public class AssessmentStatus
    {
        public int Id { get; set; }
        public int AssessmentStatusID { get; set; }
        public int AssessmentHeaderID { get; set; }
        public string? AssessmentsStatus { get; set; }
    }

    public class AssessmentStatusDetail
    {
        public int Id { get; set; }
        public int AssessmentStatusID { get; set; }
        public int AssessmentQuestionID { get; set; }
        public int AssessmentOptionID { get; set; }
        public string? SubjectiveAnswer { get; set; }
    }

    public class AssessmentDetails
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public string? CourseTitle { get; set; }
        public int ModuleID { get; set; }
        public int NumberOfQuestionMaintained { get; set; }
        public int NumberOfObjectiveQuestions { get; set; }
        public int NumberOfSubjectiveQuestions { get; set; }
        public int TotalMarkOfAllQuestions { get; set; }
        public int TotalMarksOfObjectiveQuestions { get; set; }
        public int TotalMarksOfSubjectiveQuestions { get; set; }
        public int NumberOfObjectiveQuestionsAsked { get; set; }
        public int NumberOfSubjectiveQuestionsAsked { get; set; }
        public bool RandomizedQuestion { get; set; }
        public bool AdaptiveAssessment { get; set; }
        public bool NegativeMarking { get; set; }
        public bool AllowSkipQuestion { get; set; }
        public int PassingScore { get; set; }
        public int NumberOfAttempts { get; set; }
        public bool IsPreAssessment { get; set; }

    }
}
