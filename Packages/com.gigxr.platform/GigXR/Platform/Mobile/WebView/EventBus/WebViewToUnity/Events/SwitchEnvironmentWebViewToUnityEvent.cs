using System;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to switch environments (e.g., production, QA).
    /// </summary>
    public class SwitchEnvironmentWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public SwitchEnvironmentWebViewToUnityEvent(SwitchEnvironmentWebViewToUnityDto dto)
        {
            Environment = dto.environment;
        }

        public string Environment { get; }
    }

    [Serializable]
    public class SwitchEnvironmentWebViewToUnityDto
    {
        public string environment;
    }

    public class SwitchEnvironmentWebViewEventSerializer
        : IWebViewToUnityEventSerializer<SwitchEnvironmentWebViewToUnityEvent>
    {
        public SwitchEnvironmentWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<SwitchEnvironmentWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<SwitchEnvironmentWebViewToUnityDto>(data);
            return new SwitchEnvironmentWebViewToUnityEvent(dto);
        }
    }
}