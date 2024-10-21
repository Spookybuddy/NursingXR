using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using GIGXR.GMS.Clients;
using GIGXR.GMS.Models.Accounts.Responses;
using GIGXR.GMS.Models.Sessions;
using GIGXR.GMS.Models.Sessions.Requests;
using GIGXR.GMS.Models.Sessions.Responses;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events;
using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.AppEvents.Events.Scenarios;
using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Core.FeatureManagement;
using GIGXR.Platform.Core.Settings;
using GIGXR.Platform.Core.User;
using GIGXR.Platform.Data;
using GIGXR.Platform.GMS;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Networking.EventBus.Events;
using GIGXR.Platform.Networking.EventBus.Events.Connection;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
using GIGXR.Platform.Networking.EventBus.Events.Sessions;
using GIGXR.Platform.Networking.EventBus.Events.Stages;
using GIGXR.Platform.Scenarios.EventArgs;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.GMS.Exceptions;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus.Events.Scenarios;
using Newtonsoft.Json.Linq;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace GIGXR.Platform.Sessions
{
    /// <summary>
    /// Responsible for connecting the various systems together to make a cohesive session. This class works with the other manager classes
    /// to enable a session to be networked, allowing users to interact with assets, stage, and session data to teach other users in the virtual environment.
    /// </summary>
    public partial class SessionManager : ISessionManager, IDisposable
    {
        public event EventHandler<EventArgs> SessionStartedSave;

        public event EventHandler<EventArgs> SessionFinishedSave;

        public class SessionSavedArgs : EventArgs
        {
            public bool MarkedAsSavedInGMS { get; private set; }

            public SessionSavedArgs(bool markedSaved)
            {
                MarkedAsSavedInGMS = markedSaved;
            }
        }

        #region Dependencies

        public IScenarioManager ScenarioManager { get; }

        public INetworkManager NetworkManager { get; }

        public GmsApiClient ApiClient { get; }

        public AppEventBus EventBus { get; }

        public ProfileManager ProfileManager { get; }

        private IFeatureManager FeatureManager { get; }

        private IDependencyProvider DependencyProvider { get; }

        // The calibration manager has to be created last due to the Build Platform
        // so when the session manager is created, this won't be possible to retrieve
        // yet, so instead use the IDependencyProvider directly when the value is accessed
        // the first time to retrieve the dependency
        private ICalibrationManager CalibrationManager
        {
            get
            {
                if (_calibrationManager == null)
                {
                    _calibrationManager = DependencyProvider.GetDependency<ICalibrationManager>();
                }

                return _calibrationManager;
            }
        }

        private ICalibrationManager _calibrationManager;

        #endregion

        public SessionManager
        (
            IScenarioManager scenarioManager,
            INetworkManager networkManager,
            GmsApiClient gmsApiClient,
            AppEventBus appEvent,
            ProfileManager profileManager,
            IFeatureManager featureManager,
            IDependencyProvider provider
        )
        {
            ScenarioManager = scenarioManager;
            NetworkManager = networkManager;
            ApiClient = gmsApiClient;
            EventBus = appEvent;
            ProfileManager = profileManager;
            FeatureManager = featureManager;
            DependencyProvider = provider;

            GIGXR.Platform.Utilities.Logger.AddTaggedLogger(nameof(SessionManager), "Session Console");

            userCapabilities = new SessionUser();

            // Local Scenario Events

            ScenarioManager.ScenarioPlaying += ScenarioManager_ScenarioPlayingAsync;

            // Session manager is in charge of scenario authority while present.
            ScenarioManager.AssetManager.AssetContext.SetContext(
                nameof(ScenarioManager.AssetManager.AssetContext.IsScenarioAuthority),
                () => { return this.IsHost; }
            );

            // Room Changes

            NetworkManager.Subscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);
            NetworkManager.Subscribe<JoinRoomFailedNetworkEvent>(OnJoinRoomFailedNetworkEvent);
            NetworkManager.Subscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
            NetworkManager.Subscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Subscribe<RoomPropertiesUpdateNetworkEvent>(OnRoomPropertiesUpdateNetworkEvent);

            NetworkManager.Subscribe<DisconnectedNetworkEvent>(OnDisconnectedNetworkEvent);

            // TODO Move all EventRegistration of codes out of here
            NetworkManager
                .RegisterNetworkEvent<ClientReadyNetworkEvent, ClientReadyNetworkEventSerializer>
                    (NetworkCodes.ClientReadyEventCode);

            // Session Management

            NetworkManager
                .RegisterNetworkEvent<UserKickedNetworkEvent, UserKickedNetworkEventSerializer>
                    (NetworkCodes.SyncKickEventCode);

            NetworkManager
                .RegisterNetworkEvent<SessionRenamedNetworkEvent,
                    SessionRenamedNetworkEventSerializer>(NetworkCodes.SyncEditSessionEventCode);

            NetworkManager
                .RegisterNetworkEvent<UpdateFromGMSNetworkEvent,
                    UpdateFromGMSNetworkEventSerializer>(NetworkCodes.UpdateFromGMSEventCode);

            NetworkManager
                .RegisterNetworkEvent<GoToWaitingRoomLobbyNetworkEvent, GoToWaitingRoomLobbyNetworkEventSerializer>
                    (NetworkCodes.GoToLobbyNetworkEventCode);

            NetworkManager
                .RegisterNetworkEvent<LeaveWaitingRoomLobbyNetworkEvent, LeaveWaitingRoomLobbyNetworkEventSerializer>
                    (NetworkCodes.LeaveLobbyNetworkEventCode);

            NetworkManager
                .RegisterNetworkEvent<HostClosedSessionNetworkEvent, HostClosedSessionNetworkEventSerializer>
                    (NetworkCodes.HostClosedSessionEventCode);

            NetworkManager
                .RegisterNetworkEvent<ContentMarkerUpdateNetworkEvent, ContentMarkerUpdateNetworkEventSerializer>
                    (NetworkCodes.ContentMarkerUpdate);

            NetworkManager
                .RegisterNetworkEvent<RequestContentMarkerNetworkEvent, RequestContentMarkerNetworkEventSerializer>
                    (NetworkCodes.ContentMarkerRequest);

            NetworkManager.Subscribe<UserKickedNetworkEvent>(OnUserKickedNetworkEvent);
            NetworkManager.Subscribe<SessionRenamedNetworkEvent>(OnSessionRenamedNetworkEvent);
            NetworkManager.Subscribe<MasterClientSwitchedEvent>(OnMasterClientSwitchedNetworkEvent);
            NetworkManager.Subscribe<GoToWaitingRoomLobbyNetworkEvent>(OnGoToWaitingRoomLobbyNetworkEvent);
            NetworkManager.Subscribe<LeaveWaitingRoomLobbyNetworkEvent>(OnLeaveWaitingRoomLobbyNetworkEvent);
            NetworkManager.Subscribe<RoomListUpdateEvent>(OnRoomListUpdateEvent);

            // Scenario Management

            NetworkManager
                .RegisterNetworkEvent<ScenarioPlayingNetworkEvent,
                    ScenarioPlayingNetworkEventSerializer>(NetworkCodes.ScenarioPlaying);

            NetworkManager
                .RegisterNetworkEvent<ScenarioPausedNetworkEvent,
                    ScenarioPausedNetworkEventSerializer>(NetworkCodes.ScenarioPaused);

            NetworkManager
                .RegisterNetworkEvent<ScenarioStoppedNetworkEvent,
                    ScenarioStoppedNetworkEventSerializer>(NetworkCodes.ScenarioStopped);

            // Stage Management

            NetworkManager
                .RegisterNetworkEvent<AllStagesRemovedNetworkEvent,
                    AllStagesRemovedNetworkEventSerializer>(NetworkCodes.AllStagesRemovedEventCode);

            NetworkManager
                .RegisterNetworkEvent<StageLoadedNetworkEvent, StageLoadedNetworkEventSerializer>
                    (NetworkCodes.StageLoadedEventCode);

            NetworkManager
                .RegisterNetworkEvent<StageCreatedNetworkEvent, StageCreatedNetworkEventSerializer>
                    (NetworkCodes.SyncRecordEventCode);

            NetworkManager
                .RegisterNetworkEvent<StageDuplicatedNetworkEvent, StageDuplicatedEventSerializer>
                    (NetworkCodes.StageDuplicateEventCode);

            NetworkManager
                .RegisterNetworkEvent<StageRenamedNetworkEvent, StageRenamedNetworkEventSerializer>
                    (NetworkCodes.SyncRenameStageEventCode);

            NetworkManager
                .RegisterNetworkEvent<StageRemovedNetworkEvent, StageRemovedNetworkEventSerializer>
                    (NetworkCodes.SyncRemoveStageEventCode);

            NetworkManager
                .RegisterNetworkEvent<StagesSwappedNetworkEvent,
                    StagesSwappedNetworkEventSerializer>(NetworkCodes.SyncReorderStageEventCode);

            NetworkManager
                .RegisterNetworkEvent<StagesSwitchedNetworkEvent,
                    StagesSwitchedNetworkEventSerializer>(NetworkCodes.SyncSwitchStageEventCode);

            // Networked Host Management
            NetworkManager
                .RegisterNetworkEvent<RequestUserToHostNetworkEvent,
                    RequestUserToHostNetworkEventSerializer>(NetworkCodes.RequestUserToHost);

            NetworkManager
                .RegisterNetworkEvent<CancelRequestUserToHostNetworkEvent,
                        CancelRequestUserToHostNetworkEventSerializer>
                    (NetworkCodes.CancelRequestUserToHost);

            NetworkManager
                .RegisterNetworkEvent<AcceptHostRequestNetworkEvent,
                    AcceptHostRequestNetworkEventSerializer>(NetworkCodes.AcceptHostRequest);

            NetworkManager
                .RegisterNetworkEvent<RejectHostRequestNetworkEvent,
                    RejectHostRequestNetworkEventSerializer>(NetworkCodes.RejectHostRequest);

            NetworkManager
                .RegisterNetworkEvent<PromoteToHostNetworkEvent,
                    PromoteToHostNetworkEventSerializer>(NetworkCodes.PromoteHost);

            NetworkManager.Subscribe<PromoteToHostNetworkEvent>(OnPromoteToHostNetworkEvent);

            // Local Asset Management
            ScenarioManager.AssetManager.AssetInstantiated += AssetManager_AssetInstantiated;
            ScenarioManager.AssetManager.AssetDestroyed += AssetManager_AssetDestroyed;
            ScenarioManager.AssetManager.ContentMarkerUpdated += AssetManager_ContentMarkerUpdated;

            ScenarioManager.NewScenarioPathway += ScenarioManager_NewScenarioPathway;
            ScenarioManager.ScenarioPlayModeSet += ScenarioManager_ScenarioPlayModeSet;

            // Networked Asset Management
            NetworkManager
                .RegisterNetworkEvent<InstantiateAssetNetworkEvent,
                        InstantiateAssetNetworkEventSerializer>
                    (NetworkCodes.SyncInstantiateInteractable);

            NetworkManager
                .RegisterNetworkEvent<ClientPropertyUpdatedNetworkEvent,
                        ClientPropertyUpdatedNetworkEventSerializer>
                    (NetworkCodes.ClientPropertyUpdated);

            NetworkManager
                .RegisterNetworkEvent<AssetPropertyUpdateNetworkEvent,
                    AssetPropertyUpdateNetworkEventSerializer>(NetworkCodes.AssetPropertyChanged);

            NetworkManager
                .RegisterNetworkEvent<RejectPropertyUpdateNetworkEvent,
                    RejectPropertyUpdateNetworkEventSerializer>(NetworkCodes.AssetPropertyRejected);

            NetworkManager
                .RegisterNetworkEvent<RequestPropertyUpdateNetworkEvent,
                        RequestPropertyUpdateNetworkEventSerializer>
                    (NetworkCodes.RequestPropertyUpdateNetworkEvent);

            NetworkManager
                .RegisterNetworkEvent<DestroyAssetNetworkEvent,
                    DestroyAssetNetworkEventSerializer>
                (NetworkCodes.SyncDestroyInteractable);

            NetworkManager.Subscribe<InstantiateAssetNetworkEvent>
                (OnInstantiateInteractableNetworkEvent);

            NetworkManager.Subscribe<DestroyAssetNetworkEvent>
                (OnDestroyAssetNetworkEvent);

            // App Related Events

            EventBus.Subscribe<KickUserEvent>(OnKickUserEvent);

            // Host Management

            EventBus.Subscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
            EventBus.Subscribe<ReturnHostToSessionOwnerEvent>(OnReturnHostToSessionOwnerEvent);
            EventBus.Subscribe<JoinSessionFromQrEvent>(OnJoinSessionFromQrEvent);
            EventBus.Subscribe<AutoJoinSessionEvent>(OnAutoJoinSessionEvent);
            EventBus.Subscribe<FinishingSyncWithHostLocalEvent>(OnFinishingSyncWithHostLocalEvent);
        }

        public SessionDetailedView ActiveSession { get; private set; }

        public byte UserCount => (byte)(NetworkManager.CurrentRoom?.PlayerCount ?? 0);

        protected Guid previousHost;
        protected Guid sessionIdToAutoJoin;

        protected Guid sessionIdClientIsJoining;

        protected UserCard localUserCard;
        protected UserAvatar localUserAvatar;

        // set to true for all users when promoting a new host.
        // needed to determine whether a new master client is a
        // reconnecting host or new host.
        protected bool isPromotingHost;

        /// <summary>
        /// Background thread used by the MasterClient (not host) to keep the session alive
        /// </summary>
        protected KeepSessionAliveHandler keepGmsSessionAliveHandler;

        /// <summary>
        /// A class to hold a set of capabilities this user may have (e.g. host vs client)
        /// </summary>
        protected SessionUser userCapabilities;

        // The scenario is loaded before the photon room is created.
        // IsHost should return true before HostId is non-empty from photon room creation.
        protected bool isHostingNewSession;

        public bool InWaitingRoomLobby { get; set; }

        public bool HostPresentInSession
        {
            get
            {
                return NetworkManager.AllPlayers.Any(p => p.UserId == HostId.ToString());
            }
        }

        protected bool ScenarioFirstStartFlag { get; set; }

        public Guid HostId
        {
            get
            {
                if (NetworkManager == null ||
                    NetworkManager.CurrentRoom == null ||
                    NetworkManager.CurrentRoom.CustomProperties == null)
                {
                    return Guid.Empty;
                }

                return NetworkManager.CurrentRoom.CustomProperties.GetHostId();
            }
        }

        public string PathwayInfo
        {
            get
            {
                if (NetworkManager == null ||
                    NetworkManager.CurrentRoom == null ||
                    NetworkManager.CurrentRoom.CustomProperties == null)
                {
                    return null;
                }

                return NetworkManager.CurrentRoom.CustomProperties.GetScenarioPathway();
            }
        }

        public ScenarioControlTypes PlayMode
        {
            get
            {
                if (NetworkManager == null ||
                    NetworkManager.CurrentRoom == null ||
                    NetworkManager.CurrentRoom.CustomProperties == null)
                {
                    return ScenarioControlTypes.Automated;
                }

                var playMode = NetworkManager.CurrentRoom.CustomProperties.GetPlayMode();

                if (playMode == -1)
                    return ScenarioControlTypes.Automated;
                else
                    return (ScenarioControlTypes)playMode;
            }
        }

        public bool IsHost
        {
            get
            {
                Guid hostId = HostId;

                return isHostingNewSession || (hostId != Guid.Empty && hostId == ApiClient.AccountsApi.AuthenticatedAccount.AccountId);
            }
        }

        public bool IsSessionCreator
        {
            get
            {
                return ActiveSession != null &&
                       ActiveSession.CreatedById != Guid.Empty &&
                       ActiveSession.CreatedById == ApiClient.AccountsApi.AuthenticatedAccount.AccountId;
            }
        }

        private CancellationTokenSource joiningSessionTokenSource;

        private async UniTask<SessionDetailedView> GetSessionDataAsync(Guid sessionId)
        {
            SessionDetailedView session;

            try
            {
                session = await ApiClient.SessionsApi.GetSessionAsync(sessionId);
            }
            catch (GmsApiException exception)
            {
                GIGXR.Platform.Utilities.Logger.Error($"Could not retrieve session data for {sessionId}", nameof(SessionManager), exception);

                session = null;
            }

            return session;
        }

        private async UniTask LoadScenarioFromSessionAsync(SessionDetailedView session, CancellationToken cancellationToken)
        {
            EventBus.Publish(new StartingAssetSpawningLocalEvent());

            try
            {
                // Load and start Scenario
                await ScenarioManager.LoadScenarioAsync(session.HmdJson, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                GIGXR.Platform.Utilities.Logger.Error($"Could not load the Scenario from Session data {session.SessionName}.", nameof(SessionManager));

                throw;
            }
        }

        public virtual async UniTask StartSessionAsync(Guid sessionId)
        {
            if (joiningSessionTokenSource != null)
            {
                return;
            }

            joiningSessionTokenSource = new CancellationTokenSource();

            try
            {
                // Always start from clean data
                await CleanUpSessionAsync(false);

                // Start visual display of joining a session
                EventBus.Publish(new StartingJoinSessionLocalEvent(JoinTypes.FromSaved));

                isHostingNewSession = true;

                // Load from GMS
                ActiveSession = await GetSessionDataAsync(sessionId);

                GIGXR.Platform.Utilities.Logger.Info($"ActiveSession is now {ActiveSession.SessionName} {ActiveSession.SessionId}", nameof(SessionManager));

                // Load the Scenario data
                await LoadScenarioFromSessionAsync(ActiveSession, joiningSessionTokenSource.Token);

                EventBus.Publish(new StartingRoomConnectLocalEvent());

                // Start Photon Room
                var joinOrCreateedRoomSuccess = await NetworkManager.JoinOrCreateRoomAsync
                    (
                        ActiveSession.SessionId.ToString(),
                        ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString()
                    );

                EventBus.Publish(new FinishingJoinSessionLocalEvent());

                if (joinOrCreateedRoomSuccess)
                {
                    // All sessions start in Stopped mode
                    await ScenarioManager.StopScenarioAsync();

                    // TODO Colocation will always be true for the one starting the session
                    await ApiClient.SessionsApi.UpdateParticipantStatusAsync
                            (
                                ActiveSession.SessionId,
                                ApiClient.AccountsApi.AuthenticatedAccount.AccountId,
                                SessionParticipantStatus.InSessionColocated
                            );

                    // Since we use our own GigAssetManager to instantiate our prefabs, take ownership of all those objects as the host
                    NetworkManager.OwnAllNetworkObjects(ScenarioManager.AssetManager.AllInstantiatedAssets);

                    // Mark started on GMS
                    var patchSession = new PatchSessionRequest()
                    {
                        SessionId = ActiveSession.SessionId,
                        SessionStatus = SessionStatus.InProgress
                    };

                    await ApiClient.SessionsApi.PatchSessionAsync(new List<PatchSessionRequest>() { patchSession });
                }
                else
                {
                    await CleanUpSessionAsync();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                GIGXR.Platform.Utilities.Logger.Error($"Could not start the session {sessionId}. Returning to the session list.", nameof(SessionManager));

                await CleanUpSessionAsync();

                throw;
            }
            finally
            {
                isHostingNewSession = false;
            }
        }

        public virtual async UniTask StartAdHocSessionAsync()
        {
            if (joiningSessionTokenSource != null)
            {
                return;
            }

            joiningSessionTokenSource = new CancellationTokenSource();

            GIGXR.Platform.Utilities.Logger.Info("Ad Hoc Session is now starting.", nameof(SessionManager));

            // Always start from clean data
            await CleanUpSessionAsync(false);

            AccountDetailedView accountDetails
                = await ApiClient.AccountsApi.GetAuthenticatedAccountProfileAsync();

            var createSession = new CreateSessionRequest()
            {
                SessionName = ProfileManager.networkProfile.GetDefaultRoomName(accountDetails.FirstName),
                InstitutionId = ApiClient.AccountsApi.AuthenticatedAccount.InstitutionId,
                ClientAppId = Guid.Parse(ProfileManager.authenticationProfile.ApplicationId()),
                SessionPermission = SessionPermission.OpenToInstitution,
                SessionStatus = SessionStatus.InProgress,
                LessonDate = DateTime.UtcNow.ToUniversalTime(),
                Saved = false
            };

            SessionDetailedView session;

            // Load and start Scenario
            try
            {
                isHostingNewSession = true;

                // Start visual display of joining a session
                EventBus.Publish(new StartingJoinSessionLocalEvent(JoinTypes.AdHoc));

                session = await ApiClient.SessionsApi.CreateSessionAsync(createSession);

                ActiveSession = session;

                GIGXR.Platform.Utilities.Logger.Info($"ActiveSession is now {ActiveSession.SessionName} {ActiveSession.SessionId}", nameof(SessionManager));

                await LoadScenarioFromSessionAsync(ActiveSession, joiningSessionTokenSource.Token);

                EventBus.Publish(new StartingRoomConnectLocalEvent());

                // Start Photon Room
                await NetworkManager.JoinOrCreateRoomAsync
                    (
                        ActiveSession.SessionId.ToString(),
                        ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString()
                    );

                await ApiClient.SessionsApi.UpdateParticipantStatusAsync
                            (
                                ActiveSession.SessionId,
                                ApiClient.AccountsApi.AuthenticatedAccount.AccountId,
                                SessionParticipantStatus.InSessionColocated
                            );

                EventBus.Publish(new FinishingJoinSessionLocalEvent());

                await ScenarioManager.StopScenarioAsync();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                GIGXR.Platform.Utilities.Logger.Error("Could not load the scenario. Returning to the session list.", nameof(SessionManager), e);

                // The CreateSessionAsync will create a new active session, make sure that it is made invalid so it can't be brought up
                if (ActiveSession != null)
                {
                    ActiveSession.SessionStatus = GIGXR.GMS.Models.Sessions.SessionStatus.Invalid;

                    await ApiClient.SessionsApi.UpdateSessionAsync
                        (ActiveSession.SessionId, new UpdateSessionRequest(ActiveSession));
                }

                await CleanUpSessionAsync();
            }
            finally
            {
                isHostingNewSession = false;
            }
        }

        public async UniTask StartSessionFromPlanAsync(Guid sessionPlanId)
        {
            if (joiningSessionTokenSource != null)
            {
                return;
            }

            joiningSessionTokenSource = new CancellationTokenSource();

            GIGXR.Platform.Utilities.Logger.Info($"Starting session from session plan {sessionPlanId}", nameof(SessionManager));

            // Always start from clean data
            await CleanUpSessionAsync(false);

            // Load Session data from GMS
            var sessionPlanData = await ApiClient.SessionsApi.GetSessionPlanAsync(sessionPlanId);

            if (sessionPlanData != null)
            {
                // Create new session data based on session data
                var createSession = new CreateSessionRequest()
                {
                    SessionName = sessionPlanData.SessionName,
                    ClientAppId = sessionPlanData.ClientAppId,
                    InstitutionId = sessionPlanData.InstitutionId,
                    ClassId = sessionPlanData.ClassId,
                    InstructorId = sessionPlanData.InstructorId,
                    Description = sessionPlanData.Description,
                    SessionPermission = sessionPlanData.SessionPermission,
                    SessionNoteVisible = sessionPlanData.SessionNoteVisible,
                    HmdJson = sessionPlanData.HmdJson,
                    SessionNote = sessionPlanData.SessionNote != null
                                            ? sessionPlanData.SessionNote
                                            : "",
                    SessionStatus = SessionStatus.InProgress,
                    LessonDate = DateTime.UtcNow.ToUniversalTime(),
                    Saved = false
                };

                try
                {
                    // Start visual display of joining a session
                    EventBus.Publish(new StartingJoinSessionLocalEvent(JoinTypes.FromSessionPlan));

                    isHostingNewSession = true;

                    ActiveSession = await ApiClient.SessionsApi.CreateSessionAsync(createSession, sessionPlanData.ClientAppVersion);

                    GIGXR.Platform.Utilities.Logger.Info($"ActiveSession is now {ActiveSession}", nameof(SessionManager));

                    joiningSessionTokenSource.Token.ThrowIfCancellationRequested();

                    // Start Scenario
                    await LoadScenarioFromSessionAsync(ActiveSession, joiningSessionTokenSource.Token);

                    EventBus.Publish(new StartingRoomConnectLocalEvent());

                    // Start Photon Room
                    var joinSuccess = await NetworkManager.JoinOrCreateRoomAsync
                        (
                            ActiveSession.SessionId.ToString(),
                            ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString()
                        );

                    EventBus.Publish(new FinishingJoinSessionLocalEvent());

                    if (joinSuccess)
                    {
                        GIGXR.Platform.Utilities.Logger.Info($"ActiveSession is now {ActiveSession.SessionName} {ActiveSession.SessionId}", nameof(SessionManager));

                        await ApiClient.SessionsApi.UpdateParticipantStatusAsync
                            (
                                ActiveSession.SessionId,
                                ApiClient.AccountsApi.AuthenticatedAccount.AccountId,
                                SessionParticipantStatus.InSessionColocated
                            );

                        await ScenarioManager.StopScenarioAsync();

                        // Mark started on GMS

                        // Since we use our own GigAssetManager to instantiate our prefabs, take ownership of all those objects as the host
                        NetworkManager.OwnAllNetworkObjects(ScenarioManager.AssetManager.AllInstantiatedAssets);
                    }
                    else
                    {
                        GIGXR.Platform.Utilities.Logger.Error("Failed while creating Photon Room.", nameof(SessionManager));

                        await CleanUpSessionAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    GIGXR.Platform.Utilities.Logger.Warning($"Could not start Session {sessionPlanId} as a Session Plan. See error below.", nameof(SessionManager));

                    await CleanUpSessionAsync();

                    throw;
                }
                finally
                {
                    isHostingNewSession = false;
                }
            }
            else
            {
                GIGXR.Platform.Utilities.Logger.Error($"Could not retrieve Session Plan with ID {sessionPlanId}", nameof(SessionManager));

                await CleanUpSessionAsync();
            }
        }

        public virtual async UniTask<(bool, string)> JoinSessionAsync(Guid sessionId, bool isDemoSession = false)
        {
            if (joiningSessionTokenSource != null)
            {
                return (false, "Already attempting to join a session");
            }

            joiningSessionTokenSource = new CancellationTokenSource();

            GIGXR.Platform.Utilities.Logger.Info($"Joining session {sessionId}", nameof(SessionManager));

            try
            {
                // Always start from clean data
                await CleanUpSessionAsync(false);

                // When joining a session, if you've create the session in GMS, then the Photon room won't exist yet and you wouldn't be the host if you just joined, so check if you created the session first to establish this
                ActiveSession = await GetSessionDataAsync(sessionId);

                bool joinedNetworkedRoom = false;

                if (isDemoSession)
                {
                    isHostingNewSession = true;

                    // Start visual display of joining a session
                    EventBus.Publish(new StartingJoinSessionLocalEvent(JoinTypes.FromSessionPlan));

                    await LoadScenarioFromSessionAsync(ActiveSession, joiningSessionTokenSource.Token);

                    EventBus.Publish(new FinishingJoinSessionLocalEvent());

                    await ScenarioManager.PlayScenarioAsync();

                    isHostingNewSession = false;

                    return (true, "");
                }
                else if (ActiveSession == null)
                {
                    await CleanUpSessionAsync();

                    return (false, "NoSessionData");
                }
                // Rejoining a session that you created.
                else if (ActiveSession?.CreatedById == ApiClient.AccountsApi.AuthenticatedAccount.AccountId)
                {
                    isHostingNewSession = true;

                    // Start visual display of joining a session
                    EventBus.Publish(new StartingJoinSessionLocalEvent(JoinTypes.Joining));

                    await LoadScenarioFromSessionAsync(ActiveSession, joiningSessionTokenSource.Token);

                    EventBus.Publish(new StartingRoomConnectLocalEvent());

                    // Start Photon Room
                    joinedNetworkedRoom = await NetworkManager.JoinOrCreateRoomAsync
                        (
                            ActiveSession.SessionId.ToString(),
                            ApiClient.AccountsApi.AuthenticatedAccount.AccountId.ToString()
                        );

                    isHostingNewSession = false;

                    if (joinedNetworkedRoom)
                    {
                        GIGXR.Platform.Utilities.Logger.Info($"ActiveSession is now {ActiveSession.SessionName} {ActiveSession.SessionId}", nameof(SessionManager));

                        // Since we use our own GigAssetManager to instantiate our prefabs, take ownership of all those objects as the host
                        NetworkManager.OwnAllNetworkObjects(ScenarioManager.AssetManager.AllInstantiatedAssets);

                        var patchSession = new PatchSessionRequest()
                        {
                            SessionId = ActiveSession.SessionId,
                            SessionStatus = SessionStatus.InProgress
                        };

                        // host successfully started session, so set status to InProgress
                        await ApiClient.SessionsApi.PatchSessionAsync(new List<PatchSessionRequest>() { patchSession });

                        EventBus.Publish(new FinishingJoinSessionLocalEvent());
                        // Because this session is being joined, the client will get an UpdateFromGMSNetworkEvent, this will trigger sendNetworkEvents to true because at that time, it will have gotten
                        // the latest GMS session data

                        return (true, "");
                    }
                    else
                    {
                        return (false, "Network");
                    }
                }
                // Joining somebody else's session.
                else
                {
                    // Stop early if the session is already known to be locked
                    if (ActiveSession != null && ActiveSession.Locked)
                    {
                        await CleanUpSessionAsync();

                        // Return false for failure with a hint that it's because the session is locked
                        return (false, nameof(SessionDetailedView.Locked));
                    }

                    // Clients will defer joining the Photon room in the lobby, this allows for GMS to create the session before the Photon room exists
                    sessionIdClientIsJoining = sessionId;

                    // Join Photon lobby since you are not the creator, in the lobby you will wait until the Photon room exists and then join it
                    await NetworkManager.JoinLobbyAsync();

                    return (true, "");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                GIGXR.Platform.Utilities.Logger.Error($"Failed while joining session {sessionId}", nameof(SessionManager));

                await CleanUpSessionAsync();

                throw;
            }

            return (false, "Unknown");
        }

        public virtual async UniTask StopSessionAsync()
        {
            GIGXR.Platform.Utilities.Logger.Info($"Stopping session", nameof(SessionManager));

            // Mark participants as attended
            foreach (var player in NetworkManager.CurrentRoom?.Players.Values)
            {
                // We don't need to await this as we don't need to wait for GMS to approve of every user's status
                // before closing the room
                _ = UniTask.Create(() =>
                {
                    _ = ApiClient.SessionsApi.UpdateParticipantStatusAsync
                        (
                            ActiveSession.SessionId,
                            Guid.Parse(player.UserId),
                            SessionParticipantStatus.Attended
                        );

                    return UniTask.CompletedTask;
                });
            }

            // Send a networked event to all users so they can gracefully leave the room
            NetworkManager.RaiseNetworkEvent(new HostClosedSessionNetworkEvent());

            var patchSession = new PatchSessionRequest()
            {
                SessionId = ActiveSession.SessionId,
                SessionStatus = SessionStatus.Ended
            };

            // Update the GMS session status to ended
            await ApiClient.SessionsApi.PatchSessionAsync(new List<PatchSessionRequest>() { patchSession });

            // Wait until all users leave the room so the send network event has time to get to the clients before the room is closed
            await UniTask.WaitUntil(() => NetworkManager.AllPlayers.Length == 1);

            // Close Photon room
            await NetworkManager.CloseRoomAsync();

            // Alert app that the session is closed
            EventBus.Publish(new HostClosedSessionEvent());
        }

        public virtual async UniTask LeaveSessionAsync()
        {
            GIGXR.Platform.Utilities.Logger.Info($"Leaving session", nameof(SessionManager));

            // If the user leaves while the host is gone, make sure that all the lobby info is cleaned up
            if (InWaitingRoomLobby)
            {
                LeaveWaitingRoomLobby();
            }

            // Mark participant as attended
            if (ActiveSession != null)
            {
                await ApiClient.SessionsApi.UpdateParticipantStatusAsync
                    (
                        ActiveSession.SessionId,
                        ApiClient.AccountsApi.AuthenticatedAccount.AccountId,
                        SessionParticipantStatus.Attended
                    );
            }

            var leftRoom = await NetworkManager.LeaveRoomAsync();

            // If the user cancels while waiting for the host, they may not leave the room and clean up the session data
            // so make sure that is done here
            if (!leftRoom)
                await CleanUpSessionAsync();
        }

        public virtual async UniTask CancelJoiningSession(JoinTypes joinMethod)
        {
            joiningSessionTokenSource?.Cancel();
            joiningSessionTokenSource?.Dispose();
            joiningSessionTokenSource = null;

            // If the user created this session and cancels, remove it from GMS
            if (joinMethod != JoinTypes.Joining)
            {
                if (ActiveSession != null)
                {
                    ActiveSession.SessionStatus = SessionStatus.Ended;

                    await ApiClient.SessionsApi.UpdateSessionAsync(
                        ActiveSession.SessionId,
                        new UpdateSessionRequest(ActiveSession));
                }
            }

            EventBus.Publish(new CancelJoinSessionLocalEvent());

            await CleanUpSessionAsync();
        }

        public virtual async UniTask KickUserAsync(Guid accountId)
        {
            GIGXR.Platform.Utilities.Logger.Info($"Kicking user {accountId}", nameof(SessionManager));

            // Kick a user from the Photon room
            NetworkManager.RaiseNetworkEvent(new UserKickedNetworkEvent(accountId));

            // Mark them as attended
            await ApiClient.SessionsApi.UpdateParticipantStatusAsync
                (
                    ActiveSession.SessionId,
                    accountId,
                    SessionParticipantStatus.Attended
                );
        }

        public async UniTask SaveSessionAsync(bool markSessionAsSaved = false)
        {
            if (markSessionAsSaved)
            {
                ActiveSession.Saved = true;

                GIGXR.Platform.Utilities.Logger.Info($"Saving session to GMS", nameof(SessionManager));
            }

            SessionStartedSave?.Invoke(this, new SessionSavedArgs(markSessionAsSaved));

            // Save the current scenario so that the asset data for each asset type is updated
            // Do not await saving the scenario as it will block the main UI
            _ = ScenarioManager.SaveScenarioAsync();

            try
            {
                await UniTask.Create
                    (
                        async () =>
                        {
                            // Since we can't await on the SaveScenarioAsync, use the bool value to delay this task until complete
                            await UniTask.WaitUntil(() => !ScenarioManager.IsSavingScenario);

                            ActiveSession.HmdJson = JObject.FromObject(ScenarioManager.LastSavedScenario);

                            // Save the current session to the GMS
                            await ApiClient.SessionsApi.UpdateSessionAsync(ActiveSession.SessionId, new UpdateSessionRequest(ActiveSession));

                            SessionFinishedSave?.Invoke(this, new SessionSavedArgs(markSessionAsSaved));
                        }
                    );
            }
            catch (Exception e)
            {
                GIGXR.Platform.Utilities.Logger.Error($"Error while trying to update the GMS JSON for Session {ActiveSession.SessionName}.",
                                                      nameof(SessionManager),
                                                      e);
            }
        }

        public async UniTask SaveSessionCopyAsync()
        {
            GIGXR.Platform.Utilities.Logger.Info($"Saving session as a copy", nameof(SessionManager));

            SessionStartedSave?.Invoke(this, new SessionSavedArgs(true));

            var sessionCopyRequest = new CreateSessionRequest()
            {
                SessionName = ActiveSession.SessionName + " Copy",
                ClientAppId = ActiveSession.ClientAppId,
                InstitutionId = ActiveSession.InstitutionId,
                ClassId = ActiveSession.ClassId,
                InstructorId = ActiveSession.InstructorId,
                Description = ActiveSession.Description,
                SessionPermission = ActiveSession.SessionPermission,
                SessionStatus = GIGXR.GMS.Models.Sessions.SessionStatus.Ended,
                SessionNoteVisible = ActiveSession.SessionNoteVisible,
                SessionNote = ActiveSession.SessionNote,
                HmdJson = ActiveSession.HmdJson,
                Locked = false,
                Saved = true,
                LessonDate = ActiveSession.LessonDate
            };

            SessionDetailedView sessionCopyData;

            try
            {
                sessionCopyData = await ApiClient.SessionsApi.CreateSessionAsync(sessionCopyRequest, ActiveSession.ClientAppVersion);
            }
            catch (GmsApiException exception)
            {
                GIGXR.Platform.Utilities.Logger.Error("Could not save session as a copy", nameof(SessionManager), exception);

                return;
            }

            // Save current session as a copy to the GMS
            await ApiClient.SessionsApi.UpdateSessionAsync(sessionCopyData.SessionId, new UpdateSessionRequest(sessionCopyData));

            SessionFinishedSave?.Invoke(this, new SessionSavedArgs(true));
        }

        public async UniTask RenameSessionAsync(string name)
        {
            GIGXR.Platform.Utilities.Logger.Info($"Renaming session to {name}", nameof(SessionManager));

            // Rename
            ActiveSession.SessionName = name;

            // Update GMS
            // TODO Change to update, there is a Catch-22 here, if we save the session, the name change will propagate properly to GMS, however
            // only the original session owner can rename the stage. If we update the ephemeral data, the stage does not propagate to the session list
            // names so users will still see the old name in the session list as if it hasn't been renamed in session. Leave this as is, but this means that
            // after host transfer is complete, the new host will not be able to rename sessions
            await SaveSessionAsync();
            //await UpdateGMSEphemeralDataAsync();

            // Update session name locally
            EventBus.Publish(new SessionRenamedEvent(name));

            // Update session name across network
            NetworkManager.RaiseNetworkEvent(new SessionRenamedNetworkEvent(name));
        }

        protected virtual async UniTask CleanUpSessionAsync(bool leavingCleanUp = true)
        {
            GIGXR.Platform.Utilities.Logger.Info($"Cleaning up session data. {(leavingCleanUp ? "Leaving session" : "Joining Session")}",
                nameof(SessionManager),
                true);

            ScenarioFirstStartFlag = false;

            ScenarioManager.StageManager.RemoveAllStages();

            ActiveSession = null;

            sessionIdClientIsJoining = Guid.Empty;
            sessionIdToAutoJoin = Guid.Empty;

            await ScenarioManager.UnloadScenarioAsync();

            userCapabilities.RemoveAll();

            isHostingNewSession = false;
            isPromotingHost = false;

            localUserCard = null;
            localUserAvatar = null;

            keepGmsSessionAliveHandler?.Disable();

            // The handler takes in the SessionId as input, so get rid of the handler based
            // on this current session. A new one will be created when a new session is joined.
            keepGmsSessionAliveHandler = null;

            // Do not send these events if the session is being cleaned before joining
            if (leavingCleanUp)
            {
                // When leaving the session, there should be no reasons left for the content marker
                // and such to be hidden
                ScenarioManager.AssetManager.ShowAll(0);

                EventBus.Publish(new SetContentMarkerEvent(Vector3.zero, Quaternion.identity));

                EventBus.Publish(new ReturnToSessionListEvent());

                joiningSessionTokenSource?.Dispose();
                joiningSessionTokenSource = null;
            }

            NetworkManager.LeaveLobbyAsync().Forget();
        }

        public void AddSessionCapability(ISessionCapability capability)
        {
            userCapabilities.AddSessionCapability(capability);
        }

        public void RemoveSessionCapability(Type capabilityType)
        {
            userCapabilities.RemoveSessionCapability(capabilityType);
        }

        public async void SyncContentMarker()
        {
            await UniTask.WaitUntil(() => NetworkManager.InRoom);

            NetworkManager.RaiseNetworkEvent(
                new RequestContentMarkerNetworkEvent(
                    NetworkManager.GetAlternativePlayerReference(
                        ApiClient.AccountsApi.AuthenticatedAccount.AccountId)));
        }

        #region Scenario Local EventHandlers

        private async void ScenarioManager_ScenarioPlayingAsync(object sender, ScenarioPlayingEventArgs e)
        {
            if (!ScenarioFirstStartFlag)
            {
                ScenarioFirstStartFlag = true;

                // Send the Awake message to all GIG Assets so they know the scenario has started for the first time
                await ScenarioManager.AssetManager.AllInstantiatedAssetsAsync.ForEachAsync(instantiatedAsset =>
                {
                    var asset = instantiatedAsset.Item2.GameObject.GetComponent<IAssetMediator>();

                    asset.OnAssetAwake();
                });
            }
        }

        #endregion

        #region HostManagement

        private void OnHostTransferCompleteEvent(HostTransferCompleteEvent @event)
        {
            GIGXR.Platform.Utilities.Logger.Info($"Host has been transfered to {@event.NewHostId}", nameof(SessionManager));

            if (IsHost)
            {
                // The host can't be in the lobby, so leave it if they are in it, which can occur if host transfer happens
                // when the scenario is stopped
                LeaveWaitingRoomLobby();

                RemoveSessionCapability(typeof(ClientCapabilities));
                AddSessionCapability(new HostCapabilities(this, ScenarioManager, NetworkManager, ApiClient, EventBus));
            }
            else
            {
                RemoveSessionCapability(typeof(HostCapabilities));
                AddSessionCapability(new ClientCapabilities(this, ScenarioManager, NetworkManager, CalibrationManager, ApiClient, EventBus));

                // TODO Not the best way to bring this prompt back up, but if the Scenario isn't being played, then make sure to display
                // the UI to this user as they are now a client
                if (ScenarioManager.ScenarioStatus == ScenarioStatus.Paused ||
                    ScenarioManager.ScenarioStatus == ScenarioStatus.Stopped)
                {
                    EnterWaitingRoomLobby(true);
                }
                else
                {
                    LeaveWaitingRoomLobby();
                }
            }
        }

        // Everyone
        private void OnPromoteToHostNetworkEvent(PromoteToHostNetworkEvent @event)
        {
            previousHost = HostId;
            isPromotingHost = true;

            // MasterClient must be the one to specifically set the new user to be MasterClient
            if (NetworkManager.IsMasterClient)
            {
                NetworkManager.SetMasterClient(NetworkManager.GetPlayerById(@event.NewHostID));
            }
        }

        private async void OnReturnHostToSessionOwnerEvent(ReturnHostToSessionOwnerEvent @event)
        {
            NetworkManager.RaiseNetworkEvent
                (new PromoteToHostNetworkEvent(ActiveSession.CreatedBy.AccountId));

            // Edge case, if the original session owner is made the MasterClient because the previous host that they made disconnected,
            // they will not get the Host status back, unless they request it, but when they request it, they are already the MC, so no
            // event is generated, so check here to make sure the host is able to get back the session and out of the lobby
            if (ApiClient.AccountsApi.AuthenticatedAccount.AccountId ==
                ActiveSession.CreatedBy.AccountId &&
                InWaitingRoomLobby)
            {
                // Locally leave the lobby
                LeaveWaitingRoomLobby();

                // Make sure your own UI is updated to match the host privileges 
                EventBus.Publish
                    (
                        new HostTransferCompleteEvent
                            (
                                ApiClient.AccountsApi.AuthenticatedAccount.AccountId,
                                @event.PreviousHost
                            )
                    );

                await UpdateGMSEphemeralDataAsync();

                // Tell clients to update from GMS in case any users joined while there was no active host
                NetworkManager.RaiseNetworkEvent
                    (
                        new UpdateFromGMSNetworkEvent
                            (
                                ActiveSession.SessionId,
                                NetworkManager.GetAlternativePlayerReference(ActiveSession.CreatedBy.AccountId),
                                HostId,
                                ScenarioManager.StageManager.CurrentStage.StageId,
                                new ScenarioSyncData
                                (
                                    NetworkManager.ServerTime,
                                    ScenarioManager.ScenarioStatus,
                                    (int)ScenarioManager.ActiveScenarioTimer.TimeInSimulation.TotalMilliseconds,
                                    (int)ScenarioManager.ActiveScenarioTimer.TimeInPlayingScenario.TotalMilliseconds,
                                    0
                                )
                            )
                    );

                ScenarioManager.AssetManager.SyncAllRuntimeRoomData();

                // Tell all connected clients to leave the lobby as well
                NetworkManager.RaiseNetworkEvent(new LeaveWaitingRoomLobbyNetworkEvent());
            }
        }

        protected void OnJoinSessionFromQrEvent(JoinSessionFromQrEvent @event)
        {
            // The user should not join the session right away, but calibrate first, wait for the SessionScreen to trigger the join
            sessionIdToAutoJoin = @event.SessionId;
        }

        protected virtual void OnAutoJoinSessionEvent(AutoJoinSessionEvent @event)
        {
            if (sessionIdToAutoJoin != Guid.Empty)
            {
                // Tell the app through the EventBus that a new session should be join and provide the ID so it can handle it
                EventBus.Publish(new AttemptStartSessionEvent(sessionIdToAutoJoin));

                sessionIdToAutoJoin = Guid.Empty;
            }
        }

        #endregion

        #region AppBus EventHandlers

        protected async void OnKickUserEvent(KickUserEvent @event)
        {
            await KickUserAsync(@event.UserToKick);
        }

        #endregion

        #region Room Network EventHandler

        private void OnJoinedRoomNetworkEvent(JoinedRoomNetworkEvent @event)
        {
            // Generate your own user card
            localUserCard = UserCard.GenerateUserCard(@event.NickName, @event.UserId);

            if (FeatureManager.IsEnabled(FeatureFlags.Avatars))
            {
                localUserAvatar = UserRepresentations.Generate(@event.NickName);
            }

            // Determine if you are the host or not
            if (IsHost)
            {
                RemoveSessionCapability(typeof(ClientCapabilities));
                AddSessionCapability(new HostCapabilities(this, ScenarioManager, NetworkManager, ApiClient, EventBus));
            }
            else
            {
                RemoveSessionCapability(typeof(HostCapabilities));
                AddSessionCapability(new ClientCapabilities(this, ScenarioManager, NetworkManager, CalibrationManager, ApiClient, EventBus));
            }

            if (IsSessionCreator)
            {
                AddSessionCapability(new CreatorCapabilities());
            }

            TryCreateKeepSessionAliveHandler();

            // Send out a generic session status update
            EventBus.Publish(new JoinedSessionEvent(@event.NickName, Guid.Parse(@event.UserId)));
        }

        private async void OnJoinRoomFailedNetworkEvent(JoinRoomFailedNetworkEvent @event)
        {
            await CleanUpSessionAsync();
        }

        private async void OnLeftRoomNetworkEvent(LeftRoomNetworkEvent @event)
        {
            // When Photon.CloseConnection is called on a user, no event or disconnect is raised, only the OnLeftRoom event will so clean up everything here
            await CleanUpSessionAsync();

            // The static UserRepresentation does not have dependencies, so just use SessionManager to clean this
            UserRepresentations.CleanUp();

            // Send out a generic session status update
            EventBus.Publish(new LeftSessionEvent());
        }

        private async void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            if (NetworkManager.InRoom)
            {
                var userId = @event.Player.UserId;
                var nickName = @event.Player.NickName;

                if (NetworkManager.IsMasterClient)
                {
                    // if another player has left, update their participant status to attended. This is in case the user has crashed out or quit the app instead of leaving the session and marking themselves attended.
                    if (ActiveSession != null)
                    {
                        await ApiClient.SessionsApi.UpdateParticipantStatusAsync
                            (
                                ActiveSession.SessionId,
                                Guid.Parse(userId),
                                SessionParticipantStatus.Attended
                            );
                    }
                }
            }
        }

        private void OnPlayerEnteredRoomNetworkEvent(PlayerEnteredRoomNetworkEvent @event)
        {
            // Because of host transfer and host dropped connections, any user may need to do this process
            // When you are in a lobby and a player joins, check to see if they are the owner of the Photon room
            if (InWaitingRoomLobby)
            {
                // Give the host the Master Client privilege back
                if (Guid.Parse(@event.Player.UserId) == HostId)
                {
                    NetworkManager.SetMasterClient(@event.Player);
                }
                // Tell the new user to enter the lobby
                else
                {
                    NetworkManager.RaiseNetworkEvent(new GoToWaitingRoomLobbyNetworkEvent(@event.Player.ActorNumber, true));
                }
            }
        }

        private void OnRoomPropertiesUpdateNetworkEvent(RoomPropertiesUpdateNetworkEvent @event)
        {
            foreach (var propertyChange in @event.PropertiesThatChanged)
            {
                // Photon will write values to this using bytes, ignore them as we are only looking at our own values
                if (propertyChange.Key.GetType() == typeof(byte))
                    continue;

                if ((string)propertyChange.Key == GigRoomHashtableExtensions.SessionPathwayPropertyKey)
                {
                    ScenarioManager.SetPathway(PathwayData.Create((string)propertyChange.Value), false);
                }
            }
        }

        // Host
        protected virtual async UniTask UpdateGMSEphemeralDataAsync()
        {
            try
            {
                // Since both these tasks can take a long while (relative to a frame), run them asynchronously in a thread since they do not require the main thread
                await UniTask.RunOnThreadPool(
                        async () =>
                        {
                            // Export Scenario data without saving
                            var scenario = await ScenarioManager.ExportScenarioAsync(true);
                            var createRequest = new CreateEphemeralDataRequest(JObject.FromObject(scenario));

                            await UniTask.SwitchToMainThread();

                            await ApiClient.SessionsApi.CreateEphemeralDataAsync(NetworkManager.CurrentRoom.CustomProperties.GetEphemeralDataId(),
                                                                                 createRequest);
                        });
            }
            catch (Exception e)
            {
                GIGXR.Platform.Utilities.Logger.Error($"Error while trying to update the GMS JSON for Session {ActiveSession.SessionName}",
                                                      nameof(SessionManager),
                                                      e);
            }
        }

        private void OnDisconnectedNetworkEvent(DisconnectedNetworkEvent @event)
        {
            //// Send out a generic session status update
            EventBus.Publish(new LeftSessionEvent());
        }

        #endregion

        #region Asset Local EventHandlers

        private void AssetManager_AssetInstantiated(object sender, AssetInstantiatedEventArgs e)
        {
            if (NetworkManager.InRoom)
            {
                // the asset was instantiated ad-hoc, after the scenario started
                if (e.IsRuntimeInstantiation)
                {
                    // the local player is the origin of the ad-hoc instantiation. This means:
                    // 1. They will arbitrarily own it initially.
                    // 2. They need to network the instantiation.
                    if (e.RuntimeInstantiationOriginateLocally)
                    {
                        NetworkManager.OwnNetworkObject(e.AssetId, e.AssetPhotonView);
                        NetworkManager.RaiseNetworkEvent
                        (
                            new InstantiateAssetNetworkEvent
                                (
                                    e.AssetTypeId,
                                    e.AssetId,
                                    e.PresetAssetId,
                                    e.Position,
                                    e.Rotation,
                                    e.IsRuntimeInstantiation,
                                    e.RuntimeOnly,
                                    e.AssetData
                                )
                        );
                    }

                    // this instantiation is happening on the receiving end of another player's
                    // instantiation call. get the right photon view id.
                    else
                    {
                        NetworkManager.MapNetworkObject(e.AssetId, e.AssetPhotonView, NetworkManager.GetPlayerById(HostId));
                    }
                }
                // the asset was instantiated as part of the scenario loading process.
                // this means:
                // 1. It originated on the master client.
                // 2. The {asset id : photon view id} map will be built in bulk, asynchronously.
                else
                {
                    // if this is the master client, network the instantiation
                    // (is this necessary? can clients be present while the scenario loads?)
                    if (NetworkManager.IsMasterClient)
                    {
                        NetworkManager.RaiseNetworkEvent
                        (
                            new InstantiateAssetNetworkEvent
                                (
                                    e.AssetTypeId,
                                    e.AssetId,
                                    e.PresetAssetId,
                                    e.Position,
                                    e.Rotation,
                                    e.IsRuntimeInstantiation,
                                    e.RuntimeOnly,
                                    e.AssetData
                                )
                        );
                    }
                }
            }
        }

        private void AssetManager_AssetDestroyed(object sender, AssetDestroyedEventArgs e)
        {
            // Network destructions, except those which result from reloading.
            // (reload-originated destructions are downstream of events that everyone receives)
            if (NetworkManager.InRoom && !e.FromReload)
            {
                NetworkManager.RaiseNetworkEvent
                (
                    new DestroyAssetNetworkEvent(e.AssetId)
                );
            }
        }

        // Since not all classes can know about the AssetManager (due to the assembly in GIGXR.Platform.Scenarios), use
        // this to forward the local event to the EventBus
        private void AssetManager_ContentMarkerUpdated(object sender, ContentMarkerUpdateEventArgs e)
        {
            EventBus.Publish(new SetContentMarkerEvent(e.contentMarkerPosition, e.contentMarkerRotation, e.assetContentMarker));
        }

        private void ScenarioManager_NewScenarioPathway(object sender, ScenarioPathwaySetEventArgs e)
        {
            if (IsHost && e.NewValue)
            {
                if (e.givenPathway != null)
                    NetworkManager.AddPathwayToRoom(e.givenPathway.ToJsonString());
                else
                    NetworkManager.AddPathwayToRoom(PathwayData.DefaultPathway().ToJsonString());
            }
        }

        private void ScenarioManager_ScenarioPlayModeSet(object sender, ScenarioPlayModeSetEventArgs e)
        {
            if (IsHost && e.saveValue)
            {
                NetworkManager.AddPlayModeToRoom(e.playMode);
            }
        }

        #endregion

        #region Asset Network EventHandlers

        private void OnInstantiateInteractableNetworkEvent(InstantiateAssetNetworkEvent e)
        {
            ScenarioManager.AssetManager.InstantiateInteractablePositionedAndOriented
                (
                    new AssetInstantiationArgs(e.AssetTypeId, e.AssetId, e.PresetAssetTypeId, e.IsRuntimeInstantiation, false, e.RuntimeOnly, e.AssetData),
                    e.Position,
                    e.Rotation
                );
        }

        private void OnDestroyAssetNetworkEvent(DestroyAssetNetworkEvent e)
        {
            ScenarioManager.AssetManager.Destroy(e.AssetId);
        }

        #endregion

        #region Session Network EventHandlers

        private async void OnUserKickedNetworkEvent(UserKickedNetworkEvent e)
        {
            if (ApiClient.AccountsApi.AuthenticatedAccount.AccountId != e.UserID)
                return;

            // remove the user from the session, flag as a kick.
            // TODO flag as kick
            await LeaveSessionAsync();
        }

        private void OnSessionRenamedNetworkEvent(SessionRenamedNetworkEvent @event)
        {
            ActiveSession.SessionName = @event.NewName;

            EventBus.Publish(new SessionRenamedEvent(@event.NewName));
        }

        protected virtual void TryCreateKeepSessionAliveHandler()
        {
            // Only the MasterClient (not host to resist host failure), should keep the session pinged
            if (NetworkManager.IsMasterClient)
            {
                if (keepGmsSessionAliveHandler == null)
                {
                    keepGmsSessionAliveHandler = new KeepSessionAliveHandler(ApiClient.SessionsApi, ActiveSession.SessionId);
                    keepGmsSessionAliveHandler.Enable();
                }
            }
            else
            {
                if (keepGmsSessionAliveHandler != null)
                {
                    keepGmsSessionAliveHandler.Disable();
                    keepGmsSessionAliveHandler = null;
                }
            }
        }

        private async void OnMasterClientSwitchedNetworkEvent(MasterClientSwitchedEvent @event)
        {
            if (NetworkManager.InRoom)
            {
                TryCreateKeepSessionAliveHandler();

                // If the new master client is also the host, then either:
                //  1. The new master client is a reconnecting host.
                //  2. The new master client is a new host, resulting from a host transfer.
                if (Guid.Parse(@event.NewMasterClient.UserId) == HostId)
                {
                    // Whenever there is a new host, map the objects
                    if (isPromotingHost)
                    {
                        NetworkManager.MapNetworkObjects(ScenarioManager.AssetManager.AllInstantiatedAssets, NetworkManager.GetPlayerById(HostId));

                        EventBus.Publish
                            (
                                new HostTransferCompleteEvent(HostId, previousHost)
                            );
                    }
                    else
                    {
                        // Since the MasterClient is the Session host, return to normalcy
                        if (IsHost)
                        {
                            NetworkManager.OwnAllNetworkObjects(ScenarioManager.AssetManager.AllInstantiatedAssets);

                            await UpdateGMSEphemeralDataAsync();

                            // Tell all users to grab the latest data from GMS, this is important if there are any new users
                            NetworkManager.RaiseNetworkEvent
                                (
                                    new UpdateFromGMSNetworkEvent
                                    (
                                        ActiveSession.SessionId,
                                        HostId,
                                        ScenarioManager.StageManager.CurrentStage.StageId,
                                        new ScenarioSyncData
                                        (
                                            NetworkManager.ServerTime,
                                            ScenarioManager.ScenarioStatus,
                                            (int)ScenarioManager.ActiveScenarioTimer.TimeInSimulation.TotalMilliseconds,
                                            (int)ScenarioManager.ActiveScenarioTimer.TimeInPlayingScenario.TotalMilliseconds,
                                            0
                                        )
                                    )
                                );

                            ScenarioManager.AssetManager.SyncAllRuntimeRoomData();

                            LeaveWaitingRoomLobby();
                        }
                    }
                }
                // The new master client is not the host. This can only happen if the host disconnects.
                else
                {
                    // If the host is present, then the host/master client mismatch is not possible
                    Debug.Assert(!IsHost, "Master Client / Host mismatch, but Host is in session!");

                    EnterWaitingRoomLobby(true);
                }
            }

            isPromotingHost = false;
        }

        private void OnGoToWaitingRoomLobbyNetworkEvent(GoToWaitingRoomLobbyNetworkEvent @event)
        {
            EnterWaitingRoomLobby(@event.FromHostDisconnect);
        }

        private void OnLeaveWaitingRoomLobbyNetworkEvent(LeaveWaitingRoomLobbyNetworkEvent @event)
        {
            LeaveWaitingRoomLobby();
        }

        private async void OnRoomListUpdateEvent(RoomListUpdateEvent @event)
        {
            // If we are waiting for a session and that session exists in the list, then attempt to join it 
            if (sessionIdClientIsJoining != Guid.Empty &&
                @event.RoomList.Any(room => room.Name == sessionIdClientIsJoining.ToString()))
            {
                try
                {
                    // User does not need any updates about rooms anymore, it doesn't matter when this completes
                    // so don't await
                    NetworkManager.LeaveLobbyAsync().Forget();

                    // Start visual display of joining a session
                    EventBus.Publish(new StartingJoinSessionLocalEvent(JoinTypes.Joining));

                    // The room exists, you can now start loading the scenario
                    // Joining clients will load the Scenario so that objects can be interacted with at the network level asap, but data will not be in sync
                    // until after the first `OnUpdateFromGMSNetworkEvent`
                    await LoadScenarioFromSessionAsync(ActiveSession, joiningSessionTokenSource.Token);

                    // Do not display assets during this time as they will not be accurate to where they should be and if the
                    // session is in Edit mode, then they will disappear anyways
                    ScenarioManager.AssetManager.HideAll(HideAssetReasons.Syncing);

                    EventBus.Publish(new StartingRoomConnectLocalEvent());

                    // Join Photon room since you are not the creator
                    bool joinedNetworkedRoom = await NetworkManager.JoinRoomAsync(sessionIdClientIsJoining.ToString());

                    sessionIdClientIsJoining = Guid.Empty;

                    if (joinedNetworkedRoom)
                    {
                        GIGXR.Platform.Utilities.Logger.Info($"ActiveSession is now {ActiveSession.SessionName} {ActiveSession.SessionId}", nameof(SessionManager));

                        // During this time, clients will have to wait until they get the first sync event that is sent after they get the UpdateFromGMSEvent
                        // And then send out a FinishingSyncWithHostLocalEvent when sync is complete
                        EventBus.Publish(new StartingSyncWithHostLocalEvent());
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }
        }

        public void OnFinishingSyncWithHostLocalEvent(FinishingSyncWithHostLocalEvent @event)
        {
            // When sync is complete, show all the assets
            ScenarioManager.AssetManager.ShowAll(HideAssetReasons.Syncing);
        }

        public virtual void EnterWaitingRoomLobby(bool fromHostDisconnection)
        {
            // If the user is already in the lobby (from Edit mode), then make sure
            // the host message is propagated
            if (!InWaitingRoomLobby || (fromHostDisconnection && InWaitingRoomLobby))
            {
                GIGXR.Platform.Utilities.Logger.Info($"Entered waiting room", nameof(SessionManager));

                InWaitingRoomLobby = true;
                ScenarioManager.AssetManager.HideAll(HideAssetReasons.WaitingRoom);

                EventBus.Publish(new EnteredWaitingRoomLobbyEvent(fromHostDisconnection));
            }
        }

        public virtual void LeaveWaitingRoomLobby()
        {
            if (InWaitingRoomLobby)
            {
                GIGXR.Platform.Utilities.Logger.Info($"Left waiting room", nameof(SessionManager));

                InWaitingRoomLobby = false;
                ScenarioManager.AssetManager.ShowAll(HideAssetReasons.WaitingRoom);

                EventBus.Publish(new LeftWaitingRoomLobbyEvent());
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            // Local

            ScenarioManager.ScenarioPlaying -= ScenarioManager_ScenarioPlayingAsync;

            ScenarioManager.AssetManager.AssetInstantiated -= AssetManager_AssetInstantiated;
            ScenarioManager.AssetManager.AssetDestroyed -= AssetManager_AssetDestroyed;
            ScenarioManager.AssetManager.ContentMarkerUpdated -= AssetManager_ContentMarkerUpdated;

            ScenarioManager.NewScenarioPathway -= ScenarioManager_NewScenarioPathway;
            ScenarioManager.ScenarioPlayModeSet -= ScenarioManager_ScenarioPlayModeSet;

            ScenarioManager.AssetManager.AssetContext.RemoveContext(
                nameof(ScenarioManager.AssetManager.AssetContext.IsScenarioAuthority));

            // Network

            NetworkManager.Unsubscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);
            NetworkManager.Unsubscribe<JoinRoomFailedNetworkEvent>(OnJoinRoomFailedNetworkEvent);
            NetworkManager.Unsubscribe<LeftRoomNetworkEvent>(OnLeftRoomNetworkEvent);
            NetworkManager.Unsubscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);
            NetworkManager.Unsubscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Unsubscribe<RoomPropertiesUpdateNetworkEvent>(OnRoomPropertiesUpdateNetworkEvent);

            NetworkManager.Unsubscribe<DisconnectedNetworkEvent>(OnDisconnectedNetworkEvent);

            NetworkManager.Unsubscribe<SessionRenamedNetworkEvent>(OnSessionRenamedNetworkEvent);
            NetworkManager.Unsubscribe<UserKickedNetworkEvent>(OnUserKickedNetworkEvent);
            NetworkManager.Unsubscribe<MasterClientSwitchedEvent>(OnMasterClientSwitchedNetworkEvent);
            NetworkManager.Unsubscribe<GoToWaitingRoomLobbyNetworkEvent>(OnGoToWaitingRoomLobbyNetworkEvent);
            NetworkManager.Unsubscribe<LeaveWaitingRoomLobbyNetworkEvent>(OnLeaveWaitingRoomLobbyNetworkEvent);
            NetworkManager.Unsubscribe<RoomListUpdateEvent>(OnRoomListUpdateEvent);

            NetworkManager.Unsubscribe<InstantiateAssetNetworkEvent>(OnInstantiateInteractableNetworkEvent);
            NetworkManager.Unsubscribe<DestroyAssetNetworkEvent>(OnDestroyAssetNetworkEvent);

            NetworkManager.Unsubscribe<PromoteToHostNetworkEvent>(OnPromoteToHostNetworkEvent);

            // EventBus

            EventBus.Unsubscribe<KickUserEvent>(OnKickUserEvent);
            EventBus.Unsubscribe<ReturnHostToSessionOwnerEvent>(OnReturnHostToSessionOwnerEvent);
            EventBus.Unsubscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
            EventBus.Unsubscribe<JoinSessionFromQrEvent>(OnJoinSessionFromQrEvent);
            EventBus.Unsubscribe<AutoJoinSessionEvent>(OnAutoJoinSessionEvent);
            EventBus.Unsubscribe<FinishingSyncWithHostLocalEvent>(OnFinishingSyncWithHostLocalEvent);
        }

        #endregion
    }
}