namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events
{
    /// <summary>
    /// An event to notify the WebView that Unity was unable to evict needed resources to make room on the device for
    /// more downloads.
    /// </summary>
    public class CannotEvictNeededResourcesUnityToWebViewEvent : BaseUnityToWebViewEvent
    {
    }

    public class CannotEvictNeededResourcesUnityToWebViewEventSerializer
        : IUnityToWebViewEventSerializer<CannotEvictNeededResourcesUnityToWebViewEvent>
    {
        public string Serialize(CannotEvictNeededResourcesUnityToWebViewEvent @event)
        {
            return "";
        }
    }
}