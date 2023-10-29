using System;
using System.ComponentModel.DataAnnotations;

namespace Publication.API.Validation
{
    public class AllowFutureDtValidationAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public AllowFutureDtValidationAttribute(string comparisonProperty)
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
                return new ValidationResult("Validity Date must be greater than Start date");

            if (currentValue < DateTime.Now.Date)
                return new ValidationResult("Date must be greater than current date");

            if (comparisonValue < DateTime.Now.Date)
                return new ValidationResult("Date must be greater than current date");


            return ValidationResult.Success;
        }
    }
}
