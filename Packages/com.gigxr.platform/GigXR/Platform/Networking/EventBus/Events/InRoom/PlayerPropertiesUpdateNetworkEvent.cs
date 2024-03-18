using ExitGames.Client.Photon;
using Photon.Realtime;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class PlayerPropertiesUpdateNetworkEvent : BaseNetworkEvent
    {
        public Player TargetPlayer { get; }
        public Hashtable ChangedProps { get; }

        public PlayerPropertiesUpdateNetworkEvent(Player targetPlayer, Hashtable changedProps)
        {
            TargetPlayer = targetPlayer;
            ChangedProps = changedProps;
        }

        public override string ToString()
        {
            return $"TargetPlayer: {TargetPlayer}";
        }
    }
}