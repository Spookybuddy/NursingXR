namespace GIGXR.Platform.Networking.EventBus.Events.Scenarios
{
    using GIGXR.Platform.Scenarios;

    public class ScenarioStoppedNetworkEvent : ScenarioSyncedNetworkEvent
    {
        public override string ToString() => "Scenario Stopped";

        public ScenarioStoppedNetworkEvent(ScenarioSyncData syncData) : base(syncData)
        {
        }
    }

    public class
        ScenarioStoppedNetworkEventSerializer : ICustomNetworkEventSerializer<
            ScenarioStoppedNetworkEvent>
    {
        public object[] Serialize(ScenarioStoppedNetworkEvent @event) => new object[] { @event.SyncData };

        public ScenarioStoppedNetworkEvent Deserialize(object[] data) =>
            new ScenarioStoppedNetworkEvent((ScenarioSyncData)data[0]);
    }
}