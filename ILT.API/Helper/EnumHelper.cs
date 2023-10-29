using System;
using System.ComponentModel;

namespace ILT.API.Common
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
        public enum ApiStatusCode
        {
            Unauthorized = 401,
            Ok = 200,
            BadRequest = 400,
            InternalServerError = 500
        }
    }

}