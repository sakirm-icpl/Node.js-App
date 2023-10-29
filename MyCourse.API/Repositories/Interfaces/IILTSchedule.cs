using MyCourse.API.APIModel;
using MyCourse.API.Model;
using MyCourse.API.Common;
//using MyCourse.API.Model.ILT;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IILTSchedule : IRepository<ILTSchedule>
    {
        //Task<APIILTSchedular> GetByID(int Id);
        //Task<List<APIILTSchedular>> GetAllActiveSchedules(int page, int pageSize, int UserId, string OrganisationCode, string schduleType, string search = null, string searchText = null);
        //Task<int> GetAllActiveSchedulesCount(int UserId, string OrganisationCode, string search = null, string searchText = null,bool showAllData =false);
        //Task<ApiResponse> PostILT(APIILTSchedular aPIILTSchedular, int userId, string OrgCode);
        //Task<List<TypeAhead>> GetAcademyTypeAhead(string trainerType, string search = null);
        //Task<List<TypeAhead>> GetAcademyData(string trainerType);
        //Task<int> CancellationSchedule(int scheduleId, string reason, string OrganisationCode);
        Task<ScheduleCode> GetScheduleCode(int UserId);
        //Task<List<ILTSchedule>> GetByModuleID(int id);
        //Task<bool> CheckModuleName(string moduleName);
        //Task<string> SaveImage(IFormFile uploadedFile, int UserId);
        //Task<int> SendScheduleCreationBellNotification(int? courseId, int scheduleid, string token, string organizationcode);
        //Task<bool> CourseEnrolledByTNA(int? courseId);
        //Task<bool> AddCalendarData(string summary, string location, DateTime startDate, DateTime endDate, string eamilID);
        //Task<bool> AddCalendarEvent(string summary, string location, DateTime startDate, DateTime endDate, string eamilID);
        //Task<List<APIILTSchedular>> GetAllSchedules();
        //Task<List<APIILTSchedularExport>> GetAllSchedulesForExport(int UserId, string OrganizationCode,bool showAllData = false);
        //Task<APIZoomDetailsToken> CreateZoomMeeting(string code);
        //Task<ILTOnlineSetting> GetZoomConfiguration();
        //Task<bool> AddUserAsTrainer(APIUserAsTrainer aPIUserAsTrainer, int UserId);
        //Task<List<APIUserAsTrainer>> GetUserAsTrainer(APIGetUserAsTrainer aPIGetUserAsTrainer);
        //Task<int> GetUserAsTrainerCount(APIGetUserAsTrainer aPIGetUserAsTrainer);

        //Task<TeamsAccessToken> addUpdateTeamsAccessToken(TeamsAccessToken teamsAccessToken);
        //Task<Response> StartBigBlueMeeting(string meetingID,int UserId,string UserName);
        //bool ValidateSQLInjection(APIILTSchedular aPIILTSchedular);
        //Task CancelScheduleCode(APIScheduleCode aPIScheduleCode, int UserId);
        //Task<ApiResponse> PutILT(APIILTSchedular aPIILTSchedular, int userId, string OrganisationCode);
        //Task<ApiResponse> ProcessImportFile(APIILTScheduleImport aPIILTScheduleImport, int UserId, string OrgCode);
        //Task<byte[]> ExportImportFormat(string OrgCode);
        //Task<FileInfo> ExportILTScheduleReject(string OrgCode);
        //Task<List<TeamsScheduleDetails>> PostTeamsMeeting(Teams teams, int userId, string organisationCode);
        //Task<HttpResponseMessage> GetTeamsMeeting(string JoinUrl);
        //Task<List<TeamsScheduleDetailsV2>> GetTeamsDetails(string ScheduleId);
        //Task<MeetingsReportData> GetMeetingDetails(string MeetingId, string Username, string Code);
        //string GetMeetingScheduleEmail(int? TeamsUserId);
        //Task<Meeting> GetTeamsMeetingReportForId(string meetingId, string reportId, string Username);
        //Task<TeamsScheduleDetails> UpdateTeamsMeeting(UpdateTeams teams, int userId, string organisationCode, int id, AuthenticationResult authenticationResult, HolidayList[] HolidayList);
        //FileInfo ExportTeamsMeetingReportForId(ExportTeamsMeetingReport[] exportTeamsMeetingReports, string ExportAs);
        //FileInfo ExportZoomMeetingReportForId(ZoomReport zoomReport, string ExportAs);
        //Task<List<ILTSchedule>> GetTeamsMeetingsDetails(string ScheduleId);
        //Task<List<ILTSchedule>> GetWebinarMeetingsDetails(int ScheduleId);
        //string ZoomToken();
        //Task<APIZoomMeetingResponce> CallZoomMeetings(APIZoomDetailsToken obj, Teams aPIILTSchedular, int userId);
        //Task<ZoomMeetingDetailsV2> GetZoomDetails(string ScheduleId);
        //Task<int> UpdateZoomMeeting(UpdateZoom zoom, int userId);
        //Task<ZoomMeetingParticipants> GetZoomMeetingParticipants(long UniqueZoomMeetingId);
        //Task<ZoomMeetingDetailsForReport> ZoomMeetingDetailsForReport(long UniqueZoomMeetingId);
        //Task<AuthenticationResult> GetTeamsToken(IPublicClientApplication app, string Username, int meeting);
        //IPublicClientApplication GetTeamsAuthentication();
        //Task<GoogleMeetRessponce> PostGsuitMeet(MeetDetails teams, int userId, string organisationCode);
        //Task<GoogleMeetRessponce> UpdateGsuitMeet(UpdateGsuitMeeting meet, int userId, string organisationCode);
        //Task<List<APIILTSchedular>> GetAllActiveSchedulesV2(ApiScheduleGet apiScheduleGet, int UserId, string OrganisationCode);
        //Task<GoogleMeetDetailsV2> GetGsuitDetails(int ScheduleId);
        //Task<List<TeamsScheduleDetails>> CancelRemoveDatesSchedule(List<UpdateTeams> teams, int userid);
        //Task<AuthenticationResult> GetTeamsToken();
    }
}
