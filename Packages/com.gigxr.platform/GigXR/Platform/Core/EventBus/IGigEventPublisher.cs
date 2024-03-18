namespace GIGXR.Platform.Core.EventBus
{
    /// <summary>
    /// An interface for a publisher of a GIG Event Bus.
    /// </summary>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public interface IGigEventPublisher<TCategory>
    {
        /// <summary>
        /// Publish an event to subscribers.
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>Whether there were any subscribers notified.</returns>
        bool Publish<TEvent>(TEvent @event) where TEvent : IGigEvent<TCategory>;
    }
}