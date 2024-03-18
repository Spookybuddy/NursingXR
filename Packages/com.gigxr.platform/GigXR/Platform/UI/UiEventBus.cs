namespace GIGXR.Platform.UI
{
    using AppEvents;
    using Core.EventBus;

    /// <summary>
    /// An class that allows both publishing and subscribing related to UI-specific. events.
    /// For general app events, please use <see cref="AppEventBus"/>
    /// </summary>
    public class UiEventBus : GigEventBus<UiEventBus>
    {

    }
}