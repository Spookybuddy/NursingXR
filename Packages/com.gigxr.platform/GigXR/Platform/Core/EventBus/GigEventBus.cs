using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Core.EventBus
{
    /// <summary>
    /// The default implementation of a GIG Event Bus which allows for pub/sub of strongly-typed custom events.
    /// </summary>
    /// <remarks>
    /// This class can be decorated to extend its functionality. See <c>GigEventBusDebugLogDecorator</c> for an example
    /// where all publish, subscribe, and unsubscribe events are logged to the console.
    /// </remarks>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public class GigEventBus<TCategory> : IGigEventBus<TCategory>
    {
        /// <summary>
        /// Event handlers that are subscribed to a specific type of event.
        /// </summary>
        private readonly Dictionary<Type, List<object>> eventHandlers = new Dictionary<Type, List<object>>();

        private bool appIsQuitting;

        public GigEventBus()
        {
            Application.quitting += Application_quitting;
        }

        private void Application_quitting()
        {
            Application.quitting -= Application_quitting;

            appIsQuitting = true;
        }

        /// <inheritdoc cref="IGigEventPublisher{TCategory}.Publish{TEvent}"/>
        public bool Publish<TEvent>(TEvent @event) where TEvent : IGigEvent<TCategory>
        {
            // Do not send out events while the application is quitting as it will
            // just generate NRE as objects are destroyed
            if (appIsQuitting)
                return false;

            var type = typeof(TEvent);

            lock (eventHandlers)
            {
                if (!eventHandlers.ContainsKey(type) || eventHandlers[type].Count == 0)
                {
                    // No registered handlers.
                    return false;
                }
            }

            GIGXR.Platform.Utilities.Logger.Info($"Publishing event type {type} to {eventHandlers[type].Count} subscribers.", "EventBus");

            List<object> handlersWhenPublished;
            lock (eventHandlers[type])
            {
                handlersWhenPublished = new List<object>(eventHandlers[type]);
            }

            foreach (var handlerReference in handlersWhenPublished)
            {
                var handler = (Action<TEvent>)handlerReference;
                try
                {
                    GIGXR.Platform.Utilities.Logger.Info($"Handler {handler.Target} working on {type}", "EventBus", context: handler.Target as UnityEngine.Object);

                    handler?.Invoke(@event);
                }
                catch (Exception exception)
                {
                    // Do not fall over if an invoked subscription throws an Exception.
                    Debug.LogWarning($"Error notifying event subscriber! {@event}");
                    Debug.LogException(exception);
                }
            }

            return true;
        }

        /// <inheritdoc cref="IGigEventSubscriber{TCategory}.Subscribe{TEvent}"/>
        public bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>
        {
            var type = typeof(TEvent);

            lock (eventHandlers)
            {
                if (!eventHandlers.ContainsKey(type))
                {
                    eventHandlers.Add(type, new List<object>());
                }
            }

            lock(eventHandlers[type])
            {
                foreach (var handlerReference in eventHandlers[type])
                {
                    // Check if this handler is already registered.
                    if (handlerReference.Equals(eventHandler))
                        return false;
                }

                eventHandlers[type].Add(eventHandler);
            }

            return true;
        }

        /// <inheritdoc cref="IGigEventSubscriber{TCategory}.Unsubscribe{TEvent}"/>
        public bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<TCategory>
        {
            var type = typeof(TEvent);

            lock (eventHandlers)
            {
                if (!eventHandlers.ContainsKey(type))
                {
                    // No handlers are registered for this type.
                    return false;
                }
            }

            lock(eventHandlers[type])
            { 
                return eventHandlers[type].Remove(eventHandler);
            }
        }
    }
}