using System;
using System.Collections.Generic;

namespace ILT.API.APIModel
{
    public class APITrainingNominationImport
    {
        public string UserId { get; set; }
        public string CourseName { get; set; }
        public string ScheduleCode { get; set; }
        public string ModuleName { get; set; }
        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
        public string OTP { get; set; }
    }
    public class APITrainingNominationPath
    {
        public string Path { get; set; }
    }
    public class APITrainingNominationImportColumns
    {
        public const string CourseCode = "CourseCode";
        public const string BatchCode = "BatchCode";
        public const string ModuleName = "ModuleName";
        public const string ScheduleCode = "ScheduleCode";
        public const string UserId = "UserId";
        public const string UserIdEncrypted = "UserIdEncrypted";
        public const string OTP = "OTP";
    }
    public class APITrainingNominationImportResult
    {
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public int CourseId { get; set; }
        public bool IsApplicableToAll { get; set; }
        public string ModuleName { get; set; }
        public int ModuleId { get; set; }
        public string ScheduleCode { get; set; }
        public int ScheduleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int PlaceId { get; set; }
        public string PlaceName { get; set; }
        public string PostalAddress { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string UserId { get; set; }
        public string UserIdEncrypted { get; set; }
        public int UserMasterId { get; set; }
        public string UserName { get; set; }
        public string MobileNumber { get; set; }
        public string OTP { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string Action { get; set; }
        public string WebinarType { get; set; }
    }
    public class APITrainingNominationNotification
    {
        public int CourseId { get; set; }
        public List<ApiNotification> aPINotification { get; set; }
    }
}
