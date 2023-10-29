using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class CourseLog 
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public string CourseGUID { get; set; }
    }
}
