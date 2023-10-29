using System.Collections.Generic;

namespace CourseReport.API.APIModel
{
    public class ApiCalendarCourseModuleReport
    {
        public List<APICalendarCourseReport> Courses { get; set; }
        public List<APICalendarModuleReport> Modules { get; set; }
    }
}
