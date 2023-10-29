using System.ComponentModel.DataAnnotations.Schema;

namespace CourseReport.API.Model
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
        public bool IsExternalProvider { get; set; }
        public string ExternalProvider { get; set; }
    }
}
