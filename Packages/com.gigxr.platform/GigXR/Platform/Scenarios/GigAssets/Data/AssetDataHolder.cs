namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using Newtonsoft.Json;

    /// <summary>
    /// Simple data class to hold together an AssetTypeComponent and it's data
    /// </summary>
    public class AssetDataHolder
    {
        [JsonIgnore]
        public IAssetTypeComponent attachedAssetTypeComponent;

        public int version;

        public BaseAssetData assetData;

        public AssetDataHolder(IAssetTypeComponent component, BaseAssetData data)
        {
            attachedAssetTypeComponent = component;
            version = component != null ? component.Version : 0;
            assetData = data;
        }
    }
}