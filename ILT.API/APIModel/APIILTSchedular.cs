using ILT.API.Helper;
using ILT.API.Model.ILT;
using ILT.API.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.APIModel
{
    public class APIILTSchedular
    {
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
        public int ModuleId { get; set; }
        public int? BatchId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Required]
        public string StartTime { get; set; }
        [Required]
        public string EndTime { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime RegistrationEndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int PlaceID { get; set; }
        public string TrainerType { get; set; }
        //[Required]
        public string PlaceName { get; set; }
        public string ModuleName { get; set; }
        [Required]
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public int? AcademyAgencyID { get; set; }
       // [Required]
        public string AcademyAgencyName { get; set; }
        public int? AcademyTrainerID { get; set; }
        public TrainerList[] TrainerList { get; set; }
        public HolidayList[] HolidayList { get; set; }
        public TopicList[] TopicList { get; set; }
        public string AgencyTrainerName { get; set; }
        public string AcademyTrainerName { get; set; }
        public string TrainerDescription { get; set; }
        public string ScheduleType { get; set; }
        public string ReasonForCancellation { get; set; }
       // [Required]
        public string City { get; set; }
       // [Required]
        public string SeatCapacity { get; set; }
        public string ContactNumber { get; set; }
     //   [Required]
        public string postalAddress { get; set; }
        public string ContactPersonName { get; set; }
       // [Required]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.Internal, CommonValidation.External,CommonValidation.Other })]
        public string PlaceType { get; set; }
        public int CourseID { get; set; }
        public bool Status { get; set; }
        public string EventLogo { get; set; }
        [Range(0, float.MaxValue, ErrorMessage = "Cost must be a positive number")]
        public float Cost { get; set; }
        public string Currency { get; set; }
        public string WebinarType { get; set; }

        public string zoomCode { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string UserName { get; set; }
        public bool UserCreated { get; set; }
        public List<TeamsScheduleDetails> teamsScheduleDetails { get; set; }
        public APIZoomMeetingResponce zoomScheduleDetails { get; set; }
        public GoogleMeetDetails googleMeetDetails { get; set; }
        public string? Purpose { get; set; }
        public int ScheduleCapacity { get; set; } = 0;

        #region "Fields specific to bigBlueButton"
        public bool Record { get; set; } = false;

        public bool AutoStartRecording { get; set; } = false;

        public bool AllowStartStopRecording { get; set; } = false;
        public bool? RequestApproval { get; set; } // flag added for wns for schedule go through request approve by admin null for non wns
        #endregion
    }

    public class APIILTSchedularExport 
    {
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
        public int ModuleId { get; set; }
        public int? BatchId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int PlaceID { get; set; }
        public string TrainerType { get; set; }
        public string PlaceName { get; set; }
        public string ModuleName { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public int? AcademyAgencyID { get; set; }
        public string AcademyAgencyName { get; set; }
        public string AcademyTrainerID { get; set; }
        public TrainerList[] TrainerList { get; set; }
        public string AgencyTrainerName { get; set; }
        public string AcademyTrainerName { get; set; }
        public string TrainerDescription { get; set; }
        public string ScheduleType { get; set; }
        public string ReasonForCancellation { get; set; }
        public string City { get; set; }
        public string SeatCapacity { get; set; }
        public string ContactNumber { get; set; }
        public string postalAddress { get; set; }
        public string ContactPersonName { get; set; }
        public string PlaceType { get; set; }
        public int CourseID { get; set; }
        public bool Status { get; set; }
        public string EventLogo { get; set; }
        public float Cost { get; set; }
        public string Currency { get; set; }
        public string Region { get; set; }
        public string TopicName { get; set; }
        public string DepartmentOfTrainer { get; set; }
        public string SubFunctionOfTrainer { get; set; }
        public string ConductedBy { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string NominationCount { get; set; }
        public string LastModified { get; set; }

    }
    public class SchedularTypeahead
    {
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
    }
    public class TrainerList
    {
        public int? AcademyTrainerID { get; set; }
        [Required]
        public string AcademyTrainerName { get; set; }
        [Required]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.Internal, CommonValidation.External, CommonValidation.Consultant,CommonValidation.Other })]
        public string TrainerType { get; set; }
        public string TrainerEmail { get; set; }
        public string? NameUserId { get; set; }
    }
    public class TrainerListWithUserNameId
    {
        public int? AcademyTrainerID { get; set; }
        [Required]
        public string AcademyTrainerName { get; set; }
        [Required]
        [CommonValidationAttribute(AllowValue = new string[] { CommonValidation.Internal, CommonValidation.External, CommonValidation.Consultant })]
        public string TrainerType { get; set; }
    }

    public class ScheduleCancellation
    {
        public string ScheduleID { get; set; }
        public string Reason { get; set; }
    }

    public class HolidayList
    {
        public DateTime Date { get; set; }
        public bool IsHoliday { get; set; }
        public string Reason { get; set; }
    }

    public class APIUserAsTrainer
    {
        public string TrainerId { get; set; }
        public string TrainerUserId { get; set; }
        public string UserName { get; set; }
        public int CountTotalRecord { get; set; }
    }

    public class APIGetUserAsTrainer
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public string search { get; set; }
    }
    public class EnrollmentSchedular
    {
        public int CourseId { get; set; }
        public int moduleId { get; set; }
        public int scheduleId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string BU_EmailID { get; set; }
        public string LM_EmailID { get; set; }
        public string LA_EmailID { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public string CourseTitle { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public string TrainingPlace { get; set; }
        public string Comments { get; set; }
        public string OrganizationCode { get; set; }
        public int LM_ID { get; set; }
        public int LA_ID { get; set; }
        public string ModuleName { get; set; }
        public string Reason { get; set; }
    }
    public class APIILTScheduleImport
    {
        public string Path { get; set; }
    }
    public class APIILTScheduleImportColumns
    {
        public const string CourseCode = "CourseCode";
        public const string BatchCode = "BatchCode";
        public const string ModuleName = "ModuleName";
        public const string StartDate = "StartDate";
        public const string EndDate = "EndDate";
        public const string StartTime = "StartTime";
        public const string EndTime = "EndTime";
        public const string RegistrationEndDate = "RegistrationEndDate";
        public const string TrainerType = "TrainerType";
        public const string TrainerName = "TrainerName";
        public const string TrainerNameEncrypted = "TrainerNameEncrypted";
        public const string TrainingPlaceType = "TrainingPlaceType";
        public const string AcademyAgencyName = "AcademyAgencyName";
        public const string TrainingPlaceName = "TrainingPlaceName";
        public const string SeatCapacity = "SeatCapacity";
        public const string City = "City";
        public const string Venue = "Venue";
        public const string CoordinatorName = "CoordinatorName";
        public const string ContactNumber = "ContactNumber";
        public const string Currency = "Currency";
        public const string Cost = "Cost";
        public const string ScheduleCapacity = "ScheduleCapacity";
        public const string ScheduleCode = "ScheduleCode";
        public const string vilt = "ViltType";
        public const string viltcredential = "ViltCredentials";
    }
    public class APIILTScheduleRejected
    {
        public string CourseCode { get; set; }
        public string BatchCode { get; set; }
        public string ModuleName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string RegistrationEndDate { get; set; }
        public string TrainerType { get; set; }
        public string TrainerName { get; set; }
        public string TrainingPlaceType { get; set; }
        public string AcademyAgencyName { get; set; }
        public string TrainingPlaceName { get; set; }
        public string SeatCapacity { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public string CoordinatorName { get; set; }
        public string ContactNumber { get; set; }
        public string Currency { get; set; }
        public string Cost { get; set; }
        public string ScheduleCode { get; set; }
        public string ScheduleCapacity { get; set; }
        public string vilt { get; set; }
        public string viltcredential { get; set; }
        public string CourseType { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class Teams
    {
        public int ScheduleID { get; set; }
        public string Username { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CourseName { get; set; }
        public int CourseID { get; set; }
        public List<TrainersData> TrinerData { get; set; }
        public HolidayList[] HolidayList { get; set; }
    }
    public class TrainersData
    {
        public string EmailId { get; set; }
        public string EmailName { get; set; }
    }
    public class UpdateTeams
    {
        public int id { get; set; }
        public int ScheduleID { get; set; }
        public int userWebinarId { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CourseName { get; set; }
        public int CourseID { get; set; }
        public string MeetingId { get; set; }
    }
    public class UpdateTeamsV2
    {
        public List<UpdateTeams> updateTeams { get; set; }
        public HolidayList[] HolidayList { get; set; }
    }
    public class TeamsScheduleCode
    {
        public string Code { get; set; }
    }
    public class ScheduleDetails
    {
        public int Id { get; set; }
    }
    public class WebinarSchedule
    {
        public int Code { get; set; }
    }
    public class MeetingsReportData
    {
        public string[] ReportId { get; set; }
        public string MeetingId { get; set; }
        public string Code { get; set; }
        public int[] totalParticipantCount { get; set; }
    }
    public class Meeting
    {
        public string id { get; set; }
        public int totalParticipantCount { get; set; }
        public string meetingStartDateTime { get; set; }
        public string meetingEndDateTime { get; set; }
        public AttendanceRecored[] attendanceRecords { get; set; }
    }
    public class TeamsMeeting
    {
        public string data { get; set; }
        public Meeting[] value { get; set; }
    }
    public class AttendanceRecored
    {
        public string id { get; set; }
        public string emailAddress { get; set; }
        public int totalAttendanceInSeconds { get; set; }
        public string role { get; set; }
        public Identitys identity { get; set; }
        public AttendanceIntervals[] attendanceIntervals { get; set; }
    }
    //public class Identitys
    //{
    //    public string id { get; set; }
    //    public string displayName { get; set; }
    //    public string tenantId { get; set; }
    //}
    public class AttendanceIntervals
    {
        public string joinDateTime { get; set; }
        public string leaveDateTime { get; set; }
        public string durationInSeconds { get; set; }
    }
    public class MeetingReportId
    {
        public string reportId { get; set; }
        public string Code { get; set; }
        public string meetingId { get; set; }
    }
    public class ExportTeamsMeetingReport
    {
        public string emailAddress { get; set; }
        public string joinDateTime { get; set; }
        public string leaveDateTime { get; set; }
        public string displayName { get; set; }
        public string role { get; set; }
        public string meetingStartDateTime { get; set; }
        public string meetingEndDateTime { get; set; }
    }
    public class ExportTeams
    {
        public ExportTeamsMeetingReport[] exportTeamsMeetingReport { get; set; }
        public string ExportAs { get; set; }
    }
    public class Zoom
    {
        public int ScheduleID { get; set; }
        public string Username { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CourseName { get; set; }
        public int CourseID { get; set; }
    }
    public class UpdateZoom
    {
        public int ScheduleID { get; set; }
        public int userWebinarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CourseName { get; set; }
        public int CourseID { get; set; }
        public string MeetingId { get; set; }
    }
    public class ExportZoomMeetingReport
    {
        public string name { get; set; }
        public string user_email { get; set; }
        public string start_time { get; set; }
        public string join_time { get; set; }
        public string end_time { get; set; }
        public string leave_time { get; set; }
        public string status { get; set; }
    }
    public class EventMeeting
    {
        public string subject { get; set; }
        public Start start { get; set; }
        public End end { get; set; }
        public Boolean IsOnlineMeeting { get; set; }
        public string OnlineMeetingProvider { get; set; }
        public Attendance[] attendees { get; set; } 
        public organizer organizer { get; set; }
        public Boolean isOrganizer { get; set; }
    }
    public class Body
    {
        public string contentType { get; set; }
        public string content { get; set; }
    } 
    public class Start
    {
        public string dateTime { get; set; }
        public string timeZone { get; set; }
    }
    public class End
    {
        public string dateTime { get; set; }
        public string timeZone { get; set; }
    }
    public class Location
    {
        public string displayName { get; set; }
    }
    public class Attendance 
    {
        public EmailAddress emailAddress { get; set; }
        public string type { get; set; }
        public Status1 status { get; set; }
    } 
    public class organizer
    {
        public EmailAddress emailAddress { get; set; }
    }
    public class EmailAddress
    {
        public string address { get; set; }
        public string name { get; set; }
    }
    public class Status1
    {
        public string response { get; set; }
        public string time{ get; set; }
    }
    public class OnlineMeeting
    {
        public string joinUrl { get; set; }
    }

    public class TeamsEventResponse
    {

        public string id { get; set; }
        public string createdDateTime { get; set; }
        public string lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public string iCalUId { get; set; }
        public string reminderMinutesBeforeStart { get; set; }
        public Boolean isReminderOn { get; set; }

        public string subject { get; set; }

        public Boolean isCancelled { get; set; }

        public Boolean isOrganizer { get; set; }

        public string transactionId { get; set; }

        public string webLink { get; set; }
        public string onlineMeetingUrl { get; set; }
        public Boolean isOnlineMeeting { get; set; }
        public string onlineMeetingProvider { get; set; }
        public OnlineMeeting onlineMeeting { get; set; }
   
        public Start start { get; set; }
        public End end { get; set; }
        public Attendance[] attendees { get; set; }
        public organizer organizer { get; set; }
    
    }

    public class ApiScheduleGet
    {
        [Required]
        public int page { get; set; }
        [Required]
        public int pageSize { get; set; }
        public string searchText { get; set; }
        public string search { get; set; }
        public bool showAllData { get; set; }
    }
    public class ApiNominationGet
    {
        [Required]
        public int page { get; set; }
        [Required]
        public int pageSize { get; set; }
        public string searchParameter { get; set; }
        public string searchText { get; set; }
        public bool showAllData { get; set; }
    }
    public class ApiRoleCourseTypeAhead
    {
       
        public int Id { get; set; }
       
        public string Title { get; set; }
        public int CreatedBy { get; set; }
        public int? AreaId { get; set; }
        public int? LocationId { get; set; }
        public int? GroupId { get; set; }
        public int? BusinessId { get; set; }
    }
    public class TrainerCourseTypeAhead
    {

        public int Id { get; set; }

        public string Title { get; set; }
       
    }

    public class UpdateGsuit
    {
        public string eventId { get; set; }
        public string Username { get; set; }

    }
    public class UpdateGsuitMeeting
    {
        public int ScheduleID { get; set; }
        public string Username { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CourseName { get; set; }
        public int CourseID { get; set; }
        public string eventId { get; set; }
        public TrainersData[] TrinerData { get; set; }
        public HolidayList[] HolidayList { get; set; }
    }

    public class MeetDetails
    {
        public int ScheduleID { get; set; }
        public string Username { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CourseName { get; set; }
        public int CourseID { get; set; }
        public TrainersData[] TrinerData { get; set; }
        public HolidayList[] HolidayList { get; set; }
    }
}
