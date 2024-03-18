namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity
{
    /// <summary>
    /// An interface for handling WebView to Unity events.
    /// </summary>
    public interface IWebViewToUnityEventHandler
    {
        /// <summary>
        /// Enables receiving events from the WebView.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables receiving events from the WebView.
        /// </summary>
        void Disable();

        /// <summary>
        /// Registers an event to be received from the WebView.
        /// </summary>
        /// <param name="eventCode">The event code, must match what the GMS is expecting.</param>
        /// <typeparam name="TWebViewEvent">The type of event.</typeparam>
        /// <typeparam name="TWebViewEventSerializer">The type of the event serializer.</typeparam>
        /// <returns></returns>
        bool RegisterWebViewToUnityEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IWebViewToUnityEvent
            where TWebViewEventSerializer : IWebViewToUnityEventSerializer<TWebViewEvent>;
    }
}