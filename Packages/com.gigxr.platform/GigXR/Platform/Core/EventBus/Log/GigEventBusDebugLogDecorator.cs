using System;
using UnityEngine;

namespace GIGXR.Platform.Core.EventBus.Log
{
    /// <summary>
    /// A decorator for <c>IGigEventBus</c> that prints to the console.
    /// </summary>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public class GigEventBusDebugLogDecorator<TCategory> : IGigEventBus<TCategory>
    {
        private readonly IGigEventBus<TCategory> eventBus;

        public GigEventBusDebugLogDecorator(IGigEventBus<TCategory> eventBus)
        {
            this.eventBus = eventBus;
        }

        /// <summary>
        /// Prints to the console and delegates normal publish functionality.
        /// </summary>
        /// <inheritdoc cref="IGigEventPublisher{TCategory}.Publish{TEvent}"/>
        public bool Publish<TEvent>(TEvent @event) where TEvent : IGigEvent<TCategory>
        {
            var categoryName = typeof(TCategory).Name;
            var eventName = typeof(TEvent).Name;
            Debug.Log(
                $"EventLog<{categoryName}>: Publishing {eventName}");

            return eventBus.Publish(@event);
        }

        /// <summary>
        /// Prints to the console and delegates normal subscribe functionality.
        /// </summary>
        /// <inheritdoc cref="IGigEventSubscriber{TCategory}.Subscribe{TEvent}"/>
        public bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>
        {
            var categoryName = typeof(TCategory).Name;
            var className = eventHandler.Method.ReflectedType?.Name ?? "UnknownClass";
            var methodName = eventHandler.Method.Name;
            var eventName = typeof(TEvent).Name;
            Debug.Log($"EventLog<{categoryName}>: Subscribing to {eventName} with {className}.{methodName}");

            return eventBus.Subscribe(eventHandler);
        }

        /// <summary>
        /// Prints to the console and delegates normal unsubscribe functionality.
        /// </summary>
        /// <inheritdoc cref="IGigEventSubscriber{TCategory}.Unsubscribe{TEvent}"/>
        public bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>
        {
            var categoryName = typeof(TCategory).Name;
            var className = eventHandler.Method.ReflectedType?.Name ?? "UnknownClass";
            var methodName = eventHandler.Method.Name;
            var eventName = typeof(TEvent).Name;
            Debug.Log($"EventLog<{categoryName}>: Unsubscribing to {eventName} with {className}.{methodName}");

            return eventBus.Unsubscribe(eventHandler);
        }
    }
}