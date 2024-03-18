namespace GIGXR.Platform.Networking.EventBus
{
    /// <summary>
    /// An interface for defining how an event can be serialized to be sent across the internet.
    /// </summary>
    /// <typeparam name="TNetworkEvent">The event type to serialize.</typeparam>
    public interface ICustomNetworkEventSerializer<TNetworkEvent> where TNetworkEvent : ICustomNetworkEvent
    {
        /// <summary>
        /// Serialize a typed event to an array of objects.
        /// </summary>
        /// <param name="event">The event to serialize.</param>
        /// <returns>An array of objects representing this event.</returns>
        object[] Serialize(TNetworkEvent @event);

        /// <summary>
        /// Deserialize a typed event from an array of objects.
        /// </summary>
        /// <param name="data">The array of objects to deserialize.</param>
        /// <returns>An event deserialized from the raw data.</returns>
        TNetworkEvent Deserialize(object[] data);
    }
}