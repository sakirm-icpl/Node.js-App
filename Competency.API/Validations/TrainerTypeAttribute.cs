using Competency.API.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace Competency.API.Validations
{
    public class TrainerTypeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string _value = Convert.ToString(value);
            if (_value.Equals(TrainerType.Internal) || _value.Equals(TrainerType.External) || _value.Equals(TrainerType.Consultant))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Invalid Trainer Type");
            }
        }
    }
}
