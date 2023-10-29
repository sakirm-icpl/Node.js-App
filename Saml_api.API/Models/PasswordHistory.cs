using System.ComponentModel.DataAnnotations;

namespace Saml.API.Models
{
    public class PasswordHistory : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Password { get; set; }
    }
}
