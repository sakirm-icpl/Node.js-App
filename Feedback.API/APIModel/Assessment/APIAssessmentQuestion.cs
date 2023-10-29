using Courses.API.APIModel;

using Feedback.API.Helper;
using Feedback.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace Assessment.API.APIModel
{
    public class APIAssessmentOptions
    {
        public int AssessmentOptionID { get; set; }
        public string? OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public string? UploadImage { get; set; }
    }



    public class APIAssessmentQuestion
    {
        public int Id { get; set; }
        public string? Section { get; set; }
        public string? LearnerInstruction { get; set; }
        public int SubjectiveAnswerLimit { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? QuestionText { get; set; }
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.Simple, CommonValidation.Difficult, CommonValidation.Tough })]
        public string? DifficultyLevel { get; set; }

        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.subjective, CommonValidation.MultipleSelection, CommonValidation.SingleSelection })]
        public string? OptionType { get; set; }
        public string? ModelAnswer { get; set; }
        public string? MediaFile { get; set; }
        public bool RandomizedQuestion { get; set; }
        public string? AnswerAsImages { get; set; }
        [Range(1, 10, ErrorMessage = "Marks must in the range of 1 to 25")]
        public int Marks { get; set; }
        public bool Status { get; set; }
        [Range(0, 5, ErrorMessage = "Options must in the range of 2 to 5")]
        public int Options { get; set; }
        public int CorrectOptionID { get; set; }
        public string? QuestionStyle { get; set; }
        public int? SequenceNumber { get; set; }
        public string? QuestionType { get; set; }
        public string? Metadata { get; set; }
        public bool IsMemoQuestion { get; set; }
        [MaxLength(20)]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.subjective, CommonValidation.Objective, CommonValidation.Image, CommonValidation.ImageText, CommonValidation.TextAudio, CommonValidation.TextVideo })]
        public string? ContentType { get; set; }
        public string? ContentPath { get; set; }
        public AssessmentOptions[]? aPIassessmentOptions { get; set; }
        public double? NegativeMarkingPercentage { get; set; }
        public APIJobRole[]? CompetencySkillsData { get; set; }
        public int? CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public int? PostAssessmentId { get; set; }
    }


    public class APIGetAssessmentQuestionOptions
    {
        public string? LearnerInstruction { get; set; }
        public string? DifficultyLevel { get; set; }
        public int Marks { get; set; }
        public string? ModelAnswer { get; set; }
        [Required]
        [MaxLength(500)]
        public string? QuestionText { get; set; }
        public string? AnswerAsImages { get; set; }
        public string? QuestionStyle { get; set; }
        public string? QuestionType { get; set; }
        public string? Section { get; set; }
        public string? ContentType { get; set; }
        public string? ContentPath { get; set; }
        public string? Metadata { get; set; }
        public bool Status { get; set; }
        public string? OptionType { get; set; }
        public int QuestionId { get; set; }
        public int OptionsId { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public string? OptionText { get; set; }
        public string? OptionContentType { get; set; }
        public string? OptionContentPath { get; set; }
        public int QId { get; set; }
        public bool IsFixed { get; set; }
        public int NoOfQuestionsToShow { get; set; }

    }
    public class APIAssessmentFilePath
    {
        public string? Path { get; set; }

        public int totalRecordInsert { get; set; }

        public int totalRecordRejected { get; set; }
    }

    public class APIAssessmentQuestionConfiguration
    {
        public int Id { get; set; }
        public string? Question { get; set; }
        public int QuestionID { get; set; }
        public string? OptionType { get; set; }
        public string? Section { get; set; }
        public int passingPercentage { get; set; }
        public int Duration { get; set; }
        public string? QuestionType { get; set; }
        public int maximumNoOfAttempts { get; set; }
        public string? MetaData { get; set; }
        public string? Name { get; set; }
        public int Marks { get; set; }
        public bool? IsFixed { get; set; }
        public bool? IsRandomQuestion { get; set; }
        public int? NoOfQuestionsToShow { get; set; }
        public bool? IsNegativeMarking { get; set; }
        public bool? IsEvaluationBySME { get; set; }
        public double? NegativeMarkingPercentage { get; set; }

    }

    public class APIAdaptiveAssessmentQuestion
    {
        public int Id { get; set; }
        public string? Section { get; set; }
        public string? LearnerInstruction { get; set; }
        public int SubjectiveAnswerLimit { get; set; }
        public string? QuestionText { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? OptionType { get; set; }
        public string? ModelAnswer { get; set; }
        public string? MediaFile { get; set; }
        public bool RandomizedQuestion { get; set; }
        public string? AnswerAsImages { get; set; }
        public int Marks { get; set; }
        public bool Status { get; set; }
        public int Options { get; set; }
        public string? QuestionStyle { get; set; }
        public string? QuestionType { get; set; }
        public string? Metadata { get; set; }
        public bool IsMemoQuestion { get; set; }
        public int? CourseId { get; set; }
        public int? ModuleId { get; set; }
        public string? ContentType { get; set; }
        public string? ContentPath { get; set; }
        public AssessmentOptions[]? aPIassessmentOptions { get; set; }
    }

    public class APIAssessmentQuestionSearch
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public string? ColumnName { get; set; }
        public bool? IsMemoQuestions { get; set; }
        public bool showAllData { get; set; }
    }

    public class APISubjectiveAQReview
    {
        public int Id { get; set; }
        public int AssessmentQuestionDetailsId { get; set; }
        public string? QuestionText { get; set; }
        public int Marks { get; set; }
        public string? SelectedAnswer { get; set; }
        public int? ObtainedMarks { get; set; }
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }


    }
    public class APITrainingReommendationNeeds
    {

        public string? JobRole { get; set; }


        public string? Department { get; set; }

        public string? Section { get; set; }

        public string? Level { get; set; }

        public string? Status { get; set; }

        public string? TrainingProgram { get; set; }

        public string? Category { get; set; }
        public string? Year { get; set; }
        public string? CourseName { get; set; }
        public string? ModuleName { get; set; }

    }

}
