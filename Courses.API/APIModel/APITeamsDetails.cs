using System;

namespace Courses.API.APIModel
{

    public class APITeamsDetailsGetToken
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string authorize_url { get; set; }
        public string access_token_url { get; set; }
        public string redirect_uri { get; set; }
        public string authorization_method { get; set; }
        public string uri { get; set; }
    }
    public class APIbody
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }
    public class APIstart
    {
        public string dateTime { get; set; }
        public string timeZone { get; set; }
    }
    public class APIend
    {
        public string dateTime { get; set; }
        public string timeZone { get; set; }
    }
    public class APIlocation
    {
        public string displayName { get; set; }
        public string locationType { get; set; }
        public string uniqueId { get; set; }
        public string uniqueIdType { get; set; }
    }
    public class APIorganizer
    {
        public APIemailAddress emailAddress { get; set; }
    }
    public class APIemailAddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }
    public class APIattendees
    {
        public string type { get; set; }
        public APIstatus status { get; set; }
        public APIemailAddress emailAddress { get; set; }
    }

    //public class EventAttendee
    //{
    //    public string Email { get; set; }
    //}

    public class APIPatchattendees
    {
        public APIattendees[] attendees { get; set; }
    }
    public class APIstatus
    {
        public string response { get; set; } = "none";
        public string time { get; set; } = "0001-01-01T00:00:00Z";
    }

    public class APITeamsCreateResponce
    {
        public string id { get; set; }
        public string createdDateTime { get; set; }
        public string lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public string originalStartTimeZone { get; set; }
        public string originalEndTimeZone { get; set; }
        public string iCalUId { get; set; }
        public string reminderMinutesBeforeStart { get; set; }
        public string isReminderOn { get; set; }
        public string hasAttachments { get; set; }
        public string subject { get; set; }
        public string bodyPreview { get; set; }
        public bool isCancelled { get; set; }
        public bool isOrganizer { get; set; }
        public string webLink { get; set; }
        public APIbody body { get; set; }
        public APIstart start { get; set; }
        public APIend end { get; set; }
        public APIlocation location { get; set; }
        public APIattendees [] attendees { get; set; }
        public APIorganizer organizer { get; set; }
    }

    public class APITeamsRequest
    {
        public string subject { get; set; }
        public APIbody body { get; set; }
        public APIstart start { get; set; }
        public APIend end { get; set; }
        public APIlocation location { get; set; }
        public APIattendees [] attendees { get; set; }

    }
    public class TeamsMeet
    { 
        public string startDateTime { get; set; }
        public string endDateTime { get; set; }
        public string subject { get; set; }
        public Participant participants { get; set; }
        public lobbyBypassSettings lobbyBypassSettings { get; set; }
    }
    public class Participant 
    { 
        public Organizer organizer { get; set; }
    }
    public class Organizer
    { 
        public Identity identity { get; set; }
    }   
    public class Identity  
    {
        public User user { get; set; }
    }
    public class User
    {
        public string id { get; set; }
    }
    public class TeamsResponse
    {
        public string id { get; set; }
        //public string creationDateTime{ get; set; }
        public string startDateTime { get; set; }
        public string endDateTime { get; set; }
        public string joinUrl { get; set; }
        public string joinWebUrl { get; set; }
        public string meetingCode { get; set; }
        public string subject { get; set; }
        public Boolean isBroadcast { get; set; }
        public string autoAdmittedUsers { get; set; }
        public string outerMeetingAutoAdmittedUsers { get; set; }
        public Boolean isEntryExitAnnounced { get; set; }
        public string allowedPresenters { get; set; }
        public string allowMeetingChat { get; set; }
        public Boolean allowTeamworkReactions { get; set; }
        public Boolean allowAttendeeToEnableMic { get; set; }
        public Boolean allowAttendeeToEnableCamera { get; set; }
        public Boolean recordAutomatically { get; set; }
        public string[] capabilities { get; set; }
        public string videoTeleconferenceId { get; set; }
        public string externalId { get; set; }
        public string broadcastSettings { get; set; }
        public string audioConferencing { get; set; }
        public string meetingInfo { get; set; }
        public Participants participants { get; set; }

        public lobbyBypassSettings lobbyBypassSettings { get; set; }
    }
    public class Participants
    {
        public Organizers organizer { get; set; }
    }
    public class Organizers
    {
        public string upn { get; set; }
        public string role { get; set; }
        public Identitys identity { get; set; } 
        public string[] attendees { get; set; }
    }
    public class Identitys
    {
        public string acsUser { get; set; }
        public string spoolUser { get; set; }
        public string guest { get; set; }
        public string phone { get; set; }
        public string encrypted { get; set; }
        public string onPremises { get; set; }
        public string acsApplicationInstance { get; set; }
        public string spoolApplicationInstance { get; set; }
        public string applicationInstance { get; set; }
        public string application { get; set; }
        public string device { get; set; }
        public Users user { get; set; }

    }
    public class Users
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string tenantId { get; set; }
        public string identityProvider { get; set; }
    }
    public class lobbyBypassSettings
    {
        public string scope { get; set; }
        public Boolean isDialInBypassEnabled { get; set; }
    }
   
}
