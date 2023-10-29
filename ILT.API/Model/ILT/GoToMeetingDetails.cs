namespace ILT.API.Model.ILT
{
    public class GoToMeetingDetails : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public string JoinURL { get; set; }
        public string StartMeetingURL { get; set; }
        public int UniqueMeetingId { get; set; }
        public string ConferenceCallInfo { get; set; }
        public int MaxParticipants { get; set; }

    }
}
