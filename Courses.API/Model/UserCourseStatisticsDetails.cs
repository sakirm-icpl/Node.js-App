using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class UserCourseStatisticsDetails
    {
        public int Id { get; set; }
        [Required]
        public int CourseType { get; set; }
        public int NotStarted { get; set; }
        public int NotStartedDuration { get; set; }
        public int Inprogress { get; set; }
        public int InprogressDuration { get; set; }
        public int Completed { get; set; }
        public int CompletedDuration { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}


