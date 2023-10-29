using System;

namespace Courses.API.APIModel.ILT
{
    public class APITrainingAttendanceForAll
    {
        public int attendanceID { get; set; }
        public int ScheduleID { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PlaceName { get; set; }
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Place { get; set; }
        public string Venue { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string UserName { get; set; }
    }
}
