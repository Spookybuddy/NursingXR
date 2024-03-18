using System;
using Newtonsoft.Json;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView requesting to join a session.
    /// </summary>
    public class JoinSessionWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public JoinSessionWebViewToUnityEvent(JoinSessionWebViewToUnityDto dto)
        {
            SessionId = Guid.Parse(dto.sessionId);
        }

        public Guid SessionId { get; }
    }

    [Serializable]
    public class JoinSessionWebViewToUnityDto
    {
        public string sessionId;
    }

    public class JoinSessionWebViewEventSerializer : IWebViewToUnityEventSerializer<JoinSessionWebViewToUnityEvent>
    {
        public JoinSessionWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<JoinSessionWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<JoinSessionWebViewToUnityDto>(data);
            return new JoinSessionWebViewToUnityEvent(dto);
        }
    }
}