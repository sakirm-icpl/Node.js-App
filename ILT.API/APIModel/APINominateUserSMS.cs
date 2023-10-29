using System;

namespace ILT.API.APIModel
{
    public class APINominateUserSMS
    {
        public string OTP { get; set; }
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string UserName { get; set; }
        public string CourseTitle { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int UserID { get; set; }
        public string organizationCode { get; set; }
        public string MobileNumber { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }
    }
    public class UserListForSMS
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
    }
    public class UserListGsuitNomination
    {
        public int ScheduleId { get; set; }
        public string UserName { get; set; }        
        public string EmailId { get; set; }
    }
}
