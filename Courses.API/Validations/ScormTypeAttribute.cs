using System;
using System.ComponentModel.DataAnnotations;


namespace Courses.API.Validations
{
    public class ScormTypeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            string scormType = (string)value;

            if (string.Equals(scormType, "scorm", StringComparison.OrdinalIgnoreCase)
              || string.Equals(scormType, "scorm1.2", StringComparison.OrdinalIgnoreCase)
              || string.Equals(scormType, "scorm2004", StringComparison.OrdinalIgnoreCase)
              || string.Equals(scormType, "nonscorm", StringComparison.OrdinalIgnoreCase)
              )
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(this.FormatErrorMessage(validationContext.MemberName));
            }
        }
    }
}
