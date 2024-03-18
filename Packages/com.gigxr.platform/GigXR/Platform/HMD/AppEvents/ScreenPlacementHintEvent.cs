namespace GIGXR.Platform.HMD.AppEvents.Events.UI
{
    using GIGXR.Platform.HMD.UI;
    using UnityEngine;

    public class ScreenPlacementHintEvent : BaseScreenRequestEvent
    {
        public Vector3 WorldPosition { get; }

        public Quaternion WorldRotation { get; }

        public bool IncludeToolbar { get; }

        /// <summary>
        /// Event sent out to a screen type and toolbar with world position and rotation to help position themselves.
        /// </summary>
        /// <param name="targetScreen">The screen type the information should be passed onto</param>
        /// <param name="position">The old screen's position in world space</param>
        /// <param name="rotation">The old screen's position in world space</param>
        /// <param name="includeToolbar">If true, the toolbar will also update it's position and rotation to match</param>
        public ScreenPlacementHintEvent(BaseScreenObject.ScreenType targetScreen, Vector3 position, Quaternion rotation, bool includeToolbar) : base(targetScreen)
        {
            TargetScreen = targetScreen;
            WorldPosition = position;
            WorldRotation = rotation;
            IncludeToolbar = includeToolbar;
        }
    }
}