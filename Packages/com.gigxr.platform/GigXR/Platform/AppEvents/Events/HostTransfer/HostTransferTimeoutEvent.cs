namespace GIGXR.Platform.AppEvents.Events
{
    using GIGXR.Platform.Core.EventBus;

    public class HostTransferTimeoutEvent : IGigEvent<AppEventBus>
    {
        public HostTransferTimeoutEvent()
        {
        }
    }
}