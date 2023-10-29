using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class Log_ClearBookMarking
    {
        public int Id { get; set; }
        public int Modifiedby { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CourseID { get; set; }
        public int ModuleID { get; set; }
        public int UserID { get; set; }
    }
}
