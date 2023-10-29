using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APICourseWiseEmailReminderNew : CommonFields
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalUserCount { get; set; }
    }
}
