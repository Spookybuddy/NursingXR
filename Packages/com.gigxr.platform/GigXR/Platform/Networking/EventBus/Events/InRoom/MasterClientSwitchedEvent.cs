using Photon.Realtime;

namespace GIGXR.Platform.Networking.EventBus.Events.InRoom
{
    public class MasterClientSwitchedEvent : BaseNetworkEvent
    {
        public Player NewMasterClient { get; }

        public MasterClientSwitchedEvent(Player newMasterClient)
        {
            NewMasterClient = newMasterClient;
        }

        public override string ToString()
        {
            return $"NewMasterClient: {NewMasterClient}";
        }
    }
}