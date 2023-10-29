using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class ApiSection
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string CourseCode { get; set; }
        [Required]
        public int SectionNumber { get; set; }
        public APILesson[] ApiLessons { get; set; }
    }


    public class APILesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int LessonNumber { get; set; }
        public int SectionId { get; set; }
    }
}
