using Cysharp.Threading.Tasks;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using GIGXR.Platform.AppEvents;
using System.Collections;
using UnityEngine;
using GIGXR.Platform.UI;
using System.Threading;

namespace GIGXR.Platform.Mobile.AR
{
    /// <summary>
    ///     The ArTarget handles the instantiation and placement
    ///     of the calibration placeholder during scanning.
    /// </summary>
    public class ArTarget : BaseUiObject
    {
        public static ArTarget Instance;

        #region Private Fields
        /// <summary>
        ///     The AR target prefab to instantiate when a sufficient plane is detected, set in the Profile Manager
        /// </summary>
        private GameObject arTargetPrefab;

        /// <summary>
        ///     The minimum area of the generated planes required to start the placement state, set in the Profile Manager 
        /// </summary>
        private float minAreaForPlane = 2f;

        /// <summary>
        ///     The cached AR target after instantiation
        /// </summary>
        private ArObject lastArObjectTarget;

        /// <summary>
        ///     The coroutine which runs to try to instantiate the placeholder until it is successfully instantiated.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource { get; set; }

        private WaitForSeconds wfs = new WaitForSeconds(0.2f);

        #endregion

        #region Initialization and Cleanup

        private void Awake()
        {
            Instance = this;

            // TODO Improve MobileCompositeRoot lookup
            arTargetPrefab = FindObjectOfType<MobileCompositionRoot>().MobileProfile.ArTargetPrefab;
            minAreaForPlane = FindObjectOfType<MobileCompositionRoot>().MobileProfile.ScanArea;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ArStartScanningEvent>(OnArStartScanningEvent);
            EventBus.Unsubscribe<ArSessionResetStartingEvent>(OnArSessionResetStartEvent);
            EventBus.Unsubscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
        }

        private void OnDisable()
        {
            StopTargetCalibrationRoutine();
        }

        #endregion

        #region Target Placement Helpers

        private void StartTargetCalibrationRoutine()
        {
            StopTargetCalibrationRoutine();

            if(cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();

                _ = TargetCalibrationRoutine(cancellationTokenSource.Token);
            }
        }

        private void StopTargetCalibrationRoutine()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();

                cancellationTokenSource = null;
            }
        }

        private async UniTask TargetCalibrationRoutine(CancellationToken token)
        {
            lastArObjectTarget = await GenerateTargetRoutine();

            await lastArObjectTarget.KeepTargetAtLastRaycastHitRoutine(token);
        }

        private async UniTask<ArObject> GenerateTargetRoutine()
        {
            while (!CanGenerateTarget())
            {
                await UniTask.Delay(200, true);
            }

            return InstantiateArObjectTarget();
        }

        private bool CanGenerateTarget()
        {
            //If no plane is detected return
            if (!PlaneDetection.Instance.PlaneDetected || PlaneDetection.Instance.PlaneArea <= minAreaForPlane)
                return false;

            return true;
        }

        /// <summary>
        /// Set the target placement for positioning the calibration root on session start
        /// </summary>
        public void SetTargetPlacement()
        {
            var worldRotation = lastArObjectTarget != null ? lastArObjectTarget.gameObject.transform.rotation : Quaternion.identity;
            var sessionPosition = Vector3.zero;

            if (PlaneDetection.Instance.LastRaycastHit.HasValue)
            {
                sessionPosition = PlaneDetection.Instance.LastRaycastHit.Value.sessionRelativePose.position;
            }

            EventBus.Publish(new ArTargetPlacedEvent(sessionPosition, worldRotation));
        }

        /// <summary>
        /// Instantiate the target AR object
        /// </summary>
        private ArObject InstantiateArObjectTarget()
        {
            //Instantiate the AR target
            var arTarget = Instantiate(arTargetPrefab);

            var arObject = arTarget.GetComponent<ArObject>();

            EventBus.Publish(new ArTargetInstantiatedEvent(arObject));

            //Access the AR object component on the instantiated target
            return arObject;
        }

        #endregion

        #region Event Handlers

        private async void OnArStartScanningEvent(ArStartScanningEvent @event)
        {
            // hack: wait 1 second, so planes which are hidden by reset events aren't seen here
            await UniTask.Delay(1000);

            // start calibrating
            StartTargetCalibrationRoutine();
        }

        private void OnArTargetPlacedEvent(ArTargetPlacedEvent @event)
        {
            StopTargetCalibrationRoutine();
        }

        private void OnArSessionResetStartEvent(ArSessionResetStartingEvent @event)
        {
            StopTargetCalibrationRoutine();

            if(lastArObjectTarget != null)
            {
                Destroy(lastArObjectTarget.gameObject);
            }
        }

        protected override void SubscribeToEventBuses()
        {
            EventBus.Subscribe<ArStartScanningEvent>(OnArStartScanningEvent);
            EventBus.Subscribe<ArSessionResetStartingEvent>(OnArSessionResetStartEvent);
            EventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
        }

        #endregion
    }
}
