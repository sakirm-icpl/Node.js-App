using Feedback.API.Helper;
using Feedback.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace Feedback.API.APIModel
{
    public class CourseFeedbackAPI
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string? TrainingType { get; set; }
        public int QuestionNumber { get; set; }
        [Required]
        [MaxLength(500)]
        public string? QuestionText { get; set; }
        [Required]
        [MaxLength(10)]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.ObjectiveFeedback, CommonValidation.subjective, CommonValidation.emoji })]
        public string? QuestionType { get; set; }
        public Option[]? Options { get; set; }
        public string? Section { get; set; }
        public int? SubjectiveAnswerLimit { get; set; }
        public bool Skip { get; set; }
        public int AnswersCounter { get; set; }
        public bool IsEmoji { get; set; }
        [MaxLength(20)]
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public int? ModuleId { get; set; }
        public int? NoOfOption { get; set; }
        public string? CourseTitle { get; set; }
        public string? Error { get; set; }
        public string? Option1 { get; set; }
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }
        public string? Option4 { get; set; }
        public string? Option5 { get; set; }

        public string? Option6 { get; set; }
        public string? Option7 { get; set; }
        public string? Option8 { get; set; }
        public string? Option9 { get; set; }
        public string? Option10 { get; set; }
        public bool IsSubjective { get; set; }
        public int? Rating { get; set; }
        public int optionSelector { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? UserName { get; set; }
        public int? CourseId { get; set; }
        [MaxLength(200)]
        public string? Metadata { get; set; }
    }
    public class CourseFeedbackAnsAPI
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string? TrainingType { get; set; }
        public int QuestionNumber { get; set; }
        [Required]
        [MaxLength(500)]
        public string? QuestionText { get; set; }
        [Required]
        [MaxLength(10)]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.ObjectiveFeedback, CommonValidation.subjective, CommonValidation.emoji })]
        public string? QuestionType { get; set; }
        public Option[]? Options { get; set; }
        public string? Section { get; set; }
        public int? SubjectiveAnswerLimit { get; set; }
        public bool Skip { get; set; }
        public int AnswersCounter { get; set; }
        public bool IsEmoji { get; set; }
        [MaxLength(20)]
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public int? ModuleId { get; set; }
        public int? NoOfOption { get; set; }
        public string? CourseTitle { get; set; }
        public string? Error { get; set; }
        public string? Option1 { get; set; }
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }
        public string? Option4 { get; set; }
        public string? Option5 { get; set; }
        public bool IsSubjective { get; set; }
        public int? Rating { get; set; }
        public int optionSelector { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? SelectedAns { get; set; }
        public int? EmojiAns { get; set; }
        public int? SelectedOptionId { get; set; }
    }


    public class Option
    {
        public int? Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? option { get; set; }
        [Range(0, 5, ErrorMessage = "Ratings must in the range of 0 to 5")]
        public int Rating { get; set; }
    }
    public class APIFeedbackFilePath
    {
        public string? Path { get; set; }
    }

    public class APIFQCourse
    {
        public string? Tilte { get; set; }
        public int CourseId { get; set; }
    }

    public class APICourseFeedbackSearch
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public string? ColumnName { get; set; }
        public bool showAllData { get; set; }
        public bool? IsEmoji { get; set; } = null;
    }
    public class FeedbackQueCountAPI
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string? QuestionText { get; set; }

    }

    public class UserFeedbackQueAPI
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string? QuestionText { get; set; }
        [Required]
        [MaxLength(10)]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.ObjectiveFeedback, CommonValidation.subjective, CommonValidation.emoji })]
        public string? QuestionType { get; set; }
        public int? SubjectiveAnswerLimit { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmoji { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? UserName { get; set; }

        public int? NoOfOption { get; set; }
        public int CreatedBy { get; set; }

        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
        public bool UserCreated { get; set; }
        public string? Metadata { get; set; }
        public string? CourseTitle { get; set; }

    }

    public class UserFeedbackQueTotalAPI
    {
        public List<UserFeedbackQueAPI>? Data { get; set; }
        public int TotalRecords { get; set; }
    }
    public class APIDistinctFieldId
    {
        public int FieldId { get; set; }
    }
}
