using Cysharp.Threading.Tasks;
using GIGXR.Platform.Mobile.AppEvents.Events.AR;
using GIGXR.Platform.UI;
using System.Threading;
using UnityEngine;

// Component for AR scene objects
namespace GIGXR.Platform.Mobile.AR
{
    public class ArObject : BaseUiObject
    {
        /// <summary>
        /// Set position of the AR object
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public async UniTask KeepTargetAtLastRaycastHitRoutine(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate, token);

                //If the AR target has been instantiated and cached and planes are detected
                if (PlaneDetection.Instance.PlaneDetected)
                {
                    if (PlaneDetection.Instance.LastRaycastHit.HasValue)
                    {
                        // Position the target at the raycast
                        SetPosition(PlaneDetection.Instance.LastRaycastHit.Value.pose.position);

                        // Set the target active
                        SetActive(true);
                    }
                }
            }
        }

        private void OnArTargetPlacedEvent(ArTargetPlacedEvent @event)
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
        }

        protected override void SubscribeToEventBuses()
        {
            EventBus.Subscribe<ArTargetPlacedEvent>(OnArTargetPlacedEvent);
        }
    }
}