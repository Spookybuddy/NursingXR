namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView requesting the current downloads.
    /// </summary>
    public class GetDownloadsWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
    }

    public class GetDownloadsWebViewEventSerializer : IWebViewToUnityEventSerializer<GetDownloadsWebViewToUnityEvent>
    {
        public GetDownloadsWebViewToUnityEvent Deserialize(string data)
        {
            return new GetDownloadsWebViewToUnityEvent();
        }
    }
}