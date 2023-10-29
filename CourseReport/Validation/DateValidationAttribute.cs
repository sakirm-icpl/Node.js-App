using System;
using System.ComponentModel.DataAnnotations;

namespace CourseReport.API.Validation
{
    public class DateValidationAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateValidationAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            DateTime currentValue = (DateTime)value;  // from date

            System.Reflection.PropertyInfo property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            DateTime comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance);  // to date

            if (currentValue > comparisonValue)
                return new ValidationResult("To date must be greater than from date");

            if (ErrorMessage == "AllowPast")
            {

                if (currentValue > DateTime.Now.Date)
                    return new ValidationResult("From date must be less than current date");

                if (comparisonValue > DateTime.Now.Date)
                    return new ValidationResult("To date must be less than current date");
            }
            else if (ErrorMessage == "AllowFuture")
            {

                if (currentValue < DateTime.Now.Date)
                    return new ValidationResult("From date must be greater than current date");

                if (comparisonValue < DateTime.Now.Date)
                    return new ValidationResult("To date must be greater than current date");
            }

            return ValidationResult.Success;
        }
    }
}

