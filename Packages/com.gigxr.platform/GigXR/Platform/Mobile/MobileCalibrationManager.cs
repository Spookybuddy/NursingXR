namespace GIGXR.Platform.Mobile
{
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Managers;
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.Mobile.AppEvents.Events.AR;
    using UnityEngine;
    using GIGXR.Platform.Sessions;
    using GIGXR.Platform.Core;

    /// <summary>
    /// CalibrationManager for mobile users that provides a single manual calibration option, which utilizes ARFoundation's floor scanning
    /// and a raycast from the Phone's location to place the calibrated center at the user's choice of location along the floor.
    /// </summary>
    public class MobileCalibrationManager : CalibrationManager
    {
        #region CalibrationManager Completion

        public MobileCalibrationManager(GIGXRCore core, AppEventBus appEventBus, IGigAssetManager assetManager, ISessionManager sessionManager) : base(core, appEventBus, assetManager, sessionManager)
        {
            calibratedStartActionsPerMode.Add(ICalibrationManager.CalibrationModes.Manual, MobileStartARCalibration);
            calibratedStopActionsPerMode.Add(ICalibrationManager.CalibrationModes.Manual, MobileStopARCalibration);

            appEventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
        }

        /// <summary>
        ///     This method starts the scanning process which mobile users
        ///     use to place their calibrated origin.
        /// </summary>
        private void MobileStartARCalibration()
        {
            EventBus.Publish(new ArStartScanningEvent());
        }

        /// <summary>
        ///     This method is called downstream of a published <see cref="AppEvents.Events.AR.ArTargetPlacedEvent"/> when the AR Target is placed
        ///     at the end of the scanning process. It receives the AR Target's position and orientation as arguments.
        /// </summary>
        /// <param name="wasCancelled">
        ///     True if the calibration is stopping due to cancellation. False if calibration stopped due to completion.
        /// </param>
        /// <param name="targetSessionPosition">
        ///     The session position of the AR Target, to be used as the position of the calibrated origin.
        /// </param>
        /// <param name="targetWorldOrientation">
        ///     The world orientation of the AR Target, to be used as the orientation of the calibrated origin.
        /// </param>
        private void MobileStopARCalibration(bool wasCancelled, Vector3 targetSessionPosition, Quaternion targetWorldOrientation)
        {
            if (!wasCancelled)
            {
                UpdateAnchorRoot(targetSessionPosition, targetWorldOrientation);
            }
        }

        #endregion

        #region Event Handlers

        private void OnArTargetPlacedEvent(ArTargetPlacedEvent @event)
        {
            StopCalibration(false, @event.TargetSessionPosition, @event.TargetWorldRotation);
        }

        #endregion
    }
}