using Courses.API.Helper;
using Courses.API.Validations;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Courses.API.APIModel
{

    public class APICourse
    {
        public int Id { get; set; }
        [MaxLength(500)]
        [Required]
        [CSVInjection]
        public string Code { get; set; }

        [MaxLength(150)]
        [Required]
        [CSVInjection]
        public string Title { get; set; }
        [MaxLength(30)]
        [Required]
        [CourseType]
        public string CourseType { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public int? CourseAdminID { get; set; }
        public bool LearningApproach { get; set; }
        [MaxLength(40)]
        public string Language { get; set; }
        public float FinalAssessmentScore { get; set; }
        [MaxLength(25)]
        public string IsSuperiorfeedback { get; set; }
        public bool IsCertificateIssued { get; set; }
        [Range(0, int.MaxValue)]
        public int CompletionPeriodDays { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Cost must be a positive number")]
        public float CourseFee { get; set; }
        [MaxLength(25)]
        public string Currency { get; set; }
        [Range(0, 100, ErrorMessage = "Credit point must be between 0 to 100")]
        public float CreditsPoints { get; set; }
        public string ThumbnailPath { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryId { get; set; }
        public ApiCourseModuleAssociation[] ModuleAssociation { get; set; }
        public bool IsPreAssessment { get; set; }
        public int? PreAssessmentId { get; set; }
        public bool IsAssessment { get; set; }
        public int? AssessmentId { get; set; }
        public bool IsFeedback { get; set; }
        public int? FeedbackId { get; set; }
        public bool IsAssignment { get; set; }
        public int? AssignmentId { get; set; }
        public bool isApplicableToAll { get; set; }
        public bool IsApplicableToExternal { get; set; }
        public string AdminName { get; set; }
        public int? SubCategoryId { get; set; }
        [MaxLength(500)]
        public string Metadata { get; set; }
        public bool IsSection { get; set; }
        public bool IsDiscussionBoard { get; set; }
        public int? MemoId { get; set; }
        public bool IsMemoCourse { get; set; }
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.Boss, CommonValidation.Mini, CommonValidation.Normal })]
        public string Mission { get; set; }
        [Range(0, 100)]
        public int? Points { get; set; }
        public bool IsAchieveMastery { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number")]
        public int DurationInMinutes { get; set; }
        public int TotalModules { get; set; }
        public bool IsShowInCatalogue { get; set; }
        public bool IsModuleHasAssFeed { get; set; }
        public APIJobRole[] CompetencySkillsData { get; set; }

        public APISubSubCategory[] subsubCategoryId { get; set; }
        public int[] courseAdminIDs { get; set; }        
        public bool IsManagerEvaluation { get; set; }
        public int? ManagerEvaluationId { get; set; }
        public int? prerequisiteCourse { get; set; }
        [DefaultValue("false")]
        public bool IsExternalProvider { get; set; }
        public string ExternalProvider { get; set; }
        public string CourseURL { get; set; }
        public int? noOfDays { get; set; }
        [DefaultValue("false")]
        public bool IsRetraining { get; set; }
        public bool IsFeedbackOptional { get; set; }
        public float GroupCourseFee { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [DefaultValue(false)]
        public bool? CanAutoActivated { get; set; }
        [DefaultValue(true)]
        public bool? IsVisibleAfterExpiry { get; set; }
        [DefaultValue(false)]
        public bool? IsDashboardCourse { get; set; }
        public int? isAssessmentReview { get; set; }
        public bool? PublishCourse { get; set; }
        public string LxpDetails { get; set; }
        public bool? IsPrivateContent { get; set; }
        public int? VendorId { get; set; } = null;
        public bool IsOJT { get; set; }
        public int? OJTId { get; set; }

        [DefaultValue("true")]
        public bool isVisibleAssessmentDetails { get; set; }
        public bool? isRefresherMandatory { get; set; }

    }

    public class APIxternalCourse
    {
        public int Id { get; set; }
        [MaxLength(20)]
        [Required]
        [CSVInjection]
        public string Code { get; set; }

        [MaxLength(150)]
        [Required]
        [CSVInjection]
        public string Title { get; set; }

        [MaxLength(30)]
        [Required]
        [CourseType]
        public string CourseType { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public int? CourseAdminID { get; set; }
        public bool LearningApproach { get; set; }
        [MaxLength(40)]
        public string Language { get; set; }
        public float FinalAssessmentScore { get; set; }
        [MaxLength(25)]
        public string IsSuperiorfeedback { get; set; }
        public bool IsCertificateIssued { get; set; }
        [Range(0, int.MaxValue)]
        public int CompletionPeriodDays { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Cost must be a positive number")]
        public float CourseFee { get; set; }
        [MaxLength(25)]
        public string Currency { get; set; }
        [Range(0, 100, ErrorMessage = "Credit point must be between 0 to 100")]
        public float CreditsPoints { get; set; }
        public string ThumbnailPath { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryId { get; set; }
        public bool IsPreAssessment { get; set; }
        public int? PreAssessmentId { get; set; }
        public bool IsAssessment { get; set; }
        public int? AssessmentId { get; set; }
        public bool IsFeedback { get; set; }
        public int? FeedbackId { get; set; }
        public bool IsAssignment { get; set; }
        public int? AssignmentId { get; set; }
        public bool isApplicableToAll { get; set; }
        public string AdminName { get; set; }
        public int? SubCategoryId { get; set; }
        [MaxLength(500)]
        public string Metadata { get; set; }

        public bool IsSection { get; set; }
        public bool IsDiscussionBoard { get; set; }
        public int? MemoId { get; set; }
        public bool IsMemoCourse { get; set; }

        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.Boss, CommonValidation.Mini, CommonValidation.Normal })]
        public string Mission { get; set; }
        [Range(0, 100)]
        public int Points { get; set; }
        public bool IsAchieveMastery { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number")]
        public int DurationInMinutes { get; set; }
        public int TotalModules { get; set; }
        public bool IsShowInCatalogue { get; set; }
        public bool IsModuleHasAssFeed { get; set; }
        public APIJobRole[] CompetencySkillsData { get; set; }
        public bool IsManagerEvaluation { get; set; }
        public int? ManagerEvaluationId { get; set; }
        public int? prerequisiteCourse { get; set; }
        [DefaultValue("false")]
        public bool IsExternalProvider { get; set; }
        public string ExternalProvider { get; set; }
        public string CourseURL { get; set; }
    }


    public class ApiCourseModuleAssociation
    {
        public int Id { get; set; }
        public int? PreAssessmentId { get; set; }
        public int? AssessmentId { get; set; }
        public int? FeedbackId { get; set; }
        public int? AssignmentId { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsAssignment { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
        public string PreAssessmentName { get; set; }
        public string AssessmentName { get; set; }
        public string FeedbackName { get; set; }
        public string Name { get; set; }
        public string ModuleType { get; set; }
        public int? SectionId { get; set; }
        public string SectionTitle { get; set; }
        public int? SequenceNo { get; set; }
        [Range(0, int.MaxValue)]
        public int? CompletionPeriodDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? CanAutoActivated { get; set; }
        public bool? IsVisibleAfterExpiry { get; set; }
    }

    public class ApiCourseModuleSequence
    {
        [Required]
        public int ModuleId { get; set; }
        [Required]
        public int SequenceNo { get; set; }
    }

    public class APICourseWithID
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

  
    public class ApiCourseModule
    {
        [Required]
        public int page { get; set; }
        [Required]
        public int pageSize { get; set; }
        public string search { get; set; }
        public string columnName { get; set; }
        public string searchString { get; set; }
        public bool showAllData { get; set; }
    }

    public class ApiGetCourse
    {
        [Required]
        public int page { get; set; } = 1;
        [Required]
        public int pageSize { get; set; } = 10;
        public int? categoryId { get; set; }
        public bool? IsActive { get; set; }
        public string search { get; set; }
        public string filter { get; set; }
        public bool showAllData { get; set; }
    }

}
public class APICourseCategoryTypeahead
{
    public int ID { get; set; }
    public string Name { get; set; }
}

public class APIPreRequisiteCourseStatus
{
    public int CourseID { get; set; }

}
public class APIPrerequisiteCourseStatus
{
    public int? Id { get; set; }

    public string Name { get; set; }
    public string CourseStatus { get; set; }

}