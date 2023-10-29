using System;
using System.Collections.Generic;

namespace Courses.API.APIModel
{
    public class ApiCourseInfo
    {
        public int CourseId { get; set; }
        public string CourseType { get; set; }
        public string CourseCode { get; set; }
        public string CategoryName { get; set; }
        public string CourseTitle { get; set; }
        public string ThumbnailPath { get; set; }
        public bool LearningApproach { get; set; }
        public int? PreAssessmentId { get; set; }
        public int? AssessmentId { get; set; }
        public int? FeedbackId { get; set; }
        public int? AssignmentId { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsAssignment { get; set; }
         public string Status { get; set; }
        public string AssessmentStatus { get; set; }
        public string PreAssessmentStatus { get; set; }
        public string FeedbackStatus { get; set; }
        public string ContentStatus { get; set; }
        public string AssignmentStatus { get; set; }
      public int Duration { get; set; }
        public APIModuleInfo Modules { get; set; }
        }
    public class CourseSectionInfo
    {
        public int? SectionId { get; set; }
        public string SectionTitle { get; set; }
        public string SectionDescription { get; set; }
        public int SectionNumber { get; set; }
        public List<APIModulesofCourses> Modules { get; set; }
        public APIZoom[] aPIZooms { get; set; }
     }

    public class APIModuleInfo
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string MimeType { get; set; }
        public string Path { get; set; }
        public string ZipPath { get; set; }
        public string ModuleType { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }
        public string AssesmentType { get; set; }
        public bool IsLearnerFeedback { get; set; }
        public bool IsTrainerFeedback { get; set; }
        public bool IsMobileCompatible { get; set; }
        public float? Duration { get; set; }
        public int? CreditPoints { get; set; }
        public int? PreAssessmentId { get; set; }
        public int? AssessmentId { get; set; }
        public int? FeedbackId { get; set; }
        public int? AssignmentId { get; set; }
        public int? LCMSId { get; set; }
        public string YoutubeVideoId { get; set; }
        public string ExternalLCMSId { get; set; }
        public bool IsPreAssessment { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsAssignment { get; set; }
        public string Status { get; set; }
        public string AssessmentStatus { get; set; }
        public string PreAssessmentStatus { get; set; }
        public string FeedbackStatus { get; set; }
        public string ContentStatus { get; set; }
        public int? SectionId { get; set; }
        public int? ScheduleID { get; set; }
        public String ScheduleCode { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string RegistrationEndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string PlaceName { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string TrainingRequestStatus { get; set; }
        public int? SequenceNo { get; set; }
        public string Location { get; set; }
        public string InternalName { get; set; }
        public bool IsEmbed { get; set; }

    }
    public class APIZoom
    {
        public string zoom_link { get; set; }
        public string is_zoom_created { get; set; }
        public string zoom_name { get; set; }
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public string ScheduleTime { get; set; }
        public bool EnableSchedule { get; set; }
    }
    public class APITeams
    {
        public string teams_link { get; set; }
        public string is_teams_created { get; set; }
        public string teams_name { get; set; }
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public string ScheduleTime { get; set; }
        public bool EnableSchedule { get; set; }
    }
    public class APIGoogleMeet
    {
        public string gsuit_link { get; set; }
        public string is_gsuit_created { get; set; }
        public string gsuit_name { get; set; }
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public string ScheduleTime { get; set; }
        public bool EnableSchedule { get; set; }
    }
}
