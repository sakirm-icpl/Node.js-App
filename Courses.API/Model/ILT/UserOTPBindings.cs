using System;

namespace Courses.API.Model.ILT
{
    public class UserOTPBindings
    {

        public int ID { get; set; }
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public string OTP { get; set; }
        public Boolean IsAddedInNomination { get; set; }
        public DateTime AttendanceDate { get; set; }
    }
}
