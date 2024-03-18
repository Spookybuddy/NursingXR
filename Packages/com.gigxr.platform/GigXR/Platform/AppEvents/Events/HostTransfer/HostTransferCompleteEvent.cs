namespace GIGXR.Platform.AppEvents.Events
{
    using GIGXR.Platform.Core.EventBus;
    using Platform.Scenarios;
    using System;

    public class HostTransferCompleteEvent : IGigEvent<AppEventBus>
    {
        public Guid NewHostId { get; }

        public Guid PreviousHostId { get; }

        public HostTransferCompleteEvent(Guid newHostId, Guid prevHostId)
        {
            NewHostId                  = newHostId;
            PreviousHostId             = prevHostId;
        }
    }
}