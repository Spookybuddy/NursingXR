namespace GIGXR.Platform.Scenarios
{
    using Cysharp.Threading.Tasks;
    using Data;
    using EventArgs;
    using GigAssets;
    using GIGXR.Platform.Scenarios.Stages;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Threading;

    /// <summary>
    /// Responsible for managing the lifecycle of a scenario.
    /// A scenario is the aggregate of assets, stages, and rules (triggers, conditions, and actions).
    /// </summary>
    public interface IScenarioManager
    {
        Type ScenarioClassType { get; }

        ScenarioTimer ActiveScenarioTimer { get; }

        public event EventHandler<ScenarioStatusChangedEventArgs> ScenarioStatusChanged;
        public event EventHandler<ScenarioUnloadedEventArgs> ScenarioUnloaded;
        public event EventHandler<ScenarioLoadingEventArgs> ScenarioLoading;
        public event EventHandler<ScenarioLoadedEventArgs> ScenarioLoaded;
        public event EventHandler<ScenarioPlayingEventArgs> EarlyScenarioPlaying;
        public event EventHandler<ScenarioPlayingEventArgs> ScenarioPlaying;
        public event EventHandler<ScenarioPlayingEventArgs> LateScenarioPlaying;
        public event EventHandler<ScenarioPausedEventArgs> ScenarioPaused;
        public event EventHandler<ScenarioStoppedEventArgs> ScenarioStopped;
        public event EventHandler<ScenarioUnloadingEventArgs> ScenarioUnloading;
        public event EventHandler<ScenarioResetEventArgs> ScenarioReset;

        public event EventHandler<ScenarioPathwaySetEventArgs> NewScenarioPathway;

        public event EventHandler<ScenarioPlayModeSetEventArgs> ScenarioPlayModeSet;

        public ScenarioStatus ScenarioStatus { get; }

        public TimeSpan TimeInPlayingScenario { get; }
        public TimeSpan TimeInSimulation { get; }

        IGigAssetManager AssetManager { get; }
        IStageManager StageManager { get; }
        IScenarioData LastSavedScenario { get; }

        PathwayData SelectedPathway { get; }

        ScenarioControlTypes SelectedPlayMode { get; }

        bool IsSwitchingStatus { get; }
        bool IsSavingScenario { get; }
        bool IsScenarioLoaded { get; }

        CancellationToken CurrentScenarioPlayCancellationToken { get; }

        UniTask<bool> LoadScenarioAsync(JObject scenarioData, CancellationToken cancellationToken);
        UniTask<bool> UnloadScenarioAsync();
        UniTask<bool> PlayScenarioAsync();
        UniTask<bool> PauseScenarioAsync();
        UniTask<bool> StopScenarioAsync();
        UniTask ResetScenario();
        UniTask<IScenarioData> SaveScenarioAsync(bool saveAssetData = true);
        UniTask<IScenarioData> ExportScenarioAsync(bool includeRuntimeAssets);

        UniTask TrySyncScenarioAsync(int currentServerTime, ScenarioSyncData syncData);
        void SyncScenarioTimer(int totalMillisecondsInSimulation, int totalMillisecondsInScenario, int totalMillisecondsInCurrentStage, int millisecondSyncOffset);
        void AddSimulationTime(int milliseconds);
        void SetTimeOffsets(int scenarioOffsetMilliseconds, int stageOffsetMilliseconds);

        void SetPathway(PathwayData givenPathway, bool newValue);

        void SetPlayMode(ScenarioControlTypes type, bool saveValue);
    }
}