namespace GIGXR.Platform.Scenarios
{
    using Cysharp.Threading.Tasks;
    using Cysharp.Threading.Tasks.Linq;
    using Data;
    using GIGXR.Platform.Scenarios.EventArgs;
    using GigAssets;
    using GigAssets.Data;
    using GigAssets.EventArgs;
    using GIGXR.Platform.Core.FeatureManagement;
    using GIGXR.Platform.Core.Settings;
    using Stages;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using GIGXR.Platform.Utilities;
    using System.Threading;
    using System.ComponentModel;

    public enum ScenarioControlTypes
    {
        Automated,
        [Description("Semi-Manual")]
        SemiManual,
        [Description("Fully-Manual")]
        FullyManual
    }

    public class ScenarioManager : IScenarioManager
    {
        public ScenarioTimer ActiveScenarioTimer => scenarioTimer;

        private readonly ScenarioTimer scenarioTimer;

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

        private ScenarioStatus _scenarioStatus = ScenarioStatus.Unloaded;

        public ScenarioStatus ScenarioStatus
        {
            get
            {
                return _scenarioStatus;
            }

            private set
            {
                if (_scenarioStatus != value)
                {
                    ScenarioStatusChanged?.InvokeSafely
                        (this, new ScenarioStatusChangedEventArgs { OldStatus = _scenarioStatus, NewStatus = value });

                    _scenarioStatus = value;
                }
            }
        }

        public bool IsSwitchingStatus { get; private set; }

        public TimeSpan TimeInPlayingScenario => scenarioTimer.TimeInPlayingScenario;
        public TimeSpan TimeInPlayingStage => StageManager.TimeInPlayingStage;
        public TimeSpan TimeInSimulation => scenarioTimer.TimeInSimulation;

        public IGigAssetManager AssetManager { get; }
        public IStageManager StageManager { get; }
        public Scenario LastSavedScenario { get; private set; }

        private readonly IFeatureManager FeatureManager;

        public PathwayData SelectedPathway { get { return _selectedPathway; } }

        public bool IsSavingScenario { get; private set; }

        public bool IsScenarioLoaded
        {
            get
            {
                return ScenarioStatus == ScenarioStatus.Loaded ||
                       ScenarioStatus == ScenarioStatus.Playing ||
                       ScenarioStatus == ScenarioStatus.Paused ||
                       ScenarioStatus == ScenarioStatus.Stopped;
            }
        }

        public CancellationToken CurrentScenarioPlayCancellationToken
        {
            get
            {
                if(currentScenarioPlayCancellationSource == null)
                    return CancellationToken.None;
                
                return currentScenarioPlayCancellationSource.Token;
            }
        }

        private CancellationTokenSource currentScenarioPlayCancellationSource;

        public ScenarioControlTypes SelectedPlayMode => _selectedPlayMode;

        // TODO Might want to refactor sync stuff into a new class to use
        private int? lastSyncTimeStamp = null;

        private PathwayData _selectedPathway;

        private ScenarioControlTypes _selectedPlayMode;

        public ScenarioManager
        (
            IGigAssetManager assetManager,
            IStageManager stageManager,
            IFeatureManager featureManager
        )
        {
            AssetManager   = assetManager;
            StageManager   = stageManager;
            FeatureManager = featureManager;

            scenarioTimer = new ScenarioTimer(this, stageManager);

            GIGXR.Platform.Utilities.Logger.AddTaggedLogger(nameof(ScenarioManager), "Scenario Console");
        }

