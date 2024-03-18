using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to queue a download set.
    /// </summary>
    public class QueueDownloadSetWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public QueueDownloadSetWebViewToUnityEvent(QueueDownloadSetWebViewToUnityDto dto)
        {
            ResourceIds = dto.resourceIds;
            CanEvictNeededResources = dto.canEvictNeededResources;
        }

        public List<Guid> ResourceIds { get; }
        public bool CanEvictNeededResources { get; }
    }

    [Serializable]
    public class QueueDownloadSetWebViewToUnityDto
    {
        public List<Guid> resourceIds;
        public bool canEvictNeededResources;
    }

    public class QueueDownloadSetWebViewEventSerializer
        : IWebViewToUnityEventSerializer<QueueDownloadSetWebViewToUnityEvent>
    {
        public QueueDownloadSetWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<QueueDownloadSetWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<QueueDownloadSetWebViewToUnityDto>(data);
            return new QueueDownloadSetWebViewToUnityEvent(dto);
        }
    }
}