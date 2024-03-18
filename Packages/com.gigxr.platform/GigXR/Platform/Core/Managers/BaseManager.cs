using UnityEngine;

namespace GIGXR.Platform.Managers
{
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.Core.DependencyInjection;

    public abstract class BaseManager : MonoBehaviour
    {
        protected AppEventBus appEventBus;

        [InjectDependencies]
        public void Construct(AppEventBus appEventBusInstance)
        {
            appEventBus = appEventBusInstance;

            SubscribeToEventAppEventBus();
        }

        protected abstract void SubscribeToEventAppEventBus();
    }
}
