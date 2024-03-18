using System;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to create a session.
    /// </summary>
    public class CreateSessionWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public CreateSessionWebViewToUnityEvent(CreateSessionWebViewToUnityDto dto)
        {
            SessionId = Guid.Parse(dto.sessionId);
        }

        public Guid SessionId { get; }
    }

    [Serializable]
    public class CreateSessionWebViewToUnityDto
    {
        public string sessionId;
    }

    public class CreateSessionWebViewEventSerializer : IWebViewToUnityEventSerializer<CreateSessionWebViewToUnityEvent>
    {
        public CreateSessionWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<CreateSessionWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<CreateSessionWebViewToUnityDto>(data);
            return new CreateSessionWebViewToUnityEvent(dto);
        }
    }
}