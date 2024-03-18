using UnityEngine;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Core;

namespace GIGXR.Platform.UI
{
    /// <summary>
    /// Base UI Object used in both HMD and Mobile devices providing a UI and AppEventBus for the user.
    /// </summary>
    public abstract class BaseUiObject : MonoBehaviour
    {
        protected UiEventBus uiEventBus;
        protected AppEventBus EventBus;
        protected IBuilderManager UiBuilder;

        protected bool isConstructed = false;

        [InjectDependencies]
        public void Construct(AppEventBus appEventBusInstance, UiEventBus uiEventBusInstance, IBuilderManager uiBuilder)
        {
            if (isConstructed)
            {
                return;
            }

            EventBus   = appEventBusInstance;
            uiEventBus = uiEventBusInstance;
            UiBuilder = uiBuilder;

            isConstructed = true;

            SubscribeToEventBuses();
        }

        protected virtual void OnEnable()
        {
            TryToConstruct();
        }

        protected void TryToConstruct()
        {
            if (!isConstructed)
            {
                FindObjectOfType<GIGXRCore>()?.SetRuntimeDependencies(this);
            }
        }

        protected abstract void SubscribeToEventBuses();
    }
}