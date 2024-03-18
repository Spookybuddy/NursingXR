using System;


namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Catch-all for data which would otherwise require an additional dependency
    /// injection into an AssetTypeComponent. Expand IGigAssetContext and GigAssetContext
    /// as needed.
    /// </summary>
    public interface IAssetContextProvider<TAssetContext> where TAssetContext : IAssetContext
    {
        public TAssetContext AssetContext { get; }
    }
}
