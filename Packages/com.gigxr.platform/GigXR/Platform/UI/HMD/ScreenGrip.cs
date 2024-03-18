using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.UI.HMD
{
    /// <summary>
    /// Utility to make sure that a Grip MonoBehavior always points to the correct GameObject and not itself.
    /// </summary>
    [RequireComponent(typeof(ObjectManipulator))]
    public class ScreenGrip : MonoBehaviour
    {
        private ObjectManipulator manipulator;

        void Start()
        {
            manipulator = GetComponent<ObjectManipulator>();

            TryFixManipulation();
        }

        public void TryFixManipulation()
        {
            if(manipulator != null)
            {
                var screen = GetComponentInParent<ScreenObject>();

                if (manipulator.HostTransform != screen.transform)
                {
                    //Debug.LogWarning($"The ObjectManipulator on {name} is pointing to itself. Correcting.", this);
                    manipulator.HostTransform = screen.transform;
                }
            }
        }
    }
}