using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class ScormVarResult
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int ModuleId { get; set; }
        [MaxLength(20)]
        public string Result { get; set; }
        public int NoOfAttempts { get; set; }
        public float? Score { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
