using System;
using System.Collections.Generic;
using GIGXR.Platform.Core.Settings;
using GIGXR.Platform.Mobile.WebView.EventBus;
using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;

namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    /// <summary>
    /// Responsible for manging the switch environment functionality of the WebView.
    /// </summary>
    public class WebViewEnvironmentSwitchingHandler
    {
        private readonly IWebViewController webViewController;
        private readonly IWebViewEventBus webViewEventBus;
        private readonly AuthenticationProfile AuthenticationProfile;

        public WebViewEnvironmentSwitchingHandler(
            IWebViewController webViewController,
            IWebViewEventBus webViewEventBus,
            AuthenticationProfile authProfile)
        {
            this.webViewController = webViewController;
            this.webViewEventBus = webViewEventBus;
            AuthenticationProfile = authProfile;
        }

        public void Enable()
        {
            webViewEventBus.Subscribe<GetEnvironmentCredentialsWebViewToUnityEvent>(OnGetEnvironmentCredentialsEvent);
            webViewEventBus.Subscribe<SwitchEnvironmentWebViewToUnityEvent>(OnSwitchEnvironmentEvent);
        }

        public void Disable()
        {
            webViewEventBus.Unsubscribe<GetEnvironmentCredentialsWebViewToUnityEvent>(OnGetEnvironmentCredentialsEvent);
            webViewEventBus.Unsubscribe<SwitchEnvironmentWebViewToUnityEvent>(OnSwitchEnvironmentEvent);
        }

        private void OnGetEnvironmentCredentialsEvent(GetEnvironmentCredentialsWebViewToUnityEvent @event)
        {
            var environmentCredentials = new Dictionary<string, string>
            {
                // TODO this needs to be data driven, mobile should not define it's own QR switching codes
                {"qa", "gigxr"},
                {"production", "gigxr"}
            };

            var responseEvent = new GetEnvironmentCredentialsResponseUnityToWebViewEvent(environmentCredentials);
            webViewEventBus.RaiseUnityToWebViewEvent(responseEvent);
        }

        private void OnSwitchEnvironmentEvent(SwitchEnvironmentWebViewToUnityEvent @event)
        {
            var environment = @event.Environment;

            if (environment == null)
                return;

#if DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Setting environment to {environment}");
#endif

            if(AuthenticationProfile.TrySetNewEnvironmentViaCode(environment))
            {
                webViewController.SetBaseUri(new Uri(AuthenticationProfile.ApiUrl()));

                // Refresh with the new environment.
                webViewController.LoadPath();
            }
        }
    }
}