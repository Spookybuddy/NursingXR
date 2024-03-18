namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using Core.Settings;
    using UnityEngine;

    [System.Serializable]
    public class PositionAssetData : BaseAssetData
    {        
        /// <summary>
        /// The position of this asset.
        /// </summary>
        public AssetPropertyDefinition<Vector3> position;
    }
}