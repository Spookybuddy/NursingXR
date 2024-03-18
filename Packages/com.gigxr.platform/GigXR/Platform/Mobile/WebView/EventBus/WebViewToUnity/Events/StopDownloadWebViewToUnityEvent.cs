using System;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to stop a download.
    /// </summary>
    public class StopDownloadWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public StopDownloadWebViewToUnityEvent(StopDownloadWebViewToUnityDto dto)
        {
            ResourceId = dto.resourceId;
        }

        public Guid ResourceId { get; }
    }

    [Serializable]
    public class StopDownloadWebViewToUnityDto
    {
        public Guid resourceId;
    }

    public class StopDownloadWebViewEventSerializer : IWebViewToUnityEventSerializer<StopDownloadWebViewToUnityEvent>
    {
        public StopDownloadWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<StopDownloadWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<StopDownloadWebViewToUnityDto>(data);
            return new StopDownloadWebViewToUnityEvent(dto);
        }
    }
}