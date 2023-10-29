using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIBatchAnnouncement
    {
        public int? Id { get; set; }
        [Range(0, int.MaxValue)]
        public int CourseId { get; set; }
        [MaxLength(100)]
        public string BatchCode { get; set; }
        [MaxLength(200)]
        public string BatchTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastRegistrationDate { get; set; }
        [MaxLength(50)]
        public string SelectValue { get; set; }
        [MaxLength(50)]
        public string RegistrationLimit { get; set; }
        [MaxLength(200)]
        public string EventBadgeFile { get; set; }
        [MaxLength(150)]
        public string courseTitle { get; set; }
    }
}
