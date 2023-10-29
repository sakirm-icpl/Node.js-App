using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseApplicability.API.Model
{
    [Table("Course", Schema = "Course")]
    public class Course : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(30)]
        [Required]
        public string Code { get; set; }
        [MaxLength(150)]
        [Required]
        public string Title { get; set; }

        [MaxLength(30)]
        [Required]
        public string CourseType { get; set; }
        public string Description { get; set; }

        public int? CourseAdminID { get; set; }
        [MaxLength(15)]
        public bool? LearningApproach { get; set; }
        [MaxLength(40)]
        public string Language { get; set; }
        public bool IsCertificateIssued { get; set; }
        public int CompletionPeriodDays { get; set; }
        public float CourseFee { get; set; }
        [MaxLength(25)]
        public string Currency { get; set; }
        public float CreditsPoints { get; set; }
        [MaxLength(200)]
        public string ThumbnailPath { get; set; }
        public int? CategoryId { get; set; }
        public bool IsPreAssessment { get; set; }
        public int? PreAssessmentId { get; set; }
        public bool IsAssessment { get; set; }
        public int? AssessmentId { get; set; }
        public bool IsFeedback { get; set; }
        public int? FeedbackId { get; set; }
        public bool IsAssignment { get; set; }
        public int? AssignmentId { get; set; }
        public bool IsApplicableToAll { get; set; }
        public bool IsApplicableToExternal { get; set; }
        public string AdminName { get; set; }
        public int? SubCategoryId { get; set; }
        [MaxLength(500)]
        public string Metadata { get; set; }
        public bool IsSection { get; set; }
        public bool IsDiscussionBoard { get; set; }
        public int? MemoId { get; set; }
        public bool IsMemoCourse { get; set; }
        public string Mission { get; set; }
        public int Points { get; set; }
        public bool IsAchieveMastery { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        public int DurationInMinutes { get; set; }
        public int TotalModules { get; set; }

        public bool IsShowInCatalogue { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RowGuid { get; set; }

        public bool IsModuleHasAssFeed { get; set; }

        public bool IsManagerEvaluation { get; set; }
        public int? ManagerEvaluationId { get; set; }
        public int? prerequisiteCourse { get; set; }
        public bool IsExternalProvider { get; set; }
        public string ExternalProvider { get; set; }
        public string CourseURL { get; set; }
        public int? noOfDays { get; set; }
        public bool IsRetraining { get; set; }
        public bool IsFeedbackOptional { get; set; }
        public float GroupCourseFee { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CanAutoActivated { get; set; }
        public bool IsVisibleAfterExpiry { get; set; }
        public bool? IsDashboardCourse { get; set; }
        public int? isAssessmentReview { get; set; }
        public bool? PublishCourse { get; set; }
        public string LxpDetails { get; set; } //glint , glintplus
        public bool? IsPrivateContent { get; set; }
        public int? VendorId { get; set; }
        public bool IsOJT { get; set; }
        public int? OJTId { get; set; }
        [DefaultValue("true")]
        public bool isVisibleAssessmentDetails { get; set; }
        public string? CourseLevel { get; set; }

    }
    public class SkillSoft
    {
        public int ID { get; set; }
        public string Token { get; set; }
        public string OrgId { get; set; }
    }
    public class AssessmentReview
    {
        public Boolean isAssessmentReview { get; set; }
    }
    public class CourseDetails : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(30)]
        [Required]
        public string Code { get; set; }
        [MaxLength(150)]
        [Required]
        public string Title { get; set; }

        [MaxLength(30)]
        [Required]
        public string CourseType { get; set; }
        public string Description { get; set; }

        public int? CourseAdminID { get; set; }
        [MaxLength(15)]
        public bool? LearningApproach { get; set; }
        [MaxLength(40)]
        public string Language { get; set; }
        public bool IsCertificateIssued { get; set; }
        public int CompletionPeriodDays { get; set; }
        public float CourseFee { get; set; }
        [MaxLength(25)]
        public string Currency { get; set; }
        public float CreditsPoints { get; set; }
        [MaxLength(200)]
        public string ThumbnailPath { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsPreAssessment { get; set; }
        public int? PreAssessmentId { get; set; }
        public bool IsAssessment { get; set; }
        public int? AssessmentId { get; set; }
        public bool IsFeedback { get; set; }
        public int? FeedbackId { get; set; }
        public bool IsAssignment { get; set; }
        public int? AssignmentId { get; set; }
        public bool IsApplicableToAll { get; set; }
        public string AdminName { get; set; }
        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        [MaxLength(500)]
        public string Metadata { get; set; }
        public bool IsSection { get; set; }
        public bool IsDiscussionBoard { get; set; }
        public int? MemoId { get; set; }
        public bool IsMemoCourse { get; set; }
        public string Mission { get; set; }
        public int Points { get; set; }
        public bool IsAchieveMastery { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        public int DurationInMinutes { get; set; }
        public int TotalModules { get; set; }

        public bool IsShowInCatalogue { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RowGuid { get; set; }

        public bool IsModuleHasAssFeed { get; set; }

        public bool IsManagerEvaluation { get; set; }
        public int? ManagerEvaluationId { get; set; }
        public int? prerequisiteCourse { get; set; }
        public bool IsExternalProvider { get; set; }
        public string ExternalProvider { get; set; }
        public string CourseURL { get; set; }
        public int? noOfDays { get; set; }
        public bool IsRetraining { get; set; }
        public bool IsFeedbackOptional { get; set; }
        public float GroupCourseFee { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CanAutoActivated { get; set; }
        public bool IsVisibleAfterExpiry { get; set; }
        public bool? IsDashboardCourse { get; set; }
        public int? isAssessmentReview { get; set; }
        public bool? PublishCourse { get; set; }
        public bool? IsPrivateContent { get; set; }
    }
}
