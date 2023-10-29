using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class UserMemo
    {
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int UserId { get; set; }
        public bool IsSubmited { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
