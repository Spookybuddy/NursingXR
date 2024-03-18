using System;

namespace GIGXR.Platform.Core.EventBus.Log
{
    /// <summary>
    /// A decorator for <c>IGigEventBus</c> that saves published events to a log.
    /// </summary>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public class GigEventBusLogDecorator<TCategory> : IGigEventBus<TCategory>
    {
        private readonly IGigEventBus<TCategory> eventBus;
        private readonly IGigEventBusLog<TCategory> eventBusLog;

        public GigEventBusLogDecorator(IGigEventBus<TCategory> eventBus, IGigEventBusLog<TCategory> eventBusLog)
        {
            this.eventBus = eventBus;
            this.eventBusLog = eventBusLog;
        }

        /// <summary>
        /// Saves a published event to a log and delegates normal publish functionality.
        /// </summary>
        /// <inheritdoc cref="IGigEventPublisher{TCategory}.Publish{TEvent}"/>
        public bool Publish<TEvent>(TEvent @event) where TEvent : IGigEvent<TCategory>
        {
            eventBusLog.Append(@event);

            return eventBus.Publish(@event);
        }

        /// <inheritdoc cref="IGigEventSubscriber{TCategory}.Subscribe{TEvent}"/>
        public bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>
        {
            return eventBus.Subscribe(eventHandler);
        }

        /// <inheritdoc cref="IGigEventSubscriber{TCategory}.Unsubscribe{TEvent}"/>
        public bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>
        {
            return eventBus.Unsubscribe(eventHandler);
        }
    }
}