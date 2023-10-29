using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class APIChangePassword
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
