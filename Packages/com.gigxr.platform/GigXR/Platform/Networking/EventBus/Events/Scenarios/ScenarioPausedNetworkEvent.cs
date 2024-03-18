namespace GIGXR.Platform.Networking.EventBus.Events.Scenarios
{
    using GIGXR.Platform.Scenarios;

    public class ScenarioPausedNetworkEvent : ScenarioSyncedNetworkEvent
    {
        public override string ToString() => "Scenario Paused";

        public ScenarioPausedNetworkEvent(ScenarioSyncData syncData) : base(syncData)
        {
        }
    }

    public class
        ScenarioPausedNetworkEventSerializer : ICustomNetworkEventSerializer<
            ScenarioPausedNetworkEvent>
    {
        public object[] Serialize(ScenarioPausedNetworkEvent @event) => new object[] { @event.SyncData };

        public ScenarioPausedNetworkEvent Deserialize(object[] data) =>
            new ScenarioPausedNetworkEvent((ScenarioSyncData)data[0]);
    }
}