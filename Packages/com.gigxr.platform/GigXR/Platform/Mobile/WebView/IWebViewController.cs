using System;

namespace GIGXR.Platform.Mobile.WebView
{
    public interface IWebViewController
    {
        /// <summary>
        /// Sets the base URI for future path changes.
        /// </summary>
        /// <param name="uri">The Uri to use for a base path.</param>
        void SetBaseUri(Uri uri);

        /// <summary>
        /// Loads a path relative to the base URI.
        /// </summary>
        /// <param name="path">The path to load.</param>
        void LoadPath(string path = "");

        /// <summary>
        /// Reloads the current page.
        /// </summary>
        void Reload();
    }
}