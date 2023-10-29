using System;

namespace Courses.API.APIModel.ILT
{
    public class APIILTRequestResponse
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public int ScheduleID { get; set; }
        public string ScheduleCode { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string TrainingRequesStatus { get; set; }
        public string Reason { get; set; }
        public string EmailId { get; set; }
        public bool? IsExpired { get; set; }
    }
    public class APIILTBatchRequestResponse
    {
        public int BatchID { get; set; }
        public string TrainingRequestStatus { get; set; }
    }
    public class APIILTScheduleRequestStatus
    {
        public string ScheduleCode { get; set; }
        public string TrainingName { get; set; }
        public string Venue { get; set; }
        public string RequestStatus { get; set; }
        public string Response { get; set; }
    }
    public class APIILTCheckUserBusy
    {
        public bool IsUserBusy { get; set; }
        public string ScheduleCode { get; set; }
        public bool CanNominate { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class APIILTBatchRequests
    {
        public int RequestId { get; set; }
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int UserMasterId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string TrainingRequestStatus { get; set; }
        public DateTime RequestedDate { get; set; }
        public string Reason { get; set; }
        public bool? IsExpired { get; set; }
    }
    public class APIILTBatchRequestApprove
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int UserId { get; set; }
        public string TrainingRequestStatus { get; set; }
        public string Reason { get; set; }
    }
    public class APIILTBatchRequestApproveStatus
    {
        public int RequestId { get; set; }
        public string ScheduleCode { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }
    public class APIILTRequestedBatches
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Description { get; set; }
    }
}