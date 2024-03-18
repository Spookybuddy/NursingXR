namespace GIGXR.Platform.Managers
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using Microsoft.MixedReality.Toolkit.UI;
    using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Cysharp.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Container for info in a button on a prompt.
    /// Specifies text to appear on the button, and action
    /// to be taken when the button is pressed.
    /// </summary>
    public class ButtonPromptInfo
    {
        public string buttonText;
        public Action onPressAction;

        /// <summary>
        /// A commonly used button configuration for confirmation prompts:
        /// "No" text, and no action taken if clicked.
        /// </summary>
        public static ButtonPromptInfo No
        {
            get
            {
                return new ButtonPromptInfo()
                {
                    buttonText = "No"
                    // No Action
                };
            }
        }
    }

    /// <summary>
    /// Responsible for the creation and management of prompts, dialogs, and indicators.
    /// Direct access to the <c>PromptManager</c> is not needed (or useful).
    /// Instead, prompts are created and managed in private handlers for the following events:
    /// <see cref="ShowPromptEvent"/>
    /// <see cref="ShowCancellablePromptEvent"/>
    /// <see cref="ShowTimedPromptEvent"/>
    /// <see cref="ShowGameObjectPromptEvent"/>
    /// <see cref="HideGameObjectPromptEvent"/>
    /// <see cref="ShowProgressIndicatorEvent"/>
    /// <see cref="HideProgressIndicatorEvent"/>
    /// </summary>
    public class PromptManager : BaseUiObject
    {
        // --- Enums:

        public enum WindowStates
        {
            Off = 0,
            Narrow = 12,
            Wide = 33
        }

        // --- Serialized Variables:

        [SerializeField]
        private GameObject promptScreenPrefab;

        [SerializeField]
        private GameObject progressIndicatorPrefab;

        [SerializeField]
        private float transformDistance = .25f;

        [SerializeField]
        private int backgroundHeight = 18;

        // --- Private Variables:

        // private bool _isShowingProgressIndicator; // todo idk about this
        private bool isProgressIndicatorActive 
        {
            get
            {
                return ProgressIndicator != null &&
                       (((MonoBehaviour)ProgressIndicator.Value.Item1).gameObject
                           .activeInHierarchy);
            }
        }

        private (IProgressIndicator, RadialView)? _progressIndicator;

        private (IProgressIndicator, RadialView)? ProgressIndicator
        {
            get
            {
                if (_progressIndicator == null)
                {
                    GameObject instantiatedProgressIndicator = Instantiate(progressIndicatorPrefab);

                    _progressIndicator = (instantiatedProgressIndicator.GetComponent<IProgressIndicator>(),
                        instantiatedProgressIndicator.GetComponent<RadialView>());
                    
                    // Should be hidden at first, sometimes Hide is called before Show.
                    instantiatedProgressIndicator.SetActive(false);
                }

                return _progressIndicator;
            }
        }

        // The Value is a Tuple of the PromptScreen that can hold a message and the GameObject which is the actual object that is 
        private Dictionary<GameObject, (IPromptScreen, GameObject)> gameObjectPrompts =
            new Dictionary<GameObject, (IPromptScreen, GameObject)>();

        private Dictionary<CancellationToken, IPromptScreen> cancellableScreens = 
            new Dictionary<CancellationToken, IPromptScreen>();

        protected void OnDestroy()
        {
            EventBus.Unsubscribe<ShowPromptEvent>(OnShowPromptEvent);
            EventBus.Unsubscribe<ShowCancellablePromptEvent>(OnShowCancellablePromptEvent);
            EventBus.Unsubscribe<UpdateCancellablePromptEvent>(OnUpdateCancellablePromptEvent);
            EventBus.Unsubscribe<ShowTimedPromptEvent>(OnShowTimedPromptEvent);
            EventBus.Unsubscribe<ShowGameObjectPromptEvent>(OnShowGameObjectPromptEvent);
            EventBus.Unsubscribe<HideGameObjectPromptEvent>(OnHideGameObjectPromptEvent);
            EventBus.Unsubscribe<ShowProgressIndicatorEvent>(OnShowProgressIndicatorEvent);
            EventBus.Unsubscribe<HideProgressIndicatorEvent>(OnHideProgressIndicatorEvent);
            EventBus.Unsubscribe<ShowPredicatePromptEvent>(OnShowPredicatePromptEvent);
        }

        #region BaseUiObjectImplementation

        protected override void SubscribeToEventBuses()
        {
            EventBus.Subscribe<ShowPromptEvent>(OnShowPromptEvent);
            EventBus.Subscribe<ShowCancellablePromptEvent>(OnShowCancellablePromptEvent);
            EventBus.Subscribe<UpdateCancellablePromptEvent>(OnUpdateCancellablePromptEvent);
            EventBus.Subscribe<ShowTimedPromptEvent>(OnShowTimedPromptEvent);
            EventBus.Subscribe<ShowGameObjectPromptEvent>(OnShowGameObjectPromptEvent);
            EventBus.Subscribe<HideGameObjectPromptEvent>(OnHideGameObjectPromptEvent);
            EventBus.Subscribe<ShowProgressIndicatorEvent>(OnShowProgressIndicatorEvent);
            EventBus.Subscribe<HideProgressIndicatorEvent>(OnHideProgressIndicatorEvent);
            EventBus.Subscribe<ShowPredicatePromptEvent>(OnShowPredicatePromptEvent);
        }

        #endregion

        #region EventHandlers

        private void OnShowPromptEvent(ShowPromptEvent @event)
        {
            // Set up the prompt window
            InstantiatePromptScreen
            (
                @event.HeaderText,
                @event.MainText,
                @event.PromptButtons,
                @event.WindowWidth.HasValue ? @event.WindowWidth.Value : WindowStates.Wide,
                @event.PlacementData
            );

            // OnShowPromptEvent assumes that there will be buttons or the cancel button available to destroy/remove
            // this prompt and leaves it to the developer using this EventType to manage the prompt
        }

        private void OnShowCancellablePromptEvent(ShowCancellablePromptEvent @event)
        {
            // Set up the prompt window
            IPromptScreen cancellablePromptScreen = InstantiatePromptScreen
            (
                @event.HeaderText, 
                @event.MainText,
                @event.PromptButtons,
                @event.WindowWidth.HasValue ? @event.WindowWidth.Value : WindowStates.Wide,
                @event.PlacementData
            );

            cancellableScreens.Add(@event.Token, cancellablePromptScreen);

            // Run a task that checks for canceling the token, when it occurs, bring down the prompt
            UniTask.Create
                (
                    async () =>
                    {
                        await UniTask.WaitUntil(() => @event.Token.IsCancellationRequested);

                        cancellableScreens.Remove(@event.Token);

                        if(cancellablePromptScreen != null)
                            cancellablePromptScreen.RemoveScreen();
                    }
                );
        }

        private void OnUpdateCancellablePromptEvent(UpdateCancellablePromptEvent @event)
        {
            if(cancellableScreens.ContainsKey(@event.Token))
            {
                if(!string.IsNullOrEmpty(@event.HeaderText))
                {
                    cancellableScreens[@event.Token].SetHeaderText(@event.HeaderText);
                }

                if (!string.IsNullOrEmpty(@event.MainText))
                {
                    cancellableScreens[@event.Token].SetWindowText(@event.MainText);
                }
            }
        }

        private void OnShowTimedPromptEvent(ShowTimedPromptEvent @event)
        {
            IPromptScreen timedPromptScreen = InstantiatePromptScreen
            (
                @event.HeaderText,
                @event.MainText,
                @event.PromptButtons,
                @event.WindowWidth.HasValue ? @event.WindowWidth.Value : WindowStates.Wide,
                @event.PlacementData
            );

            // Run a task that checks runs for the duration of the delay, then after, bring down the prompt
            UniTask.Create
                (
                    async () =>
                    {
                        await UniTask.Delay(@event.TimeDelayMilliSeconds);

                        if (timedPromptScreen != null)
                            timedPromptScreen.RemoveScreen();
                    }
                );
        }

        private void OnShowPredicatePromptEvent(ShowPredicatePromptEvent @event)
        {
            IPromptScreen timedPromptScreen = InstantiatePromptScreen
            (
                @event.HeaderText, 
                @event.MainText,
                @event.PromptButtons,
                @event.WindowWidth.HasValue ? @event.WindowWidth.Value : WindowStates.Wide,
                @event.PlacementData
            );

            // Run a task that checks the predicate, keep it open until the predicate returns true
            UniTask.Create
                (
                    async () =>
                    {
                        // As long as the predicate returns false, keep the prompt up
                        await UniTask.WaitUntil(@event.TerminationPredicate);

                        if (timedPromptScreen != null)
                            timedPromptScreen.RemoveScreen();
                    }
                );
        }

        private void OnShowGameObjectPromptEvent(ShowGameObjectPromptEvent @event)
        {
            if (!gameObjectPrompts.ContainsKey(@event.ObjectPrompt))
            {
                IPromptScreen gameObjectPrompt = InstantiatePromptScreen
                    (
                        @event.HeaderText,
                        @event.MainText,
                        @event.PromptButtons,
                        @event.WindowWidth.HasValue ? @event.WindowWidth.Value : WindowStates.Wide,
                        @event.PlacementData
                    );

                // Only display if there is text visible for this display
                gameObjectPrompt.SelfGameObject.SetActive(!string.IsNullOrEmpty(@event.MainText) ||
                                                          !string.IsNullOrEmpty(@event.HeaderText));

                // Does not assume anything about the prefab, if the GameObject should follow the user's gaze, the
                // prefab itself should establish that
                GameObject instantiatedGameObject = Instantiate(@event.ObjectPrompt);

                gameObjectPrompts.Add(@event.ObjectPrompt, (gameObjectPrompt, instantiatedGameObject));
            }
            else
            {
                Debug.LogWarning($"PromptManager: {@event.ObjectPrompt} is already being shown.");
            }
        }

        private void OnHideGameObjectPromptEvent(HideGameObjectPromptEvent @event)
        {
            if (gameObjectPrompts.ContainsKey(@event.ObjectPrompt))
            {
                gameObjectPrompts[@event.ObjectPrompt].Item1.RemoveScreen();

                Destroy(gameObjectPrompts[@event.ObjectPrompt].Item2);

                gameObjectPrompts.Remove(@event.ObjectPrompt);
            }
            else
            {
                Debug.LogWarning($"PromptManager: {@event.ObjectPrompt} is not a known GameObject Prompt.");
            }
        }

        private async void OnShowProgressIndicatorEvent(ShowProgressIndicatorEvent @event)
        {
            DebugUtilities.LogVerbose("OnShowProgressIndicatorEvent");
            
            if (ProgressIndicator != null &&
                ProgressIndicator.Value.Item1.State == ProgressIndicatorState.Closed)
            {
                await ProgressIndicator.Value.Item1.OpenAsync();

                // todo maybe needs more time here?
                // We need to give the progress indicator a moment to reset in front of the user
                await UniTask.Yield();

                // Don't let the progress indicator stay stuck in front of the user when they move their heads
                ProgressIndicator.Value.Item2.MaxViewDegrees = 15;
            }
        }

        private async void OnHideProgressIndicatorEvent(HideProgressIndicatorEvent @event)
        {
            DebugUtilities.LogVerbose("OnHideProgressIndicatorEvent");
            
            if (ProgressIndicator != null &&
                ProgressIndicator.Value.Item1.State == ProgressIndicatorState.Open &&
                isProgressIndicatorActive)
            {
                await ProgressIndicator.Value.Item1.CloseAsync();

                // Causes the progress indicator to appear directly in front of the user the next time it is loaded
                ProgressIndicator.Value.Item2.MaxViewDegrees = 0;
            }
        }

        #endregion

        private IPromptScreen InstantiatePromptScreen
        (
            string                 header,
            string                 message,
            List<ButtonPromptInfo> buttons,
            WindowStates           windowWidth,
            UIPlacementData        transformData
        )
        {
            GameObject newScreenGameObject = Instantiate(promptScreenPrefab);

            IPromptScreen promptScreen = newScreenGameObject.GetComponentInChildren<IPromptScreen>();

            promptScreen.SetDependencies(EventBus);

            promptScreen.SetHeaderText(header);

            promptScreen.SetWindowText(message);

            if (transformData != null)
                promptScreen.SetButtonLayout(transformData);

            if (transformData?.WindowSize != null)
                promptScreen.SetWindowSize((int)transformData.WindowSize.Value.x, (int)transformData.WindowSize.Value.y);
            else
                promptScreen.SetWindowSize((int)windowWidth, backgroundHeight);

            promptScreen.CreateButtons(buttons);

            PositionTransform(newScreenGameObject.transform, transformData);

            promptScreen.AdjustGridTransform(transformData);

            promptScreen.PlaySFX();

            return promptScreen;
        }

        // --- Private Methods:

        private void PositionTransform(Transform promptTransform, UIPlacementData placementData)
        {
            Transform placementStart = placementData?.HostTransform ?? transform;

            if(promptTransform != null)
            {
                promptTransform.SetParent(placementStart, false);
                promptTransform.forward = placementStart.forward;

                // The default position for a prompt is right in front of the transform that is holding it
                promptTransform.position = placementStart.position + (placementStart.forward * transformDistance);

                if (placementData != null)
                {
                    if (placementData.PositionOffset != Vector3.zero)
                        promptTransform.localPosition += placementData.PositionOffset;

                    if (placementData.RotationOffset != Vector3.zero)
                        promptTransform.localRotation *= Quaternion.Euler(placementData.RotationOffset);
                }
            }
        }
    }
}