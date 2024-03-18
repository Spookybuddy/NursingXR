using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.HMD
{
    /// <summary>
    /// Controls the visual of the side menu that appears next to the main
    /// hand menu. Links an implementation of the HandMenuTabScriptableObject
    /// to the InstrumentsHandMenu.
    /// </summary>
    public class HandMenuSidePanel : MonoBehaviour, IActivatable
    {
        public HandMenuTabScriptableObject linkedTabInformation;

        private bool added = false;

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeInHierarchy);
        }

        public void Disable()
        {
            Deactivate();
        }

        // In order for this to work, the Side Panels must start as active in the prefab
        public void OnEnable()
        {
            if(!added)
            {
                var parent = GetComponentInParent<IActivatableParent>();

                added = parent != null ? parent.AddActivatable(this, linkedTabInformation) : false;

                gameObject.SetActive(false);
            }
        }
    }
}