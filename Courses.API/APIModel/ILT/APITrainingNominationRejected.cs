using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel.ILT
{
    public class APITrainingNominationRejected
    {
        public string CourseName { get; set; }
        public string ModuleName { get; set; }
        public string ScheduleCode { get; set; }
        public string UserId { get; set; }
        public string ErrMessage { get; set; }
    }
}
