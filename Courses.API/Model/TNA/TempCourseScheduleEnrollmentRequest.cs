namespace Courses.API.Model.TNA
{
    public class TempCourseScheduleEnrollmentRequest : BaseModel
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }
        public int UserID { get; set; }
        public string RequestStatus { get; set; }
        public bool IsRequestSendToLevel1 { get; set; }
        public string UserStatusInfo { get; set; }
        public string RequestedFrom { get; set; }
        public string SentBy { get; set; }

    }
}
