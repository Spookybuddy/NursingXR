using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

public static class EnumExtensions
{
    public static string GetEnumDescription(this Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                                            typeof(DescriptionAttribute),
                                            false);

        if (attributes != null &&
            attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }

    // Underlying enum type must be Integers
    public static bool HasAnyFlagInCommon(this Enum type, Enum value)
    {
        return (Convert.ToInt32(type) & Convert.ToInt32(value)) != 0;
    }
}
