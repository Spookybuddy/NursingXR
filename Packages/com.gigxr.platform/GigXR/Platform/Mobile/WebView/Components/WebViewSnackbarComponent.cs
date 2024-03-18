using GIGXR.Platform.Core.DependencyValidator;
using GIGXR.Platform.Mobile.WebView.EventBus;
using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.Components
{
    /// <summary>
    /// Provides a way to queue a snackbar message to be sent to the WebView.
    ///
    /// Will be sent now if the WebView is visible, otherwise it will be sent the next time the WebView is shown even if
    /// that is after an app restart.
    /// </summary>
    /// <remarks>
    /// The lifecycle of a queued message is actually non-trivial:
    ///
    /// Message queued when WebView is not visible:
    /// - Show next time the WebView is enabled
    /// - Show next app start
    ///
    /// Message queued when WebView is visible:
    /// - If page is finished loading, show now
    /// - If page is loading, wait for it to finish
    /// - If WebView is disabled when there is a queued message, show it next time
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UniWebView))]
    public class WebViewSnackbarComponent : MonoBehaviour, IWebViewSnackbarComponent
    {
        // --- Private Variables:

        [SerializeField, RequireDependency] private UniWebView uniWebView;

        private IWebViewEventBus WebViewEventBus { get; set; }

        private bool isStarted;

        private bool isPageLoading;

        private string QueuedSnackbarMessage
        {
            get => PlayerPrefs.GetString("gigxr-queued-snackbar-message", "");
            set => PlayerPrefs.SetString("gigxr-queued-snackbar-message", value);
        }

        // --- Unity Methods:

        private void OnEnable()
        {
            uniWebView.OnPageStarted += OnPageStarted;
            uniWebView.OnPageFinished += OnPageFinished;

            if (isStarted && !isPageLoading)
                SendSnackbarMessage();
        }

        public void InitializeAfterOnEnable(IWebViewEventBus webViewEventBus)
        {
            WebViewEventBus = webViewEventBus;
        }

        private void OnDisable()
        {
            isPageLoading = false;
            uniWebView.OnPageStarted -= OnPageStarted;
            uniWebView.OnPageFinished -= OnPageFinished;
        }

        private void Start() => isStarted = true;

        // --- Event Handlers:

        private void OnPageStarted(UniWebView webview, string url)
        {
            isPageLoading = true;
        }

        private void OnPageFinished(UniWebView webView, int statusCode, string url)
        {
            isPageLoading = false;
            SendSnackbarMessage();
        }

        // --- Public Methods:

        public void QueueSnackbarMessage(string message)
        {
            QueuedSnackbarMessage = message;

            if (isActiveAndEnabled && isStarted && !isPageLoading)
                SendSnackbarMessage();
        }

        // --- Private Methods:

        private void SendSnackbarMessage()
        {
            if (string.IsNullOrWhiteSpace(QueuedSnackbarMessage))
                return;

            WebViewEventBus.RaiseUnityToWebViewEvent(new SnackbarUnityToWebViewEvent(QueuedSnackbarMessage));
            QueuedSnackbarMessage = "";
        }
    }
}