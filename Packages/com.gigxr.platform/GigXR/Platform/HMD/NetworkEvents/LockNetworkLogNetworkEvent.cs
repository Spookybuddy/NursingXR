namespace GIGXR.Platform.HMD.NetworkEvents.Sessions
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.Networking.EventBus;
    using Photon.Realtime;

    public class LockNetworkLogNetworkEvent : ICustomNetworkEvent
    {
        public bool IsReadOnly { get; }
        public LockNetworkLogNetworkEvent(bool readOnly)
        {
            IsReadOnly = readOnly;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;
    }

    public class LockNetworkLogNetworkEventSerializer : ICustomNetworkEventSerializer<LockNetworkLogNetworkEvent>
    {
        public object[] Serialize(LockNetworkLogNetworkEvent @event) => new object[] { @event.IsReadOnly };

        public LockNetworkLogNetworkEvent Deserialize(object[] data) => new LockNetworkLogNetworkEvent((bool)data[0]);
    }
}