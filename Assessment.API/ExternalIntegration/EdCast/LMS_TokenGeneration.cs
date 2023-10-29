
using System;

namespace Assessment.API.ExternalIntegration.EdCast
{
    public class LMS_TokenGeneration
    {


    }
    public class APIEdcastToken
    {
        public string? client_id { get; set; }
        public string? client_secret { get; set; }
        public string? grant_type { get; set; }

    }

    public class APIEdcastDetailsToken
    {

        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public string? expires_in { get; set; }
        public string? error { get; set; }

    }

    public class APICourseAssignment
    {

        public string? email_id { get; set; }
        public string? status { get; set; }
        public string? external_id { get; set; }
        public string? due_date { get; set; }
        public string? assigned_date { get; set; }
    }



    public class APIcoursePost
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public string? url { get; set; }
        public string? external_id { get; set; }
        public string? course_type { get; set; }
        public string? content_type { get; set; }
        public bool is_private { get; set; }
        public string? duration_sec { get; set; }
        public DateTime published_at { get; set; }
        public string? status { get; set; }
        public string? image_url { get; set; }
        public string[]? tags { get; set; }
        public string[]? prices_data { get; set; }
        // public int[] group_ids { get; set; }
        // public int[] channel_ids { get; set; }

    }

    public class APICoursePostAPIkey
    {
        public string? api_key { get; set; }
        public APICoursePostToDarwinbox[]? activity { get; set; }
    }
    public class APICourseStatusPostAPIkey
    {
        public string? api_key { get; set; }
        public APIUpdateCourseStatusToDB[]? employees { get; set; }
    }
    public class APICoursePostToDarwinbox
    {
        public string? learning_activity_code { get; set; }
        public string? learning_activity_name { get; set; }
        public string? learning_activity_description { get; set; }
        public string? currency { get; set; }
        public string? vendor { get; set; }
        public string? program { get; set; }
        public int allow_search_to { get; set; }
        public string? cost { get; set; }
        public string? available_from { get; set; }
        public string? available_to { get; set; }
        public string? content_link { get; set; }
    }

    public class APIUpdateCourseStatusToDB
    {
        public string? employee_id { get; set; }
        public string? activity_id { get; set; }
        public string? enrolled_on { get; set; }
        public string? start_date { get; set; }
        public string? complete_date { get; set; }
        public string? last_updated_on { get; set; }
        public string? action { get; set; }
        public string? unenrolled_on { get; set; }
        public string? completion_percentage { get; set; }
        public string? avg_assessment_result { get; set; }
        public string? enrollment_type { get; set; }
        public string? time_spent_on_learning { get; set; }
    }

    public class APIEdCastTransactionDetails
    {
        public string? error { get; set; }
        public string? message { get; set; }
        // public string external_id { get; set; }
        public data? data { get; set; }

    }
    public class APIDarwinTransactionDetails
    {
        public string[]? error { get; set; }
        public string[]? errors { get; set; }
        public string? message { get; set; }
        public string? status { get; set; }

    }
    public class data
    {
        public string? id { get; set; }
        public string? http_method { get; set; }

        public string? status { get; set; }
        public payload? payload { get; set; }

    }

    public class payload
    {
        public string? name { get; set; }
        public string? external_id { get; set; }
        public string? url { get; set; }
        public string? published_at { get; set; }

    }


}
