using System;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to delete a download.
    /// </summary>
    public class DeleteDownloadWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public DeleteDownloadWebViewToUnityEvent(DeleteDownloadWebViewToUnityDto dto)
        {
            ResourceId = dto.resourceId;
        }

        public Guid ResourceId { get; }
    }

    [Serializable]
    public class DeleteDownloadWebViewToUnityDto
    {
        public Guid resourceId;
    }

    public class DeleteDownloadWebViewEventSerializer
        : IWebViewToUnityEventSerializer<DeleteDownloadWebViewToUnityEvent>
    {
        public DeleteDownloadWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<DeleteDownloadWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<DeleteDownloadWebViewToUnityDto>(data);
            return new DeleteDownloadWebViewToUnityEvent(dto);
        }
    }
}