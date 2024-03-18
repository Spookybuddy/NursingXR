namespace GIGXR.Platform.Sessions
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.Networking.EventBus;
    using Photon.Realtime;

    /// <summary>
    /// Network Event that is sent out when a session is closed so the users may leave the session gracefully.
    /// </summary>
    public class HostClosedSessionNetworkEvent : ICustomNetworkEvent
    {
        public HostClosedSessionNetworkEvent()
        {
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;
    }

    public class HostClosedSessionNetworkEventSerializer : ICustomNetworkEventSerializer<HostClosedSessionNetworkEvent>
    {
        public object[] Serialize(HostClosedSessionNetworkEvent @event) => new object[] { };

        public HostClosedSessionNetworkEvent Deserialize(object[] data) => new HostClosedSessionNetworkEvent();
    }
}