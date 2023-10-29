namespace ILT.API.APIModel
{
    public class APIAttendance
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; }
        public bool isPresent { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public string PlaceName { get; set; }
        public string ScheduleCode { get; set; }
    }
}
