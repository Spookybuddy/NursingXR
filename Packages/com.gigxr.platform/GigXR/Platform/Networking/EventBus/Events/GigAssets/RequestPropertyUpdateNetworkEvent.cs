namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    public class RequestPropertyUpdateNetworkEvent : ICustomNetworkEvent
    {
        public Guid UserId { get; }

        public Guid AssetId { get; }

        public string AssetPropertyName { get; }

        public byte[] Value { get; }

        public RequestPropertyUpdateNetworkEvent(Guid userId, Guid assetId, string property, byte[] val)
        {
            UserId = userId;
            AssetId = assetId;
            AssetPropertyName = property;
            Value = val;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.MasterClient
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"{UserId} is requesting Asset {AssetId}'s property {AssetPropertyName} be updated to {Value}";
        }
    }

    public class RequestPropertyUpdateNetworkEventSerializer : ICustomNetworkEventSerializer<RequestPropertyUpdateNetworkEvent>
    {
        public object[] Serialize(RequestPropertyUpdateNetworkEvent @event) => new object[] { @event.UserId, @event.AssetId, @event.AssetPropertyName, @event.Value };

        public RequestPropertyUpdateNetworkEvent Deserialize(object[] data) => new RequestPropertyUpdateNetworkEvent((Guid)data[0], (Guid)data[1], (string)data[2], (byte[])data[3]);
    }
}