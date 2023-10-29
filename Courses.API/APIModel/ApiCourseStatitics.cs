using System;

namespace Courses.API.APIModel
{
    public class ApiCourseStatitics
    {
        public int CompletedCourseCount { get; set; }
        public int InprogressCourseCount { get; set; }
        public int NotStartedCourseCount { get; set; }
        public DateTime LastRefreshedDate { get; set; }
    }
}
