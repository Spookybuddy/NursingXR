using System;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events
{
    /// <summary>
    /// An event from the WebView to set the content preferences.
    /// </summary>
    public class SetContentPreferencesWebViewToUnityEvent : BaseWebViewToUnityToUnityEvent
    {
        public SetContentPreferencesWebViewToUnityEvent(SetContentPreferencesWebViewToUnityDto dto)
        {
            AllowAutomaticBackgroundDownloads = dto.allowAutomaticBackgroundDownloads;
            MaximumContentStorage = dto.maximumContentStorage;
        }

        public bool AllowAutomaticBackgroundDownloads { get; }
        public int MaximumContentStorage { get; }
    }

    [Serializable]
    public class SetContentPreferencesWebViewToUnityDto
    {
        public bool allowAutomaticBackgroundDownloads;
        public int maximumContentStorage;
    }

    public class SetContentPreferencesWebViewEventSerializer
        : IWebViewToUnityEventSerializer<SetContentPreferencesWebViewToUnityEvent>
    {
        public SetContentPreferencesWebViewToUnityEvent Deserialize(string data)
        {
            // var dto = JsonUtility.FromJson<SetContentPreferencesWebViewToUnityDto>(data);
            var dto = JsonConvert.DeserializeObject<SetContentPreferencesWebViewToUnityDto>(data);
            return new SetContentPreferencesWebViewToUnityEvent(dto);
        }
    }
}