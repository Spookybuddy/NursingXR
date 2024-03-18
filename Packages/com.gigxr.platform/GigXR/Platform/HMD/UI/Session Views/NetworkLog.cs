using GIGXR.Platform.HMD.AppEvents.Events;
using GIGXR.Platform.UI;
using GIGXR.Platform.Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GIGXR.Platform.AppEvents.Events.Session;

namespace GIGXR.Platform.HMD
{
    public enum NetworkEventType
    {
        SessionStarted,
        UserJoined,
        UserLeft,
        SessionTerminated,
        ContentLoaded,
        ContentRemoved,
        ContentRenamed,
        SessionRenamed,
        StageRenamed
    }

    [RequireComponent(typeof(TextMeshProUGUI))]
    public class NetworkLog : BaseUiObject
    {
        // --- Constants:

        private const string Format = "{2}: {0} {1}. \n";

        // --- Private Variables:

        private TextMeshProUGUI    networkLogTextBox;
        private List<NetworkEvent> networkEvents;
        private bool               logReadOnly;
        private string             logText = "";

        private TextMeshProUGUI NetworkLogTextBox
        {
            get
            {
                if (networkLogTextBox == null)
                {
                    networkLogTextBox = GetComponent<TextMeshProUGUI>();
                }

                return networkLogTextBox;
            }
        }

        // --- Unity Methods

        private void Awake()
        {
            networkLogTextBox = GetComponent<TextMeshProUGUI>();
        }

        // --- Public Methods:

        protected override void SubscribeToEventBuses()
        {
            EventBus.Subscribe<WriteToNetworkLogEvent>(OnWriteToNetworkLogEvent);
            EventBus.Subscribe<LockNetworkLogEvent>(OnLockNetworkLogEvent);
            EventBus.Subscribe<ClearNetworkLogEvent>(OnClearNetworkLogEvent);
            EventBus.Subscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<WriteToNetworkLogEvent>(OnWriteToNetworkLogEvent);
            EventBus.Unsubscribe<LockNetworkLogEvent>(OnLockNetworkLogEvent);
            EventBus.Unsubscribe<ClearNetworkLogEvent>(OnClearNetworkLogEvent);
            EventBus.Unsubscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);
        }

        #region EventHandlers

        private void OnWriteToNetworkLogEvent(WriteToNetworkLogEvent @event)
        {
            if (!logReadOnly)
            {
                string time = DateTime.Now.ToLongTimeString();

                NetworkEvent networkEvent = new NetworkEvent
                    (
                        @event.EventType,
                        @event.Message,
                        time
                    );

                // if we have no event log, or we are starting a new session, reinitialize the log.
                if (networkEvents == null ||
                    networkEvent.EventType == NetworkEventType.SessionStarted)
                    networkEvents = new List<NetworkEvent>();

                networkEvents.Add(networkEvent);
                logText                = UpdateLogText(networkEvent);
                NetworkLogTextBox.text = logText;
            }
        }

        private void OnLockNetworkLogEvent(LockNetworkLogEvent @event)
        {
            logReadOnly = @event.ReadOnly;
        }

        private void OnClearNetworkLogEvent(ClearNetworkLogEvent @event)
        {
            Clear();
        }

        private void OnReturnToSessionListEvent(ReturnToSessionListEvent @event)
        {
            Clear();
        }

        private void Clear()
        {
            networkEvents?.Clear();
            logText = "";
            NetworkLogTextBox.text = "";
        }

        #endregion

        // --- Private Methods:

        /// <summary>
        /// Updates the log text to display in app.
        /// </summary>
        /// <param name="networkEvent"></param>
        /// <returns></returns>
        private string UpdateLogText(NetworkEvent networkEvent)
        {
            string entry = "";

            switch (networkEvent.EventType)
            {
                case NetworkEventType.SessionStarted:
                    entry = string.Format
                        (
                            Format,
                            "Session started: ",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.UserJoined:
                    entry = string.Format
                        (
                            Format,
                            "Session joined by",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.UserLeft:
                    entry = string.Format
                        (
                            Format,
                            "Session left by",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.SessionTerminated:
                    entry = string.Format
                        (
                            Format,
                            "Session ended by",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.ContentLoaded:
                    entry = string.Format
                        (
                            Format,
                            "Loaded ",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.ContentRemoved:
                    entry = string.Format
                        (
                            Format,
                            "Removed ",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.ContentRenamed:
                    entry = string.Format
                        (
                            Format,
                            "Content Altered: ",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.SessionRenamed:
                    entry = string.Format
                        (
                            Format,
                            "Session Renamed as: ",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
                case NetworkEventType.StageRenamed:
                    entry = string.Format
                        (
                            Format,
                            "Stage Renamed as: ",
                            networkEvent.Data,
                            networkEvent.EventTime
                        );

                    break;
            }

            return string.Concat(entry, logText);
        }

        // --- Serialized Classes:
        [Serializable]
        private class NetworkEvent
        {
            public NetworkEvent
            (
                NetworkEventType e,
                string data,
                string dt
            )
            {
                EventType = e;
                Data      = data;
                EventTime = dt;
            }

            public NetworkEventType EventType;
            public string           Data;
            public string           EventTime;
        }
    }
}