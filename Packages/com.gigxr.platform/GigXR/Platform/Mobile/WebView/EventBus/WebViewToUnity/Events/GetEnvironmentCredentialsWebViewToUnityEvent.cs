namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView requesting the current environment switching credentials.
    /// </summary>
    public class GetEnvironmentCredentialsWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
    }

    public class GetEnvironmentCredentialsWebViewEventSerializer
        : IWebViewToUnityEventSerializer<GetEnvironmentCredentialsWebViewToUnityEvent>
    {
        public GetEnvironmentCredentialsWebViewToUnityEvent Deserialize(string data)
        {
            return new GetEnvironmentCredentialsWebViewToUnityEvent();
        }
    }
}