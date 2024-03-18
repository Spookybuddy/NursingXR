using Photon.Realtime;
using System.Collections.Generic;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class LeftLobbyEvent : BaseNetworkEvent
    {
        public LeftLobbyEvent()
        {
        }

        public override string ToString()
        {
            return $"Left Lobby";
        }
    }
}