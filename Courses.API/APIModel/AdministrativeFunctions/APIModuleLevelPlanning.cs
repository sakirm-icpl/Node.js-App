using System;

namespace Courses.API.APIModel.AdministrativeFunctions
{
    public class APIModuleLevelPlanning
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
    }

    public class APIModuleLevelPlanningDetail
    {
        public int? Id { get; set; }
        public int TrainingPlaceId { get; set; }
        public int ModuleLevelPlanningId { get; set; }
        public DateTime StartDate { get; set; }
        public string StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public string EndTime { get; set; }
        public int CoTrainerId { get; set; }
        public int HRCoOrdinatorId { get; set; }
    }
}
