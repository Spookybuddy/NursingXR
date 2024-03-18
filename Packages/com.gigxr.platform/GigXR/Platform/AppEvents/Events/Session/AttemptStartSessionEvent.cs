using System;

namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event that is sent out when the user starts to join a session.
    /// </summary>
    public class AttemptStartSessionEvent : BaseSessionStatusChangeEvent
    {
        public Guid SessionId { get; }

        public AttemptStartSessionEvent(Guid sessionId) 
        {
            SessionId = sessionId;
        }
    }
}