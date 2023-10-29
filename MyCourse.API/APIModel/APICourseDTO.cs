using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.APIModel
{
    public class APICourseDTO
    {
        public int Id { get; set; }
        [MaxLength(150)]
        public string Title { get; set; }
        [MaxLength(50)]
        public string CourseType { get; set; }
    }
    public class APICoursesData
    {
        public int Id { get; set; }
        [MaxLength(150)]
        public string Title { get; set; }
        [MaxLength(50)]
        public string CourseType { get; set; }
        public int CreatedBy { get; set; }
    }
    public class APICourseTypeahead
    {
        public int Id { get; set; }
        [MaxLength(150)]
        public string Title { get; set; }
        public string CourseType { get; set; }

    }
}
