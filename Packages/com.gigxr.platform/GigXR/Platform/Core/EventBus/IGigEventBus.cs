namespace GIGXR.Platform.Core.EventBus
{
    /// <summary>
    /// An interface that allows both publishing and subscribing to a GIG Event Bus.
    /// </summary>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public interface IGigEventBus<TCategory> : IGigEventPublisher<TCategory>, IGigEventSubscriber<TCategory>
    {
    }
}