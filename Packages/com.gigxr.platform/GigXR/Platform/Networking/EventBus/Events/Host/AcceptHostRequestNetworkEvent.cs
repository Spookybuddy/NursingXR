namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    public class AcceptHostRequestNetworkEvent : ICustomNetworkEvent
    {
        public string UserName { get; }

        public Guid UserId { get; }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.MasterClient
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public AcceptHostRequestNetworkEvent(string userName, Guid userId)
        {
            UserName = userName;
            UserId = userId;
        }

        public override string ToString()
        {
            return $"{UserId} Accepted Host Request";
        }
    }

    public class AcceptHostRequestNetworkEventSerializer : ICustomNetworkEventSerializer<AcceptHostRequestNetworkEvent>
    {
        public object[] Serialize(AcceptHostRequestNetworkEvent @event) => new object[] { @event.UserName, @event.UserId };

        public AcceptHostRequestNetworkEvent Deserialize(object[] data) => new AcceptHostRequestNetworkEvent((string)data[0], (Guid)data[1]);
    }
}