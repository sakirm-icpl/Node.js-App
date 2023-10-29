namespace ILT.API.Model.ILT
{
    public class ILTTrainingAttendance : BaseModel
    {
        public int ID { get; set; }
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }
        public bool IsPresent { get; set; }
        public string Status { get; set; }
      
    }
}

