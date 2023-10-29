using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class SocialMediaRejected
    {
        [Required]
        public int Id { get; set; }
        [MaxLength(250)]
        public string UserId { get; set; }
        public int CreatedBy { get; set; }
        [MaxLength(250)]
        public DateTime CreatedDate { get; set; }
        [Required]
        [MaxLength(500)]
        public string ErrorMessage { get; set; }



    }
}
