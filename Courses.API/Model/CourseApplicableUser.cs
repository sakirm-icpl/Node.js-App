namespace Courses.API.Model
{
    public class CourseApplicableUser
    {
        public static int courseId { get; internal set; }
        public string UserID { get; internal set; }
        public string UserName { get; internal set; }
    }

    public class CategoryApplicableUser
    {
        public static int CategoryId { get; internal set; }
        public string UserID { get; internal set; }
        public string UserName { get; internal set; }
    }
}
