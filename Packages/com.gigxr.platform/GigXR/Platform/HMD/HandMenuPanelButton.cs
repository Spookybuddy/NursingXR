using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GIGXR.Platform.HMD
{
    public interface IActivatableParent
    {
        bool AddActivatable(IActivatable activatable, HandMenuTabScriptableObject linkedTabInformation);
    }

    public interface IActivatable
    {
        void Activate();

        void Deactivate();

        void Disable();

        void Toggle();
    }

    [RequireComponent(typeof(ToggleButtonComponent))]
    public class HandMenuPanelButton : MonoBehaviour, IActivatable
    {
        public HandMenuTabScriptableObject linkedTabInformation;

        public UnityEvent activateState;

        public UnityEvent deactivateState;

        private bool added = false;

        private ToggleButtonComponent toggleButton;

        public void Activate()
        {
            toggleButton.ForceState(true);

            activateState?.Invoke();
        }

        public void Deactivate()
        {
            toggleButton.ForceState(false);

            deactivateState?.Invoke();
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if(!toggleButton.CurrentToggleState)
            {
                activateState?.Invoke();
            }
            else
            {
                deactivateState?.Invoke();
            }

            toggleButton.ForceState(!toggleButton.CurrentToggleState);
        }

        public void OnEnable()
        {
            toggleButton = GetComponent<ToggleButtonComponent>();

            if (linkedTabInformation != null)
            {
                if (!added)
                {
                    var parent = GetComponentInParent<IActivatableParent>();

                    added = parent != null ? parent.AddActivatable(this, linkedTabInformation) : false;
                }
            }
        }
    }
}