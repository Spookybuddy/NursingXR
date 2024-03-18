namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    /// <summary>
    /// Network event that is sent from the Host to all other users when a stage is deleted
    /// </summary>
    public class StageRemovedNetworkEvent : ICustomNetworkEvent
    {
        public Guid StageId { get; }
        
        public StageRemovedNetworkEvent(Guid guid)
        {
            StageId = guid;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Stage Removed";
        }
    }

    public class StageRemovedNetworkEventSerializer : ICustomNetworkEventSerializer<StageRemovedNetworkEvent>
    {
        public object[] Serialize(StageRemovedNetworkEvent @event) => new object[] { @event.StageId };

        public StageRemovedNetworkEvent Deserialize(object[] data) => new StageRemovedNetworkEvent((Guid)data[0]);
    }
}