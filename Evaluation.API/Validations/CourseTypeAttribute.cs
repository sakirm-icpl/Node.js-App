using Evaluation.API.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace Evaluation.API.Validations
{
    public class CourseTypeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string _value = Convert.ToString(value);
            if (_value.Equals(CourseType.Elearning) || _value.Equals(CourseType.Classroom) || _value.Equals(CourseType.vilt) || _value.Equals(CourseType.Assessment) || _value.Equals(CourseType.Blended))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Invalid course type");
            }
        }
    }
}
