namespace GIGXR.Platform.AppEvents.Events.Session
{
    /// <summary>
    /// Event that is sent out after the user calibrates in case they are joining a session via QR Code
    /// </summary>
    public class AutoJoinSessionEvent : BaseSessionStatusChangeEvent
    {
        public AutoJoinSessionEvent()
        {
        }
    }
}