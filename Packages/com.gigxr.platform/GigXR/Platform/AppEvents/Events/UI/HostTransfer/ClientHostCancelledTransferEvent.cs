namespace GIGXR.Platform.AppEvents.Events.UI
{
    using GIGXR.Platform.Core.EventBus;
    using System;

    public class ClientHostCancelledTransferEvent : IGigEvent<AppEventBus>
    {
        public string HostName { get; }

        public ClientHostCancelledTransferEvent(string host)
        {
            HostName = host;
        }
    }
}