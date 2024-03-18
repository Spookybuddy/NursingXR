namespace GIGXR.Platform.AppEvents.Events.Scenarios
{
    using GIGXR.Platform.Scenarios;

    /// <summary>
    /// Event sent out locally when the scenario pauses.
    /// </summary>
    public class ScenarioPausingLocalEvent : SyncLocalScenarioEvent
    {
        public ScenarioPausingLocalEvent()
        {
        }

        public ScenarioPausingLocalEvent(ScenarioSyncData syncData) : base(syncData)
        {
        }
    }
}
