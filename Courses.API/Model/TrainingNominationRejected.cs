using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class TrainingNominationRejected : CommonFields
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CourseName { get; set; }
        public string ScheduleCode { get; set; }
        public string ModuleName { get; set; }
        public string ErrMessage { get; set; }

    }
}
