using Photon.Realtime;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class PlayerEnteredRoomNetworkEvent : BaseNetworkEvent
    {
        public Player Player { get; }

        public PlayerEnteredRoomNetworkEvent(Player player)
        {
            Player = player;
        }

        public override string ToString()
        {
            return $"Player: {Player}";
        }
    }
}