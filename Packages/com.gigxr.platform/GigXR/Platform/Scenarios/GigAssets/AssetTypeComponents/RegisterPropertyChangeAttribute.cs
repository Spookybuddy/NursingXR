namespace GIGXR.Platform.Scenarios.GigAssets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Marks a method to be tied to a property change for an AssetTypeComponent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RegisterPropertyChangeAttribute : Attribute
    {
        public string PropertyName { get; }

        public RegisterPropertyChangeAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}