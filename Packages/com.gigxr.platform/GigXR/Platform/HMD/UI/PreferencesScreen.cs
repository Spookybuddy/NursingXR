namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.Core;
    using GIGXR.Platform.Core.FeatureManagement;

    /// <summary>
    /// HMD specific screen for displaying the inputs for logging into GMS. This screen provides an option
    /// to login via QR code or via a login button after the user types their username and password.
    /// 
    /// TMP_InputField are used for inputs so they will automatically bring up the system keyboard.
    /// </summary>
    public class PreferencesScreen : BaseScreenObject
    {
        public override ScreenType ScreenObjectType => ScreenType.Preferences;

        protected void Start()
        {
            Initialize();

            // Not ideal, but the FeatureFlagComponent can't reference GIGXR Core and is
            // disabled at the start, so use this to inject their dependencies
            var core = FindObjectOfType<GIGXRCore>();
            
            foreach(var comp in GetComponentsInChildren<FeatureFlagComponent>(true))
            {
                _ = core.SetRuntimeDependencies(comp);
            }
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();
        }
    }
}