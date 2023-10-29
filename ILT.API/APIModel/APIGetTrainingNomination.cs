namespace ILT.API.APIModel
{
    public class APIGetTrainingNomination
    {
        public int scheduleID { get; set; }
        public int courseId { get; set; }
        public int moduleId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public string search { get; set; }
        public string searchText { get; set; }
        public string Type { get; set; }
    }
    public class APIScheduleID
    {
        public int scheduleID { get; set; }
    }
    public class APIGetDetailsForUserAttendance
    {
        public int scheduleID { get; set; }
        public int courseId { get; set; }
        public int moduleId { get; set; }
        public int UserId { get; set; }
    }

    public class APIDetailsOfUserAfterAttendance
    {
        public string AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; }
    }

    public class APIDeleteUserNomination
    {
        public int scheduleID { get; set; }
        public int courseId { get; set; }
        public int moduleId { get; set; }
        public string UserIdEncrypted { get; set; }
    }
}

