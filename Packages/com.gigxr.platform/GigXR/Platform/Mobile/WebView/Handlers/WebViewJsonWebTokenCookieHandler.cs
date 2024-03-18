using GIGXR.Platform.Mobile.WebView.EventBus;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;

namespace GIGXR.Platform.Mobile.WebView.Handlers
{
    using GIGXR.Platform.Core.Settings;
    using Interfaces;
    using System;

    /// <summary>
    /// Responsible for restoring a persisted JsonWebToken as a cookie to the WebView.
    /// </summary>
    public class WebViewJsonWebTokenCookieHandler : IDisposable
    {
        private readonly IAuthenticationManager authenticationManager;
        private readonly AuthenticationProfile profile;
        private readonly IWebViewEventBus webViewEventBus;

        private const string PLAYER_PREFS_JWT_KEY = "gigxr-json-web-token";

        public WebViewJsonWebTokenCookieHandler(
            IAuthenticationManager authenticationManager,
            AuthenticationProfile profile,
            IWebViewEventBus webViewEventBus)
        {
            this.authenticationManager = authenticationManager;
            this.profile = profile;
            this.webViewEventBus = webViewEventBus;

            webViewEventBus.Subscribe<JsonWebTokenWebViewToUnityEvent>(OnJsonWebTokenWebViewToUnityEvent);
            webViewEventBus.Subscribe<LogoutWebViewToUnityEvent>(OnLogoutWebViewToUnityEvent);
        }

        public void RestoreJsonWebTokenCookie()
        {
            if (UnityEngine.PlayerPrefs.HasKey(PLAYER_PREFS_JWT_KEY))
            {
                string jwt = UnityEngine.PlayerPrefs.GetString(PLAYER_PREFS_JWT_KEY);
                SetCookie(profile.GmsUrl(), $"jwt={jwt}");
            }
            else
            {
                SetCookie(profile.GmsUrl(), "gigxr-unity=1");
            }
        }

        protected virtual void SetCookie(string url, string cookie) => UniWebView.SetCookie(url, cookie);

        protected virtual void SaveCookie(string jwt) => UnityEngine.PlayerPrefs.SetString(PLAYER_PREFS_JWT_KEY, jwt);

        protected virtual void DeleteSavedCookie() => UnityEngine.PlayerPrefs.DeleteKey(PLAYER_PREFS_JWT_KEY);

        private void OnJsonWebTokenWebViewToUnityEvent(JsonWebTokenWebViewToUnityEvent @event) => SaveCookie(@event.JsonWebToken);

        private void OnLogoutWebViewToUnityEvent(LogoutWebViewToUnityEvent @event) => DeleteSavedCookie();

        public void Dispose()
        {
            webViewEventBus.Unsubscribe<JsonWebTokenWebViewToUnityEvent>(OnJsonWebTokenWebViewToUnityEvent);
            webViewEventBus.Unsubscribe<LogoutWebViewToUnityEvent>(OnLogoutWebViewToUnityEvent);
        }
    }
}