namespace GIGXR.Platform.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;
    using System;

    public class StartNewHostPromptEvent : IGigEvent<AppEventBus>
    {
        public string UserName { get; }

        public int ActorNumber { get; }

        public Guid UserGuid { get; }

        public StartNewHostPromptEvent(string userName, int actorNumber, Guid userGuid)
        {
            UserName = userName;
            ActorNumber = actorNumber;
            UserGuid = userGuid;
        }
    }
}