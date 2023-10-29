using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel.ILT
{
    public class APIScheduleCode
    {
        [Required]
        public string ScheduleCode { get; set; }
    }
}