        public async UniTask<bool> LoadScenarioAsync(Scenario scenario, CancellationToken cancellationToken)
        {
            if (LastSavedScenario != null)
            {
                return false;
            }

            GIGXR.Platform.Utilities.Logger.Info($"Scenario data for {scenario.scenarioName} is now being loaded.", nameof(ScenarioManager));

            IsSwitchingStatus = true;

            ScenarioStatus = ScenarioStatus.Loading;
            ScenarioLoading?.InvokeSafely(this, new ScenarioLoadingEventArgs());

            LastSavedScenario      = scenario;
            Physics.autoSimulation = false;

            AssetManager.HideAll(HideAssetReasons.Loading);

            try
            {
                if (scenario.assets == null ||
                    scenario.assets.Count == 0)
                {
                    GIGXR.Platform.Utilities.Logger.Warning($"The Scenario {scenario.scenarioName} has 0 assets in the session plan.", nameof(ScenarioManager));
                }
                else
                {
                    // Iterate through the list of preset scenario ID and assign them to the Assets in the Scenario.
                    // Default to the assetId for assets which are missing a preset id.
                    foreach (var currentAsset in scenario.assets)
                    {
                        currentAsset.presetAssetId = scenario.presetAssetMappings.Find
                                                             ((t) => t.assetId == currentAsset.assetId)
                                                         ?.presetAssetId ??
                                                     currentAsset.AssetId.ToString();
                    }

                    var assetTypeIds = scenario.assets.Select(asset => asset.assetTypeId);

                    var uniqueAssetTypeIds = new HashSet<string>(assetTypeIds);

                    if (scenario.loadedAssetTypes != null)
                    {
                        uniqueAssetTypeIds.UnionWith(scenario.loadedAssetTypes);
                    }

                    await AssetManager.AssetTypeLoader.LoadAssetTypesAsync(uniqueAssetTypeIds);
                }
            }
            catch (Exception e)
            {
                GIGXR.Platform.Utilities.Logger.Error
                    (
                        $"Failed while trying to load asset types for Scenario {scenario.scenarioName}",
                        nameof(ScenarioManager),
                        e
                    );

                ScenarioStatus = ScenarioStatus.Unloaded;
                ScenarioUnloaded?.InvokeSafely(this, new ScenarioUnloadedEventArgs());

                IsSwitchingStatus = false;

                LastSavedScenario      = null;
                Physics.autoSimulation = true;

                throw;
            }

            cancellationToken.ThrowIfCancellationRequested();

            AssetManager.AssetInstantiated += OnRuntimeInstantiationDuringScenarioLoad;

            await AssetManager.LoadStagesAndInstantiateAssetsAsync(LastSavedScenario.stages, 
                                                                   LastSavedScenario.assets, 
                                                                   cancellationToken);

            AssetManager.AssetInstantiated -= OnRuntimeInstantiationDuringScenarioLoad;

            cancellationToken.ThrowIfCancellationRequested();

            ScenarioStatus = ScenarioStatus.Loaded;

            ScenarioLoaded?.InvokeSafely(this,
                                         new ScenarioLoadedEventArgs(LastSavedScenario.stages, 
                                                                     LastSavedScenario.assets, 
                                                                     LastSavedScenario.pathways));

            // Instantiation is complete, show the assets, due to the content marker, wait a second
            // so they overlap and the assets don't 'flash' in front of the host
            UniTask.Void(async () =>
            {
                await UniTask.Delay(2000, true);

                AssetManager.ShowAll(HideAssetReasons.Loading);
            });

            IsSwitchingStatus = false;

            GIGXR.Platform.Utilities.Logger.Info("Scenario data has finished loading.", nameof(ScenarioManager));

            return true;
        }

        /// <summary>
        /// When a runtime instantiation occurs during the scenario load, it is part of the loaded
        /// scenario (this handles runtime instantiations done OnAssetMounted).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRuntimeInstantiationDuringScenarioLoad(object sender, AssetInstantiatedEventArgs e)
        {
            if (e.IsRuntimeInstantiation)
            {
                GIGXR.Platform.Utilities.Logger.Warning
                    (
                        "Runtime instantiation performed during scenario load. " +
                        "This asset will not persist through reloads. Consider performing runtime " +
                        "instantiations in ScenarioPlaying / ScenarioStopped event listeners instead.",
                        nameof(ScenarioManager)
                    );
            }
        }

        public async UniTask<bool> UnloadScenarioAsync()
        {
            if (LastSavedScenario == null)
            {
                return false;
            }

            GIGXR.Platform.Utilities.Logger.Info("Scenario is now being unloaded.", nameof(ScenarioManager));

            IsSwitchingStatus = true;

            ScenarioStatus = ScenarioStatus.Unloading;
            ScenarioUnloading?.InvokeSafely(this, new ScenarioUnloadingEventArgs());

            AssetManager.DestroyAll();
            AssetManager.StageManager.RemoveAllStages();

            await AssetManager.AssetTypeLoader.UnloadAllAssetTypesAsync();
            await AssetManager.AddressablesGameObjectLoader.UnloadAllAddressableGameObjectsAsync();

            LastSavedScenario      = null;
            Physics.autoSimulation = true;

            lastSyncTimeStamp = null;

            ScenarioStatus = ScenarioStatus.Unloaded;
            ScenarioUnloaded?.InvokeSafely(this, new ScenarioUnloadedEventArgs());

            ClearAllEventHandlers();

            IsSwitchingStatus = false;

            GIGXR.Platform.Utilities.Logger.Info("Scenario has been unloaded.", nameof(ScenarioManager));

            return true;
        }

        private void ClearAllEventHandlers()
        {
            ScenarioUnloaded = null;
        }

