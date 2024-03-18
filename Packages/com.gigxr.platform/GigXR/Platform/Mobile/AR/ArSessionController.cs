namespace GIGXR.Platform.Mobile.AR
{
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Mobile.AppEvents.Events.AR;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Networking.EventBus.Events.Connection;
    using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
    using GIGXR.Platform.AppEvents.Events.Calibration;
    using GIGXR.Platform.Scenarios.GigAssets;

    /// <summary>
    ///     The ArSessionController listens for AR scanning
    ///     and network events, and controls core AR components.
    ///     
    ///     See <see cref="ARSession"/> <see cref="ARPlaneManager"/>
    ///     <see cref="PlaneDetection"/>
    /// </summary>
    [RequireComponent(typeof(ARSession))]
    [RequireComponent(typeof(ARPlaneManager))]
    public class ArSessionController : MonoBehaviour
    {
        public ARSession BaseSession { get; private set; }
        public ARPlaneManager BasePlaneManager { get; private set; }
        public PlaneDetection BasePlaneDetection { get; private set; }
        public ARSessionOrigin SessionOrigin { get; private set; }
        public ARRaycastManager RaycastManager { get; private set; }

        private AppEventBus EventBus { get; set; }

        private INetworkManager NetworkManager { get; set; }

        private IGigAssetManager AssetManager { get; set; }

        // If we're in the process of resetting, we don't want to start another reset.
        // We could disable buttons, but it's a lot easier to just ignore any calls made
        // from the UI when a reset is in progress.
        private bool resetIsInProgress;

        private GameObject planePrefab;

        #region Initialization and Cleanup

        private void Awake()
        {
            BaseSession = GetComponent<ARSession>();

            SessionOrigin = GetComponent<ARSessionOrigin>();

            BasePlaneManager = GetComponent<ARPlaneManager>();

            RaycastManager = GetComponent<ARRaycastManager>();

            planePrefab = FindObjectOfType<MobileCompositionRoot>().MobileProfile.ArPlanePrefab;

            BasePlaneManager.planePrefab = planePrefab;

            BasePlaneDetection = Camera.main.gameObject.GetComponent<PlaneDetection>();

            BasePlaneDetection.SetPlaneManager(BasePlaneManager);

            Debug.Assert
            (
                BasePlaneDetection != null,
                "ArSessionController requires PlaneDetection on the main camera."
            );
        }

        [InjectDependencies]
        public void Construct(INetworkManager networkManager, AppEventBus eventBus, IGigAssetManager assetManager)
        {
            EventBus = eventBus;
            NetworkManager = networkManager;
            AssetManager = assetManager;

            NetworkManager.Subscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Subscribe<JoinRoomFailedNetworkEvent>(OnJoinRoomFailedNetworkEvent);
            NetworkManager.Subscribe<DisconnectedNetworkEvent>(OnDisconnectedNetworkEvent);

            EventBus.Subscribe<ArStartScanningEvent>(OnArStartScanningEvent);
            EventBus.Subscribe<StopArPlaneEvent>(OnStopArPlaneEvent);
            EventBus.Subscribe<StartArPlaneEvent>(OnStartArPlaneEvent);
            EventBus.Subscribe<ArSessionResetEvent>(OnArSessionResetEvent);
            EventBus.Subscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
            EventBus.Subscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }

        private void OnDestroy()
        {
            NetworkManager.Unsubscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Unsubscribe<JoinRoomFailedNetworkEvent>(OnJoinRoomFailedNetworkEvent);
            NetworkManager.Unsubscribe<DisconnectedNetworkEvent>(OnDisconnectedNetworkEvent);

            EventBus.Unsubscribe<ArStartScanningEvent>(OnArStartScanningEvent);
            EventBus.Unsubscribe<StartArPlaneEvent>(OnStartArPlaneEvent);
            EventBus.Unsubscribe<StopArPlaneEvent>(OnStopArPlaneEvent);
            EventBus.Unsubscribe<ArSessionResetEvent>(OnArSessionResetEvent);
            EventBus.Unsubscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
            EventBus.Unsubscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }

        #endregion

        #region Event Handlers

        private void OnLeftRoomNetworkEvent(LeftRoomNetworkEvent @event)
        {
            EventBus.Publish(new ArSessionResetEvent());
        }

        private void OnJoinRoomFailedNetworkEvent(JoinRoomFailedNetworkEvent @event)
        {
            EventBus.Publish(new ArSessionResetEvent());
        }

        private void OnDisconnectedNetworkEvent(DisconnectedNetworkEvent @event)
        {
            EventBus.Publish(new ArSessionResetEvent());
        }

        private void OnArStartScanningEvent(ArStartScanningEvent @event)
        {
            Debug.Log("ArSessionController OnArStartScanningEvent");

            StartTracking();

            StartPlaneTracking();
        }

        private void OnStartArPlaneEvent(StartArPlaneEvent @event)
        {
            StartPlaneTracking();
        }

        private void OnStopArPlaneEvent(StopArPlaneEvent @event)
        {
            StopPlaneTracking();
        }

        public void StartTracking(bool withReset = true)
        {
            if (withReset)
            {
                BaseSession.Reset();
            }

            BaseSession.enabled = true;
        }

        private void StartPlaneTracking()
        {
            BasePlaneManager.planePrefab = planePrefab;

            // If the BasePlaneManager is already enabled, then there is plane data from the scan with
            // a plane already active. Bring this plane back up to use (e.g. For the content marker)
            if (BasePlaneManager.enabled)
            {
                BasePlaneManager.SetTrackablesActive(true);
            }
            else
            {
                BasePlaneManager.enabled = true;
            }

            BasePlaneDetection.StartPlaneDetection(RaycastManager);
        }

        private void StopPlaneTracking()
        {
            // Set the plane prefab to null so that the AR Plane tracker is unable to create
            // new visual planes, but the user will be able to use the plane to set
            // the content marker using the previous
            // https://forum.unity.com/threads/arfoundation-hide-arplanes-trackables-without-deactivating-visualization.1052615/#post-6814529
            BasePlaneManager.planePrefab = null;

            BasePlaneDetection.StopPlaneDetection();

            // Do not disable If you disable the ARPlaneManager - any ARReferencePoint
            // objects will lose their anchoring and begin to drift, change position or
            // rotation. https://forum.unity.com/threads/arfoundation-applyworldmap-makecontentappearat-issue.616648/
            //BasePlaneManager.enabled = false;

            // Hide the visuals of the plane, but keep the data around
            BasePlaneManager.SetTrackablesActive(false);
        }

        // This is intended as the only entry point to reset the AR session.
        private void OnArSessionResetEvent(ArSessionResetEvent @event)
        {
            // Block overlapping resets
            if (resetIsInProgress)
                return;

            resetIsInProgress = true;

            // Give any utilities relying on the AR session a chance to
            // prepare for the reset event.
            EventBus.Publish(new ArSessionResetStartingEvent());

            // Reset planes and session.
            BasePlaneManager.enabled = false;
            BasePlaneDetection.StopPlaneDetection();

            BaseSession.Reset();

            // Disable the session.
            BaseSession.enabled = false;

            // Let subscribers know that reset is complete.
            EventBus.Publish(new ArSessionResetCompleteEvent());

            resetIsInProgress = false;
        }

        private void OnStartContentMarkerEvent(StartContentMarkerEvent @event)
        {
            Debug.Log("ArSessionController OnStartContentMarkerEvent");

            // Hide all the assets so they don't interfere with setting the content
            if(@event.WithAssetsHidden)
            {
                AssetManager.HideAll(HideAssetReasons.ContentMarker);
            }

            // When starting with content marker, we want to make sure that the previous detected planes are still available
            // so they ray-casting for positioning the content marker will still work
            StartTracking(false);

            StartPlaneTracking();
        }

        private void OnCancelContentMarkerEvent(CancelContentMarkerEvent @event)
        {
            Debug.Log("ArSessionController OnCancelContentMarkerEvent");

            StopPlaneTracking();
        }

        #endregion
    }
}
