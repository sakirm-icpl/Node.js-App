namespace CourseReport.API.APIModel
{
    public class APISchedulerReport
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public string CourseCategory { get; set; }
        public string CourseSubCategory { get; set; }
        public string CourseSubSubCategory { get; set; }
        public string CourseType { get; set; }
        public string ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string CourseAssignedDate { get; set; }
        public string IsAdaptiveAssessment { get; set; }
        public string CourseStartDate { get; set; }
        public string ContentCompletionDate { get; set; }
        public string ScheduleCode { get; set; }
        public string IsAssessmentAvailable { get; set; }
        public string AssessmentStatus { get; set; }
        public string AssessmentDate { get; set; }
        public string AssessmentResult { get; set; }
        public string AssessmentPercentage { get; set; }
        public string IsFeedbackAvailable { get; set; }
        public string FeedbackStatus { get; set; }
        public string FeedbackDate { get; set; }
        public string CourseCompletionDate { get; set; }
        public string CourseStatus { get; set; }
        public string Percentage { get; set; }
        public string Department { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public string UserStatus { get; set; }
        public string ReportsTo { get; set; }
        public string DateOfJoining { get; set; }
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        public string ConfigurationColumn1 { get; set; }
        public string ConfigurationColumn2 { get; set; }
        public string ConfigurationColumn3 { get; set; }
        public string ConfigurationColumn4 { get; set; }
        public string ConfigurationColumn5 { get; set; }
        public string ConfigurationColumn6 { get; set; }
        public string ConfigurationColumn7 { get; set; }
        public string ConfigurationColumn8 { get; set; }
        public string ConfigurationColumn9 { get; set; }
        public string ConfigurationColumn10 { get; set; }
        public string ConfigurationColumn11 { get; set; }
        public string ConfigurationColumn12 { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }
        public string PlaceName { get; set; }
        public string TrainerName { get; set; }
        public string CourseDuration { get; set; }

        public string UserRole { get; set; }

        public string  NoOfAttempts { get; set; }
        public string UserDuration { get; set; }
        public string Section { get; set; }
        public string WebTimeSpentInMinutes { get; set; }
        public string AppTimeSpentInMinutes { get; set; }
        public string DeviceMostActive { get; set; }

        public string? EmailId { get; set; }
        public string? Function { get; set; }
        public string? Region { get; set; }
        public string? Grade { get; set; }
        public string? Level { get; set; }
        public string? JobTitle { get; set; }
        public string? ReportingManager { get; set; }
    }
}
