using GIGXR.Platform.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.AppEvents.Events.UI
{
    /// <summary>
    /// An event published to show a prompt.
    /// </summary>
    public class ShowPromptEvent : BasePromptEvent
    {
        [Obsolete("Use one of the constructors that utilizes the UIPlacementData input.")]
        public ShowPromptEvent(string mainText, 
                               List<ButtonPromptInfo> buttons,  
                               PromptManager.WindowStates windowWidth,
                               Transform hostTransform) : base("", mainText, buttons, windowWidth, hostTransform)
        {
        }

        [Obsolete("Use one of the constructors that utilizes the UIPlacementData input and no WindowStates input.")]
        public ShowPromptEvent(string mainText,
                               List<ButtonPromptInfo> buttons,
                               PromptManager.WindowStates windowWidth,
                               UIPlacementData placementData = null) : base("", mainText, buttons, windowWidth, placementData)
        {
        }

        public ShowPromptEvent(string mainText,
                               List<ButtonPromptInfo> buttons,
                               UIPlacementData placementData = null) : base("", mainText, buttons, placementData)
        {
        }

        /// <summary>
        /// Prompt with a header
        /// </summary>
        public ShowPromptEvent(string headerText,
                               string mainText,
                               List<ButtonPromptInfo> buttons,
                               UIPlacementData placementData = null) : base(headerText, mainText, buttons, placementData)
        {
        }
    }
}