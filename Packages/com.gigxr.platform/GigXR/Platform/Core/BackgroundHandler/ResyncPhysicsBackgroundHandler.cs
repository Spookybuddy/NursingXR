using System.Threading;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Core;
using UnityEngine;

namespace GIGXR.Platform
{
    /// <summary>
    /// Responsible for resyncing the physics colliders if physics has been disabled
    /// to automatically update.
    /// </summary>
    public class ResyncPhysicsBackgroundHandler : BaseBackgroundHandler
    {
        public ResyncPhysicsBackgroundHandler()
        {
        }

        protected override UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken)
        {
            if (!Physics.autoSimulation)
            {
                Physics.SyncTransforms();
            }

            return UniTask.CompletedTask;
        }
    }
}