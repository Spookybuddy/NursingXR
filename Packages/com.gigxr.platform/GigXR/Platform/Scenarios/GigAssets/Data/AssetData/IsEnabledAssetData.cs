namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    [System.Serializable]
    public class IsEnabledAssetData : BaseAssetData
    {
        /// <summary>
        /// Whether this asset is enabled.
        /// </summary>
        public AssetPropertyDefinition<bool> isEnabled;
        
        /// <summary>
        /// Whether this asset is enabled for the host only.
        /// </summary>
        public AssetPropertyDefinition<bool> isEnabledHostOnly;
    }
}