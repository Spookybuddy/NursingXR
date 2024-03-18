namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;

    public class SettingScreenFollowBehaviorEvent : BaseScreenRequestEvent
    {
        public readonly bool IsBehaviorOn;
        public readonly TrackedObjectType ObjectToFollow;

        /// <summary>
        /// Sends a request to toggle MRTK behavior for the targetScreen.
        /// </summary>
        /// <param name="targetScreen"></param>
        /// <param name="behaviorToggle"></param>
        public SettingScreenFollowBehaviorEvent
        (
            BaseScreenObject.ScreenType targetScreen,
            bool behaviorToggle,
            TrackedObjectType objectToFollow = TrackedObjectType.Head
        ) : base(targetScreen)
        {
            IsBehaviorOn = behaviorToggle;
            ObjectToFollow = objectToFollow;
        }
    }
}