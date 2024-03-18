namespace GIGXR.Platform.Networking.EventBus.Events.Sessions
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    /// <summary>
    /// A networked event that is sent to the Host after a Client has mapped their assets to the Photon ID
    /// and can begin syncing them.
    /// </summary>
    public class ClientReadyNetworkEvent : ICustomNetworkEvent
    {
        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.MasterClient
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public ClientReadyNetworkEvent()
        {
        }

        public override string ToString()
        {
            return $"Client is ready";
        }
    }

    public class ClientReadyNetworkEventSerializer : ICustomNetworkEventSerializer<ClientReadyNetworkEvent>
    {
        public object[] Serialize(ClientReadyNetworkEvent @event) => new object[] { };

        public ClientReadyNetworkEvent Deserialize(object[] data) => new ClientReadyNetworkEvent();
    }
}