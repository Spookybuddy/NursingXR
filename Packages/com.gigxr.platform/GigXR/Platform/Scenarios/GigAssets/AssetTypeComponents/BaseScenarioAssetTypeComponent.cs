namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using System;
    using System.Threading;
    
    /// <summary>
    /// An extension of BaseAssetTypeComponent that comes with a ScenarioManager dependency and methods that are 
    /// linked to the Play/Paused/Stopped state.
    /// </summary>
    /// <typeparam name="TBaseAssetData"></typeparam>
    public abstract class BaseScenarioAssetTypeComponent<TBaseAssetData> : BaseAssetTypeComponent<TBaseAssetData>, IDisposable
        where TBaseAssetData : BaseAssetData
    {
        protected virtual PlayerLoopTiming PlayUpdateLoopTiming { get; } = PlayerLoopTiming.Update;
        
        /// <summary>
        /// Allows implementing classes to not run a background update loop if this is set to false.
        /// </summary>
        protected virtual bool RunUpdateLoop { get; } = true;

        private IScenarioManager ScenarioManager;

        private CancellationTokenSource playUpdateTokenSource;

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManager)
        {
            ScenarioManager = scenarioManager;

            ScenarioManager.ScenarioStatusChanged += ScenarioManager_ScenarioStatusChanged;
        }

        public void Dispose()
        {
            StopPlayUpdate();

            ScenarioManager.ScenarioStatusChanged -= ScenarioManager_ScenarioStatusChanged;
        }

        private void ScenarioManager_ScenarioStatusChanged(object sender, Scenarios.EventArgs.ScenarioStatusChangedEventArgs e)
        {
            switch (e.NewStatus)
            {
                case Scenarios.Data.ScenarioStatus.Unloaded:
                    break;
                case Scenarios.Data.ScenarioStatus.Loading:
                    break;
                case Scenarios.Data.ScenarioStatus.Loaded:
                    break;
                case Scenarios.Data.ScenarioStatus.Playing:
                    OnPlayStart();

                    if (playUpdateTokenSource == null && RunUpdateLoop)
                    {
                        playUpdateTokenSource = new CancellationTokenSource();

                        _ = OnPlayAsync(playUpdateTokenSource.Token);
                    }
                    
                    break;
                case Scenarios.Data.ScenarioStatus.Paused:
                    StopPlayUpdate();

                    OnPause();
                    break;
                case Scenarios.Data.ScenarioStatus.Stopped:
                    StopPlayUpdate();

                    OnResetStop();
                    break;
                case Scenarios.Data.ScenarioStatus.Unloading:
                    StopPlayUpdate();
                    break;
                default:
                    break;
            }
        }

        private void StopPlayUpdate()
        {
            if (playUpdateTokenSource != null)
            {
                playUpdateTokenSource.Cancel();

                playUpdateTokenSource.Dispose();
                playUpdateTokenSource = null;
            }
        }

        private async UniTask OnPlayAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                OnPlayUpdate();

                await UniTask.Yield(PlayUpdateLoopTiming);
            }
        }

        /// <summary>
        /// Single fire method call sent on the frame when the scenario starts playing.
        /// </summary>
        protected abstract void OnPlayStart();

        /// <summary>
        /// Called every frame set by the PlayerLoopTiming property.
        /// 
        /// Set RunUpdateLoop to false if this will be unused.
        /// </summary>
        protected abstract void OnPlayUpdate();

        /// <summary>
        /// Single fire method call sent on the frame the scenario goes to pause.
        /// 
        /// Data reset is not expected, just frozen.
        /// </summary>
        protected abstract void OnPause();

        /// <summary>
        /// Single fire method call sent on the frame the scenario is stopped or is reset. 
        /// 
        /// Data reset is expected.
        /// </summary>
        protected abstract void OnResetStop();
    }
}