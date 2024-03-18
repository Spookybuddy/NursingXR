namespace GIGXR.Platform.Scenarios.GigAssets
{
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// A specialized AssetTypeComponent that does not use any AssetData, so it does not contribute
    /// to the JSON payload that builds up an asset, but it provides behaviors that are driven by
    /// local states of the asset.
    /// </summary>
    public abstract class LocalAssetTypeComponent : AssetTypeComponent
    {
        protected IAssetMediator attachedAssetMediator 
        { 
            get
            {
                if (_attachedAssetMediator == null)
                    InitializeMediatorBasedReferences();

                return _attachedAssetMediator;
            }
        }

        private IAssetMediator _attachedAssetMediator;

        protected virtual IAssetMediator InitializeMediatorBasedReferences()
        {
            _attachedAssetMediator = GetComponent<IAssetMediator>();
            return _attachedAssetMediator;
        }
    }
}