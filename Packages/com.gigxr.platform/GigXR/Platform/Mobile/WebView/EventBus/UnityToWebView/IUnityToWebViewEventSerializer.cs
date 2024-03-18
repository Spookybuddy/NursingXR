namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView
{
    /// <summary>
    /// An interface for serializing a C# object to be sent to the WebView.
    /// </summary>
    /// <typeparam name="TWebViewEvent">The type of event.</typeparam>
    public interface IUnityToWebViewEventSerializer<in TWebViewEvent> where TWebViewEvent : IUnityToWebViewEvent
    {
        string Serialize(TWebViewEvent @event);
    }
}