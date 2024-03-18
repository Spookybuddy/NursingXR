namespace GIGXR.Platform
{
    using GIGXR.Platform.Core;
    using GIGXR.Platform.Scenarios.GigAssets.Loader;

    /// <summary>
    /// The composition root of a scene. This is where the object graph for plain C# classes is configured.
    /// </summary>
    public class CompositionRoot : GIGXRCore
    {
        IAssetTypeLoader assetTypeLoader;

        public override IAssetTypeLoader AssetTypeLoader
        {
            get
            {
                if (assetTypeLoader == null)
                {
                    assetTypeLoader = new AddressablesAssetTypeLoader();
                }

                return assetTypeLoader;
            }
        }
        
        // Not sure if this should go here in Composition Root, but it is similar to the assetTypeLoader.
        // Can be used to load Addressable game objects as needed by asset types. 
        IAddressablesGameObjectLoader addressablesGameObjectLoader;
        
        public override IAddressablesGameObjectLoader AddressablesGameObjectLoader
        {
            get
            {
                if (addressablesGameObjectLoader == null)
                {
                    addressablesGameObjectLoader = new AddressablesGameObjectLoader();
                }

                return addressablesGameObjectLoader;
            }
        }

        protected override void BuildExtraDependencies()
        {
            // Not needed at this level
        }
    }
}