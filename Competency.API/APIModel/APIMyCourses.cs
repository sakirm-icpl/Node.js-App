using System;
using System.ComponentModel.DataAnnotations;

namespace Competency.API.APIModel
{
    public class APIMyCourses
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Mission { get; set; }
        public string Description { get; set; }
        public string ThumbnailPath { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubSubCategoryName { get; set; }
        public string Code { get; set; }
        public float CourseFee { get; set; }
        public string Currency { get; set; }
        public string CourseType { get; set; }
        public int CompletionPeriodDays { get; set; }
        public int CourseCreationdDays { get; set; }
        public int? NumberofModules { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }

        public int? SubSubCategoryId { get; set; }
        public float? CourseRating { get; set; }
        public string Status { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsAssignment { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsCertificateIssued { get; set; }
        public DateTime? CourseStartDate { get; set; }
        public DateTime? CourseCompleteDate { get; set; }
        public int? RewardPoint { get; set; }
        public bool? IsCourseApplicable { get; set; }
        public string CourseApprovalStatus { get; set; }
        public string CompetencyCategory { get; set; }
        public int CompetencyCategoryID { get; set; }
        public string AssessmentPercentage { get; set; }
        public string ScheduleRequestStatus { get; set; }
        public int DurationInMinutes { get; set; }
        public string AssessmentResult { get; set; }
        public bool IsPreRequisiteCourse { get; set; }
        public bool IsExternalProvider { get; set; }
        public string ExternalProvider { get; set; }
        public string CourseURL { get; set; }
        public bool IsRetraining { get; set; }
        public string NodalApprovalStatus { get; set; }
        public bool IsSCORM { get; set; }
        public string FirstAccessDate { get; set; }
        public string LastAccessDate { get; set; }
        public string TimeSpent { get; set; }
        public int views { get; set; }
        public int Progressinpercentage { get; set; }
        public int score { get; set; }
    }
    public class APIGetUserID
    {
        public string UserId { get; set; }
    }
    public class characteristics
    {
        public Boolean earnsBadge { get; set; }
        public Boolean hasAssessment { get; set; }
    }
    public class contentType
    {
        public string percipioType { get; set; }
        public string category { get; set; }
        public string displayLabel { get; set; }
        public string source { get; set; }
    }
    public class localizedMetadata
    {
        public string localeCode { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }
    public class lifecycle
    {
        public string status { get; set; }
        public string publishDate { get; set; }
        public string lastUpdatedDate { get; set; }
        public string plannedRetirementDate { get; set; }
        public string retiredDate { get; set; }
        public Boolean includedFromActivity { get; set; }
    }
    public class aiccLaunch
    {
        public string url { get; set; }

        public string Params { get; set; }
    }
    public class channels
    {
        public string id { get; set; }
        public string title { get; set; }
        public string link { get; set; }
    }
    public class publication {
        public int copyrightYear { get; set; }
        public string isbn { get; set; }
        public string publisher {get;set;}
        }
    public class associations
    {
        public string[] areas { get; set; }
        public string[] subjects { get; set; }

        public channels[] channels { get; set; }
        public string parent { get; set; }
        public string translationGroupId { get; set; }
        public channels[] journeys { get; set; }
        public string[] collections { get; set; }
    }
    public class credentials
    {
        public string cpeCredits { get; set; }
            public Boolean nasbaReady { get; set; }
            public string pduCredits{ get; set; }
    } 
    public class technologies
    {
        public string title { get; set; }
        public string version { get; set; }
    }
    public class SkillSoftData
    {
        public string id { get; set; }
        public string code { get; set; }
        public string xapiActivityId { get; set; }
        public string xapiActivityTypeId { get; set; }
        public characteristics characteristics { get; set; }
        public contentType contentType { get; set; }
        public string[] localeCodes { get; set; }
        public localizedMetadata[] localizedMetadata { get; set; }
        public lifecycle lifecycle { get; set; }
        public string link { get; set; }
        public aiccLaunch aiccLaunch { get; set; }
        public string imageUrl { get; set; }
        public string alternateImageUrl { get; set; }
        public string[] keywords { get; set; }
        public string duration { get; set; }
        public string[] by { get; set; }
        //public string publication { get; set; }
        public credentials credentials { get; set; }
        public string[] expertiseLevels { get; set; }
        public string[] modalities { get; set; }
        public technologies[] technologies { get; set; }
        public associations associations { get; set; }
        public string[] learningObjectives { get; set; }
    }
    public class AllSkillSoftCourses
    {
        public int Id { get; set; }

        public string Code { get; set; }
      
        public string Title { get; set; }

        public string CourseType { get; set; }
        public string Description { get; set; }

        public bool? LearningApproach { get; set; }

        public string Language { get; set; }
        public bool IsCertificateIssued { get; set; }
        public int CompletionPeriodDays { get; set; }
 
        public float CreditsPoints { get; set; }
        public bool IsAssessment { get; set; }

        public bool IsFeedback { get; set; }

        public bool IsAssignment { get; set; }

        public bool IsApplicableToAll { get; set; }

        public string Metadata { get; set; }
        public bool IsSection { get; set; }
        public bool IsDiscussionBoard { get; set; }
        public bool IsMemoCourse { get; set; }
        public string Mission { get; set; }
        public int Points { get; set; }
        public bool IsAchieveMastery { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        public int DurationInMinutes { get; set; }
        public int TotalModules { get; set; }

        public bool IsShowInCatalogue { get; set; }
        public bool IsManagerEvaluation { get; set; }

        public bool IsExternalProvider { get; set; }
        public string ExternalProvider { get; set; }
        public string CourseURL { get; set; }

        public bool IsRetraining { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeedbackOptional { get; set; }
        public float GroupCourseFee { get; set; }
        public bool? IsVisibleAfterExpiry { get; set; }
        public bool? IsDashboardCourse { get; set; }
        public string status { get; set; }
        public string thumbnailPath { get; set; }
        public string categoryName { get; set; }
    }

    public class APIDarwinCoursEnroll
    {
        public string userId { get; set; }        
        [Required]
        public string courseCode { get; set; }
        public string api_key { get; set; }
        [Required]
        public string orgCode { get; set; }
    }
    public class AllExternalCourses
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public int CompletionPeriodDays { get; set; }
        public int DurationInMinutes { get; set; }
        public string ExternalProvider { get; set; }
        public string CourseURL { get; set; }
        public string status { get; set; }
        public string thumbnailPath { get; set; }
        public string categoryName { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public string CourseType { get; set; }
        public string CourseInstructor { get; set; }
        public string CourseOwner { get; set; }
        public string CourseLevel { get; set; }
        public string MobileNativeDeeplink { get; set; }
    }
    public class MyCourseForApplicability
    {
        public string Category { get; set; }
        public string Language { get; set; }
        public string Vender { get; set; }
        public string Level { get; set; }
        public int Duration { get; set; }
        public string courseInstructor { get; set; }
        public string courseOwner { get; set; }
    }
}
