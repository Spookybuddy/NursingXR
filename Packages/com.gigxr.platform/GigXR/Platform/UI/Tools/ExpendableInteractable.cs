using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.UI
{
    /// <summary>
    /// Helper component that gives an MRTK Interactable 'expandable' behaviors through the State and Theme's flow 
    /// from MRTK. Here, the 'Custom' state is meant to be Expanded, while the Default state is Collapsed. The attached
    /// Interactable must have the correct Theme setup for this behavior to work.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ExpendableInteractable : MonoBehaviour
    {
        public Interactable InteractableComponent { get { return interactable; } }

        private Interactable interactable;

        private bool isExpanded 
        { 
            get 
            {
                var value = interactable.GetStateValue(InteractableStates.InteractableStateEnum.Custom);
                return value == 1; 
            } 
        }

        // TODO At the moment, only the Session List is utilizing this and we only want to allow one session to be
        // expanded at a time
        private static ExpendableInteractable currentInteractableExpanded;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        public void TryCollapseExpand()
        {
            if (currentInteractableExpanded != null && currentInteractableExpanded != this)
            {
                currentInteractableExpanded.InteractableComponent.SetState(InteractableStates.InteractableStateEnum.Custom, false);
            }

            // If not expanded, expand (custom state). Otherwise, collapse (default state)
            interactable.SetState(InteractableStates.InteractableStateEnum.Custom, !isExpanded);

            currentInteractableExpanded = this;
        }
    }
}