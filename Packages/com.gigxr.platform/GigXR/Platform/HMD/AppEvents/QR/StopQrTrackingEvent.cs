namespace GIGXR.Platform.HMD.AppEvents.Events
{
    /// <summary>
    /// An event that indicates that QR tracking has stopped, not that it will stop QR tracking. Use a reference to IQrCodeManager if you
    /// need to manage or use QR Tracking.
    /// </summary>
    public class StopQrTrackingEvent : QrCodeEvent
    {
        public bool ReturnToAuthScreen { get; }

        public StopQrTrackingEvent(bool returnToAuthScreen)
        {
            ReturnToAuthScreen = returnToAuthScreen;
        }
    }
}