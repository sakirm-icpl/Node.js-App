using System;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.Validation
{
    public class RestrictFutureDtValidationAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public RestrictFutureDtValidationAttribute(string comparisonProperty)
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

            if (currentValue.Date > comparisonValue.Date)
                return new ValidationResult("To date must be greater than from date");

            if (currentValue.Date > DateTime.Now.Date)
                return new ValidationResult("From date must be less than current date");

            if (comparisonValue.Date > DateTime.Now.Date)
                return new ValidationResult("To date must be less than current date");


            return ValidationResult.Success;
        }
    }
}

