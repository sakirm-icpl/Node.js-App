using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class OrganizationPreferences : CommonFields
    {
        public int Id { get; set; }
        public string LandingPage { get; set; }
        public string Language { get; set; }
        [MaxLength(300)]
        public string LogoPath { get; set; }
        [MaxLength(100)]
        public string ColorCode { get; set; }
    }
}
