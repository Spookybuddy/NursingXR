namespace GIGXR.Platform.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;
    using System.Threading;

    public class ShowHostWaitPromptEvent : IGigEvent<AppEventBus>
    {
        public string UserName { get; }

        public int ActorNumber { get; }

        public CancellationToken PromptToken { get; }

        public ShowHostWaitPromptEvent(string userName, int actorNumber, CancellationToken token)
        {
            UserName = userName;
            ActorNumber = actorNumber;
            PromptToken = token;
        }
    }
}