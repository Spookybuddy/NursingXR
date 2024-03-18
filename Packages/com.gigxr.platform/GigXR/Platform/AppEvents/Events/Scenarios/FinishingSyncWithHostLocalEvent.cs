namespace GIGXR.Platform.AppEvents.Events.Scenarios
{
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Core.EventBus;

    /// <summary>
    /// Local event that is sent out when a client is finished with the sync process
    /// during joining a session.
    /// </summary>
    public class FinishingSyncWithHostLocalEvent : IGigEvent<AppEventBus>
    {
        public FinishingSyncWithHostLocalEvent()
        {
        }
    }
}