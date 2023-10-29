using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Model.ILT
{
    public class ILTScheduleRejected
    {
        public int Id { get; set; }
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
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
