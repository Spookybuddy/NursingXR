namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity
{
    /// <summary>
    /// An interface for deserializing an event from the WebView to a C# object.
    /// </summary>
    /// <typeparam name="TWebViewEvent">The type of event.</typeparam>
    public interface IWebViewToUnityEventSerializer<out TWebViewEvent> where TWebViewEvent : IWebViewToUnityEvent
    {
        TWebViewEvent Deserialize(string data);
    }
}