namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView
{
    /// <summary>
    /// An interface for handling Unity to WebView events.
    /// </summary>
    public interface IUnityToWebViewEventHandler
    {
        /// <summary>
        /// Register an event to be raised from Unity and sent to the WebView.
        /// </summary>
        /// <param name="eventCode">The event code, must match what the GMS is expecting.</param>
        /// <typeparam name="TWebViewEvent">The type of event.</typeparam>
        /// <typeparam name="TWebViewEventSerializer">The type of the event serializer.</typeparam>
        /// <returns></returns>
        bool RegisterUnityToWebViewEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IUnityToWebViewEvent
            where TWebViewEventSerializer : IUnityToWebViewEventSerializer<TWebViewEvent>;

        /// <summary>
        /// Raises an event from Unity to be sent to the WebView.
        /// </summary>
        /// <param name="event">The event to send.</param>
        /// <typeparam name="TWebViewEvent">The type of event.</typeparam>
        /// <returns></returns>
        bool RaiseUnityToWebViewEvent<TWebViewEvent>(TWebViewEvent @event) where TWebViewEvent : IUnityToWebViewEvent;
    }
}