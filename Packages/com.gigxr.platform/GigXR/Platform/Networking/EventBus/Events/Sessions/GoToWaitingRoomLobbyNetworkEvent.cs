namespace GIGXR.Platform.Networking.EventBus.Events.Sessions
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    /// <summary>
    /// A networked event that is sent when the host disconnects from a session that allows the users to enter
    /// a lobby that hides the assets until the host returns
    /// </summary>
    public class GoToWaitingRoomLobbyNetworkEvent : ICustomNetworkEvent
    {
        private RaiseEventOptions _raiseEventOptionss;

        public RaiseEventOptions RaiseEventOptions { get => _raiseEventOptionss; }

        public SendOptions SendOptions => SendOptions.SendReliable;

        public int TargetPlayerActorNumber { get; }

        public bool FromHostDisconnect { get; }

        public GoToWaitingRoomLobbyNetworkEvent(int targetPlayerActorNumber, bool fromHostDisconnect)
        {
            TargetPlayerActorNumber = targetPlayerActorNumber;
            FromHostDisconnect = fromHostDisconnect;

            // This makes sure only the new user who joined the session will get this message
            _raiseEventOptionss = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others,
                TargetActors = new[] { TargetPlayerActorNumber }
            };
        }

        public override string ToString()
        {
            return $"Go to waiting room lobby";
        }
    }

    public class GoToWaitingRoomLobbyNetworkEventSerializer : ICustomNetworkEventSerializer<GoToWaitingRoomLobbyNetworkEvent>
    {
        public object[] Serialize(GoToWaitingRoomLobbyNetworkEvent @event) => new object[] { @event.TargetPlayerActorNumber, @event.FromHostDisconnect };

        public GoToWaitingRoomLobbyNetworkEvent Deserialize(object[] data) => new GoToWaitingRoomLobbyNetworkEvent((int)data[0], (bool)data[1]);
    }
}