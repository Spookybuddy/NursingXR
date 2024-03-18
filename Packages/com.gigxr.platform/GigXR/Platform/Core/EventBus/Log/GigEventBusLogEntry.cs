using System;

namespace GIGXR.Platform.Core.EventBus.Log
{
    /// <summary>
    /// An entry in a GIG Event Bus Log.
    /// </summary>
    public class GigEventBusLogEntry<TCategory>
    {
        /// <summary>
        /// The date and time an event was recorded in UTC.
        /// </summary>
        public DateTime DateTimeUtc { get; }

        // The recorded event.
        public IGigEvent<TCategory> Event { get; }

        /// <summary>
        /// Creates a new GIG Event Bus Log Entry.
        /// </summary>
        /// <param name="event">The event to log.</param>
        public GigEventBusLogEntry(IGigEvent<TCategory> @event)
        {
            DateTimeUtc = DateTime.UtcNow;
            Event = @event;
        }
    }
}