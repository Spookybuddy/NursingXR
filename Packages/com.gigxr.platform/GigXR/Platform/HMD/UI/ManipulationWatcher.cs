using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Linq;
using UnityEngine;

namespace GIGXR.Platform.HMD.UI
{
    /// <summary>
    /// Allows a GameObject to cause the physics engine to resync after a manipulation event.
    /// <remarks>Until https://github.com/microsoft/MixedRealityToolkit-Unity/issues/9409 is merged in
    /// the BoundsControl does not actually work via code and you must use the Editor to set up the events
    /// callbacks.</remarks>
    /// </summary>
    public class ManipulationWatcher : MonoBehaviour
    {
        private ObjectManipulator[] manipulators;
        private BoundsControl[] bounds;

        ResyncPhysicsBackgroundHandler resyncClientPhysics;

        void OnEnable()
        {
            manipulators = GetComponentsInChildren<ObjectManipulator>(true);

            foreach (var manip in manipulators)
            {
                manip.OnManipulationEnded.AddListener(OnManipulationEnded);
            }

            bounds = GetComponentsInChildren<BoundsControl>(true);

            // HACK If there are bounds, then set up a listener 
            // Remove this when MRTK fixes the bounds event issue
            if(bounds.Length > 0 && !Physics.autoSimulation)
            {
                resyncClientPhysics = new ResyncPhysicsBackgroundHandler();
                resyncClientPhysics.Enable();
            }

            foreach (var boundsControl in bounds)
            {
                boundsControl.TranslateStopped.AddListener(OnTranslationStopped);
                boundsControl.RotateStopped.AddListener(OnRotationStopped);
                boundsControl.ScaleStopped.AddListener(OnScaleStopped);
            }
        }

        void OnDestroy()
        {
            if(resyncClientPhysics != null)
            {
                resyncClientPhysics.Disable();
            }

            foreach (var manip in manipulators ?? Enumerable.Empty<ObjectManipulator>())
            {
                manip.OnManipulationEnded.RemoveListener(OnManipulationEnded);
            }

            foreach (var boundsControl in bounds ?? Enumerable.Empty<BoundsControl>())
            {
                boundsControl.TranslateStopped.RemoveListener(OnTranslationStopped);
                boundsControl.RotateStopped.RemoveListener(OnRotationStopped);
                boundsControl.ScaleStopped.RemoveListener(OnScaleStopped);
            }
        }

        private void OnManipulationEnded(ManipulationEventData t)
        {
            ResyncPhysics();
        }

        private void OnTranslationStopped()
        {
            ResyncPhysics();
        }

        private void OnRotationStopped()
        {
            ResyncPhysics();
        }

        private void OnScaleStopped()
        {
            ResyncPhysics();
        }

        public void ResyncPhysics()
        {
            if (!Physics.autoSimulation)
            {
                Physics.SyncTransforms();
            }
        }
    }
}