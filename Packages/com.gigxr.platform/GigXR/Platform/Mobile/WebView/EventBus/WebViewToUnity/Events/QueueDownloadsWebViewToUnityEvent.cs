using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to queue a list of downloads.
    /// </summary>
    public class QueueDownloadsWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public QueueDownloadsWebViewToUnityEvent(QueueDownloadsWebViewToUnityDto dto)
        {
            ResourceIds = dto.resourceIds;
        }

        public List<Guid> ResourceIds { get; }
    }

    [Serializable]
    public class QueueDownloadsWebViewToUnityDto
    {
        public List<Guid> resourceIds;
    }

    public class QueueDownloadsWebViewEventSerializer
        : IWebViewToUnityEventSerializer<QueueDownloadsWebViewToUnityEvent>
    {
        public QueueDownloadsWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<QueueDownloadsWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<QueueDownloadsWebViewToUnityDto>(data);
            return new QueueDownloadsWebViewToUnityEvent(dto);
        }
    }
}