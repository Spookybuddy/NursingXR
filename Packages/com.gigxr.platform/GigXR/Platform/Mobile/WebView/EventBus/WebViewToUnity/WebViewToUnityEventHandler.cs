using System;
using System.Collections.Generic;
using System.Linq;
using GIGXR.Platform.Core.EventBus;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.EventBus.WebViewToUnity
{
    /// <summary>
    /// Handles WebView to Unity events.
    /// </summary>
    public class WebViewToUnityEventHandler : IWebViewToUnityEventHandler
    {
        private readonly Dictionary<string, Type> eventCodeToEventSerializerType = new Dictionary<string, Type>();

        private readonly IUniWebView uniWebView;
        private readonly IGigEventBus<WebViewEventBus> eventBus;

        public WebViewToUnityEventHandler(IUniWebView uniWebView, IGigEventBus<WebViewEventBus> eventBus)
        {
            this.uniWebView = uniWebView;
            this.eventBus = eventBus;
        }

        public void Enable()
        {
            uniWebView.OnMessageReceived += OnMessageReceived;
        }

        public void Disable()
        {
            uniWebView.OnMessageReceived -= OnMessageReceived;
        }

        public bool RegisterWebViewToUnityEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IWebViewToUnityEvent
            where TWebViewEventSerializer : IWebViewToUnityEventSerializer<TWebViewEvent>
        {
            if (eventCodeToEventSerializerType.ContainsKey(eventCode))
            {
                Debug.LogWarning("This eventCode is already registered! Ignoring.");
                return false;
            }

            eventCodeToEventSerializerType.Add(eventCode, typeof(TWebViewEventSerializer));

            return true;
        }

        private void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            var eventCode = message.Path;

            if (!eventCodeToEventSerializerType.ContainsKey(eventCode))
            {
                // Nothing registered for this event code.
                return;
            }

            var serializerType = eventCodeToEventSerializerType[eventCode];

            if (!message.Args.ContainsKey("payload"))
                return;

            var payload = message.Args["payload"];

            if (!TryRuntimeDeserialize(serializerType, payload, out var customEvent))
                return;

            if (!TryRuntimeGetEventTypeFromSerializerType(serializerType, out var eventType))
                return;

            eventBus.RuntimeIl2CppSafePublish(eventType, customEvent);
        }

        protected bool TryRuntimeDeserialize(Type serializerType, string payload, out object customEvent)
        {
            // IWebViewToUnityEventSerializer<IWebViewEvent>
            var serializer = Activator.CreateInstance(serializerType);
            var deserializeMethod = serializer.GetType()
                .GetMethod(nameof(IWebViewToUnityEventSerializer<IWebViewToUnityEvent>.Deserialize));
            if (deserializeMethod == null)
            {
                customEvent = null;
                return false;
            }

            try
            {
                // customEvent.IsAssignableFrom(ICustomNetworkEvent) <= string
                customEvent = deserializeMethod.Invoke(serializer, new object[] {payload});
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error deserializing WebView event! {serializerType}");
                Debug.LogException(exception);
                customEvent = null;
                return false;
            }
        }

        protected bool TryRuntimeGetEventTypeFromSerializerType(Type serializerType, out Type eventType)
        {
            try
            {
                eventType = serializerType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Where(i => i.GetGenericTypeDefinition() == typeof(IWebViewToUnityEventSerializer<>))
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
    }
}