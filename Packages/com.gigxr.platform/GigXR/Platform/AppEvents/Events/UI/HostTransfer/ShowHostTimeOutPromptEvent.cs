namespace GIGXR.Platform.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;
    using System;

    public class ShowHostTimeOutPromptEvent : IGigEvent<AppEventBus>
    {
        public string UserName { get; }

        public Guid UserGuid { get; }

        public ShowHostTimeOutPromptEvent(string userName, Guid userGuid)
        {
            UserName = userName;
            UserGuid = userGuid;
        }
    }
}