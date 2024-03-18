namespace GIGXR.Platform.Mobile.UI
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.AppEvents.Events.Calibration;
    using GIGXR.Platform.AppEvents.Events.Session;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Core.DependencyValidator;
    using GIGXR.Platform.Core.UI;
    using GIGXR.Platform.Core.User;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Managers;
    using GIGXR.Platform.Mobile.AppEvents.Events.AR;
    using GIGXR.Platform.Mobile.AppEvents.Events.UI;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Networking.EventBus.Events.InRoom;
    using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
    using GIGXR.Platform.Sessions;
    using System;
    using UnityEngine;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Tables;
    using TMPro;
    using GIGXR.Platform.Scenarios.GigAssets;

    /// <summary>
    ///     The session screen has no UI elements of its own. It provides
    ///     a public API by which other UI elements can re-scan while
    ///     in session, and handles screen switching for reference
    ///     by all UI elements present in session.
    ///     
    /// TODO Mobile does not use StringTables like HMD SessionScreen
    /// </summary>
    public class SessionScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.Session;

        [SerializeField, RequireDependency]
        private LocalizedStringTable sessionStringTable;

        private StringTable _sessionStringTable;

        [SerializeField]
        private GameObject userCardHolder;

        [SerializeField]
        private GameObject disconnectMessagePrefab;

        [SerializeField, RequireDependency]
        private TextMeshProUGUI rescanText;

        private CoreSessionScreen coreScreen;

        private bool isSettingContentMarkerRescan;

        private bool isSettingAnchorRootRescan;

        private INetworkManager NetworkManager { get; set; }
        private ISessionManager SessionManager { get; set; }
        private IGigAssetManager AssetManager { get; set; }
        private ICalibrationManager CalibrationManager { get; set; }

        protected override void Awake()
        {
            base.Awake();

            // Could move this somewhere else, but this allows the UserCards to know where to parent themselves
            // when they are instantiated over the network for the mobile users, which is actually unused and
            // off the screen for these users, but they need to create their own so that the HMD users will see
            // their user cards
            UserCard.SetupAllUserCards(userCardHolder.transform, 0);
        }

        #region Initialization

        [InjectDependencies]
        public void Construct
        (
            INetworkManager networkManager,
            ISessionManager sessionManager,
            ICalibrationManager calibrationManager
        )
        {
            NetworkManager = networkManager;
            SessionManager = sessionManager;
            CalibrationManager = calibrationManager;
            AssetManager = SessionManager.ScenarioManager.AssetManager;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _sessionStringTable = sessionStringTable.GetTable();

            if(isConstructed)
            {
                coreScreen = new CoreSessionScreen(SessionManager, NetworkManager, EventBus, RootScreenTransform, _sessionStringTable);
                coreScreen.SubscribeToEvents();
            }
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
            EventBus.Subscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
            EventBus.Subscribe<EnteredWaitingRoomLobbyEvent>(OnEnteredWaitingRoomLobbyEvent);
            EventBus.Subscribe<LeftWaitingRoomLobbyEvent>(OnLeftWaitingRoomLobbyEvent);
            EventBus.Subscribe<SetUserLocationLocalEvent>(OnSetUserLocationLocalEvent);
            EventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);

            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
            NetworkManager.Subscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Subscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);
        }

        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();

            coreScreen.UnsubscribeToEvents();

            EventBus.Unsubscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
            EventBus.Unsubscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
            EventBus.Unsubscribe<EnteredWaitingRoomLobbyEvent>(OnEnteredWaitingRoomLobbyEvent);
            EventBus.Unsubscribe<LeftWaitingRoomLobbyEvent>(OnLeftWaitingRoomLobbyEvent);
            EventBus.Unsubscribe<SetUserLocationLocalEvent>(OnSetUserLocationLocalEvent);
            EventBus.Unsubscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);

            NetworkManager.Unsubscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
            NetworkManager.Unsubscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Unsubscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);
        }

        #region Event Callbacks

        private void OnArTargetPlacedEvent(ArTargetPlacedEvent @event)
        {
            isSettingAnchorRootRescan = false;

            EventBus.Publish(new StopArPlaneEvent());
        }

        private void OnSetContentMarkerEvent(SetContentMarkerEvent @event)
        {
            EventBus.Publish(new StopArPlaneEvent());

            isSettingContentMarkerRescan = false;

            rescanText.text = "Reposition";

            uiEventBus.Publish(new SwitchingActiveScreenEventMobile(ScreenTypeMobile.Session, this.ScreenType));
        }

        private void OnCancelContentMarkerEvent(CancelContentMarkerEvent @event)
        {
            rescanText.text = "Reposition";

            uiEventBus.Publish(new SwitchingActiveScreenEventMobile(ScreenTypeMobile.Session, this.ScreenType));
        }

        /// <summary>
        /// Callback from Network Manager when the Photon room is successfully joined
        /// </summary>
        private async void OnJoinedRoomNetworkEvent(JoinedRoomNetworkEvent @event)
        {
            EventBus.Publish
            (
                new ShowTimedPromptEvent
                (
                    _sessionStringTable.GetEntry("welcomeText").GetLocalizedString(@event.NickName),
                    null,
                    PromptManager.WindowStates.Wide,
                    RootScreenTransform,
                    2000
                )
            );
        }

        /// <summary>
        /// Network Manager callback for when a player has left the Photon room
        /// </summary>
        private void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            if (Guid.Parse(@event.Player.UserId) != SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId)
            {
                EventBus.Publish
                (
                    new ShowTimedPromptEvent
                    (
                        _sessionStringTable.GetEntry("userLeftSessionText").GetLocalizedString(@event.Player.NickName),
                        null,
                        PromptManager.WindowStates.Wide,
                        RootScreenTransform,
                        2000
                    )
                );
            }
        }

        /// <summary>
        /// Network Manager callback for when a player has joined the photon room
        /// </summary>
        private void OnPlayerEnteredRoomNetworkEvent(PlayerEnteredRoomNetworkEvent @event)
        {
            EventBus.Publish
            (
                new ShowTimedPromptEvent
                (
                    _sessionStringTable.GetEntry("userEnterSessionText").GetLocalizedString(@event.Player.NickName),
                    null,
                    PromptManager.WindowStates.Wide,
                    RootScreenTransform,
                    2000
                )
            );
        }

        private void OnEnteredWaitingRoomLobbyEvent(EnteredWaitingRoomLobbyEvent @event)
        {
            if (NetworkManager.InRoom)
            {
                // Display a message over the SessionLog that the host has disconnected
                EventBus.Publish(new ShowGameObjectPromptEvent(disconnectMessagePrefab));

                if (@event.FromHostDisconnection)
                {
                    coreScreen.DisplayHostDisconnectPrompt();
                }
            }
        }

        private void OnLeftWaitingRoomLobbyEvent(LeftWaitingRoomLobbyEvent @event)
        {
            if (NetworkManager.InRoom)
            {
                EventBus.Publish(new HideGameObjectPromptEvent(disconnectMessagePrefab));
            }
        }

        private void OnSetUserLocationLocalEvent(SetUserLocationLocalEvent @event)
        {
            switch(@event.JoinSessionStatus)
            {
                // If the user selected co-located, we map that to mean host controlled
                case Data.SessionParticipantStatus.InSessionColocated:
                    CalibrationManager.SetContentMarkerMode(ContentMarkerControlMode.Host);

                    // The SessionManager will handle syncing the Content Marker position, so
                    // bring down the tracking processes here
                    EventBus.Publish(new StopArPlaneEvent());

                    SessionManager.SyncContentMarker();

                    break;
                // If the user selected Remote, we map that to mean they control their own content marker
                case Data.SessionParticipantStatus.InSessionRemote:
                    CalibrationManager.SetContentMarkerMode(ContentMarkerControlMode.Self);

                    EventBus.Publish(new StartContentMarkerEvent(true));

                    break;
            }
        }

        #endregion

        /// <summary>
        /// Allow start to re-place content marker (<see cref="StartContentMarkerEvent"/>)
        /// </summary>
        public void Rescan()
        {
            // Only allow the rescan to occur while not rescanning for the anchor root
            // TODO Improve this UX
            if(!isSettingAnchorRootRescan && 
               CalibrationManager.CurrentCalibrationMode == ICalibrationManager.CalibrationModes.None)
            {
                if (!isSettingContentMarkerRescan)
                {
                    // Avoid overlapping rescan calls
                    isSettingContentMarkerRescan = true;

                    // Start scanning again
                    EventBus.Publish(new StartContentMarkerEvent(false));

                    rescanText.text = "Cancel";
                }
                else
                {
                    isSettingContentMarkerRescan = false;

                    EventBus.Publish(new CancelContentMarkerEvent());
                }
            }
        }

        /// <summary>
        /// Called from Unity Editor.
        /// </summary>
        public void RescanAnchorRoot()
        {
            // Don't allow to set the anchor root while set the content marker
            if(!isSettingContentMarkerRescan)
            {
                if (!isSettingAnchorRootRescan)
                {
                    isSettingAnchorRootRescan = true;

                    // Stop any existing calibration-in-progress
                    CalibrationManager.StopCalibration(true, Vector3.zero, Quaternion.identity);

                    EventBus.Publish(new ArSessionResetEvent());

                    EventBus.Publish(new StartAnchorRootEvent(true));

                    // Start scanning again
                    CalibrationManager.StartCalibration(ICalibrationManager.CalibrationModes.Manual);
                }
            }
        }
    }
}