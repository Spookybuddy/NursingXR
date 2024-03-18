using System.Collections.Generic;

namespace GIGXR.Platform.Core.EventBus.Log
{
    /// <summary>
    /// An interface for storing and retrieving records of <c>IGigEvent</c>s.
    /// </summary>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public interface IGigEventBusLog<TCategory>
    {
        /// <summary>
        /// Append a <c>IGigEvent</c> to the log.
        /// </summary>
        /// <param name="event">The event to append.</param>
        void Append(IGigEvent<TCategory> @event);

        /// <summary>
        /// Retrieve a list of all events.
        /// </summary>
        /// <returns>A list of events.</returns>
        List<GigEventBusLogEntry<TCategory>> ToList();

        /// <summary>
        /// Retrieve a list of events filtered by event category.
        /// </summary>
        /// <param name="category">The event category to filter by.</param>
        /// <returns>A list of filtered events.</returns>
        List<GigEventBusLogEntry<TCategory>> ToListByCategory(string category);

        /// <summary>
        /// Clears the log.
        /// </summary>
        void Clear();
    }
}