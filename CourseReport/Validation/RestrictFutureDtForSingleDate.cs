using System;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.Validation
{
    public class RestrictFutureDtForSingleDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime CurrentValue = (DateTime)value;

            if (CurrentValue.Date > DateTime.Now.Date)
                return new ValidationResult("Date must be less than current date");

            return ValidationResult.Success;
        }
    }
}
