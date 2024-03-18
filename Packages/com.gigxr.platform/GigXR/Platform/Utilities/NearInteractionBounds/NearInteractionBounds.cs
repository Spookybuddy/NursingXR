using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// This class manages the enabled state of interactive game objects in a UI to allow near-touch-interactive UI (namely buttons) to exist in close proximity to each other without interfering with each other.
    /// This does not support runtime changes to rects, changes to child bounds relative to their parent bounds, etc. In other words, all serialized data is private and is assumed to not change at runtime.
    /// 
    /// This works as follows:
    ///     - The NearInteractionBounds with no parent registers with MRTK source state and hand joint events.
    ///     - When one of the 2 index finger tips is inside the bounds set by this component (in its local space), it marks itself as near interacting.
    ///     - Similarly, when both index fingers have left the bounds or the corresponding controllers are lost, this is marked as not near interacting.
    ///     - Children listen to their parent's near interacting state. When the parent is near interacting, children register for the parent's propagated joint position updates.
    ///     - When a child is near interacting, it updates its parent with this fact, and the parent propagates this to its siblings. A child is interactable if any of the following is true:
    ///         - Its parent is not near-interacting (so there isn't worry of near-interaction-interference between siblings)
    ///         - Its parent is near-interacting, and either it is also near interacting or none of its siblings are.
    ///
    /// This means that children don't spend time processing joint position updates until their immediate parent is already in near-interaction range.
    /// This, of course, assumes that the bounds set for a parent entirely encompasses the bounds set on the children, and that bounds associated with interactive elements align with those elements.
    /// 
    /// There is a tradeoff here: if interactive elements are near interacting, then their siblings will not be able to far interact. So, you can't hover one hand over a button and press the button next to it with the other hand's far interaction.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class NearInteractionBounds : MonoBehaviour, IMixedRealitySourceStateHandler, IMixedRealityHandJointHandler
    {
        #region Serialized

        [Tooltip("The bounds above this in the hierarchy. This will not check for near interaction unless its parent is in near interaction. If this has no parent, it will listen directly to MRTK input source and finger joint updates. This should be under the parent in the Unity Hierarchy.")]
        [SerializeField] private NearInteractionBounds parent;

        [Tooltip("The bounds in which a pointer finger must be to be considered near interacting, in local space. These should entirely contain any children.")]
        [SerializeField] private Bounds nearInteractionBounds;

        [Tooltip("If true, the x-y coordinates of the size and center of the bounds will be automatically upated to match this component's RectTransform. This will be applied at deisgn time only; no runtime adjustments will be made.")]
        [SerializeField] private bool matchBoundsXYToRect = true;

        [SerializeField] private List<InteractivityStateHandler> managedInteractivityHandlers;

        #endregion

        #region Private Fields and Properties

        private RectTransform ThisRectTransform => transform as RectTransform;

        private IMixedRealityHand leftHandController;
        private IMixedRealityHand rightHandController;

        // should only be referenced directly by wrapping property
        private Vector3? leftIndexTipPosition;

        // should only be referenced directly by wrapping property
        private Vector3? rightIndexTipPosition;

        // should only be referenced directly by wrapping property
        private bool isNearInteracting;

        private HashSet<NearInteractionBounds> nearInteractingChildren = new HashSet<NearInteractionBounds>();

        #endregion

        #region Events and Properties for NearInteractionBounds Hierarchy

        private event EventHandler NearInteractionEntered;
        private event EventHandler NearInteractionExited;

        private event EventHandler<Vector3?> LeftIndexTipPositionUpdated;
        private event EventHandler<Vector3?> RightIndexTipPositionUpdated;

        private event EventHandler<ICollection<NearInteractionBounds>> NearInteractingChildrenUpdated;

        private Vector3? LeftIndexTipPosition
        {
            get => leftIndexTipPosition;
            set
            {
                leftIndexTipPosition = value;
                UpdateNearInteractionState();
                LeftIndexTipPositionUpdated?.Invoke(this, leftIndexTipPosition);
            }
        }

        private Vector3? RightIndexTipPosition
        {
            get => rightIndexTipPosition;
            set
            {
                rightIndexTipPosition = value;
                UpdateNearInteractionState();
                RightIndexTipPositionUpdated?.Invoke(this, rightIndexTipPosition);
            }
        }

        private bool IsNearInteracting
        {
            get => isNearInteracting;
            set
            {
                if (isNearInteracting != value)
                {
                    isNearInteracting = value;
                    
                    if (isNearInteracting)
                    {
                        NearInteractionEntered?.Invoke(this, null);
                        if (parent != null)
                        {
                            parent.MarkNearInteractingChild(this);
                        }
                        else
                        {
                            EnableInteractivity(true);
                        }
                    }
                    else
                    {
                        NearInteractionExited?.Invoke(this, null);
                        if (parent != null)
                        {
                            parent.MarkNearInteractingChild(this);
                        }
                        else
                        {
                            EnableInteractivity(true);
                        }
                    }
                }
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Serialize where possible. Only use this when necessary to incorporate runtime-instantiated
        /// Does not support setting parent to null, or changing parent.
        /// Only supports setting a non-null parent from a null parent.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(NearInteractionBounds parent)
        {
            if (parent == null)
            {
                throw new Exception("Cannot set NearInteractionBounds parent to null");
            }

            if (this.parent == parent)
            {
                return;
            }

            if (this.parent != null)
            {
                throw new Exception("Cannot change NearInteractionBounds parent when already not null");
            }

            if (enabled)
            {
                // kinda hacky, but update state to match being disabled to unregister handlers and reset internal data, then change parent and enable with a parent
                OnDisable();
                this.parent = parent;
                OnEnable();
            }
            else
            {
                this.parent = parent;
            }
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            if (parent == null)
            {
                // this has no parent, so it is the top of the hierarchy. It will register directly with MRTK input sources
                RegisterWithMRTKInputSystem();
            }
            else
            {
                // this will listen to its parent for updates
                RegisterForParentNearInteractionEvents();

                // if this was made active when the parent was already active, get up-to-speed with the current state
                if (parent.IsNearInteracting)
                {
                    OnParentNearInteractionEntered(null, null);
                    LeftIndexTipPosition = parent.LeftIndexTipPosition;
                    RightIndexTipPosition = parent.RightIndexTipPosition;
                }
            }
        }

        private void OnDisable()
        {
            if (parent == null)
            {
                UnregisterWithMRTKInputSystem();
            }
            else
            {
                UnregisterForParentNearInteractionEvents();
            }

            leftHandController = null;
            rightHandController = null;

            LeftIndexTipPosition = null;
            RightIndexTipPosition = null;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            if (matchBoundsXYToRect)
            {
                Vector3 rectCenter = ThisRectTransform.rect.center;
                Vector3 rectSize = ThisRectTransform.rect.size;

                Vector3 boundsCenter = new Vector3(rectCenter.x, rectCenter.y, nearInteractionBounds.center.z);
                Vector3 boundsSize = new Vector3(rectSize.x, rectSize.y, nearInteractionBounds.size.z);

                bool dirty = false;

                if (nearInteractionBounds.center != boundsCenter)
                {
                    nearInteractionBounds.center = boundsCenter;
                    dirty = true;
                }

                if (nearInteractionBounds.size != boundsSize)
                {
                    nearInteractionBounds.size = boundsSize;
                    dirty = true;
                }

                if (dirty)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
#endif
        }

        #endregion

        #region Event Registration Helpers

        private bool registeredWithMRTKInputSystem = false;
        private void RegisterWithMRTKInputSystem()
        {
            if (registeredWithMRTKInputSystem)
                return;

            registeredWithMRTKInputSystem = true;

            CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);
        }

        private void UnregisterWithMRTKInputSystem()
        {
            if (!registeredWithMRTKInputSystem)
                return;

            registeredWithMRTKInputSystem = false;

            CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityHandJointHandler>(this);
        }

        private bool registeredForParentNearInteractionEvents = false;

        private void RegisterForParentNearInteractionEvents()
        {
            if (registeredForParentNearInteractionEvents || parent == null)
                return;

            registeredForParentNearInteractionEvents = true;

            parent.NearInteractionEntered += OnParentNearInteractionEntered;
            parent.NearInteractionExited += OnParentNearInteractionExited;
        }

        private void UnregisterForParentNearInteractionEvents()
        {
            if (!registeredForParentNearInteractionEvents || parent == null)
                return;

            registeredForParentNearInteractionEvents = false;

            parent.NearInteractionEntered -= OnParentNearInteractionEntered;
            parent.NearInteractionExited -= OnParentNearInteractionExited;
        }

        private bool registeredForParentJointAndSiblingEvents = false;
        private void RegisterForParentJointAndSiblingEvents()
        {
            if (registeredForParentJointAndSiblingEvents || parent == null)
                return;

            registeredForParentJointAndSiblingEvents = true;

            parent.LeftIndexTipPositionUpdated += OnParentLeftIndexTipPositionUpdated;
            parent.RightIndexTipPositionUpdated += OnParentRightIndexTipPositionUpdated;
            parent.NearInteractingChildrenUpdated += OnSiblingNearInteractionChanged;
        }

        private void UnregisterForParentJointAndSiblingEvents()
        {
            if (!registeredForParentJointAndSiblingEvents || parent == null)
                return;

            registeredForParentJointAndSiblingEvents = false;

            parent.LeftIndexTipPositionUpdated -= OnParentLeftIndexTipPositionUpdated;
            parent.RightIndexTipPositionUpdated -= OnParentRightIndexTipPositionUpdated;
            parent.NearInteractingChildrenUpdated -= OnSiblingNearInteractionChanged;
        }

        #endregion

        #region IMixedRealitySourceStateHandler

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            var controller = eventData.Controller as IMixedRealityHand;

            // only deal with hands
            if (controller != null)
            {
                switch (controller.ControllerHandedness)
                {
                    case Handedness.Left:
                        leftHandController = controller;
                        break;

                    case Handedness.Right:
                        rightHandController = controller;
                        break;
                }
            }
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            var controller = eventData.Controller as IMixedRealityHand;

            // only deal with hands
            if (controller != null)
            {
                switch (controller.ControllerHandedness)
                {
                    case Handedness.Left:
                        leftHandController = null;
                        LeftIndexTipPosition = null;
                        break;

                    case Handedness.Right:
                        rightHandController = null;
                        RightIndexTipPosition = null;
                        break;
                }
            }
        }

        #endregion

        #region IMixedRealityHandJointHandler

        public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
        {
            if (leftHandController != null && eventData.InputSource.SourceId == leftHandController.InputSource.SourceId)
            {
                if (eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out var pose))
                {
                    LeftIndexTipPosition = pose.Position;
                }
            }
            else if (rightHandController != null && eventData.InputSource.SourceId == rightHandController.InputSource.SourceId)
            {
                if (eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out var pose))
                {
                    RightIndexTipPosition = pose.Position;
                }
            }
        }

        #endregion

        private void OnParentNearInteractionEntered(object sender, EventArgs args)
        {
            // disable near interactivity when parent enters near-interaction, to be re-enabled when this particular child enters near-interaction
            EnableInteractivity(false);

            RegisterForParentJointAndSiblingEvents();
        }

        private void OnParentNearInteractionExited(object sender, EventArgs args)
        {
            UnregisterForParentJointAndSiblingEvents();

            LeftIndexTipPosition = null;
            RightIndexTipPosition = null;

            // enable interactivity whenever parent is not in near-interaction, so far-interaction is not blocked
            EnableInteractivity(true);
        }

        private void OnParentLeftIndexTipPositionUpdated(object sender, Vector3? leftIndexTipPosition)
        {
            LeftIndexTipPosition = leftIndexTipPosition;
        }

        private void OnParentRightIndexTipPositionUpdated(object sender, Vector3? rightIndexTipPosition)
        {
            RightIndexTipPosition = rightIndexTipPosition;
        }

        private void OnSiblingNearInteractionChanged(object sender, ICollection<NearInteractionBounds> nearInteractingSiblings)
        {
            EnableInteractivity(nearInteractingSiblings.Count == 0 || nearInteractingSiblings.Contains(this));
        }

        private void EnableInteractivity(bool enable)
        {
            foreach (var handler in managedInteractivityHandlers)
            {
                handler.EnableInteractivity(enable);
            }
        }

        private void MarkNearInteractingChild(NearInteractionBounds child)
        {
            bool invoke;
            if (child.IsNearInteracting)
            {
                invoke = nearInteractingChildren.Add(child);
            }
            else
            {
                invoke = nearInteractingChildren.Remove(child);
            }

            if (invoke)
            {
                NearInteractingChildrenUpdated?.Invoke(this, nearInteractingChildren);
            }
        }
        private void UpdateNearInteractionState()
        {
            IsNearInteracting = BoundsContains(LeftIndexTipPosition) || BoundsContains(RightIndexTipPosition);
        }
        private bool BoundsContains(Vector3? worldPosition)
        {
            if (!worldPosition.HasValue)
                return false;

            Vector3 localPosition = transform.InverseTransformPoint(worldPosition.Value);
            return nearInteractionBounds.Contains(localPosition);
        }
    }

    /// <summary>
    /// This is not intended to be used itself (obviously). Make subclasses to suit needs.
    /// (This would be abstract if Unity allowed serialized references to abstractions).
    /// </summary>
    public class InteractivityStateHandler : MonoBehaviour
    {
        public virtual void EnableInteractivity(bool enabled)
        {

        }
    }
}
