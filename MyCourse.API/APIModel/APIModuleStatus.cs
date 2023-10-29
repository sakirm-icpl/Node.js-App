using System;

namespace MyCourse.API.APIModel
{
    public class APIModuleStatus
    {
        public int Moduleid { get; set; }
        public string ModuleName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; }
        public string ModuleType { get; set; }
        public string TrainingType { get; set; }
        public string AssessmentPercentage { get; set; }
        public string ScheduleCode { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TrainingPlace { get; set; }
        public string TrainerName { get; set; }
        public string AcademyAgencyName { get; set; }
        public string AssessmentResult { get; set; }
        public string assLevel { get; set; }
        public string assStatus { get; set; }

    }
}
