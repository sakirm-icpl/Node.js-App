using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APINewPassword
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$")]
        public string NewPassword { get; set; }
        [Required]
        public string OrganizationCode { get; set; }
        [Required]
        public string OTP { get; set; }

        public string dob { get; set; }
        public string confirmPassword { get; set; }
    }
}
