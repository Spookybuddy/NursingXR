namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;

    /// <summary>
    /// Network event that is sent from the Host to all other users when a new stage is loaded
    /// </summary>
    public class StageLoadedNetworkEvent : ICustomNetworkEvent
    {
        public StageLoadedNetworkEvent()
        {

        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Stage Loaded";
        }
    }

    public class StageLoadedNetworkEventSerializer : ICustomNetworkEventSerializer<StageLoadedNetworkEvent>
    {
        public object[] Serialize(StageLoadedNetworkEvent @event) => new object[] { };

        public StageLoadedNetworkEvent Deserialize(object[] data) => new StageLoadedNetworkEvent();
    }
}