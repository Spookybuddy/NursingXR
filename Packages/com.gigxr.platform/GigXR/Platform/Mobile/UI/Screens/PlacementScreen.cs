using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using GIGXR.Platform.Mobile.AppEvents.Events.UI;
using GIGXR.Platform.Mobile.AR;
using System;

namespace GIGXR.Platform.Mobile.UI
{
    /// <summary>
    ///     The PlacementScreen is active during the second stage of
    ///     calibration. While on this screen, users move their calibration
    ///     marker (an instance of the 
    ///     <see cref="ProfileManager.MobileProfile.ArTargetPrefab"/>)
    ///     around on detected horizontal planes (and continue to detect
    ///     more planes) until they hit a "Finish" button, at
    ///     which point their placeholder's position and orientation
    ///     will be used in a <see cref="ArTargetPlacedEvent"/> and
    ///     will reach <see cref="MobileCalibrationManager.MobileStopARCalibration"/>
    ///     downstream.
    /// </summary>
    public class PlacementScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.Placement;

        private void OnApplicationQuit()
        {
            EventBus.Unsubscribe<ArTargetInstantiatedEvent>(OnArTargetInstantiatedEvent);
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<ArTargetInstantiatedEvent>(OnArTargetInstantiatedEvent);
        }

        private void OnArTargetInstantiatedEvent(ArTargetInstantiatedEvent @event)
        {
            uiEventBus.Publish
            (
                new SwitchingActiveScreenEventMobile(ScreenTypeMobile.Placement, this.ScreenType)
            );
        }

        // Called via Unity Editor in the PlacementScreen prefab
        public void PlaceTarget()
        {
            //TODO Make actual reference
            ArTarget.Instance.SetTargetPlacement();

            // Bring down the screen immediately
            RootScreenObject.SetActive(false);
        }
    }
}
