using System;
using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity;

namespace GIGXR.Platform.Mobile.WebView.EventBus
{
    /// <summary>
    /// An event bus for publish/subscribe to/from the WebView frontend.
    /// </summary>
    public interface IWebViewEventBus
    {
        bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IWebViewToUnityEvent;

        bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IWebViewToUnityEvent;

        bool RegisterWebViewToUnityEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IWebViewToUnityEvent
            where TWebViewEventSerializer : IWebViewToUnityEventSerializer<TWebViewEvent>;

        bool RegisterUnityToWebViewEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IUnityToWebViewEvent
            where TWebViewEventSerializer : IUnityToWebViewEventSerializer<TWebViewEvent>;

        bool RaiseUnityToWebViewEvent<TWebViewEvent>(TWebViewEvent @event) where TWebViewEvent : IUnityToWebViewEvent;
    }
}