using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.API.Models
{
    [Table("Course", Schema = "Course")]
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public float CourseFee { get; set; }
        public float GroupCourseFee { get; set; }
        public bool IsDeleted { get; set; }
        public string? CourseURL { get; set; }
    }
}
