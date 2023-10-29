namespace TNA.API.Model
{
    public class TrainingNomination : BaseModel
    {
        public int ID { get; set; }
        public int ScheduleID { get; set; }
        public string TrainingRequestStatus { get; set; }
        public int UserID { get; set; }
        public int? ReferenceRequestID { get; set; }
        public string RequestCode { get; set; }
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string OTP { get; set; }
        public bool IsActiveNomination { get; set; }
    }
}
