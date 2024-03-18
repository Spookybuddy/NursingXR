namespace GIGXR.Platform.ScenarioBuilder.Data
{
    using GIGXR.Platform.Scenarios.GigAssets;
    using Newtonsoft.Json;
    using System;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    /// <summary>
    /// A <c>PresetAsset</c> is a wrapper for an <c>Asset</c> that provides a human-usable identifier.
    /// </summary>
    [Serializable]
    public class PresetAsset
    {
        /// <summary>
        /// Used to reference difference asset instances in Rules.
        /// </summary>
        [Header("Human friendly Preset Asset ID, e.g., \"cube-one\"")]
        public string presetAssetId;
        
        /// <summary>
        /// Reference to the addressable asset that will be instantiated.
        /// </summary>
        [Header("Asset Type Prefab Reference")]
        public AssetReference assetTypePrefabReference;

        [HideInInspector]
        [Header("The Asset Type prefab, direct reference.")]
        public AssetMediator assetTypePrefab;

        public string assetId;

        [HideInInspector]
        [Obsolete("This value used to hold the entire JSON blob of the asset data, but now a Prefab copy of the asset will hold this data instead.")]
        public string assetData;

        /// <summary>
        /// The Asset Type ID is used to locate the asset.
        /// This could also be done using the Addressable Key or AssetGUID.
        /// </summary>
        [JsonIgnore]
        public string AssetTypeId
        {
            get => assetTypePrefabReference.RuntimeKey.ToString();
        }

        [JsonIgnore]
        public Guid AssetId
        {
            get => Guid.Parse(assetId);
            set => assetId = value.ToString();
        }
    }
}