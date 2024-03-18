namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events
{
    /// <summary>
    /// An event to send to The WebView to instruct it to display a snackbar message.
    /// </summary>
    public class SnackbarUnityToWebViewEvent : BaseUnityToWebViewEvent
    {
        public SnackbarUnityToWebViewEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public class SnackbarWebViewMessageEventSerializer : IUnityToWebViewEventSerializer<SnackbarUnityToWebViewEvent>
    {
        public string Serialize(SnackbarUnityToWebViewEvent @event)
        {
            return $"'{@event.Message}'";
        }
    }
}