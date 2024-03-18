namespace GIGXR.Platform.Networking
{
    /// <summary>
    /// Holds all the byte codes that are used for Photon events.
    /// </summary>
    public static class NetworkCodes
    {
        // Platform

        public const byte ClientReadyEventCode = 1;
        public const byte AssetPropertyRejected = 8;
        public const byte AssetPropertyChanged = 9;
        public const byte RequestPropertyUpdateNetworkEvent = 11;
        public const byte ClientPropertyUpdated = 13;

        // SessionScreen

        public const byte SyncGenerateUserCardEventCode = 12;  
        public const byte LeaveLobbyNetworkEventCode = 17;  
        public const byte GoToLobbyNetworkEventCode = 18;
        public const byte UpdateFromGMSEventCode = 19;
        public const byte SyncEditSessionEventCode = 20;
        public const byte HostClosedSessionEventCode = 14;
        public const byte ContentMarkerUpdate = 15;
        public const byte ContentMarkerRequest = 16;

        // StageManager

        public const byte SyncUpdateEventCode = 21;
        public const byte SyncRecordEventCode = 22;
        public const byte SyncRefreshEventCode = 23;
        public const byte StageLoadedEventCode = 39;
        public const byte AllStagesRemovedEventCode = 40;
        public const byte SyncSwitchStageEventCode = 41;
        public const byte SyncRenameStageEventCode = 42;
        public const byte SyncRemoveStageEventCode = 43;
        public const byte SyncReorderStageEventCode = 44;
        public const byte StageDuplicateEventCode = 45;
        public const byte SyncAllStagesEventCode = 90;

        // NetworkLog

        public const byte SyncWriteEventCode = 26;
        public const byte SyncLockLog = 27;

        // PhotonLocalTransformView

        public const byte SyncLocalsEventCode = 29;

        // SessionManager

        public const byte SyncKickEventCode = 24;
        public const byte SyncCloseRoom = 30;

        // Interactable should remove

        public const byte SyncLocalsInteractableEventCode = 40;

        // InteractableManager

        public const byte SyncInstantiateInteractable = 47;
        public const byte SyncDestroyInteractable = 48;

        // Host Management

        public const byte RequestUserToHost = 50;
        public const byte CancelRequestUserToHost = 51;
        public const byte AcceptHostRequest = 52;
        public const byte RejectHostRequest = 53;
        public const byte PromoteHost = 54;
        
        // Scenario Management

        public const byte ScenarioPlaying = 70;
        public const byte ScenarioPaused = 71;
        public const byte ScenarioStopped = 72;

        // Custom Serialization
        public const byte GuidSerialization = 120;
        public const byte ScenarioSyncDataSerializer = 123;
        public const byte ColorSerialization = 124;
        public const byte TransformSerialization = 125;
        public const byte SVectorSerialization = 126;
        public const byte SQuaternionSerialization = 127;
    }
}