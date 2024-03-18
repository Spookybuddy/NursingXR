namespace GIGXR.Platform.Scenarios.GigAssets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Marks a method to be tied to a property change for an AssetTypeComponent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RegisterPropertyValidatorAttribute : Attribute
    {
        public string PropertyName { get; }

        public RegisterPropertyValidatorAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
