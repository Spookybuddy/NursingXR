using GIGXR.Platform.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.AppEvents.Events.UI
{
    /// <summary>
    /// An event published to show a prompt that will be removed when the Predicate input is true.
    /// </summary>
    public class ShowPredicatePromptEvent : BasePromptEvent
    {
        /// <summary>
        /// Boolean function determining the duration of the prompt.
        /// Prompt will be removed when this function returns true.
        /// </summary>
        public Func<bool> TerminationPredicate { get; }

        /// <summary>
        /// See <see cref="BasePromptEvent.BasePromptEvent(string, List{ButtonPromptInfo}, bool, PromptManager.WindowStates, Transform, Action)"/>
        /// </summary>
        [Obsolete("Use the ShowPredicatePromptEvent with the UIPlacementData input.")]
        public ShowPredicatePromptEvent(Func<bool> terminationPredicate, 
                                        string mainText, 
                                        List<ButtonPromptInfo> buttons,  
                                        PromptManager.WindowStates windowWidth, 
                                        Transform hostTransform = null) : base("", mainText, buttons, windowWidth, hostTransform)
        {
            TerminationPredicate = terminationPredicate;
        }

        public ShowPredicatePromptEvent(Func<bool> terminationPredicate,
                                        string headerText,
                                        string mainText,
                                        List<ButtonPromptInfo> buttons,
                                        UIPlacementData placementData = null) : base(headerText, mainText, buttons, placementData)
        {
            TerminationPredicate = terminationPredicate;
        }
    }
}