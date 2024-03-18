using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Scenarios;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GIGXR.Platform.Networking.EventBus
{
    /// <summary>
    /// Allows for strongly typed events to be used across the Photon Network. Uses a GIG Event Bus to forward events
    /// locally.
    /// </summary>
    public class CustomNetworkEventHandler : ICustomNetworkEventHandler, IDisposable, IOnEventCallback
    {
        /// <summary>
        /// Stores event serializers indexed by event code.
        /// </summary>
        private readonly Dictionary<byte, Type> eventCodeToEventSerializerType = new Dictionary<byte, Type>();

        /// <summary>
        /// Stores event codes indexed by event type.
        /// </summary>
        private readonly Dictionary<Type, byte> eventTypeToEventCode = new Dictionary<Type, byte>();

        /// <summary>
        /// The GIG Event Bus to forward events to.
        /// </summary>
        private readonly IGigEventBus<NetworkManager> eventBus;

        /// <summary>
        /// Initialize a <c>CustomNetworkEventHandler</c>.
        /// </summary>
        /// <param name="eventBus">The event bus to forward events.</param>
        public CustomNetworkEventHandler(IGigEventBus<NetworkManager> eventBus)
        {
            this.eventBus = eventBus;

            // Register classes that Photon can't natively serialize
            PhotonPeer.RegisterType(typeof(Guid), NetworkCodes.GuidSerialization, GuidSerializer.Serialize, GuidSerializer.Deserialize);
            PhotonPeer.RegisterType(typeof(Color), NetworkCodes.ColorSerialization, ColorSerializer.Serialize, ColorSerializer.Deserialize);
            PhotonPeer.RegisterType(typeof(ScenarioSyncData), NetworkCodes.ScenarioSyncDataSerializer, ScenarioSyncDataSerializer.Serialize, ScenarioSyncDataSerializer.Deserialize);
        }

        public void Dispose()
        {
            Disable();
        }

        public void Enable()
        {
            AddCallbackTarget();
        }

        public void Disable()
        {
            RemoveCallbackTarget();
        }

        /// <inheritdoc cref="ICustomNetworkEventHandler.RegisterNetworkEvent{TNetworkEvent,TNetworkEventSerializer}"/>
        public bool RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(byte eventCode)
            where TNetworkEvent : ICustomNetworkEvent
            where TNetworkEventSerializer : ICustomNetworkEventSerializer<TNetworkEvent>
        {
            if (eventCodeToEventSerializerType.ContainsKey(eventCode))
            {
                Debug.LogWarning($"The eventCode {eventCode} is already registered! Ignoring.");
                return false;
            }

            eventCodeToEventSerializerType.Add(eventCode, typeof(TNetworkEventSerializer));
            eventTypeToEventCode.Add(typeof(TNetworkEvent), eventCode);

            return true;
        }

        /// <inheritdoc cref="ICustomNetworkEventHandler.RaiseNetworkEvent{TNetworkEvent}"/>
        public bool RaiseNetworkEvent<TNetworkEvent>(TNetworkEvent @event) where TNetworkEvent : ICustomNetworkEvent
        {
            var type = typeof(TNetworkEvent);

            if (!eventTypeToEventCode.ContainsKey(type))
            {
                Debug.LogWarning($"The eventCode {type} is not registered! You must register it first!");
                return false;
            }

            GIGXR.Platform.Utilities.Logger.Info($"Publishing network event type {type} {@event}", "NetworkManager");

            var eventCode = eventTypeToEventCode[type];
            var serializerType = eventCodeToEventSerializerType[eventCode];

            if (!TryRuntimeSerialize(serializerType, @event, out var rawData))
                return false;

            return RaiseEvent(eventCode, rawData, @event.RaiseEventOptions, @event.SendOptions);
        }

        public bool RaiseNetworkEvent(ICustomNetworkEvent @event, Type type)
        {
            if (!eventTypeToEventCode.ContainsKey(type))
            {
                Debug.LogWarning($"The eventCode {type} is not registered! You must register it first!");
                return false;
            }

            var eventCode = eventTypeToEventCode[type];
            var serializerType = eventCodeToEventSerializerType[eventCode];

            if (!TryRuntimeSerialize(serializerType, @event, out var rawData))
                return false;

            return RaiseEvent(eventCode, rawData, @event.RaiseEventOptions, @event.SendOptions);
        }

        /// <inheritdoc cref="IOnEventCallback.OnEvent"/>
        public void OnEvent(EventData photonEvent) => OnEventInternal(photonEvent.Code, photonEvent.CustomData);

        /// <summary>
        /// Internal version of the OnEvent() callback so it can be tested. `EventData` from Photon is not testable
        /// because it uses an internal set method for CustomData.
        /// </summary>
        /// <param name="eventCode">The event code to identify the type of event.</param>
        /// <param name="customData">The custom data of the event.</param>
        public void OnEventInternal(byte eventCode, object customData)
        {
            if (!eventCodeToEventSerializerType.ContainsKey(eventCode))
            {
                // Nothing registered for this event code.
                return;
            }

            // object[] <= object
            var rawData = (object[])customData;

            var serializerType = eventCodeToEventSerializerType[eventCode];

            if (!TryRuntimeDeserialize(serializerType, rawData, out var customEvent))
                return;

            if (!TryRuntimeGetEventTypeFromSerializerType(serializerType, out var eventType))
                return;

            GIGXR.Platform.Utilities.Logger.Info($"On Network event {eventCode} {customData}", "NetworkManager");

            eventBus.RuntimeIl2CppSafePublish(eventType, customEvent);
        }

        /// <summary>
        /// IL2CPP/AOT compatible version of runtime serialization.
        /// </summary>
        protected bool TryRuntimeSerialize<TNetworkEvent>(Type serializerType, TNetworkEvent @event, out object[] data)
            where TNetworkEvent : ICustomNetworkEvent
        {
            // ICustomNetworkEventSerializer<ICustomNetworkEvent>
            var serializer = Activator.CreateInstance(serializerType);
            var serializeMethod = serializer.GetType()
                .GetMethod(nameof(ICustomNetworkEventSerializer<ICustomNetworkEvent>.Serialize));
            if (serializeMethod == null)
            {
                data = null;
                return false;
            }

            // object[] <= ICustomNetworkEvent
            try
            {
                data = (object[])serializeMethod.Invoke(serializer, new object[] { @event });
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error serializing Network event! {serializerType}");
                Debug.LogException(exception);
                data = null;
                return false;
            }
        }

        /// <summary>
        /// IL2CPP/AOT compatible version of runtime deserialization.
        /// </summary>
        protected bool TryRuntimeDeserialize(Type serializerType, object[] rawData, out object customEvent)
        {
            // ICustomNetworkEventSerializer<ICustomNetworkEvent>
            var serializer = Activator.CreateInstance(serializerType);
            var deserializeMethod = serializer.GetType()
                .GetMethod(nameof(ICustomNetworkEventSerializer<ICustomNetworkEvent>.Deserialize));
            if (deserializeMethod == null)
            {
                customEvent = null;
                return false;
            }

            try
            {
                customEvent = deserializeMethod.Invoke(serializer, new object[] { rawData });
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error deserializing Network event! {serializerType}");
                Debug.LogException(exception);
                customEvent = null;
                return false;
            }
        }

        /// <summary>
        /// IL2CPP/AOT compatible version of getting an event type from a serializer type.
        /// </summary>
        protected bool TryRuntimeGetEventTypeFromSerializerType(Type serializerType, out Type eventType)
        {
            try
            {
                eventType = serializerType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Where(i => i.GetGenericTypeDefinition() == typeof(ICustomNetworkEventSerializer<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .FirstOrDefault();

                return eventType != null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error getting event type from serializer! {serializerType}");
                Debug.LogException(exception);
                eventType = null;
                return false;
            }
        }

        /// <inheritdoc cref="PhotonNetwork.AddCallbackTarget"/>
        protected virtual void AddCallbackTarget()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        /// <inheritdoc cref="PhotonNetwork.RemoveCallbackTarget"/>
        protected virtual void RemoveCallbackTarget()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        /// <inheritdoc cref="PhotonNetwork.RaiseEvent"/>
        protected virtual bool RaiseEvent(
            byte eventCode,
            object eventContent,
            RaiseEventOptions raiseEventOptions,
            SendOptions sendOptions)
        {
            if (PhotonNetwork.InRoom)
                return PhotonNetwork.RaiseEvent(eventCode, eventContent, raiseEventOptions, sendOptions);
            else
                return false;
        }
    }
}