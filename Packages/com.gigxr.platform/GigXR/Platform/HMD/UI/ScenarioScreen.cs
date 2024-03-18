namespace GIGXR.Platform.Core.UI
{
    using GIGXR.Platform.AppEvents.Events.Session;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Core.DependencyValidator;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.HMD.UI;
    using GIGXR.Platform.Scenarios;
    using GIGXR.Platform.Scenarios.Data;
    using GIGXR.Platform.Scenarios.EventArgs;
    using GIGXR.Platform.Scenarios.Stages.Data;
    using GIGXR.Platform.Scenarios.Stages.EventArgs;
    using GIGXR.Platform.Sessions;
    using System;
    using System.Linq;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Class to interface between the scenario screen buttons and the scenario manager without cross referencing between the two prefabs.
    /// </summary>
    public class ScenarioScreen : BaseScreenObject
    {
        public class StageButton
        {
            internal ButtonObjectWithArgs buttonObjectReference;
            internal Guid _guid;

            public StageButton
            (
                ButtonObjectWithArgs buttonObjectReference,
                Guid guid
            )
            {
                this.buttonObjectReference = buttonObjectReference;
                _guid = guid;
            }
        }

        [Tooltip("The prefab that will be spawn the actual 3D UI object with the scenario controls.")]
        [RequireDependency]
        [SerializeField]
        private GameObject scenarioScreenObjectPrefab;

        private IScenarioManager _scenarioManager;
        private ISessionManager _sessionManager;

        [RequireDependency, SerializeField] private GameObject stageButtonPrefab;

        [HideInInspector] public ScenarioScreenObject _scenarioScreenObject;

        public ScenarioControlsController ScenarioController
        {
            get
            {
                if (_scenarioController == null)
                    _scenarioController = GetComponentInChildren<ScenarioControlsController>(true);

                return _scenarioController;
            }
        }

        private ScenarioControlsController _scenarioController;

        public override ScreenType ScreenObjectType => ScreenType.ScenarioManagement;

        protected override void OnEnable()
        {
            GameObject newScreen = Instantiate(scenarioScreenObjectPrefab, transform);

            _scenarioScreenObject = newScreen.GetComponentInChildren<ScenarioScreenObject>(true);

            _scenarioScreenObject.SetScenarioControlsController(ScenarioController);

            // Hide the screen so that the EventBus and Message system controls object activation from here on out
            newScreen.SetActive(false);

            Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _scenarioManager.StageManager.StageSwitched -= OnStageSwitched;

            _scenarioManager.ScenarioPlaying -= OnScenarioPlaying;
            _scenarioManager.ScenarioStopped -= OnScenarioStopped;
            _scenarioManager.ScenarioPaused -= OnScenarioPaused;

            _scenarioManager.StageManager.StageSwitched -= OnStageSwitched;

            EventBus.Unsubscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
            EventBus.Unsubscribe<LeftSessionEvent>(OnLeftSessionEvent);

            uiEventBus.Unsubscribe<SwitchedActiveSubScreenEvent>(OnSwitchedActiveSubScreenEvent);
        }

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManagerReference, ISessionManager sessionManager)
        {
            _scenarioManager = scenarioManagerReference;
            _sessionManager = sessionManager;

            _scenarioManager.StageManager.StageSwitched += OnStageSwitched;

            _scenarioManager.ScenarioPlaying += OnScenarioPlaying;
            _scenarioManager.ScenarioStopped += OnScenarioStopped;
            _scenarioManager.ScenarioPaused += OnScenarioPaused;
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<JoinedSessionEvent>(OnJoinedSessionEvent);
            EventBus.Subscribe<LeftSessionEvent>(OnLeftSessionEvent);

            uiEventBus.Subscribe<SwitchedActiveSubScreenEvent>(OnSwitchedActiveSubScreenEvent);
        }

        private void OnStageSwitched(object sender, StageSwitchedEventArgs e)
        {
            _scenarioScreenObject.HighlightActiveStageButton(e.NewStageID);
        }

        private void TrySetOptionData()
        {
            // Set the pathway at start for the host only
            if (_sessionManager.IsHost)
            {
                // If the pathway isn't set yet, use the defaults
                if (string.IsNullOrEmpty(_sessionManager.PathwayInfo))
                {
                    // Default to the first option in the list per design
                    if (_scenarioManager.LastSavedScenario.pathways != null)
                        _scenarioManager.SetPathway(_scenarioManager.LastSavedScenario.pathways.FirstOrDefault(), true);
                    else
                        _scenarioManager.SetPathway(PathwayData.DefaultPathway(), true);
                }
                else
                {
                    _scenarioScreenObject.RefreshPathwayButton(PathwayData.Create(_sessionManager.PathwayInfo).pathwayDisplayName);
                }

                _scenarioScreenObject.RefreshPlayModeText(_sessionManager.PlayMode);
            }
        }

        private void OnScenarioPlaying(object sender, ScenarioPlayingEventArgs e)
        {
            _scenarioScreenObject.SetAllStageButtonsDisabledState(false);
        }

        private void OnScenarioStopped(object sender, ScenarioStoppedEventArgs e)
        {
            _scenarioScreenObject.SetAllStageButtonsDisabledState(false);
        }

        private void OnScenarioPaused(object sender, ScenarioPausedEventArgs e)
        {
            _scenarioScreenObject.SetAllStageButtonsDisabledState(false);
        }

        private void OnJoinedSessionEvent(JoinedSessionEvent obj)
        {
            SetupStageButtonGrid();
        }

        private void OnLeftSessionEvent(LeftSessionEvent obj)
        {
            _scenarioScreenObject.TearDownStageButtonGrid();

            _scenarioScreenObject.ResetButtonText();
        }

        private void OnSwitchedActiveSubScreenEvent(SwitchedActiveSubScreenEvent @event)
        {
            if (@event.TargetScreen == ScreenObjectType)
            {
                _scenarioScreenObject.SwitchScenarioSubscreens(@event.SubScreenStateToSwitchTo);
            }
        }

        private void SetupStageButtonGrid()
        {
            var stages = _scenarioManager.StageManager.Stages.ToList();

            for (int index = 0; index < stages.Count; index++)
            {
                var stage = new { index = index, value = stages[index] };
                _scenarioScreenObject.AddStageButton(GenerateStageButton(stage.value, stage.index));
            }

            // OrganizeStageButtons(stageButtonsList.Count - buttonsToDisplay < 0 ? 0 : stageButtonsList.Count - buttonsToDisplay);
            _scenarioScreenObject.HighlightStageButton(0);
            _scenarioScreenObject.OrganizeStageButtons(0);
        }

        private StageButton GenerateStageButton(Stage stage, int index)
        {
            GameObject stageButtonObject = Instantiate(stageButtonPrefab);

            ButtonObjectWithArgs buttonObject = stageButtonObject.GetComponent<ButtonObjectWithArgs>();
            TextMeshProUGUI buttonTextField = stageButtonObject.GetComponentInChildren<TextMeshProUGUI>();

            // Get stage data
            Guid associatedStageGuid = stage.StageId;
            string stageTitle = stage.stageTitle;

            // Wire up button clicks
            buttonObject.SetArgsType(ArgsType.Guid);
            buttonObject.SetArgs(new ButtonObjectWithArgs.Args(associatedStageGuid));

            // Set stage button text to be the title
            int maxTitleLength = 18;
            string formattedTitle = (index + 1) + ") " + stageTitle;

            if (stageTitle.Length > maxTitleLength)
            {
                formattedTitle = formattedTitle.Substring(0, maxTitleLength) + "...";
            }

            buttonTextField.SetText(formattedTitle);

            StageButton stageButton = new StageButton(buttonObject, associatedStageGuid);

            return stageButton;
        }
    }
}
