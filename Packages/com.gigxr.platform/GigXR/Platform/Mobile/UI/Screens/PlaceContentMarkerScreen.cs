using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using GIGXR.Platform.Mobile.AppEvents.Events.UI;
using GIGXR.Platform.Mobile.AR;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using System.Threading;

namespace GIGXR.Platform.Mobile.UI
{
    public class PlaceContentMarkerScreen : BaseScreenObjectMobile
    {
        public override ScreenTypeMobile ScreenType => ScreenTypeMobile.ContentMarker;

        private CancellationTokenSource cancellationTokenSource;

        private IScenarioManager ScenarioManager { get; set; }

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManager)
        {
            ScenarioManager = scenarioManager;
        }

        protected override void SubscribeToEventBuses()
        {
            base.SubscribeToEventBuses();

            EventBus.Subscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
            EventBus.Subscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }

        /// <summary>
        /// Bring up this screen when placing the content marker
        /// </summary>
        /// <param name="event"></param>
        private void OnStartContentMarkerEvent(StartContentMarkerEvent @event)
        {
            uiEventBus.Publish(new SwitchingActiveScreenEventMobile(ScreenTypeMobile.ContentMarker, ScreenType));

            StartContentMarker(@event.WithAssetsHidden);
        }

        private void OnCancelContentMarkerEvent(CancelContentMarkerEvent @event)
        {
            ScenarioManager.AssetManager.RemoveContentMarkerHandle(true);

            RootScreenObject.SetActive(false);
        }

        private void StartContentMarker(bool assetsHidden)
        {
            // When in Edit mode, as a mobile user, you are a client and cannot see any assets, so make sure that the
            // content marker model is visible in this situation, otherwise the assets will be seen while playing
            ScenarioManager.AssetManager.SpawnContentMarkerHandle(assetsHidden, true);

            var arContentMarker = ScenarioManager.AssetManager.ContentMarkerInstance.gameObject.AddComponent<ArObject>();

            ScenarioManager.AssetManager.ContentMarkerInstance.gameObject.AddComponent<TouchToRotationComponent>();

            if(cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();

                _ = arContentMarker.KeepTargetAtLastRaycastHitRoutine(cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Called via Unity Editor
        /// </summary>
        public void SetContentMarker()
        {
            if(cancellationTokenSource != null)
            {
                cancellationTokenSource.Dispose();

                cancellationTokenSource = null;
            }
            
            ScenarioManager.AssetManager.SetContentMarker();

            RootScreenObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventBus.Unsubscribe<StartContentMarkerEvent>(OnStartContentMarkerEvent);
            EventBus.Unsubscribe<CancelContentMarkerEvent>(OnCancelContentMarkerEvent);
        }
    }
}