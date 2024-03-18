using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.UI;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Used to disable components based on the GMS Version set
    /// in the Environmental Configuration SO.
    /// 
    /// Despite the name, it is not located in the Utilities folder due to needing
    /// to reference ProfileManager and avoiding a circular dependency. Can fix later.
    /// 
    /// Inherits from BaseUiObject to guarantee injection step on activation.
    /// </summary>
    public class GmsVersionLimitComponent : BaseUiObject
    {
        [Tooltip("If set to true, the version matter doesn't matter, just its existence.")]
        public bool versionMustBeDefined;

        /* TODO If needed
        [Tooltip("Array of specific versions this component will be active for.")]
        public string[] versionInclusionList;

        [Tooltip("Array of specific versions this component will be deactivated for.")]
        public string[] versionExclusionList;*/

        private ProfileManager ProfileManager { get; set; }

        [InjectDependencies]
        public void Construct(ProfileManager profileManager)
        {
            ProfileManager = profileManager;

            if ((versionMustBeDefined && string.IsNullOrEmpty(ProfileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion)) ||
                ProfileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion == "1.0")
            {
                gameObject.SetActive(false);

                Debug.LogWarning($"{name} has been disabled due to the GMS version not being defined.", this);
            }
        }

        protected override void SubscribeToEventBuses()
        {
        }
    }
}