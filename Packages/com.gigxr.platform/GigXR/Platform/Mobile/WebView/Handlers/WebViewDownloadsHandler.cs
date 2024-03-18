namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    using GIGXR.GMS.Models;
    using GIGXR.Platform.Mobile.WebView.Components;
    using GIGXR.Platform.Mobile.WebView.EventBus;
    using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events;
    using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;

    /// <summary>
    /// Responsible for managing the downloads functionality of the WebView.
    /// </summary>
    public class WebViewDownloadsHandler
    {
        private readonly IWebViewEventBus webViewEventBus;
        private readonly IWebViewSnackbarComponent webViewSnackbarComponent;
        
        // TODO: Switch to interface when implemented.
        // TODO: CU-250prkv - ContentManager
        // private readonly ContentManager contentManager;

        public WebViewDownloadsHandler(
            IWebViewEventBus webViewEventBus,
            IWebViewSnackbarComponent webViewSnackbarComponent)
            // ContentManager contentManager)
        {
            this.webViewEventBus = webViewEventBus;
            this.webViewSnackbarComponent = webViewSnackbarComponent;
            // this.contentManager = contentManager;
        }

        public void Enable()
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.DownloadSetFailedInsufficientStorage += DownloadSetFailedInsufficientStorageHandler;
            // contentManager.CannotEvictNeededResources += CannotEvictNeededResourcesHandler;
            // contentManager.DownloadComplete += OnDownloadComplete;

            webViewEventBus.Subscribe<GetDownloadsWebViewToUnityEvent>(OnGetDownloadsWebViewToUnityEvent);
            webViewEventBus.Subscribe<QueueDownloadsWebViewToUnityEvent>(OnQueueDownloadsWebViewToUnityEvent);
            webViewEventBus.Subscribe<QueueDownloadSetWebViewToUnityEvent>(OnQueueDownloadSetWebViewToUnityEvent);
            webViewEventBus.Subscribe<DeleteDownloadWebViewToUnityEvent>(OnDeleteDownloadWebViewToUnityEvent);
            webViewEventBus.Subscribe<StopDownloadWebViewToUnityEvent>(OnStopDownloadWebViewToUnityEvent);

            webViewEventBus.Subscribe<GetContentPreferencesWebViewToUnityEvent>(
                OnGetContentPreferencesWebViewToUnityEvent);
            webViewEventBus.Subscribe<SetContentPreferencesWebViewToUnityEvent>(
                OnSetContentPreferencesWebViewToUnityEvent);
        }

        public void Disable()
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.DownloadSetFailedInsufficientStorage -= DownloadSetFailedInsufficientStorageHandler;
            // contentManager.CannotEvictNeededResources -= CannotEvictNeededResourcesHandler;
            // contentManager.DownloadComplete -= OnDownloadComplete;

            webViewEventBus.Unsubscribe<GetDownloadsWebViewToUnityEvent>(OnGetDownloadsWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<QueueDownloadsWebViewToUnityEvent>(OnQueueDownloadsWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<QueueDownloadSetWebViewToUnityEvent>(OnQueueDownloadSetWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<DeleteDownloadWebViewToUnityEvent>(OnDeleteDownloadWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<StopDownloadWebViewToUnityEvent>(OnStopDownloadWebViewToUnityEvent);

            webViewEventBus.Unsubscribe<GetContentPreferencesWebViewToUnityEvent>(
                OnGetContentPreferencesWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<SetContentPreferencesWebViewToUnityEvent>(
                OnSetContentPreferencesWebViewToUnityEvent);
        }

        private void OnDownloadComplete(Resource resource)
        {
#if !UNITY_WSA_10_0
            // TODO: CU-250prkv - ContentManager
            // var availableStorage = contentManager.GetAvailableStorage();
            // var lowStorageWarning = NumberUtils.MegabytesToBytes(ProfileManager.Instance.Mobile.LowStorageWarning);
            // CloudLogger.LogInformation($"Available: {NumberUtils.GetBytesReadable(availableStorage)}, " +
            //                            "Threshold: {NumberUtils.GetBytesReadable(lowStorageWarning)}");
            // if (availableStorage < lowStorageWarning)
            // {
            //     var message =
            //         $"Memory is low, less than {NumberUtils.GetBytesReadable(lowStorageWarning)} of storage available.";
            //
            //     webViewSnackbarComponent.QueueSnackbarMessage(message);
            // }
#endif
        }

        private void DownloadSetFailedInsufficientStorageHandler()
        {
            webViewEventBus.RaiseUnityToWebViewEvent(new DownloadSetFailedInsufficientStorageUnityToWebViewEvent());
        }

        private void CannotEvictNeededResourcesHandler()
        {
            webViewEventBus.RaiseUnityToWebViewEvent(new CannotEvictNeededResourcesUnityToWebViewEvent());
        }

        private void OnGetDownloadsWebViewToUnityEvent(GetDownloadsWebViewToUnityEvent @event)
        { 
            // TODO: CU-250prkv - ContentManager
            // var downloads = contentManager.GetDownloads();

            // webViewEventBus.RaiseUnityToWebViewEvent(new GetDownloadsResponseUnityToWebViewEvent(downloads));
        }

        private void OnQueueDownloadsWebViewToUnityEvent(QueueDownloadsWebViewToUnityEvent @event)
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.QueueDownloads(@event.ResourceIds);
        }

        private void OnQueueDownloadSetWebViewToUnityEvent(QueueDownloadSetWebViewToUnityEvent @event)
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.QueueDownloadSet(@event.ResourceIds, @event.CanEvictNeededResources);
        }

        private void OnDeleteDownloadWebViewToUnityEvent(DeleteDownloadWebViewToUnityEvent @event)
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.DeleteDownload(@event.ResourceId);
        }

        private void OnStopDownloadWebViewToUnityEvent(StopDownloadWebViewToUnityEvent @event)
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.StopDownload(@event.ResourceId);
        }

        private void OnGetContentPreferencesWebViewToUnityEvent(GetContentPreferencesWebViewToUnityEvent @event)
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.MaximumContentStorage.TryGetValueGigabytes(out var gigabytes);
            // var allowAutomaticBackgroundDownloads = contentManager.AllowAutomaticBackgroundDownloads;
            // var eventToSend =
            //     new ContentPreferencesResponseUnityToWebViewEvent(allowAutomaticBackgroundDownloads, gigabytes);
            //
            // webViewEventBus.RaiseUnityToWebViewEvent(eventToSend);
        }

        private void OnSetContentPreferencesWebViewToUnityEvent(SetContentPreferencesWebViewToUnityEvent @event)
        {
            // TODO: CU-250prkv - ContentManager
            // contentManager.AllowAutomaticBackgroundDownloads = @event.AllowAutomaticBackgroundDownloads;
            // contentManager.MaximumContentStorage.TrySet(@event.MaximumContentStorage);
        }
    }
}