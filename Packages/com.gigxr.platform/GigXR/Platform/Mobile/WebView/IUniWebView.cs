using System;

namespace GIGXR.Platform.Mobile.WebView
{
    /// <summary>
    /// A wrapper interface around UniWebView for decoupling and testing purposes. Add methods here that map directly to
    /// UniWebView when needed.
    /// </summary>
    public interface IUniWebView
    {
        event UniWebView.MessageReceivedDelegate OnMessageReceived;
        event UniWebView.ShouldCloseDelegate OnShouldClose;

        void EvaluateJavaScript(string jsString, Action<UniWebViewNativeResultPayload> completionHandler = null);
        void Load(string url, bool skipEncoding = false, string readAccessURL = null);
        void Reload();
        void Show();
        void SetUserAgent(string agent);
    }
}