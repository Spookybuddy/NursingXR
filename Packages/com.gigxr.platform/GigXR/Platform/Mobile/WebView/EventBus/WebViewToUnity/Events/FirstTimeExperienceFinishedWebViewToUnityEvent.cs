namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView that the first time experience has finished.
    /// </summary>
    public class FirstTimeExperienceFinishedWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
    }

    public class FirstTimeExperienceFinishedWebViewEventSerializer
        : IWebViewToUnityEventSerializer<FirstTimeExperienceFinishedWebViewToUnityEvent>
    {
        public FirstTimeExperienceFinishedWebViewToUnityEvent Deserialize(string data)
        {
            return new FirstTimeExperienceFinishedWebViewToUnityEvent();
        }
    }
}