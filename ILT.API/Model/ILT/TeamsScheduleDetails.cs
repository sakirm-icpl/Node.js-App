namespace ILT.API.Model.ILT
{
    public class TeamsScheduleDetails : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public string MeetingId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string iCalUId { get; set; }
        public string ?JoinUrl {get;set;}
        public int ?UserWebinarId { get; set; }
    }
    public class TeamsScheduleDetailsV2 : TeamsScheduleDetails
    {
        public string? Username { get; set; }
    }
}
