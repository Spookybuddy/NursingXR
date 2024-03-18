namespace GIGXR.Platform.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;

    public class ShowRejectedHostPromptEvent : IGigEvent<AppEventBus>
    {
        public string UserName { get; }

        public ShowRejectedHostPromptEvent(string userName)
        {
            UserName = userName;
        }
    }
}