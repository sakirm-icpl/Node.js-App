using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APICategoryDTO
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Title { get; set; }
    }

    public class APICategoriesCourses
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public int?  CoursesCount { get; set; }
        public List<APICourseByCategory> Courses { get; set; } = new List<APICourseByCategory>();
  
    }
    public class APICourseByCategory
    {
        public int CourseId { get; set; }
        [MaxLength(100)]
        public string Title { get; set; }
    }

    public class TnaCategories
    {
        public string CategoryType { get; set; }
    }
}
