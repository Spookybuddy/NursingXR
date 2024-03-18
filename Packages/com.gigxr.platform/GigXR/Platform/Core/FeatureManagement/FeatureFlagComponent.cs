namespace GIGXR.Platform.Core.FeatureManagement
{
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Core.Settings;
    using UnityEngine;

    public enum TurnOffAction
    {
        Destroy = 0,
        Disable = 1
    }

    /// <summary>
    /// A MonoBehavior component that allows a GameObject to be linked to a Feature set by the FeatureFlagProfile.
    /// If the feature is not enabled, this component is used to destroy or disable that GameObject.
    /// </summary>
    public class FeatureFlagComponent : MonoBehaviour
    {
        [SerializeField]
        private FeatureFlags LinkedFeatures;

        [SerializeField]
        private TurnOffAction turnOffAction;

        private IFeatureManager FeatureManager;

        [InjectDependencies]
        public void Construct(IFeatureManager featureManager)
        {
            if (LinkedFeatures == FeatureFlags.None)
            {
                return;
            }

            FeatureManager = featureManager;

            CheckFeatureFlags();

            FeatureManager.RuntimeFeatureChanged += FeatureManager_RuntimeFeatureChanged;
        }

        protected virtual void OnDestroy()
        {
            if(FeatureManager != null)
            {
                FeatureManager.RuntimeFeatureChanged -= FeatureManager_RuntimeFeatureChanged;
            }
        }

        private void InvokeTurnOffAction(Object unityObject)
        {
            switch (turnOffAction)
            {
                case TurnOffAction.Destroy:
                    Destroy(unityObject);
                    break;
                case TurnOffAction.Disable:
                    if(unityObject is MonoBehaviour mono)
                    {
                        mono.enabled = false;
                    }
                    else if(unityObject is GameObject gameObject)
                    {
                        gameObject.SetActive(false);
                    }
                    break;
            }
        }

        private void CheckFeatureFlags()
        {
            // The feature is included, get rid of this component
            if (FeatureManager.IsEnabled(LinkedFeatures))
            {
                InvokeTurnOffAction(this);
            }
            // The feature is not included, get rid of the GO
            else
            {
                InvokeTurnOffAction(gameObject);
            }
        }
        private void FeatureManager_RuntimeFeatureChanged(object sender, FeatureChangeEventArgs e)
        {
            // Only check if the attachment marker is the feature that changed
            if (e.changedFeatures.HasAnyFlagInCommon(LinkedFeatures))
            {
                CheckFeatureFlags();
            }
        }
    }
}