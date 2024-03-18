namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    public class RejectHostRequestNetworkEvent : ICustomNetworkEvent
    {
        public string UserName { get; }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.MasterClient
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public RejectHostRequestNetworkEvent(string userName)
        {
            UserName = userName;
        }

        public override string ToString()
        {
            return $"Rejected Host Request";
        }
    }

    public class RejectHostRequestNetworkEventSerializer : ICustomNetworkEventSerializer<RejectHostRequestNetworkEvent>
    {
        public object[] Serialize(RejectHostRequestNetworkEvent @event) => new object[] { @event.UserName };

        public RejectHostRequestNetworkEvent Deserialize(object[] data) => new RejectHostRequestNetworkEvent((string)data[0]);
    }
}