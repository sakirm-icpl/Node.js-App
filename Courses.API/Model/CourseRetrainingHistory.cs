using System.ComponentModel.DataAnnotations;
using System;

namespace Courses.API.Model
{
    public class CourseRetrainingHistory 
    {
        public int Id { get; set; }

        [Required]   
        public int UserID { get; set; }

        [Required]
        public int CourseID { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
