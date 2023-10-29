using Survey.API.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APISurveyManagementPut
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        [Required]
        [MaxLength(500)]
        //[RegularExpression("^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;: ]*$")]
        [CSVInjection]
        public string SurveySubject { get; set; }
        public string SurveyPurpose { get; set; }
        [Required]
       
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ValidityDate { get; set; }
        [Required]
        [Range(1, 120, ErrorMessage = "Average Respond Time must in the range of 1 to 120")]
        public int AverageRespondTime { get; set; }
        public string ApplicabilityParameter { get; set; }
        public string ApplicabilityParameterValue { get; set; }
        public int? ApplicabilityParameterValueId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Target Response Count must in the range of 1 to int MaxValue")]
        public int TargetResponseCount { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int LcmsId { get; set; }
        public bool Status { get; set; }
        public string LcmsName { get; set; }

        public bool IsApplicableToAll { get; set; }
    }
}
