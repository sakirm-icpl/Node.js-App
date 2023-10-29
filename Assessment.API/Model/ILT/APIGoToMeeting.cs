using System;

namespace Assessment.API.APIModel.ILT
{
    public class APIGoToMeeting
    {
        public class Customer
        {
            public string? CustomerID { get; set; }
            public string? ContactName { get; set; }
            public string? City { get; set; }
        }

        public class TokenResponce
        {
            public string? access_token { get; set; }
            public string? expires_in { get; set; }
            public string? refresh_token { get; set; }
            public string? organizer_key { get; set; }
            public string? account_key { get; set; }
            public string? account_type { get; set; }
            public string? firstName { get; set; }
            public string? lastName { get; set; }
            public string? email { get; set; }
            public string? platform { get; set; }
            public string? version { get; set; }
            public string? hostURL { get; set; }
        }


        public class MeetingAttendees
        {
            public string? leaveTime { get; set; }
            public string? lastName { get; set; }
            public string? numAttendees { get; set; }
            public string? joinTime { get; set; }
            public string? subject { get; set; }
            public string? attendeeEmail { get; set; }
            public string? meetingId { get; set; }
            public string? organizerkey { get; set; }
            public string? meetingType { get; set; }
            public string? attendeeName { get; set; }
            public string? duration { get; set; }
            public string? firstName { get; set; }
            public string? groupName { get; set; }
            public string? meetingInstanceKey { get; set; }
            public string? name { get; set; }
            public string? conferenceCallInfo { get; set; }
            public string? startTime { get; set; }
            public string? endTime { get; set; }
            public string? email { get; set; }

        }



        public class MeetingResponce
        {
            public string? joinURL { get; set; }
            public Int64 meetingid { get; set; }
            public string? maxParticipants { get; set; }
            public Int64 uniqueMeetingId { get; set; }
            public string? conferenceCallInfo { get; set; }
        }


        public class GetMeetingResponce
        {
            public string? passwordRequired { get; set; }
            public string? createTime { get; set; }
            public string? subject { get; set; }
            public string? conferenceCallInfo { get; set; }
            public string? startTime { get; set; }
            public string? meetingid { get; set; }
            public string? endTime { get; set; }
            public string? uniqueMeetingId { get; set; }
            public string? meetingType { get; set; }
            public string? status { get; set; }
            public string? maxParticipants { get; set; }
        }

    }
}
