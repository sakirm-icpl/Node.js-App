using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIForgotPassword
    {
        [Required]
        public string OrganizationCode { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Otp { get; set; }
    }
}
