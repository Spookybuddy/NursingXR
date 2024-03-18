using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Cysharp.Threading.Tasks;
using GIGXR.GMS.Models.Accounts.Responses;
using GIGXR.GMS.Models.Sessions.Responses;
using GIGXR.Platform.AppEvents.Events.Scenarios;
using GIGXR.Platform.Core.UI;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Networking.EventBus.Events.Stages;
using GIGXR.Platform.UI;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.Managers;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.AppEvents.Events.UI.ButtonEvents;
using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.HMD.AppEvents.Events;
using GIGXR.Platform.HMD.AppEvents.Events.UI;
using GIGXR.Platform.HMD.NetworkEvents.Sessions;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Core.DependencyValidator;
using GIGXR.Platform.AppEvents.Events;
using GIGXR.Platform.Interfaces;
using GIGXR.GMS.Models.Sessions.Requests;
using GIGXR.GMS.Models.Sessions;
using GIGXR.Platform.AppEvents.Events.Authentication;
using GIGXR.Platform.GMS.Exceptions;

namespace GIGXR.Platform.HMD.UI
{
    public enum SessionListTypes
    {
        ActiveSessions,
        SavedSessions,
        SessionPlans
    }

    /// <summary>
    /// HMD Specific Screen to display two subscreens, the SessionList SubScreen which displays all the available
    /// sessions that a user can join as well as the button to logout. The SessionLog SubScreen displays the information
    /// related to being in a session included showing the users who are currently in the session and buttons to leave
    /// and manage the session.
    /// </summary>
    public partial class SessionScreen : BaseScreenObject
    {
        [RequireDependency]
        public LocalizedStringTable sessionStringTable;

        [RequireDependency]
        public GameObject sessionScreenPrefab;

        [RequireDependency]
        // TODO  Do we still want to use this
        public GameObject disconnectMessagePrefab;

        [Header("User Profile")]
        [RequireDependency, SerializeField]
        private GameObject userProfilePrefab;

        [SerializeField]
        private Vector3 userProfilePositionOffset;

        [SerializeField]
        private Vector3 userProfileRotationOffset;

        protected CoreSessionScreen coreScreen;

        // There is only one allowed user profile to be displayed at a time
        protected bool userProfileIsOpen = false;

        private UserProfile userProfileInstance;

        protected UserProfile UserProfileInstance
        {
            get
            {
                if (userProfileInstance == null)
                {
                    userProfileInstance = CreateUserProfile();
                }

                return userProfileInstance;
            }
        }

        public ScreenObject ScreenObject { get { return screenObject; } }
        private ScreenObject screenObject;

        protected SessionListSubScreenObject sessionListSubScreen;
        protected SessionLogSubScreenObject sessionLogSubScreen;
        protected SessionPromptSubScreenObject sessionPromptSubScreen;

        // TODO Should probably stop creating token sources and refactor the PromptManager to be a bit more friendly
        private CancellationTokenSource promptUserHostSource;
        private CancellationTokenSource hostRequestClientUISource;
        private CancellationTokenSource hostClientRejectionUiTokenSource;

        private CancellationTokenSource clientHostCancelTransferPrompt;

        protected StringTable _sessionStringTable;

        protected ISessionManager SessionManager { get; set; }

        protected ICalibrationManager CalibrationManager { get; set; }

        protected INetworkManager NetworkManager { get; set; }

