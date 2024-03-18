namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    /// <summary>
    /// Network event that is sent from the Host to all other users when a stage is duplicated
    /// </summary>
    public class StageDuplicatedNetworkEvent : ICustomNetworkEvent
    {
        public Guid StageID { get; }

        public StageDuplicatedNetworkEvent(Guid stageID)
        {
            StageID = stageID;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Stage {StageID} duplicated";
        }
    }

    public class StageDuplicatedEventSerializer : ICustomNetworkEventSerializer<StageDuplicatedNetworkEvent>
    {
        public object[] Serialize(StageDuplicatedNetworkEvent @event) => new object[] { @event.StageID };

        public StageDuplicatedNetworkEvent Deserialize(object[] data) => new StageDuplicatedNetworkEvent((Guid)data[0]);
    }
}