namespace GIGXR.Platform.Core.UI
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Core.DependencyValidator;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.HMD.UI;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Managers;
    using GIGXR.Platform.Scenarios;
    using GIGXR.Platform.Scenarios.Data;
    using GIGXR.Platform.Scenarios.EventArgs;
    using GIGXR.Platform.Sessions;
    using GIGXR.Platform.UI;
    using Microsoft.MixedReality.Toolkit.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// 
    /// </summary>
    public class ScenarioScreenObject : BaseUiObject, IScrollInput
    {
        [RequireDependency, SerializeField] private GridObjectCollection stageButtonGrid;

        [RequireDependency, SerializeField] private TMP_Text timeInPlayingScenarioText;

        [RequireDependency, SerializeField] private TMP_Text timeInPlayingStageText;

        [RequireDependency, SerializeField] private TMP_Text aboutScenarioText;

        [RequireDependency, SerializeField] private GameObject playModePanel;

        [RequireDependency, SerializeField] private GameObject optionSubPanel;

        // Tab related buttons
        [RequireDependency, SerializeField] private ButtonComponent controlsButtonComponent;

        [RequireDependency, SerializeField] private ButtonComponent aboutButtonComponent;

        // TODO Want to separate out the subscreen objects at some point into their own class to manage them better
        [RequireDependency, SerializeField] private ButtonComponent selectPathwayButtonComponent;

        [RequireDependency, SerializeField] private ButtonComponent selectPlayModeButtonComponent;

        [RequireDependency, SerializeField] private GameObject selectPlayModeParent;

        private CancellationTokenSource promptCurrentScenarioPathwayTokenSource;
        private CancellationTokenSource promptPlayModeTokenSource;
        private CancellationTokenSource answerPromptTokenSource;

        private int stageButtonScrollIndex;
        private const int buttonsToDisplay = 3;

        private const string TIME_FORMAT_COLON_SEPERATED = @"hh\:mm\:ss";

        private UIPlacementData scenarioPathwayPromptData;
        private UIPlacementData scenarioPlayModePromptData;
        private UIPlacementData stopScenarioPromptData;

        private List<ScenarioScreen.StageButton> stageButtonsList = new List<ScenarioScreen.StageButton>();

        private ScenarioScreen parentScreen;

        private ScenarioControlsController scenarioController;

        private IScenarioManager _scenarioManager;
        private ISessionManager _sessionManager;
        string[] lastReceivedStagesList;

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManager, ISessionManager sessionManager)
        {
            _scenarioManager = scenarioManager;
            _sessionManager = sessionManager;

            lastReceivedStagesList = null;

        }

        protected override void SubscribeToEventBuses()
        {
            _scenarioManager.ScenarioLoaded += OnScenarioLoaded;
            _scenarioManager.ScenarioUnloaded += OnScenarioUnloaded;
            _scenarioManager.ScenarioPlayModeSet += OnScenarioPlayModeSet;
            _scenarioManager.NewScenarioPathway += OnNewScenarioPathway;
        }

        public void SetScenarioControlsController(ScenarioControlsController scenarioController)
        {
            this.scenarioController = scenarioController;
        }

        private void OnNewScenarioPathway(object sender, ScenarioPathwaySetEventArgs e)
        {
            UpdatePathwayButtonText(e.givenPathway);
        }

        private void OnScenarioPlayModeSet(object sender, ScenarioPlayModeSetEventArgs e)
        {
            UpdatePlayModeButtonText(e.playMode);
        }

        private void OnScenarioLoaded(object sender, ScenarioLoadedEventArgs e)
        {
            SetAboutText(_scenarioManager.LastSavedScenario.scenarioName);
            UnlockAllStages();
        }

        private void OnScenarioUnloaded(object sender, ScenarioUnloadedEventArgs e)
        {
            // Make sure the 'More Options' window is brought down between sessions
            OnCancelOptionButtonClicked();
        }

        private void OnStageButtonClickedGuid(Guid guid)
        {
            _scenarioManager.StageManager.SwitchToStage(guid);
        }

        #region UnityMethods

        protected void Awake()
        {
            stageButtonScrollIndex = 0;

            parentScreen = GetComponentInParent<ScenarioScreen>();

            // Position to the right of the Scenario Screen
            var positionOffset = new Vector3(0.15f, 0.02f, 0.025f);
            var buttonGridPosition = new Vector3(0.0f, 0.0f, -0.009f);

            scenarioPathwayPromptData = new UIPlacementData()
            {
                HostTransform = optionSubPanel.transform,
                ButtonGridLayout = GridLayoutOrder.Vertical,
                PositionOffset = positionOffset,
                ButtonGridLocalPositionOverride = buttonGridPosition
            };

            scenarioPlayModePromptData = new UIPlacementData()
            {
                HostTransform = optionSubPanel.transform,
                ButtonGridLayout = GridLayoutOrder.Vertical,
                PositionOffset = positionOffset,
                ButtonGridLocalPositionOverride = buttonGridPosition,
            };

            // Appear above the Scenario Screen
            stopScenarioPromptData = new UIPlacementData()
            {
                HostTransform = playModePanel.transform, PositionOffset = new Vector3(0.0f, 0.0f, -0.025f)
            };
        }

        private void Update()
        {
            if (_scenarioManager != null)
            {
                // TODO: make this cleaner
                timeInPlayingScenarioText.text =
                    $"Time in Scenario  {_scenarioManager.TimeInPlayingScenario.ToString(TIME_FORMAT_COLON_SEPERATED)}";

                timeInPlayingStageText.text =
                    $"Time in Stage  {_scenarioManager.StageManager.TimeInPlayingStage.ToString(TIME_FORMAT_COLON_SEPERATED)}";
            }
        }

        private void OnDestroy()
        {
            _scenarioManager.ScenarioLoaded -= OnScenarioLoaded;
            _scenarioManager.ScenarioUnloaded -= OnScenarioUnloaded;
            _scenarioManager.ScenarioPlayModeSet -= OnScenarioPlayModeSet;
            _scenarioManager.NewScenarioPathway -= OnNewScenarioPathway;
        }

        #endregion

        #region PublicAPI

        /// <summary>
        /// Based on the last saved scenario, prompt the user for their play mode option.
        /// 
        /// Called via Unity Editor.
        /// </summary>
        public void PromptPlayMode()
        {
            BringDownPathwayPrompt();

            if (promptPlayModeTokenSource == null)
            {
                promptPlayModeTokenSource = new CancellationTokenSource();

                var allPlayModeButtons = new List<ButtonPromptInfo>();

                // Add a button for each scenario control type
                foreach (ScenarioControlTypes scenarioType in (ScenarioControlTypes[])Enum.GetValues
                             (typeof(ScenarioControlTypes)))
                {
                    allPlayModeButtons.Add
                    (
                        new ButtonPromptInfo()
                        {
                            buttonText = scenarioType.GetEnumDescription(),
                            onPressAction = () =>
                            {
                                _scenarioManager.SetPlayMode(scenarioType, true);

                                promptPlayModeTokenSource.Dispose();
                                promptPlayModeTokenSource = null;
                            }
                        }
                    );
                }

                scenarioPlayModePromptData.WindowSize = new Vector2
                    ((int)PromptManager.WindowStates.Narrow + 3, (5 * allPlayModeButtons.Count) + 15);

                EventBus.Publish
                (
                    new ShowCancellablePromptEvent
                    (
                        promptPlayModeTokenSource.Token,
                        "Select Play Mode",
                        "", // No main text
                        allPlayModeButtons,
                        scenarioPlayModePromptData
                    )
                );
            }
            else
            {
                BringDownPlayModePrompt();
            }
        }

        /// <summary>
        /// Based on the last saved scenario, ask the user what pathway they want their scenario to be in.
        /// 
        /// Called via Unity Editor.
        /// </summary>
        public void PromptCurrentScenarioPathways()
        {
            BringDownPlayModePrompt();

            PromptScenarioPathway(_scenarioManager.LastSavedScenario.pathways, scenarioPathwayPromptData);
        }

        /// <summary>
        /// Sets the subscreen to be the Scenario Options screen.
        /// 
        /// Called via Unity Editor.
        /// </summary>
        public void OnOptionTabButtonClicked()
        {
            optionSubPanel.SetActive(!optionSubPanel.activeInHierarchy);

            if (optionSubPanel.activeInHierarchy)
            {
                _scenarioManager.SetPathway(_scenarioManager.SelectedPathway, true);
                _scenarioManager.SetPlayMode(_scenarioManager.SelectedPlayMode, true);
            }
            else
            {
                BringDownPathwayPrompt();
                BringDownPlayModePrompt();
            }
        }

        /// <summary>
        /// Cancel button for the Options subscreen.
        /// 
        /// Called via Unity Editor.
        /// </summary>
        public void OnCancelOptionButtonClicked()
        {
            optionSubPanel.SetActive(false);

            BringDownPathwayPrompt();
            BringDownPlayModePrompt();
        }

        private UniTask<bool> PromptStopSessionAsync()
        {
            UniTaskCompletionSource<bool> answerPromise = new UniTaskCompletionSource<bool>();

            if (answerPromptTokenSource == null)
            {
                answerPromptTokenSource = new CancellationTokenSource();

                // TODO Need to externalize text
                // Display a message to the user that the scenario must be stopped in
                // order to set these values
                var allPathwayButtons = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = "Yes",
                        onPressAction = async () =>
                        {
                            await _scenarioManager.StopScenarioAsync();

                            // HACK The BoundControl will not show up correctly if you go from Stop to Edit quickly, 
                            // so this delay exists just to avoid those from occurring too closely to each other
                            // The same thing can happen in a session if the host presses the buttons quickly
                            await UniTask.Delay(100, true);

                            answerPromise.TrySetResult(true);

                            // Bring down the prompt
                            answerPromptTokenSource.Cancel();
                            answerPromptTokenSource.Dispose();
                            answerPromptTokenSource = null;
                        }
                    },
                    new ButtonPromptInfo()
                    {
                        buttonText = "No",
                        onPressAction = () =>
                        {
                            // Bring down the prompt
                            answerPromptTokenSource.Cancel();
                            answerPromptTokenSource.Dispose();
                            answerPromptTokenSource = null;

                            answerPromise.TrySetResult(false);
                        }
                    },
                };

                EventBus.Publish
                (
                    new ShowCancellablePromptEvent
                    (
                        answerPromptTokenSource.Token,
                        "",
                        "Are you sure you want to stop the scenario and enter Edit Mode? Doing so will end the scenario and you will need to start over.",
                        allPathwayButtons,
                        stopScenarioPromptData
                    )
                );
            }
            else
            {
                answerPromise.TrySetResult(false);
            }

            return answerPromise.Task;
        }

        private void SetScreenTab(bool state)
        {
            playModePanel.SetActive(state);
        }

        /// <summary>
        ///  Called from Unity Editor
        /// </summary>
        public void OnControlsTabButtonClicked()
        {
            uiEventBus.Publish(new SettingActiveSubScreenEvent(parentScreen.ScreenObjectType, SubScreenState.ControlsTab));
        }

        /// <summary>
        ///  Called from Unity Editor
        /// </summary>
        public void OnAboutScenarioTabButtonClicked()
        {
            uiEventBus.Publish(new SettingActiveSubScreenEvent(parentScreen.ScreenObjectType, SubScreenState.AboutScenarioTab));
        }

        public void SwitchScenarioSubscreens(SubScreenState subScreenStateToSwitchTo)
        {
            controlsButtonComponent.Highlight(subScreenStateToSwitchTo == SubScreenState.ControlsTab);
            aboutButtonComponent.Highlight(subScreenStateToSwitchTo == SubScreenState.AboutScenarioTab);
        }

        public void ResetButtonText()
        {
            _scenarioManager.SetPlayMode(ScenarioControlTypes.Automated, false);
            _scenarioManager.SetPathway(PathwayData.DefaultPathway(), false);
        }

        public void RefreshPlayModeText(ScenarioControlTypes type)
        {
            UpdatePlayModeButtonText(type);
        }

        public void RefreshPathwayButton(string pathway)
        {
            selectPathwayButtonComponent.SetButtonText(pathway);
        }

        private void UpdatePathwayButtonText(PathwayData pathway)
        {
            // Update the text of the button with the pathway text
            if (pathway?.pathwayDisplayName != null)
            {
                selectPathwayButtonComponent.SetButtonText(pathway.pathwayDisplayName);
            }
            else
            {
                // TODO Externalize default pathway text?
                selectPathwayButtonComponent.SetButtonText("Default");
            }
        }

        private void UpdatePlayModeButtonText(ScenarioControlTypes type)
        {
            selectPlayModeButtonComponent.SetButtonText(type.GetEnumDescription());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startActiveIndex"></param>
        public void OrganizeStageButtons(int startActiveIndex)
        {
            for (int i = 0; i < stageButtonsList.Count; ++i)
            {
                stageButtonsList[i].buttonObjectReference.transform.SetParent(stageButtonGrid.transform);
                stageButtonsList[i].buttonObjectReference.transform.localRotation = Quaternion.identity;
                bool active = i >= startActiveIndex && i < (startActiveIndex + buttonsToDisplay);
                stageButtonsList[i].buttonObjectReference.SetActive(active);
            }

            stageButtonGrid.UpdateCollection();

            if (lastReceivedStagesList != null)
            LockUnlockStageButtons(lastReceivedStagesList);
            else
            {
                Debug.Log("NRE Testing temporary: NULL stage list.");
            }
        }

        public void TearDownStageButtonGrid()
        {
            stageButtonScrollIndex = 0;

            foreach (var stageButton in stageButtonsList)
            {
                stageButton.buttonObjectReference.OnButtonClickedGuid -= OnStageButtonClickedGuid;
                Destroy(stageButton.buttonObjectReference.gameObject);
            }

            stageButtonsList.Clear();
        }

        /// <summary>
        /// Sets the button for the input stage index highlighted, and un-highlights others.
        /// </summary>
        /// <param name="index"></param>
        public void HighlightStageButton(int index)
        {
            for (int i = 0; i < stageButtonsList.Count; ++i)
            {
                stageButtonsList[i].buttonObjectReference.Highlight(i == index);
            }
        }

        public void SetAllStageButtonsDisabledState(bool state)
        {
            foreach (ScenarioScreen.StageButton currentStageButton in stageButtonsList)
            {
                currentStageButton.buttonObjectReference.IsDisabled(state, state);
            }
        }

        public void AddStageButton(ScenarioScreen.StageButton newStageButton)
        {
            stageButtonsList.Add(newStageButton);

            newStageButton.buttonObjectReference.OnButtonClickedGuid += OnStageButtonClickedGuid;
        }

        public void HighlightActiveStageButton(Guid guid)
        {
            int index = 0;

            foreach (var stageButton in stageButtonsList)
            {
                if (stageButton._guid == guid)
                {
                    HighlightStageButton(index);

                    // Make the button appear at the center of the list, unless it's the first or last button in the list
                    if (index > 0 && index < stageButtonsList.Count - 1)
                    {
                        stageButtonScrollIndex = index - 1;

                        OrganizeStageButtons(stageButtonScrollIndex);
                    }
                }

                index++;
            }
        }

        public void HidePlayModePanel()
        {
            selectPlayModeParent.SetActive(false);
        }

        public void ShowPlayModePanel()
        {
            selectPlayModeParent.SetActive(true);
        }

        
        /// <summary>
        /// Method responsible for locking stages in some scenarios where you don't want the user to go back or further. It uses the data from the ScenarioConfigurationATC to know which stage to lock
        ///
        /// WARNING: The correct way of disabling buttons is not being used in this method because it's not compatible with the way the stage buttons were implemented. (using InteractableStates / Disabled)
        /// Instead, we're just disabling the colliders and changing the TMP_TEXT color directly for a grey one. There is some room for improvement here, but we didn't have time to do it. 
        /// </summary>
        /// <param name="stagesToLock"></param>
        
        public void LockUnlockStageButtons(string[] stagesToLock)
        {
            if (stagesToLock == null)
                return;

            //Check if we are in play mode fist, 
           lastReceivedStagesList = stagesToLock;

                Debug.Log("Will unlock all stages first");
                UnlockAllStages();
                
               Debug.Log("[STAGELOCK] Trying to lock/unlock stages");
                foreach (string stageGuidAsString in stagesToLock)
                {
                    Debug.Log("[STAGELOCK] Trying to lock/unlock stage number " + stageGuidAsString);
                    for (int i = 0; i < stageButtonsList.Count; i++)
                    {
                        if (stageButtonsList[i]._guid.ToString() == stageGuidAsString)
                        {
                            Debug.Log("[STAGELOCK] Will disable the button " + stageGuidAsString);

                            stageButtonsList[i].buttonObjectReference.gameObject.GetComponent<Collider>().enabled = false;
                            stageButtonsList[i].buttonObjectReference.gameObject.GetComponent<Interactable>().enabled = false;
                            stageButtonsList[i].buttonObjectReference.gameObject.GetComponentInChildren<TMP_Text>().color =
                                Color.gray;
                            //stageButtonsList[i].buttonObjectReference.gameObject.GetComponent<Interactable>().SetState(InteractableStates.InteractableStateEnum.Disabled, true);

                            Debug.Log("[STAGELOCK]For the stage  " + stageGuidAsString + " my values are");
                            foreach (var state in stageButtonsList[i]
                                         .buttonObjectReference.gameObject.GetComponent<Interactable>()
                                         .States.StateList)
                            {
                                Debug.Log
                                    ("[STAGELOCK]State: " + state.Name + " - " + state.Value + " - Index: " + state.ActiveIndex);
                            }
                        }
                    }
                }
        }

        public void UnlockAllStages()
        {
            for (int i = 0; i < stageButtonsList.Count; i++)
            {
                Debug.Log("[STAGELOCK] Will Enable the button " + stageButtonsList[i]._guid);
                //stageButtonsList[i].buttonObjectReference.gameObject.GetComponent<Interactable>().SetState(InteractableStates.InteractableStateEnum.Disabled, false);
                stageButtonsList[i].buttonObjectReference.gameObject.GetComponent<Collider>().enabled = true;
                stageButtonsList[i].buttonObjectReference.gameObject.GetComponent<Interactable>().enabled = true;
                stageButtonsList[i].buttonObjectReference.gameObject.GetComponentInChildren<TMP_Text>().color = Color.white;
            }
        }

        #endregion

        #region PrivateMethods

        private void PromptScenarioPathway(IEnumerable<PathwayData> pathways, UIPlacementData promptPlacementData)
        {
            if (pathways == null || pathways.Count() == 0)
            {
                _scenarioManager.SetPathway(null, true);
            }
            else if (pathways.Count() == 1)
            {
                _scenarioManager.SetPathway(pathways.First(), true);
            }
            // There are two or more choices so actually display the prompt
            else
            {
                if (promptCurrentScenarioPathwayTokenSource == null)
                {
                    promptCurrentScenarioPathwayTokenSource = new CancellationTokenSource();

                    var allPathwayButtons = new List<ButtonPromptInfo>();

                    // Add a button for each pathway data
                    foreach (PathwayData currentPathway in pathways)
                    {
                        allPathwayButtons.Add
                        (
                            new ButtonPromptInfo()
                            {
                                buttonText = currentPathway.pathwayDisplayName,
                                onPressAction = () =>
                                {
                                    _scenarioManager.SetPathway(currentPathway, true);

                                    promptCurrentScenarioPathwayTokenSource.Dispose();
                                    promptCurrentScenarioPathwayTokenSource = null;
                                }
                            }
                        );
                    }

                    promptPlacementData.WindowSize = new Vector2
                        ((int)PromptManager.WindowStates.Narrow + 3, (5 * allPathwayButtons.Count) + 15);

                    EventBus.Publish
                    (
                        new ShowCancellablePromptEvent
                        (
                            promptCurrentScenarioPathwayTokenSource.Token,
                            "Select Pathway",
                            "", // No main text
                            allPathwayButtons,
                            promptPlacementData
                        )
                    );
                }
                else
                {
                    BringDownPathwayPrompt();
                }
            }
        }

        private void BringDownPlayModePrompt()
        {
            if (promptPlayModeTokenSource != null)
            {
                promptPlayModeTokenSource.Cancel();
                promptPlayModeTokenSource.Dispose();

                promptPlayModeTokenSource = null;
            }
        }

        private void BringDownPathwayPrompt()
        {
            if (promptCurrentScenarioPathwayTokenSource != null)
            {
                promptCurrentScenarioPathwayTokenSource.Cancel();
                promptCurrentScenarioPathwayTokenSource.Dispose();

                promptCurrentScenarioPathwayTokenSource = null;
            }
        }

        private void SetAboutText(string sessionName)
        {
            aboutScenarioText.text = $"Name: {sessionName}";
        }

        private string TimeFormatVariableSpelledOut(TimeSpan time)
        {
            return (time.Hours != 0 ? "h\\ \\h\\ " : "") + (time.Minutes != 0 ? "m\\ \\m\\ " : "") + "s\\ \\s";
        }

        #endregion

        public void ScrollUp()
        {
            if (stageButtonScrollIndex > 0)
                stageButtonScrollIndex -= 1;

            OrganizeStageButtons(stageButtonScrollIndex);
        }

        public void ScrollDown()
        {
            if (stageButtonScrollIndex < stageButtonsList.Count - buttonsToDisplay)
                stageButtonScrollIndex += 1;

            OrganizeStageButtons(stageButtonScrollIndex);
        }
    }
}