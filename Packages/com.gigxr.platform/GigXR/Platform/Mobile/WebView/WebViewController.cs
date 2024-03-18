using System;
using GIGXR.Platform.Core.DependencyValidator;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Mobile.WebView.Components;
using GIGXR.Platform.Mobile.WebView.EventBus;
using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;
using GIGXR.Platform.Mobile.WebView.Handlers;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GIGXR.Platform.Mobile.WebView
{
    using Platform.AppEvents;
    using Platform.Core.DependencyInjection;
    using GIGXR.Platform.Mobile;
    using Interfaces;
    using Sessions;
    using GIGXR.Platform.UI;

    /// <summary>
    /// Responsible for managing UniWebView by delegating to other classes.
    ///
    /// Delegation to functionality that requires MonoBehaviours is done via components (MonoBehaviours). Everything
    /// else is done via plain C# classes.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UniWebViewComponent))]
    [RequireComponent(typeof(WebViewFirstTimeExperienceComponent))]
    [RequireComponent(typeof(WebViewSnackbarComponent))]
    public class WebViewController : MonoBehaviour, IWebViewController
    {
        // --- Private Variables:

        [SerializeField, RequireDependency]
        private UniWebViewComponent uniWebView;

        [SerializeField, RequireDependency]
        private WebViewFirstTimeExperienceComponent webViewFirstTimeExperienceComponent;

        [SerializeField, RequireDependency]
        private WebViewSnackbarComponent webViewSnackbarComponent;

        // TODO: CU-250prkv - ContentManager
        // [SerializeField, RequireDependency]  
        // private ContentManager contentManager;

        private IFirebaseManager FirebaseManager { get; set; }
        public IWebViewEventBus WebViewEventBus { get; private set; }
        private ICalibrationManager CalibrationManager { get; set; }
        private ISessionManager SessionManager { get; set; }
        private IAuthenticationManager AuthenticationManager { get; set; }
        
        private AppEventBus AppEventBus { get; set; }

        private WebViewUriAndPathHandler webViewUriAndPathHandler;
        private WebViewUserAgentHandler webViewUserAgentHandler;
        private WebViewJsonWebTokenCookieHandler cookieHandler;
        private WebViewEnvironmentSwitchingHandler webViewEnvironmentSwitchingHandler;
        private WebViewDownloadsHandler downloadsHandler;
        private WebViewFirebaseHandler firebaseHandler;
        private WebViewAuthenticationHandler authenticationHandler;
        private WebViewSessionHandler sessionHandler;

        bool constructionComplete = false;

        // --- Unity Methods:

        [InjectDependencies]
        public void Construct(
            ICalibrationManager calibrationManager,
            ISessionManager sessionManager,
            IAuthenticationManager authenticationManager,
            AppEventBus appEventBus,
            IFirebaseManager firebaseManager,
            ProfileManager profileManager,
            UiEventBus uiEventBusInstance)
        {
            CalibrationManager = calibrationManager;
            SessionManager = sessionManager;
            AuthenticationManager = authenticationManager;
            AppEventBus = appEventBus;

            var eventBus = new GigEventBus<WebViewEventBus>();
            WebViewEventBus = new WebViewEventBus(
                eventBus,
                new UnityToWebViewEventHandler(uniWebView),
                new WebViewToUnityEventHandler(uniWebView, eventBus));

            webViewUriAndPathHandler =
                new WebViewUriAndPathHandler(uniWebView, webViewFirstTimeExperienceComponent, profileManager.authenticationProfile);
            webViewUserAgentHandler = new WebViewUserAgentHandler(uniWebView);

            webViewEnvironmentSwitchingHandler = new WebViewEnvironmentSwitchingHandler(this, WebViewEventBus, profileManager.authenticationProfile);
            downloadsHandler = new WebViewDownloadsHandler(WebViewEventBus, webViewSnackbarComponent);
            firebaseHandler = new WebViewFirebaseHandler(firebaseManager, this, WebViewEventBus);

            sessionHandler = new WebViewSessionHandler(uiEventBusInstance, AppEventBus, CalibrationManager, SessionManager, this, WebViewEventBus);
            cookieHandler = new WebViewJsonWebTokenCookieHandler(AuthenticationManager, profileManager.authenticationProfile, WebViewEventBus);
            authenticationHandler = new WebViewAuthenticationHandler(AuthenticationManager, WebViewEventBus);

            webViewEnvironmentSwitchingHandler.Enable();

            downloadsHandler.Enable();

            firebaseHandler.Enable();

            cookieHandler.RestoreJsonWebTokenCookie();

            var eventRegistrar = new WebViewEventRegistrar(WebViewEventBus);
            eventRegistrar.RegisterEvents();

            webViewFirstTimeExperienceComponent.InitializeAfterOnEnable(WebViewEventBus);
            webViewSnackbarComponent.InitializeAfterOnEnable(WebViewEventBus);

            WebViewEventBus.Subscribe<MinimumLogLevelWebViewToUnityEvent>(OnMinimumLogLevelEvent);

            constructionComplete = true;
        }

        private void OnEnable()
        {
            uniWebView.OnShouldClose += PreventAndroidBackButtonClosingWebView;   
        }

        private void OnDisable()
        {
            uniWebView.OnShouldClose -= PreventAndroidBackButtonClosingWebView;
            webViewEnvironmentSwitchingHandler.Disable();
            downloadsHandler.Disable();
            firebaseHandler.Disable();
            WebViewEventBus.Unsubscribe<MinimumLogLevelWebViewToUnityEvent>(OnMinimumLogLevelEvent);
        }

        private async void Start()
        {
            await UniTask.WaitUntil(() => constructionComplete);
            webViewUriAndPathHandler.InitializeAfterStart();
            webViewUserAgentHandler.SetUserAgent();
            uniWebView.Show();
        }

        // --- Event Handlers:

        /// <summary>
        /// Prevent the WebView from being closed by a "go back" or "closing" operation.
        /// </summary>
        private bool PreventAndroidBackButtonClosingWebView(UniWebView webView) => false;

        private void OnMinimumLogLevelEvent(MinimumLogLevelWebViewToUnityEvent @event)
            => CloudLogger.UserMinimumLogLevel = @event.MinimumLogLevel;

        // --- Public Methods:

        /// <summary>
        ///     Set the base URI, to be treated as the base of all loaded paths.
        ///     See <see cref="WebViewUriAndPathHandler.SetBaseUri"/>
        /// </summary>
        public void SetBaseUri(Uri uri) => webViewUriAndPathHandler.SetBaseUri(uri);

        /// <summary>
        ///     Load the specified path, appended to the base URI.
        ///     See <see cref="WebViewUriAndPathHandler.LoadPath(string)"/>
        /// </summary>
        /// <param name="path">
        ///     The path to load.
        /// </param>
        public void LoadPath(string path = "") => webViewUriAndPathHandler.LoadPath(path);

        /// <summary>
        ///     Reload the current page.
        ///     See <see cref="UniWebViewComponent.Reload"/>
        /// </summary>
        public void Reload() => uniWebView.Reload();
    }
}