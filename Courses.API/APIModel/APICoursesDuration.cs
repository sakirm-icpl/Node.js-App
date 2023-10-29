using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APICoursesDuration
    {
        public decimal UserAssignedHours { get; set; }
        public decimal UserTimeSpent { get; set; }
    }
    public class CoursesDuration
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
