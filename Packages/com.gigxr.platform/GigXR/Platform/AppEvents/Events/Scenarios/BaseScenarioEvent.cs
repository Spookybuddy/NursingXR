namespace GIGXR.Platform.AppEvents.Events.Scenarios
{
    using GIGXR.Platform.Core.EventBus;
    using GIGXR.Platform.Scenarios;

    /// <summary>
    /// Base AppEvent for anything related to Scenarios.
    /// </summary>
    public abstract class BaseScenarioEvent : IGigEvent<AppEventBus>
    {
    }

    public class SyncLocalScenarioEvent : BaseScenarioEvent
    {
        public ScenarioSyncData? SyncData { get; }

        public SyncLocalScenarioEvent(ScenarioSyncData syncData)
        {
            SyncData = syncData;
        }

        public SyncLocalScenarioEvent()
        {
        }
    }
}