using System;

namespace Courses.API.Model.CourseDetail
{
    public class CourseInstructor
    {
        public int Id { get; set; }
        public string photoUrl { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
