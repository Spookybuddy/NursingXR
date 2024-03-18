namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// The internal representation of an asset property value for a specific stage.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    [Serializable]
    public class StageAssetProperty<TSerializable>
    {
        /// <summary>
        /// The stage for this asset property value.
        /// </summary>
        public string stageId;

        /// <summary>
        /// Whether this asset property should use the shared value for this stage.
        /// </summary>
        public bool useShared;

        /// <summary>
        /// Whether this asset property should reset to its initial value on stage changes.
        /// Ignored if <see cref="useShared"/> is true.
        /// </summary>
        public bool resetValueOnStageChange;

        /// <summary>
        /// The local value specific to this asset instance and stage combination.
        /// </summary>
        public TSerializable localValue;

        /// <summary>
        /// Value provided by scenario data, to which the <see cref="localValue"/> is reset on stage changes.
        /// </summary>
        [NonSerialized]
        [JsonIgnore]
        public TSerializable initialValue;
    }
}