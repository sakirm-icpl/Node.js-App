using System.ComponentModel.DataAnnotations;

namespace Competency.API.Model.Competency
{
    public class CompetenciesMapping : BaseModel
    {
        public int? Id { get; set; }
        public int? CourseCategoryId { get; set; }
       
        [Required]
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        [Required]
        public int CompetencyCategoryId { get; set; }
        public int? CompetencySubCategoryId { get; set; }
        public int? CompetencySubSubCategoryId { get; set; }
        [Required]
        public int CompetencyId { get; set; }
        public int? CompetencyLevelId { get; set; }
    }
}
