namespace GIGXR.Platform.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;
    using System;

    public class ClientStartNewHostEvent : IGigEvent<AppEventBus>
    {
        public Action ClientAcceptsAction { get; }

        public Action ClientRejectsAction { get; }

        public string HostName { get; }

        public ClientStartNewHostEvent(Action accept, Action reject, string host)
        {
            ClientAcceptsAction = accept;
            ClientRejectsAction = reject;
            HostName = host;
        }
    }
}