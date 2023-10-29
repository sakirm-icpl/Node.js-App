using System;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Validations
{
    public class CSVInjectionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string _value = Convert.ToString(value);
            if (_value.StartsWith('+') || _value.StartsWith('-') || _value.StartsWith('=') || _value.StartsWith('@'))
            {
                return new ValidationResult("Vulnerable to CSV Injection");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}
