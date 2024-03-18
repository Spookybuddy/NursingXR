namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView notifying Unity the user has logged out.
    /// </summary>
    public class LogoutWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
    }

    public class LogoutWebViewEventSerializer : IWebViewToUnityEventSerializer<LogoutWebViewToUnityEvent>
    {
        public LogoutWebViewToUnityEvent Deserialize(string data)
        {
            return new LogoutWebViewToUnityEvent();
        }
    }
}