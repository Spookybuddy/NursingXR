namespace GIGXR.Platform.Core.FeatureManagement
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core.Settings;
    using System;

    public class FeatureChangeEventArgs : EventArgs
    {
        public FeatureFlags changedFeatures;
        // If false, the feature was removed
        public bool wasAdded;
    }

    /// <summary>
    /// An implementation of IFeatureManager that uses a basic enum as the data store.
    /// </summary>
    public class BasicEnumFeatureManager : IFeatureManager
    {
        private FeatureFlags featureFlagsFunc;

        public event EventHandler<FeatureChangeEventArgs> RuntimeFeatureChanged;

        public BasicEnumFeatureManager(FeatureFlags featureFlagsFunc)
        {
            // Since we pass in the original FeatureFlags value from the Editor, when we edit the value below
            // with AddRuntimeFeature/RemoveRuntimeFeature, it will stop matching the initial ProfileManager
            this.featureFlagsFunc = featureFlagsFunc;
        }

        public bool IsEnabled(FeatureFlags feature)
        {
            if (featureFlagsFunc == FeatureFlags.None)
            {
                return false;
            }

            return featureFlagsFunc.HasFlag(feature);
        }

        public UniTask<bool> IsEnabledAsync(FeatureFlags feature)
        {
            if (featureFlagsFunc == FeatureFlags.None)
            {
                return UniTask.FromResult(false);
            }

            return UniTask.FromResult(featureFlagsFunc.HasFlag(feature));
        }

        public void AddRuntimeFeature(FeatureFlags feature)
        {
            featureFlagsFunc |= feature;

            RuntimeFeatureChanged?.Invoke(this, new FeatureChangeEventArgs()
                {
                    changedFeatures = feature,
                    wasAdded = true
                });
        }

        public void RemoveRuntimeFeature(FeatureFlags feature)
        {
            featureFlagsFunc &= ~feature;

            RuntimeFeatureChanged?.Invoke(this, new FeatureChangeEventArgs()
                {
                    changedFeatures = feature,
                    wasAdded = false
                });
        }
    }
}