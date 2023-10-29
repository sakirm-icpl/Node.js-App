using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.Competency
{
    public class RolewiseCompetenciesMapping : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int RoleId { get; set; }
        [Required]
        [MaxLength(200)]
        public string RoleName { get; set; }
        [Required]
        public int CompetencyCategoryId { get; set; }
        [Required]
        public int CompetencyId { get; set; }
    }
}
