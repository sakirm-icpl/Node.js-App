using System.ComponentModel.DataAnnotations;

namespace Evaluation.API.Model
{
    public class CompetenciesMaster : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubSubCategoryId { get; set; }
        [Required]
        [MaxLength(50)]
        public string CompetencyName { get; set; }
        [Required]
        [MaxLength(100)]
        public string CompetencyDescription { get; set; }
    }
}
