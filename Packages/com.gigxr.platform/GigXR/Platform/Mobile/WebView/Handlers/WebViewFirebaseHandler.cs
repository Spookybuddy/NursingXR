namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Mobile.WebView.EventBus;
    using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events;
    using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;

    /// <summary>
    /// Responsible for Firebase related WebView functionality.
    /// </summary>
    public class WebViewFirebaseHandler
    {
        private readonly IFirebaseManager firebaseManager;
        private readonly IWebViewController webViewController;
        private readonly IWebViewEventBus webViewEventBus;

        public WebViewFirebaseHandler(
            IFirebaseManager firebaseManager,
            IWebViewController webViewController,
            IWebViewEventBus webViewEventBus)
        {
            this.firebaseManager = firebaseManager;
            this.webViewController = webViewController;
            this.webViewEventBus = webViewEventBus;
        }

        public void Enable()
        {
            webViewEventBus.Subscribe<EnableNotificationsWebViewToUnityEvent>(OnEnableNotificationsWebViewToUnityEvent);
            webViewEventBus.Subscribe<LogoutWebViewToUnityEvent>(OnLogoutWebViewToUnityEvent);
            firebaseManager.DynamicLinkReceived += OnDeepLinkReceived;
        }

        public void Disable()
        {
            webViewEventBus.Unsubscribe<EnableNotificationsWebViewToUnityEvent>(
                OnEnableNotificationsWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<LogoutWebViewToUnityEvent>(OnLogoutWebViewToUnityEvent);
            firebaseManager.DynamicLinkReceived -= OnDeepLinkReceived;
        }

        private void OnEnableNotificationsWebViewToUnityEvent(EnableNotificationsWebViewToUnityEvent @event)
        {
            firebaseManager.TryEnableMessaging();
            webViewEventBus.RaiseUnityToWebViewEvent(new NotificationDialogDismissedUnityToWebViewEvent());
        }

        private void OnLogoutWebViewToUnityEvent(LogoutWebViewToUnityEvent @event)
        {
            // TODO This endpoint doesn't exist in GMS anymore, not sure if we should keep this around
            //firebaseManager.DeleteFirebaseToken();
        }

        private void OnDeepLinkReceived(string path)
        {
            webViewController.LoadPath(path);
        }
    }
}