namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event sent out when the user joins a session as a client
    /// </summary>
    public class ClientJoinedSessionEvent : BaseSessionStatusChangeEvent
    {
        public ClientJoinedSessionEvent() { }
    }
}