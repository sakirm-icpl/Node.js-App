using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class UserMasterOtpHistory
    {
        public int Id { get; set; }
        [MaxLength(1000)]
        public int Count { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string IPAddress { get; set; }
    }
}
