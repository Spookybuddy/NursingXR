namespace GIGXR.Platform.Sessions
{
    using Cysharp.Threading.Tasks;
    using GIGXR.GMS.Clients;
    using GIGXR.GMS.Models.Sessions.Responses;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.AppEvents.Events.Scenarios;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Scenarios;
    using System;

    /// <summary>
    /// Responsible for managing the lifecycle of a session.
    /// </summary>
    public interface ISessionManager
    {
        public event EventHandler<EventArgs> SessionStartedSave;

        public event EventHandler<EventArgs> SessionFinishedSave;

        public IScenarioManager ScenarioManager { get; }

        public INetworkManager NetworkManager { get; }

        public GmsApiClient ApiClient { get; }

        public AppEventBus EventBus { get; }

        public SessionDetailedView ActiveSession { get; }
        
        public byte UserCount { get; }

        public Guid HostId { get; }

        public bool IsHost { get; }

        public bool IsSessionCreator { get; }

        public bool InWaitingRoomLobby { get; }

        public bool HostPresentInSession { get; }

        public string PathwayInfo { get; }

        public ScenarioControlTypes PlayMode { get; }

        UniTask StartAdHocSessionAsync();

        UniTask StartSessionAsync(Guid sessionId);

        UniTask StartSessionFromPlanAsync(Guid sessionPlanId);

        UniTask StopSessionAsync();

        UniTask CancelJoiningSession(JoinTypes joinMethod);

        UniTask KickUserAsync(Guid accountId);

        UniTask SaveSessionAsync(bool markSessionAsSaved = false);

        UniTask SaveSessionCopyAsync();

        UniTask RenameSessionAsync(string name);

        /// <summary>
        /// Attempts to join a session
        /// </summary>
        /// <param name="sessionId">The GUID of the session to join</param>
        /// <param name="isDemoSession">Optional, defaults to false, if set to true, the session will load without networking.</param>
        /// <returns>True if successfully joins, false otherwise. The string contains a message for failure.</returns>
        UniTask<(bool, string)> JoinSessionAsync(Guid sessionId, bool isDemoSession = false);

        UniTask LeaveSessionAsync();

        void EnterWaitingRoomLobby(bool fromHostDisconnect);
        void LeaveWaitingRoomLobby();

        void AddSessionCapability(ISessionCapability capability);

        void RemoveSessionCapability(Type capabilityType);

        void SyncContentMarker();
    }
}