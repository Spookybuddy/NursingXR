using GIGXR.Platform.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.AppEvents.Events.UI
{
    /// <summary>
    /// An event to show a timed prompt.
    /// </summary>
    public class ShowTimedPromptEvent : BasePromptEvent
    {
        /// <summary>
        /// The amount of time the prompt is visible, in milliseconds.
        /// </summary>
        public int TimeDelayMilliSeconds { get; }

        /// <summary>
        /// Display a timed prompt with an explicit time, in milliseconds, but with a default time of 5000 ms
        /// 
        /// For argument information, see
        /// <see cref="BasePromptEvent.BasePromptEvent(string, List{ButtonPromptInfo}, bool, PromptManager.WindowStates, Transform, Action)"/>
        /// </summary>
        [Obsolete("Use the constructor with UIPlacementData")]
        public ShowTimedPromptEvent(string mainText, 
                                    List<ButtonPromptInfo> buttons, 
                                    PromptManager.WindowStates windowWidth,
                                    Transform hostTransform,
                                    int timeInMilliSeconds = 5000) : base("", mainText, buttons, windowWidth, hostTransform)
        {
            TimeDelayMilliSeconds = timeInMilliSeconds;
        }

        public ShowTimedPromptEvent(string mainText,
                                    List<ButtonPromptInfo> buttons,
                                    UIPlacementData placementData = null,
                                    int timeInMilliSeconds = 5000) : base("", mainText, buttons, placementData)
        {
            TimeDelayMilliSeconds = timeInMilliSeconds;
        }

        /// TimedPrompt with a Header
        public ShowTimedPromptEvent(string header,
                                    string mainText,
                                    List<ButtonPromptInfo> buttons,
                                    UIPlacementData placementData = null,
                                    int timeInMilliSeconds = 5000) : base(header, mainText, buttons, placementData)
        {
            TimeDelayMilliSeconds = timeInMilliSeconds;
        }
    }
}