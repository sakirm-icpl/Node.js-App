using Assessment.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class CourseWiseEmailReminder : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [MaxLength(500)]
        public string MailSubject { get; set; }
        public string TemplateContent { get; set; }

        public int TotalUserCount { get; set; }

    }
    public class CourseWiseSMSReminder : CommonFields
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int TotalUserCount { get; set; }

    }
}
