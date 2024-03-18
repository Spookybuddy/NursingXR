namespace GIGXR.Platform.HMD.UI
{
    using UnityEngine;
    using System.Collections.Generic;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using GIGXR.Platform.UI;
    using GIGXR.Platform.Interfaces;
    using System;
    using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
    using GIGXR.Platform.AppEvents.Events.Authentication;
    using GIGXR.Platform.Core.DependencyInjection;
    using Microsoft.MixedReality.Toolkit;

    /// <summary>
    /// Provides a subscription-based screen management system for the core UI.
    /// Classes derived from <c>BaseScreenObject</c> must register their ScreenType in
    /// an Initialize override method.
    /// </summary>
    [RequireComponent(typeof(ManipulationWatcher))]
    public abstract class BaseScreenObject : BaseUiObject, IBaseScreen
    {
        /// <summary>
        /// Enumerated list of screens used in this app.
        /// </summary>
        /// TODO We should move away from an enum based approach, makes it hard to add new screens
        public enum ScreenType
        {
            None = 0,
            Authentication = 1,
            Calibration = 2,
            QrScanning = 3,
            SessionManagement = 4,
            StageManagement = 5,
            ScenarioManagement = 6,
            Prompt = 7,
            Preferences = 8
        }

        public abstract ScreenType ScreenObjectType { get; }

        /// <summary>
        /// TODO -
        /// this should not be optional, it is probably better to always
        /// have at least one "subscreen" for consistency. I think screens
        /// and subscreens should be at the same level now so you can either
        /// enable the main ScreenObject, or one of its variations. 
        /// </summary>
        [SerializeField]
        [Header("Optional - set the first subscreen that should be displayed:")]
        private SubScreenState defaultSubScreen;

        [SerializeField, Tooltip("Optional. If provided, the first and any duplicate screens" +
            "will be created from this prefab.")]
        protected GameObject screenPrefab;

        public event EventHandler RootScreenObjectSet;

        /// <summary>
        /// Used to visually toggle or move the screen. 
        /// </summary>
        public ScreenObject RootScreenObject
        {
            get
            {
                if(_rootScreenObject == null)
                    _rootScreenObject = GetComponentInChildren<ScreenObject>(true);

                return _rootScreenObject;
            }
        }

        private ScreenObject _rootScreenObject;

        private SolverHandler _followSolverHandler;
        private Follow _followBehavior;

        private SolverHandler FollowSolverHandler
        {
            get
            {
                // See if there is already a Solver Handler configured
                if(_followSolverHandler == null)
                    _followSolverHandler = RootScreenObject.gameObject.EnsureComponent<SolverHandler>();

                return _followSolverHandler;
            }
        }

        private Follow FollowBehavior
        {
            get
            {
                // See if there is already a Solver Handler configured
                if (_followBehavior == null)
                    _followBehavior = RootScreenObject.gameObject.GetComponent<Follow>();

                // If it is still null, create the SolverHandler yourself
                if (_followBehavior == null)
                {
                    _followBehavior = RootScreenObject.gameObject.AddComponent<Follow>();
                    _followBehavior.DefaultDistance = 0.75f;
                    _followBehavior.MinDistance = 0.15f;
                    _followBehavior.MaxDistance = 0.75f;
                    _followBehavior.OrientationType = SolverOrientationType.FaceTrackedObject;
                    _followBehavior.ReorientWhenOutsideParameters = false;
                    _followBehavior.FaceTrackedObjectWhileClamped = false;

                    _followBehavior.IgnoreAngleClamp = false;
                    _followBehavior.MaxViewHorizontalDegrees = 60;
                    _followBehavior.MaxViewVerticalDegrees = 40;
                }

                return _followBehavior;
            }
        }

        /// <summary>
        /// Used as a reference transform for other UI objects. 
        /// </summary>
        public Transform RootScreenTransform => RootScreenObject.transform;

        /// <summary>
        /// Maps SubScreen types to the game object references. 
        /// </summary>
        private Dictionary<SubScreenState, SubScreenObject> SubScreenObjectsDictionary
            = new Dictionary<SubScreenState, SubScreenObject>();

        /// <summary>
        /// The current active subscreen, if applicable. 
        /// </summary>
        protected SubScreenObject ActiveSubScreen { get; set; }

        #region StaticHelpers

        private static readonly Dictionary<ScreenType, BaseScreenObject> KnownScreens
            = new Dictionary<ScreenType, BaseScreenObject>();

        #endregion

        protected virtual void OnDestroy()
        {
            if (KnownScreens.ContainsKey(ScreenObjectType))
            {
                KnownScreens.Remove(ScreenObjectType);
            }

            uiEventBus.Unsubscribe<TogglingScreenEvent>(OnTogglingScreenEvent);
            uiEventBus.Unsubscribe<SwitchingActiveScreenEvent>(OnSwitchingActiveScreenEvent);
            uiEventBus.Unsubscribe<SettingScreenVisibilityEvent>(OnSettingScreenVisibilityEvent);
            uiEventBus.Unsubscribe<SettingActiveSubScreenEvent>(OnSettingActiveSubScreenEvent);
            uiEventBus.Unsubscribe<SettingScreenFollowBehaviorEvent>(OnSettingScreenFollowBehaviorEvent);
            uiEventBus.Unsubscribe<ScreenPlacementHintEvent>(OnScreenPlacementHintEvent);

            EventBus.Unsubscribe<StartLogOutEvent>(OnStartLogOutEvent);
        }

        protected override void SubscribeToEventBuses()
        {
            uiEventBus.Subscribe<TogglingScreenEvent>(OnTogglingScreenEvent);
            uiEventBus.Subscribe<SwitchingActiveScreenEvent>(OnSwitchingActiveScreenEvent);
            uiEventBus.Subscribe<SettingScreenVisibilityEvent>(OnSettingScreenVisibilityEvent);
            uiEventBus.Subscribe<SettingActiveSubScreenEvent>(OnSettingActiveSubScreenEvent);
            uiEventBus.Subscribe<SettingScreenFollowBehaviorEvent>(OnSettingScreenFollowBehaviorEvent);
            uiEventBus.Subscribe<ScreenPlacementHintEvent>(OnScreenPlacementHintEvent);

            EventBus.Subscribe<StartLogOutEvent>(OnStartLogOutEvent);
        }

        /// <summary>
        /// Locates GameObject references and sets up the subscreen dictionary.
        /// </summary>
        protected virtual void Initialize()
        {
            foreach (var currentSubScreen in GetComponentsInChildren<SubScreenObject>(true))
            {
                if (SubScreenObjectsDictionary.ContainsKey(currentSubScreen.SubState))
                {
                    Debug.LogWarning
                    (
                        $"Could not add {currentSubScreen.SubState} as it already exists in SubScreenObjectsDictionary."
                    );
                }
                else
                {
                    SubScreenObjectsDictionary.Add(currentSubScreen.SubState, currentSubScreen);
                }
            }

            if(RootScreenObject == null)
            {
                // Draw the screen if it's data driven
                var firstScreen = ScreenObjectFactory();

                // Not all screens will be using this, so always check
                if (firstScreen != null)
                {
                    firstScreen.SetActive(false);
                }
            }

            Debug.Assert(this.ScreenObjectType != ScreenType.None,
                        "ERROR: Please set ScreenType in every class derived from BaseScreen.",
                        this);

            Debug.Assert(RootScreenObject != null,
                        "ERROR: RootScreenObject is null for: " + this.ScreenObjectType,
                        this);

            if (RootScreenObject != null)
                RootScreenObjectSet?.Invoke(this, EventArgs.Empty);

            if (KnownScreens.ContainsKey(ScreenObjectType))
            {
                Debug.LogWarning
                    ($"Could not add {ScreenObjectType} as it already exists in known screens.");
            }
            else
            {
                KnownScreens.Add(ScreenObjectType, this);
            }
        }

        protected virtual void OnStartLogOutEvent(StartLogOutEvent @event)
        {
            DisableAllSubScreens();

            RootScreenObject.SetActive(false);
        }

        /// <summary>
        /// Optional method for base classes to override. If they do not use a 
        /// prefab to generate a screen, but instead rely on creating the UI
        /// at runtime, then this method should be filled in so that it can
        /// be accessed by other classes.
        /// </summary>
        /// <returns></returns>
        public virtual GameObject ScreenObjectFactory(bool setAsChild = true)
        {
            if (screenPrefab != null)
            {
                if(setAsChild)
                {
                    return Instantiate(screenPrefab, transform);
                }
                else
                {
                    return Instantiate(screenPrefab);
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the new active subscreen and disables the old one, if applicable.
        /// </summary>
        /// <param name="newSubScreenState">The new target subscreen to become active.</param>
        private void SwitchActiveSubScreen(SubScreenState newSubScreenState)
        {
            if (SubScreenObjectsDictionary.ContainsKey(newSubScreenState))
            {
                DisableAllSubScreens();

                // Set and bring up the new SubScreen
                ActiveSubScreen = SubScreenObjectsDictionary[newSubScreenState];
                ActiveSubScreen.SetActive(true);
            }
            else if (newSubScreenState == SubScreenState.None)
            {
                DisableAllSubScreens();
            }
        }

        /// <summary>
        /// Takes down all subscreens. 
        /// </summary>
        private void DisableAllSubScreens()
        {
            foreach (SubScreenObject subScreenObject in SubScreenObjectsDictionary.Values)
            {
                subScreenObject.SetActive(false);
            }

            ActiveSubScreen = null;
        }

        /// <summary>
        /// Responds to togglingScreenEvent.
        /// If this screen is the target screen, it is toggled to the opposite state.
        /// </summary>
        /// <param name="togglingScreenEvent"></param>
        private void OnTogglingScreenEvent(TogglingScreenEvent togglingScreenEvent)
        {
            if ((RootScreenObject == null) ||
                (togglingScreenEvent.TargetScreen != ScreenObjectType))
            {
                return;
            }

            uiEventBus.Publish
            (
                new SettingScreenVisibilityEvent(ScreenObjectType, !RootScreenObject.IsActive)
            );
        }

        /// <summary>
        /// Sets the screen visibility for the target screen, and enables a default subscreen
        /// if one exists.
        /// </summary>
        /// <param name="settingScreenVisibilityEvent"></param>
        protected virtual void OnSettingScreenVisibilityEvent
            (SettingScreenVisibilityEvent settingScreenVisibilityEvent)
        {
            DebugUtilities.LogVerbose
            (
                $"[eventbus] OnSettingScreenVisibilityEvent {settingScreenVisibilityEvent.TargetScreen} should be {settingScreenVisibilityEvent.ShouldBeActive}"
            );

            if (settingScreenVisibilityEvent.TargetScreen != ScreenObjectType)
            {
                return;
            }

            if (RootScreenObject != null)
                RootScreenObject.SetActive(settingScreenVisibilityEvent.ShouldBeActive);
            /*else
            {
                Debug.LogError
                    ($"RootScreenObject is null for {ScreenObjectType} on {gameObject}", this);
            }*/

            // Turn on default subscreen, if it exists
            if ((settingScreenVisibilityEvent.ShouldBeActive) &&
                (defaultSubScreen != SubScreenState.None))
            {
                uiEventBus.Publish
                    (new SettingActiveSubScreenEvent(ScreenObjectType, defaultSubScreen));
            }
        }

        /// <summary>
        /// Switches active subscreen in response to SettingActiveSubScreenEvent.
        /// </summary>
        /// <param name="settingActiveSubScreenEvent"></param>
        private void OnSettingActiveSubScreenEvent(SettingActiveSubScreenEvent settingActiveSubScreenEvent)
        {
            if ((settingActiveSubScreenEvent.TargetScreen != ScreenObjectType) ||
                (!RootScreenObject.IsActive))
            {
                return;
            }

            SwitchActiveSubScreen(settingActiveSubScreenEvent.SubScreenStateToSwitchTo);

            uiEventBus.Publish
                (new SwitchedActiveSubScreenEvent(settingActiveSubScreenEvent.TargetScreen, settingActiveSubScreenEvent.SubScreenStateToSwitchTo));
        }

        private void OnSettingScreenFollowBehaviorEvent(SettingScreenFollowBehaviorEvent @event)
        {
            if(@event.TargetScreen == ScreenObjectType)
            {
                FollowSolverHandler.enabled = @event.IsBehaviorOn;
                FollowBehavior.enabled = @event.IsBehaviorOn;

                FollowSolverHandler.TrackedTargetType = @event.ObjectToFollow;
                
                FollowSolverHandler.UpdateSolvers = @event.IsBehaviorOn;
            }
        }

        private void OnScreenPlacementHintEvent(ScreenPlacementHintEvent @event)
        {
            if(@event.TargetScreen == ScreenObjectType)
            {
                RootScreenObject.transform.position = @event.WorldPosition;
                RootScreenObject.transform.rotation = @event.WorldRotation;
            }
        }

        /// <summary>
        /// Enables this screen if it is the new target active screen. Disables it
        /// otherwise.
        /// </summary>
        /// <param name="switchingActiveScreenEvent"></param>
        private void OnSwitchingActiveScreenEvent
            (SwitchingActiveScreenEvent switchingActiveScreenEvent)
        {
            DebugUtilities.LogVerbose
            (
                $"OnSwitchingActiveScreenEvent Target {switchingActiveScreenEvent.TargetScreen} Sender {switchingActiveScreenEvent.SenderScreen}"
            );

            uiEventBus.Publish
            (
                switchingActiveScreenEvent.TargetScreen != ScreenObjectType
                    ? new SettingScreenVisibilityEvent(ScreenObjectType, false)
                    : new SettingScreenVisibilityEvent(ScreenObjectType, true)
            );
        }
    }
}