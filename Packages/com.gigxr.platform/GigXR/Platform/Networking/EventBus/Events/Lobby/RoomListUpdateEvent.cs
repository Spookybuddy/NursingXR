using Photon.Realtime;
using System.Collections.Generic;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class RoomListUpdateEvent : BaseNetworkEvent
    {
        public List<RoomInfo> RoomList { get; }

        public RoomListUpdateEvent(List<RoomInfo> roomList)
        {
            RoomList = roomList;
        }

        public override string ToString()
        {
            return $"RoomList: {RoomList}";
        }
    }
}