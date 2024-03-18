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
    public class UpdateCancellablePromptEvent : BasePromptEvent
    {
        /// <summary>
        /// The cancellation token passed in when this event is constructed.
        /// Can be used to cancel the prompt.
        /// </summary>
        public CancellationToken Token { get; }

        public UpdateCancellablePromptEvent(CancellationToken token,
                                            string headerText = null,
                                            string mainText = null,
                                            List<ButtonPromptInfo> buttons = null,
                                            UIPlacementData placementData = null) : base(headerText, mainText, buttons, placementData)
        {
            Token = token;
        }
    }
}