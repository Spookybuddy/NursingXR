namespace GIGXR.Platform.HMD.UI
{
    using Core.DependencyInjection;
    using Core.DependencyValidator;
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Scenarios.Data;
    using GIGXR.Platform.UI;
    using Microsoft.MixedReality.Toolkit.UI;
    using Scenarios;
    using Scenarios.EventArgs;
    using System;
    using UnityEngine;

    public class ScenarioControlsController : BaseUiObject
    {
        [SerializeField, RequireDependency]
        private ButtonComponent pauseButton;

        private LabeledIcon pauseIconObject;

        [SerializeField, RequireDependency]
        private LabeledIconScriptableObject pauseIconInfo;

        [SerializeField, RequireDependency]
        private ButtonComponent stopButton;

        private LabeledIcon stopIconObject;

        [SerializeField, RequireDependency]
        private LabeledIconScriptableObject stopIconInfo;

        [SerializeField, RequireDependency]
        private LabeledIconScriptableObject playIconInfo;

        public event EventHandler<ScenarioButtonPressedArgs> ScenarioButtonPressed;

        public class ScenarioButtonPressedArgs : EventArgs
        {
            public ScenarioStatus nextStatus;

            public ScenarioButtonPressedArgs(ScenarioStatus status)
            {
                nextStatus = status;
            }
        }

        private IScenarioManager ScenarioManager { get; set; }

        //Scenario configuration controls
        private bool hidepauseButton;

        private void Awake()
        {
            pauseIconObject = pauseButton.GetComponent<LabeledIcon>();
            stopIconObject = stopButton.GetComponent<LabeledIcon>();

            stopIconObject.Configure(stopIconInfo);
        }

        private void OnDestroy()
        {
            ScenarioManager.ScenarioLoaded -= OnScenarioLoaded;
        }

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManager)
        {
            ScenarioManager = scenarioManager;
        }

        private void OnScenarioLoaded(object sender, ScenarioLoadedEventArgs e)
        {
            pauseIconObject.Configure(playIconInfo);
            SetPauseButtonDisableState(false);
            SetStopButtonDisableState(true);
        }

        public void HidePauseButton()
        {
            hidepauseButton = true;

            if (ScenarioManager.ScenarioStatus == ScenarioStatus.Playing)
                SetPauseButtonDisableState(true);
        }

        public void ShowPauseButton()
        {
            hidepauseButton = false;

            if (ScenarioManager.ScenarioStatus == ScenarioStatus.Playing)
                SetPauseButtonDisableState(false);
        }

        public void HideAutomatedControls()
        {

        }

        public void SetAllButtonsDisableState(bool state)
        {
            SetPauseButtonDisableState(state);
            SetStopButtonDisableState(state);
        }

        public void SetPauseButtonDisableState(bool state)
        {
            pauseButton.SetInteractableTheme(InteractableStates.InteractableStateEnum.Disabled, state);
        }

        public void SetStopButtonDisableState(bool state)
        {
            stopButton.SetInteractableTheme(InteractableStates.InteractableStateEnum.Disabled, state);
        }

        /// <summary>
        /// Called via the Unity Editor
        /// </summary>
        public async void OnPauseButtonClicked()
        {
            if (ScenarioManager.ScenarioStatus == ScenarioStatus.Playing)
            {
                SetAllButtonsDisableState(true);

                ScenarioButtonPressed?.Invoke(this, new ScenarioButtonPressedArgs(ScenarioStatus.Paused));

                // Change the icons right away so that the UI shows the reaction to the user interaction
                pauseIconObject.Configure(playIconInfo);

                _ = UniTask.Create(async () =>
                {
                    await UniTask.WaitUntil(() => ScenarioManager.ScenarioStatus == ScenarioStatus.Paused);

                    // While the scenario is paused, we will still need access to play and stop buttons
                    SetAllButtonsDisableState(false);
                });

                var wentIntoPauseMode = await ScenarioManager.PauseScenarioAsync();

                // In the event that the UI did not go into Pause mode, change the icons back
                if (!wentIntoPauseMode)
                {
                    pauseIconObject.Configure(pauseIconInfo);
                }

                return;
            }

            if (ScenarioManager.ScenarioStatus == ScenarioStatus.Paused ||
                ScenarioManager.ScenarioStatus == ScenarioStatus.Stopped)
            {
                SetAllButtonsDisableState(true);

                ScenarioButtonPressed?.Invoke(this, new ScenarioButtonPressedArgs(ScenarioStatus.Playing));

                // Change the icons right away so that the UI shows the reaction to the user interaction
                //This will only be called if there is an ATC called ScenarioConfiguration in the scene that has called the option to hide the pause buttons
                if (!hidepauseButton)
                {
                    pauseIconObject.Configure(pauseIconInfo);
                }

                _ = UniTask.Create(async () =>
                {
                    await UniTask.WaitUntil(() => ScenarioManager.ScenarioStatus == ScenarioStatus.Playing);

                    // When playing, we need access to both buttons to pause and stop the scenario
                    SetAllButtonsDisableState(false);

                    //This will only be called if there is an ATC called ScenarioConfiguration in the scene that has called the option to hide the pause buttons
                    if (hidepauseButton)
                    {
                        SetPauseButtonDisableState(true);
                    }

                });

                var wentIntoPlayMode = await ScenarioManager.PlayScenarioAsync();

                // In the event that the UI did not go into Play mode, change the icons back
                if (!wentIntoPlayMode)
                {
                    pauseIconObject.Configure(playIconInfo);
                    SetPauseButtonDisableState(false);
                }

                return;
            }
        }

        /// <summary>
        /// Called via the Unity Editor
        /// </summary>
        public async void OnStopButtonClicked()
        {
            if (ScenarioManager.ScenarioStatus == ScenarioStatus.Playing ||
                ScenarioManager.ScenarioStatus == ScenarioStatus.Paused)
            {
                SetAllButtonsDisableState(true);

                ScenarioButtonPressed?.Invoke(this, new ScenarioButtonPressedArgs(ScenarioStatus.Stopped));

                // Change the icons right away so that the UI shows the reaction to the user interaction
                pauseIconObject.Configure(playIconInfo);

                _ = UniTask.Create(async () =>
                {
                    await UniTask.WaitUntil(() => ScenarioManager.ScenarioStatus == ScenarioStatus.Stopped);

                    // You can't click the stop button after clicking the stop button, so only bring the pause/play button back
                    SetPauseButtonDisableState(false);
                });

                var wentIntoStopMode = await ScenarioManager.StopScenarioAsync();

                // In the event that the UI did not go into Play mode, change the icons back
                if (!wentIntoStopMode)
                {
                    pauseIconObject.Configure(pauseIconInfo);
                }

                return;
            }
        }

        protected override void SubscribeToEventBuses()
        {
            ScenarioManager.ScenarioLoaded += OnScenarioLoaded;
        }
    }
}