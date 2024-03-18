using System;

namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event that is sent out to propogate that a user has been kicked
    /// </summary>
    public class KickUserEvent : BaseSessionStatusChangeEvent
    {
        public Guid UserToKick { get; }

        public KickUserEvent(Guid newSessionName)
        {
            UserToKick = newSessionName;
        }
    }
}