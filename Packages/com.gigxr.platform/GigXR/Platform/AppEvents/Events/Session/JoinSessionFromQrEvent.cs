using System;

namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event sent out that allows a user to join a session via a QR Code
    /// </summary>
    public class JoinSessionFromQrEvent : BaseSessionStatusChangeEvent
    {
        public Guid SessionId { get; }

        public JoinSessionFromQrEvent(Guid id)
        {
            SessionId = id;
        }
    }
}