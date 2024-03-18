using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.AppEvents;

namespace GIGXR.Platform.HMD.AppEvents.Events.Authentication
{
    /// <summary>
    /// Base event for any authentication issues for the HMD screens
    /// </summary>

    public abstract class BaseAuthenticationScreenEvent : IGigEvent<AppEventBus>
    {
    }
}