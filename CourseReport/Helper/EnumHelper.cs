using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.Helper
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(this Enum value)
        {
            Type enumType = value.GetType();
            System.Reflection.FieldInfo field = enumType.GetField(value.ToString());
            object[] attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }
        public static string GetEnumName(this Enum value)
        {
            Type enumType = value.GetType();
            //var field = enumType.GetField(value.ToString());
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
    public enum StatusCode
    {
        Unauthorized = 401,
    }

}