        public override ScreenType ScreenObjectType => ScreenType.SessionManagement;

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager, INetworkManager networkManager, ICalibrationManager calibrationManager)
        {
            SessionManager = sessionManager;
            NetworkManager = networkManager;      
            CalibrationManager = calibrationManager;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Bring up your screen object into the scene
            GameObject screenGameObject = Instantiate(sessionScreenPrefab, transform);
            screenObject = GetComponentInChildren<ScreenObject>(true);
            sessionLogSubScreen = GetComponentInChildren<SessionLogSubScreenObject>(true);
            sessionListSubScreen = GetComponentInChildren<SessionListSubScreenObject>(true);
            sessionPromptSubScreen = GetComponentInChildren<SessionPromptSubScreenObject>(true);

            // Hide the screen so that the EventBus and Message system controls object activation from here on out
            screenGameObject.SetActive(false);

            _sessionStringTable = sessionStringTable.GetTable();

            sessionLogSubScreen.SetSessionStringTable(_sessionStringTable);

            sessionPromptSubScreen.SetData(this);

            Initialize();

            if(isConstructed)
            {
                coreScreen = new CoreSessionScreen(SessionManager, NetworkManager, EventBus, RootScreenTransform, _sessionStringTable, sessionLogSubScreen.transform);
                coreScreen.SubscribeToEvents();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            coreScreen.UnsubscribeToEvents();

            NetworkManager.Unsubscribe<StageRenamedNetworkEvent>(OnStageRenamedNetworkEvent);
            NetworkManager.Unsubscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);
            NetworkManager.Unsubscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Unsubscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
            NetworkManager.Unsubscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Unsubscribe<JoinRoomFailedNetworkEvent>(OnJoinRoomFailedNetworkEvent);

            EventBus.Unsubscribe<SetAnchorRootEvent>(OnSetAnchorRootEvent);
            EventBus.Unsubscribe<AttemptStartSessionEvent>(OnAttemptStartSessionEvent);
            EventBus.Unsubscribe<JoinSessionCancelledEvent>(OnJoinSessionCancelledEvent);
            EventBus.Unsubscribe<ClientJoinedSessionEvent>(OnClientJoinedActiveSessionEvent);
            EventBus.Unsubscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
            EventBus.Unsubscribe<HostCreatedSessionEvent>(OnHostCreatedSessionFromSessionPlanEvent);
            EventBus.Unsubscribe<HostClosedSessionEvent>(OnHostClosedSessionEvent);
            EventBus.Unsubscribe<EnteredWaitingRoomLobbyEvent>(OnEnteredLobbyEvent);
            EventBus.Unsubscribe<SessionRenamedEvent>(OnSessionRenamedEvent);
            EventBus.Unsubscribe<OpenUserProfileEvent>(OnOpenUserProfileEvent);

            // Network Log
            NetworkManager.Unsubscribe<WriteNetworkLogNetworkEvent>(OnWriteNetworkLogNetworkEvent);
            NetworkManager.Unsubscribe<LockNetworkLogNetworkEvent>(OnLockNetworkLogNetworkEvent);

            // Host Transfer
            EventBus.Unsubscribe<ShowHostWaitPromptEvent>(OnShowHostWaitPromptEvent);
            EventBus.Unsubscribe<ShowHostTimeOutPromptEvent>(OnShowHostTimeOutPromptEvent);
            EventBus.Unsubscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
            EventBus.Unsubscribe<ShowNewHostEvent>(OnShowNewHostEvent);
            EventBus.Unsubscribe<PromptReclaimHostEvent>(OnPromptReclaimHostEvent);
            EventBus.Unsubscribe<ClientHostCancelledTransferEvent>(OnClientHostCancelledTransferEvent);
            EventBus.Unsubscribe<StartNewHostPromptEvent>(OnStartNewHostPromptEvent);
            EventBus.Unsubscribe<ShowRejectedHostPromptEvent>(OnShowRejectedHostPromptEvent);
            EventBus.Unsubscribe<ClientStartNewHostEvent>(OnClientStartNewHostEvent);
            EventBus.Unsubscribe<RemoveHostRequestEvent>(OnRemoveHostRequestEvent);
            EventBus.Unsubscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
            EventBus.Unsubscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
            EventBus.Unsubscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            // Network Log
            NetworkManager
                .RegisterNetworkEvent<WriteNetworkLogNetworkEvent,
                    WriteNetworkLogNetworkEventSerializer>(NetworkCodes.SyncWriteEventCode);
            NetworkManager
                .RegisterNetworkEvent<LockNetworkLogNetworkEvent,
                    LockNetworkLogNetworkEventSerializer>(NetworkCodes.SyncLockLog);

            NetworkManager.Subscribe<StageRenamedNetworkEvent>(OnStageRenamedNetworkEvent);
            NetworkManager.Subscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);
            NetworkManager.Subscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
            NetworkManager.Subscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Subscribe<JoinRoomFailedNetworkEvent>(OnJoinRoomFailedNetworkEvent);

            EventBus.Subscribe<SetAnchorRootEvent>(OnSetAnchorRootEvent);
            EventBus.Subscribe<AttemptStartSessionEvent>(OnAttemptStartSessionEvent);
            EventBus.Subscribe<JoinSessionCancelledEvent>(OnJoinSessionCancelledEvent);
            EventBus.Subscribe<ClientJoinedSessionEvent>(OnClientJoinedActiveSessionEvent);
            EventBus.Subscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
            EventBus.Subscribe<HostCreatedSessionEvent>(OnHostCreatedSessionFromSessionPlanEvent);
            EventBus.Subscribe<HostClosedSessionEvent>(OnHostClosedSessionEvent);
            EventBus.Subscribe<EnteredWaitingRoomLobbyEvent>(OnEnteredLobbyEvent);         
            EventBus.Subscribe<SessionRenamedEvent>(OnSessionRenamedEvent);
            EventBus.Subscribe<OpenUserProfileEvent>(OnOpenUserProfileEvent);

            // Network Log
            NetworkManager.Subscribe<WriteNetworkLogNetworkEvent>(OnWriteNetworkLogNetworkEvent);
            NetworkManager.Subscribe<LockNetworkLogNetworkEvent>(OnLockNetworkLogNetworkEvent);

