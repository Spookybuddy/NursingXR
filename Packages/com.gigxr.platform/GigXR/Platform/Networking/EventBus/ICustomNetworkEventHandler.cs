using System;

namespace GIGXR.Platform.Networking.EventBus
{
    /// <summary>
    /// An interface for providing Photon networked event support to be used with a GIG Event Bus.
    /// </summary>
    public interface ICustomNetworkEventHandler
    {
        /// <summary>
        /// Enable custom network events.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable custom network events.
        /// </summary>
        void Disable();

        /// <summary>
        /// Register a custom network event.
        /// </summary>
        /// <param name="eventCode">The unique event code to be used internally.</param>
        /// <typeparam name="TNetworkEvent">The event type to register.</typeparam>
        /// <typeparam name="TNetworkEventSerializer">A serializer for the event to register.</typeparam>
        /// <returns>Whether the registration was successful.</returns>
        bool RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(byte eventCode)
            where TNetworkEvent : ICustomNetworkEvent
            where TNetworkEventSerializer : ICustomNetworkEventSerializer<TNetworkEvent>;

        /// <summary>
        /// Raise a custom network event to other clients.
        /// </summary>
        /// <param name="event">The event to send.</param>
        /// <typeparam name="TNetworkEvent">The event type to be sent.</typeparam>
        /// <returns>Whether the event was able to be sent successfully.</returns>
        bool RaiseNetworkEvent<TNetworkEvent>(TNetworkEvent @event) where TNetworkEvent : ICustomNetworkEvent;

        /// <summary>
        /// Raise a custom network event to other clients with a Typed variable.
        /// </summary>
        /// <typeparam name="TNetworkEvent"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        bool RaiseNetworkEvent(ICustomNetworkEvent @event, Type type);
    }
}