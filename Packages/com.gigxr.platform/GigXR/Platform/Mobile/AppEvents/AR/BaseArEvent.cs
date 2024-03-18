using GIGXR.Platform.AppEvents;
using GIGXR.Platform.Core.EventBus;

namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    /// <summary>
    ///     The base event for events used to control AR utilities.
    ///     AR events should inherit from this class.
    /// </summary>
    public abstract class BaseArEvent : IGigEvent<AppEventBus>
    {
    }
}
