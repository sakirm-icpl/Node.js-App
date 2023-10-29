using Assessment.API.Models;
using System;

namespace Courses.API.APIModel
{
    public class APICourseWiseEmailReminder
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalUserCount { get; set; }
    }

    public class APICourseWiseSMSReminder : CommonFields
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int TotalUserCount { get; set; }
    }
}
