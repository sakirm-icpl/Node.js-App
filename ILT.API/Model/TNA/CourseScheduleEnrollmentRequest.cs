namespace ILT.API.Model.TNA
{
    public class CourseScheduleEnrollmentRequest : BaseModel
    {
        public int Id { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }
        public int UserID { get; set; }
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
        public int? BeSpokeId { get; set; }

    }
}
