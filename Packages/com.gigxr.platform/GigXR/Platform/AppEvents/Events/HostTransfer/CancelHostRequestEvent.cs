namespace GIGXR.Platform.AppEvents.Events
{
    using GIGXR.Platform.Core.EventBus;

    public class CancelHostRequestEvent : IGigEvent<AppEventBus>
    {
        public int ActorNumber { get; }

        public CancelHostRequestEvent(int actorNumber)
        {
            ActorNumber = actorNumber;
        }
    }
}