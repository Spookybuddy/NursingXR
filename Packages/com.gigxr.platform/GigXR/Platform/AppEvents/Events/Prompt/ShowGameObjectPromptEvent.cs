using GIGXR.Platform.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.AppEvents.Events.UI
{
    /// <summary>
    /// An event to show a prompt accompanied by an instance of a prefab.
    /// E.g. we show a GigXR logo when users are waiting for the
    /// host to reconnect.
    /// </summary>
    public class ShowGameObjectPromptEvent : BasePromptEvent
    {
        /// <summary>
        /// The prefab to be instantiated alongside the prompt.
        /// </summary>
        public GameObject ObjectPrompt { get; }

        /// <summary>
        /// Generic constructor to show a prompt alongside a GameObject.
        /// For info about other arguments, see
        /// <see cref="BasePromptEvent.BasePromptEvent(string, List{ButtonPromptInfo}, bool, PromptManager.WindowStates, Transform, Action)"/>
        /// </summary>
        /// <param name="objectToEnable">
        /// The prefab to instantiate alongside the prompt.
        /// No assumptions are made about this prefabs desired behavior.
        /// Any behavior (e.g. gaze tracking) should be handled by the prefab.
        /// </param>
        public ShowGameObjectPromptEvent(GameObject objectToEnable, 
                                         string mainText, 
                                         List<ButtonPromptInfo> buttons,  
                                         PromptManager.WindowStates windowWidth, 
                                         Transform hostTransform = null) : base("", mainText, buttons, windowWidth, hostTransform)
        {
            ObjectPrompt = objectToEnable;
        }

        /// <summary>
        /// Constructor using default values for generic prompt data.
        /// Show a prompt and an instance of the specified prefab.
        /// </summary>
        /// <param name="objectToEnable">
        /// The prefab to instantiate alongside the prompt.
        /// No assumptions are made about this prefabs desired behavior.
        /// Any behavior (e.g. gaze tracking) should be handled by the prefab.
        /// </param>
        public ShowGameObjectPromptEvent(GameObject objectToEnable) : base()
        {
            ObjectPrompt = objectToEnable;
        }
    }

    /// <summary>
    /// An event to destroy a prompt associated with an instantiated
    /// prefab via <see cref="ShowGameObjectPromptEvent"/>
    /// </summary>
    public class HideGameObjectPromptEvent : BasePromptEvent
    {
        /// <summary>
        /// The prefab that was instantiated alongside the prompt;
        /// used as a key to find the prompt and instance.
        /// </summary>
        public GameObject ObjectPrompt { get; }

        /// <summary>
        /// Constructor to destroy a prompt and instance associated
        /// with the specified prefab.
        /// </summary>
        /// <param name="objectToEnable">
        /// The prefab that was instantiated by the preceeding <see cref="ShowGameObjectPromptEvent"/>
        /// </param>
        public HideGameObjectPromptEvent(GameObject objectToEnable) : base()
        {
            ObjectPrompt = objectToEnable;
        }
    }
}