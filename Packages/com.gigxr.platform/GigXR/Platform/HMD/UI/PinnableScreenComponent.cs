namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.UI;
    using Microsoft.MixedReality.Toolkit.UI;
    using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Provides screen related data to the HMD Toolbar so it can have it's own button with pinning actions.
    /// </summary>
    [RequireComponent(typeof(BaseScreenObject))]
    public class PinnableScreenComponent : BaseUiObject
    {
        #region EditorSetValues

        [SerializeField]
        private ToolbarButtonScriptableObject toolbarInfo;

        #endregion

        private bool IsDockedOnToolbar { get { return followToolbarSolverHandler.UpdateSolvers; } }

        private BaseScreenObject _attachedScreenObject;

        private BaseScreenObject AttachedScreenObject
        {
            get
            {
                if (_attachedScreenObject == null)
                    _attachedScreenObject = GetComponent<BaseScreenObject>();

                return _attachedScreenObject;
            }
        }

        private SolverHandler followToolbarSolverHandler;
        private Follow followToolbar;

        private ObjectManipulator[] manipulators;

        protected void Start()
        {
            if(AttachedScreenObject.RootScreenObject == null)
            {
                AttachedScreenObject.RootScreenObjectSet += AttachedScreenObject_RootScreenObjectSet;
            }
            else
            {
                SetupToolbarFollower();
            }
        }

        protected void OnDestroy()
        {
            uiEventBus.Publish(new RemoveScreenFromToolbarEvent(AttachedScreenObject.ScreenObjectType));

            uiEventBus.Unsubscribe<ReturningScreenToOriginEvent>(OnReturningScreenToOriginEvent);
            uiEventBus.Unsubscribe<TogglingScreenEvent>(OnTogglingScreenEvent);
            uiEventBus.Unsubscribe<UndockedScreenFromToolbarEvent>(OnUndockedScreenFromToolbarEvent);
            uiEventBus.Unsubscribe<SetToolbarStateEvent>(OnSetToolbarStateEvent);

            foreach (var manip in manipulators ?? Enumerable.Empty<ObjectManipulator>())
            {
                manip.OnManipulationStarted.RemoveListener(TryToUndockScreenFromToolbar);
            }
        }

        private void SetupToolbarFollower()
        {
            // Setup the screen to follow the toolbar using MRTK's Solver system, creating everything in the script
            followToolbarSolverHandler = _attachedScreenObject.RootScreenObject.gameObject.AddComponent<SolverHandler>();
            followToolbarSolverHandler.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.CustomOverride;
            // TODO Improve this lookup
            followToolbarSolverHandler.TransformOverride = FindObjectOfType<ToolbarProperties>().transform;

            followToolbar = _attachedScreenObject.RootScreenObject.gameObject.AddComponent<Follow>();
            followToolbar.DefaultDistance = 0;
            followToolbar.MinDistance = 0;
            followToolbar.MaxDistance = 0;
            followToolbar.OrientationType = Microsoft.MixedReality.Toolkit.Utilities.SolverOrientationType.FollowTrackedObject;
            followToolbar.ReorientWhenOutsideParameters = false;
            followToolbar.IgnoreAngleClamp = true;
            followToolbar.FaceTrackedObjectWhileClamped = false;

            // When the screens are created, the toolbar won't be up yet, so don't start following it until the first SetToolbarStateEvent
            followToolbarSolverHandler.UpdateSolvers = false;

            // Set up watchers to check if screen has been pinned to the world
            manipulators = GetComponentsInChildren<ObjectManipulator>(true);

            foreach (var manip in manipulators ?? Enumerable.Empty<ObjectManipulator>())
            {
                manip.OnManipulationStarted.AddListener(TryToUndockScreenFromToolbar);
            }

            uiEventBus.Publish(new AddScreenToToolbarEvent(_attachedScreenObject.ScreenObjectType, toolbarInfo));
        }

        private void AttachedScreenObject_RootScreenObjectSet(object sender, System.EventArgs e)
        {
            _attachedScreenObject.RootScreenObjectSet -= AttachedScreenObject_RootScreenObjectSet;

            SetupToolbarFollower();
        }

        protected override void SubscribeToEventBuses()
        {
            uiEventBus.Subscribe<ReturningScreenToOriginEvent>(OnReturningScreenToOriginEvent);
            uiEventBus.Subscribe<TogglingScreenEvent>(OnTogglingScreenEvent);
            uiEventBus.Subscribe<UndockedScreenFromToolbarEvent>(OnUndockedScreenFromToolbarEvent);
            uiEventBus.Subscribe<SetToolbarStateEvent>(OnSetToolbarStateEvent);
        }

        private void OnReturningScreenToOriginEvent(ReturningScreenToOriginEvent e)
        {
            if (_attachedScreenObject.ScreenObjectType == e.TargetScreen)
            {
                // Reset the position of the screen to the toolbar
                gameObject.transform.position = followToolbarSolverHandler.TransformTarget.position;

                // Make the screen follow the toolbar again
                followToolbarSolverHandler.UpdateSolvers = true;
            }
            else
            {
                // A different screen is active and attached to the toolbar dock, remove your screen
                if (_attachedScreenObject.RootScreenObject != null &&
                    _attachedScreenObject.RootScreenObject.IsActive &&
                    IsDockedOnToolbar)
                {
                    uiEventBus.Publish(new SettingScreenVisibilityEvent(_attachedScreenObject.ScreenObjectType, false));
                }
            }
        }

        private void OnTogglingScreenEvent(TogglingScreenEvent togglingScreenEvent)
        {
            // Reset the position of the screen to the toolbar if it is still docked so that any toolbar movement offset is accounted for
            if(togglingScreenEvent.TargetScreen == _attachedScreenObject.ScreenObjectType && 
               IsDockedOnToolbar)
            {
                gameObject.transform.position = followToolbarSolverHandler.TransformTarget.position;
            }
            // Turn off this pinnable screen if it was not toggled to, it is active in the scene, and docked with the toolbar
            else if (togglingScreenEvent.TargetScreen != _attachedScreenObject.ScreenObjectType &&
                     _attachedScreenObject.RootScreenObject != null &&
                     _attachedScreenObject.RootScreenObject.IsActive &&
                     IsDockedOnToolbar)
            {
                uiEventBus.Publish(new SettingScreenVisibilityEvent(_attachedScreenObject.ScreenObjectType, false));
            }
        }

        private void OnUndockedScreenFromToolbarEvent(UndockedScreenFromToolbarEvent @event)
        {
            if(@event.UndockedScreen == _attachedScreenObject.ScreenObjectType)
            {
                followToolbarSolverHandler.UpdateSolvers = false;
            }
        }

        /// <summary>
        /// Adjusts following the toolbar when it is active or not.
        /// </summary>
        /// <param name="event"></param>
        private void OnSetToolbarStateEvent(SetToolbarStateEvent @event)
        {
            followToolbarSolverHandler.UpdateSolvers = @event.ToolbarState;
        }

        private void TryToUndockScreenFromToolbar(ManipulationEventData t)
        {
            uiEventBus.Publish(new TryToUndockScreenFromToolbarEvent(_attachedScreenObject.ScreenObjectType));
        }
    }
}