using GIGXR.Platform.Mobile.GMS;
using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;

namespace GIGXR.Platform.Mobile.WebView.EventBus
{   
    /// <summary>
    /// Responsible for registering the WebView events with the event bus.
    ///
    /// This could use automatic registration in the future.
    /// </summary>
    public class WebViewEventRegistrar
    {
        private readonly IWebViewEventBus eventBus;

        public WebViewEventRegistrar(IWebViewEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void RegisterEvents()
        {
            RegisterUnityToWebViewEvents();
            RegisterWebViewToUnityEvents();
        }

        private void RegisterUnityToWebViewEvents()
        {
            eventBus
                .RegisterUnityToWebViewEvent<DownloadSetFailedInsufficientStorageUnityToWebViewEvent,
                    DownloadSetFailedInsufficientStorageUnityToWebViewEventSerializer>(
                    GmsMessageAction.DownloadSetFailedInsufficientStorage);

            eventBus
                .RegisterUnityToWebViewEvent<CannotEvictNeededResourcesUnityToWebViewEvent,
                    CannotEvictNeededResourcesUnityToWebViewEventSerializer>(GmsMessageAction.CannotEvictNeededClips);

            eventBus
                .RegisterUnityToWebViewEvent<GetDownloadsResponseUnityToWebViewEvent,
                    GetDownloadsResponseUnityToWebViewEventSerializer>(GmsMessageAction.GetDownloadsResponse);

            eventBus
                .RegisterUnityToWebViewEvent<ContentPreferencesResponseUnityToWebViewEvent,
                    ContentPreferencesResponseUnityToWebViewEventSerializer>(
                    GmsMessageAction.ContentPreferencesResponse);

            eventBus
                .RegisterUnityToWebViewEvent<NotificationDialogDismissedUnityToWebViewEvent,
                    NotificationDialogDismissedUnityToWebViewEventSerializer>(GmsMessageAction
                    .NotificationDialogDismissed);

            eventBus
                .RegisterUnityToWebViewEvent<GetEnvironmentCredentialsResponseUnityToWebViewEvent,
                    GetEnvironmentCredentialsResponseUnityToWebViewEventSerializer>(GmsMessageAction
                    .GetEnvironmentCredentialsResponse);

            eventBus
                .RegisterUnityToWebViewEvent<SnackbarUnityToWebViewEvent,
                    SnackbarWebViewMessageEventSerializer>(GmsMessageAction.SnackbarMessage);
        }

        private void RegisterWebViewToUnityEvents()
        {
            eventBus
                .RegisterWebViewToUnityEvent<CreateSessionWebViewToUnityEvent, CreateSessionWebViewEventSerializer>(
                    GmsMessageAction.CreateSession);

            eventBus
                .RegisterWebViewToUnityEvent<DeleteDownloadWebViewToUnityEvent, DeleteDownloadWebViewEventSerializer>(
                    GmsMessageAction.DeleteDownload);

            eventBus
                .RegisterWebViewToUnityEvent<EnableNotificationsWebViewToUnityEvent,
                    EnableNotificationsWebViewEventSerializer>(GmsMessageAction.EnableNotifications);

            eventBus
                .RegisterWebViewToUnityEvent<FirstTimeExperienceFinishedWebViewToUnityEvent,
                    FirstTimeExperienceFinishedWebViewEventSerializer>(GmsMessageAction.FirstTimeExperienceFinished);

            eventBus
                .RegisterWebViewToUnityEvent<GetContentPreferencesWebViewToUnityEvent,
                    GetContentPreferencesWebViewEventSerializer>(GmsMessageAction.GetContentPreferences);

            eventBus
                .RegisterWebViewToUnityEvent<GetDownloadsWebViewToUnityEvent, GetDownloadsWebViewEventSerializer>(
                    GmsMessageAction.GetDownloads);

            eventBus
                .RegisterWebViewToUnityEvent<GetEnvironmentCredentialsWebViewToUnityEvent,
                    GetEnvironmentCredentialsWebViewEventSerializer>(GmsMessageAction.GetEnvironmentCredentials);

            eventBus
                .RegisterWebViewToUnityEvent<JoinDemoSessionWebViewToUnityEvent, JoinDemoSessionWebViewEventSerializer>(
                    GmsMessageAction.JoinDemoSession);

            eventBus
                .RegisterWebViewToUnityEvent<JoinSessionWebViewToUnityEvent, JoinSessionWebViewEventSerializer>(
                    GmsMessageAction.JoinSession);

            eventBus
                .RegisterWebViewToUnityEvent<JsonWebTokenWebViewToUnityEvent, JsonWebTokenWebViewEventSerializer>(
                    GmsMessageAction.JsonWebToken);

            eventBus
                .RegisterWebViewToUnityEvent<LogoutWebViewToUnityEvent, LogoutWebViewEventSerializer>(GmsMessageAction
                    .Logout);

            eventBus
                .RegisterWebViewToUnityEvent<MinimumLogLevelWebViewToUnityEvent, MinimumLogLevelWebViewEventSerializer>(
                    GmsMessageAction.MinimumLogLevel);

            eventBus
                .RegisterWebViewToUnityEvent<QueueDownloadSetWebViewToUnityEvent, QueueDownloadSetWebViewEventSerializer
                >(GmsMessageAction.QueueDownloadSet);

            eventBus
                .RegisterWebViewToUnityEvent<QueueDownloadsWebViewToUnityEvent, QueueDownloadsWebViewEventSerializer>(
                    GmsMessageAction.QueueDownloads);

            eventBus
                .RegisterWebViewToUnityEvent<SetContentPreferencesWebViewToUnityEvent,
                    SetContentPreferencesWebViewEventSerializer>(GmsMessageAction.SetContentPreferences);

            eventBus
                .RegisterWebViewToUnityEvent<StopDownloadWebViewToUnityEvent, StopDownloadWebViewEventSerializer>(
                    GmsMessageAction.StopDownload);

            eventBus
                .RegisterWebViewToUnityEvent<SwitchEnvironmentWebViewToUnityEvent,
                    SwitchEnvironmentWebViewEventSerializer>(GmsMessageAction.SwitchEnvironment);
        }
    }
}