using System.ComponentModel.DataAnnotations;

namespace Competency.API.Model.Competency
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
