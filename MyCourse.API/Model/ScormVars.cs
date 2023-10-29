using System;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class ScormVars
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string VarName { get; set; }
        [Required]
        public string VarValue { get; set; }
        [Required]
        public int UserId { get; set; }
        public int NoOfAttempt { get; set; }
        [MaxLength(100)]
        public string CourseGuid { get; set; }
        [Required]
        public int ModuleId { get; set; }
        [Required]
        public int CourseId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
