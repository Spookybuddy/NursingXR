using GIGXR.Platform.Mobile.WebView.EventBus;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;
using GIGXR.Platform.Mobile.AppEvents.Events.AR;

namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    using Platform.AppEvents.Events.Session;
    using GIGXR.Platform.AppEvents;
    using Interfaces;
    using Sessions;
    using System;
    using GIGXR.Platform.UI;
    using GIGXR.Platform.Mobile.AppEvents.Events.UI;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Responsible for managing the WebView's session functionality.
    /// </summary>
    public class WebViewSessionHandler : IDisposable
    {
        private UiEventBus UiEventBus { get; }
        private AppEventBus EventBus { get; }
        private ICalibrationManager CalibrationManager { get; }
        private ISessionManager SessionManager { get; }
        private IWebViewController WebViewController { get; }
        private IWebViewEventBus WebViewEventBus { get; }

        private Guid joiningSessionId;

        private bool isDemoSession = false;

        public WebViewSessionHandler(UiEventBus uiEventBus,
                                     AppEventBus appEventBus,
                                     ICalibrationManager calibrationManager,
                                     ISessionManager sessionManager,
                                     IWebViewController webViewController,
                                     IWebViewEventBus webViewEventBus)
        {
            UiEventBus = uiEventBus;
            EventBus = appEventBus;
            CalibrationManager = calibrationManager;
            SessionManager = sessionManager;
            WebViewController = webViewController;
            WebViewEventBus = webViewEventBus;

            WebViewEventBus.Subscribe<CreateSessionWebViewToUnityEvent>(OnCreateSessionWebViewToUnityEvent);
            WebViewEventBus.Subscribe<JoinSessionWebViewToUnityEvent>(OnJoinSessionWebViewToUnityEvent);
            WebViewEventBus.Subscribe<JoinDemoSessionWebViewToUnityEvent>(OnJoinDemoSessionWebViewToUnityEvent);

            EventBus.Subscribe<LeftSessionEvent>(OnLeftSessionEvent);
            EventBus.Subscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
            EventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
            EventBus.Subscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
        }

        private void OnCreateSessionWebViewToUnityEvent(CreateSessionWebViewToUnityEvent @event)
        {
            // Do we support session creation from mobile?? NOPE
            //sessionManager.StartSessionAsync(@event.SessionId);

            //appEventBus.Publish<ArStartScanningEvent>
            //(
            //    new ArStartScanningEvent(@event.SessionId)
            //);
        }

        private void OnJoinSessionWebViewToUnityEvent(JoinSessionWebViewToUnityEvent @event)
        {
            UnityEngine.Debug.Log($"JoinSessionWebViewToUnityEvent {@event.SessionId}");
            
            isDemoSession = false;

            joiningSessionId = @event.SessionId;

            // Start the mobile calibration process
            CalibrationManager.StartCalibration(ICalibrationManager.CalibrationModes.Manual);
        }

        private void OnJoinDemoSessionWebViewToUnityEvent(JoinDemoSessionWebViewToUnityEvent @event)
        {
            UnityEngine.Debug.Log($"JoinDemoSessionWebViewToUnityEvent {@event.SessionId}");

            isDemoSession = true;

            joiningSessionId = @event.SessionId;
        }

        private void OnLeftSessionEvent(LeftSessionEvent @event)
        {
            WebViewController.LoadPath();
        }

        private void OnReturnToSessionListEvent(ReturnToSessionListEvent @event)
        {
            WebViewController.LoadPath();
        }

        private void OnJoinedSessionEvent(JoinedSessionEvent @event)
        {
            UpdateUserDevice().Forget();
        }

        public async UniTaskVoid UpdateUserDevice()
        {
            // Mobile users select the InClass/Remote prompt in the UniWebView, so we must query the
            // GMS API endpoint to get this value
            var t = await SessionManager.ApiClient.SessionsApi.GetParticipantStatusAsync
                (
                    SessionManager.ActiveSession.SessionId,
                    SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId
                );

            EventBus.Publish(new SetUserLocationLocalEvent(t.SessionParticipantStatus));
        }

        private async void OnArTargetPlacedEvent(ArTargetPlacedEvent @event)
        {
            if (joiningSessionId != Guid.Empty)
            {
                var joinSuccess = await SessionManager.JoinSessionAsync(joiningSessionId, isDemoSession);

                if(!joinSuccess.Item1)
                {
                    UnityEngine.Debug.LogError($"Issues while trying to join session: {joinSuccess.Item2}");

                    EventBus.Publish(new ReturnToSessionListEvent());
                }
                else
                {
                    // Bring up the session screen so that the joining prompts may
                    UiEventBus.Publish(new SwitchingActiveScreenEventMobile(UI.BaseScreenObjectMobile.ScreenTypeMobile.Session, UI.BaseScreenObjectMobile.ScreenTypeMobile.WebView));
                }
            }

            joiningSessionId = Guid.Empty;
        }

        public void Dispose()
        {
            WebViewEventBus.Unsubscribe<CreateSessionWebViewToUnityEvent>(OnCreateSessionWebViewToUnityEvent);
            WebViewEventBus.Unsubscribe<JoinSessionWebViewToUnityEvent>(OnJoinSessionWebViewToUnityEvent);
            WebViewEventBus.Unsubscribe<JoinDemoSessionWebViewToUnityEvent>(OnJoinDemoSessionWebViewToUnityEvent);

            EventBus.Unsubscribe<LeftSessionEvent>(OnLeftSessionEvent);
            EventBus.Unsubscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
            EventBus.Unsubscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
            EventBus.Unsubscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
        }
    }
}
