namespace GIGXR.Platform.Mobile.GMS
{
    public static class GmsMessageAction
    {
        // GMS => Unity
        public const string JsonWebToken = "json-web-token";
        public const string CreateSession = "create-session";
        public const string JoinSession = "join-session";
        public const string JoinDemoSession = "join-demo-session";
        public const string GetDownloads = "get-downloads";
        public const string QueueDownloads = "queue-downloads";
        public const string QueueDownloadSet = "queue-download-set";
        public const string DeleteDownload = "delete-download";
        public const string StopDownload = "stop-download";
        public const string GetContentPreferences = "get-content-preferences";
        public const string SetContentPreferences = "set-content-preferences";
        public const string EnableNotifications = "enable-notifications";
        public const string FirstTimeExperienceFinished = "first-time-experience-finished";
        public const string Logout = "logout";
        public const string MinimumLogLevel = "minimum-log-level";
        public const string GetEnvironmentCredentials = "get-environment-credentials";
        public const string SwitchEnvironment = "switch-environment";

        // Unity => GMS
        public const string DownloadSetFailedInsufficientStorage = "download-set-failed-insufficient-storage";
        public const string CannotEvictNeededClips = "cannot-evict-needed-clips";
        public const string GetDownloadsResponse = "get-downloads-response";
        public const string ContentPreferencesResponse = "content-preferences-response";
        public const string GetEnvironmentCredentialsResponse = "environment-credentials";
        public const string NotificationDialogDismissed = "notification-dialog-dismissed";
        public const string SnackbarMessage = "snackbar-message";
        public const string Log = "log";
    }
}