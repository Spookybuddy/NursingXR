namespace GIGXR.Platform.Mobile.WebView.Components
{
    /// <summary>
    /// An interface for queueing a snackbar message to be sent to the WebView the next time that is possible.
    /// </summary>
    public interface IWebViewSnackbarComponent
    {
        void QueueSnackbarMessage(string message);
    }
}