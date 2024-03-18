namespace GIGXR.Platform.Scenarios.GigAssets.EventArgs
{
    using System;

    /// <summary>
    /// Specifications for destruction of an asset
    /// </summary>
    public class AssetDestroyedEventArgs : EventArgs
    {
        /// <summary>
        /// The id of the asset to destroy
        /// </summary>
        public Guid AssetId;

        /// <summary>
        /// True if the destruction is downstream of a call to reload assets
        /// </summary>
        public bool FromReload;

        public AssetDestroyedEventArgs(Guid assetId, bool fromReload)
        {
            AssetId = assetId;
            FromReload = fromReload;
        }
    }
}