using ExitGames.Client.Photon;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class RoomPropertiesUpdateNetworkEvent : BaseNetworkEvent
    {
        public Hashtable PropertiesThatChanged { get; }

        public RoomPropertiesUpdateNetworkEvent(Hashtable propertiesThatChanged)
        {
            PropertiesThatChanged = propertiesThatChanged;
        }
    }
}