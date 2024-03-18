namespace GIGXR.Platform.AppEvents.Events.Scenarios
{
    /// <summary>
    /// Event sent out locally when the session has started to load.
    /// </summary>
    public class FinishingJoinSessionLocalEvent : BaseScenarioEvent
    {
        public FinishingJoinSessionLocalEvent()
        {
        }
    }

    public class CancelJoinSessionLocalEvent : BaseScenarioEvent
    {
        public CancelJoinSessionLocalEvent()
        {
        }
    }
}
