using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Models
{
    [Table("AssessmentSheetConfiguration", Schema = "Course")]

    public class AssessmentSheetConfiguration : CommonFields
    {
        public int ID { get; set; }
        [MaxLength(50)]
        [Required]
        public int PassingPercentage { get; set; }
        public int MaximumNoOfAttempts { get; set; }
        [MaxLength(50)]
        [Required]
        public int Durations { get; set; }
        public bool? IsFixed { get; set; }
        public int? NoOfQuestionsToShow { get; set; }
        public bool? IsNegativeMarking { get; set; }
        [DefaultValue(false)]
        public bool? IsRandomQuestion { get; set; }
        public int? NegativeMarkingPercentage { get; set; }
        public bool? IsEvaluationBySME { get; set; }
    }
}
