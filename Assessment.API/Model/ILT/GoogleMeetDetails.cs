using Assessment.API.Model;
using Google.Apis.Calendar.v3.Data;
using System.Collections.Generic;

namespace Assessment.API.Model.ILT
{
    public class GoogleMeetDetails : BaseModel
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public int ScheduleID { get; set; }
        public string ICalUID { get; set; }
        public string MeetingId { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public string HtmlLink { get; set; }
        public string HangoutLink { get; set; }
        public string OrganizerEmail { get; set; }
        public int? UserWebinarId { get; set; }
    }

    public class GoogleMeetRessponce
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public GoogleMeetDetails meetDetails { get; set; }

    }
    public class GoogleMeetAttendees
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public List<EventAttendee> attendees { get; set; }
    }

    public class GoogleMeetDetailsV2 : GoogleMeetDetails
    {
        public string? Username { get; set; }

    }

}
