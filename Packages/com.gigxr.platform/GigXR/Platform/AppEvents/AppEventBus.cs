using GIGXR.Platform.Core.EventBus;

namespace GIGXR.Platform.AppEvents
{
    using UI;

    /// <summary>
    /// An class that allows both publishing and subscribing related to general app events.
    /// For UI-specific events, please use <see cref="UiEventBus"/>
    /// </summary>
    public class AppEventBus : GigEventBus<AppEventBus>
    {
    }
}
