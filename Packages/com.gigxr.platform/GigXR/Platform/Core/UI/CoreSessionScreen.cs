namespace GIGXR.Platform.Core.UI
{
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.AppEvents.Events.Scenarios;
    using GIGXR.Platform.AppEvents.Events.Session;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Managers;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Networking.EventBus.Events.InRoom;
    using GIGXR.Platform.Sessions;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.Localization.Tables;

    /// <summary>
    /// The set of prompts and actions that are the same between SessionScreens on Mobile and HMD.
    /// </summary>
    public class CoreSessionScreen
    {
        private ISessionManager SessionManager { get; }

        private INetworkManager NetworkManager { get; }

        private AppEventBus EventBus { get; }

        private StringTable _sessionStringTable { get; }

        private Transform RootScreenTransform { get; }

        public CoreSessionScreen(ISessionManager sessionManager, INetworkManager networkManager, AppEventBus eventBus, Transform rootScreenTransform, StringTable sessionStringTable, Transform sessionLogTransform = null)
        {
            SessionManager = sessionManager;
            NetworkManager = networkManager;
            EventBus = eventBus;
            _sessionStringTable = sessionStringTable;
            RootScreenTransform = rootScreenTransform;

            sessionPlacementData = new UIPlacementData()
            {
                HostTransform = RootScreenTransform
            };

            sessionLogPlacementData = new UIPlacementData()
            {
                HostTransform = sessionLogTransform ?? RootScreenTransform
            };
        }

        private CancellationTokenSource joiningSessionTokenSource;
        private CancellationTokenSource sessionClientWaitForHostPrompt;

        private UIPlacementData sessionPlacementData;

        private UIPlacementData sessionLogPlacementData;

        public void SubscribeToEvents()
        {
            NetworkManager.Subscribe<JoinedLobbyEvent>(OnJoinedLobbyEvent);
            NetworkManager.Subscribe<LeftLobbyEvent>(OnLeftLobbyEvent); 
            NetworkManager.Subscribe<HostClosedSessionNetworkEvent>(OnHostClosedSessionNetworkEvent);

            EventBus.Subscribe<StartingSyncWithHostLocalEvent>(OnStartingSyncWithHostLocalEvent);
            EventBus.Subscribe<FinishingSyncWithHostLocalEvent>(OnFinishingSyncWithHostLocalEvent);
            EventBus.Subscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
            EventBus.Subscribe<StartingJoinSessionLocalEvent>(OnStartingJoinSessionLocalEvent);
            EventBus.Subscribe<FinishingJoinSessionLocalEvent>(OnFinishingJoinSessionLocalEvent);
            EventBus.Subscribe<CancelJoinSessionLocalEvent>(OnCancelJoinSessionLocalEvent);
            EventBus.Subscribe<StartingAssetSpawningLocalEvent>(OnStartingAssetSpawningLocalEvent);
            EventBus.Subscribe<StartingRoomConnectLocalEvent>(OnStartingRoomConnectLocalEvent);
        }

        public void UnsubscribeToEvents()
        {
            NetworkManager.Unsubscribe<JoinedLobbyEvent>(OnJoinedLobbyEvent);
            NetworkManager.Unsubscribe<LeftLobbyEvent>(OnLeftLobbyEvent);
            NetworkManager.Unsubscribe<HostClosedSessionNetworkEvent>(OnHostClosedSessionNetworkEvent);

            EventBus.Unsubscribe<StartingSyncWithHostLocalEvent>(OnStartingSyncWithHostLocalEvent);
            EventBus.Unsubscribe<FinishingSyncWithHostLocalEvent>(OnFinishingSyncWithHostLocalEvent);
            EventBus.Unsubscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
            EventBus.Unsubscribe<StartingJoinSessionLocalEvent>(OnStartingJoinSessionLocalEvent);
            EventBus.Unsubscribe<FinishingJoinSessionLocalEvent>(OnFinishingJoinSessionLocalEvent);
            EventBus.Unsubscribe<CancelJoinSessionLocalEvent>(OnCancelJoinSessionLocalEvent);
            EventBus.Unsubscribe<StartingAssetSpawningLocalEvent>(OnStartingAssetSpawningLocalEvent);
            EventBus.Unsubscribe<StartingRoomConnectLocalEvent>(OnStartingRoomConnectLocalEvent);
        }

        #region NetworkManagerEventHandlers

        private void OnJoinedLobbyEvent(JoinedLobbyEvent @event)
        {
            ShowWaitForHostPrompt();
        }

        private void OnLeftLobbyEvent(LeftLobbyEvent @event)
        {
            BringDownWaitForHostPrompt();
        }

        #endregion

        #region EventBusEventHandlers

        private void OnFinishingJoinSessionLocalEvent(FinishingJoinSessionLocalEvent @event)
        {
            CleanupJoiningSessionPrompt();
        }

        private void OnStartingSyncWithHostLocalEvent(StartingSyncWithHostLocalEvent @event)
        {
            UpdateJoiningSessionPrompt(_sessionStringTable.GetEntry("syncingText").GetLocalizedString());
        }

        private void OnFinishingSyncWithHostLocalEvent(FinishingSyncWithHostLocalEvent @event)
        {
            CleanupJoiningSessionPrompt();
        }

        private void OnReturnToSessionListEvent(ReturnToSessionListEvent @event)
        {
            CleanupJoiningSessionPrompt();
        }

        private void OnStartingAssetSpawningLocalEvent(StartingAssetSpawningLocalEvent @event)
        {
            UpdateJoiningSessionPrompt(_sessionStringTable.GetEntry("spawnAssetText").GetLocalizedString());
        }

        private void OnStartingRoomConnectLocalEvent(StartingRoomConnectLocalEvent @event)
        {
            UpdateJoiningSessionPrompt(_sessionStringTable.GetEntry("connectRoomText").GetLocalizedString());
        }

        private void OnStartingJoinSessionLocalEvent(StartingJoinSessionLocalEvent @event)
        {
            DisplayJoiningSessionPrompt(@event.JoinMethod);
        }

        private void OnCancelJoinSessionLocalEvent(CancelJoinSessionLocalEvent @event)
        {
            CleanupJoiningSessionPrompt();
        }

        private void OnHostClosedSessionNetworkEvent(HostClosedSessionNetworkEvent @event)
        {
            EventBus.Publish
                (
                    new ShowTimedPromptEvent
                    (
                        _sessionStringTable.GetEntry("sessionClosedText").GetLocalizedString(),
                        null,
                        sessionPlacementData
                    )
                );
        }

        #endregion

        #region PublicAPI

        public void DisplayHostDisconnectPrompt()
        {
            var leaveSessionButtons = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("leaveSessionPrompt").GetLocalizedString(),
                        onPressAction = async () =>
                                        {
                                            await SessionManager.LeaveSessionAsync();
                                        }
                    }
                };

            EventBus.Publish
            (
                // Show this prompt until the Host and the Master Client match
                new ShowPredicatePromptEvent
                (
                    () => SessionManager.HostId == NetworkManager.MasterClientId,
                    _sessionStringTable.GetEntry("hostLeftSession").GetLocalizedString(),
                    leaveSessionButtons,
                    PromptManager.WindowStates.Wide,
                    RootScreenTransform
                )
            );
        }

        public void ShowWaitForHostPrompt()
        {
            if (sessionClientWaitForHostPrompt == null)
            {
                sessionClientWaitForHostPrompt = new CancellationTokenSource();

                var leaveSessionButton = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("leaveSessionPrompt").GetLocalizedString(),
                        onPressAction = async () =>
                                        {
                                            await SessionManager.LeaveSessionAsync();
                                        }
                    }
                };

                // If the game does not exist but they attempt to join, then the room has not been created yet
                // Subscribe to the lobby event and wait for the host to create the room
                EventBus.Publish
                (
                    new ShowCancellablePromptEvent
                    (
                        sessionClientWaitForHostPrompt.Token,
                        _sessionStringTable.GetEntry("waitForHostText").GetLocalizedString(),
                        leaveSessionButton,
                        PromptManager.WindowStates.Wide,
                        RootScreenTransform
                    )
                );
            }
        }

        public void BringDownWaitForHostPrompt()
        {
            if (sessionClientWaitForHostPrompt != null)
            {
                sessionClientWaitForHostPrompt.Cancel();
                sessionClientWaitForHostPrompt.Dispose();
                sessionClientWaitForHostPrompt = null;
            }
        }

        private void DisplayJoiningSessionPrompt(JoinTypes joinMethod)
        {
            if (joiningSessionTokenSource == null)
            {
                joiningSessionTokenSource = new CancellationTokenSource();

                var cancelButton = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("cancelText").GetLocalizedString(),
                        onPressAction = () =>
                        {
                            SessionManager.CancelJoiningSession(joinMethod);
                        }
                    }
                };

                // Show a prompt over this screen that will be removed after the user has joined the
                // session, with a button for cancellation
                EventBus.Publish
                (
                    new ShowCancellablePromptEvent
                    (
                        joiningSessionTokenSource.Token,
                        "",
                        _sessionStringTable.GetEntry("fetchSessionDataText").GetLocalizedString(),
                        cancelButton,
                        sessionPlacementData
                    )
                );
            }
        }

        private void UpdateJoiningSessionPrompt(string text, string header = null)
        {
            if (joiningSessionTokenSource != null)
            {
                // Show a prompt over this screen that will be removed after the user has joined the
                // session, with a button for cancellation
                EventBus.Publish
                (
                    new UpdateCancellablePromptEvent
                    (
                        joiningSessionTokenSource.Token,
                        header,
                        text
                    )
                );
            }
        }

        private void CleanupJoiningSessionPrompt()
        {
            joiningSessionTokenSource?.Cancel();
            joiningSessionTokenSource?.Dispose();
            joiningSessionTokenSource = null;
        }

        #endregion
    }
}