using System;
using GIGXR.Platform.Utilities;
using GIGXR.Platform.Mobile.WebView.Components;
using GIGXR.Platform.Core.Settings;

namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    /// <summary>
    /// Responsible for managing the Uri and path of UniWebView.
    /// </summary>
    public class WebViewUriAndPathHandler
    {
        private readonly IUniWebView uniWebView;
        private readonly IWebViewFirstTimeExperienceComponent webViewFirstTimeExperienceComponent;
        private readonly AuthenticationProfile profile;

        private Uri baseUri;
        private string bufferedPath;

        public WebViewUriAndPathHandler(
            IUniWebView uniWebView,
            IWebViewFirstTimeExperienceComponent webViewFirstTimeExperienceComponent,
            AuthenticationProfile profile)
        {
            this.uniWebView = uniWebView;
            this.webViewFirstTimeExperienceComponent = webViewFirstTimeExperienceComponent;
            this.profile = profile;
        }

        /// <summary>
        ///     False until this component has been initialized during <see cref="WebViewController.Start"/>.
        ///     While false, attempts to load paths will instead buffer the path, for loading
        ///     after initialization.
        /// </summary>
        public bool WebViewLoaded { get; private set; }

        /// <summary>
        ///     This method serves the purpose that Unity's "Start" method serves for Monobehaviours.
        ///     It is called by <see cref="WebViewController.Start"/>
        /// </summary>
        public void InitializeAfterStart()
        {
            WebViewLoaded = true;

            // Our SDK Sandbox environment does not match the QA/Prod and uses an additional subdomain for API access vs the front end
            // Check to see if this subdomain is used and if so, remove it
            if (profile.GmsUrl().Contains("api."))
            {
                var path = profile.GmsUrl().Replace("api.", "");

                SetBaseUri(new Uri(path));
            }
            else
            {
                SetBaseUri(new Uri(profile.GmsUrl()));
            }

            if (webViewFirstTimeExperienceComponent.SkipFirstTimeExperience)
            {
                LoadBufferedOrDefaultPath();
            }
            else
            {
                LoadPath("/first-time-experience");
            }
        }

        /// <summary>
        ///     Set the base URI; all loaded paths are treated as local paths from this URI.
        /// </summary>
        /// <param name="uri"></param>
        public void SetBaseUri(Uri uri)
        {
            uri = uri.AppendQueryString("&unity=1" +
#if DEVELOPMENT_BUILD
                                        // Enables development features on the GMS.
                                        "&developer=1" +
#endif
                                        "&clientAppId=" + profile.ApplicationId());
            baseUri = uri;
        }

        /// <summary>
        ///     Load the specified path, as a local path appended to the base URI.
        ///     If the web view has not loaded yet, buffer the path to load after
        ///     it finishes loading.
        ///     
        ///     After the full path is constructed from the base URI and the path
        ///     parameter, it is passed to <see cref="IUniWebView.Load(string)"/>
        /// </summary>
        /// <param name="path">
        ///     The path to load (after appending onto the base URI).
        ///     If this is omitted or empty, the base URI will be loaded.
        /// </param>
        public void LoadPath(string path = "")
        {
            if (!WebViewLoaded)
            {
                // If LoadPath() is called before Start(), buffer the path to be loaded later.
                bufferedPath = path;
                return;
            }

            var uri = baseUri.AppendPath(path);
            uniWebView.Load(uri.ToString());
        }

        /// <summary>
        ///     If a path was buffered by an attempt to load before web view was initialized,
        ///     load that path. Otherwise load the default path (the base URI).
        /// </summary>
        public void LoadBufferedOrDefaultPath()
        {
            if (bufferedPath != null)
            {
                LoadPath(bufferedPath);
                bufferedPath = null;
                return;
            }

            LoadPath();
        }
    }
}