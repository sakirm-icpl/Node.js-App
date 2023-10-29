using System;

namespace Courses.API.Model
{
    public class TrendingCourse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int Count { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
