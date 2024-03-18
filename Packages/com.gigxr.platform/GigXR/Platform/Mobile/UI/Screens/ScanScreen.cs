using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using GIGXR.Platform.Mobile.AppEvents.Events.UI;

namespace GIGXR.Platform.Mobile.UI
{
    /// <summary>
    ///     The ScanScreen is active during the first stage of calibration.
    ///     While this on this screen, users scan their environment using
    ///     plane (floor) detection to find suitable locations to place
    ///     their calibrated origin.
    ///     
    ///     Once a surface has been detected with enough contiguous surface
    ///     to place the calibration marker, the 
    ///     <see cref="ProfileManager.MobileProfile.ArTargetPrefab"/> will 
    ///     be instantiated and the user will be moved to the
    ///     <see cref="PlacementScreen"/>.
    /// </summary>
    public class ScanScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.Scan;

        #region Initialization

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<ArStartScanningEvent>(OnArStartScanningEvent);
        }

        #endregion

        #region Unity

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventBus.Unsubscribe<ArStartScanningEvent>(OnArStartScanningEvent);
        }

        #endregion

        #region App Event Handlers

        /// <summary>
        /// When an <see cref="ArStartScanningEvent"/> is published,
        /// publish a UI event to switch to the <c>ScanScreen</c>.
        /// If in the Unity Editor, create an <see cref="ArTargetInstantiatedEvent"/>
        /// 1 second later to allow editor tests to get through calibration without
        /// a floor scan.
        /// </summary>
        private void OnArStartScanningEvent(ArStartScanningEvent @event)
        {
            SwitchToScanScreen();
        }

        private void SwitchToScanScreen()
        {
            uiEventBus.Publish
            (
                new SwitchingActiveScreenEventMobile(ScreenTypeMobile.Scan, this.ScreenType)
            );

#if UNITY_EDITOR
            // In Editor hack, skips the scan and just enables the Placement Button
            uiEventBus.Publish
            (
                new SwitchingActiveScreenEventMobile(ScreenTypeMobile.Placement, this.ScreenType)
            );
#endif
        }
        #endregion
    }
}