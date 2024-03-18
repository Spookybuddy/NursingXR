namespace GIGXR.Platform.HMD
{
    using GIGXR.Platform.AppEvents.Events.Calibration;
    using GIGXR.Platform.HMD.Interfaces;
    using GIGXR.Platform.Managers;
    using Microsoft.MixedReality.QR;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using UnityEngine;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.HMD.AppEvents.Events;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.Sessions;
    using GIGXR.Platform.Core;

    /// <summary>
    /// CalibrationManager for HMD users that provides two types of calibration modes. Manually, which lets the user move the calibration cube
    /// to wherever they would like to place it in their scene. QR, which utilizes a QR code found on GMS to place the calibration cube at the
    /// location of the QR code.
    /// </summary>
    public class HMDCalibrationManager : CalibrationManager
    {
        public IQrCodeManager QrCodeManager { get; private set; }

        public HMDCalibrationManager(GIGXRCore core, AppEventBus appEventBus, IGigAssetManager assetManager, ISessionManager sessionManager, IQrCodeManager qrCodeManager) : base(core, appEventBus, assetManager, sessionManager)
        {
            QrCodeManager = qrCodeManager;

            calibratedStartActionsPerMode.Add(ICalibrationManager.CalibrationModes.Qr, StartQrCalibration);
            calibratedStopActionsPerMode.Add(ICalibrationManager.CalibrationModes.Qr, StopQrCalibration);

            calibratedStartActionsPerMode.Add(ICalibrationManager.CalibrationModes.Manual, StartManualCalibration);
            calibratedStopActionsPerMode.Add(ICalibrationManager.CalibrationModes.Manual, StopManualCalibration);
        }

        /// <summary>
        /// Starts the manual calibration process.
        /// </summary>
        public void StartManualCalibration()
        {
            // turn off the spatial awareness for now, this is undesirable during manual calibration as whilst
            // we technically want the floor, we do not care about walls.
            Microsoft.MixedReality.Toolkit.CoreServices.SpatialAwarenessSystem.Disable();
        }

        public void StopManualCalibration(bool calibrationWasCancelled, Vector3 targetPosition, Quaternion targetOrientation)
        {
            if (!calibrationWasCancelled)
            {
                DebugUtilities.Log("[HMDCalibrationManager] Stopping Manual Calibration");

                // Update the calibration root position and rotation accordingly.
                UpdateAnchorRoot(targetPosition, targetOrientation);

                // Session Screen uses this to return to Session List or Session Log: 
                EventBus.Publish(new SetAnchorRootEvent(targetPosition, targetOrientation));
            }

            // for manual, we also turn off the spatial understanding system, switch this back on.
            Microsoft.MixedReality.Toolkit.CoreServices.SpatialAwarenessSystem.Enable();
        }

        public void StopQrCalibration(bool calibrationWasCancelled, Vector3 targetPosition, Quaternion targetOrientation)
        {
            if (!calibrationWasCancelled)
            {
                DebugUtilities.Log("[HMDCalibrationManager] Stopping Qr Calibration");

                UpdateAnchorRoot(targetPosition, targetOrientation);

                // Session Screen uses this to return to Session List or Session Log: 
                EventBus.Publish(new SetAnchorRootEvent(targetPosition, targetOrientation));
            }

            // if in QR calibration, unsubscribe from the QrCodeManager events.
            QrCodeManager.QrCodeAdded -= Instance_QRCodeAdded;
            QrCodeManager.QrCodeUpdated -= Instance_QRCodeUpdated;
            QrCodeManager.QrCodeRemoved -= Instance_QRCodeRemoved;

            QrCodeManager.StopQrTracking();
        }

        /// <summary>
        /// Starts a QR Calibration process. Currently supported on Hololens only.
        /// </summary>
        public void StartQrCalibration()
        {
            DebugUtilities.Log("[HMDCalibrationManager] Starting Qr Calibration");

            CurrentCalibrationMode = ICalibrationManager.CalibrationModes.Qr;

            // Subscribe to QrCodeManager events 
            QrCodeManager.QrCodeAdded += Instance_QRCodeAdded;
            QrCodeManager.QrCodeUpdated += Instance_QRCodeUpdated;
            QrCodeManager.QrCodeRemoved += Instance_QRCodeRemoved;

            // TODO Externalize
            QrCodeManager.StartQrTracking("Position your gaze so the Anchor QR code is within the frame.\nOnce the anchor appears over the code, tap Finish.");
        }

        /// <summary>
        /// Called when the QRCodeController QrCodeAdded event is called. This occurs when the Calibration QR marker is scanned for the first time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_QRCodeAdded(object sender, QRCode e)
        {
            if (IsCalibrationQr(e.Data.ToString()))
            {
                EventBus.Publish(new UpdateQrObjectEvent(e));
            }
        }

        /// <summary>
        /// Called when the QRCodeController QRCodeUpdated event is called. This occurs when the Calibration QR marker is scanned for the second time onwards to update its transform.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_QRCodeUpdated(object sender, QRCode e)
        {
            if (IsCalibrationQr(e.Data.ToString()))
            {
                EventBus.Publish(new UpdateQrObjectEvent(e));
            }
        }

        /// <summary>
        /// Called when the QRCodeController QRCodeRemoved event is called. This occurs when the Calibration QR marker is no longer tracked or recorded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_QRCodeRemoved(object sender, QRCode e)
        {
            EventBus.Publish(new UpdateQrObjectEvent(e, true));
        }
    }
}