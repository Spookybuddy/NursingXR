namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    /// <summary>
    /// Network event that is sent from the Host to all other users when the order of two stages is changed
    /// </summary>
    public class StagesSwappedNetworkEvent : ICustomNetworkEvent
    {
        public Guid Stage1ID { get; }
        public Guid Stage2ID { get; }

        public StagesSwappedNetworkEvent(Guid first, Guid second)
        {
            Stage1ID = first;
            Stage2ID = second;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => throw new System.NotImplementedException();

        public override string ToString()
        {
            return $"Stages swapped";
        }
    }

    public class StagesSwappedNetworkEventSerializer : ICustomNetworkEventSerializer<StagesSwappedNetworkEvent>
    {
        public object[] Serialize(StagesSwappedNetworkEvent @event) => new object[] { @event.Stage1ID, @event.Stage2ID };

        public StagesSwappedNetworkEvent Deserialize(object[] data) => new StagesSwappedNetworkEvent((Guid)data[0], (Guid)data[1]);
    }
}