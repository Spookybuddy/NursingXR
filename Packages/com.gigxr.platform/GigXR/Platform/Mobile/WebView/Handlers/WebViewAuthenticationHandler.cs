namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    using Interfaces;
    using EventBus;
    using EventBus.WebViewToUnity.Events;
    using System;

    /// <summary>
    /// Responsible for managing authentication functionality from the WebView.
    /// </summary>
    public class WebViewAuthenticationHandler : IDisposable
    {
        private readonly IAuthenticationManager authenticationManager;
        private readonly IWebViewEventBus webViewEventBus;

        public WebViewAuthenticationHandler(
            IAuthenticationManager authenticationManager,
            IWebViewEventBus webViewEventBus)
        {
            this.authenticationManager = authenticationManager;
            this.webViewEventBus = webViewEventBus;

            webViewEventBus.Subscribe<JsonWebTokenWebViewToUnityEvent>(OnJsonWebTokenWebViewToUnityEvent);
            webViewEventBus.Subscribe<LogoutWebViewToUnityEvent>(OnLogoutWebViewToUnityEvent);
        }

        private void OnJsonWebTokenWebViewToUnityEvent(JsonWebTokenWebViewToUnityEvent @event)
        {
            authenticationManager.AuthenticateWithJsonWebToken(@event.JsonWebToken);
        }

        private void OnLogoutWebViewToUnityEvent(LogoutWebViewToUnityEvent @event)
        {
            authenticationManager.LogOut();
        }

        public void Dispose()
        {
            webViewEventBus.Unsubscribe<JsonWebTokenWebViewToUnityEvent>(OnJsonWebTokenWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<LogoutWebViewToUnityEvent>(OnLogoutWebViewToUnityEvent);
        }
    }
}