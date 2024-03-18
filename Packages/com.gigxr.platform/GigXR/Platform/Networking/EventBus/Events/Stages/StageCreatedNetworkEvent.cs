namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    /// <summary>
    /// Network event that is sent from the Host to all other users when a new stage is created
    /// </summary>
    public class StageCreatedNetworkEvent : ICustomNetworkEvent
    {
        public Guid StageID { get; }

        public StageCreatedNetworkEvent(Guid newID)
        {
            StageID = newID;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Stage Created";
        }
    }

    public class StageCreatedNetworkEventSerializer : ICustomNetworkEventSerializer<StageCreatedNetworkEvent>
    {
        public object[] Serialize(StageCreatedNetworkEvent @event) => new object[] { @event.StageID };

        public StageCreatedNetworkEvent Deserialize(object[] data) => new StageCreatedNetworkEvent((Guid)data[0]);
    }
}