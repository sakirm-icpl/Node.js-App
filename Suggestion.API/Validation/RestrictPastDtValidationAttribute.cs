using System;
using System.ComponentModel.DataAnnotations;

namespace Suggestion.API.Validation
{
    public class RestrictPastDtValidationAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public RestrictPastDtValidationAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //ErrorMessage = ErrorMessageString;
            DateTime currentValue = (DateTime)value;  // from date

            System.Reflection.PropertyInfo property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            DateTime comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance);  // to date

            if (currentValue > comparisonValue)
                return new ValidationResult("ValidityDate date must be greater than Start date");

            if (currentValue < DateTime.Now.Date)
                return new ValidationResult("Start must be greater than or equal to current date");

            if (currentValue < DateTime.Now.Date)
                return new ValidationResult("ValidityDate must be greater than or equal to current date");

            return ValidationResult.Success;
        }
    }
}
