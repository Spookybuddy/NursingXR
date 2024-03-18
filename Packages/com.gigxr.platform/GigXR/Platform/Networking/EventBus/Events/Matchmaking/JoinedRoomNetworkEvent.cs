namespace GIGXR.Platform.Networking.EventBus.Events.Matchmaking
{
    public class JoinedRoomNetworkEvent : BaseNetworkEvent
    {
        public string UserId { get; }

        public string NickName { get; }

        public JoinedRoomNetworkEvent(string userId, string nickname)
        {
            UserId = userId;
            NickName = nickname;
        }
    }
}