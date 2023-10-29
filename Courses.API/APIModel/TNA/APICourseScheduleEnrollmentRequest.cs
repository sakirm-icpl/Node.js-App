using System;

namespace Courses.API.APIModel.TNA
{
    public class APICourseScheduleEnrollmentRequest
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public int ScheduleID { get; set; }
        public string ScheduleCode { get; set; }
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string RequestStatus { get; set; }
        public bool IsRequestSendToLevel1 { get; set; }
        public bool IsRequestSendToLevel2 { get; set; }
        public bool IsRequestSendToLevel3 { get; set; }
        public bool IsRequestSendToLevel4 { get; set; }
        public bool IsRequestSendToLevel5 { get; set; }
        public bool IsRequestSendToLevel6 { get; set; }
        public string UserStatusInfo { get; set; }
        public string RequestedFrom { get; set; }
        public int RequestedFromLevel { get; set; }
        public string SentBy { get; set; }
        public string Comment { get; set; }
        public DateTime RequestedOn { get; set; }
        public string CourseFee { get; set; }
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public string Currency { get; set; }
        public int? BeSpokeId { get; set; }
    }

    public class APIBeSpokeEnrollmentRequest
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public int ScheduleID { get; set; }
        public string ScheduleCode { get; set; }
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string RequestStatus { get; set; }
        public bool IsRequestSendToLevel1 { get; set; }
        public bool IsRequestSendToLevel2 { get; set; }
        public bool IsRequestSendToLevel3 { get; set; }
        public bool IsRequestSendToLevel4 { get; set; }
        public bool IsRequestSendToLevel5 { get; set; }
        public string UserStatusInfo { get; set; }
        public string RequestedFrom { get; set; }
        public int RequestedFromLevel { get; set; }
        public string SentBy { get; set; }
        public string Comment { get; set; }
        public DateTime RequestedOn { get; set; }        
        public int? BeSpokeId { get; set; }
    }
}
