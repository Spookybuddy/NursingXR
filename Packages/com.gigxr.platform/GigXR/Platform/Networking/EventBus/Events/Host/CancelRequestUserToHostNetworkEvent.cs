namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    public class CancelRequestUserToHostNetworkEvent : ICustomNetworkEvent
    {
        public int ActorNumber { get; }

        public string HostName { get; }

        private RaiseEventOptions _raiseEventOptionss;

        public RaiseEventOptions RaiseEventOptions { get => _raiseEventOptionss; }

        public SendOptions SendOptions => SendOptions.SendReliable;

        public CancelRequestUserToHostNetworkEvent(int actorNumber, string hostName)
        {
            ActorNumber = actorNumber;
            HostName = hostName;

            // This makes sure only the new user who joined the session will get this message
            _raiseEventOptionss = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others,
                TargetActors = new[] { ActorNumber }
            };
        }

        public override string ToString()
        {
            return $"Cancelling request to Actor {ActorNumber} to Become Host";
        }
    }

    public class CancelRequestUserToHostNetworkEventSerializer : ICustomNetworkEventSerializer<CancelRequestUserToHostNetworkEvent>
    {
        public object[] Serialize(CancelRequestUserToHostNetworkEvent @event) => new object[] { @event.ActorNumber, @event.HostName };

        public CancelRequestUserToHostNetworkEvent Deserialize(object[] data) => new CancelRequestUserToHostNetworkEvent((int)data[0], (string)data[1]);
    }
}