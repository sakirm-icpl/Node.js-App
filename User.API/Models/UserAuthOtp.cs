using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class UserAuthOtp
    {
        public int Id { get; set; }
        [MaxLength(1000)]
        public string Otp { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime OtpExpiryDate { get; set; }
    }
}
