namespace GIGXR.Platform.AppEvents.Events
{
    using GIGXR.Platform.Core.EventBus;
    using System;

    public class StartHostRequestEvent : IGigEvent<AppEventBus>
    {
        public Guid CurrentUserId { get; }

        public string CurrentUserName { get; }

        public bool RestartRequest { get; }

        public StartHostRequestEvent(Guid currentUserId, string currentUserName)
        {
            CurrentUserId = currentUserId;
            CurrentUserName = currentUserName;
        }

        public StartHostRequestEvent(Guid currentUserId, string currentUserName, bool restart)
        {
            CurrentUserId = currentUserId;
            CurrentUserName = currentUserName;
            RestartRequest = restart;
        }
    }
}