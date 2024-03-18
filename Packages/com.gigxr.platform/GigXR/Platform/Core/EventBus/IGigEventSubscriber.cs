using System;

namespace GIGXR.Platform.Core.EventBus
{
    /// <summary>
    /// An interface for a subscriber of a GIG Event Bus.
    /// </summary>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public interface IGigEventSubscriber<TCategory>
    {
        /// <summary>
        /// Subscribe to a specific type of event.
        /// </summary>
        /// <param name="eventHandler">The callback that will be triggered.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>Whether the subscription was successful.</returns>
        bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>;

        /// <summary>
        /// Unsubscribe to a specific type of event.
        /// </summary>
        /// <param name="eventHandler">The callback that will be removed.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>Whether the unsubscription was successful.</returns>
        bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>;
    }
}