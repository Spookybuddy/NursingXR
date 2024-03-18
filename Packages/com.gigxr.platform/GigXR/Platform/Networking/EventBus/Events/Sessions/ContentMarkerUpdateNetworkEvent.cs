namespace GIGXR.Platform.Networking.EventBus.Events.Sessions
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;
    using UnityEngine;

    /// <summary>
    /// A networked event that is sent from the MasterClient to all other users
    /// when a session is renamed
    /// </summary>
    public class ContentMarkerUpdateNetworkEvent : ICustomNetworkEvent
    {
        public Vector3 NewPosition { get; private set; }

        public Quaternion NewRotation { get; private set; }

        public int TargetActor { get; private set; }

        RaiseEventOptions _raiseEventOptions;

        public RaiseEventOptions RaiseEventOptions => _raiseEventOptions;

        public SendOptions SendOptions => SendOptions.SendReliable;

        public ContentMarkerUpdateNetworkEvent(Vector3 newPosition, Quaternion newRotation, int actorReference = -1)
        {
            NewPosition = newPosition;
            NewRotation = newRotation;
            TargetActor = actorReference;

            if (actorReference >= 0)
            {
                _raiseEventOptions = new RaiseEventOptions()
                {
                    Receivers = ReceiverGroup.Others,
                    TargetActors = new int[] { actorReference }
                };
            }
            else
            {
                _raiseEventOptions = new RaiseEventOptions()
                {
                    Receivers = ReceiverGroup.Others
                };
            }
        }

        public override string ToString()
        {
            return $"Content Marker Updated: {NewPosition} - {NewRotation}";
        }
    }

    public class ContentMarkerUpdateNetworkEventSerializer : ICustomNetworkEventSerializer<ContentMarkerUpdateNetworkEvent>
    {
        public object[] Serialize(ContentMarkerUpdateNetworkEvent @event) => new object[] { @event.NewPosition, @event.NewRotation, @event.TargetActor };

        public ContentMarkerUpdateNetworkEvent Deserialize(object[] data) => new ContentMarkerUpdateNetworkEvent((Vector3)data[0], (Quaternion)data[1], (int)data[2]);
    }
    
    public class RequestContentMarkerNetworkEvent : ICustomNetworkEvent
    {
        public int RequestingActor { get; }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.MasterClient
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public RequestContentMarkerNetworkEvent(int actor)
        {
            RequestingActor = actor;
        }

        public override string ToString()
        {
            return $"Content Marker Request from: {RequestingActor}";
        }
    }

    public class RequestContentMarkerNetworkEventSerializer : ICustomNetworkEventSerializer<RequestContentMarkerNetworkEvent>
    {
        public object[] Serialize(RequestContentMarkerNetworkEvent @event) => new object[] { @event.RequestingActor };

        public RequestContentMarkerNetworkEvent Deserialize(object[] data) => new RequestContentMarkerNetworkEvent((int)data[0]);
    }
}