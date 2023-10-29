using System;

namespace ILT.API.Model.ILT
{
    public class APIRequestScheduleDetails
    {
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
        public string Cost { get; set; }
        public string Currency { get; set; }
        public bool? IsTrainer { get; set; }
        public int? BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string? Purpose { get; set; }
        public string? OverallStatus { get; set; }
        public bool? RequestApproval { get; set; }
    }
}
