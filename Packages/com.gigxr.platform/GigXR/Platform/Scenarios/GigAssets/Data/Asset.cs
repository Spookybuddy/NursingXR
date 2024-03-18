namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Represents an asset as stored internally in the scenarios assembly.
    /// </summary>
    [Serializable]
    public class Asset
    {
        /// <summary>
        /// The identifier for the asset type this asset is an instance of.
        /// </summary>
        public string assetTypeId;

        /// <summary>
        /// The preset identifier for the asset type this asset is an instance of.
        /// </summary>
        public string presetAssetId;

        /// <summary>
        /// The unique identifier for this asset instance.
        /// </summary>
        public string assetId;
        
        /// <summary>
        /// The JSON-serialized data for this asset instance.
        /// </summary>
        public string assetData;

        /// <summary>
        /// True for unsaved assets, which are not part of the scenario
        /// nor should be saved with the session. Loaded into a separate
        /// InstantiatedAsset list when loading sessions.
        /// </summary>
        [JsonIgnore]
        public bool runtimeOnly;

        public Asset()
        {
        }

        [JsonIgnore]
        public Guid AssetId
        {
            get => Guid.Parse(assetId);
            set => assetId = value.ToString();
        }

        [JsonIgnore]
        public Guid AssetTypeId
        {
            get => Guid.Parse(assetTypeId);
            set => assetTypeId = value.ToString();
        }
    }
}