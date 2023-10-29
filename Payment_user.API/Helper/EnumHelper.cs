//======================================
// <copyright file="EnumHelper.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using System;
using System.ComponentModel;

namespace Payment.API.Helper
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(this Enum value)
        {
            var enumType = value.GetType();
            var field = enumType.GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }
        public static string GetEnumName(this Enum value)
        {
            var enumType = value.GetType();
            return Enum.GetName(enumType, value);
        }

        public enum UserColumn
        {
            ConfigurationColumn1,
            ConfigurationColumn2,
            ConfigurationColumn3,
            ConfigurationColumn4,
            ConfigurationColumn5,
            ConfigurationColumn6,
            ConfigurationColumn7,
            ConfigurationColumn8,
            ConfigurationColumn9,
            ConfigurationColumn10,
            ConfigurationColumn11,
            ConfigurationColumn12,
            Currency,
            DateOfBirth,
            CustomerCode,
            EmailId,
            Gender,
            Language,
            MobileNumber,
            TimeZone,
            UserName,
            UserType,
            UserRole,
            Location,
            Business,
            Group,
            Area,
        }

        public enum EmployeementType
        {
            Internal,
            External
        }

        public enum Gender
        {
            Male,
            Female
        }
        public enum Language
        {
            English,
            Hindi,
            Arabic,
            French,
            German,
            Chinese
        }
        public enum ApiStatusCode
        {
            Unauthorized = 401,
            Ok = 200,
            BadRequest = 400,
            InternalServerError = 500
        }

        public static bool CheckBirthDate(DateTime birthDate)
        {
            bool isValidDate = true;
            int result = DateTime.Compare(birthDate, DateTime.Now.AddYears(-18));
            if (result == 1)
            {
                isValidDate = false;
            }
            return isValidDate;
        }

        public static bool CheckPastTodayDate(DateTime inputDate)
        {
            bool isValidDate = true;
            int result = DateTime.Compare(inputDate, DateTime.Now);
            if (result == 1)
                isValidDate = false;
            return isValidDate;
        }

    }
}
