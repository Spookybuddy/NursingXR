namespace GIGXR.Platform.Scenarios
{
    using Cysharp.Threading.Tasks;
    using Data;
    using EventArgs;
    using GigAssets;
    using Stages;
    using System;
    using System.Threading;

    /// <summary>
    /// For use in Editor only.
    /// </summary>
    public class EditorScenarioManager : IScenarioManager
    {
        public EditorScenarioManager
        (
            IGigAssetManager assetManager
        )
        {
            AssetManager = assetManager;
        }

        public ScenarioTimer ActiveScenarioTimer => throw new NotImplementedException();

        public ScenarioStatus ScenarioStatus => throw new NotImplementedException();

        public TimeSpan TimeInPlayingScenario => throw new NotImplementedException();

        public TimeSpan TimeInPlayingStage => throw new NotImplementedException();

        public TimeSpan TimeInSimulation => throw new NotImplementedException();

        public IGigAssetManager AssetManager { get; }

        public IStageManager StageManager => throw new NotImplementedException();

        public Scenario LastSavedScenario => throw new NotImplementedException();

        public PathwayData SelectedPathway => throw new NotImplementedException();

        public bool IsSwitchingStatus => throw new NotImplementedException();

        public bool IsSavingScenario => throw new NotImplementedException();

        public bool IsScenarioLoaded => throw new NotImplementedException();

        public ScenarioControlTypes SelectedPlayMode => throw new NotImplementedException();

        public CancellationToken CurrentScenarioPlayCancellationToken => throw new NotImplementedException();

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

        public void AddSimulationTime(int milliseconds)
        {
            throw new NotImplementedException();
        }

        public UniTask<Scenario> ExportScenarioAsync(bool includeRuntimeAssets)
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> LoadScenarioAsync(Scenario scenario, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> PauseScenarioAsync()
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> PlayScenarioAsync()
        {
            throw new NotImplementedException();
        }

        public UniTask ResetScenario()
        {
            throw new NotImplementedException();
        }

        public UniTask<Scenario> SaveScenarioAsync(bool saveAssetData = true)
        {
            throw new NotImplementedException();
        }

        public void SetPathway(PathwayData givenPathway, bool newValue)
        {
            throw new NotImplementedException();
        }

        public void SetPlayMode(ScenarioControlTypes type, bool saveValue)
        {
            throw new NotImplementedException();
        }

        public void SetTimeOffsets(int scenarioOffsetMilliseconds, int stageOffsetMilliseconds)
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> StopScenarioAsync()
        {
            throw new NotImplementedException();
        }

        public void SyncScenarioTimer(int totalMillisecondsInSimulation, int totalMillisecondsInScenario, int totalMillisecondsInCurrentStage, int millisecondSyncOffset)
        {
            throw new NotImplementedException();
        }

        public UniTask TrySyncScenarioAsync(int currentServerTime, ScenarioSyncData syncData)
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> UnloadScenarioAsync()
        {
            throw new NotImplementedException();
        }
    }
}