using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace GIGXR.Platform.CommonAssetTypes.CommonComponents
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine.PlayerLoop;

    /// <summary>
    /// Simple component used to make the host transform follow the
    /// position and rotation of a target transform without parenting.
    /// </summary>
    public class TransformFollow : LocalAssetTypeComponent
    {
        public Vector3 positionOffset;

        /// <summary>
        /// CancellationTokenSource, created by this Monobehaviour,
        /// to allow FixedUpdate in a Task to be enabled / disabled without
        /// disabling this component.
        /// </summary>
        private CancellationTokenSource transformUpdateTokenSource;

        private bool hasPositionAssetTypeComponent;
   
        /// <summary>
        /// Start following the specified transform.
        /// </summary>
        /// <param name="target"></param>
        public void Follow(Transform target)
        {
            if (transformUpdateTokenSource == null)
            {
                transformUpdateTokenSource = new CancellationTokenSource();
                
                LateTransformUpdate(target, transformUpdateTokenSource.Token).Forget();
            }

            if (attachedAssetMediator != null && hasPositionAssetTypeComponent)
            {
                attachedAssetMediator.CallAssetMethod(nameof(PositionAssetTypeComponent.SetTransformUpdates), new object[] { false });
            }

            transform.position = target.position + positionOffset;
            transform.rotation = target.rotation;
        }

        /// <summary>
        /// Stop following.
        /// </summary>
        public void Stop()
        {
            if (attachedAssetMediator != null && hasPositionAssetTypeComponent)
            {
                attachedAssetMediator.CallAssetMethod(nameof(PositionAssetTypeComponent.SetTransformUpdates), new object[] { true });
            }

            if (transformUpdateTokenSource != null)
            {
                transformUpdateTokenSource.Cancel();
                transformUpdateTokenSource.Dispose();
                transformUpdateTokenSource = null;
            }
        }

        private void OnDestroy()
        {
            if (transformUpdateTokenSource != null)
            {
                transformUpdateTokenSource.Cancel();
                transformUpdateTokenSource.Dispose();
                transformUpdateTokenSource = null;
            }
        }

        private async UniTask LateTransformUpdate(Transform target, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);

                // Target can become null when going straight out of Edit mode
                if (target != null)
                {
                    transform.position = target.position + positionOffset;
                    transform.rotation = target.rotation;
                }
            }
        }

        protected override IAssetMediator InitializeMediatorBasedReferences()
        {
            IAssetMediator mediator = base.InitializeMediatorBasedReferences();
            hasPositionAssetTypeComponent = (mediator?.GetAssetTypeComponent(nameof(PositionAssetData.position)) ?? null) != null;

            return mediator;
        }
    }
}
