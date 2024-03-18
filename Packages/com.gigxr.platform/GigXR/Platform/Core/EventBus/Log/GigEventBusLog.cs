using System.Collections.Generic;
using System.Linq;

namespace GIGXR.Platform.Core.EventBus.Log
{
    /// <summary>
    /// The default implementation of a GIG Event Bus Log which stores a record of events.
    /// </summary>
    /// <typeparam name="TCategory">The category of the GIG Event Bus.</typeparam>
    public class GigEventBusLog<TCategory> : IGigEventBusLog<TCategory>
    {
        /// <summary>
        /// A list of recorded events.
        /// </summary>
        private readonly List<GigEventBusLogEntry<TCategory>> logEntries = new List<GigEventBusLogEntry<TCategory>>();

        /// <inheritdoc cref="IGigEventBusLog{TCategory}.Append"/>
        public void Append(IGigEvent<TCategory> @event)
        {
            logEntries.Add(new GigEventBusLogEntry<TCategory>(@event));
        }

        /// <inheritdoc cref="IGigEventBusLog{TCategory}.ToList"/>
        public List<GigEventBusLogEntry<TCategory>> ToList()
        {
            return logEntries;
        }

        /// <inheritdoc cref="IGigEventBusLog{TCategory}.ToListByCategory"/>
        public List<GigEventBusLogEntry<TCategory>> ToListByCategory(string category)
        {
            // TODO - 
            // return logEntries.Where(entry => entry.Event.EventCategory == category).ToList();
            return new List<GigEventBusLogEntry<TCategory>>();
        }

        /// <inheritdoc cref="IGigEventBusLog{TCategory}.Clear"/>
        public void Clear()
        {
            logEntries.Clear();
        }
    }
}