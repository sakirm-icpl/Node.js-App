using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APIUserAttendanceDetails
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string EmailId { get; set; }
        public string CourseName { get; set; }
        public string ScheduleCode { get; set; }
        public string OrganisationCode { get; set; }
        public string ModuleName { get; set; }
        public bool IsFeedback { get; set; }
        public int CourseId { get; set; }
        public int Id { get; set; }
        public DateTime AttendanceDate { get; set; }
    }
}
