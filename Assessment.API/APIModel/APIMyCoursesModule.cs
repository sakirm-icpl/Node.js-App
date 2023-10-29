using Assessment.API.Common;
using System.ComponentModel.DataAnnotations;

namespace Assessment.API.APIModel
{
    public class APIMyCoursesModule
    {
        public int CourseId { get; set; }
        public string? CourseType { get; set; }
        public string? CourseCode { get; set; }
        public string? CategoryName { get; set; }
        public string? CourseTitle { get; set; }
        public int? NumberofModules { get; set; }
        public int? CompletionPeriodDays { get; set; }
        public float CourseFee { get; set; }
        [MaxLength(25)]
        public string? Currency { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? Description { get; set; }
        public bool LearningApproach { get; set; }
        public string? language { get; set; }
        public float? CourseCreditPoints { get; set; }
        public int? PreAssessmentId { get; set; }
        public int? AssessmentId { get; set; }
        public int? FeedbackId { get; set; }
        public bool IsFeedbackOptional { get; set; }
        public int? AssignmentId { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsAssignment { get; set; }
        public bool IsCertificateIssued { get; set; }
        public string? Status { get; set; }
        public string? AssessmentStatus { get; set; }
        public string? PreAssessmentStatus { get; set; }
        public string? FeedbackStatus { get; set; }
        public string? ContentStatus { get; set; }
        public string? AdminName { get; set; }
        public float? CourseRating { get; set; }
        public int CourseRatingCount { get; set; }
        public bool IsAdaptiveLearning { get; set; }
        public int ProgressPercentage { get; set; }
        public int Duration { get; set; }
        public string? CourseAssignedDate { get; set; }
        public string? LastActivityDate { get; set; }
        public bool IsDilinkingILT { get; set; }
        public string? AssignmentStatus { get; set; }
        public bool IsModuleHasAssFeed { get; set; }
        public bool IsManagerEvaluation { get; set; }
        public string? ManagerEvaluationStatus { get; set; }
        public bool IsPreRequisiteCourse { get; set; }
        public List<APIModulesofCourses>? Modules { get; set; }
        public List<CourseSection>? Sections { get; set; }
        public List<APIModulesofCourses>? ModuleSequence { get; set; }
        public bool IsShowViewBatches { get; set; }
        public string? ExpiryMessage { get; set; }
        public bool? IsCourseExpired { get; set; }
        public string? ExternalProvider { get; set; }
        public string? ExternalProviderCategory { get; set; }
        public bool IsOJT { get; set; }
        public string? OJTStatus { get; set; }
        public int? OJTId { get; set; }
        public bool isVisibleAssessmentDetails { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? RetrainingDate { get; set; }
    }

    public class CourseSection
    {
        public int? SectionId { get; set; }
        public string? SectionTitle { get; set; }
        public string? SectionDescription { get; set; }
        public int SectionNumber { get; set; }
        public List<APIModulesofCourses>? Modules { get; set; }
    }

    public class MultiLangualContentInfo
    {

        public string? ContentPath { get; set; }
        public string? Language { get; set; }
        public string? InternalName { get; set; }
        public string? MimeType { get; set; }
        public string? ModuleType { get; set; }
        public string? YoutubeVideoId { get; set; }
        public string? LanguageDisplay { get; set; }
    }
    public class APIModulesofCourses
    {
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public string? MimeType { get; set; }
        public string? Path { get; set; }
        public string? ZipPath { get; set; }
        public string? ModuleType { get; set; }
        public string? ActualModuleType { get; set; }
        public string? Thumbnail { get; set; }
        public string? Description { get; set; }
        public string? AssesmentType { get; set; }
        public bool IsLearnerFeedback { get; set; }
        public bool IsTrainerFeedback { get; set; }
        public bool IsMobileCompatible { get; set; }
        public float? Duration { get; set; }
        public int? CreditPoints { get; set; }
        public int? PreAssessmentId { get; set; }
        public int? AssessmentId { get; set; }
        public int? FeedbackId { get; set; }
        public int? LCMSId { get; set; }
        public string? YoutubeVideoId { get; set; }
        public string? ExternalLCMSId { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public string? Status { get; set; }
        public string? AssessmentStatus { get; set; }
        public string? PreAssessmentStatus { get; set; }
        public string? FeedbackStatus { get; set; }
        public string? ContentStatus { get; set; }
        public int? SectionId { get; set; }
        public int? BatchId { get; set; }
        public string? BatchCode { get; set; }
        public string? BatchName { get; set; }
        public int? ScheduleID { get; set; }
        public String? ScheduleCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? PlaceName { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? TrainingRequestStatus { get; set; }
        public int? SequenceNo { get; set; }
        public string? Location { get; set; }
        public string? FinalDate { get; set; }
        public int? CompletionPeriodDays { get; set; }
        public bool? IsEnableModule { get; set; }
        public string? ActivityID { get; set; }
        public bool IsMultilingual { get; set; }
        public string? selectedLanguageCode { get; set; }
        public bool IsEmbed { get; set; }
        public List<BBMeetingDetails>? BBBmeetingDetails { get; set; }
        public List<APIZoom>? apizooms { get; set; }
        public List<APITeams>? apiteams { get; set; }
        public List<APIGoogleMeet>? apigsuit { get; set; }

        public string? AttendanceStatus { get; set; }
        public string? WAIVERStatus { get; set; }
        public int ScheduleCreatedBy { get; set; }
        public DateTime Tz_StartDt { get; set; }
        public DateTime Tz_EndDt { get; set; }
    }
    public class TimeZoneList
    {
        public string? Text { get; set; }
        public string? Value { get; set; }
    }
}
