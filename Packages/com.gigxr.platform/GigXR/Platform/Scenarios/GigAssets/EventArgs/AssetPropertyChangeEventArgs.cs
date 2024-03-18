namespace GIGXR.Platform.Scenarios.GigAssets.EventArgs
{
    using Data;

    using System;
    using System.ComponentModel;

    public class AssetPropertyChangeEventArgs : PropertyChangedEventArgs
    {
        public string AssetPropertyName { get; }

        public Guid AssetId { get; private set; }

        public object AssetPropertyValue { get; private set; }

        public AssetPropertyChangeOrigin Origin { get; private set; }


        public AssetPropertyChangeEventArgs(string assetName, string propertyName, object assetValue, AssetPropertyChangeOrigin origin) : base(propertyName)
        {
            AssetPropertyName = assetName;
            AssetPropertyValue = assetValue;
            Origin = origin;
        }

        public virtual void SetAssetID(Guid assetId)
        {
            AssetId = assetId;
        }
    }
}