using TNA.API.Model;
using System;
using System.Collections.Generic;

namespace TNA.API.APIModel
{
    public class APIILTRequest
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public bool IsNominated { get; set; }
        public int ID { get; set; }
        public int ScheduleID { get; set; }
        public string ScheduleCode { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string TrainingRequesStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public int PlaceID { get; set; }
        public string PlaceName { get; set; }
        public string TrainerType { get; set; }
        public int? AcademyAgencyID { get; set; }
        public string AcademyAgencyName { get; set; }
        public int? AcademyTrainerID { get; set; }
        public string AgencyTrainerName { get; set; }
        public string AcademyTrainerName { get; set; }
        public string TrainerDescription { get; set; }
        public string ScheduleType { get; set; }
        public string City { get; set; }
        public string SeatCapacity { get; set; }
        public string ContactNumber { get; set; }
        public string postalAddress { get; set; }
        public string ContactPersonName { get; set; }
        public string PlaceType { get; set; }
        public string EventLogo { get; set; }
        public string OverallStatus { get; set; }
        public string Cost { get; set; }
        public string Currency { get; set; }
        public bool? IsTrainer { get; set; }
        public int? BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string? Purpose { get; set; }
        public bool? RequestApproval { get; set; }
    }
    public class APIILTRequestRsponse
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public bool IsNominated { get; set; }
        public List<APIRequestScheduleDetails> APIRequestScheduleDetails { get; set; }
        public string OverallStatus { get; set; }
        public APIRequestScheduleDetails APIRequestScheduleDetailsForRegistered { get; set; }
        public APITopicList[] TopicList { get; set; }
    }

    public class APITopicList
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
    }

    public class ReportsToDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
    }
    public class APIILTBatchResponse
    {
        public int BatchId { get; set; }
        public int CourseId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Description { get; set; }
        public string RegionName { get; set; }
        public string BatchRequestStatus { get; set; }
        public string BatchAttendanceStatus { get; set; }
        public string BatchNominationStatus { get; set; }
        public List<APIScheduleDetails> APIScheduleDetailsList { get; set; }
    }
    public class APIScheduleDetails
    {
        public int ScheduleId { get; set; }
        public int ModuleId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public DateTime ScheduleRegistrationEndDate { get; set; }
        public string ScheduleStartTime { get; set; }
        public string ScheduleEndTime { get; set; }
        public string ScheduleType { get; set; }
        public string TrainingRequestStatus { get; set; }
        public string NominationStatus { get; set; }
        public string AttendanceStatus { get; set; }
        public int PlaceID { get; set; }
        public string PlaceName { get; set; }
        public int SeatCapacity { get; set; }
        public string PlaceType { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string CityName { get; set; }
        public string PostalAddress { get; set; }
        public int AcademyAgencyID { get; set; }
        public string AcademyAgencyName { get; set; }
    }
    public class APIILTBatchPreResponse
    {
        public int BatchId { get; set; }
        public int CourseId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Description { get; set; }
        public string RegionName { get; set; }
        public int ScheduleId { get; set; }
        public int ModuleId { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public DateTime ScheduleRegistrationEndDate { get; set; }
        public string ScheduleStartTime { get; set; }
        public string ScheduleEndTime { get; set; }
        public string ScheduleType { get; set; }
        public string TrainingRequestStatus { get; set; }
        public string NominationStatus { get; set; }
        public string AttendanceStatus { get; set; }
        public int PlaceID { get; set; }
        public string PlaceName { get; set; }
        public int SeatCapacity { get; set; }
        public string PlaceType { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string CityName { get; set; }
        public string PostalAddress { get; set; }
        public int AcademyAgencyID { get; set; }
        public string AcademyAgencyName { get; set; }
        public bool IsTrainer { get; set; }
    }
}
