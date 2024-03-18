namespace GIGXR.Platform.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;

    public class ShowNewHostEvent : IGigEvent<AppEventBus>
    {
        public string UserName { get; }

        public ShowNewHostEvent(string userName)
        {
            UserName = userName;
        }
    }
}