using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.Competency
{
    public class AssessmentCompetenciesMapping : BaseModel
    {
        public int? Id { get; set; }       
        [Required]
        public int AssessmentQuestionId { get; set; }  
        [Required]
        public int CompetencyId { get; set; }      
    }
}