            // Host Transfer
            EventBus.Subscribe<ShowHostWaitPromptEvent>(OnShowHostWaitPromptEvent);
            EventBus.Subscribe<ShowHostTimeOutPromptEvent>(OnShowHostTimeOutPromptEvent);
            EventBus.Subscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
            EventBus.Subscribe<ShowNewHostEvent>(OnShowNewHostEvent);
            EventBus.Subscribe<PromptReclaimHostEvent>(OnPromptReclaimHostEvent);
            EventBus.Subscribe<ClientHostCancelledTransferEvent>(OnClientHostCancelledTransferEvent);
            EventBus.Subscribe<StartNewHostPromptEvent>(OnStartNewHostPromptEvent);
            EventBus.Subscribe<ShowRejectedHostPromptEvent>(OnShowRejectedHostPromptEvent);
            EventBus.Subscribe<ClientStartNewHostEvent>(OnClientStartNewHostEvent);
            EventBus.Subscribe<RemoveHostRequestEvent>(OnRemoveHostRequestEvent);
            EventBus.Subscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
            EventBus.Subscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
            EventBus.Subscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }

        private void OnStageRenamedNetworkEvent(StageRenamedNetworkEvent e)
        {
            NetworkManager.RaiseNetworkEvent
                (new WriteNetworkLogNetworkEvent(NetworkEventType.StageRenamed, e.NewName));
        }

        protected override void OnSettingScreenVisibilityEvent
            (SettingScreenVisibilityEvent settingScreenVisibilityEvent)
        {
            base.OnSettingScreenVisibilityEvent(settingScreenVisibilityEvent);

            // Whenever the SessionScreen is enabled, make a determination on whether to show the SessionLog or SessionList
            if (settingScreenVisibilityEvent.TargetScreen == ScreenObjectType &&
                settingScreenVisibilityEvent.ShouldBeActive)
            {
                // Set your own first main subscreen state
                if (SessionManager.NetworkManager.InRoom)
                {
                    // For clients, the Content Marker Control Mode will be set by a prompt so as long as it's none,
                    // they have not responded to it
                    if(CalibrationManager.CurrentContentMarkerControlMode == ContentMarkerControlMode.None)
                    {
                        // Bring up the subscreen that shows the active session data
                        uiEventBus.Publish
                        (
                            new SettingActiveSubScreenEvent(ScreenObjectType, SubScreenState.Generic1)
                        );
                    }
                    else
                    {
                        // Bring up the subscreen that shows the active session data
                        uiEventBus.Publish
                        (
                            new SettingActiveSubScreenEvent(ScreenObjectType, SubScreenState.SessionLog)
                        );
                    }
                }
            }
        }

        private void DisplaySessionList()
        {
            // Bring up the toolbar now that calibration is complete, will be with the user as long as they are logged in
            uiEventBus.Publish(new SetToolbarStateEvent(true));

            // Set the Subscreen type to the SessionList
            uiEventBus.Publish(new SettingActiveSubScreenEvent(ScreenObjectType, SubScreenState.SessionsList));

            // Reset to the last selected session tab
            uiEventBus.Publish(new SelectTabFromToggleGroup((int)sessionListSubScreen.CurrentSessionListType));
        }

        private void OnSetAnchorRootEvent(SetAnchorRootEvent @events)
        {
            // Bring up your own window to display screen
            uiEventBus.Publish(new SettingScreenVisibilityEvent(ScreenObjectType, true));

            // User has calibrated while in session, display the session details again
            if (SessionManager.ActiveSession != null)
            {
                DisplaySessionLog
                (
                    SessionManager.IsHost,
                    SessionManager.IsSessionCreator
                );
            }
            // The user finished calibration after logging in, show all available sessions
            else
            {
                DisplaySessionList();

                EventBus.Publish(new AutoJoinSessionEvent());
            }
        }

        protected virtual async void OnAttemptStartSessionEvent(AttemptStartSessionEvent @event)
        {
            try
            {
                // Joining/creating sessions are expected to take a moment, so bring up the progress
                // indicator so that some feedback of progress is given to the user
                EventBus.Publish(new ShowProgressIndicatorEvent());

                switch (sessionListSubScreen.CurrentSessionListType)
                {
                    case SessionListTypes.SavedSessions:

                        var isCreator = await SessionManager.ApiClient.SessionsApi.IsSessionOwner
                        (
                            @event.SessionId,
                            SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId
                        );

                        if (isCreator)
                        {
                            await SessionManager.StartSessionAsync(@event.SessionId);

                            EventBus.Publish(new HostCreatedSessionEvent());
                        }
                        else
                        {
                            await AttemptJoin(@event.SessionId);
                        }

                        break;
                    case SessionListTypes.SessionPlans:
                        await SessionManager.StartSessionFromPlanAsync(@event.SessionId);

                        EventBus.Publish(new HostCreatedSessionEvent());

                        break;

                    default:
                        await AttemptJoin(@event.SessionId);

                        break;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Joining session {@event.SessionId} canceled.");
            }
            catch (GmsServerException e)
            {
                Debug.LogWarning($"Did not load session due to GMS issue. {e}");
                
                // TODO Could make this more descriptive about why it failed
                EventBus.Publish(new ShowTimedPromptEvent(
                    $"{_sessionStringTable.GetEntry("failedServerIssueText").GetLocalizedString()}\n{e.Message}",
                    null,
                    new UIPlacementData()
                    {
                        HostTransform = RootScreenObject.transform
                    },
                    3000));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Did not load session {@event.SessionId}. See error below");
                Debug.LogException(e);

                // TODO Could make this more descriptive about why it failed
                EventBus.Publish
                (
                    new ShowTimedPromptEvent
                    (
                        _sessionStringTable.GetEntry("failedJoinSessionText").GetLocalizedString(),
                        null,
                        PromptManager.WindowStates.Wide,
                        RootScreenObject.transform,
                        2000 
                    )
                );
            }
            finally
            {
                // We always want the buttons brought up
                uiEventBus.Publish(new SettingGlobalButtonStateEvent(true, false));

                // We always want the progress indicator to leave when done
                EventBus.Publish(new HideProgressIndicatorEvent());
            }
        }

        private async UniTask AttemptJoin(Guid sessionId)
        {
            // Joining/creating sessions are expected to take a moment, so bring up the progress indicator so that some feedback of progress is given to the user
            EventBus.Publish(new ShowProgressIndicatorEvent());

            var isOwner = await SessionManager.ApiClient.SessionsApi.IsSessionOwner(sessionId,
                            SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId);

            try
            {
                var joinedSession = await SessionManager.JoinSessionAsync(sessionId);

                if(joinedSession.Item1)
                {
                    if (isOwner)
                    {
                        // Owner starts in stopped mode right away while other users will wait for the first update from the host with the state data
                        await SessionManager.ScenarioManager.StopScenarioAsync();

                        EventBus.Publish(new HostCreatedSessionEvent());
                    }
                    else
                    {
                        EventBus.Publish(new ClientJoinedSessionEvent());
                    }
                }
                else
                {
                    // The session failed to join because it is locked
                    if(joinedSession.Item2 == nameof(SessionDetailedView.Locked))
                    {
                        var closeButton = new List<ButtonPromptInfo>()
                        {
                            new ButtonPromptInfo()
                            {
                                buttonText = _sessionStringTable.GetEntry("closeText").GetLocalizedString(),
                                onPressAction = () => sessionListSubScreen.RefreshSessions()
                            }
                        };

                        EventBus.Publish
                        (
                            new ShowPromptEvent
                            (
                                _sessionStringTable.GetEntry("joiningLockedSession").GetLocalizedString(),
                                closeButton,
                                new UIPlacementData()
                                {
                                    HostTransform = RootScreenObject.transform
                                }
                            )
                        );
                    }
                    else if(joinedSession.Item2 == "Network")
                    {
                        // The error happened at the network layer, so it will handle displaying the prompt
                        EventBus.Publish(new ReturnToSessionListEvent());
                    }
                    else
                    {
                        // TODO Could make this more descriptive about why it failed
                        EventBus.Publish
                        (
                            new ShowTimedPromptEvent
                            (
                                _sessionStringTable.GetEntry("failedJoinSessionText").GetLocalizedString(),
                                null,
                                new UIPlacementData()
                                {
                                    HostTransform = RootScreenObject.transform
                                }
                            )
                        );

                        EventBus.Publish(new ReturnToSessionListEvent());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // TODO Could make this more descriptive about why it failed
                EventBus.Publish
                (
                    new ShowTimedPromptEvent
                    (
                        _sessionStringTable.GetEntry("failedJoinSessionText").GetLocalizedString(),
                        null,
                        new UIPlacementData()
                        {
                            HostTransform = RootScreenObject.transform
                        }
                    )
                );

                EventBus.Publish(new ReturnToSessionListEvent());
            }
            finally
            {
                // Bring buttons back up so that they are all click-able again
                uiEventBus.Publish(new SettingGlobalButtonStateEvent(true, false));
            }
        }

        protected void OnJoinSessionCancelledEvent(JoinSessionCancelledEvent @event)
        {
            EventBus.Publish(new HideProgressIndicatorEvent());
        }

        private void OnReturnToSessionListEvent(ReturnToSessionListEvent @event)
        {
            DisplaySessionList();

            EventBus.Publish(new HideProgressIndicatorEvent());
        }

        public void DisplayClientSessionLog()
        {
            DisplaySessionLog(false, false);
        }

        public void DisplayHostSessionLog()
        {
            DisplaySessionLog(true, true);
        }

        private void OnClientJoinedActiveSessionEvent(ClientJoinedSessionEvent @event)
        {
            ShowContentMarkerUI(false, true, true);
        }

        private void OnHostCreatedSessionFromSessionPlanEvent(HostCreatedSessionEvent @event)
        {
            ShowContentMarkerUI(true, true, true);
        }
        
        private void OnHostClosedSessionEvent(HostClosedSessionEvent @event)
        {
            ReturnToSessionList();
        }

        private void OnEnteredLobbyEvent(EnteredWaitingRoomLobbyEvent @event)
        {
            if (NetworkManager.InRoom)
            {
                if (@event.FromHostDisconnection)
                {
                    coreScreen.DisplayHostDisconnectPrompt();
                }
            }
        }

        private void OnShowHostWaitPromptEvent(ShowHostWaitPromptEvent @event)
        {
            // Create a button that explicitly says cancel on per design requirements of this display
            var cancelButton = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("cancelText").GetLocalizedString(),
                    onPressAction = () =>
                                    {
                                        EventBus.Publish
                                            (new CancelHostRequestEvent(@event.ActorNumber));
                                    }
                }
            };

            // TODO need to pass in icons with the text to the PromptManager per design
            EventBus.Publish
            (
                new ShowCancellablePromptEvent
                (
                    @event.PromptToken,
                    _sessionStringTable.GetEntry("pleaseWaitForUserToAccept").GetLocalizedString(@event.UserName),
                    cancelButton,
                    PromptManager.WindowStates.Wide,
                    RootScreenObject.transform
                )
            );
        }

        private void OnShowHostTimeOutPromptEvent(ShowHostTimeOutPromptEvent @event)
        {
            var yesNoButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("yesText").GetLocalizedString(),
                    onPressAction = () =>
                                    {
                                        EventBus.Publish
                                        (
                                            new StartHostRequestEvent
                                                (@event.UserGuid, @event.UserName, true)
                                        );
                                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("noText").GetLocalizedString(),
                    onPressAction = () =>
                                    {
                                        EventBus.Publish(new HostTransferTimeoutEvent());
                                    }
                }
            };

            EventBus.Publish
            (
                new ShowPromptEvent
                (
                    _sessionStringTable.GetEntry("userTimeout").GetLocalizedString(@event.UserName),
                    yesNoButtonList,
                    PromptManager.WindowStates.Wide,
                    RootScreenObject.transform
                )
            );
        }

        private void OnHostTransferCompleteEvent(HostTransferCompleteEvent @event)
        {
            // Update the sidebars so that the new host will be able to close the session, while the former host will be able to leave
            sessionLogSubScreen.SetSidebarButton(SessionManager.IsHost, SessionManager.IsSessionCreator);

            // If the host transfer is complete and you are now the MasterClient, display a dialog per design requirements
            if (SessionManager.IsHost)
            {
                var continueButton = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry
                                ("continueText")
                            .GetLocalizedString(),
                        // No action, just confirmation
                    }
                };

                // TODO need to pass in icons with the text to the PromptManager per design
                EventBus.Publish
                (
                    new ShowPromptEvent
                    (
                        _sessionStringTable.GetEntry("newHost").GetLocalizedString(),
                        continueButton,
                        PromptManager.WindowStates.Wide,
                        RootScreenObject.transform
                    )
                );
            }

            // Only show the reclaim host section if you're the session owner and not the master client
            UserProfileInstance.ShowReclaimHost
            (
                !SessionManager.IsHost &&
                SessionManager.ActiveSession.CreatedById ==
                SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId
            );

            // Make sure the host controls match the correct user's host status, unless you have your own card open
            if (userProfileIsOpen &&
                (UserProfileInstance.CurrentUserId !=
                 SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId))
            {
                UserProfileInstance.ShowHostControls(SessionManager.IsHost);
            }
            else if (userProfileIsOpen)
            {
                UserProfileInstance.ShowHostControls(false);
            }
        }

        protected void OnShowNewHostEvent(ShowNewHostEvent @event)
        {
            RemoveHostRequestPrompt();

            var continueButton = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("continueText").GetLocalizedString(),
                    // No action, just confirmation
                }
            };

            // TODO need to pass in icons with the text to the PromptManager per design
            EventBus.Publish
            (
                new ShowPromptEvent
                (
                    _sessionStringTable.GetEntry("userNewHost").GetLocalizedString(@event.UserName),
                    continueButton,
                    PromptManager.WindowStates.Wide,
                    RootScreenObject.transform
                )
            );
        }

        private void OnPromptReclaimHostEvent(PromptReclaimHostEvent @event)
        {
            var yesNoButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("confirmText").GetLocalizedString(),
                    onPressAction = () =>
                                    {
                                        EventBus.Publish
                                        (
                                            new ReturnHostToSessionOwnerEvent(SessionManager.HostId)
                                        );
                                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("cancelText").GetLocalizedString()
                    // No action
                }
            };

            EventBus.Publish
            (
                new ShowPromptEvent
                (
                    _sessionStringTable.GetEntry("promptReclaimHost").GetLocalizedString(),
                    yesNoButtonList,
                    PromptManager.WindowStates.Wide,
                    RootScreenObject.transform
                )
            );
        }

        protected void OnStartNewHostPromptEvent(StartNewHostPromptEvent @event)
        {
            // A rejection notice is still up for the previous host transfer
            if (hostClientRejectionUiTokenSource != null)
            {
                hostClientRejectionUiTokenSource.Cancel();
                hostClientRejectionUiTokenSource.Dispose();
                hostClientRejectionUiTokenSource = null;
            }

            GIGXR.Platform.Utilities.Logger.Info($"Prompting {@event.UserName} to be the new host", nameof(SessionManager));

            // Bring up a UI window that indicates the user is in a waiting state for the other user to respond to their request
            promptUserHostSource = new CancellationTokenSource();

            EventBus.Publish
            (
                new ShowHostWaitPromptEvent
                    (@event.UserName, @event.ActorNumber, promptUserHostSource.Token)
            );

            // Set up a timeout in case the user takes a long time to respond
            UniTask.Create
                (
                    async () =>
                    {
                        // TODO Should externalize the 20s timeout somewhere
                        await UniTask.Delay(TimeSpan.FromSeconds(20), false, PlayerLoopTiming.Update, promptUserHostSource.Token);

                        // Timeout has occurred, bring down the old UI and re-prompt the user if they want to try again
                        RemoveHostRequestPrompt();

                        EventBus.Publish
                            (new ShowHostTimeOutPromptEvent(@event.UserName, @event.UserGuid));
                    }
                );
        }

        private void OnShowRejectedHostPromptEvent(ShowRejectedHostPromptEvent @event)
        {
            RemoveHostRequestPrompt();

            GIGXR.Platform.Utilities.Logger.Info($"User {@event.UserName} has rejected being the new host", nameof(SessionManager));

            hostClientRejectionUiTokenSource = new CancellationTokenSource();

            var closeButton = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("closeText").GetLocalizedString(),
                    onPressAction = () =>
                                    {
                                        hostClientRejectionUiTokenSource.Dispose();
                                        hostClientRejectionUiTokenSource = null;
                                    }
                }
            };

            // TODO need to pass in icons with the text to the PromptManager per design
            EventBus.Publish
            (
                new ShowCancellablePromptEvent
                (
                    hostClientRejectionUiTokenSource.Token,
                    _sessionStringTable.GetEntry
                            ("userRejectsHostText")
                        .GetLocalizedString(@event.UserName),
                    closeButton,
                    PromptManager.WindowStates.Wide,
                    RootScreenObject.transform
                )
            );
        }

        protected void OnClientStartNewHostEvent(ClientStartNewHostEvent @event)
        {
            // Don't let cancel prompts and new host prompts collide
            if (clientHostCancelTransferPrompt != null)
            {
                clientHostCancelTransferPrompt.Cancel();
                clientHostCancelTransferPrompt.Dispose();
                clientHostCancelTransferPrompt = null;
            }

            hostRequestClientUISource = new CancellationTokenSource();

            var yesNoButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("yesText").GetLocalizedString(),
                    onPressAction = () =>
                                    {
                                        @event.ClientAcceptsAction?.Invoke();

                                        hostRequestClientUISource?.Cancel();
                                        hostRequestClientUISource?.Dispose();
                                        hostRequestClientUISource = null;
                                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("noText").GetLocalizedString(),
                    onPressAction = () =>
                                    {
                                        @event.ClientRejectsAction?.Invoke();

                                        hostRequestClientUISource?.Cancel();
                                        hostRequestClientUISource?.Dispose();
                                        hostRequestClientUISource = null;
                                    }
                }
            };

            // Display a prompt to the user asking them to become the new host
            // TODO Need to pass in icons with the text to the PromptManager per design
            EventBus.Publish
            (
                new ShowCancellablePromptEvent
                (
                    hostRequestClientUISource.Token,
                    _sessionStringTable.GetEntry
                            ("acceptHostRequestText")
                        .GetLocalizedString(@event.HostName),
                    yesNoButtonList,
                    PromptManager.WindowStates.Wide,
                    RootScreenObject.transform
                )
            );

            // Set up a timeout in case the user takes a long time to respond
            _ = UniTask.Create
            (
                async () =>
                {
                    // TODO Should externalize the 20s timeout somewhere else?
                    await UniTask.Delay(TimeSpan.FromSeconds(20), false, PlayerLoopTiming.Update, hostRequestClientUISource.Token);

                    hostRequestClientUISource?.Cancel();
                    hostRequestClientUISource?.Dispose();
                    hostRequestClientUISource = null;
                }
            );
        }

        private void OnClientHostCancelledTransferEvent(ClientHostCancelledTransferEvent @event)
        {
            RemoveClientRequestPrompt();

            // TODO Add in StringTables or maybe remove this UI reference from SessionManager
            var continueButton = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry
                            ("continueText")
                        .GetLocalizedString(@event.HostName),
                    onPressAction = () =>
                                    {
                                        clientHostCancelTransferPrompt.Dispose();
                                        clientHostCancelTransferPrompt = null;
                                    }
                    // No action, just confirmation
                }
            };

            clientHostCancelTransferPrompt = new CancellationTokenSource();

            // TODO need to pass in icons with the text to the PromptManager per design
            EventBus.Publish
            (
                new ShowCancellablePromptEvent
                (
                    clientHostCancelTransferPrompt.Token,
                    _sessionStringTable.GetEntry
                            ("hostCancelledText")
                        .GetLocalizedString(@event.HostName),
                    continueButton,
                    PromptManager.WindowStates.Wide,
                    RootScreenObject.transform
                )
            );
        }

        private void OnRemoveHostRequestEvent(RemoveHostRequestEvent @event)
        {
            RemoveHostRequestPrompt();
        }

        private void OnSessionRenamedEvent(SessionRenamedEvent @event)
        {
            // Update the network log with this info
            NetworkManager.RaiseNetworkEvent
                (new WriteNetworkLogNetworkEvent(NetworkEventType.SessionRenamed, @event.NewSessionName));
        }

        protected virtual async void OnOpenUserProfileEvent(OpenUserProfileEvent @event)
        {
            if (!userProfileIsOpen)
            {
                userProfileIsOpen = true;

                // Update the instance with the user's details
                await UpdateUserProfileDetails(@event);

                // Make the window visible
                UserProfileInstance.ShowUserProfile();
            }
            // Replace the user profile details with the new clicked on user card
            else if (UserProfileInstance.CurrentUserId != @event.UserId)
            {
                await UpdateUserProfileDetails(@event);

                // Make the window visible
                UserProfileInstance.ShowUserProfile();
            }
            // The same user profile was clicked, so close it
            else
            {
                UserProfileInstance.CloseUserProfile();
            }

            // If the window is open and you are the host of the session, show the host panel, unless it's your own user card
            if (userProfileIsOpen)
            {
                var userCardIsMine
                    = SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId ==
                      @event.UserId;
                var isSessionHost = SessionManager.IsHost;
                var isSessionOwner = SessionManager.ActiveSession.CreatedById ==
                                     SessionManager.ApiClient.AccountsApi.AuthenticatedAccount
                                         .AccountId;
                var userCardIsHost = SessionManager.HostId == @event.UserId;

                var hostControlsOpen = SessionManager.IsHost && !userCardIsMine;

                // Do not show host controls on your own user card
                UserProfileInstance.ShowHostControls(hostControlsOpen);

                // Special case, if the user card is the session host or session owner and the current user is the session owner but not the host, then show a button to reclaim the host status
                UserProfileInstance.ShowReclaimHost
                (
                    !isSessionHost &&
                    (userCardIsHost || userCardIsMine) &&
                    isSessionOwner &&
                    !hostControlsOpen
                );
            }
            else
            {
                UserProfileInstance.ShowHostControls(false);
                UserProfileInstance.ShowReclaimHost(false);
            }
        }

        protected void ShowContentMarkerUI(bool showContentPlacement, bool hideAssets, bool firstView)
        {
            uiEventBus.Publish(new SwitchingActiveScreenEvent(ScreenObjectType));

            // We are using Generic1 to represent the SessionPrompt screen, when a client joins, they must answer
            // if they will control the content marker or if the host will
            uiEventBus.Publish(new SettingActiveSubScreenEvent(ScreenObjectType, SubScreenState.Generic1));

            uiEventBus.Publish(new SetToolbarButtonsStateEvent(true));

            sessionPromptSubScreen.SetView(showContentPlacement, hideAssets, firstView);
        }

        protected void OnStartContentMarkerEvent(StartContentMarkerEvent @event)
        {
            ShowContentMarkerUI(true, @event.WithAssetsHidden, false);
        }

        protected void OnSetContentMarkerEvent(SetContentMarkerEvent @event)
        {
            uiEventBus.Publish(new SetToolbarButtonsStateEvent(false));
        }

        protected void OnCancelContentMarkerEvent(CancelContentMarkerEvent @event)
        {
            uiEventBus.Publish(new SetToolbarButtonsStateEvent(false));
        }

        protected virtual async UniTask UpdateUserProfileDetails(OpenUserProfileEvent @event)
        {
            AccountDetailedView userDetails
                = await SessionManager.ApiClient.AccountsApi.GetAccountProfileAsync(@event.UserId);

            UserProfileInstance.SetUserDetails(@event.UserCardReference, userDetails);

            UserProfileInstance.SetDeviceIcon(@event.UserCardReference.DeviceString);
        }

        private void RemoveClientRequestPrompt()
        {
            hostRequestClientUISource?.Cancel();
            hostRequestClientUISource?.Dispose();
            hostRequestClientUISource = null;
        }

        private void RemoveHostRequestPrompt()
        {
            promptUserHostSource.Cancel();
            promptUserHostSource.Dispose();
            promptUserHostSource = null;
        }

        private void ReturnToSessionList()
        {
            // Bring up the main Session Management screen
            uiEventBus.Publish(new SettingScreenVisibilityEvent(ScreenObjectType, true));
        }

        private void DisplaySessionLog(bool isHost, bool isCreator)
        {
            // Clear the session list info before moving onto the Session Log
            sessionListSubScreen.Clear();

            // Set the Session Window to the In-Session Host UI
            uiEventBus.Publish
                (new SettingActiveSubScreenEvent(ScreenObjectType, SubScreenState.SessionLog));

            sessionLogSubScreen.SetSidebarButton(isHost, isCreator);
        }

        #region NetworkLog Network EventHandlers

        private void OnWriteNetworkLogNetworkEvent(WriteNetworkLogNetworkEvent e)
        {
            EventBus.Publish(new WriteToNetworkLogEvent(e.EventType, e.Message));
        }

        private void OnLockNetworkLogNetworkEvent(LockNetworkLogNetworkEvent e)
        {
            EventBus.Publish(new LockNetworkLogEvent(e.IsReadOnly));
        }

        #endregion

        // --- Public Methods:

        /// <summary>
        /// Instructs the session manager to kick off an ad hoc session.
        /// </summary>
        public void StartAdHocSession()
        {
            SessionManager.StartAdHocSessionAsync();
        }

        // --- NetworkManager Event Handlers:

        private void OnJoinedRoomNetworkEvent(JoinedRoomNetworkEvent @event)
        {
            GIGXR.Platform.Utilities.Logger.Info($"User {@event.NickName} has joined the session", nameof(SessionManager));

            sessionLogSubScreen.SetSessionNameText(SessionManager.ActiveSession.SessionName);

            EventBus.Publish(new HideProgressIndicatorEvent());

            if (NetworkManager.IsMasterClient)
            {
                NetworkManager.RaiseNetworkEvent(new LockNetworkLogNetworkEvent(false));
                NetworkManager.RaiseNetworkEvent
                (
                    new WriteNetworkLogNetworkEvent
                        (NetworkEventType.SessionStarted, SessionManager.ActiveSession.SessionName)
                );
                NetworkManager.RaiseNetworkEvent
                    (new WriteNetworkLogNetworkEvent(NetworkEventType.UserJoined, @event.NickName));
            }
        }

        protected virtual void OnLeftRoomNetworkEvent(LeftRoomNetworkEvent @event)
        {
            // Close the user panel if it is open when the user leaves the room
            if (userProfileIsOpen)
            {
                UserProfileInstance.CloseUserProfile();
            }
        }

        private void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            GIGXR.Platform.Utilities.Logger.Info($"User {@event.Player.NickName} has left the session", nameof(SessionManager));

            if (NetworkManager.IsMasterClient)
            {
                NetworkManager.RaiseNetworkEvent
                (
                    new WriteNetworkLogNetworkEvent
                        (NetworkEventType.UserLeft, @event.Player.NickName)
                );
            }

            // Close the User Panel if the open user panel is the panel of the user who just left
            if (userProfileIsOpen &&
                UserProfileInstance.CurrentUserId == Guid.Parse(@event.Player.UserId))
            {
                UserProfileInstance.CloseUserProfile();
            }
        }

        private void OnPlayerEnteredRoomNetworkEvent(PlayerEnteredRoomNetworkEvent @event)
        {
            GIGXR.Platform.Utilities.Logger.Info($"User {@event.Player.NickName} has joined the session", nameof(SessionManager));

            if (NetworkManager.IsMasterClient)
            {
                NetworkManager.RaiseNetworkEvent
                (
                    new WriteNetworkLogNetworkEvent
                        (NetworkEventType.UserJoined, @event.Player.NickName)
                );
            }
        }

        private void OnJoinRoomFailedNetworkEvent(JoinRoomFailedNetworkEvent @event)
        {
            // Publish that it failed to join along with the message from the network
            EventBus.Publish
            (
                new ShowTimedPromptEvent
                (
                    $"{_sessionStringTable.GetEntry("failedJoinSessionText").GetLocalizedString()}\n{@event.NetworkName} Issue\n{@event.Message}",
                    null,
                    new UIPlacementData()
                    {
                        HostTransform = RootScreenObject.transform
                    }
                )
            );
        }

        // User Profile

        private UserProfile CreateUserProfile()
        {
            GameObject userProfileGameObject = Instantiate(userProfilePrefab, RootScreenObject.transform);

            userProfileGameObject.transform.localPosition = userProfilePositionOffset;
            userProfileGameObject.transform.localRotation = Quaternion.Euler(userProfileRotationOffset);

            userProfileGameObject.SetActive(false);

            return userProfileGameObject.GetComponent<UserProfile>();
        }
    }
}