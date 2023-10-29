using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class BatchAnnouncement : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string BatchCode { get; set; }
        [MaxLength(200)]
        [Required]
        public string BatchTitle { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime LastRegistrationDate { get; set; }
        [Required]
        [MaxLength(50)]
        public string SelectValue { get; set; }
        [Required]
        [MaxLength(50)]
        public string RegistrationLimit { get; set; }
        [MaxLength(200)]
        public string EventBadgeFile { get; set; }
        [Required]
        public int CourseId { get; set; }
    }
}
