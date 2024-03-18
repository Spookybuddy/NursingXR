namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using Newtonsoft.Json;
    using System;
    using UnityEngine;

    /// <summary>
    /// Represents a metadata field for an asset.
    ///
    /// A metadata field will have a design and runtime data, but is <b>not</b> stage-aware.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    [Serializable]
    public class AssetMetadataDefinition<TSerializable>
    {
        /// <summary>
        /// The design-time data for this metadata field.
        /// </summary>
        [JsonIgnore]
        public AssetMetadataDesignTimeData<TSerializable> designTimeData;
        
        /// <summary>
        /// The runtime data for this metadata field.
        /// </summary>
        [Header("Only edit runtime data inside of the Preset Scenario Builder!")]
        public AssetMetadataRuntimeData<TSerializable> runtimeData;
    }
}