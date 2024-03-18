using GIGXR.Platform.Core.EventBus;

namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView
{
    /// <summary>
    /// A Unity to WebView event.
    /// </summary>
    public interface IUnityToWebViewEvent : IGigEvent<WebViewEventBus> // TODO - how to set this one
    {
    }
}