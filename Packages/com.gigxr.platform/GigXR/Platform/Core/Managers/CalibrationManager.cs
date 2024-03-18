using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Interfaces;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.Core;

namespace GIGXR.Platform.Managers
{
    /// <summary>
    /// General manager to handle the calibration hooks and updates the calibration root transform data, but leaves the actual
    /// implementation details for how an app is calibrated based on the hardware.
    /// </summary>
    public class CalibrationManager : ICalibrationManager, IDisposable
    {
        public const string GigXRCode = "GIGXR";

        protected bool IsCurrentlyCalibrating { get { return CurrentCalibrationMode != ICalibrationManager.CalibrationModes.None; } }

        protected Dictionary<ICalibrationManager.CalibrationModes, Action> calibratedStartActionsPerMode = new Dictionary<ICalibrationManager.CalibrationModes, Action>();

        protected Dictionary<ICalibrationManager.CalibrationModes, Action<bool, Vector3, Quaternion>> calibratedStopActionsPerMode = new Dictionary<ICalibrationManager.CalibrationModes, Action<bool, Vector3, Quaternion>>();

        public ICalibrationManager.CalibrationModes CurrentCalibrationMode { get; protected set; }

        public ICalibrationManager.CalibrationModes LastUsedCalibrationMode { get; protected set; }

        public ContentMarkerControlMode CurrentContentMarkerControlMode { get; protected set; }

        public event EventHandler<EventArgs> ContentMarkerControlModeSet;

        protected AppEventBus EventBus { get; }

        protected IGigAssetManager AssetManager { get; }

        protected ISessionManager SessionManager { get; }

        public GIGXRCore Core { get; private set; }

        public CalibrationManager(GIGXRCore core, AppEventBus appEventBus, IGigAssetManager assetManager, ISessionManager sessionManager)
        {
            Core = core;
            AssetManager = assetManager;
            EventBus = appEventBus;
            SessionManager = sessionManager;

            EventBus.Subscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
        }

        public void StartCalibration(ICalibrationManager.CalibrationModes calibrationMode)
        {
            if (IsCurrentlyCalibrating)
            {
                DebugUtilities.LogError($"[CalibrationManager] Cannot start calibration into {calibrationMode} mode as Calibration is already in mode {CurrentCalibrationMode}");
                return;
            }

            DebugUtilities.Log("[CalibrationManager] Starting Calibration");

            if (calibratedStartActionsPerMode.ContainsKey(calibrationMode))
            {
                CurrentCalibrationMode = calibrationMode;

                calibratedStartActionsPerMode[calibrationMode]?.Invoke();
            }
            else
            {
                DebugUtilities.LogError($"[CalibrationManager] No Start Action for Calibration Mode {calibrationMode}");
            }
        }

        /// <summary>
        /// Evaluates the input decoded string against the expected calibration string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsCalibrationQr(string data)
        {
            return data == GigXRCode;
        }

        public void StopCalibration(bool calibrationWasCancelled, Vector3 targetPosition, Quaternion targetOrientation)
        {
            if (!IsCurrentlyCalibrating)
            {
                return;
            }

            if (calibratedStopActionsPerMode.ContainsKey(CurrentCalibrationMode))
            {
                LastUsedCalibrationMode = CurrentCalibrationMode;

                calibratedStopActionsPerMode[CurrentCalibrationMode]?.Invoke(calibrationWasCancelled, targetPosition, targetOrientation);
            }
            else
            {
                DebugUtilities.LogError($"[CalibrationManager] No Stop Action for Calibration Mode {CurrentCalibrationMode}");
            }

            CurrentCalibrationMode = ICalibrationManager.CalibrationModes.None;
        }

        protected void UpdateAnchorRoot(Vector3 targetPosition, Quaternion targetOrientation)
        {
            AssetManager.CalibrationRootProvider.AnchorRoot.position = targetPosition;
            AssetManager.CalibrationRootProvider.AnchorRoot.rotation = targetOrientation;
        }

        private void OnSetContentMarkerEvent(SetContentMarkerEvent @event)
        {
            AssetManager.CalibrationRootProvider.ContentMarkerRoot.localPosition = @event.contentMarkerPosition;
            AssetManager.CalibrationRootProvider.ContentMarkerRoot.localRotation = @event.contentMarkerRotation;

            // Apply the offset (local position) of the content marker to the content marker root so that the asset appears in the same
            // place while all assets use the same position values
            if (@event.assetContentMarker != null)
            {
                var worldPosition = AssetManager.CalibrationRootProvider.ContentToWorldPosition(@event.assetContentMarker.AttachedGameObject.transform.localPosition);

                var difference = worldPosition - AssetManager.CalibrationRootProvider.ContentMarkerRoot.position;

                AssetManager.CalibrationRootProvider.ContentMarkerRoot.position -= difference;
            }
        }

        public void SetContentMarkerMode(ContentMarkerControlMode contentMarkerControlMode)
        {
            CurrentContentMarkerControlMode = contentMarkerControlMode;

            ContentMarkerControlModeSet?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
        }
    }
}