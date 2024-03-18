namespace GIGXR.Platform.HMD.QR
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.MixedReality.QR;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using GIGXR.Platform.HMD.Interfaces;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.HMD.AppEvents.Events;
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Managers;

    /// <summary>
    /// Class to manage the scanning and decoding of QR codes using the HoloLens PV camera and webcam,
    /// for calibration and authentication respectively.
    /// </summary>
    public class QrCodeManager : IQrCodeManager
    {
        public event EventHandler<bool> QrCodesTrackingStateChanged;
        public event EventHandler<string> QrCodeSeen;
        public event EventHandler<QRCode> QrCodeAdded;
        public event EventHandler<QRCode> QrCodeUpdated;
        public event EventHandler<QRCode> QrCodeRemoved;

        private readonly Queue<ActionData> pendingActions = new Queue<ActionData>();
        private CancellationTokenSource updateRoutineSource = null;

        private QRCodeWatcher? qrCodeWatcher = null;

        private QRCodeWatcherAccessStatus accessStatus;

        private bool isTrackerRunning;
        private bool isSupported;

        private TimeSpan delayTimeSpan;
        private DateTime qrCodeStartTime;

        private CancellationTokenSource qrTrackingCancellationToken;

        private QRCodeDecodeControllerForWSA QrController;

        private AppEventBus EventBus { get; set; }

        public QrCodeManager(QRCodeDecodeControllerForWSA qrCodeDecodeController, AppEventBus appEventBus)
        {
            // get a reference to the QRCodeDecoder for authentication.
            QrController = qrCodeDecodeController;
            EventBus = appEventBus;

            delayTimeSpan = TimeSpan.FromSeconds(.1f);

            // on device, check the capability is accepted for camera access.
#if UNITY_EDITOR
            isSupported = false;
#else
            isSupported = QRCodeWatcher.IsSupported();
#endif
        }

        /// <summary>
        /// https://github.com/microsoft/MixedReality-WorldLockingTools-Samples/issues/20#issuecomment-806338759
        /// </summary>
        /// <returns></returns>
        private async UniTask<bool> GetPermissions()
        {
#if WINDOWS_UWP
            try
            {
                var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings 
                {
                    StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Video
                };

                await new Windows.Media.Capture.MediaCapture().InitializeAsync(settings);
                
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
#else
            return true;
#endif
        }

        private async UniTask StartupQrTrackingAsync(string promptMessage)
        {
            if (isSupported)
            {
                accessStatus = await QRCodeWatcher.RequestAccessAsync();

                // once capability access has been accepted, set up the QR tracking object for calibration.
                if (accessStatus == QRCodeWatcherAccessStatus.Allowed)
                {
                    SetupQrTracking();

                    // Handle events from the QR code plugin
                    if (updateRoutineSource == null)
                    {
                        updateRoutineSource = new CancellationTokenSource();

                        // We don't want to await this task as it functions as an update loop
                        _ = UpdateRoutineAsync(updateRoutineSource.Token);
                    }

                    EventBus.Publish(new SetQrFeedbackTextEvent(promptMessage));
                }
                else if (accessStatus == QRCodeWatcherAccessStatus.DeniedByUser)
                {
                    EventBus.Publish(new QrDeniedFeedbackEvent());
                }
                else
                {
                    EventBus.Publish(new SetQrFeedbackTextEvent("Camera capability not allowed."));
                }
            }
            else
            {
                EventBus.Publish(new SetQrFeedbackTextEvent("Camera capability not supported."));
            }
        }

        /// <summary>
        /// Starts the QR tracking operation.
        /// </summary>
        public async void StartQrTracking(string promptMessage)
        {
            var permissionsGot = await GetPermissions();

            if (permissionsGot)
            {
                await StartupQrTrackingAsync(promptMessage);

                if (isTrackerRunning)
                {
                    DebugUtilities.LogError("[QrCodeManager] QR tracker was already running");
                    return;
                }

                // Set the scene text object
                EventBus.Publish(new StartedQrTrackingEvent());

#if UNITY_EDITOR
                // Tell the QR controller to start working if in the Editor
                QrController.StartWork();

                QrController.onQRScanFinished += QrController_onQRScanFinished;
#endif

                try
                {
                    qrCodeWatcher?.Start();

                    isTrackerRunning = true;
                    QrCodesTrackingStateChanged?.Invoke(this, true);
                }
                catch (Exception ex)
                {
                    DebugUtilities.LogError($"[QRCodesManager] Starting QRCodeWatcher Exception: {ex}");

                    CancelQrTracking();
                }
            }
            else
            {
                DebugUtilities.LogError($"[QRCodesManager] Could not get permissions when requested.");

                CancelQrTracking();

                // If a user denies camera permissions, then we won't get the permissions, use the request here
                // to grab the access status
                if (isSupported)
                {
                    var accessStatus = await QRCodeWatcher.RequestAccessAsync();

                    if (accessStatus == QRCodeWatcherAccessStatus.DeniedByUser)
                    {
                        EventBus.Publish(new QrDeniedFeedbackEvent());
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void QrController_onQRScanFinished(object sender, string e)
        {
            QrCodeSeen?.Invoke(this, e);
        }
#endif

        public void CancelQrTracking()
        {
            StopQrTracking(true);

            // todo ; mostly just to test something:
            EventBus.Publish(new HideProgressIndicatorEvent());
        }

        /// <summary>
        /// Ends the QR tracking operation.
        /// </summary>
        public void StopQrTracking()
        {
            StopQrTracking(false);
        }

        private void StopQrTracking(bool wasCanceled)
        {
            DebugUtilities.Log($"[QRCodesManager] Stop QR tracking Canceled?: {wasCanceled}");

            if (qrCodeWatcher != null)
            {
                qrCodeWatcher.Added -= QRCodeWatcher_Added;
                qrCodeWatcher.Updated -= QRCodeWatcher_Updated;
                qrCodeWatcher.Removed -= QRCodeWatcher_Removed;
            }

#if UNITY_EDITOR
            if (QrController != null &&
                QrController.IsRunning)
            {
                QrController.onQRScanFinished -= QrController_onQRScanFinished;

                QrController.StopWork();
            }
#endif

            if (updateRoutineSource != null)
            {
                updateRoutineSource.Cancel();
                updateRoutineSource.Dispose();
                updateRoutineSource = null;
            }

            // if we have an active cancellation token from the timeout awaiter, cancel it now.
            if (qrTrackingCancellationToken != null)
            {
                qrTrackingCancellationToken.Cancel();
                qrTrackingCancellationToken.Dispose();
                qrTrackingCancellationToken = null;
            }

            EventBus.Publish(new StopQrTrackingEvent(wasCanceled));

            isTrackerRunning = false;
            qrCodeWatcher?.Stop();

            // and notify all handlers.
            QrCodesTrackingStateChanged?.Invoke(this, false);
        }

        /// <summary>
        /// Called when the QRCode Plugin detects a code has been removed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void QRCodeWatcher_Removed(object sender, QRCodeRemovedEventArgs args)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, args.Code));
            }
        }

        /// <summary>
        /// Called when the QR Code plugin updates the transform data for an existing QR code object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void QRCodeWatcher_Updated(object sender, QRCodeUpdatedEventArgs args)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, args.Code));
            }
        }

        /// <summary>
        /// Called when the QR Code Plugin detects a new QR code that we do not have cached.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void QRCodeWatcher_Added(object sender, QRCodeAddedEventArgs args)
        {
            lock (pendingActions)
            {
                // Ignore QR codes that were added before we were interested in tracking QR codes
                if (args.Code.LastDetectedTime > qrCodeStartTime)
                {
                    pendingActions.Enqueue(new ActionData(ActionData.Type.Added, args.Code));
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the QRCodeWatcher, and registers for its events.
        /// </summary>
        private void SetupQrTracking()
        {
            try
            {
                if (qrCodeWatcher == null)
                {
                    qrCodeWatcher = new QRCodeWatcher();
                }

                qrCodeStartTime = DateTime.Now;

                qrCodeWatcher.Added += QRCodeWatcher_Added;
                qrCodeWatcher.Updated += QRCodeWatcher_Updated;
                qrCodeWatcher.Removed += QRCodeWatcher_Removed;

                isTrackerRunning = false;
            }
            catch (Exception ex)
            {

                DebugUtilities.LogError($"[QRCodesManager] Exception: {ex}");
            }
        }

        /// <summary>
        /// Handles events from the QR Code plugin. Caches the calibration QR and sets/updates the spatial coordinate data for it.
        /// </summary>
        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();

                    switch (action.type)
                    {
                        case ActionData.Type.Added:
                            {
                                try
                                {
                                    DebugUtilities.Log($"[QrCodeManager] QRCodeWatcher_Added {action.qrCode}");
                                    // notify all handlers (in most use cases, just the Calibration Manager) that the QR code has been found. 
                                    QrCodeAdded?.Invoke(this, action.qrCode);
                                }
                                catch (Exception ex)
                                {
                                    DebugUtilities.LogError($"[QrCodeManager] QRCodeWatcher_Added Exception: {ex.Message}");
                                }

                                break;
                            }
                        case ActionData.Type.Updated:
                            {
                                // Notify all handlers
                                try
                                {
                                    DebugUtilities.Log($"[QrCodeManager] QRCodeWatcher_Updated {action.qrCode}");
                                    QrCodeUpdated?.Invoke(this, action.qrCode);
                                }
                                catch (Exception ex)
                                {
                                    DebugUtilities.LogError($"[QrCodeManager] QRCodeWatcher_Updated Exception: {ex.Message}");
                                }

                                break;
                            }
                        case ActionData.Type.Removed:
                            {
                                // Notify all handlers
                                try
                                {
                                    DebugUtilities.Log($"[QrCodeManager] QRCodeWatcher_Removed {action.qrCode}");
                                    QrCodeRemoved?.Invoke(this, action.qrCode);
                                }
                                catch (Exception ex)
                                {
                                    DebugUtilities.LogError($"[QrCodeManager] QRCodeWatcher_Removed Exception: {ex.Message}");
                                }

                                break;
                            }
                    }
                }
            }
        }

        // --- Tasks:

        private async UniTask UpdateRoutineAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(delayTimeSpan);

                HandleEvents();
            }
        }

        private struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            }

            public Type type;
            public QRCode qrCode;

            public ActionData
            (
                Type type,
                QRCode qRCode
            ) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }
    }
}