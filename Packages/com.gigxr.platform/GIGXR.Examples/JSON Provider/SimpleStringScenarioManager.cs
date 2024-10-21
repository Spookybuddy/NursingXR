using Cysharp.Threading.Tasks;
using GIGXR.Platform;
using GIGXR.Platform.Core;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.EventArgs;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.Stages;
using GIGXR.Platform.Utilities;
using GIGXR.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using UnityEngine;

public class SimpleStringScenarioManager : IScenarioManager
{
    public Type ScenarioClassType => typeof(SimpleStringProvider);

    public ScenarioTimer ActiveScenarioTimer => throw new NotImplementedException();

    public ScenarioStatus ScenarioStatus => ScenarioStatus.Playing;

    public TimeSpan TimeInPlayingScenario => TimeSpan.Zero;

    public TimeSpan TimeInSimulation => TimeSpan.Zero;

    public IGigAssetManager AssetManager => _assetManager;
    private IGigAssetManager _assetManager;

    public IStageManager StageManager => _stageManager;
    private IStageManager _stageManager;

    public IScenarioData LastSavedScenario => _scenarioData;
    private SimpleStringClass _scenarioData;

    public PathwayData SelectedPathway => new PathwayData();

    public ScenarioControlTypes SelectedPlayMode => ScenarioControlTypes.Automated;

    public bool IsSwitchingStatus => false;

    public bool IsSavingScenario { get; private set; }

    public bool IsScenarioLoaded => true;

    public CancellationToken CurrentScenarioPlayCancellationToken
    {
        get
        {
            if (currentScenarioPlayCancellationSource == null)
                return CancellationToken.None;

            return currentScenarioPlayCancellationSource.Token;
        }
    }

    private CancellationTokenSource currentScenarioPlayCancellationSource;

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

    public SimpleStringScenarioManager()
    {
        _stageManager = new StageManager();
        _assetManager = new GigAssetManager
                (
                    GameObject.FindObjectOfType<BasicCalibrationRootComponent>(),
                    null,
                    null,
                    _stageManager,
                    UnityScheduler.Instance,
                    GameObject.FindObjectOfType<GIGXRCore>().DependencyProvider.GetDependency<ProfileManager>(),
                    null,
                    null
                );
    }

    public void AddSimulationTime(int milliseconds)
    {
    }

    public UniTask<IScenarioData> ExportScenarioAsync(bool includeRuntimeAssets)
    {
        IScenarioData data = new SimpleStringClass() { name = _scenarioData.name };

        return UniTask.FromResult(data);
    }

    public UniTask<bool> LoadScenarioAsync(JObject scenarioData, CancellationToken cancellationToken)
    {
        // Deserialize the JSON data into the known class and then start using the data
        var scenario = scenarioData.ToObject<SimpleStringClass>(DefaultNewtonsoftJsonConfiguration.JsonSerializer);

        _scenarioData = scenario;

        Debug.Log($"Scenario has been loaded. Name: {scenario.name}");

        return UniTask.FromResult(true);
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
        return UniTask.CompletedTask;
    }

    public UniTask<IScenarioData> SaveScenarioAsync(bool saveAssetData = true)
    {
        if (LastSavedScenario == null)
        {
            GIGXR.Platform.Utilities.Logger.Warning($"Cannot save scenario since {nameof(LastSavedScenario)} is null", nameof(ScenarioManager));

            UniTask.FromException(new Exception("No LastSavedScenario"));
        }

        IsSavingScenario = true;

        // TODO Update any data on LastSavedScenario here

        IsSavingScenario = false;

        return UniTask.FromResult(LastSavedScenario);
    }

    public void SetPathway(PathwayData givenPathway, bool newValue)
    {
    }

    public void SetPlayMode(ScenarioControlTypes type, bool saveValue)
    {
    }

    public void SetTimeOffsets(int scenarioOffsetMilliseconds, int stageOffsetMilliseconds)
    {
    }

    public UniTask<bool> StopScenarioAsync()
    {
        return UniTask.FromResult(true);
    }

    public void SyncScenarioTimer(int totalMillisecondsInSimulation, int totalMillisecondsInScenario, int totalMillisecondsInCurrentStage, int millisecondSyncOffset)
    {
    }

    public UniTask TrySyncScenarioAsync(int currentServerTime, ScenarioSyncData syncData)
    {
        return UniTask.CompletedTask;
    }

    public UniTask<bool> UnloadScenarioAsync()
    {
        return UniTask.FromResult(true);
    }
}
