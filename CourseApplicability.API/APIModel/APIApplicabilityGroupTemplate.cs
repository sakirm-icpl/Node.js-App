using System.ComponentModel.DataAnnotations;

namespace CourseApplicability.API.APIModel
{
    public class APIApplicabilityGroupTemplate
    {
        public int? Id { get; set; }
        [MaxLength(50)]
        public string ApplicabilityGroupName { get; set; }
        [MaxLength(150)]
        public string ApplicabilityGroupDescription { get; set; }
        public ApplicabilityGroupRules[] ApplicabilityGroupRule { get; set; }
    }

    public class ApplicabilityGroupRules
    {
        [MaxLength(50)]
        public string ApplicabilityRule { get; set; }
        [MaxLength(50)]
        public string ParameterValue { get; set; }
        [MaxLength(5)]
        public string Condition { get; set; }
    }

    public class APIScheduleVisibilityTemplate
    {
        public int? Id { get; set; }
        [MaxLength(50)]
        public string ApplicabilityGroupName { get; set; }
        [MaxLength(150)]
        public string ApplicabilityGroupDescription { get; set; }
        public ScheduleVisibilityRules[] ScheduleVisibilityRules { get; set; }
    }

    public class ScheduleVisibilityRules
    {
        [MaxLength(50)]
        public string ApplicabilityRule { get; set; }
        [MaxLength(50)]
        public string ParameterValue { get; set; }
        [MaxLength(5)]
        public string Condition { get; set; }
    }
}

