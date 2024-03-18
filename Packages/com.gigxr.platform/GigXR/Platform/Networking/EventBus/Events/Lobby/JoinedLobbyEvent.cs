namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class JoinedLobbyEvent : BaseNetworkEvent
    {
        public JoinedLobbyEvent()
        {
        }

        public override string ToString()
        {
            return $"Joined Lobby";
        }
    }
}