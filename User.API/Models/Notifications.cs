using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class Notifications 
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(500)]
        public string Message { get; set; }
        [MaxLength(100)]
        public string Type { get; set; }
        [MaxLength(500)]
        public string Url { get; set; }
        public bool ApplicableToAll { get; set; }

        public int? QuizId { get; set; }
        public int? SurveyId { get; set; }

        public int? CourseId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

    }
    public class ApplicableNotifications : CommonFields
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsRead { get; set; }
        public bool IsReadCount { get; set; }
        public int NotificationId { get; set; }
    }
}
