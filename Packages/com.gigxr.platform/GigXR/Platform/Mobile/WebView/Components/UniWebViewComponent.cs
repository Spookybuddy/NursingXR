using System;
using GIGXR.Platform.Core.DependencyValidator;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.Components
{
    /// <summary>
    /// A wrapper around UniWebView for decoupling and testing purposes. Add methods here that map directly to
    /// UniWebView when needed.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UniWebView))]
    public class UniWebViewComponent : MonoBehaviour, IUniWebView
    {
        [SerializeField, RequireDependency] private UniWebView uniWebView;

        public event UniWebView.MessageReceivedDelegate OnMessageReceived
        {
            add => uniWebView.OnMessageReceived += value;
            remove => uniWebView.OnMessageReceived -= value;
        }

        public event UniWebView.ShouldCloseDelegate OnShouldClose
        {
            add => uniWebView.OnShouldClose += value;
            remove => uniWebView.OnShouldClose -= value;
        }

        public void EvaluateJavaScript(string jsString, Action<UniWebViewNativeResultPayload> completionHandler = null)
            => uniWebView.EvaluateJavaScript(jsString, completionHandler);

        public void Load(string url, bool skipEncoding = false, string readAccessURL = null)
            => uniWebView.Load(url, skipEncoding, readAccessURL);

        public void Reload()
            => uniWebView.Reload();

        public void Show()
            => uniWebView.Show();

        public void SetUserAgent(string agent)
            => uniWebView.SetUserAgent(agent);
    }
}