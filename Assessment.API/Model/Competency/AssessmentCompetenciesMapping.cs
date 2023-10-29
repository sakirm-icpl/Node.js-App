using Assessment.API.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Model.Competency
{
    [Table("AssessmentCompetenciesMapping", Schema = "Course")]

    public class AssessmentCompetenciesMapping : BaseModel
    {
        public int? Id { get; set; }
        [Required]
        public int AssessmentQuestionId { get; set; }
        [Required]
        public int CompetencyId { get; set; }
    }
}
