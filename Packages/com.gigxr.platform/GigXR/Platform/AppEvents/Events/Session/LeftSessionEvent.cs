namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event sent out when the user themselves leaves a session
    /// </summary>
    public class LeftSessionEvent : BaseSessionStatusChangeEvent
    {
        public LeftSessionEvent() { }
    }
}