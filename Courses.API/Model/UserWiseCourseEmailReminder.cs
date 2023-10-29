using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class UserWiseCourseEmailReminder : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [MaxLength(500)]
        public string MailSubject { get; set; }
        public string UserId { get; set; }
        public string TemplateContent { get; set; }

        public int TotalUserCount { get; set; }
    }
}
