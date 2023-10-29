using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseApplicability.API.Model
{
    [Table("ContentCompletionStatus", Schema = "Course")]
    public class ContentCompletionStatus
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string Location { get; set; }

        public int? ScheduleId { get; set; }
        public bool? IsUserConsent { get; set; }
    }
}
