namespace GIGXR.Platform.AppEvents.Events.Scenarios
{
    using GIGXR.Platform.Scenarios;

    /// <summary>
    /// Event sent out locally when the scenario goes to play mode.
    /// </summary>
    public class ScenarioPlayingLocalEvent : SyncLocalScenarioEvent
    {
        public ScenarioPlayingLocalEvent()
        {
        }

        public ScenarioPlayingLocalEvent(ScenarioSyncData syncData) : base(syncData)
        {
        }
    }
}
