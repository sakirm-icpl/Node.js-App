using System;

namespace Courses.API.APIModel.TNA
{
    public class APIModuleCompletionDetails
    {
        public string ModuleCompletionStatus { get; set; }
        public int ScheduleID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Venue { get; set; }
        public string AcademyName { get; set; }
        public string ScheduleCode { get; set; }
        public string ModuleName { get; set; }
        public TopicList[] TopicList { get; set; }
    }

    public class TopicList
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
    }
}
