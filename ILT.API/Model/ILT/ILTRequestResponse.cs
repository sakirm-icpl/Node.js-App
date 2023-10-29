namespace ILT.API.Model.ILT
{
    public class ILTRequestResponse : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public int ModuleID { get; set; }
        public int ScheduleID { get; set; }
        public int UserID { get; set; }
        public string TrainingRequesStatus { get; set; }
        public int? ReferenceRequestID { get; set; }
        public string Reason { get; set; }
        public int BatchID { get; set; }
    }

}
