using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

namespace GIGXR.Platform.HMD.UI
{
    public class SessionPromptSubScreenObject : BaseUiObject
    {
        [SerializeField]
        private GameObject HostView;

        [SerializeField]
        private GameObject cancelButton;

        [SerializeField]
        private GridObjectCollection buttonCollection;

        [SerializeField]
        private GameObject ClientView;

        private SessionScreen sessionScreen;

        private GameObject temporayDoneButton;

        private ISessionManager SessionManager { get; set; }

        private ICalibrationManager CalibrationManager { get; set; }

        private IGigAssetManager AssetManager { get; set; }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager, ICalibrationManager calibrationManager, IGigAssetManager assetManager)
        {
            SessionManager = sessionManager;
            CalibrationManager = calibrationManager;
            AssetManager = assetManager;
        }

        protected override void SubscribeToEventBuses()
        {
            EventBus.Subscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
            EventBus.Subscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
            EventBus.Unsubscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }

        public void SetData(SessionScreen sessionScreen)
        {
            this.sessionScreen = sessionScreen;
        }

        public async void SetView(bool showContentPlacement, bool hideAssets = true, bool firstView = false)
        {
            HostView.SetActive(showContentPlacement);
            ClientView.SetActive(!showContentPlacement);

            // When viewed the first time, the cancel button needs to be hidden and the user
            // forced to set the content marker
            cancelButton.gameObject.SetActive(!firstView);

            buttonCollection.UpdateCollection();

            if (hideAssets)
            {
                AssetManager.HideAll(HideAssetReasons.ContentMarker);

                await AssetManager.DisableInteractivityForAllAssetsAsync();
            }

            // If the client used QR scanning for the Anchor, then default to the Host control for the content marker
            if (!SessionManager.IsHost &&
                CalibrationManager.LastUsedCalibrationMode == ICalibrationManager.CalibrationModes.Qr)
            {
                SetHostControlled();
            }
            else if (showContentPlacement)
            {
                AssetManager.SpawnContentMarkerHandle(hideAssets);

                // If the assets are not hidden, but shown with the content marker, we still want the bounding
                // colliders and interactions to be disabled while the models are shown
                if(!hideAssets)
                {
                    await AssetManager.DisableInteractivityForAllAssetsAsync();
                }
            }

            // On the first view, show a copy of the Move/Done Button that the user is able to use as the 
            // normal button is actually hidden as it's attached to the Content Marker Asset
            if(firstView)
            {
                if(AssetManager.ContentMarkerAsset != null)
                {
                    var contentMarkerATC = AssetManager.ContentMarkerAsset.AttachedGameObject.GetComponent<ContentMarkerAssetTypeComponent>();

                    var buttonHelper = contentMarkerATC.MoveSceneButton?.GetComponentInChildren<StartContentMarkerComponent>();
                    
                    if(buttonHelper != null)
                    {
                        buttonHelper.ForceStart();
                    }

                    temporayDoneButton = contentMarkerATC.CreateSceneButton();
                    
                    temporayDoneButton.transform.position = contentMarkerATC.ProxyTransform.position + contentMarkerATC.buttonOffset;
                    temporayDoneButton.transform.rotation = contentMarkerATC.ProxyTransform.rotation;

                    // Make sure that the button follows the content marker so it moves with it's position as it is not in the same hierarchy
                    var followToolbarSolverHandler = temporayDoneButton.AddComponent<SolverHandler>();
                    followToolbarSolverHandler.TrackedTargetType = TrackedObjectType.CustomOverride;
                    followToolbarSolverHandler.TransformOverride = contentMarkerATC.ProxyTransform;
                    followToolbarSolverHandler.AdditionalOffset = contentMarkerATC.buttonOffset;

                    var followToolbar = temporayDoneButton.AddComponent<Follow>();
                    followToolbar.DefaultDistance = 0;
                    followToolbar.MinDistance = 0;
                    followToolbar.MaxDistance = 0;
                    followToolbar.OrientationType = SolverOrientationType.Unmodified;
                    followToolbar.ReorientWhenOutsideParameters = false;
                    followToolbar.IgnoreAngleClamp = true;
                    followToolbar.FaceTrackedObjectWhileClamped = false;

                    var temporyButtonContentMarkerHelper = temporayDoneButton.GetComponent<StartContentMarkerComponent>();

                    temporyButtonContentMarkerHelper.ForceStart();
                }
            }
        }

        /// <summary>
        /// Called via Unity Editor.
        /// </summary>
        public void SetHostControlled()
        {
            sessionScreen.DisplayClientSessionLog();

            CalibrationManager.SetContentMarkerMode(ContentMarkerControlMode.Host);

            SessionManager.SyncContentMarker();

            AssetManager.ShowAll(HideAssetReasons.ContentMarker);
        }

        /// <summary>
        /// Called via Unity Editor.
        /// </summary>
        public void SetSelfControlled()
        {
            CalibrationManager.SetContentMarkerMode(ContentMarkerControlMode.Self);

            // Since the client will control their content marker, show the prompt for the
            // client user to set their own content marker
            SetView(true, true, true);
        }

        /// <summary>
        /// Called via Unity Editor.
        /// </summary>
        public void SetContentMarker()
        {
            AssetManager.SetContentMarker();
        }

        /// <summary>
        /// Called via Unity Editor.
        /// </summary>
        public void CancelContentMarker()
        {
            EventBus.Publish(new CancelContentMarkerEvent());
        }

        private void OnSetContentMarkerEvent(SetContentMarkerEvent @event)
        {
            if (SessionManager.IsHost)
            {
                // As far as the host is concerned, if the response is None, they have not
                // set the content marker's position for the first time yet. When it's set to
                // Self, then we know they have responded to it at least once.
                CalibrationManager.SetContentMarkerMode(ContentMarkerControlMode.Self);

                sessionScreen.DisplayHostSessionLog();
            }
            else
            {
                sessionScreen.DisplayClientSessionLog();
            }

            SetAssetInteractabilityState(SessionManager.IsHost);

            TryDestroyTemporaryButton();
        }

        private void OnCancelContentMarkerEvent(CancelContentMarkerEvent @event)
        {
            SetAssetInteractabilityState(SessionManager.IsHost);

            if (SessionManager.IsHost)
            {
                sessionScreen.DisplayHostSessionLog();
            }
            else
            {
                sessionScreen.DisplayClientSessionLog();
            }

            AssetManager.RemoveContentMarkerHandle(true);

            TryDestroyTemporaryButton();
        }

        private void TryDestroyTemporaryButton()
        {
            if (temporayDoneButton != null)
            {
                Destroy(temporayDoneButton);
            }
        }

        private async void SetAssetInteractabilityState(bool isHost)
        {
            // Only the host should set up the bounding boxes for Edit mode
            if (isHost)
            {
                await AssetManager.EnableOrDisableInteractivityForPlayScenarioAsync();
            }
            else
            {
                await AssetManager.DisableInteractivityForAllAssetsAsync();
            }
        }
    }
}