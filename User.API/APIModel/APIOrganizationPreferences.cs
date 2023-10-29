using System.ComponentModel.DataAnnotations;
using User.API.Helper;
using User.API.Validation;

namespace User.API.APIModel
{
    public class APIOrganizationPreferences
    {
        public int Id { get; set; }
        public string LandingPage { get; set; }
        [MaxLength(50)]
        [CSVInjection]
        [CheckValidationAttribute(AllowValue = new string[] { Record.English, Record.Hindi, Record.Marathi, Record.Arabic, Record.Kannada })]
        public string Language { get; set; }
        [MaxLength(300)]
        public string LogoPath { get; set; }
        [MaxLength(100)]
        public string ColorCode { get; set; }
        public bool IsLogoUpdated { get; set; }
    }

}
