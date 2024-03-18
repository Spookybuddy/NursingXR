namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using UnityEngine;

    [System.Serializable]
    public class ScaleAssetData : BaseAssetData
    {
        /// <summary>
        /// The scale of this asset.
        /// </summary>
        public AssetPropertyDefinition<Vector3> scale;
    }
}