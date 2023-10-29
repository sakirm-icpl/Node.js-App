namespace Courses.API.Model.ILT
{
    public class ZoomMeetingDetails : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public string Join_url { get; set; }
        public string Start_url { get; set; }
        public string UniqueMeetingId { get; set; }
        public string Host_id { get; set; }
        public string Start_time { get; set; }
        public string Topic { get; set; }
        public string Uuid { get; set; }
        public string Timezone { get; set; }
        public string Duration { get; set; }
        public int UserWebinarId { get; set; }

    }
    public class ZoomMeetingDetailsV2 : ZoomMeetingDetails
    {
        public string? Username { get; set; }
    }
}
