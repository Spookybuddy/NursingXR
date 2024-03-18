namespace GIGXR.Platform.HMD
{
    using Microsoft.MixedReality.Toolkit.Input;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Helper class that allows you to register a speech keyword on a MonoBehavior without using the MRTK Interactable.
    /// </summary>
    public class MixedRealitySpeechMonoBehavior : MonoBehaviour, IMixedRealitySpeechHandler
    {
        public string speechKeyword;

        public UnityEvent speechKeywordSaidEvent;

        void Start()
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
        }

        void OnApplicationQuit()
        {
            Microsoft.MixedReality.Toolkit.CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySpeechHandler>(this);
        }

        // --- Interface Implementations:

        public void OnSpeechKeywordRecognized(SpeechEventData eventData)
        {
            if (eventData.Command.Keyword.ToLower() == speechKeyword.ToLower())
            {
                speechKeywordSaidEvent?.Invoke();
            }
        }
    }
}