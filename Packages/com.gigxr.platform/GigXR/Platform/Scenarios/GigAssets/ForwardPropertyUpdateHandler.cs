namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Data;
    using EventArgs;
    using System;
    using UnityEngine;

    public class ForwardPropertyUpdateHandler
    {
        public event EventHandler<AssetPropertyChangeEventArgs> AssetPropertyUpdated;

        public void RegisterPropertyUpdateEventHandler(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IAssetMediator assetMediator))
            {
                assetMediator.PropertyChanged += AssetMediator_PropertyChanged;
            }
        }

        public void UnregisterPropertyUpdateEventHandler(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IAssetMediator assetMediator))
            {
                assetMediator.PropertyChanged -= AssetMediator_PropertyChanged;
            }
        }

        private void AssetMediator_PropertyChanged(object sender, AssetPropertyChangeEventArgs e)
        {
            if (e.PropertyName == nameof(IAssetPropertyRuntime<object>.Value) ||
                e.PropertyName == nameof(IAssetPropertyRuntime<object>.UseShared))
            {
                AssetPropertyUpdated?.Invoke(sender, e);
            }
        }
    }
}