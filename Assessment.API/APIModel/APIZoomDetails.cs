using System;

namespace Assessment.API.APIModel
{
    public class APIZoomDetails
    {
        public string? Code { get; set; }


    }

    public class APIZoomDetailsGetToken
    {
        public string? client_id { get; set; }
        public string? client_secret { get; set; }
        public string? authorize_url { get; set; }
        public string? access_token_url { get; set; }
        public string? redirect_uri { get; set; }
        public string? authorization_method { get; set; }
        public string? uri { get; set; }


    }

    public class APIZoomDetailsToken
    {

        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public string? refresh_token { get; set; }
        public string? expires_in { get; set; }
        public string? scope { get; set; }

    }


    public class APIZoomCreate
    {
        public string? topic { get; set; }
        public int type { get; set; }  // default 2
        public string? start_time { get; set; }
        public int duration { get; set; }
        public string? timezone { get; set; }
        public string? password { get; set; }
        public string? agenda { get; set; }

        public APIZoomCreateSettings? settings { get; set; }
        public APIZoomCreateRecurrence? recurrence { get; set; }

    }
    public class APIZoomCreateRecurrence
    {

        public int type { get; set; }
        public int repeat_interval { get; set; }
        public int weekly_days { get; set; }
        public int monthly_day { get; set; }
        public int monthly_week { get; set; }
        public int monthly_week_day { get; set; }
        public int end_times { get; set; }
        public string? end_date_time { get; set; }


    }

    public class APIZoomCreateSettings
    {
        public Boolean host_video { get; set; }
        public Boolean participant_video { get; set; }
        public Boolean cn_meeting { get; set; }
        public Boolean in_meeting { get; set; }
        public Boolean join_before_host { get; set; }
        public Boolean mute_upon_entry { get; set; }
        public Boolean watermark { get; set; }
        public Boolean use_pmi { get; set; }
        public int approval_type { get; set; }
        public int registration_type { get; set; }
        public string? audio { get; set; }
        public string? auto_recording { get; set; }
        public Boolean enforce_login { get; set; }
        public string? enforce_login_domains { get; set; }
        public string? alternative_hosts { get; set; }
        public Boolean meeting_authentication { get; set; }
        public string? authentication_domains { get; set; }

    }

    public class APIZoomMeetingResponce
    {
        public string? uuid { get; set; }
        public string? id { get; set; }
        public string? host_id { get; set; }
        public string? topic { get; set; }
        public int type { get; set; }
        public string? start_time { get; set; }
        public int duration { get; set; }
        public string? timezone { get; set; }
        public string? created_at { get; set; }
        public string? start_url { get; set; }
        public string? join_url { get; set; }
        public string? password { get; set; }
        public string? h323_password { get; set; }


    }
    public class ZoomParticipants
    {
        public string? id { get; set; }
        public string? user_id { get; set; }
        public string? name { get; set; }
        public string? user_email { get; set; }
        public string? join_time { get; set; }
        public string? leave_time { get; set; }
        public int duration { get; set; }
        public string? attentiveness_score { get; set; }
        public Boolean failover { get; set; }
        public string? status { get; set; }
        public string? customer_key { get; set; }
    }
    public class ZoomMeetingParticipants
    {
        public int page_count { get; set; }
        public int page_size { get; set; }
        public int total_records { get; set; }
        public string? next_page_token { get; set; }
        public ZoomParticipants[]? participants { get; set; }
    }
    public class ZoomMeetingDetailsForReport
    {
        public string? dept { get; set; }
        public int duration { get; set; }
        public string? end_time { get; set; }
        public string? host_id { get; set; }
        public long id { get; set; }
        public int participants_count { get; set; }
        public string? source { get; set; }
        public string? start_time { get; set; }
        public string? topic { get; set; }
        public int total_minutes { get; set; }
        public int type { get; set; }
        public string? user_email { get; set; }
        public string? user_name { get; set; }
        public string? uuid { get; set; }
    }
    public class ZoomReport
    {
        public ZoomMeetingParticipants? zoomMeetingParticipants { get; set; }
        public ZoomMeetingDetailsForReport? ZoomMeetingDetailsForReport { get; set; }

    }
    public class ExportZoom
    {
        public ZoomReport? zoomReports { get; set; }
        public string? ExportAs { get; set; }
    }

}
