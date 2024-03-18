using System;
using UnityEngine;

namespace GIGXR.Platform
{
    /// <summary>
    /// TODO Does not seem to work is using Array or List of a custom type
    /// </summary>
    public class ShowBasedOnEnumAttribute : PropertyAttribute
    {
        public readonly string enumValue;
        public readonly Type enumType;
        public readonly string enumPropertyValueName;

        public ShowBasedOnEnumAttribute(string value, Type type, string propertyHoldingEnumValue)
        {
            enumValue = value;
            enumType = type;
            enumPropertyValueName = propertyHoldingEnumValue;
        }
    }
}