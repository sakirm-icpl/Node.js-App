using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
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
