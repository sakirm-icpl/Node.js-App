using System;
using System.Collections.Generic;

namespace CourseReport.API.APIModel
{
    public class ApiCalendarV2
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleType { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Currency { get; set; }
        public string Cityname { get; set; }
        public string PlaceName { get; set; }
        public string PostalAddress { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public string Actions { get; set; }
        public List<APICalendarSchedules> aPICalendarSchedulesList { get; set; }
    }

    public class ApiCalendarV2ForTrainer
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int ScheduleId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleType { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string StartTimes { get; set; }
        public string EndTimes { get; set; }
        public string Currency { get; set; }
        public string Cityname { get; set; }
        public string PlaceName { get; set; }
        public string PostalAddress { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public string Actions { get; set; }
        public string? link { get; set; }
        public string FromTimeZone { get; set; }
        public string ToTimeZone { get; set; }
        public bool IsShowInCatalogue { get; set; }
        public List<APICalendarSchedules> aPICalendarSchedulesList { get; set; }
    }
    public class APICalendarSchedules
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int ScheduleId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleType { get; set; }
        public string ScheduleCode { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Currency { get; set; }
        public string Cityname { get; set; }
        public string PlaceName { get; set; }
        public string Title { get; set; }
          public string FromTimeZone { get; set; }
        public string ToTimeZone { get; set; }
        public bool IsShowInCatalogue { get; set; }
    }
    public class ApiCalendarV3
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int ScheduleId { get; set; }
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string ModuleName { get; set; }
        public string ModuleType { get; set; }
        public DateTime BatchStartDate { get; set; }
        public DateTime BatchEndDate { get; set; }
        public TimeSpan BatchStartTime { get; set; }
        public TimeSpan BatchEndTime { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public TimeSpan ScheduleStartTime { get; set; }
        public TimeSpan ScheduleEndTime { get; set; }
        public string Currency { get; set; }
        public string Cityname { get; set; }
        public string PlaceName { get; set; }
        public string PostalAddress { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
        public string Actions { get; set; }
        public float Cost { get; set; }
        public string FromTimeZone { get; set; }
        public string ToTimeZone { get; set; }
        public bool IsShowInCatalogue { get; set; }
    }
    public class TimeZoneList
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}