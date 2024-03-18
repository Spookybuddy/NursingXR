namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events
{
    /// <summary>
    /// An event for when the notification (iOS) dialog has been dismissed.
    /// </summary>
    public class NotificationDialogDismissedUnityToWebViewEvent : BaseUnityToWebViewEvent
    {
    }

    public class NotificationDialogDismissedUnityToWebViewEventSerializer
        : IUnityToWebViewEventSerializer<NotificationDialogDismissedUnityToWebViewEvent>
    {
        public string Serialize(NotificationDialogDismissedUnityToWebViewEvent @event)
        {
            return "";
        }
    }
}