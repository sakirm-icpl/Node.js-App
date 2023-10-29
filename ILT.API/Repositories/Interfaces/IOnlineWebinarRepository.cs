using ILT.API.APIModel;
using ILT.API.Model.ILT;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IOnlineWebinarRepository
    {
        Task<AuthenticationResult> GetTeamsToken();
        Task<List<TeamsScheduleDetails>> CallTeamsEventCalendars(string TeamsAccessToken, Teams teams, int userId, string objectID, string MeetingId);
        Task<List<TeamsScheduleDetails>> PostTeamsMeeting(Teams teams, int userId);
        Task<TeamsScheduleDetails> UpdateTeamsMeeting(int userId, Teams teams, UserWebinarMaster userWebinarMaster, int id, AuthenticationResult authenticationResult);
        Task<TeamsScheduleDetails> cancleTeamsMeeting(int userId, int scheduleID, UserWebinarMaster userWebinarMaster, TeamsScheduleDetails teamsScheduleDetails);
        Task<EventAttendee[]> CallGSuitUpdateEventCalendars(UpdateGsuit teams, List<EventAttendee> gsuitattendees);
        Task<GoogleMeetRessponce> CallGSuitEventCalendars(MeetDetails teams, int userId);
        Task<GoogleMeetRessponce> UpdateGSuitEventCalendars(UpdateGsuitMeeting meet, int userId);
        Task<GoogleMeetRessponce> cancleGsuitMeeting(int userId, UserWebinarMaster userWebinarMaster, GoogleMeetDetails meetdetails1);
        Task<GoogleMeetAttendees> GetGSuitEventCalendars(GoogleMeetDetails teams);
    }
}
