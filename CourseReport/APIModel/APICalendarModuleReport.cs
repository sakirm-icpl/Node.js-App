using System;
using System.Collections.Generic;

namespace CourseReport.API.APIModel
{
    public class APICalendarModuleReport
    {
        public int Id { get; set; }
        public bool IsAssessment { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsPreAssessment { get; set; }
        public string ModuleStatus { get; set; }
        public string AssessmentStatus { get; set; }
        public string PreAssessmentStatus { get; set; }
        public string FeedbackStatus { get; set; }
        public string CourseTitle { get; set; }
        public string ModuleTitle { get; set; }
        public DateTime ModuleStartDate { get; set; }
        public DateTime ModuleEndDate { get; set; }
        public DateTime AssessmentSubmitDate { get; set; }
        public DateTime PreAssessmentSubmitDate { get; set; }
        public DateTime FeedbcaskSubmitDate { get; set; }
    }

    public class ILTScheduleCalenderData
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string ModuleName { get; set; }
        public string ModuleType { get; set; }
        public string ScheduleCode { get; set; }
        public string RequestStatus { get; set; }
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public float Cost { get; set; }
        public string Currency { get; set; }
        public string Cityname { get; set; }
        public string PlaceName { get; set; }
        public string PostalAddress { get; set; }
        public string FromTimeZone { get; set; }
        public string ToTimeZone { get; set; }
        public bool IsShowInCatalogue { get; set; }
    }
    public class ILTScheduleCalenderDataExport
    {
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string ModuleName { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public DateTime BatchStartDate { get; set; }
        public DateTime BatchEndDate { get; set; }
        public TimeSpan BatchStartTime { get; set; }
        public TimeSpan BatchEndTime { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public TimeSpan ScheduleStartTime { get; set; }
        public TimeSpan ScheduleEndTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Cityname { get; set; }
        public string PlaceName { get; set; }
        public string PostalAddress { get; set; }
        public bool IsTrainer { get; set; }
        public string FromTimeZone { get; set; }
        public string ToTimeZone { get; set; }
    }
}
