using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIScorm
    {
        [Required]
        public string VarName { get; set; }
        [Required]
        public string VarValue { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
    }
}
