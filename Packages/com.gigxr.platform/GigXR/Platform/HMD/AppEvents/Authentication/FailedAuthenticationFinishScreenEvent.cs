namespace GIGXR.Platform.HMD.AppEvents.Events.Authentication
{
    /// <summary>
    /// Event sent out when the user is not successful in logging in.
    /// </summary>
    public class FailedAuthenticationFinishScreenEvent : BaseAuthenticationScreenEvent
    {
        public string Message { get; }

        public FailedAuthenticationFinishScreenEvent(string message) 
        {
            Message = message;
        }
    }
}