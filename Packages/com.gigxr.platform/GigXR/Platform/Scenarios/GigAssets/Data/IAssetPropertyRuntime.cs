namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// An interface for accessing the runtime data of an asset property.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    public interface IAssetPropertyRuntime<TSerializable>
    {
        /// <summary>
        /// The current stageId set on this asset property.
        /// </summary>
        Guid StageId { get; }

        /// <summary>
        /// Whether this asset property for the current stage is using the shared value.
        /// </summary>
        bool UseShared { get; set; }

        /// <summary>
        /// The current value taking into account the current stage and use shared setting.
        /// </summary>
        TSerializable Value { get; set; }

        /// <summary>
        /// Method that allows for the asset runtime data to be updated across the network.
        /// </summary>
        void UpdateValue(TSerializable newValue, AssetPropertyChangeOrigin origin);

        /// <summary>
        /// Method that allows for the asset runtime data to be updated only for the client locally.
        /// </summary>
        /// <param name="newValue"></param>
        void UpdateValueLocally(TSerializable newValue);

        /// <summary>
        /// Register a validator, which will block new property updates at runetime if the validator returns false.
        /// </summary>
        /// <param name="validator"></param>
        void RegisterValidator(Func<object, (object, bool)> validator);

        /// <summary>
        /// Unregister a registered validator.
        /// </summary>
        /// <param name="validator"></param>
        void UnregisterValidator(Func<object, (object, bool)> validator);

        /// <summary>
        /// Control whether changes to stage values persist through stage changes.
        /// Default is for changes to persist. Call with "false" to cause stage values to reset on stage entry.
        /// </summary>
        void SetValuePersistance(bool persist);
    }
}