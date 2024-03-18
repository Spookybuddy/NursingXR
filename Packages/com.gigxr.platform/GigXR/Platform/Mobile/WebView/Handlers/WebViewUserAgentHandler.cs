namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    /// <summary>
    /// Responsible for setting the UserAgent of UniWebView.
    /// </summary>
    public class WebViewUserAgentHandler
    {
        private readonly IUniWebView uniWebView;

        public WebViewUserAgentHandler(IUniWebView uniWebView)
        {
            this.uniWebView = uniWebView;
        }

        public void SetUserAgent()
        {
#if UNITY_IOS
            // This is needed for iPad compatibility. iPad seems to send a desktop user agent.
            uniWebView.SetUserAgent("GIGXR App (iPad/iPhone/iPod)");
#endif

            // TODO: Probably expand this to show the real app and version.
        }
    }
}