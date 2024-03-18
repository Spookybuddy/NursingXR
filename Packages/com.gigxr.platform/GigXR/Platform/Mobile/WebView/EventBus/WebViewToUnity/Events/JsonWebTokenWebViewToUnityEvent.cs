using System;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView with a json web token of the current user.
    /// Used to allow mobile users to skip user/pass authentication if they
    /// have elected to stay signed in.
    /// </summary>
    public class JsonWebTokenWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public JsonWebTokenWebViewToUnityEvent(JsonWebTokenWebViewToUnityDto dto)
        {
            JsonWebToken = dto.jsonWebToken;
        }

        public string JsonWebToken { get; }
    }

    public class JsonWebTokenWebViewEventSerializer : IWebViewToUnityEventSerializer<JsonWebTokenWebViewToUnityEvent>
    {
        public JsonWebTokenWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<JsonWebTokenWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<JsonWebTokenWebViewToUnityDto>(data);
            return new JsonWebTokenWebViewToUnityEvent(dto);
        }
    }
}