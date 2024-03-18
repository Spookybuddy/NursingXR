namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event that is sent out when the host closes the session
    /// </summary>
    public class HostClosedSessionEvent : BaseSessionStatusChangeEvent
    {
        public HostClosedSessionEvent() { }
    }
}