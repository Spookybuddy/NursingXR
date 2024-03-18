using GIGXR.Platform.Managers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GIGXR.Platform.AppEvents.Events.UI
{
    /// <summary>
    /// An event published to show a cancelate prompt.
    /// </summary>
    public class ShowCancellablePromptEvent : BasePromptEvent
    {
        /// <summary>
        /// The cancellation token passed in when this event is constructed.
        /// Can be used to cancel the prompt.
        /// </summary>
        public CancellationToken Token { get; }

        /// <summary>
        /// See <see cref="BasePromptEvent.BasePromptEvent(string, List{ButtonPromptInfo}, bool, PromptManager.WindowStates, Transform, Action)"/>
        /// </summary>
        public ShowCancellablePromptEvent(CancellationToken token, 
                                          string mainText, 
                                          List<ButtonPromptInfo> buttons, 
                                          PromptManager.WindowStates windowWidth, 
                                          Transform hostTransform) : base("", mainText, buttons, windowWidth, hostTransform)
        {
            Token = token;
        }

        public ShowCancellablePromptEvent(CancellationToken token,
                                          string mainText,
                                          List<ButtonPromptInfo> buttons,
                                          PromptManager.WindowStates windowWidth,
                                          UIPlacementData placementData = null) : base("", mainText, buttons, windowWidth, placementData)
        {
            Token = token;
        }

        public ShowCancellablePromptEvent(CancellationToken token,
                                          string headerText,
                                          string mainText,
                                          List<ButtonPromptInfo> buttons,
                                          UIPlacementData placementData = null) : base(headerText, mainText, buttons, placementData)
        {
            Token = token;
        }
    }
}