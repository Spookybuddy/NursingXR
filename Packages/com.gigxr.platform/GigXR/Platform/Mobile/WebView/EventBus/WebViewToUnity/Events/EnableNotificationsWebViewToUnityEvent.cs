namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to enable notifications.
    /// </summary>
    public class EnableNotificationsWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
    }

    public class EnableNotificationsWebViewEventSerializer
        : IWebViewToUnityEventSerializer<EnableNotificationsWebViewToUnityEvent>
    {
        public EnableNotificationsWebViewToUnityEvent Deserialize(string data)
        {
            return new EnableNotificationsWebViewToUnityEvent();
        }
    }
}