        public async UniTask<bool> PlayScenarioAsync()
        {
            // Do not go into a new status changing event if there is already another event going on
            if (IsSwitchingStatus || IsSavingScenario)
                return false;

            // Cannot play a scenario that is already being played
            if (ScenarioStatus == ScenarioStatus.Playing)
                return false;

            GIGXR.Platform.Utilities.Logger.Info("Scenario is switching states to Play Mode.", nameof(ScenarioManager));

            IsSwitchingStatus = true;

            currentScenarioPlayCancellationSource = new CancellationTokenSource();

            ScenarioStatus preceedingStatus = ScenarioStatus;

            EarlyScenarioPlaying?.InvokeSafely(this, new ScenarioPlayingEventArgs(preceedingStatus));

            await AssetManager.EnableOrDisableInteractivityForPlayScenarioAsync();
            
            ScenarioStatus = ScenarioStatus.Playing;
            ScenarioPlaying?.InvokeSafely(this, new ScenarioPlayingEventArgs(preceedingStatus));

            // TODO Not ideal, but StageManager doesn't know about ScenarioManager so we can't sub to the event
            StageManager.StartTimer();

            IsSwitchingStatus = false;

            // invoked after IsSwitchingStatus is false, so property updates downstream of this event can be networked correctly
            LateScenarioPlaying?.InvokeSafely(this, new ScenarioPlayingEventArgs(preceedingStatus));

            GIGXR.Platform.Utilities.Logger.Info("Scenario is now in Play Mode.", nameof(ScenarioManager));

            return true;
        }

        public async UniTask<bool> PauseScenarioAsync()
        {
            // Do not go into a new status changing event if there is already another event going on
            if (IsSwitchingStatus || IsSavingScenario)
                return false;

            // Cannot pause a scenario that is already paused
            if (ScenarioStatus == ScenarioStatus.Paused)
                return false;

            GIGXR.Platform.Utilities.Logger.Info("Scenario is switching states to Pause Mode.", nameof(ScenarioManager));

            IsSwitchingStatus = true;

            await AssetManager.DisableInteractivityForAllAssetsAsync();
            
            ScenarioStatus = ScenarioStatus.Paused;
            ScenarioPaused?.InvokeSafely(this, new ScenarioPausedEventArgs());

            // TODO Not ideal, but StageManager doesn't know about ScenarioManager so we can't sub to the event
            StageManager.StopTimer();

            IsSwitchingStatus = false;

            GIGXR.Platform.Utilities.Logger.Info("Scenario is now in Pause Mode.", nameof(ScenarioManager));

            return true;
        }

        private void CancelScenarioToken()
        {
            if (currentScenarioPlayCancellationSource != null)
            {
                currentScenarioPlayCancellationSource.Cancel();
                currentScenarioPlayCancellationSource.Dispose();
                currentScenarioPlayCancellationSource = null;
            }
        }

        public async UniTask<bool> StopScenarioAsync()
        {
            // Do not go into a new status changing event if there is already another event going on
            if (IsSwitchingStatus || IsSavingScenario)
                return false;

            // Cannot stop a scenario that is already stopped
            if (ScenarioStatus == ScenarioStatus.Stopped)
                return false;

            GIGXR.Platform.Utilities.Logger.Info("Scenario is switching states to Stop Mode.", nameof(ScenarioManager));

            IsSwitchingStatus = true;

            CancelScenarioToken();

            // The only situation where this can happen is with clients exiting play mode
            // This is gross and I don't like it, but clients need to save or else the reset method below
            // will wipe the host changes made in Edit mode, if we don't reset, then we can have inaccurate
            // stage and asset data locally otherwise
            while (IsSavingScenario)
            {
                await UniTask.Yield();
            }

            await ResetScenario();
            
            ScenarioStatus = ScenarioStatus.Stopped;
            ScenarioStopped?.InvokeSafely(this, new ScenarioStoppedEventArgs());

            IsSwitchingStatus = false;

            GIGXR.Platform.Utilities.Logger.Info("Scenario is now in Stop Mode.", nameof(ScenarioManager));

            return true;
        }

        public async UniTask ResetScenario()
        {
            await AssetManager.DisableInteractivityForAllAssetsAsync();

            // Reset Scenario back to last saved state.
            _ = AssetManager.ReloadStagesAndAssetsAsync(LastSavedScenario.stages, LastSavedScenario.assets, Guid.Empty);

            // TODO Do not need to reload Rules because they are immutable at this time.

            ScenarioReset?.InvokeSafely(this, new ScenarioResetEventArgs());
        }

        public async UniTask<Scenario> SaveScenarioAsync(bool saveAssetData = true)
        {
            if (LastSavedScenario == null)
            {
                GIGXR.Platform.Utilities.Logger.Warning($"Cannot save scenario since {nameof(LastSavedScenario)} is null", nameof(ScenarioManager));
                return null;
            }

            IsSavingScenario = true;

            LastSavedScenario.stages = AssetManager.StageManager.Stages.ToList();

            // Only save the asset data when specified (e.g. only in Edit mode)
            if(saveAssetData)
            {
                var serializedAssetList = await AssetManager.SerializeToAssetDataAsync(false);
                LastSavedScenario.assets = serializedAssetList;
            }

            IsSavingScenario = false;

            return LastSavedScenario;
        }

