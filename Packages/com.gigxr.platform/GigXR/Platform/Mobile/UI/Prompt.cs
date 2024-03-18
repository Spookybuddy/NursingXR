using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using GIGXR.Platform.Managers;
using System.Collections.Generic;
using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Core.DependencyInjection;

namespace GIGXR.Platform.Mobile.UI
{
    using UnityEngine.UI;

    /// <summary>
    /// Can be attached to a gameobject with a Button - Will trigger an awaited prompt that invokes Unity events
    /// assigned in the inspector
    /// </summary>
    // [RequireComponent(typeof(ButtonComponent))]
    [RequireComponent(typeof(Button))]
    public class Prompt : MonoBehaviour
    {
        /// <summary>
        /// The prompt message
        /// </summary>
        [Space]
        [Header("Prompt properties")]
        public string promptMessage = "Are you sure you wish to leave the session?";

        /// <summary>
        /// The text on the confirmation button
        /// </summary>
        public string ConfirmText = "Ok";

        /// <summary>
        /// The text on the decline button
        /// </summary>
        public string DeclineText = "Cancel";

        /// <summary>
        /// A collection of Unity Events assigned in the inspector
        /// </summary>
        [Space]
        [Header("Unity Events")]
        public Prompts Prompts;

        /// <summary>
        /// The button to click to initialise the prompt - must be attached to the same gameobject
        /// </summary>
        // private ButtonComponent buttonComponent;
        private Button buttonComponent;

        private AppEventBus EventBus { get; set; }

        [InjectDependencies]
        public void Construct(AppEventBus eventBus)
        {
            EventBus = eventBus;
        }

        /// <summary>
        /// Subscribe the button to TriggerPrompt
        /// </summary>
        private void Awake()
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());

            buttonComponent         =  GetComponent<Button>();
            buttonComponent.onClick.AddListener(TriggerShowAwaitedBoolPrompt); // += TriggerShowAwaitedBoolPrompt;

            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        private void InvokeAcceptPrompt()
        {
            Prompts.acceptEvents.Invoke();
        }

        private void InvokeDeclinePrompt()
        {
            Prompts.declineEvents.Invoke();
        }

        /// <summary>
        /// Trigger an awaited prompt with that will invoke unity events from the Prompts class depending on the clicked button
        /// </summary>
        private void TriggerShowAwaitedBoolPrompt()
        {
            var okButton = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = ConfirmText,
                    onPressAction = () =>
                    {
                        InvokeAcceptPrompt();
                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = DeclineText,
                    onPressAction = () =>
                    {
                        InvokeDeclinePrompt();
                    }
                }
            };

            EventBus.Publish
            (
                new ShowPromptEvent(promptMessage, okButton, PromptManager.WindowStates.Wide)
            );
        }
    }

    [Serializable]
    public class Prompts
    {
        public UnityEvent acceptEvents;
        public UnityEvent declineEvents;
    }
}
