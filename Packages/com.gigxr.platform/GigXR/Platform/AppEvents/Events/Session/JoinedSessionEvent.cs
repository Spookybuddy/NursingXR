using System;

namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event sent out when the user themselves joins a session
    /// </summary>
    public class JoinedSessionEvent : BaseSessionStatusChangeEvent
    {
        public string NickName { get; }

        public Guid UserId { get; }

        public JoinedSessionEvent(string name, Guid id) 
        {
            NickName = name;
            UserId = id;
        }
    }
}