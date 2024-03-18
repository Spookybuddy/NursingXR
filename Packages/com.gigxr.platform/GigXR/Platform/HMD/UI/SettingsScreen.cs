namespace GIGXR.Platform.HMD.UI
{
    using Platform.AppEvents.Events.UI;
    using UnityEngine;

    /// <summary>
    /// Placeholder class
    /// </summary>
    public class SettingsScreen : BaseScreenObject
    {
        public override ScreenType ScreenObjectType => ScreenType.None;

        protected override void OnEnable()
        {
            base.OnEnable();

            Initialize();
        }
    }
}