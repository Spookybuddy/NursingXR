namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Pun;
    using Photon.Realtime;
    using System;
    using System.Collections.Generic;

    public class AssetPropertyUpdateNetworkEvent : ICustomNetworkEvent
    {
        public Guid AssetId { get; }

        public string PropertyName { get; }

        public byte[] Value { get; }

        public AssetPropertyUpdateNetworkEvent(Guid id, string property, byte[] val)
        {
            AssetId = id;
            PropertyName = property;
            Value = val;
        }

        public AssetPropertyUpdateNetworkEvent(Guid id, string property, byte[] val, Guid excludedClient)
        {
            AssetId = id;
            PropertyName = property;
            Value = val;

            // If this constructor is used, then a user sent a request to update a property and it was accepted, send this updated property to all
            // other users but the excluded one
            List<int> allOtherActors = new List<int>();
            var allPlayers = PhotonNetwork.PlayerList;

            foreach(var currentPlayer in allPlayers)
            {
                // Both these users already have the most recent value and don't need this event
                if (Guid.Parse(currentPlayer.UserId) == excludedClient || currentPlayer.IsMasterClient)
                    continue;

                allOtherActors.Add(currentPlayer.ActorNumber);
            }

            // This makes sure only the new user who joined the session will get this message
            _raiseEventOptions = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others,
                TargetActors = allOtherActors.ToArray()
            };
        }

        protected RaiseEventOptions _raiseEventOptions;

        public RaiseEventOptions RaiseEventOptions { get => _raiseEventOptions; }

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Asset {AssetId}'s property {PropertyName} updated to {Value}";
        }
    }

    public class RejectPropertyUpdateNetworkEvent : AssetPropertyUpdateNetworkEvent
    {
        public int ActorNumber { get; }

        public RejectPropertyUpdateNetworkEvent(Guid id, string property, byte[] value, int actorReference) : base(id, property, value)
        {
            _raiseEventOptions = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others,
                TargetActors = new int[] { actorReference }
            };
        }
    }

    public class ClientPropertyUpdatedNetworkEvent : ICustomNetworkEvent
    {
        public Guid AssetId { get; }

        public string PropertyName { get; }

        public ClientPropertyUpdatedNetworkEvent(Guid id, string property)
        {
            AssetId = id;
            PropertyName = property;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;
    }
    
    public class ClientPropertyUpdatedNetworkEventSerializer : ICustomNetworkEventSerializer<ClientPropertyUpdatedNetworkEvent>
    {
        public object[] Serialize(ClientPropertyUpdatedNetworkEvent @event) => new object[] { @event.AssetId, @event.PropertyName };

        public ClientPropertyUpdatedNetworkEvent Deserialize(object[] data) => new ClientPropertyUpdatedNetworkEvent((Guid)data[0], (string)data[1]);
    }

    public class AssetPropertyUpdateNetworkEventSerializer : ICustomNetworkEventSerializer<AssetPropertyUpdateNetworkEvent>
    {
        public object[] Serialize(AssetPropertyUpdateNetworkEvent @event) => new object[] { @event.AssetId, @event.PropertyName, @event.Value };

        public AssetPropertyUpdateNetworkEvent Deserialize(object[] data) => new AssetPropertyUpdateNetworkEvent((Guid)data[0], (string)data[1], (byte[])data[2]);
    }

    public class RejectPropertyUpdateNetworkEventSerializer : ICustomNetworkEventSerializer<RejectPropertyUpdateNetworkEvent>
    {
        public object[] Serialize(RejectPropertyUpdateNetworkEvent @event) => new object[] { @event.AssetId, @event.PropertyName, @event.Value, @event.ActorNumber };

        public RejectPropertyUpdateNetworkEvent Deserialize(object[] data) => new RejectPropertyUpdateNetworkEvent((Guid)data[0], (string)data[1], (byte[])data[2], (int)data[3]);
    }
}