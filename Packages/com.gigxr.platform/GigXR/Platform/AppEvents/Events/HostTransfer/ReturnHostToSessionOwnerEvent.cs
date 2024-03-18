namespace GIGXR.Platform.AppEvents.Events
{
    using GIGXR.Platform.Core.EventBus;
    using System;

    /// <summary>
    /// An event that is sent out when the host of the session reclaims the 
    /// host status after being disconnected
    /// </summary>
    public class ReturnHostToSessionOwnerEvent : IGigEvent<AppEventBus>
    {
        public Guid PreviousHost { get; }

        public ReturnHostToSessionOwnerEvent(Guid prevHost)
        {
            PreviousHost = prevHost;
        }
    }
}