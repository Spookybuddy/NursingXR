namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event that is sent out when a Session is renamed.
    /// </summary>
    public class SessionRenamedEvent : BaseSessionStatusChangeEvent
    {
        public string NewSessionName { get; }

        public SessionRenamedEvent(string newSessionName)
        {
            NewSessionName = newSessionName;
        }
    }
}