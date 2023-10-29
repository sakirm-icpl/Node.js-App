﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Gadget.API.Validation
{
    public class CommonValidationAttribute : ValidationAttribute
    {
        public string[] AllowValue { get; set; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // string[] myarr = AllowValue.ToString().Split(',');
            string _value = Convert.ToString(value);
            if (AllowValue.Contains(value) || string.IsNullOrEmpty(_value))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Invalid data");
            }
        }
    }
}
