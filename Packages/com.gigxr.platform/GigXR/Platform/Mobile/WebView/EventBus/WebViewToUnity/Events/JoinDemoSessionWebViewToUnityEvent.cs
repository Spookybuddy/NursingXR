using System;
using Newtonsoft.Json;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView requesting to join a demo session.
    /// </summary>
    public class JoinDemoSessionWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public JoinDemoSessionWebViewToUnityEvent(JoinDemoSessionWebViewToUnityDto dto)
        {
            SessionId = Guid.Parse(dto.sessionId);
        }

        public Guid SessionId { get; }
    }

    [Serializable]
    public class JoinDemoSessionWebViewToUnityDto
    {
        public string sessionId;
    }

    public class
        JoinDemoSessionWebViewEventSerializer : IWebViewToUnityEventSerializer<JoinDemoSessionWebViewToUnityEvent>
    {
        public JoinDemoSessionWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<JoinDemoSessionWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<JoinDemoSessionWebViewToUnityDto>(data);
            return new JoinDemoSessionWebViewToUnityEvent(dto);
        }
    }
}