using System;

namespace GIGXR.Platform.Core.EventBus
{
    public static class GigEventBusExtensions
    {
        /// <summary>
        /// A non-type safe version of Publish(). This should only be used when the type cannot be known until runtime.
        ///
        /// The implementation should be IL2CPP/AOT compatible. E.g., does not use <c>dynamic</c>.
        /// </summary>
        /// <param name="eventBus">The event bus object to operate on.</param>
        /// <param name="eventType">The type of event to publish matching customEvent.</param>
        /// <param name="customEvent">The custom event to publish matching eventType.</param>
        /// <returns>Whether there were any subscribers notified.</returns>
        public static bool RuntimeIl2CppSafePublish<TCategory>(
            this IGigEventBus<TCategory> eventBus,
            Type eventType,
            object customEvent)
        {
            var publishMethod = eventBus.GetType().GetMethod(nameof(IGigEventBus<object>.Publish));
            if (publishMethod == null)
                return false;

            try
            {
                var genericMethod = publishMethod.MakeGenericMethod(eventType);
                return (bool)genericMethod.Invoke(eventBus, new[] {customEvent});
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}