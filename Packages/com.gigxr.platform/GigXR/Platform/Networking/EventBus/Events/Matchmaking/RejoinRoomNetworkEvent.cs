namespace GIGXR.Platform.Networking.EventBus.Events.Matchmaking
{
    public class RejoinRoomNetworkEvent : BaseNetworkEvent
    {
        public string RoomName { get; }

        public RejoinRoomNetworkEvent(string roomName)
        {
            RoomName = roomName;
        }

        public override string ToString()
        {
            return $"Rejoining {RoomName}";
        }
    }
}