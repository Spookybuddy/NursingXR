namespace GIGXR.Platform.AppEvents.Events.Scenarios
{
    using GIGXR.Platform.Scenarios;

    /// <summary>
    /// Event sent out locally when the scenario stops.
    /// </summary>
    public class ScenarioStoppingLocalEvent : SyncLocalScenarioEvent
    {
        public ScenarioStoppingLocalEvent()
        {
        }

        public ScenarioStoppingLocalEvent(ScenarioSyncData syncData) : base(syncData)
        {
        }
    }
}
