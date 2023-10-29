using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APIClearScorm
    {
        public int CourseID { get; set; }
        public int ModuleID { get; set; }
        public string UserID { get; set; }

    }
    public class APIBookMarkingData
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public string UserID { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
    }
}
