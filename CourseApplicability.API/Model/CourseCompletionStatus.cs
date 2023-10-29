using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseApplicability.API.Model
{
    [Table("CourseCompletionStatus", Schema = "Course")]
    public class CourseCompletionStatus
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
