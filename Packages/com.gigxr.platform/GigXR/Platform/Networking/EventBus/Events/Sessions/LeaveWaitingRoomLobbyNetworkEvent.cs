namespace GIGXR.Platform.Networking.EventBus.Events.Sessions
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    /// <summary>
    /// A networked event that is sent out when the host returns and all users must leave the lobby so they
    /// can view assets again.
    /// </summary>
    public class LeaveWaitingRoomLobbyNetworkEvent : ICustomNetworkEvent
    {
        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions();

        public SendOptions SendOptions => SendOptions.SendReliable;

        public LeaveWaitingRoomLobbyNetworkEvent()
        {
        }

        public override string ToString()
        {
            return $"Leave Lobby";
        }
    }

    public class LeaveWaitingRoomLobbyNetworkEventSerializer : ICustomNetworkEventSerializer<LeaveWaitingRoomLobbyNetworkEvent>
    {
        public object[] Serialize(LeaveWaitingRoomLobbyNetworkEvent @event) => new object[] { };

        public LeaveWaitingRoomLobbyNetworkEvent Deserialize(object[] data) => new LeaveWaitingRoomLobbyNetworkEvent();
    }
}