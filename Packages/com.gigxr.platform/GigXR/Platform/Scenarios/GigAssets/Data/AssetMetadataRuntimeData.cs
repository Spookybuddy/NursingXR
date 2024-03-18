namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using System;

    /// <summary>
    /// Represents the runtime data for an asset metadata field.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    [Serializable]
    public class AssetMetadataRuntimeData<TSerializable>
    {
        /// <summary>
        /// The runtime value for an asset metadata field.
        /// </summary>
        public TSerializable value;
    }
}