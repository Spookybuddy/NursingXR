namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events
{
    /// <summary>
    /// An event to notify the WebView when a download set operation failed due to insufficient space being available
    /// on the device.
    /// </summary>
    public class DownloadSetFailedInsufficientStorageUnityToWebViewEvent : BaseUnityToWebViewEvent
    {
    }

    public class DownloadSetFailedInsufficientStorageUnityToWebViewEventSerializer
        : IUnityToWebViewEventSerializer<DownloadSetFailedInsufficientStorageUnityToWebViewEvent>
    {
        public string Serialize(DownloadSetFailedInsufficientStorageUnityToWebViewEvent @event)
        {
            return "";
        }
    }
}