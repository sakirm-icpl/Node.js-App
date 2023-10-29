using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
    public class GradingRules : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int ModelId { get; set; }

        [MaxLength(100)]
        public string GradingRuleID { get; set; }
        [Required]
        [MaxLength(100)]
        public string ScorePercentage { get; set; }
        [Required]
        [MaxLength(100)]
        public string Grade { get; set; }
    }
}
