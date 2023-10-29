using System.ComponentModel.DataAnnotations;

namespace Competency.API.Model.Competency
{
    public class RolewiseCourseMapping : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int JobRoleId { get; set; }       
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int ApplicableFromDays { get; set; }
    }
}
