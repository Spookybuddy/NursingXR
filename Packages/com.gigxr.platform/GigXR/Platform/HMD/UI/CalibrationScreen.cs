namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.HMD.AppEvents.Events.Authentication;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.UI;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.AppEvents.Events.Calibration;
    using GIGXR.Platform.HMD.QR;
    using System;
    using GIGXR.Platform.HMD.AppEvents.Events;
    using Microsoft.MixedReality.QR;
    using GIGXR.Platform.Utilities;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
    using Microsoft.MixedReality.Toolkit.UI;
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.HMD.Interfaces;
    using GIGXR.Platform.Managers;
    using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

    /// <summary>
    /// Class to interface Calibration Buttons to the Calibration Manager whilst avoiding cross prefab dependency.
    /// Some platforms require this screen whilst others do not.
    /// </summary>
    public class CalibrationScreen : BaseScreenObject
    {
        [SerializeField]
        private GameObject QrCodeObjectPrefab;

        // Visible GameObject used in manual calibration of the anchor point
        protected Transform calibrationAnchorHandleInstance;

        private GameObject qrCodeObjectInstance;
        private QrCodeMonoBehavior qrCodeMonoBehavior;
        private TextMeshProUGUI qrCalibrationText;

        private bool hasSetAnchorOnce = false;

        private bool qrCodeDetected = false;

        private UIPlacementData calibrationPlacementData;

        private ICalibrationManager AnchorCalibrationManager { get; set; }

        private IGigAssetManager AssetManager { get; set; }

        private ProfileManager ProfileManager { get; set; }

        private IQrCodeManager QrCodeManager { get; set; }

        public override ScreenType ScreenObjectType => ScreenType.Calibration;

        [InjectDependencies]
        public void Construct(ICalibrationManager calibrationManager, IGigAssetManager assetManager, ProfileManager profileManager, IQrCodeManager qrCodeManager)
        {
            AnchorCalibrationManager = calibrationManager;
            AssetManager = assetManager;
            ProfileManager = profileManager;
            QrCodeManager = qrCodeManager;

            QrCodeManager.QrCodeSeen += QrCodeManager_QrCodeSeen;
        }

        private void QrCodeManager_QrCodeSeen(object sender, string e)
        {
            if(e == CalibrationManager.GigXRCode)
            {
                SpawnQrCodeMonoBehavior(Guid.Empty, null);
            }
        }

        #region UnityAPI

        protected void Awake()
        {
            calibrationPlacementData = new UIPlacementData()
            {
                HostTransform = RootScreenTransform
            };

            SpatialGraphCoordinateSystem.PositionUpdatedEvent += OnPositionUpdated;

            // A bit of a mouthful, but we only need to get the TextMesh component to set some text and wanted to avoid extra classes
            qrCalibrationText = GetComponentsInChildren<SubScreenObject>(true)
                                    .Where((currentSubScreen) => currentSubScreen.SubState == SubScreenState.QRCalibration)
                                    .First()
                                    .GetComponentInChildren<TextMeshProUGUI>(true);

            RootScreenObject.ScreenBroughtUp += ScreenBroughtUp;
            RootScreenObject.ScreenBroughtDown += ScreenBroughtDown;
        }

        private void ScreenBroughtUp(object sender, EventArgs e)
        {
            // Whenever the screen is brought up, show the position of the calibration handle if the user has set the anchor before
            if(hasSetAnchorOnce)
            {
                SpawnCalibrationHandle(false);
            }
        }

        private void ScreenBroughtDown(object sender, EventArgs e)
        {
            // Only try to remove the calibration visual if a calibration process did not bring it up
            if (AnchorCalibrationManager.CurrentCalibrationMode == ICalibrationManager.CalibrationModes.None)
            {
                TryToDestroyAnchorRootHandle();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SpatialGraphCoordinateSystem.PositionUpdatedEvent -= OnPositionUpdated;

            RootScreenObject.ScreenBroughtUp -= ScreenBroughtUp;
            RootScreenObject.ScreenBroughtDown -= ScreenBroughtDown;

            QrCodeManager.QrCodeSeen -= QrCodeManager_QrCodeSeen;

            EventBus.Unsubscribe<SuccessfulAuthenticationFinishScreenEvent>(OnAuthenticatedUserEvent);
            EventBus.Unsubscribe<SetAnchorRootEvent>(OnSetAnchorRootEvent);
            EventBus.Unsubscribe<UpdateQrObjectEvent>(OnUpdateQrObjectEvent);
            EventBus.Unsubscribe<SetQrFeedbackTextEvent>(OnSetQrFeedbackTextEvent);
        }

        #endregion

        #region BaseScreenObjectOverrides

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<SuccessfulAuthenticationFinishScreenEvent>(OnAuthenticatedUserEvent);
            EventBus.Subscribe<SetAnchorRootEvent>(OnSetAnchorRootEvent);
            EventBus.Subscribe<UpdateQrObjectEvent>(OnUpdateQrObjectEvent);
            EventBus.Subscribe<SetQrFeedbackTextEvent>(OnSetQrFeedbackTextEvent);
        }

        #endregion

        #region EventBusHandlers

        // Bring up your own window when authentication is finished
        private void OnAuthenticatedUserEvent(SuccessfulAuthenticationFinishScreenEvent @event)
        {
            // Bring up your own window
            uiEventBus.Publish(new SwitchingActiveScreenEvent(ScreenType.Calibration));
        }

        private void OnUpdateQrObjectEvent(UpdateQrObjectEvent @event)
        {
            if (@event.Remove)
            {
                // Remove whatever QR Code you have
                if (@event.QrCodeToUpdate == null)
                {
                    TryToDestroyQrObjectInstance();
                }
                // The QR Code provided has to match the Instance one
                else
                {
                    if (@event.QrCodeToUpdate == qrCodeMonoBehavior?.QrCode)
                    {
                        TryToDestroyQrObjectInstance();
                    }
                }
            }
            else
            {
                // The class SpatialGraphCoordinateSystem handles updating the position based on the position of the QR Code, this makes
                // sure the display objects exists
                SpawnQrCodeMonoBehavior(@event.QrCodeToUpdate.SpatialGraphNodeId, @event.QrCodeToUpdate);
            }
        }

        private void OnSetAnchorRootEvent(SetAnchorRootEvent @events)
        {
            hasSetAnchorOnce = true;

            // We no longer need the calibration cube and we don't expect the user to calibrate a
            // whole lot, so destroy the object
            TryToDestroyAnchorRootHandle();

            TryToDestroyQrObjectInstance();
        }

        #endregion

        #region EventHandlers

        private void OnPositionUpdated(Vector3 newPosition, Quaternion rot)
        {
            if(calibrationAnchorHandleInstance == null)
            {
                SpawnCalibrationHandle(false);
            }

            Vector3 newRotation = new Vector3(0, rot.eulerAngles.y, 0);

            calibrationAnchorHandleInstance.transform.position = newPosition;

            calibrationAnchorHandleInstance.transform.eulerAngles = newRotation;
        }

        private void OnSetQrFeedbackTextEvent(SetQrFeedbackTextEvent @event)
        {
            if (qrCalibrationText != null)
            {
                qrCalibrationText.text = @event.FeedbackText;
            }
        }

        #endregion

        #region PublicAPI

        /// <summary>
        /// Instructs the Calibration Manage to start QR calibration.
        /// 
        /// Called via Unity Editor
        /// </summary>
        public void StartQRCalibration()
        {
            DebugUtilities.Log("[CalibrationScreen] Start QR Calibration");

            AnchorCalibrationManager.StartCalibration(ICalibrationManager.CalibrationModes.Qr);

            // When a QR code is first detected, then the calibration handle will spawn
        }

        /// <summary>
        /// Instructs the Calibration Manager to start manual calibration.
        /// 
        /// Called via Unity Editor
        /// </summary>
        public void StartManualCalibration()
        {
            DebugUtilities.Log("[CalibrationScreen] Start Manual Calibration");

            SpawnCalibrationHandle();

            AnchorCalibrationManager.StartCalibration(ICalibrationManager.CalibrationModes.Manual);
        }

        /// <summary>
        /// Instructs the Calibration Manager to stop calibration and set the calibration screen to the menu sub state.
        /// Called via the Unity Editor
        /// </summary>
        public void FinishCalibration()
        {
            // If in the QR Scanning state and a QR code hasn't been detected, give a prompt
            if(AnchorCalibrationManager.CurrentCalibrationMode == ICalibrationManager.CalibrationModes.Qr &&
               !qrCodeDetected)
            {
                EventBus.Publish(new ShowTimedPromptEvent("QR Code not detected", null, calibrationPlacementData, 2000));

                // Redirect the user to make a selection again for the anchoring method
                uiEventBus.Publish(new SettingActiveSubScreenEvent(ScreenObjectType, SubScreenState.CalibrationMenu));

                StopCalibrationProcess(true);
            }
            else
            {
                // Bring down the main calibration menu
                uiEventBus.Publish(new SettingScreenVisibilityEvent(ScreenObjectType, false));

                StopCalibrationProcess(false);
            }
        }

        /// <summary>
        /// Called from the Unity editor.
        /// </summary>
        public void OnHitBackButton()
        {
            StopCalibrationProcess(true);

            // Bring down all the subscreen menus
            uiEventBus.Publish(new SettingActiveSubScreenEvent(ScreenObjectType, SubScreenState.CalibrationMenu));

            // The user will be back at the main calibration menu so show the non-interactable calibration handle visual
            if (hasSetAnchorOnce)
            {
                SpawnCalibrationHandle(false);
            }
        }

        #endregion

        #region PrivateMethods

        private void TryToDestroyAnchorRootHandle()
        {
            if (calibrationAnchorHandleInstance != null)
            {
                DebugUtilities.Log("[CalibrationScreen] Destroying Anchor Root Handle");

                Destroy(calibrationAnchorHandleInstance.gameObject);

                calibrationAnchorHandleInstance = null;

                qrCodeDetected = false;
            }
        }

        private void TryToDestroyQrObjectInstance()
        {
            if (qrCodeObjectInstance != null)
            {
                Destroy(qrCodeObjectInstance);
                qrCodeObjectInstance = null;
                qrCodeMonoBehavior = null;
            }
        }

        private void SpawnQrCodeMonoBehavior(Guid spatialGraphNode, QRCode qrCode)
        {
            if (qrCodeObjectInstance == null)
            {
                qrCodeObjectInstance = Instantiate(QrCodeObjectPrefab);
                qrCodeMonoBehavior = qrCodeObjectInstance.GetComponent<QrCodeMonoBehavior>();

                if(qrCode != null)
                {
                    qrCodeMonoBehavior.Setup(spatialGraphNode, qrCode);
                }

                qrCodeDetected = true;

                SpawnCalibrationHandle(false);

                // Only in the Editor is this script needed, otherwise SpatialGraphCoordinateSystem.PositionUpdatedEvent will be sent out on UWP devices
                // and update the position accordingly
#if UNITY_EDITOR
                SetupFollower(qrCodeObjectInstance.transform, calibrationAnchorHandleInstance);
#endif
            }
        }

        private void SetupFollower(Transform leader, Transform follower)
        {
            var followToolbarSolverHandler = follower.gameObject.AddComponent<SolverHandler>();
            followToolbarSolverHandler.TrackedTargetType = TrackedObjectType.CustomOverride;
            followToolbarSolverHandler.TransformOverride = leader;

            var followToolbar = follower.gameObject.AddComponent<Follow>();
            followToolbar.DefaultDistance = 0;
            followToolbar.MinDistance = 0;
            followToolbar.MaxDistance = 0;
            followToolbar.OrientationType = SolverOrientationType.FollowTrackedObject;
            followToolbar.ReorientWhenOutsideParameters = false;
            followToolbar.IgnoreAngleClamp = true;
            followToolbar.FaceTrackedObjectWhileClamped = false;
        }

        private void SpawnCalibrationHandle(bool interactable = true)
        {
            if (calibrationAnchorHandleInstance == null)
            {
                DebugUtilities.Log("[CalibrationScreen] Spawning Calibration Handle");

                calibrationAnchorHandleInstance = Instantiate(ProfileManager.CalibrationProfile.DefaultCalibrationHandle).transform;
            }

            // Always reset the position of the calibration marker
            if (interactable)
            {
                // Place the calibration marker below the screen and at the floor so that the user can
                // interact with it and place it easily
                calibrationAnchorHandleInstance.position = new Vector3(RootScreenTransform.position.x,
                                                                       RootScreenTransform.position.y - 0.35f,
                                                                       RootScreenTransform.position.z);

                // Now point it towards the user
                calibrationAnchorHandleInstance.GetComponent<RotateTowardsTarget>()?.RotateTowards();
            }
            else
            {
                // The calibration handle is for viewing purposes, don't reset the position
                calibrationAnchorHandleInstance.position = AssetManager.CalibrationRootProvider.AnchorRoot.position;
                calibrationAnchorHandleInstance.rotation = AssetManager.CalibrationRootProvider.AnchorRoot.rotation;
            }

            calibrationAnchorHandleInstance.GetComponent<BoundsControl>().enabled = interactable;
            calibrationAnchorHandleInstance.GetComponent<ObjectManipulator>().enabled = interactable;
        }

        /// <summary>
        /// Stop calibration without changing screen state. 
        /// </summary>
        private void StopCalibrationProcess(bool calibrationWasCancelled)
        {
            DebugUtilities.Log($"[CalibrationScreen] Stopping Calibration {(calibrationWasCancelled ? "Cancelled" : "")}");

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            // This will be null when doing QR scanning in the Editor
            if(calibrationAnchorHandleInstance != null)
            {
                position = calibrationAnchorHandleInstance.position;
                rotation = calibrationAnchorHandleInstance.rotation;
            }

            // Use the location of the calibration handle to set the location of the asset root
            AnchorCalibrationManager.StopCalibration(calibrationWasCancelled, position, rotation);

            TryToDestroyAnchorRootHandle();
        }

        #endregion
    }
}