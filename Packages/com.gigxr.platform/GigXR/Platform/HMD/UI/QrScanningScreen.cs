namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.HMD.AppEvents.Events;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.HMD.AppEvents.Events.Authentication;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using UnityEngine;

    /// <summary>
    /// UI class that handles the Window that appears for HMD users when scanning for QR Codes.
    /// </summary>
    public class QrScanningScreen : BaseScreenObject
    {
        [SerializeField]
        private GameObject scanGridPrefab;

#if UNITY_EDITOR
        [SerializeField]
        [Header("Device camera reference. Will only be enabled during QR scanning")]
        private GameObject deviceCamera;

        [SerializeField]
        private CameraPlaneController cameraPlaneController;
#endif

        private GameObject scanGridInstance;

        public override ScreenType ScreenObjectType => ScreenType.QrScanning;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Spawn the scan grid prefab so the user has some 'guides' for lining up the QR Code, the prefab uses MRTK head solvers
            // to follow the user's gaze
            if (scanGridPrefab != null)
            {
                scanGridInstance = Instantiate(scanGridPrefab);

                // make sure the scangrid is off.
                scanGridInstance.SetActive(false);
            }

            Initialize();
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<FailedAuthenticationFinishScreenEvent>(OnFailedAuthenticationFinishScreenEvent);

            EventBus.Subscribe<StartedQrTrackingEvent>(OnStartQrTrackingEvent);
            EventBus.Subscribe<StopQrTrackingEvent>(OnStopQrTrackingEvent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventBus.Unsubscribe<StartedQrTrackingEvent>(OnStartQrTrackingEvent);
            EventBus.Unsubscribe<StopQrTrackingEvent>(OnStopQrTrackingEvent);
        }

        private void OnFailedAuthenticationFinishScreenEvent(FailedAuthenticationFinishScreenEvent @event)
        {
            uiEventBus.Publish
            (
                new SwitchingActiveScreenEvent(ScreenType.Authentication, this.ScreenObjectType)
            );
        }

        private void OnStartQrTrackingEvent(StartedQrTrackingEvent @event)
        {
            DebugUtilities.Log("[QrScanningScreen] OnStartQrTrackingEvent");

            scanGridInstance.SetActive(true);

            // Debug login via QR code + webcam
#if UNITY_EDITOR
            cameraPlaneController.GetComponent<MeshRenderer>().enabled = true;

            deviceCamera.SetActive(true);
#endif
        }

        private void OnStopQrTrackingEvent(StopQrTrackingEvent @event)
        {
#if UNITY_EDITOR
            // Debug login via QR code + webcam
            cameraPlaneController.GetComponent<MeshRenderer>().enabled = false;

            deviceCamera.SetActive(false);
#endif

            scanGridInstance.SetActive(false);
        }
    }
}