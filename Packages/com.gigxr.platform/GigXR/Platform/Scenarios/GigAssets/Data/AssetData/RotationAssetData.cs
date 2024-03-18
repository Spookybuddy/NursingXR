namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using UnityEngine;

    [System.Serializable]
    public class RotationAssetData : BaseAssetData
    {
        /// <summary>
        /// The rotation of this asset.
        /// </summary>
        public AssetPropertyDefinition<Quaternion> rotation;
    }
}