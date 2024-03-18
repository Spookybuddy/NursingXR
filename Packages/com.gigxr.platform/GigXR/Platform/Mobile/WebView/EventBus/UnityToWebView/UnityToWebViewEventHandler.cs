using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView
{
    /// <summary>
    /// Handles Unity to WebView events.
    /// </summary>
    public class UnityToWebViewEventHandler : IUnityToWebViewEventHandler
    {
        private readonly Dictionary<string, Type> eventCodeToEventSerializerType = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> eventTypeToEventCode = new Dictionary<Type, string>();

        private readonly IUniWebView uniWebView;

        public UnityToWebViewEventHandler(IUniWebView uniWebView)
        {
            this.uniWebView = uniWebView;
        }

        public bool RegisterUnityToWebViewEvent<TWebViewEvent, TWebViewEventSerializer>(string eventCode)
            where TWebViewEvent : IUnityToWebViewEvent
            where TWebViewEventSerializer : IUnityToWebViewEventSerializer<TWebViewEvent>
        {
            if (eventCodeToEventSerializerType.ContainsKey(eventCode))
            {
                Debug.LogWarning("This eventCode is already registered! Ignoring.");
                return false;
            }

            eventCodeToEventSerializerType.Add(eventCode, typeof(TWebViewEventSerializer));
            eventTypeToEventCode.Add(typeof(TWebViewEvent), eventCode);

            return true;
        }

        public bool RaiseUnityToWebViewEvent<TWebViewEvent>(TWebViewEvent @event)
            where TWebViewEvent : IUnityToWebViewEvent
        {
            var type = typeof(TWebViewEvent);

            if (!eventTypeToEventCode.ContainsKey(type))
            {
                Debug.LogWarning("This eventCode is not registered! You must register it first!");
                return false;
            }

            var eventCode = eventTypeToEventCode[type];
            var serializerType = eventCodeToEventSerializerType[eventCode];

            if (!TryRuntimeSerialize(serializerType, @event, out var json))
                return false;

            EvaluateJavaScript($"receiveUnityMessage('{eventCode}', {json});");
            return true;
        }

        /// <summary>
        /// IL2CPP/AOT compatible version of runtime serialization.
        /// </summary>
        protected bool TryRuntimeSerialize<TWebViewEvent>(Type serializerType, TWebViewEvent @event, out string json)
            where TWebViewEvent : IUnityToWebViewEvent
        {
            // IUnityToWebViewEventSerializer<IUnityToWebViewEvent>
            var serializer = Activator.CreateInstance(serializerType);
            var serializeMethod = serializer.GetType()
                .GetMethod(nameof(IUnityToWebViewEventSerializer<IUnityToWebViewEvent>.Serialize));
            if (serializeMethod == null)
            {
                json = null;
                return false;
            }

            try
            {
                // string <= IWebViewEvent
                json = (string)serializeMethod.Invoke(serializer, new object[] {@event});
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error serializing WebView event! {serializerType}");
                Debug.LogException(exception);
                json = null;
                return false;
            }
        }

        protected virtual void EvaluateJavaScript(string jsString) => uniWebView.EvaluateJavaScript(jsString);
    }
}