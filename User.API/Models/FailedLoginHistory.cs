using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class FailedLoginHistory
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(1000)]
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
