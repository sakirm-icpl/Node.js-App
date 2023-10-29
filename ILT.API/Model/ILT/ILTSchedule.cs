using System;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.ILT
{
    public class ILTSchedule : BaseModel
    {
        public int ID { get; set; }
        public string ScheduleCode { get; set; }
        public int CourseId { get; set; }
        public int? BatchId { get; set; }
        public int ModuleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public int PlaceID { get; set; }
        public string TrainerType { get; set; }
        public int? AcademyAgencyID { get; set; }
        public int? AcademyTrainerID { get; set; }
        public string AcademyTrainerName { get; set; }
        public string AgencyTrainerName { get; set; }
        public string TrainerDescription { get; set; }
        public string ScheduleType { get; set; }
        public string ReasonForCancellation { get; set; }
        public string EventLogo { get; set; }
        public float Cost { get; set; }
        [MaxLength(25)]
        public string Currency { get; set; }
        public string WebinarType { get; set; }
        public string? Purpose { get; set; }
        public int ScheduleCapacity { get; set; } = 0;
        public bool? RequestApproval { get; set; } // flag added for wns for schedule go through request approve by admin null for non wns
    }
}
