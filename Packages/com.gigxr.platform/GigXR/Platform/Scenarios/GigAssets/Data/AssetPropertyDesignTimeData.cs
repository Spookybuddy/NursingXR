namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using GIGXR.Platform.Scenarios.GigAssets.Validation;
    using System;

    /// <summary>
    /// Represents the design-time data for an asset property.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    [Serializable]
    public class AssetPropertyDesignTimeData<TSerializable>
    {
        /// <summary>
        /// Is this property editable by the scenario author?
        /// </summary>
        public bool isEditableByAuthor;

        /// <summary>
        /// The default value for this property.
        /// </summary>
        public TSerializable defaultValue;
    }
}