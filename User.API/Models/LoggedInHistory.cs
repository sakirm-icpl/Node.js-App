using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class LoggedInHistory
    {
        public int Id { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        public DateTime LoggedInTime { get; set; }
        public DateTime? LogOutTime { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? LocalLoggedInTime { get; set; }
        public bool? IsLoggedInWeb { get; set; }

        [MaxLength(100)]
        public string IEMI { get; set; }
        [MaxLength(1000)]
        public string BrowserInfo { get; set; }
        [MaxLength(100)]
        public string IPAddress { get; set; }
        public DateTime? LocalLoggedOutTime { get; set; }
        public int TotalTimeInMinutes { get; set; }
    }
}
