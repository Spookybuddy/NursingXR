using System.Collections.Generic;
using GIGXR.Platform.Downloads.Data;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events
{
    /// <summary>
    /// An event to sent to the WebView to show the current state of the downloads on the device.
    /// </summary>
    public class GetDownloadsResponseUnityToWebViewEvent : BaseUnityToWebViewEvent
    {
        public IEnumerable<DownloadInfo> Downloads { get; }

        public GetDownloadsResponseUnityToWebViewEvent(IEnumerable<DownloadInfo> downloads)
        {
            Downloads = downloads;
        }
    }

    public class GetDownloadsResponseUnityToWebViewEventSerializer
        : IUnityToWebViewEventSerializer<GetDownloadsResponseUnityToWebViewEvent>
    {
        public string Serialize(GetDownloadsResponseUnityToWebViewEvent @event)
        {
            return JsonConvert.SerializeObject(@event.Downloads);
        }
    }
}