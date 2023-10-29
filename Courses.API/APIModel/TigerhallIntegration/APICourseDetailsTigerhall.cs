using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel.TigerhallIntegration
{
    public class APICourseDetailsTigerhall
    {
        public List<successDataPoints> successDataPoints { get; set; }
        public List<failedDataPoints> failedDataPoints { get; set; }
    }
    public class successDataPoints
    {
       public List<string> CourseID { get; set; }
    }
    public class failedDataPoints
    {
        public List<string> CourseID { get; set; }
    }


}
