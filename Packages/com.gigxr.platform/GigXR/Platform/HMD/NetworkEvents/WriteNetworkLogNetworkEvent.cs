namespace GIGXR.Platform.HMD.NetworkEvents.Sessions
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.HMD;
    using GIGXR.Platform.Networking.EventBus;
    using Photon.Realtime;

    public class WriteNetworkLogNetworkEvent : ICustomNetworkEvent
    {
        public NetworkEventType EventType { get; }
        public string Message { get; }

        public WriteNetworkLogNetworkEvent(NetworkEventType eventType, string message)
        {
            EventType = eventType;
            Message = message;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };

        public SendOptions SendOptions => SendOptions.SendReliable;
    }

    public class WriteNetworkLogNetworkEventSerializer : ICustomNetworkEventSerializer<WriteNetworkLogNetworkEvent>
    {
        public object[] Serialize(WriteNetworkLogNetworkEvent @event) => new object[] { @event.EventType, @event.Message };

        public WriteNetworkLogNetworkEvent Deserialize(object[] data) => new WriteNetworkLogNetworkEvent((NetworkEventType)data[0], (string)data[1]);
    }
}