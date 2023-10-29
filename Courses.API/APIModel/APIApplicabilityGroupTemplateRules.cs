using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIApplicabilityGroupTemplateRules
    {
        public int? Id { get; set; }
        [MaxLength(50)]
        public string ApplicabilityGroupName { get; set; }
        [MaxLength(150)]
        public string ApplicabilityGroupDescription { get; set; }
        [MaxLength(50)]
        public string ApplicabilityParameter1 { get; set; }
        [MaxLength(50)]
        public string ApplicabilityValue1 { get; set; }
        [Range(0, int.MaxValue)]
        public int ApplicabilityValueId1 { get; set; }
        [MaxLength(5)]
        public string Condition1 { get; set; }
        [MaxLength(50)]
        public string ApplicabilityParameter2 { get; set; }
        [MaxLength(50)]
        public string ApplicabilityValue2 { get; set; }
        [Range(0, int.MaxValue)]
        public int ApplicabilityValueId2 { get; set; }
        [MaxLength(5)]
        public string Condition2 { get; set; }
        public string ApplicabilityParameter3 { get; set; }
        [MaxLength(50)]
        public string ApplicabilityValue3 { get; set; }
        [Range(0, int.MaxValue)]
        public int ApplicabilityValueId3 { get; set; }
        [MaxLength(5)]
        public string Condition3 { get; set; }
        [MaxLength(50)]
        public string ApplicabilityParameter4 { get; set; }
        [MaxLength(50)]
        public string ApplicabilityValue4 { get; set; }
        [Range(0, int.MaxValue)]
        public int ApplicabilityValueId4 { get; set; }
        [MaxLength(5)]
        public string Condition4 { get; set; }
        public string ApplicabilityParameter5 { get; set; }
        [MaxLength(50)]
        public string ApplicabilityValue5 { get; set; }
        [Range(0, int.MaxValue)]
        public int ApplicabilityValueId5 { get; set; }
        public string ErrorMessage { get; set; }
    }
}
