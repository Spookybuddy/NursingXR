namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    public class PromoteToHostNetworkEvent : ICustomNetworkEvent
    {
        public Guid NewHostID { get; }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.All
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public PromoteToHostNetworkEvent(Guid newHost)
        {
            NewHostID = newHost;
        }

        public override string ToString()
        {
            return $"New Host {NewHostID}";
        }
    }

    public class PromoteToHostNetworkEventSerializer : ICustomNetworkEventSerializer<PromoteToHostNetworkEvent>
    {
        public object[] Serialize(PromoteToHostNetworkEvent @event) => new object[] { @event.NewHostID };

        public PromoteToHostNetworkEvent Deserialize(object[] data) => new PromoteToHostNetworkEvent((Guid)data[0]);
    }
}