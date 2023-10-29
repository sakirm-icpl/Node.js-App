// ======================================
// <copyright file="EnumHelper.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel;

namespace QuizManagement.API.Helper
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
    }
}