        public async UniTask<Scenario> ExportScenarioAsync(bool includeRuntimeAssets)
        {
            List<Asset> serializedAssetList = await AssetManager.SerializeToAssetDataAsync(includeRuntimeAssets);

            return new Scenario
            {
                stages = AssetManager.StageManager.Stages.ToList(),
                assets = serializedAssetList,

                // These are immutable after Session creation (for now).
                presetStageMappings = LastSavedScenario.presetStageMappings,
                presetAssetMappings = LastSavedScenario.presetAssetMappings,
            };
        }

        public void SyncScenarioTimer
        (
            int totalMillisecondsInSimulation,
            int totalMillisecondsInScenario,
            int totalMillisecondsInCurrentStage,
            int syncOffsetMilliseconds
        )
        {
            scenarioTimer.SyncScenarioTimer
                (totalMillisecondsInSimulation + syncOffsetMilliseconds, totalMillisecondsInScenario + syncOffsetMilliseconds);

            StageManager.SetStageOffset(totalMillisecondsInCurrentStage + syncOffsetMilliseconds);
        }

        public void AddSimulationTime(int milliseconds)
        {
            scenarioTimer.AddSimulationTime(milliseconds);
        }

        public void SetTimeOffsets(int scenarioOffsetMilliseconds, int stageOffsetMilliseconds)
        {
            scenarioTimer.SetScenarioOffset(scenarioOffsetMilliseconds);
        }

        public UniTask TrySyncScenarioAsync(int currentServerTime, ScenarioSyncData syncData)
        {
            GIGXR.Platform.Utilities.Logger.Info($"Attempting to sync scenario lastSyncTimeStamp: {lastSyncTimeStamp} vs syncData: {syncData.TimeStamp}.", nameof(ScenarioManager));

            // Reject any sync requests that are after the last set sync. This can occur if a user joins while the session is in one state and changes
            // before the sync is applied during the loading process
            if (lastSyncTimeStamp == null ||
                lastSyncTimeStamp < syncData.TimeStamp)
            {
                lastSyncTimeStamp = syncData.TimeStamp;

                // Check to see when the message was sent vs when it actually is so the clock is actually in sync with
                // the delay over the network accounted for
                var offsetMilliseconds = Math.Abs(currentServerTime - syncData.TimeStamp);

                // If the scenario is paused, stopped, or in Edit mode (i.e. not playing), then the offset does not need to be applied as the clocks
                // will not be updating in real time at during those time and do not need to compensate for delay of the message
                if (syncData.ScenarioStatus != ScenarioStatus.Playing)
                {
                    offsetMilliseconds = 0;
                }

                SyncScenarioTimer
                    (
                        syncData.TotalMillisecondsInSimulation,
                        syncData.TotalMillisecondsInScenario,
                        syncData.TotalMillisecondsInCurrentStage,
                        offsetMilliseconds
                    );

                switch (syncData.ScenarioStatus)
                {
                    case ScenarioStatus.Playing:
                        _ = PlayScenarioAsync();
                        break;
                    case ScenarioStatus.Paused:
                        _ = PauseScenarioAsync();
                        break;
                    case ScenarioStatus.Stopped:
                        _ = StopScenarioAsync();
                        break;
                    case ScenarioStatus.Unloading:
                    case ScenarioStatus.Unloaded:
                    case ScenarioStatus.Loading:
                    case ScenarioStatus.Loaded:
                    default:
                        break;
                }
            }

            return UniTask.CompletedTask;
        }

        public void SetPathway(PathwayData givenPathway, bool newValue)
        {
            if(newValue)
                GIGXR.Platform.Utilities.Logger.Info($"Scenario pathway has been updated to {givenPathway.pathwayDisplayName}.", nameof(ScenarioManager));

            _selectedPathway = givenPathway;
            
            NewScenarioPathway?.InvokeSafely(this, new ScenarioPathwaySetEventArgs(_selectedPathway, newValue));
        }

        public void SetPlayMode(ScenarioControlTypes type, bool saveValue)
        {
            if(saveValue)
                GIGXR.Platform.Utilities.Logger.Info($"Scenario play mode has been updated to {_selectedPathway.pathwayDisplayName}.", nameof(ScenarioManager));

            _selectedPlayMode = type;

            ScenarioPlayModeSet?.InvokeSafely(this, new ScenarioPlayModeSetEventArgs(type, saveValue));
        }
    }
}