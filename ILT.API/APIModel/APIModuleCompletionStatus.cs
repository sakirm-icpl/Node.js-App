using System.ComponentModel.DataAnnotations;

namespace ILT.API.APIModel
{
    public class APIModuleCompletionStatus
    {
        public int Id { get; set; }
        [Required]
        public int ModuleId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
