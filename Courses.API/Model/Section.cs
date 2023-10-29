using Assessment.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class Section : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string CourseCode { get; set; }
        public int SectionNumber { get; set; }
    }
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int LessonNumber { get; set; }
        public int SectionId { get; set; }
    }
}
