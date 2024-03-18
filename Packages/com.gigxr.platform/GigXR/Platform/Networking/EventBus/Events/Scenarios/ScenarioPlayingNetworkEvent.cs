namespace GIGXR.Platform.Networking.EventBus.Events.Scenarios
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.Scenarios;
    using Photon.Realtime;

    /// <summary>
    /// Custom network event that provides the data needed to re-sync a scenario.
    /// </summary>
    public class ScenarioSyncedNetworkEvent : ICustomNetworkEvent
    {
        public ScenarioSyncData SyncData { get; }

        public RaiseEventOptions RaiseEventOptions { get; } = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString() => "Scenario";

        public ScenarioSyncedNetworkEvent(ScenarioSyncData syncData)
        {
            SyncData = syncData;
        }
    }

    public class ScenarioPlayingNetworkEvent : ScenarioSyncedNetworkEvent
    {
        public override string ToString() => "Scenario Playing";

        public ScenarioPlayingNetworkEvent(ScenarioSyncData syncData) : base(syncData)
        {
        }
    }

    public class ScenarioPlayingNetworkEventSerializer : ICustomNetworkEventSerializer<ScenarioPlayingNetworkEvent>
    {
        public object[] Serialize(ScenarioPlayingNetworkEvent @event) => new object[] { @event.SyncData };

        public ScenarioPlayingNetworkEvent Deserialize
            (object[] data) => new ScenarioPlayingNetworkEvent((ScenarioSyncData)data[0]);
    }
}