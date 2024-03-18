using Photon.Realtime;
using System.Collections.Generic;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class LobbyStatisticsUpdateEvent : BaseNetworkEvent
    {
        public List<TypedLobbyInfo> LobbyStatistics { get; }

        public LobbyStatisticsUpdateEvent(List<TypedLobbyInfo> lobbyStatistics)
        {
            LobbyStatistics = lobbyStatistics;
        }

        public override string ToString()
        {
            return $"RoomList: {LobbyStatistics}";
        }
    }
}