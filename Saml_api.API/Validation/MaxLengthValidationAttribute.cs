using log4net;
using System.ComponentModel.DataAnnotations;
using Saml.API.APIModel;
using Saml.API.Helper;

namespace Saml.API.Validation
{
    public class MaxLengthValidationAttribute : ValidationAttribute
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MaxLengthValidationAttribute));
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                APIUserMaster user = (APIUserMaster)validationContext.ObjectInstance;
                if (user == null)
                    return ValidationResult.Success;
                if (string.Equals(user.CustomerCode, "cpcl", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    if (user.ConfigurationColumn6 != null && !string.IsNullOrEmpty(user.ConfigurationColumn6))
                    {
                        if (user.ConfigurationColumn6.Length > 2000)
                            return new ValidationResult("The field ConfigurationColumn6 must be a string or array type with a maximum length of '2000'.");
                    }
                    return ValidationResult.Success;
                }
                else
                {
                    if (user.ConfigurationColumn6 != null && !string.IsNullOrEmpty(user.ConfigurationColumn6))
                    {
                        if (user.ConfigurationColumn6.Length > 100)
                            return new ValidationResult("The field ConfigurationColumn6 must be a string or array type with a maximum length of '100'.");
                    }
                    return ValidationResult.Success;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return ValidationResult.Success;
            }
        }
    }
}
