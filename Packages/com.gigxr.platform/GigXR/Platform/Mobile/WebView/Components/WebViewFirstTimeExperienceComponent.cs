using GIGXR.Platform.Mobile.WebView.EventBus;
using GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity.Events;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.Components
{
    /// <summary>
    /// Responsible for managing the first time experience state of the WebView.
    /// </summary>
    [DisallowMultipleComponent]
    public class WebViewFirstTimeExperienceComponent : MonoBehaviour, IWebViewFirstTimeExperienceComponent
    {
        private IWebViewEventBus EventBus { get; set; }

        public bool SkipFirstTimeExperience
        {
            get => PlayerPrefs.GetInt("gigxr-skip-first-time-experience", 0) == 1;
            set => PlayerPrefs.SetInt("gigxr-skip-first-time-experience", value ? 1 : 0);
        }

        public void InitializeAfterOnEnable(IWebViewEventBus eventBus)
        {
            EventBus = eventBus;

            EventBus.Subscribe<FirstTimeExperienceFinishedWebViewToUnityEvent>(OnFirstTimeExperienceFinishedEvent);
        }

        private void OnApplicationQuit()
        {
            EventBus.Unsubscribe<FirstTimeExperienceFinishedWebViewToUnityEvent>(OnFirstTimeExperienceFinishedEvent);
        }

        private void OnFirstTimeExperienceFinishedEvent(FirstTimeExperienceFinishedWebViewToUnityEvent @event)
        {
            SkipFirstTimeExperience = true;
        }
    }
}