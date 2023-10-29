using System;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class UserCoursesStatistics
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public int NotStarted { get; set; }
        public int Inprogress { get; set; }
        public int Completed { get; set; }
        public DateTime LastRefreshedDate { get; set; }
    }
}
