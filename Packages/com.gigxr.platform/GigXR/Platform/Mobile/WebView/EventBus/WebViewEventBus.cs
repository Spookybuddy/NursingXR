using System;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity;

namespace GIGXR.Platform.Mobile.WebView.EventBus
{
    public class WebViewEventBus : IDisposable, IWebViewEventBus
    {
        private readonly IGigEventBus<WebViewEventBus> eventBus;
        private readonly IUnityToWebViewEventHandler unityToWebViewEventHandler;
        private readonly IWebViewToUnityEventHandler webViewToUnityEventHandler;

        public WebViewEventBus(
            IGigEventBus<WebViewEventBus> eventBus,
            IUnityToWebViewEventHandler unityToWebViewEventHandler,
            IWebViewToUnityEventHandler webViewToUnityEventHandler)
        {
            this.eventBus = eventBus;
            this.unityToWebViewEventHandler = unityToWebViewEventHandler;
            this.webViewToUnityEventHandler = webViewToUnityEventHandler;

            webViewToUnityEventHandler.Enable();
        }

        public void Dispose()
        {
            webViewToUnityEventHandler.Disable();
        }

        public bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IWebViewToUnityEvent
        {
            return eventBus.Subscribe(eventHandler);
        }

        public bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IWebViewToUnityEvent
        {
            return eventBus.Unsubscribe(eventHandler);
        }

        public bool RegisterWebViewToUnityEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IWebViewToUnityEvent
            where TWebViewEventSerializer : IWebViewToUnityEventSerializer<TWebViewEvent>
        {
            return webViewToUnityEventHandler
                .RegisterWebViewToUnityEvent<TWebViewEvent, TWebViewEventSerializer>(eventCode);
        }

        public bool RegisterUnityToWebViewEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IUnityToWebViewEvent
            where TWebViewEventSerializer : IUnityToWebViewEventSerializer<TWebViewEvent>
        {
            return unityToWebViewEventHandler
                .RegisterUnityToWebViewEvent<TWebViewEvent, TWebViewEventSerializer>(eventCode);
        }

        public bool RaiseUnityToWebViewEvent<TWebViewEvent>(TWebViewEvent @event)
            where TWebViewEvent : IUnityToWebViewEvent
        {
            return unityToWebViewEventHandler.RaiseUnityToWebViewEvent(@event);
        }
    }
}