using System;
using Newtonsoft.Json;

namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events
{
    /// <summary>
    /// A event to notify the WebView of the current content preferences.
    /// </summary>
    public class ContentPreferencesResponseUnityToWebViewEvent : BaseUnityToWebViewEvent
    {
        public bool AllowAutomaticBackgroundDownloads { get; }
        public int MaximumContentStorage { get; }

        public ContentPreferencesResponseUnityToWebViewEvent(
            bool allowAutomaticBackgroundDownloads,
            int maximumContentStorage)
        {
            AllowAutomaticBackgroundDownloads = allowAutomaticBackgroundDownloads;
            MaximumContentStorage = maximumContentStorage;
        }
    }

    [Serializable]
    public class ContentPreferencesResponseDto
    {
        public bool allowAutomaticBackgroundDownloads;
        public int maximumContentStorage;

        public ContentPreferencesResponseDto(ContentPreferencesResponseUnityToWebViewEvent @event)
        {
            allowAutomaticBackgroundDownloads = @event.AllowAutomaticBackgroundDownloads;
            maximumContentStorage = @event.MaximumContentStorage;
        }
    }

    public class ContentPreferencesResponseUnityToWebViewEventSerializer
        : IUnityToWebViewEventSerializer<ContentPreferencesResponseUnityToWebViewEvent>
    {
        public string Serialize(ContentPreferencesResponseUnityToWebViewEvent @event)
        {
            var dto = new ContentPreferencesResponseDto(@event);
            return JsonConvert.SerializeObject(dto);
        }
    }
}