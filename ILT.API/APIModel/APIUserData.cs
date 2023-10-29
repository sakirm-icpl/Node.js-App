using System;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.APIModel
{
    public class APIUserData
    {
        [Required]
        public int userId { get; set; }
        [Required]
        public string userName { get; set; }
        [Required]
        public string emailId { get; set; }
        [Required]
        public string mobileNumber { get; set; }
        public string OTP { get; set; }
    }
    public class APINominationResponse
    {
        public string CourseName { get; set; }
        public string BatchCode { get; set; }
        public string ModuleName { get; set; }
        public int ScheduleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TrainerName { get; set; }
        public string ScheduleCode { get; set; }
        public string Title { get; set; }
        public string PostalAddress { get; set; }
        public string ContactNumber { get; set; }
        public string ContactPerson { get; set; }
        public string PlaceName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string OTP { get; set; }
        public string ModuleType { get; set; }
        public string WebinarType { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public bool IsApplicableToAll { get; set; }
        public string UserIdEncrypted { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
