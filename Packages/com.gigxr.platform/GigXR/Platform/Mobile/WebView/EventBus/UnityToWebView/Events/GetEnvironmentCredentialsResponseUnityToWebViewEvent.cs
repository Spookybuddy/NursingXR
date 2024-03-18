using System.Collections.Generic;
using System.Text;

namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events
{
    /// <summary>
    /// An event to sent to the WebView with the current environment switching credentials.
    /// </summary>
    public class GetEnvironmentCredentialsResponseUnityToWebViewEvent : BaseUnityToWebViewEvent
    {
        public Dictionary<string, string> EnvironmentCredentials { get; }

        public GetEnvironmentCredentialsResponseUnityToWebViewEvent(Dictionary<string, string> environmentCredentials)
        {
            EnvironmentCredentials = environmentCredentials;
        }
    }

    public class GetEnvironmentCredentialsResponseUnityToWebViewEventSerializer
        : IUnityToWebViewEventSerializer<GetEnvironmentCredentialsResponseUnityToWebViewEvent>
    {
        public string Serialize(GetEnvironmentCredentialsResponseUnityToWebViewEvent @event)
        {
            var jsArray = new StringBuilder();
            jsArray.Append("[");

            foreach (var pair in @event.EnvironmentCredentials)
            {
                jsArray.Append($"['{pair.Key}', '{pair.Value}'],");
            }

            jsArray.Append("]");

            return jsArray.ToString();
        }
    }
}