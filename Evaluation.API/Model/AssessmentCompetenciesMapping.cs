using System.ComponentModel.DataAnnotations;

namespace Evaluation.API.Model
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
