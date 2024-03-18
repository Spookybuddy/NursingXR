using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Intended for use with NearInteractionBounds. Buttons can be made to work with this; simply move the
    /// Interactable, PressableButtonHL2, NearTouchInteractable, Collider, etc to a child-less game object with
    /// no visual elements and mark this game object for management.
    /// </summary>
    public class BasicInteractivityStateHandler : InteractivityStateHandler
    {
        [SerializeField] private List<GameObject> managedGameObjects;

        public override void EnableInteractivity(bool enabled)
        {
            foreach (var go in managedGameObjects)
            {
                go.SetActive(enabled);
            }
        }
    }
}
