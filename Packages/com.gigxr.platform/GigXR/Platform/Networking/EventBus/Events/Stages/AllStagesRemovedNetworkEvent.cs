namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    /// <summary>
    /// Network event that is sent from the Host to all other users when all stages are removed
    /// </summary>
    public class AllStagesRemovedNetworkEvent : ICustomNetworkEvent
    {
        public AllStagesRemovedNetworkEvent()
        {
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"All Stages Removed";
        }
    }

    public class AllStagesRemovedNetworkEventSerializer : ICustomNetworkEventSerializer<AllStagesRemovedNetworkEvent>
    {
        public object[] Serialize(AllStagesRemovedNetworkEvent @event) => new object[] { };

        public AllStagesRemovedNetworkEvent Deserialize(object[] data) => new AllStagesRemovedNetworkEvent();
    }
}