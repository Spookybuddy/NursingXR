namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView requesting content preferences.
    /// </summary>
    public class GetContentPreferencesWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
    }

    public class GetContentPreferencesWebViewEventSerializer
        : IWebViewToUnityEventSerializer<GetContentPreferencesWebViewToUnityEvent>
    {
        public GetContentPreferencesWebViewToUnityEvent Deserialize(string data)
        {
            return new GetContentPreferencesWebViewToUnityEvent();
        }
    }
}