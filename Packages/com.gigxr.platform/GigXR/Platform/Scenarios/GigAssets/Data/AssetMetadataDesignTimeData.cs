namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using System;

    /// <summary>
    /// Represents the design-time data for an asset metadata field.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    [Serializable]
    public class AssetMetadataDesignTimeData<TSerializable>
    {
        /// <summary>
        /// The default value for this metadata field.
        /// </summary>
        public TSerializable defaultValue;
    }
}