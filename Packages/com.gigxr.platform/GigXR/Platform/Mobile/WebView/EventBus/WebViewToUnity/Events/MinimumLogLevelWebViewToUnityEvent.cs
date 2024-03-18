using System;
using GIGXR.Platform.Mobile;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to set the minimum log level for the current user.
    /// </summary>
    public class MinimumLogLevelWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public MinimumLogLevelWebViewToUnityEvent(MinimumLogLevelWebViewToUnityDto dto)
        {
            MinimumLogLevel = dto.minimumLogLevel;
        }

        public CloudLogLevel MinimumLogLevel { get; }
    }

    [Serializable]
    public class MinimumLogLevelWebViewToUnityDto
    {
        public CloudLogLevel minimumLogLevel;
    }

    public class
        MinimumLogLevelWebViewEventSerializer : IWebViewToUnityEventSerializer<MinimumLogLevelWebViewToUnityEvent>
    {
        public MinimumLogLevelWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<MinimumLogLevelWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<MinimumLogLevelWebViewToUnityDto>(data);
            return new MinimumLogLevelWebViewToUnityEvent(dto);
        }
    }
}