namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    public class RequestUserToHostNetworkEvent : ICustomNetworkEvent
    {
        public int ActorNumber { get; }

        public string HostName { get; }

        private RaiseEventOptions _raiseEventOptionss;

        public RaiseEventOptions RaiseEventOptions { get => _raiseEventOptionss; }

        public SendOptions SendOptions => SendOptions.SendReliable;

        public RequestUserToHostNetworkEvent(int actorNumber, string hostName)
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
            return $"Requesting Actor {ActorNumber} to Become Host";
        }
    }

    public class RequestUserToHostNetworkEventSerializer : ICustomNetworkEventSerializer<RequestUserToHostNetworkEvent>
    {
        public object[] Serialize(RequestUserToHostNetworkEvent @event) => new object[] { @event.ActorNumber, @event.HostName };

        public RequestUserToHostNetworkEvent Deserialize(object[] data) => new RequestUserToHostNetworkEvent((int)data[0], (string)data[1]);
    }
}