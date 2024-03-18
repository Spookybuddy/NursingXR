using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Mobile.AppEvents.Events.UI;
using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using UnityEngine;

namespace GIGXR.Platform.Mobile.UI
{
    /// <summary>
    ///     The ResetScanScreen consists of a single button, which can be used
    ///     to reset calibration (i.e. restart scanning).
    /// </summary>
    public class ResetScanScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.ResetScan;

        private ICalibrationManager CalibrationManager;

        #region Initialization

        [InjectDependencies]
        public void Construct(ICalibrationManager calibrationManager)
        {
            CalibrationManager = calibrationManager;
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlaced);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventBus.Unsubscribe<ArTargetPlacedEvent>(OnArTargetPlaced);
        }

        #endregion

        private void OnArTargetPlaced(ArTargetPlacedEvent @event)
        {
            RootScreenObject.SetActive(false);
        }

        #region UI Event Handlers

        protected override void OnSwitchingActiveScreenEvent(SwitchingActiveScreenEventMobile @event)
        {
            // Scanning can be reset from either the during scan
            bool scanningWillBeInProgress = 
                @event.TargetScreen == ScreenTypeMobile.Scan;

            RootScreenObject.SetActive
            (
                scanningWillBeInProgress
            );
        }

        #endregion

        #region Public API for UI Element

        // Referenced by Reset Scan Button in mobile hierarchy.
        // Restarts / resets a scan-in-progress.
        private bool isResettingScan;

        public void ResetScan()
        {
            if (!isResettingScan)
            {
                // Avoid overlapping resets
                isResettingScan = true;

                // If the reset occurs during setting the anchor, do a calibration reset
                if(CalibrationManager.CurrentCalibrationMode != ICalibrationManager.CalibrationModes.None)
                {
                    // Stop any existing calibration-in-progress
                    CalibrationManager.StopCalibration(true, Vector3.zero, Quaternion.identity);

                    // Now, kick off the reset
                    EventBus.Publish(new ArSessionResetEvent());

                    // Start scanning again
                    CalibrationManager.StartCalibration(ICalibrationManager.CalibrationModes.Manual);
                }
                else
                {
                    EventBus.Publish(new ArSessionResetEvent());

                    EventBus.Publish(new ArStartScanningEvent());
                }

                isResettingScan = false;
            }
        }

        #endregion
    }
}
