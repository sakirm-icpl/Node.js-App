using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TNA.API.APIModel
{
    public class APIILTTrainingAttendance
    {
        public int? ID { get; set; }
        public int ScheduleID { get; set; }
        public int ModuleID { get; set; }
        public int UserID { get; set; }
        public string EUserID { get; set; }
        public int CourseID { get; set; }
        public bool IsPresent { get; set; }
        public List<UserDetails> UserDetails { get; set; }
        public string OTP { get; set; }
        public bool isWeb { get; set; }
        public string AttendanceStatus { get; set; }
        public string MobileNumber { get; set; }
        public string CourseTitle { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public int AttendanceBy { get; set; }
    }
    public class UserDetails
    {
        [Required]
        public int UserID { get; set; }
        [Required]
        public string EmailId { get; set; }
        public string CurrentDate { get; set; }
        public string CurrentTime { get; set; }
    }

    public class APIILTScheduleDetails
    {
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public int ModuleID { get; set; }
        public string ModuleTitle { get; set; }
        public List<ScheduleList> ScheduleDetails { get; set; }
    }
    public class APIILTCourseDetails
    {
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public int ModuleID { get; set; }
        public string ModuleTitle { get; set; }
        public string CourseType { get; set; }
    }
    public class ScheduleList
    {
        public int ScheduleID { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public string Place { get; set; }
        public string Venue { get; set; }
        public string StartTimeNew { get; set; }
        public string EndTimeNew { get; set; }
    }

    public class APIAttendanceImport
    {
        public string ModuleName { get; set; }
        public string CourseCode { get; set; }
        public string ScheduleCode { get; set; }
        public string UserId { get; set; }
        public string IsPresent { get; set; }
        public string IsWaiver { get; set; }
        public DateTime? AttendanceDate { get; set; }


        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }


    }

    public class APIAbsentUserDetails
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string EmailId { get; set; }
        public string AdminEmail { get; set; }
        public string SkiplevelmanagerEmailId { get; set; }
        public string CourseTitle { get; set; }
        public string ScheduleCode { get; set; }
        public string OrganisationCode { get; set; }

    }
    public class APIILTTrainingAttendanceUsers
    {
        public string EUserID { get; set; }
        public int UserID { get; set; }
        public bool IsPresent { get; set; }
        public string OTP { get; set; }
        public bool IsWeb { get; set; }
        public string AttendanceStatus { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public int AttendanceBy { get; set; }
    }
    public class APIILTAttendanceResponse
    {
        public int UserMasterId { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleCode { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string CourseType { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsApplicableToAll { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool IsPresent { get; set; }
        public bool IsWeb { get; set; }
        public string OTP { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public int AttendanceBy { get; set; }
        public string AttendanceStatus { get; set; }
        public bool IsNominated { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public int TrainerId { get; set; }
        public string TrainerUserId { get; set; }
        public string TrainerName { get; set; }
        public string ManagerName { get; set; }
        public string ManagerEmailId { get; set; }
        public string ManagerMobileNumber { get; set; }
        public string skipLevelManagerName { get; set; }
        public string skipLevelManagerEmailId { get; set; }
        public string skipLevelManagerMobileNumber { get; set; }
        public int AttendanceId { get; set; }
        public bool IsDuplicate { get; set; }
        public string Status { get; set; }
        public string OverallStatus { get; set; }
        public bool IsAttedanceEmailSent { get; set; }
        public string CourseStatus { get; set; }
        public string RecordStatus { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class APITrainingAttendanceImportColumns
    {
        public const string CourseCode = "CourseCode";
        public const string ModuleName = "ModuleName";
        public const string ScheduleCode = "ScheduleCode";
        public const string UserId = "UserId";
        public const string IsPresent = "IsPresent";
        public const string IsWaiver = "IsWaiver";
        public const string AttendanceDate = "AttendanceDate";
    }
}
