using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model
{
    public class CoursesEnrollRequest
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]

        public string CourseTitle { get; set; }
        public Course Course { get; set; }
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Status { get; set; }

        public DateTime Date { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class CoursesEnrollRequestDetails
    {
        public int? Id { get; set; }

        public string Status { get; set; }

        public string Reason { get; set; }

        public DateTime DateCreated { get; set; }

        public int CoursesEnrollRequestId { get; set; }

        public int ActionTakenBy { get; set; }

        public bool IsDeleted { get; set; }
    }

}
