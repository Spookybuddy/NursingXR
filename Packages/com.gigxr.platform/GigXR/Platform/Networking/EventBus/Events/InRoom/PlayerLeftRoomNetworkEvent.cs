using Photon.Realtime;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class PlayerLeftRoomNetworkEvent : BaseNetworkEvent
    {
        public Player Player { get; }

        public PlayerLeftRoomNetworkEvent(Player player)
        {
            Player = player;
        }

        public override string ToString()
        {
            return $"Player: {Player}";
        }
    }
}