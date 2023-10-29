using System.ComponentModel.DataAnnotations;

namespace Assessment.API.Models
{
    public class AssessmentConfiguration : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string Attribute { get; set; }
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }
        [Required]
        [MaxLength(50)]
        public string Value { get; set; }

    }
}
