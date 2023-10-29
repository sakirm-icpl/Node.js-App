using System;

namespace Courses.API.APIModel.ActivitiesManagement
{
    public class APITargetSetting
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string TargetDescription { get; set; }
        public string FrequencyOfAssessment { get; set; }
        public DateTime DateOfAssessment { get; set; }
        public string Status { get; set; }
    }
}
