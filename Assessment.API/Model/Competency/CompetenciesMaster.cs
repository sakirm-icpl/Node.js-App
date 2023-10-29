using Assessment.API.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Model.Competency
{
    [Table("CompetenciesMaster", Schema = "Course")]

    public class CompetenciesMaster : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubSubCategoryId { get; set; }
        [Required]
        [MaxLength(50)]
        public string? CompetencyName { get; set; }
        [Required]
        [MaxLength(100)]
        public string? CompetencyDescription { get; set; }
    }
}
